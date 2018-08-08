using System;
using System.Drawing;
using System.Windows.Forms;
using EVEMon.Common.Constants;
using EVEMon.Common.Enumerations;
using EVEMon.Common.Factories;
using EVEMon.Common.Helpers;
using EVEMon.Common.Extensions;

namespace EVEMon.BlankCharacter
{
    public partial class BlankCharacterControl : UserControl
    {
        /// <summary>
        /// A list of all of the bloodlines sorted by the order they appear in the picture
        /// control.
        /// </summary>
        private static readonly Bloodline[] BLOODLINES_BY_INDEX =
        {
            // Amarr 0-2
            Bloodline.Amarr, Bloodline.Ni_Kunni, Bloodline.Khanid,
            // Caldari 3-5
            Bloodline.Deteis, Bloodline.Civire, Bloodline.Achura,
            // Gallente 6-8
            Bloodline.Gallente, Bloodline.Intaki, Bloodline.Jin_Mei,
            // Minmatar 9-11
            Bloodline.Sebiestor, Bloodline.Brutor, Bloodline.Vherokior
        };
        /// <summary>
        /// A list of all the ancestries sorted by the bloodline which uses them.
        /// </summary>
        private static readonly Ancestry[] ANCESTRIES_BY_INDEX =
        {
            // Amarr
            Ancestry.Liberal_Holders, Ancestry.Wealthy_Commoners, Ancestry.Religious_Reclaimers,
            // Ni-Kunni
            Ancestry.Free_Merchants, Ancestry.Border_Runners, Ancestry.Navy_Veterans,
            // Khanid
            Ancestry.Cyber_Knights, Ancestry.Unionists, Ancestry.Zealots,
            // Deteis
            Ancestry.Merchandisers, Ancestry.Scientists, Ancestry.Tube_Child,
            // Civire
            Ancestry.Entrepreneurs, Ancestry.Mercs, Ancestry.Dissenters,
            // Achura
            Ancestry.Inventors, Ancestry.Monks, Ancestry.Stargazers,
            // Gallente
            Ancestry.Activists, Ancestry.Miners, Ancestry.Immigrants,
            // Intaki
            Ancestry.Artists, Ancestry.Diplomats, Ancestry.Reborn,
            // Jin-Mei
            Ancestry.Sang_Do_Caste, Ancestry.Saan_Go_Caste, Ancestry.Jing_Ko_Caste,
            // Sebiestor
            Ancestry.Tinkerers, Ancestry.Traders, Ancestry.Rebels,
            // Brutor
            Ancestry.Workers, Ancestry.Tribal_Traditionalists, Ancestry.Slave_Child,
            // Vherokior:
            Ancestry.Drifters, Ancestry.Mystics, Ancestry.Retailers
        };

        private Font m_amarrFont;
        private Font m_caldariFont;
        private Font m_gallenteFont;
        private Font m_minmatarFont;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlankCharacterControl"/> class.
        /// </summary>
        public BlankCharacterControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the Load event of the BlankCharacterControl.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void BlankCharacterControl_Load(object sender, EventArgs e)
        {
            ForeColor = SystemColors.GrayText;

            m_amarrFont = FontFactory.GetFont("Niagara Solid", 10f);
            m_caldariFont = FontFactory.GetFont("Impact", 8.25f);
            m_gallenteFont = FontFactory.GetFont("Arial Rounded MT Bold", 8.25f);
            m_minmatarFont = FontFactory.GetFont("Moolboran", 9f);

            BlankCharacterUIHelper.Race = Race.Amarr;

            UpdateBloodlineControl();
        }


        #region Update Methods

        /// <summary>
        /// Updates the bloodline controls.
        /// </summary>
        private void UpdateBloodlineControl()
        {
            int index = 0;
            switch (BlankCharacterUIHelper.Race)
            {
                case Race.Amarr:
                    lblAncestry1.Font = lblAncestry2.Font = lblAncestry3.Font = m_amarrFont;
                    break;
                case Race.Caldari:
                    index = 3;
                    lblAncestry1.Font = lblAncestry2.Font = lblAncestry3.Font = m_caldariFont;
                    break;
                case Race.Gallente:
                    index = 6;
                    lblAncestry1.Font = lblAncestry2.Font = lblAncestry3.Font = m_gallenteFont;
                    break;
                case Race.Minmatar:
                    index = 9;
                    lblAncestry1.Font = lblAncestry2.Font = lblAncestry3.Font = m_minmatarFont;
                    break;
                default:
                    throw new NotImplementedException();
            }

            // Calculate bloodlines based on image position (they used to match up in code,
            // but the codes were remapped to match the SDE)
            Bloodline bloodline1 = BLOODLINES_BY_INDEX[index], bloodline2 =
                BLOODLINES_BY_INDEX[index + 1], bloodline3 = BLOODLINES_BY_INDEX[index + 2];
            pbBloodline1.Image = ilBloodline.Images[index];
            pbBloodline2.Image = ilBloodline.Images[index + 1];
            pbBloodline3.Image = ilBloodline.Images[index + 2];
            pbBloodline1.Tag = bloodline1;
            pbBloodline2.Tag = bloodline2;
            pbBloodline3.Tag = bloodline3;

            if (rbBloodline1.Checked)
                BlankCharacterUIHelper.Bloodline = bloodline1;
            if (rbBloodline2.Checked)
                BlankCharacterUIHelper.Bloodline = bloodline2;
            if (rbBloodline3.Checked)
                BlankCharacterUIHelper.Bloodline = bloodline3;

            UpdateAncestryControl();
        }

        /// <summary>
        /// Updates the ancestry controls.
        /// </summary>
        private void UpdateAncestryControl()
        {
            int index = 0;

            switch (BlankCharacterUIHelper.Bloodline)
            {
                case Bloodline.Unknown:
                case Bloodline.Amarr:
                    break;
                case Bloodline.Ni_Kunni:
                    index = 3;
                    break;
                case Bloodline.Khanid:
                    index = 6;
                    break;
                case Bloodline.Deteis:
                    index = 9;
                    break;
                case Bloodline.Civire:
                    index = 12;
                    break;
                case Bloodline.Achura:
                    index = 15;
                    break;
                case Bloodline.Gallente:
                    index = 18;
                    break;
                case Bloodline.Intaki:
                    index = 21;
                    break;
                case Bloodline.Jin_Mei:
                    index = 24;
                    break;
                case Bloodline.Sebiestor:
                    index = 27;
                    break;
                case Bloodline.Brutor:
                    index = 30;
                    break;
                case Bloodline.Vherokior:
                    index = 33;
                    break;
                default:
                    throw new NotImplementedException();
            }

            // Calculate bloodlines based on image position (they used to match up in code,
            // but the codes were remapped to match the SDE)
            Ancestry ancestry1 = ANCESTRIES_BY_INDEX[index], ancestry2 = ANCESTRIES_BY_INDEX[
                index + 1], ancestry3 = ANCESTRIES_BY_INDEX[index + 2];
            pbAncestry1.Image = ilAncestry.Images[index];
            pbAncestry2.Image = ilAncestry.Images[index + 1];
            pbAncestry3.Image = ilAncestry.Images[index + 2];
            lblAncestry1.Text = ancestry1.ToString().ToUpper(CultureConstants.DefaultCulture).
                UnderscoresToSpaces();
            lblAncestry2.Text = ancestry2.ToString().ToUpper(CultureConstants.DefaultCulture).
                UnderscoresToSpaces();
            lblAncestry3.Text = ancestry3.ToString().ToUpper(CultureConstants.DefaultCulture).
                UnderscoresToSpaces();
            lblAncestry1.Tag = ancestry1;
            lblAncestry2.Tag = ancestry2;
            lblAncestry3.Tag = ancestry3;

            if (rbAncestry1.Checked)
                BlankCharacterUIHelper.Ancestry = ancestry1;
            if (rbAncestry2.Checked)
                BlankCharacterUIHelper.Ancestry = ancestry2;
            if (rbAncestry3.Checked)
                BlankCharacterUIHelper.Ancestry = ancestry3;
        }

        #endregion


        #region Event Handlers

        /// <summary>
        /// Handles the Click event of the rbAmarr control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void rbAmarr_Click(object sender, EventArgs e)
        {
            BlankCharacterUIHelper.Race = Race.Amarr;
            UpdateBloodlineControl();
        }

        /// <summary>
        /// Handles the Click event of the rbCaldari control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void rbCaldari_Click(object sender, EventArgs e)
        {
            BlankCharacterUIHelper.Race = Race.Caldari;
            UpdateBloodlineControl();
        }

        /// <summary>
        /// Handles the Click event of the rbGallente control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void rbGallente_Click(object sender, EventArgs e)
        {
            BlankCharacterUIHelper.Race = Race.Gallente;
            UpdateBloodlineControl();
        }

        /// <summary>
        /// Handles the Click event of the rbMinmatar control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void rbMinmatar_Click(object sender, EventArgs e)
        {
            BlankCharacterUIHelper.Race = Race.Minmatar;
            UpdateBloodlineControl();
        }

        /// <summary>
        /// Handles the Click event of the rbBloodline1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void rbBloodline1_Click(object sender, EventArgs e)
        {
            BlankCharacterUIHelper.Bloodline = (Bloodline)pbBloodline1.Tag;
            UpdateAncestryControl();
        }

        /// <summary>
        /// Handles the Click event of the rbBloodline2 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void rbBloodline2_Click(object sender, EventArgs e)
        {
            BlankCharacterUIHelper.Bloodline = (Bloodline)pbBloodline2.Tag;
            UpdateAncestryControl();
        }

        /// <summary>
        /// Handles the Click event of the rbBloodline3 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void rbBloodline3_Click(object sender, EventArgs e)
        {
            BlankCharacterUIHelper.Bloodline = (Bloodline)pbBloodline3.Tag;
            UpdateAncestryControl();
        }

        /// <summary>
        /// Handles the Click event of the rbAncestry1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void rbAncestry1_Click(object sender, EventArgs e)
        {
            BlankCharacterUIHelper.Ancestry = (Ancestry)lblAncestry1.Tag;
        }

        /// <summary>
        /// Handles the Click event of the rbAncestry2 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void rbAncestry2_Click(object sender, EventArgs e)
        {
            BlankCharacterUIHelper.Ancestry = (Ancestry)lblAncestry2.Tag;
        }

        /// <summary>
        /// Handles the Click event of the rbAncestry3 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void rbAncestry3_Click(object sender, EventArgs e)
        {
            BlankCharacterUIHelper.Ancestry = (Ancestry)lblAncestry3.Tag;
        }

        /// <summary>
        /// Handles the Click event of the rbFemale control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void rbFemale_Click(object sender, EventArgs e)
        {
            BlankCharacterUIHelper.Gender = Gender.Female;
        }

        /// <summary>
        /// Handles the Click event of the rbMale control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void rbMale_Click(object sender, EventArgs e)
        {
            BlankCharacterUIHelper.Gender = Gender.Male;
        }

        /// <summary>
        /// Handles the TextChanged event of the tbCharacterName control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void tbCharacterName_TextChanged(object sender, EventArgs e)
        {
            BlankCharacterUIHelper.CharacterName = tbCharacterName.Text;
        }

        #endregion

    }
}
