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
using EVEMon.DetailsWindow;
using EVEMon.SkillPlanner;

namespace EVEMon.CharacterMonitoring
{
    internal sealed partial class CharacterContractsList : UserControl, IListView
    {
        #region Fields

        private readonly List<ContractColumnSettings> m_columns = new List<ContractColumnSettings>();
        private readonly List<Contract> m_list = new List<Contract>();

        private InfiniteDisplayToolTip m_tooltip;
        private ContractGrouping m_grouping;
        private ContractColumn m_sortCriteria;
        private IssuedFor m_showIssuedFor;

        private string m_textFilter = String.Empty;
        private bool m_sortAscending = true;

        private bool m_isUpdatingColumns;
        private bool m_columnsChanged;
        private bool m_init;

        #endregion


        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        public CharacterContractsList()
        {
            InitializeComponent();

            lvContracts.Hide();
            lvContracts.AllowColumnReorder = true;
            lvContracts.Columns.Clear();

            m_showIssuedFor = IssuedFor.All;

            showDetailsToolStripMenuItem.Font = FontFactory.GetFont("Segoe UI", 9F, FontStyle.Bold);
            noContractsLabel.Font = FontFactory.GetFont("Tahoma", 11.25F, FontStyle.Bold);

            ListViewHelper.EnableDoubleBuffer(lvContracts);

            lvContracts.ColumnClick += lvContracts_ColumnClick;
            lvContracts.ColumnWidthChanged += lvContracts_ColumnWidthChanged;
            lvContracts.ColumnReordered += lvContracts_ColumnReordered;
            lvContracts.MouseDown += listView_MouseDown;
            lvContracts.MouseMove += listView_MouseMove;
            lvContracts.MouseLeave += listView_MouseLeave;
        }

        #endregion


        #region Properties

        /// <summary>
        /// Gets the character associated with this monitor.
        /// </summary>
        internal CCPCharacter Character { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="lvContracts"/> is visible.
        /// </summary>
        /// <value><c>true</c> if visible; otherwise, <c>false</c>.</value>
        internal bool Visibility
        {
            get { return lvContracts.Visible; }
            set { lvContracts.Visible = value; }
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
        /// Gets or sets the enumeration of research points to display.
        /// </summary>
        internal IEnumerable<Contract> Contracts
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
        /// Gets or sets the grouping mode.
        /// </summary>
        [Browsable(false)]
        public Enum Grouping
        {
            get { return m_grouping; }
            set
            {
                m_grouping = (ContractGrouping)value;
                if (m_init)
                    UpdateColumns();
            }
        }

        /// <summary>
        /// Gets or sets which "Issued for" contracts to display.
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
        /// Gets or sets the settings used for columns.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public IEnumerable<IColumnSettings> Columns
        {
            get
            {
                // Add the visible columns; matching the display order
                List<ContractColumnSettings> newColumns = new List<ContractColumnSettings>();
                foreach (ColumnHeader header in lvContracts.Columns.Cast<ColumnHeader>().OrderBy(x => x.DisplayIndex))
                {
                    ContractColumnSettings columnSetting = m_columns.First(x => x.Column == (ContractColumn)header.Tag);
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
                    m_columns.AddRange(value.Cast<ContractColumnSettings>());

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

            m_tooltip = new InfiniteDisplayToolTip(lvContracts);

            EveMonClient.TimerTick += EveMonClient_TimerTick;
            EveMonClient.ContractsUpdated += EveMonClient_ContractsUpdated;
            EveMonClient.EveIDToNameUpdated += EveMonClient_EveIDToNameUpdated;
            EveMonClient.ConquerableStationListUpdated += EveMonClient_ConquerableStationListUpdated;
            EveMonClient.CharacterContractItemsDownloaded += EveMonClient_ContractItemsDownloaded;
            EveMonClient.CorporationContractItemsDownloaded += EveMonClient_ContractItemsDownloaded;
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

            EveMonClient.TimerTick -= EveMonClient_TimerTick;
            EveMonClient.ContractsUpdated -= EveMonClient_ContractsUpdated;
            EveMonClient.EveIDToNameUpdated -= EveMonClient_EveIDToNameUpdated;
            EveMonClient.ConquerableStationListUpdated -= EveMonClient_ConquerableStationListUpdated;
            EveMonClient.CharacterContractItemsDownloaded -= EveMonClient_ContractItemsDownloaded;
            EveMonClient.CorporationContractItemsDownloaded -= EveMonClient_ContractItemsDownloaded;
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

            lvContracts.Visible = false;

            Contracts = Character?.Contracts;
            Columns = Settings.UI.MainWindow.Contracts.Columns;
            Grouping = Character?.UISettings.ContractsGroupBy;
            TextFilter = String.Empty;

            UpdateColumns();

            m_init = true;

            UpdateListVisibility();
        }

        # endregion


        #region Update Methods

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

            lvContracts.BeginUpdate();
            m_isUpdatingColumns = true;

            try
            {
                lvContracts.Columns.Clear();
                lvContracts.Groups.Clear();
                lvContracts.Items.Clear();

                foreach (ContractColumnSettings column in m_columns.Where(x => x.Visible))
                {
                    ColumnHeader header = lvContracts.Columns.Add(column.Column.GetHeader(), column.Width);
                    header.Tag = column.Column;

                    switch (column.Column)
                    {
                        case ContractColumn.Price:
                        case ContractColumn.Reward:
                        case ContractColumn.Collateral:
                        case ContractColumn.Buyout:
                        case ContractColumn.Volume:
                        case ContractColumn.Issued:
                        case ContractColumn.Duration:
                        case ContractColumn.DaysToComplete:
                            header.TextAlign = HorizontalAlignment.Right;
                            break;
                    }
                }

                // We update the content
                UpdateContent();
            }
            finally
            {
                lvContracts.EndUpdate();
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

            int scrollBarPosition = lvContracts.GetVerticalScrollBarPosition();

            // Store the selected item (if any) to restore it after the update
            int selectedItem = lvContracts.SelectedItems.Count > 0 ? lvContracts.
                SelectedItems[0].Tag.GetHashCode() : 0;

            lvContracts.BeginUpdate();
            try
            {
                bool filterInactive = Character != null && Settings.UI.MainWindow.Contracts.
                    HideInactiveContracts, filterIssued = m_showIssuedFor != IssuedFor.All;
                var contracts = new LinkedList<Contract>();
                foreach (var con in m_list)
                    // Filter on valid contracts matching text
                    if (con.ContractType != ContractType.None && con.StartStation != null &&
                        con.EndStation != null && IsTextMatching(con, m_textFilter))
                    {
                        con.UpdateContractItems();
                        // Filter on issued type and availability
                        if ((!filterIssued || con.IssuedFor == m_showIssuedFor) &&
                            (!filterInactive || con.IsAvailable || con.NeedsAttention))
                        contracts.AddLast(con);
                    }

                UpdateSort();
                UpdateContentByGroup(contracts);

                // Restore the selected item (if any)
                if (selectedItem > 0)
                {
                    foreach (ListViewItem lvItem in lvContracts.Items.Cast<ListViewItem>().Where(
                        lvItem => lvItem.Tag.GetHashCode() == selectedItem))
                    {
                        lvItem.Selected = true;
                    }
                }

                // Adjust the size of the columns
                AdjustColumns();

                UpdateListVisibility();
            }
            finally
            {
                lvContracts.EndUpdate();
                lvContracts.SetVerticalScrollBarPosition(scrollBarPosition);
            }
        }

        /// <summary>
        /// Updates the list visibility.
        /// </summary>
        private void UpdateListVisibility()
        {
            // Display or hide the "no contracts" label
            if (!m_init)
                return;

            noContractsLabel.Visible = lvContracts.Items.Count == 0;
            lvContracts.Visible = !noContractsLabel.Visible;
        }

        /// <summary>
        /// Updates the content by group.
        /// </summary>
        /// <param name="contracts">The contracts.</param>
        private void UpdateContentByGroup(IEnumerable<Contract> contracts)
        {
            switch (m_grouping)
            {
                case ContractGrouping.State:
                    IOrderedEnumerable<IGrouping<ContractState, Contract>> groups0 =
                        contracts.GroupBy(x => x.State).OrderBy(x => (int)x.Key);
                    UpdateContent(groups0);
                    break;
                case ContractGrouping.StateDesc:
                    IOrderedEnumerable<IGrouping<ContractState, Contract>> groups1 =
                        contracts.GroupBy(x => x.State).OrderByDescending(x => (int)x.Key);
                    UpdateContent(groups1);
                    break;
                case ContractGrouping.Issued:
                    IOrderedEnumerable<IGrouping<DateTime, Contract>> groups4 =
                        contracts.GroupBy(x => x.Issued.ToLocalTime().Date).OrderBy(x => x.Key);
                    UpdateContent(groups4);
                    break;
                case ContractGrouping.IssuedDesc:
                    IOrderedEnumerable<IGrouping<DateTime, Contract>> groups5 =
                        contracts.GroupBy(x => x.Issued.ToLocalTime().Date).OrderByDescending(x => x.Key);
                    UpdateContent(groups5);
                    break;
                case ContractGrouping.ContractType:
                    IOrderedEnumerable<IGrouping<ContractType, Contract>> groups6 =
                        contracts.GroupBy(x => x.ContractType).OrderBy(x => x.Key);
                    UpdateContent(groups6);
                    break;
                case ContractGrouping.ContractTypeDesc:
                    IOrderedEnumerable<IGrouping<ContractType, Contract>> groups7 =
                        contracts.GroupBy(x => x.ContractType).OrderByDescending(x => x.Key);
                    UpdateContent(groups7);
                    break;
                case ContractGrouping.StartLocation:
                    IOrderedEnumerable<IGrouping<Station, Contract>> groups8 =
                        contracts.GroupBy(x => x.StartStation).OrderBy(x => x.Key.Name);
                    UpdateContent(groups8);
                    break;
                case ContractGrouping.StartLocationDesc:
                    IOrderedEnumerable<IGrouping<Station, Contract>> groups9 =
                        contracts.GroupBy(x => x.StartStation).OrderByDescending(x => x.Key.Name);
                    UpdateContent(groups9);
                    break;
            }
        }

        /// <summary>
        /// Updates the content of the listview.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="groups"></param>
        private void UpdateContent<TKey>(IEnumerable<IGrouping<TKey, Contract>> groups)
        {
            lvContracts.Items.Clear();
            lvContracts.Groups.Clear();

            // Add the groups
            foreach (IGrouping<TKey, Contract> group in groups)
            {
                string groupText;
                if (group.Key is ContractState)
                    groupText = ((ContractState)(Object)group.Key).GetHeader();
                else if (group.Key is ContractType)
                    groupText = ((ContractType)(Object)group.Key).GetDescription();
                else if (group.Key is DateTime)
                    groupText = ((DateTime)(Object)group.Key).ToShortDateString();
                else
                    groupText = group.Key.ToString();

                ListViewGroup listGroup = new ListViewGroup(groupText);
                lvContracts.Groups.Add(listGroup);

                // Add the items in every group
                lvContracts.Items.AddRange(
                    group.Select(contract =>
                        new
                        {
                            contract,
                            item = new ListViewItem(contract.ContractText, listGroup)
                            {
                                UseItemStyleForSubItems = false,
                                Tag = contract
                            }
                        }).Select(x => CreateSubItems(x.contract, x.item)).ToArray());
            }
        }

        /// <summary>
        /// Creates the list view sub items.
        /// </summary>
        /// <param name="contract">The contract.</param>
        /// <param name="item">The item.</param>
        private ListViewItem CreateSubItems(Contract contract, ListViewItem item)
        {
            // Display text as dimmed if the contract is no longer available
            if (!contract.IsAvailable && !contract.NeedsAttention)
                item.ForeColor = SystemColors.GrayText;

            // Add enough subitems to match the number of columns
            while (item.SubItems.Count < lvContracts.Columns.Count + 1)
            {
                item.SubItems.Add(string.Empty);
            }

            // Creates the subitems
            for (int i = 0; i < lvContracts.Columns.Count; i++)
            {
                ContractColumn column = (ContractColumn)lvContracts.Columns[i].Tag;
                SetColumn(contract, item.SubItems[i], column);
            }

            // Tooltip
            var builder = new StringBuilder();
            builder.Append($"Issued For: {contract.IssuedFor}").AppendLine();
            builder.Append($"Issued: {contract.Issued.ToLocalTime()}").AppendLine();
            builder.Append($"Duration: {contract.Duration} ");
            builder.Append($"Day{(contract.Duration.S())}").AppendLine();

            if (contract.ContractType == ContractType.Courier)
            {
                builder.Append($"Days To Complete: {contract.DaysToComplete} ");
                builder.Append($"Day{(contract.DaysToComplete.S())}");
                builder.AppendLine();
            }

            string prefix = contract.ContractType == ContractType.Courier ? "Starting " :
                string.Empty;
            string startSS = contract.StartStation.SolarSystem?.FullLocation ??
                EveMonConstants.UnknownText;
            builder.Append($"{prefix}Solar System: {startSS}").AppendLine();
            builder.Append($"{prefix}Station: {contract.StartStation.Name}").AppendLine();

            if (contract.ContractType == ContractType.Courier)
            {
                string endSS = contract.EndStation.SolarSystem?.FullLocation ??
                    EveMonConstants.UnknownText;
                builder.Append($"Ending Solar System: {endSS}").AppendLine();
                builder.Append($"Ending Station: {contract.EndStation.Name}").AppendLine();
            }

            item.ToolTipText = builder.ToString();

            return item;
        }

        /// <summary>
        /// Adjusts the columns.
        /// </summary>
        private void AdjustColumns()
        {
            foreach (ColumnHeader column in lvContracts.Columns)
            {
                if (m_columns[column.Index].Width == -1)
                    m_columns[column.Index].Width = -2;

                column.Width = m_columns[column.Index].Width;

                // Due to .NET design we need to prevent the last colummn to resize to the right end

                // Return if it's not the last column and not set to auto-resize
                if (column.Index != lvContracts.Columns.Count - 1 || m_columns[column.Index].Width != -2)
                    continue;

                const int Pad = 4;

                // Calculate column header text width with padding
                int columnHeaderWidth = TextRenderer.MeasureText(column.Text, Font).Width + Pad * 2;

                // If there is an image assigned to the header, add its width with padding
                if (lvContracts.SmallImageList != null && column.ImageIndex > -1)
                    columnHeaderWidth += lvContracts.SmallImageList.ImageSize.Width + Pad;

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
            lvContracts.ListViewItemSorter = new ListViewItemComparerByTag<Contract>(
                new ContractComparer(m_sortCriteria, m_sortAscending));

            UpdateSortVisualFeedback();
        }

        /// <summary>
        /// Updates the sort feedback (the arrow on the header).
        /// </summary>
        private void UpdateSortVisualFeedback()
        {
            foreach (ColumnHeader columnHeader in lvContracts.Columns.Cast<ColumnHeader>())
            {
                ContractColumn column = (ContractColumn)columnHeader.Tag;
                if (m_sortCriteria == column)
                    columnHeader.ImageIndex = m_sortAscending ? 0 : 1;
                else
                    columnHeader.ImageIndex = 2;
            }
        }

        /// <summary>
        /// Formats the price according to the settings.
        /// </summary>
        /// <param name="price">The price to display.</param>
        /// <returns>The price as a string.</returns>
        private static string FormatPrice(decimal price)
        {
            return FormatHelper.FormatIf(Settings.UI.MainWindow.Contracts.NumberAbsFormat,
                2, price, AbbreviationFormat.AbbreviationSymbols);
        }

        /// <summary>
        /// Updates the listview sub-item.
        /// </summary>
        /// <param name="contract"></param>
        /// <param name="item"></param>
        /// <param name="column"></param>
        private static void SetColumn(Contract contract, ListViewItem.ListViewSubItem item, ContractColumn column)
        {
            Station start = contract.StartStation, end = contract.EndStation;
            switch (column)
            {
            case ContractColumn.Status:
                item.Text = contract.Status.GetDescription();
                item.ForeColor = contract.Overdue ? Color.Red : Color.Black;
                break;
            case ContractColumn.ContractText:
                item.Text = contract.ContractText;
                break;
            case ContractColumn.Title:
                item.Text = contract.Description;
                break;
            case ContractColumn.ContractType:
                item.Text = contract.ContractType.GetDescription();
                break;
            case ContractColumn.Issuer:
                item.Text = contract.Issuer;
                break;
            case ContractColumn.Assignee:
                item.Text = contract.Assignee;
                break;
            case ContractColumn.Acceptor:
                item.Text = contract.Acceptor;
                break;
            case ContractColumn.Availability:
                item.Text = contract.Availability.GetDescription();
                break;
            case ContractColumn.Price:
                item.Text = FormatPrice(contract.Price);
                break;
            case ContractColumn.Buyout:
                item.Text = FormatPrice(contract.Buyout);
                break;
            case ContractColumn.Reward:
                item.Text = FormatPrice(contract.Reward);
                break;
            case ContractColumn.Collateral:
                item.Text = FormatPrice(contract.Collateral);
                break;
            case ContractColumn.Volume:
                item.Text = FormatPrice(contract.Volume);
                break;
            case ContractColumn.StartLocation:
                item.Text = start.FullLocation;
                break;
            case ContractColumn.StartRegion:
                item.Text = start.SolarSystemChecked.Constellation.Region.Name;
                break;
            case ContractColumn.StartSolarSystem:
                item.Text = start.SolarSystemChecked.Name;
                item.ForeColor = start.SolarSystemChecked.SecurityLevelColor;
                break;
            case ContractColumn.StartStation:
                item.Text = start.Name;
                break;
            case ContractColumn.EndLocation:
                item.Text = end.FullLocation;
                break;
            case ContractColumn.EndRegion:
                item.Text = end.SolarSystemChecked.Constellation.Region.Name;
                break;
            case ContractColumn.EndSolarSystem:
                item.Text = end.SolarSystemChecked.Name;
                item.ForeColor = end.SolarSystemChecked.SecurityLevelColor;
                break;
            case ContractColumn.EndStation:
                item.Text = end.Name;
                break;
            case ContractColumn.Issued:
                item.Text = contract.Issued.ToLocalTime().ToShortDateString();
                break;
            case ContractColumn.Accepted:
                item.Text = contract.Accepted == DateTime.MinValue ? string.Empty : contract.
                    Accepted.ToLocalTime().ToShortDateString();
                break;
            case ContractColumn.Completed:
                item.Text = contract.Completed == DateTime.MinValue ? string.Empty : contract.
                    Completed.ToLocalTime().ToShortDateString();
                break;
            case ContractColumn.Duration:
                item.Text = $"{contract.Duration} Day{(contract.Duration.S())}";
                break;
            case ContractColumn.DaysToComplete:
                item.Text = contract.DaysToComplete == 0 ? string.Empty :
                    $"{contract.DaysToComplete} Day{(contract.DaysToComplete.S())}";
                break;
            case ContractColumn.Expiration:
                ListViewItemFormat format = FormatExpiration(contract);
                item.Text = format.Text;
                item.ForeColor = format.TextColor;
                break;
            case ContractColumn.IssuedFor:
                item.Text = contract.IssuedFor.ToString();
                break;
            default:
                throw new NotImplementedException();
            }
        }

        #endregion


        #region Helper Methods

        /// <summary>
        /// Checks the given text matches the item.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="text">The text.</param>
        /// <returns>
        /// 	<c>true</c> if [is text matching] [the specified x]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsTextMatching(Contract x, string text)
            => String.IsNullOrEmpty(text) ||
               x.Status.GetDescription().ToUpperInvariant().Contains(text, ignoreCase: true) ||
               x.ContractText.ToUpperInvariant().Contains(text, ignoreCase: true) ||
               x.ContractType.GetDescription().ToUpperInvariant().Contains(text, ignoreCase: true) ||
               x.Issuer.ToUpperInvariant().Contains(text, ignoreCase: true) ||
               x.Assignee.ToUpperInvariant().Contains(text, ignoreCase: true) ||
               x.Acceptor.ToUpperInvariant().Contains(text, ignoreCase: true) ||
               x.Description.ToUpperInvariant().Contains(text, ignoreCase: true) ||
               x.Availability.GetDescription().ToUpperInvariant().Contains(text, ignoreCase: true) ||
               x.StartStation.Name.ToUpperInvariant().Contains(text, ignoreCase: true) ||
               x.StartStation.SolarSystemChecked.Name.ToUpperInvariant().Contains(text, ignoreCase: true) ||
               x.StartStation.SolarSystemChecked.Constellation.Name.ToUpperInvariant().Contains(text, ignoreCase: true) ||
               x.StartStation.SolarSystemChecked.Constellation.Region.Name.ToUpperInvariant().Contains(text, ignoreCase: true);

        /// <summary>
        /// Gets the text and formatting for the expiration cell
        /// </summary>
        /// <param name="contract">Contract to generate a format for</param>
        /// <returns>ListViewItemFormat object describing the format of the cell</returns>
        private static ListViewItemFormat FormatExpiration(Contract contract)
        {
            // Initialize to sensible defaults
            ListViewItemFormat format = new ListViewItemFormat
            {
                TextColor = Color.Black,
                Text = contract.Expiration.ToRemainingTimeShortDescription(DateTimeKind.Utc)
                    .ToUpper(CultureConstants.DefaultCulture)
            };

            // Contract is expiring soon
            if (contract.IsAvailable && contract.Expiration < DateTime.UtcNow.AddDays(1))
                format.TextColor = Color.DarkOrange;

            // We have all the information for formatting an available order
            if (contract.IsAvailable)
                return format;

            // Contract isn't available so lets format it as such
            format.Text = contract.State.ToString();

            if (contract.State == ContractState.Expired)
                format.TextColor = Color.Red;

            if (contract.State == ContractState.Rejected || contract.State == ContractState.Failed)
                format.TextColor = Color.DarkRed;

            if (contract.State == ContractState.Finished)
                format.TextColor = Color.DarkGreen;

            return format;
        }

        /// <summary>
        /// Shows the contract details.
        /// </summary>
        private void ShowContractDetails()
        {
            var items = lvContracts.SelectedItems;
            if (items.Count > 0)
            {
                // Fixes a bug where double clicks as the list was updating would crash
                var contract = items[0].Tag as Contract;
                if (contract != null && contract.ContractType == ContractType.Courier ||
                    contract.ContractItems.Any())
                    WindowsFactory.ShowByTag<ContractDetailsWindow, Contract>(contract);
            }
        }

        #endregion


        #region Local Event Handlers

        /// <summary>
        /// Exports item info to CSV format.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exportToCSVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListViewExporter.CreateCSV(lvContracts);
        }

        /// <summary>
        /// On double click shows the contract details window.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void lvContracts_DoubleClick(object sender, EventArgs e)
        {
            ShowContractDetails();
        }

        /// <summary>
        /// On column reorder we update the settings.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvContracts_ColumnReordered(object sender, ColumnReorderedEventArgs e)
        {
            m_columnsChanged = true;
        }

        /// <summary>
        /// When the user manually resizes a column, we make sure to update the column preferences.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvContracts_ColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
        {
            if (m_isUpdatingColumns || m_columns.Count <= e.ColumnIndex)
                return;

            if (m_columns[e.ColumnIndex].Width == lvContracts.Columns[e.ColumnIndex].Width)
                return;

            m_columns[e.ColumnIndex].Width = lvContracts.Columns[e.ColumnIndex].Width;
            m_columnsChanged = true;
        }

        /// <summary>
        /// When the user clicks a column header, we update the sorting.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvContracts_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ContractColumn column = (ContractColumn)lvContracts.Columns[e.Column].Tag;
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

            lvContracts.Cursor = Cursors.Default;
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

            lvContracts.Cursor = CustomCursors.ContextMenu;

            ListViewItem item = lvContracts.GetItemAt(e.Location.X, e.Location.Y);
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
        /// Upon selected shows the contract details window.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void showDetailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowContractDetails();
        }

        /// <summary>
        /// Shows the context menu only when a contract is selected.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
        private void contextMenu_Opening(object sender, CancelEventArgs e)
        {
            bool visible = lvContracts.SelectedItems.Count != 0;

            showDetailsToolStripMenuItem.Visible =
                showDetailsMenuSeparator.Visible =
                    showInBrowserMenuItem.Visible =
                        showInBrowserMenuSeparator.Visible = visible;

            if (!visible)
                return;

            Contract contract = lvContracts.SelectedItems[0]?.Tag as Contract;

            if (contract?.ContractItems == null)
                return;

            visible = contract.ContractItems.Count() == 1;

            showInBrowserMenuItem.Visible =
                showInBrowserMenuSeparator.Visible = visible;

            if (!visible)
                return;

            Item contractItem = contract.ContractItems.FirstOrDefault()?.Item;

            if (contractItem == null)
                return;

            Blueprint blueprint = StaticBlueprints.GetBlueprintByID(contractItem.ID);
            Ship ship = contractItem as Ship;
            Skill skill = Character.Skills[contractItem.ID];

            if (skill == Skill.UnknownSkill)
                skill = null;

            string text = ship != null ? "Ship" : blueprint != null ? "Blueprint" : skill != null ? "Skill" : "Item";

            showInBrowserMenuItem.Text = $"Show In {text} Browser...";
        }

        /// <summary>
        /// Handles the Click event of the showInBrowserMenuItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void showInBrowserMenuItem_Click(object sender, EventArgs e)
        {
            Contract contract = lvContracts.SelectedItems[0]?.Tag as Contract;

            Item contractItem = contract?.ContractItems?.FirstOrDefault()?.Item;

            if (contractItem == null)
                return;

            Ship ship = contractItem as Ship;
            Blueprint blueprint = StaticBlueprints.GetBlueprintByID(contractItem.ID);
            Skill skill = Character.Skills[contractItem.ID];

            if (skill == Skill.UnknownSkill)
                skill = null;

            PlanWindow planWindow = PlanWindow.ShowPlanWindow(Character);

            if (ship != null)
                planWindow.ShowShipInBrowser(ship);
            else if (blueprint != null)
                planWindow.ShowBlueprintInBrowser(blueprint);
            else if (skill != null)
                planWindow.ShowSkillInBrowser(skill);
            else
                planWindow.ShowItemInBrowser(contractItem);
        }

        # endregion


        #region Global Events

        /// <summary>
        /// On timer tick, we update the column settings if any changes have been made to them.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EveMonClient_TimerTick(object sender, EventArgs e)
        {
            if (!Visible || !m_columnsChanged)
                return;

            Settings.UI.MainWindow.Contracts.Columns.Clear();
            Settings.UI.MainWindow.Contracts.Columns.AddRange(Columns.Cast<ContractColumnSettings>());

            // Recreate the columns
            Columns = Settings.UI.MainWindow.Contracts.Columns;
            m_columnsChanged = false;
        }

        /// <summary>
        /// When the contracts change, update the list.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EVEMon.Common.CustomEventArgs.ContractsEventArgs"/> instance containing the event data.</param>
        private void EveMonClient_ContractsUpdated(object sender, CharacterChangedEventArgs e)
        {
            if (Character == null || e.Character != Character)
                return;

            Contracts = Character.Contracts;
            UpdateColumns();
        }

        /// <summary>
        /// When the character contract items are downloaded, update the list.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EVEMon.Common.CustomEventArgs.CharacterChangedEventArgs"/> instance containing the event data.</param>
        private void EveMonClient_ContractItemsDownloaded(object sender, CharacterChangedEventArgs e)
        {
            if (Character == null || e.Character != Character)
                return;

            UpdateContent();
        }
        
        /// <summary>
        /// When the EveIDToName list updates, update the list.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EVEMon.Common.CustomEventArgs.CharacterChangedEventArgs"/> instance containing the event data.</param>
        private void EveMonClient_EveIDToNameUpdated(object sender, EventArgs e)
        {
            UpdateColumns();
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

            foreach (Contract contract in m_list)
            {
                contract.UpdateStation();
            }

            UpdateColumns();
        }

        # endregion


        #region Helper Classes

        private class ListViewItemFormat
        {
            /// <summary>
            /// Gets or sets the color of the text.
            /// </summary>
            /// <value>The color of the text.</value>
            public Color TextColor { get; set; }

            /// <summary>
            /// Gets or sets the text.
            /// </summary>
            /// <value>The text.</value>
            public string Text { get; set; }
        }

        #endregion

    }
}
