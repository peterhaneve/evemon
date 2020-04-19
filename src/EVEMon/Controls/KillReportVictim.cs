using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using EVEMon.Common.Constants;
using EVEMon.Common.Controls;
using EVEMon.Common.Data;
using EVEMon.Common.Enumerations;
using EVEMon.Common.Extensions;
using EVEMon.Common.Helpers;
using EVEMon.Common.Models;
using EVEMon.Common.Service;
using EVEMon.SkillPlanner;

namespace EVEMon.Controls
{
    public partial class KillReportVictim : UserControl
    {
        private KillLog m_killLog;


        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="KillReportVictim"/> class.
        /// </summary>
        public KillReportVictim()
        {
            InitializeComponent();
        }

        #endregion


        #region Properties

        /// <summary>
        /// Gets or sets the kill log.
        /// </summary>
        /// <value>
        /// The kill log.
        /// </value>
        internal KillLog KillLog
        {
            get { return m_killLog; }
            set
            {
                m_killLog = value;
                UpdateContent();
            }
        }

        #endregion


        #region Content Management Methods

        /// <summary>
        /// Updates the content.
        /// </summary>
        private void UpdateContent()
        {
            Task.WhenAll(
                GetImageForAsync(CharacterPictureBox),
                GetImageForAsync(ShipPictureBox),
                GetImageForAsync(CorpPictureBox));

            if (m_killLog.Victim.AllianceID != 0)
            {
                Task.WhenAll(GetImageForAsync(AlliancePictureBox));
                AllianceNameLabel.Text = m_killLog.Victim.AllianceName;
            }
            else
            {
                CorpAllianceFlowLayoutPanel.Controls.Remove(AllianceNameLabel);
                CorpAllianceFlowLayoutPanel.Padding = new Padding(0, 12, 0, 0);
            }

            CharacterNameLabel.Text = m_killLog.Victim.Name;
            CorpNameLabel.Text = m_killLog.Victim.CorporationName;

            Item ship = StaticItems.GetItemByID(m_killLog.Victim.ShipTypeID);
            ShipNameLabel.Text = ship.Name;
            ShipGroupLabel.Text = string.Format(CultureConstants.DefaultCulture, ShipGroupLabel.Text, ship.GroupName);

            KillTimeLabel.Text = m_killLog.KillTime.ToLocalTime().DateTimeToDotFormattedString();
            SolarSystemLabel.Text = m_killLog.SolarSystem?.Name;
            SecStatusLabel.Text = m_killLog.SolarSystem?.SecurityLevel.ToNumericString(1);
            SecStatusLabel.ForeColor = m_killLog.SolarSystem?.SecurityLevelColor ?? SystemColors.ControlText;
            ConstelationLabel.Text = m_killLog.SolarSystem?.Constellation?.Name;
            RegionLabel.Text = m_killLog.SolarSystem?.Constellation?.Region?.Name;
        }

        /// <summary>
        /// Gets the image for the specified picture box.
        /// </summary>
        /// <param name="pictureBox">The picture box.</param>
        private async Task GetImageForAsync(PictureBox pictureBox)
        {
            Image img = await ImageService.GetImageAsync(GetImageUrl(pictureBox)).ConfigureAwait(false);
            if (img != null)
                pictureBox.Image = img;
        }

        /// <summary>
        /// Gets the image URL.
        /// </summary>
        /// <param name="pictureBox">The picture box.</param>
        /// <returns></returns>
        private Uri GetImageUrl(PictureBox pictureBox)
        {
            if (pictureBox.Equals(CharacterPictureBox))
                return ImageHelper.GetPortraitUrl(m_killLog.Victim.ID,
                    (int)EveImageSize.x128);

            if (pictureBox.Equals(ShipPictureBox))
                return ImageHelper.GetTypeRenderURL(m_killLog.Victim.ShipTypeID,
                    (int)EveImageSize.x128);

            if (pictureBox.Equals(CorpPictureBox))
                return ImageHelper.GetCorporationImageURL(m_killLog.Victim.CorporationID);

            // Picture box is for alliance
            return ImageHelper.GetAllianceImageURL(m_killLog.Victim.AllianceID);
        }

        #endregion


        #region Local Events

        /// <summary>
        /// Handles the Click event of the CopyPictureBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void CopyPictureBox_Click(object sender, EventArgs e)
        {
            KillLogExporter.CopyKillInfoToClipboard(m_killLog);
        }

        /// <summary>
        /// Handles the MouseDown event of the ShipPictureBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void ShipPictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            PictureBox pictureBox = sender as PictureBox;

            if (pictureBox == null)
                return;

            // Right click reset the cursor
            pictureBox.Cursor = Cursors.Default;

            // Display the context menu
            contextMenuStrip.Show(pictureBox, e.Location);
        }

        /// <summary>
        /// Handles the MouseMove event of the ShipPictureBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void ShipPictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            PictureBox pictureBox = sender as PictureBox;

            if (pictureBox == null)
                return;

            if ((pictureBox == ShipPictureBox) && (m_killLog.Victim.ShipTypeID == 0))
            {
                pictureBox.Cursor = Cursors.Default;
                return;
            }

            pictureBox.Cursor = CustomCursors.ContextMenu;
        }

        /// <summary>
        /// Handles the Click event of the showInShipBrowserMenuItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void showInShipBrowserMenuItem_Click(object sender, EventArgs e)
        {
            Item item = StaticItems.GetItemByID(m_killLog.Victim.ShipTypeID);

            if (item != null)
                PlanWindow.ShowPlanWindow(m_killLog.Character).ShowShipInBrowser(item);
        }

        #endregion
    }
}
