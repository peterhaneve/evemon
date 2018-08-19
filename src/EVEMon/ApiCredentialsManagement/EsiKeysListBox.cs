using EVEMon.Common.Constants;
using EVEMon.Common.Controls;
using EVEMon.Common.Factories;
using EVEMon.Common.Models;
using EVEMon.Common.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace EVEMon.ApiCredentialsManagement
{
    /// <summary>
    /// Displays a list of API keys.
    /// </summary>
    public sealed class EsiKeysListBox : NoFlickerListBox
    {
        private readonly Font m_smallFont;
        private readonly Font m_smallBoldFont;
        private readonly Font m_strikeoutFont;
        private readonly Font m_middleFont;
        private readonly Font m_boldFont;

        private readonly List<ESIKey> m_esiKeys = new List<ESIKey>();
        private bool m_pendingUpdate;

        /// <summary>
        /// Constructor.
        /// </summary>
        public EsiKeysListBox()
        {
            DrawMode = DrawMode.OwnerDrawFixed;
            DrawItem += OnDrawItem;
            
            m_smallFont = FontFactory.GetFont(Font.FontFamily, 6.5f);
            m_smallBoldFont = FontFactory.GetFont(m_smallFont, FontStyle.Bold);
            m_strikeoutFont = FontFactory.GetFont(m_smallFont, FontStyle.Strikeout);
            m_middleFont = FontFactory.GetFont(Font.FontFamily, 8.0f);
            m_boldFont = FontFactory.GetFont(Font, FontStyle.Bold);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="T:System.Windows.Forms.Control"/> 
        /// and its child controls and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            DrawItem -= OnDrawItem;
            base.Dispose(disposing);
        }

        /// <summary>
        /// Gets or sets the enumeration of displayed ESI keys.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public IEnumerable<ESIKey> ESIKeys
        {
            get { return m_esiKeys; }
            set
            {
                m_esiKeys.Clear();
                if (value != null)
                    m_esiKeys.AddRange(value);

                UpdateContent();
            }
        }

        /// <summary>
        /// Gets the size of the check box.
        /// </summary>
        /// <value>The size of the check box.</value>
        internal static Size CheckBoxSize => new Size(12, 12);

        /// <summary>
        /// Updates the content.
        /// </summary>
        private void UpdateContent()
        {
            if (!Visible)
            {
                m_pendingUpdate = true;
                return;
            }
            m_pendingUpdate = false;

            int scrollBarPosition = TopIndex;
            ESIKey oldSelection = SelectedItem as ESIKey;

            BeginUpdate();
            try
            {
                Items.Clear();
                foreach (ESIKey apiKey in ESIKeys)
                {
                    Items.Add(apiKey);
                    if (apiKey == oldSelection)
                        SelectedIndex = Items.Count - 1;
                }
            }
            finally
            {
                EndUpdate();
                TopIndex = scrollBarPosition;
            }
        }

        /// <summary>
        /// When the visibility changes, we check whether there is an update pending.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            if (Visible && m_pendingUpdate)
                UpdateContent();
        }

        /// <summary>
        /// Draws the ESI key info.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.DrawItemEventArgs"/> instance containing the event data.</param>
        private void OnDrawItem(object sender, DrawItemEventArgs e)
        {
            ItemHeight = m_boldFont.Height + m_middleFont.Height + m_smallBoldFont.Height * 2;

            // Background
            Graphics g = e.Graphics;
            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            Brush fontBrush = isSelected ? SystemBrushes.HighlightText : SystemBrushes.ControlText;
            e.DrawBackground();

            if (e.Index < 0 || e.Index >= Items.Count)
                return;

            ESIKey esiKey = (ESIKey)Items[e.Index];
            Image icon = GetIcon(esiKey);

            Margin = new Padding((ItemHeight - icon.Height) / 2);
            int left = e.Bounds.Left + Margin.Left;
            int top = e.Bounds.Top + Margin.Top / 4;

            // Draws the checbox
            CheckBoxRenderer.DrawCheckBox(g, new Point(left, (ItemHeight - CheckBoxSize.Height) / 2),
                                          esiKey.Monitored ? CheckBoxState.CheckedNormal : CheckBoxState.UncheckedNormal);
            left += CheckBoxSize.Width + Margin.Left * 2;

            // Draws the picture of the API key type
            g.DrawImage(icon, new Rectangle(left, top + Margin.Top, icon.Width, icon.Height));

            // Texts drawing
            DrawTexts(g, esiKey, left, top, fontBrush, icon);

            e.DrawFocusRectangle();
        }

        /// <summary>
        /// Draws the texts.
        /// </summary>
        /// <param name="top">The top.</param>
        /// <param name="g">The g.</param>
        /// <param name="esiKey">The ESI key.</param>
        /// <param name="left">The left.</param>
        /// <param name="fontBrush">The font brush.</param>
        /// <param name="icon">The icon.</param>
        private void DrawTexts(Graphics g, ESIKey esiKey, int left, int top, Brush fontBrush, Image icon)
        {
            // Draws the texts on the upper third
            left += icon.Width + Margin.Left;

            // Api key ID
            string apiKeyId = esiKey.ID.ToString(CultureConstants.InvariantCulture);
            g.DrawString(apiKeyId, m_boldFont, fontBrush, new Point(left, top + 2));
            int indentedLeft = left + g.MeasureString(apiKeyId, m_boldFont).ToSize().Width + Margin.Left * 2;

            // Api key verification code
            string tokenText = string.IsNullOrEmpty(esiKey.AccessToken) ? "No access token" : "Access token present";
            g.DrawString(tokenText, Font, fontBrush, new Point(indentedLeft, top));
            indentedLeft += g.MeasureString(esiKey.AccessToken, Font).ToSize().Width + Margin.Left * 2;

            // Draw the texts on the middle third
            top += g.MeasureString(apiKeyId, Font).ToSize().Height;

#if false
            // Account header
            string accountHeader = "Account";
            g.DrawString(accountHeader, m_boldFont, fontBrush, new Point(left, top));
            indentedLeft = left + g.MeasureString(accountHeader, m_boldFont).ToSize().Width + Margin.Left * 2;

            // Account created
            string accountCreatedText = esiKey.AccountCreated != DateTime.MinValue
                ? esiKey.AccountCreated.ToLocalTime().ToString(CultureConstants.DefaultCulture)
                : "-";
            string accountCreated = $"Created: {accountCreatedText}";

            g.DrawString(accountCreated, m_middleFont, fontBrush, new Point(indentedLeft, top));
            indentedLeft += g.MeasureString(accountCreated, m_middleFont).ToSize().Width + Margin.Left * 2;

            // Account paid until
            string accountPaidUntilText = esiKey.AccountExpires != DateTime.MinValue
                ? esiKey.AccountExpires.ToLocalTime().ToString(CultureConstants.DefaultCulture)
                : "-";
            string accountPaidUntil = $"Paid Until: {accountPaidUntilText}";
            g.DrawString(accountPaidUntil, m_middleFont, fontBrush, new Point(indentedLeft, top));
            indentedLeft += g.MeasureString(accountPaidUntil, m_middleFont).ToSize().Width + Margin.Left * 2;

            // Account status header
            string accountStatusHeader = "Status: ";   
            g.DrawString(accountStatusHeader, m_middleFont, fontBrush, new Point(indentedLeft, top));
            indentedLeft += g.MeasureString(accountStatusHeader, m_middleFont).ToSize().Width;

            // Account status body
            string accountStatusBody = esiKey.AccountExpires != DateTime.MinValue
                ? esiKey.AccountExpires > DateTime.UtcNow
                    ? "Active"
                    : "Expired"
                : "-";
            Brush accountStatusBrush = esiKey.AccountExpires != DateTime.MinValue
                ? new SolidBrush(esiKey.AccountExpires > DateTime.UtcNow ? Color.DarkGreen : Color.Red)
                : fontBrush;
            g.DrawString(accountStatusBody, m_middleFont, accountStatusBrush, new Point(indentedLeft, top));
            
            // Draws the texts on the lower third
            top += g.MeasureString(accountCreated, m_middleFont).ToSize().Height;
#endif
            bool isFirst = true;

            foreach (CharacterIdentity identity in esiKey.CharacterIdentities)
            {
                // Draws "; " between ids
                if (!isFirst)
                {
                    g.DrawString("; ", m_smallFont, fontBrush, new Point(left, top));
                    left += g.MeasureString("; ", Font).ToSize().Width;
                }
                isFirst = false;

                // Selects font
                Font font = m_smallFont;
                CCPCharacter ccpCharacter = identity.CCPCharacter;
                if (ccpCharacter != null && ccpCharacter.Monitored)
                    font = m_smallBoldFont;

                // Draws character's name
                g.DrawString(identity.CharacterName, font, fontBrush, new Point(left, top));
                left += g.MeasureString(identity.CharacterName, font).ToSize().Width;
            }
        }
        
        /// <summary>
        /// Gets the icon.
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <returns></returns>
        private static Image GetIcon(ESIKey apiKey)
        {
            Image icon;
            if (apiKey.HasError)
                icon = Resources.KeyWrong32;
            else
                icon = Resources.DefaultCharacterImage32;
            return icon;
        }
    }
}
