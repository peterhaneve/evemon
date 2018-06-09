using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using EVEMon.Common;
using EVEMon.Common.Collections;
using EVEMon.Common.Constants;
using EVEMon.Common.Controls;
using EVEMon.Common.CustomEventArgs;
using EVEMon.Common.Enumerations;
using EVEMon.Common.Enumerations.UISettings;
using EVEMon.Common.Extensions;
using EVEMon.Common.Factories;
using EVEMon.Common.Helpers;
using EVEMon.Common.Interfaces;
using EVEMon.Common.Models;
using EVEMon.Common.Scheduling;
using EVEMon.Common.SettingsObjects;
using EVEMon.NotificationWindow;

namespace EVEMon.SkillPlanner
{
    /// <summary>
    /// The main control of the plan editor window, the list of plan entries.
    /// </summary>
    public sealed partial class PlanEditorControl : UserControl
    {
        #region Fields

        private const string PluggableColumn = "chPluggable";
        private const int ArrowUpIndex = 4;
        private const int ArrowDownIndex = 5;

        private int m_lastImplantSetIndex;
        private bool m_areRemappingPointsActive;

        private bool m_init;

        private Font m_plannedSkillFont;
        private Font m_prerequisiteSkillFont;
        private Color m_nonImmedTrainablePlanEntryColor;
        private Color m_remappingBackColor;
        private Color m_remappingForeColor;

        private RemappingPoint m_previousRemappingPoint;

        private Plan m_plan;
        private Character m_character;

        // The ImplantsControl or the AttributesOptimizationForm
        private IPlanOrderPluggable m_pluggable;

        // Drag and drop
        private MouseButtons m_dragButton = MouseButtons.None;

        // Columns
        private bool m_isUpdatingColumns;
        private bool m_columnsOrderChanged;
        private readonly List<PlanColumnSettings> m_columns = new List<PlanColumnSettings>();

        // Tooltip
        private readonly InfiniteDisplayToolTip m_tooltip;

        #endregion


        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        public PlanEditorControl()
        {
            InitializeComponent();
            pscPlan.RememberDistanceKey = "PlanEditor";

            m_tooltip = new InfiniteDisplayToolTip(lvSkills);

            ListViewHelper.EnableDoubleBuffer(lvSkills);

            pscPlan.Panel2Collapsed = true;

            lvSkills.ColumnWidthChanged += lvSkills_ColumnWidthChanged;
            lvSkills.ColumnClick += lvSkills_ColumnClick;
            tsSortPriorities.Click += tsSortPriorities_Clicked;
            cbChooseImplantSet.DropDown += cbChooseImplantSet_DropDown;

            EveMonClient.CharacterUpdated += EveMonClient_CharacterUpdated;
            EveMonClient.CharacterSkillQueueUpdated += EveMonClient_CharacterSkillQueueUpdated;
            EveMonClient.CharacterImplantSetCollectionChanged += EveMonClient_CharacterImplantSetCollectionChanged;
            EveMonClient.PlanChanged += EveMonClient_PlanChanged;
            EveMonClient.SettingsChanged += EveMonClient_SettingsChanged;
            EveMonClient.TimerTick += EveMonClient_TimerTick;
            EveMonClient.SchedulerChanged += EveMonClient_SchedulerChanged;
            Disposed += OnDisposed;
        }

        #endregion


        #region Inherited Events

        /// <summary>
        /// On load, updates the controls.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Return on design mode
            if (DesignMode || this.IsDesignModeHosted())
                return;

            Font = FontFactory.GetFont("Tahoma", 8.25F);

            m_columns.Clear();
            m_columns.AddRange(Settings.UI.PlanWindow.Columns);

            m_plannedSkillFont = FontFactory.GetFont(lvSkills.Font, FontStyle.Bold);
            m_prerequisiteSkillFont = FontFactory.GetFont(lvSkills.Font);
            m_nonImmedTrainablePlanEntryColor = SystemColors.GrayText;
            m_remappingForeColor = SystemColors.HotTrack;
            m_remappingBackColor = SystemColors.Info;

            m_init = true;
        }

        /// <summary>
        /// Unsubscribe events on disposing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDisposed(object sender, EventArgs e)
        {
            m_tooltip.Dispose();
            EveMonClient.CharacterUpdated -= EveMonClient_CharacterUpdated;
            EveMonClient.CharacterSkillQueueUpdated -= EveMonClient_CharacterSkillQueueUpdated;
            EveMonClient.CharacterImplantSetCollectionChanged -= EveMonClient_CharacterImplantSetCollectionChanged;
            EveMonClient.PlanChanged -= EveMonClient_PlanChanged;
            EveMonClient.SettingsChanged -= EveMonClient_SettingsChanged;
            EveMonClient.TimerTick -= EveMonClient_TimerTick;
            EveMonClient.SchedulerChanged -= EveMonClient_SchedulerChanged;
            Disposed -= OnDisposed;
        }

        #endregion


        #region Properties

        /// <summary>
        /// Gets or sets the plan represented by this editor.
        /// </summary>
        internal Plan Plan
        {
            get { return m_plan; }
            set
            {
                if (value == null || m_plan == value)
                    return;

                m_plan = value;
                m_character = (Character)m_plan.Character;
                DisplayPlan = new PlanScratchpad(m_character);
                m_lastImplantSetIndex = -1;

                // Children controls
                skillSelectControl.Plan = m_plan;

                // Update Implant Set control
                UpdateImplantSetList();
                cbChooseImplantSet.SelectedIndex = 0;

                // Build the plan
                UpdateDisplayPlan(updateSkillList: false);

                if (!m_init)
                    return;

                // Update the columns
                UpdateListColumns();
            }
        }

        /// <summary>
        /// Gets the version of the plan as it is currently displayed.
        /// </summary>
        internal PlanScratchpad DisplayPlan { get; private set; }

        /// <summary>
        /// Gets the number of unique skills selected (two levels of same skill counts for one unique skill).
        /// </summary>
        private int UniqueSkillsCount => SelectedEntries.GetUniqueSkillsCount();

        /// <summary>
        /// Gets the number of not known skills selected (two levels of same skill counts for one unique skill).
        /// </summary>
        private int NotKnownSkillsCount => SelectedEntries.GetNotKnownSkillsCount();

        /// <summary>
        /// Gets the cost of known skills selected.
        /// </summary>
        private long SkillBooksCost => SelectedEntries.GetTotalBooksCost();

        /// <summary>
        /// Gets the cost of not known skills selected.
        /// </summary>
        private long NotKnownSkillBooksCost => SelectedEntries.GetNotKnownSkillBooksCost();

        /// <summary>
        /// Gets the skill points of the planned skill levels
        /// </summary>
        private long TotalSkillPoints => SelectedEntries.GetTotalSkillPoints();

        #endregion


        #region Global events

        /// <summary>
        /// When the character change, some entries may now be known, so we reupdate everything.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EveMonClient_CharacterUpdated(object sender, CharacterChangedEventArgs e)
        {
            if (e.Character != m_character || m_character == null)
                return;

            UpdateDisplayPlan();
        }

        /// <summary>
        /// When the character skill queue change, update everything.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EveMonClient_CharacterSkillQueueUpdated(object sender, CharacterChangedEventArgs e)
        {
            if (e.Character != m_character || m_character == null)
                return;

            UpdateDisplayPlan();
        }

        /// <summary>
        /// When the character implant sets changed, update the implant set selection. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void EveMonClient_CharacterImplantSetCollectionChanged(object sender, EventArgs e)
        {
            if (m_character == null)
                return;

            UpdateImplantSetList();
            cbChooseImplantSet.SelectedIndex = m_lastImplantSetIndex < cbChooseImplantSet.Items.Count ? m_lastImplantSetIndex : 0;
            await UpdateImplantSet();
        }

        /// <summary>
        /// When the plan changed, entries may have changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EveMonClient_PlanChanged(object sender, PlanChangedEventArgs e)
        {
            if (e.Plan != Plan || Plan == null)
                return;

            UpdateDisplayPlan(false);
            UpdateListColumns();
        }

        /// <summary>
        /// When the settings changed, implant sets, the highlights and such may be different. 
        /// Entries are still the same but we may need to update implant sets, highlights and others.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EveMonClient_SettingsChanged(object sender, EventArgs e)
        {
            if (Plan == null)
                return;

            UpdateSkillList();
        }

        /// <summary>
        /// When the scheduler changed, the blocking highlights may be different. 
        /// Entries are still the same but we may need to update the blocking highlights.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EveMonClient_SchedulerChanged(object sender, EventArgs e)
        {
            if (Plan == null)
                return;

            UpdateSkillList();
        }

        /// <summary>
        /// Per second check if columns have been reordered.
        /// When true, column settings get saved and reloaded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EveMonClient_TimerTick(object sender, EventArgs e)
        {
            if (!Visible)
                return;

            if (m_columnsOrderChanged)
            {
                Settings.UI.PlanWindow.Columns.Clear();
                Settings.UI.PlanWindow.Columns.AddRange(ExportColumnSettings());
                ImportColumnSettings(Settings.UI.PlanWindow.Columns);
            }

            m_columnsOrderChanged = false;
        }

        #endregion


        #region Content creation and refresh

        /// <summary>
        /// Update the Implant Set control list.
        /// </summary>
        private void UpdateImplantSetList()
        {
            // Populate the "choose implant set"
            cbChooseImplantSet.Items.Clear();
            foreach (ImplantSet set in m_character.ImplantSets)
            {
                cbChooseImplantSet.Items.Add(set);
            }

            int comboBoxArrowWidth = 16 * (int)Math.Truncate(Graphics.FromHwnd(Handle).DpiX / EveMonConstants.DefaultDpi);
            int maxWidth = Math.Min(m_character.ImplantSets.Max(set =>
                TextRenderer.MeasureText(set.Name, cbChooseImplantSet.Font).Width) + comboBoxArrowWidth,
                (int)(cbChooseImplantSet.Font.Size * EveMonConstants.ImplantSetNameMaxLength));

            cbChooseImplantSet.Size = new Size(Math.Max(maxWidth, cbChooseImplantSet.Size.Width), cbChooseImplantSet.Size.Height);
        }

        /// <summary>
        /// Whenever the sorting option or the base plan changed, we update the sorted plan.
        /// </summary>
        private void UpdateDisplayPlan(bool updateSkillList = true)
        {
            DisplayPlan.RebuildPlanFrom(m_plan, true);

            // Share the remapping points
            PlanEntry[] srcEntries = m_plan.ToArray();
            PlanEntry[] destEntries = DisplayPlan.ToArray();
            for (int i = 0; i < srcEntries.Length; i++)
            {
                destEntries[i].Remapping = srcEntries[i].Remapping;
            }

            // Apply the sort
            DisplayPlan.Sort(m_plan.SortingPreferences);

            // Update the Skill List
            if (updateSkillList)
                UpdateSkillList();
        }

        /// <summary>
        /// Rebuild the list items from the entries and their remapping points. Plan entries items are not fully initialized, 
        /// only their tags and a couple of things.
        /// Full intialization, especially the columns values and such, will be completed in <see cref="UpdateListViewItems"/>.
        /// </summary>
        private void UpdateSkillList()
        {
            if (m_plan == null)
                return;

            // Disable autorefresh timer, it will be enabled if a training entry is found
            tmrAutoRefresh.Stop();

            // Stores selection and focus, to restore them after the update
            Dictionary<int, bool> selection = StoreSelection();
            int focusedHashCode = lvSkills.FocusedItem?.Tag.GetHashCode() ?? 0;

            lvSkills.BeginUpdate();
            try
            {
                // Scroll through entries and their remapping points
                List<ListViewItem> items = new List<ListViewItem>();
                foreach (PlanEntry entry in DisplayPlan)
                {
                    // In any case, we must insert a new item for this plan's entry at the current index
                    // Do we need to insert a remapping point ?
                    if (entry.Remapping != null)
                    {
                        ListViewItem rlv = new ListViewItem
                        {
                            UseItemStyleForSubItems = true,
                            Tag = entry.Remapping,
                            ImageIndex = 3
                        };
                        items.Add(rlv);
                    }

                    // Insert the entry
                    ListViewItem lvi = new ListViewItem { Tag = entry };
                    items.Add(lvi);

                    // Enable refresh every 30s if a skill is in training
                    tmrAutoRefresh.Enabled |= entry.CharacterSkill.IsTraining;
                }

                // We avoid clear + AddRange because it causes the sliders position to reset
                while (lvSkills.Items.Count > 0)
                {
                    lvSkills.Items.RemoveAt(lvSkills.Items.Count - 1);
                }

                lvSkills.Items.AddRange(items.ToArray());

                // Restore selection and focus
                RestoreSelection(selection);
                ListViewItem focusedItem = lvSkills.Items.Cast<ListViewItem>()
                    .FirstOrDefault(x => x.Tag.GetHashCode() == focusedHashCode);

                if (focusedItem != null)
                    focusedItem.Focused = true;

                lvSkills.Select();

                // Complete the items initialization
                UpdateListViewItems();
            }
            finally
            {
                lvSkills.EndUpdate();
            }
        }

        /// <summary>
        /// Completes the items initialization or refresh them. Especially their columns values.
        /// </summary>
        private void UpdateListViewItems()
        {
            CheckForPluggable();

            // Start updating the list
            lvSkills.BeginUpdate();
            try
            {
                // Scroll through entries
                for (int i = 0; i < lvSkills.Items.Count; i++)
                {
                    ListViewItem lvi = lvSkills.Items[i];
                    PlanEntry entry = lvi.Tag as PlanEntry;

                    // Add enough subitems to match the number of columns
                    while (lvi.SubItems.Count < lvSkills.Columns.Count)
                    {
                        lvi.SubItems.Add(String.Empty);
                    }

                    // The item represents a skill level entry
                    if (entry != null)
                        FormatEntry(entry, lvi);
                        // The item represents a remapping point
                    else
                        FormatRemappingPoint(lvi);
                }
            }
            finally
            {
                lvSkills.EndUpdate();
            }

            UpdateStatusBar();
        }

        /// <summary>
        /// Formats the entry.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <param name="lvi">The lvi.</param>
        private void FormatEntry(PlanEntry entry, ListViewItem lvi)
        {
            // Is it a prerequisite or a top level entry ?
            if (!Settings.UI.SafeForWork)
            {
                lvi.Font = Settings.UI.PlanWindow.HighlightPlannedSkills && entry.Type == PlanEntryType.Planned
                    ? m_plannedSkillFont
                    : m_prerequisiteSkillFont;
            }

            // Gray out entries that cannot be trained immediately
            if (!entry.CanTrainNow && Settings.UI.PlanWindow.DimUntrainable)
                lvi.ForeColor = m_nonImmedTrainablePlanEntryColor;

            // Checks if this entry has not prereq-met
            if (!entry.CharacterSkill.IsKnown)
                lvi.ForeColor = Color.Red;

            // Checks if this entry is a non-public skill
            if (!entry.CharacterSkill.IsPublic)
                lvi.ForeColor = Color.DarkRed;

            // Checks if this entry is not known but has prereq-met
            if (!entry.CharacterSkill.IsKnown && entry.CharacterSkill.IsPublic &&
                entry.CharacterSkill.ArePrerequisitesMet)
                lvi.ForeColor = Color.LightSlateGray;

            // Checks if this entry is partially trained
            bool level = entry.Level == entry.CharacterSkill.Level + 1;
            if (Settings.UI.PlanWindow.HighlightPartialSkills)
            {
                bool partiallyTrained = entry.CharacterSkill.FractionCompleted > 0 &&
                                        entry.CharacterSkill.FractionCompleted < 1;
                if (level && partiallyTrained)
                    lvi.ForeColor = Color.Green;
            }

            HighlightQueuedSkills(lvi, entry);

            // Checks if this entry is currently training (even if it's paused)
            if (entry.CharacterSkill.IsTraining && level)
            {
                lvi.BackColor = Color.LightSteelBlue;
                lvi.ForeColor = Color.Black;
            }

            // Checks whether this entry will be blocked
            string blockingEntry = String.Empty;
            if (Settings.UI.PlanWindow.HighlightConflicts)
            {
                bool isAutoBlocking;
                bool isBlocked = Scheduler.SkillIsBlockedAt(entry.EndTime, out blockingEntry, out isAutoBlocking);
                bool showBlocked = true;
                CCPCharacter ccpCharacter = m_character as CCPCharacter;

                if (ccpCharacter != null)
                {
                    PlanEntry planEntry = m_plan.GetEntry(entry.Skill, entry.Level);
                    int indexOfEntry = m_plan.IndexOf(planEntry);
                    QueuedSkill queueSkill = ccpCharacter.SkillQueue.ElementAtOrDefault(indexOfEntry);

                    if (queueSkill != null)
                    {
                        showBlocked = (Plan.Name != EveMonConstants.CurrentSkillQueueText) ||
                                      !isAutoBlocking ||
                                      ((ccpCharacter.SkillQueue.Count == 1) &&
                                       (indexOfEntry == 0) &&
                                       (queueSkill.Skill == planEntry.Skill.ToCharacter(m_character)));
                    }
                }

                if (isBlocked && showBlocked)
                {
                    lvi.ForeColor = Color.Red;
                    lvi.BackColor = Color.LightGray;
                }
            }

            // Update every column
            lvi.UseItemStyleForSubItems = m_pluggable == null;
            for (int columnIndex = 0; columnIndex < lvSkills.Columns.Count; columnIndex++)
            {
                // Regular columns (not pluggable-dependent)
                if (lvSkills.Columns[columnIndex].Tag != null)
                {
                    PlanColumnSettings columnSettings = (PlanColumnSettings)lvSkills.Columns[columnIndex].Tag;

                    lvi.SubItems[columnIndex].BackColor = lvi.BackColor;
                    lvi.SubItems[columnIndex].ForeColor = lvi.ForeColor;
                    lvi.SubItems[columnIndex].Text = GetColumnTextForItem(entry, columnSettings.Column, blockingEntry);
                }
                    // Training time differences
                else
                {
                    TimeSpan timeDifference;
                    string result = String.Empty;
                    if (entry.OldTrainingTime < entry.TrainingTime)
                    {
                        result = "+";
                        timeDifference = entry.TrainingTime - entry.OldTrainingTime;
                        lvi.SubItems[columnIndex].BackColor = lvi.BackColor;
                        lvi.SubItems[columnIndex].ForeColor = Color.DarkRed;
                    }
                    else if (entry.OldTrainingTime > entry.TrainingTime)
                    {
                        result = "-";
                        timeDifference = entry.OldTrainingTime - entry.TrainingTime;
                        lvi.SubItems[columnIndex].BackColor = lvi.BackColor;
                        lvi.SubItems[columnIndex].ForeColor = Color.DarkGreen;
                    }
                    else
                    {
                        timeDifference = TimeSpan.Zero;
                        lvi.SubItems[columnIndex].BackColor = lvi.BackColor;
                        lvi.SubItems[columnIndex].ForeColor = lvi.ForeColor;
                    }

                    result += timeDifference.ToDescriptiveText(DescriptiveTextOptions.IncludeCommas);
                    lvi.SubItems[columnIndex].Text = result;
                }
            }
        }

        /// <summary>
        /// Formats the remapping point.
        /// </summary>
        /// <param name="lvi">The lvi.</param>
        private void FormatRemappingPoint(ListViewItem lvi)
        {
            RemappingPoint point = (RemappingPoint)lvi.Tag;
            for (int columnIndex = 0; columnIndex < lvSkills.Columns.Count; columnIndex++)
            {
                PlanColumnSettings columnSettings = (PlanColumnSettings)lvSkills.Columns[columnIndex].Tag;

                lvi.SubItems[columnIndex].Text = String.Empty;
                lvi.SubItems[columnIndex].BackColor = m_remappingBackColor;
                lvi.SubItems[columnIndex].ForeColor = m_remappingForeColor;

                // We display the text in the SkillName column for better visibility
                if (columnSettings != null && columnSettings.Column == PlanColumn.SkillName)
                {
                    lvi.SubItems[columnIndex].Text = m_areRemappingPointsActive
                        ? point.ToString()
                        : "Remapping (ignored)";
                }
            }
        }

        /// <summary>
        /// Checks for pluggable.
        /// </summary>
        private void CheckForPluggable()
        {
            // When there is a pluggable (implants calculator or attributes optimizer)
            // This one provides us the scratchpad to update training times
            m_areRemappingPointsActive = true;
            if (m_pluggable != null)
            {
                bool areRemappingPointsActive;
                m_pluggable.UpdateStatistics(DisplayPlan, out areRemappingPointsActive);
                m_areRemappingPointsActive = areRemappingPointsActive;
            }
            else
            {
                CharacterScratchpad scratchpad = new CharacterScratchpad(m_character);
                if (m_plan.ChosenImplantSet != null)
                    scratchpad = scratchpad.After(m_plan.ChosenImplantSet);

                DisplayPlan.UpdateStatistics(scratchpad, true, true);
            }
        }

        /// <summary>
        /// Applies formatting to a ListViewItem if the entry is queued.
        /// </summary>
        /// <param name="lvi">ListViewItem to be formatted.</param>
        /// <param name="entry">Entry used to identify is queued.</param>
        private void HighlightQueuedSkills(ListViewItem lvi, PlanEntry entry)
        {
            if (!Settings.UI.PlanWindow.HighlightQueuedSkills)
                return;

            CCPCharacter ccpCharacter = m_character as CCPCharacter;

            // Current character isn't a CCP character, so can't have a Queue
            if (ccpCharacter == null)
                return;

            if (ccpCharacter.SkillQueue.Any(skillLevel => skillLevel.ToString() == entry.ToString()))
                lvi.ForeColor = Color.RoyalBlue;
        }

        /// <summary>
        /// Gets the text to display in the given column for the provided entry.
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="column"></param>
        /// <param name="blockingEntry"></param>
        /// <exception cref="NotImplementedException"></exception>
        /// <returns></returns>
        private static string GetColumnTextForItem(PlanEntry entry, PlanColumn column, string blockingEntry)
        {
            const int MaxNotesLength = 60;

            switch (column)
            {
                case PlanColumn.SkillName:
                    return entry.ToString();
                case PlanColumn.PlanGroup:
                    return entry.PlanGroupsDescription;
                case PlanColumn.TrainingTime:
                    return entry.TrainingTime.ToDescriptiveText(DescriptiveTextOptions.IncludeCommas);
                case PlanColumn.TrainingTimeNatural:
                    return entry.NaturalTrainingTime.ToDescriptiveText(DescriptiveTextOptions.IncludeCommas);
                case PlanColumn.EarliestStart:
                    return $"{entry.StartTime:ddd} {entry.StartTime:G}";
                case PlanColumn.EarliestEnd:
                    return $"{entry.EndTime:ddd} {entry.EndTime:G}";
                case PlanColumn.PercentComplete:
                    return $"{Math.Floor(entry.FractionCompleted * 100)}%";
                case PlanColumn.SkillRank:
                    return entry.Skill.Rank.ToString(CultureConstants.DefaultCulture);
                case PlanColumn.PrimaryAttribute:
                    return entry.Skill.PrimaryAttribute.ToString();
                case PlanColumn.SecondaryAttribute:
                    return entry.Skill.SecondaryAttribute.ToString();
                case PlanColumn.SkillGroup:
                    return entry.Skill.Group.Name;
                case PlanColumn.PlanType:
                    return entry.Type.ToString();
                case PlanColumn.SPTotal:
                    return entry.EstimatedTotalSkillPoints.ToNumericString(0);
                case PlanColumn.SPPerHour:
                    return entry.SpPerHour.ToString(CultureConstants.DefaultCulture);
                case PlanColumn.Priority:
                    return entry.Priority.ToString(CultureConstants.DefaultCulture);
                case PlanColumn.Conflicts:
                    return blockingEntry;
                case PlanColumn.Notes:
                    {
                        if (String.IsNullOrEmpty(entry.Notes))
                            return String.Empty;

                        string result = Regex.Replace(entry.Notes, @"(\r|\n)+", " ", RegexOptions.None);
                        if (result.Length <= MaxNotesLength)
                            return result;

                        return result.Substring(0, MaxNotesLength) + "...";
                    }
                case PlanColumn.Cost:
                    {
                        if (entry.Level != 1 || entry.CharacterSkill.IsKnown)
                            return String.Empty;
                        return entry.CharacterSkill.IsOwned ? "Owned" : entry.Skill.FormattedCost;
                    }
                case PlanColumn.SkillPointsRequired:
                    return entry.SkillPointsRequired.ToNumericString(0);
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Update the columns list according to the settings.
        /// </summary>
        private void UpdateListColumns()
        {
            m_isUpdatingColumns = true;
            lvSkills.BeginUpdate();

            try
            {
                // Clear and add the columns
                lvSkills.Columns.Clear();
                lvSkills.Items.Clear();

                foreach (PlanColumnSettings column in m_columns.Where(x => x.Visible))
                {
                    // Add the column
                    ColumnHeader header = lvSkills.Columns.Add(column.Column.GetHeader(), column.Width);
                    header.Tag = column;

                    // Add a temporary column when there is a pluggable (implants calc, attributes optimizer, etc)
                    if (m_pluggable == null || column.Column != PlanColumn.TrainingTime)
                        continue;

                    header = lvSkills.Columns.Add(PluggableColumn, "Diff with Calc Atts", -2);
                    header.Tag = null;
                }

                // Update the items
                UpdateSkillList();

                // Update the sort arrows
                UpdateSortVisualFeedback();

                // Adjust the size of the columns
                AdjustColumns();
            }
            finally
            {
                lvSkills.EndUpdate();
                m_isUpdatingColumns = false;
            }
        }

        /// <summary>
        /// Stores the selection to a dictionary and returns it. Dictionary keys are the tags' hash codes.
        /// </summary>
        /// <returns></returns>
        private Dictionary<int, bool> StoreSelection()
        {
            Dictionary<int, bool> c = new Dictionary<int, bool>();

            // Compute and store a string ID for every item
            foreach (ListViewItem lvi in lvSkills.SelectedItems)
            {
                c[lvi.Tag.GetHashCode()] = true;
            }

            return c;
        }

        /// <summary>
        /// Restores the selection from a dictionary where keys are tags' hash codes.
        /// </summary>
        /// <param name="c"></param>
        private void RestoreSelection(IDictionary<int, bool> c)
        {
            for (int i = lvSkills.Items.Count - 1; i >= 0; i--)
            {
                // Retrieve this item's tag hash code
                ListViewItem lvi = lvSkills.Items[i];
                int hashCode = lvi.Tag.GetHashCode();

                // Check whether this id must be selected
                bool selected;
                if (c.TryGetValue(hashCode, out selected))
                    c.Remove(hashCode);

                lvi.Selected = selected;
            }
        }

        /// <summary>
        /// Every 30s a timer ticks and causes the list to refresh.
        /// If there are obsolete entries user gets asked how to handle them.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tmrAutoRefresh_Tick(object sender, EventArgs e)
        {
            PlanWindow planWindow = ParentForm as PlanWindow;

            UpdateListViewItems();
            planWindow?.CheckObsoleteEntries();
        }

        /// <summary>
        /// Removes all obsolete entries and rebuilds the plan.
        /// </summary>
        public void ClearObsoleteEntries(ObsoleteRemovalPolicy policy)
        {
            m_plan.CleanObsoleteEntries(policy);
            UpdateDisplayPlan();
        }

        /// <summary>
        /// Updates the status bar.
        /// </summary>
        private void UpdateStatusBar()
        {
            PlanWindow planWindow = ParentForm as PlanWindow;

            // 0 items selected or
            // 1 items are selected and status bar only updates on multi-select
            if ((lvSkills.SelectedItems.Count < 1) ||
                ((lvSkills.SelectedItems.Count == 1) && Settings.UI.PlanWindow.OnlyShowSelectionSummaryOnMultiSelect))
            {
                planWindow?.UpdateStatusBar();
                return;
            }

            // Multi-selection
            int entriesCount = SelectedEntries.Count();

            // We compute the training time
            TimeSpan selectedTrainingTime = SelectedEntries
                .Aggregate(TimeSpan.Zero, (current, entry) => current.Add(entry.TrainingTime));

            planWindow?.UpdateSkillStatusLabel(true, entriesCount, UniqueSkillsCount);
            planWindow?.UpdateTimeStatusLabel(true, entriesCount, selectedTrainingTime);
            planWindow?.UpdateCostStatusLabel(true, SkillBooksCost, NotKnownSkillBooksCost);
            planWindow?.UpdateSkillPointsStatusLabel(true, entriesCount, TotalSkillPoints);
        }

        /// <summary>
        /// Updates the implant set.
        /// </summary>
        private async Task UpdateImplantSet()
        {
            m_plan.ChosenImplantSet = cbChooseImplantSet.SelectedItem as ImplantSet;
            m_lastImplantSetIndex = cbChooseImplantSet.SelectedIndex;
            DisplayPlan.ChosenImplantSet = m_plan.ChosenImplantSet;

            Task updateOnImplantSetChange = m_pluggable?.UpdateOnImplantSetChange();
            if (updateOnImplantSetChange != null)
                await updateOnImplantSetChange;
        }

        #endregion


        #region Pluggable management

        /// <summary>
        /// Connects a window implementing <see cref="IPlanOrderPluggable"/> to this window to enable displaying the training time differences.
        /// </summary>
        /// <param name="pluggable"></param>
        internal void ShowWithPluggable(IPlanOrderPluggable pluggable)
        {
            lvSkills.BeginUpdate();
            try
            {
                // Update columns when a new pluggable is inserted
                if (m_pluggable == null)
                {
                    m_pluggable = pluggable;
                    pluggable.Disposed += pluggable_Disposed;
                    UpdateListColumns();
                    return;
                }
                
                // Updates the list view
                UpdateListViewItems();
            }
            finally
            {
                lvSkills.EndUpdate();
            }
        }

        /// <summary>
        /// Once the pluggable window is disposed, we hide the training time difference again.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void pluggable_Disposed(object o, EventArgs e)
        {
            m_pluggable.Disposed -= pluggable_Disposed;
            m_pluggable = null;
            UpdateListColumns();
        }

        #endregion


        #region Generic helper methods

        /// <summary>
        /// Updates the plan editor's skill selection control.
        /// </summary>
        internal void UpdatePlanEditorSkillSelection()
        {
            skillSelectControl.UpdateContent();
        }

        /// <summary>
        /// Sets the plan editor's skill selection control selected skill.
        /// </summary>
        /// <param name="skill">The skill.</param>
        internal void SetPlanEditorSkillSelectorSelectedSkill(Skill skill)
        {
            skillSelectControl.SelectedSkill = skill;
        }

        /// <summary>
        /// From an entry of the display plan, retrieve the entry of the base plan.
        /// </summary>
        /// <param name="displayEntry"></param>
        /// <returns></returns>
        private PlanEntry GetOriginalEntry(ISkillLevel displayEntry) => m_plan.GetEntry(displayEntry.Skill, displayEntry.Level);

        /// <summary>
        /// Gets the plan entry attached to the given item.
        /// </summary>
        /// <param name="lvi"></param>
        /// <returns></returns>
        private static PlanEntry GetPlanEntry(ListViewItem lvi) => lvi?.Tag as PlanEntry;

        /// <summary>
        /// Gets the first selected item which has a plan entry as a tag.
        /// </summary>
        /// <returns></returns>
        private PlanEntry GetFirstSelectedEntry() => lvSkills.SelectedItems[0].Tag as PlanEntry;

        /// <summary>
        /// Gets an enumeration over the selected entries.
        /// </summary>
        private IEnumerable<PlanEntry> SelectedEntries => lvSkills.SelectedItems.Cast<ListViewItem>().Select(x => x.Tag).OfType<PlanEntry>();

        #endregion


        #region List items and columns reordering/resizing

        /// <summary>
        /// When the user manually resizes a column, we make sure to update the column preferences.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvSkills_ColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
        {
            if (m_isUpdatingColumns)
                return;

            if (m_columns.Count <= e.ColumnIndex)
                return;

            m_columns[e.ColumnIndex].Width = lvSkills.Columns[e.ColumnIndex].Width;
            Settings.UI.PlanWindow.Columns.Clear();
            Settings.UI.PlanWindow.Columns.AddRange(ExportColumnSettings());
        }

        /// <summary>
        /// When the user click moves up, we move the list view items and rebuild the plan from 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsbMoveUp_Click(object sender, EventArgs e)
        {
            List<ListViewItem> items = lvSkills.Items.Cast<ListViewItem>().ToList();

            // Skip the head
            int index = 0;
            while (index < items.Count)
            {
                if (!items[index].Selected)
                    break;
                index++;
            }

            // Move up the following items
            while (index < items.Count)
            {
                ListViewItem item = items[index];
                if (item.Selected)
                {
                    items.RemoveAt(index);
                    items.Insert(index - 1, item);
                }
                index++;
            }

            // Rebuild the plan
            RebuildPlanFromListViewOrder(items);
        }

        /// <summary>
        /// When the user click moves down, we move the list view items and rebuild the plan from 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsbMoveDown_Click(object sender, EventArgs e)
        {
            List<ListViewItem> items = lvSkills.Items.Cast<ListViewItem>().ToList();

            // Skip the tail
            int index = items.Count - 1;
            while (index >= 0)
            {
                if (!items[index].Selected)
                    break;
                index--;
            }

            // Move up the following items
            while (index >= 0)
            {
                ListViewItem item = items[index];
                if (item.Selected)
                {
                    items.RemoveAt(index);
                    items.Insert(index + 1, item);
                }
                index--;
            }

            // Rebuild the plan
            RebuildPlanFromListViewOrder(items);
        }

        /// <summary>
        /// Moves the currently selected skill to the top of the plan
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MoveToTopMenuItem_Click(object sender, EventArgs e)
        {
            // Extract the list and the selected item
            List<ListViewItem> items = lvSkills.Items.Cast<ListViewItem>().ToList();
            int index = items.First(x => x.Selected).Index;
            ListViewItem item = items[index];

            // Remove the item from the list, and add it at the top
            items.RemoveAt(index);
            items.Insert(0, item);

            RebuildPlanFromListViewOrder(items);
        }

        /// <summary>
        /// When an item is moved acrosss the listview, we rebuild the entire plan from the listview.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvSkills_ListViewItemsDragged(object sender, EventArgs e)
        {
            RebuildPlanFromListViewOrder(lvSkills.Items);
        }

        /// <summary>
        /// Rebuild the plan from the list view items.
        /// </summary>
        /// <param name="items"></param>
        private void RebuildPlanFromListViewOrder(IEnumerable items)
        {
            // Create the new entries
            List<PlanEntry> entries = new List<PlanEntry>();

            RemappingPoint remapping = null;
            foreach (ListViewItem item in items)
            {
                PlanEntry entry = item.Tag as PlanEntry;
                if (entry != null)
                {
                    entry.Remapping = remapping;
                    entries.Add(entry);
                    remapping = null;
                    continue;
                }

                remapping = item.Tag as RemappingPoint;
            }

            // Since the list is not sorted anymore, we disable/hide the sort buttons and feedback
            m_plan.SortingPreferences.Order = ThreeStateSortOrder.None;
            m_plan.SortingPreferences.GroupByPriority = false;
            UpdateSortVisualFeedback();

            // Fetch them to the plan
            m_plan.RebuildPlanFrom(entries);
        }

        /// <summary>
        /// Rebuild the column settings from the currently displayed columns.
        /// </summary>
        public void ImportColumnSettings(IEnumerable<PlanColumnSettings> columns)
        {
            // Recreate the columns
            m_columns.Clear();
            m_columns.AddRange(columns);

            // Update the UI
            UpdateListColumns();
        }

        /// <summary>
        /// Rebuild the column settings from the currently displayed columns.
        /// </summary>
        public IEnumerable<PlanColumnSettings> ExportColumnSettings()
        {
            // Recreate the columns
            List<PlanColumnSettings> newList = new List<PlanColumnSettings>();

            // Add the visible columns at the beggining
            foreach (ColumnHeader columnHeader in lvSkills.Columns.Cast<ColumnHeader>().OrderBy(x => x.DisplayIndex))
            {
                // Retrieve the column and skip if null
                PlanColumnSettings column = columnHeader.Tag as PlanColumnSettings;
                if (column == null)
                    continue;

                if (column.Width > -1)
                    column.Width = columnHeader.Width;
                column.Visible = true;
                newList.Add(column);
            }

            // Then the non-displayed ones
            foreach (PlanColumnSettings column in m_columns.Where(x => !newList.Contains(x)))
            {
                column.Visible = false;
                newList.Add(column);
            }

            return newList;
        }

        #endregion


        #region Entries removal

        /// <summary>
        /// Remove all the selected entries when one or more get selected.
        /// </summary>
        private void RemoveSelectedEntries()
        {
            if (lvSkills.SelectedItems.Count == 0)
                return;

            if (lvSkills.SelectedItems.Count == 1 && lvSkills.SelectedItems[0].Tag is RemappingPoint)
            {
                tsbToggleRemapping_Click(null, null);
                return;
            }

            IPlanOperation operation = PrepareSelectionRemoval();
            if (operation == null)
                return;

            PlanWindow planWindow = ParentForm as PlanWindow;
            if (planWindow == null)
                return;

            PlanHelper.SelectPerform(new PlanToOperationWindow(operation), planWindow, operation);
        }

        /// <summary>
        /// Prepares the selection removal.
        /// </summary>
        /// <returns></returns>
        private IPlanOperation PrepareSelectionRemoval()
        {
            IEnumerable<PlanEntry> entriesToRemove = lvSkills.SelectedItems.Cast<ListViewItem>()
                .Select(x => x.Tag).OfType<PlanEntry>();
            IPlanOperation operation = m_plan.TryRemoveSet(entriesToRemove);
            return operation;
        }

        #endregion


        #region Sorting

        /// <summary>
        /// The user toggled the "group priorities" button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsSortPriorities_Clicked(object sender, EventArgs e)
        {
            m_plan.SortingPreferences.GroupByPriority = tsSortPriorities.Checked;
            UpdateDisplayPlan();
        }

        /// <summary>
        /// When the user clicks a column header, we sort things up.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvSkills_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ColumnHeader column = lvSkills.Columns[e.Column];
            PlanEntrySort criteria = GetPlanSort(column);

            // Update sort order
            if (criteria != PlanEntrySort.None)
            {
                if (m_plan.SortingPreferences.Criteria == criteria)
                {
                    switch (m_plan.SortingPreferences.Order)
                    {
                        case ThreeStateSortOrder.None:
                            m_plan.SortingPreferences.Order = ThreeStateSortOrder.Ascending;
                            break;
                        case ThreeStateSortOrder.Ascending:
                            m_plan.SortingPreferences.Order = ThreeStateSortOrder.Descending;
                            break;
                        case ThreeStateSortOrder.Descending:
                            m_plan.SortingPreferences.Order = ThreeStateSortOrder.None;
                            break;
                    }
                }
                else
                    m_plan.SortingPreferences.Order = ThreeStateSortOrder.Ascending;
            }

            // Updates the criteria
            m_plan.SortingPreferences.Criteria = criteria;

            // Updates UI and display plan
            UpdateSortVisualFeedback();
            UpdateDisplayPlan();
        }

        /// <summary>
        /// Gets a column by the given sort key. Null if not found or "none".
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        private ColumnHeader GetColumn(PlanEntrySort criteria) => criteria == PlanEntrySort.None
           ? null
           : lvSkills.Columns.Cast<ColumnHeader>().FirstOrDefault(header => GetPlanSort(header) == criteria);

        /// <summary>
        /// Gets the sort key for the given column header.
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        private static PlanEntrySort GetPlanSort(ColumnHeader header)
        {
            if (header.Tag == null)
                return PlanEntrySort.TimeDifference;

            PlanColumnSettings ct = (PlanColumnSettings)header.Tag;
            switch (ct.Column)
            {
                case PlanColumn.SkillName:
                    return PlanEntrySort.Name;
                case PlanColumn.Cost:
                    return PlanEntrySort.Cost;
                case PlanColumn.TrainingTime:
                    return PlanEntrySort.TrainingTime;
                case PlanColumn.TrainingTimeNatural:
                    return PlanEntrySort.TrainingTimeNatural;
                case PlanColumn.PrimaryAttribute:
                    return PlanEntrySort.PrimaryAttribute;
                case PlanColumn.SecondaryAttribute:
                    return PlanEntrySort.SecondaryAttribute;
                case PlanColumn.Priority:
                    return PlanEntrySort.Priority;
                case PlanColumn.SkillGroup:
                    return PlanEntrySort.SkillGroupDuration;
                case PlanColumn.PlanGroup:
                    return PlanEntrySort.PlanGroup;
                case PlanColumn.PercentComplete:
                    return PlanEntrySort.PercentCompleted;
                case PlanColumn.SkillRank:
                    return PlanEntrySort.Rank;
                case PlanColumn.SPPerHour:
                    return PlanEntrySort.SPPerHour;
                case PlanColumn.Notes:
                    return PlanEntrySort.Notes;
                case PlanColumn.PlanType:
                    return PlanEntrySort.PlanType;
                case PlanColumn.SkillPointsRequired:
                    return PlanEntrySort.SkillPointsRequired;
                default:
                    return PlanEntrySort.None;
            }
        }

        /// <summary>
        /// Updates the sort visual feedback for the specified column.
        /// </summary>
        /// <remarks>
        /// The ColumnHeader.ImageIndex has a bug under Vista that
        /// causes the value to be set to 0 if you set it to -1,
        /// resulting in the wrong icon being selected for the sort:
        /// https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=395739
        /// </remarks>
        private void UpdateSortVisualFeedback()
        {
            // Updates the menu icons on the left toolbar
            tsSortPriorities.Checked = m_plan.SortingPreferences.GroupByPriority;

            ColumnHeader columnWithSortCriteria = GetColumn(m_plan.SortingPreferences.Criteria);
            foreach (ColumnHeader columnHeader in lvSkills.Columns.Cast<ColumnHeader>())
            {
                if (columnWithSortCriteria == columnHeader)
                {
                    if (m_plan.SortingPreferences.Criteria == PlanEntrySort.None ||
                        m_plan.SortingPreferences.Order == ThreeStateSortOrder.None)
                    {
                        columnHeader.ImageIndex = 6;
                        continue;
                    }

                    columnHeader.ImageIndex = m_plan.SortingPreferences.Order == ThreeStateSortOrder.Ascending
                        ? ArrowUpIndex
                        : ArrowDownIndex;
                }
                else
                    columnHeader.ImageIndex = 6;
            }
        }

        /// <summary>
        /// Adjusts the columns.
        /// </summary>
        private void AdjustColumns()
        {
            // Find the index of the pluggable column
            int pluggableColumnIndex = lvSkills.Columns.IndexOfKey(PluggableColumn);

            foreach (ColumnHeader column in lvSkills.Columns)
            {
                // Adjust the column index when we are past the pluggable column
                // in order to avoid an ArgumentOutOfRangeException
                int columnIndex = column.Index;
                if (pluggableColumnIndex != -1 && column.Index > pluggableColumnIndex)
                    columnIndex--;

                if (m_columns[columnIndex].Width == -1)
                    m_columns[columnIndex].Width = -2;

                column.Width = m_columns[columnIndex].Width;

                // Due to .NET design we need to prevent the last colummn to resize to the right end

                // Return if it's not the last column and not set to auto-resize
                if (column.Index != lvSkills.Columns.Count - 1 || m_columns[columnIndex].Width != -2)
                    continue;

                const int Pad = 4;

                // Calculate column header text width with padding
                int columnHeaderWidth = TextRenderer.MeasureText(column.Text, Font).Width + Pad * 2;

                // If there is an image assigned to the header, add its width with padding
                if (lvSkills.SmallImageList != null && column.ImageIndex > -1)
                    columnHeaderWidth += lvSkills.SmallImageList.ImageSize.Width + Pad;

                // Calculate the width of the header and the items of the column
                int columnMaxWidth = lvSkills.Columns[columnIndex].ListView.Items.Cast<ListViewItem>().Select(
                    item => TextRenderer.MeasureText(item.SubItems[columnIndex].Text, Font).Width).Concat(
                        new[] { columnHeaderWidth }).Max() + Pad + 1;

                // Assign the width found
                column.Width = columnMaxWidth;
            }
        }

        #endregion


        #region Context Menu

        /// <summary>
        /// When the context menu is opened, we update the status of the menus.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void contextMenu_Opening(object sender, CancelEventArgs e)
        {
            e.Cancel = !m_plan.Any();

            if (e.Cancel)
                return;

            PlanEntry entry = lvSkills.SelectedItems.Count > 0 ? GetPlanEntry(lvSkills.SelectedItems[0]) : null;

            // By default, all hidden
            foreach (ToolStripItem item in contextMenu.Items)
            {
                item.Visible = false;
            }

            // Nothing more to do when nothing selected
            if (lvSkills.SelectedItems.Count == 0)
                return;

            // "Change note"
            miChangeNote.Visible = entry != null;
            miChangeNote.Text = @"View/Change Note...";

            miChangePriority.Visible =
                changeMenuSeparator.Visible = entry != null;

            miCopyTo.Visible = entry != null;

            // Reset text in case of previous multiple selection
            miRemoveFromPlan.Text = @"Remove from Plan";
            
            // When there is only one selected item...
            if (lvSkills.SelectedItems.Count == 1)
            {
                // When the selected item is a remapping, only "remove from plan" is visible
                if (entry == null)
                {
                    miRemoveFromPlan.Visible = true;
                    return;
                }

                // Enable other items
                miShowInSkillBrowser.Visible = true;
                miShowInSkillExplorer.Visible = true;
                showInMenuSeparator.Visible = true;

                // "Change Planned Level"
                bool showChangeLevel = SetChangeLevelMenu();
                miChangeLevel.Visible = showChangeLevel;

                // If "Change Planned Level" hidden, "remove from plan" is visible 
                miRemoveFromPlan.Visible = !showChangeLevel;

                // "Move to top"
                bool showMoveToTop = lvSkills.Items.IndexOf(lvSkills.SelectedItems[0]) > 1;
                MoveToTopMenuItem.Visible = showMoveToTop;

                planMenuSeparator.Visible = showMoveToTop || !showChangeLevel;

                // "Plan groups"
                if (entry.PlanGroups.Count > 0)
                {
                    miPlanGroups.Visible = true;
                    copyMenuSeparator.Visible = true;

                    List<string> planGroups = new List<string>(entry.PlanGroups);
                    planGroups.Sort();

                    miPlanGroups.DropDownItems.Clear();
                    miPlanGroups.DropDownItems.AddRange(planGroups.Select(CreateMenuItemControl).ToArray());
                }
            }
                // Multiple items selected
            else
            {
                miRemoveFromPlan.Visible = true;
                planMenuSeparator.Visible = true;
                IPlanOperation operation = PrepareSelectionRemoval();
                if (PlanHelper.RequiresWindow(operation))
                    miRemoveFromPlan.Text += @"...";
            }

            // "Mark as owned"
            IList<Skill> skills = lvSkills.SelectedItems.Cast<ListViewItem>()
                .Select(x => x.Tag).OfType<PlanEntry>().Select(x => x.CharacterSkill).ToList();

            if (skills.Any(x => !x.IsKnown))
            {
                miMarkOwned.Text = skills.Any(x => !x.IsOwned) ? "Mark as owned" : "Mark as unowned";
                miMarkOwned.Visible = true;
                markOwnedMenuSeaprator.Visible = true;
            }
            else
            {
                miMarkOwned.Text = @"Mark as owned";
                miMarkOwned.Visible = false;
            }
        }

        /// <summary>
        /// Creates the menu item control.
        /// </summary>
        /// <param name="planGroup">The plan group.</param>
        /// <returns></returns>
        private ToolStripItem CreateMenuItemControl(string planGroup)
        {
            ToolStripMenuItem item = new ToolStripMenuItem(planGroup);
            item.Width = TextRenderer.MeasureText(planGroup, item.Font).Width;
            item.Click += planGroupMenu_Click;
            return item;
        }

        /// <summary>
        /// Update the status of the "Plan to level N" menu entries.
        /// </summary>
        /// <returns>True if at least one of the entries could be set.</returns>
        private bool SetChangeLevelMenu()
        {
            PlanEntry pe = GetFirstSelectedEntry();

            // Scroll through levels (and menus, one per level)
            bool result = false;
            for (int level = 0; level <= 5; level++)
            {
                m_plan.UpdatesRegularPlanToMenu(miChangeLevel.DropDownItems[level], pe.CharacterSkill, level);
                result |= miChangeLevel.DropDownItems[level].Enabled;
            }

            return result;
        }

        /// <summary>
        /// Context menu > "Select entries from group..." > Groupname
        /// Selects all the items which belong to the same group.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void planGroupMenu_Click(object sender, EventArgs e)
        {
            string planGroup = ((ToolStripButton)sender).Text;
            foreach (ListViewItem item in lvSkills.Items)
            {
                item.Selected = GetPlanEntry(item).PlanGroups.Contains(planGroup);
            }
        }

        /// <summary>
        /// Context menu > Show skill in browser.
        /// Displays the selected entry's skill in the skill browser.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void miShowInSkillBrowser_Click(object sender, EventArgs e)
        {
            Skill skill = ((PlanEntry)lvSkills.SelectedItems[0]?.Tag)?.CharacterSkill;

            // Open the skill browser
            PlanWindow planWindow = ParentForm as PlanWindow;
            planWindow?.ShowSkillInBrowser(skill);
        }

        /// <summary>
        /// Context menu > Show skill in explorer.
        /// Displays the selected entry's skill in the skill explorer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void miShowInSkillExplorer_Click(object sender, EventArgs e)
        {
            Skill skill = ((PlanEntry)lvSkills.SelectedItems[0]?.Tag)?.CharacterSkill;
            
            // Open the skill explorer
            SkillExplorerWindow.ShowSkillExplorerWindow(m_character, m_plan).ShowSkillInExplorer(skill);
        }

        /// <summary>
        /// Context menu > Remove from plan.
        /// Removes the seleted entry or remapping point from the plan.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void miRemoveFromPlan_Click(object sender, EventArgs e)
        {
            RemoveSelectedEntries();
        }

        /// <summary>
        /// Context menu > Change priority.
        /// Opens a dialog box to edit the priorities. Check for concflicts and asks the user when needed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void miChangePriority_Click(object sender, EventArgs e)
        {
            IList<PlanEntry> entries = SelectedEntries.ToList();
            using (PlanPrioritiesEditorWindow form = new PlanPrioritiesEditorWindow())
            {
                // Gets the entry's priority (or default if more than one item selected)
                form.Priority = PlanEntry.DefaultPriority;
                if (lvSkills.SelectedItems.Count == 1)
                {
                    PlanEntry pe = GetPlanEntry(lvSkills.SelectedItems[0]);
                    if (pe != null)
                        form.Priority = pe.Priority;
                }

                // User canceled ?
                DialogResult dr = form.ShowDialog();
                if (dr == DialogResult.Cancel)
                    return;

                // Update priorities, while performing backup for subsequent check
                if (m_plan.TrySetPriority(DisplayPlan, entries, form.Priority))
                    return;

                ShowPriorityDialogBox(entries, form);
            }
        }

        /// <summary>
        /// Shows the priority dialog box.
        /// </summary>
        /// <param name="entries">The entries.</param>
        /// <param name="form">The form.</param>
        private void ShowPriorityDialogBox(IEnumerable<PlanEntry> entries, PlanPrioritiesEditorWindow form)
        {
            bool showDialog = Settings.UI.PlanWindow.PrioritiesMsgBox.ShowDialogBox;

            // User wishes the dialog to be displayed
            if (showDialog)
            {
                string text = String.Concat("This would result in a priority conflict.",
                                            " (Either pre-requisites with a lower priority or dependant skills with a higher priority).\r\n\r\n",
                                            "Click Yes if you wish to do this and adjust the other skills\r\nor No if you do not wish to change the priority.");
                const string CaptionText = "Priority Conflict";
                const string CbOptionText = "Do not show this dialog again";

                // Shows the custom dialog box
                DialogResult dialogResult = MessageBoxCustom.Show(this, text, CaptionText, CbOptionText, MessageBoxButtons.YesNo,
                                                                  MessageBoxIcon.Exclamation);
                Settings.UI.PlanWindow.PrioritiesMsgBox.ShowDialogBox = !MessageBoxCustom.CheckBoxChecked;

                // When the checkbox is checked we store the dialog result
                if (MessageBoxCustom.CheckBoxChecked)
                    Settings.UI.PlanWindow.PrioritiesMsgBox.DialogResult = dialogResult;

                if (dialogResult == DialogResult.Yes)
                    m_plan.SetPriority(DisplayPlan, entries, form.Priority);
            }
                // User wishes the dialog not to be displayed and has set the dialog result to "Yes"
            else if (Settings.UI.PlanWindow.PrioritiesMsgBox.DialogResult == DialogResult.Yes)
                m_plan.SetPriority(DisplayPlan, entries, form.Priority);
        }

        /// <summary>
        /// Context menu > Change note.
        /// Opens a box to change the plan's notes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void miChangeNote_Click(object sender, EventArgs e)
        {
            IList<PlanEntry> entries = SelectedEntries.ToList();
            if (!entries.Any())
                return;

            // We get the current skill's note and call the note editor window with this initial value
            string noteText = entries.First().Notes;
            string title = entries.Count() == 1 ? entries.First().Skill.ToString() : "Selected entries";
            using (PlanNotesEditorWindow f = new PlanNotesEditorWindow(title))
            {
                f.NoteText = noteText;
                DialogResult dr = f.ShowDialog();
                if (dr == DialogResult.Cancel)
                    return;

                noteText = f.NoteText;
            }

            // We update every item
            foreach (PlanEntry entry in entries)
            {
                entry.Notes = noteText;
            }

            m_plan.RebuildPlanFrom(DisplayPlan, true);
        }

        /// <summary>
        /// Context > Copy to new plan...
        /// Opens a dialog to prompt the user for a name and create a plan with the selected entries.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void miCopyToNewPlan_Click(object sender, EventArgs e)
        {
            IList<PlanEntry> entries = SelectedEntries.ToList();
            if (!entries.Any())
                return;

            // Ask the user for a new name
            string planName,
                   planDescription;
            using (NewPlanWindow npw = new NewPlanWindow())
            {
                DialogResult dr = npw.ShowDialog();
                if (dr == DialogResult.Cancel)
                    return;
                planName = npw.PlanName;
                planDescription = npw.PlanDescription;
            }

            // Create a new plan
            Plan newPlan = new Plan(m_character) { Name = planName, Description = planDescription };
            IPlanOperation operation = newPlan.TryAddSet(entries, $"Exported from {m_plan.Name}");
            operation.Perform();

            // Add plan and save
            m_character.Plans.Add(newPlan);
        }

        /// <summary>
        /// Context > Mark as owned/unowned.
        /// Toggle the owned flag for the selected skills. 
        /// When multiple entries are selected and have different flags, we mark them as all owned.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void miMarkOwned_Click(object sender, EventArgs e)
        {
            bool unowned = SelectedEntries.All(x => !x.CharacterSkill.IsOwned);

            using (m_plan.SuspendingEvents())
            {
                foreach (PlanEntry entry in SelectedEntries)
                {
                    entry.CharacterSkill.IsOwned = unowned;
                }
            }

            // We update the skill tree
            skillSelectControl.UpdateContent();

            // Update also the skill browser
            PlanWindow planWindow = ParentForm as PlanWindow;
            planWindow?.UpdateSkillBrowser();

            // Update the Owned Skill books window if open
            OwnedSkillBooksWindow ownedSkillBooksWindow =
                WindowsFactory.GetByTag<OwnedSkillBooksWindow, Character>((Character)m_plan.Character);

            ownedSkillBooksWindow?.UpdateList();
        }

        /// <summary>
        /// Context > Change planned level > Level N
        /// Change the planned level, or remove if 0
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void miChangeToLevel_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = sender as ToolStripMenuItem;

            IPlanOperation operation = menu?.Tag as IPlanOperation;

            if (operation == null)
                return;

            PlanWindow planWindow = ParentForm as PlanWindow;
            if (planWindow == null)
                return;

            PlanHelper.SelectPerform(new PlanToOperationWindow(operation), planWindow, operation);
        }

        /// <summary>
        /// Context > Copy selected skills to Clipboard
        /// Copy selected entries to clipoboard 
        /// for importation in EVE client Skill Queue
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void miCopySelectedToClipboard_Click(object sender, EventArgs e)
        {
            // Create a new plan
            Plan newPlan = new Plan(m_character);
            IPlanOperation operation = newPlan.TryAddSet(SelectedEntries, $"Exported from {m_plan.Name}");
            operation.Perform();

            // Prompt the user for settings. When null, the user cancelled
            PlanExportSettings settings = UIHelper.PromptUserForPlanExportSettings(newPlan);
            if (settings == null)
                return;
            
            string output = PlanIOHelper.ExportAsText(newPlan, settings);

            // Copy the result to the clipboard
            try
            {
                Clipboard.Clear();
                Clipboard.SetText(output);

                MessageBox.Show(@"The selected entries have been copied to the clipboard.",
                    @"Plan Copied",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (ExternalException ex)
            {
                ExceptionHandler.LogException(ex, true);

                MessageBox.Show(@"The copy to clipboard has failed. You may retry later.",
                    @"Plan Copy Failure",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        #endregion


        #region Drag'n drop from outer controls (inner drag'n drop is in reordering region)

        /// <summary>
        /// When the user drop a skill on the list, we plans it to the next unplanned level.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvSkills_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                // Quits if the button is never the left one nor the right one
                if (m_dragButton != MouseButtons.Left && m_dragButton != MouseButtons.Right)
                    return;

                // Checks there is a skill
                Skill dragSkill = GetDraggingSkill(e);
                if (dragSkill == null)
                    return;

                // Gets the item and returns if none created (already on lv5)
                ListViewItem newItem = CreatePlanItemForSkill(dragSkill);
                if (newItem == null)
                    return;

                // By default, drop index is at the end of the list
                int dragIndex = lvSkills.Items.Count;

                // If the user is dropping on an item, infere the drag index from this item's index
                Point cp = lvSkills.PointToClient(new Point(e.X, e.Y));
                ListViewItem hoverItem = lvSkills.GetItemAt(cp.X, cp.Y);
                if (hoverItem != null)
                {
                    dragIndex = hoverItem.Index;
                    Rectangle hoverBounds = hoverItem.GetBounds(ItemBoundsPortion.ItemOnly);

                    // If the user is dropping on the lower half of the item, increase the dragging index
                    if (cp.Y > hoverBounds.Top + hoverBounds.Height / 2)
                        dragIndex++;
                }

                // Performs the insertion
                lvSkills.Items.Insert(dragIndex, newItem);
                RebuildPlanFromListViewOrder(lvSkills.Items);
            }
            finally
            {
                // Clean up our mess
                lvSkills.ClearDropMarker();
                e.Effect = DragDropEffects.None;
                m_dragButton = MouseButtons.None;
            }
        }

        /// <summary>
        /// When the user drags over the skill list, updates the drop marker and dragging button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvSkills_DragOver(object sender, DragEventArgs e)
        {
            // Checks there is a dragged skill
            Skill dragSkill = GetDraggingSkill(e);
            if (dragSkill == null)
                return;

            // Updates the dragging button
            SetDragMouseButton(e);

            // Gets the hovered item
            e.Effect = DragDropEffects.Move;
            Point cp = lvSkills.PointToClient(new Point(e.X, e.Y));
            ListViewItem hoverItem = lvSkills.GetItemAt(cp.X, cp.Y);

            // Updates the drop marker below the hovered item.
            if (hoverItem != null)
            {
                Rectangle hoverBounds = hoverItem.GetBounds(ItemBoundsPortion.ItemOnly);
                lvSkills.DrawDropMarker(hoverItem.Index, cp.Y > hoverBounds.Top + hoverBounds.Height / 2);
            }
            else
                lvSkills.ClearDropMarker();
        }

        /// <summary>
        /// When the user begins a drag/drop operation, updates the drag/drop button and cursor.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvSkills_DragEnter(object sender, DragEventArgs e)
        {
            // Sets up the drag button
            SetDragMouseButton(e);

            // Gets the dragging skill set up by the source control
            Skill dragSkill = GetDraggingSkill(e);
            if (dragSkill != null)
                e.Effect = DragDropEffects.Move;
        }

        /// <summary>
        /// When the user leaves a drag/drop operation, clear the drop marker and such.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvSkills_DragLeave(object sender, EventArgs e)
        {
            m_dragButton = MouseButtons.None;
            lvSkills.ClearDropMarker();
        }

        /// <summary>
        /// Looks for a <see cref="Skill"/> in the data of the provided event arguments.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private static Skill GetDraggingSkill(DragEventArgs e)
        {
            if (e.Data.GetDataPresent("System.Windows.Forms.TreeNode"))
                return (Skill)((TreeNode)e.Data.GetData("System.Windows.Forms.TreeNode")).Tag;

            return null;
        }

        /// <summary>
        /// Creates a plan entry and a list view item for it, from the given skill.
        /// </summary>
        /// <param name="skill"></param>
        /// <returns></returns>
        private ListViewItem CreatePlanItemForSkill(Skill skill)
        {
            // Gets the planned level of the skill.
            Int64 newLevel = m_plan.GetPlannedLevel(skill) + 1;
            if (skill.Level >= newLevel)
                newLevel = skill.Level + 1;

            // Quits if already on lv5
            if (newLevel > 5)
                return null;

            // Creates the plan entry and list item for this level
            PlanEntry newEntry = new PlanEntry(m_plan, skill, newLevel);
            ListViewItem newItem = new ListViewItem(newEntry.ToString()) { Tag = newEntry };

            return newItem;
        }

        /// <summary>
        /// Gets the mouse button used to drag
        /// </summary>
        /// <param name="e"></param>
        private void SetDragMouseButton(DragEventArgs e)
        {
            if ((e.KeyState & (int)Keys.LButton) != 0)
                m_dragButton = MouseButtons.Left;
            else if ((e.KeyState & (int)Keys.RButton) != 0)
                m_dragButton = MouseButtons.Right;
        }

        #endregion


        #region Other list events : keyboard, click, hovering, selection change

        /// <summary>
        /// On a doube-click on one of the list items, we open the skill browser.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvSkills_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (lvSkills.SelectedItems.Count != 1)
                return;

            // When the first entry is a skill, shows it in the skill browser
            if (GetFirstSelectedEntry() != null)
                miShowInSkillBrowser_Click(sender, e);
            // When it is a remapping point, edit it
            else
            {
                // Retrieves the point
                ListViewItem nextItem = lvSkills.Items[lvSkills.SelectedIndices[0] + 1];
                RemappingPoint remappingPoint = GetPlanEntry(nextItem).Remapping;

                // Display the attributes optimizer
                // if it's not already shown
                // When we click on another point the previous form closes
                if (remappingPoint != m_previousRemappingPoint)
                    WindowsFactory.GetAndCloseByTag<AttributesOptimizerWindow, PlanEditorControl>(this);

                m_previousRemappingPoint = remappingPoint;
                WindowsFactory.ShowByTag<AttributesOptimizerWindow, PlanEditorControl>(ParentForm, this, remappingPoint);
            }
        }

        /// <summary>
        /// Handles key press.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvSkills_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    miChangeNote_Click(sender, e);
                    break;
                case Keys.F9:
                    tsbToggleRemapping_Click(null, null);
                    break;
                case Keys.F5:
                    UpdateDisplayPlan();
                    break;
                case Keys.Delete:
                    RemoveSelectedEntries();
                    break;
                case Keys.A:
                    if (e.Control)
                        lvSkills.SelectAll();
                    break;
            }
        }

        /// <summary>
        /// When the user selects another entry, we do not immediately process the change but rather delay it through a timer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvSkills_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tmrSelect.Enabled)
                return;

            tmrSelect.Start();
        }

        /// <summary>
        /// When the selection update timer ticks, we process the changes caused by a selection change.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tmrSelect_Tick(object sender, EventArgs e)
        {
            tmrSelect.Stop();
            OnSelectionChanged();
        }

        /// <summary>
        /// Handles the selection change (delayed every 100ms through a timer).
        /// </summary>
        private void OnSelectionChanged()
        {
            if (lvSkills.SelectedIndices.Count == 0)
            {
                tsbMoveUp.Enabled = false;
                tsbMoveDown.Enabled = false;
                ResetPrereqMarks();
            }
            else
            {
                tsbMoveUp.Enabled = lvSkills.SelectedIndices[0] != 0;
                tsbMoveDown.Enabled = lvSkills.SelectedIndices[lvSkills.SelectedIndices.Count - 1] != lvSkills.Items.Count - 1;
                ResetPrereqMarks();
            }

            // Creates the prerequisite indicators
            foreach (ListViewItem current in lvSkills.Items)
            {
                bool isSameSkill = false;
                bool isPreRequisite = false;
                bool isPostRequisite = false;

                // Checks whether it is a prerequisite of the currently selected entry and whether we should highlight it
                if (!Settings.UI.SafeForWork && Settings.UI.PlanWindow.HighlightPrerequisites && SelectedEntries.Count() == 1)
                {
                    PlanEntry currentEntry = current.Tag as PlanEntry;
                    PlanEntry selectedEntry = lvSkills.SelectedItems[0].Tag as PlanEntry;
                    if (currentEntry != null && selectedEntry != null)
                    {
                        Int64 neededLevel;
                        if (currentEntry.Skill.HasAsImmediatePrereq(selectedEntry.Skill, out neededLevel))
                        {
                            if (currentEntry.Level == 1 && neededLevel >= selectedEntry.Level)
                                isPostRequisite = true;
                        }

                        if (selectedEntry.Skill.HasAsImmediatePrereq(currentEntry.Skill, out neededLevel))
                        {
                            if (currentEntry.Level == neededLevel)
                                isPreRequisite = true;
                        }

                        if (currentEntry.Skill == selectedEntry.Skill)
                            isSameSkill = true;
                    }
                }

                // Color depends on the entry's status
                if (current.Tag is RemappingPoint)
                    current.ImageIndex = 3;
                else if (isSameSkill)
                    current.ImageIndex = 1;
                else if (isPreRequisite)
                    current.ImageIndex = 2;
                else if (isPostRequisite)
                    current.ImageIndex = 0;
                else
                    current.ImageIndex = -1;
            }

            UpdateStatusBar();
        }

        /// <summary>
        /// Resets the prerequisites marks.
        /// </summary>
        private void ResetPrereqMarks()
        {
            // We avoid using Invalidate() as it cause the whole listview to flicker 
            foreach (ListViewItem current in lvSkills.Items)
            {
                current.ImageIndex = -1;
            }
        }

        /// <summary>
        /// When the user hovers an item, we update the tooltip.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvSkills_ItemHover(object sender, ListViewItemMouseHoverEventArgs e)
        {
            if (e.Item == null)
                return;

            // Is it an entry ?
            if (e.Item.Tag is PlanEntry)
            {
                Skill skill = GetPlanEntry(e.Item).CharacterSkill;
                StringBuilder builder = new StringBuilder(skill.Description.WordWrap(100, false));

                if (!skill.IsKnown)
                {
                    builder.AppendLine();
                    builder.Append("You do not know this skill - you ");
                    if (!skill.IsOwned)
                        builder.Append("do not ");
                    builder.Append("own the skillbook.");
                }
                e.Item.ToolTipText = builder.ToString();
            }
                // Then it is a remapping point
            else if (e.Item.Tag is RemappingPoint)
            {
                RemappingPoint point = e.Item.Tag as RemappingPoint;
                e.Item.ToolTipText = m_areRemappingPointsActive ? point.ToLongString() : "Remapping (ignored)";
            }
        }

        /// <summary>
        /// Upon column reordering we force a column settings update.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvSkills_ColumnReordered(object sender, ColumnReorderedEventArgs e)
        {
            m_columnsOrderChanged = true;
        }

        /// <summary>
        /// When the mouse gets pressed, we change the cursor.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void lvSkills_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                return;

            lvSkills.Cursor = Cursors.Default;
        }

        /// <summary>
        /// When the mouse moves over the list, we show the item's tooltip if over an item.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
        private void lvSkills_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                return;

            if (!m_plan.Any())
            {
                lvSkills.Cursor = Cursors.Default;
                return;
            }

            lvSkills.Cursor = CustomCursors.ContextMenu;

            ListViewItem item = lvSkills.GetItemAt(e.Location.X, e.Location.Y);
            if (item == null)
            {
                m_tooltip.Hide();
                return;
            }

            m_tooltip.Show(item.ToolTipText, e.Location);
        }

        /// <summary>
        /// When the mouse leaves the list, we hide the item's tooltip.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void lvSkills_MouseLeave(object sender, EventArgs e)
        {
            m_tooltip.Hide();
        }

        #endregion


        #region Other controls' handlers

        /// <summary>
        /// Left toolbar > Toggle skills panel.
        /// Display a skill list on the right.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toggleSkillsPanelButton_Click(object sender, EventArgs e)
        {
            pscPlan.Panel2Collapsed = !pscPlan.Panel2Collapsed;
            tsbToggleSkills.Checked = !pscPlan.Panel2Collapsed;
            pscPlan.SplitterDistance = pscPlan.Width - 300;
        }

        /// <summary>
        /// Left toolbar > Toggle remapping point.
        /// Adds or remove a remapping point.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsbToggleRemapping_Click(object sender, EventArgs e)
        {
            if (lvSkills.SelectedIndices.Count == 0)
                return;

            ListViewItem item = lvSkills.SelectedItems[0];
            object tag = item.Tag;

            // Remove an existing point
            if (tag is RemappingPoint)
            {
                // Selects the next item and focuses it.
                int entryIndex = lvSkills.SelectedItems[0].Index + 1;
                lvSkills.Items[entryIndex].Selected = true;
                lvSkills.Items[entryIndex].Focused = true;

                // Retrieve the original entry after this item and remove its remapping point.
                PlanEntry entry = lvSkills.Items[entryIndex].Tag as PlanEntry;
                PlanEntry originalEntry = GetOriginalEntry(entry);
                originalEntry.Remapping = null;
            }
                // Toggle on a skill
            else
            {
                // Retrieves the focused item's hash code.
                item.Focused = true;

                PlanEntry entry = tag as PlanEntry;
                PlanEntry originalEntry = GetOriginalEntry(entry);

                // Add a remapping point
                originalEntry.Remapping = originalEntry.Remapping ?? new RemappingPoint();
            }
        }

        /// <summary>
        /// Left toolbar > Toggle color key panel.
        /// Display a color key panel at the bottom.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsbColorKey_Click(object sender, EventArgs e)
        {
            pFooter.Visible = !pFooter.Visible;
            tsbColorKey.Checked = pFooter.Visible;
        }

        /// <summary>
        /// When the user clicks the "Columns Settings",
        /// we display the suggestions window and save the changes.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void columnSettingsMenuItem_Click(object sender, EventArgs e)
        {
            // Update the settings from the current columns
            using (PlanColumnSelectWindow dialog = new PlanColumnSelectWindow(ExportColumnSettings()))
            {
                if (dialog.ShowDialog() != DialogResult.OK)
                    return;

                ImportColumnSettings(dialog.Columns.Cast<PlanColumnSettings>());
                Settings.UI.PlanWindow.Columns.Clear();
                Settings.UI.PlanWindow.Columns.AddRange(ExportColumnSettings());
            }
        }

        /// <summary>
        /// Auto-Sizes the columns width.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void autoSizeColumnsMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ColumnHeader column in lvSkills.Columns.Cast<ColumnHeader>())
            {
                m_columns[column.Index].Width = -2;
            }
            UpdateListColumns();
        }

        /// <summary>
        /// When the implant set changes we update the plan.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void cbChooseImplantSet_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbChooseImplantSet.SelectedIndex == m_lastImplantSetIndex)
                return;

            await UpdateImplantSet();

            if (m_init)
                UpdateSkillList();
        }

        /// <summary>
        /// When the user clicks the "choose implant set", we update the list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbChooseImplantSet_DropDown(object sender, EventArgs e)
        {
            UpdateImplantSetList();
        }

        /// <summary>
        /// When the user doesn't select a set, display the last selected.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbChooseImplantSet_DropDownClosed(object sender, EventArgs e)
        {
            if (cbChooseImplantSet.SelectedIndex == -1)
                cbChooseImplantSet.SelectedIndex = m_lastImplantSetIndex;
        }

        #endregion
    }
}