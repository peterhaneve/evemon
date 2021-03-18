namespace EVEMon.Controls
{
    partial class OverviewItem
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
            this.pbCharacterPortrait = new EVEMon.Common.Controls.CharacterPortrait();
            this.badge = new System.Windows.Forms.Label();
            this.lblTotalSkillPoints = new EVEMon.Controls.OverviewLabel();
            this.lblExtraInfo = new EVEMon.Controls.OverviewLabel();
            this.lblSkillQueueTrainingTime = new EVEMon.Controls.OverviewLabel();
            this.lblCompletionTime = new EVEMon.Controls.OverviewLabel();
            this.lblCharName = new EVEMon.Controls.OverviewLabel();
            this.lblSkillInTraining = new EVEMon.Controls.OverviewLabel();
            this.lblRemainingTime = new EVEMon.Controls.OverviewLabel();
            this.lblBalance = new EVEMon.Controls.OverviewLabel();
            this.SuspendLayout();
            // 
            // pbCharacterPortrait
            // 
            this.pbCharacterPortrait.Character = null;
            this.pbCharacterPortrait.Enabled = false;
            this.pbCharacterPortrait.Location = new System.Drawing.Point(9, 11);
            this.pbCharacterPortrait.Name = "pbCharacterPortrait";
            this.pbCharacterPortrait.Size = new System.Drawing.Size(92, 92);
            this.pbCharacterPortrait.TabIndex = 0;
            this.pbCharacterPortrait.TabStop = false;
            // 
            // badge
            // 
            this.badge.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.badge.AutoSize = true;
            this.badge.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.badge.Location = new System.Drawing.Point(86, 86);
            this.badge.Margin = new System.Windows.Forms.Padding(0);
            this.badge.Name = "badge";
            this.badge.Size = new System.Drawing.Size(13, 13);
            this.badge.TabIndex = 9;
            this.badge.Text = "0";
            // 
            // lblTotalSkillPoints
            // 
            this.lblTotalSkillPoints.AutoEllipsis = true;
            this.lblTotalSkillPoints.BackColor = System.Drawing.Color.Transparent;
            this.lblTotalSkillPoints.Enabled = false;
            this.lblTotalSkillPoints.ForeColor = System.Drawing.Color.DimGray;
            this.lblTotalSkillPoints.Location = new System.Drawing.Point(107, 45);
            this.lblTotalSkillPoints.Name = "lblTotalSkillPoints";
            this.lblTotalSkillPoints.Size = new System.Drawing.Size(185, 14);
            this.lblTotalSkillPoints.TabIndex = 4;
            this.lblTotalSkillPoints.Text = "100,000,000 SP";
            // 
            // lblLocation
            // 
            this.lblExtraInfo.AutoEllipsis = true;
            this.lblExtraInfo.BackColor = System.Drawing.Color.Transparent;
            this.lblExtraInfo.Enabled = false;
            this.lblExtraInfo.ForeColor = System.Drawing.Color.DimGray;
            this.lblExtraInfo.Location = new System.Drawing.Point(9, 101);
            this.lblExtraInfo.Name = "lblLocation";
            this.lblExtraInfo.Size = new System.Drawing.Size(92, 13);
            this.lblExtraInfo.TabIndex = 1;
            this.lblExtraInfo.Text = "Egghelende";
            this.lblExtraInfo.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblSkillQueueTrainingTime
            // 
            this.lblSkillQueueTrainingTime.AutoEllipsis = true;
            this.lblSkillQueueTrainingTime.BackColor = System.Drawing.Color.Transparent;
            this.lblSkillQueueTrainingTime.Enabled = false;
            this.lblSkillQueueTrainingTime.ForeColor = System.Drawing.Color.DimGray;
            this.lblSkillQueueTrainingTime.Location = new System.Drawing.Point(107, 101);
            this.lblSkillQueueTrainingTime.Name = "lblSkillQueueTrainingTime";
            this.lblSkillQueueTrainingTime.Size = new System.Drawing.Size(215, 13);
            this.lblSkillQueueTrainingTime.TabIndex = 8;
            this.lblSkillQueueTrainingTime.Text = "Queue ends in 157d 23h 59m";
            // 
            // lblCompletionTime
            // 
            this.lblCompletionTime.AutoEllipsis = true;
            this.lblCompletionTime.BackColor = System.Drawing.Color.Transparent;
            this.lblCompletionTime.Enabled = false;
            this.lblCompletionTime.ForeColor = System.Drawing.Color.DimGray;
            this.lblCompletionTime.Location = new System.Drawing.Point(107, 88);
            this.lblCompletionTime.Name = "lblCompletionTime";
            this.lblCompletionTime.Size = new System.Drawing.Size(215, 13);
            this.lblCompletionTime.TabIndex = 7;
            this.lblCompletionTime.Text = "Mon 12/31/2017, 12:32:15 PM";
            // 
            // lblCharName
            // 
            this.lblCharName.AutoEllipsis = true;
            this.lblCharName.BackColor = System.Drawing.Color.Transparent;
            this.lblCharName.Enabled = false;
            this.lblCharName.Location = new System.Drawing.Point(107, 11);
            this.lblCharName.Name = "lblCharName";
            this.lblCharName.Size = new System.Drawing.Size(186, 14);
            this.lblCharName.TabIndex = 2;
            this.lblCharName.Text = "Character Name";
            // 
            // lblSkillInTraining
            // 
            this.lblSkillInTraining.AutoEllipsis = true;
            this.lblSkillInTraining.BackColor = System.Drawing.Color.Transparent;
            this.lblSkillInTraining.Enabled = false;
            this.lblSkillInTraining.ForeColor = System.Drawing.Color.DimGray;
            this.lblSkillInTraining.Location = new System.Drawing.Point(107, 75);
            this.lblSkillInTraining.Name = "lblSkillInTraining";
            this.lblSkillInTraining.Size = new System.Drawing.Size(215, 13);
            this.lblSkillInTraining.TabIndex = 6;
            this.lblSkillInTraining.Text = "Magnetometric Sensor Compensation III";
            // 
            // lblRemainingTime
            // 
            this.lblRemainingTime.AutoEllipsis = true;
            this.lblRemainingTime.BackColor = System.Drawing.Color.Transparent;
            this.lblRemainingTime.Enabled = false;
            this.lblRemainingTime.ForeColor = System.Drawing.Color.DimGray;
            this.lblRemainingTime.Location = new System.Drawing.Point(107, 61);
            this.lblRemainingTime.Name = "lblRemainingTime";
            this.lblRemainingTime.Size = new System.Drawing.Size(186, 14);
            this.lblRemainingTime.TabIndex = 5;
            this.lblRemainingTime.Text = "10d 12h 35m 24s";
            // 
            // lblBalance
            // 
            this.lblBalance.AutoEllipsis = true;
            this.lblBalance.BackColor = System.Drawing.Color.Transparent;
            this.lblBalance.Enabled = false;
            this.lblBalance.ForeColor = System.Drawing.Color.DimGray;
            this.lblBalance.Location = new System.Drawing.Point(107, 29);
            this.lblBalance.Name = "lblBalance";
            this.lblBalance.Size = new System.Drawing.Size(185, 14);
            this.lblBalance.TabIndex = 3;
            this.lblBalance.Text = "124,534,125,453.02 ISK";
            // 
            // OverviewItem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.badge);
            this.Controls.Add(this.lblTotalSkillPoints);
            this.Controls.Add(this.lblExtraInfo);
            this.Controls.Add(this.lblSkillQueueTrainingTime);
            this.Controls.Add(this.lblCompletionTime);
            this.Controls.Add(this.lblCharName);
            this.Controls.Add(this.lblSkillInTraining);
            this.Controls.Add(this.lblRemainingTime);
            this.Controls.Add(this.lblBalance);
            this.Controls.Add(this.pbCharacterPortrait);
            this.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Name = "OverviewItem";
            this.Size = new System.Drawing.Size(330, 120);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private EVEMon.Common.Controls.CharacterPortrait pbCharacterPortrait;
        private OverviewLabel lblTotalSkillPoints;
        private OverviewLabel lblCharName;
        private OverviewLabel lblSkillInTraining;
        private OverviewLabel lblRemainingTime;
        private OverviewLabel lblBalance;
        private OverviewLabel lblCompletionTime;
        private OverviewLabel lblSkillQueueTrainingTime;
        private OverviewLabel lblExtraInfo;
        private System.Windows.Forms.Label badge;
    }
}