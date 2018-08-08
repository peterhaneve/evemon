using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using EVEMon.Common;
using EVEMon.Common.Collections;
using EVEMon.Common.Constants;
using EVEMon.Common.Controls;
using EVEMon.Common.CustomEventArgs;
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
    internal sealed partial class CharacterResearchPointsList : UserControl, IListView
    {
        #region Fields

        private readonly List<ResearchColumnSettings> m_columns = new List<ResearchColumnSettings>();
        private readonly List<ResearchPoint> m_list = new List<ResearchPoint>();

        private ResearchColumn m_sortCriteria;

        private string m_textFilter = String.Empty;
        private bool m_sortAscending = true;
        private bool m_columnsChanged;
        private bool m_isUpdatingColumns;
        private bool m_init;

        #endregion


        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        public CharacterResearchPointsList()
        {
            InitializeComponent();

            lvResearchPoints.Hide();
            lvResearchPoints.AllowColumnReorder = true;
            lvResearchPoints.Columns.Clear();

            noResearchPointsLabel.Font = FontFactory.GetFont("Tahoma", 11.25F, FontStyle.Bold);

            ListViewHelper.EnableDoubleBuffer(lvResearchPoints);

            lvResearchPoints.ColumnClick += listView_ColumnClick;
            lvResearchPoints.ColumnWidthChanged += listView_ColumnWidthChanged;
            lvResearchPoints.ColumnReordered += listView_ColumnReordered;
            lvResearchPoints.MouseDown += listView_MouseDown;
            lvResearchPoints.MouseMove += listView_MouseMove;
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
                    UpdateColumns();
            }
        }

        /// <summary>
        /// Gets or sets the enumeration of research points to display.
        /// </summary>
        private IEnumerable<ResearchPoint> ResearchPoints
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
        public Enum Grouping { get; set; }

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
                List<ResearchColumnSettings> newColumns = new List<ResearchColumnSettings>();
                foreach (ColumnHeader header in lvResearchPoints.Columns.Cast<ColumnHeader>().OrderBy(x => x.DisplayIndex))
                {
                    ResearchColumnSettings columnSetting = m_columns.First(x => x.Column == (ResearchColumn)header.Tag);
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
                    m_columns.AddRange(value.Cast<ResearchColumnSettings>());

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

            EveMonClient.TimerTick += EveMonClient_TimerTick;
            EveMonClient.ConquerableStationListUpdated += EveMonClient_ConquerableStationListUpdated;
            EveMonClient.CharacterResearchPointsUpdated += EveMonClient_CharacterResearchPointsUpdated;
            Disposed += OnDisposed;
        }

        /// <summary>
        /// Unsubscribe events on disposing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDisposed(object sender, EventArgs e)
        {
            EveMonClient.TimerTick -= EveMonClient_TimerTick;
            EveMonClient.ConquerableStationListUpdated -= EveMonClient_ConquerableStationListUpdated;
            EveMonClient.CharacterResearchPointsUpdated -= EveMonClient_CharacterResearchPointsUpdated;
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

            lvResearchPoints.Visible = false;

            ResearchPoints = Character?.ResearchPoints;
            Columns = Settings.UI.MainWindow.Research.Columns;
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
        private void UpdateColumns()
        {
            // Returns if not visible
            if (!Visible)
                return;

            lvResearchPoints.BeginUpdate();
            m_isUpdatingColumns = true;

            try
            {
                lvResearchPoints.Columns.Clear();
                lvResearchPoints.Groups.Clear();
                lvResearchPoints.Items.Clear();

                foreach (ResearchColumnSettings column in m_columns.Where(x => x.Visible))
                {
                    ColumnHeader header = lvResearchPoints.Columns.Add(column.Column.GetHeader(), column.Width);
                    header.Tag = column.Column;

                    switch (column.Column)
                    {
                        case ResearchColumn.CurrentRP:
                        case ResearchColumn.PointsPerDay:
                            header.TextAlign = HorizontalAlignment.Right;
                            break;
                        case ResearchColumn.Level:
                            header.TextAlign = HorizontalAlignment.Center;
                            break;
                    }
                }

                // We update the content
                UpdateContent();
            }
            finally
            {
                lvResearchPoints.EndUpdate();
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

            int scrollBarPosition = lvResearchPoints.GetVerticalScrollBarPosition();

            // Store the selected item (if any) to restore it after the update
            int selectedItem = lvResearchPoints.SelectedItems.Count > 0 ?
                lvResearchPoints.SelectedItems[0].Tag.GetHashCode() : 0;

            lvResearchPoints.BeginUpdate();
            try
            {
                IEnumerable<ResearchPoint> researchPoints = m_list.Where(x => !string.
                    IsNullOrEmpty(x.AgentName) && !string.IsNullOrEmpty(x.Field) &&
                    x.Station != null).Where(x => IsTextMatching(x, m_textFilter));

                UpdateSort();

                lvResearchPoints.Items.Clear();

                // Add the items
                lvResearchPoints.Items.AddRange(researchPoints.Select(researchPoint => new
                {
                    researchPoint,
                    item = new ListViewItem(researchPoint.AgentName)
                    {
                        UseItemStyleForSubItems = false,
                        Tag = researchPoint
                    }
                }).Select(x => CreateSubItems(x.researchPoint, x.item)).ToArray());

                // Restore the selected item (if any)
                if (selectedItem > 0)
                {
                    foreach (ListViewItem lvItem in lvResearchPoints.Items.Cast<ListViewItem>().Where(
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
                lvResearchPoints.EndUpdate();
                lvResearchPoints.SetVerticalScrollBarPosition(scrollBarPosition);
            }
        }

        /// <summary>
        /// Creates the list view sub items.
        /// </summary>
        /// <param name="researchPoint">The research point.</param>
        /// <param name="item">The item.</param>
        private ListViewItem CreateSubItems(ResearchPoint researchPoint, ListViewItem item)
        {
            // Add enough subitems to match the number of columns
            while (item.SubItems.Count < lvResearchPoints.Columns.Count + 1)
            {
                item.SubItems.Add(String.Empty);
            }

            // Creates the subitems
            for (int i = 0; i < lvResearchPoints.Columns.Count; i++)
            {
                SetColumn(researchPoint, item.SubItems[i], (ResearchColumn)lvResearchPoints.Columns[i].Tag);
            }

            return item;
        }

        /// <summary>
        /// Updates the list visibility.
        /// </summary>
        private void UpdateListVisibility()
        {
            // Display or hide the "no research points" label
            if (!m_init)
                return;

            noResearchPointsLabel.Visible = lvResearchPoints.Items.Count == 0;
            lvResearchPoints.Visible = !noResearchPointsLabel.Visible;
        }

        /// <summary>
        /// Adjusts the columns.
        /// </summary>
        private void AdjustColumns()
        {
            foreach (ColumnHeader column in lvResearchPoints.Columns)
            {
                if (m_columns[column.Index].Width == -1)
                    m_columns[column.Index].Width = -2;

                column.Width = m_columns[column.Index].Width;

                // Due to .NET design we need to prevent the last colummn to resize to the right end

                // Return if it's not the last column and not set to auto-resize
                if (column.Index != lvResearchPoints.Columns.Count - 1 || m_columns[column.Index].Width != -2)
                    continue;

                const int Pad = 4;

                // Calculate column header text width with padding
                int columnHeaderWidth = TextRenderer.MeasureText(column.Text, Font).Width + Pad * 2;

                // If there is an image assigned to the header, add its width with padding
                if (lvResearchPoints.SmallImageList != null && column.ImageIndex > -1)
                    columnHeaderWidth += lvResearchPoints.SmallImageList.ImageSize.Width + Pad;

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
            lvResearchPoints.ListViewItemSorter = new ListViewItemComparerByTag<ResearchPoint>(
                new ResearchPointComparer(m_sortCriteria, m_sortAscending));

            UpdateSortVisualFeedback();
        }

        /// <summary>
        /// Updates the sort feedback (the arrow on the header).
        /// </summary>
        private void UpdateSortVisualFeedback()
        {
            foreach (ColumnHeader columnHeader in lvResearchPoints.Columns.Cast<ColumnHeader>())
            {
                ResearchColumn column = (ResearchColumn)columnHeader.Tag;
                if (m_sortCriteria == column)
                    columnHeader.ImageIndex = m_sortAscending ? 0 : 1;
                else
                    columnHeader.ImageIndex = 2;
            }
        }

        /// <summary>
        /// Updates the listview sub-item.
        /// </summary>
        /// <param name="researchPoint"></param>
        /// <param name="item"></param>
        /// <param name="column"></param>
        private static void SetColumn(ResearchPoint researchPoint, ListViewItem.ListViewSubItem item, ResearchColumn column)
        {
            var station = researchPoint.Station;
            switch (column)
            {
            case ResearchColumn.Agent:
                item.Text = researchPoint.AgentName;
                break;
            case ResearchColumn.Level:
                item.Text = researchPoint.AgentLevel.ToString(CultureConstants.DefaultCulture);
                break;
            case ResearchColumn.Field:
                item.Text = researchPoint.Field;
                break;
            case ResearchColumn.CurrentRP:
                item.Text = researchPoint.CurrentRP.ToNumericString(2);
                break;
            case ResearchColumn.PointsPerDay:
                item.Text = researchPoint.PointsPerDay.ToNumericString(2);
                break;
            case ResearchColumn.StartDate:
                item.Text = researchPoint.StartDate.ToLocalTime().ToString("G");
                break;
            case ResearchColumn.Location:
                item.Text = station.FullLocation;
                break;
            case ResearchColumn.Region:
                item.Text = station.SolarSystemChecked.Constellation.Region.Name;
                break;
            case ResearchColumn.SolarSystem:
                item.Text = station.SolarSystem?.Name ?? EveMonConstants.UnknownText;
                item.ForeColor = station.SolarSystemChecked.SecurityLevelColor;
                break;
            case ResearchColumn.Station:
                item.Text = station.Name;
                break;
            case ResearchColumn.Quality:
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
        private static bool IsTextMatching(ResearchPoint x, string text) => String.IsNullOrEmpty(text)
            || x.AgentName.ToUpperInvariant().Contains(text, ignoreCase: true)
            || x.Field.ToUpperInvariant().Contains(text, ignoreCase: true)
            || x.Station.Name.ToUpperInvariant().Contains(text, ignoreCase: true)
            || x.Station.SolarSystemChecked.Name.ToUpperInvariant().Contains(text, ignoreCase: true)
            || x.Station.SolarSystemChecked.Constellation.Name.ToUpperInvariant().Contains(text, ignoreCase: true)
            || x.Station.SolarSystemChecked.Constellation.Region.Name.ToUpperInvariant().Contains(text, ignoreCase: true);

        #endregion


        #region Local Event Handlers

        /// <summary>
        /// Exports item info to CSV format.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exportToCSVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListViewExporter.CreateCSV(lvResearchPoints);
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

            if (m_columns[e.ColumnIndex].Width == lvResearchPoints.Columns[e.ColumnIndex].Width)
                return;

            m_columns[e.ColumnIndex].Width = lvResearchPoints.Columns[e.ColumnIndex].Width;
            m_columnsChanged = true;
        }

        /// <summary>
        /// When the user clicks a column header, we update the sorting.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ResearchColumn column = (ResearchColumn)lvResearchPoints.Columns[e.Column].Tag;
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
            if (e.Button == MouseButtons.Left)
                return;

            lvResearchPoints.Cursor = Cursors.Default;
        }

        /// <summary>
        /// When the mouse moves over the list, we change the cursor.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
        private void listView_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                return;

            lvResearchPoints.Cursor = CustomCursors.ContextMenu;
        }

        /// <summary>
        /// Handles the Opening event of the contextMenu control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="CancelEventArgs"/> instance containing the event data.</param>
        private void contextMenu_Opening(object sender, CancelEventArgs e)
        {
            showInSkillBrowserMenuItem.Visible =
                showObjectInBrowserMenuItem.Visible =
                    showInSkillBrowserMenuSeparator.Visible = lvResearchPoints.SelectedItems.Count != 0;
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

            ResearchPoint researchPoint = lvResearchPoints.SelectedItems[0]?.Tag as ResearchPoint;

            // showInSkillBrowserMenuItem
            if (menuItem == showInSkillBrowserMenuItem)
            {
                if (researchPoint?.Field == null)
                    return;

                Skill skill = Character.Skills[researchPoint.Skill.ID];

                if (skill != Skill.UnknownSkill)
                    PlanWindow.ShowPlanWindow(Character).ShowSkillInBrowser(skill);

                return;
            }

            if (researchPoint?.ResearchedItem == null)
                return;

            PlanWindow.ShowPlanWindow(Character).ShowItemInBrowser(researchPoint?.ResearchedItem);
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

            Settings.UI.MainWindow.Research.Columns.Clear();
            Settings.UI.MainWindow.Research.Columns.AddRange(Columns.Cast<ResearchColumnSettings>());

            // Recreate the columns
            Columns = Settings.UI.MainWindow.Research.Columns;
            m_columnsChanged = false;
        }

        /// <summary>
        /// When the research points change update the list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EveMonClient_CharacterResearchPointsUpdated(object sender, CharacterChangedEventArgs e)
        {
            if (Character == null || e.Character != Character)
                return;

            ResearchPoints = Character.ResearchPoints;
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

            foreach (ResearchPoint researchPoint in m_list)
            {
                researchPoint.UpdateStation();
            }

            UpdateColumns();
        }

        #endregion
    }
}
