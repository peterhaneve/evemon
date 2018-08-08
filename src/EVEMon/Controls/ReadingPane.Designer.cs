namespace EVEMon.Controls
{
    partial class ReadingPane
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
            this.previewPanePanel = new System.Windows.Forms.Panel();
            this.wbMailBody = new System.Windows.Forms.WebBrowser();
            this.flPanelHeader = new System.Windows.Forms.FlowLayoutPanel();
            this.lblMessageHeader = new System.Windows.Forms.Label();
            this.lblSender = new System.Windows.Forms.Label();
            this.lblSendDate = new System.Windows.Forms.Label();
            this.lblRecipient = new System.Windows.Forms.Label();
            this.previewPanePanel.SuspendLayout();
            this.flPanelHeader.SuspendLayout();
            this.SuspendLayout();
            // 
            // previewPanePanel
            // 
            this.previewPanePanel.BackColor = System.Drawing.SystemColors.Control;
            this.previewPanePanel.Controls.Add(this.wbMailBody);
            this.previewPanePanel.Controls.Add(this.flPanelHeader);
            this.previewPanePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.previewPanePanel.Location = new System.Drawing.Point(0, 0);
            this.previewPanePanel.Name = "previewPanePanel";
            this.previewPanePanel.Padding = new System.Windows.Forms.Padding(5);
            this.previewPanePanel.Size = new System.Drawing.Size(267, 160);
            this.previewPanePanel.TabIndex = 2;
            // 
            // wbMailBody
            // 
            this.wbMailBody.AllowWebBrowserDrop = false;
            this.wbMailBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wbMailBody.IsWebBrowserContextMenuEnabled = false;
            this.wbMailBody.Location = new System.Drawing.Point(5, 74);
            this.wbMailBody.MinimumSize = new System.Drawing.Size(20, 20);
            this.wbMailBody.Name = "wbMailBody";
            this.wbMailBody.ScriptErrorsSuppressed = true;
            this.wbMailBody.Size = new System.Drawing.Size(257, 81);
            this.wbMailBody.TabIndex = 2;
            this.wbMailBody.Url = new System.Uri("about:blank\r", System.UriKind.Absolute);
            this.wbMailBody.WebBrowserShortcutsEnabled = false;
            this.wbMailBody.Navigating += new System.Windows.Forms.WebBrowserNavigatingEventHandler(this.wbMailBody_Navigating);
            this.wbMailBody.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.wbMailBody_PreviewKeyDown);
            // 
            // flPanelHeader
            // 
            this.flPanelHeader.BackColor = System.Drawing.SystemColors.Window;
            this.flPanelHeader.Controls.Add(this.lblMessageHeader);
            this.flPanelHeader.Controls.Add(this.lblSender);
            this.flPanelHeader.Controls.Add(this.lblSendDate);
            this.flPanelHeader.Controls.Add(this.lblRecipient);
            this.flPanelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.flPanelHeader.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flPanelHeader.Location = new System.Drawing.Point(5, 5);
            this.flPanelHeader.MinimumSize = new System.Drawing.Size(0, 65);
            this.flPanelHeader.Name = "flPanelHeader";
            this.flPanelHeader.Size = new System.Drawing.Size(257, 69);
            this.flPanelHeader.TabIndex = 1;
            this.flPanelHeader.Paint += new System.Windows.Forms.PaintEventHandler(this.flPanelHeader_Paint);
            // 
            // lblMessageHeader
            // 
            this.lblMessageHeader.AutoSize = true;
            this.lblMessageHeader.Location = new System.Drawing.Point(3, 0);
            this.lblMessageHeader.Name = "lblMessageHeader";
            this.lblMessageHeader.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.lblMessageHeader.Size = new System.Drawing.Size(88, 19);
            this.lblMessageHeader.TabIndex = 4;
            this.lblMessageHeader.Text = "Message Header";
            // 
            // lblSender
            // 
            this.lblSender.AutoSize = true;
            this.lblSender.Location = new System.Drawing.Point(3, 19);
            this.lblSender.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.lblSender.Name = "lblSender";
            this.lblSender.Size = new System.Drawing.Size(33, 13);
            this.lblSender.TabIndex = 1;
            this.lblSender.Text = "From:";
            // 
            // lblSendDate
            // 
            this.lblSendDate.AutoSize = true;
            this.lblSendDate.Location = new System.Drawing.Point(3, 35);
            this.lblSendDate.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.lblSendDate.Name = "lblSendDate";
            this.lblSendDate.Size = new System.Drawing.Size(32, 13);
            this.lblSendDate.TabIndex = 2;
            this.lblSendDate.Text = "Sent:";
            // 
            // lblRecipient
            // 
            this.lblRecipient.AutoSize = true;
            this.lblRecipient.Location = new System.Drawing.Point(3, 51);
            this.lblRecipient.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.lblRecipient.Name = "lblRecipient";
            this.lblRecipient.Size = new System.Drawing.Size(23, 13);
            this.lblRecipient.TabIndex = 3;
            this.lblRecipient.Text = "To:";
            // 
            // ReadingPane
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.previewPanePanel);
            this.Name = "ReadingPane";
            this.Size = new System.Drawing.Size(267, 160);
            this.previewPanePanel.ResumeLayout(false);
            this.flPanelHeader.ResumeLayout(false);
            this.flPanelHeader.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel previewPanePanel;
        private System.Windows.Forms.Label lblMessageHeader;
        private System.Windows.Forms.Label lblSender;
        private System.Windows.Forms.Label lblSendDate;
        private System.Windows.Forms.Label lblRecipient;
        private System.Windows.Forms.WebBrowser wbMailBody;
        private System.Windows.Forms.FlowLayoutPanel flPanelHeader;

    }
}
