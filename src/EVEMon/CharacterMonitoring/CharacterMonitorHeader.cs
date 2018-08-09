using EVEMon.ApiCredentialsManagement;
using EVEMon.Common;
using EVEMon.Common.Constants;
using EVEMon.Common.Controls;
using EVEMon.Common.CustomEventArgs;
using EVEMon.Common.Enumerations;
using EVEMon.Common.Enumerations.CCPAPI;
using EVEMon.Common.Extensions;
using EVEMon.Common.Factories;
using EVEMon.Common.Interfaces;
using EVEMon.Common.Models;
using EVEMon.Common.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using static EVEMon.Common.Models.AccountStatus;

namespace EVEMon.CharacterMonitoring
{
    /// <summary>
    /// Implements the header component of the main character monitor user interface.
    /// </summary>
    internal sealed partial class CharacterMonitorHeader : UserControl
    {
        #region Fields

        private Character m_character;
        private Int64 m_spAtLastRedraw;
        private String m_nextCloneJumpAtLastRedraw;
        private volatile bool m_updatingLabels;

        #endregion


        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="CharacterMonitorHeader"/> class.
        /// </summary>
        public CharacterMonitorHeader()
        {
            InitializeComponent();

            // Fonts
            Font = FontFactory.GetFont("Tahoma");
            CharacterNameLabel.Font = FontFactory.GetFont("Tahoma", 11.25F, FontStyle.Bold);
            m_updatingLabels = false;
        }

        #endregion


        #region Inherited Events

        /// <summary>
        /// Occurs when control loads.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (DesignMode || this.IsDesignModeHosted())
                return;

            // Subscribe to events
            EveMonClient.CharacterUpdated += EveMonClient_CharacterUpdated;
            EveMonClient.CharacterInfoUpdated += EveMonClient_CharacterInfoUpdated;
            EveMonClient.MarketOrdersUpdated += EveMonClient_MarketOrdersUpdated;
            EveMonClient.AccountStatusUpdated += EveMonClient_AccountStatusUpdated;
            EveMonClient.ConquerableStationListUpdated += EveMonClient_ConquerableStationListUpdated;
            EveMonClient.CharacterLabelChanged += EveMonClient_CharacterLabelChanged;
            Disposed += OnDisposed;
        }

        /// <summary>
        /// Occurs when visibility changes.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            if (DesignMode || this.IsDesignModeHosted() || !Visible)
                return;

            UpdateFrequentControls();
            UpdateInfrequentControls();
        }

        /// <summary>
        /// Called when the control is disposed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OnDisposed(object sender, EventArgs e)
        {
            EveMonClient.CharacterUpdated -= EveMonClient_CharacterUpdated;
            EveMonClient.CharacterInfoUpdated -= EveMonClient_CharacterInfoUpdated;
            EveMonClient.MarketOrdersUpdated -= EveMonClient_MarketOrdersUpdated;
            EveMonClient.AccountStatusUpdated -= EveMonClient_AccountStatusUpdated;
            EveMonClient.ConquerableStationListUpdated -= EveMonClient_ConquerableStationListUpdated;
            EveMonClient.CharacterLabelChanged -= EveMonClient_CharacterLabelChanged;
        }

        #endregion


        #region Updating Methods

        /// <summary>
        /// Updates the controls whos content changes frequently.
        /// </summary>
        private void UpdateFrequentControls()
        {
            if (m_character == null)
                return;

            SuspendLayout();
            try
            {
                RefreshThrobber();

                // Only update the skill summary when the skill points change
                // or we have a timer on the next clone jump
                var totalSkillPoints = GetTotalSkillPoints();
                var nextCloneJumpAvailable = GetNextCloneJumpTime();

                if (m_spAtLastRedraw != totalSkillPoints || m_nextCloneJumpAtLastRedraw != nextCloneJumpAvailable)
                {
                    SkillSummaryLabel.Text = FormatSkillSummary();
                    RemapsCloneJumpSummaryLabel.Text = FormatRemapCloneJumpSummary();
                }

                m_spAtLastRedraw = totalSkillPoints;
                m_nextCloneJumpAtLastRedraw = nextCloneJumpAvailable;
            }
            finally
            {
                ResumeLayout(false);
            }
        }

        /// <summary>
        /// Gets the total skill points.
        /// </summary>
        /// <returns></returns>
        private long GetTotalSkillPoints()
        {
            var totalSkillPoints = m_character.SkillPoints;

            var ccpCharacter = m_character as CCPCharacter;
            var queuedSkill = ccpCharacter?.SkillQueue.FirstOrDefault();
            if (ccpCharacter != null && ccpCharacter.IsTraining &&
                queuedSkill != null && queuedSkill.SkillName == Skill.UnknownSkill.Name)
            {
                totalSkillPoints += queuedSkill.CurrentSP - queuedSkill.StartSP;
            }
            return totalSkillPoints;
        }

        /// <summary>
        /// Gets the next clone jump time.
        /// </summary>
        /// <returns></returns>
        private string GetNextCloneJumpTime()
        {
            var nextCloneJumpAvailable = m_character.JumpCloneLastJumpDate
                .AddHours(24 - m_character.Skills[DBConstants.InfomorphSynchronizingSkillID].Level);

            return nextCloneJumpAvailable > DateTime.UtcNow
                ? nextCloneJumpAvailable.ToRemainingTimeDigitalDescription(DateTimeKind.Utc)
                : "Now";
        }

        /// <summary>
        /// Updates the controls whos content changes infrequently.
        /// </summary>
        private void UpdateInfrequentControls()
        {
            if (m_character == null)
                return;

            SuspendLayout();
            try
            {
                // Safe for work implementation
                MainTableLayoutPanel.ColumnStyles[0].SizeType = Settings.UI.SafeForWork ? SizeType.Absolute : SizeType.AutoSize;
                MainTableLayoutPanel.ColumnStyles[0].Width = 0;
                CharacterPortrait.Visible = !Settings.UI.SafeForWork;

                CharacterPortrait.Character = m_character;
                CharacterNameLabel.Text = m_character.AdornedName;
                BioInfoLabel.Text = (m_character.Gender ?? "Gender") + " - " + (m_character.
                    Race ?? "Race") + " - " + (m_character.Bloodline ?? "Bloodline") + " - " +
                    (m_character.Ancestry ?? "Ancestry");
                BirthdayLabel.Text = "Birthday: " + m_character.Birthday.ToLocalTime();
                CorporationNameLabel.Text = "Corporation: " + (m_character.CorporationName ??
                    EveMonConstants.UnknownText);
                AllianceNameLabel.Text = "Alliance: " + (m_character.IsInNPCCorporation ?
                    "None" : (m_character.AllianceName ?? EveMonConstants.UnknownText));

                FormatBalance();
                FormatAttributes();
                UpdateInfoControls();
                UpdateCharacterLabel(EveMonClient.Characters.GetKnownLabels());
                UpdateAccountStatusInfo();
            }
            finally
            {
                ResumeLayout(false);
            }
        }

        /// <summary>
        /// Updates the info controls.
        /// </summary>
        private void UpdateInfoControls()
        {
            if (m_character == null)
                return;

            SuspendLayout();
            try
            {
                SecurityStatusLabel.Text = $"Security Status: {m_character.SecurityStatus:N2}";
                ActiveShipLabel.Text = m_character.GetActiveShipText();
                LocationInfoLabel.Text = $"Located in: {m_character.GetLastKnownLocationText()}";

                string dockedInfoText = m_character.GetLastKnownDockedText();
                DockedInfoLabel.Text = string.IsNullOrWhiteSpace(dockedInfoText) ? " " :
                    "Docked at: " + dockedInfoText;
            }
            finally
            {
                ResumeLayout(false);
            }
        }

        /// <summary>
        /// Updates the character label.
        /// </summary>
        private void UpdateCharacterLabel(IEnumerable<string> allLabels)
        {
            m_updatingLabels = true;
            try
            {
                // Update the character labels
                CustomLabelComboBox.Items.Clear();
                foreach (string label in allLabels)
                    CustomLabelComboBox.Items.Add(label);
                CustomLabelComboBox.Text = m_character.Label;
            }
            finally
            {
                m_updatingLabels = false;
            }
        }

        /// <summary>
        /// Updates the account status info.
        /// </summary>
        private void UpdateAccountStatusInfo()
        {
            if (m_character == null)
                return;

            CCPCharacter ccpCharacter = m_character as CCPCharacter;
            if (ccpCharacter == null)
            {
                AccountStatusTableLayoutPanel.Visible = false;
                return;
            }

            SuspendLayout();
            try
            {
                AccountActivityLabel.Text = m_character.CharacterStatus.ToString();

                switch (m_character.CharacterStatus.CurrentStatus)
                {
                    case AccountStatusType.Omega:
                        AccountActivityLabel.ForeColor = Color.DarkGreen;
                        break;
                    case AccountStatusType.Alpha:
                        AccountActivityLabel.ForeColor = SystemColors.ControlText;
                        break;
                    default:
                        AccountActivityLabel.ForeColor = Color.Red;
                        break;
                }

                // When account status is re-implemented, this will need to be shown again
                PaidUntilLabel.Text = string.Empty;
            }
            finally
            {
                ResumeLayout(false);
            }
        }

        #endregion


        #region Helper Methods

        /// <summary>
        /// Sets the character.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <value>The character.</value>
        public void SetCharacter(Character character)
        {
            m_character = character;
        }

        /// <summary>
        /// Formats the balance.
        /// </summary>
        private void FormatBalance()
        {
            if (m_character == null)
                return;

            BalanceLabel.Text = $"Balance: {m_character.Balance:N} ISK";

            CCPCharacter ccpCharacter = m_character as CCPCharacter;

            if (ccpCharacter == null)
                return;

            IQueryMonitor marketMonitor = ccpCharacter.QueryMonitors[ESIAPICharacterMethods.MarketOrders];
            if (!Settings.UI.SafeForWork && !ccpCharacter.HasSufficientBalance && marketMonitor != null && marketMonitor.Enabled)
            {
                BalanceLabel.ForeColor = Color.Orange;
                BalanceLabel.Font = FontFactory.GetFont(Font, FontStyle.Bold);
                return;
            }

            BalanceLabel.ForeColor = !Settings.UI.SafeForWork && m_character.Balance < 0 ? Color.Red : SystemColors.ControlText;
            BalanceLabel.Font = FontFactory.GetFont(Font);
        }

        /// <summary>
        /// Refreshes the throbber.
        /// </summary>
        private void RefreshThrobber()
        {
            CCPCharacter ccpCharacter = m_character as CCPCharacter;

            if (ccpCharacter == null)
            {
                HideThrobber();
                return;
            }

            if (ccpCharacter.QueryMonitors.Any(monitor => monitor.IsUpdating))
            {
                SetThrobberUpdating();
                return;
            }

            if (!NetworkMonitor.IsNetworkAvailable)
            {
                SetThrobberStrobing("Network Unavailable");
                return;
            }

            SetThrobberStopped();
            UpdateCountdown();
        }

        /// <summary>
        /// Updates the countdown.
        /// </summary>
        private void UpdateCountdown()
        {
            CCPCharacter ccpCharacter = m_character as CCPCharacter;

            if (ccpCharacter == null)
                return;

            IQueryMonitor nextMonitor = ccpCharacter.QueryMonitors.NextMonitorToBeUpdated;

            if (nextMonitor == null)
            {
                UpdateLabel.Text = String.Empty;
                return;
            }

            TimeSpan timeLeft = nextMonitor.NextUpdate.Subtract(DateTime.UtcNow);

            if (timeLeft <= TimeSpan.Zero)
            {
                UpdateLabel.Text = @"Pending...";
                return;
            }

            if (UpdateThrobber.State == ThrobberState.Rotating)
                return;

            UpdateLabel.Text = $"{Math.Floor(timeLeft.TotalHours):#00}:{timeLeft.Minutes:d2}:{timeLeft.Seconds:d2}";
        }

        /// <summary>
        /// Sets the throbber stopped.
        /// </summary>
        private void SetThrobberStopped()
        {
            UpdateThrobber.State = ThrobberState.Stopped;

            CCPCharacter ccpCharacter = m_character as CCPCharacter;

            if (ccpCharacter == null)
                return;

            if (!ccpCharacter.Identity.ESIKeys.Any() || ccpCharacter.QueryMonitors.Any(x => !x.CanForceUpdate))
            {
                ToolTip.SetToolTip(UpdateThrobber, String.Empty);
                return;
            }

            ToolTip.SetToolTip(UpdateThrobber, "Click to update now");
        }

        /// <summary>
        /// Sets the throbber strobing.
        /// </summary>
        /// <param name="status">The status.</param>
        private void SetThrobberStrobing(string status)
        {
            UpdateLabel.Text = String.Empty;
            UpdateThrobber.Visible = true;
            UpdateThrobber.State = ThrobberState.Strobing;
            ToolTip.SetToolTip(UpdateThrobber, status);
        }

        /// <summary>
        /// Sets the throbber updating.
        /// </summary>
        private void SetThrobberUpdating()
        {
            UpdateLabel.Text = String.Empty;
            UpdateThrobber.State = ThrobberState.Rotating;
            UpdateThrobber.Visible = true;
            ToolTip.SetToolTip(UpdateThrobber, "Retrieving data from API...");
        }

        /// <summary>
        /// Hides the throbber.
        /// </summary>
        private void HideThrobber()
        {
            UpdateLabel.Text = String.Empty;
            UpdateThrobber.Visible = false;
            UpdateThrobber.State = ThrobberState.Stopped;
        }

        /// <summary>
        /// Populates the attribute text for the attribute labels.
        /// </summary>
        private void FormatAttributes()
        {
            SetAttributeLabel(lblINTAttribute, EveAttribute.Intelligence);
            SetAttributeLabel(lblPERAttribute, EveAttribute.Perception);
            SetAttributeLabel(lblCHAAttribute, EveAttribute.Charisma);
            SetAttributeLabel(lblWILAttribute, EveAttribute.Willpower);
            SetAttributeLabel(lblMEMAttribute, EveAttribute.Memory);
        }

        /// <summary>
        /// Gets the update status.
        /// </summary>
        /// <returns>Status text to display in the tool tip.</returns>
        private string GetUpdateStatus()
        {
            CCPCharacter ccpCharacter = m_character as CCPCharacter;

            if (ccpCharacter == null)
                return String.Empty;

            StringBuilder output = new StringBuilder();

            // Skip character's corporation monitors if they are bound with the character's personal monitor
            foreach (IQueryMonitor monitor in ccpCharacter.QueryMonitors.OrderedByUpdateTime.Where(
                monitor => monitor.Method.HasHeader() && monitor.HasAccess).Where(
                monitor => (monitor.Method.GetType() != typeof(ESIAPICorporationMethods))))
            {
                output.AppendLine(GetStatusForMonitor(monitor));
            }

            return output.ToString();
        }

        /// <summary>
        /// Generates text representing the time to next update.
        /// </summary>
        /// <param name="monitor">The monitor.</param>
        /// <returns>String describing the time until the next update.</returns>
        private static string GenerateTimeToNextUpdateText(IQueryMonitor monitor)
        {
            TimeSpan timeToNextUpdate = monitor.NextUpdate.Subtract(DateTime.UtcNow);

            if (monitor.Status == QueryStatus.Disabled)
                return "(Disabled)";

            if (timeToNextUpdate <= TimeSpan.Zero)
                return "(Pending)";

            if (monitor.NextUpdate == DateTime.MaxValue)
                return "(Never)";

            return timeToNextUpdate.TotalMinutes >= 60
                ? $"(~{Math.Floor(timeToNextUpdate.TotalHours)}h)"
                : $"({Math.Floor(timeToNextUpdate.TotalMinutes)}m)";
        }

        /// <summary>
        /// Gets the update status for a monitor.
        /// </summary>
        /// <param name="monitor">The monitor.</param>
        /// <returns>Status text for the monitor.</returns>
        private static string GetStatusForMonitor(IQueryMonitor monitor)
        {
            StringBuilder output = new StringBuilder();

            output.Append($"{monitor}: ");

            if (monitor.Status == QueryStatus.Pending)
            {
                output.Append(GetDetailedStatusForMonitor(monitor));
                return output.ToString();
            }

            output.Append(monitor.Status.GetDescription());
            return output.ToString();
        }

        /// <summary>
        /// Gets the detailed status for monitor.
        /// </summary>
        /// <param name="monitor">The monitor.</param>
        /// <returns>Detailed status text for the monitor.</returns>
        private static string GetDetailedStatusForMonitor(IQueryMonitor monitor)
        {
            if (monitor.NextUpdate == DateTime.MaxValue)
                return "Never";

            TimeSpan remainingTime = monitor.NextUpdate.Subtract(DateTime.UtcNow);
            if (remainingTime.Minutes < 1)
                return "Less than a minute";

            return remainingTime.ToDescriptiveText(
                DescriptiveTextOptions.FullText |
                DescriptiveTextOptions.SpaceText |
                DescriptiveTextOptions.SpaceBetween, false);
        }

        /// <summary>
        /// Creates the new monitor toolstrip menu item.
        /// </summary>
        /// <param name="monitor">The monitor.</param>
        /// <returns>New menu item for a monitor.</returns>
        private static ToolStripMenuItem CreateNewMonitorToolStripMenuItem(IQueryMonitor monitor)
        {
            string menuText = $"Update {monitor} {GenerateTimeToNextUpdateText(monitor)}";

            ToolStripMenuItem menu;
            ToolStripMenuItem tempMenu = null;
            try
            {
                tempMenu = new ToolStripMenuItem(menuText)
                {
                    Tag = monitor.Method,
                    Enabled = monitor.Enabled && monitor.HasAccess && monitor.CanForceUpdate
                };

                menu = tempMenu;
                tempMenu = null;
            }
            finally
            {
                tempMenu?.Dispose();
            }
            return menu;
        }

        /// <summary>
        /// Gets the attribute text for a character.
        /// </summary>
        /// <param name="label">The attribute label.</param>
        /// <param name="eveAttribute">The eve attribute.</param>
        /// <returns>Formatted string describing the attribute and its value.</returns>
        private void SetAttributeLabel(Control label, EveAttribute eveAttribute)
        {
            label.Text = m_character[eveAttribute].EffectiveValue.ToString(CultureConstants.DefaultCulture);

            label.Tag = eveAttribute;
        }

        /// <summary>
        /// Formats the characters skill summary as a multi-line string.
        /// </summary>
        /// <returns>Formatted list of information about a characters skills.</returns>
        private string FormatSkillSummary()
        {
            StringBuilder output = new StringBuilder();

            output
                .Append($"Known Skills: {m_character.KnownSkillCount}")
                .AppendLine()
                .Append($"Skills at Level V: {m_character.GetSkillCountAtLevel(5)}")
                .AppendLine()
                .Append($"Total SP: {GetTotalSkillPoints():N0}")
                .AppendLine()
                .Append($"Free SP: {m_character.FreeSkillPoints:N0}");

            return output.ToString();
        }

        /// <summary>
        /// Formats the characters remap and clone jump summary as a multi-line string.
        /// </summary>
        /// <returns>Formatted list of information about a characters skills.</returns>
        private string FormatRemapCloneJumpSummary()
        {
            StringBuilder output = new StringBuilder();

            string remapAvailableText = m_character.LastReMapTimed.AddYears(1) > DateTime.UtcNow
                ? m_character.LastReMapTimed.AddYears(1).ToLocalTime().ToString(CultureConstants.DefaultCulture)
                : "Now";

            output
                .Append($"Bonus Remaps: {m_character.AvailableReMaps}")
                .AppendLine()
                .Append($"Next Neural Remap: {remapAvailableText}")
                .AppendLine()
                .Append($"Next Clone Jump: {GetNextCloneJumpTime()}");

            return output.ToString();
        }

        #endregion


        #region Global Events

        /// <summary>
        /// Handles the CharacterLabelChanged event of the EveMonClient  control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="LabelChangedEventArgs"/> instance containing the event data.</param>
        private void EveMonClient_CharacterLabelChanged(object sender, LabelChangedEventArgs e)
        {
            UpdateCharacterLabel(e.AllLabels);
        }
        
        /// <summary>
        /// Handles the CharacterUpdated event of the EveMonClient control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="CharacterChangedEventArgs"/> instance containing the event data.</param>
        private void EveMonClient_CharacterUpdated(object sender, CharacterChangedEventArgs e)
        {
            // No need to do this if control is not visible
            if (!Visible || e.Character != m_character)
                return;

            UpdateInfrequentControls();
        }

        /// <summary>
        /// Handles the CharacterInfoUpdated event of the EveMonClient control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="CharacterChangedEventArgs"/> instance containing the event data.</param>
        private void EveMonClient_CharacterInfoUpdated(object sender, CharacterChangedEventArgs e)
        {
            // No need to do this if control is not visible
            if (!Visible || e.Character != m_character)
                return;

            UpdateInfoControls();
        }

        /// <summary>
        /// Handles the ConquerableStationListUpdated event of the EveMonClient control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void EveMonClient_ConquerableStationListUpdated(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Handles the MarketOrdersChanged event of the EveMonClient control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="CharacterChangedEventArgs"/> instance containing the event data.</param>
        private void EveMonClient_MarketOrdersUpdated(object sender, CharacterChangedEventArgs e)
        {
            // No need to do this if control is not visible
            if (!Visible || e.Character != m_character)
                return;

            FormatBalance();
        }

        /// <summary>
        /// Handles the AccountStatusUpdated event of the EveMonClient control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void EveMonClient_AccountStatusUpdated(object sender, EventArgs e)
        {
            UpdateAccountStatusInfo();
        }

        #endregion


        #region Local Events

        /// <summary>
        /// Occurs when the user presses a key in the label box.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CustomLabelComboBox_KeyUp(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return) && !m_updatingLabels)
            {
                m_character.Label = CustomLabelComboBox.Text;
                e.Handled = true;
            }
        }

        /// <summary>
        /// Occurs when the user selects or types in a new character label for this character.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CustomLabelComboBox_TextChanged(object sender, EventArgs e)
        {
            if (!m_updatingLabels)
                m_character.Label = CustomLabelComboBox.Text;
        }
        
        /// <summary>
        /// Occurs when the user click the throbber.
        /// Query the API for or a full update when possible, or show the throbber's context menu.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
        private void UpdateThrobber_MouseDown(object sender, MouseEventArgs e)
        {
            UpdateThrobber.Cursor = Cursors.Default;

            CCPCharacter ccpCharacter = m_character as CCPCharacter;

            // This is not a CCP character, it can't be updated
            if (ccpCharacter == null)
               return;

            if (e.Button == MouseButtons.Right)
                return;

            // There has been an error in the past (Authorization, Server Error, etc.)
            // or updating now will return the same data because the cache has not expired
            // or character has no associated API key
            if (UpdateThrobber.State == ThrobberState.Strobing || !ccpCharacter.Identity.ESIKeys.Any() ||
                ccpCharacter.QueryMonitors.Any(x => !x.CanForceUpdate))
            {
                ThrobberContextMenu.Show(MousePosition);
                return;
            }

            // All checks out query everything
            ccpCharacter.QueryMonitors.QueryEverything();
        }

        /// <summary>
        /// Handles the MouseMove event of the UpdateThrobber control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs" /> instance containing the event data.</param>
        private void UpdateThrobber_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                return;

            UpdateThrobber.Cursor = CustomCursors.ContextMenu;
        }

        /// <summary>
        /// Handles the MouseHover event of the UpdateLabel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void UpdateLabel_MouseHover(object sender, EventArgs e)
        {
            ToolTip.SetToolTip(UpdateLabel, GetUpdateStatus());
        }

        /// <summary>
        /// Handles the Opening event of the ThrobberContextMenu control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
        private void ThrobberContextMenu_Opening(object sender, CancelEventArgs e)
        {
            UpdateThrobber.Cursor = Cursors.Default;

            // Remove all the items after the separator including the separator
            int separatorIndex = ThrobberContextMenu.Items.IndexOf(ThrobberSeparator);
            while (separatorIndex > -1 && separatorIndex < ThrobberContextMenu.Items.Count)
            {
                ThrobberContextMenu.Items.RemoveAt(separatorIndex);
            }

            CCPCharacter ccpCharacter = m_character as CCPCharacter;

            // Exit for non-CCP characters or no associated API key
            if (ccpCharacter == null || !ccpCharacter.Identity.ESIKeys.Any() || !ccpCharacter.QueryMonitors.Any())
            {
                QueryEverythingMenuItem.Enabled = false;
                return;
            }

            // Enables / Disables the "query everything" menu item
            QueryEverythingMenuItem.Enabled = ccpCharacter.QueryMonitors.All(x => x.HasAccess && x.CanForceUpdate);

            // Add a separator before monitor items if it doesn't exist already
            if (!ThrobberContextMenu.Items.Contains(ThrobberSeparator))
                ThrobberContextMenu.Items.Add(ThrobberSeparator);

            // Add monitor items
            // Skip character's corporation monitors if they are bound with the character's personal monitor
            foreach (ToolStripMenuItem menuItem in ccpCharacter.QueryMonitors
                .Where(monitor => monitor.Method.HasHeader() && monitor.HasAccess)
                .Where(monitor => monitor.Method.GetType() != typeof(ESIAPICorporationMethods))
                .Select(CreateNewMonitorToolStripMenuItem))
            {
                ThrobberContextMenu.Items.Add(menuItem);
            }
        }

        /// <summary>
        /// Handles the ItemClicked event of the ThrobberContextMenu control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.ToolStripItemClickedEventArgs"/> instance containing the event data.</param>
        private void ThrobberContextMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            CCPCharacter ccpCharacter = m_character as CCPCharacter;

            if (ccpCharacter == null)
                return;

            if (e.ClickedItem == QueryEverythingMenuItem)
            {
                SetThrobberUpdating();
                ccpCharacter.QueryMonitors.QueryEverything();
                return;
            }

            Enum method = e.ClickedItem.Tag as Enum;

            if (method == null)
                return;

            SetThrobberUpdating();

            foreach (IQueryMonitor monitor in ccpCharacter.QueryMonitors.Where(
                monitor => monitor.Method.ToString().Contains(method.ToString())))
            {
                ccpCharacter.QueryMonitors.Query(monitor.Method);
            }
        }

        /// <summary>
        /// Handles the MouseHover event of the SkillSummaryLabel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void SkillSummaryLabel_MouseHover(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            for (int skillLevel = 0; skillLevel <= 5; skillLevel++)
            {
                int count = m_character.GetSkillCountAtLevel(skillLevel);

                if (skillLevel > 0)
                    sb.AppendLine();

                sb.Append($"Skills at Level {skillLevel}: {count.ToString(CultureConstants.DefaultCulture).PadLeft(5)}");
            }

            ToolTip.SetToolTip((Label)sender, sb.ToString());
        }

        /// <summary>
        /// When the user hovers over one of the attribute label, we display a tooltip such as :
        /// 19.8 (7 base + 7 remap points + 4 implants)
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void AttributeLabel_MouseHover(object sender, EventArgs e)
        {
            // Retrieve the attribute from the sender
            Label attributeLabel = (Label)sender;
            EveAttribute eveAttribute = (EveAttribute)attributeLabel.Tag;

            // Format the values for the tooltip
            ICharacterAttribute attribute = m_character[eveAttribute];
            string toolTip = attribute.ToString("%e (%B base + %r remap points + %i implants)");
            ToolTip.SetToolTip(attributeLabel, toolTip);
        }

        /// <summary>
        /// Handles the Resize event of the CharacterMonitorHeader control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void CharacterMonitorHeader_Resize(object sender, EventArgs e)
        {
            Height = MainTableLayoutPanel.Height;
            MainTableLayoutPanel.Width = Width;
        }

        /// <summary>
        /// Handles the Click event of the ChangeAPIKeyInfoMenuItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void ChangeAPIKeyInfoMenuItem_Click(object sender, EventArgs e)
        {
            // This menu should be enabled only for CCP characters
            // Open the ESI keys management dialog since multiple keys can affect one character
            //WindowsFactory.ShowByTag<EsiKeyUpdateOrAdditionWindow, IEnumerable<ESIKey>>(m_character.Identity.ESIKeys);
            using (EsiKeysManagementWindow window = new EsiKeysManagementWindow())
            {
                window.ShowDialog(this);
            }
        }

        #endregion
        
    }
}
