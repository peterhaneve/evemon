using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using EVEMon.Common;
using EVEMon.Common.Collections;
using EVEMon.Common.Controls;
using EVEMon.Common.Controls.MultiPanel;
using EVEMon.Common.CustomEventArgs;
using EVEMon.Common.Enumerations;
using EVEMon.Common.Enumerations.CCPAPI;
using EVEMon.Common.Enumerations.UISettings;
using EVEMon.Common.Extensions;
using EVEMon.Common.Factories;
using EVEMon.Common.Interfaces;
using EVEMon.Common.Models;
using EVEMon.Common.Notifications;
using EVEMon.Common.SettingsObjects;
using EVEMon.DetailsWindow;

namespace EVEMon.CharacterMonitoring
{
    internal sealed partial class CharacterMonitorBody : UserControl
    {
        #region Fields

        private readonly List<ToolStripButton> m_advancedFeatures = new List<ToolStripButton>();
        private ToolStripItem[] m_preferenceMenu;
        private Character m_character;

        #endregion


        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="CharacterMonitorBody"/> class.
        /// </summary>
        public CharacterMonitorBody()
        {
            InitializeComponent();
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

            // Fonts
            Font = FontFactory.GetFont("Tahoma");

            // We make a copy of the preference menu for later use
            m_preferenceMenu = new ToolStripItem[preferencesMenu.DropDownItems.Count];
            preferencesMenu.DropDownItems.CopyTo(m_preferenceMenu, 0);

            multiPanel.SelectionChange += multiPanel_SelectionChange;

            // Subscribe events
            EveMonClient.TimerTick += EveMonClient_TimerTick;
            EveMonClient.SettingsChanged += EveMonClient_SettingsChanged;
            EveMonClient.CharacterAssetsUpdated += EveMonClient_UpdatePageControls;
            EveMonClient.MarketOrdersUpdated += EveMonClient_UpdatePageControls;
            EveMonClient.ContractsUpdated += EveMonClient_UpdatePageControls;
            EveMonClient.CharacterWalletJournalUpdated += EveMonClient_UpdatePageControls;
            EveMonClient.CharacterWalletTransactionsUpdated += EveMonClient_UpdatePageControls;
            EveMonClient.IndustryJobsUpdated += EveMonClient_UpdatePageControls;
            EveMonClient.CharacterPlanetaryColoniesUpdated += EveMonClient_UpdatePageControls;
            EveMonClient.CharacterResearchPointsUpdated += EveMonClient_UpdatePageControls;
            EveMonClient.CharacterEVEMailMessagesUpdated += EveMonClient_UpdatePageControls;
            EveMonClient.CharacterEVENotificationsUpdated += EveMonClient_UpdatePageControls;
            EveMonClient.NotificationSent += EveMonClient_NotificationSent;
            EveMonClient.NotificationInvalidated += EveMonClient_NotificationInvalidated;
            Disposed += OnDisposed;


            // Updates the controls
            UpdateInfrequentControls();
            UpdateNotifications();

            // Picks the last selected page
            multiPanel.SelectedPage = null;
            ToolStripItem item = null;

            // Only for CCP characters
            if (m_character is CCPCharacter)
            {
                item = toolStripFeatures.Items.Cast<ToolStripItem>().FirstOrDefault(
                    x => m_character.UISettings.SelectedPage == (string)x.Tag);

                // If it's not an advanced feature page make it visible
                if (item != null && !m_advancedFeatures.Contains(item))
                    item.Visible = true;

                // If it's an advanced feature page reset to skills page
                if (item != null && m_advancedFeatures.Contains(item))
                    item = skillsIcon;
            }

            toolbarIcon_Click(item ?? skillsIcon, EventArgs.Empty);
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
            UpdateFeaturesMenu();
        }

        /// <summary>
        /// Called when the control is disposed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OnDisposed(object sender, EventArgs e)
        {
            EveMonClient.TimerTick -= EveMonClient_TimerTick;
            EveMonClient.ESIKeyInfoUpdated -= EveMonClient_APIKeyInfoUpdated;
            EveMonClient.SettingsChanged -= EveMonClient_SettingsChanged;
            EveMonClient.CharacterAssetsUpdated -= EveMonClient_UpdatePageControls;
            EveMonClient.MarketOrdersUpdated -= EveMonClient_UpdatePageControls;
            EveMonClient.ContractsUpdated -= EveMonClient_UpdatePageControls;
            EveMonClient.CharacterWalletJournalUpdated -= EveMonClient_UpdatePageControls;
            EveMonClient.CharacterWalletTransactionsUpdated -= EveMonClient_UpdatePageControls;
            EveMonClient.IndustryJobsUpdated -= EveMonClient_UpdatePageControls;
            EveMonClient.CharacterPlanetaryColoniesUpdated -= EveMonClient_UpdatePageControls;
            EveMonClient.CharacterResearchPointsUpdated -= EveMonClient_UpdatePageControls;
            EveMonClient.CharacterEVEMailMessagesUpdated -= EveMonClient_UpdatePageControls;
            EveMonClient.CharacterEVENotificationsUpdated -= EveMonClient_UpdatePageControls;
            EveMonClient.NotificationSent -= EveMonClient_NotificationSent;
            EveMonClient.NotificationInvalidated -= EveMonClient_NotificationInvalidated;
            Disposed -= OnDisposed;
        }

        #endregion


        #region Update display methods on character change

        /// <summary>
        /// Updates the controls whos content changes frequently.
        /// </summary>
        private void UpdateFrequentControls()
        {
            SuspendLayout();
            try
            {
                // Hides or shows the warning about a character with no API key
                warningLabel.Visible = !m_character.Identity.ESIKeys.Any();
            }
            finally
            {
                ResumeLayout(false);
                Refresh();
            }
        }

        /// <summary>
        /// Updates the controls whos content changes infrequently.
        /// </summary>
        private void UpdateInfrequentControls()
        {
            // No need to do anything when the control is not visible
            if (!Visible)
                return;

            SuspendLayout();
            try
            {
                // Reset the text filter
                if (toolStripContextual.Visible)
                    searchTextBox.Text = String.Empty;

                // Read the settings
                if (Settings.UI.SafeForWork)
                {
                    // Takes care of the features icons
                    foreach (ToolStripItem item in toolStripFeatures.Items.Cast<ToolStripItem>().Where(
                        item => item is ToolStripButton || item is ToolStripDropDownButton))
                    {
                        item.DisplayStyle = ToolStripItemDisplayStyle.Text;
                    }

                    // Takes care of the special second toolstrip icons 
                    foreach (ToolStripItem item in toolStripContextual.Items)
                    {
                        item.DisplayStyle = ToolStripItemDisplayStyle.Text;
                    }

                    return;
                }

                // Display image and text of the features according to user preference
                foreach (ToolStripButton item in toolStripFeatures.Items.OfType<ToolStripButton>())
                {
                    item.DisplayStyle = ToolStripItemDisplayStyle.Image;
                }

                foreach (ToolStripButton item in toolStripContextual.Items.OfType<ToolStripButton>())
                {
                    item.DisplayStyle = ToolStripItemDisplayStyle.Image;
                }

                featuresMenu.DisplayStyle = ToolStripItemDisplayStyle.Image;

                preferencesMenu.DisplayStyle = ToolStripItemDisplayStyle.Image;
                groupMenu.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            }
            finally
            {
                ResumeLayout(false);
                Refresh();
            }
        }

        /// <summary>
        /// Hides or shows the features menu.
        /// </summary>
        private void UpdateFeaturesMenu()
        {
            if (EveMonClient.ESIKeys.Any(apiKey => !apiKey.IsProcessed) || !m_character.Identity.ESIKeys.Any())
                return;

            CCPCharacter ccpCharacter = m_character as CCPCharacter;
            if (ccpCharacter == null)
                return;

            tsPagesSeparator.Visible = featuresMenu.Visible = true;
            tsToggleSeparator.Visible = toggleSkillsIcon.Visible;
            m_advancedFeatures.ForEach(SetVisibility);
            ToggleAdvancedFeaturesMonitoring();
            Refresh();
        }

        /// <summary>
        /// Sets the button visibility.
        /// </summary>
        /// <param name="button">The button.</param>
        private void SetVisibility(ToolStripButton button)
        {
            IEnumerable<IQueryMonitor> monitors = GetButtonMonitors(button);
            bool visible = monitors.Any(monitor => monitor.HasAccess) && IsEnabledFeature(button.Text);
            button.Visible = visible;

            // Quit if the button should stay visible
            // (Do not use the buttons' 'Visible' property as condition,
            // because there is a .NET bug when returning from minimized state) 
            if (visible)
                return;

            // Buttons' related monitor lost access to data while it was enabled, so...
            // 1. Remove buttons' related page from settings
            if (m_character.UISettings.AdvancedFeaturesEnabledPages.Contains(button.Text))
                m_character.UISettings.AdvancedFeaturesEnabledPages.Remove(button.Text);

            // 2. Uncheck in dropdown menu
            if (featuresMenu.DropDownItems.Count == 0)
                return;

            int index = featuresMenu.DropDownItems.IndexOf(SelectionToolStripSeparator) + 1;

            foreach (ToolStripMenuItem item in featuresMenu.DropDownItems.Cast<ToolStripItem>()
                .Skip(index).Cast<ToolStripMenuItem>().Where(
                    item => item.Text == button.Text && item.Checked))
            {
                item.Checked = !item.Checked;

                // Update the selected page
                UpdateSelectedPage();
            }
        }

        /// <summary>
        /// Hides or shows the pages controls.
        /// </summary>
        private void UpdatePageControls()
        {
            // No need to do anything when the control is not visible
            if (!Visible)
                return;

            // Enables / Disables the skill page controls
            toggleSkillsIcon.Enabled = m_character.Skills.Any();

            // Exit if it's a non-CCPCharacter
            CCPCharacter ccpCharacter = m_character as CCPCharacter;
            if (ccpCharacter == null)
                return;

            // Ensures the visibility of the toolstrip items
            foreach (ToolStripItem item in toolStripContextual.Items)
            {
                item.Visible = true;
            }

            // Show the wallet journal charts button only when on wallet journal page
            walletJournalCharts.Visible = multiPanel.SelectedPage == walletJournalPage;

            // Show contacts buttons only when on contacts page
            allContacts.Visible = contactsExcellent.Visible = multiPanel.SelectedPage == contactsPage;
            contactsGood.Visible = contactsNeutral.Visible = multiPanel.SelectedPage == contactsPage;
            contactsBad.Visible = contactsTerrible.Visible = inWatchList.Visible = multiPanel.SelectedPage == contactsPage;

            // Enables / Disables the contacts page related controls
            if (multiPanel.SelectedPage == contactsPage)
            {
                groupMenu.Visible = searchTextBox.Visible = searchTextDel.Visible = preferencesMenu.Visible = false;
                toolStripContextual.Enabled = true;
            }

            // Enables / Disables the kill logs page related controls
            if (multiPanel.SelectedPage == killLogPage)
            {
                groupMenu.Visible = false;
                searchTextBox.Visible = searchTextDel.Visible = Settings.UI.MainWindow.CombatLog.ShowCondensedLogs;
                toolStripContextual.Enabled = true;
            }

            // Enables / Disables the assets page related controls
            if (multiPanel.SelectedPage == assetsPage)
                toolStripContextual.Enabled = ccpCharacter.Assets.Any();

            // Enables / Disables the market orders page related controls
            if (multiPanel.SelectedPage == ordersPage)
                toolStripContextual.Enabled = ccpCharacter.MarketOrders.Any();

            // Enables / Disables the contracts page related controls
            if (multiPanel.SelectedPage == contractsPage)
                toolStripContextual.Enabled = ccpCharacter.Contracts.Any();

            // Enables / Disables the wallet journal page related controls
            if (multiPanel.SelectedPage == walletJournalPage)
                toolStripContextual.Enabled = ccpCharacter.WalletJournal.Any();

            // Enables / Disables the wallet transactions page related controls
            if (multiPanel.SelectedPage == walletTransactionsPage)
                toolStripContextual.Enabled = ccpCharacter.WalletTransactions.Any();

            // Enables / Disables the industry jobs page related controls
            if (multiPanel.SelectedPage == jobsPage)
                toolStripContextual.Enabled = ccpCharacter.IndustryJobs.Any();

            // Enables / Disables the planetary colonies page related controls
            if (multiPanel.SelectedPage == planetaryPage)
                toolStripContextual.Enabled = ccpCharacter.PlanetaryColonies.Any();

            // Enables / Disables the research points page related controls
            if (multiPanel.SelectedPage == researchPage)
            {
                groupMenu.Visible = false;
                toolStripContextual.Enabled = ccpCharacter.ResearchPoints.Any();
            }

            // Enables / Disables the EVE mail messages page related controls
            if (multiPanel.SelectedPage == mailMessagesPage)
                toolStripContextual.Enabled = ccpCharacter.EVEMailMessages.Any();

            // Enables / Disables the EVE notifications page related controls
            if (multiPanel.SelectedPage == eveNotificationsPage)
                toolStripContextual.Enabled = ccpCharacter.EVENotifications.Any();
        }

        /// <summary>
        /// Toggles the advanced features monitoring.
        /// </summary>
        private void ToggleAdvancedFeaturesMonitoring()
        {
            // Quit if it's a non-CCPCharacter
            CCPCharacter ccpCharacter = m_character as CCPCharacter;
            if (ccpCharacter == null)
                return;

            foreach (ToolStripButton button in m_advancedFeatures)
            {
                List<IQueryMonitor> monitors = GetButtonMonitors(button);

                if (!monitors.Any())
                    continue;

                foreach (IQueryMonitor monitor in monitors)
                {
                    monitor.Enabled = IsEnabledFeature(button.Text);
                    if (!monitor.QueryOnStartup || !monitor.Enabled || monitor.LastResult != null)
                        continue;

                    if (monitor.Method is ESIAPICharacterMethods &&
                        (ESIAPICharacterMethods)monitor.Method == ESIAPICharacterMethods.FactionalWarfareStats &&
                        ccpCharacter.IsFactionalWarfareNotEnlisted)
                    {
                        monitor.Enabled = !ccpCharacter.IsFactionalWarfareNotEnlisted;
                        continue;
                    }

                    ccpCharacter.QueryMonitors.Query(monitor.Method);
                }
            }
        }

        /// <summary>
        /// Updates advanced features pages selection and settings.
        /// </summary>
        private void UpdateAdvancedFeaturesPagesSettings()
        {
            UpdateSelectedPage();

            int index = featuresMenu.DropDownItems.IndexOf(SelectionToolStripSeparator) + 1;

            List<string> enabledAdvancedFeaturesPages =
                featuresMenu.DropDownItems.Cast<ToolStripItem>().Skip(index).Cast<ToolStripMenuItem>().Where(
                    menuItem => menuItem.Checked).Select(menuItem => menuItem.Text).ToList();

            m_character.UISettings.AdvancedFeaturesEnabledPages.Clear();
            enabledAdvancedFeaturesPages.ForEach(page => m_character.UISettings.AdvancedFeaturesEnabledPages.Add(page));
        }

        /// <summary>
        /// Updates the selected page.
        /// </summary>
        private void UpdateSelectedPage()
        {
            if (m_advancedFeatures.Any(featureIcon =>
                multiPanel.SelectedPage.Text == (string)featureIcon.Tag && !featureIcon.Visible))
            {
                toolbarIcon_Click(skillsIcon, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Update the notifications list.
        /// </summary>
        private void UpdateNotifications()
        {
            notificationList.Notifications = EveMonClient.Notifications.Where(x => x.Sender == m_character);
            Refresh();
        }

        #endregion


        #region Global Events

        /// <summary>
        /// Occur on every second.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void EveMonClient_TimerTick(object sender, EventArgs e)
        {
            UpdateFrequentControls();
        }

        /// <summary>
        /// When the API key info updates, update the features menu.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void EveMonClient_APIKeyInfoUpdated(object sender, EventArgs e)
        {
            UpdateFeaturesMenu();
        }

        /// <summary>
        /// Updates the controls on settings change.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void EveMonClient_SettingsChanged(object sender, EventArgs e)
        {
            UpdateInfrequentControls();
        }

        /// <summary>
        /// Updates the page controls on an event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="CharacterChangedEventArgs"/> instance containing the event data.</param>
        private void EveMonClient_UpdatePageControls(object sender, CharacterChangedEventArgs e)
        {
            if (e.Character != m_character)
                return;

            UpdatePageControls();
        }

        /// <summary>
        /// Update the notifications list.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EVEMon.Common.Notifications.NotificationInvalidationEventArgs"/> instance containing the event data.</param>
        private void EveMonClient_NotificationInvalidated(object sender, NotificationInvalidationEventArgs e)
        {
            UpdateNotifications();
        }

        /// <summary>
        /// Update the notifications list.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void EveMonClient_NotificationSent(object sender, NotificationEventArgs e)
        {
            UpdateNotifications();
        }

        #endregion


        #region Local Events


        #region Features Toolstrip Controls Event Handlers

        /// <summary>
        /// When the selected page changes, we may have to update the related controls visibility.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MultiPanelSelectionChangeEventArgs"/> instance containing the event data.</param>
        private void multiPanel_SelectionChange(object sender, MultiPanelSelectionChangeEventArgs e)
        {
            if (e.NewPage == null)
                return;

            // Stores the setting
            m_character.UISettings.SelectedPage = e.NewPage.Text;

            // Update the buttons visibility
            toggleSkillsIcon.Visible = e.NewPage == skillsPage;
            tsPagesSeparator.Visible = featuresMenu.Visible;
            tsToggleSeparator.Visible = toggleSkillsIcon.Visible;
            toolStripContextual.Visible = m_advancedFeatures.Any(button => (string)button.Tag != standingsPage.Text &&
                                                                           (string)button.Tag != factionalWarfareStatsPage.Text &&
                                                                           (string)button.Tag != medalsPage.Text &&
                                                                           (string)button.Tag == e.NewPage.Text);

            // Reset the text filter
            searchTextBox.Text = String.Empty;

            // Update the page controls
            UpdatePageControls();
        }

        /// <summary>
        /// Handles the Click event of the toolbarIcon control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void toolbarIcon_Click(object sender, EventArgs e)
        {
            foreach (ToolStripItem item in toolStripFeatures.Items)
            {
                // Skip tags without tag, those ones do not represent "pages switches"
                if (item.Tag == null)
                    continue;

                // Is it the item we clicked ?
                ToolStripButton button = item as ToolStripButton;
                if (button != null && item == sender)
                {
                    // Page is already selected
                    if (button.Checked && multiPanel.SelectedPage != null)
                        continue;

                    // Selects the proper page
                    multiPanel.SelectedPage =
                        multiPanel.Controls.Cast<MultiPanelPage>().First(x => x.Name == (string)item.Tag);

                    // Checks it
                    button.Checked = true;
                }
                // Or another one representing another page ?
                else if (button != null)
                {
                    // Unchecks it
                    button.Checked = false;
                }
            }
        }

        /// <summary>
        /// Toggles all the skill groups to collapse or open.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void toggleSkillsIcon_Click(object sender, EventArgs e)
        {
            skillsList.ToggleAll();
        }

        /// <summary>
        /// On menu opening we create the menu items and update their checked state.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void featureMenu_DropDownOpening(object sender, EventArgs e)
        {
            // Remove everything after the separator
            int index = featuresMenu.DropDownItems.IndexOf(SelectionToolStripSeparator) + 1;
            while (featuresMenu.DropDownItems.Count > index)
            {
                featuresMenu.DropDownItems.RemoveAt(index);
            }

            // Create the menu items
            List<ToolStripMenuItem> toolStripMenuItems = m_advancedFeatures
                .Select(button => new { button, monitor = GetButtonMonitors(button) })
                .Where(item => item.monitor != null
                               && multiPanel.Controls.OfType<MultiPanelPage>().Any(page => page.Name == (string)item.button.Tag))
                .Select(item =>
                {
                    ToolStripMenuItem tsmi;
                    ToolStripMenuItem tempToolStripMenuItem = null;
                    try
                    {
                        tempToolStripMenuItem = new ToolStripMenuItem(item.button.Text)
                        {
                            Checked = IsEnabledFeature(item.button.Text),
                            Enabled = item.monitor.Any(monitor => monitor.HasAccess)
                        };

                        tsmi = tempToolStripMenuItem;
                        tempToolStripMenuItem = null;
                    }
                    finally
                    {
                        tempToolStripMenuItem?.Dispose();
                    }
                    return tsmi;
                }).ToList();

            // Add items to dropdown menu
            featuresMenu.DropDownItems.AddRange(toolStripMenuItems.ToArray<ToolStripItem>());

            // Enable/Disable the "Enable All / Disable All" controls
            EnableAllToolStripMenuItem.Enabled = toolStripMenuItems.Where(item => item.Enabled).Any(item => !item.Checked);
            DisableAllToolStripMenuItem.Enabled = toolStripMenuItems.Where(item => item.Enabled).Any(item => item.Checked);
        }

        /// <summary>
        /// Occurs when the user click an item in the features menu.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void featuresMenu_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)e.ClickedItem;

            if (item.Equals(EnableAllToolStripMenuItem) || item.Equals(DisableAllToolStripMenuItem))
                return;

            item.Checked = !item.Checked;

            m_advancedFeatures.ForEach(featureIcon => featureIcon.Visible = item.Text == featureIcon.Text
                ? item.Checked
                : featureIcon.Visible);

            UpdateAdvancedFeaturesPagesSettings();
            ToggleAdvancedFeaturesMonitoring();
        }

        /// <summary>
        /// Occurs when the user click the 'Enable All' menu item in features menu.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void EnableAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int index = featuresMenu.DropDownItems.IndexOf(SelectionToolStripSeparator) + 1;
            foreach (ToolStripMenuItem item in featuresMenu.DropDownItems.Cast<ToolStripItem>()
                .Skip(index).Cast<ToolStripMenuItem>().Where(item => item.Enabled))
            {
                item.Checked = true;
                m_advancedFeatures.First(featureIcon => item.Text == featureIcon.Text).Visible = true;
            }

            UpdateAdvancedFeaturesPagesSettings();
            ToggleAdvancedFeaturesMonitoring();
        }

        /// <summary>
        /// Occurs when the user click the 'Disable All' menu item in features menu.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void DisableAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int index = featuresMenu.DropDownItems.IndexOf(SelectionToolStripSeparator) + 1;
            foreach (ToolStripMenuItem item in featuresMenu.DropDownItems.Cast<ToolStripItem>()
                .Skip(index).Cast<ToolStripMenuItem>().Where(item => item.Enabled))
            {
                item.Checked = false;
                m_advancedFeatures.First(featureIcon => item.Text == featureIcon.Text).Visible = false;
            }

            UpdateAdvancedFeaturesPagesSettings();
            ToggleAdvancedFeaturesMonitoring();
        }

        #endregion


        #region Contextual Toolstip Controls Event Handlers


        #region WalletJournal Control Event Handlers

        /// <summary>
        /// Handles the Click event of the walletJournalCharts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void walletJournalCharts_Click(object sender, EventArgs e)
        {
            CCPCharacter ccpCharacter = m_character as CCPCharacter;
            if (ccpCharacter == null)
                return;

            WindowsFactory.ShowByTag<WalletJournalChartWindow, CCPCharacter>(ccpCharacter);
        }

        #endregion


        #region Contacts Control Event Handlers

        /// <summary>
        /// Handles the Click event of the contactsToolbarIcon control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void contactsToolbarIcon_Click(object sender, EventArgs e)
        {
            foreach (ToolStripButton item in toolStripContextual.Items.OfType<ToolStripButton>())
            {
                item.Checked = item == sender;
            }

            contactsList.ShowAllContacts = sender.Equals(allContacts);
            contactsList.ShowContactsInWatchList = sender.Equals(inWatchList);

            if (sender.Equals(contactsExcellent))
                contactsList.ShowContactsWithStandings = StandingStatus.Excellent;

            if (sender.Equals(contactsGood))
                contactsList.ShowContactsWithStandings = StandingStatus.Good;

            if (sender.Equals(contactsNeutral))
                contactsList.ShowContactsWithStandings = StandingStatus.Neutral;

            if (sender.Equals(contactsBad))
                contactsList.ShowContactsWithStandings = StandingStatus.Bad;

            if (sender.Equals(contactsTerrible))
                contactsList.ShowContactsWithStandings = StandingStatus.Terrible;

            contactsList.UpdateContent();
        }

        #endregion


        #region GroupBy Control Event Handlers

        /// <summary>
        /// On opening we create the menu items for "Group By..." in panel.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void groupMenu_DropDownOpening(object sender, EventArgs e)
        {
            groupMenu.DropDownItems.Clear();

            if (multiPanel.SelectedPage == assetsPage)
                CreateGroupMenuList<AssetGrouping, Enum>(assetsList);

            if (multiPanel.SelectedPage == ordersPage)
                CreateGroupMenuList<MarketOrderGrouping, Enum>(ordersList);

            if (multiPanel.SelectedPage == contractsPage)
                CreateGroupMenuList<ContractGrouping, Enum>(contractsList);

            if (multiPanel.SelectedPage == walletJournalPage)
                CreateGroupMenuList<WalletJournalGrouping, Enum>(walletJournalList);

            if (multiPanel.SelectedPage == walletTransactionsPage)
                CreateGroupMenuList<WalletTransactionGrouping, Enum>(walletTransactionsList);

            if (multiPanel.SelectedPage == jobsPage)
                CreateGroupMenuList<IndustryJobGrouping, Enum>(jobsList);

            if (multiPanel.SelectedPage == planetaryPage)
                CreateGroupMenuList<PlanetaryGrouping, Enum>(planetaryList);

            if (multiPanel.SelectedPage == mailMessagesPage)
                CreateGroupMenuList<EVEMailMessagesGrouping, Enum>(mailMessagesList);

            if (multiPanel.SelectedPage == eveNotificationsPage)
                CreateGroupMenuList<EVENotificationsGrouping, Enum>(eveNotificationsList);
        }

        /// <summary>
        /// Occurs when the user click an item in the "Group By..." menu.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.ToolStripItemClickedEventArgs"/> instance containing the event data.</param>
        private void groupMenu_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            ToolStripItem item = e.ClickedItem;

            if (multiPanel.SelectedPage == assetsPage)
                GroupMenuSetting<AssetGrouping, Enum>(item, assetsList);

            if (multiPanel.SelectedPage == ordersPage)
                GroupMenuSetting<MarketOrderGrouping, Enum>(item, ordersList);

            if (multiPanel.SelectedPage == contractsPage)
                GroupMenuSetting<ContractGrouping, Enum>(item, contractsList);

            if (multiPanel.SelectedPage == walletJournalPage)
                GroupMenuSetting<WalletJournalGrouping, Enum>(item, walletJournalList);

            if (multiPanel.SelectedPage == walletTransactionsPage)
                GroupMenuSetting<WalletTransactionGrouping, Enum>(item, walletTransactionsList);

            if (multiPanel.SelectedPage == jobsPage)
                GroupMenuSetting<IndustryJobGrouping, Enum>(item, jobsList);

            if (multiPanel.SelectedPage == planetaryPage)
                GroupMenuSetting<PlanetaryGrouping, Enum>(item, planetaryList);

            if (multiPanel.SelectedPage == mailMessagesPage)
                GroupMenuSetting<EVEMailMessagesGrouping, Enum>(item, mailMessagesList);

            if (multiPanel.SelectedPage == eveNotificationsPage)
                GroupMenuSetting<EVENotificationsGrouping, Enum>(item, eveNotificationsList);
        }

        #endregion


        #region Search Control Event Handlers

        /// <summary>
        /// Occurs when the search text changes.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void searchTextBox_TextChanged(object sender, EventArgs e)
        {
            if (searchTextTimer == null)
            {
                UpdateListSearchTextFilter();
                return;
            }

            if (searchTextTimer.Enabled)
                searchTextTimer.Stop();

            searchTextTimer.Start();
        }

        /// <summary>
        /// Handles the Tick event of the searchTextTimer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void searchTextTimer_Tick(object sender, EventArgs e)
        {
            searchTextTimer.Stop();
            UpdateListSearchTextFilter();
        }

        /// <summary>
        /// Handles the MouseUp event of the searchTextDel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void searchTextDel_MouseUp(object sender, MouseEventArgs e)
        {
            searchTextBox.Clear();
        }

        #endregion


        #region Preferences Controls Event Handlers

        /// <summary>
        /// On menu opening we update the menu items.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void preferencesMenu_DropDownOpening(object sender, EventArgs e)
        {
            bool hideInactive = true;
            autoSizeColumnMenuItem.Enabled = true;

            if (multiPanel.SelectedPage == killLogPage)
            {
                preferencesMenu.DropDownItems.Clear();
                preferencesMenu.DropDownItems.Add(autoSizeColumnMenuItem);
                preferencesMenu.DropDownItems.Add(combatLogSeparator);
                preferencesMenu.DropDownItems.Add(combatLogMenuItem);

                combatLogMenuItem.Checked = Settings.UI.MainWindow.CombatLog.ShowCondensedLogs;
                autoSizeColumnMenuItem.Enabled = combatLogMenuItem.Checked;
            }

            if (multiPanel.SelectedPage == assetsPage || multiPanel.SelectedPage == walletJournalPage
                || multiPanel.SelectedPage == walletTransactionsPage)
            {
                bool numberFormat = Settings.UI.MainWindow.Assets.NumberAbsFormat;

                preferencesMenu.DropDownItems.Clear();
                foreach (ToolStripItem item in m_preferenceMenu.Where(
                    item => !item.Equals(hideInactiveMenuItem) && !item.Equals(tsOptionsSeparator) &&
                            !item.Equals(showOnlyCharMenuItem) && !item.Equals(showOnlyCorpMenuItem) &&
                            !item.Equals(tsReadingPaneSeparator) && !item.Equals(readingPaneMenuItem) &&
                            !item.Equals(combatLogSeparator) && !item.Equals(combatLogMenuItem) &&
                            !item.Equals(tsPlanetarySeparator) && !item.Equals(showOnlyExtractorMenuItem)))
                {
                    preferencesMenu.DropDownItems.Add(item);
                }

                numberAbsFormatMenuItem.Text = numberFormat ? "Full Number Format" : "Abbreviating Number Format";
            }

            if (multiPanel.SelectedPage == ordersPage)
            {
                bool numberFormat = Settings.UI.MainWindow.MarketOrders.NumberAbsFormat;
                hideInactive = Settings.UI.MainWindow.MarketOrders.HideInactiveOrders;

                preferencesMenu.DropDownItems.Clear();
                foreach (ToolStripItem item in m_preferenceMenu.Where(
                    item => !item.Equals(tsReadingPaneSeparator) && !item.Equals(readingPaneMenuItem) &&
                            !item.Equals(combatLogSeparator) && !item.Equals(combatLogMenuItem) &&
                            !item.Equals(tsPlanetarySeparator) && !item.Equals(showOnlyExtractorMenuItem)))
                {
                    preferencesMenu.DropDownItems.Add(item);
                }

                numberAbsFormatMenuItem.Text = numberFormat ? "Full Number Format" : "Abbreviating Number Format";
                showOnlyCharMenuItem.Checked = ordersList.ShowIssuedFor == IssuedFor.Character;
                showOnlyCorpMenuItem.Checked = ordersList.ShowIssuedFor == IssuedFor.Corporation;
            }

            if (multiPanel.SelectedPage == contractsPage)
            {
                bool numberFormat = Settings.UI.MainWindow.Contracts.NumberAbsFormat;
                hideInactive = Settings.UI.MainWindow.Contracts.HideInactiveContracts;

                preferencesMenu.DropDownItems.Clear();
                foreach (ToolStripItem item in m_preferenceMenu.Where(
                    item => !item.Equals(tsReadingPaneSeparator) && !item.Equals(readingPaneMenuItem) &&
                            !item.Equals(combatLogSeparator) && !item.Equals(combatLogMenuItem) &&
                            !item.Equals(tsPlanetarySeparator) && !item.Equals(showOnlyExtractorMenuItem)))
                {
                    preferencesMenu.DropDownItems.Add(item);
                }

                numberAbsFormatMenuItem.Text = numberFormat ? "Full Number Format" : "Abbreviating Number Format";
                showOnlyCharMenuItem.Checked = contractsList.ShowIssuedFor == IssuedFor.Character;
                showOnlyCorpMenuItem.Checked = contractsList.ShowIssuedFor == IssuedFor.Corporation;
            }

            if (multiPanel.SelectedPage == jobsPage)
            {
                hideInactive = Settings.UI.MainWindow.IndustryJobs.HideInactiveJobs;

                preferencesMenu.DropDownItems.Clear();
                foreach (ToolStripItem item in m_preferenceMenu.Where(
                    item => !item.Equals(numberAbsFormatMenuItem) && !item.Equals(tsReadingPaneSeparator) &&
                            !item.Equals(readingPaneMenuItem) && !item.Equals(combatLogSeparator) &&
                            !item.Equals(combatLogMenuItem) && !item.Equals(tsPlanetarySeparator) &&
                            !item.Equals(showOnlyExtractorMenuItem)))
                {
                    preferencesMenu.DropDownItems.Add(item);
                }

                showOnlyCharMenuItem.Checked = jobsList.ShowIssuedFor == IssuedFor.Character;
                showOnlyCorpMenuItem.Checked = jobsList.ShowIssuedFor == IssuedFor.Corporation;
            }

            if (multiPanel.SelectedPage == planetaryPage)
            {
                preferencesMenu.DropDownItems.Clear();
                preferencesMenu.DropDownItems.Add(columnSettingsMenuItem);
                preferencesMenu.DropDownItems.Add(autoSizeColumnMenuItem);
                preferencesMenu.DropDownItems.Add(tsPlanetarySeparator);
                preferencesMenu.DropDownItems.Add(showOnlyExtractorMenuItem);
                showOnlyExtractorMenuItem.Checked = Settings.UI.MainWindow.Planetary.ShowEcuOnly;

                return;
            }

            if (multiPanel.SelectedPage == researchPage)
            {
                preferencesMenu.DropDownItems.Clear();
                preferencesMenu.DropDownItems.Add(columnSettingsMenuItem);
                preferencesMenu.DropDownItems.Add(autoSizeColumnMenuItem);
                return;
            }

            if (multiPanel.SelectedPage == mailMessagesPage || multiPanel.SelectedPage == eveNotificationsPage)
            {
                preferencesMenu.DropDownItems.Clear();
                preferencesMenu.DropDownItems.Add(columnSettingsMenuItem);
                preferencesMenu.DropDownItems.Add(autoSizeColumnMenuItem);
                preferencesMenu.DropDownItems.Add(tsReadingPaneSeparator);
                preferencesMenu.DropDownItems.Add(readingPaneMenuItem);
                return;
            }

            hideInactiveMenuItem.Text = hideInactive ? "Unhide Inactive" : "Hide Inactive";
        }

        /// <summary>
        /// Display the window to select columns.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void columnSettingsMenuItem_Click(object sender, EventArgs e)
        {
            if (multiPanel.SelectedPage == assetsPage)
            {
                using (AssetsColumnsSelectWindow f =
                    new AssetsColumnsSelectWindow(assetsList.Columns.Cast<AssetColumnSettings>()))
                {
                    DialogResult dr = f.ShowDialog();
                    if (dr == DialogResult.OK)
                    {
                        assetsList.Columns = f.Columns;
                        Settings.UI.MainWindow.Assets.Columns.Clear();
                        Settings.UI.MainWindow.Assets.Columns.AddRange(assetsList.Columns.Cast<AssetColumnSettings>());
                    }
                }
            }

            if (multiPanel.SelectedPage == ordersPage)
            {
                using (MarketOrdersColumnsSelectWindow f =
                    new MarketOrdersColumnsSelectWindow(ordersList.Columns.Cast<MarketOrderColumnSettings>()))
                {
                    DialogResult dr = f.ShowDialog();
                    if (dr == DialogResult.OK)
                    {
                        ordersList.Columns = f.Columns;
                        Settings.UI.MainWindow.MarketOrders.Columns.Clear();
                        Settings.UI.MainWindow.MarketOrders.Columns.AddRange(ordersList.Columns.Cast<MarketOrderColumnSettings>());
                    }
                }
            }

            if (multiPanel.SelectedPage == contractsPage)
            {
                using (ContractsColumnsSelectWindow f =
                    new ContractsColumnsSelectWindow(contractsList.Columns.Cast<ContractColumnSettings>()))
                {
                    DialogResult dr = f.ShowDialog();
                    if (dr == DialogResult.OK)
                    {
                        contractsList.Columns = f.Columns;
                        Settings.UI.MainWindow.Contracts.Columns.Clear();
                        Settings.UI.MainWindow.Contracts.Columns.AddRange(contractsList.Columns.Cast<ContractColumnSettings>());
                    }
                }
            }

            if (multiPanel.SelectedPage == walletJournalPage)
            {
                using (WalletJournalColumnsSelectWindow f =
                    new WalletJournalColumnsSelectWindow(walletJournalList.Columns.Cast<WalletJournalColumnSettings>()))
                {
                    DialogResult dr = f.ShowDialog();
                    if (dr == DialogResult.OK)
                    {
                        walletJournalList.Columns = f.Columns;
                        Settings.UI.MainWindow.WalletJournal.Columns.Clear();
                        Settings.UI.MainWindow.WalletJournal.Columns.AddRange(
                            walletJournalList.Columns.Cast<WalletJournalColumnSettings>());
                    }
                }
            }

            if (multiPanel.SelectedPage == walletTransactionsPage)
            {
                using (WalletTransactionsColumnsSelectWindow f = new WalletTransactionsColumnsSelectWindow(
                    walletTransactionsList.Columns.Cast<WalletTransactionColumnSettings>()))
                {
                    DialogResult dr = f.ShowDialog();
                    if (dr == DialogResult.OK)
                    {
                        walletTransactionsList.Columns = f.Columns;
                        Settings.UI.MainWindow.WalletTransactions.Columns.Clear();
                        Settings.UI.MainWindow.WalletTransactions.Columns.AddRange(
                            walletTransactionsList.Columns.Cast<WalletTransactionColumnSettings>());
                    }
                }
            }

            if (multiPanel.SelectedPage == jobsPage)
            {
                using (IndustryJobsColumnsSelectWindow f =
                    new IndustryJobsColumnsSelectWindow(jobsList.Columns.Cast<IndustryJobColumnSettings>()))
                {
                    DialogResult dr = f.ShowDialog();
                    if (dr == DialogResult.OK)
                    {
                        jobsList.Columns = f.Columns;
                        Settings.UI.MainWindow.IndustryJobs.Columns.Clear();
                        Settings.UI.MainWindow.IndustryJobs.Columns.AddRange(jobsList.Columns.Cast<IndustryJobColumnSettings>());
                    }
                }
            }

            if (multiPanel.SelectedPage == planetaryPage)
            {
                using (PlanetaryColumnsSelectWindow f =
                    new PlanetaryColumnsSelectWindow(planetaryList.Columns.Cast<PlanetaryColumnSettings>()))
                {
                    DialogResult dr = f.ShowDialog();
                    if (dr == DialogResult.OK)
                    {
                        planetaryList.Columns = f.Columns;
                        Settings.UI.MainWindow.Planetary.Columns.Clear();
                        Settings.UI.MainWindow.Planetary.Columns.AddRange(planetaryList.Columns.Cast<PlanetaryColumnSettings>());
                    }
                }
            }

            if (multiPanel.SelectedPage == researchPage)
            {
                using (ResearchColumnsSelectWindow f =
                    new ResearchColumnsSelectWindow(researchList.Columns.Cast<ResearchColumnSettings>()))
                {
                    DialogResult dr = f.ShowDialog();
                    if (dr == DialogResult.OK)
                    {
                        researchList.Columns = f.Columns;
                        Settings.UI.MainWindow.Research.Columns.Clear();
                        Settings.UI.MainWindow.Research.Columns.AddRange(researchList.Columns.Cast<ResearchColumnSettings>());
                    }
                }
            }

            if (multiPanel.SelectedPage == mailMessagesPage)
            {
                using (EveMailMessagesColumnsSelectWindow f =
                    new EveMailMessagesColumnsSelectWindow(mailMessagesList.Columns.Cast<EveMailMessageColumnSettings>()))
                {
                    DialogResult dr = f.ShowDialog();
                    if (dr == DialogResult.OK)
                    {
                        mailMessagesList.Columns = f.Columns;
                        Settings.UI.MainWindow.EVEMailMessages.Columns.Clear();
                        Settings.UI.MainWindow.EVEMailMessages.Columns.AddRange(
                            mailMessagesList.Columns.Cast<EveMailMessageColumnSettings>());
                    }
                }
            }

            if (multiPanel.SelectedPage == eveNotificationsPage)
            {
                using (EveNotificationsColumnsSelectWindow f =
                    new EveNotificationsColumnsSelectWindow(eveNotificationsList.Columns.Cast<EveNotificationColumnSettings>()))
                {
                    DialogResult dr = f.ShowDialog();
                    if (dr == DialogResult.OK)
                    {
                        eveNotificationsList.Columns = f.Columns;
                        Settings.UI.MainWindow.EVENotifications.Columns.Clear();
                        Settings.UI.MainWindow.EVENotifications.Columns.AddRange(
                            eveNotificationsList.Columns.Cast<EveNotificationColumnSettings>());
                    }
                }
            }
        }

        /// <summary>
        /// Auto-Sizes the columns width of the page list.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void autoSizeColumnMenuItem_Click(object sender, EventArgs e)
        {
            IListView list = multiPanel.SelectedPage.Controls.OfType<IListView>().FirstOrDefault();

            list?.AutoResizeColumns();
        }

        /// <summary>
        /// Hide/Show the inactive entries.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void hideInactiveMenuItem_Click(object sender, EventArgs e)
        {
            bool hideInactive = true;

            if (multiPanel.SelectedPage == ordersPage)
            {
                hideInactive = Settings.UI.MainWindow.MarketOrders.HideInactiveOrders;
                Settings.UI.MainWindow.MarketOrders.HideInactiveOrders = !hideInactive;
                ordersList.UpdateColumns();
            }

            if (multiPanel.SelectedPage == contractsPage)
            {
                hideInactive = Settings.UI.MainWindow.Contracts.HideInactiveContracts;
                Settings.UI.MainWindow.Contracts.HideInactiveContracts = !hideInactive;
                contractsList.UpdateColumns();
            }

            if (multiPanel.SelectedPage == jobsPage)
            {
                hideInactive = Settings.UI.MainWindow.IndustryJobs.HideInactiveJobs;
                Settings.UI.MainWindow.IndustryJobs.HideInactiveJobs = !hideInactive;
                jobsList.UpdateColumns();
            }
            hideInactiveMenuItem.Text = !hideInactive ? "Unhide Inactive" : "Hide Inactive";
        }

        /// <summary>
        /// Switches between Abbreviating/Full number format.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private async void numberAbsFormatMenuItem_Click(object sender, EventArgs e)
        {
            if (multiPanel.SelectedPage == assetsPage)
            {
                bool numberFormat = Settings.UI.MainWindow.Assets.NumberAbsFormat;
                numberAbsFormatMenuItem.Text = !numberFormat ? "Number Full Format" : "Number Abbreviating Format";
                Settings.UI.MainWindow.Assets.NumberAbsFormat = !numberFormat;
                await assetsList.UpdateColumnsAsync();
            }

            if (multiPanel.SelectedPage == ordersPage)
            {
                bool numberFormat = Settings.UI.MainWindow.MarketOrders.NumberAbsFormat;
                numberAbsFormatMenuItem.Text = !numberFormat ? "Number Full Format" : "Number Abbreviating Format";
                Settings.UI.MainWindow.MarketOrders.NumberAbsFormat = !numberFormat;
                ordersList.UpdateColumns();
            }

            if (multiPanel.SelectedPage == contractsPage)
            {
                bool numberFormat = Settings.UI.MainWindow.Contracts.NumberAbsFormat;
                numberAbsFormatMenuItem.Text = !numberFormat ? "Number Full Format" : "Number Abbreviating Format";
                Settings.UI.MainWindow.Contracts.NumberAbsFormat = !numberFormat;
                contractsList.UpdateColumns();
            }

            if (multiPanel.SelectedPage == walletJournalPage)
            {
                bool numberFormat = Settings.UI.MainWindow.WalletJournal.NumberAbsFormat;
                numberAbsFormatMenuItem.Text = !numberFormat ? "Number Full Format" : "Number Abbreviating Format";
                Settings.UI.MainWindow.WalletJournal.NumberAbsFormat = !numberFormat;
                walletJournalList.UpdateColumns();
            }

            if (multiPanel.SelectedPage == walletTransactionsPage)
            {
                bool numberFormat = Settings.UI.MainWindow.WalletTransactions.NumberAbsFormat;
                numberAbsFormatMenuItem.Text = !numberFormat ? "Number Full Format" : "Number Abbreviating Format";
                Settings.UI.MainWindow.WalletTransactions.NumberAbsFormat = !numberFormat;
                walletTransactionsList.UpdateColumns();
            }
        }

        /// <summary>
        /// Displays only the entries issued for character.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void showOnlyCharMenuItem_Click(object sender, EventArgs e)
        {
            if (multiPanel.SelectedPage == ordersPage)
            {
                ordersList.ShowIssuedFor = showOnlyCharMenuItem.Checked ? IssuedFor.Character : IssuedFor.All;
                showOnlyCorpMenuItem.Checked = ordersList.ShowIssuedFor == IssuedFor.Corporation;
            }

            if (multiPanel.SelectedPage == contractsPage)
            {
                contractsList.ShowIssuedFor = showOnlyCharMenuItem.Checked ? IssuedFor.Character : IssuedFor.All;
                showOnlyCorpMenuItem.Checked = contractsList.ShowIssuedFor == IssuedFor.Corporation;
            }

            if (multiPanel.SelectedPage == jobsPage)
            {
                jobsList.ShowIssuedFor = showOnlyCharMenuItem.Checked ? IssuedFor.Character : IssuedFor.All;
                showOnlyCorpMenuItem.Checked = jobsList.ShowIssuedFor == IssuedFor.Corporation;
            }
        }

        /// <summary>
        /// Displays only the entries issued for corporation.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void showOnlyCorpMenuItem_Click(object sender, EventArgs e)
        {
            if (multiPanel.SelectedPage == ordersPage)
            {
                ordersList.ShowIssuedFor = showOnlyCorpMenuItem.Checked ? IssuedFor.Corporation : IssuedFor.All;
                showOnlyCharMenuItem.Checked = ordersList.ShowIssuedFor == IssuedFor.Character;
            }

            if (multiPanel.SelectedPage == contractsPage)
            {
                contractsList.ShowIssuedFor = showOnlyCorpMenuItem.Checked ? IssuedFor.Corporation : IssuedFor.All;
                showOnlyCharMenuItem.Checked = contractsList.ShowIssuedFor == IssuedFor.Character;
            }

            if (multiPanel.SelectedPage == jobsPage)
            {
                jobsList.ShowIssuedFor = showOnlyCorpMenuItem.Checked ? IssuedFor.Corporation : IssuedFor.All;
                showOnlyCharMenuItem.Checked = jobsList.ShowIssuedFor == IssuedFor.Character;
            }
        }

        /// <summary>
        /// Handles the DropDownOpening event of the readingPaneMenuItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void readingPaneMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            string paneSetting = ReadingPanePositioning.Off.ToString();

            if (multiPanel.SelectedPage == mailMessagesPage)
                paneSetting = Settings.UI.MainWindow.EVEMailMessages.ReadingPanePosition.ToString();

            if (multiPanel.SelectedPage == eveNotificationsPage)
                paneSetting = Settings.UI.MainWindow.EVENotifications.ReadingPanePosition.ToString();

            foreach (ToolStripMenuItem menuItem in readingPaneMenuItem.DropDownItems)
            {
                menuItem.Checked = (string)menuItem.Tag == paneSetting;
            }
        }

        /// <summary>
        /// Handles the Click event of the paneRightMenuItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void paneRightMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ToolStripMenuItem menuItem in readingPaneMenuItem.DropDownItems)
            {
                menuItem.Checked = false;
            }

            paneRightMenuItem.Checked = true;

            if (multiPanel.SelectedPage == mailMessagesPage)
            {
                mailMessagesList.PanePosition = ReadingPanePositioning.Right;
                Settings.UI.MainWindow.EVEMailMessages.ReadingPanePosition = mailMessagesList.PanePosition;
            }

            if (multiPanel.SelectedPage == eveNotificationsPage)
            {
                eveNotificationsList.PanePosition = ReadingPanePositioning.Right;
                Settings.UI.MainWindow.EVENotifications.ReadingPanePosition = eveNotificationsList.PanePosition;
            }
        }

        /// <summary>
        /// Handles the Click event of the paneBottomMenuItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void paneBottomMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ToolStripMenuItem menuItem in readingPaneMenuItem.DropDownItems)
            {
                menuItem.Checked = false;
            }

            paneBottomMenuItem.Checked = true;

            if (multiPanel.SelectedPage == mailMessagesPage)
            {
                mailMessagesList.PanePosition = ReadingPanePositioning.Bottom;
                Settings.UI.MainWindow.EVEMailMessages.ReadingPanePosition = mailMessagesList.PanePosition;
            }

            if (multiPanel.SelectedPage == eveNotificationsPage)
            {
                eveNotificationsList.PanePosition = ReadingPanePositioning.Bottom;
                Settings.UI.MainWindow.EVENotifications.ReadingPanePosition = eveNotificationsList.PanePosition;
            }
        }

        /// <summary>
        /// Handles the Click event of the paneOffMenuItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void paneOffMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ToolStripMenuItem menuItem in readingPaneMenuItem.DropDownItems)
            {
                menuItem.Checked = false;
            }

            paneOffMenuItem.Checked = true;

            if (multiPanel.SelectedPage == mailMessagesPage)
            {
                mailMessagesList.PanePosition = ReadingPanePositioning.Off;
                Settings.UI.MainWindow.EVEMailMessages.ReadingPanePosition = mailMessagesList.PanePosition;
            }

            if (multiPanel.SelectedPage == eveNotificationsPage)
            {
                eveNotificationsList.PanePosition = ReadingPanePositioning.Off;
                Settings.UI.MainWindow.EVENotifications.ReadingPanePosition = eveNotificationsList.PanePosition;
            }
        }

        /// <summary>
        /// Handles the Click event of the combatLogMenuItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void combatLogMenuItem_Click(object sender, EventArgs e)
        {
            Settings.UI.MainWindow.CombatLog.ShowCondensedLogs = combatLogMenuItem.Checked;
            killLogList.UpdateKillLogView();
            UpdatePageControls();
        }

        /// <summary>
        /// Handles the Click event of the showOnlyExtractorMenuItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void showOnlyExtractorMenuItem_Click(object sender, EventArgs e)
        {
            Settings.UI.MainWindow.Planetary.ShowEcuOnly = showOnlyExtractorMenuItem.Checked;
            planetaryList.UpdateColumns();
        }

        #endregion


        #endregion


        #endregion


        #region Generic Helper Methods

        /// <summary>
        /// Creates the group menu list.
        /// </summary>
        /// <typeparam name="T">The grouping type.</typeparam>
        /// <typeparam name="T1">The grouping base type.</typeparam>
        /// <param name="list">The list.</param>
        private void CreateGroupMenuList<T, T1>(IListView list)
            where T : T1
        {
            foreach (ToolStripButton menu in EnumExtensions.GetValues<T>().Select(
                grouping => new { grouping, group = grouping as Enum }).Where(
                    menu => menu.group != null).Select(
                        menu =>
                        {
                            ToolStripButton tsb;
                            ToolStripButton tempToolStripButton = null;
                            try
                            {
                                tempToolStripButton = new ToolStripButton(menu.group.GetHeader())
                                {
                                    Checked = list.Grouping.CompareTo(menu.@group) == 0,
                                    Tag = menu.grouping
                                };

                                tsb = tempToolStripButton;
                                tempToolStripButton = null;
                            }
                            finally
                            {
                                tempToolStripButton?.Dispose();
                            }
                            return tsb;
                        }))
            {
                groupMenu.DropDownItems.Add(menu);
            }
        }

        /// <summary>
        /// Sets and stores in settings the GroupBy selection.
        /// </summary>
        /// <typeparam name="T">The grouping type.</typeparam>
        /// <typeparam name="T1">The grouping base type.</typeparam>
        /// <param name="item">The item.</param>
        /// <param name="list">The list.</param>
        private void GroupMenuSetting<T, T1>(ToolStripItem item, IListView list)
            where T : T1
        {
            Enum grouping = item.Tag as Enum;
            if (grouping == null)
                return;

            list.Grouping = grouping;
            T obj = default(T);

            if (obj is AssetGrouping)
                m_character.UISettings.AssetsGroupBy = (AssetGrouping)grouping;

            if (obj is MarketOrderGrouping)
                m_character.UISettings.OrdersGroupBy = (MarketOrderGrouping)grouping;

            if (obj is ContractGrouping)
                m_character.UISettings.ContractsGroupBy = (ContractGrouping)grouping;

            if (obj is WalletJournalGrouping)
                m_character.UISettings.WalletJournalGroupBy = (WalletJournalGrouping)grouping;

            if (obj is WalletTransactionGrouping)
                m_character.UISettings.WalletTransactionsGroupBy = (WalletTransactionGrouping)grouping;

            if (obj is IndustryJobGrouping)
                m_character.UISettings.JobsGroupBy = (IndustryJobGrouping)grouping;

            if (obj is PlanetaryGrouping)
                m_character.UISettings.PlanetaryGroupBy = (PlanetaryGrouping)grouping;

            if (obj is EVEMailMessagesGrouping)
                m_character.UISettings.EVEMailMessagesGroupBy = (EVEMailMessagesGrouping)grouping;

            if (obj is EVENotificationsGrouping)
                m_character.UISettings.EVENotificationsGroupBy = (EVENotificationsGrouping)grouping;
        }

        #endregion


        #region Helper Methods

        /// <summary>
        /// Clears the notifications.
        /// </summary>
        internal void ClearNotifications()
        {
            notificationList.Notifications = null;
        }

        /// <summary>
        /// Updates the list search text filter.
        /// </summary>
        private void UpdateListSearchTextFilter()
        {
            IListView list = multiPanel.SelectedPage.Controls.OfType<IListView>().FirstOrDefault();

            if (list == null)
                return;

            list.TextFilter = searchTextBox.Text;
        }

        /// <summary>
        /// Sets the character.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <value>The character.</value>
        public void SetCharacter(Character character)
        {
            if (m_character == character)
                return;

            m_character = character;
            CompleteControlsInitialization();
        }

        /// <summary>
        /// Completes the controls initialization.
        /// </summary>
        private void CompleteControlsInitialization()
        {
            CCPCharacter ccpCharacter = m_character as CCPCharacter;

            skillsList.Character = m_character;
            skillQueueList.Character = ccpCharacter;
            employmentList.Character = ccpCharacter;
            standingsList.Character = ccpCharacter;
            contactsList.Character = ccpCharacter;
            factionalWarfareStatsList.Character = ccpCharacter;
            medalsList.Character = ccpCharacter;
            killLogList.Character = ccpCharacter;
            assetsList.Character = ccpCharacter;
            ordersList.Character = ccpCharacter;
            contractsList.Character = ccpCharacter;
            walletJournalList.Character = ccpCharacter;
            walletTransactionsList.Character = ccpCharacter;
            jobsList.Character = ccpCharacter;
            planetaryList.Character = ccpCharacter;
            researchList.Character = ccpCharacter;
            mailMessagesList.Character = ccpCharacter;
            eveNotificationsList.Character = ccpCharacter;
            notificationList.Notifications = null;

            // Create a list of the advanced features
            m_advancedFeatures.AddRange(new[]
            {
                standingsIcon, contactsIcon, factionalWarfareStatsIcon, medalsIcon,
                killLogIcon, assetsIcon, ordersIcon, contractsIcon, walletJournalIcon,
                walletTransactionsIcon, jobsIcon, planetaryIcon, researchIcon, mailMessagesIcon,
                eveNotificationsIcon, calendarEventsIcon
            });

            // Hide all advanced features related controls
            m_advancedFeatures.ForEach(x => x.Visible = false);
            tsPagesSeparator.Visible = featuresMenu.Visible = false;
            tsToggleSeparator.Visible = toggleSkillsIcon.Visible = false;
            toolStripContextual.Visible = false;
            warningLabel.Visible = false;

            if (ccpCharacter == null)
                skillQueueIcon.Visible = employmentIcon.Visible = false;

            // Subscribe event
            EveMonClient.ESIKeyInfoUpdated += EveMonClient_APIKeyInfoUpdated;
        }

        /// <summary>
        /// Gets true if the page in question is enabled.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private bool IsEnabledFeature(string text) => m_character.UISettings.AdvancedFeaturesEnabledPages.Any(x => x == text);

        /// <summary>
        /// Gets the monitors related to the toolstrip button.
        /// </summary>
        /// <param name="button">The button.</param>
        /// <returns></returns>
        private List<IQueryMonitor> GetButtonMonitors(ToolStripItem button)
        {
            MultiPanelPage page = multiPanel.Controls.Cast<MultiPanelPage>().FirstOrDefault(x => x.Name == (string)button.Tag);
            CCPCharacter ccpCharacter = (CCPCharacter)m_character;

            List<IQueryMonitor> monitors = new List<IQueryMonitor>();
            if (page == null)
                return monitors;

            if (Enum.IsDefined(typeof(ESIAPICharacterMethods), page.Tag))
            {
                ESIAPICharacterMethods method =
                    (ESIAPICharacterMethods)Enum.Parse(typeof(ESIAPICharacterMethods), (string)page.Tag);
                if (ccpCharacter.QueryMonitors[method] != null)
                    monitors.Add(ccpCharacter.QueryMonitors[method]);
            }

            if (Enum.IsDefined(typeof(ESIAPIGenericMethods), page.Tag))
            {
                ESIAPIGenericMethods method = (ESIAPIGenericMethods)Enum.Parse(typeof(ESIAPIGenericMethods), (string)page.Tag);
                if (ccpCharacter.QueryMonitors[method] != null)
                    monitors.Add(ccpCharacter.QueryMonitors[method]);
            }

            string corpMethod = $"Corporation{page.Tag}";
            if (Enum.IsDefined(typeof(ESIAPICorporationMethods), corpMethod))
            {
                ESIAPICorporationMethods method =
                    (ESIAPICorporationMethods)Enum.Parse(typeof(ESIAPICorporationMethods), corpMethod);
                if (ccpCharacter.QueryMonitors[method] != null)
                    monitors.Add(ccpCharacter.QueryMonitors[method]);
            }

            return monitors;
        }

        #endregion


        # region Screenshot Method

        /// <summary>
        /// Takes a screeenshot of this character's monitor and returns it (used for PNG exportation)
        /// </summary>
        /// <returns>Screenshot of a character.</returns>
        internal Bitmap GetCharacterScreenshot()
        {
            skillsList.Height = skillsList.PreferredSize.Height;
            skillsList.Dock = DockStyle.None;

            Bitmap bitmap;
            using (Bitmap tempBitmap = new Bitmap(skillsList.Width, skillsList.Height))
            {
                skillsList.DrawToBitmap(tempBitmap, new Rectangle(0, 0, skillsList.Width, skillsList.Height));
                bitmap = (Bitmap)tempBitmap.Clone();
            }

            skillsList.Dock = DockStyle.Fill;

            return bitmap;
        }

        # endregion

    }
}
