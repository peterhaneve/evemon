using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
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
using EVEMon.Common.Models.Comparers;
using EVEMon.Common.SettingsObjects;
using EVEMon.SkillPlanner;

namespace EVEMon.CharacterMonitoring
{
    internal sealed partial class CharacterIndustryJobsList : UserControl, IListView
    {
        #region Fields

        private readonly List<IndustryJobColumnSettings> m_columns = new List<IndustryJobColumnSettings>();
        private readonly List<IndustryJob> m_list = new List<IndustryJob>();

        private InfiniteDisplayToolTip m_tooltip;
        private Timer m_refreshTimer;
        private IndustryJobGrouping m_grouping;
        private IndustryJobColumn m_sortCriteria;
        private IssuedFor m_showIssuedFor;

        private string m_textFilter = String.Empty;
        private bool m_sortAscending = true;

        private bool m_isUpdatingColumns;
        private bool m_columnsChanged;
        private bool m_init;

        private int m_columnTTCDisplayIndex;

        // Panel info variables
        private int m_skillBasedManufacturingJobs, m_skillBasedResearchingJobs;

        private int m_remoteManufacturingRange, m_remoteResearchingRange;

        private int m_activeManufJobsIssuedForCharacterCount,
            m_activeManufJobsIssuedForCorporationCount;

        private int m_activeResearchJobsIssuedForCharacterCount,
            m_activeResearchJobsIssuedForCorporationCount;

        #endregion


        # region Constructor

        public CharacterIndustryJobsList()
        {
            InitializeComponent();
            InitializeExpandablePanelControls();

            lvJobs.Hide();
            lvJobs.AllowColumnReorder = true;
            lvJobs.Columns.Clear();
            industryExpPanelControl.Hide();

            m_showIssuedFor = IssuedFor.All;

            noJobsLabel.Font = FontFactory.GetFont("Tahoma", 11.25F, FontStyle.Bold);
            industryExpPanelControl.Font = FontFactory.GetFont("Tahoma", 8.25f);

            ListViewHelper.EnableDoubleBuffer(lvJobs);

            lvJobs.ColumnClick += listView_ColumnClick;
            lvJobs.ColumnWidthChanged += listView_ColumnWidthChanged;
            lvJobs.ColumnReordered += listView_ColumnReordered;
            lvJobs.MouseDown += listView_MouseDown;
            lvJobs.MouseMove += listView_MouseMove;
            lvJobs.MouseLeave += listView_MouseLeave;
        }

        #endregion


        #region Properties

        /// <summary>
        /// Gets the character associated with this monitor.
        /// </summary>
        internal CCPCharacter Character { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="lvJobs"/> is visible.
        /// </summary>
        /// <value><c>true</c> if visible; otherwise, <c>false</c>.</value>
        internal bool Visibility
        {
            get { return lvJobs.Visible; }
            set { lvJobs.Visible = value; }
        }

        /// <summary>
        /// Gets or sets the text filter.
        /// </summary>
        [Browsable(false)]
        public string TextFilter
        {
            get { return m_textFilter; }
            set
            {
                m_textFilter = value;
                if (m_init)
                    UpdateColumns();
            }
        }

        /// <summary>
        /// Gets or sets the grouping mode.
        /// </summary>
        [Browsable(false)]
        public Enum Grouping
        {
            get { return m_grouping; }
            set
            {
                m_grouping = (IndustryJobGrouping)value;
                if (m_init)
                    UpdateColumns();
            }
        }

        /// <summary>
        /// Gets or sets which "Issued for" jobs to display.
        /// </summary>
        internal IssuedFor ShowIssuedFor
        {
            get { return m_showIssuedFor; }
            set
            {
                m_showIssuedFor = value;
                if (m_init)
                    UpdateColumns();
            }
        }

        /// <summary>
        /// Gets true when character has active jobs issued for corporation.
        /// </summary>
        private bool HasActiveCorporationIssuedJobs
            => m_list.Any(x => x.State == JobState.Active && x.IssuedFor == IssuedFor.Corporation);

        /// <summary>
        /// Gets or sets the enumeration of jobs to display.
        /// </summary>
        internal IEnumerable<IndustryJob> Jobs
        {
            get { return m_list; }
            set
            {
                m_list.Clear();
                if (value == null)
                    return;

                m_list.AddRange(value);
            }
        }

        /// <summary>
        /// Gets or sets the settings used for columns.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public IEnumerable<IColumnSettings> Columns
        {
            get
            {
                // Add the visible columns; matching the display order
                List<IndustryJobColumnSettings> newColumns = new List<IndustryJobColumnSettings>();
                foreach (ColumnHeader header in lvJobs.Columns.Cast<ColumnHeader>().OrderBy(x => x.DisplayIndex))
                {
                    IndustryJobColumnSettings columnSetting = m_columns.First(x => x.Column == (IndustryJobColumn)header.Tag);
                    if (columnSetting.Width > -1)
                        columnSetting.Width = header.Width;

                    newColumns.Add(columnSetting);
                }

                // Then add the other columns
                newColumns.AddRange(m_columns.Where(x => !x.Visible));

                return newColumns;
            }
            set
            {
                m_columns.Clear();
                if (value != null)
                    m_columns.AddRange(value.Cast<IndustryJobColumnSettings>());

                // Whenever the columns changes, we need to
                // reset the dipslay index of the TTC column
                m_columnTTCDisplayIndex = -1;

                if (m_init)
                    UpdateColumns();
            }
        }

        #endregion


        # region Inherited Events

        /// <summary>
        /// On load subscribe the events.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (DesignMode || this.IsDesignModeHosted())
                return;

            m_tooltip = new InfiniteDisplayToolTip(lvJobs);
            m_refreshTimer = new Timer();

            m_refreshTimer.Tick += refresh_TimerTick;
            m_refreshTimer.Interval = 1000;

            EveMonClient.TimerTick += EveMonClient_TimerTick;
            EveMonClient.IndustryJobsUpdated += EveMonClient_IndustryJobsUpdated;
            EveMonClient.ConquerableStationListUpdated += EveMonClient_ConquerableStationListUpdated;
            EveMonClient.CharacterIndustryJobsCompleted += EveMonClient_CharacterIndustryJobsCompleted;
            EveMonClient.EveIDToNameUpdated += EveMonClient_EveIDToNameUpdated;
            Disposed += OnDisposed;
        }

        /// <summary>
        /// Unsubscribe events on disposing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDisposed(object sender, EventArgs e)
        {
            m_tooltip.Dispose();
            m_refreshTimer.Dispose();

            EveMonClient.TimerTick -= EveMonClient_TimerTick;
            EveMonClient.IndustryJobsUpdated -= EveMonClient_IndustryJobsUpdated;
            EveMonClient.ConquerableStationListUpdated -= EveMonClient_ConquerableStationListUpdated;
            EveMonClient.CharacterIndustryJobsCompleted -= EveMonClient_CharacterIndustryJobsCompleted;
            EveMonClient.EveIDToNameUpdated -= EveMonClient_EveIDToNameUpdated;
            Disposed -= OnDisposed;
        }

        /// <summary>
        /// When the control becomes visible again, we update the content.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            if (DesignMode || this.IsDesignModeHosted() || Character == null || !Visible)
                return;

            // Prevents the properties to call UpdateColumns() till we set all properties
            m_init = false;

            lvJobs.Visible = false;
            industryExpPanelControl.Visible = false;

            Jobs = Character?.IndustryJobs;
            Columns = Settings.UI.MainWindow.IndustryJobs.Columns;
            Grouping = Character?.UISettings.JobsGroupBy;
            TextFilter = String.Empty;

            UpdateColumns();

            m_init = true;

            UpdateListVisibility();
        }

        # endregion


        #region Updates Main Industry Window

        /// <summary>
        /// Autoresizes the columns.
        /// </summary>
        public void AutoResizeColumns()
        {
            m_columns.ForEach(column =>
            {
                if (column.Visible)
                    column.Width = -2;
            });

            UpdateColumns();
        }

        /// <summary>
        /// Updates the columns.
        /// </summary>
        internal void UpdateColumns()
        {
            // Returns if not visible
            if (!Visible)
                return;

            lvJobs.BeginUpdate();
            m_isUpdatingColumns = true;

            try
            {
                lvJobs.Columns.Clear();
                lvJobs.Groups.Clear();
                lvJobs.Items.Clear();

                foreach (IndustryJobColumnSettings column in m_columns.Where(x => x.Visible))
                {
                    ColumnHeader header = lvJobs.Columns.Add(column.Column.GetHeader(), column.Width);
                    header.Tag = column.Column;

                    switch (column.Column)
                    {
                        case IndustryJobColumn.TTC:
                        case IndustryJobColumn.Cost:
                        case IndustryJobColumn.Probability:
                            header.TextAlign = HorizontalAlignment.Right;
                            break;
                        case IndustryJobColumn.Runs:
                            header.TextAlign = HorizontalAlignment.Center;
                            break;
                    }
                }

                // We update the content
                UpdateContent();
            }
            finally
            {
                lvJobs.EndUpdate();
                m_isUpdatingColumns = false;
            }
        }

        /// <summary>
        /// Updates the content of the listview.
        /// </summary>
        private void UpdateContent()
        {
            // Returns if not visible
            if (!Visible)
                return;
            int scrollBarPosition = lvJobs.GetVerticalScrollBarPosition();
            // Store the selected item (if any) to restore it after the update
            int selectedItem = lvJobs.SelectedItems.Count > 0 ? lvJobs.SelectedItems[0].Tag.
                GetHashCode() : 0;
            lvJobs.BeginUpdate();
            try
            {
                bool hideInactive = Character != null && Settings.UI.MainWindow.IndustryJobs.
                    HideInactiveJobs, hideIssued = m_showIssuedFor != IssuedFor.All;
                var jobs = new LinkedList<IndustryJob>();
                // Filter jobs
                foreach (var job in m_list)
                {
                    job.UpdateLocation(Character);
                    job.UpdateInstallation(Character);

                    if (job.InstalledItem != null && job.OutputItem != null && IsTextMatching(
                        job, m_textFilter))
                    {
                        if ((!hideInactive || job.IsActive) && (!hideIssued || job.IssuedFor ==
                                m_showIssuedFor))
                            jobs.AddLast(job);
                    }
                }
                UpdateSort();
                UpdateContentByGroup(jobs);
                // Restore the selected item (if any)
                if (selectedItem > 0)
                    foreach (ListViewItem lvItem in lvJobs.Items.Cast<ListViewItem>().Where(
                            lvItem => lvItem.Tag.GetHashCode() == selectedItem))
                        lvItem.Selected = true;
                // Adjust the size of the columns
                AdjustColumns();
                // Update the expandable panel info
                UpdateExpPanelContent();
                UpdateListVisibility();
            }
            finally
            {
                lvJobs.EndUpdate();
                lvJobs.SetVerticalScrollBarPosition(scrollBarPosition);
            }
        }

        /// <summary>
        /// Updates the list visibility.
        /// </summary>
        private void UpdateListVisibility()
        {
            // Display or hide the "no jobs" label
            if (!m_init)
                return;

            noJobsLabel.Visible = lvJobs.Items.Count == 0;
            industryExpPanelControl.Visible = true;
            industryExpPanelControl.Header.Visible = true;
            lvJobs.Visible = !noJobsLabel.Visible;
            m_refreshTimer.Enabled = lvJobs.Visible;
        }

        /// <summary>
        /// Updates the content by group.
        /// </summary>
        /// <param name="jobs">The jobs.</param>
        private void UpdateContentByGroup(IEnumerable<IndustryJob> jobs)
        {
            switch (m_grouping)
            {
                case IndustryJobGrouping.State:
                    IOrderedEnumerable<IGrouping<JobState, IndustryJob>> groups0 =
                        jobs.GroupBy(x => x.State).OrderBy(x => (int)x.Key);
                    UpdateContent(groups0);
                    break;
                case IndustryJobGrouping.StateDesc:
                    IOrderedEnumerable<IGrouping<JobState, IndustryJob>> groups1 =
                        jobs.GroupBy(x => x.State).OrderByDescending(x => (int)x.Key);
                    UpdateContent(groups1);
                    break;
                case IndustryJobGrouping.EndDate:
                    IOrderedEnumerable<IGrouping<DateTime, IndustryJob>> groups2 =
                        jobs.GroupBy(x => x.EndDate.ToLocalTime().Date).OrderBy(x => x.Key);
                    UpdateContent(groups2);
                    break;
                case IndustryJobGrouping.EndDateDesc:
                    IOrderedEnumerable<IGrouping<DateTime, IndustryJob>> groups3 =
                        jobs.GroupBy(x => x.EndDate.ToLocalTime().Date).OrderByDescending(x => x.Key);
                    UpdateContent(groups3);
                    break;
                case IndustryJobGrouping.InstalledItemType:
                    IOrderedEnumerable<IGrouping<string, IndustryJob>> groups4 =
                        jobs.GroupBy(x => x.InstalledItem.MarketGroup.CategoryPath).OrderBy(x => x.Key);
                    UpdateContent(groups4);
                    break;
                case IndustryJobGrouping.InstalledItemTypeDesc:
                    IOrderedEnumerable<IGrouping<string, IndustryJob>> groups5 =
                        jobs.GroupBy(x => x.InstalledItem.MarketGroup.CategoryPath).OrderByDescending(x => x.Key);
                    UpdateContent(groups5);
                    break;
                case IndustryJobGrouping.OutputItemType:
                    IOrderedEnumerable<IGrouping<string, IndustryJob>> groups6 =
                        jobs.GroupBy(x => x.OutputItem.MarketGroup.CategoryPath).OrderBy(x => x.Key);
                    UpdateContent(groups6);
                    break;
                case IndustryJobGrouping.OutputItemTypeDesc:
                    IOrderedEnumerable<IGrouping<string, IndustryJob>> groups7 =
                        jobs.GroupBy(x => x.OutputItem.MarketGroup.CategoryPath).OrderByDescending(x => x.Key);
                    UpdateContent(groups7);
                    break;
                case IndustryJobGrouping.Activity:
                    IOrderedEnumerable<IGrouping<string, IndustryJob>> groups8 =
                        jobs.GroupBy(x => x.Activity.GetDescription()).OrderBy(x => x.Key);
                    UpdateContent(groups8);
                    break;
                case IndustryJobGrouping.ActivityDesc:
                    IOrderedEnumerable<IGrouping<string, IndustryJob>> groups9 =
                        jobs.GroupBy(x => x.Activity.GetDescription()).OrderByDescending(x => x.Key);
                    UpdateContent(groups9);
                    break;
                case IndustryJobGrouping.Location:
                    IOrderedEnumerable<IGrouping<string, IndustryJob>> groups10 =
                        jobs.GroupBy(x => x.Installation).OrderBy(x => x.Key);
                    UpdateContent(groups10);
                    break;
                case IndustryJobGrouping.LocationDesc:
                    IOrderedEnumerable<IGrouping<string, IndustryJob>> groups11 =
                        jobs.GroupBy(x => x.Installation).OrderByDescending(x => x.Key);
                    UpdateContent(groups11);
                    break;
            }
        }

        /// <summary>
        /// Updates the content of the listview.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="groups"></param>
        private void UpdateContent<TKey>(IEnumerable<IGrouping<TKey, IndustryJob>> groups)
        {
            lvJobs.Items.Clear();
            lvJobs.Groups.Clear();

            // Add the groups
            foreach (IGrouping<TKey, IndustryJob> group in groups)
            {
                string groupText;
                if (group.Key is JobState)
                    groupText = ((JobState)(Object)group.Key).GetHeader();
                else if (group.Key is DateTime)
                    groupText = ((DateTime)(Object)group.Key).ToShortDateString();
                else
                    groupText = group.Key.ToString();

                ListViewGroup listGroup = new ListViewGroup(groupText);
                lvJobs.Groups.Add(listGroup);

                // Add the items in every group
                lvJobs.Items.AddRange(
                    group.Select(job => new
                    {
                        job,
                        item = new ListViewItem(job.InstalledItem.Name, listGroup)
                        {
                            UseItemStyleForSubItems = false,
                            Tag = job
                        }

                    }).Select(x => CreateSubItems(x.job, x.item)).ToArray());
            }
        }

        /// <summary>
        /// Creates the list view sub items.
        /// </summary>
        /// <param name="job">The job.</param>
        /// <param name="item">The item.</param>
        private ListViewItem CreateSubItems(IndustryJob job, ListViewItem item)
        {
            // Display text as dimmed if the job is no longer available
            if (!job.IsActive)
                item.ForeColor = SystemColors.GrayText;

            // Add enough subitems to match the number of columns
            while (item.SubItems.Count < lvJobs.Columns.Count + 1)
            {
                item.SubItems.Add(String.Empty);
            }

            // Creates the subitems
            for (int i = 0; i < lvJobs.Columns.Count; i++)
            {
                ColumnHeader header = lvJobs.Columns[i];
                IndustryJobColumn column = (IndustryJobColumn)header.Tag;
                SetColumn(job, item.SubItems[i], column);
            }

            // Tooltip
            StringBuilder builder = new StringBuilder();
            builder
                .Append($"Issued For: {job.IssuedFor}")
                .AppendLine()
                .Append($"Installed: {job.InstalledTime.ToLocalTime()}")
                .AppendLine()
                .Append($"Finishes: {job.EndDate.ToLocalTime()}")
                .AppendLine()
                .Append($"Activity: {job.Activity.GetDescription()}")
                .AppendLine()
                .Append($"Solar System: {job.SolarSystem.FullLocation}")
                .AppendLine()
                .Append($"Installation: {job.Installation}")
                .AppendLine();

            item.ToolTipText = builder.ToString();

            return item;
        }

        /// <summary>
        /// Adjusts the columns.
        /// </summary>
        private void AdjustColumns()
        {
            foreach (ColumnHeader column in lvJobs.Columns)
            {
                if (m_columns[column.Index].Width == -1)
                    m_columns[column.Index].Width = -2;

                column.Width = m_columns[column.Index].Width;

                // Due to .NET design we need to prevent the last colummn to resize to the right end

                // Return if it's not the last column and not set to auto-resize
                if (column.Index != lvJobs.Columns.Count - 1 || m_columns[column.Index].Width != -2)
                    continue;

                const int Pad = 4;

                // Calculate column header text width with padding
                int columnHeaderWidth = TextRenderer.MeasureText(column.Text, Font).Width + Pad * 2;

                // If there is an image assigned to the header, add its width with padding
                if (lvJobs.SmallImageList != null && column.ImageIndex > -1)
                    columnHeaderWidth += lvJobs.SmallImageList.ImageSize.Width + Pad;

                // Calculate the width of the header and the items of the column
                int columnMaxWidth = column.ListView.Items.Cast<ListViewItem>().Select(
                    item => TextRenderer.MeasureText(item.SubItems[column.Index].Text, Font).Width).Concat(
                        new[] { columnHeaderWidth }).Max() + Pad + 1;

                // Assign the width found
                column.Width = columnMaxWidth;
            }
        }

        /// <summary>
        /// Updates the item sorter.
        /// </summary>
        private void UpdateSort()
        {
            lvJobs.ListViewItemSorter = new ListViewItemComparerByTag<IndustryJob>(
                new IndustryJobComparer(m_sortCriteria, m_sortAscending));

            UpdateSortVisualFeedback();
        }

        /// <summary>
        /// Updates the sort feedback (the arrow on the header).
        /// </summary>
        private void UpdateSortVisualFeedback()
        {
            foreach (ColumnHeader columnHeader in lvJobs.Columns.Cast<ColumnHeader>())
            {
                IndustryJobColumn column = (IndustryJobColumn)columnHeader.Tag;
                if (m_sortCriteria == column)
                    columnHeader.ImageIndex = m_sortAscending ? 0 : 1;
                else
                    columnHeader.ImageIndex = 2;
            }
        }

        /// <summary>
        /// Updates the listview sub-item.
        /// </summary>
        /// <param name="job"></param>
        /// <param name="item"></param>
        /// <param name="column"></param>
        private static void SetColumn(IndustryJob job, ListViewItem.ListViewSubItem item, IndustryJobColumn column)
        {
            switch (column)
            {
                case IndustryJobColumn.State:
                    item.Text = job.State == JobState.Active
                        ? job.ActiveJobState.GetDescription()
                        : job.State.ToString();
                    item.ForeColor = GetStateColor(job);
                    break;
                case IndustryJobColumn.TTC:
                    item.Text = job.TTC;
                    if (job.State == JobState.Paused)
                        item.ForeColor = Color.Red;
                    break;
                case IndustryJobColumn.InstalledItem:
                    item.Text = job.InstalledItem.Name;
                    break;
                case IndustryJobColumn.InstalledItemType:
                    item.Text = job.InstalledItem.MarketGroup.CategoryPath;
                    break;
                case IndustryJobColumn.OutputItem:
                    item.Text = $"{GetUnitCount(job)} Unit" +
                                $"{(GetUnitCount(job) > 1 ? "s" : String.Empty)}" +
                                $" of {job.OutputItem.Name}";
                    break;
                case IndustryJobColumn.OutputItemType:
                    item.Text = job.OutputItem.MarketGroup.CategoryPath;
                    break;
                case IndustryJobColumn.Activity:
                    item.Text = job.Activity.GetDescription();
                    break;
                case IndustryJobColumn.InstallTime:
                    item.Text = $"{job.InstalledTime.ToLocalTime()}";
                    break;
                case IndustryJobColumn.EndTime:
                    item.Text = $"{job.EndDate.ToLocalTime()}";
                    break;
                case IndustryJobColumn.OriginalOrCopy:
                    item.Text = EveMonConstants.UnknownText;
                    break;
                case IndustryJobColumn.InstalledME:
                    item.Text = String.Empty; /*(job.Activity == BlueprintActivity.ResearchingMaterialEfficiency
                        ? job.InstalledME.ToString(CultureConstants.DefaultCulture)
                        : String.Empty);*/
                    break;
                case IndustryJobColumn.EndME:
                    item.Text = String.Empty; /*(job.Activity == BlueprintActivity.ResearchingMaterialEfficiency
                        ? (job.InstalledME + job.Runs).ToString(CultureConstants.DefaultCulture)
                        : String.Empty);*/
                    break;
                case IndustryJobColumn.InstalledPE:
                    item.Text = String.Empty; /*(job.Activity == BlueprintActivity.ResearchingTimeEfficiency
                        ? job.InstalledTE.ToString(CultureConstants.DefaultCulture)
                        : String.Empty);*/
                    break;
                case IndustryJobColumn.EndPE:
                    item.Text = String.Empty; /*(job.Activity == BlueprintActivity.ResearchingTimeEfficiency
                        ? (job.InstalledTE + job.Runs).ToString(CultureConstants.DefaultCulture)
                        : String.Empty);*/
                    break;
                case IndustryJobColumn.Location:
                    item.Text = job.FullLocation;
                    break;
                case IndustryJobColumn.Region:
                    item.Text = job.SolarSystem.Constellation.Region.Name;
                    break;
                case IndustryJobColumn.SolarSystem:
                    item.Text = job.SolarSystem.Name;
                    item.ForeColor = job.SolarSystem.SecurityLevelColor;
                    break;
                case IndustryJobColumn.Installation:
                    item.Text = job.Installation;
                    break;
                case IndustryJobColumn.IssuedFor:
                    item.Text = job.IssuedFor.ToString();
                    break;
                case IndustryJobColumn.LastStateChange:
                    item.Text = $"{job.LastStateChange.ToLocalTime()}";
                    break;
                case IndustryJobColumn.Cost:
                    item.Text = job.Cost.ToNumericString(2);
                    break;
                case IndustryJobColumn.Probability:
                    item.Text = Math.Abs(job.Probability) < Double.Epsilon
                        ? String.Empty
                        : $"{job.Probability:P1}";
                    break;
                case IndustryJobColumn.Runs:
                    item.Text = $"{job.Runs}";
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        #endregion


        # region Helper Methods

        /// <summary>
        /// Gets the unit count.
        /// </summary>
        /// <param name="job">The job.</param>
        /// <returns></returns>
        private static int GetUnitCount(IndustryJob job)
        {
            if (job.Activity != BlueprintActivity.Manufacturing)
                return 1;

            // Returns the ammount produced
            return job.Runs * job.OutputItem.PortionSize;
        }

        /// <summary>
        /// Gets the color of the state.
        /// </summary>
        /// <param name="job">The job.</param>
        /// <returns></returns>
        private static Color GetStateColor(IndustryJob job)
        {
            switch (job.State)
            {
                case JobState.Canceled:
                    return Color.DarkGray;
                case JobState.Failed:
                    return Color.DarkRed;
                case JobState.Paused:
                    return Color.RoyalBlue;
                case JobState.Active:
                    return GetActiveJobStateColor(job.ActiveJobState);
                default:
                    return SystemColors.GrayText;
            }
        }

        /// <summary>
        /// Gets the color of the active job state.
        /// </summary>
        /// <param name="activeJobState">State of the active job.</param>
        /// <returns></returns>
        private static Color GetActiveJobStateColor(ActiveJobState activeJobState)
        {
            switch (activeJobState)
            {
                case ActiveJobState.Pending:
                    return Color.Red;
                case ActiveJobState.InProgress:
                    return Color.Orange;
                case ActiveJobState.Ready:
                    return Color.Green;
                default:
                    return SystemColors.GrayText;
            }
        }

        /// <summary>
        /// Checks the given text matches the item.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="text">The text.</param>
        /// <returns>
        /// 	<c>true</c> if [is text matching] [the specified x]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsTextMatching(IndustryJob x, string text) => String.IsNullOrEmpty(text)
                                                                          ||
                                                                          x.InstalledItem.Name.ToUpperInvariant()
                                                                              .Contains(text, ignoreCase: true)
                                                                          ||
                                                                          x.OutputItem.Name.ToUpperInvariant()
                                                                              .Contains(text, ignoreCase: true)
                                                                          ||
                                                                          x.Installation.ToUpperInvariant()
                                                                              .Contains(text, ignoreCase: true)
                                                                          ||
                                                                          x.SolarSystem.Name.ToUpperInvariant()
                                                                              .Contains(text, ignoreCase: true)
                                                                          ||
                                                                          x.SolarSystem.Constellation.Name.ToUpperInvariant()
                                                                              .Contains(text, ignoreCase: true)
                                                                          ||
                                                                          x.SolarSystem.Constellation.Region.Name.ToUpperInvariant
                                                                              ().Contains(text, ignoreCase: true);

        /// <summary>
        /// Updates the time to completion.
        /// </summary>
        private void UpdateTimeToCompletion()
        {
            const int Pad = 4;
            int columnTTCIndex = m_columns.IndexOf(m_columns.FirstOrDefault(x => x.Column == IndustryJobColumn.TTC));

            foreach (ListViewItem listViewItem in lvJobs.Items.Cast<ListViewItem>())
            {
                IndustryJob job = (IndustryJob)listViewItem.Tag;
                if (!job.IsActive || job.ActiveJobState == ActiveJobState.Ready)
                    continue;

                // Update the time to completion
                if (columnTTCIndex != -1 && m_columns[columnTTCIndex].Visible)
                {
                    if (m_columnTTCDisplayIndex == -1)
                        m_columnTTCDisplayIndex = lvJobs.Columns[columnTTCIndex].DisplayIndex;

                    listViewItem.SubItems[m_columnTTCDisplayIndex].Text = job.TTC;

                    // Using AutoResizeColumn when TTC is the first column
                    // results to a nasty visual bug due to ListViewItem.ImageIndex placeholder
                    if (m_columnTTCDisplayIndex == 0)
                    {
                        // Calculate column header text width with padding
                        int columnHeaderWidth =
                            TextRenderer.MeasureText(lvJobs.Columns[m_columnTTCDisplayIndex].Text, Font).Width + Pad * 2;

                        // If there is an image assigned to the header, add its width with padding
                        if (ilIcons.ImageSize.Width > 0)
                            columnHeaderWidth += ilIcons.ImageSize.Width + Pad;

                        int columnWidth = lvJobs.Items.Cast<ListViewItem>().Select(
                            item => TextRenderer.MeasureText(item.SubItems[m_columnTTCDisplayIndex].Text, Font).Width).Concat(
                                new[] { columnHeaderWidth }).Max() + Pad + 2;
                        lvJobs.Columns[m_columnTTCDisplayIndex].Width = columnWidth;
                    }
                    else
                        lvJobs.AutoResizeColumn(m_columnTTCDisplayIndex, ColumnHeaderAutoResizeStyle.HeaderSize);
                }

                // Job was pending and its time to start
                if (job.ActiveJobState == ActiveJobState.Pending && job.StartDate < DateTime.UtcNow)
                {
                    job.ActiveJobState = ActiveJobState.InProgress;
                    UpdateContent();
                }

                if (job.TTC.Length != 0)
                    continue;

                // Job is ready
                job.ActiveJobState = ActiveJobState.Ready;
                UpdateContent();
            }
        }

        #endregion


        #region Event Handlers

        /// <summary>
        /// Exports item info to CSV format.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exportToCSVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListViewExporter.CreateCSV(lvJobs);
        }

        /// <summary>
        /// Handles the Tick event of the m_timer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void refresh_TimerTick(object sender, EventArgs e)
        {
            UpdateTimeToCompletion();
        }

        /// <summary>
        /// On column reorder we update the settings.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView_ColumnReordered(object sender, ColumnReorderedEventArgs e)
        {
            m_columnsChanged = true;
        }

        /// <summary>
        /// When the user manually resizes a column, we make sure to update the column preferences.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView_ColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
        {
            if (m_isUpdatingColumns || m_columns.Count <= e.ColumnIndex)
                return;

            // Don't update the columns if the TTC column width changes
            if (e.ColumnIndex == m_columnTTCDisplayIndex)
                return;

            if (m_columns[e.ColumnIndex].Width == lvJobs.Columns[e.ColumnIndex].Width)
                return;

            m_columns[e.ColumnIndex].Width = lvJobs.Columns[e.ColumnIndex].Width;
            m_columnsChanged = true;
        }

        /// <summary>
        /// When the user clicks a column header, we update the sorting.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            IndustryJobColumn column = (IndustryJobColumn)lvJobs.Columns[e.Column].Tag;
            if (m_sortCriteria == column)
                m_sortAscending = !m_sortAscending;
            else
            {
                m_sortCriteria = column;
                m_sortAscending = true;
            }

            m_isUpdatingColumns = true;

            // Updates the item sorter
            UpdateSort();

            m_isUpdatingColumns = false;
        }

        /// <summary>
        /// When the mouse gets pressed, we change the cursor.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void listView_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            lvJobs.Cursor = Cursors.Default;
        }

        /// <summary>
        /// When the mouse moves over the list, we show the item's tooltip if over an item.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
        private void listView_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                return;

            lvJobs.Cursor = CustomCursors.ContextMenu;

            ListViewItem item = lvJobs.GetItemAt(e.Location.X, e.Location.Y);
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
        private void listView_MouseLeave(object sender, EventArgs e)
        {
            m_tooltip.Hide();
        }

        /// <summary>
        /// Handles the Opening event of the contextMenu control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="CancelEventArgs"/> instance containing the event data.</param>
        private void contextMenu_Opening(object sender, CancelEventArgs e)
        {
            bool visible = lvJobs.SelectedItems.Count != 0;

            showInstalledInBrowserMenuItem.Visible =
                showProducedInBrowserMenuItem.Visible =
                    showInBrowserMenuSeparator.Visible = visible;

            if (!visible)
                return;

            IndustryJob job = lvJobs.SelectedItems[0]?.Tag as IndustryJob;

            if (job?.InstalledItem == null || job.OutputItem == null)
                return;

            Blueprint blueprint = StaticBlueprints.GetBlueprintByID(job.OutputItem.ID);
            Ship ship = job.OutputItem as Ship;

            string text = ship != null ? "Ship" : blueprint != null ? "Blueprint" : "Item";

            showProducedInBrowserMenuItem.Text = $"Show Output In {text} Browser...";
        }

        /// <summary>
        /// Handles the Click event of the showInBrowserMenuItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void showInBrowserMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripItem menuItem = sender as ToolStripItem;

            if (menuItem == null)
                return;

            IndustryJob job = lvJobs.SelectedItems[0]?.Tag as IndustryJob;

            if (menuItem == showInstalledInBrowserMenuItem)
            {
                if (job?.OutputItem == null)
                    return;

                // showProducedInBrowserMenuItem was clicked
                Ship ship = job.OutputItem as Ship;
                Blueprint blueprint = StaticBlueprints.GetBlueprintByID(job.OutputItem.ID);

                PlanWindow planWindow = PlanWindow.ShowPlanWindow(Character);

                if (ship != null)
                    planWindow.ShowShipInBrowser(ship);
                else if (blueprint != null)
                    planWindow.ShowBlueprintInBrowser(blueprint);
                else
                    planWindow.ShowItemInBrowser(job.OutputItem);

                return;
            }

            if (job?.InstalledItem == null)
                return;

            // showInstalledInBrowserMenuItem was clicked
            PlanWindow.ShowPlanWindow(Character).ShowBlueprintInBrowser(job.InstalledItem);
        }

        # endregion


        #region Global Events

        /// <summary>
        /// Handles the IndustryJobsCompleted event of the EveMonClient control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="IndustryJobsEventArgs"/> instance containing the event data.</param>
        private void EveMonClient_CharacterIndustryJobsCompleted(object sender, IndustryJobsEventArgs e)
        {
            UpdateContent();
        }

        /// <summary>
        /// When the ID to name conversion is updated, update the list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EveMonClient_EveIDToNameUpdated(object sender, EventArgs e)
        {
            UpdateContent();
        }

        /// <summary>
        /// When the industry jobs are updated, update the list and the expandable panel info.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EveMonClient_IndustryJobsUpdated(object sender, CharacterChangedEventArgs e)
        {
            if (Character == null || e.Character != Character)
                return;

            Jobs = Character.IndustryJobs;
            UpdateColumns();
        }

        /// <summary>
        /// On timer tick, we update the internal timer and 
        /// and the columns settings if any changes have beem made to them.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EveMonClient_TimerTick(object sender, EventArgs e)
        {
            if (!Visible)
            {
                if (m_refreshTimer.Enabled)
                    m_refreshTimer.Stop();

                return;
            }

            // Find how many jobs are active and not ready
            int activeJobs = lvJobs.Items.Cast<ListViewItem>().Select(
                item => (IndustryJob)item.Tag).Count(job => job.IsActive && job.ActiveJobState != ActiveJobState.Ready);

            // We use time dilation according to the ammount of active jobs that are not ready,
            // due to excess CPU usage for computing the 'time to completion' when there are too many jobs
            m_refreshTimer.Interval = 900 + 100 * activeJobs;

            if (!m_columnsChanged)
                return;

            Settings.UI.MainWindow.IndustryJobs.Columns.Clear();
            Settings.UI.MainWindow.IndustryJobs.Columns.AddRange(Columns.Cast<IndustryJobColumnSettings>());

            // Recreate the columns
            Columns = Settings.UI.MainWindow.IndustryJobs.Columns;
            m_columnsChanged = false;
        }

        /// <summary>
        /// When Conquerable Station List updates, update the list.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void EveMonClient_ConquerableStationListUpdated(object sender, EventArgs e)
        {
            if (Character == null)
                return;

            foreach (IndustryJob job in m_list)
            {
                job.UpdateInstallation(this.Character);
            }

            UpdateColumns();
        }

#endregion


        #region Updates Expandable Panel On Global Events

        /// <summary>
        /// Updates the content of the expandable panel.
        /// </summary>
        private void UpdateExpPanelContent()
        {
            if (Character == null)
            {
                industryExpPanelControl.Visible = false;
                return;
            }

            // Calculate the related info for the panel
            CalculatePanelInfo();

            // Update the Header text of the panel
            UpdateHeaderText();

            // Update the info in the panel
            UpdatePanelInfo();

            // Force to redraw
            industryExpPanelControl.Refresh();
        }

        /// <summary>
        /// Updates the header text of the panel.
        /// </summary>
        private void UpdateHeaderText()
        {
            const int BaseJobs = 1;
            Int64 maxManufacturingJobs = BaseJobs + m_skillBasedManufacturingJobs;
            Int64 maxResearchingJobs = BaseJobs + m_skillBasedResearchingJobs;
            Int64 remainingManufacturingJobs = maxManufacturingJobs - m_activeManufJobsIssuedForCharacterCount -
                                               m_activeManufJobsIssuedForCorporationCount;
            Int64 remainingResearchingJobs = maxResearchingJobs - m_activeResearchJobsIssuedForCharacterCount -
                                             m_activeResearchJobsIssuedForCorporationCount;

            string manufJobsRemainingText =
                $"Manufacturing Jobs Remaining: {remainingManufacturingJobs} out of {maxManufacturingJobs} max";
            string researchJobsRemainingText =
                $"Researching Jobs Remaining: {remainingResearchingJobs} out of {maxResearchingJobs} max";
            industryExpPanelControl.HeaderText = $"{manufJobsRemainingText}{String.Empty,5}{researchJobsRemainingText}";
        }

        /// <summary>
        /// Updates the labels text in the panel.
        /// </summary>
        private void UpdatePanelInfo()
        {
            // Basic label text
            m_lblActiveManufacturingJobs.Text =
                $"Active Manufacturing Jobs: {m_activeManufJobsIssuedForCharacterCount + m_activeManufJobsIssuedForCorporationCount}";
            m_lblActiveResearchingJobs.Text =
                $"Active Researching Jobs: {m_activeResearchJobsIssuedForCharacterCount + m_activeResearchJobsIssuedForCorporationCount}";
            m_lblRemoteManufacturingRange.Text =
                $"Remote Manufacturing Range: limited to {StaticGeography.GetRange(m_remoteManufacturingRange)}";
            m_lblRemoteResearchingRange.Text =
                $"Remote Researching Range: limited to {StaticGeography.GetRange(m_remoteResearchingRange)}";

            if (HasActiveCorporationIssuedJobs)
            {
                // Supplemental label text
                m_lblActiveCharManufacturingJobs.Text = $"Character Issued: {m_activeManufJobsIssuedForCharacterCount}";
                m_lblActiveCorpManufacturingJobs.Text = $"Corporation Issued: {m_activeManufJobsIssuedForCorporationCount}";
                m_lblActiveCharResearchingJobs.Text = $"Character Issued: {m_activeResearchJobsIssuedForCharacterCount}";
                m_lblActiveCorpResearchingJobs.Text = $"Corporation Issued: {m_activeResearchJobsIssuedForCorporationCount}";
            }

            // Update label position
            UpdatePanelControlPosition();
        }

        /// <summary>
        /// Updates expandable panel controls positions.
        /// </summary>
        private void UpdatePanelControlPosition()
        {
            industryExpPanelControl.SuspendLayout();

            const int Pad = 5;
            int height = industryExpPanelControl.ExpandDirection == Direction.Up ? Pad : industryExpPanelControl.HeaderHeight;

            m_lblActiveManufacturingJobs.Location = new Point(5, height);
            m_lblRemoteManufacturingRange.Location = new Point(m_lblRemoteManufacturingRange.Location.X, height);
            height += m_lblActiveManufacturingJobs.Height;
            m_lblRemoteResearchingRange.Location = new Point(m_lblRemoteResearchingRange.Location.X, height);

            if (HasActiveCorporationIssuedJobs)
            {
                m_lblActiveCharManufacturingJobs.Location = new Point(15, height);
                m_lblActiveCharManufacturingJobs.Visible = true;
                height += m_lblActiveCharManufacturingJobs.Height;

                m_lblActiveCorpManufacturingJobs.Location = new Point(15, height);
                m_lblActiveCorpManufacturingJobs.Visible = true;
                height += m_lblActiveCorpManufacturingJobs.Height;
                height += Pad;
            }
            else
            {
                m_lblActiveCharManufacturingJobs.Visible = false;
                m_lblActiveCorpManufacturingJobs.Visible = false;
            }

            m_lblActiveResearchingJobs.Location = new Point(5, height);
            height += m_lblActiveResearchingJobs.Height;

            if (HasActiveCorporationIssuedJobs)
            {
                m_lblActiveCharResearchingJobs.Location = new Point(15, height);
                m_lblActiveCharResearchingJobs.Visible = true;
                height += m_lblActiveCharResearchingJobs.Height;

                m_lblActiveCorpResearchingJobs.Location = new Point(15, height);
                m_lblActiveCorpResearchingJobs.Visible = true;
                height += m_lblActiveCorpResearchingJobs.Height;
            }
            else
            {
                m_lblActiveCharResearchingJobs.Visible = false;
                m_lblActiveCorpResearchingJobs.Visible = false;
            }

            height += Pad;

            // Update panel's expanded height
            industryExpPanelControl.ExpandedHeight = height + (industryExpPanelControl.ExpandDirection == Direction.Up
                ? industryExpPanelControl.HeaderHeight
                : Pad);

            industryExpPanelControl.ResumeLayout(false);
        }


        /// <summary>
        /// Calculates the industry jobs related info for the panel.
        /// </summary>
        private void CalculatePanelInfo()
        {
            m_activeManufJobsIssuedForCharacterCount = m_list.Count(
                x => x.State == JobState.Active && x.Activity == BlueprintActivity.Manufacturing &&
                     x.IssuedFor == IssuedFor.Character);

            m_activeManufJobsIssuedForCorporationCount = m_list.Count(
                x => x.State == JobState.Active && x.Activity == BlueprintActivity.Manufacturing &&
                     x.IssuedFor == IssuedFor.Corporation);

            m_activeResearchJobsIssuedForCharacterCount = m_list.Count(
                x => x.State == JobState.Active && x.Activity != BlueprintActivity.Manufacturing &&
                     x.IssuedFor == IssuedFor.Character);

            m_activeResearchJobsIssuedForCorporationCount = m_list.Count(
                x => x.State == JobState.Active && x.Activity != BlueprintActivity.Manufacturing &&
                     x.IssuedFor == IssuedFor.Corporation);

            // Calculate character's max manufacturing jobs
            m_skillBasedManufacturingJobs = (int)(Character.Skills[DBConstants.MassProductionSkillID].
                LastConfirmedLvl + Character.Skills[DBConstants.AdvancedMassProductionSkillID].
                LastConfirmedLvl);

            // Calculate character's max researching jobs
            m_skillBasedResearchingJobs = (int)(Character.Skills[DBConstants.LaboratoryOperationSkillID].
                LastConfirmedLvl + Character.Skills[DBConstants.AdvancedLaboratoryOperationSkillID].
                LastConfirmedLvl);

            // Calculate character's remote manufacturing range
            m_remoteManufacturingRange = (int)Character.Skills[DBConstants.SupplyChainManagementSkillID].
                LastConfirmedLvl;

            // Calculate character's remote researching range
            m_remoteResearchingRange = (int)Character.Skills[DBConstants.ScientificNetworkingSkillID].
                LastConfirmedLvl;
        }

        # endregion


        #region Initialize Expandable Panel Controls

        // Basic labels constructor
        private readonly Label m_lblActiveManufacturingJobs = new Label();
        private readonly Label m_lblActiveResearchingJobs = new Label();
        private readonly Label m_lblRemoteManufacturingRange = new Label();
        private readonly Label m_lblRemoteResearchingRange = new Label();

        // Supplemental labels constructor
        private readonly Label m_lblActiveCharManufacturingJobs = new Label();
        private readonly Label m_lblActiveCorpManufacturingJobs = new Label();
        private readonly Label m_lblActiveCharResearchingJobs = new Label();
        private readonly Label m_lblActiveCorpResearchingJobs = new Label();

        private void InitializeExpandablePanelControls()
        {
            industryExpPanelControl.SuspendLayout();

            // Add basic labels to panel
            industryExpPanelControl.Controls.AddRange(new Control[]
            {
                m_lblActiveManufacturingJobs,
                m_lblActiveResearchingJobs,
                m_lblRemoteManufacturingRange,
                m_lblRemoteResearchingRange
            });

            // Add supplemental labels to panel
            industryExpPanelControl.Controls.AddRange(new Control[]
            {
                m_lblActiveCharManufacturingJobs,
                m_lblActiveCorpManufacturingJobs,
                m_lblActiveCharResearchingJobs,
                m_lblActiveCorpResearchingJobs
            });

            // Apply properties
            foreach (Label label in industryExpPanelControl.Controls.OfType<Label>())
            {
                label.ForeColor = SystemColors.ControlText;
                label.BackColor = Color.Transparent;
                label.AutoSize = true;
            }

            // Special properties
            m_lblRemoteManufacturingRange.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            m_lblRemoteResearchingRange.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            m_lblRemoteManufacturingRange.Location = new Point(170, 0);
            m_lblRemoteResearchingRange.Location = new Point(170, 0);

            // Subscribe events
            foreach (Label label in industryExpPanelControl.Controls.OfType<Label>())
            {
                label.MouseClick += OnExpandablePanelMouseClick;
            }

            industryExpPanelControl.ResumeLayout(false);
        }

        /// <summary>
        /// Called when the expandable panel gets mouse clicked.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
        private void OnExpandablePanelMouseClick(object sender, MouseEventArgs e)
        {
            industryExpPanelControl.OnMouseClick(sender, e);
        }

        #endregion
    }
}
