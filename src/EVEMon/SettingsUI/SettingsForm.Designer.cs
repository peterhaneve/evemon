using System;
using EVEMon.Common.Controls.MultiPanel;

namespace EVEMon.SettingsUI
{
    partial class SettingsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }


        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Updates", 11, 11);
            System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("Network", 7, 7);
            System.Windows.Forms.TreeNode treeNode3 = new System.Windows.Forms.TreeNode("Logitech Keyboards", 4, 4);
            System.Windows.Forms.TreeNode treeNode4 = new System.Windows.Forms.TreeNode("Portable EVE Clients", 15, 15);
            System.Windows.Forms.TreeNode treeNode5 = new System.Windows.Forms.TreeNode("Market Price Providers", 16, 16);
            System.Windows.Forms.TreeNode treeNode6 = new System.Windows.Forms.TreeNode("General", 10, 10, new System.Windows.Forms.TreeNode[] {
            treeNode1,
            treeNode2,
            treeNode3,
            treeNode4,
            treeNode5});
            System.Windows.Forms.TreeNode treeNode7 = new System.Windows.Forms.TreeNode("Main Window", 6, 6);
            System.Windows.Forms.TreeNode treeNode8 = new System.Windows.Forms.TreeNode("Icons", 13, 13);
            System.Windows.Forms.TreeNode treeNode9 = new System.Windows.Forms.TreeNode("Messages", 14, 14);
            System.Windows.Forms.TreeNode treeNode10 = new System.Windows.Forms.TreeNode("Skill Planner", 8, 8, new System.Windows.Forms.TreeNode[] {
            treeNode8,
            treeNode9});
            System.Windows.Forms.TreeNode treeNode11 = new System.Windows.Forms.TreeNode("System Tray Icon", 2, 2);
            System.Windows.Forms.TreeNode treeNode12 = new System.Windows.Forms.TreeNode("External Calendar", 5, 5);
            System.Windows.Forms.TreeNode treeNode13 = new System.Windows.Forms.TreeNode("Scheduler", 1, 1, new System.Windows.Forms.TreeNode[] {
            treeNode12});
            System.Windows.Forms.TreeNode treeNode14 = new System.Windows.Forms.TreeNode("Skill Completion Mails", 12, 12);
            System.Windows.Forms.TreeNode treeNode15 = new System.Windows.Forms.TreeNode("Notifications", 9, 9, new System.Windows.Forms.TreeNode[] {
            treeNode14});
            System.Windows.Forms.TreeNode treeNode16 = new System.Windows.Forms.TreeNode("Cloud Storage Service", 17, 17);
            this.systemTrayIconGroupBox = new System.Windows.Forms.GroupBox();
            this.rbSystemTrayOptionsNever = new System.Windows.Forms.RadioButton();
            this.rbSystemTrayOptionsAlways = new System.Windows.Forms.RadioButton();
            this.rbSystemTrayOptionsMinimized = new System.Windows.Forms.RadioButton();
            this.bottomPanel = new System.Windows.Forms.Panel();
            this.applyButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.lblMainWindowPage = new System.Windows.Forms.Label();
            this.lblSize = new System.Windows.Forms.Label();
            this.CharacterMonitorGroupBox = new System.Windows.Forms.GroupBox();
            this.nudSkillQueueWarningThresholdDays = new System.Windows.Forms.NumericUpDown();
            this.lblSkillQueueWarningThresholdDays = new System.Windows.Forms.Label();
            this.lblSkillQueuWarningThreshold = new System.Windows.Forms.Label();
            this.cbColorQueuedSkills = new System.Windows.Forms.CheckBox();
            this.cbShowPrereqMetSkills = new System.Windows.Forms.CheckBox();
            this.cbColorPartialSkills = new System.Windows.Forms.CheckBox();
            this.cbAlwaysShowSkillQueueTime = new System.Windows.Forms.CheckBox();
            this.cbShowNonPublicSkills = new System.Windows.Forms.CheckBox();
            this.cbShowAllPublicSkills = new System.Windows.Forms.CheckBox();
            this.WindowTitleGroupBox = new System.Windows.Forms.GroupBox();
            this.cbWindowsTitleList = new System.Windows.Forms.ComboBox();
            this.cbSkillInTitle = new System.Windows.Forms.CheckBox();
            this.cbTitleToTime = new System.Windows.Forms.CheckBox();
            this.lblGeneralPage = new System.Windows.Forms.Label();
            this.lblEnvironment = new System.Windows.Forms.Label();
            this.lblSkillPlannerPage = new System.Windows.Forms.Label();
            this.lblNetworkPageProxy = new System.Windows.Forms.Label();
            this.lblProxyHostIPAddress = new System.Windows.Forms.Label();
            this.lblProxyPort = new System.Windows.Forms.Label();
            this.lblHTTP = new System.Windows.Forms.Label();
            this.lblEmailNotificationPage = new System.Windows.Forms.Label();
            this.lblNotificationsPage = new System.Windows.Forms.Label();
            this.lblTrayIconPage = new System.Windows.Forms.Label();
            this.lblSchedulerUIPage = new System.Windows.Forms.Label();
            this.lblText = new System.Windows.Forms.Label();
            this.lblBlockingEvents = new System.Windows.Forms.Label();
            this.lblRecurringEvents = new System.Windows.Forms.Label();
            this.lblSimpleEvents = new System.Windows.Forms.Label();
            this.lblExternalCalendarPage = new System.Windows.Forms.Label();
            this.lblG15Page = new System.Windows.Forms.Label();
            this.lblCycleTrainingSeconds = new System.Windows.Forms.Label();
            this.lblG15CycleCharSeconds = new System.Windows.Forms.Label();
            this.lblIconsPage = new System.Windows.Forms.Label();
            this.gbSkillBrowserIconSet = new System.Windows.Forms.GroupBox();
            this.iconsSetTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.cbSkillIconSet = new System.Windows.Forms.ComboBox();
            this.tvlist = new System.Windows.Forms.TreeView();
            this.lblObsoletePlanEntries = new System.Windows.Forms.Label();
            this.ttToolTipCodes = new System.Windows.Forms.ToolTip(this.components);
            this.cbUseIncreasedContrastOnOverview = new System.Windows.Forms.CheckBox();
            this.overviewGroupCharactersInTrainingCheckBox = new System.Windows.Forms.CheckBox();
            this.overviewShowSkillQueueTrainingTimeCheckBox = new System.Windows.Forms.CheckBox();
            this.overviewShowWalletCheckBox = new System.Windows.Forms.CheckBox();
            this.overviewShowPortraitCheckBox = new System.Windows.Forms.CheckBox();
            this.cbShowOverViewTab = new System.Windows.Forms.CheckBox();
            this.colorDialog = new System.Windows.Forms.ColorDialog();
            this.treeView = new System.Windows.Forms.TreeView();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.leftPanel = new System.Windows.Forms.Panel();
            this.multiPanel = new EVEMon.Common.Controls.MultiPanel.MultiPanel();
            this.generalPage = new EVEMon.Common.Controls.MultiPanel.MultiPanelPage();
            this.btnEVEMonDataDir = new System.Windows.Forms.Button();
            this.cbWorksafeMode = new System.Windows.Forms.CheckBox();
            this.compatibilityCombo = new System.Windows.Forms.ComboBox();
            this.runAtStartupComboBox = new System.Windows.Forms.CheckBox();
            this.mainWindowPage = new EVEMon.Common.Controls.MultiPanel.MultiPanelPage();
            this.OverviewGroupBox = new System.Windows.Forms.GroupBox();
            this.overviewPanel = new System.Windows.Forms.Panel();
            this.cbShowSkillpointsOnOverview = new System.Windows.Forms.CheckBox();
            this.overviewPortraitSizeComboBox = new System.Windows.Forms.ComboBox();
            this.skillPlannerPage = new EVEMon.Common.Controls.MultiPanel.MultiPanelPage();
            this.cbAdvanceEntryAdd = new System.Windows.Forms.CheckBox();
            this.cbSummaryOnMultiSelectOnly = new System.Windows.Forms.CheckBox();
            this.cbHighlightQueuedSiklls = new System.Windows.Forms.CheckBox();
            this.cbHighlightPartialSkills = new System.Windows.Forms.CheckBox();
            this.cbHighlightConflicts = new System.Windows.Forms.CheckBox();
            this.cbHighlightPrerequisites = new System.Windows.Forms.CheckBox();
            this.cbHighlightPlannedSkills = new System.Windows.Forms.CheckBox();
            this.networkPage = new EVEMon.Common.Controls.MultiPanel.MultiPanelPage();
            this.esiSettingsGroupBox = new System.Windows.Forms.GroupBox();
            this.scopesTextBox = new System.Windows.Forms.TextBox();
            this.lblScopes = new System.Windows.Forms.Label();
            this.lblClientSecret = new System.Windows.Forms.Label();
            this.lblClientID = new System.Windows.Forms.Label();
            this.esiSettingsLabel = new System.Windows.Forms.LinkLabel();
            this.clientSecretTextBox = new System.Windows.Forms.TextBox();
            this.clientIDTextBox = new System.Windows.Forms.TextBox();
            this.ProxyServerGroupBox = new System.Windows.Forms.GroupBox();
            this.customProxyCheckBox = new System.Windows.Forms.CheckBox();
            this.customProxyPanel = new System.Windows.Forms.Panel();
            this.proxyPortTextBox = new System.Windows.Forms.TextBox();
            this.proxyAuthenticationButton = new System.Windows.Forms.Button();
            this.proxyHttpHostTextBox = new System.Windows.Forms.TextBox();
            this.emailNotificationsPage = new EVEMon.Common.Controls.MultiPanel.MultiPanelPage();
            this.mailNotificationCheckBox = new System.Windows.Forms.CheckBox();
            this.notificationsPage = new EVEMon.Common.Controls.MultiPanel.MultiPanelPage();
            this.cbPlaySoundOnSkillComplete = new System.Windows.Forms.CheckBox();
            this.trayIconPage = new EVEMon.Common.Controls.MultiPanel.MultiPanelPage();
            this.mainWindowBehaviourGroupBox = new System.Windows.Forms.GroupBox();
            this.rbMinToTaskBar = new System.Windows.Forms.RadioButton();
            this.rbMinToTray = new System.Windows.Forms.RadioButton();
            this.rbExitEVEMon = new System.Windows.Forms.RadioButton();
            this.trayIconPopupGroupBox = new System.Windows.Forms.GroupBox();
            this.trayPopupDisabledRadio = new System.Windows.Forms.RadioButton();
            this.trayPopupButton = new System.Windows.Forms.Button();
            this.trayPopupRadio = new System.Windows.Forms.RadioButton();
            this.trayTooltipRadio = new System.Windows.Forms.RadioButton();
            this.trayTooltipButton = new System.Windows.Forms.Button();
            this.updatesPage = new EVEMon.Common.Controls.MultiPanel.MultiPanelPage();
            this.lblUpdatesPage = new System.Windows.Forms.Label();
            this.cbCheckTime = new System.Windows.Forms.CheckBox();
            this.cbCheckForUpdates = new System.Windows.Forms.CheckBox();
            this.schedulerUIPage = new EVEMon.Common.Controls.MultiPanel.MultiPanelPage();
            this.panelColorText = new System.Windows.Forms.Panel();
            this.panelColorRecurring2 = new System.Windows.Forms.Panel();
            this.panelColorRecurring1 = new System.Windows.Forms.Panel();
            this.panelColorSingle2 = new System.Windows.Forms.Panel();
            this.panelColorSingle1 = new System.Windows.Forms.Panel();
            this.panelColorBlocking = new System.Windows.Forms.Panel();
            this.externalCalendarPage = new EVEMon.Common.Controls.MultiPanel.MultiPanelPage();
            this.externalCalendarCheckbox = new System.Windows.Forms.CheckBox();
            this.g15Page = new EVEMon.Common.Controls.MultiPanel.MultiPanelPage();
            this.g15CheckBox = new System.Windows.Forms.CheckBox();
            this.g15Panel = new System.Windows.Forms.Panel();
            this.cbG15ShowEVETime = new System.Windows.Forms.CheckBox();
            this.cbG15ShowTime = new System.Windows.Forms.CheckBox();
            this.panelCycleQueueInfo = new System.Windows.Forms.Panel();
            this.cbG15CycleTimes = new System.Windows.Forms.CheckBox();
            this.ACycleTimesInterval = new System.Windows.Forms.NumericUpDown();
            this.panelCycleCharInfo = new System.Windows.Forms.Panel();
            this.cbG15ACycle = new System.Windows.Forms.CheckBox();
            this.ACycleInterval = new System.Windows.Forms.NumericUpDown();
            this.iconsPage = new EVEMon.Common.Controls.MultiPanel.MultiPanelPage();
            this.messagesPage = new EVEMon.Common.Controls.MultiPanel.MultiPanelPage();
            this.gbMessageBox = new System.Windows.Forms.GroupBox();
            this.lblPrioritesConflict = new System.Windows.Forms.Label();
            this.btnPrioritiesReset = new System.Windows.Forms.Button();
            this.ObsoleteEntryRemovalGroupBox = new System.Windows.Forms.GroupBox();
            this.RemoveAllLabel = new System.Windows.Forms.Label();
            this.AlwaysAskLabel = new System.Windows.Forms.Label();
            this.RemoveConfirmedLabel = new System.Windows.Forms.Label();
            this.alwaysAskRadioButton = new System.Windows.Forms.RadioButton();
            this.removeAllRadioButton = new System.Windows.Forms.RadioButton();
            this.removeConfirmedRadioButton = new System.Windows.Forms.RadioButton();
            this.portableEveClientsPage = new EVEMon.Common.Controls.MultiPanel.MultiPanelPage();
            this.lblPECIDescription = new System.Windows.Forms.Label();
            this.PECIGroupBox = new System.Windows.Forms.GroupBox();
            this.marketPriceProvidersPage = new EVEMon.Common.Controls.MultiPanel.MultiPanelPage();
            this.gbMarketPriceProviders = new System.Windows.Forms.GroupBox();
            this.cbProvidersList = new System.Windows.Forms.ComboBox();
            this.SelectedProviderLabel = new System.Windows.Forms.Label();
            this.marketPriceProviderPageLabel = new System.Windows.Forms.Label();
            this.cloudStorageServicePage = new EVEMon.Common.Controls.MultiPanel.MultiPanelPage();
            this.providerAuthenticationGroupBox = new System.Windows.Forms.GroupBox();
            this.cloudStorageGroupBox = new System.Windows.Forms.GroupBox();
            this.cloudStorageProviderLogoPictureBox = new System.Windows.Forms.PictureBox();
            this.cloudStorageProvidersComboBox = new System.Windows.Forms.ComboBox();
            this.lblSelectedProvider = new System.Windows.Forms.Label();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.settingsFileStorageGroupBox = new System.Windows.Forms.GroupBox();
            this.emailNotificationsControl = new EVEMon.SettingsUI.EmailNotificationsControl();
            this.notificationsControl = new EVEMon.SettingsUI.NotificationsControl();
            this.updateSettingsControl = new EVEMon.SettingsUI.UpdateSettingsControl();
            this.externalCalendarControl = new EVEMon.SettingsUI.ExternalCalendarControl();
            this.portableEveClientsControl = new EVEMon.SettingsUI.PortableEveClientsControl();
            this.cloudStorageServiceControl = new EVEMon.SettingsUI.CloudStorageServiceControl();
            this.settingsFileStorageControl = new EVEMon.SettingsUI.SettingsFileStorageControl();
            this.insertAllScopesButton = new System.Windows.Forms.Button();
            this.systemTrayIconGroupBox.SuspendLayout();
            this.bottomPanel.SuspendLayout();
            this.CharacterMonitorGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudSkillQueueWarningThresholdDays)).BeginInit();
            this.WindowTitleGroupBox.SuspendLayout();
            this.gbSkillBrowserIconSet.SuspendLayout();
            this.iconsSetTableLayoutPanel.SuspendLayout();
            this.leftPanel.SuspendLayout();
            this.multiPanel.SuspendLayout();
            this.generalPage.SuspendLayout();
            this.mainWindowPage.SuspendLayout();
            this.OverviewGroupBox.SuspendLayout();
            this.overviewPanel.SuspendLayout();
            this.skillPlannerPage.SuspendLayout();
            this.networkPage.SuspendLayout();
            this.esiSettingsGroupBox.SuspendLayout();
            this.ProxyServerGroupBox.SuspendLayout();
            this.customProxyPanel.SuspendLayout();
            this.emailNotificationsPage.SuspendLayout();
            this.notificationsPage.SuspendLayout();
            this.trayIconPage.SuspendLayout();
            this.mainWindowBehaviourGroupBox.SuspendLayout();
            this.trayIconPopupGroupBox.SuspendLayout();
            this.updatesPage.SuspendLayout();
            this.schedulerUIPage.SuspendLayout();
            this.externalCalendarPage.SuspendLayout();
            this.g15Page.SuspendLayout();
            this.g15Panel.SuspendLayout();
            this.panelCycleQueueInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ACycleTimesInterval)).BeginInit();
            this.panelCycleCharInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ACycleInterval)).BeginInit();
            this.iconsPage.SuspendLayout();
            this.messagesPage.SuspendLayout();
            this.gbMessageBox.SuspendLayout();
            this.ObsoleteEntryRemovalGroupBox.SuspendLayout();
            this.portableEveClientsPage.SuspendLayout();
            this.PECIGroupBox.SuspendLayout();
            this.marketPriceProvidersPage.SuspendLayout();
            this.gbMarketPriceProviders.SuspendLayout();
            this.cloudStorageServicePage.SuspendLayout();
            this.providerAuthenticationGroupBox.SuspendLayout();
            this.cloudStorageGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cloudStorageProviderLogoPictureBox)).BeginInit();
            this.settingsFileStorageGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // systemTrayIconGroupBox
            // 
            this.systemTrayIconGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.systemTrayIconGroupBox.Controls.Add(this.rbSystemTrayOptionsNever);
            this.systemTrayIconGroupBox.Controls.Add(this.rbSystemTrayOptionsAlways);
            this.systemTrayIconGroupBox.Controls.Add(this.rbSystemTrayOptionsMinimized);
            this.systemTrayIconGroupBox.Location = new System.Drawing.Point(9, 68);
            this.systemTrayIconGroupBox.Name = "systemTrayIconGroupBox";
            this.systemTrayIconGroupBox.Size = new System.Drawing.Size(419, 100);
            this.systemTrayIconGroupBox.TabIndex = 9;
            this.systemTrayIconGroupBox.TabStop = false;
            this.systemTrayIconGroupBox.Text = "Show System Tray Icon";
            // 
            // rbSystemTrayOptionsNever
            // 
            this.rbSystemTrayOptionsNever.AutoSize = true;
            this.rbSystemTrayOptionsNever.Location = new System.Drawing.Point(6, 20);
            this.rbSystemTrayOptionsNever.Name = "rbSystemTrayOptionsNever";
            this.rbSystemTrayOptionsNever.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.rbSystemTrayOptionsNever.Size = new System.Drawing.Size(57, 17);
            this.rbSystemTrayOptionsNever.TabIndex = 1;
            this.rbSystemTrayOptionsNever.TabStop = true;
            this.rbSystemTrayOptionsNever.Tag = "";
            this.rbSystemTrayOptionsNever.Text = "Never";
            this.rbSystemTrayOptionsNever.UseVisualStyleBackColor = true;
            this.rbSystemTrayOptionsNever.CheckedChanged += new System.EventHandler(this.OnMustEnableOrDisable);
            // 
            // rbSystemTrayOptionsAlways
            // 
            this.rbSystemTrayOptionsAlways.AutoSize = true;
            this.rbSystemTrayOptionsAlways.Location = new System.Drawing.Point(6, 66);
            this.rbSystemTrayOptionsAlways.Name = "rbSystemTrayOptionsAlways";
            this.rbSystemTrayOptionsAlways.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.rbSystemTrayOptionsAlways.Size = new System.Drawing.Size(61, 17);
            this.rbSystemTrayOptionsAlways.TabIndex = 3;
            this.rbSystemTrayOptionsAlways.TabStop = true;
            this.rbSystemTrayOptionsAlways.Tag = "";
            this.rbSystemTrayOptionsAlways.Text = "Always";
            this.rbSystemTrayOptionsAlways.UseVisualStyleBackColor = true;
            // 
            // rbSystemTrayOptionsMinimized
            // 
            this.rbSystemTrayOptionsMinimized.AutoSize = true;
            this.rbSystemTrayOptionsMinimized.Location = new System.Drawing.Point(6, 43);
            this.rbSystemTrayOptionsMinimized.Name = "rbSystemTrayOptionsMinimized";
            this.rbSystemTrayOptionsMinimized.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.rbSystemTrayOptionsMinimized.Size = new System.Drawing.Size(106, 17);
            this.rbSystemTrayOptionsMinimized.TabIndex = 2;
            this.rbSystemTrayOptionsMinimized.TabStop = true;
            this.rbSystemTrayOptionsMinimized.Tag = "";
            this.rbSystemTrayOptionsMinimized.Text = "When Minimized";
            this.rbSystemTrayOptionsMinimized.UseVisualStyleBackColor = true;
            // 
            // bottomPanel
            // 
            this.bottomPanel.BackColor = System.Drawing.SystemColors.ControlLight;
            this.bottomPanel.Controls.Add(this.applyButton);
            this.bottomPanel.Controls.Add(this.okButton);
            this.bottomPanel.Controls.Add(this.cancelButton);
            this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.bottomPanel.Location = new System.Drawing.Point(0, 436);
            this.bottomPanel.Name = "bottomPanel";
            this.bottomPanel.Size = new System.Drawing.Size(644, 46);
            this.bottomPanel.TabIndex = 8;
            // 
            // applyButton
            // 
            this.applyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.applyButton.Location = new System.Drawing.Point(557, 11);
            this.applyButton.Name = "applyButton";
            this.applyButton.Size = new System.Drawing.Size(75, 23);
            this.applyButton.TabIndex = 4;
            this.applyButton.Text = "&Apply";
            this.applyButton.UseVisualStyleBackColor = true;
            this.applyButton.Click += new System.EventHandler(this.applyButton_Click);
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.Location = new System.Drawing.Point(395, 11);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 3;
            this.okButton.Text = "&OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(476, 11);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 2;
            this.cancelButton.Text = "&Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // lblMainWindowPage
            // 
            this.lblMainWindowPage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMainWindowPage.ForeColor = System.Drawing.SystemColors.MenuHighlight;
            this.lblMainWindowPage.Location = new System.Drawing.Point(4, 12);
            this.lblMainWindowPage.Name = "lblMainWindowPage";
            this.lblMainWindowPage.Size = new System.Drawing.Size(424, 44);
            this.lblMainWindowPage.TabIndex = 19;
            this.lblMainWindowPage.Text = resources.GetString("lblMainWindowPage.Text");
            // 
            // lblSize
            // 
            this.lblSize.AutoSize = true;
            this.lblSize.Location = new System.Drawing.Point(20, 52);
            this.lblSize.Name = "lblSize";
            this.lblSize.Size = new System.Drawing.Size(27, 13);
            this.lblSize.TabIndex = 31;
            this.lblSize.Text = "Size";
            // 
            // CharacterMonitorGroupBox
            // 
            this.CharacterMonitorGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CharacterMonitorGroupBox.Controls.Add(this.nudSkillQueueWarningThresholdDays);
            this.CharacterMonitorGroupBox.Controls.Add(this.lblSkillQueueWarningThresholdDays);
            this.CharacterMonitorGroupBox.Controls.Add(this.lblSkillQueuWarningThreshold);
            this.CharacterMonitorGroupBox.Controls.Add(this.cbColorQueuedSkills);
            this.CharacterMonitorGroupBox.Controls.Add(this.cbShowPrereqMetSkills);
            this.CharacterMonitorGroupBox.Controls.Add(this.cbColorPartialSkills);
            this.CharacterMonitorGroupBox.Controls.Add(this.cbAlwaysShowSkillQueueTime);
            this.CharacterMonitorGroupBox.Controls.Add(this.cbShowNonPublicSkills);
            this.CharacterMonitorGroupBox.Controls.Add(this.cbShowAllPublicSkills);
            this.CharacterMonitorGroupBox.Location = new System.Drawing.Point(3, 161);
            this.CharacterMonitorGroupBox.Name = "CharacterMonitorGroupBox";
            this.CharacterMonitorGroupBox.Size = new System.Drawing.Size(426, 126);
            this.CharacterMonitorGroupBox.TabIndex = 7;
            this.CharacterMonitorGroupBox.TabStop = false;
            this.CharacterMonitorGroupBox.Text = "Character Monitor";
            // 
            // nudSkillQueueWarningThresholdDays
            // 
            this.nudSkillQueueWarningThresholdDays.Location = new System.Drawing.Point(65, 101);
            this.nudSkillQueueWarningThresholdDays.Maximum = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.nudSkillQueueWarningThresholdDays.Name = "nudSkillQueueWarningThresholdDays";
            this.nudSkillQueueWarningThresholdDays.Size = new System.Drawing.Size(33, 20);
            this.nudSkillQueueWarningThresholdDays.TabIndex = 16;
            this.nudSkillQueueWarningThresholdDays.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // lblSkillQueueWarningThresholdDays
            // 
            this.lblSkillQueueWarningThresholdDays.AutoSize = true;
            this.lblSkillQueueWarningThresholdDays.Location = new System.Drawing.Point(22, 103);
            this.lblSkillQueueWarningThresholdDays.Name = "lblSkillQueueWarningThresholdDays";
            this.lblSkillQueueWarningThresholdDays.Size = new System.Drawing.Size(37, 13);
            this.lblSkillQueueWarningThresholdDays.TabIndex = 15;
            this.lblSkillQueueWarningThresholdDays.Text = "Days :";
            // 
            // lblSkillQueuWarningThreshold
            // 
            this.lblSkillQueuWarningThreshold.AutoSize = true;
            this.lblSkillQueuWarningThreshold.Location = new System.Drawing.Point(9, 84);
            this.lblSkillQueuWarningThreshold.Name = "lblSkillQueuWarningThreshold";
            this.lblSkillQueuWarningThreshold.Size = new System.Drawing.Size(154, 13);
            this.lblSkillQueuWarningThreshold.TabIndex = 14;
            this.lblSkillQueuWarningThreshold.Text = "Skill Queue Warning Threshold";
            // 
            // cbColorQueuedSkills
            // 
            this.cbColorQueuedSkills.AutoSize = true;
            this.cbColorQueuedSkills.Location = new System.Drawing.Point(188, 38);
            this.cbColorQueuedSkills.Name = "cbColorQueuedSkills";
            this.cbColorQueuedSkills.Size = new System.Drawing.Size(135, 17);
            this.cbColorQueuedSkills.TabIndex = 13;
            this.cbColorQueuedSkills.Text = "Highlight Queued Skills";
            this.ttToolTipCodes.SetToolTip(this.cbColorQueuedSkills, "When enabled, highlights all\r\nqueued skills in character\'s skill list");
            this.cbColorQueuedSkills.UseVisualStyleBackColor = true;
            // 
            // cbShowPrereqMetSkills
            // 
            this.cbShowPrereqMetSkills.AutoSize = true;
            this.cbShowPrereqMetSkills.Location = new System.Drawing.Point(15, 57);
            this.cbShowPrereqMetSkills.Name = "cbShowPrereqMetSkills";
            this.cbShowPrereqMetSkills.Size = new System.Drawing.Size(158, 17);
            this.cbShowPrereqMetSkills.TabIndex = 12;
            this.cbShowPrereqMetSkills.Text = "Show Also Prereq-Met Skills";
            this.ttToolTipCodes.SetToolTip(this.cbShowPrereqMetSkills, "When enabled, shows all prerequisites\r\nmet skills in character\'s skill list");
            this.cbShowPrereqMetSkills.UseVisualStyleBackColor = true;
            // 
            // cbColorPartialSkills
            // 
            this.cbColorPartialSkills.AutoSize = true;
            this.cbColorPartialSkills.Location = new System.Drawing.Point(188, 20);
            this.cbColorPartialSkills.Name = "cbColorPartialSkills";
            this.cbColorPartialSkills.Size = new System.Drawing.Size(172, 17);
            this.cbColorPartialSkills.TabIndex = 11;
            this.cbColorPartialSkills.Text = "Highlight Partially Trained Skills";
            this.ttToolTipCodes.SetToolTip(this.cbColorPartialSkills, "When enabled, highlights all partially\r\ntrained skills in character\'s skill list");
            this.cbColorPartialSkills.UseVisualStyleBackColor = true;
            // 
            // cbAlwaysShowSkillQueueTime
            // 
            this.cbAlwaysShowSkillQueueTime.AutoSize = true;
            this.cbAlwaysShowSkillQueueTime.Location = new System.Drawing.Point(188, 57);
            this.cbAlwaysShowSkillQueueTime.Name = "cbAlwaysShowSkillQueueTime";
            this.cbAlwaysShowSkillQueueTime.Size = new System.Drawing.Size(213, 17);
            this.cbAlwaysShowSkillQueueTime.TabIndex = 2;
            this.cbAlwaysShowSkillQueueTime.Text = "Always show time above the skill queue";
            this.ttToolTipCodes.SetToolTip(this.cbAlwaysShowSkillQueueTime, "When enabled, always displays the total\r\nqueue time above the skill queue bar");
            this.cbAlwaysShowSkillQueueTime.UseVisualStyleBackColor = true;
            // 
            // cbShowNonPublicSkills
            // 
            this.cbShowNonPublicSkills.AutoSize = true;
            this.cbShowNonPublicSkills.Enabled = false;
            this.cbShowNonPublicSkills.Location = new System.Drawing.Point(15, 38);
            this.cbShowNonPublicSkills.Name = "cbShowNonPublicSkills";
            this.cbShowNonPublicSkills.Size = new System.Drawing.Size(158, 17);
            this.cbShowNonPublicSkills.TabIndex = 1;
            this.cbShowNonPublicSkills.Text = "Show Also Non-Public Skills";
            this.ttToolTipCodes.SetToolTip(this.cbShowNonPublicSkills, "When enabled, shows all non-public skills in character\'s skill list");
            this.cbShowNonPublicSkills.UseVisualStyleBackColor = true;
            // 
            // cbShowAllPublicSkills
            // 
            this.cbShowAllPublicSkills.AutoSize = true;
            this.cbShowAllPublicSkills.Location = new System.Drawing.Point(15, 20);
            this.cbShowAllPublicSkills.Name = "cbShowAllPublicSkills";
            this.cbShowAllPublicSkills.Size = new System.Drawing.Size(149, 17);
            this.cbShowAllPublicSkills.TabIndex = 0;
            this.cbShowAllPublicSkills.Text = "Show Also All Public Skills";
            this.ttToolTipCodes.SetToolTip(this.cbShowAllPublicSkills, "When enabled, shows all public skills in character\'s skill list");
            this.cbShowAllPublicSkills.UseVisualStyleBackColor = true;
            this.cbShowAllPublicSkills.CheckedChanged += new System.EventHandler(this.OnMustEnableOrDisable);
            // 
            // WindowTitleGroupBox
            // 
            this.WindowTitleGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.WindowTitleGroupBox.Controls.Add(this.cbWindowsTitleList);
            this.WindowTitleGroupBox.Controls.Add(this.cbSkillInTitle);
            this.WindowTitleGroupBox.Controls.Add(this.cbTitleToTime);
            this.WindowTitleGroupBox.Location = new System.Drawing.Point(3, 59);
            this.WindowTitleGroupBox.Name = "WindowTitleGroupBox";
            this.WindowTitleGroupBox.Size = new System.Drawing.Size(426, 96);
            this.WindowTitleGroupBox.TabIndex = 14;
            this.WindowTitleGroupBox.TabStop = false;
            this.WindowTitleGroupBox.Text = "Window Title";
            // 
            // cbWindowsTitleList
            // 
            this.cbWindowsTitleList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbWindowsTitleList.FormattingEnabled = true;
            this.cbWindowsTitleList.Items.AddRange(new object[] {
            "Single character - finishing skill next",
            "Single character - selected character",
            "Multi character - finishing skill next first",
            "Multi character - selected character first "});
            this.cbWindowsTitleList.Location = new System.Drawing.Point(15, 66);
            this.cbWindowsTitleList.Name = "cbWindowsTitleList";
            this.cbWindowsTitleList.Size = new System.Drawing.Size(224, 21);
            this.cbWindowsTitleList.TabIndex = 1;
            // 
            // cbSkillInTitle
            // 
            this.cbSkillInTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.cbSkillInTitle.AutoSize = true;
            this.cbSkillInTitle.Location = new System.Drawing.Point(15, 43);
            this.cbSkillInTitle.Name = "cbSkillInTitle";
            this.cbSkillInTitle.Size = new System.Drawing.Size(121, 17);
            this.cbSkillInTitle.TabIndex = 7;
            this.cbSkillInTitle.Text = "Show skill in training";
            this.ttToolTipCodes.SetToolTip(this.cbSkillInTitle, "When enabled, shows the character\'s skill\r\nin training according to choice below");
            this.cbSkillInTitle.UseVisualStyleBackColor = true;
            // 
            // cbTitleToTime
            // 
            this.cbTitleToTime.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.cbTitleToTime.AutoSize = true;
            this.cbTitleToTime.Location = new System.Drawing.Point(15, 20);
            this.cbTitleToTime.Margin = new System.Windows.Forms.Padding(12, 3, 3, 3);
            this.cbTitleToTime.Name = "cbTitleToTime";
            this.cbTitleToTime.Size = new System.Drawing.Size(190, 17);
            this.cbTitleToTime.TabIndex = 6;
            this.cbTitleToTime.Text = "Show character info in window title";
            this.ttToolTipCodes.SetToolTip(this.cbTitleToTime, "When enabled, shows the character\'s info in window title");
            this.cbTitleToTime.UseVisualStyleBackColor = true;
            this.cbTitleToTime.CheckedChanged += new System.EventHandler(this.OnMustEnableOrDisable);
            // 
            // lblGeneralPage
            // 
            this.lblGeneralPage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblGeneralPage.ForeColor = System.Drawing.SystemColors.MenuHighlight;
            this.lblGeneralPage.Location = new System.Drawing.Point(4, 23);
            this.lblGeneralPage.Name = "lblGeneralPage";
            this.lblGeneralPage.Size = new System.Drawing.Size(424, 42);
            this.lblGeneralPage.TabIndex = 20;
            this.lblGeneralPage.Text = resources.GetString("lblGeneralPage.Text");
            // 
            // lblEnvironment
            // 
            this.lblEnvironment.AutoSize = true;
            this.lblEnvironment.Location = new System.Drawing.Point(3, 181);
            this.lblEnvironment.Name = "lblEnvironment";
            this.lblEnvironment.Size = new System.Drawing.Size(210, 13);
            this.lblEnvironment.TabIndex = 1;
            this.lblEnvironment.Text = "Environment (requires restart to take effect)";
            // 
            // lblSkillPlannerPage
            // 
            this.lblSkillPlannerPage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSkillPlannerPage.ForeColor = System.Drawing.SystemColors.MenuHighlight;
            this.lblSkillPlannerPage.Location = new System.Drawing.Point(4, 20);
            this.lblSkillPlannerPage.Name = "lblSkillPlannerPage";
            this.lblSkillPlannerPage.Size = new System.Drawing.Size(424, 28);
            this.lblSkillPlannerPage.TabIndex = 19;
            this.lblSkillPlannerPage.Text = "You can select whether to highlight any entry in the Skill Planner according to i" +
    "ts status and more.";
            // 
            // lblNetworkPageProxy
            // 
            this.lblNetworkPageProxy.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblNetworkPageProxy.ForeColor = System.Drawing.SystemColors.MenuHighlight;
            this.lblNetworkPageProxy.Location = new System.Drawing.Point(9, 17);
            this.lblNetworkPageProxy.Name = "lblNetworkPageProxy";
            this.lblNetworkPageProxy.Size = new System.Drawing.Size(399, 32);
            this.lblNetworkPageProxy.TabIndex = 8;
            this.lblNetworkPageProxy.Text = "By default, EVEMon will use the same Proxy settings as Internet Explorer (can be " +
    "configured through the Control Panel).";
            // 
            // lblProxyHostIPAddress
            // 
            this.lblProxyHostIPAddress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblProxyHostIPAddress.Location = new System.Drawing.Point(50, 8);
            this.lblProxyHostIPAddress.Name = "lblProxyHostIPAddress";
            this.lblProxyHostIPAddress.Size = new System.Drawing.Size(182, 13);
            this.lblProxyHostIPAddress.TabIndex = 3;
            this.lblProxyHostIPAddress.Text = "Host/IP Address";
            this.lblProxyHostIPAddress.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblProxyPort
            // 
            this.lblProxyPort.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblProxyPort.Location = new System.Drawing.Point(221, 8);
            this.lblProxyPort.Name = "lblProxyPort";
            this.lblProxyPort.Size = new System.Drawing.Size(56, 13);
            this.lblProxyPort.TabIndex = 4;
            this.lblProxyPort.Text = "Port";
            this.lblProxyPort.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblHTTP
            // 
            this.lblHTTP.AutoSize = true;
            this.lblHTTP.Location = new System.Drawing.Point(8, 27);
            this.lblHTTP.Name = "lblHTTP";
            this.lblHTTP.Size = new System.Drawing.Size(39, 13);
            this.lblHTTP.TabIndex = 0;
            this.lblHTTP.Text = "HTTP:";
            this.lblHTTP.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblEmailNotificationPage
            // 
            this.lblEmailNotificationPage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblEmailNotificationPage.AutoSize = true;
            this.lblEmailNotificationPage.ForeColor = System.Drawing.SystemColors.MenuHighlight;
            this.lblEmailNotificationPage.Location = new System.Drawing.Point(4, 20);
            this.lblEmailNotificationPage.Name = "lblEmailNotificationPage";
            this.lblEmailNotificationPage.Size = new System.Drawing.Size(366, 13);
            this.lblEmailNotificationPage.TabIndex = 19;
            this.lblEmailNotificationPage.Text = "EVEMon can send you an email whenever a skill level completes its training.";
            // 
            // lblNotificationsPage
            // 
            this.lblNotificationsPage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblNotificationsPage.ForeColor = System.Drawing.SystemColors.MenuHighlight;
            this.lblNotificationsPage.Location = new System.Drawing.Point(4, 14);
            this.lblNotificationsPage.Name = "lblNotificationsPage";
            this.lblNotificationsPage.Size = new System.Drawing.Size(424, 43);
            this.lblNotificationsPage.TabIndex = 19;
            this.lblNotificationsPage.Text = "You can choose what notifications will be shown in your system\'s tray notificatio" +
    "n area or in EVEMon\'s main window and when. You can also toggle the sound notifi" +
    "cation upon skill completion on or off.";
            // 
            // lblTrayIconPage
            // 
            this.lblTrayIconPage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTrayIconPage.ForeColor = System.Drawing.SystemColors.MenuHighlight;
            this.lblTrayIconPage.Location = new System.Drawing.Point(4, 20);
            this.lblTrayIconPage.Name = "lblTrayIconPage";
            this.lblTrayIconPage.Size = new System.Drawing.Size(424, 31);
            this.lblTrayIconPage.TabIndex = 18;
            this.lblTrayIconPage.Text = "Here you can set the visible status of EVEMon\'s Tray Icon, configure the style of" +
    " the Tray Icon\'s popup info and EVEMon\'s behaviour upon pressing the Close butto" +
    "n.\r\n";
            // 
            // lblSchedulerUIPage
            // 
            this.lblSchedulerUIPage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSchedulerUIPage.ForeColor = System.Drawing.SystemColors.MenuHighlight;
            this.lblSchedulerUIPage.Location = new System.Drawing.Point(6, 34);
            this.lblSchedulerUIPage.Name = "lblSchedulerUIPage";
            this.lblSchedulerUIPage.Size = new System.Drawing.Size(422, 45);
            this.lblSchedulerUIPage.TabIndex = 6;
            this.lblSchedulerUIPage.Text = "Select the colors used in the scheduler. Using the scheduler, EVEMon can warn you" +
    " about skill that will complete at times you will be away from your computer.";
            // 
            // lblText
            // 
            this.lblText.AutoSize = true;
            this.lblText.Location = new System.Drawing.Point(6, 109);
            this.lblText.Name = "lblText";
            this.lblText.Size = new System.Drawing.Size(31, 13);
            this.lblText.TabIndex = 5;
            this.lblText.Text = "Text:";
            // 
            // lblBlockingEvents
            // 
            this.lblBlockingEvents.AutoSize = true;
            this.lblBlockingEvents.Location = new System.Drawing.Point(6, 132);
            this.lblBlockingEvents.Name = "lblBlockingEvents";
            this.lblBlockingEvents.Size = new System.Drawing.Size(87, 13);
            this.lblBlockingEvents.TabIndex = 0;
            this.lblBlockingEvents.Text = "Blocking Events:";
            // 
            // lblRecurringEvents
            // 
            this.lblRecurringEvents.AutoSize = true;
            this.lblRecurringEvents.Location = new System.Drawing.Point(6, 180);
            this.lblRecurringEvents.Name = "lblRecurringEvents";
            this.lblRecurringEvents.Size = new System.Drawing.Size(92, 13);
            this.lblRecurringEvents.TabIndex = 1;
            this.lblRecurringEvents.Text = "Recurring Events:";
            // 
            // lblSimpleEvents
            // 
            this.lblSimpleEvents.AutoSize = true;
            this.lblSimpleEvents.Location = new System.Drawing.Point(6, 157);
            this.lblSimpleEvents.Name = "lblSimpleEvents";
            this.lblSimpleEvents.Size = new System.Drawing.Size(77, 13);
            this.lblSimpleEvents.TabIndex = 2;
            this.lblSimpleEvents.Text = "Simple Events:";
            // 
            // lblExternalCalendarPage
            // 
            this.lblExternalCalendarPage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblExternalCalendarPage.ForeColor = System.Drawing.SystemColors.MenuHighlight;
            this.lblExternalCalendarPage.Location = new System.Drawing.Point(2, 17);
            this.lblExternalCalendarPage.Name = "lblExternalCalendarPage";
            this.lblExternalCalendarPage.Size = new System.Drawing.Size(429, 72);
            this.lblExternalCalendarPage.TabIndex = 11;
            this.lblExternalCalendarPage.Text = resources.GetString("lblExternalCalendarPage.Text");
            // 
            // lblG15Page
            // 
            this.lblG15Page.AutoSize = true;
            this.lblG15Page.ForeColor = System.Drawing.SystemColors.MenuHighlight;
            this.lblG15Page.Location = new System.Drawing.Point(3, 40);
            this.lblG15Page.Name = "lblG15Page";
            this.lblG15Page.Size = new System.Drawing.Size(347, 13);
            this.lblG15Page.TabIndex = 5;
            this.lblG15Page.Text = "EVEMon supports the LCD display of the Logitech G15/G510 keyboard.";
            // 
            // lblCycleTrainingSeconds
            // 
            this.lblCycleTrainingSeconds.AutoSize = true;
            this.lblCycleTrainingSeconds.Location = new System.Drawing.Point(265, 6);
            this.lblCycleTrainingSeconds.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
            this.lblCycleTrainingSeconds.Name = "lblCycleTrainingSeconds";
            this.lblCycleTrainingSeconds.Size = new System.Drawing.Size(47, 13);
            this.lblCycleTrainingSeconds.TabIndex = 9;
            this.lblCycleTrainingSeconds.Text = "seconds";
            // 
            // lblG15CycleCharSeconds
            // 
            this.lblG15CycleCharSeconds.AutoSize = true;
            this.lblG15CycleCharSeconds.Location = new System.Drawing.Point(186, 6);
            this.lblG15CycleCharSeconds.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
            this.lblG15CycleCharSeconds.Name = "lblG15CycleCharSeconds";
            this.lblG15CycleCharSeconds.Size = new System.Drawing.Size(47, 13);
            this.lblG15CycleCharSeconds.TabIndex = 6;
            this.lblG15CycleCharSeconds.Text = "seconds";
            // 
            // lblIconsPage
            // 
            this.lblIconsPage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblIconsPage.ForeColor = System.Drawing.SystemColors.MenuHighlight;
            this.lblIconsPage.Location = new System.Drawing.Point(4, 20);
            this.lblIconsPage.Name = "lblIconsPage";
            this.lblIconsPage.Size = new System.Drawing.Size(424, 32);
            this.lblIconsPage.TabIndex = 15;
            this.lblIconsPage.Text = "You can customize the icons used in the skill planner; if you have a good idea fo" +
    "r a set of icons instructions to create your own can be found on wiki.";
            // 
            // gbSkillBrowserIconSet
            // 
            this.gbSkillBrowserIconSet.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbSkillBrowserIconSet.Controls.Add(this.iconsSetTableLayoutPanel);
            this.gbSkillBrowserIconSet.Location = new System.Drawing.Point(7, 55);
            this.gbSkillBrowserIconSet.Name = "gbSkillBrowserIconSet";
            this.gbSkillBrowserIconSet.Size = new System.Drawing.Size(225, 204);
            this.gbSkillBrowserIconSet.TabIndex = 14;
            this.gbSkillBrowserIconSet.TabStop = false;
            this.gbSkillBrowserIconSet.Text = "Skill Browser Icon Set";
            // 
            // iconsSetTableLayoutPanel
            // 
            this.iconsSetTableLayoutPanel.AutoSize = true;
            this.iconsSetTableLayoutPanel.ColumnCount = 1;
            this.iconsSetTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.iconsSetTableLayoutPanel.Controls.Add(this.cbSkillIconSet, 0, 0);
            this.iconsSetTableLayoutPanel.Controls.Add(this.tvlist, 0, 1);
            this.iconsSetTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.iconsSetTableLayoutPanel.Location = new System.Drawing.Point(3, 16);
            this.iconsSetTableLayoutPanel.Name = "iconsSetTableLayoutPanel";
            this.iconsSetTableLayoutPanel.RowCount = 2;
            this.iconsSetTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.iconsSetTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.iconsSetTableLayoutPanel.Size = new System.Drawing.Size(219, 185);
            this.iconsSetTableLayoutPanel.TabIndex = 15;
            // 
            // cbSkillIconSet
            // 
            this.cbSkillIconSet.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbSkillIconSet.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbSkillIconSet.FormattingEnabled = true;
            this.cbSkillIconSet.Location = new System.Drawing.Point(3, 3);
            this.cbSkillIconSet.Name = "cbSkillIconSet";
            this.cbSkillIconSet.Size = new System.Drawing.Size(213, 21);
            this.cbSkillIconSet.TabIndex = 3;
            this.cbSkillIconSet.SelectedIndexChanged += new System.EventHandler(this.skillIconSetComboBox_SelectedIndexChanged);
            // 
            // tvlist
            // 
            this.tvlist.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tvlist.Location = new System.Drawing.Point(3, 30);
            this.tvlist.Name = "tvlist";
            this.tvlist.Size = new System.Drawing.Size(213, 152);
            this.tvlist.TabIndex = 9;
            // 
            // lblObsoletePlanEntries
            // 
            this.lblObsoletePlanEntries.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblObsoletePlanEntries.ForeColor = System.Drawing.SystemColors.MenuHighlight;
            this.lblObsoletePlanEntries.Location = new System.Drawing.Point(5, 30);
            this.lblObsoletePlanEntries.Name = "lblObsoletePlanEntries";
            this.lblObsoletePlanEntries.Size = new System.Drawing.Size(424, 28);
            this.lblObsoletePlanEntries.TabIndex = 24;
            this.lblObsoletePlanEntries.Text = "You can configure how EVEMon handles skills that appear to be completed and reset" +
    " the appearing messages behavior.";
            // 
            // cbUseIncreasedContrastOnOverview
            // 
            this.cbUseIncreasedContrastOnOverview.AutoSize = true;
            this.cbUseIncreasedContrastOnOverview.Location = new System.Drawing.Point(161, 37);
            this.cbUseIncreasedContrastOnOverview.Name = "cbUseIncreasedContrastOnOverview";
            this.cbUseIncreasedContrastOnOverview.Size = new System.Drawing.Size(137, 17);
            this.cbUseIncreasedContrastOnOverview.TabIndex = 34;
            this.cbUseIncreasedContrastOnOverview.Text = "Use Increased Contrast";
            this.ttToolTipCodes.SetToolTip(this.cbUseIncreasedContrastOnOverview, "When enabled, increases the contrast of the shown info");
            this.cbUseIncreasedContrastOnOverview.UseVisualStyleBackColor = true;
            // 
            // overviewGroupCharactersInTrainingCheckBox
            // 
            this.overviewGroupCharactersInTrainingCheckBox.AutoSize = true;
            this.overviewGroupCharactersInTrainingCheckBox.Location = new System.Drawing.Point(161, 56);
            this.overviewGroupCharactersInTrainingCheckBox.Name = "overviewGroupCharactersInTrainingCheckBox";
            this.overviewGroupCharactersInTrainingCheckBox.Size = new System.Drawing.Size(162, 17);
            this.overviewGroupCharactersInTrainingCheckBox.TabIndex = 33;
            this.overviewGroupCharactersInTrainingCheckBox.Text = "Group Characters In Training";
            this.ttToolTipCodes.SetToolTip(this.overviewGroupCharactersInTrainingCheckBox, "When enabled, groups the character\'s\r\nthat are currently in training");
            this.overviewGroupCharactersInTrainingCheckBox.UseVisualStyleBackColor = true;
            // 
            // overviewShowSkillQueueTrainingTimeCheckBox
            // 
            this.overviewShowSkillQueueTrainingTimeCheckBox.AutoSize = true;
            this.overviewShowSkillQueueTrainingTimeCheckBox.Location = new System.Drawing.Point(161, 3);
            this.overviewShowSkillQueueTrainingTimeCheckBox.Name = "overviewShowSkillQueueTrainingTimeCheckBox";
            this.overviewShowSkillQueueTrainingTimeCheckBox.Size = new System.Drawing.Size(177, 17);
            this.overviewShowSkillQueueTrainingTimeCheckBox.TabIndex = 32;
            this.overviewShowSkillQueueTrainingTimeCheckBox.Text = "Show Skill Queue Training Time";
            this.ttToolTipCodes.SetToolTip(this.overviewShowSkillQueueTrainingTimeCheckBox, "When enabled, shows the character\'s\r\nskill queue training time");
            this.overviewShowSkillQueueTrainingTimeCheckBox.UseVisualStyleBackColor = true;
            // 
            // overviewShowWalletCheckBox
            // 
            this.overviewShowWalletCheckBox.AutoSize = true;
            this.overviewShowWalletCheckBox.Location = new System.Drawing.Point(3, 3);
            this.overviewShowWalletCheckBox.Name = "overviewShowWalletCheckBox";
            this.overviewShowWalletCheckBox.Size = new System.Drawing.Size(128, 17);
            this.overviewShowWalletCheckBox.TabIndex = 30;
            this.overviewShowWalletCheckBox.Text = "Show Wallet Balance";
            this.ttToolTipCodes.SetToolTip(this.overviewShowWalletCheckBox, "When enabled, shows the character\'s wallet balance");
            this.overviewShowWalletCheckBox.UseVisualStyleBackColor = true;
            // 
            // overviewShowPortraitCheckBox
            // 
            this.overviewShowPortraitCheckBox.AutoSize = true;
            this.overviewShowPortraitCheckBox.Location = new System.Drawing.Point(3, 20);
            this.overviewShowPortraitCheckBox.Name = "overviewShowPortraitCheckBox";
            this.overviewShowPortraitCheckBox.Size = new System.Drawing.Size(138, 17);
            this.overviewShowPortraitCheckBox.TabIndex = 26;
            this.overviewShowPortraitCheckBox.Text = "Show Character Portrait";
            this.ttToolTipCodes.SetToolTip(this.overviewShowPortraitCheckBox, "When enabled, shows the character\'s portrait\r\nas a thumbnail alongside the charac" +
        "ter\'s name");
            this.overviewShowPortraitCheckBox.UseVisualStyleBackColor = true;
            // 
            // cbShowOverViewTab
            // 
            this.cbShowOverViewTab.AutoSize = true;
            this.cbShowOverViewTab.Location = new System.Drawing.Point(14, 17);
            this.cbShowOverViewTab.Name = "cbShowOverViewTab";
            this.cbShowOverViewTab.Size = new System.Drawing.Size(129, 17);
            this.cbShowOverViewTab.TabIndex = 0;
            this.cbShowOverViewTab.Text = "Show \"Overview\" tab";
            this.ttToolTipCodes.SetToolTip(this.cbShowOverViewTab, "When enabled, shows the Overview tab");
            this.cbShowOverViewTab.UseVisualStyleBackColor = true;
            this.cbShowOverViewTab.CheckedChanged += new System.EventHandler(this.OnMustEnableOrDisable);
            // 
            // treeView
            // 
            this.treeView.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.treeView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.treeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView.FullRowSelect = true;
            this.treeView.HideSelection = false;
            this.treeView.ImageIndex = 0;
            this.treeView.ImageList = this.imageList;
            this.treeView.ItemHeight = 20;
            this.treeView.Location = new System.Drawing.Point(6, 6);
            this.treeView.Name = "treeView";
            treeNode1.ImageIndex = 11;
            treeNode1.Name = "UpdatesNode";
            treeNode1.SelectedImageIndex = 11;
            treeNode1.Tag = "updatesPage";
            treeNode1.Text = "Updates";
            treeNode2.ImageIndex = 7;
            treeNode2.Name = "networkNode";
            treeNode2.SelectedImageIndex = 7;
            treeNode2.Tag = "networkPage";
            treeNode2.Text = "Network";
            treeNode3.ImageIndex = 4;
            treeNode3.Name = "g15Node";
            treeNode3.SelectedImageIndex = 4;
            treeNode3.Tag = "g15Page";
            treeNode3.Text = "Logitech Keyboards";
            treeNode4.ImageIndex = 15;
            treeNode4.Name = "PortableEveClientsNode";
            treeNode4.SelectedImageIndex = 15;
            treeNode4.Tag = "portableEveClientsPage";
            treeNode4.Text = "Portable EVE Clients";
            treeNode5.ImageIndex = 16;
            treeNode5.Name = "MarketPriceProvidersNode";
            treeNode5.SelectedImageIndex = 16;
            treeNode5.Tag = "marketPriceProvidersPage";
            treeNode5.Text = "Market Price Providers";
            treeNode6.ImageIndex = 10;
            treeNode6.Name = "generalNode";
            treeNode6.SelectedImageIndex = 10;
            treeNode6.Tag = "generalPage";
            treeNode6.Text = "General";
            treeNode7.ImageIndex = 6;
            treeNode7.Name = "Node3";
            treeNode7.SelectedImageIndex = 6;
            treeNode7.Tag = "mainWindowPage";
            treeNode7.Text = "Main Window";
            treeNode8.ImageIndex = 13;
            treeNode8.Name = "IconsNode";
            treeNode8.SelectedImageIndex = 13;
            treeNode8.Tag = "iconsPage";
            treeNode8.Text = "Icons";
            treeNode9.ImageIndex = 14;
            treeNode9.Name = "MassagesNode";
            treeNode9.SelectedImageIndex = 14;
            treeNode9.Tag = "messagesPage";
            treeNode9.Text = "Messages";
            treeNode10.ImageIndex = 8;
            treeNode10.Name = "Node4";
            treeNode10.SelectedImageIndex = 8;
            treeNode10.Tag = "skillPlannerPage";
            treeNode10.Text = "Skill Planner";
            treeNode11.ImageIndex = 2;
            treeNode11.Name = "trayIconNode";
            treeNode11.SelectedImageIndex = 2;
            treeNode11.Tag = "trayIconPage";
            treeNode11.Text = "System Tray Icon";
            treeNode12.ImageIndex = 5;
            treeNode12.Name = "Node11";
            treeNode12.SelectedImageIndex = 5;
            treeNode12.Tag = "externalCalendarPage";
            treeNode12.Text = "External Calendar";
            treeNode13.ImageIndex = 1;
            treeNode13.Name = "Node10";
            treeNode13.SelectedImageIndex = 1;
            treeNode13.Tag = "schedulerUIPage";
            treeNode13.Text = "Scheduler";
            treeNode14.ImageIndex = 12;
            treeNode14.Name = "Node7";
            treeNode14.SelectedImageIndex = 12;
            treeNode14.Tag = "emailNotificationsPage";
            treeNode14.Text = "Skill Completion Mails";
            treeNode15.ImageIndex = 9;
            treeNode15.Name = "Node2";
            treeNode15.SelectedImageIndex = 9;
            treeNode15.Tag = "notificationsPage";
            treeNode15.Text = "Notifications";
            treeNode16.ImageIndex = 17;
            treeNode16.Name = "CloudStorageServiceNode";
            treeNode16.SelectedImageIndex = 17;
            treeNode16.Tag = "cloudStorageServicePage";
            treeNode16.Text = "Cloud Storage Service";
            this.treeView.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode6,
            treeNode7,
            treeNode10,
            treeNode11,
            treeNode13,
            treeNode15,
            treeNode16});
            this.treeView.SelectedImageIndex = 0;
            this.treeView.ShowLines = false;
            this.treeView.ShowPlusMinus = false;
            this.treeView.ShowRootLines = false;
            this.treeView.Size = new System.Drawing.Size(187, 424);
            this.treeView.TabIndex = 6;
            this.treeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView_AfterSelect);
            // 
            // imageList
            // 
            this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
            this.imageList.TransparentColor = System.Drawing.Color.Empty;
            this.imageList.Images.SetKeyName(0, "Transparent");
            this.imageList.Images.SetKeyName(1, "Calendar");
            this.imageList.Images.SetKeyName(2, "EVEMon");
            this.imageList.Images.SetKeyName(3, "IGB");
            this.imageList.Images.SetKeyName(4, "LogitechKeyboard");
            this.imageList.Images.SetKeyName(5, "gcalendar");
            this.imageList.Images.SetKeyName(6, "MainWindow");
            this.imageList.Images.SetKeyName(7, "Network");
            this.imageList.Images.SetKeyName(8, "Plan");
            this.imageList.Images.SetKeyName(9, "Problem");
            this.imageList.Images.SetKeyName(10, "Settings");
            this.imageList.Images.SetKeyName(11, "Update");
            this.imageList.Images.SetKeyName(12, "Email");
            this.imageList.Images.SetKeyName(13, "book");
            this.imageList.Images.SetKeyName(14, "messagebox");
            this.imageList.Images.SetKeyName(15, "EveClient");
            this.imageList.Images.SetKeyName(16, "Wallet");
            this.imageList.Images.SetKeyName(17, "CloudStorage");
            // 
            // leftPanel
            // 
            this.leftPanel.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.leftPanel.Controls.Add(this.treeView);
            this.leftPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.leftPanel.Location = new System.Drawing.Point(0, 0);
            this.leftPanel.Name = "leftPanel";
            this.leftPanel.Padding = new System.Windows.Forms.Padding(6);
            this.leftPanel.Size = new System.Drawing.Size(199, 436);
            this.leftPanel.TabIndex = 9;
            // 
            // multiPanel
            // 
            this.multiPanel.CausesValidation = false;
            this.multiPanel.Controls.Add(this.generalPage);
            this.multiPanel.Controls.Add(this.mainWindowPage);
            this.multiPanel.Controls.Add(this.skillPlannerPage);
            this.multiPanel.Controls.Add(this.networkPage);
            this.multiPanel.Controls.Add(this.emailNotificationsPage);
            this.multiPanel.Controls.Add(this.notificationsPage);
            this.multiPanel.Controls.Add(this.trayIconPage);
            this.multiPanel.Controls.Add(this.updatesPage);
            this.multiPanel.Controls.Add(this.schedulerUIPage);
            this.multiPanel.Controls.Add(this.externalCalendarPage);
            this.multiPanel.Controls.Add(this.g15Page);
            this.multiPanel.Controls.Add(this.iconsPage);
            this.multiPanel.Controls.Add(this.messagesPage);
            this.multiPanel.Controls.Add(this.portableEveClientsPage);
            this.multiPanel.Controls.Add(this.marketPriceProvidersPage);
            this.multiPanel.Controls.Add(this.cloudStorageServicePage);
            this.multiPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.multiPanel.Location = new System.Drawing.Point(199, 0);
            this.multiPanel.Name = "multiPanel";
            this.multiPanel.Padding = new System.Windows.Forms.Padding(5);
            this.multiPanel.SelectedPage = this.skillPlannerPage;
            this.multiPanel.Size = new System.Drawing.Size(445, 436);
            this.multiPanel.TabIndex = 7;
            // 
            // generalPage
            // 
            this.generalPage.Controls.Add(this.btnEVEMonDataDir);
            this.generalPage.Controls.Add(this.lblGeneralPage);
            this.generalPage.Controls.Add(this.cbWorksafeMode);
            this.generalPage.Controls.Add(this.compatibilityCombo);
            this.generalPage.Controls.Add(this.lblEnvironment);
            this.generalPage.Controls.Add(this.runAtStartupComboBox);
            this.generalPage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.generalPage.Location = new System.Drawing.Point(5, 5);
            this.generalPage.Name = "generalPage";
            this.generalPage.Size = new System.Drawing.Size(435, 426);
            this.generalPage.TabIndex = 0;
            this.generalPage.Text = "generalPage";
            // 
            // btnEVEMonDataDir
            // 
            this.btnEVEMonDataDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnEVEMonDataDir.AutoSize = true;
            this.btnEVEMonDataDir.Location = new System.Drawing.Point(296, 392);
            this.btnEVEMonDataDir.Name = "btnEVEMonDataDir";
            this.btnEVEMonDataDir.Size = new System.Drawing.Size(132, 23);
            this.btnEVEMonDataDir.TabIndex = 21;
            this.btnEVEMonDataDir.Text = "EVEMon Data Directory";
            this.btnEVEMonDataDir.UseVisualStyleBackColor = true;
            this.btnEVEMonDataDir.Click += new System.EventHandler(this.btnEVEMonDataDir_Click);
            // 
            // cbWorksafeMode
            // 
            this.cbWorksafeMode.AutoSize = true;
            this.cbWorksafeMode.Location = new System.Drawing.Point(3, 131);
            this.cbWorksafeMode.Name = "cbWorksafeMode";
            this.cbWorksafeMode.Size = new System.Drawing.Size(264, 17);
            this.cbWorksafeMode.TabIndex = 6;
            this.cbWorksafeMode.Text = "Run in \"safe for work\" mode (no portraits or colors)";
            this.cbWorksafeMode.UseVisualStyleBackColor = true;
            this.cbWorksafeMode.CheckedChanged += new System.EventHandler(this.OnMustEnableOrDisable);
            // 
            // compatibilityCombo
            // 
            this.compatibilityCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.compatibilityCombo.FormattingEnabled = true;
            this.compatibilityCombo.Items.AddRange(new object[] {
            "Windows",
            "Wine"});
            this.compatibilityCombo.Location = new System.Drawing.Point(231, 178);
            this.compatibilityCombo.Name = "compatibilityCombo";
            this.compatibilityCombo.Size = new System.Drawing.Size(121, 21);
            this.compatibilityCombo.TabIndex = 7;
            // 
            // runAtStartupComboBox
            // 
            this.runAtStartupComboBox.AutoSize = true;
            this.runAtStartupComboBox.Location = new System.Drawing.Point(3, 88);
            this.runAtStartupComboBox.Name = "runAtStartupComboBox";
            this.runAtStartupComboBox.Size = new System.Drawing.Size(140, 17);
            this.runAtStartupComboBox.TabIndex = 5;
            this.runAtStartupComboBox.Text = "Run EVEMon at Startup";
            this.runAtStartupComboBox.UseVisualStyleBackColor = true;
            // 
            // mainWindowPage
            // 
            this.mainWindowPage.Controls.Add(this.lblMainWindowPage);
            this.mainWindowPage.Controls.Add(this.OverviewGroupBox);
            this.mainWindowPage.Controls.Add(this.CharacterMonitorGroupBox);
            this.mainWindowPage.Controls.Add(this.WindowTitleGroupBox);
            this.mainWindowPage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainWindowPage.Location = new System.Drawing.Point(5, 5);
            this.mainWindowPage.Name = "mainWindowPage";
            this.mainWindowPage.Size = new System.Drawing.Size(435, 426);
            this.mainWindowPage.TabIndex = 1;
            this.mainWindowPage.Text = "mainWindowPage";
            // 
            // OverviewGroupBox
            // 
            this.OverviewGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.OverviewGroupBox.Controls.Add(this.overviewPanel);
            this.OverviewGroupBox.Controls.Add(this.cbShowOverViewTab);
            this.OverviewGroupBox.Location = new System.Drawing.Point(1, 293);
            this.OverviewGroupBox.Name = "OverviewGroupBox";
            this.OverviewGroupBox.Size = new System.Drawing.Size(428, 127);
            this.OverviewGroupBox.TabIndex = 15;
            this.OverviewGroupBox.TabStop = false;
            this.OverviewGroupBox.Text = "Overview";
            // 
            // overviewPanel
            // 
            this.overviewPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.overviewPanel.Controls.Add(this.cbShowSkillpointsOnOverview);
            this.overviewPanel.Controls.Add(this.cbUseIncreasedContrastOnOverview);
            this.overviewPanel.Controls.Add(this.overviewGroupCharactersInTrainingCheckBox);
            this.overviewPanel.Controls.Add(this.overviewShowSkillQueueTrainingTimeCheckBox);
            this.overviewPanel.Controls.Add(this.overviewShowWalletCheckBox);
            this.overviewPanel.Controls.Add(this.lblSize);
            this.overviewPanel.Controls.Add(this.overviewShowPortraitCheckBox);
            this.overviewPanel.Controls.Add(this.overviewPortraitSizeComboBox);
            this.overviewPanel.Location = new System.Drawing.Point(29, 36);
            this.overviewPanel.Name = "overviewPanel";
            this.overviewPanel.Size = new System.Drawing.Size(393, 85);
            this.overviewPanel.TabIndex = 32;
            // 
            // cbShowSkillpointsOnOverview
            // 
            this.cbShowSkillpointsOnOverview.AutoSize = true;
            this.cbShowSkillpointsOnOverview.Location = new System.Drawing.Point(161, 20);
            this.cbShowSkillpointsOnOverview.Name = "cbShowSkillpointsOnOverview";
            this.cbShowSkillpointsOnOverview.Size = new System.Drawing.Size(107, 17);
            this.cbShowSkillpointsOnOverview.TabIndex = 35;
            this.cbShowSkillpointsOnOverview.Text = "Show Skill Points";
            this.cbShowSkillpointsOnOverview.UseVisualStyleBackColor = true;
            // 
            // overviewPortraitSizeComboBox
            // 
            this.overviewPortraitSizeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.overviewPortraitSizeComboBox.FormattingEnabled = true;
            this.overviewPortraitSizeComboBox.Location = new System.Drawing.Point(52, 49);
            this.overviewPortraitSizeComboBox.Name = "overviewPortraitSizeComboBox";
            this.overviewPortraitSizeComboBox.Size = new System.Drawing.Size(79, 21);
            this.overviewPortraitSizeComboBox.TabIndex = 28;
            // 
            // skillPlannerPage
            // 
            this.skillPlannerPage.Controls.Add(this.cbAdvanceEntryAdd);
            this.skillPlannerPage.Controls.Add(this.cbSummaryOnMultiSelectOnly);
            this.skillPlannerPage.Controls.Add(this.lblSkillPlannerPage);
            this.skillPlannerPage.Controls.Add(this.cbHighlightQueuedSiklls);
            this.skillPlannerPage.Controls.Add(this.cbHighlightPartialSkills);
            this.skillPlannerPage.Controls.Add(this.cbHighlightConflicts);
            this.skillPlannerPage.Controls.Add(this.cbHighlightPrerequisites);
            this.skillPlannerPage.Controls.Add(this.cbHighlightPlannedSkills);
            this.skillPlannerPage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.skillPlannerPage.Location = new System.Drawing.Point(5, 5);
            this.skillPlannerPage.Name = "skillPlannerPage";
            this.skillPlannerPage.Size = new System.Drawing.Size(435, 426);
            this.skillPlannerPage.TabIndex = 3;
            this.skillPlannerPage.Text = "skillPlannerPage";
            this.skillPlannerPage.Visible = false;
            // 
            // cbAdvanceEntryAdd
            // 
            this.cbAdvanceEntryAdd.AutoSize = true;
            this.cbAdvanceEntryAdd.Location = new System.Drawing.Point(14, 198);
            this.cbAdvanceEntryAdd.Name = "cbAdvanceEntryAdd";
            this.cbAdvanceEntryAdd.Size = new System.Drawing.Size(211, 17);
            this.cbAdvanceEntryAdd.TabIndex = 21;
            this.cbAdvanceEntryAdd.Text = "Set Priority When Adding Skills To Plan";
            this.cbAdvanceEntryAdd.UseVisualStyleBackColor = true;
            // 
            // cbSummaryOnMultiSelectOnly
            // 
            this.cbSummaryOnMultiSelectOnly.AutoSize = true;
            this.cbSummaryOnMultiSelectOnly.Location = new System.Drawing.Point(14, 175);
            this.cbSummaryOnMultiSelectOnly.Name = "cbSummaryOnMultiSelectOnly";
            this.cbSummaryOnMultiSelectOnly.Size = new System.Drawing.Size(232, 17);
            this.cbSummaryOnMultiSelectOnly.TabIndex = 20;
            this.cbSummaryOnMultiSelectOnly.Text = "Show Plan Summary Only On \"Multi-Select\"";
            this.cbSummaryOnMultiSelectOnly.UseVisualStyleBackColor = true;
            // 
            // cbHighlightQueuedSiklls
            // 
            this.cbHighlightQueuedSiklls.AutoSize = true;
            this.cbHighlightQueuedSiklls.Location = new System.Drawing.Point(14, 152);
            this.cbHighlightQueuedSiklls.Name = "cbHighlightQueuedSiklls";
            this.cbHighlightQueuedSiklls.Size = new System.Drawing.Size(135, 17);
            this.cbHighlightQueuedSiklls.TabIndex = 14;
            this.cbHighlightQueuedSiklls.Text = "Highlight Queued Skills";
            this.cbHighlightQueuedSiklls.UseVisualStyleBackColor = true;
            // 
            // cbHighlightPartialSkills
            // 
            this.cbHighlightPartialSkills.AutoSize = true;
            this.cbHighlightPartialSkills.Location = new System.Drawing.Point(14, 129);
            this.cbHighlightPartialSkills.Name = "cbHighlightPartialSkills";
            this.cbHighlightPartialSkills.Size = new System.Drawing.Size(172, 17);
            this.cbHighlightPartialSkills.TabIndex = 10;
            this.cbHighlightPartialSkills.Text = "Highlight Partially Trained Skills";
            this.cbHighlightPartialSkills.UseVisualStyleBackColor = true;
            // 
            // cbHighlightConflicts
            // 
            this.cbHighlightConflicts.AutoSize = true;
            this.cbHighlightConflicts.Location = new System.Drawing.Point(14, 83);
            this.cbHighlightConflicts.Name = "cbHighlightConflicts";
            this.cbHighlightConflicts.Size = new System.Drawing.Size(158, 17);
            this.cbHighlightConflicts.TabIndex = 9;
            this.cbHighlightConflicts.Text = "Highlight Schedule Conflicts";
            this.cbHighlightConflicts.UseVisualStyleBackColor = true;
            // 
            // cbHighlightPrerequisites
            // 
            this.cbHighlightPrerequisites.AutoSize = true;
            this.cbHighlightPrerequisites.Location = new System.Drawing.Point(14, 106);
            this.cbHighlightPrerequisites.Name = "cbHighlightPrerequisites";
            this.cbHighlightPrerequisites.Size = new System.Drawing.Size(130, 17);
            this.cbHighlightPrerequisites.TabIndex = 8;
            this.cbHighlightPrerequisites.Text = "Highlight Prerequisites";
            this.cbHighlightPrerequisites.UseVisualStyleBackColor = true;
            // 
            // cbHighlightPlannedSkills
            // 
            this.cbHighlightPlannedSkills.AutoSize = true;
            this.cbHighlightPlannedSkills.Location = new System.Drawing.Point(14, 60);
            this.cbHighlightPlannedSkills.Name = "cbHighlightPlannedSkills";
            this.cbHighlightPlannedSkills.Size = new System.Drawing.Size(136, 17);
            this.cbHighlightPlannedSkills.TabIndex = 0;
            this.cbHighlightPlannedSkills.Text = "Highlight Planned Skills";
            this.cbHighlightPlannedSkills.UseVisualStyleBackColor = true;
            // 
            // networkPage
            // 
            this.networkPage.Controls.Add(this.esiSettingsGroupBox);
            this.networkPage.Controls.Add(this.ProxyServerGroupBox);
            this.networkPage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.networkPage.Location = new System.Drawing.Point(5, 5);
            this.networkPage.Name = "networkPage";
            this.networkPage.Size = new System.Drawing.Size(435, 426);
            this.networkPage.TabIndex = 4;
            this.networkPage.Text = "networkPage";
            this.networkPage.Visible = false;
            // 
            // esiSettingsGroupBox
            // 
            this.esiSettingsGroupBox.Controls.Add(this.insertAllScopesButton);
            this.esiSettingsGroupBox.Controls.Add(this.scopesTextBox);
            this.esiSettingsGroupBox.Controls.Add(this.lblScopes);
            this.esiSettingsGroupBox.Controls.Add(this.lblClientSecret);
            this.esiSettingsGroupBox.Controls.Add(this.lblClientID);
            this.esiSettingsGroupBox.Controls.Add(this.esiSettingsLabel);
            this.esiSettingsGroupBox.Controls.Add(this.clientSecretTextBox);
            this.esiSettingsGroupBox.Controls.Add(this.clientIDTextBox);
            this.esiSettingsGroupBox.Location = new System.Drawing.Point(3, 194);
            this.esiSettingsGroupBox.Name = "esiSettingsGroupBox";
            this.esiSettingsGroupBox.Size = new System.Drawing.Size(409, 137);
            this.esiSettingsGroupBox.TabIndex = 1;
            this.esiSettingsGroupBox.TabStop = false;
            this.esiSettingsGroupBox.Text = "ESI Client Settings";
            // 
            // scopesTextBox
            // 
            this.scopesTextBox.Location = new System.Drawing.Point(82, 83);
            this.scopesTextBox.Name = "scopesTextBox";
            this.scopesTextBox.Size = new System.Drawing.Size(315, 20);
            this.scopesTextBox.TabIndex = 9;
            // 
            // lblScopes
            // 
            this.lblScopes.AutoSize = true;
            this.lblScopes.Location = new System.Drawing.Point(33, 86);
            this.lblScopes.Name = "lblScopes";
            this.lblScopes.Size = new System.Drawing.Size(43, 13);
            this.lblScopes.TabIndex = 8;
            this.lblScopes.Text = "Scopes";
            // 
            // lblClientSecret
            // 
            this.lblClientSecret.AutoSize = true;
            this.lblClientSecret.Location = new System.Drawing.Point(9, 62);
            this.lblClientSecret.Name = "lblClientSecret";
            this.lblClientSecret.Size = new System.Drawing.Size(67, 13);
            this.lblClientSecret.TabIndex = 2;
            this.lblClientSecret.Text = "Client Secret";
            // 
            // lblClientID
            // 
            this.lblClientID.AutoSize = true;
            this.lblClientID.Location = new System.Drawing.Point(29, 39);
            this.lblClientID.Name = "lblClientID";
            this.lblClientID.Size = new System.Drawing.Size(47, 13);
            this.lblClientID.TabIndex = 2;
            this.lblClientID.Text = "Client ID";
            // 
            // esiSettingsLabel
            // 
            this.esiSettingsLabel.AutoSize = true;
            this.esiSettingsLabel.LinkArea = new System.Windows.Forms.LinkArea(31, 40);
            this.esiSettingsLabel.Location = new System.Drawing.Point(9, 16);
            this.esiSettingsLabel.Name = "esiSettingsLabel";
            this.esiSettingsLabel.Size = new System.Drawing.Size(364, 17);
            this.esiSettingsLabel.TabIndex = 1;
            this.esiSettingsLabel.TabStop = true;
            this.esiSettingsLabel.Text = "Create ESI client application: https://peterhaneve.github.io/evemon-esi\r\n";
            this.esiSettingsLabel.UseCompatibleTextRendering = true;
            this.esiSettingsLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.esiSettingsLabel_LinkClicked);
            // 
            // clientSecretTextBox
            // 
            this.clientSecretTextBox.Location = new System.Drawing.Point(82, 59);
            this.clientSecretTextBox.Name = "clientSecretTextBox";
            this.clientSecretTextBox.Size = new System.Drawing.Size(315, 20);
            this.clientSecretTextBox.TabIndex = 7;
            this.clientSecretTextBox.UseSystemPasswordChar = true;
            // 
            // clientIDTextBox
            // 
            this.clientIDTextBox.Location = new System.Drawing.Point(82, 36);
            this.clientIDTextBox.Name = "clientIDTextBox";
            this.clientIDTextBox.Size = new System.Drawing.Size(315, 20);
            this.clientIDTextBox.TabIndex = 6;
            // 
            // ProxyServerGroupBox
            // 
            this.ProxyServerGroupBox.Controls.Add(this.customProxyCheckBox);
            this.ProxyServerGroupBox.Controls.Add(this.lblNetworkPageProxy);
            this.ProxyServerGroupBox.Controls.Add(this.customProxyPanel);
            this.ProxyServerGroupBox.Location = new System.Drawing.Point(3, 31);
            this.ProxyServerGroupBox.Name = "ProxyServerGroupBox";
            this.ProxyServerGroupBox.Size = new System.Drawing.Size(409, 157);
            this.ProxyServerGroupBox.TabIndex = 0;
            this.ProxyServerGroupBox.TabStop = false;
            this.ProxyServerGroupBox.Text = "Proxy Server Settings";
            // 
            // customProxyCheckBox
            // 
            this.customProxyCheckBox.AutoSize = true;
            this.customProxyCheckBox.Location = new System.Drawing.Point(9, 74);
            this.customProxyCheckBox.Name = "customProxyCheckBox";
            this.customProxyCheckBox.Size = new System.Drawing.Size(119, 17);
            this.customProxyCheckBox.TabIndex = 9;
            this.customProxyCheckBox.Text = "Use a custom proxy";
            this.customProxyCheckBox.UseVisualStyleBackColor = true;
            this.customProxyCheckBox.CheckedChanged += new System.EventHandler(this.OnMustEnableOrDisable);
            // 
            // customProxyPanel
            // 
            this.customProxyPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.customProxyPanel.Controls.Add(this.proxyPortTextBox);
            this.customProxyPanel.Controls.Add(this.lblProxyHostIPAddress);
            this.customProxyPanel.Controls.Add(this.proxyAuthenticationButton);
            this.customProxyPanel.Controls.Add(this.lblProxyPort);
            this.customProxyPanel.Controls.Add(this.lblHTTP);
            this.customProxyPanel.Controls.Add(this.proxyHttpHostTextBox);
            this.customProxyPanel.Location = new System.Drawing.Point(17, 97);
            this.customProxyPanel.Name = "customProxyPanel";
            this.customProxyPanel.Size = new System.Drawing.Size(386, 54);
            this.customProxyPanel.TabIndex = 6;
            // 
            // proxyPortTextBox
            // 
            this.proxyPortTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.proxyPortTextBox.Location = new System.Drawing.Point(239, 24);
            this.proxyPortTextBox.MaxLength = 5;
            this.proxyPortTextBox.Name = "proxyPortTextBox";
            this.proxyPortTextBox.Size = new System.Drawing.Size(38, 20);
            this.proxyPortTextBox.TabIndex = 2;
            this.proxyPortTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.proxyPortTextBox_Validating);
            // 
            // proxyAuthenticationButton
            // 
            this.proxyAuthenticationButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.proxyAuthenticationButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.proxyAuthenticationButton.Location = new System.Drawing.Point(283, 22);
            this.proxyAuthenticationButton.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.proxyAuthenticationButton.Name = "proxyAuthenticationButton";
            this.proxyAuthenticationButton.Size = new System.Drawing.Size(97, 23);
            this.proxyAuthenticationButton.TabIndex = 5;
            this.proxyAuthenticationButton.Text = "Authentication...";
            this.proxyAuthenticationButton.UseVisualStyleBackColor = true;
            this.proxyAuthenticationButton.Click += new System.EventHandler(this.proxyAuthenticationButton_Click);
            // 
            // proxyHttpHostTextBox
            // 
            this.proxyHttpHostTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.proxyHttpHostTextBox.Location = new System.Drawing.Point(50, 24);
            this.proxyHttpHostTextBox.Name = "proxyHttpHostTextBox";
            this.proxyHttpHostTextBox.Size = new System.Drawing.Size(182, 20);
            this.proxyHttpHostTextBox.TabIndex = 1;
            // 
            // emailNotificationsPage
            // 
            this.emailNotificationsPage.Controls.Add(this.lblEmailNotificationPage);
            this.emailNotificationsPage.Controls.Add(this.mailNotificationCheckBox);
            this.emailNotificationsPage.Controls.Add(this.emailNotificationsControl);
            this.emailNotificationsPage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.emailNotificationsPage.Location = new System.Drawing.Point(5, 5);
            this.emailNotificationsPage.Name = "emailNotificationsPage";
            this.emailNotificationsPage.Size = new System.Drawing.Size(435, 426);
            this.emailNotificationsPage.TabIndex = 6;
            this.emailNotificationsPage.Text = "emailNotificationsPage";
            this.emailNotificationsPage.Visible = false;
            // 
            // mailNotificationCheckBox
            // 
            this.mailNotificationCheckBox.AutoSize = true;
            this.mailNotificationCheckBox.Location = new System.Drawing.Point(7, 51);
            this.mailNotificationCheckBox.Name = "mailNotificationCheckBox";
            this.mailNotificationCheckBox.Size = new System.Drawing.Size(215, 17);
            this.mailNotificationCheckBox.TabIndex = 0;
            this.mailNotificationCheckBox.Text = "Send email when skill training completes";
            this.mailNotificationCheckBox.UseVisualStyleBackColor = true;
            this.mailNotificationCheckBox.CheckedChanged += new System.EventHandler(this.OnMustEnableOrDisable);
            // 
            // emailNotificationsControl
            // notificationsPage
            // 
            this.notificationsPage.Controls.Add(this.lblNotificationsPage);
            this.notificationsPage.Controls.Add(this.cbPlaySoundOnSkillComplete);
            this.notificationsPage.Controls.Add(this.notificationsControl);
            this.notificationsPage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.notificationsPage.Location = new System.Drawing.Point(5, 5);
            this.notificationsPage.Name = "notificationsPage";
            this.notificationsPage.Size = new System.Drawing.Size(435, 426);
            this.notificationsPage.TabIndex = 7;
            this.notificationsPage.Text = "notificationsPage";
            this.notificationsPage.Visible = false;
            // 
            // cbPlaySoundOnSkillComplete
            // 
            this.cbPlaySoundOnSkillComplete.AutoSize = true;
            this.cbPlaySoundOnSkillComplete.Location = new System.Drawing.Point(3, 401);
            this.cbPlaySoundOnSkillComplete.Name = "cbPlaySoundOnSkillComplete";
            this.cbPlaySoundOnSkillComplete.Size = new System.Drawing.Size(215, 17);
            this.cbPlaySoundOnSkillComplete.TabIndex = 3;
            this.cbPlaySoundOnSkillComplete.Text = "Play sound when skill training completes";
            this.cbPlaySoundOnSkillComplete.UseVisualStyleBackColor = true;
            // 
            // trayIconPage
            // 
            this.trayIconPage.Controls.Add(this.lblTrayIconPage);
            this.trayIconPage.Controls.Add(this.mainWindowBehaviourGroupBox);
            this.trayIconPage.Controls.Add(this.trayIconPopupGroupBox);
            this.trayIconPage.Controls.Add(this.systemTrayIconGroupBox);
            this.trayIconPage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.trayIconPage.Location = new System.Drawing.Point(5, 5);
            this.trayIconPage.Name = "trayIconPage";
            this.trayIconPage.Size = new System.Drawing.Size(435, 426);
            this.trayIconPage.TabIndex = 8;
            this.trayIconPage.Text = "trayIconPage";
            this.trayIconPage.Visible = false;
            // 
            // mainWindowBehaviourGroupBox
            // 
            this.mainWindowBehaviourGroupBox.Controls.Add(this.rbMinToTaskBar);
            this.mainWindowBehaviourGroupBox.Controls.Add(this.rbMinToTray);
            this.mainWindowBehaviourGroupBox.Controls.Add(this.rbExitEVEMon);
            this.mainWindowBehaviourGroupBox.Location = new System.Drawing.Point(9, 307);
            this.mainWindowBehaviourGroupBox.Name = "mainWindowBehaviourGroupBox";
            this.mainWindowBehaviourGroupBox.Size = new System.Drawing.Size(419, 91);
            this.mainWindowBehaviourGroupBox.TabIndex = 17;
            this.mainWindowBehaviourGroupBox.TabStop = false;
            this.mainWindowBehaviourGroupBox.Text = "Main Window Close Behaviour";
            // 
            // rbMinToTaskBar
            // 
            this.rbMinToTaskBar.AutoSize = true;
            this.rbMinToTaskBar.Location = new System.Drawing.Point(12, 66);
            this.rbMinToTaskBar.Name = "rbMinToTaskBar";
            this.rbMinToTaskBar.Size = new System.Drawing.Size(133, 17);
            this.rbMinToTaskBar.TabIndex = 2;
            this.rbMinToTaskBar.TabStop = true;
            this.rbMinToTaskBar.Text = "Minimize to the taskbar";
            this.rbMinToTaskBar.UseVisualStyleBackColor = true;
            // 
            // rbMinToTray
            // 
            this.rbMinToTray.AutoSize = true;
            this.rbMinToTray.Location = new System.Drawing.Point(12, 43);
            this.rbMinToTray.Name = "rbMinToTray";
            this.rbMinToTray.Size = new System.Drawing.Size(150, 17);
            this.rbMinToTray.TabIndex = 1;
            this.rbMinToTray.TabStop = true;
            this.rbMinToTray.Text = "Minimize to the system tray";
            this.rbMinToTray.UseVisualStyleBackColor = true;
            // 
            // rbExitEVEMon
            // 
            this.rbExitEVEMon.AutoSize = true;
            this.rbExitEVEMon.Location = new System.Drawing.Point(12, 20);
            this.rbExitEVEMon.Name = "rbExitEVEMon";
            this.rbExitEVEMon.Size = new System.Drawing.Size(87, 17);
            this.rbExitEVEMon.TabIndex = 0;
            this.rbExitEVEMon.TabStop = true;
            this.rbExitEVEMon.Text = "Exit EVEMon";
            this.rbExitEVEMon.UseVisualStyleBackColor = true;
            // 
            // trayIconPopupGroupBox
            // 
            this.trayIconPopupGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.trayIconPopupGroupBox.Controls.Add(this.trayPopupDisabledRadio);
            this.trayIconPopupGroupBox.Controls.Add(this.trayPopupButton);
            this.trayIconPopupGroupBox.Controls.Add(this.trayPopupRadio);
            this.trayIconPopupGroupBox.Controls.Add(this.trayTooltipRadio);
            this.trayIconPopupGroupBox.Controls.Add(this.trayTooltipButton);
            this.trayIconPopupGroupBox.Location = new System.Drawing.Point(9, 184);
            this.trayIconPopupGroupBox.Name = "trayIconPopupGroupBox";
            this.trayIconPopupGroupBox.Size = new System.Drawing.Size(419, 104);
            this.trayIconPopupGroupBox.TabIndex = 10;
            this.trayIconPopupGroupBox.TabStop = false;
            this.trayIconPopupGroupBox.Text = "Icon Popup Style";
            // 
            // trayPopupDisabledRadio
            // 
            this.trayPopupDisabledRadio.AutoSize = true;
            this.trayPopupDisabledRadio.Location = new System.Drawing.Point(6, 78);
            this.trayPopupDisabledRadio.Name = "trayPopupDisabledRadio";
            this.trayPopupDisabledRadio.Size = new System.Drawing.Size(66, 17);
            this.trayPopupDisabledRadio.TabIndex = 5;
            this.trayPopupDisabledRadio.TabStop = true;
            this.trayPopupDisabledRadio.Text = "Disabled";
            this.trayPopupDisabledRadio.UseVisualStyleBackColor = true;
            // 
            // trayPopupButton
            // 
            this.trayPopupButton.Location = new System.Drawing.Point(69, 46);
            this.trayPopupButton.Name = "trayPopupButton";
            this.trayPopupButton.Size = new System.Drawing.Size(75, 23);
            this.trayPopupButton.TabIndex = 4;
            this.trayPopupButton.Text = "Configure";
            this.trayPopupButton.UseVisualStyleBackColor = true;
            this.trayPopupButton.Click += new System.EventHandler(this.trayPopupButton_Click);
            // 
            // trayPopupRadio
            // 
            this.trayPopupRadio.AutoSize = true;
            this.trayPopupRadio.Location = new System.Drawing.Point(6, 49);
            this.trayPopupRadio.Name = "trayPopupRadio";
            this.trayPopupRadio.Size = new System.Drawing.Size(56, 17);
            this.trayPopupRadio.TabIndex = 3;
            this.trayPopupRadio.TabStop = true;
            this.trayPopupRadio.Text = "Popup";
            this.trayPopupRadio.UseVisualStyleBackColor = true;
            // 
            // trayTooltipRadio
            // 
            this.trayTooltipRadio.AutoSize = true;
            this.trayTooltipRadio.Location = new System.Drawing.Point(6, 20);
            this.trayTooltipRadio.Name = "trayTooltipRadio";
            this.trayTooltipRadio.Size = new System.Drawing.Size(57, 17);
            this.trayTooltipRadio.TabIndex = 0;
            this.trayTooltipRadio.TabStop = true;
            this.trayTooltipRadio.Text = "Tooltip";
            this.trayTooltipRadio.UseVisualStyleBackColor = true;
            // 
            // trayTooltipButton
            // 
            this.trayTooltipButton.Location = new System.Drawing.Point(69, 17);
            this.trayTooltipButton.Name = "trayTooltipButton";
            this.trayTooltipButton.Size = new System.Drawing.Size(75, 23);
            this.trayTooltipButton.TabIndex = 2;
            this.trayTooltipButton.Text = "Configure";
            this.trayTooltipButton.UseVisualStyleBackColor = true;
            this.trayTooltipButton.Click += new System.EventHandler(this.trayTooltipButton_Click);
            // 
            // updatesPage
            // 
            this.updatesPage.Controls.Add(this.updateSettingsControl);
            this.updatesPage.Controls.Add(this.lblUpdatesPage);
            this.updatesPage.Controls.Add(this.cbCheckTime);
            this.updatesPage.Controls.Add(this.cbCheckForUpdates);
            this.updatesPage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.updatesPage.Location = new System.Drawing.Point(5, 5);
            this.updatesPage.Name = "updatesPage";
            this.updatesPage.Size = new System.Drawing.Size(435, 426);
            this.updatesPage.TabIndex = 9;
            this.updatesPage.Text = "updatesPage";
            this.updatesPage.Visible = false;
            // 
            // lblUpdatesPage
            // 
            this.lblUpdatesPage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblUpdatesPage.ForeColor = System.Drawing.SystemColors.MenuHighlight;
            this.lblUpdatesPage.Location = new System.Drawing.Point(3, 2);
            this.lblUpdatesPage.Name = "lblUpdatesPage";
            this.lblUpdatesPage.Size = new System.Drawing.Size(429, 34);
            this.lblUpdatesPage.TabIndex = 9;
            this.lblUpdatesPage.Text = "The following settings help reducing the network load, especially for high-latenc" +
    "y connections and clients with many characters.";
            // 
            // cbCheckTime
            // 
            this.cbCheckTime.AutoSize = true;
            this.cbCheckTime.Location = new System.Drawing.Point(15, 39);
            this.cbCheckTime.Name = "cbCheckTime";
            this.cbCheckTime.Size = new System.Drawing.Size(146, 17);
            this.cbCheckTime.TabIndex = 0;
            this.cbCheckTime.Text = "Check system clock sync";
            this.cbCheckTime.UseVisualStyleBackColor = true;
            // 
            // cbCheckForUpdates
            // 
            this.cbCheckForUpdates.AutoSize = true;
            this.cbCheckForUpdates.Location = new System.Drawing.Point(15, 62);
            this.cbCheckForUpdates.Name = "cbCheckForUpdates";
            this.cbCheckForUpdates.Size = new System.Drawing.Size(158, 17);
            this.cbCheckForUpdates.TabIndex = 0;
            this.cbCheckForUpdates.Text = "Check for EVEMon updates";
            this.cbCheckForUpdates.UseVisualStyleBackColor = true;
            // 
            // schedulerUIPage
            // 
            this.schedulerUIPage.Controls.Add(this.lblSchedulerUIPage);
            this.schedulerUIPage.Controls.Add(this.panelColorText);
            this.schedulerUIPage.Controls.Add(this.lblText);
            this.schedulerUIPage.Controls.Add(this.panelColorRecurring2);
            this.schedulerUIPage.Controls.Add(this.lblBlockingEvents);
            this.schedulerUIPage.Controls.Add(this.panelColorRecurring1);
            this.schedulerUIPage.Controls.Add(this.lblRecurringEvents);
            this.schedulerUIPage.Controls.Add(this.panelColorSingle2);
            this.schedulerUIPage.Controls.Add(this.lblSimpleEvents);
            this.schedulerUIPage.Controls.Add(this.panelColorSingle1);
            this.schedulerUIPage.Controls.Add(this.panelColorBlocking);
            this.schedulerUIPage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.schedulerUIPage.Location = new System.Drawing.Point(5, 5);
            this.schedulerUIPage.Name = "schedulerUIPage";
            this.schedulerUIPage.Size = new System.Drawing.Size(435, 426);
            this.schedulerUIPage.TabIndex = 10;
            this.schedulerUIPage.Text = "schedulerUIPage";
            this.schedulerUIPage.Visible = false;
            // 
            // panelColorText
            // 
            this.panelColorText.BackColor = System.Drawing.Color.White;
            this.panelColorText.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelColorText.Cursor = System.Windows.Forms.Cursors.Hand;
            this.panelColorText.Location = new System.Drawing.Point(107, 105);
            this.panelColorText.Name = "panelColorText";
            this.panelColorText.Size = new System.Drawing.Size(43, 17);
            this.panelColorText.TabIndex = 4;
            this.panelColorText.Click += new System.EventHandler(this.colorPanel_Click);
            // 
            // panelColorRecurring2
            // 
            this.panelColorRecurring2.BackColor = System.Drawing.Color.LightGreen;
            this.panelColorRecurring2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelColorRecurring2.Cursor = System.Windows.Forms.Cursors.Hand;
            this.panelColorRecurring2.Location = new System.Drawing.Point(156, 176);
            this.panelColorRecurring2.Name = "panelColorRecurring2";
            this.panelColorRecurring2.Size = new System.Drawing.Size(43, 17);
            this.panelColorRecurring2.TabIndex = 4;
            this.panelColorRecurring2.Click += new System.EventHandler(this.colorPanel_Click);
            // 
            // panelColorRecurring1
            // 
            this.panelColorRecurring1.BackColor = System.Drawing.Color.Green;
            this.panelColorRecurring1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelColorRecurring1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.panelColorRecurring1.Location = new System.Drawing.Point(107, 176);
            this.panelColorRecurring1.Name = "panelColorRecurring1";
            this.panelColorRecurring1.Size = new System.Drawing.Size(43, 17);
            this.panelColorRecurring1.TabIndex = 4;
            this.panelColorRecurring1.Click += new System.EventHandler(this.colorPanel_Click);
            // 
            // panelColorSingle2
            // 
            this.panelColorSingle2.BackColor = System.Drawing.Color.LightBlue;
            this.panelColorSingle2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelColorSingle2.Cursor = System.Windows.Forms.Cursors.Hand;
            this.panelColorSingle2.Location = new System.Drawing.Point(156, 153);
            this.panelColorSingle2.Name = "panelColorSingle2";
            this.panelColorSingle2.Size = new System.Drawing.Size(43, 17);
            this.panelColorSingle2.TabIndex = 4;
            this.panelColorSingle2.Click += new System.EventHandler(this.colorPanel_Click);
            // 
            // panelColorSingle1
            // 
            this.panelColorSingle1.BackColor = System.Drawing.Color.Blue;
            this.panelColorSingle1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelColorSingle1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.panelColorSingle1.Location = new System.Drawing.Point(107, 153);
            this.panelColorSingle1.Name = "panelColorSingle1";
            this.panelColorSingle1.Size = new System.Drawing.Size(43, 17);
            this.panelColorSingle1.TabIndex = 4;
            this.panelColorSingle1.Click += new System.EventHandler(this.colorPanel_Click);
            // 
            // panelColorBlocking
            // 
            this.panelColorBlocking.BackColor = System.Drawing.Color.Red;
            this.panelColorBlocking.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelColorBlocking.Cursor = System.Windows.Forms.Cursors.Hand;
            this.panelColorBlocking.Location = new System.Drawing.Point(107, 128);
            this.panelColorBlocking.Name = "panelColorBlocking";
            this.panelColorBlocking.Size = new System.Drawing.Size(43, 17);
            this.panelColorBlocking.TabIndex = 3;
            this.panelColorBlocking.Click += new System.EventHandler(this.colorPanel_Click);
            // 
            // externalCalendarPage
            // 
            this.externalCalendarPage.Controls.Add(this.externalCalendarControl);
            this.externalCalendarPage.Controls.Add(this.lblExternalCalendarPage);
            this.externalCalendarPage.Controls.Add(this.externalCalendarCheckbox);
            this.externalCalendarPage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.externalCalendarPage.Location = new System.Drawing.Point(5, 5);
            this.externalCalendarPage.Name = "externalCalendarPage";
            this.externalCalendarPage.Size = new System.Drawing.Size(435, 426);
            this.externalCalendarPage.TabIndex = 11;
            this.externalCalendarPage.Text = "externalCalendarPage";
            this.externalCalendarPage.Visible = false;
            // 
            // externalCalendarCheckbox
            // 
            this.externalCalendarCheckbox.AutoSize = true;
            this.externalCalendarCheckbox.CausesValidation = false;
            this.externalCalendarCheckbox.Location = new System.Drawing.Point(6, 98);
            this.externalCalendarCheckbox.Name = "externalCalendarCheckbox";
            this.externalCalendarCheckbox.Size = new System.Drawing.Size(131, 17);
            this.externalCalendarCheckbox.TabIndex = 0;
            this.externalCalendarCheckbox.Text = "Use External Calendar";
            this.externalCalendarCheckbox.UseVisualStyleBackColor = true;
            this.externalCalendarCheckbox.Click += new System.EventHandler(this.OnMustEnableOrDisable);
            // 
            // g15Page
            // 
            this.g15Page.Controls.Add(this.g15CheckBox);
            this.g15Page.Controls.Add(this.lblG15Page);
            this.g15Page.Controls.Add(this.g15Panel);
            this.g15Page.Dock = System.Windows.Forms.DockStyle.Fill;
            this.g15Page.Location = new System.Drawing.Point(5, 5);
            this.g15Page.Name = "g15Page";
            this.g15Page.Size = new System.Drawing.Size(435, 426);
            this.g15Page.TabIndex = 13;
            this.g15Page.Text = "g15Page";
            this.g15Page.Visible = false;
            // 
            // g15CheckBox
            // 
            this.g15CheckBox.AutoSize = true;
            this.g15CheckBox.Location = new System.Drawing.Point(6, 102);
            this.g15CheckBox.Name = "g15CheckBox";
            this.g15CheckBox.Size = new System.Drawing.Size(136, 17);
            this.g15CheckBox.TabIndex = 0;
            this.g15CheckBox.Text = "Use G15/G510 Display";
            this.g15CheckBox.UseVisualStyleBackColor = true;
            this.g15CheckBox.CheckedChanged += new System.EventHandler(this.OnMustEnableOrDisable);
            // 
            // g15Panel
            // 
            this.g15Panel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.g15Panel.Controls.Add(this.cbG15ShowEVETime);
            this.g15Panel.Controls.Add(this.cbG15ShowTime);
            this.g15Panel.Controls.Add(this.panelCycleQueueInfo);
            this.g15Panel.Controls.Add(this.panelCycleCharInfo);
            this.g15Panel.Location = new System.Drawing.Point(6, 125);
            this.g15Panel.Margin = new System.Windows.Forms.Padding(0);
            this.g15Panel.Name = "g15Panel";
            this.g15Panel.Size = new System.Drawing.Size(399, 112);
            this.g15Panel.TabIndex = 7;
            // 
            // cbG15ShowEVETime
            // 
            this.cbG15ShowEVETime.AutoSize = true;
            this.cbG15ShowEVETime.Location = new System.Drawing.Point(10, 59);
            this.cbG15ShowEVETime.Name = "cbG15ShowEVETime";
            this.cbG15ShowEVETime.Size = new System.Drawing.Size(103, 17);
            this.cbG15ShowEVETime.TabIndex = 9;
            this.cbG15ShowEVETime.Text = "Show EVE Time";
            this.cbG15ShowEVETime.UseVisualStyleBackColor = true;
            // 
            // cbG15ShowTime
            // 
            this.cbG15ShowTime.AutoSize = true;
            this.cbG15ShowTime.Location = new System.Drawing.Point(114, 59);
            this.cbG15ShowTime.Name = "cbG15ShowTime";
            this.cbG15ShowTime.Size = new System.Drawing.Size(116, 17);
            this.cbG15ShowTime.TabIndex = 8;
            this.cbG15ShowTime.Text = "Show System Time";
            this.cbG15ShowTime.UseVisualStyleBackColor = true;
            // 
            // panelCycleQueueInfo
            // 
            this.panelCycleQueueInfo.AutoSize = true;
            this.panelCycleQueueInfo.Controls.Add(this.cbG15CycleTimes);
            this.panelCycleQueueInfo.Controls.Add(this.ACycleTimesInterval);
            this.panelCycleQueueInfo.Controls.Add(this.lblCycleTrainingSeconds);
            this.panelCycleQueueInfo.Location = new System.Drawing.Point(7, 29);
            this.panelCycleQueueInfo.Name = "panelCycleQueueInfo";
            this.panelCycleQueueInfo.Size = new System.Drawing.Size(315, 28);
            this.panelCycleQueueInfo.TabIndex = 7;
            // 
            // cbG15CycleTimes
            // 
            this.cbG15CycleTimes.AutoSize = true;
            this.cbG15CycleTimes.Location = new System.Drawing.Point(3, 5);
            this.cbG15CycleTimes.Margin = new System.Windows.Forms.Padding(3, 5, 0, 3);
            this.cbG15CycleTimes.Name = "cbG15CycleTimes";
            this.cbG15CycleTimes.Size = new System.Drawing.Size(212, 17);
            this.cbG15CycleTimes.TabIndex = 7;
            this.cbG15CycleTimes.Text = "Cycle Characters Skill Queue info every";
            this.cbG15CycleTimes.UseVisualStyleBackColor = true;
            this.cbG15CycleTimes.CheckedChanged += new System.EventHandler(this.OnMustEnableOrDisable);
            // 
            // ACycleTimesInterval
            // 
            this.ACycleTimesInterval.AutoSize = true;
            this.ACycleTimesInterval.Location = new System.Drawing.Point(221, 4);
            this.ACycleTimesInterval.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.ACycleTimesInterval.Maximum = new decimal(new int[] {
            120,
            0,
            0,
            0});
            this.ACycleTimesInterval.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.ACycleTimesInterval.Name = "ACycleTimesInterval";
            this.ACycleTimesInterval.Size = new System.Drawing.Size(45, 20);
            this.ACycleTimesInterval.TabIndex = 8;
            this.ACycleTimesInterval.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // panelCycleCharInfo
            // 
            this.panelCycleCharInfo.AutoSize = true;
            this.panelCycleCharInfo.Controls.Add(this.cbG15ACycle);
            this.panelCycleCharInfo.Controls.Add(this.ACycleInterval);
            this.panelCycleCharInfo.Controls.Add(this.lblG15CycleCharSeconds);
            this.panelCycleCharInfo.Location = new System.Drawing.Point(7, 3);
            this.panelCycleCharInfo.Name = "panelCycleCharInfo";
            this.panelCycleCharInfo.Size = new System.Drawing.Size(236, 28);
            this.panelCycleCharInfo.TabIndex = 6;
            // 
            // cbG15ACycle
            // 
            this.cbG15ACycle.AutoSize = true;
            this.cbG15ACycle.Location = new System.Drawing.Point(3, 5);
            this.cbG15ACycle.Margin = new System.Windows.Forms.Padding(3, 5, 0, 3);
            this.cbG15ACycle.Name = "cbG15ACycle";
            this.cbG15ACycle.Size = new System.Drawing.Size(135, 17);
            this.cbG15ACycle.TabIndex = 4;
            this.cbG15ACycle.Text = "Cycle Characters every";
            this.cbG15ACycle.UseVisualStyleBackColor = true;
            this.cbG15ACycle.CheckedChanged += new System.EventHandler(this.OnMustEnableOrDisable);
            // 
            // ACycleInterval
            // 
            this.ACycleInterval.AutoSize = true;
            this.ACycleInterval.Location = new System.Drawing.Point(142, 4);
            this.ACycleInterval.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.ACycleInterval.Maximum = new decimal(new int[] {
            120,
            0,
            0,
            0});
            this.ACycleInterval.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.ACycleInterval.Name = "ACycleInterval";
            this.ACycleInterval.Size = new System.Drawing.Size(45, 20);
            this.ACycleInterval.TabIndex = 5;
            this.ACycleInterval.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.ACycleInterval.ValueChanged += new System.EventHandler(this.ACycleInterval_ValueChanged);
            // 
            // iconsPage
            // 
            this.iconsPage.Controls.Add(this.lblIconsPage);
            this.iconsPage.Controls.Add(this.gbSkillBrowserIconSet);
            this.iconsPage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.iconsPage.Location = new System.Drawing.Point(5, 5);
            this.iconsPage.Name = "iconsPage";
            this.iconsPage.Size = new System.Drawing.Size(435, 426);
            this.iconsPage.TabIndex = 16;
            this.iconsPage.Text = "iconsPage";
            // 
            // messagesPage
            // 
            this.messagesPage.Controls.Add(this.gbMessageBox);
            this.messagesPage.Controls.Add(this.lblObsoletePlanEntries);
            this.messagesPage.Controls.Add(this.ObsoleteEntryRemovalGroupBox);
            this.messagesPage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.messagesPage.Location = new System.Drawing.Point(5, 5);
            this.messagesPage.Name = "messagesPage";
            this.messagesPage.Size = new System.Drawing.Size(435, 426);
            this.messagesPage.TabIndex = 17;
            this.messagesPage.Text = "messagesPage";
            // 
            // gbMessageBox
            // 
            this.gbMessageBox.Controls.Add(this.lblPrioritesConflict);
            this.gbMessageBox.Controls.Add(this.btnPrioritiesReset);
            this.gbMessageBox.Location = new System.Drawing.Point(5, 314);
            this.gbMessageBox.Name = "gbMessageBox";
            this.gbMessageBox.Size = new System.Drawing.Size(424, 58);
            this.gbMessageBox.TabIndex = 25;
            this.gbMessageBox.TabStop = false;
            this.gbMessageBox.Text = "Pop-up Messages";
            // 
            // lblPrioritesConflict
            // 
            this.lblPrioritesConflict.AutoSize = true;
            this.lblPrioritesConflict.Location = new System.Drawing.Point(20, 25);
            this.lblPrioritesConflict.Name = "lblPrioritesConflict";
            this.lblPrioritesConflict.Size = new System.Drawing.Size(84, 13);
            this.lblPrioritesConflict.TabIndex = 1;
            this.lblPrioritesConflict.Text = "Priorities Conflict";
            // 
            // btnPrioritiesReset
            // 
            this.btnPrioritiesReset.Location = new System.Drawing.Point(332, 20);
            this.btnPrioritiesReset.Name = "btnPrioritiesReset";
            this.btnPrioritiesReset.Size = new System.Drawing.Size(75, 23);
            this.btnPrioritiesReset.TabIndex = 0;
            this.btnPrioritiesReset.Text = "Reset";
            this.btnPrioritiesReset.UseVisualStyleBackColor = true;
            this.btnPrioritiesReset.Click += new System.EventHandler(this.btnPrioritiesReset_Click);
            // 
            // ObsoleteEntryRemovalGroupBox
            // 
            this.ObsoleteEntryRemovalGroupBox.Controls.Add(this.RemoveAllLabel);
            this.ObsoleteEntryRemovalGroupBox.Controls.Add(this.AlwaysAskLabel);
            this.ObsoleteEntryRemovalGroupBox.Controls.Add(this.RemoveConfirmedLabel);
            this.ObsoleteEntryRemovalGroupBox.Controls.Add(this.alwaysAskRadioButton);
            this.ObsoleteEntryRemovalGroupBox.Controls.Add(this.removeAllRadioButton);
            this.ObsoleteEntryRemovalGroupBox.Controls.Add(this.removeConfirmedRadioButton);
            this.ObsoleteEntryRemovalGroupBox.Location = new System.Drawing.Point(5, 120);
            this.ObsoleteEntryRemovalGroupBox.Name = "ObsoleteEntryRemovalGroupBox";
            this.ObsoleteEntryRemovalGroupBox.Size = new System.Drawing.Size(425, 187);
            this.ObsoleteEntryRemovalGroupBox.TabIndex = 23;
            this.ObsoleteEntryRemovalGroupBox.TabStop = false;
            this.ObsoleteEntryRemovalGroupBox.Text = "Obsolete Plan Entry Removal";
            // 
            // RemoveAllLabel
            // 
            this.RemoveAllLabel.Location = new System.Drawing.Point(24, 144);
            this.RemoveAllLabel.Name = "RemoveAllLabel";
            this.RemoveAllLabel.Size = new System.Drawing.Size(394, 27);
            this.RemoveAllLabel.TabIndex = 5;
            this.RemoveAllLabel.Text = "If EVEMon believes a skill level has been completed, whether it has been confirme" +
    "d by the API or not it will be removed when the plan is opened.";
            // 
            // AlwaysAskLabel
            // 
            this.AlwaysAskLabel.Location = new System.Drawing.Point(24, 91);
            this.AlwaysAskLabel.Name = "AlwaysAskLabel";
            this.AlwaysAskLabel.Size = new System.Drawing.Size(394, 27);
            this.AlwaysAskLabel.TabIndex = 4;
            this.AlwaysAskLabel.Text = "Always display the \"Obsolete Entries\" link at the bottom of the skill planner bef" +
    "ore removing entries.";
            // 
            // RemoveConfirmedLabel
            // 
            this.RemoveConfirmedLabel.Location = new System.Drawing.Point(24, 39);
            this.RemoveConfirmedLabel.Name = "RemoveConfirmedLabel";
            this.RemoveConfirmedLabel.Size = new System.Drawing.Size(394, 27);
            this.RemoveConfirmedLabel.TabIndex = 3;
            this.RemoveConfirmedLabel.Text = "Once the API has confirmed a skill level has completed it is removed the next tim" +
    "e a plan is opened. This is the default behaviour.";
            // 
            // alwaysAskRadioButton
            // 
            this.alwaysAskRadioButton.AutoSize = true;
            this.alwaysAskRadioButton.Location = new System.Drawing.Point(7, 73);
            this.alwaysAskRadioButton.Name = "alwaysAskRadioButton";
            this.alwaysAskRadioButton.Size = new System.Drawing.Size(78, 17);
            this.alwaysAskRadioButton.TabIndex = 2;
            this.alwaysAskRadioButton.TabStop = true;
            this.alwaysAskRadioButton.Text = "Always ask";
            this.alwaysAskRadioButton.UseVisualStyleBackColor = true;
            // 
            // removeAllRadioButton
            // 
            this.removeAllRadioButton.AutoSize = true;
            this.removeAllRadioButton.Location = new System.Drawing.Point(7, 126);
            this.removeAllRadioButton.Name = "removeAllRadioButton";
            this.removeAllRadioButton.Size = new System.Drawing.Size(206, 17);
            this.removeAllRadioButton.TabIndex = 1;
            this.removeAllRadioButton.TabStop = true;
            this.removeAllRadioButton.Text = "Remove entry once training completes";
            this.removeAllRadioButton.UseVisualStyleBackColor = true;
            // 
            // removeConfirmedRadioButton
            // 
            this.removeConfirmedRadioButton.AutoSize = true;
            this.removeConfirmedRadioButton.Location = new System.Drawing.Point(7, 21);
            this.removeConfirmedRadioButton.Name = "removeConfirmedRadioButton";
            this.removeConfirmedRadioButton.Size = new System.Drawing.Size(221, 17);
            this.removeConfirmedRadioButton.TabIndex = 0;
            this.removeConfirmedRadioButton.TabStop = true;
            this.removeConfirmedRadioButton.Text = "Remove confirmed entry (Recommended)";
            this.removeConfirmedRadioButton.UseVisualStyleBackColor = true;
            // 
            // portableEveClientsPage
            // 
            this.portableEveClientsPage.Controls.Add(this.lblPECIDescription);
            this.portableEveClientsPage.Controls.Add(this.PECIGroupBox);
            this.portableEveClientsPage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.portableEveClientsPage.Location = new System.Drawing.Point(5, 5);
            this.portableEveClientsPage.Name = "portableEveClientsPage";
            this.portableEveClientsPage.Size = new System.Drawing.Size(435, 426);
            this.portableEveClientsPage.TabIndex = 20;
            this.portableEveClientsPage.Text = "portableEveClientsPage";
            // 
            // lblPECIDescription
            // 
            this.lblPECIDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblPECIDescription.ForeColor = System.Drawing.SystemColors.MenuHighlight;
            this.lblPECIDescription.Location = new System.Drawing.Point(4, 15);
            this.lblPECIDescription.Name = "lblPECIDescription";
            this.lblPECIDescription.Size = new System.Drawing.Size(424, 95);
            this.lblPECIDescription.TabIndex = 21;
            this.lblPECIDescription.Text = resources.GetString("lblPECIDescription.Text");
            // 
            // PECIGroupBox
            // 
            this.PECIGroupBox.Controls.Add(this.portableEveClientsControl);
            this.PECIGroupBox.Location = new System.Drawing.Point(7, 129);
            this.PECIGroupBox.Name = "PECIGroupBox";
            this.PECIGroupBox.Size = new System.Drawing.Size(421, 283);
            this.PECIGroupBox.TabIndex = 0;
            this.PECIGroupBox.TabStop = false;
            this.PECIGroupBox.Text = "Portable EVE Client Installations";
            // 
            // marketPriceProvidersPage
            // 
            this.marketPriceProvidersPage.Controls.Add(this.gbMarketPriceProviders);
            this.marketPriceProvidersPage.Controls.Add(this.marketPriceProviderPageLabel);
            this.marketPriceProvidersPage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.marketPriceProvidersPage.Location = new System.Drawing.Point(5, 5);
            this.marketPriceProvidersPage.Name = "marketPriceProvidersPage";
            this.marketPriceProvidersPage.Size = new System.Drawing.Size(435, 426);
            this.marketPriceProvidersPage.TabIndex = 21;
            this.marketPriceProvidersPage.Text = "marketPriceProvidersPage";
            // 
            // gbMarketPriceProviders
            // 
            this.gbMarketPriceProviders.Controls.Add(this.cbProvidersList);
            this.gbMarketPriceProviders.Controls.Add(this.SelectedProviderLabel);
            this.gbMarketPriceProviders.Location = new System.Drawing.Point(3, 68);
            this.gbMarketPriceProviders.Name = "gbMarketPriceProviders";
            this.gbMarketPriceProviders.Size = new System.Drawing.Size(214, 67);
            this.gbMarketPriceProviders.TabIndex = 22;
            this.gbMarketPriceProviders.TabStop = false;
            this.gbMarketPriceProviders.Text = "Market Price Provider";
            // 
            // cbProvidersList
            // 
            this.cbProvidersList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbProvidersList.FormattingEnabled = true;
            this.cbProvidersList.Location = new System.Drawing.Point(61, 28);
            this.cbProvidersList.Name = "cbProvidersList";
            this.cbProvidersList.Size = new System.Drawing.Size(147, 21);
            this.cbProvidersList.TabIndex = 1;
            // 
            // SelectedProviderLabel
            // 
            this.SelectedProviderLabel.AutoSize = true;
            this.SelectedProviderLabel.Location = new System.Drawing.Point(6, 31);
            this.SelectedProviderLabel.Name = "SelectedProviderLabel";
            this.SelectedProviderLabel.Size = new System.Drawing.Size(49, 13);
            this.SelectedProviderLabel.TabIndex = 0;
            this.SelectedProviderLabel.Text = "Provider:";
            // 
            // marketPriceProviderPageLabel
            // 
            this.marketPriceProviderPageLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.marketPriceProviderPageLabel.AutoSize = true;
            this.marketPriceProviderPageLabel.ForeColor = System.Drawing.SystemColors.MenuHighlight;
            this.marketPriceProviderPageLabel.Location = new System.Drawing.Point(3, 38);
            this.marketPriceProviderPageLabel.Name = "marketPriceProviderPageLabel";
            this.marketPriceProviderPageLabel.Size = new System.Drawing.Size(294, 13);
            this.marketPriceProviderPageLabel.TabIndex = 21;
            this.marketPriceProviderPageLabel.Text = "Request prices for all EVE items from a market price provider.";
            // 
            // cloudStorageServicePage
            // 
            this.cloudStorageServicePage.Controls.Add(this.providerAuthenticationGroupBox);
            this.cloudStorageServicePage.Controls.Add(this.cloudStorageGroupBox);
            this.cloudStorageServicePage.Controls.Add(this.linkLabel1);
            this.cloudStorageServicePage.Controls.Add(this.settingsFileStorageGroupBox);
            this.cloudStorageServicePage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cloudStorageServicePage.Location = new System.Drawing.Point(5, 5);
            this.cloudStorageServicePage.Name = "cloudStorageServicePage";
            this.cloudStorageServicePage.Size = new System.Drawing.Size(435, 426);
            this.cloudStorageServicePage.TabIndex = 22;
            this.cloudStorageServicePage.Text = "cloudStorageServicePage";
            // 
            // providerAuthenticationGroupBox
            // 
            this.providerAuthenticationGroupBox.Controls.Add(this.cloudStorageServiceControl);
            this.providerAuthenticationGroupBox.Location = new System.Drawing.Point(6, 288);
            this.providerAuthenticationGroupBox.Name = "providerAuthenticationGroupBox";
            this.providerAuthenticationGroupBox.Size = new System.Drawing.Size(421, 128);
            this.providerAuthenticationGroupBox.TabIndex = 24;
            this.providerAuthenticationGroupBox.TabStop = false;
            this.providerAuthenticationGroupBox.Text = "Cloud Storage Provider Authentication";
            // 
            // cloudStorageGroupBox
            // 
            this.cloudStorageGroupBox.Controls.Add(this.cloudStorageProviderLogoPictureBox);
            this.cloudStorageGroupBox.Controls.Add(this.cloudStorageProvidersComboBox);
            this.cloudStorageGroupBox.Controls.Add(this.lblSelectedProvider);
            this.cloudStorageGroupBox.Location = new System.Drawing.Point(6, 81);
            this.cloudStorageGroupBox.Name = "cloudStorageGroupBox";
            this.cloudStorageGroupBox.Size = new System.Drawing.Size(232, 67);
            this.cloudStorageGroupBox.TabIndex = 23;
            this.cloudStorageGroupBox.TabStop = false;
            this.cloudStorageGroupBox.Text = "Cloud Storage Provider";
            // 
            // cloudStorageProviderLogoPictureBox
            // 
            this.cloudStorageProviderLogoPictureBox.Location = new System.Drawing.Point(176, 11);
            this.cloudStorageProviderLogoPictureBox.Name = "cloudStorageProviderLogoPictureBox";
            this.cloudStorageProviderLogoPictureBox.Size = new System.Drawing.Size(50, 50);
            this.cloudStorageProviderLogoPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.cloudStorageProviderLogoPictureBox.TabIndex = 2;
            this.cloudStorageProviderLogoPictureBox.TabStop = false;
            // 
            // cloudStorageProvidersComboBox
            // 
            this.cloudStorageProvidersComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cloudStorageProvidersComboBox.FormattingEnabled = true;
            this.cloudStorageProvidersComboBox.Location = new System.Drawing.Point(61, 28);
            this.cloudStorageProvidersComboBox.Name = "cloudStorageProvidersComboBox";
            this.cloudStorageProvidersComboBox.Size = new System.Drawing.Size(107, 21);
            this.cloudStorageProvidersComboBox.TabIndex = 1;
            this.cloudStorageProvidersComboBox.SelectedIndexChanged += new System.EventHandler(this.cloudStorageProvidersComboBox_SelectedIndexChanged);
            // 
            // lblSelectedProvider
            // 
            this.lblSelectedProvider.AutoSize = true;
            this.lblSelectedProvider.Location = new System.Drawing.Point(6, 31);
            this.lblSelectedProvider.Name = "lblSelectedProvider";
            this.lblSelectedProvider.Size = new System.Drawing.Size(49, 13);
            this.lblSelectedProvider.TabIndex = 0;
            this.lblSelectedProvider.Text = "Provider:";
            // 
            // linkLabel1
            // 
            this.linkLabel1.ForeColor = System.Drawing.SystemColors.MenuHighlight;
            this.linkLabel1.LinkArea = new System.Windows.Forms.LinkArea(0, 0);
            this.linkLabel1.Location = new System.Drawing.Point(8, 25);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(419, 41);
            this.linkLabel1.TabIndex = 5;
            this.linkLabel1.Text = resources.GetString("linkLabel1.Text");
            // 
            // settingsFileStorageGroupBox
            // 
            this.settingsFileStorageGroupBox.Controls.Add(this.settingsFileStorageControl);
            this.settingsFileStorageGroupBox.Location = new System.Drawing.Point(6, 154);
            this.settingsFileStorageGroupBox.Name = "settingsFileStorageGroupBox";
            this.settingsFileStorageGroupBox.Size = new System.Drawing.Size(422, 127);
            this.settingsFileStorageGroupBox.TabIndex = 4;
            this.settingsFileStorageGroupBox.TabStop = false;
            this.settingsFileStorageGroupBox.Text = "Settings File Storage";
            // 
            // emailNotificationsControl
            // 
            this.emailNotificationsControl.Location = new System.Drawing.Point(7, 74);
            this.emailNotificationsControl.Name = "emailNotificationsControl";
            this.emailNotificationsControl.Settings = null;
            this.emailNotificationsControl.Size = new System.Drawing.Size(355, 337);
            this.emailNotificationsControl.TabIndex = 20;
            // 
            // notificationsControl
            // 
            this.notificationsControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.notificationsControl.AutoScroll = true;
            this.notificationsControl.BackColor = System.Drawing.SystemColors.Window;
            this.notificationsControl.Location = new System.Drawing.Point(3, 56);
            this.notificationsControl.Name = "notificationsControl";
            this.notificationsControl.Settings = null;
            this.notificationsControl.Size = new System.Drawing.Size(429, 337);
            this.notificationsControl.TabIndex = 4;
            // 
            // updateSettingsControl
            // 
            this.updateSettingsControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.updateSettingsControl.AutoScroll = true;
            this.updateSettingsControl.Location = new System.Drawing.Point(11, 85);
            this.updateSettingsControl.Name = "updateSettingsControl";
            this.updateSettingsControl.Settings = null;
            this.updateSettingsControl.Size = new System.Drawing.Size(413, 340);
            this.updateSettingsControl.TabIndex = 10;
            // 
            // externalCalendarControl
            // 
            this.externalCalendarControl.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.externalCalendarControl.Enabled = false;
            this.externalCalendarControl.Location = new System.Drawing.Point(0, 123);
            this.externalCalendarControl.Name = "externalCalendarControl";
            this.externalCalendarControl.Size = new System.Drawing.Size(435, 303);
            this.externalCalendarControl.TabIndex = 12;
            // 
            // portableEveClientsControl
            // 
            this.portableEveClientsControl.AutoScroll = true;
            this.portableEveClientsControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.portableEveClientsControl.Location = new System.Drawing.Point(3, 16);
            this.portableEveClientsControl.Name = "portableEveClientsControl";
            this.portableEveClientsControl.Size = new System.Drawing.Size(415, 264);
            this.portableEveClientsControl.TabIndex = 0;
            // 
            // cloudStorageServiceControl
            // 
            this.cloudStorageServiceControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cloudStorageServiceControl.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.cloudStorageServiceControl.Location = new System.Drawing.Point(3, 16);
            this.cloudStorageServiceControl.Name = "cloudStorageServiceControl";
            this.cloudStorageServiceControl.Size = new System.Drawing.Size(415, 109);
            this.cloudStorageServiceControl.TabIndex = 0;
            // 
            // settingsFileStorageControl
            // 
            this.settingsFileStorageControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.settingsFileStorageControl.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.settingsFileStorageControl.Location = new System.Drawing.Point(3, 16);
            this.settingsFileStorageControl.Name = "settingsFileStorageControl";
            this.settingsFileStorageControl.Size = new System.Drawing.Size(416, 108);
            this.settingsFileStorageControl.TabIndex = 0;
            // 
            // insertAllScopesButton
            // 
            this.insertAllScopesButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.insertAllScopesButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.insertAllScopesButton.Location = new System.Drawing.Point(300, 109);
            this.insertAllScopesButton.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.insertAllScopesButton.Name = "insertAllScopesButton";
            this.insertAllScopesButton.Size = new System.Drawing.Size(97, 23);
            this.insertAllScopesButton.TabIndex = 6;
            this.insertAllScopesButton.Text = "Insert all scopes";
            this.insertAllScopesButton.UseVisualStyleBackColor = true;
            this.insertAllScopesButton.Click += new System.EventHandler(this.insertAllScopesButton_Click);
            // 
            // SettingsForm
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(644, 482);
            this.Controls.Add(this.multiPanel);
            this.Controls.Add(this.leftPanel);
            this.Controls.Add(this.bottomPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(400, 400);
            this.Name = "SettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "EVEMon Settings";
            this.Load += new System.EventHandler(this.SettingsForm_Load);
            this.systemTrayIconGroupBox.ResumeLayout(false);
            this.systemTrayIconGroupBox.PerformLayout();
            this.bottomPanel.ResumeLayout(false);
            this.CharacterMonitorGroupBox.ResumeLayout(false);
            this.CharacterMonitorGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudSkillQueueWarningThresholdDays)).EndInit();
            this.WindowTitleGroupBox.ResumeLayout(false);
            this.WindowTitleGroupBox.PerformLayout();
            this.gbSkillBrowserIconSet.ResumeLayout(false);
            this.gbSkillBrowserIconSet.PerformLayout();
            this.iconsSetTableLayoutPanel.ResumeLayout(false);
            this.leftPanel.ResumeLayout(false);
            this.multiPanel.ResumeLayout(false);
            this.generalPage.ResumeLayout(false);
            this.generalPage.PerformLayout();
            this.mainWindowPage.ResumeLayout(false);
            this.OverviewGroupBox.ResumeLayout(false);
            this.OverviewGroupBox.PerformLayout();
            this.overviewPanel.ResumeLayout(false);
            this.overviewPanel.PerformLayout();
            this.skillPlannerPage.ResumeLayout(false);
            this.skillPlannerPage.PerformLayout();
            this.networkPage.ResumeLayout(false);
            this.esiSettingsGroupBox.ResumeLayout(false);
            this.esiSettingsGroupBox.PerformLayout();
            this.ProxyServerGroupBox.ResumeLayout(false);
            this.ProxyServerGroupBox.PerformLayout();
            this.customProxyPanel.ResumeLayout(false);
            this.customProxyPanel.PerformLayout();
            this.emailNotificationsPage.ResumeLayout(false);
            this.emailNotificationsPage.PerformLayout();
            this.notificationsPage.ResumeLayout(false);
            this.notificationsPage.PerformLayout();
            this.trayIconPage.ResumeLayout(false);
            this.mainWindowBehaviourGroupBox.ResumeLayout(false);
            this.mainWindowBehaviourGroupBox.PerformLayout();
            this.trayIconPopupGroupBox.ResumeLayout(false);
            this.trayIconPopupGroupBox.PerformLayout();
            this.updatesPage.ResumeLayout(false);
            this.updatesPage.PerformLayout();
            this.schedulerUIPage.ResumeLayout(false);
            this.schedulerUIPage.PerformLayout();
            this.externalCalendarPage.ResumeLayout(false);
            this.externalCalendarPage.PerformLayout();
            this.g15Page.ResumeLayout(false);
            this.g15Page.PerformLayout();
            this.g15Panel.ResumeLayout(false);
            this.g15Panel.PerformLayout();
            this.panelCycleQueueInfo.ResumeLayout(false);
            this.panelCycleQueueInfo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ACycleTimesInterval)).EndInit();
            this.panelCycleCharInfo.ResumeLayout(false);
            this.panelCycleCharInfo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ACycleInterval)).EndInit();
            this.iconsPage.ResumeLayout(false);
            this.messagesPage.ResumeLayout(false);
            this.gbMessageBox.ResumeLayout(false);
            this.gbMessageBox.PerformLayout();
            this.ObsoleteEntryRemovalGroupBox.ResumeLayout(false);
            this.ObsoleteEntryRemovalGroupBox.PerformLayout();
            this.portableEveClientsPage.ResumeLayout(false);
            this.PECIGroupBox.ResumeLayout(false);
            this.marketPriceProvidersPage.ResumeLayout(false);
            this.marketPriceProvidersPage.PerformLayout();
            this.gbMarketPriceProviders.ResumeLayout(false);
            this.gbMarketPriceProviders.PerformLayout();
            this.cloudStorageServicePage.ResumeLayout(false);
            this.providerAuthenticationGroupBox.ResumeLayout(false);
            this.cloudStorageGroupBox.ResumeLayout(false);
            this.cloudStorageGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cloudStorageProviderLogoPictureBox)).EndInit();
            this.settingsFileStorageGroupBox.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion


        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.CheckBox cbPlaySoundOnSkillComplete;
        private System.Windows.Forms.GroupBox ProxyServerGroupBox;
        private System.Windows.Forms.TextBox proxyPortTextBox;
        private System.Windows.Forms.TextBox proxyHttpHostTextBox;
        private System.Windows.Forms.Button proxyAuthenticationButton;
        private System.Windows.Forms.CheckBox cbCheckForUpdates;
        private System.Windows.Forms.CheckBox cbWorksafeMode;
        private System.Windows.Forms.CheckBox cbHighlightPlannedSkills;
        private System.Windows.Forms.CheckBox cbHighlightPrerequisites;
        private System.Windows.Forms.CheckBox runAtStartupComboBox;
        private System.Windows.Forms.CheckBox cbTitleToTime;
        private System.Windows.Forms.ComboBox cbWindowsTitleList;
        private System.Windows.Forms.CheckBox g15CheckBox;
        private System.Windows.Forms.RadioButton rbSystemTrayOptionsNever;
        private System.Windows.Forms.RadioButton rbSystemTrayOptionsMinimized;
        private System.Windows.Forms.RadioButton rbSystemTrayOptionsAlways;
        private System.Windows.Forms.ToolTip ttToolTipCodes;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.CheckBox cbSkillInTitle;
        private System.Windows.Forms.CheckBox cbHighlightConflicts;
        private System.Windows.Forms.CheckBox cbHighlightPartialSkills;
        private System.Windows.Forms.ColorDialog colorDialog;
        private System.Windows.Forms.Panel panelColorBlocking;
        private System.Windows.Forms.Panel panelColorRecurring2;
        private System.Windows.Forms.Panel panelColorRecurring1;
        private System.Windows.Forms.Panel panelColorSingle2;
        private System.Windows.Forms.Panel panelColorSingle1;
        private System.Windows.Forms.Panel panelColorText;
        private System.Windows.Forms.Button trayTooltipButton;
        private System.Windows.Forms.CheckBox externalCalendarCheckbox;
        private System.Windows.Forms.CheckBox cbShowAllPublicSkills;
        private System.Windows.Forms.CheckBox cbShowNonPublicSkills;
        private System.Windows.Forms.CheckBox cbCheckTime;
        private System.Windows.Forms.CheckBox cbShowOverViewTab;
        private System.Windows.Forms.ComboBox compatibilityCombo;
        private EVEMon.SettingsUI.NotificationsControl notificationsControl;
        private System.Windows.Forms.TreeView treeView;
        private MultiPanel multiPanel;
        private MultiPanelPage generalPage;
        private MultiPanelPage mainWindowPage;
        private MultiPanelPage skillPlannerPage;
        private MultiPanelPage networkPage;
        private MultiPanelPage emailNotificationsPage;
        private MultiPanelPage notificationsPage;
        private MultiPanelPage trayIconPage;
        private MultiPanelPage updatesPage;
        private MultiPanelPage schedulerUIPage;
        private MultiPanelPage externalCalendarPage;
        private System.Windows.Forms.Panel customProxyPanel;
        private System.Windows.Forms.Panel leftPanel;
        private MultiPanelPage g15Page;
        private System.Windows.Forms.CheckBox customProxyCheckBox;
        private System.Windows.Forms.GroupBox trayIconPopupGroupBox;
        private System.Windows.Forms.Button trayPopupButton;
        private System.Windows.Forms.RadioButton trayPopupRadio;
        private System.Windows.Forms.RadioButton trayTooltipRadio;
        private System.Windows.Forms.Label lblUpdatesPage;
        private System.Windows.Forms.Button applyButton;
        private System.Windows.Forms.GroupBox OverviewGroupBox;
        private System.Windows.Forms.ComboBox overviewPortraitSizeComboBox;
        private System.Windows.Forms.CheckBox overviewShowPortraitCheckBox;
        private System.Windows.Forms.CheckBox overviewShowWalletCheckBox;
        private System.Windows.Forms.Panel overviewPanel;
        private UpdateSettingsControl updateSettingsControl;
        private System.Windows.Forms.GroupBox mainWindowBehaviourGroupBox;
        private System.Windows.Forms.RadioButton rbMinToTaskBar;
        private System.Windows.Forms.RadioButton rbMinToTray;
        private System.Windows.Forms.RadioButton rbExitEVEMon;
        private System.Windows.Forms.CheckBox cbAlwaysShowSkillQueueTime;
        private System.Windows.Forms.CheckBox cbColorPartialSkills;
        private System.Windows.Forms.CheckBox cbShowPrereqMetSkills;
        private System.Windows.Forms.CheckBox cbColorQueuedSkills;
        private System.Windows.Forms.CheckBox cbHighlightQueuedSiklls;
        private System.Windows.Forms.CheckBox overviewShowSkillQueueTrainingTimeCheckBox;
        private System.Windows.Forms.Panel panelCycleCharInfo;
        private System.Windows.Forms.CheckBox cbG15ACycle;
        private System.Windows.Forms.NumericUpDown ACycleInterval;
        private System.Windows.Forms.Panel g15Panel;
        private System.Windows.Forms.CheckBox cbG15ShowTime;
        private System.Windows.Forms.Panel panelCycleQueueInfo;
        private System.Windows.Forms.CheckBox cbG15CycleTimes;
        private System.Windows.Forms.NumericUpDown ACycleTimesInterval;
        private System.Windows.Forms.CheckBox cbSummaryOnMultiSelectOnly;
        private System.Windows.Forms.ImageList imageList;
        private System.Windows.Forms.RadioButton trayPopupDisabledRadio;
        private System.Windows.Forms.CheckBox overviewGroupCharactersInTrainingCheckBox;
        private System.Windows.Forms.CheckBox cbAdvanceEntryAdd;
        private System.Windows.Forms.CheckBox cbG15ShowEVETime;
        private System.Windows.Forms.CheckBox cbUseIncreasedContrastOnOverview;
        private MultiPanelPage iconsPage;
        private System.Windows.Forms.TableLayoutPanel iconsSetTableLayoutPanel;
        private System.Windows.Forms.ComboBox cbSkillIconSet;
        private System.Windows.Forms.TreeView tvlist;
        private MultiPanelPage messagesPage;
        private System.Windows.Forms.GroupBox ObsoleteEntryRemovalGroupBox;
        private System.Windows.Forms.Label RemoveAllLabel;
        private System.Windows.Forms.Label AlwaysAskLabel;
        private System.Windows.Forms.Label RemoveConfirmedLabel;
        private System.Windows.Forms.RadioButton alwaysAskRadioButton;
        private System.Windows.Forms.RadioButton removeAllRadioButton;
        private System.Windows.Forms.RadioButton removeConfirmedRadioButton;
        private System.Windows.Forms.GroupBox gbMessageBox;
        private System.Windows.Forms.Label lblPrioritesConflict;
        private System.Windows.Forms.Button btnPrioritiesReset;
        private System.Windows.Forms.Panel bottomPanel;
        private System.Windows.Forms.Label lblMainWindowPage;
        private System.Windows.Forms.Label lblSize;
        private System.Windows.Forms.GroupBox CharacterMonitorGroupBox;
        private System.Windows.Forms.GroupBox WindowTitleGroupBox;
        private System.Windows.Forms.Label lblGeneralPage;
        private System.Windows.Forms.Label lblEnvironment;
        private System.Windows.Forms.Label lblSkillPlannerPage;
        private System.Windows.Forms.Label lblNetworkPageProxy;
        private System.Windows.Forms.Label lblProxyHostIPAddress;
        private System.Windows.Forms.Label lblProxyPort;
        private System.Windows.Forms.Label lblHTTP;
        private System.Windows.Forms.Label lblEmailNotificationPage;
        private System.Windows.Forms.Label lblNotificationsPage;
        private System.Windows.Forms.Label lblTrayIconPage;
        private System.Windows.Forms.Label lblSchedulerUIPage;
        private System.Windows.Forms.Label lblText;
        private System.Windows.Forms.Label lblBlockingEvents;
        private System.Windows.Forms.Label lblRecurringEvents;
        private System.Windows.Forms.Label lblSimpleEvents;
        private System.Windows.Forms.Label lblExternalCalendarPage;
        private System.Windows.Forms.Label lblG15Page;
        private System.Windows.Forms.Label lblCycleTrainingSeconds;
        private System.Windows.Forms.Label lblG15CycleCharSeconds;
        private System.Windows.Forms.Label lblIconsPage;
        private System.Windows.Forms.GroupBox gbSkillBrowserIconSet;
        private System.Windows.Forms.Label lblObsoletePlanEntries;
        private System.Windows.Forms.GroupBox systemTrayIconGroupBox;
        private System.Windows.Forms.Button btnEVEMonDataDir;
        private EmailNotificationsControl emailNotificationsControl;
        private System.Windows.Forms.CheckBox mailNotificationCheckBox;
        private MultiPanelPage portableEveClientsPage;
        private System.Windows.Forms.Label lblPECIDescription;
        private System.Windows.Forms.GroupBox PECIGroupBox;
        private PortableEveClientsControl portableEveClientsControl;
        private MultiPanelPage marketPriceProvidersPage;
        private System.Windows.Forms.Label marketPriceProviderPageLabel;
        private System.Windows.Forms.GroupBox gbMarketPriceProviders;
        private System.Windows.Forms.ComboBox cbProvidersList;
        private System.Windows.Forms.Label SelectedProviderLabel;
        private MultiPanelPage cloudStorageServicePage;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.GroupBox settingsFileStorageGroupBox;
        private SettingsFileStorageControl settingsFileStorageControl;
        private System.Windows.Forms.GroupBox cloudStorageGroupBox;
        private System.Windows.Forms.ComboBox cloudStorageProvidersComboBox;
        private System.Windows.Forms.Label lblSelectedProvider;
        private System.Windows.Forms.GroupBox providerAuthenticationGroupBox;
        private CloudStorageServiceControl cloudStorageServiceControl;
        private System.Windows.Forms.PictureBox cloudStorageProviderLogoPictureBox;
        private ExternalCalendarControl externalCalendarControl;
        private System.Windows.Forms.NumericUpDown nudSkillQueueWarningThresholdDays;
        private System.Windows.Forms.Label lblSkillQueueWarningThresholdDays;
        private System.Windows.Forms.Label lblSkillQueuWarningThreshold;
        private System.Windows.Forms.CheckBox cbShowSkillpointsOnOverview;
        private System.Windows.Forms.GroupBox esiSettingsGroupBox;
        private System.Windows.Forms.TextBox clientIDTextBox;
        private System.Windows.Forms.LinkLabel esiSettingsLabel;
        private System.Windows.Forms.Label lblClientSecret;
        private System.Windows.Forms.Label lblClientID;
        private System.Windows.Forms.TextBox clientSecretTextBox;
        private System.Windows.Forms.TextBox scopesTextBox;
        private System.Windows.Forms.Label lblScopes;
        private System.Windows.Forms.Button insertAllScopesButton;
    }
}
