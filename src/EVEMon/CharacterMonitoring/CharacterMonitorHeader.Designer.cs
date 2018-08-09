namespace EVEMon.CharacterMonitoring
{
    internal sealed partial class CharacterMonitorHeader
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.MainTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.AccountStatusTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.AccountStatusLabel = new System.Windows.Forms.Label();
            this.AccountActivityLabel = new System.Windows.Forms.Label();
            this.PaidUntilLabel = new System.Windows.Forms.Label();
            this.ThrobberFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.UpdateThrobber = new EVEMon.Common.Controls.Throbber();
            this.ThrobberContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ChangeAPIKeyInfoMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.QueryEverythingMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ThrobberSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.UpdateLabel = new System.Windows.Forms.Label();
            this.BioFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.CharacterNameLabel = new System.Windows.Forms.Label();
            this.BioInfoLabel = new System.Windows.Forms.Label();
            this.BalanceLabel = new System.Windows.Forms.Label();
            this.BirthdayLabel = new System.Windows.Forms.Label();
            this.CorporationNameLabel = new System.Windows.Forms.Label();
            this.CorporationInfoFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.AllianceNameLabel = new System.Windows.Forms.Label();
            this.SecurityStatusLabel = new System.Windows.Forms.Label();
            this.ActiveShipLabel = new System.Windows.Forms.Label();
            this.ActiveShipFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.LocationInfoLabel = new System.Windows.Forms.Label();
            this.DockedInfoLabel = new System.Windows.Forms.Label();
            this.CharacterPortrait = new EVEMon.Common.Controls.CharacterPortrait();
            this.SkillSummaryPanel = new System.Windows.Forms.Panel();
            this.RemapsCloneJumpSummaryLabel = new System.Windows.Forms.Label();
            this.tlpAttributes = new System.Windows.Forms.TableLayoutPanel();
            this.lblMEMAttribute = new System.Windows.Forms.Label();
            this.lblWILAttribute = new System.Windows.Forms.Label();
            this.lblCHAAttribute = new System.Windows.Forms.Label();
            this.lblPERAttribute = new System.Windows.Forms.Label();
            this.AttributeCharismaLabel = new System.Windows.Forms.Label();
            this.AttributePerceptionLabel = new System.Windows.Forms.Label();
            this.AttributeIntelligenceLabel = new System.Windows.Forms.Label();
            this.AttributeMemoryLabel = new System.Windows.Forms.Label();
            this.AttributeWillpowerLabel = new System.Windows.Forms.Label();
            this.lblINTAttribute = new System.Windows.Forms.Label();
            this.SkillSummaryLabel = new System.Windows.Forms.Label();
            this.CustomLabelComboBox = new System.Windows.Forms.ComboBox();
            this.CharacterLabel = new System.Windows.Forms.Label();
            this.ToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.MainTableLayoutPanel.SuspendLayout();
            this.AccountStatusTableLayoutPanel.SuspendLayout();
            this.ThrobberFlowLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.UpdateThrobber)).BeginInit();
            this.ThrobberContextMenu.SuspendLayout();
            this.BioFlowLayoutPanel.SuspendLayout();
            this.SkillSummaryPanel.SuspendLayout();
            this.tlpAttributes.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainTableLayoutPanel
            // 
            this.MainTableLayoutPanel.AutoSize = true;
            this.MainTableLayoutPanel.ColumnCount = 3;
            this.MainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.MainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.MainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.MainTableLayoutPanel.Controls.Add(this.AccountStatusTableLayoutPanel, 0, 1);
            this.MainTableLayoutPanel.Controls.Add(this.ThrobberFlowLayoutPanel, 2, 0);
            this.MainTableLayoutPanel.Controls.Add(this.BioFlowLayoutPanel, 1, 0);
            this.MainTableLayoutPanel.Controls.Add(this.CharacterPortrait, 0, 0);
            this.MainTableLayoutPanel.Controls.Add(this.SkillSummaryPanel, 1, 1);
            this.MainTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.MainTableLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.MainTableLayoutPanel.Name = "MainTableLayoutPanel";
            this.MainTableLayoutPanel.RowCount = 2;
            this.MainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.MainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.MainTableLayoutPanel.Size = new System.Drawing.Size(525, 204);
            this.MainTableLayoutPanel.TabIndex = 0;
            // 
            // AccountStatusTableLayoutPanel
            // 
            this.AccountStatusTableLayoutPanel.ColumnCount = 2;
            this.AccountStatusTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.AccountStatusTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.AccountStatusTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.AccountStatusTableLayoutPanel.Controls.Add(this.AccountStatusLabel, 0, 0);
            this.AccountStatusTableLayoutPanel.Controls.Add(this.AccountActivityLabel, 1, 0);
            this.AccountStatusTableLayoutPanel.Controls.Add(this.PaidUntilLabel, 0, 1);
            this.AccountStatusTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AccountStatusTableLayoutPanel.Location = new System.Drawing.Point(0, 131);
            this.AccountStatusTableLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.AccountStatusTableLayoutPanel.Name = "AccountStatusTableLayoutPanel";
            this.AccountStatusTableLayoutPanel.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.AccountStatusTableLayoutPanel.RowCount = 2;
            this.AccountStatusTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.AccountStatusTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.AccountStatusTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.AccountStatusTableLayoutPanel.Size = new System.Drawing.Size(136, 73);
            this.AccountStatusTableLayoutPanel.TabIndex = 11;
            // 
            // AccountStatusLabel
            // 
            this.AccountStatusLabel.AutoSize = true;
            this.AccountStatusLabel.Location = new System.Drawing.Point(0, 5);
            this.AccountStatusLabel.Margin = new System.Windows.Forms.Padding(0);
            this.AccountStatusLabel.Name = "AccountStatusLabel";
            this.AccountStatusLabel.Size = new System.Drawing.Size(83, 13);
            this.AccountStatusLabel.TabIndex = 0;
            this.AccountStatusLabel.Text = "Account Status:";
            // 
            // AccountActivityLabel
            // 
            this.AccountActivityLabel.AutoSize = true;
            this.AccountActivityLabel.Location = new System.Drawing.Point(83, 5);
            this.AccountActivityLabel.Margin = new System.Windows.Forms.Padding(0);
            this.AccountActivityLabel.Name = "AccountActivityLabel";
            this.AccountActivityLabel.Size = new System.Drawing.Size(25, 13);
            this.AccountActivityLabel.TabIndex = 1;
            this.AccountActivityLabel.Text = "???";
            // 
            // PaidUntilLabel
            // 
            this.PaidUntilLabel.AutoSize = true;
            this.AccountStatusTableLayoutPanel.SetColumnSpan(this.PaidUntilLabel, 2);
            this.PaidUntilLabel.Location = new System.Drawing.Point(0, 18);
            this.PaidUntilLabel.Margin = new System.Windows.Forms.Padding(0);
            this.PaidUntilLabel.Name = "PaidUntilLabel";
            this.PaidUntilLabel.Size = new System.Drawing.Size(126, 13);
            this.PaidUntilLabel.TabIndex = 2;
            this.PaidUntilLabel.Text = "dd/MM/YYYY HH:mm:ss";
            // 
            // ThrobberFlowLayoutPanel
            // 
            this.ThrobberFlowLayoutPanel.AutoSize = true;
            this.ThrobberFlowLayoutPanel.Controls.Add(this.UpdateThrobber);
            this.ThrobberFlowLayoutPanel.Controls.Add(this.UpdateLabel);
            this.ThrobberFlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ThrobberFlowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.ThrobberFlowLayoutPanel.Location = new System.Drawing.Point(464, 0);
            this.ThrobberFlowLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.ThrobberFlowLayoutPanel.Name = "ThrobberFlowLayoutPanel";
            this.ThrobberFlowLayoutPanel.Size = new System.Drawing.Size(61, 131);
            this.ThrobberFlowLayoutPanel.TabIndex = 10;
            // 
            // UpdateThrobber
            // 
            this.UpdateThrobber.ContextMenuStrip = this.ThrobberContextMenu;
            this.UpdateThrobber.Dock = System.Windows.Forms.DockStyle.Right;
            this.UpdateThrobber.Location = new System.Drawing.Point(34, 3);
            this.UpdateThrobber.MaximumSize = new System.Drawing.Size(24, 24);
            this.UpdateThrobber.MinimumSize = new System.Drawing.Size(24, 24);
            this.UpdateThrobber.Name = "UpdateThrobber";
            this.UpdateThrobber.Size = new System.Drawing.Size(24, 24);
            this.UpdateThrobber.State = EVEMon.Common.Enumerations.ThrobberState.Stopped;
            this.UpdateThrobber.TabIndex = 4;
            this.UpdateThrobber.TabStop = false;
            this.UpdateThrobber.MouseDown += new System.Windows.Forms.MouseEventHandler(this.UpdateThrobber_MouseDown);
            this.UpdateThrobber.MouseMove += new System.Windows.Forms.MouseEventHandler(this.UpdateThrobber_MouseMove);
            // 
            // ThrobberContextMenu
            // 
            this.ThrobberContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ChangeAPIKeyInfoMenuItem,
            this.QueryEverythingMenuItem,
            this.ThrobberSeparator});
            this.ThrobberContextMenu.Name = "cmsThrobberMenu";
            this.ThrobberContextMenu.Size = new System.Drawing.Size(234, 54);
            this.ThrobberContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.ThrobberContextMenu_Opening);
            this.ThrobberContextMenu.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.ThrobberContextMenu_ItemClicked);
            // 
            // ChangeAPIKeyInfoMenuItem
            // 
            this.ChangeAPIKeyInfoMenuItem.Name = "ChangeAPIKeyInfoMenuItem";
            this.ChangeAPIKeyInfoMenuItem.Size = new System.Drawing.Size(233, 22);
            this.ChangeAPIKeyInfoMenuItem.Text = "Change API Key Information...";
            this.ChangeAPIKeyInfoMenuItem.Click += new System.EventHandler(this.ChangeAPIKeyInfoMenuItem_Click);
            // 
            // QueryEverythingMenuItem
            // 
            this.QueryEverythingMenuItem.Name = "QueryEverythingMenuItem";
            this.QueryEverythingMenuItem.Size = new System.Drawing.Size(233, 22);
            this.QueryEverythingMenuItem.Text = "Update Everything";
            // 
            // ThrobberSeparator
            // 
            this.ThrobberSeparator.Name = "ThrobberSeparator";
            this.ThrobberSeparator.Size = new System.Drawing.Size(230, 6);
            // 
            // UpdateLabel
            // 
            this.UpdateLabel.AutoSize = true;
            this.UpdateLabel.Dock = System.Windows.Forms.DockStyle.Right;
            this.UpdateLabel.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.UpdateLabel.Location = new System.Drawing.Point(0, 30);
            this.UpdateLabel.Margin = new System.Windows.Forms.Padding(0);
            this.UpdateLabel.Name = "UpdateLabel";
            this.UpdateLabel.Size = new System.Drawing.Size(61, 13);
            this.UpdateLabel.TabIndex = 5;
            this.UpdateLabel.Text = "0000:00:00";
            this.UpdateLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.UpdateLabel.MouseHover += new System.EventHandler(this.UpdateLabel_MouseHover);
            // 
            // BioFlowLayoutPanel
            // 
            this.BioFlowLayoutPanel.AutoSize = true;
            this.BioFlowLayoutPanel.Controls.Add(this.CharacterNameLabel);
            this.BioFlowLayoutPanel.Controls.Add(this.BioInfoLabel);
            this.BioFlowLayoutPanel.Controls.Add(this.BalanceLabel);
            this.BioFlowLayoutPanel.Controls.Add(this.BirthdayLabel);
            this.BioFlowLayoutPanel.Controls.Add(this.CorporationNameLabel);
            this.BioFlowLayoutPanel.Controls.Add(this.CorporationInfoFlowLayoutPanel);
            this.BioFlowLayoutPanel.Controls.Add(this.AllianceNameLabel);
            this.BioFlowLayoutPanel.Controls.Add(this.SecurityStatusLabel);
            this.BioFlowLayoutPanel.Controls.Add(this.ActiveShipLabel);
            this.BioFlowLayoutPanel.Controls.Add(this.ActiveShipFlowLayoutPanel);
            this.BioFlowLayoutPanel.Controls.Add(this.LocationInfoLabel);
            this.BioFlowLayoutPanel.Controls.Add(this.DockedInfoLabel);
            this.BioFlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BioFlowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.BioFlowLayoutPanel.Location = new System.Drawing.Point(136, 0);
            this.BioFlowLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.BioFlowLayoutPanel.Name = "BioFlowLayoutPanel";
            this.BioFlowLayoutPanel.Size = new System.Drawing.Size(328, 131);
            this.BioFlowLayoutPanel.TabIndex = 9;
            // 
            // CharacterNameLabel
            // 
            this.CharacterNameLabel.AutoSize = true;
            this.CharacterNameLabel.Location = new System.Drawing.Point(0, 0);
            this.CharacterNameLabel.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.CharacterNameLabel.Name = "CharacterNameLabel";
            this.CharacterNameLabel.Size = new System.Drawing.Size(84, 13);
            this.CharacterNameLabel.TabIndex = 4;
            this.CharacterNameLabel.Text = "Character Name";
            this.CharacterNameLabel.UseMnemonic = false;
            // 
            // BioInfoLabel
            // 
            this.BioInfoLabel.AutoSize = true;
            this.BioInfoLabel.Location = new System.Drawing.Point(0, 13);
            this.BioInfoLabel.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.BioInfoLabel.Name = "BioInfoLabel";
            this.BioInfoLabel.Size = new System.Drawing.Size(43, 13);
            this.BioInfoLabel.TabIndex = 5;
            this.BioInfoLabel.Text = "Bio Info";
            // 
            // BalanceLabel
            // 
            this.BalanceLabel.AutoSize = true;
            this.BalanceLabel.Location = new System.Drawing.Point(0, 26);
            this.BalanceLabel.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.BalanceLabel.Name = "BalanceLabel";
            this.BalanceLabel.Size = new System.Drawing.Size(93, 13);
            this.BalanceLabel.TabIndex = 7;
            this.BalanceLabel.Text = "Balance: 0.00 ISK";
            // 
            // BirthdayLabel
            // 
            this.BirthdayLabel.AutoSize = true;
            this.BirthdayLabel.Location = new System.Drawing.Point(0, 39);
            this.BirthdayLabel.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.BirthdayLabel.Name = "BirthdayLabel";
            this.BirthdayLabel.Size = new System.Drawing.Size(45, 13);
            this.BirthdayLabel.TabIndex = 8;
            this.BirthdayLabel.Text = "Birthday";
            // 
            // CorporationNameLabel
            // 
            this.CorporationNameLabel.AutoSize = true;
            this.CorporationNameLabel.Location = new System.Drawing.Point(0, 52);
            this.CorporationNameLabel.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.CorporationNameLabel.Name = "CorporationNameLabel";
            this.CorporationNameLabel.Size = new System.Drawing.Size(82, 13);
            this.CorporationNameLabel.TabIndex = 6;
            this.CorporationNameLabel.Text = "Corporation Info";
            this.CorporationNameLabel.UseMnemonic = false;
            // 
            // CorporationInfoFlowLayoutPanel
            // 
            this.CorporationInfoFlowLayoutPanel.AutoSize = true;
            this.CorporationInfoFlowLayoutPanel.Location = new System.Drawing.Point(0, 65);
            this.CorporationInfoFlowLayoutPanel.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.CorporationInfoFlowLayoutPanel.Name = "CorporationInfoFlowLayoutPanel";
            this.CorporationInfoFlowLayoutPanel.Size = new System.Drawing.Size(0, 0);
            this.CorporationInfoFlowLayoutPanel.TabIndex = 13;
            // 
            // AllianceNameLabel
            // 
            this.AllianceNameLabel.AutoSize = true;
            this.AllianceNameLabel.Location = new System.Drawing.Point(0, 65);
            this.AllianceNameLabel.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.AllianceNameLabel.Name = "AllianceNameLabel";
            this.AllianceNameLabel.Size = new System.Drawing.Size(65, 13);
            this.AllianceNameLabel.TabIndex = 7;
            this.AllianceNameLabel.Text = "Alliance Info";
            this.AllianceNameLabel.UseMnemonic = false;
            // 
            // SecurityStatusLabel
            // 
            this.SecurityStatusLabel.AutoSize = true;
            this.SecurityStatusLabel.Location = new System.Drawing.Point(0, 78);
            this.SecurityStatusLabel.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.SecurityStatusLabel.Name = "SecurityStatusLabel";
            this.SecurityStatusLabel.Size = new System.Drawing.Size(99, 13);
            this.SecurityStatusLabel.TabIndex = 9;
            this.SecurityStatusLabel.Text = "Security Status Info";
            // 
            // ActiveShipLabel
            // 
            this.ActiveShipLabel.AutoSize = true;
            this.ActiveShipLabel.Location = new System.Drawing.Point(0, 91);
            this.ActiveShipLabel.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.ActiveShipLabel.Name = "ActiveShipLabel";
            this.ActiveShipLabel.Size = new System.Drawing.Size(82, 13);
            this.ActiveShipLabel.TabIndex = 10;
            this.ActiveShipLabel.Text = "Active Ship Info";
            this.ActiveShipLabel.UseMnemonic = false;
            // 
            // ActiveShipFlowLayoutPanel
            // 
            this.ActiveShipFlowLayoutPanel.AutoSize = true;
            this.ActiveShipFlowLayoutPanel.Location = new System.Drawing.Point(0, 104);
            this.ActiveShipFlowLayoutPanel.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.ActiveShipFlowLayoutPanel.Name = "ActiveShipFlowLayoutPanel";
            this.ActiveShipFlowLayoutPanel.Size = new System.Drawing.Size(0, 0);
            this.ActiveShipFlowLayoutPanel.TabIndex = 12;
            this.ActiveShipFlowLayoutPanel.WrapContents = false;
            // 
            // LocationInfoLabel
            // 
            this.LocationInfoLabel.AutoSize = true;
            this.LocationInfoLabel.Location = new System.Drawing.Point(0, 104);
            this.LocationInfoLabel.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.LocationInfoLabel.Name = "LocationInfoLabel";
            this.LocationInfoLabel.Size = new System.Drawing.Size(69, 13);
            this.LocationInfoLabel.TabIndex = 12;
            this.LocationInfoLabel.Text = "Location Info";
            this.LocationInfoLabel.UseMnemonic = false;
            // 
            // DockedInfoLabel
            // 
            this.DockedInfoLabel.AutoSize = true;
            this.DockedInfoLabel.Location = new System.Drawing.Point(0, 117);
            this.DockedInfoLabel.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.DockedInfoLabel.Name = "DockedInfoLabel";
            this.DockedInfoLabel.Size = new System.Drawing.Size(66, 13);
            this.DockedInfoLabel.TabIndex = 14;
            this.DockedInfoLabel.Text = "Docked Info";
            this.DockedInfoLabel.UseMnemonic = false;
            // 
            // CharacterPortrait
            // 
            this.CharacterPortrait.AutoSize = true;
            this.CharacterPortrait.Character = null;
            this.CharacterPortrait.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CharacterPortrait.Location = new System.Drawing.Point(3, 3);
            this.CharacterPortrait.Margin = new System.Windows.Forms.Padding(3, 3, 5, 0);
            this.CharacterPortrait.MinimumSize = new System.Drawing.Size(128, 128);
            this.CharacterPortrait.Name = "CharacterPortrait";
            this.CharacterPortrait.Size = new System.Drawing.Size(128, 128);
            this.CharacterPortrait.TabIndex = 2;
            this.CharacterPortrait.TabStop = false;
            // 
            // SkillSummaryPanel
            // 
            this.SkillSummaryPanel.AutoSize = true;
            this.MainTableLayoutPanel.SetColumnSpan(this.SkillSummaryPanel, 2);
            this.SkillSummaryPanel.Controls.Add(this.RemapsCloneJumpSummaryLabel);
            this.SkillSummaryPanel.Controls.Add(this.tlpAttributes);
            this.SkillSummaryPanel.Controls.Add(this.SkillSummaryLabel);
            this.SkillSummaryPanel.Controls.Add(this.CustomLabelComboBox);
            this.SkillSummaryPanel.Controls.Add(this.CharacterLabel);
            this.SkillSummaryPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SkillSummaryPanel.Location = new System.Drawing.Point(136, 131);
            this.SkillSummaryPanel.Margin = new System.Windows.Forms.Padding(0);
            this.SkillSummaryPanel.Name = "SkillSummaryPanel";
            this.SkillSummaryPanel.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.SkillSummaryPanel.Size = new System.Drawing.Size(389, 73);
            this.SkillSummaryPanel.TabIndex = 4;
            // 
            // RemapsCloneJumpSummaryLabel
            // 
            this.RemapsCloneJumpSummaryLabel.AutoSize = true;
            this.RemapsCloneJumpSummaryLabel.BackColor = System.Drawing.Color.Transparent;
            this.RemapsCloneJumpSummaryLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.RemapsCloneJumpSummaryLabel.Location = new System.Drawing.Point(89, 5);
            this.RemapsCloneJumpSummaryLabel.Margin = new System.Windows.Forms.Padding(0);
            this.RemapsCloneJumpSummaryLabel.Name = "RemapsCloneJumpSummaryLabel";
            this.RemapsCloneJumpSummaryLabel.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.RemapsCloneJumpSummaryLabel.Size = new System.Drawing.Size(138, 39);
            this.RemapsCloneJumpSummaryLabel.TabIndex = 6;
            this.RemapsCloneJumpSummaryLabel.Text = "Bonus Remaps: 0\r\nNext Neural Remap: Now\r\nNext Clone Jump: Now";
            // 
            // tlpAttributes
            // 
            this.tlpAttributes.AutoSize = true;
            this.tlpAttributes.ColumnCount = 2;
            this.tlpAttributes.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpAttributes.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpAttributes.Controls.Add(this.lblMEMAttribute, 1, 4);
            this.tlpAttributes.Controls.Add(this.lblWILAttribute, 1, 3);
            this.tlpAttributes.Controls.Add(this.lblCHAAttribute, 1, 2);
            this.tlpAttributes.Controls.Add(this.lblPERAttribute, 1, 1);
            this.tlpAttributes.Controls.Add(this.AttributeCharismaLabel, 0, 2);
            this.tlpAttributes.Controls.Add(this.AttributePerceptionLabel, 0, 1);
            this.tlpAttributes.Controls.Add(this.AttributeIntelligenceLabel, 0, 0);
            this.tlpAttributes.Controls.Add(this.AttributeMemoryLabel, 0, 4);
            this.tlpAttributes.Controls.Add(this.AttributeWillpowerLabel, 0, 3);
            this.tlpAttributes.Controls.Add(this.lblINTAttribute, 1, 0);
            this.tlpAttributes.Dock = System.Windows.Forms.DockStyle.Left;
            this.tlpAttributes.Location = new System.Drawing.Point(0, 5);
            this.tlpAttributes.Name = "tlpAttributes";
            this.tlpAttributes.RowCount = 5;
            this.tlpAttributes.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpAttributes.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpAttributes.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpAttributes.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpAttributes.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpAttributes.Size = new System.Drawing.Size(89, 68);
            this.tlpAttributes.TabIndex = 5;
            // 
            // lblMEMAttribute
            // 
            this.lblMEMAttribute.AutoSize = true;
            this.lblMEMAttribute.Location = new System.Drawing.Point(67, 52);
            this.lblMEMAttribute.Name = "lblMEMAttribute";
            this.lblMEMAttribute.Size = new System.Drawing.Size(19, 13);
            this.lblMEMAttribute.TabIndex = 9;
            this.lblMEMAttribute.Text = "17";
            this.lblMEMAttribute.MouseHover += new System.EventHandler(this.AttributeLabel_MouseHover);
            // 
            // lblWILAttribute
            // 
            this.lblWILAttribute.AutoSize = true;
            this.lblWILAttribute.Location = new System.Drawing.Point(67, 39);
            this.lblWILAttribute.Name = "lblWILAttribute";
            this.lblWILAttribute.Size = new System.Drawing.Size(19, 13);
            this.lblWILAttribute.TabIndex = 8;
            this.lblWILAttribute.Text = "17";
            this.lblWILAttribute.MouseHover += new System.EventHandler(this.AttributeLabel_MouseHover);
            // 
            // lblCHAAttribute
            // 
            this.lblCHAAttribute.AutoSize = true;
            this.lblCHAAttribute.Location = new System.Drawing.Point(67, 26);
            this.lblCHAAttribute.Name = "lblCHAAttribute";
            this.lblCHAAttribute.Size = new System.Drawing.Size(19, 13);
            this.lblCHAAttribute.TabIndex = 7;
            this.lblCHAAttribute.Text = "17";
            this.lblCHAAttribute.MouseHover += new System.EventHandler(this.AttributeLabel_MouseHover);
            // 
            // lblPERAttribute
            // 
            this.lblPERAttribute.AutoSize = true;
            this.lblPERAttribute.Location = new System.Drawing.Point(67, 13);
            this.lblPERAttribute.Name = "lblPERAttribute";
            this.lblPERAttribute.Size = new System.Drawing.Size(19, 13);
            this.lblPERAttribute.TabIndex = 6;
            this.lblPERAttribute.Text = "17";
            this.lblPERAttribute.MouseHover += new System.EventHandler(this.AttributeLabel_MouseHover);
            // 
            // AttributeCharismaLabel
            // 
            this.AttributeCharismaLabel.AutoSize = true;
            this.AttributeCharismaLabel.Location = new System.Drawing.Point(0, 26);
            this.AttributeCharismaLabel.Margin = new System.Windows.Forms.Padding(0);
            this.AttributeCharismaLabel.Name = "AttributeCharismaLabel";
            this.AttributeCharismaLabel.Size = new System.Drawing.Size(53, 13);
            this.AttributeCharismaLabel.TabIndex = 2;
            this.AttributeCharismaLabel.Text = "Charisma:";
            // 
            // AttributePerceptionLabel
            // 
            this.AttributePerceptionLabel.AutoSize = true;
            this.AttributePerceptionLabel.Location = new System.Drawing.Point(0, 13);
            this.AttributePerceptionLabel.Margin = new System.Windows.Forms.Padding(0);
            this.AttributePerceptionLabel.Name = "AttributePerceptionLabel";
            this.AttributePerceptionLabel.Size = new System.Drawing.Size(61, 13);
            this.AttributePerceptionLabel.TabIndex = 1;
            this.AttributePerceptionLabel.Text = "Perception:";
            // 
            // AttributeIntelligenceLabel
            // 
            this.AttributeIntelligenceLabel.AutoSize = true;
            this.AttributeIntelligenceLabel.Location = new System.Drawing.Point(0, 0);
            this.AttributeIntelligenceLabel.Margin = new System.Windows.Forms.Padding(0);
            this.AttributeIntelligenceLabel.Name = "AttributeIntelligenceLabel";
            this.AttributeIntelligenceLabel.Size = new System.Drawing.Size(64, 13);
            this.AttributeIntelligenceLabel.TabIndex = 0;
            this.AttributeIntelligenceLabel.Text = "Intelligence:";
            // 
            // AttributeMemoryLabel
            // 
            this.AttributeMemoryLabel.AutoSize = true;
            this.AttributeMemoryLabel.Location = new System.Drawing.Point(0, 52);
            this.AttributeMemoryLabel.Margin = new System.Windows.Forms.Padding(0);
            this.AttributeMemoryLabel.Name = "AttributeMemoryLabel";
            this.AttributeMemoryLabel.Size = new System.Drawing.Size(47, 13);
            this.AttributeMemoryLabel.TabIndex = 4;
            this.AttributeMemoryLabel.Text = "Memory:";
            // 
            // AttributeWillpowerLabel
            // 
            this.AttributeWillpowerLabel.AutoSize = true;
            this.AttributeWillpowerLabel.Location = new System.Drawing.Point(0, 39);
            this.AttributeWillpowerLabel.Margin = new System.Windows.Forms.Padding(0);
            this.AttributeWillpowerLabel.Name = "AttributeWillpowerLabel";
            this.AttributeWillpowerLabel.Size = new System.Drawing.Size(56, 13);
            this.AttributeWillpowerLabel.TabIndex = 3;
            this.AttributeWillpowerLabel.Text = "Willpower:";
            // 
            // lblINTAttribute
            // 
            this.lblINTAttribute.AutoSize = true;
            this.lblINTAttribute.Location = new System.Drawing.Point(67, 0);
            this.lblINTAttribute.Name = "lblINTAttribute";
            this.lblINTAttribute.Size = new System.Drawing.Size(19, 13);
            this.lblINTAttribute.TabIndex = 5;
            this.lblINTAttribute.Text = "17";
            this.lblINTAttribute.MouseHover += new System.EventHandler(this.AttributeLabel_MouseHover);
            // 
            // SkillSummaryLabel
            // 
            this.SkillSummaryLabel.AutoSize = true;
            this.SkillSummaryLabel.BackColor = System.Drawing.Color.Transparent;
            this.SkillSummaryLabel.Dock = System.Windows.Forms.DockStyle.Right;
            this.SkillSummaryLabel.Location = new System.Drawing.Point(295, 5);
            this.SkillSummaryLabel.Margin = new System.Windows.Forms.Padding(0);
            this.SkillSummaryLabel.Name = "SkillSummaryLabel";
            this.SkillSummaryLabel.Size = new System.Drawing.Size(94, 52);
            this.SkillSummaryLabel.TabIndex = 1;
            this.SkillSummaryLabel.Text = "Known Skills: 0\r\nSkills at Level V: 0\r\nTotal SP: 0\r\nFree SP: 0";
            this.SkillSummaryLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.SkillSummaryLabel.MouseHover += new System.EventHandler(this.SkillSummaryLabel_MouseHover);
            // 
            // CustomLabelComboBox
            // 
            this.CustomLabelComboBox.Location = new System.Drawing.Point(138, 47);
            this.CustomLabelComboBox.MaxLength = 256;
            this.CustomLabelComboBox.Name = "CustomLabelComboBox";
            this.CustomLabelComboBox.Size = new System.Drawing.Size(130, 21);
            this.CustomLabelComboBox.TabIndex = 3;
            this.ToolTip.SetToolTip(this.CustomLabelComboBox, "Character label");
            this.CustomLabelComboBox.SelectedIndexChanged += new System.EventHandler(this.CustomLabelComboBox_TextChanged);
            this.CustomLabelComboBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.CustomLabelComboBox_KeyUp);
            this.CustomLabelComboBox.Validated += new System.EventHandler(this.CustomLabelComboBox_TextChanged);
            // 
            // CharacterLabel
            // 
            this.CharacterLabel.AutoSize = true;
            this.CharacterLabel.Location = new System.Drawing.Point(89, 50);
            this.CharacterLabel.Margin = new System.Windows.Forms.Padding(0);
            this.CharacterLabel.Name = "CharacterLabel";
            this.CharacterLabel.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.CharacterLabel.Size = new System.Drawing.Size(46, 13);
            this.CharacterLabel.TabIndex = 3;
            this.CharacterLabel.Text = "Label:";
            // 
            // ToolTip
            // 
            this.ToolTip.AutoPopDelay = 5000000;
            this.ToolTip.InitialDelay = 500;
            this.ToolTip.IsBalloon = true;
            this.ToolTip.ReshowDelay = 100;
            // 
            // CharacterMonitorHeader
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.MainTableLayoutPanel);
            this.Name = "CharacterMonitorHeader";
            this.Size = new System.Drawing.Size(525, 204);
            this.Resize += new System.EventHandler(this.CharacterMonitorHeader_Resize);
            this.MainTableLayoutPanel.ResumeLayout(false);
            this.MainTableLayoutPanel.PerformLayout();
            this.AccountStatusTableLayoutPanel.ResumeLayout(false);
            this.AccountStatusTableLayoutPanel.PerformLayout();
            this.ThrobberFlowLayoutPanel.ResumeLayout(false);
            this.ThrobberFlowLayoutPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.UpdateThrobber)).EndInit();
            this.ThrobberContextMenu.ResumeLayout(false);
            this.BioFlowLayoutPanel.ResumeLayout(false);
            this.BioFlowLayoutPanel.PerformLayout();
            this.SkillSummaryPanel.ResumeLayout(false);
            this.SkillSummaryPanel.PerformLayout();
            this.tlpAttributes.ResumeLayout(false);
            this.tlpAttributes.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel MainTableLayoutPanel;
        private EVEMon.Common.Controls.CharacterPortrait CharacterPortrait;
        private System.Windows.Forms.ContextMenuStrip ThrobberContextMenu;
        private System.Windows.Forms.ToolStripMenuItem ChangeAPIKeyInfoMenuItem;
        private System.Windows.Forms.ToolStripMenuItem QueryEverythingMenuItem;
        private System.Windows.Forms.ToolStripSeparator ThrobberSeparator;
        private System.Windows.Forms.ToolTip ToolTip;
        private System.Windows.Forms.FlowLayoutPanel BioFlowLayoutPanel;
        private System.Windows.Forms.Label CharacterNameLabel;
        private System.Windows.Forms.Label BioInfoLabel;
        private System.Windows.Forms.Label BalanceLabel;
        private System.Windows.Forms.Label BirthdayLabel;
        private System.Windows.Forms.FlowLayoutPanel CorporationInfoFlowLayoutPanel;
        private System.Windows.Forms.Label CorporationNameLabel;
        private System.Windows.Forms.Label SecurityStatusLabel;
        private System.Windows.Forms.FlowLayoutPanel ActiveShipFlowLayoutPanel;
        private System.Windows.Forms.Label ActiveShipLabel;
        private System.Windows.Forms.FlowLayoutPanel ThrobberFlowLayoutPanel;
        private EVEMon.Common.Controls.Throbber UpdateThrobber;
        private System.Windows.Forms.Label UpdateLabel;
        private System.Windows.Forms.Label AllianceNameLabel;
        private System.Windows.Forms.Label LocationInfoLabel;
        private System.Windows.Forms.Label DockedInfoLabel;
        private System.Windows.Forms.Panel SkillSummaryPanel;
        private System.Windows.Forms.TableLayoutPanel AccountStatusTableLayoutPanel;
        private System.Windows.Forms.Label AccountStatusLabel;
        private System.Windows.Forms.Label AccountActivityLabel;
        private System.Windows.Forms.Label PaidUntilLabel;
        private System.Windows.Forms.TableLayoutPanel tlpAttributes;
        private System.Windows.Forms.Label lblMEMAttribute;
        private System.Windows.Forms.Label lblWILAttribute;
        private System.Windows.Forms.Label lblCHAAttribute;
        private System.Windows.Forms.Label lblPERAttribute;
        private System.Windows.Forms.Label AttributeCharismaLabel;
        private System.Windows.Forms.Label AttributePerceptionLabel;
        private System.Windows.Forms.Label AttributeIntelligenceLabel;
        private System.Windows.Forms.Label AttributeMemoryLabel;
        private System.Windows.Forms.Label AttributeWillpowerLabel;
        private System.Windows.Forms.Label lblINTAttribute;
        private System.Windows.Forms.Label SkillSummaryLabel;
        private System.Windows.Forms.Label RemapsCloneJumpSummaryLabel;
        private System.Windows.Forms.ComboBox CustomLabelComboBox;
        private System.Windows.Forms.Label CharacterLabel;
    }
}
