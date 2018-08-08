using EVEMon.About;
using EVEMon.ApiCredentialsManagement;
using EVEMon.BlankCharacter;
using EVEMon.CharacterMonitoring;
using EVEMon.CharactersComparison;
using EVEMon.Common;
using EVEMon.Common.CloudStorageServices;
using EVEMon.Common.Collections.Global;
using EVEMon.Common.Constants;
using EVEMon.Common.Controls;
using EVEMon.Common.CustomEventArgs;
using EVEMon.Common.Enumerations;
using EVEMon.Common.Enumerations.UISettings;
using EVEMon.Common.Extensions;
using EVEMon.Common.Factories;
using EVEMon.Common.Helpers;
using EVEMon.Common.Models;
using EVEMon.Common.Notifications;
using EVEMon.Common.Properties;
using EVEMon.Common.Scheduling;
using EVEMon.Common.Serialization.Settings;
using EVEMon.Common.Service;
using EVEMon.Common.SettingsObjects;
using EVEMon.DetailsWindow;
using EVEMon.ImplantControls;
using EVEMon.LogitechG15;
using EVEMon.NotificationWindow;
using EVEMon.PieChart;
using EVEMon.SettingsUI;
using EVEMon.SkillPlanner;
using EVEMon.Updater;
using EVEMon.Watchdog;
using EVEMon.WindowsApi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EVEMon
{
    public sealed partial class MainWindow : EVEMonForm
    {
        #region Fields

        private readonly List<NotificationEventArgs> m_popupNotifications = new List<NotificationEventArgs>();
        private readonly bool m_startMinimized;

        private Form m_trayPopup;
        private DateTime m_nextPopupUpdate = DateTime.UtcNow;
        private ToolStripItem[] m_characterEnabledMenuItems;
        private ToolStripItem[] m_settingsEnabledMenuItems;

        private string m_apiProviderName;
        private bool m_isMouseClicked;
        private bool m_isUpdating;
        private bool m_isUpdatingData;
        private bool m_isShowingUpdateWindow;
        private bool m_isShowingDataUpdateWindow;
        private bool m_isUpdatingTabOrder;
        private bool m_isUpdateEventsSubscribed;
        private bool m_initialized;

        #endregion


        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            RememberPositionKey = "MainWindow";
            notificationList.Notifications = null;

            tabLoadingLabel.Font = FontFactory.GetFont("Tahoma", 11.25F, FontStyle.Bold);
            noCharactersLabel.Font = FontFactory.GetFont("Tahoma", 11.25F, FontStyle.Bold);

            noCharactersLabel.Hide();

            trayIcon.Text = EveMonClient.FileVersionInfo.ProductName;

            lblStatus.Text = $"EVE Time: {DateTime.UtcNow:HH:mm}";
            lblServerStatus.Text = $"|  {EveMonClient.EVEServer?.StatusText ?? EveMonConstants.UnknownText}";

            tsDatafilesLoadingProgressBar.Step =
                (int)Math.Ceiling((double)tsDatafilesLoadingProgressBar.Maximum / EveMonClient.Datafiles.Count);

            foreach (ToolStripItem item in mainMenuBar.Items)
            {
                item.MouseDown += mainMenuBar_MouseDown;
                item.MouseMove += mainMenuBar_MouseMove;
            }

            foreach (ToolStripItem item in mainToolBar.Items)
            {
                item.MouseDown += mainToolBar_MouseDown;
                item.MouseMove += mainToolBar_MouseMove;
            }

            if (EveMonClient.IsDebugBuild)
                DisplayTestMenu();

            m_startMinimized = Environment.GetCommandLineArgs().Contains("-startMinimized");
        }

        /// <summary>
        /// Forces cleanup, we will jump from 50MB to less than 10MB.
        /// </summary>
        private static void TriggerAutoShrink()
        {
            // Quit if the client has been shut down
            if (EveMonClient.Closed)
                return;

            AutoShrink.Dirty(TimeSpan.FromSeconds(5).Seconds);
        }

        #endregion


        #region Loading, closing, resizing, etc

        /// <summary>
        /// Once the window is loaded, we complete initialization.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (DesignMode)
                return;

            m_apiProviderName = EveMonClient.APIProviders?.CurrentProvider?.Name;

            // Collext the menu buttons that get enabled by a character
            m_characterEnabledMenuItems = new ToolStripItem[]
            {
                hideCharacterMenu, deleteCharacterMenu, exportCharacterMenu,
                skillsPieChartMenuItem, implantsMenuItem, showOwnedSkillbooksMenuItem,
                manageCharacterTbMenu,plansTbMenu, tsbManagePlans,
                skillsPieChartTbMenu, tsbImplantGroups, tsbShowOwned
            };

            m_settingsEnabledMenuItems = new ToolStripItem[]
            {
                loadSettingsToolStripMenuItem, resetSettingsToolStripMenuItem,
                saveSettingsToolStripMenuItem, exitToolStripMenuItem,
                dataBrowserMenuItem, blankCreatorToolStripMenuItem,
                optionsToolStripMenuItem,

                resetSettingsToolStripButton, exitToolStripButton, tsbOptions,
                closeToolStripMenuItem
            };

            // Start minimized ?
            if (m_startMinimized)
            {
                WindowState = FormWindowState.Minimized;
                ShowInTaskbar = Settings.UI.MainWindowCloseBehaviour == CloseBehaviour.MinimizeToTaskbar
                                || Settings.UI.SystemTrayIcon == SystemTrayBehaviour.Disabled;
                Visible = ShowInTaskbar;
            }

            // Start the one-second timer 
            EveMonClient.Run(Thread.CurrentThread);

            // Check with NIST that the local clock is synchronized
            TimeCheck.ScheduleCheck(TimeSpan.FromSeconds(1));

            // Notify Gooogle Analytics about start up
            GAnalyticsTracker.TrackStart(GetType());

            // Prepare control's visibility
            menubarToolStripMenuItem.Checked = mainMenuBar.Visible = Settings.UI.MainWindow.ShowMenuBar;
            toolbarToolStripMenuItem.Checked = mainToolBar.Visible = !Settings.UI.MainWindow.ShowMenuBar;

            // Prepare settings controls
            UpdateSettingsControlsVisibility(enabled: false);

            // Show the tab control according to Overview settings
            tcCharacterTabs.Visible = Settings.UI.MainWindow.ShowOverview;

            // Updates the controls visibility according to settings
            UpdateControlsVisibility();

            // Subscribe events
            TimeCheck.TimeCheckCompleted += TimeCheck_TimeCheckCompleted;
            GlobalDatafileCollection.LoadingProgress += GlobalDatafileCollection_LoadingProgress;
            EveMonClient.NotificationSent += EveMonClient_NotificationSent;
            EveMonClient.NotificationInvalidated += EveMonClient_NotificationInvalidated;
            EveMonClient.MonitoredCharacterCollectionChanged += EveMonClient_MonitoredCharacterCollectionChanged;
            EveMonClient.ServerStatusUpdated += EveMonClient_ServerStatusUpdated;
            EveMonClient.QueuedSkillsCompleted += EveMonClient_QueuedSkillsCompleted;
            EveMonClient.SettingsChanged += EveMonClient_SettingsChanged;
            EveMonClient.TimerTick += EveMonClient_TimerTick;
            
            EveMonClient.Trace("Main window - loaded", printMethod: false);
        }

        /// <summary>
        /// Occurs when the window is shown.
        /// </summary>
        /// <param name="e">A <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);

            if (!m_initialized)
                await InitializeData();

            // Welcome message
            TipWindow.ShowTip(this, "startup", "Getting Started", Properties.Resources.
                MessageGettingStarted);
        }

        /// <summary>
        /// Initializes the data.
        /// </summary>
        /// <returns></returns>
        private async Task InitializeData()
        {
            // Load static data
            await GlobalDatafileCollection.LoadAsync();

            // Load cache data
            await TaskHelper.RunIOBoundTaskAsync(() => {
                EveIDToName.InitializeFromFile();
                EveIDToStation.InitializeFromFile();
            });

            // Load characters related settings
            await Settings.ImportDataAsync();

            // Initialize G15
            if (OSFeatureCheck.IsWindowsNT)
                G15Handler.Initialize();

            m_initialized = true;

            // Force cleanup
            TriggerAutoShrink();
        }

        /// <summary>
        /// Occurs whenever the window is resized.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (!m_initialized)
                return;

            UpdateStatusLabel();
            UpdateWindowTitle();
            UpdateNotifications();

            // Updates tray icon visibility
            if (WindowState != FormWindowState.Minimized &&
                Settings.UI.MainWindowCloseBehaviour != CloseBehaviour.MinimizeToTaskbar)
            {
                return;
            }

            trayIcon.Visible = Settings.UI.SystemTrayIcon == SystemTrayBehaviour.AlwaysVisible ||
                               (Settings.UI.SystemTrayIcon == SystemTrayBehaviour.ShowWhenMinimized &&
                                WindowState == FormWindowState.Minimized);

            Visible = Settings.UI.MainWindowCloseBehaviour == CloseBehaviour.MinimizeToTaskbar ||
                Settings.UI.SystemTrayIcon == SystemTrayBehaviour.Disabled;
        }

        /// <summary>
        /// Occurs when the form is going to be closed. 
        /// We may decide to cancel the closing and rather minimize to tray bar.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            // Is there a reason that we should really close the window
            if (!Visible || m_isUpdating || m_isUpdatingData || e.CloseReason == CloseReason.ApplicationExitCall ||
                e.CloseReason == CloseReason.TaskManagerClosing || e.CloseReason == CloseReason.WindowsShutDown)
            {
                GAnalyticsTracker.TrackEnd(GetType());
                return;
            }

            // Should we actually exit ?
            if (Settings.UI.MainWindowCloseBehaviour == CloseBehaviour.Exit)
            {
                // Prevents the closing if we are restoring the settings at that time 
                // or we are still initializing
                // or user has set to upload to cloud storage service provider and it fails
                e.Cancel = Settings.IsRestoring || !m_initialized || !TryUploadToCloudStorageProviderAsync().Result;

                return;
            }

            // If the user has right clicked the task bar item while
            // this window is minimized, and chosen close then the
            // following will evaluate to false and EVEMon will close
            if (WindowState == FormWindowState.Minimized)
                return;

            // Cancel the close operation and minimize the window
            // Display of the tray icon and window will be handled by 
            // MainWindow_Resize
            e.Cancel = true;
            WindowState = FormWindowState.Minimized;
        }

        /// <summary>
        /// When closing, ensures we're leaving with a proper state.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);

            // Hide the system tray icons
            niAlertIcon.Visible = false;
            trayIcon.Visible = false;

            // Unsubscribe events
            TimeCheck.TimeCheckCompleted -= TimeCheck_TimeCheckCompleted;
            GlobalDatafileCollection.LoadingProgress -= GlobalDatafileCollection_LoadingProgress;
            EveMonClient.NotificationSent -= EveMonClient_NotificationSent;
            EveMonClient.NotificationInvalidated -= EveMonClient_NotificationInvalidated;
            EveMonClient.MonitoredCharacterCollectionChanged -= EveMonClient_MonitoredCharacterCollectionChanged;
            EveMonClient.ServerStatusUpdated -= EveMonClient_ServerStatusUpdated;
            EveMonClient.QueuedSkillsCompleted -= EveMonClient_QueuedSkillsCompleted;
            EveMonClient.SettingsChanged -= EveMonClient_SettingsChanged;
            EveMonClient.TimerTick -= EveMonClient_TimerTick;
        }

        /// <summary>
        /// On minimizing, we force garbage collection.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnDeactivate(EventArgs e)
        {
            base. OnDeactivate(e);

            // Only cleanup if we're deactivating to the minimized state (e.g. systray)
            if (WindowState == FormWindowState.Minimized)
                TriggerAutoShrink();
        }

        /// <summary>
        /// Callback for time synchronization.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TimeCheckSyncEventArgs"/> instance containing the event data.</param>
        private void TimeCheck_TimeCheckCompleted(object sender, TimeCheckSyncEventArgs e)
        {
            if (!Settings.Updates.CheckTimeOnStartup || e.IsSynchronised ||
                (e.ServerTimeToLocalTime == DateTime.MinValue.ToLocalTime()))
            {
                return;
            }

            using (TimeCheckNotification timeDialog = new TimeCheckNotification(e.ServerTimeToLocalTime, e.LocalTime))
            {
                timeDialog.ShowDialog(this);
            }
        }

        #endregion


        #region Tabs management

        /// <summary>
        /// Occurs when the monitored characters collection is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EveMonClient_MonitoredCharacterCollectionChanged(object sender, EventArgs e)
        {
            if (m_isUpdatingTabOrder)
                return;

            mainLoadingThrobber.State = ThrobberState.Stopped;
            mainLoadingThrobber.Hide();
            tabLoadingLabel.Hide();

            UpdateSettingsControlsVisibility(enabled: true);

            UpdateTabs();
        }

        /// <summary>
        /// Updates the tab pages.
        /// </summary>
        private void UpdateTabs()
        {
            // Layouts the tab pages
            LayoutTabPages();

            // Updates the controls related to tab selection
            UpdateControlsOnTabSelectionChange();
        }

        /// <summary>
        /// Layouts the tab pages.
        /// </summary>
        private void LayoutTabPages()
        {
            this.LockWindowUpdate(true);

            try
            {
                TabPage selectedTab = tcCharacterTabs.SelectedTab;

                // Collect the existing pages
                Dictionary<Character, TabPage> pages = tcCharacterTabs.TabPages.Cast<TabPage>().Where(
                    page => page.Tag is Character).ToDictionary(page => (Character)page.Tag);

                // Rebuild the pages
                int index = 0;
                foreach (Character character in EveMonClient.MonitoredCharacters)
                {
                    // Retrieve the current page, or null if we're past the limits
                    TabPage currentPage = index < tcCharacterTabs.TabCount ? tcCharacterTabs.TabPages[index] : null;

                    // Is it the overview ? We'll deal with it later
                    if (currentPage == tpOverview)
                        currentPage = ++index < tcCharacterTabs.TabCount ? tcCharacterTabs.TabPages[index] : null;

                    // Does the page match with the character ?
                    if ((Character)currentPage?.Tag != character)
                    {
                        // Retrieve the page when it was previously created
                        // Is the page later in the collection ?
                        TabPage page;
                        if (pages.TryGetValue(character, out page))
                            tcCharacterTabs.TabPages.Remove(page); // Remove the page from old location
                        else
                            page = CreateTabPage(character); // Create a new page

                        // Inserts the page in the proper location
                        tcCharacterTabs.TabPages.Insert(index, page);
                    }

                    // Remove processed character from the dictionary and move forward
                    if (character != null)
                        pages.Remove(character);

                    index++;
                }

                // Ensures the overview has been added when necessary
                AddOverviewTab();

                // Dispose the removed tabs
                foreach (TabPage page in pages.Values)
                {
                    page.Dispose();
                }

                // Reselect
                if (selectedTab != null && tcCharacterTabs.TabPages.Contains(selectedTab))
                    tcCharacterTabs.SelectedTab = selectedTab;
                else if (tcCharacterTabs.TabCount > 0)
                    tcCharacterTabs.SelectedTab = tcCharacterTabs.TabPages[0];
            }
            finally
            {
                tcCharacterTabs.Visible = tcCharacterTabs.Controls.Count > 0;
                this.LockWindowUpdate(false);
            }
        }

        /// <summary>
        /// Adds the overview tab.
        /// </summary>
        private void AddOverviewTab()
        {
            if (tpOverview == null)
                return;

            if (Settings.UI.MainWindow.ShowOverview)
            {
                // Trim the overview page index
                int overviewIndex = Math.Max(0, Math.Min(tcCharacterTabs.TabCount - 1, Settings.UI.MainWindow.OverviewIndex));

                // Inserts it if it doesn't exist
                if (!tcCharacterTabs.TabPages.Contains(tpOverview))
                    tcCharacterTabs.TabPages.Insert(overviewIndex, tpOverview);

                // If it exist insert it at the correct position
                if (tcCharacterTabs.TabPages.IndexOf(tpOverview) != overviewIndex)
                {
                    tcCharacterTabs.TabPages.Remove(tpOverview);
                    tcCharacterTabs.TabPages.Insert(overviewIndex, tpOverview);
                }

                // Select the Overview tab if it's the only tab
                if (tcCharacterTabs.TabCount == 1)
                    tcCharacterTabs.SelectedTab = tpOverview;

                return;
            }

            // Or remove it when it should not be here anymore
            if (tcCharacterTabs.TabPages.Contains(tpOverview))
                tcCharacterTabs.TabPages.Remove(tpOverview);
        }

        /// <summary>
        /// Creates the tab page for the given character.
        /// </summary>
        /// <param name="character">The character</param>
        /// <returns>A tab page.</returns>
        private static TabPage CreateTabPage(Character character)
        {
            // Create the tab
            TabPage page;
            TabPage tempPage = null;
            try
            {
                tempPage = new TabPage(character.Name);
                tempPage.UseVisualStyleBackColor = true;
                tempPage.Padding = new Padding(5);
                tempPage.Tag = character;

                // Create the character monitor
                CreateCharacterMonitor(character, tempPage);

                page = tempPage;
                tempPage = null;
            }
            finally
            {
                tempPage?.Dispose();
            }

            return page;
        }

        /// <summary>
        /// Creates the character monitor.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <param name="tabPage">The tab page.</param>
        private static void CreateCharacterMonitor(Character character, Control tabPage)
        {
            CharacterMonitor tempMonitor = null;
            try
            {
                tempMonitor = new CharacterMonitor(character);
                tempMonitor.Parent = tabPage;
                tempMonitor.Dock = DockStyle.Fill;

                CharacterMonitor monitor = tempMonitor;
                tempMonitor = null;
                tabPage.Controls.Add(monitor);
            }
            finally
            {
                tempMonitor?.Dispose();
            }
        }

        /// <summary>
        /// When tabs are moved by the user (through drag'n drop), we update the settings.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tcCharacterTabs_DragDrop(object sender, DragEventArgs e)
        {
            Settings.UI.MainWindow.OverviewIndex = tcCharacterTabs.TabPages.IndexOf(tpOverview);

            IEnumerable<Character> order = tcCharacterTabs.TabPages.Cast<TabPage>().Where(
                page => page.Tag is Character).Select(page => page.Tag as Character);

            m_isUpdatingTabOrder = true;
            EveMonClient.MonitoredCharacters.Update(order);
            m_isUpdatingTabOrder = false;
        }

        /// <summary>
        /// Occurs whenever the user changes the tabs selection.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tcCharacterTabs_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateControlsOnTabSelectionChange();
        }

        /// <summary>
        /// Enables / disables the menu buttons (remove chars, plans, etc).
        /// </summary>
        private void UpdateControlsOnTabSelectionChange()
        {
            // Enable or disable the menu buttons
            foreach (ToolStripItem item in m_characterEnabledMenuItems)
            {
                item.Enabled = GetCurrentCharacter() != null;
            }
        }

        /// <summary>
        /// When a character is clicked on the overview, select the appropriate tab.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void overview_CharacterClicked(object sender, CharacterChangedEventArgs e)
        {
            foreach (TabPage tab in tcCharacterTabs.TabPages.Cast<TabPage>().Select(
                tab => new { tab, character = tab.Tag as Character }).Where(
                    tab => tab.character == e.Character).Select(character => character.tab))
            {
                tcCharacterTabs.SelectedTab = tab;
                return;
            }
        }

        /// <summary>
        /// Gets the currently selected character; or null when the tabs selection does not match.
        /// </summary>
        /// <returns></returns>
        private Character GetCurrentCharacter() => tcCharacterTabs.SelectedTab?.Tag as Character;

        /// <summary>
        /// Gets the currently selected monitor; or null when the tabs selection does not match.
        /// </summary>
        /// <returns></returns>
        private CharacterMonitor GetCurrentMonitor()
        {
            if (tcCharacterTabs.SelectedTab == null || tcCharacterTabs.SelectedTab.Controls.Count == 0)
                return null;

            return tcCharacterTabs.SelectedTab.Controls[0] as CharacterMonitor;
        }

        /// <summary>
        /// Handles the LoadingProgress event of the GlobalDatafileCollection control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        private void GlobalDatafileCollection_LoadingProgress(object sender, EventArgs e)
        {
            tsDatafilesLoadingProgressBar.PerformStep();
        }

        #endregion


        #region Notifications, server status change, skill completion sound

        /// <summary>
        /// Occurs when the server status is updated.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EveMonClient_ServerStatusUpdated(object sender, EveServerEventArgs e)
        {
            lblServerStatus.Text = $"|  {e.Server.StatusText}";
        }

        /// <summary>
        /// Update the notifications list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EveMonClient_NotificationInvalidated(object sender, NotificationInvalidationEventArgs e)
        {
            UpdateNotifications();
        }

        /// <summary>
        /// Update the notifications list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EveMonClient_NotificationSent(object sender, NotificationEventArgs e)
        {
            // Updates the notifications list of the main window
            UpdateNotifications();

            // Takes care of the tooltip
            NotificationCategorySettings catSettings = Settings.Notifications.Categories[e.Category];
            ToolTipNotificationBehaviour behaviour = catSettings.ToolTipBehaviour;
            if (behaviour == ToolTipNotificationBehaviour.Never)
                return;

            // Add and reorder by API key and character
            m_popupNotifications.Add(e);

            // Group by API key
            IEnumerable<IGrouping<long, NotificationEventArgs>> groups = m_popupNotifications
                .GroupBy(
                    notification =>
                    {
                        // It's an API server related notification
                        if (notification.Sender == null)
                            return 0;

                        // It's an API key related notification
                        if (notification.SenderAPIKey != null)
                            return notification.SenderAPIKey.ID;

                        // It's a corporation related notification
                        if (notification.SenderCorporation != null)
                            return notification.SenderCorporation.ID;

                        // It's a character related notification
                        return notification.SenderCharacter is UriCharacter
                            ? 1
                            : notification.SenderCharacter.CharacterID;
                    });

            // Add every group, order by character's name, accounts being on top
            List<NotificationEventArgs> newList = new List<NotificationEventArgs>();
            foreach (IGrouping<long, NotificationEventArgs> group in groups)
            {
                newList.AddRange(group.OrderBy(x => x.SenderCharacter?.Name ?? String.Empty));
            }

            m_popupNotifications.Clear();
            m_popupNotifications.AddRange(newList);

            // If the info must be presented once only, schedule a deletion
            if (behaviour == ToolTipNotificationBehaviour.Once)
            {
                NotificationEventArgs.ScheduleAction(TimeSpan.FromMinutes(1),
                    () =>
                    {
                        if (!m_popupNotifications.Contains(e))
                            return;

                        m_popupNotifications.Remove(e);

                        if (m_popupNotifications.Count == 0)
                            niAlertIcon.Visible = false;
                    });
            }

            // Now check whether we must 
            DisplayTooltipNotifications();
        }

        /// <summary>
        /// Update the notifications list.
        /// </summary>
        private void UpdateNotifications()
        {
            if (WindowState == FormWindowState.Minimized)
                return;

            notificationList.Notifications = EveMonClient.Notifications.Where(x => x.Sender == null || x.SenderAPIKey != null);
        }

        /// <summary>
        /// Displays the tooltip.
        /// </summary>
        private void DisplayTooltipNotifications()
        {
            // Ensures the active entries do not prohibit EVEMon to fire tooltips
            if (Scheduler.SilentMode)
            {
                niAlertIcon.Visible = false;
                return;
            }

            int maxlevel = 0,
                textlenght = 0,
                count = 0;
            Object lastSender = m_popupNotifications[0].Sender;
            StringBuilder builder = new StringBuilder();

            // We build the tooltip notification text
            foreach (NotificationEventArgs notification in m_popupNotifications)
            {
                // Tooltip notification text space is limited 
                // so we apply restrains in how many notifications will be shown
                if (textlenght <= 100)
                {
                    bool senderIsCharacter = (notification.Sender != null) &&
                                             (notification.Sender == notification.SenderCharacter);

                    bool senderIsCorporation = (notification.Sender != null) &&
                                               (notification.Sender == notification.SenderCorporation);

                    string tooltipText = notification.ToString();
                    maxlevel = Math.Max(maxlevel, (int)notification.Priority);
                    int level = (int)notification.Priority;

                    if (notification.Sender != lastSender)
                        builder.AppendLine();

                    lastSender = notification.Sender;

                    if (senderIsCharacter || senderIsCorporation)
                    {
                        switch (level)
                        {
                            case 0:
                                tooltipText = tooltipText.Replace(".",
                                    $" for {(senderIsCharacter ? notification.SenderCharacter.Name : notification.SenderCorporation.Name)}.");
                                break;
                            case 1:
                                tooltipText = tooltipText.Replace("This character", senderIsCharacter
                                    ? notification.SenderCharacter.Name
                                    : notification.SenderCorporation.Name);

                                break;
                            case 2:
                                tooltipText = tooltipText.Replace(".",
                                    $" of {(senderIsCharacter ? notification.SenderCharacter.Name : notification.SenderCorporation.Name)}.");
                                break;
                        }
                    }

                    builder.AppendLine(tooltipText);
                }
                // When the text gets too long we add an informative text once
                else if (count == 0)
                {
                    builder.AppendLine("\r\nMore notifications are available.\nCheck character monitor for more information.");
                    count++;
                }

                textlenght = builder.Length;
            }
            niAlertIcon.BalloonTipText = builder.ToString();

            // Icon 
            // (In Win7 icon is displayed only when there is a BalloonTipTitle present,
            // which makes this part of the code useless)
            switch (maxlevel)
            {
                case 0:
                    niAlertIcon.BalloonTipIcon = ToolTipIcon.Info;
                    break;
                case 1:
                    niAlertIcon.BalloonTipIcon = ToolTipIcon.Warning;
                    break;
                case 2:
                    niAlertIcon.BalloonTipIcon = ToolTipIcon.Error;
                    break;
                default:
                    throw new NotImplementedException();
            }

            // Display tooltip notification
            niAlertIcon.Visible = true;
            niAlertIcon.ShowBalloonTip(10000);

            // Next auto update
            m_nextPopupUpdate = DateTime.UtcNow.AddMinutes(1);
        }

        /// <summary>
        /// When the alerts ballon is clicked, we clear everything.
        /// </summary>
        private void OnAlertBalloonClicked()
        {
            niAlertIcon.Visible = false;
            m_popupNotifications.Clear();
        }

        /// <summary>
        /// Checks whether a sound must be played on skill training.
        /// </summary>
        /// <returns></returns>
        private static void TryPlaySkillCompletionSound()
        {
            // Returns if the user disabled the option
            if (!Settings.Notifications.PlaySoundOnSkillCompletion)
                return;

            // Checks the schedule 
            if (Scheduler.SilentMode)
                return;

            // Play the sound
            using (SoundPlayer sp = new SoundPlayer(Resources.SkillTrained))
            {
                sp.Play();
            }
        }

        /// <summary>
        /// Occurs when the alerts ballon icon is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void niAlertIcon_BalloonTipClicked(object sender, EventArgs e)
        {
            OnAlertBalloonClicked();
        }

        /// <summary>
        /// Occurs when the alerts icon is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void niAlertIcon_Click(object sender, EventArgs e)
        {
            OnAlertBalloonClicked();
        }

        /// <summary>
        /// Occurs when the alerts icon is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void niAlertIcon_MouseClick(object sender, MouseEventArgs e)
        {
            OnAlertBalloonClicked();
        }

        /// <summary>
        /// Occurs when skills have been completed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void EveMonClient_QueuedSkillsCompleted(object sender, QueuedSkillsEventArgs e)
        {
            // Play a sound
            TryPlaySkillCompletionSound();
        }

        #endregion


        #region Per-second updates

        /// <summary>
        /// Occurs every second.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EveMonClient_TimerTick(object sender, EventArgs e)
        {
            UpdateStatusLabel();
            UpdateWindowTitle();

            // Checks whether the tooltip must be displayed
            if (m_popupNotifications.Count != 0 && DateTime.UtcNow > m_nextPopupUpdate)
                DisplayTooltipNotifications();

            charactersComparisonMenuItem.Enabled =
                characterComparisonToolStripButton.Enabled =
                    EveMonClient.Characters != null && EveMonClient.Characters.Any();
        }

        /// <summary>
        /// Updates the status bar.
        /// </summary>
        private void UpdateStatusLabel()
        {
            if (tsDatafilesLoadingProgressBar.Visible &&
                tsDatafilesLoadingProgressBar.Value == tsDatafilesLoadingProgressBar.Maximum)
            {
                tsDatafilesLoadingProgressBar.Visible = false;
            }

            if (WindowState == FormWindowState.Minimized)
                return;

            DateTime serverTime = EveMonClient.EVEServer?.ServerDateTime ?? DateTime.UtcNow;
            lblStatus.Text = $"EVE Time: {serverTime:HH:mm}";
            lblStatus.ToolTipText = $"YC{serverTime.Year - 1898} ({serverTime.Date.ToShortDateString()})";
        }

        /// <summary>
        /// Updates the window's title.
        /// </summary>
        private void UpdateWindowTitle()
        {
            if (WindowState == FormWindowState.Minimized)
                return;

            // If character's trainings must be displayed in title
            if (!Settings.UI.MainWindow.ShowCharacterInfoInTitleBar)
            {
                Text = EveMonClient.FileVersionInfo.ProductName;
                return;
            }

            StringBuilder builder;

            // Retrieve the selected character
            CCPCharacter selectedChar = GetCurrentCharacter() as CCPCharacter;

            int trimTimeSpanComponents = 0;

            do
            {
                builder = new StringBuilder();

                // Scroll through the ordered list of chars in training
                SortedList<TimeSpan, CCPCharacter> orderedTrainingTimes = GetOrderedCharactersTrainingTime();
                foreach (TimeSpan ts in orderedTrainingTimes.Keys)
                {
                    CCPCharacter character = orderedTrainingTimes[ts];

                    TimeSpan trimmedTime = ts;

                    // First pass we remove the seconds from the TimeSpan,
                    // if training time is over one minute
                    if (trimTimeSpanComponents >= 1 && ts.Hours >= 0 && ts.Minutes > 0)
                        trimmedTime = trimmedTime.Add(TimeSpan.FromSeconds(0 - ts.Seconds));

                    // Second pass we remove the minutes from the TimeSpan,
                    // if training time is over one hour
                    if (trimTimeSpanComponents >= 2 && ts.Hours > 0)
                        trimmedTime = trimmedTime.Add(TimeSpan.FromMinutes(0 - ts.Minutes));

                    switch (Settings.UI.MainWindow.TitleFormat)
                    {
                        // (Default) Single Char - finishing skill next
                        case MainWindowTitleFormat.Default:
                        case MainWindowTitleFormat.NextCharToFinish:
                            if (builder.Length == 0)
                                builder.Append(AppendCharacterTrainingTime(character, trimmedTime));
                            break;

                        // Single Char - selected char
                        case MainWindowTitleFormat.SelectedChar:
                            if (selectedChar == character)
                                builder.Append(AppendCharacterTrainingTime(character, trimmedTime));
                            break;

                        // Multi Char - finishing skill next first
                        case MainWindowTitleFormat.AllCharacters:
                            if (builder.Length > 0)
                                builder.Append(" | ");
                            builder.Append(AppendCharacterTrainingTime(character, trimmedTime));
                            break;

                        // Multi Char - selected char first
                        case MainWindowTitleFormat.AllCharactersButSelectedOneAhead:
                            // Selected char ? Insert at the beginning
                            if (selectedChar == character)
                            {
                                // Precreate the string for this char
                                StringBuilder subBuilder = new StringBuilder();
                                subBuilder.Append(AppendCharacterTrainingTime(character, trimmedTime));
                                if (builder.Length > 0)
                                    subBuilder.Append(" | ");

                                // Insert it at the beginning
                                builder.Insert(0, subBuilder.ToString());
                            }
                            // Non-selected char ? Same as "3"
                            else
                            {
                                if (builder.Length > 0)
                                    builder.Append(" | ");
                                builder.Append(AppendCharacterTrainingTime(character, trimmedTime));
                            }
                            break;
                    }
                }

                // If we go through the loop again we will remove another component of the TimeSpan
                trimTimeSpanComponents++;
            } // Each pass we remove one component of the time span up until the hours
            while (builder.Length > MaxTitleLength && trimTimeSpanComponents < 3);

            // Adds EVEMon at the end if there is space in the title bar
            string appSuffix = $" - {EveMonClient.FileVersionInfo.ProductName}";
            if (builder.Length + appSuffix.Length <= MaxTitleLength)
                builder.Append(appSuffix);

            // Set the window title
            Text = builder.ToString();
        }

        /// <summary>
        /// Produces a sorted list of characters in training, ordered from the shortest to the longest training time.
        /// </summary>
        /// <remarks>Pulled this code out of cm_ShortInfoChanged, as I needed to use the returned List in multiple places</remarks>
        /// <returns></returns>
        private SortedList<TimeSpan, CCPCharacter> GetOrderedCharactersTrainingTime()
        {
            SortedList<TimeSpan, CCPCharacter> sortedList = new SortedList<TimeSpan, CCPCharacter>();
            foreach (TabPage tp in tcCharacterTabs.TabPages)
            {
                // Is it a character bound to CCP ?
                if (!(tp.Tag is CCPCharacter))
                    continue;

                // Is it in training ?
                CCPCharacter character = tp.Tag as CCPCharacter;
                if (!character.IsTraining)
                    continue;

                TimeSpan ts = character.CurrentlyTrainingSkill.RemainingTime;

                // While the timespan is not unique, we add 1ms
                while (sortedList.ContainsKey(ts))
                {
                    ts += TimeSpan.FromTicks(1);
                }

                // Add it to the sorted list
                sortedList.Add(ts, character);
            }
            return sortedList;
        }

        /// <summary>
        /// Appends the given training time for the specified character to the provided <see cref="StringBuilder"/>. *
        /// Format is : "1d, 5h, 32m John Doe (Eidetic Memory)"
        /// Used to update the window's title.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <param name="time">The time.</param>
        /// <returns></returns>
        private static string AppendCharacterTrainingTime(Character character, TimeSpan time)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append($"{time.ToDescriptiveText(DescriptiveTextOptions.None)} {character.Name}");

            if (Settings.UI.MainWindow.ShowSkillNameInWindowTitle)
                builder.Append($" ({character.CurrentlyTrainingSkill.SkillName})");

            return builder.ToString();
        }

        #endregion


        #region Updates manager

        /// <summary>
        /// Occurs when a program update is available. Display the information form to the user.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnUpdateAvailable(object sender, UpdateAvailableEventArgs e)
        {
            // Notify the user and prompt him
            if (m_isShowingUpdateWindow)
                return;

            m_isShowingUpdateWindow = true;

            // New release of the same major version available
            if (!String.IsNullOrWhiteSpace(e.UpdateMessage))
            {
                using (UpdateNotifyForm form = new UpdateNotifyForm(e))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        m_isUpdating = true;

                        // Save the settings to make sure we don't lose anything
                        await Settings.SaveImmediateAsync();
                        Close();
                    }
                }
            }
            // new major version release
            else
            {
                string message = $"A new version ({e.NewestVersion}) is available at " +
                    $"{NetworkConstants.EVEMonMainPage}.{Environment.NewLine}" +
                    $"{Environment.NewLine}Your current version is: {e.CurrentVersion}.";
                    
                MessageBoxCustom.Show(this, message, @"EVEMon Update Available", "Ignore this upgrade",
                    icon: MessageBoxIcon.Information);

                if (MessageBoxCustom.CheckBoxChecked)
                    Settings.Updates.MostRecentDeniedMajorUpgrade = e.NewestVersion.ToString();
            }

            m_isShowingUpdateWindow = false;
            m_isUpdating = false;
        }

        /// <summary>
        /// Occurs when new datafiles versions are available. Display the information form to the user.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnDataUpdateAvailable(object sender, DataUpdateAvailableEventArgs e)
        {
            if (m_isShowingDataUpdateWindow)
                return;

            m_isShowingDataUpdateWindow = true;
            using (DataUpdateNotifyForm form = new DataUpdateNotifyForm(e))
            {
                if (form.ShowDialog() == DialogResult.OK)
                    await RestartApplicationAsync();
            }

            m_isShowingDataUpdateWindow = false;
            m_isUpdatingData = false;
        }

        /// <summary>
        /// Triggers a restart of EVEMon.
        /// </summary>
        private async Task RestartApplicationAsync()
        {
            // Save the settings to make sure we don't lose anything
            await Settings.SaveImmediateAsync();

            // Try to save settings to cloud storage service provider
            bool canExit = await TryUploadToCloudStorageProviderAsync();
            if (!canExit)
                return;

            // Set the updating data flag so EVEMon exits cleanly
            m_isUpdatingData = true;

            // Find the expected path for 'EVEMon.Watchdog.exe'
            string executable = typeof(WatchdogWindow).Assembly.Location;

            // If the 'Watchdog' exist start the process
            if (File.Exists(executable))
            {
                try
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = executable,
                        Arguments = String.Join(" ", Environment.GetCommandLineArgs()),
                        UseShellExecute = false
                    };

                    Process.Start(startInfo);
                }
                catch (InvalidOperationException e)
                {
                    ExceptionHandler.LogException(e, true);
                }
            }

            Application.Exit();
        }

        #endregion


        #region Menus and toolbar

        /// <summary>
        /// File > Add API key...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addAPIKeyMenu_Click(object sender, EventArgs e)
        {
            using (EsiKeyUpdateOrAdditionWindow window = new EsiKeyUpdateOrAdditionWindow())
            {
                window.ShowDialog(this);
            }
        }

        /// <summary>
        /// File > Manage API keys...
        /// Open the api keys management window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void manageAPIKeysMenuItem_Click(object sender, EventArgs e)
        {
            using (EsiKeysManagementWindow window = new EsiKeysManagementWindow())
            {
                window.ShowDialog(this);
            }
        }

        /// <summary>
        /// File > Hide Character...
        /// Unmonitor this character.
        /// It will still be in the settings unless the users removes the API key
        /// and confirm the deletion of characters.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void hideCharacterMenu_Click(object sender, EventArgs e)
        {
            Character character = GetCurrentCharacter();
            if (character == null)
                return;

            // Close any open associated windows
            CloseOpenWindowsOf(character);

            character.Monitored = false;
        }

        /// <summary>
        /// File > Delete Character...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deleteCharacterMenu_Click(object sender, EventArgs e)
        {
            Character character = GetCurrentCharacter();
            if (character == null)
                return;

            using (CharacterDeletionWindow window = new CharacterDeletionWindow(character))
            {
                window.ShowDialog(this);
            }

            // Close any open associated windows
            CloseOpenWindowsOf(character);
        }

        /// <summary>
        /// File > Export Character...
        /// Exports the character's infos.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void saveCharacterInfosMenuItem_Click(object sender, EventArgs e)
        {
            Character character = GetCurrentCharacter();
            if (character == null)
                return;

            UIHelper.CharacterMonitorScreenshot = GetCurrentMonitor().GetCharacterScreenshot();
            await UIHelper.ExportCharacterAsync(character);
        }

        /// <summary>
        /// File > Save settings...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void saveSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Prompts the user for a location
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            DialogResult result = saveFileDialog.ShowDialog();

            // Copy settings if OK
            if (result == DialogResult.OK)
                await Settings.CopySettingsAsync(saveFileDialog.FileName);
        }

        /// <summary>
        /// File > Restore settings...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void loadSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Prompts the user for a location
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

            // Load settings if OK
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;

            // Clear any notifications
            ClearNotifications();

            // Close any open character associated windows
            WindowsFactory.CloseAllTagged();

            // Hide the TabControl
            noCharactersLabel.Hide();
            tcCharacterTabs.Hide();
            mainLoadingThrobber.State = ThrobberState.Rotating;
            mainLoadingThrobber.Show();
            tabLoadingLabel.Show();

            UpdateSettingsControlsVisibility(enabled: false);

            // Open the specified settings
            await Settings.RestoreAsync(openFileDialog.FileName);

            // Remove the tip window if it exist and is confirmed in settings
            if (Settings.UI.ConfirmedTips.Contains("startup") && Controls.OfType<TipWindow>().Any())
                Controls.Remove(Controls.OfType<TipWindow>().First());
        }

        /// <summary>
        /// File > Clear Cache.
        /// Called when the user clickes the "clear cache" toolbar's button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clearCacheToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Manually delete the Settings file for any non-recoverable errors
            DialogResult dr = MessageBox.Show(Properties.Resources.PromptClearCache,
                @"Confirm Cache Clearing", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

            if (dr == DialogResult.Yes)
                EveMonClient.ClearCache();
        }

        /// <summary>
        /// File > Reset settings.
        /// Called when the user clickes the "reset settings" toolbar's button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void resetSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Manually delete the Settings file for any non-recoverable errors
            DialogResult dr = MessageBox.Show(Properties.Resources.PromptResetSettings,
                @"Confirm Settings Reset", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

            if (dr != DialogResult.Yes)
                return;

            // Close any open character associated windows
            WindowsFactory.CloseAllTagged();

            // Clear any notifications
            ClearNotifications();

            // Hide the TabControl
            tcCharacterTabs.Hide();

            // Reset the settings
            await Settings.ResetAsync();

            // Show the TabControl
            tcCharacterTabs.Show();

            // Trigger the tip window
            OnShown(e);
        }

        /// <summary>
        /// File > Exit.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Try to save settings to cloud storage service provider
            bool canExit = await TryUploadToCloudStorageProviderAsync();

            if (canExit)
                Application.Exit();
        }

        /// <summary>
        /// Edit's drop down menu opening.
        /// Enabled/disable the items.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void editToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            Character character = GetCurrentCharacter();
            copySkillsToClipboardBBFormatToolStripMenuItem.Enabled = character != null;
        }

        /// <summary>
        /// Edit > Copy skills to clipboard (BBCode format).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void copySkillsToClipboardBBFormatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Character character = GetCurrentCharacter();
            if (character == null)
                return;

            try
            {
                // Try to copy
                Clipboard.Clear();
                Clipboard.SetText(CharacterExporter.ExportAsBBCode(character));
            }
            catch (ExternalException ex)
            {
                // Occurs when another process is using the clipboard
                ExceptionHandler.LogException(ex, true);
                MessageBox.Show(Properties.Resources.ErrorClipboardFailure, "Error copying",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Plan's menu drop down.
        /// Enable/disable menu items and rebuild items for plans.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void plansToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            Character character = GetCurrentCharacter();

            // Enable or disable items
            tsmiNewPlan.Enabled = tsmiImportPlanFromFile.Enabled =
                tsmiManagePlans.Enabled = plansSeparator.Visible = (character != null);

            CCPCharacter ccpCharacter = character as CCPCharacter;
            tsmiCreatePlanFromSkillQueue.Enabled = ccpCharacter != null && ccpCharacter.SkillQueue.Any();

            // Remove everything after the separator
            int index = plansToolStripMenuItem.DropDownItems.IndexOf(plansSeparator) + 1;
            while (plansToolStripMenuItem.DropDownItems.Count > index)
            {
                plansToolStripMenuItem.DropDownItems.RemoveAt(index);
            }

            // Add new entries
            character?.Plans.AddTo(plansToolStripMenuItem.DropDownItems, InitializePlanItem);
        }

        /// <summary>
        /// Plans > New Plan...
        /// Displays the "New Plan" window.
        /// </summary>
        /// <param name="sender">menu item clicked</param>
        /// <param name="e"></param>
        private void tsmiNewPlan_Click(object sender, EventArgs e)
        {
            Character character = GetCurrentCharacter();
            if (character == null)
                return;

            // Ask the user for a new name
            Plan newPlan;
            using (NewPlanWindow npw = new NewPlanWindow())
            {
                DialogResult dr = npw.ShowDialog();
                if (dr == DialogResult.Cancel)
                    return;

                // Create a new plan
                newPlan = new Plan(character)
                {
                    Name = npw.PlanName,
                    Description = npw.PlanDescription
                };
            }

            // Add plan and save
            character.Plans.Add(newPlan);

            // Show the editor for this plan
            PlanWindow.ShowPlanWindow(plan: newPlan);
        }

        /// <summary>
        /// File > Create Plan from Skill Queue...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsmiCreatePlanFromSkillQueue_Click(object sender, EventArgs e)
        {
            Character character = GetCurrentCharacter();

            if (character == null)
                return;

            // Create new plan
            Plan newPlan = PlanWindow.CreateNewPlan(character, EveMonConstants.CurrentSkillQueueText);

            if (newPlan == null)
                return;

            // Add skill queue to new plan and insert it on top of the plans
            bool planCreated = PlanIOHelper.CreatePlanFromCharacterSkillQueue(newPlan, character);

            // Show the editor for this plan
            if (planCreated)
                PlanWindow.ShowPlanWindow(plan: newPlan);
        }

        /// <summary>
        /// File > Import Plan from file...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsmiImportPlanFromFile_Click(object sender, EventArgs e)
        {
            Character character = GetCurrentCharacter();

            // Prompt the user to select a file
            DialogResult dr = ofdOpenDialog.ShowDialog();
            if (dr == DialogResult.Cancel)
                return;

            // Load from file and returns if an error occurred (user has already been warned)
            SerializablePlan serial = PlanIOHelper.ImportFromXML(ofdOpenDialog.FileName);
            if (serial == null)
                return;

            // Imports the plan
            Plan loadedPlan = new Plan(character);
            loadedPlan.Import(serial);

            // Prompt the user for the plan name
            using (NewPlanWindow npw = new NewPlanWindow())
            {
                npw.PlanName = Path.GetFileNameWithoutExtension(ofdOpenDialog.FileName);
                DialogResult xdr = npw.ShowDialog();
                if (xdr == DialogResult.Cancel)
                    return;

                loadedPlan.Name = npw.PlanName;
                loadedPlan.Description = npw.PlanDescription;
                character.Plans.Add(loadedPlan);
            }
        }

        /// <summary>
        /// Plans > Manage...
        /// Displays the "Manage Plans" window.
        /// </summary>
        /// <param name="sender">menu item clicked</param>
        /// <param name="e"></param>
        private void manageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Character character = GetCurrentCharacter();

            if (character == null)
                return;

            WindowsFactory.ShowByTag<PlanManagementWindow, Character>(character);
        }

        /// <summary>
        /// Initializes tool strip menu item for the plan.
        /// </summary>
        /// <param name="planItem"></param>
        /// <param name="plan"></param>
        private void InitializePlanItem(ToolStripItem planItem, Plan plan)
        {
            if (WindowsFactory.GetByTag<PlanWindow, Character>((Character)plan.Character)?.Plan == plan)
                planItem.Font = FontFactory.GetFont(planItem.Font, FontStyle.Italic | FontStyle.Bold);

            planItem.Tag = plan;
            planItem.Click += planItem_Click;
        }

        /// <summary>
        /// Plans > Name of the plan.
        /// Open the plan.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void planItem_Click(object sender, EventArgs e)
        {
            // Retrieve the plan
            Plan plan = ((ToolStripMenuItem)sender)?.Tag as Plan;

            // Show or bring to front if a window with the same plan as tag already exists
            PlanWindow.ShowPlanWindow(GetCurrentCharacter(), plan);
        }
        
        /// <summary>
        /// Tools > Characters Comparison.
        /// Open the Characters Comparison window.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void charactersComparisonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WindowsFactory.ShowUnique<CharactersComparisonWindow>();
        }

        /// <summary>
        /// Tools > Blank Character Creator...
        /// Open the blank character creation window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void blankCreatorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (BlankCharacterWindow form = new BlankCharacterWindow())
            {
                form.ShowDialog(this);
            }
        }

        /// <summary>
        /// Tools > EVE Data Browser > Skill Browser.
        /// Open the plan window in skill browser.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void skillBrowserMenuItem_Click(object sender, EventArgs e)
        {
            PlanWindow.ShowPlanWindow().ShowSkillBrowser();
        }

        /// <summary>
        /// Tools > EVE Data Browser > Certificate Browser.
        /// Open the plan window in certificate browser.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void certificateBrowserMenuItem_Click(object sender, EventArgs e)
        {
            PlanWindow.ShowPlanWindow().ShowCertificateBrowser();
        }

        /// <summary>
        /// Tools > EVE Data Browser > Ship Browser.
        /// Open the plan window in ship browser.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void shipBrowserMenuItem_Click(object sender, EventArgs e)
        {
            PlanWindow.ShowPlanWindow().ShowShipBrowser();
        }

        /// <summary>
        /// Tools > EVE Data Browser > Item Browser.
        /// Open the plan window in item browser.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void itemBrowserMenuItem_Click(object sender, EventArgs e)
        {
            PlanWindow.ShowPlanWindow().ShowItemBrowser();
        }

        /// <summary>
        /// Tools > EVE Data Browser > Blueprint Browser.
        /// Open the plan window in blueprint browser.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void blueprintBrowserMenuItem_Click(object sender, EventArgs e)
        {
            PlanWindow.ShowPlanWindow().ShowBlueprintBrowser();
        }

        /// <summary>
        /// Tools > Skills pie chart.
        /// Displays the skills pie chart
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsSkillsPieChartTool_Click(object sender, EventArgs e)
        {
            // Return if no selected tab (cannot infere which character the chart should represent)
            Character character = GetCurrentCharacter();
            if (character == null)
                return;

            // Create the window
            WindowsFactory.ShowByTag<SkillsPieChart, Character>(character);
        }

        /// <summary>
        /// Tools > Manual implants group.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void manualImplantGroupsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Character character = GetCurrentCharacter();
            if (character == null)
                return;

            WindowsFactory.ShowByTag<ImplantSetsWindow, Character>(character);
        }

        /// <summary>
        /// Tools > Show owned skillbooks.
        /// Displays a message box with the owned skills.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsShowOwnedSkillbooks_Click(object sender, EventArgs e)
        {
            Character character = GetCurrentCharacter();
            if (character == null)
                return;

            WindowsFactory.ShowByTag<OwnedSkillBooksWindow, Character>(character);
        }

        /// <summary>
        /// Tools > Options.
        /// Open the settings form.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void optionsMenuItem_Click(object sender, EventArgs e)
        {
            using (SettingsForm form = new SettingsForm())
            {
                form.ShowDialog(this);
            }
        }

        /// <summary>
        /// Help > UserVoice
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void issuesFeaturesMenuItem_Click(object sender, EventArgs e)
        {
            Util.OpenURL(new Uri(NetworkConstants.EVEMonUserVoice));
        }

        /// <summary>
        /// Help > Forums.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void forumsMenuItem_Click(object sender, EventArgs e)
        {
            Util.OpenURL(new Uri(NetworkConstants.EVEMonForums));
        }

        /// <summary>
        /// Help > Follow us on Twitter.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void twitterMenuItem_Click(object sender, EventArgs e)
        {
            Util.OpenURL(new Uri(NetworkConstants.EVEMonTwitter));
        }

        /// <summary>
        /// Help > Manual.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void readTheDocsManualMenuItem_Click(object sender, EventArgs e)
        {
            Util.OpenURL(new Uri(NetworkConstants.EVEMonManual));
        }

        /// <summary>
        /// Help > About.
        /// Open the "about" form.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WindowsFactory.ShowUnique<AboutWindow>();
        }

        /// <summary>
        /// Menu bar's context menu > Menubar.
        /// Hide/show the menu bar.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menubarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mainMenuBar.Visible = !mainMenuBar.Visible;
            mainToolBar.Visible = !mainMenuBar.Visible;
            Settings.UI.MainWindow.ShowMenuBar = mainMenuBar.Visible;
            Settings.Save();
        }

        /// <summary>
        /// When the mouse gets pressed, we change the cursor.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void mainMenuBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            mainMenuBar.Cursor = Cursors.Default;
        }

        /// <summary>
        /// When the mouse moves over the list, we change the cursor.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
        private void mainMenuBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                return;

            mainMenuBar.Cursor = CustomCursors.ContextMenu;
        }

        /// <summary>
        /// Menu bar's context menu > Toolbar.
        /// Hide/show the tool bar.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolbarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mainToolBar.Visible = !mainToolBar.Visible;
            mainMenuBar.Visible = !mainToolBar.Visible;
            Settings.UI.MainWindow.ShowMenuBar = mainMenuBar.Visible;
            Settings.Save();
        }

        /// <summary>
        /// When the mouse gets pressed, we change the cursor.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void mainToolBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            mainToolBar.Cursor = Cursors.Default;
        }

        /// <summary>
        /// When the mouse moves over the list, we change the cursor.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
        private void mainToolBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                return;

            mainToolBar.Cursor = CustomCursors.ContextMenu;
        }

        /// <summary>
        /// Toolbar > Plans icon's dropdown opening.
        /// Rebuild the menu items for plans.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsdbPlans_DropDownOpening(object sender, EventArgs e)
        {
            // Clear the menu items and rebuild them
            plansTbMenu.DropDownItems.Clear();

            GetCurrentCharacter()?.Plans.AddTo(plansTbMenu.DropDownItems, InitializePlanItem);
        }

        /// <summary>
        /// Toolbar > Context menu's dropdown opening.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolbarContext_Opening(object sender, CancelEventArgs e)
        {
            menubarToolStripMenuItem.Enabled = toolbarToolStripMenuItem.Checked = mainToolBar.Visible;
            toolbarToolStripMenuItem.Enabled = menubarToolStripMenuItem.Checked = mainMenuBar.Visible;
        }

        /// <summary>
        /// Closes any open windows of the specified character.
        /// </summary>
        /// <param name="character">The character.</param>
        private static void CloseOpenWindowsOf(Character character)
        {
            // Close any open Skill Planner window
            foreach (Plan plan in character.Plans)
            {
                WindowsFactory.GetAndCloseByTag<PlanWindow, Plan>(plan);
            }

            // Close any open Skill Pie Chart window
            WindowsFactory.GetAndCloseByTag<SkillsPieChart, Character>(character);

            // Close any open Implant AllGroups window
            WindowsFactory.GetAndCloseByTag<ImplantSetsWindow, Character>(character);

            // Close any open Show Owned Skillbooks window
            WindowsFactory.GetAndCloseByTag<OwnedSkillBooksWindow, Character>(character);

            // Now CCP character related windows
            CCPCharacter ccpCharacter = character as CCPCharacter;

            if (ccpCharacter == null)
                return;

            // Close any open Wallet Journal Chart window
            WindowsFactory.GetAndCloseByTag<WalletJournalChartWindow, CCPCharacter>(ccpCharacter);

            // Close any open EVE Mail window
            foreach (EveMailMessage mailMessage in ccpCharacter.EVEMailMessages)
            {
                WindowsFactory.GetAndCloseByTag<EveMessageWindow, EveMailMessage>(mailMessage);
            }

            // Close any open EVE Notification window
            foreach (EveNotification eveNotification in ccpCharacter.EVENotifications)
            {
                WindowsFactory.GetAndCloseByTag<EveMessageWindow, EveNotification>(eveNotification);
            }

            // Close any open Contract Details window
            foreach (Contract contract in ccpCharacter.Contracts)
            {
                WindowsFactory.GetAndCloseByTag<ContractDetailsWindow, Contract>(contract);
            }

            // Close any open Kill Report window
            foreach (KillLog killLog in ccpCharacter.KillLog)
            {
                WindowsFactory.GetAndCloseByTag<KillReportWindow, KillLog>(killLog);
            }
        }

        /// <summary>
        /// Clears the notifications.
        /// </summary>
        private void ClearNotifications()
        {
            // Clear the global notification collection
            EveMonClient.Notifications.Clear();

            // Clear all main window notifications
            notificationList.Notifications = null;

            // Clear all tray icon notifications
            m_popupNotifications.Clear();

            // Clear all character monitor notifications
            foreach (CharacterMonitor monitor in tcCharacterTabs.TabPages.Cast<TabPage>()
                .Select(tabPage => tabPage.Controls[0] as CharacterMonitor))
            {
                monitor?.ClearNotifications();
            }
        }

        #endregion


        #region Tray icon

        /// <summary>
        /// Remove the popup if its showing.
        /// </summary>
        private void HidePopup()
        {
            if (m_trayPopup == null)
                return;

            try
            {
                m_trayPopup.Close();
            }
            catch (InvalidOperationException ex)
            {
                ExceptionHandler.LogException(ex, true);
            }
            finally
            {
                m_trayPopup.Dispose();
            }
        }

        /// <summary>
        /// Occurs when the user click the tray icon.
        /// If it's not a right click button, we restore or minimize the window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void trayIcon_Click(object sender, EventArgs e)
        {
            // Returns for right-button click
            MouseEventArgs mouseClick = e as MouseEventArgs;
            if (mouseClick != null && mouseClick.Button == MouseButtons.Right)
                return;

            // Set the mouse clicked flag
            m_isMouseClicked = true;

            // Update the tray icon's visibility
            HidePopup();

            // Restore the main window
            RestoreMainWindow();
        }

        /// <summary>
        /// Occurs when the mouse hovers over the tray icon.
        /// Make the popup visible.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void trayIcon_MouseHover(object sender, EventArgs e)
        {
            // When clicking on the tray icon we need to prevent the popup showing due to pending hovering event
            if (m_isMouseClicked)
            {
                m_isMouseClicked = false;
                return;
            }

            // Only display the pop up window if the context menu isn't showing and main window is not restoring
            if (trayIconContextMenuStrip.Visible)
                return;

            // Stop if the popup is disabled
            if (Settings.UI.SystemTrayPopup.Style == TrayPopupStyles.Disabled)
                return;

            // Create the popup
            if (Settings.UI.SystemTrayPopup.Style == TrayPopupStyles.PopupForm)
                m_trayPopup = new TrayPopupWindow();
            else
                m_trayPopup = new TrayTooltipWindow();

            // Show the tooltip
            m_trayPopup.Show();

            // Ensure that the tooltip will be shown on top of all other windows
            m_trayPopup.BringToFront();
        }

        /// <summary>
        /// Occurs when the mouse leaves the tray icon.
        /// Hide the popup.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void trayIcon_MouseLeave(object sender, EventArgs e)
        {
            HidePopup();
            TriggerAutoShrink();
        }

        /// <summary>
        /// Tray icon's context menu drop down opening.
        /// Update the menu items for characters plans.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void trayIconToolStrip_Opening(object sender, CancelEventArgs e)
        {
            HidePopup();

            // Create the Plans sub-menu
            List<Character> characters = new List<Character>(EveMonClient.MonitoredCharacters);
            characters.Sort((x, y) => String.Compare(x.Name, y.Name, StringComparison.CurrentCulture));
            foreach (Character character in characters)
            {
                ToolStripMenuItem characterItem = new ToolStripMenuItem(character.Name);
                planToolStripMenuItem.DropDownItems.Add(characterItem);

                character.Plans.AddTo(characterItem.DropDownItems, InitializePlanItem);
            }
        }

        /// <summary>
        /// Tray icon's context menu > Restore.
        /// Restore the window to its normal size.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void restoreToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RestoreMainWindow();
        }

        /// <summary>
        /// Tray icon's context menu > Close.
        /// Quit the application.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Try to save settings to cloud storage service provider
            bool canExit = await TryUploadToCloudStorageProviderAsync();

            if (canExit)
                Application.Exit();
        }

        /// <summary>
        /// Clear the menu items for characters plans. Rebuild on opening anyway.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void trayIconToolStrip_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            // Clear the existing items
            planToolStripMenuItem.DropDownItems.Clear();
        }

        /// <summary>
        /// Restores the main window.
        /// </summary>
        private void RestoreMainWindow()
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Visible = true;
                WindowState = FormWindowState.Normal;
                ShowInTaskbar = Visible;
                trayIcon.Visible = Settings.UI.SystemTrayIcon == SystemTrayBehaviour.AlwaysVisible;
            }

            Activate();
        }

        #endregion


        #region Reaction to settings change

        /// <summary>
        /// Occurs when the settings form has been validated.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EveMonClient_SettingsChanged(object sender, EventArgs e)
        {
            UpdateControlsVisibility();
        }

        /// <summary>
        /// Updates the controls visibility according to settings
        /// </summary>
        private void UpdateControlsVisibility()
        {
            // Quit if the client has been shut down
            if (EveMonClient.Closed)
                return;

            // Displays or not the 'no characters added' label
            noCharactersLabel.Visible = !EveMonClient.MonitoredCharacters.Any();
            
            // Tray icon's visibility
            trayIcon.Visible = Settings.UI.SystemTrayIcon == SystemTrayBehaviour.AlwaysVisible
                               || (Settings.UI.SystemTrayIcon == SystemTrayBehaviour.ShowWhenMinimized &&
                                   WindowState == FormWindowState.Minimized);

            // Update manager configuration
            UpdateManager.Enabled = Settings.Updates.CheckEVEMonVersion;

            if (UpdateManager.Enabled && !m_isUpdateEventsSubscribed)
            {
                EveMonClient.UpdateAvailable += OnUpdateAvailable;
                EveMonClient.DataUpdateAvailable += OnDataUpdateAvailable;
                m_isUpdateEventsSubscribed = true;
            }

            if (!UpdateManager.Enabled && m_isUpdateEventsSubscribed)
            {
                EveMonClient.UpdateAvailable -= OnUpdateAvailable;
                EveMonClient.DataUpdateAvailable -= OnDataUpdateAvailable;
                m_isUpdateEventsSubscribed = false;
            }

            // Rebuild tabs (the overview may have been removed)
            if (!Settings.IsRestoring && tcCharacterTabs.TabPages.Contains(tpOverview) != Settings.UI.MainWindow.ShowOverview)
            {
                UpdateTabs();
            }

            // Whenever we switch API provider we update
            // the server status and every monitored CCP character
            if (m_apiProviderName == EveMonClient.APIProviders.CurrentProvider.Name)
                return;

            // Clear any notifications
            ClearNotifications();

            m_apiProviderName = EveMonClient.APIProviders.CurrentProvider.Name;
            EveMonClient.EVEServer.ForceUpdate();

            foreach (ESIKey apiKey in EveMonClient.ESIKeys)
            {
                apiKey.ForceUpdate();
            }

            foreach (CCPCharacter character in EveMonClient.MonitoredCharacters.OfType<CCPCharacter>())
            {
                character.QueryMonitors.QueryEverything();
            }
        }
        #endregion


        #region Helper Methods

        /// <summary>
        /// Updates the settings controls visibility.
        /// </summary>
        /// <param name="enabled">if set to <c>true</c> [enabled].</param>
        private void UpdateSettingsControlsVisibility(bool enabled)
        {
            // Enable or disable the menu buttons
            foreach (ToolStripItem item in m_settingsEnabledMenuItems)
            {
                item.Enabled = enabled;
            }
            
            UpdateControlsOnTabSelectionChange();
        }

        /// <summary>
        /// Asynchronously tries to upload to cloud storage provider.
        /// </summary>
        /// <returns></returns>
        private async Task<bool> TryUploadToCloudStorageProviderAsync()
        {
            // Return a success if settings have not been set to upload
            if (Settings.CloudStorageServiceProvider.Provider == null)
                return true;

            if (CloudStorageServiceSettings.Default.UploadAlways &&
                Settings.CloudStorageServiceProvider.Provider.HasCredentialsStored)
            {
                lblCSSProviderStatus.Text = $"Uploading to {Settings.CloudStorageServiceProvider.Provider.Name}";
                lblCSSProviderStatus.Visible = true;
            }

            bool success = await Settings.CloudStorageServiceProvider.Provider.UploadSettingsFileOnExitAsync()
                .ConfigureAwait(false);

            lblCSSProviderStatus.Visible = false;

            return success;
        }

        #endregion


        #region Testing Functions

        /// <summary>
        /// Displays the test menu.
        /// </summary>
        private void DisplayTestMenu()
        {
            testsToolStripMenuItem.Visible = true;
            testTrayToolStripMenuItem.Visible = true;
            testsToolStripSeperator.Visible = true;
            testCharacterNotificationToolStripMenuItem.Visible = true;
        }

        /// <summary>
        /// Enables the character notification if the current character is valid.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void testToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            testCharacterNotificationToolStripMenuItem.Enabled = GetCurrentCharacter() != null;
        }

        /// <summary>
        /// Thrown an exception just to test the exception handler is working.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExceptionWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            throw new InvalidOperationException("Test Exception");
        }

        /// <summary>
        /// Thrown an exception with an inner exception just to test the exception handler is working.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exceptionWindowRecursiveExceptionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            throw new InvalidOperationException("Test Exception", new InvalidOperationException("Inner Exception"));
        }

        /// <summary>
        /// Tests notification display in the MainWindow.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void testNotificationToolstripMenuItem_Click(object sender, EventArgs e)
        {
            NotificationEventArgs notification = new NotificationEventArgs(null, NotificationCategory.TestNofitication)
            {
                Priority = NotificationPriority.Information,
                Behaviour = NotificationBehaviour.Overwrite,
                Description = "Test Notification"
            };
            EveMonClient.Notifications.Notify(notification);
        }

        /// <summary>
        /// Tests character's notification display in the Character Monitor.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void testCharacterNotificationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CharacterMonitor.TestCharacterNotification(GetCurrentCharacter());
        }

        /// <summary>
        /// Resets the HTTP timeout to 1 second.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void testTimeoutOneSecToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show($"Timeout was: {Settings.Updates.HttpTimeout}, now 1");
            Settings.Updates.HttpTimeout = 1;
        }

        /// <summary>
        /// Restarts the application.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private async void restartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await RestartApplicationAsync();
        }

        #endregion
    }
}
