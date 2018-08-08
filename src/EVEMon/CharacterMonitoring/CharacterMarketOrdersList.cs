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
    /// <summary>
    /// Displays a list of market orders.
    /// </summary>
    internal sealed partial class CharacterMarketOrdersList : UserControl, IListView
    {
        #region Fields

        private readonly List<MarketOrderColumnSettings> m_columns = new List<MarketOrderColumnSettings>();
        private readonly List<MarketOrder> m_list = new List<MarketOrder>();

        private InfiniteDisplayToolTip m_tooltip;
        private MarketOrderGrouping m_grouping;
        private MarketOrderColumn m_sortCriteria;
        private IssuedFor m_showIssuedFor;

        private string m_textFilter = String.Empty;
        private bool m_sortAscending = true;

        private bool m_isUpdatingColumns;
        private bool m_columnsChanged;
        private bool m_init;

        // Panel info variables
        private int m_skillBasedOrders;

        private float m_baseBrokerFee,
                      m_transactionTax;

        private int m_askRange, m_bidRange, m_modificationRange, m_remoteBidRange;

        private int m_activeOrdersIssuedForCharacter,
                    m_activeOrdersIssuedForCorporation;

        private int m_activeSellOrdersIssuedForCharacterCount,
                    m_activeSellOrdersIssuedForCorporationCount;

        private int m_activeBuyOrdersIssuedForCharacterCount,
                    m_activeBuyOrdersIssuedForCorporationCount;

        private decimal m_sellOrdersIssuedForCharacterTotal,
                        m_sellOrdersIssuedForCorporationTotal;

        private decimal m_buyOrdersIssuedForCharacterTotal,
                        m_buyOrdersIssuedForCorporationTotal;

        private decimal m_issuedForCharacterTotalEscrow,
                        m_issuedForCorporationTotalEscrow;

        private decimal m_issuedForCharacterEscrowAdditionalToCover,
                        m_issuedForCorporationEscrowAdditionalToCover;

        #endregion


        # region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        public CharacterMarketOrdersList()
        {
            InitializeComponent();
            InitializeExpandablePanelControls();

            lvOrders.Hide();
            lvOrders.AllowColumnReorder = true;
            lvOrders.Columns.Clear();

            m_showIssuedFor = IssuedFor.All;

            noOrdersLabel.Font = FontFactory.GetFont("Tahoma", 11.25F, FontStyle.Bold);
            marketExpPanelControl.Font = FontFactory.GetFont("Tahoma", 8.25f);
            marketExpPanelControl.Hide();

            ListViewHelper.EnableDoubleBuffer(lvOrders);

            lvOrders.ColumnClick += listView_ColumnClick;
            lvOrders.ColumnWidthChanged += listView_ColumnWidthChanged;
            lvOrders.ColumnReordered += listView_ColumnReordered;
            lvOrders.MouseDown += listView_MouseDown;
            lvOrders.MouseMove += listView_MouseMove;
            lvOrders.MouseLeave += listView_MouseLeave;
        }

        #endregion


        #region Properties

        /// <summary>
        /// Gets the character associated with this monitor.
        /// </summary>
        internal CCPCharacter Character { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="lvOrders"/> is visible.
        /// </summary>
        /// <value><c>true</c> if visible; otherwise, <c>false</c>.</value>
        internal bool Visibility
        {
            get { return lvOrders.Visible; }
            set { lvOrders.Visible = value; }
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
                m_grouping = (MarketOrderGrouping)value;
                if (m_init)
                    UpdateColumns();
            }
        }

        /// <summary>
        /// Gets or sets which "Issued for" orders to display.
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
        /// Gets true when character has active issued order for corporation.
        /// </summary>
        private bool HasActiveCorporationIssuedOrders
            => m_list.Any(x => (x.State == OrderState.Active || x.State == OrderState.Modified)
                               && x.IssuedFor == IssuedFor.Corporation);

        /// <summary>
        /// Gets or sets the enumeration of orders to display.
        /// </summary>
        internal IEnumerable<MarketOrder> Orders
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
                List<MarketOrderColumnSettings> newColumns = new List<MarketOrderColumnSettings>();
                foreach (ColumnHeader header in lvOrders.Columns.Cast<ColumnHeader>().OrderBy(x => x.DisplayIndex))
                {
                    MarketOrderColumnSettings columnSetting = m_columns.First(x => x.Column == (MarketOrderColumn)header.Tag);
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
                    m_columns.AddRange(value.Cast<MarketOrderColumnSettings>());

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

            m_tooltip = new InfiniteDisplayToolTip(lvOrders);

            EveMonClient.TimerTick += EveMonClient_TimerTick;
            EveMonClient.MarketOrdersUpdated += EveMonClient_MarketOrdersUpdated;
            EveMonClient.ConquerableStationListUpdated += EveMonClient_ConquerableStationListUpdated;
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
            EveMonClient.MarketOrdersUpdated -= EveMonClient_MarketOrdersUpdated;
            EveMonClient.ConquerableStationListUpdated -= EveMonClient_ConquerableStationListUpdated;
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

            lvOrders.Visible = false;
            marketExpPanelControl.Visible = false;

            Orders = Character?.MarketOrders;
            Columns = Settings.UI.MainWindow.MarketOrders.Columns;
            Grouping = Character?.UISettings.OrdersGroupBy;
            TextFilter = String.Empty;

            UpdateColumns();

            m_init = true;

            UpdateListVisibility();
        }

        # endregion


        #region Updates Main Market Window

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

            lvOrders.BeginUpdate();
            m_isUpdatingColumns = true;

            try
            {
                lvOrders.Columns.Clear();
                lvOrders.Groups.Clear();
                lvOrders.Items.Clear();

                foreach (MarketOrderColumnSettings column in m_columns.Where(x => x.Visible))
                {
                    ColumnHeader header = lvOrders.Columns.Add(column.Column.GetHeader(), column.Width);
                    header.Tag = column.Column;

                    switch (column.Column)
                    {
                        case MarketOrderColumn.Issued:
                        case MarketOrderColumn.LastStateChange:
                        case MarketOrderColumn.InitialVolume:
                        case MarketOrderColumn.RemainingVolume:
                        case MarketOrderColumn.TotalPrice:
                        case MarketOrderColumn.Escrow:
                        case MarketOrderColumn.Duration:
                        case MarketOrderColumn.UnitaryPrice:
                            header.TextAlign = HorizontalAlignment.Right;
                            break;
                        case MarketOrderColumn.Volume:
                            header.TextAlign = HorizontalAlignment.Center;
                            break;
                    }
                }

                // We update the content
                UpdateContent();
            }
            finally
            {
                lvOrders.EndUpdate();
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

            int scrollBarPosition = lvOrders.GetVerticalScrollBarPosition();

            // Store the selected item (if any) to restore it after the update
            int selectedItem = lvOrders.SelectedItems.Count > 0
                ? lvOrders.SelectedItems[0].Tag.GetHashCode()
                : 0;

            lvOrders.BeginUpdate();
            try
            {
                IEnumerable<MarketOrder> orders = m_list
                    .Where(x => x.Item != null && x.Station != null).Where(x => IsTextMatching(x, m_textFilter));

                if (Character != null && Settings.UI.MainWindow.MarketOrders.HideInactiveOrders)
                    orders = orders.Where(x => x.IsAvailable);

                if (m_showIssuedFor != IssuedFor.All)
                    orders = orders.Where(x => x.IssuedFor == m_showIssuedFor);

                UpdateSort();

                UpdateContentByGroup(orders);

                // Restore the selected item (if any)
                if (selectedItem > 0)
                {
                    foreach (ListViewItem lvItem in lvOrders.Items.Cast<ListViewItem>().Where(
                        lvItem => lvItem.Tag.GetHashCode() == selectedItem))
                    {
                        lvItem.Selected = true;
                    }
                }

                // Adjust the size of the columns
                AdjustColumns();

                // Update the expandable panel info
                UpdateExpPanelContent();

                UpdateListVisibility();
            }
            finally
            {
                lvOrders.EndUpdate();
                lvOrders.SetVerticalScrollBarPosition(scrollBarPosition);
            }
        }

        /// <summary>
        /// Updates the list visibility.
        /// </summary>
        private void UpdateListVisibility()
        {
            // Display or hide the "no orders" label
            if (!m_init)
                return;

            noOrdersLabel.Visible = lvOrders.Items.Count == 0;
            marketExpPanelControl.Header.Visible = true;
            marketExpPanelControl.Visible = true;
            lvOrders.Visible = !noOrdersLabel.Visible;
        }

        /// <summary>
        /// Updates the content by group.
        /// </summary>
        /// <param name="orders">The orders.</param>
        private void UpdateContentByGroup(IEnumerable<MarketOrder> orders)
        {
            switch (m_grouping)
            {
                case MarketOrderGrouping.State:
                    IOrderedEnumerable<IGrouping<OrderState, MarketOrder>> groups0 =
                        orders.GroupBy(x => x.State).OrderBy(x => (int)x.Key);
                    UpdateContent(groups0);
                    break;
                case MarketOrderGrouping.StateDesc:
                    IOrderedEnumerable<IGrouping<OrderState, MarketOrder>> groups1 =
                        orders.GroupBy(x => x.State).OrderByDescending(x => (int)x.Key);
                    UpdateContent(groups1);
                    break;
                case MarketOrderGrouping.Issued:
                    IOrderedEnumerable<IGrouping<DateTime, MarketOrder>> groups2 =
                        orders.GroupBy(x => x.Issued.ToLocalTime().Date).OrderBy(x => x.Key);
                    UpdateContent(groups2);
                    break;
                case MarketOrderGrouping.IssuedDesc:
                    IOrderedEnumerable<IGrouping<DateTime, MarketOrder>> groups3 =
                        orders.GroupBy(x => x.Issued.ToLocalTime().Date).OrderByDescending(x => x.Key);
                    UpdateContent(groups3);
                    break;
                case MarketOrderGrouping.ItemType:
                    IOrderedEnumerable<IGrouping<MarketGroup, MarketOrder>> groups4 =
                        orders.GroupBy(x => x.Item.MarketGroup).OrderBy(x => x.Key.Name);
                    UpdateContent(groups4);
                    break;
                case MarketOrderGrouping.ItemTypeDesc:
                    IOrderedEnumerable<IGrouping<MarketGroup, MarketOrder>> groups5 =
                        orders.GroupBy(x => x.Item.MarketGroup).OrderByDescending(x => x.Key.Name);
                    UpdateContent(groups5);
                    break;
                case MarketOrderGrouping.Location:
                    IOrderedEnumerable<IGrouping<Station, MarketOrder>> groups6 =
                        orders.GroupBy(x => x.Station).OrderBy(x => x.Key.Name);
                    UpdateContent(groups6);
                    break;
                case MarketOrderGrouping.LocationDesc:
                    IOrderedEnumerable<IGrouping<Station, MarketOrder>> groups7 =
                        orders.GroupBy(x => x.Station).OrderByDescending(x => x.Key.Name);
                    UpdateContent(groups7);
                    break;
                case MarketOrderGrouping.OrderType:
                    IOrderedEnumerable<IGrouping<string, MarketOrder>> groups8 =
                        orders.GroupBy(x => x is BuyOrder ? "Buying Orders" : "Selling Orders").OrderBy(x => x.Key);
                    UpdateContent(groups8);
                    break;
                case MarketOrderGrouping.OrderTypeDesc:
                    IOrderedEnumerable<IGrouping<string, MarketOrder>> groups9 =
                        orders.GroupBy(x => x is BuyOrder ? "Buying Orders" : "Selling Orders").OrderByDescending(x => x.Key);
                    UpdateContent(groups9);
                    break;
            }
        }

        /// <summary>
        /// Updates the content of the listview.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="groups"></param>
        private void UpdateContent<TKey>(IEnumerable<IGrouping<TKey, MarketOrder>> groups)
        {
            lvOrders.Items.Clear();
            lvOrders.Groups.Clear();

            // Add the groups
            foreach (IGrouping<TKey, MarketOrder> group in groups)
            {
                string groupText;
                if (group.Key is OrderState)
                    groupText = ((OrderState)(Object)group.Key).GetHeader();
                else if (group.Key is DateTime)
                    groupText = ((DateTime)(Object)group.Key).ToShortDateString();
                else
                    groupText = group.Key.ToString();

                ListViewGroup listGroup = new ListViewGroup(groupText);
                lvOrders.Groups.Add(listGroup);

                // Add the items in every group
                lvOrders.Items.AddRange(
                    group.Select(order => new
                    {
                        order,
                        item = new ListViewItem(order.Item.Name, listGroup)
                        {
                            UseItemStyleForSubItems = false,
                            Tag = order
                        }

                    }).Select(x => CreateSubItems(x.order, x.item)).ToArray());
            }
        }

        /// <summary>
        /// Creates the list view sub items.
        /// </summary>
        /// <param name="order">The order.</param>
        /// <param name="item">The item.</param>
        private ListViewItem CreateSubItems(MarketOrder order, ListViewItem item)
        {
            // Display text as dimmed if the order is no longer available
            if (!order.IsAvailable)
                item.ForeColor = SystemColors.GrayText;

            // Display text highlighted if the order is modified
            if (order.State == OrderState.Modified)
                item.ForeColor = SystemColors.HotTrack;

            // Add enough subitems to match the number of columns
            while (item.SubItems.Count < lvOrders.Columns.Count + 1)
            {
                item.SubItems.Add(String.Empty);
            }

            // Creates the subitems
            for (int i = 0; i < lvOrders.Columns.Count; i++)
            {
                SetColumn(order, item.SubItems[i], (MarketOrderColumn)lvOrders.Columns[i].Tag);
            }

            // Tooltip
            StringBuilder builder = new StringBuilder();
            builder
                .Append($"Issued For: {order.IssuedFor}")
                .AppendLine()
                .Append($"Issued: {order.Issued.ToLocalTime()}")
                .AppendLine()
                .Append($"Duration: {order.Duration} Day{(order.Duration.S())}")
                .AppendLine()
                .Append($"Solar System: {order.Station.SolarSystem?.FullLocation ?? EveMonConstants.UnknownText}")
                .AppendLine()
                .Append($"Station: {order.Station.Name}")
                .AppendLine();

            item.ToolTipText = builder.ToString();

            return item;
        }

        /// <summary>
        /// Adjusts the columns.
        /// </summary>
        private void AdjustColumns()
        {
            foreach (ColumnHeader column in lvOrders.Columns)
            {
                if (m_columns[column.Index].Width == -1)
                    m_columns[column.Index].Width = -2;

                column.Width = m_columns[column.Index].Width;

                // Due to .NET design we need to prevent the last colummn to resize to the right end

                // Return if it's not the last column and not set to auto-resize
                if (column.Index != lvOrders.Columns.Count - 1 || m_columns[column.Index].Width != -2)
                    continue;

                const int Pad = 4;

                // Calculate column header text width with padding
                int columnHeaderWidth = TextRenderer.MeasureText(column.Text, Font).Width + Pad * 2;

                // If there is an image assigned to the header, add its width with padding
                if (lvOrders.SmallImageList != null && column.ImageIndex > -1)
                    columnHeaderWidth += lvOrders.SmallImageList.ImageSize.Width + Pad;

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
            lvOrders.ListViewItemSorter = new ListViewItemComparerByTag<MarketOrder>(
                new MarketOrderComparer(m_sortCriteria, m_sortAscending));

            UpdateSortVisualFeedback();
        }

        /// <summary>
        /// Updates the sort feedback (the arrow on the header).
        /// </summary>
        private void UpdateSortVisualFeedback()
        {
            foreach (ColumnHeader columnHeader in lvOrders.Columns.Cast<ColumnHeader>())
            {
                MarketOrderColumn column = (MarketOrderColumn)columnHeader.Tag;
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
            return FormatHelper.FormatIf(Settings.UI.MainWindow.MarketOrders.NumberAbsFormat,
                2, price, AbbreviationFormat.AbbreviationSymbols);
        }

        /// <summary>
        /// Formats the quantity according to the settings.
        /// </summary>
        /// <param name="price">The quantity to display.</param>
        /// <returns>The quantity as a string.</returns>
        private static string FormatQuantity(long qty)
        {
            return FormatHelper.FormatIf(Settings.UI.MainWindow.MarketOrders.NumberAbsFormat,
                qty, AbbreviationFormat.AbbreviationSymbols);
        }

        /// <summary>
        /// Updates the listview sub-item.
        /// </summary>
        /// <param name="order"></param>
        /// <param name="item"></param>
        /// <param name="column"></param>
        private static void SetColumn(MarketOrder order, ListViewItem.ListViewSubItem item, MarketOrderColumn column)
        {
            BuyOrder buyOrder = order as BuyOrder;

            switch (column)
            {
            case MarketOrderColumn.Duration:
                item.Text = $"{order.Duration} Day{(order.Duration.S())}";
                break;
            case MarketOrderColumn.Expiration:
                ListViewItemFormat format = FormatExpiration(order);
                item.Text = format.Text;
                item.ForeColor = format.TextColor;
                break;
            case MarketOrderColumn.InitialVolume:
                item.Text = FormatQuantity(order.InitialVolume);
                break;
            case MarketOrderColumn.Issued:
                item.Text = order.Issued.ToLocalTime().ToShortDateString();
                break;
            case MarketOrderColumn.IssuedFor:
                item.Text = order.IssuedFor.ToString();
                break;
            case MarketOrderColumn.Item:
                item.Text = order.Item.ToString();
                break;
            case MarketOrderColumn.ItemType:
                item.Text = order.Item.MarketGroup.Name;
                break;
            case MarketOrderColumn.Location:
                item.Text = order.Station.FullLocation;
                break;
            case MarketOrderColumn.MinimumVolume:
                item.Text = FormatQuantity(order.MinVolume);
                break;
            case MarketOrderColumn.Region:
                item.Text = order.Station.SolarSystemChecked.Constellation.Region.Name;
                break;
            case MarketOrderColumn.RemainingVolume:
                item.Text = FormatQuantity(order.RemainingVolume);
                break;
            case MarketOrderColumn.SolarSystem:
                item.Text = order.Station.SolarSystem?.Name ?? EveMonConstants.UnknownText;
                item.ForeColor = order.Station.SolarSystemChecked.SecurityLevelColor;
                break;
            case MarketOrderColumn.Station:
                item.Text = order.Station.Name;
                break;
            case MarketOrderColumn.TotalPrice:
                item.Text = FormatPrice(order.TotalPrice);
                item.ForeColor = buyOrder != null ? Color.DarkRed : Color.DarkGreen;
                break;
            case MarketOrderColumn.UnitaryPrice:
                item.Text = FormatPrice(order.UnitaryPrice);
                item.ForeColor = buyOrder != null ? Color.DarkRed : Color.DarkGreen;
                break;
            case MarketOrderColumn.Volume:
                string remainingVolumeText = FormatQuantity(order.RemainingVolume);
                string initialVolumeText = FormatQuantity(order.InitialVolume);
                item.Text = $"{remainingVolumeText} / {initialVolumeText}";
                break;
            case MarketOrderColumn.LastStateChange:
                item.Text = order.LastStateChange.ToLocalTime().ToShortDateString();
                break;
            case MarketOrderColumn.OrderRange:
                if (buyOrder != null)
                    item.Text = buyOrder.RangeDescription;
                break;
            case MarketOrderColumn.Escrow:
                if (buyOrder != null)
                {
                    item.Text = FormatPrice(buyOrder.Escrow);
                    item.ForeColor = Color.DarkBlue;
                }
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
        /// <param name="x"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        private static bool IsTextMatching(MarketOrder x, string text)
            => String.IsNullOrEmpty(text) ||
               x.Item.Name.Contains(text, ignoreCase: true) ||
               x.Item.Description.Contains(text, ignoreCase: true) ||
               x.Station.Name.Contains(text, ignoreCase: true) ||
               x.Station.SolarSystemChecked.Name.Contains(text, ignoreCase: true) ||
               x.Station.SolarSystemChecked.Constellation.Name.Contains(text, ignoreCase: true) ||
               x.Station.SolarSystemChecked.Constellation.Region.Name.Contains(text, ignoreCase: true);

        /// <summary>
        /// Gets the text and formatting for the expiration cell
        /// </summary>
        /// <param name="order">Order to generate a format for</param>
        /// <returns>ListViewItemFormat object describing the format of the cell</returns>
        private static ListViewItemFormat FormatExpiration(MarketOrder order)
        {
            // Initialize to sensible defaults
            ListViewItemFormat format = new ListViewItemFormat
            {
                TextColor = Color.Black,
                Text = order.Expiration.ToRemainingTimeShortDescription(DateTimeKind.Utc)
                    .ToUpper(CultureConstants.DefaultCulture)
            };

            // Order is expiring soon
            if (order.IsAvailable && order.Expiration < DateTime.UtcNow.AddDays(1))
                format.TextColor = Color.DarkOrange;

            // We have all the information for formatting an available order
            if (order.IsAvailable)
                return format;

            // Order isn't available so lets format it as such
            format.Text = order.State.ToString();

            if (order.State == OrderState.Expired)
                format.TextColor = Color.Red;

            if (order.State == OrderState.Fulfilled)
                format.TextColor = Color.DarkGreen;

            return format;
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
            ListViewExporter.CreateCSV(lvOrders);
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

            if (m_columns[e.ColumnIndex].Width == lvOrders.Columns[e.ColumnIndex].Width)
                return;

            m_columns[e.ColumnIndex].Width = lvOrders.Columns[e.ColumnIndex].Width;
            m_columnsChanged = true;
        }

        /// <summary>
        /// When the user clicks a column header, we update the sorting.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            MarketOrderColumn column = (MarketOrderColumn)lvOrders.Columns[e.Column].Tag;
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

            lvOrders.Cursor = Cursors.Default;
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

            lvOrders.Cursor = CustomCursors.ContextMenu;

            ListViewItem item = lvOrders.GetItemAt(e.Location.X, e.Location.Y);
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
            bool visible = lvOrders.SelectedItems.Count != 0;

            showInBrowserMenuItem.Visible =
                showInBrowserMenuSeparator.Visible = visible;

            if (!visible)
                return;

            MarketOrder order = lvOrders.SelectedItems[0]?.Tag as MarketOrder;

            if (order?.Item == null)
                return;

            Blueprint blueprint = StaticBlueprints.GetBlueprintByID(order.Item.ID);
            Ship ship = order.Item as Ship;
            Skill skill = Character.Skills[order.Item.ID];

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
            MarketOrder order = lvOrders.SelectedItems[0]?.Tag as MarketOrder;

            if (order?.Item == null)
                return;

            Ship ship = order.Item as Ship;
            Blueprint blueprint = StaticBlueprints.GetBlueprintByID(order.Item.ID);
            Skill skill = Character.Skills[order.Item.ID];

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
                planWindow.ShowItemInBrowser(order.Item);
        }

        # endregion


        #region Global Event Handlers

        /// <summary>
        /// On timer tick, we update the column settings if any changes have been made to them.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EveMonClient_TimerTick(object sender, EventArgs e)
        {
            if (!Visible || !m_columnsChanged)
                return;

            Settings.UI.MainWindow.MarketOrders.Columns.Clear();
            Settings.UI.MainWindow.MarketOrders.Columns.AddRange(Columns.Cast<MarketOrderColumnSettings>());

            // Recreate the columns
            Columns = Settings.UI.MainWindow.MarketOrders.Columns;
            m_columnsChanged = false;
        }

        /// <summary>
        /// When the market orders are updated,
        /// update the list and the expandable panel info.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EveMonClient_MarketOrdersUpdated(object sender, CharacterChangedEventArgs e)
        {
            if (Character == null || e.Character != Character)
                return;

            Orders = Character.MarketOrders;
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

            foreach (MarketOrder order in m_list)
            {
                order.UpdateStation();
            }

            UpdateColumns();
        }

        # endregion


        #region Updates Expandable Panel On Global Events

        /// <summary>
        /// Updates the content of the expandable panel.
        /// </summary>
        private void UpdateExpPanelContent()
        {
            if (Character == null)
            {
                marketExpPanelControl.Visible = false;
                return;
            }

            // Calculate the related info for the panel
            CalculatePanelInfo();

            // Update the Header text of the panel
            UpdateHeaderText();

            // Update the info in the panel
            UpdatePanelInfo();

            // Force to redraw
            marketExpPanelControl.Refresh();
        }

        /// <summary>
        /// Updates the header text of the panel.
        /// </summary>
        private void UpdateHeaderText()
        {
            const int BaseOrders = 5;
            Int64 maxOrders = BaseOrders + m_skillBasedOrders;
            int activeOrders = m_activeOrdersIssuedForCharacter + m_activeOrdersIssuedForCorporation;
            Int64 remainingOrders = maxOrders - activeOrders;
            decimal activeSellOrdersTotal = m_sellOrdersIssuedForCharacterTotal + m_sellOrdersIssuedForCorporationTotal;
            decimal activeBuyOrdersTotal = m_buyOrdersIssuedForCharacterTotal + m_buyOrdersIssuedForCorporationTotal;

            string ordersRemainingText = $"Orders Remaining: {remainingOrders} out of {maxOrders} max";
            string activeSellOrdersTotalText = $"Sell Orders Total: {activeSellOrdersTotal:N} ISK";
            string activeBuyOrdersTotalText = $"Buy Orders Total: {activeBuyOrdersTotal:N} ISK";
            marketExpPanelControl.HeaderText =
                $"{ordersRemainingText}{String.Empty,5}{activeSellOrdersTotalText}{String.Empty,5}{activeBuyOrdersTotalText}";
        }

        /// <summary>
        /// Updates the labels text in the panel.
        /// </summary>
        private void UpdatePanelInfo()
        {
            // Update the basic label text
            m_lblTotalEscrow.Text =
                $"Total in Escrow: {m_issuedForCharacterTotalEscrow + m_issuedForCorporationTotalEscrow:N} ISK " +
                $"(additional {m_issuedForCharacterEscrowAdditionalToCover + m_issuedForCorporationEscrowAdditionalToCover:N} ISK to cover)";
            m_lblBaseBrokerFee.Text = $"Base Broker Fee: {m_baseBrokerFee:P2} of order value";
            m_lblTransactionTax.Text = $"Transaction Tax: {m_transactionTax:P2} of sales value";
            m_lblActiveSellOrdersCount.Text =
                $"Active Sell Orders: {m_activeSellOrdersIssuedForCharacterCount + m_activeSellOrdersIssuedForCorporationCount}";
            m_lblActiveBuyOrdersCount.Text =
                $"Active Buy Orders: {m_activeBuyOrdersIssuedForCharacterCount + m_activeBuyOrdersIssuedForCorporationCount}";
            m_lblAskRange.Text = $"Ask Range: limited to {StaticGeography.GetRange(m_askRange)}";
            m_lblBidRange.Text = $"Bid Range: limited to {StaticGeography.GetRange(m_bidRange)}";
            m_lblModificationRange.Text = $"Modification Range: limited to {StaticGeography.GetRange(m_modificationRange)}";
            m_lblRemoteBidRange.Text = Character.Skills[DBConstants.MarketingSkillID].LastConfirmedLvl > 0
                ? $"Remote Bid Range: limited to {StaticGeography.GetRange(m_remoteBidRange)}"
                : String.Empty;

            // Supplemental label text
            if (HasActiveCorporationIssuedOrders)
            {
                m_lblCharTotalEscrow.Text = $"Character Issued: {m_issuedForCharacterTotalEscrow:N} ISK " +
                                            $"(additional {m_issuedForCharacterEscrowAdditionalToCover:N} ISK to cover)";
                m_lblCorpTotalEscrow.Text = $"Corporation Issued: {m_issuedForCorporationTotalEscrow:N} ISK " +
                                            $"(additional {m_issuedForCorporationEscrowAdditionalToCover:N} ISK to cover)";
                m_lblActiveCharSellOrdersCount.Text = $"Character Issued: {m_activeSellOrdersIssuedForCharacterCount}";
                m_lblActiveCorpSellOrdersCount.Text = $"Corporation Issued: {m_activeSellOrdersIssuedForCorporationCount}";
                m_lblActiveCharBuyOrdersCount.Text = $"Character Issued: {m_activeBuyOrdersIssuedForCharacterCount}";
                m_lblActiveCorpBuyOrdersCount.Text = $"Corporation Issued: {m_activeBuyOrdersIssuedForCorporationCount}";
                m_lblActiveCharSellOrdersTotal.Text = $"Total: {m_sellOrdersIssuedForCharacterTotal:N} ISK";
                m_lblActiveCorpSellOrdersTotal.Text = $"Total: {m_sellOrdersIssuedForCorporationTotal:N} ISK";
                m_lblActiveCharBuyOrdersTotal.Text = $"Total: {m_buyOrdersIssuedForCharacterTotal:N} ISK";
                m_lblActiveCorpBuyOrdersTotal.Text = $"Total: {m_buyOrdersIssuedForCorporationTotal:N} ISK";
            }

            // Update label position
            UpdatePanelControlPosition();
        }

        /// <summary>
        /// Updates expandable panel controls positions.
        /// </summary>
        private void UpdatePanelControlPosition()
        {
            marketExpPanelControl.SuspendLayout();

            const int Pad = 5;
            int height = marketExpPanelControl.ExpandDirection == Direction.Up ? Pad : marketExpPanelControl.HeaderHeight;

            m_lblTotalEscrow.Location = new Point(5, height);
            height += m_lblTotalEscrow.Height;
            if (HasActiveCorporationIssuedOrders)
            {
                m_lblCharTotalEscrow.Location = new Point(15, height);
                m_lblCharTotalEscrow.Visible = true;
                height += m_lblCharTotalEscrow.Height;

                m_lblCorpTotalEscrow.Location = new Point(15, height);
                m_lblCorpTotalEscrow.Visible = true;
                height += m_lblCorpTotalEscrow.Height;
            }
            else
            {
                m_lblCharTotalEscrow.Visible = false;
                m_lblCorpTotalEscrow.Visible = false;
            }

            height += Pad;

            m_lblBaseBrokerFee.Location = new Point(5, height);
            m_lblAskRange.Location = new Point(m_lblAskRange.Location.X, height);
            height += m_lblBaseBrokerFee.Height;

            m_lblTransactionTax.Location = new Point(5, height);
            m_lblBidRange.Location = new Point(m_lblBidRange.Location.X, height);
            height += m_lblTransactionTax.Height;

            m_lblModificationRange.Location = new Point(m_lblModificationRange.Location.X, height);
            height += m_lblModificationRange.Height;

            m_lblActiveSellOrdersCount.Location = new Point(5, height);
            m_lblRemoteBidRange.Location = new Point(m_lblRemoteBidRange.Location.X, height);
            height += m_lblActiveSellOrdersCount.Height;

            if (HasActiveCorporationIssuedOrders)
            {
                m_lblActiveCharSellOrdersCount.Location = new Point(15, height);
                m_lblActiveCharSellOrdersTotal.Location = new Point(150, height);
                m_lblActiveCharSellOrdersCount.Visible = true;
                m_lblActiveCharSellOrdersTotal.Visible = true;
                height += m_lblCharTotalEscrow.Height;

                m_lblActiveCorpSellOrdersCount.Location = new Point(15, height);
                m_lblActiveCorpSellOrdersTotal.Location = new Point(150, height);
                m_lblActiveCorpSellOrdersCount.Visible = true;
                m_lblActiveCorpSellOrdersTotal.Visible = true;
                height += m_lblCorpTotalEscrow.Height + Pad;
            }
            else
            {
                m_lblActiveCharSellOrdersCount.Visible = false;
                m_lblActiveCharSellOrdersTotal.Visible = false;
                m_lblActiveCorpSellOrdersCount.Visible = false;
                m_lblActiveCorpSellOrdersTotal.Visible = false;
            }

            m_lblActiveBuyOrdersCount.Location = new Point(5, height);
            height += m_lblActiveBuyOrdersCount.Height;

            if (HasActiveCorporationIssuedOrders)
            {
                m_lblActiveCharBuyOrdersCount.Location = new Point(15, height);
                m_lblActiveCharBuyOrdersTotal.Location = new Point(150, height);
                m_lblActiveCharBuyOrdersCount.Visible = true;
                m_lblActiveCharBuyOrdersTotal.Visible = true;
                height += m_lblCharTotalEscrow.Height;

                m_lblActiveCorpBuyOrdersCount.Location = new Point(15, height);
                m_lblActiveCorpBuyOrdersTotal.Location = new Point(150, height);
                m_lblActiveCorpBuyOrdersCount.Visible = true;
                m_lblActiveCorpBuyOrdersTotal.Visible = true;
                height += m_lblCorpTotalEscrow.Height;
            }
            else
            {
                m_lblActiveCharBuyOrdersCount.Visible = false;
                m_lblActiveCharBuyOrdersTotal.Visible = false;
                m_lblActiveCorpBuyOrdersCount.Visible = false;
                m_lblActiveCorpBuyOrdersTotal.Visible = false;
            }

            height += Pad;

            // Update panel's expanded height
            marketExpPanelControl.ExpandedHeight = height + (marketExpPanelControl.ExpandDirection == Direction.Up
                                                                 ? marketExpPanelControl.HeaderHeight
                                                                 : Pad);

            marketExpPanelControl.ResumeLayout(false);
        }

        /// <summary>
        /// Calculates the market orders related info for the panel.
        /// </summary>
        private void CalculatePanelInfo()
        {
            IEnumerable<SellOrder> activeSellOrdersIssuedForCharacter = m_list.OfType<SellOrder>().Where(
                x => (x.State == OrderState.Active || x.State == OrderState.Modified) && x.IssuedFor == IssuedFor.Character);
            IEnumerable<SellOrder> activeSellOrdersIssuedForCorporation = m_list.OfType<SellOrder>().Where(
                x => (x.State == OrderState.Active || x.State == OrderState.Modified) && x.IssuedFor == IssuedFor.Corporation);
            IEnumerable<BuyOrder> activeBuyOrdersIssuedForCharacter = m_list.OfType<BuyOrder>().Where(
                x => (x.State == OrderState.Active || x.State == OrderState.Modified) && x.IssuedFor == IssuedFor.Character);
            IEnumerable<BuyOrder> activeBuyOrdersIssuedForCorporation = m_list.OfType<BuyOrder>().Where(
                x => (x.State == OrderState.Active || x.State == OrderState.Modified) && x.IssuedFor == IssuedFor.Corporation);

            // Calculate character's max orders
            m_skillBasedOrders = (int)(Character.Skills[DBConstants.TradeSkillID].LastConfirmedLvl * 4
                                 + Character.Skills[DBConstants.RetailSkillID].LastConfirmedLvl * 8
                                 + Character.Skills[DBConstants.WholesaleSkillID].LastConfirmedLvl * 16
                                 + Character.Skills[DBConstants.TycconSkillID].LastConfirmedLvl * 32);

            // Calculate character's base broker fee
            m_baseBrokerFee = EveConstants.BrokerFeeBase -
                             Character.Skills[DBConstants.BrokerRelationsSkillID].LastConfirmedLvl * 0.001f;

            // Calculate character's transaction tax
            m_transactionTax = EveConstants.TransactionTaxBase -
                               EveConstants.TransactionTaxBase *
                               (Character.Skills[DBConstants.AccountingSkillID].LastConfirmedLvl * 0.1f);

            // Calculate character's ask range
            m_askRange = (int)Character.Skills[DBConstants.MarketingSkillID].LastConfirmedLvl;

            // Calculate character's bid range
            m_bidRange = (int)Character.Skills[DBConstants.ProcurementSkillID].LastConfirmedLvl;

            // Calculate character's modification range
            m_modificationRange = (int)Character.Skills[DBConstants.DaytradingSkillID].LastConfirmedLvl;

            // Calculate character's remote bid range
            m_remoteBidRange = (int)Character.Skills[DBConstants.VisibilitySkillID].LastConfirmedLvl;

            // Calculate active sell & buy orders total price (character & corporation issued separately)
            m_sellOrdersIssuedForCharacterTotal = activeSellOrdersIssuedForCharacter.Sum(x => x.TotalPrice);
            m_sellOrdersIssuedForCorporationTotal = activeSellOrdersIssuedForCorporation.Sum(x => x.TotalPrice);
            m_buyOrdersIssuedForCharacterTotal = activeBuyOrdersIssuedForCharacter.Sum(x => x.TotalPrice);
            m_buyOrdersIssuedForCorporationTotal = activeBuyOrdersIssuedForCorporation.Sum(x => x.TotalPrice);

            // Calculate active sell & buy orders count (character & corporation issued separately)
            m_activeSellOrdersIssuedForCharacterCount = activeSellOrdersIssuedForCharacter.Count();
            m_activeSellOrdersIssuedForCorporationCount = activeSellOrdersIssuedForCorporation.Count();
            m_activeBuyOrdersIssuedForCharacterCount = activeBuyOrdersIssuedForCharacter.Count();
            m_activeBuyOrdersIssuedForCorporationCount = activeBuyOrdersIssuedForCorporation.Count();

            // Calculate active orders (character & corporation issued separately)
            m_activeOrdersIssuedForCharacter = m_activeSellOrdersIssuedForCharacterCount +
                                               m_activeBuyOrdersIssuedForCharacterCount;
            m_activeOrdersIssuedForCorporation = m_activeSellOrdersIssuedForCorporationCount +
                                                 m_activeBuyOrdersIssuedForCorporationCount;

            // Calculate total escrow (character & corporation issued separately)
            m_issuedForCharacterTotalEscrow = activeBuyOrdersIssuedForCharacter.Sum(x => x.Escrow);
            m_issuedForCorporationTotalEscrow = activeBuyOrdersIssuedForCorporation.Sum(x => x.Escrow);

            // Calculate escrow additional to cover (character & corporation issued separately)
            m_issuedForCharacterEscrowAdditionalToCover = m_buyOrdersIssuedForCharacterTotal - m_issuedForCharacterTotalEscrow;
            m_issuedForCorporationEscrowAdditionalToCover = m_buyOrdersIssuedForCorporationTotal -
                                                            m_issuedForCorporationTotalEscrow;
        }

        # endregion


        #region Initialize Expandable Panel Controls

        // Basic labels constructor
        private readonly Label m_lblTotalEscrow = new Label();
        private readonly Label m_lblBaseBrokerFee = new Label();
        private readonly Label m_lblTransactionTax = new Label();
        private readonly Label m_lblActiveSellOrdersCount = new Label();
        private readonly Label m_lblActiveBuyOrdersCount = new Label();
        private readonly Label m_lblAskRange = new Label();
        private readonly Label m_lblBidRange = new Label();
        private readonly Label m_lblModificationRange = new Label();
        private readonly Label m_lblRemoteBidRange = new Label();

        // Supplemental labels constructor
        private readonly Label m_lblCharTotalEscrow = new Label();
        private readonly Label m_lblCorpTotalEscrow = new Label();

        private readonly Label m_lblActiveCharSellOrdersTotal = new Label();
        private readonly Label m_lblActiveCorpSellOrdersTotal = new Label();
        private readonly Label m_lblActiveCharBuyOrdersTotal = new Label();
        private readonly Label m_lblActiveCorpBuyOrdersTotal = new Label();
        private readonly Label m_lblActiveCharSellOrdersCount = new Label();
        private readonly Label m_lblActiveCorpSellOrdersCount = new Label();
        private readonly Label m_lblActiveCharBuyOrdersCount = new Label();
        private readonly Label m_lblActiveCorpBuyOrdersCount = new Label();

        private void InitializeExpandablePanelControls()
        {
            marketExpPanelControl.SuspendLayout();

            // Add basic labels to panel
            marketExpPanelControl.Controls
                .AddRange(new Control[]
                {
                    m_lblTotalEscrow,
                    m_lblBaseBrokerFee,
                    m_lblTransactionTax,
                    m_lblActiveSellOrdersCount,
                    m_lblActiveBuyOrdersCount,
                    m_lblAskRange,
                    m_lblBidRange,
                    m_lblModificationRange,
                    m_lblRemoteBidRange
                });

            // Add supplemental labels to panel
            marketExpPanelControl.Controls
                .AddRange(new Control[]
                {
                    m_lblCharTotalEscrow,
                    m_lblCorpTotalEscrow,
                    m_lblActiveCharSellOrdersTotal,
                    m_lblActiveCorpSellOrdersTotal,
                    m_lblActiveCharBuyOrdersTotal,
                    m_lblActiveCorpBuyOrdersTotal,
                    m_lblActiveCharSellOrdersCount,
                    m_lblActiveCorpSellOrdersCount,
                    m_lblActiveCharBuyOrdersCount,
                    m_lblActiveCorpBuyOrdersCount
                });

            // Apply properties
            foreach (Label label in marketExpPanelControl.Controls.OfType<Label>())
            {
                label.ForeColor = SystemColors.ControlText;
                label.BackColor = Color.Transparent;
                label.AutoSize = true;
            }

            // Special properties
            m_lblAskRange.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            m_lblBidRange.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            m_lblModificationRange.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            m_lblRemoteBidRange.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            m_lblAskRange.Location = new Point(220, 0);
            m_lblBidRange.Location = new Point(220, 0);
            m_lblModificationRange.Location = new Point(220, 0);
            m_lblRemoteBidRange.Location = new Point(220, 0);

            // Subscribe events
            foreach (Label label in marketExpPanelControl.Controls.OfType<Label>())
            {
                label.MouseClick += OnExpandablePanelMouseClick;
            }

            marketExpPanelControl.ResumeLayout(false);
        }

        /// <summary>
        /// Called when the expandable panel gets mouse clicked.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
        private void OnExpandablePanelMouseClick(object sender, MouseEventArgs e)
        {
            marketExpPanelControl.OnMouseClick(sender, e);
        }

        #endregion


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
