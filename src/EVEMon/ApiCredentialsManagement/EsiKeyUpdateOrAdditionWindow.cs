using EVEMon.Common;
using EVEMon.Common.Constants;
using EVEMon.Common.Controls;
using EVEMon.Common.Controls.MultiPanel;
using EVEMon.Common.CustomEventArgs;
using EVEMon.Common.Enumerations;
using EVEMon.Common.Enumerations.CCPAPI;
using EVEMon.Common.Helpers;
using EVEMon.Common.Models;
using EVEMon.Common.Properties;
using EVEMon.Common.Serialization;
using EVEMon.Common.Service;
using EVEMon.SettingsUI;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EVEMon.ApiCredentialsManagement
{
    public partial class EsiKeyUpdateOrAdditionWindow : EVEMonForm
    {
        private readonly SSOAuthenticationService m_authService;
        private readonly bool m_updateMode;
        private bool m_keyValid;
        private ESIKey m_esiKey;
        private ESIKeyCreationEventArgs m_creationArgs;
        private readonly SSOWebServerHttpListener m_server;
        private readonly string m_state;

        /// <summary>
        /// Constructor for new ESI credential.
        /// </summary>
        public EsiKeyUpdateOrAdditionWindow()
        {
            InitializeComponent();
            m_keyValid = false;
            m_server = new SSOWebServerHttpListener();
            m_state = DateTime.UtcNow.ToFileTime().ToString();
            m_authService = SSOAuthenticationService.GetInstance();
        }

        /// <summary>
        /// Constructor for editing existing ESI credentials.
        /// </summary>
        /// <param name="esiKey"></param>
        public EsiKeyUpdateOrAdditionWindow(ESIKey esiKey)
            : this()
        {
            m_esiKey = esiKey;
            m_updateMode = m_esiKey != null;
        }

        /// <summary>
        /// Starts the SSO server.
        /// </summary>
        private void StartServer()
        {
            try
            {
                m_server.Start();
                WaitForToken();
            }
            catch (IOException)
            {
                MessageBox.Show(string.Format(Properties.Resources.ErrorSSOStartup,
                    SSOWebServerHttpListener.PORT), @"Cannot start authentication", MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
            }
        }

        /// <summary>
        /// Starts waiting for a token in the background, calling UpdateTokens when it arrives
        /// or fails.
        /// </summary>
        private void WaitForToken()
        {
            m_server.BeginWaitForCode(m_state, UpdateTokens);
        }

        /// <summary>
        /// Receives the result and updates the code text box if needed.
        /// </summary>
        /// <param name="results"></param>
        private void UpdateTokens(Task<string> results)
        {
            string code;
            if (results.IsFaulted)
            {
                // Retry and log
                ExceptionHandler.LogException(results.Exception, true);
                WaitForToken();
            }
            else if (!results.IsCanceled && !string.IsNullOrEmpty(code = results.Result))
            {
                // If a token is received, use SSOAuthenticationService to convert to a token
                // null is returned if the user cancels
                m_authService.VerifyAuthCode(code, GoToResults);
                Throbber.State = ThrobberState.Rotating;
                Throbber.Visible = true;
            }
        }
        
        /// <summary>
        /// Update the controls visibility depending on whether we are in update or creation mode.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (DesignMode)
                return;

            if (m_authService == null)
            {
                MessageBox.Show(Properties.Resources.ErrorSetClientID, @"Client ID not set",
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Close();
                // General > Network...
                using (SettingsForm form = new SettingsForm(0, 1))
                {
                    form.ShowDialog(this);
                }
            }

            WarningLabel.Visible = m_updateMode;
            CharactersListView.Items.Clear();

            MultiPanel.SelectedPage = CredentialsPage;
            MultiPanel.SelectionChange += MultiPanel_SelectionChange;
            UpdateButtons(CredentialsPage);
        }

        /// <summary>
        /// When we switch panels, we update the "next", "previous" and "cancel" buttons.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void MultiPanel_SelectionChange(object sender, MultiPanelSelectionChangeEventArgs args)
        {
            UpdateButtons(args?.NewPage ?? MultiPanel.SelectedPage);
        }

        /// <summary>
        /// When we switch panels, we update the "next", "previous" and "cancel" buttons.
        /// </summary>
        /// <param name="newPage">The page selected.</param>
        private void UpdateButtons(MultiPanelPage newPage)
        {
            bool nextPrev = false;
            if (newPage == CredentialsPage)
                StartServer();
            else
            {
                nextPrev = true;
                ButtonNext.Text = m_updateMode ? "&Update" : "&Import";
                ButtonNext.Focus();
            }
            ButtonPrevious.Visible = nextPrev;
            ButtonPrevious.Enabled = nextPrev;
            ButtonNext.Visible = nextPrev;
            // Disable "Import" on error
            ButtonNext.Enabled = nextPrev && (newPage != ResultPage || m_keyValid);
            Throbber.State = ThrobberState.Stopped;
            Throbber.Visible = false;
        }

        /// <summary>
        /// Previous.
        /// When the previous button is clicked, we select the first page.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonPrevious_Click(object sender, EventArgs e)
        {
            if (MultiPanel.SelectedPage == ResultPage)
                MultiPanel.SelectedPage = CredentialsPage;
        }

        /// <summary>
        /// Cancel.
        /// Closes the window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            m_creationArgs = null;
            Close();
        }

        /// <summary>
        /// Next / Import / Update.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonNext_Click(object sender, EventArgs e)
        {
            // Only works on the last page
            if (MultiPanel.SelectedPage == ResultPage)
                Complete();
        }

        /// <summary>
        /// Goes to the results page once the key has been received from the server.
        /// </summary>
        private void GoToResults(AccessResponse response)
        {
            // Fail if an empty response is received
            bool failed = string.IsNullOrEmpty(response?.AccessToken) || string.IsNullOrEmpty(
                response?.RefreshToken);
            // If the arguments have not been invalidated since the last time...
            if (m_creationArgs != null)
                MultiPanel.SelectedPage = ResultPage;
            else if (failed)
            {
                // Error when fetching the key
                KeyPicture.Image = Resources.KeyWrong32;
                KeyLabel.Text = Properties.Resources.ErrorNoToken;
                CharactersGroupBox.Text = @"Error report";
                ResultsMultiPanel.SelectedPage = ESITokenFailedErrorPage;
                MultiPanel.SelectedPage = ResultPage;
                m_keyValid = false;
            }
            else
            {
                long newID = DateTime.UtcNow.ToFileTime();
                // Are we updating existing API key?
                if (m_updateMode)
                    m_esiKey.TryUpdateAsync(response, OnUpdated);
                else
                    ESIKey.TryAddOrUpdateAsync(newID, (ulong)Settings.APIMethodAccess, response, OnUpdated);
            }
        }

        /// <summary>
        /// Validates the operation and closes the window.
        /// </summary>
        private void Complete()
        {
            if (m_creationArgs == null)
                return;

            m_esiKey = m_creationArgs.CreateOrUpdate();
            
            // Closes the window
            Close();
        }

        /// <summary>
        /// When ESI credentials have been updated.
        /// </summary>
        /// <returns></returns>
        private void OnUpdated(object sender, ESIKeyCreationEventArgs e)
        {
            m_creationArgs = e;
            var error = e.CCPError;
            CharactersGroupBox.Text = "Characters exposed by ESI key";
            // Updates the picture and label for key type
            if (error != null)
            {
                string message = error.ErrorMessage;
                KeyPicture.Image = Resources.KeyWrong32;
                KeyLabel.Text = message;
                CharactersGroupBox.Text = "Error report";
                ResultsMultiPanel.SelectedPage = GetErrorPage(e, message);
                m_keyValid = false;
            }
            else
            {
                var id = e.Identity;
                KeyPicture.Image = Resources.DefaultCharacterImage32;
                KeyLabel.Text = "This is a character ESI key.";
                ResultsMultiPanel.SelectedPage = CharactersListPage;
                // Updates the characters list
                CharactersListView.Items.Clear();
                CharactersListView.Items.Add(new ListViewItem(id.CharacterName)
                {
                    Tag = id,
                });
                m_keyValid = true;
            }
            // Issue a warning if the ESI key has no scopes
            if (e.AccessMask == 0UL)
            {
                WarningLabel.Text = Properties.Resources.ErrorNoScopes;
                WarningLabel.Visible = true;
            }
            // Issue a warning if the ESI key does not have the basic scopes
            else if (e.AccessMask < (long)CCPAPIMethodsEnum.BasicCharacterFeatures)
            {
                WarningLabel.Text = Properties.Resources.ErrorFewScopes;
                WarningLabel.Visible = true;
            }
            else
                WarningLabel.Visible = m_updateMode;
            // Selects the last page
            MultiPanel.SelectedPage = ResultPage;
        }

        /// <summary>
        /// Gets the error page.
        /// </summary>
        /// <param name="e">The <see cref="ESIKeyCreationEventArgs"/> instance containing the
        /// event data.</param>
        /// <param name="message">The error message.</param>
        /// <returns>The error page to display.</returns>
        private MultiPanelPage GetErrorPage(ESIKeyCreationEventArgs e, string message)
        {
            if (e.CCPError.IsAuthenticationFailure)
                return AuthenticationErrorPage;

            if (e.CCPError.IsLoginDeniedByAccountStatus)
                return LoginDeniedErrorPage;
            
            GeneralErrorLabel.Text = string.Format(CultureConstants.DefaultCulture,
                GeneralErrorLabel.Text, message);
            return GeneralErrorPage;
        }

        /// <summary>
        /// On the first page, when a textbox is changed, we ensure the previously generated
        /// <see cref="ESIKeyCreationEventArgs"/> is destroyed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IDTextBox_TextChanged(object sender, EventArgs e)
        {
            m_creationArgs = null;
        }

        /// <summary>
        /// On the first page, when a textbox is changed, we ensure the previously generated
        /// <see cref="ESIKeyCreationEventArgs"/> is destroyed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VerificationCodeTextBox_TextChanged(object sender, EventArgs e)
        {
            m_creationArgs = null;
        }
        
        /// <summary>
        /// Handles the LinkClicked event of the LoginDeniedLinkLabel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.LinkLabelLinkClickedEventArgs"/>
        /// instance containing the event data.</param>
        private void LoginDeniedLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Util.OpenURL(new Uri(NetworkConstants.CCPAccountManage));
        }
        
        /// <summary>
        /// Starts a browser with the ESI login page.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event
        /// data.</param>
        private void ButtonESILogin_Click(object sender, EventArgs e)
        {
            m_authService?.SpawnBrowserForLogin(m_state, SSOWebServerHttpListener.PORT);
        }
    }
}
