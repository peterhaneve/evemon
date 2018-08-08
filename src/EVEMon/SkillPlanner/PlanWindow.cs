using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using EVEMon.Common;
using EVEMon.Common.Collections;
using EVEMon.Common.Constants;
using EVEMon.Common.Controls;
using EVEMon.Common.CustomEventArgs;
using EVEMon.Common.Data;
using EVEMon.Common.Enumerations;
using EVEMon.Common.Enumerations.UISettings;
using EVEMon.Common.Extensions;
using EVEMon.Common.Factories;
using EVEMon.Common.Helpers;
using EVEMon.Common.Interfaces;
using EVEMon.Common.Models;
using EVEMon.Common.SettingsObjects;

namespace EVEMon.SkillPlanner
{
    /// <summary>
    /// Represents the plan window.
    /// </summary>
    public partial class PlanWindow : EVEMonForm
    {
        private static PlanWindow s_lastActivated;

        // Blank image list for 'Safe for work' setting
        private readonly ImageList m_emptyImageList = new ImageList();

        private Plan m_plan;
        private Character m_character;


        #region Initialization and Lifecycle

        /// <summary>
        /// Initializes a new instance of the <see cref="PlanWindow"/> class.
        /// Default constructor for designer and WindowsFactory.
        /// </summary>
        public PlanWindow()
        {
            InitializeComponent();
            RememberPositionKey = "PlanWindow";

            // ToolStripLabels don't support AutoEllipsis so we user a custom renderer
            // via: http://discuss.joelonsoftware.com/default.asp?dotnet.12.597246.5
            MainStatusStrip.Renderer = new AutoEllipsisToolStripRenderer();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlanWindow"/> class.
        /// Constructor used in WindowsFactory.
        /// </summary>
        /// <param name="plan">The plan.</param>
        public PlanWindow(Plan plan)
            : this()
        {
            Plan = plan;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlanWindow"/> class.
        /// Constructor used in WindowsFactory.
        /// </summary>
        /// <param name="character">The character.</param>
        public PlanWindow(Character character)
            : this()
        {
            Character = character;
        }

        /// <summary>
        /// On load, we update the controls.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Return on design mode
            if (DesignMode || this.IsDesignModeHosted())
                return;

            m_emptyImageList.ImageSize = new Size(24, 24);
            m_emptyImageList.Images.Add(new Bitmap(24, 24));

            // Global events (unsubscribed on window closing)
            EveMonClient.PlanNameChanged += EveMonClient_PlanNameChanged;
            EveMonClient.SettingsChanged += EveMonClient_SettingsChanged;

            // Compatibility mode : Mac OS
            if (Settings.Compatibility == CompatibilityMode.Wine)
            {
                // Under Wine, the upper tool bar is not displayed
                // We move it at the top of the first tab
                Controls.Remove(upperToolStrip);
                tabControl.TabPages[0].Controls.Add(upperToolStrip);
                tabControl.TabPages[0].Controls.SetChildIndex(upperToolStrip, 0);
            }

            //Update the controls
            UpdateControlsVisibility();

            // Update the plan controls
            UpdatePlanControls();
        }

        /// <summary>
        /// On activation, we import the up-to-date plan column settings.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            if (s_lastActivated == this)
                return;

            s_lastActivated = this;

            if (m_plan != null)
                planEditor.ImportColumnSettings(Settings.UI.PlanWindow.Columns);
        }

        /// <summary>
        /// On closing, we unsubscribe the global events to help the GC.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            // Unsubscribe global events
            EveMonClient.PlanNameChanged -= EveMonClient_PlanNameChanged;
            EveMonClient.SettingsChanged -= EveMonClient_SettingsChanged;

            // Save settings if this one is the last activated and up-to-date
            if (s_lastActivated == this)
            {
                if (tabControl.TabPages.Contains(tpPlanEditor))
                {
                    Settings.UI.PlanWindow.Columns.Clear();
                    Settings.UI.PlanWindow.Columns.AddRange(planEditor.ExportColumnSettings());
                }
                s_lastActivated = null;
            }

            Settings.Save();

            // We're closing down
            if (e.CloseReason != CloseReason.ApplicationExitCall && // and Application.Exit() was not called
                e.CloseReason != CloseReason.TaskManagerClosing &&
                // and the user isn't trying to shut the program down for some reason
                e.CloseReason != CloseReason.WindowsShutDown) // and Windows is not shutting down
            {
                // Tell the loadout importation window we're closing down
                WindowsFactory.GetAndCloseByTag<LoadoutImportationWindow, Character>(m_character);

                // Tell the ship loadout window we're closing down
                WindowsFactory.GetAndCloseByTag<ShipLoadoutSelectWindow, Character>(m_character);

                // Tell the skill explorer we're closing down
                WindowsFactory.GetAndCloseByTag<SkillExplorerWindow, Character>(m_character);

                // Tell the attributes optimization window we're closing down
                WindowsFactory.GetAndCloseByTag<AttributesOptimizerOptionsWindow, PlanEditorControl>(planEditor);
                WindowsFactory.GetAndCloseByTag<AttributesOptimizerWindow, PlanEditorControl>(planEditor);

                // Tell the implant window we're closing down
                WindowsFactory.GetAndCloseByTag<ImplantCalculatorWindow, PlanEditorControl>(planEditor);
            }
        }

        #endregion


        #region Properties

        /// <summary>
        /// Gets or sets the character.
        /// </summary>
        /// <value>
        /// The character.
        /// </value>
        private Character Character
        {
            get { return m_character; }
            set
            {
                if (value == null || m_character == value)
                    return;

                m_character = value;

                // Assign the new character to the children
                skillBrowser.Character = m_character;
                certBrowser.Character = m_character;
                shipBrowser.Character = m_character;
                itemBrowser.Character = m_character;
                blueprintBrowser.Character = m_character;
            }
        }

        /// <summary>
        /// Gets the plan represented by this window.
        /// </summary>
        internal Plan Plan
        {
            get { return m_plan; }
            private set
            {
                if (m_plan == value)
                    return;

                // Should we be transforming a Data Browser to a Skill Planner?
                bool transformToPlanner = (value != null) && (m_plan == null) && (m_character != null);

                if (value == null)
                    return;

                // If the ImplantCalculator is open, assign the new plan
                ImplantCalculatorWindow implantCalcWindow = WindowsFactory.GetByTag<ImplantCalculatorWindow, PlanEditorControl>(planEditor);
                if (implantCalcWindow != null)
                    implantCalcWindow.Plan = value;

                // Assign the new plan
                m_plan = value;

                // Assign the associated character
                m_character = (Character)m_plan.Character;

                // Update any open window that is associated with this plan window
                UpdateOpenedWindows(value);

                // Check to see if one or more obsolete entries were found,
                // we do this now so the related label is immediately visible
                CheckObsoleteEntries();

                // Jump to the appropriate tab depending on whether
                // or not the plan is empty
                tabControl.SelectedTab = m_plan.Count == 0 ? tpSkillBrowser : tpPlanEditor;

                // Update title
                UpdateTitle();

                // Assign the new plan to the children
                planEditor.Plan = m_plan;
                skillBrowser.Plan = m_plan;
                certBrowser.Plan = m_plan;
                shipBrowser.Plan = m_plan;
                itemBrowser.Plan = m_plan;
                blueprintBrowser.Plan = m_plan;

                // Transform a Data Browser to a Skill Planner
                if (transformToPlanner)
                    UpdatePlanControls();

                // Check to see if one or more invalid entries were found,
                // we do this last so as not to cause problems for background update tasks.
                CheckInvalidEntries();
            }
        }

        #endregion


        #region Helper methods

        /// <summary>
        /// Updates the title.
        /// </summary>
        private void UpdateTitle()
        {
            Text = $"{m_character.Name} [{m_plan.Name}] - EVEMon Skill Planner";
        }

        /// <summary>
        /// Updates the opened windows.
        /// </summary>
        /// <param name="value">The value.</param>
        private void UpdateOpenedWindows(Plan value)
        {
            // If the EFTLoadoutImportationForm is open, assign the new plan
            // We do the check here as we need to catch the previous plan value
            LoadoutImportationWindow eftloadoutImportation = WindowsFactory.GetByTag<LoadoutImportationWindow, Character>(m_character);
            if (eftloadoutImportation != null)
                eftloadoutImportation.Plan = value;

            // If the ShipLoadoutSelectWindow is open, assign the new plan
            ShipLoadoutSelectWindow loadoutSelect = WindowsFactory.GetByTag<ShipLoadoutSelectWindow, Character>(m_character);
            if (loadoutSelect != null)
                loadoutSelect.Plan = value;

            // If the SkillExplorerWindow is open, assign the new plan
            SkillExplorerWindow skillExplorer = WindowsFactory.GetByTag<SkillExplorerWindow, Character>(m_character);
            if (skillExplorer != null)
                skillExplorer.Plan = value;
        }

        /// <summary>
        /// Updates the controls visibility.
        /// </summary>
        private void UpdateControlsVisibility()
        {
            tabControl.ImageList = !Settings.UI.SafeForWork
                ? ilTabIcons
                : m_emptyImageList;

            foreach (ToolStripItem item in upperToolStrip.Items)
            {
                // Enable or disable the tool strip items except the plan selector and the loadout import
                item.Enabled = (item == tsddbPlans) || (item == tsbLoadoutImport) || (item == tsbClipboardImport) || tabControl.SelectedIndex == 0;

                item.DisplayStyle = !Settings.UI.SafeForWork
                    ? ToolStripItemDisplayStyle.ImageAndText
                    : ToolStripItemDisplayStyle.Text;
            }

            foreach (ToolStripItem label in MainStatusStrip.Items)
            {
                label.DisplayStyle = !Settings.UI.SafeForWork
                    ? ToolStripItemDisplayStyle.ImageAndText
                    : ToolStripItemDisplayStyle.Text;
            }
        }

        /// <summary>
        /// Updates the plan controls.
        /// </summary>
        private void UpdatePlanControls()
        {
            // Set the upper toolstrip visibility
            upperToolStrip.Visible = m_plan != null;

            // Update the status bar
            UpdateStatusBar();

            if (m_plan != null)
            {
                if (!tabControl.TabPages.Contains(tpPlanEditor))
                    tabControl.TabPages.Insert(0, tpPlanEditor);

                // Show the hint tip
                TipWindow.ShowTip(this, "planner",
                    "Welcome to the Skill Planner",
                    "Select skills to add to your plan using the list on the left." +
                    " To view the list of skills you've added to your plan," +
                    " choose \"Select Plan\" from the drop down in the upper left.");

                return;
            }

            // If we got this far we want to show the plan window as a Data Browser
            // Remove the Plan Editor if it exists
            if (tabControl.TabPages.Contains(tpPlanEditor))
                tabControl.TabPages.Remove(tpPlanEditor);

            Text = $"{(m_character == null ? String.Empty : $"{m_character.Name} - ")}EVEMon Data Browser";
        }

        /// <summary>
        /// Shows the plan window.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <param name="plan">The plan.</param>
        /// <returns></returns>
        internal static PlanWindow ShowPlanWindow(Character character = null, Plan plan = null)
        {
            // If no character is associated, open a unique Data Browser (non-associated character)
            if (character == null && plan == null)
                return WindowsFactory.ShowUnique<PlanWindow>();

            // Check if a Skill Planner is already open
            // (a Skill Planner has the same Tag as a Data Browser but it has a plan attached)
            PlanWindow planWindow = WindowsFactory.GetByTag<PlanWindow, Character>(character ?? (Character)plan.Character);

            // Do we have a Skill Planner open?
            if (planWindow?.Plan != null)
            {
                // Activate
                planWindow = WindowsFactory.ShowByTag<PlanWindow, Character>(character ?? (Character)plan.Character);

                // If a plan was passed, assign the new plan
                if (plan != null)
                    planWindow.Plan = plan;

                return planWindow;
            }

            // No plan window found, open a new one
            if (planWindow == null || plan == null)
            {
                // Open a new Data Browser associated with the character
                if (plan == null)
                    return WindowsFactory.ShowByTag<PlanWindow, Character>(character);

                // Open a new Skill Planner (use the plan as tag for the creating the window)
                // (This should be the only time a plan is used as the tag)
                planWindow = WindowsFactory.ShowByTag<PlanWindow, Plan>(plan);

                // Change the tag (we changed it to the character for window lookup)
                WindowsFactory.ChangeTag<PlanWindow, Plan, Character>(plan, (Character)plan.Character);

                return planWindow;
            }

            // Activate
            planWindow = WindowsFactory.ShowByTag<PlanWindow, Character>(character);

            // It's a Data Browser, transform it to a Skill Planner
            planWindow.Plan = plan;

            return planWindow;
        }

        /// <summary>
        /// Shows the plan editor.
        /// </summary>
        internal void ShowPlanEditor()
        {
            tabControl.SelectedTab = tpPlanEditor;
        }

        /// <summary>
        /// Shows the skill in the skill browser and switches to this tab.
        /// </summary>
        /// <param name="skill">The skill.</param>
        /// <exception cref="System.ArgumentNullException">skill</exception>
        internal void ShowSkillInBrowser(Skill skill)
        {
            skill.ThrowIfNull(nameof(skill));

            // Quit if it's an "Unknown" skill
            if (skill.ID == Int32.MaxValue)
                return;

            ShowSkillBrowser();
            skillBrowser.SelectedSkill = skill;
        }

        /// <summary>
        /// Shows the skill browser.
        /// </summary>
        internal void ShowSkillBrowser()
        {
            tabControl.SelectedTab = tpSkillBrowser;
        }

        /// <summary>
        /// Shows the certificate in the certificate browser and switches to this tab.
        /// </summary>
        /// <param name="certificate">The certificate.</param>
        /// <exception cref="System.ArgumentNullException">certificate</exception>
        internal void ShowCertificateInBrowser(CertificateLevel certificate)
        {
            certificate.ThrowIfNull(nameof(certificate));

            ShowCertificateBrowser();
            certBrowser.SelectedCertificateClass = certificate.Certificate.Class;
        }

        /// <summary>
        /// Shows the certificate browser.
        /// </summary>
        internal void ShowCertificateBrowser()
        {
            tabControl.SelectedTab = tpCertificateBrowser;
        }

        /// <summary>
        /// Shows the ship in the ship browser and switches to this tab.
        /// </summary>
        /// <param name="ship"></param>
        /// <exception cref="System.ArgumentNullException">ship</exception>
        internal void ShowShipInBrowser(Item ship)
        {
            ship.ThrowIfNull(nameof(ship));

            // Quit if it's an "Unknown" item
            if (ship.ID == Int32.MaxValue)
                return;

            ShowShipBrowser();
            shipBrowser.SelectedObject = ship;
        }

        /// <summary>
        /// Shows the ship browser.
        /// </summary>
        internal void ShowShipBrowser()
        {
            tabControl.SelectedTab = tpShipBrowser;
        }

        /// <summary>
        /// Shows the item in the item browser and switches to this tab.
        /// </summary>
        /// <param name="item"></param>
        /// <exception cref="System.ArgumentNullException">item</exception>
        internal void ShowItemInBrowser(Item item)
        {
            item.ThrowIfNull(nameof(item));

            // Quit if it's an "Unknown" item
            if (item.ID == Int32.MaxValue)
                return;

            ShowItemBrowser();
            itemBrowser.SelectedObject = item;
        }

        /// <summary>
        /// Shows the item browser.
        /// </summary>
        internal void ShowItemBrowser()
        {
            tabControl.SelectedTab = tpItemBrowser;
        }

        /// <summary>
        /// Shows the blueprint in the blueprint browser and switches to this tab.
        /// </summary>
        /// <param name="blueprint"></param>
        /// <exception cref="System.ArgumentNullException">blueprint</exception>
        internal void ShowBlueprintInBrowser(Item blueprint)
        {
            blueprint.ThrowIfNull(nameof(blueprint));

            // Quit if it's an "Unknown" item
            if (blueprint.ID == Int32.MaxValue)
                return;

            ShowBlueprintBrowser();
            blueprintBrowser.SelectedObject = blueprint;
        }

        /// <summary>
        /// Shows the blueprint browser.
        /// </summary>
        internal void ShowBlueprintBrowser()
        {
            tabControl.SelectedTab = tpBlueprintBrowser;
        }

        /// <summary>
        /// Identifies if there are obsolete entries in the skill plan,
        /// displays message if required.
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void CheckObsoleteEntries()
        {
            switch (Settings.UI.PlanWindow.ObsoleteEntryRemovalBehaviour)
            {
                case ObsoleteEntryRemovalBehaviour.AlwaysAsk:
                    ObsoleteEntriesStatusLabel.Visible = m_plan.ContainsObsoleteEntries;
                    break;
                case ObsoleteEntryRemovalBehaviour.RemoveAll:
                    m_plan.CleanObsoleteEntries(ObsoleteRemovalPolicy.RemoveAll);
                    break;
                case ObsoleteEntryRemovalBehaviour.RemoveConfirmed:
                    m_plan.CleanObsoleteEntries(ObsoleteRemovalPolicy.ConfirmedOnly);
                    ObsoleteEntriesStatusLabel.Visible = m_plan.ContainsObsoleteEntries;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Identifies if there are invalid entries in the skill plan,
        /// displays message if required.
        /// </summary>
        private void CheckInvalidEntries()
        {
            if (!m_plan.ContainsInvalidEntries)
                return;

            StringBuilder message = new StringBuilder();

            message
                .AppendLine("When loading the plan one or more skills were not found. " +
                            "This can be caused by loading a plan from a previous version of EVEMon or CCP have renamed a skill.")
                .AppendLine();

            foreach (InvalidPlanEntry entry in m_plan.InvalidEntries)
            {
                message.AppendLine($" - {entry.SkillName} planned to {entry.PlannedLevel}");
            }

            message
                .AppendLine()
                .AppendLine("Do you wish to keep these entries?")
                .AppendLine()
                .AppendLine("- If you select \"Yes\" the entries will be removed from the plan and will be stored in settings.")
                .Append("- If you select \"No\" the entries will be discarded.");

            DialogResult result = MessageBox.Show(message.ToString(),
                @"Invalid Entries Detected",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Exclamation);

            switch (result)
            {
                case DialogResult.No:
                    m_plan.ClearInvalidEntries();
                    break;
                case DialogResult.Yes:
                    m_plan.AcknoledgeInvalidEntries();
                    break;
            }
        }

        /// <summary>
        /// Sets the plan editor's skill selection control selected skill.
        /// </summary>
        /// <param name="skill">The skill.</param>
        internal void SetPlanEditorSkillSelectorSelectedSkill(Skill skill)
        {
            planEditor.SetPlanEditorSkillSelectorSelectedSkill(skill);
        }

        /// <summary>
        /// Updates the plan editor's skill selection control.
        /// </summary>
        internal void UpdatePlanEditorSkillSelection()
        {
            planEditor.UpdatePlanEditorSkillSelection();
        }

        /// <summary>
        /// Sets the skill browser's skill selection control selected skill.
        /// </summary>
        /// <param name="skill">The skill.</param>
        internal void SetSkillBrowserSkillSelectorSelectedSkill(Skill skill)
        {
            skillBrowser.SetSkillBrowserSkillSelectorSelectedSkill(skill);
        }

        /// <summary>
        /// Updates the skill browser.
        /// </summary>
        internal void UpdateSkillBrowser()
        {
            skillBrowser.UpdateSkillBrowser();
        }

        /// <summary>
        /// Checks to see if the current contents of the clipboard is a valid list of skills.
        /// </summary>
        /// <param name="text">Clipboard contents.</param>
        private bool CheckClipboardSkillQueue(string text)
        {
            string[] lines = text.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            // Nothing to evaluate
            if (lines.Length == 0)
                return false;

            // Any lines not with a valid skill ending?
            foreach (string lineAll in lines)
            {
                string line = lineAll.Trim();

                if (!string.IsNullOrEmpty(line))
                {
                    int idx = line.LastIndexOf(" ");
                    if (idx != -1)
                    {
                        if (StaticSkills.GetSkillByName(line.Substring(0, idx)) == null)
                            return false;

                        int level = Skill.GetIntFromRoman(line.Substring(idx + 1));

                        if (level < 1 || level > 5)
                            return false;
                    }
                    else
                        return false;
                }
            }

            return true;
        }

        #endregion


        #region Global events

        /// <summary>
        /// Occurs when a plan name changed.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PlanChangedEventArgs"/> instance containing the event data.</param>
        private void EveMonClient_PlanNameChanged(object sender, PlanChangedEventArgs e)
        {
            if (m_plan != e.Plan)
                return;

            UpdateTitle();
        }

        /// <summary>
        /// Occurs when the settings changed.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void EveMonClient_SettingsChanged(object sender, EventArgs e)
        {
            UpdateControlsVisibility();
        }

        #endregion


        #region Content creation

        /// <summary>
        /// Updates the skill status label.
        /// </summary>
        /// <param name="selected">if set to <c>true</c> [selected].</param>
        /// <param name="skillCount">The skill count.</param>
        /// <param name="uniqueCount">The unique count.</param>
        internal void UpdateSkillStatusLabel(bool selected, int skillCount, int uniqueCount)
        {
            SkillsStatusLabel.Text = $"{skillCount} skill{skillCount.S()} " +
                $"{(selected ? "selected" : "planned")} ({uniqueCount} unique)";
        }

        /// <summary>
        /// Updates the time status label.
        /// </summary>
        /// <param name="selected">if set to <c>true</c> [selected].</param>
        /// <param name="skillCount">The skill count.</param>
        /// <param name="totalTime">The total time.</param>
        internal void UpdateTimeStatusLabel(bool selected, int skillCount, TimeSpan totalTime)
        {
            string time = totalTime.ToDescriptiveText(DescriptiveTextOptions.IncludeCommas);
            TimeStatusLabel.AutoToolTip = false;
            TimeStatusLabel.Text = time + " to train " + (selected ?
                $"selected skill{skillCount.S()}" : "whole plan");
        }

        /// <summary>
        /// Updates the cost status label.
        /// </summary>
        /// <param name="selected">if set to <c>true</c> [selected].</param>
        /// <param name="totalcost">The totalcost.</param>
        /// <param name="cost">The cost.</param>
        internal void UpdateCostStatusLabel(bool selected, long totalcost, long cost)
        {
            CostStatusLabel.AutoToolTip = totalcost <= 0;

            if (totalcost > 0)
            {
                CostStatusLabel.ToolTipText = $"{totalcost:N2} ISK required to purchase " +
                    (selected ? "selected" : "all") +
                    $" skill{(m_plan.UniqueSkillsCount.S())} anew";
            }

            CostStatusLabel.Text = cost > 0 ? $"{cost:N2} ISK required" : "0 ISK required";
        }

        /// <summary>
        /// Updates the skill points status label.
        /// </summary>
        /// <param name="selected">if set to <c>true</c> [selected].</param>
        /// <param name="skillCount">The skill count.</param>
        /// <param name="skillPoints">The skill points.</param>
        internal void UpdateSkillPointsStatusLabel(bool selected, int skillCount, long skillPoints)
        {
            SkillPointsStatusLabel.AutoToolTip = skillPoints <= 0;

            if (skillPoints > 0)
            {
                SkillPointsStatusLabel.ToolTipText = $"{skillPoints:N0} skill points required to train " +
                    (selected ? "selected" : "all") + $" skill{skillCount.S()}";
            }

            int skillInjectorsCount = m_plan.Character.GetRequiredSkillInjectorsForSkillPoints(
                skillPoints);
            SkillPointsStatusLabel.Text = skillPoints > 0 ?
                $"{skillPoints:N0} SP required ({skillInjectorsCount:N0} Skill Injector" +
                skillInjectorsCount.S() : "0 SP required";
        }

        /// <summary>
        /// Autonomously updates the status bar with the plan's training time.
        /// </summary>
        internal void UpdateStatusBar()
        {
            MainStatusStrip.Visible = m_plan != null;

            if (m_plan == null)
                return;

            int entriesCount = m_plan.Count();

            // Training time
            CharacterScratchpad scratchpad = m_plan.ChosenImplantSet != null
                ? m_plan.Character.After(m_plan.ChosenImplantSet)
                : new CharacterScratchpad(m_character);

            TimeSpan trainingTime = planEditor.DisplayPlan.GetTotalTime(scratchpad, true);

            UpdateSkillStatusLabel(false, entriesCount, m_plan.UniqueSkillsCount);
            UpdateTimeStatusLabel(false, entriesCount, trainingTime);
            UpdateCostStatusLabel(false, m_plan.TotalBooksCost, m_plan.NotKnownSkillBooksCost);
            UpdateSkillPointsStatusLabel(false, entriesCount, planEditor.DisplayPlan.TotalSkillPoints);
        }


        /// <summary>
        /// Imports list of skills from the clipboard to the training plan
        /// </summary>
        /// <param name="text">Clipboard contents.</param>
        internal void ImportSkillsFromClipboard(string text)
        {
            string[] lines = text.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            List<StaticSkillLevel> list = new List<StaticSkillLevel>();

            // Nothing to evaluate
            if (lines.Length == 0)
                return;

            CharacterScratchpad scratchpad = new CharacterScratchpad(m_character);

            foreach (string lineAll in lines)
            {
                // When pasted from sites trailing spaces are often added
                string line = lineAll.Trim();

                // Split level and skill
                int idx = line.LastIndexOf(" ");
                if (idx != -1)
                {
                    string name = line.Substring(0, idx);
                    string level = line.Substring(idx + 1);
                    StaticSkillLevel skill = new StaticSkillLevel(name, Skill.GetIntFromRoman(level));

                    // Make sure we actually have a valid skill
                    if(skill.Skill != StaticSkill.UnknownStaticSkill)
                    {
                        // Add any dependencies that the skill may have
                        scratchpad.Train(skill.AllDependencies.Where(x => m_character.Skills[x.Skill.ID].Level < x.Level));

                        // Add the skill itself
                        scratchpad.Train(skill);
                    }   
                }

            }

            // Add all trained skills to a list
            list.AddRange(scratchpad.TrainedSkills);

            if(list.Count == 0)
            {
                MessageBox.Show(@"Pasted skills and all dependencies have already been trained.", @"Already Trained",
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else if (m_plan.AreSkillsPlanned(list))
            {
                MessageBox.Show(@"Pasted skills and all dependencies have already been trained or planned.", @"Already Trained or Planned",
                                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {
                TimeSpan trainingTime = m_character.GetTrainingTimeToMultipleSkills(list);
                string trainingDesc = trainingTime.ToDescriptiveText(DescriptiveTextOptions.IncludeCommas | DescriptiveTextOptions.SpaceText);

                DialogResult dr = MessageBox.Show($"Are you sure you want to add {list.Count} skills" +
                                                    $" with a total training time of {trainingDesc}" +
                                                    $".\n\nThis will also include any dependencies not included in your paste", "Add Skills?",
                                                    MessageBoxButtons.YesNo, 
                                                    MessageBoxIcon.Question, 
                                                    MessageBoxDefaultButton.Button2);

                if (dr == DialogResult.Yes)
                {
                   IPlanOperation operation = m_plan.TryAddSet(list, "Paste from Clipboard");

                    if (operation == null)
                        return;

                    PlanHelper.Perform(new PlanToOperationWindow(operation), this);
                    this.ShowPlanEditor();
                }
            }

        }

        #endregion


        #region Controls handlers

        /// <summary>
        /// Toolbar > Delete.
        /// Prompts the user and delete the currently selected plan.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsbDeletePlan_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show(Properties.Resources.PromptDeletePlan,
                @"Delete Plan", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

            if (dr != DialogResult.Yes)
                return;

            // Close the skill explorer
            WindowsFactory.GetAndCloseByTag<SkillExplorerWindow, PlanWindow>(this);

            // Remove the plan
            int index = m_character.Plans.IndexOf(m_plan);
            m_character.Plans.Remove(m_plan);

            // Choose which plan to show next
            // By default we choose the next one,
            // if it was the last in the list we select the previous one
            if (index > m_character.Plans.Count - 1)
                index--;

            // When no plans exists after deletion we close the window
            if (index < 0)
            {
                Close();
                return;
            }

            // New plan to show
            Plan newplan = m_character.Plans[index];
            Plan = newplan;
        }

        /// <summary>
        /// When the selected tab changes we update the list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (DesignMode || m_plan == null)
                return;

            UpdateControlsVisibility();

            // Close the implant calculator and attribute optimizer if the user moves away for the plan editor
            if (tabControl.SelectedTab == tpPlanEditor)
                return;

            // Tell the attributes optimization window we're closing down
            WindowsFactory.GetAndCloseByTag<AttributesOptimizerOptionsWindow, PlanEditorControl>(planEditor);
            WindowsFactory.GetAndCloseByTag<AttributesOptimizerWindow, PlanEditorControl>(planEditor);

            // Tell the implant window we're closing down
            WindowsFactory.GetAndCloseByTag<ImplantCalculatorWindow, PlanEditorControl>(planEditor);
        }

        /// <summary>
        /// Status bar > Obsolete entries label.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void obsoleteEntriesToolStripStatusLabel_Click(object sender, EventArgs e)
        {
            ObsoleteEntriesAction action = ObsoleteEntriesWindow.ShowDialog(m_plan);

            switch (action)
            {
                case ObsoleteEntriesAction.RemoveAll:
                    planEditor.ClearObsoleteEntries(ObsoleteRemovalPolicy.RemoveAll);
                    ObsoleteEntriesStatusLabel.Visible = false;
                    break;
                case ObsoleteEntriesAction.RemoveConfirmed:
                    planEditor.ClearObsoleteEntries(ObsoleteRemovalPolicy.ConfirmedOnly);
                    ObsoleteEntriesStatusLabel.Visible = false;
                    break;
                case ObsoleteEntriesAction.KeepAll:
                case ObsoleteEntriesAction.None:
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Select Plan > New Plan.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void newPlanToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_character == null)
                return;

            Plan newPlan = CreateNewPlan(m_character);

            if (newPlan == null)
                return;

            m_character.Plans.Add(newPlan);
            Plan = newPlan;
        }

        /// <summary>
        /// Select Plan > Create Plan from Skill Queue.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void createPlanFromSkillQueueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_character == null)
                return;

            Plan newPlan = CreateNewPlan(m_character, EveMonConstants.CurrentSkillQueueText);

            if (newPlan == null)
                return;

            // Add skill queue to new plan and insert it on top of the plans
            bool planCreated = PlanIOHelper.CreatePlanFromCharacterSkillQueue(newPlan, m_character);

            if (planCreated)
                Plan = newPlan;
        }

        /// <summary>
        /// When the plans menu is opening, we update the items.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsddbPlans_DropDownOpening(object sender, EventArgs e)
        {
            ToolStripItem[] noTagItems = tsddbPlans.DropDownItems
                .Cast<ToolStripItem>()
                .Where(item => item.Tag == null).ToArray();

            tsddbPlans.DropDownItems.Clear();
            tsddbPlans.DropDownItems.AddRange(noTagItems);

            CCPCharacter ccpCharacter = m_character as CCPCharacter;
            createPlanFromSkillQueueToolStripMenuItem.Enabled = ccpCharacter != null && ccpCharacter.SkillQueue.Any();

            m_character.Plans.AddTo(
                tsddbPlans.DropDownItems,
                (menuPlanItem, plan) =>
                {
                    menuPlanItem.Tag = plan;
                    menuPlanItem.ToolTipText = plan.Description.WordWrap(100);

                    if (plan != m_plan)
                        return;

                    // Disable selection of the current plan and make it italic and bold
                    menuPlanItem.Enabled = false;
                    menuPlanItem.Font = FontFactory.GetFont(menuPlanItem.Font, FontStyle.Italic | FontStyle.Bold);
                });
        }

        /// <summary>
        /// Occurs when the user clicks one of the children of the "Plans" menu.
        /// The item may be an existing plan or the "New plan" item.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsddbPlans_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem?.Tag == m_plan || e.ClickedItem?.Tag == null)
                return;

            // Is it another plan ?
            // Opens the existing window when there is one, or switch to this plan when no window opened
            ShowPlanWindow(m_character, m_plan);
            Plan = (Plan)e.ClickedItem.Tag;
        }

        /// <summary>
        /// Creates a new plan.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <param name="planName">Name of the plan.</param>
        /// <returns></returns>
        internal static Plan CreateNewPlan(Character character, string planName = null)
        {
            using (NewPlanWindow npw = new NewPlanWindow { PlanName = planName })
            {
                DialogResult dr = npw.ShowDialog();
                if (dr == DialogResult.Cancel)
                    return null;

                return new Plan(character)
                {
                    Name = npw.PlanName,
                    Description = npw.PlanDescription
                };
            }
        }

        /// <summary>
        /// Toolbar > Export > Plan.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void tsmiExportPlan_Click(object sender, EventArgs e)
        {
            await UIHelper.ExportPlanAsync(m_plan);
        }

        /// <summary>
        /// Toolbar > Export > After Plan Character.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void tsmiAfterPlanCharacter_Click(object sender, EventArgs e)
        {
            await UIHelper.ExportCharacterAsync(m_character, m_plan);
        }

        /// <summary>
        /// Opens the Loadout form and passes it the current Plan.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsbLoadoutImport_Click(object sender, EventArgs e)
        {
            // Opens a loadout importation window (use the plan as tag for the creating the window)
            // (This should be the only time a plan is used as the tag)
            WindowsFactory.ShowByTag<LoadoutImportationWindow, Plan>(m_plan);

            // Change the tag (we changed it to the character for window lookup)
            WindowsFactory.ChangeTag<LoadoutImportationWindow, Plan, Character>(m_plan, m_character);
        }

        /// <summary>
        /// Toolbar > Copy to clipboard.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsbCopyToClipboard_Click(object sender, EventArgs e)
        {
            // Prompt the user for settings. When null, the user cancelled
            PlanExportSettings settings = UIHelper.PromptUserForPlanExportSettings(m_plan);
            if (settings == null)
                return;

            string output = PlanIOHelper.ExportAsText(m_plan, settings);

            // Copy the result to the clipboard
            try
            {
                Clipboard.Clear();
                Clipboard.SetText(output);

                MessageBox.Show(Properties.Resources.MessageCopiedPlan, "Plan Copied",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (ExternalException ex)
            {
                ExceptionHandler.LogException(ex, true);

                MessageBox.Show(Properties.Resources.ErrorClipboardFailure, "Plan Copy Failure",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Toolbar > Implants calculator.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsbImplantCalculator_Click(object sender, EventArgs e)
        {
            WindowsFactory.ShowByTag<ImplantCalculatorWindow, PlanEditorControl>(this, planEditor);
        }

        /// <summary>
        /// Toolbar > Attributes optimizer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsbAttributesOptimizer_Click(object sender, EventArgs e)
        {
            WindowsFactory.ShowByTag<AttributesOptimizerOptionsWindow, PlanEditorControl>(this, planEditor);
        }

        /// <summary>
        /// Toolbar > Print.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsbPrintPlan_Click(object sender, EventArgs e)
        {
            PlanPrinter.Print(m_plan);
        }

        private void tsbClipboardImport_Click(object sender, EventArgs e)
        {
            string clipboard = Clipboard.GetText();

            if (CheckClipboardSkillQueue(clipboard))
            {
                ImportSkillsFromClipboard(clipboard);
            }
            else
            {
                MessageBox.Show(@"Contents of the clipboard is not a valid list of skills or contains invalid skill levels.", @"Not a Skill Set",
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        #endregion
    }
}
