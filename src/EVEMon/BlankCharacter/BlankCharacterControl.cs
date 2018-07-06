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
            Bloodline[] tags;

            switch (BlankCharacterUIHelper.Race)
            {
                case Race.Amarr:
                    lblAncestry1.Font = lblAncestry2.Font = lblAncestry3.Font = m_amarrFont;
                    tags = new Bloodline[]
                    {
                        Bloodline.Amarr, Bloodline.Ni_Kunni, Bloodline.Khanid
                    };
                    break;
                case Race.Caldari:
                    index = 3;
                    lblAncestry1.Font = lblAncestry2.Font = lblAncestry3.Font = m_caldariFont;
                    tags = new Bloodline[]
                    {
                        Bloodline.Deteis, Bloodline.Civire, Bloodline.Achura
                    };
                    break;
                case Race.Gallente:
                    index = 6;
                    tags = new Bloodline[]
                    {
                        Bloodline.Gallente, Bloodline.Intaki, Bloodline.Jin_Mei
                    };
                    break;
                case Race.Minmatar:
                    index = 9;
                    lblAncestry1.Font = lblAncestry2.Font = lblAncestry3.Font = m_minmatarFont;
                    tags = new Bloodline[]
                    {
                        Bloodline.Sebiestor, Bloodline.Brutor, Bloodline.Vherokior
                    };
                    break;
                default:
                    throw new NotImplementedException();
            }

            pbBloodline1.Image = ilBloodline.Images[index];
            pbBloodline2.Image = ilBloodline.Images[index + 1];
            pbBloodline3.Image = ilBloodline.Images[index + 2];
            pbBloodline1.Tag = tags[0];
            pbBloodline2.Tag = tags[1];
            pbBloodline3.Tag = tags[2];

            if (rbBloodline1.Checked)
                BlankCharacterUIHelper.Bloodline = tags[0];
            if (rbBloodline2.Checked)
                BlankCharacterUIHelper.Bloodline = tags[1];
            if (rbBloodline3.Checked)
                BlankCharacterUIHelper.Bloodline = tags[2];

            UpdateAncestryControl();
        }

        /// <summary>
        /// Updates the ancestry controls.
        /// </summary>
        private void UpdateAncestryControl()
        {
            int index = 0;
            Ancestry[] tags;

            switch (BlankCharacterUIHelper.Bloodline)
            {
                case Bloodline.Unknown:
                case Bloodline.Amarr:
                    tags = new Ancestry[]
                    {
                        Ancestry.Liberal_Holders, Ancestry.Wealthy_Commoners,
                        Ancestry.Religious_Reclaimers
                    };
                    break;
                case Bloodline.Ni_Kunni:
                    index = 3;
                    tags = new Ancestry[]
                    {
                        Ancestry.Free_Merchants, Ancestry.Border_Runners,
                        Ancestry.Navy_Veterans
                    };
                    break;
                case Bloodline.Khanid:
                    index = 6;
                    tags = new Ancestry[]
                    {
                        Ancestry.Cyber_Knights, Ancestry.Unionists, Ancestry.Zealots
                    };
                    break;
                case Bloodline.Deteis:
                    index = 9;
                    tags = new Ancestry[]
                    {
                        Ancestry.Merchandisers, Ancestry.Scientists, Ancestry.Tube_Child
                    };
                    break;
                case Bloodline.Civire:
                    index = 12;
                    tags = new Ancestry[]
                    {
                        Ancestry.Entrepreneurs, Ancestry.Mercs, Ancestry.Dissenters
                    };
                    break;
                case Bloodline.Achura:
                    index = 15;
                    tags = new Ancestry[]
                    {
                        Ancestry.Inventors, Ancestry.Monks, Ancestry.Stargazers
                    };
                    break;
                case Bloodline.Gallente:
                    index = 18;
                    tags = new Ancestry[]
                    {
                        Ancestry.Activists, Ancestry.Miners, Ancestry.Immigrants
                    };
                    break;
                case Bloodline.Intaki:
                    index = 21;
                    tags = new Ancestry[]
                    {
                        Ancestry.Artists, Ancestry.Diplomats, Ancestry.Reborn
                    };
                    break;
                case Bloodline.Jin_Mei:
                    index = 24;
                    tags = new Ancestry[]
                    {
                        Ancestry.Sang_Do_Caste, Ancestry.Saan_Go_Caste, Ancestry.Jing_Ko_Caste
                    };
                    break;
                case Bloodline.Sebiestor:
                    index = 27;
                    tags = new Ancestry[]
                    {
                        Ancestry.Tinkerers, Ancestry.Traders, Ancestry.Rebels
                    };
                    break;
                case Bloodline.Brutor:
                    index = 30;
                    tags = new Ancestry[]
                    {
                        Ancestry.Workers, Ancestry.Tribal_Traditionalists, Ancestry.Slave_Child
                    };
                    break;
                case Bloodline.Vherokior:
                    index = 33;
                    tags = new Ancestry[]
                    {
                        Ancestry.Drifters, Ancestry.Mystics, Ancestry.Retailers
                    };
                    break;
                default:
                    throw new NotImplementedException();
            }

            pbAncestry1.Image = ilAncestry.Images[index];
            pbAncestry2.Image = ilAncestry.Images[index + 1];
            pbAncestry3.Image = ilAncestry.Images[index + 2];
            lblAncestry1.Text = tags[0].ToString().ToUpper(CultureConstants.DefaultCulture).
                UnderscoresToSpaces();
            lblAncestry2.Text = tags[1].ToString().ToUpper(CultureConstants.DefaultCulture).
                UnderscoresToSpaces();
            lblAncestry3.Text = tags[2].ToString().ToUpper(CultureConstants.DefaultCulture).
                UnderscoresToSpaces();
            lblAncestry1.Tag = tags[0];
            lblAncestry2.Tag = tags[1];
            lblAncestry3.Tag = tags[2];

            if (rbAncestry1.Checked)
                BlankCharacterUIHelper.Ancestry = tags[0];
            if (rbAncestry2.Checked)
                BlankCharacterUIHelper.Ancestry = tags[1];
            if (rbAncestry3.Checked)
                BlankCharacterUIHelper.Ancestry = tags[2];
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
