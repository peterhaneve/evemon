using EVEMon.Common;
using EVEMon.Common.CloudStorageServices;
using EVEMon.Common.Constants;
using EVEMon.Common.Controls;
using EVEMon.Common.Controls.MultiPanel;
using EVEMon.Common.Enumerations.UISettings;
using EVEMon.Common.Extensions;
using EVEMon.Common.Factories;
using EVEMon.Common.Helpers;
using EVEMon.Common.MarketPricer;
using EVEMon.Common.Models.Comparers;
using EVEMon.Common.Resources.Skill_Select;
using EVEMon.Common.Serialization.Settings;
using EVEMon.Common.SettingsObjects;
using Microsoft.Win32;
using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Resources;
using System.Security;
using System.Windows.Forms;

namespace EVEMon.SettingsUI
{
    public partial class SettingsForm : EVEMonForm
    {
        private const string StartupRegistryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

        private readonly SerializableSettings m_settings;
        private SerializableSettings m_oldSettings;
        private bool m_isLoading;
        private TreeNode m_preSelect;


        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        public SettingsForm()
        {
            InitializeComponent();

            treeView.Font = FontFactory.GetFont("Tahoma", 9.75F);
            alwaysAskRadioButton.Font = FontFactory.GetFont("Tahoma", 8.25F, FontStyle.Bold);
            removeAllRadioButton.Font = FontFactory.GetFont("Tahoma", 8.25F, FontStyle.Bold);
            removeConfirmedRadioButton.Font = FontFactory.GetFont("Tahoma", 8.25F, FontStyle.Bold);
            settingsFileStorageControl.Font = FontFactory.GetFont("Tahoma", 8.25F);

            m_settings = Settings.Export();
            m_oldSettings = Settings.Export();
            m_preSelect = null;
        }

        /// <summary>
        /// Constructor to jump to a specific page on load.
        /// </summary>
        /// <param name="parentIndex">The index of the section to select.</param>
        /// <param name="childIndex">The index of the page in that section to select.</param>
        public SettingsForm(int parentIndex, int childIndex) : this()
        {
            var allNodes = treeView.Nodes;
            if (parentIndex < allNodes.Count && parentIndex >= 0)
            {
                // Ensure all indexes are in bounds
                var parent = allNodes[parentIndex];
                var nodes = parent.Nodes;
                if (nodes == null || nodes.Count < 1)
                    m_preSelect = parent;
                else if (childIndex >= 0 && childIndex < nodes.Count)
                    m_preSelect = nodes[childIndex];
            }
        }

        #endregion


        /// <summary>
        /// Gets a value indicating whether the settings have changed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the settings have changed; otherwise, <c>false</c>.
        /// </value>
        private bool SettingsChanged
        {
            get
            {
                var comparer = new SerializableSettingsComparer();
                return !comparer.Equals(m_settings, m_oldSettings);
            }
        }


        #region Inherited Events

        /// <summary>
        /// Occurs on form load, we update the controls values with the settings we retrieved.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingsForm_Load(object sender, EventArgs e)
        {
            if (DesignMode || this.IsDesignModeHosted())
                return;

            // Initialize members
            m_isLoading = true;

            // Platform is Unix ?
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                runAtStartupComboBox.Enabled = false;
                treeView.Nodes["trayIconNode"].Remove();
            }

            // Run with Mono ?
            if (Type.GetType("Mono.Runtime") != null)
                treeView.Nodes["generalNode"].Nodes["g15Node"].Remove();

            // Fill the overview portraits sizes
            overviewPortraitSizeComboBox.Items.AddRange(Enum.GetValues(typeof(PortraitSizes)).
                Cast<PortraitSizes>().Select(portraitSize =>
                {
                    string size = FormattableString.Invariant($"{portraitSize.GetDefaultValue()}");
                    return $"{size} by {size}";
                }).ToArray<object>());

            // Expands the left panel and selects the correct page and node
            treeView.ExpandAll();
            var node = m_preSelect ?? treeView.Nodes.Cast<TreeNode>().First();
            if (node != null)
            {
                string tag = node.Tag?.ToString() ?? string.Empty;
                treeView.SelectedNode = node;
                multiPanel.SelectedPage = multiPanel.Controls.Cast<MultiPanelPage>().
                    FirstOrDefault(page => page.Name == tag);
            }

            // Misc settings
            cbWorksafeMode.Checked = m_settings.UI.SafeForWork;
            compatibilityCombo.SelectedIndex = (int)m_settings.Compatibility;

            // Skills icon sets
            cbSkillIconSet.Items.Clear();
            for (int i = 1; i < IconSettings.Default.Properties.Count; i++)
            {
                SettingsProperty iconSettingsProperty = IconSettings.Default.Properties["Group" + i];
                if (iconSettingsProperty != null)
                    cbSkillIconSet.Items.Add(iconSettingsProperty.DefaultValue.ToString().Replace("_", " "));
            }

            // Tray icon settings
            SetTrayIconSettings();

            // G15
            SetG15Settings();

            // Skills display on the main window
            cbShowAllPublicSkills.Checked = m_settings.UI.MainWindow.ShowAllPublicSkills;
            cbShowNonPublicSkills.Checked = m_settings.UI.MainWindow.ShowNonPublicSkills;

            // Main window
            SetMainWindowSettings();

            // Main Window - Overview
            SetOverviewSettings();

            // Notifications
            notificationsControl.Settings = m_settings.Notifications;
            cbPlaySoundOnSkillComplete.Checked = m_settings.Notifications.PlaySoundOnSkillCompletion;

            // Email Notifications
            emailNotificationsControl.Settings = m_settings.Notifications;
            mailNotificationCheckBox.Checked = m_settings.Notifications.SendMailAlert;

            // Proxy settings
            customProxyCheckBox.Checked = m_settings.Proxy.Enabled;
            proxyPortTextBox.Text = m_settings.Proxy.Port.ToString(CultureConstants.DefaultCulture);
            proxyHttpHostTextBox.Text = m_settings.Proxy.Host;
            proxyAuthenticationButton.Tag = m_settings.Proxy;

            // Client ID / Secret / Scopes
            clientIDTextBox.Text = m_settings.SSOClientID;
            clientSecretTextBox.Text = m_settings.SSOClientSecret;
            scopesTextBox.Text = m_settings.SSOScopes;

            // Updates
            cbCheckTime.Checked = m_settings.Updates.CheckTimeOnStartup;
            cbCheckForUpdates.Checked = m_settings.Updates.CheckEVEMonVersion;
            updateSettingsControl.Settings = m_settings.Updates;

            // Skill Planner
            SetSkillPlannerSettings();

            // Obsolete plan entry removal behaviour
            alwaysAskRadioButton.Checked = m_settings.UI.PlanWindow.ObsoleteEntryRemovalBehaviour ==
                ObsoleteEntryRemovalBehaviour.AlwaysAsk;
            removeAllRadioButton.Checked = m_settings.UI.PlanWindow.ObsoleteEntryRemovalBehaviour ==
                ObsoleteEntryRemovalBehaviour.RemoveAll;
            removeConfirmedRadioButton.Checked = m_settings.UI.PlanWindow.ObsoleteEntryRemovalBehaviour ==
                ObsoleteEntryRemovalBehaviour.RemoveConfirmed;

            // Skill Browser Icon Set
            cbSkillIconSet.SelectedIndex = (m_settings.UI.SkillBrowser.IconsGroupIndex <=
                cbSkillIconSet.Items.Count && m_settings.UI.SkillBrowser.IconsGroupIndex > 0) ?
                (m_settings.UI.SkillBrowser.IconsGroupIndex - 1) : 0;

            // System tray popup/tooltip
            trayPopupRadio.Checked = m_settings.UI.SystemTrayPopup.Style == TrayPopupStyles.PopupForm;
            trayTooltipRadio.Checked = m_settings.UI.SystemTrayPopup.Style == TrayPopupStyles.WindowsTooltip;
            trayPopupDisabledRadio.Checked = m_settings.UI.SystemTrayPopup.Style == TrayPopupStyles.Disabled;

            // Calendar
            SetCalendarSettings();

            // External calendar
            SetExternalCalendarSettings();

            // Run at system startup
            SetStartUpSettings();

            // Market Price providers
            InitilizeMarketPriceProviderDropDown();

            // Cloud Storage Service provider
            InitializeCloudStorageServiceProviderDropDown();

            m_isLoading = false;

            // Enables / disables controls
            UpdateDisables();
        }

        /// <summary>
        /// Occurs when the user click "Cancel".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Occurs when the user click "OK".
        /// We set up the new settings if they have changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnOk_Click(object sender, EventArgs e)
        {
            // Return settings
            ApplyToSettings();

            if (SettingsChanged)
                await Settings.ImportAsync(m_settings, true);

            Close();
        }

        /// <summary>
        /// Occurs when the user click "Apply".
        /// We set up the new settings if they have changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void applyButton_Click(object sender, EventArgs e)
        {
            ApplyToSettings();

            if (!SettingsChanged)
                return;

            // Import the new settings
            await Settings.ImportAsync(m_settings, true);

            // Refresh the old settings
            m_oldSettings = Settings.Export();
        }

        #endregion


        #region Core methods

        /// <summary>
        /// Sets the tray icon settings.
        /// </summary>
        private void SetTrayIconSettings()
        {
            rbSystemTrayOptionsNever.Checked = m_settings.UI.SystemTrayIcon == SystemTrayBehaviour.Disabled;
            rbSystemTrayOptionsAlways.Checked = m_settings.UI.SystemTrayIcon == SystemTrayBehaviour.AlwaysVisible;
            rbSystemTrayOptionsMinimized.Checked = m_settings.UI.SystemTrayIcon == SystemTrayBehaviour.ShowWhenMinimized;

            switch (m_settings.UI.MainWindowCloseBehaviour)
            {
                case CloseBehaviour.MinimizeToTaskbar:
                    rbMinToTaskBar.Checked = true;
                    break;
                case CloseBehaviour.MinimizeToTray:
                    rbMinToTray.Checked = true;
                    break;
                default:
                    rbExitEVEMon.Checked = true;
                    break;
            }
        }

        /// <summary>
        /// Sets the G15 settings.
        /// </summary>
        private void SetG15Settings()
        {
            g15CheckBox.Checked = m_settings.G15.Enabled;
            cbG15ACycle.Checked = m_settings.G15.UseCharactersCycle;
            ACycleInterval.Value = m_settings.G15.CharactersCycleInterval;
            cbG15CycleTimes.Checked = m_settings.G15.UseTimeFormatsCycle;
            ACycleTimesInterval.Value = Math.Min(m_settings.G15.TimeFormatsCycleInterval, ACycleTimesInterval.Maximum);
            cbG15ShowTime.Checked = m_settings.G15.ShowSystemTime;
            cbG15ShowEVETime.Checked = m_settings.G15.ShowEVETime;
        }

        /// <summary>
        /// Sets the main window settings.
        /// </summary>
        private void SetMainWindowSettings()
        {
            cbTitleToTime.Checked = m_settings.UI.MainWindow.ShowCharacterInfoInTitleBar;
            cbWindowsTitleList.SelectedIndex = (int)m_settings.UI.MainWindow.TitleFormat - 1;
            cbSkillInTitle.Checked = m_settings.UI.MainWindow.ShowSkillNameInWindowTitle;
            cbShowPrereqMetSkills.Checked = m_settings.UI.MainWindow.ShowPrereqMetSkills;
            cbColorPartialSkills.Checked = m_settings.UI.MainWindow.HighlightPartialSkills;
            cbColorQueuedSkills.Checked = m_settings.UI.MainWindow.HighlightQueuedSkills;
            cbAlwaysShowSkillQueueTime.Checked = m_settings.UI.MainWindow.AlwaysShowSkillQueueTime;
            nudSkillQueueWarningThresholdDays.Value = m_settings.UI.MainWindow.SkillQueueWarningThresholdDays;
        }

        /// <summary>
        /// Sets the overview settings.
        /// </summary>
        private void SetOverviewSettings()
        {
            cbShowOverViewTab.Checked = m_settings.UI.MainWindow.ShowOverview;
            cbUseIncreasedContrastOnOverview.Checked = m_settings.UI.MainWindow.UseIncreasedContrastOnOverview;
            overviewShowWalletCheckBox.Checked = m_settings.UI.MainWindow.ShowOverviewWallet;
            cbShowSkillpointsOnOverview.Checked = m_settings.UI.MainWindow.ShowOverviewTotalSkillpoints;
            overviewShowPortraitCheckBox.Checked = m_settings.UI.MainWindow.ShowOverviewPortrait;
            overviewPortraitSizeComboBox.SelectedIndex = (int)m_settings.UI.MainWindow.OverviewItemSize;
            overviewShowSkillQueueTrainingTimeCheckBox.Checked = m_settings.UI.MainWindow.ShowOverviewSkillQueueTrainingTime;
            overviewGroupCharactersInTrainingCheckBox.Checked = m_settings.UI.MainWindow.PutTrainingSkillsFirstOnOverview;
        }

        /// <summary>
        /// Sets the skill planner settings.
        /// </summary>
        private void SetSkillPlannerSettings()
        {
            cbHighlightPlannedSkills.Checked = m_settings.UI.PlanWindow.HighlightPlannedSkills;
            cbHighlightPrerequisites.Checked = m_settings.UI.PlanWindow.HighlightPrerequisites;
            cbHighlightConflicts.Checked = m_settings.UI.PlanWindow.HighlightConflicts;
            cbHighlightPartialSkills.Checked = m_settings.UI.PlanWindow.HighlightPartialSkills;
            cbHighlightQueuedSiklls.Checked = m_settings.UI.PlanWindow.HighlightQueuedSkills;
            cbSummaryOnMultiSelectOnly.Checked = m_settings.UI.PlanWindow.OnlyShowSelectionSummaryOnMultiSelect;
            cbAdvanceEntryAdd.Checked = m_settings.UI.PlanWindow.UseAdvanceEntryAddition;
        }

        /// <summary>
        /// Sets the calendar settings.
        /// </summary>
        private void SetCalendarSettings()
        {
            panelColorBlocking.BackColor = (Color)m_settings.UI.Scheduler.BlockingColor;
            panelColorRecurring1.BackColor = (Color)m_settings.UI.Scheduler.RecurringEventGradientStart;
            panelColorRecurring2.BackColor = (Color)m_settings.UI.Scheduler.RecurringEventGradientEnd;
            panelColorSingle1.BackColor = (Color)m_settings.UI.Scheduler.SimpleEventGradientStart;
            panelColorSingle2.BackColor = (Color)m_settings.UI.Scheduler.SimpleEventGradientEnd;
            panelColorText.BackColor = (Color)m_settings.UI.Scheduler.TextColor;
        }

        /// <summary>
        /// Sets the external calendar settings.
        /// </summary>
        private void SetExternalCalendarSettings()
        {
            externalCalendarCheckbox.Checked = m_settings.Calendar.Enabled;

            externalCalendarControl.SetExternalCalendar(m_settings);
        }

        /// <summary>
        /// Sets the start up settings.
        /// </summary>
        private void SetStartUpSettings()
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.CurrentUser.OpenSubKey(StartupRegistryKey, true);
            }
            catch (SecurityException ex)
            {
                ExceptionHandler.LogException(ex, true);
            }
            catch (UnauthorizedAccessException ex)
            {
                ExceptionHandler.LogException(ex, true);
            }

            if (rk == null)
            {
                // No writing rights
                runAtStartupComboBox.Checked = false;
                runAtStartupComboBox.Enabled = false;
            }
            else
            {
                // Run at startup ?
                runAtStartupComboBox.Checked = rk.GetValue("EVEMon") != null;
            }
        }

        /// <summary>
        /// Fetches the controls' values to <see cref="m_settings"/>.
        /// </summary>
        private void ApplyToSettings()
        {
            // General - Compatibility
            m_settings.Compatibility = (CompatibilityMode)Math.Max(0, compatibilityCombo.SelectedIndex);
            m_settings.UI.SafeForWork = cbWorksafeMode.Checked;

            // Skill Planner
            m_settings.UI.PlanWindow.HighlightPrerequisites = cbHighlightPrerequisites.Checked;
            m_settings.UI.PlanWindow.HighlightPlannedSkills = cbHighlightPlannedSkills.Checked;
            m_settings.UI.PlanWindow.HighlightConflicts = cbHighlightConflicts.Checked;
            m_settings.UI.PlanWindow.HighlightPartialSkills = cbHighlightPartialSkills.Checked;
            m_settings.UI.PlanWindow.HighlightQueuedSkills = cbHighlightQueuedSiklls.Checked;
            m_settings.UI.PlanWindow.OnlyShowSelectionSummaryOnMultiSelect = cbSummaryOnMultiSelectOnly.Checked;
            m_settings.UI.PlanWindow.UseAdvanceEntryAddition = cbAdvanceEntryAdd.Checked;

            if (alwaysAskRadioButton.Checked)
                m_settings.UI.PlanWindow.ObsoleteEntryRemovalBehaviour = ObsoleteEntryRemovalBehaviour.AlwaysAsk;
            else if (removeAllRadioButton.Checked)
                m_settings.UI.PlanWindow.ObsoleteEntryRemovalBehaviour = ObsoleteEntryRemovalBehaviour.RemoveAll;
            else
                m_settings.UI.PlanWindow.ObsoleteEntryRemovalBehaviour = ObsoleteEntryRemovalBehaviour.RemoveConfirmed;

            // Skill Browser icon sets
            m_settings.UI.SkillBrowser.IconsGroupIndex = cbSkillIconSet.SelectedIndex + 1;

            // Main window skills filter
            m_settings.UI.MainWindow.ShowAllPublicSkills = cbShowAllPublicSkills.Checked;
            m_settings.UI.MainWindow.ShowNonPublicSkills = cbShowNonPublicSkills.Checked;
            m_settings.UI.MainWindow.ShowPrereqMetSkills = cbShowPrereqMetSkills.Checked;

            // System tray icon behaviour
            if (rbSystemTrayOptionsNever.Checked)
                m_settings.UI.SystemTrayIcon = SystemTrayBehaviour.Disabled;
            else if (rbSystemTrayOptionsMinimized.Checked)
                m_settings.UI.SystemTrayIcon = SystemTrayBehaviour.ShowWhenMinimized;
            else if (rbSystemTrayOptionsAlways.Checked)
                m_settings.UI.SystemTrayIcon = SystemTrayBehaviour.AlwaysVisible;

            // Main window close behaviour
            if (rbMinToTaskBar.Checked)
                m_settings.UI.MainWindowCloseBehaviour = CloseBehaviour.MinimizeToTaskbar;
            else if (rbMinToTray.Checked)
                m_settings.UI.MainWindowCloseBehaviour = CloseBehaviour.MinimizeToTray;
            else
                m_settings.UI.MainWindowCloseBehaviour = CloseBehaviour.Exit;

            // Market Price Provider
            m_settings.MarketPricer.ProviderName = cbProvidersList.SelectedItem?.ToString() ?? String.Empty;

            // Cloud Storage Service Provider
            m_settings.CloudStorageServiceProvider.ProviderName =
                cloudStorageProvidersComboBox.SelectedItem?.ToString() ?? String.Empty;

            // Main window
            m_settings.UI.MainWindow.ShowCharacterInfoInTitleBar = cbTitleToTime.Checked;
            m_settings.UI.MainWindow.TitleFormat = (MainWindowTitleFormat)cbWindowsTitleList.SelectedIndex + 1;
            m_settings.UI.MainWindow.ShowSkillNameInWindowTitle = cbSkillInTitle.Checked;
            m_settings.UI.MainWindow.HighlightPartialSkills = cbColorPartialSkills.Checked;
            m_settings.UI.MainWindow.HighlightQueuedSkills = cbColorQueuedSkills.Checked;
            m_settings.UI.MainWindow.AlwaysShowSkillQueueTime = cbAlwaysShowSkillQueueTime.Checked;
            m_settings.UI.MainWindow.SkillQueueWarningThresholdDays = (int)nudSkillQueueWarningThresholdDays.Value;

            // G15
            m_settings.G15.Enabled = g15CheckBox.Checked;
            m_settings.G15.UseCharactersCycle = cbG15ACycle.Checked;
            m_settings.G15.CharactersCycleInterval = (int)ACycleInterval.Value;
            m_settings.G15.UseTimeFormatsCycle = cbG15CycleTimes.Checked;
            m_settings.G15.TimeFormatsCycleInterval = (int)ACycleTimesInterval.Value;
            m_settings.G15.ShowSystemTime = cbG15ShowTime.Checked;
            m_settings.G15.ShowEVETime = cbG15ShowEVETime.Checked;

            // Notifications
            m_settings.Notifications.PlaySoundOnSkillCompletion = cbPlaySoundOnSkillComplete.Checked;
            m_settings.Notifications.SendMailAlert = mailNotificationCheckBox.Checked;

            // Email notifications
            // If enabled, validate email notification settings
            if (mailNotificationCheckBox.Checked && emailNotificationsControl.ValidateChildren())
                emailNotificationsControl.PopulateSettingsFromControls();

            // Main window - Overview
            m_settings.UI.MainWindow.ShowOverview = cbShowOverViewTab.Checked;
            m_settings.UI.MainWindow.UseIncreasedContrastOnOverview = cbUseIncreasedContrastOnOverview.Checked;
            m_settings.UI.MainWindow.ShowOverviewWallet = overviewShowWalletCheckBox.Checked;
            m_settings.UI.MainWindow.ShowOverviewTotalSkillpoints = cbShowSkillpointsOnOverview.Checked;
            m_settings.UI.MainWindow.ShowOverviewPortrait = overviewShowPortraitCheckBox.Checked;
            m_settings.UI.MainWindow.PutTrainingSkillsFirstOnOverview = overviewGroupCharactersInTrainingCheckBox.Checked;
            m_settings.UI.MainWindow.ShowOverviewSkillQueueTrainingTime = overviewShowSkillQueueTrainingTimeCheckBox.Checked;
            m_settings.UI.MainWindow.OverviewItemSize = (PortraitSizes)overviewPortraitSizeComboBox.SelectedIndex;

            // Tray icon window style
            if (trayPopupRadio.Checked)
                m_settings.UI.SystemTrayPopup.Style = TrayPopupStyles.PopupForm;
            else if (trayTooltipRadio.Checked)
                m_settings.UI.SystemTrayPopup.Style = TrayPopupStyles.WindowsTooltip;
            else
                m_settings.UI.SystemTrayPopup.Style = TrayPopupStyles.Disabled;

            // Proxy
            m_settings.Proxy.Enabled = customProxyCheckBox.Checked;
            int proxyPort;
            if (Int32.TryParse(proxyPortTextBox.Text, out proxyPort))
                m_settings.Proxy.Port = proxyPort;
            m_settings.Proxy.Host = proxyHttpHostTextBox.Text;

            // Client ID / Secret / Scopes
            m_settings.SSOClientID = (clientIDTextBox.Text ?? string.Empty).Trim();
            m_settings.SSOClientSecret = (clientSecretTextBox.Text ?? string.Empty).Trim();
            m_settings.SSOScopes = (scopesTextBox.Text ?? string.Empty).Trim();

            // Updates
            m_settings.Updates.CheckEVEMonVersion = cbCheckForUpdates.Checked;
            m_settings.Updates.CheckTimeOnStartup = cbCheckTime.Checked;

            // Scheduler colors
            m_settings.UI.Scheduler.BlockingColor = (SerializableColor)panelColorBlocking.BackColor;
            m_settings.UI.Scheduler.RecurringEventGradientStart = (SerializableColor)panelColorRecurring1.BackColor;
            m_settings.UI.Scheduler.RecurringEventGradientEnd = (SerializableColor)panelColorRecurring2.BackColor;
            m_settings.UI.Scheduler.SimpleEventGradientStart = (SerializableColor)panelColorSingle1.BackColor;
            m_settings.UI.Scheduler.SimpleEventGradientEnd = (SerializableColor)panelColorSingle2.BackColor;
            m_settings.UI.Scheduler.TextColor = (SerializableColor)panelColorText.BackColor;

            // External calendar settings
            m_settings.Calendar.Enabled = externalCalendarCheckbox.Checked;
            externalCalendarControl.ApplyExternalCalendarSettings(m_settings);
            
            // Run at startup
            if (!runAtStartupComboBox.Enabled)
                return;

            RegistryKey rk = Registry.CurrentUser.OpenSubKey(StartupRegistryKey, true);
            if (rk == null)
                return;

            if (runAtStartupComboBox.Checked)
            {
                rk.SetValue("EVEMon", $"\"{Application.ExecutablePath}\" {"-startMinimized"}");
            }
            else
                rk.DeleteValue("EVEMon", false);
        }
        
        /// <summary>
        /// Populates the combobox for the market price providers.
        /// </summary>
        private void InitilizeMarketPriceProviderDropDown()
        {
            cbProvidersList.Items.Clear();

            cbProvidersList.Items.AddRange(ItemPricer.Providers
                .Select(pricer => pricer.Name)
                .Cast<object>()
                .ToArray());

            var selectedItem = cbProvidersList.Items.Cast<string>()
                .FirstOrDefault(item => item == m_settings.MarketPricer.ProviderName);

            if (selectedItem != null)
                cbProvidersList.SelectedIndex = cbProvidersList.Items.IndexOf(selectedItem);

            if (cbProvidersList.SelectedIndex == -1 && cbProvidersList.Items.Count > 0)
                cbProvidersList.SelectedIndex = 0;
        }

        /// <summary>
        /// Populates the combobox for the cloud storage service providers.
        /// </summary>
        private void InitializeCloudStorageServiceProviderDropDown()
        {
            cloudStorageProvidersComboBox.Items.Clear();

            cloudStorageProvidersComboBox.Items.AddRange(CloudStorageServiceProvider.Providers
                .Select(provider => provider.Name)
                .Cast<object>()
                .ToArray());

            var selectedItem = cloudStorageProvidersComboBox.Items.Cast<string>()
                .FirstOrDefault(item => item == m_settings.CloudStorageServiceProvider.ProviderName);

            if (selectedItem != null)
                cloudStorageProvidersComboBox.SelectedIndex = cloudStorageProvidersComboBox.Items.IndexOf(selectedItem);

            if (cloudStorageProvidersComboBox.SelectedIndex == -1 && cloudStorageProvidersComboBox.Items.Count > 0)
                cloudStorageProvidersComboBox.SelectedIndex = 0;

            cloudStorageProviderLogoPictureBox.Image = m_settings.CloudStorageServiceProvider.Provider?.Logo;
        }

        #endregion


        #region Validation

        /// <summary>
        /// Proxy port validation.
        /// Ensures the text represents a correct port number.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void proxyPortTextBox_Validating(object sender, CancelEventArgs e)
        {
            string text = ((TextBox)sender).Text;
            e.Cancel = !IsValidPort(text, "Proxy port");
        }

        /// <summary>
        /// Checks a port is valid and displays a message box when it is not.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="portName"></param>
        /// <returns></returns>
        internal static bool IsValidPort(string str, string portName)
        {
            int port;
            if (!Int32.TryParse(str, out port))
                return false;

            if ((port >= IPEndPoint.MinPort) && (port <= IPEndPoint.MaxPort))
                return true;

            ShowErrorMessage("Invalid port",
                $"{portName} value must be between {IPEndPoint.MinPort} and {IPEndPoint.MaxPort}.");

            return false;
        }

        /// <summary>
        /// Displays an error message.
        /// </summary>
        /// <param name="caption"></param>
        /// <param name="message"></param>
        private static void ShowErrorMessage(string caption, string message)
        {
            MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        #endregion


        #region Updates

        /// <summary>
        /// This handler occurs because some controls' values changed and requires to enable/disable other controls.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMustEnableOrDisable(object sender, EventArgs e)
        {
            if (m_isLoading)
                return;

            UpdateDisables();
        }

        /// <summary>
        /// Enable or disable controls in reaction to other controls states. 
        /// </summary>
        private void UpdateDisables()
        {
            g15Panel.Enabled = g15CheckBox.Checked;
            ACycleInterval.Enabled = cbG15ACycle.Checked;
            ACycleTimesInterval.Enabled = cbG15CycleTimes.Checked;
            trayIconPopupGroupBox.Enabled = !rbSystemTrayOptionsNever.Checked;
            emailNotificationsControl.Enabled = mailNotificationCheckBox.Checked;
            customProxyPanel.Enabled = customProxyCheckBox.Checked;
            overviewPanel.Enabled = cbShowOverViewTab.Checked;

            cbWindowsTitleList.Enabled = cbTitleToTime.Checked;
            cbSkillInTitle.Enabled = cbTitleToTime.Checked;

            // Portable Eve Clients settings
            portableEveClientsControl.Enabled = !EveMonClient.EveAppDataFoldersExistInDefaultLocation;

            // Minimize to tray/task bar
            rbMinToTray.Enabled = !rbSystemTrayOptionsNever.Checked;
            if (rbSystemTrayOptionsNever.Checked && rbMinToTray.Checked)
                rbMinToTaskBar.Checked = true;

            // Calendar
            externalCalendarControl.Enabled = externalCalendarCheckbox.Checked;

            // Main window filters (show non-public skills and such)
            if (cbShowAllPublicSkills.Checked)
            {
                cbShowNonPublicSkills.Enabled = true;
                cbShowNonPublicSkills.Checked = m_settings.UI.MainWindow.ShowNonPublicSkills;
                cbShowPrereqMetSkills.Enabled = false;
                cbShowPrereqMetSkills.Checked = false;
            }
            else
            {
                cbShowNonPublicSkills.Enabled = false;
                cbShowNonPublicSkills.Checked = false;
                cbShowPrereqMetSkills.Enabled = true;
                cbShowPrereqMetSkills.Checked = m_settings.UI.MainWindow.ShowPrereqMetSkills;
            }

            // Cloud Storage Service Provider Authentiation
            providerAuthenticationGroupBox.Visible = CloudStorageServiceProvider.Providers.Any();
        }

        #endregion


        #region Buttons handlers

        /// <summary>
        /// Network > Proxy > Authentication button.
        /// Shows the proxy authentication configuration form.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void proxyAuthenticationButton_Click(object sender, EventArgs e)
        {
            ProxySettings proxySettings = m_settings.Proxy;
            using (ProxyAuthenticationWindow window = new ProxyAuthenticationWindow(proxySettings))
            {
                DialogResult result = window.ShowDialog();
                if (result == DialogResult.OK)
                    m_settings.Proxy = proxySettings;
            }
        }

        /// <summary>
        /// Tray icon tooltip > Configure.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void trayTooltipButton_Click(object sender, EventArgs e)
        {
            TrayTooltipSettings tooltipSettings = m_settings.UI.SystemTrayTooltip;
            using (TrayTooltipConfigForm f = new TrayTooltipConfigForm(tooltipSettings))
            {
                // Set current tooltip string
                f.ShowDialog();
                if (f.DialogResult != DialogResult.OK)
                    return;

                // Save changes in local copy
                m_settings.UI.SystemTrayTooltip = tooltipSettings;
                trayTooltipRadio.Checked = true;
            }
        }

        /// <summary>
        /// Tray icon popup > Configure.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void trayPopupButton_Click(object sender, EventArgs e)
        {
            TrayPopupSettings popupSettings = m_settings.UI.SystemTrayPopup;
            using (TrayPopupConfigForm f = new TrayPopupConfigForm(popupSettings))
            {
                // Edit a copy of the current settings
                f.ShowDialog();
                if (f.DialogResult != DialogResult.OK)
                    return;

                m_settings.UI.SystemTrayPopup = popupSettings;
                trayPopupRadio.Checked = true;
            }
        }
        
        /// <summary>
        /// Reset the priorities conflict custom message box.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPrioritiesReset_Click(object sender, EventArgs e)
        {
            Settings.UI.PlanWindow.PrioritiesMsgBox.ShowDialogBox = true;
            Settings.UI.PlanWindow.PrioritiesMsgBox.DialogResult = DialogResult.None;
        }

        /// <summary>
        /// Opens the EVEMon data directory.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void btnEVEMonDataDir_Click(object sender, EventArgs e)
        {
            Util.OpenURL(new Uri(EveMonClient.EVEMonDataDir));
        }

        #endregion


        #region Other handlers

        /// <summary>
        /// Gets the custom icon set.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        private static ImageList GetCustomIconSet(int index)
        {
            string groupname = String.Empty;

            if (index > 0 && index < IconSettings.Default.Properties.Count)
            {
                SettingsProperty iconSettingsProperty =
                    IconSettings.Default.Properties["Group" + index];
                if (iconSettingsProperty != null)
                    groupname = iconSettingsProperty.DefaultValue.ToString();
            }

            string groupDirectory = $"{AppDomain.CurrentDomain.BaseDirectory}Resources\\Skill_Select\\Group";
            string defaultResourcesPath = $"{groupDirectory}0\\Default.resources";
            string groupResourcesPath = $"{groupDirectory}{index}\\{groupname}.resources";

            if (!File.Exists(defaultResourcesPath) ||
                (!String.IsNullOrEmpty(groupname) && !File.Exists(groupResourcesPath)))
            {
                groupname = String.Empty;
            }

            return String.IsNullOrEmpty(groupname) ? null : GetCustomIconSet(defaultResourcesPath, groupResourcesPath);
        }

        /// <summary>
        /// Gets the icon set for the given index, using the given list for missing icons.
        /// </summary>
        /// <param name="defaultResourcesPath">The default resources path.</param>
        /// <param name="groupResourcesPath">The group resources path.</param>
        /// <returns></returns>
        private static ImageList GetCustomIconSet(string defaultResourcesPath, string groupResourcesPath)
        {
            ImageList customIconSet;
            ImageList tempImageList = null;
            try
            {
                tempImageList = new ImageList();
                IDictionaryEnumerator basicx;
                IResourceReader defaultGroupReader = null;
                tempImageList.ColorDepth = ColorDepth.Depth32Bit;
                try
                {
                    defaultGroupReader = new ResourceReader(defaultResourcesPath);

                    basicx = defaultGroupReader.GetEnumerator();

                    while (basicx.MoveNext())
                    {
                        tempImageList.Images.Add(basicx.Key.ToString(), (Icon)basicx.Value);
                    }
                }
                finally
                {
                    defaultGroupReader?.Close();
                }

                IResourceReader groupReader = null;
                try
                {
                    groupReader = new ResourceReader(groupResourcesPath);

                    basicx = groupReader.GetEnumerator();

                    while (basicx.MoveNext())
                    {
                        if (tempImageList.Images.ContainsKey(basicx.Key.ToString()))
                            tempImageList.Images.RemoveByKey(basicx.Key.ToString());

                        tempImageList.Images.Add(basicx.Key.ToString(), (Icon)basicx.Value);
                    }
                }
                finally
                {
                    groupReader?.Close();
                }

                customIconSet = tempImageList;
                tempImageList = null;
            }
            finally
            {
                tempImageList?.Dispose();
            }

            return customIconSet;
        }

        /// <summary>
        /// Skill Planner > Skill browser icon set > Icons set combo.
        /// Updates the sample below the combo box.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void skillIconSetComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            tvlist.Nodes.Clear();
            tvlist.ImageList = GetCustomIconSet(cbSkillIconSet.SelectedIndex + 1);

            if (tvlist.ImageList == null)
                return;

            TreeNode gtn = new TreeNode("Book", tvlist.ImageList.Images.IndexOfKey("book"),
                tvlist.ImageList.Images.IndexOfKey("book"));
            gtn.Nodes.Add(new TreeNode("Pre-Reqs NOT met (Rank)", tvlist.ImageList.Images.IndexOfKey("PrereqsNOTMet"),
                tvlist.ImageList.Images.IndexOfKey("PrereqsNOTMet")));
            gtn.Nodes.Add(new TreeNode("Pre-Reqs met (Rank)", tvlist.ImageList.Images.IndexOfKey("PrereqsMet"),
                tvlist.ImageList.Images.IndexOfKey("PrereqsMet")));
            for (int i = 0; i < 6; i++)
            {
                gtn.Nodes.Add(new TreeNode("Level " + i + " (Rank)", tvlist.ImageList.Images.IndexOfKey("lvl" + i),
                    tvlist.ImageList.Images.IndexOfKey("lvl" + i)));
            }
            gtn.Expand();
            tvlist.Nodes.Add(gtn);
        }

        /// <summary>
        /// Calendar > Scheduler entry colors > color controls.
        /// When clicked, displays a color picker.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void colorPanel_Click(object sender, EventArgs e)
        {
            Panel color = (Panel)sender;
            colorDialog.Color = color.BackColor;
            if (colorDialog.ShowDialog() == DialogResult.OK)
                color.BackColor = colorDialog.Color;
        }

        /// <summary>
        /// Selects the proper page.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            multiPanel.SelectedPage = multiPanel.Controls.Cast<MultiPanelPage>().FirstOrDefault(
                page => page.Name == (string)e.Node.Tag);
        }

        /// <summary>
        /// Sets the character info max cycle time.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ACycleInterval_ValueChanged(object sender, EventArgs e)
        {
            if (ACycleInterval.Value == 1)
            {
                cbG15CycleTimes.Checked = false;
                panelCycleQueueInfo.Enabled = false;
                return;
            }

            ACycleTimesInterval.Maximum = Math.Max(ACycleInterval.Value / 2, 1);
            panelCycleQueueInfo.Enabled = true;
        }

        /// <summary>
        /// Cloud Storage Service > Provider selection.
        /// Checks the provider authorization.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void cloudStorageProvidersComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (m_isLoading)
                return;

            m_settings.CloudStorageServiceProvider.ProviderName = cloudStorageProvidersComboBox.SelectedItem?.ToString();
            cloudStorageProviderLogoPictureBox.Image = m_settings.CloudStorageServiceProvider.Provider?.Logo;
            await cloudStorageServiceControl.CheckAPIAuthIsValidAsync(forceRecheck: true);
        }

        /// <summary>
        /// General > Network > ESI Settings.
        /// Opens the application registration page on link click.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void esiSettingsLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Util.OpenURL(new Uri(NetworkConstants.CCPApplicationRegistration));
        }

        /// <summary>
        /// General > Network > ESI Settings.
        /// Fills out the SSO scopes setting with the defaultsd efined in NetworkConstants
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void insertAllScopesButton_Click(object sender, EventArgs e)
        {
            scopesTextBox.Text = NetworkConstants.SSOScopes;
        }
        #endregion
    }
}
