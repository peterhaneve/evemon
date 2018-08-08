using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using EVEMon.Common;
using EVEMon.Common.Constants;
using EVEMon.Common.Controls;
using EVEMon.Common.CustomEventArgs;
using EVEMon.Common.Enumerations;
using EVEMon.Common.Extensions;
using EVEMon.Common.Factories;
using EVEMon.Common.Models;
using EVEMon.Common.Service;

namespace EVEMon.CharacterMonitoring
{
    internal sealed partial class CharacterFactionalWarfareStatsList : UserControl
    {
        #region Fields

        private FactionalWarfareStats m_charFacWarStats;

        #endregion


        #region Constructor

        public CharacterFactionalWarfareStatsList()
        {
            InitializeComponent();

            ListPanel.Visible = false;
            notEnlistedLabel.Visible = false;

            notEnlistedLabel.Font = FontFactory.GetFont("Tahoma", 11.25F, FontStyle.Bold);
            noFactionalWarfareLabel.Font = FontFactory.GetFont("Tahoma", 11.25F, FontStyle.Bold);
        }

        #endregion


        #region Properties

        /// <summary>
        /// Gets the character associated with this monitor.
        /// </summary>
        internal CCPCharacter Character { get; set; }

        #endregion


        #region Inherited events

        /// <summary>
        /// On load subscribe the events.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (DesignMode || this.IsDesignModeHosted())
                return;

            EveMonClient.CharacterFactionalWarfareStatsUpdated += EveMonClient_CharacterFactionalWarfareStatsUpdated;
            EveMonClient.EveFactionalWarfareStatsUpdated += EveMonClient_EveFactionalWarfareStatsUpdated;
            EveMonClient.SettingsChanged += EveMonClient_SettingsChanged;
            Disposed += OnDisposed;
        }

        /// <summary>
        /// Unsubscribe events on disposing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDisposed(object sender, EventArgs e)
        {
            EveMonClient.CharacterFactionalWarfareStatsUpdated -= EveMonClient_CharacterFactionalWarfareStatsUpdated;
            EveMonClient.EveFactionalWarfareStatsUpdated -= EveMonClient_EveFactionalWarfareStatsUpdated;
            EveMonClient.SettingsChanged -= EveMonClient_SettingsChanged;
            Disposed -= OnDisposed;
        }

        /// <summary>
        /// When the control becomes visible again, we update the content.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            if (Visible)
                UpdateContent();
        }

        #endregion


        #region Content Management

        /// <summary>
        /// Updates all the content.
        /// </summary>
        private void UpdateContent()
        {
            // Returns if not visible
            if (!Visible)
                return;

            // When no character, we just hide the list
            if (Character == null)
            {
                noFactionalWarfareLabel.Visible = true;
                notEnlistedLabel.Visible = false;
                ListPanel.Visible = false;
                return;
            }

            // When character is not enlisted in factional warfare, we just hide the list
            // and display the 'Not enlisted' label
            if (Character.IsFactionalWarfareNotEnlisted)
            {
                notEnlistedLabel.Visible = true;
                noFactionalWarfareLabel.Visible = false;
                ListPanel.Visible = false;
                return;
            }

            m_charFacWarStats = Character.FactionalWarfareStats;

            // Update the header controls
            UpdateHeader();

            // Update the personal stats list
            UpdatePersonalStatsList();

            // Update the militia stats list
            UpdateMilitiaStatsList();

            noFactionalWarfareLabel.Visible = m_charFacWarStats == null;
            ListPanel.Visible = !noFactionalWarfareLabel.Visible;
        }

        /// <summary>
        /// Updates the header.
        /// </summary>
        private void UpdateHeader()
        {
            if (m_charFacWarStats == null)
                return;

            List<int> factionsAgainstEnlisted =
                EveFactionalWarfareStats.GetAgainstFactionIDs(m_charFacWarStats.FactionID).ToList();

            FactionPictureBox.Visible = !Settings.UI.SafeForWork;
            CorporationPictureBox.Visible = !Settings.UI.SafeForWork;
            FightingPartiesPanel.Visible = !Settings.UI.SafeForWork || !factionsAgainstEnlisted.Any();

            // Update the images
            ImageService.GetAllianceImageAsync(FactionPictureBox, m_charFacWarStats.FactionID);
            ImageService.GetCorporationImageAsync(CorporationPictureBox, Character.CorporationID);
            ImageService.GetAllianceImageAsync(EnlistedFactionPictureBox, m_charFacWarStats.FactionID);

            if (factionsAgainstEnlisted.Any())
            {
                ImageService.GetAllianceImageAsync(PrimeAgainstFactionPictureBox, factionsAgainstEnlisted[0]);
                ImageService.GetAllianceImageAsync(AllyAgainstFactionPictureBox, factionsAgainstEnlisted[1]);
            }

            // Update the labels
            string highestRankText = m_charFacWarStats.HighestRank > m_charFacWarStats.CurrentRank
                ? $"({GetMilitiaRank(m_charFacWarStats.HighestRank)})"
                : String.Empty;

            TimeSpan timeServed = DateTime.UtcNow.Subtract(m_charFacWarStats.EnlistedDate);
            string timeServedText = timeServed < TimeSpan.FromDays(1) ? "Less than one day." :
                $"{timeServed.Days} day{(timeServed.Days.S())}";

            FactionLabel.Text = $"Faction: {m_charFacWarStats.FactionName}";
            CorporationLabel.Text = $"Corporation: {Character.CorporationName}";
            RankLabel.Text = $"Rank: {GetMilitiaRank(m_charFacWarStats.CurrentRank)} {highestRankText}";
            TimeServedLabel.Text = $"Time served: {timeServedText}";
        }

        /// <summary>
        /// Updates the personal stats list.
        /// </summary>
        private void UpdatePersonalStatsList()
        {
            if (m_charFacWarStats == null)
                return;

            lvPersonal.BeginUpdate();
            try
            {
                foreach (ListViewItem item in lvPersonal.Items.Cast<ListViewItem>())
                {
                    // Add enough subitems to match the number of columns
                    while (item.SubItems.Count < lvPersonal.Columns.Count)
                    {
                        item.SubItems.Add(String.Empty);
                    }

                    // Create the subitems
                    CreatePersonalListViewSubItems(item);
                }

                // Adjust the size of the columns
                AdjustColumns(lvPersonal);
            }
            finally
            {
                lvPersonal.EndUpdate();
            }
        }

        /// <summary>
        /// Creates the personal list view sub items.
        /// </summary>
        /// <param name="item">The item.</param>
        private void CreatePersonalListViewSubItems(ListViewItem item)
        {
            // Clear the subitems except the item itself
            for (int i = 1; i < lvPersonal.Columns.Count; i++)
            {
                item.SubItems.RemoveAt(1);
            }

            switch (item.Index)
            {
                case 0:
                    item.SubItems.Add(m_charFacWarStats.KillsYesterday.ToNumericString(0));
                    item.SubItems.Add(EveFactionalWarfareStats.TotalsKillsYesterday.ToNumericString(0));
                    break;
                case 1:
                    item.SubItems.Add(m_charFacWarStats.KillsLastWeek.ToNumericString(0));
                    item.SubItems.Add(EveFactionalWarfareStats.TotalsKillsLastWeek.ToNumericString(0));
                    break;
                case 2:
                    item.SubItems.Add(m_charFacWarStats.KillsTotal.ToNumericString(0));
                    item.SubItems.Add(EveFactionalWarfareStats.TotalsKillsTotal.ToNumericString(0));
                    break;
                case 3:
                    item.SubItems.Add(m_charFacWarStats.VictoryPointsYesterday.ToNumericString(0));
                    item.SubItems.Add(EveFactionalWarfareStats.TotalsVictoryPointsYesterday.ToNumericString(0));
                    break;
                case 4:
                    item.SubItems.Add(m_charFacWarStats.VictoryPointsLastWeek.ToNumericString(0));
                    item.SubItems.Add(EveFactionalWarfareStats.TotalsVictoryPointsLastWeek.ToNumericString(0));
                    break;
                case 5:
                    item.SubItems.Add(m_charFacWarStats.VictoryPointsTotal.ToNumericString(0));
                    item.SubItems.Add(EveFactionalWarfareStats.TotalsVictoryPointsTotal.ToNumericString(0));
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Updates the militia stats list.
        /// </summary>
        private void UpdateMilitiaStatsList()
        {
            if (m_charFacWarStats == null)
                return;

            lvMilitia.BeginUpdate();
            try
            {
                foreach (ListViewItem item in lvMilitia.Items.Cast<ListViewItem>())
                {
                    // Add enough subitems to match the number of columns
                    while (item.SubItems.Count < lvMilitia.Columns.Count)
                    {
                        item.SubItems.Add(String.Empty);
                    }

                    // Create the subitems
                    CreateMilitiaListViewSubItems(item);
                }

                // Adjust the size of the columns
                AdjustColumns(lvMilitia);
            }
            finally
            {
                lvMilitia.EndUpdate();
            }
        }

        /// <summary>
        /// Creates the militia list view sub items.
        /// </summary>
        /// <param name="item">The item.</param>
        private void CreateMilitiaListViewSubItems(ListViewItem item)
        {
            // Exit if EVE factional warfare stats have not yet been updated
            if (!EveFactionalWarfareStats.FactionalWarfareStats.Any())
                return;

            EveFactionWarfareStats amarrFacWarStats =
                EveFactionalWarfareStats.GetFactionalWarfareStatsForFaction(DBConstants.AmarrFactionID);
            EveFactionWarfareStats caldariFacWarStats =
                EveFactionalWarfareStats.GetFactionalWarfareStatsForFaction(DBConstants.CaldariFactionID);
            EveFactionWarfareStats gallenteFacWarStats =
                EveFactionalWarfareStats.GetFactionalWarfareStatsForFaction(DBConstants.GallenteFactionID);
            EveFactionWarfareStats minmatarFacWarStats =
                EveFactionalWarfareStats.GetFactionalWarfareStatsForFaction(DBConstants.MinmatarFactionID);

            // Clear the subitems except the item itself
            for (int i = 1; i < lvMilitia.Columns.Count; i++)
            {
                item.SubItems.RemoveAt(1);
            }

            switch (item.Index)
            {
                case 0:
                    item.SubItems.Add(amarrFacWarStats.Pilots.ToNumericString(0));
                    item.SubItems.Add(caldariFacWarStats.Pilots.ToNumericString(0));
                    item.SubItems.Add(gallenteFacWarStats.Pilots.ToNumericString(0));
                    item.SubItems.Add(minmatarFacWarStats.Pilots.ToNumericString(0));
                    break;
                case 1:
                    item.SubItems.Add(amarrFacWarStats.SystemsControlled.ToNumericString(0));
                    item.SubItems.Add(caldariFacWarStats.SystemsControlled.ToNumericString(0));
                    item.SubItems.Add(gallenteFacWarStats.SystemsControlled.ToNumericString(0));
                    item.SubItems.Add(minmatarFacWarStats.SystemsControlled.ToNumericString(0));
                    break;
                case 2:
                    item.SubItems.Add(amarrFacWarStats.KillsYesterday.ToNumericString(0));
                    item.SubItems.Add(caldariFacWarStats.KillsYesterday.ToNumericString(0));
                    item.SubItems.Add(gallenteFacWarStats.KillsYesterday.ToNumericString(0));
                    item.SubItems.Add(minmatarFacWarStats.KillsYesterday.ToNumericString(0));
                    break;
                case 3:
                    item.SubItems.Add(amarrFacWarStats.KillsLastWeek.ToNumericString(0));
                    item.SubItems.Add(caldariFacWarStats.KillsLastWeek.ToNumericString(0));
                    item.SubItems.Add(gallenteFacWarStats.KillsLastWeek.ToNumericString(0));
                    item.SubItems.Add(minmatarFacWarStats.KillsLastWeek.ToNumericString(0));
                    break;
                case 4:
                    item.SubItems.Add(amarrFacWarStats.KillsTotal.ToNumericString(0));
                    item.SubItems.Add(caldariFacWarStats.KillsTotal.ToNumericString(0));
                    item.SubItems.Add(gallenteFacWarStats.KillsTotal.ToNumericString(0));
                    item.SubItems.Add(minmatarFacWarStats.KillsTotal.ToNumericString(0));
                    break;
                case 5:
                    item.SubItems.Add(amarrFacWarStats.VictoryPointsYesterday.ToNumericString(0));
                    item.SubItems.Add(caldariFacWarStats.VictoryPointsYesterday.ToNumericString(0));
                    item.SubItems.Add(gallenteFacWarStats.VictoryPointsYesterday.ToNumericString(0));
                    item.SubItems.Add(minmatarFacWarStats.VictoryPointsYesterday.ToNumericString(0));
                    break;
                case 6:
                    item.SubItems.Add(amarrFacWarStats.VictoryPointsLastWeek.ToNumericString(0));
                    item.SubItems.Add(caldariFacWarStats.VictoryPointsLastWeek.ToNumericString(0));
                    item.SubItems.Add(gallenteFacWarStats.VictoryPointsLastWeek.ToNumericString(0));
                    item.SubItems.Add(minmatarFacWarStats.VictoryPointsLastWeek.ToNumericString(0));
                    break;
                case 7:
                    item.SubItems.Add(amarrFacWarStats.VictoryPointsTotal.ToNumericString(0));
                    item.SubItems.Add(caldariFacWarStats.VictoryPointsTotal.ToNumericString(0));
                    item.SubItems.Add(gallenteFacWarStats.VictoryPointsTotal.ToNumericString(0));
                    item.SubItems.Add(minmatarFacWarStats.VictoryPointsTotal.ToNumericString(0));
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Adjusts the columns.
        /// </summary>
        private void AdjustColumns(ListView listView)
        {
            foreach (ColumnHeader column in listView.Columns)
            {
                column.Width = -2;

                // Due to .NET design we need to prevent the last colummn to resize to the right end

                // Return if it's not the last column
                if (column.Index != listView.Columns.Count - 1)
                    continue;

                const int Pad = 4;

                // Calculate column header text width with padding
                int columnHeaderWidth = TextRenderer.MeasureText(column.Text, Font).Width + Pad * 2;

                // If there is an image assigned to the header, add its width with padding
                if (listView.SmallImageList != null && column.ImageIndex > -1)
                    columnHeaderWidth += listView.SmallImageList.ImageSize.Width + Pad;

                // Calculate the width of the header and the items of the column
                int columnMaxWidth = column.ListView.Items.Cast<ListViewItem>().Select(
                    item => TextRenderer.MeasureText(item.SubItems[column.Index].Text, Font).Width).Concat(
                        new[] { columnHeaderWidth }).Max() + Pad + 1;

                // Assign the width found
                column.Width = columnMaxWidth;
            }
        }

        #endregion


        #region Helper methods

        /// <summary>
        /// Gets the militia rank.
        /// </summary>
        /// <param name="rank">The rank.</param>
        /// <returns></returns>
        private string GetMilitiaRank(int rank)
        {
            switch (Character.FactionalWarfareStats.FactionID)
            {
                case DBConstants.AmarrFactionID:
                    return GetFactionMilitiaRank<AmarrMilitiaRank>(rank).GetDescription();
                case DBConstants.CaldariFactionID:
                    return GetFactionMilitiaRank<CaldariMilitiaRank>(rank).GetDescription();
                case DBConstants.GallenteFactionID:
                    return GetFactionMilitiaRank<GallenteMilitiaRank>(rank).GetDescription();
                case DBConstants.MinmatarFactionID:
                    return GetFactionMilitiaRank<MinmatarMilitiaRank>(rank).GetDescription();
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets the faction militia rank.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rank">The rank.</param>
        /// <returns></returns>
        private static T GetFactionMilitiaRank<T>(int rank)
        {
            if (rank < Enum.GetValues(typeof(T)).Length)
                return (T)Enum.ToObject(typeof(T), rank);
            return default(T);
        }

        #endregion


        #region Global events

        /// <summary>
        /// When the character factional warfare stats update, we update the content.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EveMonClient_CharacterFactionalWarfareStatsUpdated(object sender, CharacterChangedEventArgs e)
        {
            if (e.Character != Character)
                return;

            UpdateContent();
        }

        /// <summary>
        /// When the EVE factional warfare stats update, we update the content.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EveMonClient_EveFactionalWarfareStatsUpdated(object sender, EventArgs e)
        {
            UpdateContent();
        }

        /// <summary>
        /// When the settings change, we update the content.
        /// </summary>
        /// <remarks>In case 'SafeForWork' gets enabled.</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void EveMonClient_SettingsChanged(object sender, EventArgs e)
        {
            UpdateContent();
        }

        #endregion


        #region Local events

        /// <summary>
        /// Handles the ColumnWidthChanging event of the lvPersonal control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.ColumnWidthChangingEventArgs"/> instance containing the event data.</param>
        private void lvPersonal_ColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
        {
            e.Cancel = true;
            e.NewWidth = lvPersonal.Columns[e.ColumnIndex].Width;
        }

        /// <summary>
        /// Handles the ColumnWidthChanging event of the lvMilitia control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.ColumnWidthChangingEventArgs"/> instance containing the event data.</param>
        private void lvMilitia_ColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
        {
            e.Cancel = true;
            e.NewWidth = lvMilitia.Columns[e.ColumnIndex].Width;
        }

        #endregion
    }
}
