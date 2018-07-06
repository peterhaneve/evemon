using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using Region = EVEMon.Common.Data.Region;

namespace EVEMon.CharacterMonitoring
{
    internal sealed partial class CharacterAssetsList : UserControl, IListView
    {
        #region Fields

        private readonly List<AssetColumnSettings> m_columns = new List<AssetColumnSettings>();
        private readonly List<Asset> m_list = new List<Asset>();

        private InfiniteDisplayToolTip m_tooltip;
        private AssetGrouping m_grouping;
        private AssetColumn m_sortCriteria;

        private string m_textFilter = String.Empty;
        private string m_totalCostLabelDefaultText;

        private bool m_sortAscending = true;
        private bool m_columnsChanged;
        private bool m_isUpdatingColumns;
        private bool m_init;

        #endregion


        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        public CharacterAssetsList()
        {
            InitializeComponent();

            lvAssets.Visible = false;
            lvAssets.AllowColumnReorder = true;
            lvAssets.Columns.Clear();
            estimatedCostPanel.Hide();
            noPricesFoundLabel.Hide();

            noAssetsLabel.Font = FontFactory.GetFont("Tahoma", 11.25F, FontStyle.Bold);

            ListViewHelper.EnableDoubleBuffer(lvAssets);

            lvAssets.KeyDown += listView_KeyDown;
            lvAssets.ColumnClick += listView_ColumnClick;
            lvAssets.ColumnWidthChanged += listView_ColumnWidthChanged;
            lvAssets.ColumnReordered += listView_ColumnReordered;
            lvAssets.MouseDown += listView_MouseDown;
            lvAssets.MouseMove += listView_MouseMove;
            lvAssets.MouseLeave += listView_MouseLeave;
        }

        #endregion


        #region Properties

        /// <summary>
        /// Gets the character associated with this monitor.
        /// </summary>
        internal CCPCharacter Character { get; set; }

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
                    Task.WhenAll(UpdateColumnsAsync());
            }
        }

        /// <summary>
        /// Gets or sets the enumeration of assets to display.
        /// </summary>
        private IEnumerable<Asset> Assets
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
        /// Gets or sets the grouping of a listview.
        /// </summary>
        /// <value></value>
        [Browsable(false)]
        public Enum Grouping
        {
            get { return m_grouping; }
            set
            {
                m_grouping = (AssetGrouping)value;
                if (m_init)
                    Task.WhenAll(UpdateColumnsAsync());
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
                List<AssetColumnSettings> newColumns = new List<AssetColumnSettings>();
                foreach (ColumnHeader header in lvAssets.Columns.Cast<ColumnHeader>().OrderBy(x => x.DisplayIndex))
                {
                    AssetColumnSettings columnSetting = m_columns.First(x => x.Column == (AssetColumn)header.Tag);
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
                    m_columns.AddRange(value.Cast<AssetColumnSettings>());

                if (m_init)
                    Task.WhenAll(UpdateColumnsAsync());
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

            m_totalCostLabelDefaultText = lblTotalCost.Text;

            m_tooltip = new InfiniteDisplayToolTip(lvAssets);

            EveMonClient.TimerTick += EveMonClient_TimerTick;
            EveMonClient.CharacterAssetsUpdated += EveMonClient_CharacterAssetsUpdated;
            EveMonClient.CharacterInfoUpdated += EveMonClient_CharacterInfoUpdated;
            EveMonClient.ConquerableStationListUpdated += EveMonClient_ConquerableStationListUpdated;
            EveMonClient.EveFlagsUpdated += EveMonClient_EveFlagsUpdated;
            EveMonClient.SettingsChanged += EveMonClient_SettingsChanged;
            EveMonClient.ItemPricesUpdated += EveMonClient_ItemPricesUpdated;
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
            EveMonClient.CharacterAssetsUpdated -= EveMonClient_CharacterAssetsUpdated;
            EveMonClient.CharacterInfoUpdated -= EveMonClient_CharacterInfoUpdated;
            EveMonClient.ConquerableStationListUpdated -= EveMonClient_ConquerableStationListUpdated;
            EveMonClient.EveFlagsUpdated -= EveMonClient_EveFlagsUpdated;
            EveMonClient.SettingsChanged -= EveMonClient_SettingsChanged;
            EveMonClient.ItemPricesUpdated -= EveMonClient_ItemPricesUpdated;
            Disposed -= OnDisposed;
        }

        /// <summary>
        /// When the control becomes visible again, we update the content.
        /// </summary>
        /// <param name="e"></param>
        protected override async void OnVisibleChanged(EventArgs e)
        {
            if (DesignMode || this.IsDesignModeHosted() || Character == null)
                return;

            base.OnVisibleChanged(e);

            if (!Visible)
                return;

            // Prevents the properties to call UpdateColumnsAsync() till we set all properties
            m_init = false;

            lvAssets.Hide();
            estimatedCostPanel.Hide();
            noAssetsLabel.Visible = Character?.Assets.Count == 0;
            
            Assets = Character?.Assets;
            Columns = Settings.UI.MainWindow.Assets.Columns;
            Grouping = Character?.UISettings.AssetsGroupBy;
            TextFilter = String.Empty;

            await UpdateColumnsAsync();

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

            UpdateColumnsAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Updates the columns.
        /// </summary>
        internal async Task UpdateColumnsAsync()
        {
            // Returns if not visible
            if (!Visible)
                return;

            lvAssets.BeginUpdate();
            m_isUpdatingColumns = true;

            lvAssets.Hide();
            noAssetsLabel.Hide();

            lvAssets.Columns.Clear();
            lvAssets.Groups.Clear();
            lvAssets.Items.Clear();

            try
            {
                throbber.Show();
                throbber.State = ThrobberState.Rotating;
                
                AddColumns();

                // We update the content
                await UpdateContentAsync();

                throbber.State = ThrobberState.Stopped;
                throbber.Hide();
            }
            finally
            {
                lvAssets.EndUpdate();
                m_isUpdatingColumns = false;
            }
        }

        /// <summary>
        /// Adds the columns.
        /// </summary>
        private void AddColumns()
        {
            foreach (AssetColumnSettings column in m_columns.Where(x => x.Visible))
            {
                ColumnHeader header = lvAssets.Columns.Add(column.Column.GetHeader(), column.Width);
                header.Tag = column.Column;

                switch (column.Column)
                {
                    case AssetColumn.UnitaryPrice:
                    case AssetColumn.TotalPrice:
                    case AssetColumn.Quantity:
                    case AssetColumn.Volume:
                        header.TextAlign = HorizontalAlignment.Right;
                        break;
                }
            }
        }

        /// <summary>
        /// Updates the content of the listview.
        /// </summary>
        private async Task UpdateContentAsync()
        {
            // Returns if not visible
            if (!Visible)
                return;

            int scrollBarPosition = lvAssets.GetVerticalScrollBarPosition();

            // Store the selected item (if any) to restore it after the update
            int selectedItem = lvAssets.SelectedItems.Count > 0 ? lvAssets.SelectedItems[0].
                Tag.GetHashCode() : 0;

            lvAssets.BeginUpdate();
            try
            {
                var assets = m_list.Where(x => x.Item != null && x.SolarSystem != null).
                    Where(x => IsTextMatching(x, m_textFilter)).ToList();

                UpdateSort();

                await UpdateContentByGroupAsync(assets);

                await UpdateItemsCostAsync(assets);

                // Adjust the size of the columns
                AdjustColumns();

                UpdateListVisibility();

                // Restore the selected item (if any)
                if (selectedItem > 0)
                {
                    foreach (ListViewItem lvItem in lvAssets.Items.Cast<ListViewItem>().Where(
                        lvItem => lvItem.Tag.GetHashCode() == selectedItem))
                    {
                        lvItem.Selected = true;
                    }
                }
            }
            finally
            {
                lvAssets.EndUpdate();
                lvAssets.SetVerticalScrollBarPosition(scrollBarPosition);
            }
        }

        /// <summary>
        /// Updates the items cost.
        /// </summary>
        /// <param name="assets">The assets.</param>
        private async Task UpdateItemsCostAsync(IList<Asset> assets)
        {
            lblTotalCost.Text = String.Format(CultureConstants.DefaultCulture, m_totalCostLabelDefaultText,
                await TaskHelper.RunCPUBoundTaskAsync(() => assets.Sum(asset => asset.Price * asset.Quantity)));

            if (!totalCostThrobber.Visible && !Settings.MarketPricer.Pricer.Queried)
            {
                noPricesFoundLabel.Hide();
                totalCostThrobber.Show();
                totalCostThrobber.State = ThrobberState.Rotating;
                return;
            }

            totalCostThrobber.State = ThrobberState.Stopped;
            totalCostThrobber.Hide();
            noPricesFoundLabel.Visible = assets
                .Where(asset => asset.TypeOfBlueprint != BlueprintType.Copy.ToString())
                .Any(asset => Math.Abs(asset.Price) < double.Epsilon);
        }

        /// <summary>
        /// Updates the list visibility.
        /// </summary>
        private void UpdateListVisibility()
        {
            // Display or hide the "no assets" label
            if (!m_init)
                return;

            noAssetsLabel.Visible = lvAssets.Items.Count == 0;
            estimatedCostPanel.Visible = !noAssetsLabel.Visible;
            lvAssets.Visible = !noAssetsLabel.Visible;
        }

        /// <summary>
        /// Updates the content by group.
        /// </summary>
        /// <param name="assets">The assets.</param>
        private async Task UpdateContentByGroupAsync(IEnumerable<Asset> assets)
        {
            switch (m_grouping)
            {
                case AssetGrouping.None:
                    await UpdateNoGroupContentAsync(assets);
                    break;
                case AssetGrouping.Group:
                    IOrderedEnumerable<IGrouping<string, Asset>> groups1 =
                        assets.GroupBy(x => x.Item.GroupName).OrderBy(x => x.Key);
                    await UpdateContentAsync(groups1);
                    break;
                case AssetGrouping.GroupDesc:
                    IOrderedEnumerable<IGrouping<string, Asset>> groups2 =
                        assets.GroupBy(x => x.Item.GroupName).OrderByDescending(x => x.Key);
                    await UpdateContentAsync(groups2);
                    break;
                case AssetGrouping.Category:
                    IOrderedEnumerable<IGrouping<string, Asset>> groups3 =
                        assets.GroupBy(x => x.Item.CategoryName).OrderBy(x => x.Key);
                    await UpdateContentAsync(groups3);
                    break;
                case AssetGrouping.CategoryDesc:
                    IOrderedEnumerable<IGrouping<string, Asset>> groups4 =
                        assets.GroupBy(x => x.Item.CategoryName).OrderByDescending(x => x.Key);
                    await UpdateContentAsync(groups4);
                    break;
                case AssetGrouping.Container:
                    IOrderedEnumerable<IGrouping<string, Asset>> groups5 =
                        assets.GroupBy(x => x.Container).OrderBy(x => x.Key);
                    await UpdateContentAsync(groups5);
                    break;
                case AssetGrouping.ContainerDesc:
                    IOrderedEnumerable<IGrouping<string, Asset>> groups6 =
                        assets.GroupBy(x => x.Container).OrderByDescending(x => x.Key);
                    await UpdateContentAsync(groups6);
                    break;
                case AssetGrouping.Location:
                    IOrderedEnumerable<IGrouping<string, Asset>> groups7 =
                        assets.GroupBy(x => x.Location).OrderBy(x => x.Key);
                    await UpdateContentAsync(groups7);
                    break;
                case AssetGrouping.LocationDesc:
                    IOrderedEnumerable<IGrouping<string, Asset>> groups8 =
                        assets.GroupBy(x => x.Location).OrderByDescending(x => x.Key);
                    await UpdateContentAsync(groups8);
                    break;
                case AssetGrouping.Region:
                    IOrderedEnumerable<IGrouping<Region, Asset>> groups9 =
                        assets.GroupBy(x => x.SolarSystem.Constellation.Region).OrderBy(x => x.Key);
                    await UpdateContentAsync(groups9);
                    break;
                case AssetGrouping.RegionDesc:
                    IOrderedEnumerable<IGrouping<Region, Asset>> groups10 =
                        assets.GroupBy(x => x.SolarSystem.Constellation.Region).OrderByDescending(x => x.Key);
                    await UpdateContentAsync(groups10);
                    break;
                case AssetGrouping.Jumps:
                    IOrderedEnumerable<IGrouping<int, Asset>> groups11 =
                        assets.GroupBy(x => x.Jumps).OrderBy(x => x.Key);
                    await UpdateContentAsync(groups11);
                    break;
                case AssetGrouping.JumpsDesc:
                    IOrderedEnumerable<IGrouping<int, Asset>> groups12 =
                        assets.GroupBy(x => x.Jumps).OrderByDescending(x => x.Key);
                    await UpdateContentAsync(groups12);
                    break;
            }
        }

        /// <summary>
        /// Updates the content of the listview.
        /// </summary>
        /// <param name="assets">The assets.</param>
        private Task UpdateNoGroupContentAsync(IEnumerable<Asset> assets)
            => TaskHelper.RunCPUBoundTaskAsync(() =>
            {
                return assets.Select(asset => new
                {
                    asset,
                    item = new ListViewItem(asset.Item.Name)
                    {
                        UseItemStyleForSubItems = false,
                        Tag = asset
                    }
                }).Select(x => CreateSubItems(x.asset, x.item)).ToArray();

            }).ContinueWith(task =>
            {
                lvAssets.Groups.Clear();
                lvAssets.Items.Clear();

                lvAssets.Items.AddRange(task.Result);

            }, EveMonClient.CurrentSynchronizationContext);

        /// <summary>
        /// Updates the content of the listview.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="groups"></param>
        private Task UpdateContentAsync<TKey>(IEnumerable<IGrouping<TKey, Asset>> groups)
            => TaskHelper.RunCPUBoundTaskAsync(() =>
            {
                var listOfGroups = new List<ListViewGroup>();
                var listOfItems = new List<ListViewItem>();

                // Add the groups
                foreach (IGrouping<TKey, Asset> group in groups)
                {
                    string groupText;
                    if (@group.Key is int) // Really ugly way but couldn't figured another way
                        groupText = @group.First().JumpsText;
                    else
                        groupText = @group.Key?.ToString() ?? string.Empty;

                    ListViewGroup listGroup = new ListViewGroup(groupText);
                    listOfGroups.Add(listGroup);

                    ListViewItem[] items = @group.Select(asset => new
                    {
                        asset,
                        item = new ListViewItem(asset.Item.Name, listGroup)
                        {
                            UseItemStyleForSubItems = false,
                            Tag = asset
                        }
                    }).Select(x => CreateSubItems(x.asset, x.item)).ToArray();
                    listOfItems.AddRange(items);
                }

                return new Tuple<ListViewGroup[], ListViewItem[]>(listOfGroups.ToArray(), listOfItems.ToArray());

            }).ContinueWith(task =>
            {
                lvAssets.Items.Clear();
                lvAssets.Groups.Clear();

                lvAssets.Groups.AddRange(task.Result.Item1);
                lvAssets.Items.AddRange(task.Result.Item2);

            }, EveMonClient.CurrentSynchronizationContext);

        /// <summary>
        /// Creates the list view sub items.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <param name="item">The item.</param>
        private ListViewItem CreateSubItems(Asset asset, ListViewItem item)
        {
            // Add enough subitems to match the number of columns
            while (item.SubItems.Count < lvAssets.Columns.Count + 1)
            {
                item.SubItems.Add(String.Empty);
            }

            // Creates the subitems
            for (int i = 0; i < lvAssets.Columns.Count; i++)
            {
                SetColumn(asset, item.SubItems[i], (AssetColumn)lvAssets.Columns[i].Tag);
            }

            return item;
        }

        /// <summary>
        /// Adjusts the columns.
        /// </summary>
        private void AdjustColumns()
        {
            foreach (ColumnHeader column in lvAssets.Columns)
            {
                if (m_columns[column.Index].Width == -1)
                    m_columns[column.Index].Width = -2;

                column.Width = m_columns[column.Index].Width;

                // Due to .NET design we need to prevent the last colummn to resize to the right end

                // Return if it's not the last column and not set to auto-resize
                if (column.Index != lvAssets.Columns.Count - 1 || m_columns[column.Index].Width != -2)
                    continue;

                const int Pad = 4;

                // Calculate column header text width with padding
                int columnHeaderWidth = TextRenderer.MeasureText(column.Text, Font).Width + Pad * 2;

                // If there is an image assigned to the header, add its width with padding
                if (lvAssets.SmallImageList != null && column.ImageIndex > -1)
                    columnHeaderWidth += lvAssets.SmallImageList.ImageSize.Width + Pad;

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
            lvAssets.ListViewItemSorter = new ListViewItemComparerByTag<Asset>(
                new AssetComparer(m_sortCriteria, m_sortAscending));

            UpdateSortVisualFeedback();
        }

        /// <summary>
        /// Updates the sort feedback (the arrow on the header).
        /// </summary>
        private void UpdateSortVisualFeedback()
        {
            foreach (ColumnHeader columnHeader in lvAssets.Columns.Cast<ColumnHeader>())
            {
                AssetColumn column = (AssetColumn)columnHeader.Tag;
                if (m_sortCriteria == column)
                    columnHeader.ImageIndex = m_sortAscending ? 0 : 1;
                else
                    columnHeader.ImageIndex = 2;
            }
        }

        /// <summary>
        /// Updates the listview sub-item.
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="item"></param>
        /// <param name="column"></param>
        private static void SetColumn(Asset asset, ListViewItem.ListViewSubItem item, AssetColumn column)
        {
            bool numberFormat = Settings.UI.MainWindow.Assets.NumberAbsFormat;

            switch (column)
            {
                case AssetColumn.ItemName:
                    item.Text = asset.Item.Name;
                    break;
                case AssetColumn.Quantity:
                    item.Text = numberFormat
                        ? FormatHelper.Format(asset.Quantity, AbbreviationFormat.AbbreviationSymbols)
                        : asset.Quantity.ToNumericString(0);
                    break;
                case AssetColumn.UnitaryPrice:
                    item.Text = numberFormat
                        ? FormatHelper.Format(asset.Price, AbbreviationFormat.AbbreviationSymbols)
                        : asset.Price.ToNumericString(2);
                    break;
                case AssetColumn.TotalPrice:
                    item.Text = numberFormat
                        ? FormatHelper.Format(asset.Cost, AbbreviationFormat.AbbreviationSymbols)
                        : asset.Cost.ToNumericString(2);
                    break;
                case AssetColumn.Volume:
                    item.Text = numberFormat
                        ? FormatHelper.Format(asset.TotalVolume, AbbreviationFormat.AbbreviationSymbols)
                        : asset.TotalVolume.ToNumericString(2);
                    break;
                case AssetColumn.BlueprintType:
                    item.Text = asset.TypeOfBlueprint;
                    break;
                case AssetColumn.Group:
                    item.Text = asset.Item.GroupName;
                    break;
                case AssetColumn.Category:
                    item.Text = asset.Item.CategoryName;
                    break;
                case AssetColumn.Container:
                    item.Text = asset.Container;
                    break;
                case AssetColumn.Flag:
                    item.Text = asset.Flag;
                    break;
                case AssetColumn.Location:
                    item.Text = asset.Location;
                    item.ForeColor = asset.SolarSystem.SecurityLevelColor;
                    break;
                case AssetColumn.Region:
                    item.Text = asset.SolarSystem.Constellation.Region.Name;
                    break;
                case AssetColumn.SolarSystem:
                    item.Text = asset.SolarSystem.Name;
                    item.ForeColor = asset.SolarSystem.SecurityLevelColor;
                    break;
                case AssetColumn.FullLocation:
                    item.Text = asset.FullLocation;
                    break;
                case AssetColumn.Jumps:
                    item.Text = asset.JumpsText;
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
        private static bool IsTextMatching(Asset x, string text)
            => String.IsNullOrEmpty(text) ||
               x.Item.Name.ToUpperInvariant().Contains(text, ignoreCase: true) ||
               x.Item.GroupName.ToUpperInvariant().Contains(text, ignoreCase: true) ||
               x.Item.CategoryName.ToUpperInvariant().Contains(text, ignoreCase: true) ||
               x.TypeOfBlueprint.ToUpperInvariant().Contains(text, ignoreCase: true) ||
               x.Container.ToUpperInvariant().Contains(text, ignoreCase: true) ||
               x.Flag.ToUpperInvariant().Contains(text, ignoreCase: true) ||
               x.Location.ToUpperInvariant().Contains(text, ignoreCase: true) ||
               x.SolarSystem.Name.ToUpperInvariant().Contains(text, ignoreCase: true) ||
               x.SolarSystem.Constellation.Name.ToUpperInvariant().Contains(text, ignoreCase: true) ||
               x.SolarSystem.Constellation.Region.Name.ToUpperInvariant().Contains(text, ignoreCase: true);

        /// <summary>
        /// Gets the tool tip text.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        private string GetToolTipText(ListViewItem item)
        {
            if (!item.Selected || lvAssets.SelectedItems.Count < 2)
                return String.Empty;

            List<ListViewItem> selectedItems = lvAssets.SelectedItems.Cast<ListViewItem>().ToList();
            if (selectedItems.Any(selectedItem => selectedItem.Text != item.Text))
                return String.Empty;

            List<Asset> selectedAssets = selectedItems.Select(selectedItem => selectedItem.Tag).OfType<Asset>().ToList();
            long sumQuantity = selectedAssets.Sum(selectedAsset => selectedAsset.Quantity);
            double sumVolume = selectedAssets.Sum(selectedAsset => selectedAsset.TotalVolume);
            int uniqueLocations = selectedAssets.Select(asset => asset.Location).Distinct().Count();
            int minJumps = selectedAssets.Min(asset => asset.Jumps);
            int maxJumps = selectedAssets.Max(asset => asset.Jumps);
            Asset closestAsset = selectedAssets.First(asset => asset.Jumps == minJumps);
            Asset farthestAsset = selectedAssets.Last(asset => asset.Jumps == maxJumps);

            StringBuilder builder = new StringBuilder();
            builder.Append($"{item.Text} ({selectedAssets.First().Volume:N2} m³)")
                .AppendLine()
                .Append($"Total Quantity: {sumQuantity:N0} in {uniqueLocations:N0} " +
                        $"{(uniqueLocations > 1 ? "different " : String.Empty)}location{(uniqueLocations > 1 ? "s" : String.Empty)}")
                .AppendLine()
                .Append($"Total Volume: {sumVolume:N2} m³")
                .AppendLine()
                .Append($"Closest Location: {closestAsset.Location} ({closestAsset.JumpsText})")
                .AppendLine();

            if (closestAsset.Location != farthestAsset.Location)
                builder.Append($"Farthest Location: {farthestAsset.Location} ({farthestAsset.JumpsText})");

            return builder.ToString();
        }

        /// <summary>
        /// Updates the asset location.
        /// </summary>
        private async Task UpdateAssetLocationAsync()
        {
            // Invoke it on a worker thread cause it may be time intensive
            // if character owns many stuff in several locations
            await TaskHelper.RunCPUBoundTaskAsync(() =>
            {
                Character.Assets.UpdateLocation();
                Assets = Character.Assets;
            });

            await UpdateColumnsAsync();
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
            ListViewExporter.CreateCSV(lvAssets);
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

            if (m_columns[e.ColumnIndex].Width == lvAssets.Columns[e.ColumnIndex].Width)
                return;

            m_columns[e.ColumnIndex].Width = lvAssets.Columns[e.ColumnIndex].Width;
            m_columnsChanged = true;
        }

        /// <summary>
        /// When the user clicks a column header, we update the sorting.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            AssetColumn column = (AssetColumn)lvAssets.Columns[e.Column].Tag;
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

            lvAssets.Cursor = Cursors.Default;
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

            lvAssets.Cursor = CustomCursors.ContextMenu;

            ListViewItem item = lvAssets.GetItemAt(e.Location.X, e.Location.Y);
            if (item == null)
            {
                m_tooltip.Hide();
                return;
            }

            m_tooltip.Show(GetToolTipText(item), e.Location);
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
        /// Handles key press.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.A:
                    if (e.Control)
                        lvAssets.SelectAll();
                    break;
            }
        }

        /// <summary>
        /// Handles the Opening event of the contextMenu control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="CancelEventArgs"/> instance containing the event data.</param>
        private void contextMenu_Opening(object sender, CancelEventArgs e)
        {
            bool visible = lvAssets.SelectedItems.Count > 0 &&
                  lvAssets.SelectedItems
                      .Cast<ListViewItem>()
                      .All(item => item != null &&
                                   item.Text == ((Asset)lvAssets.SelectedItems[0]?.Tag)?.Item.Name);

            showInBrowserMenuItem.Visible =
                showInBrowserMenuSeparator.Visible = visible;

            if (!visible)
                return;

            Asset asset = lvAssets.SelectedItems[0]?.Tag as Asset;

            if (asset?.Item == null)
                return;

            Blueprint blueprint = StaticBlueprints.GetBlueprintByID(asset.Item.ID);
            Ship ship = asset.Item as Ship;
            Skill skill = Character.Skills[asset.Item.ID];

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
            Asset asset = lvAssets.SelectedItems[0]?.Tag as Asset;

            if (asset?.Item == null)
                return;

            Ship ship = asset.Item as Ship;
            Blueprint blueprint = StaticBlueprints.GetBlueprintByID(asset.Item.ID);
            Skill skill = Character.Skills[asset.Item.ID];

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
                planWindow.ShowItemInBrowser(asset.Item);
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

            Settings.UI.MainWindow.Assets.Columns.Clear();
            Settings.UI.MainWindow.Assets.Columns.AddRange(Columns.Cast<AssetColumnSettings>());

            // Recreate the columns
            Columns = Settings.UI.MainWindow.Assets.Columns;
            m_columnsChanged = false;
        }

        /// <summary>
        /// When the assets update, update the list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void EveMonClient_CharacterAssetsUpdated(object sender, CharacterChangedEventArgs e)
        {
            if (Character == null || e.Character != Character)
                return;

            Assets = Character.Assets;
            await UpdateContentAsync();
        }

        /// <summary>
        /// When the conquerable station list updates, update the list.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private async void EveMonClient_ConquerableStationListUpdated(object sender, EventArgs e)
        {
            if (Character == null)
                return;

            await UpdateAssetLocationAsync();
        }

        /// <summary>
        /// When the eve flags updates, update the list.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private async void EveMonClient_EveFlagsUpdated(object sender, EventArgs e)
        {
            if (Character == null)
                return;

            await UpdateContentAsync();
        }

        /// <summary>
        /// When the character info updates, update the list.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <remarks>Mainly to update the jumps from charater last known location to assets.</remarks>
        private async void EveMonClient_CharacterInfoUpdated(object sender, CharacterChangedEventArgs e)
        {
            if (Character == null || e.Character != Character)
                return;

            await UpdateAssetLocationAsync();
        }

        /// <summary>
        /// Handles the SettingsChanged event of the EveMonClient control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private async void EveMonClient_SettingsChanged(object sender, EventArgs e)
        {
            // No need to do this if control is not visible
            if (!Visible)
                return;

            await UpdateContentAsync();
        }

        /// <summary>
        /// Occurs when the item prices get updated.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private async void EveMonClient_ItemPricesUpdated(object sender, EventArgs e)
        {
            await UpdateContentAsync();
        }

        #endregion
    }
}
