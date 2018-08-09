using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using EVEMon.Common;
using EVEMon.Common.Constants;
using EVEMon.Common.Controls;
using EVEMon.Common.CustomEventArgs;
using EVEMon.Common.Data;
using EVEMon.Common.Enumerations;
using EVEMon.Common.Enumerations.CCPAPI;
using EVEMon.Common.Enumerations.UISettings;
using EVEMon.Common.Extensions;
using EVEMon.Common.Factories;
using EVEMon.Common.Interfaces;
using EVEMon.Common.Models;
using EVEMon.Common.Scheduling;
using EVEMon.Common.SettingsObjects;

namespace EVEMon.Controls
{
    /// <summary>
    /// Represents an item displayed on the overview.
    /// </summary>
    public partial class OverviewItem : UserControl
    {
        #region Fields

        private readonly bool m_isTooltip;

        private Color m_settingsForeColor;
        private bool m_showConflicts;
        private bool m_showSkillInTraining;
        private bool m_showCompletionTime;
        private bool m_showRemainingTime;
        private bool m_showWalletBalance;
        private bool m_showSkillpoints;
        private bool m_showPortrait;
        private bool m_showSkillQueueTrainingTime;
        private int m_portraitSize = 96;

        private bool m_hovered;
        private bool m_pressed;
        private int m_preferredWidth = 1;
        private int m_preferredHeight = 1;
        private int m_minWidth;

        private bool m_hasRemainingTime;
        private bool m_hasCompletionTime;
        private bool m_hasSkillInTraining;
        private bool m_hasSkillQueueTrainingTime;

        private float m_regularFontSize;
        private float m_mediumFontSize;
        private float m_bigFontSize;

        #endregion


        #region Constructors

        /// <summary>
        /// Default constructor for designer.
        /// </summary>
        private OverviewItem()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor used in-code.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <param name="isTooltip">if set to <c>true</c> if this instance is used as tooltip.</param>
        internal OverviewItem(Character character, bool isTooltip = false)
            : this()
        {
            m_isTooltip = isTooltip;
            Character = character;
        }

        #endregion


        #region Inherited Events

        /// <summary>
        /// Completes initialization.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Returns in design mode or when no char
            if (DesignMode || this.IsDesignModeHosted())
                return;

            DoubleBuffered = true;

            // Font sizes
            m_regularFontSize = 8.25F;
            m_mediumFontSize = 9.75F;
            m_bigFontSize = 11.25F;

            // Initializes fonts
            lblCharName.Font = FontFactory.GetFont("Tahoma", m_bigFontSize, FontStyle.Bold);
            lblBalance.Font = FontFactory.GetFont("Tahoma", m_mediumFontSize, FontStyle.Bold);
            lblRemainingTime.Font = FontFactory.GetFont("Tahoma", m_mediumFontSize);
            lblSkillInTraining.Font = FontFactory.GetFont("Tahoma", m_regularFontSize);
            lblCompletionTime.Font = FontFactory.GetFont("Tahoma", m_regularFontSize);
            lblSkillQueueTrainingTime.Font = FontFactory.GetFont("Tahoma", m_regularFontSize);

            // Initializes the portrait
            pbCharacterPortrait.Hide();
            pbCharacterPortrait.Character = Character;

            // Initialize the skill queue training time label text
            lblSkillQueueTrainingTime.Text = String.Empty;

            // Global events
            EveMonClient.CharacterSkillQueueUpdated += EveMonClient_CharacterSkillQueueUpdated;
            EveMonClient.QueuedSkillsCompleted += EveMonClient_QueuedSkillsCompleted;
            EveMonClient.MarketOrdersUpdated += EveMonClient_MarketOrdersUpdated;
            EveMonClient.CharacterUpdated += EveMonClient_CharacterUpdated;
            EveMonClient.SchedulerChanged += EveMonClient_SchedulerChanged;
            EveMonClient.SettingsChanged += EveMonClient_SettingsChanged;
            EveMonClient.TimerTick += EveMonClient_TimerTick;
            EveMonClient.CharacterLabelChanged += EveMonClient_CharacterLabelChanged;
            Disposed += OnDisposed;

            UpdateOnSettingsChanged();
        }

        /// <summary>
        /// On dispose, unsubscribe events.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDisposed(object sender, EventArgs e)
        {
            EveMonClient.CharacterSkillQueueUpdated -= EveMonClient_CharacterSkillQueueUpdated;
            EveMonClient.QueuedSkillsCompleted -= EveMonClient_QueuedSkillsCompleted;
            EveMonClient.MarketOrdersUpdated -= EveMonClient_MarketOrdersUpdated;
            EveMonClient.CharacterUpdated -= EveMonClient_CharacterUpdated;
            EveMonClient.SchedulerChanged -= EveMonClient_SchedulerChanged;
            EveMonClient.SettingsChanged -= EveMonClient_SettingsChanged;
            EveMonClient.TimerTick -= EveMonClient_TimerTick;
            EveMonClient.CharacterLabelChanged -= EveMonClient_CharacterLabelChanged;
            Disposed -= OnDisposed;
        }

        /// <summary>
        /// Occurs when the visibility changed.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            if (!Visible)
                return;

            UpdateContent();
            UpdateTrainingTime();
        }

        /// <summary>
        /// Paints a button behind when hovered.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (!m_hovered)
                return;

            ButtonRenderer.DrawButton(e.Graphics, DisplayRectangle, m_pressed
                ? PushButtonState.Pressed
                : PushButtonState.Hot);
        }

        /// <summary>
        /// When the mouse enters control, we need to display the back button.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);

            if (!Clickable)
                return;

            // Show back button
            m_hovered = true;
            Invalidate();
        }

        /// <summary>
        /// When the mouse leaves the control, we need to hide the button background.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            m_hovered = false;
            Invalidate();
        }

        /// <summary>
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"/> that contains the event data.</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            m_pressed = true;
            Invalidate();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseUp"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"/> that contains the event data.</param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            m_pressed = false;
            Invalidate();
        }

        #endregion


        #region Public Properties

        /// <summary>
        /// Gets the character control is bound to.
        /// </summary>
        public Character Character { get; }

        /// <summary>
        /// Gets or sets true whether a button should appear on hover.
        /// </summary>
        [Description("When true, a background button will appear on hover and the control will fire Click event")]
        public bool Clickable { get; set; }

        #endregion


        #region Content update

        /// <summary>
        /// Updates when settings changed.
        /// </summary>
        internal void UpdateOnSettingsChanged()
        {
            TrayPopupSettings trayPopupSettings = Settings.UI.SystemTrayPopup;
            MainWindowSettings mainWindowSettings = Settings.UI.MainWindow;
            PortraitSizes portraitSize = m_isTooltip
                ? trayPopupSettings.PortraitSize
                : mainWindowSettings.OverviewItemSize;

            // Misc fields
            m_portraitSize = portraitSize.GetDefaultValue();
            m_showConflicts = !m_isTooltip || trayPopupSettings.HighlightConflicts;
            m_showCompletionTime = !m_isTooltip || trayPopupSettings.ShowCompletionTime;
            m_showRemainingTime = !m_isTooltip || trayPopupSettings.ShowRemainingTime;
            m_showSkillInTraining = !m_isTooltip || trayPopupSettings.ShowSkillInTraining;
            m_showWalletBalance = m_isTooltip
                ? trayPopupSettings.ShowWallet
                : mainWindowSettings.ShowOverviewWallet;
            m_showSkillpoints = !m_isTooltip && mainWindowSettings.ShowOverviewTotalSkillpoints;
            m_showPortrait = m_isTooltip
                ? trayPopupSettings.ShowPortrait
                : mainWindowSettings.ShowOverviewPortrait;
            m_showSkillQueueTrainingTime = m_isTooltip
                ? trayPopupSettings.ShowSkillQueueTrainingTime
                : mainWindowSettings.ShowOverviewSkillQueueTrainingTime;

            // Update colors
            UpdateContrastColor();

            // Update the controls
            UpdateContent();
        }

        /// <summary>
        /// Updates the color of the contrast.
        /// </summary>
        private void UpdateContrastColor()
        {
            m_settingsForeColor =
                (m_isTooltip && Settings.UI.SystemTrayPopup.UseIncreasedContrast)
                || (!m_isTooltip && Settings.UI.MainWindow.UseIncreasedContrastOnOverview)
                    ? Color.Black
                    : Color.DimGray;

            lblBalance.ForeColor = m_settingsForeColor;
            lblTotalSkillPoints.ForeColor = m_settingsForeColor;
            lblRemainingTime.ForeColor = m_settingsForeColor;
            lblSkillInTraining.ForeColor = m_settingsForeColor;
            lblCompletionTime.ForeColor = m_settingsForeColor;
        }

        /// <summary>
        /// Update the controls.
        /// </summary>
        private void UpdateContent()
        {
            if (!Visible)
                return;

            // Update character's 'Adorned Name' and 'Portrait' in case they have changed
            lblCharName.Text = Character.LabelPrefix + Character.AdornedName;
            pbCharacterPortrait.Character = Character;
            lblTotalSkillPoints.Text = string.Format("{0:N0} SP", Character.SkillPoints);

            FormatBalance();

            CCPCharacter ccpCharacter = Character as CCPCharacter;
            QueuedSkill trainingSkill = Character.CurrentlyTrainingSkill;
            // Character in training ? We have labels to fill
            if (Character.IsTraining || (ccpCharacter != null && trainingSkill != null &&
                ccpCharacter.SkillQueue.IsPaused))
            {
                // Update the skill in training label
                lblSkillInTraining.Text = trainingSkill.ToString();
                DateTime endTime = trainingSkill.EndTime.ToLocalTime();

                // Updates the time remaining label
                lblRemainingTime.Text = (ccpCharacter != null && ccpCharacter.SkillQueue.
                    IsPaused) ? "Paused" : trainingSkill.RemainingTime.ToDescriptiveText(
                    DescriptiveTextOptions.IncludeCommas);

                // Update the completion time
                lblCompletionTime.Text = (ccpCharacter != null && ccpCharacter.SkillQueue.
                    IsPaused) ? string.Empty : $"{endTime:ddd} {endTime:G}";

                // Changes the completion time color on scheduling block
                string blockingEntry;
                bool isAutoBlocking;
                bool isBlocking = Scheduler.SkillIsBlockedAt(endTime, out blockingEntry,
                    out isAutoBlocking);
                lblCompletionTime.ForeColor = (m_showConflicts && isBlocking &&
                    (ccpCharacter == null || ccpCharacter.SkillQueue.Count == 1 ||
                    !isAutoBlocking)) ? Color.Red : m_settingsForeColor;

                // Update the skill queue training time label
                UpdateSkillQueueTrainingTime();

                // Show the training labels
                m_hasSkillInTraining = true;
                m_hasCompletionTime = true;
                m_hasRemainingTime = true;
                m_hasSkillQueueTrainingTime = true;
            }
            else
            {
                // Hide the training labels
                m_hasSkillInTraining = false;
                m_hasCompletionTime = false;
                m_hasRemainingTime = false;
                m_hasSkillQueueTrainingTime = false;
            }

            // Adjusts all the controls layout
            PerformCustomLayout(m_isTooltip);
        }

        /// <summary>
        /// Formats the balance.
        /// </summary>
        private void FormatBalance()
        {
            lblBalance.Text = $"{Character.Balance:N} ISK";

            CCPCharacter ccpCharacter = Character as CCPCharacter;

            if (ccpCharacter == null)
                return;

            IQueryMonitor marketMonitor = ccpCharacter.QueryMonitors[ESIAPICharacterMethods.MarketOrders];
            if (!Settings.UI.SafeForWork && !ccpCharacter.HasSufficientBalance && marketMonitor != null && marketMonitor.Enabled)
            {
                lblBalance.ForeColor = Color.Orange;
                return;
            }

            lblBalance.ForeColor = !Settings.UI.SafeForWork && ccpCharacter.Balance < 0 ? Color.Red : m_settingsForeColor;
        }

        /// <summary>
        /// Updates the controls' visibility.
        /// </summary>
        /// <returns></returns>
        private void UpdateVisibilities()
        {
            lblRemainingTime.Visible = m_hasRemainingTime & m_showRemainingTime;
            lblCompletionTime.Visible = m_hasCompletionTime & m_showCompletionTime;
            lblSkillInTraining.Visible = m_hasSkillInTraining & m_showSkillInTraining;
            lblSkillQueueTrainingTime.Visible = m_hasSkillQueueTrainingTime & m_showSkillQueueTrainingTime;
            lblBalance.Visible = m_showWalletBalance;
            lblTotalSkillPoints.Visible = m_showSkillpoints;
        }

        /// <summary>
        /// Updates the training time.
        /// </summary>
        private void UpdateTrainingTime()
        {
            if (!Visible || !Character.IsTraining)
                return;

            TimeSpan remainingTime = Character.CurrentlyTrainingSkill.RemainingTime;
            lblRemainingTime.Text = remainingTime.ToDescriptiveText(DescriptiveTextOptions.IncludeCommas);

            UpdateSkillQueueTrainingTime();
        }

        /// <summary>
        /// Updates the skill queue training time.
        /// </summary>
        /// <returns></returns>
        private void UpdateSkillQueueTrainingTime()
        {
            CCPCharacter ccpCharacter = Character as CCPCharacter;

            // Current character isn't a CCP character, so can't have a Queue
            if (ccpCharacter == null)
                return;

            if (ccpCharacter.SkillQueue.IsPaused)
            {
                lblSkillQueueTrainingTime.Text = String.Empty;
                return;
            }

            TimeSpan skillQueueEndTime = ccpCharacter.SkillQueue.EndTime.Subtract(DateTime.UtcNow);
            TimeSpan timeLeft = SkillQueue.WarningThresholdTimeSpan.Subtract(skillQueueEndTime);

            // Negative time ?
            // Skill queue is populated with more than a day
            if (timeLeft < TimeSpan.Zero)
            {
                // More than one entry in queue ? Display total queue remaining time
                if (ccpCharacter.SkillQueue.Count > 1)
                {
                    lblSkillQueueTrainingTime.ForeColor = m_settingsForeColor;
                    lblSkillQueueTrainingTime.Text =
                        $"Queue ends in {ccpCharacter.SkillQueue.EndTime.ToRemainingTimeShortDescription(DateTimeKind.Utc)}";

                    return;
                }

                // We don't display anything
                lblSkillQueueTrainingTime.Text = String.Empty;
                return;
            }

            // Training completed ?
            if (timeLeft == TimeSpan.Zero)
            {
                // We don't display anything
                lblSkillQueueTrainingTime.Text = String.Empty;
                return;
            }

            // Skill queue is empty ?
            if (timeLeft > SkillQueue.WarningThresholdTimeSpan)
            {
                lblSkillQueueTrainingTime.Text = @"Skill queue is empty";
                return;
            }

            // Less than one minute ? Display seconds else display time without seconds
            string endTimeText = skillQueueEndTime < TimeSpan.FromMinutes(1)
                ? skillQueueEndTime.ToDescriptiveText(DescriptiveTextOptions.IncludeCommas)
                : skillQueueEndTime.ToDescriptiveText(DescriptiveTextOptions.IncludeCommas, includeSeconds: false);

            lblSkillQueueTrainingTime.ForeColor = Color.Red;
            lblSkillQueueTrainingTime.Text = $"Queue ends in {endTimeText}";
        }

        #endregion


        #region Global Events

        /// <summary>
        /// When a character label is changed, we update if it is our character.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EveMonClient_CharacterLabelChanged(object sender, LabelChangedEventArgs e)
        {
            if (e.Character == Character)
                UpdateContent();
        }

        /// <summary>
        /// On every second, we update the remaining time.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EveMonClient_TimerTick(object sender, EventArgs e)
        {
            UpdateTrainingTime();
        }

        /// <summary>
        /// When the scheduler changed, we may have to display a warning (blocking entry).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EveMonClient_SchedulerChanged(object sender, EventArgs e)
        {
            UpdateContent();
        }

        /// <summary>
        /// When the settings changed, update if necessary.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EveMonClient_SettingsChanged(object sender, EventArgs e)
        {
            UpdateOnSettingsChanged();
        }

        /// <summary>
        /// On skill completion.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EveMonClient_QueuedSkillsCompleted(object sender, QueuedSkillsEventArgs e)
        {
            if (e.Character != Character)
                return;

            // Character still training ? Jump to next skill
            if (Character.IsTraining)
                UpdateContent();
            else
            {
                lblRemainingTime.Text = @"Completed";
                m_hasCompletionTime = false;
                UpdateVisibilities();
            }
        }

        /// <summary>
        /// On character market orders updated, update the balance format.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EVEMon.Common.CustomEventArgs.CharacterChangedEventArgs"/> instance containing the event data.</param>
        private void EveMonClient_MarketOrdersUpdated(object sender, CharacterChangedEventArgs e)
        {
            if (e.Character != Character)
                return;

            FormatBalance();
        }

        /// <summary>
        /// On character sheet changed, update everything.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EveMonClient_CharacterUpdated(object sender, CharacterChangedEventArgs e)
        {
            if (e.Character != Character)
                return;

            UpdateContent();
        }

        /// <summary>
        /// On character skill queue changed, update everything.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EveMonClient_CharacterSkillQueueUpdated(object sender, CharacterChangedEventArgs e)
        {
            if (e.Character != Character)
                return;

            UpdateContent();
        }

        #endregion


        #region Layout

        /// <summary>
        /// Adjusts all the controls layout.
        /// </summary>
        /// <param name="tooltip"></param>
        private void PerformCustomLayout(bool tooltip)
        {
            if (!Visible)
                return;

            SuspendLayout();

            UpdateVisibilities();

            bool showPortrait = m_showPortrait && !Settings.UI.SafeForWork;
            int portraitSize = m_portraitSize;

            int margin = 10;
            if (tooltip)
            {
                margin = portraitSize <= PortraitSizes.x48.GetDefaultValue()
                    ? 2
                    : portraitSize <= PortraitSizes.x64.GetDefaultValue()
                        ? 4
                        : portraitSize <= PortraitSizes.x80.GetDefaultValue()
                            ? 6
                            : margin;
            }

            // Label height
            int smallLabelHeight = 13;
            int labelHeight = portraitSize <= PortraitSizes.x48.GetDefaultValue()
                ? smallLabelHeight
                : portraitSize <= PortraitSizes.x64.GetDefaultValue()
                    ? 16
                    : 18;

            // Label width
            int labelWidth = !tooltip
                ? (int)(GetMinimumWidth() * (Graphics.FromHwnd(Handle).DpiX / EveMonConstants.DefaultDpi))
                : 0;

            // Big font size
            float bigFontSize = portraitSize <= PortraitSizes.x48.GetDefaultValue()
                ? m_regularFontSize
                : portraitSize <= PortraitSizes.x64.GetDefaultValue()
                    ? m_mediumFontSize
                    : m_bigFontSize;

            // Medium font size
            float mediumFontSize = portraitSize <= PortraitSizes.x64.GetDefaultValue()
                ? m_regularFontSize
                : m_mediumFontSize;

            // Margin between the two labels groups
            int verticalMargin = m_showSkillQueueTrainingTime ? 4 : 16;
            if (portraitSize <= PortraitSizes.x80.GetDefaultValue())
                verticalMargin = 0;

            // Adjust portrait
            pbCharacterPortrait.Location = new Point(margin, margin);
            pbCharacterPortrait.Size = new Size(portraitSize, portraitSize);
            pbCharacterPortrait.Visible = showPortrait;

            // Adjust the top labels
            int top = margin - 2;
            int left = showPortrait ? portraitSize + margin * 2 : margin;
            int rightPad = tooltip ? 10 : 0;

            Size size = GetSizeForLabel(lblCharName, bigFontSize, left, top, rightPad, labelWidth, labelHeight);
            labelWidth = size.Width;
            labelHeight = size.Height;
            top += labelHeight;

            if (lblBalance.Visible)
            {
                size = GetSizeForLabel(lblBalance, mediumFontSize, left, top, rightPad, labelWidth, labelHeight);
                labelWidth = size.Width;
                labelHeight = size.Height;
                top += labelHeight;
            }

            if (lblTotalSkillPoints.Visible)
            {
                size = GetSizeForLabel(lblTotalSkillPoints, mediumFontSize, left, top, rightPad, labelWidth, labelHeight);
                labelWidth = size.Width;
                labelHeight = size.Height;
                top += labelHeight;
            }

            if (lblRemainingTime.Visible || lblSkillInTraining.Visible || lblCompletionTime.Visible)
                top += verticalMargin;

            if (lblRemainingTime.Visible)
            {
                size = GetSizeForLabel(lblRemainingTime, mediumFontSize, left, top, rightPad, labelWidth, labelHeight);
                labelWidth = size.Width;
                labelHeight = size.Height;
                top += labelHeight;
            }

            if (lblSkillInTraining.Visible)
            {
                size = GetSizeForLabel(lblSkillInTraining, m_regularFontSize, left, top, rightPad, labelWidth, smallLabelHeight);
                labelWidth = size.Width;
                smallLabelHeight = size.Height;
                top += smallLabelHeight;
            }

            if (lblCompletionTime.Visible)
            {
                size = GetSizeForLabel(lblCompletionTime, m_regularFontSize, left, top, rightPad, labelWidth, smallLabelHeight);
                labelWidth = size.Width;
                smallLabelHeight = size.Height;
                top += smallLabelHeight;
            }

            if (lblSkillQueueTrainingTime.Visible)
            {
                size = GetSizeForLabel(lblSkillQueueTrainingTime, m_regularFontSize, left, top, rightPad, labelWidth,
                    smallLabelHeight);
                labelWidth = size.Width;
                smallLabelHeight = size.Height;
                top += smallLabelHeight;
            }

            Width = m_preferredWidth = left + labelWidth + margin;
            Height = m_preferredHeight = pbCharacterPortrait.Visible
                ? Math.Max(pbCharacterPortrait.Height + 2 * margin, top + margin)
                : top + margin;

            ResumeLayout(false);
        }

        /// <summary>
        /// Gets the minimum width.
        /// </summary>
        /// <returns></returns>
        private int GetMinimumWidth()
        {
            if (m_minWidth != 0)
                return m_minWidth;

            int longestSkillNameLength = StaticSkills.AllSkills.Max(skill => skill.Name.Length);
            StaticSkill longestSkill = StaticSkills.AllSkills.First(skill => skill.Name.Length == longestSkillNameLength);

            return m_minWidth = (int)Graphics.FromHwnd(Handle)
                .MeasureString($"{longestSkill.Name} {Skill.GetRomanFromInt(3)}", FontFactory.GetFont("Tahoma", m_regularFontSize))
                .Width;
        }

        /// <summary>
        /// Gets the size for the specified label.
        /// </summary>
        /// <param name="label">The label.</param>
        /// <param name="fontSize">Size of the font.</param>
        /// <param name="left">The left.</param>
        /// <param name="top">The top.</param>
        /// <param name="rightPad">The right pad.</param>
        /// <param name="labelWidth">Width of the label.</param>
        /// <param name="labelHeight">Height of the label.</param>
        /// <returns></returns>
        private static Size GetSizeForLabel(Label label, float fontSize, int left, int top, int rightPad, int labelWidth,
            int labelHeight)
        {
            Font font = FontFactory.GetFont(label.Font.FontFamily, fontSize, label.Font.Style);
            label.Font = font;
            label.Location = new Point(left, top);
            labelWidth = Math.Max(labelWidth, label.PreferredWidth + rightPad);
            labelHeight = Math.Max(labelHeight, font.Height);
            label.Size = new Size(labelWidth, labelHeight);
            return label.Size;
        }

        /// <summary>
        /// Gets the preferred size for control. Used by parents to decide which size they will grant to their children.
        /// </summary>
        /// <param name="proposedSize"></param>
        /// <returns></returns>
        public override Size GetPreferredSize(Size proposedSize) => new Size(m_preferredWidth, m_preferredHeight);

        #endregion
    }
}
