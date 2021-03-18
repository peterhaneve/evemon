using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using EVEMon.Common;
using EVEMon.Common.Controls;
using EVEMon.Common.CustomEventArgs;
using EVEMon.Common.Enumerations;
using EVEMon.Common.Enumerations.UISettings;
using EVEMon.Common.Extensions;
using EVEMon.Common.Factories;
using EVEMon.Common.Models;
using EVEMon.Common.Notifications;
using EVEMon.Common.SettingsObjects;

namespace EVEMon.Controls
{
    public partial class Overview : UserControl
    {
        public event EventHandler<CharacterChangedEventArgs> CharacterClicked;

        private bool m_grouping;
        private bool m_safeForWork;
        private PortraitSizes m_portraitSize;
        private bool m_showPortrait;


        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Overview()
        {
            InitializeComponent();

            labelNoCharacters.Font = FontFactory.GetFont("Tahoma", 11.25F, FontStyle.Bold);
            labelLoading.Font = FontFactory.GetFont("Tahoma", 11.25F, FontStyle.Bold);
            overviewLoadingThrobber.BringToFront();
        }

        #endregion


        #region Inherited Events

        /// <summary>
        /// On load, update the controls.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (DesignMode || this.IsDesignModeHosted())
                return;

            overviewLoadingThrobber.State = ThrobberState.Rotating;
            overviewLoadingThrobber.Show();

            DoubleBuffered = true;

            EveMonClient.MonitoredCharacterCollectionChanged += EveMonClient_MonitoredCharacterCollectionChanged;
            EveMonClient.CharacterUpdated += EveMonClient_CharacterUpdated;
            EveMonClient.SettingsChanged += EveMonClient_SettingsChanged;

            Disposed += OnDisposed;
        }

        /// <summary>
        /// On visibility, we may need to refresh the display.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            if (!DesignMode && !this.IsDesignModeHosted() && Visible)
                UpdateContent();
        }

        /// <summary>
        /// On disposing, unsubscribe events.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDisposed(object sender, EventArgs e)
        {
            EveMonClient.MonitoredCharacterCollectionChanged -= EveMonClient_MonitoredCharacterCollectionChanged;
            EveMonClient.CharacterUpdated -= EveMonClient_CharacterUpdated;
            EveMonClient.SettingsChanged -= EveMonClient_SettingsChanged;
            Disposed -= OnDisposed;
        }

        /// <summary>
        /// Adjust the layout on size change.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            PerformCustomLayout();
        }

        #endregion


        #region Content creation and layout

        /// <summary>
        /// Updates the overview content.
        /// </summary>
        private void UpdateContent()
        {
            // User has disabled the Overview
            if (!Settings.UI.MainWindow.ShowOverview || EveMonClient.MonitoredCharacters == null)
                return;

            List<OverviewItem> overviewItems = Controls.OfType<OverviewItem>().ToList();
            // Updates the visibility of the label for when no characters are loaded
            if (!EveMonClient.MonitoredCharacters.Any())
            {
                if (overviewItems.Count > 0)
                    CleanUp(overviewItems);

                labelNoCharacters.Show();

                return;
            }

            // Collect the existing overview items
            var items = overviewItems.ToDictionary(page => (Character)page.Tag);

            // Create the order we will layout the controls
            List<Character> characters = new List<Character>();

            if (m_grouping)
            {
                characters.AddRange(EveMonClient.MonitoredCharacters.Where(x => x.IsTraining));
                characters.AddRange(EveMonClient.MonitoredCharacters.Where(x => !x.IsTraining));
            }
            else
                characters.AddRange(EveMonClient.MonitoredCharacters);

            int index = 0;
            
            foreach (Character character in characters)
            {
                // Retrieve the current overview item, or null if we're past the limits
                OverviewItem currentOverviewItem = index < overviewItems.Count ? overviewItems[index] : null;

                // Does the overview item match with the character ?
                if ((Character)currentOverviewItem?.Tag != character)
                {
                    // Retrieve the overview item when it was previously created
                    // Is the overview item later in the collection ?
                    OverviewItem overviewItem;
                    if (items.TryGetValue(character, out overviewItem))
                        overviewItems.Remove(overviewItem); // Remove the overview item from old location
                    else
                        overviewItem = GetOverviewItem(character); // Create a new overview item

                    // Inserts the overview item in the proper location
                    overviewItems.Insert(index, overviewItem);
                }

                Int32 badgeCount = EveMonClient.Notifications.Where(p => character.Equals(p.SenderCharacter) && p.Category != NotificationCategory.MarketOrdersEnding).Distinct().Count();

                System.Diagnostics.Debug.WriteLine($"{character.Name} - {badgeCount}");
                Label badge = (Label)overviewItems[index].Controls.Find("badge", true)[0];

                badge.Visible = badgeCount != 0;
                badge.Text = $"{badgeCount}";

                MainWindowSettings mainWindowSettings = Settings.UI.MainWindow;
                PortraitSizes portraitSize = mainWindowSettings.OverviewItemSize;
                badge.Left = portraitSize.GetDefaultValue() - badge.Width + 3;
                // Remove processed character from the dictionary and move forward
                if (character != null)
                    items.Remove(character);

                index++;
            }

            // Remove the remaining items
            CleanUp(items.Values);
            foreach (OverviewItem item in items.Values)
            {
                overviewItems.Remove(item);
            }

            // Add the created items to the Overview
            Controls.AddRange(overviewItems.ToArray<Control>());

            PerformCustomLayout();
        }

        /// <summary>
        /// Updates from settings.
        /// </summary>
        private void UpdateFromSettings()
        {
            m_safeForWork = Settings.UI.SafeForWork;
            m_grouping = Settings.UI.MainWindow.PutTrainingSkillsFirstOnOverview;
            m_portraitSize = Settings.UI.MainWindow.OverviewItemSize;
            m_showPortrait = Settings.UI.MainWindow.ShowOverviewPortrait;

            // Update the controls
            UpdateContent();
        }

        /// <summary>
        /// Cleans up the existing controls.
        /// </summary>
        /// <param name="items">The items.</param>
        private void CleanUp(IEnumerable<OverviewItem> items)
        {
            // Dispose every one of the control to prevent event triggering
            foreach (OverviewItem item in items)
            {
                item.Click -= item_Click;
                item.Dispose();
            }

            // Clear the controls list
            Controls.Clear();
            Controls.Add(labelNoCharacters);
        }

        /// <summary>
        /// Gets the overview item.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <returns></returns>
        private OverviewItem GetOverviewItem(Character character)
        {
            OverviewItem overviewItem;
            OverviewItem tempOverviewItem = null;
            try
            {
                // Creates a new page
                tempOverviewItem = new OverviewItem(character);
                tempOverviewItem.Click += item_Click;
                tempOverviewItem.Clickable = true;
                tempOverviewItem.Tag = character;

                // Ensure that the control gets created before we add it,
                // (when Overview is created and then we hide a character,
                // the control gets created after the custom layout has been performed,
                // causing the controls to get misplaced)
                tempOverviewItem.CreateControl();

                overviewItem = tempOverviewItem;
                tempOverviewItem = null;
            }
            finally
            {
                tempOverviewItem?.Dispose();
            }

            return overviewItem;
        }

        /// <summary>
        /// Updates the number of rows and columns.
        /// </summary>
        /// <remarks>
        /// Cannot use a tableLayoutPanel in the end : too slow, too buggy.
        /// </remarks>
        private void PerformCustomLayout()
        {
            int clientWidth = ClientSize.Width, clientHeight = ClientSize.Height;
            if (!Visible || clientWidth < 1 || clientHeight < 1)
                return;

            IList<OverviewItem> overviewItems = Controls.OfType<OverviewItem>().ToList();

            // Check there is at least one control
            int numControls = overviewItems.Count;
            if (numControls == 0)
                return;

            const int Pad = 20;

            // Store and reset the scroll bar position
            int scrollBarPosition = VerticalScroll.Value;
            VerticalScroll.Value = 0;

            this.SuspendDrawing();

            SuspendLayout();

            try
            {
                // Retrieve the item width (should be the same for all controls) and compute the item and row width
                int itemWidth = overviewItems.Max(item => item.PreferredSize.Width);

                // Computes the number of columns and rows we need
                int numColumns = Math.Max(1, Math.Min(numControls, clientWidth / itemWidth));

                // Computes the horizontal margin
                int neededWidth = numColumns * (itemWidth + Pad) - Pad;
                int marginH = Math.Max(0, (clientWidth - neededWidth) / 2);

                // Measure the total height
                int rowIndex = 0;
                int rowHeight = 0;
                int height = 0;
                foreach (OverviewItem overviewItem in overviewItems)
                {
                    // Add the item to the row
                    rowHeight = Math.Max(rowHeight, overviewItem.PreferredSize.Height);
                    rowIndex++;

                    // Skip if row not complete yet
                    if (rowIndex != numColumns)
                        continue;

                    height += rowHeight + Pad;
                    rowHeight = 0;
                    rowIndex = 0;
                }

                // Computes the vertical margin
                height -= Pad;
                
                // We put 1/3 at the top, 2/3 at the bottom
                int marginV = Math.Max(0, (clientHeight - height) / 3); 

                // Adjust the controls bounds
                rowIndex = 0;
                rowHeight = 0;
                height = marginV;
                foreach (OverviewItem overviewItem in overviewItems)
                {
                    var size = overviewItem.PreferredSize;
                    // Set the control bound
                    overviewItem.SetBounds(marginH + rowIndex * (itemWidth + Pad), height, size.Width,
                        size.Height);
                    rowHeight = Math.Max(rowHeight, size.Height);
                    rowIndex++;

                    // Skip if row not complete yet
                    if (rowIndex != numColumns)
                        continue;

                    height += rowHeight + Pad;
                    rowHeight = 0;
                    rowIndex = 0;
                }

                labelNoCharacters.Visible = !EveMonClient.MonitoredCharacters.Any();

                base.AdjustFormScrollbars(true);
            }
            finally
            {
                ResumeLayout(false);

                this.ResumeDrawing();

                // Restore the scroll bar position
                VerticalScroll.Value = Math.Min(scrollBarPosition, VerticalScroll.Maximum);
            }
        }

        /// <summary>
        /// Gets a value indicating whether [overview settings changed].
        /// </summary>
        /// <value>
        /// <c>true</c> if [overview settings changed]; otherwise, <c>false</c>.
        /// </value>
        private bool OverviewSettingsChanged()
            => (m_grouping != Settings.UI.MainWindow.PutTrainingSkillsFirstOnOverview) ||
               (m_portraitSize != Settings.UI.MainWindow.OverviewItemSize) ||
               (m_showPortrait != Settings.UI.MainWindow.ShowOverviewPortrait) ||
               (m_safeForWork != Settings.UI.SafeForWork);

        #endregion


        #region Globals and locals events

        /// <summary>
        /// Occur when the monitored characters collection changed. We update the layout.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EveMonClient_MonitoredCharacterCollectionChanged(object sender, EventArgs e)
        {
            UpdateContent();
        }

        /// <summary>
        /// When aby character updates, we update the layout.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EVEMon.Common.CustomEventArgs.CharacterChangedEventArgs"/> instance containing the event data.</param>
        private void EveMonClient_CharacterUpdated(object sender, CharacterChangedEventArgs e)
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
            if (labelLoading.Visible)
            {
                overviewLoadingThrobber.State = ThrobberState.Stopped;
                overviewLoadingThrobber.Hide();
                labelLoading.Hide();
            }

            // Update only when settings that effect the overview have changed
            if (!OverviewSettingsChanged())
                return;

            // Force an update of each overview item before upating the content
            // This is mandatory in order to determine the overview items positioning
            Controls.OfType<OverviewItem>().ToList().ForEach(item => item.UpdateOnSettingsChanged());
            UpdateFromSettings();
        }

        /// <summary>
        /// When an item has been clicked, fires the appropriate event to notify the main window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void item_Click(object sender, EventArgs e)
        {
            OverviewItem item = sender as OverviewItem;

            if (item != null)
                CharacterClicked?.ThreadSafeInvoke(this, new CharacterChangedEventArgs(item.Character));
        }

        #endregion
    }
}
