using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using EVEMon.Common;
using EVEMon.Common.Constants;
using EVEMon.Common.Controls;
using EVEMon.Common.Factories;
using EVEMon.Common.Models;
using EVEMon.Common.Extensions;

namespace EVEMon.ApiCredentialsManagement
{
    public partial class CharacterDeletionWindow : EVEMonForm
    {
        private readonly Character m_character;
        private List<ESIKey> m_esiKeys;

        /// <summary>
        /// Constructor.
        /// </summary>
        private CharacterDeletionWindow()
        {
            InitializeComponent();
            characterToRemoveLabel.Font = FontFactory.GetFont("Tahoma", FontStyle.Bold);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="character"></param>
        public CharacterDeletionWindow(Character character)
            : this()
        {
            m_character = character;
        }

        /// <summary>
        /// Occurs when the control loads.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (DesignMode)
                return;

            esiKeysListView.Items.Clear();

            // Replaces end of text with character's name
            characterToRemoveLabel.Text = string.Format(CultureConstants.DefaultCulture,
                characterToRemoveLabel.Text, m_character.Name);

            // Find the API keys bind only to this character
            m_esiKeys = EveMonClient.ESIKeys.Select(apiKey => new
            {
                apiKey, identities = apiKey.CharacterIdentities
            }).Where(apiKey => apiKey.identities.Count() == 1 && apiKey.identities.
                Contains(m_character.Identity)).Select(apiKey => apiKey.apiKey).ToList();

            esiKeysListView.Items.AddRange(m_esiKeys.Select(key => new ListViewItem(
                key.ID.ToString(CultureConstants.DefaultCulture))).ToArray());

            // Checks whether there will be no characters left after this deletion and hide/display the relevant labels
            bool noCharactersLeft = m_esiKeys.Any() && m_character is CCPCharacter;
            noCharactersLabel.Text = string.Format(CultureConstants.DefaultCulture,
                noCharactersLabel.Text, m_esiKeys.Count.S());

            noCharactersLabel.Visible = noCharactersLeft;

            // Resize window if there is no ESI key to remove
            if (!noCharactersLeft)
                Size = new Size(Size.Width, Size.Height - esiKeysListView.Height / 2);
        }

        /// <summary>
        /// Delete button click.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deleteButton_Click(object sender, EventArgs e)
        {
            // Always clear the ESI keys since keys are locked to a character
            // Note: Keep this order of removal
            m_esiKeys.ForEach(apiKey => EveMonClient.ESIKeys.Remove(apiKey));
            EveMonClient.Characters.Remove(m_character);
            Close();
        }

        /// <summary>
        /// Closes the window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
