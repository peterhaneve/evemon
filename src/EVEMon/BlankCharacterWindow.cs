using System;
using System.Windows.Forms;
using EVEMon.Common;
using EVEMon.Common.Controls;
using EVEMon.Common.Helpers;

namespace EVEMon.BlankCharacter
{
    public partial class BlankCharacterWindow : EVEMonForm
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="BlankCharacterWindow"/> class.
        /// </summary>
        public BlankCharacterWindow()
        {
            InitializeComponent();
        }


        #endregion


        #region Inherited Event Handlers

        /// <summary>
        /// Handles the Load event of the BlankCharacterWindow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void BlankCharacterWindow_Load(object sender, EventArgs e)
        {
            EveMonClient.TimerTick += EveMonClient_TimerTick;
            Disposed += OnDisposed;

            buttonOK.Text = "Save";
            buttonOK.Enabled = false;
        }

        /// <summary>
        /// Called when the instance get disposed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OnDisposed(object sender, EventArgs e)
        {
            EveMonClient.TimerTick -= EveMonClient_TimerTick;
            Disposed -= OnDisposed;
        }

        #endregion


        #region Global Event Handlers

        /// <summary>
        /// Handles the TimerTick event of the EveMonClient control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void EveMonClient_TimerTick(object sender, EventArgs e)
        {
            buttonOK.Enabled = !String.IsNullOrEmpty(BlankCharacterUIHelper.CharacterName);
            AcceptButton = buttonOK.Enabled ? buttonOK : buttonCancel;
        }

        #endregion


        #region Control Handlers

        /// <summary>
        /// Handles the Click event of the buttonOK control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private async void buttonOK_Click(object sender, EventArgs e)
        {
            // Three choices for one button
            switch (buttonOK.DialogResult)
            {
                    // Save blank character
                case DialogResult.None:
                    await BlankCharacterUIHelper.SaveAsync(OnCharacterSaved);
                    break;
                    // Add blank character
                case DialogResult.OK:
                    await BlankCharacterUIHelper.AddBlankCharacterAsync(OnCharacterImported);
                    break;
            }
        }

        /// <summary>
        /// Handles the Click event of the buttonCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        #endregion


        #region Callback Methods

        /// <summary>
        /// Called when character is saved.
        /// </summary>
        private void OnCharacterSaved()
        {
            buttonOK.Text = "Import";
            buttonOK.DialogResult = DialogResult.OK;

            // Disabling control editing
            blankCharacterControl.Enabled = false;
        }

        /// <summary>
        /// Called when character is imported.
        /// </summary>
        private void OnCharacterImported()
        {
            Close();
        }

        #endregion
    }
}