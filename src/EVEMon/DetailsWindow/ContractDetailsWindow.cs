using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using EVEMon.Common.Constants;
using EVEMon.Common.Controls;
using EVEMon.Common.Data;
using EVEMon.Common.Enumerations;
using EVEMon.Common.Enumerations.CCPAPI;
using EVEMon.Common.Extensions;
using EVEMon.Common.Factories;
using EVEMon.Common.Helpers;
using EVEMon.Common.Models;
using EVEMon.Common.Models.Comparers;
using EVEMon.SkillPlanner;
using Region = EVEMon.Common.Data.Region;

namespace EVEMon.DetailsWindow
{
    public sealed partial class ContractDetailsWindow : EVEMonForm
    {
        #region Fields

        private readonly Contract m_contract;
        private readonly Size m_startingSize;
        private readonly SolarSystem m_characterLastSolarSystem;
        private readonly IEnumerable<ContractItem> m_contractItems;

        private ContractItemsListView m_lvIncludedItems;
        private ContractItemsListView m_lvNotIncludedItems;
        private IEnumerable<SolarSystem> m_characterLastLocationToStartRoute;
        private IEnumerable<SolarSystem> m_characterLastLocationToEndRoute;
        private IEnumerable<SolarSystem> m_startToEndRoute;
        private IEnumerable<SolarSystem> m_oldRoute;
        private IEnumerable<SolarSystem> m_route;
        private Font m_boldFont;
        private Font m_mediumBoldFont;
        private Font m_bigBoldFont;
        private bool m_buttonSwitch;
        private int m_height;

        private const int Pad = 3;
        private const int FirstIntendPosition = 9;
        private const int SecondIndentPosition = 110;
        private const int ListViewHeight = 120;

        #endregion


        #region Constructors

        /// <summary>
        /// Private Constructor.
        /// </summary>
        private ContractDetailsWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="contract">The contract.</param>
        /// <exception cref="System.ArgumentNullException">contract</exception>
        public ContractDetailsWindow(Contract contract)
            : this()
        {
            contract.ThrowIfNull(nameof(contract));

            RememberPositionKey = "ContractDetailsWindow";
            m_startingSize = new Size(Width, Height);
            m_contract = contract;
            m_contractItems = contract.ContractItems;
            m_characterLastSolarSystem = m_contract.Character.LastKnownSolarSystem;
        }

        #endregion


        #region Inherited Events

        /// <summary>
        /// On load, completes controls initialization.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            RoutePanelParent.Width = 160;
            RoutePanelParent.BorderStyle = BorderStyle.FixedSingle;
            Size = m_startingSize;

            ButtonPanel.Visible = m_contract.ContractType == ContractType.Auction &&
                m_contract.ContractBids.Any();

            m_boldFont = FontFactory.GetDefaultFont(FontStyle.Bold);
            m_mediumBoldFont = FontFactory.GetDefaultFont(9.25f, FontStyle.Bold);
            m_bigBoldFont = FontFactory.GetDefaultFont(10.25f, FontStyle.Bold);

            // Initialize a control for the contract's outgoing items
            if (m_contractItems.Any(x => x.Included) && m_contractItems.Count(x => x.Included) > 1)
            {
                m_lvIncludedItems = new ContractItemsListView(m_contract.Character, m_contractItems.Where(x => x.Included))
                {
                    SmallImageList = ImageListIcons
                };
                Controls.Add(m_lvIncludedItems);
            }

            // Initialize a control for the contract's incoming items
            if (m_contractItems.Any(x => !x.Included))
            {
                m_lvNotIncludedItems = new ContractItemsListView(m_contract.Character, m_contractItems.Where(x => !x.Included))
                {
                    SmallImageList = ImageListIcons
                };
                Controls.Add(m_lvNotIncludedItems);
            }

            // Form has fixed size if it doesn't contain any list
            if (!Controls.OfType<ContractItemsListView>().Any())
                FormBorderStyle = FormBorderStyle.FixedDialog;

            // Adjust the control's height to our needs
            if (!m_contract.IsTrading)
                return;

            Height += ListViewHeight;
            MinimumSize = new Size(Width, Height);
        }


        #endregion


        #region Private Properties

        /// <summary>
        /// Gets the availability info.
        /// </summary>
        /// <value>The availability info.</value>
        private string AvailabilityInfo
        {
            get
            {
                Region startStationRegion = m_contract.StartStation.SolarSystemChecked.
                    Constellation.Region;
                Region characterLastKnownRegion = m_characterLastSolarSystem != null ?
                    m_characterLastSolarSystem.Constellation.Region : null;
                string destinationRegionText = (characterLastKnownRegion != null) ?
                    ((characterLastKnownRegion == startStationRegion) ? "(Current Region)" :
                    "(Other Region)") : string.Empty;
                string secondHalfText = m_contract.Availability == ContractAvailability.Private ?
                    m_contract.Assignee : $"Region: {startStationRegion.Name}  {destinationRegionText}";
                return $"{m_contract.Availability.GetDescription()} - {secondHalfText}";
            }
        }

        /// <summary>
        /// Gets the start to end route.
        /// </summary>
        /// <value>The start to end route.</value>
        private IEnumerable<SolarSystem> GetStartToEndRoute => m_startToEndRoute ??
            (m_startToEndRoute = m_contract.StartStation.SolarSystemChecked.GetFastestPathTo(
            m_contract.EndStation.SolarSystemChecked, PathSearchCriteria.FewerJumps));

        /// <summary>
        /// Gets the character last location to start route.
        /// </summary>
        /// <value>The character last location to start route.</value>
        private IEnumerable<SolarSystem> GetCharacterLastLocationToStartRoute =>
            m_characterLastLocationToStartRoute ?? (m_characterLastLocationToStartRoute =
            m_characterLastSolarSystem.GetFastestPathTo(m_contract.StartStation.
            SolarSystemChecked, PathSearchCriteria.FewerJumps));

        /// <summary>
        /// Gets the character last location to end route.
        /// </summary>
        /// <value>The character last location to end route.</value>
        private IEnumerable<SolarSystem> GetCharacterLastLocationToEndRoute =>
            m_characterLastLocationToEndRoute ?? (m_characterLastLocationToEndRoute =
            m_characterLastSolarSystem.GetFastestPathTo(m_contract.EndStation.
            SolarSystemChecked, PathSearchCriteria.FewerJumps));

        #endregion


        #region Details Drawing Methods

        /// <summary>
        /// Handles the Paint event of the DetailsPanel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.PaintEventArgs"/> instance containing the event data.</param>
        private void DetailsPanel_Paint(object sender, PaintEventArgs e)
        {
            // Draw the header section
            DrawHeader(e);

            // Draw the generic info section
            DrawGenericInfo(e);

            // Draw the courier info section
            if (m_contract.ContractType == ContractType.Courier)
            {
                DrawCourierInfo(e);
                return;
            }

            // Draw the auction info section
            DrawAuctionInfo(e);

            // Draw the item exchange info section
            DrawPriceInfo(e);

            // Draw the contract's items info section
            DrawItemsInfo(e);
        }

        /// <summary>
        /// Draws the items info.
        /// </summary>
        /// <param name="e">The <see cref="System.Windows.Forms.PaintEventArgs"/> instance containing the event data.</param>
        private void DrawItemsInfo(PaintEventArgs e)
        {
            m_height += Pad;

            string exclamation = m_contract.IssuerID == m_contract.Character.CharacterID ? "Buyer" : "You";

            if (m_contractItems.Any(x => x.Included))
            {
                // Draw the header text
                DrawColoredText(e, $"{exclamation} Will Get", m_mediumBoldFont,
                    new Point(FirstIntendPosition, m_height), Color.Green);

                if (m_contractItems.Count(x => x.Included) > 1)
                    DisplayListView(m_lvIncludedItems);
                else
                {
                    ContractItem contractItem = m_contractItems.First();

                    // Display the item's image
                    m_height += Pad * 2;
                    int left = DetailsPanel.Left + FirstIntendPosition + Pad * 2;
                    ItemImage.EveItem = contractItem.Item;
                    ItemImage.Location = new Point(left, m_height);
                    ItemImage.BringToFront();
                    ItemImage.Visible = true;

                    // Draw the item's name and quantity
                    m_height += Pad * 2;
                    string itemText = $"{contractItem.Item.Name} x {contractItem.Quantity:N0}";
                    Size itemTextSize = e.Graphics.MeasureString(itemText, m_bigBoldFont).ToSize();
                    int itemTextHeight = itemTextSize.Width < DetailsPanel.Width - SecondIndentPosition
                        ? itemTextSize.Height
                        : itemTextSize.Height * 2;
                    e.Graphics.DrawString(itemText, m_bigBoldFont, Brushes.Black,
                        new Rectangle(left + ItemImage.Width + Pad * 2, m_height,
                            DetailsPanel.Width - SecondIndentPosition, itemTextHeight));

                    m_height += itemTextHeight;
                    int position = left + ItemImage.Width + Pad * 2 - DetailsPanel.Left - SecondIndentPosition;

                    // Draw the item's category and group
                    if (!String.IsNullOrEmpty(contractItem.Item.CategoryName) &&
                        !String.IsNullOrEmpty(contractItem.Item.GroupName))
                    {
                        string itemCategoryGroup = $"{contractItem.Item.CategoryName}  /  {contractItem.Item.GroupName}";
                        DrawText(e, String.Empty, itemCategoryGroup, m_mediumBoldFont, true, position);
                    }

                    // Draw additional type info when item is a blueprint
                    if (contractItem.RawQuantity < 0 &&
                        contractItem.Item.MarketGroup.BelongsIn(DBConstants.BlueprintsMarketGroupID))
                    {
                        string itemTypeText = $"BLUEPRINT {(contractItem.RawQuantity == -2 ? "COPY" : "ORIGINAL")}";
                        DrawText(e, String.Empty, itemTypeText, m_boldFont, true, position);
                    }

                    m_height += ItemImage.Height / 2;
                }
            }

            if (m_contractItems.All(x => x.Included))
                return;

            // Draw the header text
            DrawColoredText(e, $"{exclamation} Will Give", m_mediumBoldFont,
                new Point(FirstIntendPosition, m_height), Color.Red);

            // Display the item's in a list view
            DisplayListView(m_lvNotIncludedItems);
        }

        /// <summary>
        /// Displays the list view.
        /// </summary>
        /// <param name="listView">The list view.</param>
        private void DisplayListView(Control listView)
        {
            m_height += Pad;
            listView.Location = new Point(FirstIntendPosition, m_height);
            listView.BringToFront();
            listView.Visible = true;

            int listViewHeight = m_contract.IsTrading && listView == m_lvIncludedItems
                ? (DetailsPanel.Height - m_height - Pad * 2) / 2
                : DetailsPanel.Height - m_height - Pad * 2;
            listView.Size = new Size(DetailsPanel.Width - FirstIntendPosition * 2, listViewHeight);

            m_height += listView.Height + Pad;
        }

        /// <summary>
        /// Draws the price info.
        /// </summary>
        /// <param name="e">The <see cref="System.Windows.Forms.PaintEventArgs"/> instance containing the event data.</param>
        private void DrawPriceInfo(PaintEventArgs e)
        {
            if (m_contract.ContractType != ContractType.ItemExchange || (m_contract.Price == 0 && m_contract.Reward == 0))
                return;

            m_height += Pad;

            string labelText = $"{(m_contract.IssuerID == m_contract.Character.CharacterID ? "Buyer" : "You")} " +
                               $"Will {(m_contract.Price > 0 ? "Pay" : "Get")}";

            if (m_contract.Price > 0)
            {
                string priceText = String.Format(CultureConstants.DefaultCulture, GetNumberFormat(m_contract.Price),
                    m_contract.Price,
                    m_contract.Price < 10000M
                        ? String.Empty
                        : $" ({FormatHelper.Format(m_contract.Price, AbbreviationFormat.AbbreviationWords, false)})");
                DrawText(e, labelText, String.Empty, m_mediumBoldFont, false);
                DrawColoredText(e, priceText, m_mediumBoldFont, new Point(SecondIndentPosition, m_height), Color.Red);
            }
            else
            {
                string rewardText = String.Format(CultureConstants.DefaultCulture, GetNumberFormat(m_contract.Reward),
                    m_contract.Reward,
                    m_contract.Reward < 10000M
                        ? String.Empty
                        : $" ({FormatHelper.Format(m_contract.Reward, AbbreviationFormat.AbbreviationWords, false)})");
                DrawText(e, labelText, String.Empty, m_mediumBoldFont, false);
                DrawColoredText(e, rewardText, m_mediumBoldFont, new Point(SecondIndentPosition, m_height), Color.Green);
            }

            // Draw the lower line
            DrawLine(e);
        }

        /// <summary>
        /// Draws the auction info.
        /// </summary>
        /// <param name="e">The <see cref="System.Windows.Forms.PaintEventArgs"/> instance containing the event data.</param>
        private void DrawAuctionInfo(PaintEventArgs e)
        {
            if (m_contract.ContractType != ContractType.Auction)
                return;

            m_height += Pad;

            string text = string.Format(CultureConstants.DefaultCulture, GetNumberFormat(m_contract.Price),
                m_contract.Price, m_contract.Price < 10000M ? string.Empty :
                $" ({FormatHelper.Format(m_contract.Price, AbbreviationFormat.AbbreviationWords, false)})");
            DrawText(e, "Starting Bid", text, Font);

            text = m_contract.Buyout == 0 ? "(None)" : string.Format(CultureConstants.DefaultCulture, GetNumberFormat(m_contract.Buyout),
                m_contract.Buyout, m_contract.Price < 10000M ? string.Empty
                : $" ({FormatHelper.Format(m_contract.Buyout, AbbreviationFormat.AbbreviationWords, false)})");
            DrawText(e, "Buyout Price", text, Font);

            decimal amount = m_contract.ContractBids.Select(bid => bid.Amount).Concat(new[] { 0M }).Max();
            int numberOfBids = m_contract.ContractBids.Count();
            text = numberOfBids == 0 ? "No Bids" :
                string.Format(CultureConstants.DefaultCulture, GetNumberFormat(amount), amount, string.Empty) +
                $" ({numberOfBids} bid{(numberOfBids.S())} so far)";

            DrawText(e, "Current Bid", text, Font);

            text = m_contract.IsAvailable ? m_contract.Expiration.
                ToRemainingTimeShortDescription(DateTimeKind.Utc) : m_contract.State.ToString();

            Color color = (m_contract.IsAvailable && m_contract.Expiration < DateTime.UtcNow.AddDays(1)) ?
                Color.DarkOrange : (m_contract.State == ContractState.Expired ? Color.Red : ForeColor);

            DrawText(e, "Time Left", string.Empty, Font, false);
            DrawColoredText(e, text, Font, new Point(SecondIndentPosition, m_height), color);

            // Draw the lower line
            DrawLine(e);
        }

        /// <summary>
        /// Draws the courier info.
        /// </summary>
        /// <param name="e">The <see cref="System.Windows.Forms.PaintEventArgs"/> instance containing the event data.</param>
        private void DrawCourierInfo(PaintEventArgs e)
        {
            m_height += Pad;

            if (m_contract.Accepted == DateTime.MinValue)
            {
                DrawText(e, "Complete In", $"{m_contract.DaysToComplete} Day{(m_contract.DaysToComplete.S())}", Font);
            }
            else
            {
                DateTime timeToComplete = m_contract.Accepted.AddDays(m_contract.DaysToComplete);
                string timeToCompleteText = timeToComplete.Subtract(DateTime.UtcNow).
                    ToDescriptiveText(DescriptiveTextOptions.SpaceText |
                    DescriptiveTextOptions.FullText | DescriptiveTextOptions.SpaceBetween,
                    includeSeconds: false);
                string timeToCompleteFormattedDateTimeText = timeToComplete.ToLocalTime().DateTimeToDotFormattedString();

                DrawText(e, "Time Left", $"{timeToCompleteText} ({timeToCompleteFormattedDateTimeText})", Font);
            }

            DrawText(e, "Volume", $"{m_contract.Volume:N1} m³", Font);

            string text = string.Format(CultureConstants.DefaultCulture, GetNumberFormat(m_contract.Reward), m_contract.Reward,
                m_contract.Reward < 10000M ? string.Empty :
                $"({FormatHelper.Format(m_contract.Reward, AbbreviationFormat.AbbreviationWords, false)})");

            // null SolarSystem is OK here, count will be zero as expected
            int startToEndSystemJumps = GetStartToEndRoute.Count(system => system !=
                m_contract.StartStation.SolarSystem);
            decimal iskPerJump = startToEndSystemJumps > 0 ? (m_contract.Reward /
                startToEndSystemJumps) : 0;
            string iskPerJumpText = iskPerJump > 0 ?
                $"({string.Format(CultureConstants.DefaultCulture, GetNumberFormat(iskPerJump), iskPerJump, string.Empty)} /  Jump)" :
                string.Empty;

            DrawText(e, "Reward", string.Empty, Font, false);
            DrawText(e, string.Empty, iskPerJumpText, Font, false, e.Graphics.MeasureString(text, Font).ToSize().Width);
            DrawColoredText(e, text, Font, new Point(SecondIndentPosition, m_height), Color.Green);

            text = string.Format(CultureConstants.DefaultCulture, GetNumberFormat(m_contract.Collateral), m_contract.Collateral,
                m_contract.Collateral < 10000M ? string.Empty :
                $" ({FormatHelper.Format(m_contract.Collateral, AbbreviationFormat.AbbreviationWords, false)})");

            DrawText(e, "Collateral", string.Empty, Font, false);
            DrawColoredText(e, text, Font, new Point(SecondIndentPosition, m_height), Color.Red);

            DrawStationText(e, "Destination", m_contract.EndStation);
        }

        /// <summary>
        /// Draws the generic contract info.
        /// </summary>
        /// <param name="e">The <see cref="System.Windows.Forms.PaintEventArgs"/> instance containing the event data.</param>
        private void DrawGenericInfo(PaintEventArgs e)
        {
            m_height += Pad;

            DrawText(e, "Info by Issuer", m_contract.Description, Font);
            DrawText(e, "Type", m_contract.ContractType.GetDescription(), Font);
            DrawText(e, "Issued by", m_contract.Issuer, Font);
            DrawText(e, "Availability", AvailabilityInfo, Font);
            if (m_contract.AcceptorID != 0)
                DrawText(e, "Contractor", m_contract.Acceptor, Font);
            DrawText(e, "Status", m_contract.Status.GetDescription(), Font);
            DrawStationText(e, "Location", m_contract.StartStation);
            DrawText(e, "Issued Date",
                m_contract.Issued.ToLocalTime().DateTimeToDotFormattedString(), Font);

            DrawExpirationOrCompletionText(e);

            // Draw the lower line
            DrawLine(e);
        }

        /// <summary>
        /// Draws the expiration or completion text.
        /// </summary>
        /// <param name="e">The <see cref="System.Windows.Forms.PaintEventArgs"/> instance containing the event data.</param>
        private void DrawExpirationOrCompletionText(PaintEventArgs e)
        {
            // Don't display "expiration date" for auction contracts
            if (m_contract.ContractType == ContractType.Auction)
                return;

            DrawExpirationText(e);

            if (m_contract.ContractType == ContractType.Courier && m_contract.Accepted != DateTime.MinValue)
            {
                DrawText(e, "Complete Before",
                    m_contract.Accepted.AddDays(m_contract.DaysToComplete)
                        .ToLocalTime().DateTimeToDotFormattedString(), Font);
            }

            if (m_contract.Status != CCPContractStatus.Outstanding && m_contract.Completed != DateTime.MinValue)
            {
                DrawText(e, "Completed Date",
                    m_contract.Completed.ToLocalTime().DateTimeToDotFormattedString(), Font);
            }
        }

        /// <summary>
        /// Draws the expiration text.
        /// </summary>
        /// <param name="e">The <see cref="System.Windows.Forms.PaintEventArgs"/> instance containing the event data.</param>
        private void DrawExpirationText(PaintEventArgs e)
        {
            if (m_contract.Status != CCPContractStatus.Outstanding)
                return;

            string expirationTimeText = m_contract.Expiration.ToLocalTime().DateTimeToDotFormattedString();
            string expirationRemainingTimeText = m_contract.IsAvailable
                ? m_contract.Expiration.ToRemainingTimeShortDescription(DateTimeKind.Utc)
                : m_contract.State.ToString();

            Size expirationTimeTextSize = e.Graphics.MeasureString(expirationTimeText, Font).ToSize();
            Size expirationRemainingTimeTextSize = e.Graphics.MeasureString(expirationRemainingTimeText, Font).ToSize();
            Color color = m_contract.IsAvailable && m_contract.Expiration < DateTime.UtcNow.AddDays(1)
                ? Color.DarkOrange
                : m_contract.State == ContractState.Expired
                    ? Color.Red
                    : ForeColor;

            int position = expirationTimeTextSize.Width;
            DrawText(e, "Expiration Date", expirationTimeText, Font, false);
            DrawText(e, String.Empty, "(", Font, false, position);
            position += Pad * 2;
            Point point = new Point(SecondIndentPosition + position, m_height);
            DrawColoredText(e, expirationRemainingTimeText, Font, point, color, false);
            position += expirationRemainingTimeTextSize.Width;
            DrawText(e, String.Empty, ")", Font, true, position);
        }

        /// <summary>
        /// Draws the station text.
        /// </summary>
        /// <param name="e">The <see cref="System.Windows.Forms.PaintEventArgs"/> instance containing the event data.</param>
        /// <param name="labelText">The label text.</param>
        /// <param name="station">The station.</param>
        private void DrawStationText(PaintEventArgs e, string labelText, Station station)
        {
            Graphics g = e.Graphics;
            SolarSystem system = null;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            string secLevelText = string.Empty;
            if (station != null)
            {
                // If we fetch from an inaccessible citadel not on hammertime, station can
                // be null
                system = station.SolarSystemChecked;
                secLevelText = system.SecurityLevel.ToString("N1");
            }

            // Calculate the amount of whitespaces needed after which the station name must be drawn
            // in order to not overlap the security level text
            // (I know it looks weird but couldn't thing of another way)
            int secLevelTextWidth = g.MeasureString(secLevelText, Font).ToSize().Width;
            int spaceWidth = g.MeasureString(" ", Font).ToSize().Width;
            int tempSpaceWidth = 0;
            int spaceCount = 0;

            do
            {
                tempSpaceWidth += spaceWidth;
                spaceCount++;
            } while (secLevelTextWidth > tempSpaceWidth);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < spaceCount; i++)
            {
                sb.Append(" ");
            }
            sb.Append(station?.Name ?? EveMonConstants.UnknownText);
            string stationText = sb.ToString();

            //Draw the label text
            g.DrawString(labelText, Font, Brushes.DimGray, new Point(DetailsPanel.Left + FirstIntendPosition, m_height));

            // Draw the sec level of the solar system, colored accordingly
            if (secLevelText != null)
                DrawColoredText(e, secLevelText, Font, new Point(DetailsPanel.Left +
                    SecondIndentPosition, m_height), system.SecurityLevelColor, false);

            // Draw the station name
            Size stationTextSize = g.MeasureString(stationText, Font).ToSize();
            int stationTextHeight = stationTextSize.Width < DetailsPanel.Width - SecondIndentPosition ?
                stationTextSize.Height : stationTextSize.Height * 2;
            g.DrawString(stationText, Font, Brushes.Black, new Rectangle(DetailsPanel.Left + SecondIndentPosition, m_height,
                DetailsPanel.Width - SecondIndentPosition, stationTextHeight));
            m_height += stationTextHeight + Pad;

            // Draw warning text if station is a conquerable one or citadel
            if (station == null || StaticGeography.GetStationByID(station.ID) == null)
                DrawColoredText(e, "Station may be inaccessible!", Font, new Point(SecondIndentPosition, m_height), Color.DarkRed);

            if (station != null)
            {
                // Draw jumps from
                DrawJumps(e, station);
                // Draw the "route through" info text
                DrawRouteThroughText(e, station);
            }
        }

        /// <summary>
        /// Draws the jumps.
        /// </summary>
        /// <param name="e">The <see cref="System.Windows.Forms.PaintEventArgs"/> instance containing the event data.</param>
        /// <param name="station">The station.</param>
        private void DrawJumps(PaintEventArgs e, Station station)
        {
            Graphics g = e.Graphics;

            int startToEndSystemJumps;
            string jumpsText;
            Size jumpsTextSize;

            // Draw the "jumps from current location to contract's start solar system" info text
            if (m_characterLastSolarSystem != null && station == m_contract.StartStation)
            {
                // null SolarSystem is OK here
                startToEndSystemJumps = GetCharacterLastLocationToStartRoute.Count(system =>
                    system != station.SolarSystem);
                jumpsText = m_contract.Character.LastKnownStation == station ? "Current Station" :
                    (startToEndSystemJumps == 0 ? "Current System" :
                    $"{startToEndSystemJumps} jump{(startToEndSystemJumps.S())} away - ");

                jumpsTextSize = g.MeasureString(jumpsText, Font).ToSize();
                if (startToEndSystemJumps != 0)
                {
                    CurrentToStartLinkLabel.Location = new Point(SecondIndentPosition + jumpsTextSize.Width, m_height);
                    CurrentToStartLinkLabel.BringToFront();
                    CurrentToStartLinkLabel.Visible = true;
                }

                DrawText(e, string.Empty, jumpsText, Font);
            }

            // Draw the "jumps from current location to contract's end solar system" info text
            if (m_characterLastSolarSystem != null && m_contract.StartStation != m_contract.EndStation &&
                station == m_contract.EndStation)
            {
                // null SolarSystem is OK here
                startToEndSystemJumps = GetCharacterLastLocationToEndRoute.Count(system =>
                    system != station.SolarSystem);
                jumpsText = m_contract.Character.LastKnownStation == station ?
                    "Current Station" : (startToEndSystemJumps == 0 ?
                    "Destination is within same solar system of start location" :
                    $"{startToEndSystemJumps} jump{(startToEndSystemJumps.S())} from current location - ");

                jumpsTextSize = g.MeasureString(jumpsText, Font).ToSize();
                if (startToEndSystemJumps != 0)
                {
                    CurrentToEndLinkLabel.Location = new Point(SecondIndentPosition + jumpsTextSize.Width, m_height);
                    CurrentToEndLinkLabel.BringToFront();
                    CurrentToEndLinkLabel.Visible = true;
                }

                DrawText(e, string.Empty, jumpsText, Font);
            }

            // Draw the "jumps between start and end solar system" info text
            if (m_contract.StartStation == m_contract.EndStation || station != m_contract.
                EndStation || (m_characterLastSolarSystem != null &&
                m_characterLastSolarSystem == m_contract.StartStation.SolarSystem))
                return;

            startToEndSystemJumps = GetStartToEndRoute.Count(system =>
                system != station.SolarSystem);
            jumpsText = startToEndSystemJumps == 0 ?
                "Destination is within same solar system of start location" :
                $"{startToEndSystemJumps} jump{(startToEndSystemJumps.S())} from start location - ";

            jumpsTextSize = g.MeasureString(jumpsText, Font).ToSize();
            if (startToEndSystemJumps != 0)
            {
                StartToEndLinkLabel.Location = new Point(SecondIndentPosition + jumpsTextSize.Width, m_height);
                StartToEndLinkLabel.BringToFront();
                StartToEndLinkLabel.Visible = true;
            }

            DrawText(e, string.Empty, jumpsText, Font);
        }

        /// <summary>
        /// Draws the "route through" text.
        /// </summary>
        /// <param name="e">The <see cref="System.Windows.Forms.PaintEventArgs"/> instance containing the event data.</param>
        /// <param name="station">The station.</param>
        private void DrawRouteThroughText(PaintEventArgs e, Station station)
        {
            if (m_characterLastSolarSystem == null)
                return;

            bool routeThroughNullSec = false;
            bool routeThroughLowSec = false;
            bool routeThroughHighSec = false;

            if (station == m_contract.StartStation)
            {
                routeThroughNullSec = !m_characterLastSolarSystem.IsNullSec &&
                                      GetCharacterLastLocationToStartRoute.Any(system => system.IsNullSec);
                routeThroughLowSec = !m_characterLastSolarSystem.IsLowSec &&
                                     GetCharacterLastLocationToStartRoute.Any(system => system.IsLowSec);
                routeThroughHighSec = !m_characterLastSolarSystem.IsHighSec &&
                                      GetCharacterLastLocationToStartRoute.Any(system => system.IsHighSec);
            }

            if (m_contract.StartStation != m_contract.EndStation && station == m_contract.EndStation)
            {
                routeThroughNullSec = !m_characterLastSolarSystem.IsNullSec &&
                                      GetStartToEndRoute.Any(system => system.IsNullSec);
                routeThroughLowSec = !m_characterLastSolarSystem.IsLowSec &&
                                     GetStartToEndRoute.Any(system => system.IsLowSec);
                routeThroughHighSec = !m_characterLastSolarSystem.IsHighSec &&
                                      GetStartToEndRoute.Any(system => system.IsHighSec);
            }

            // Quit if path is through same sec
            if (!routeThroughNullSec && !routeThroughLowSec && !routeThroughHighSec)
                return;

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            const string RouteText = "Route will take you through: ";
            Size routeTextSize = g.MeasureString(RouteText, Font).ToSize();

            DrawText(e, String.Empty, RouteText, Font, false);
            int left = DetailsPanel.Left + SecondIndentPosition + routeTextSize.Width;

            // Route through "Null Sec"
            if (routeThroughNullSec)
            {
                const string NullSecText = "Null Sec";
                DrawColoredText(e, NullSecText, Font, new Point(left, m_height), Color.Red, false);
                left += g.MeasureString(NullSecText, Font).ToSize().Width;
            }

            // Add comma when route through both "Null Sec" and "Low Sec"
            if (routeThroughNullSec && routeThroughLowSec)
            {
                const string CommaText = ", ";
                DrawText(e, String.Empty, CommaText, Font, false, left - Pad - DetailsPanel.Left - SecondIndentPosition);
                left += g.MeasureString(CommaText, Font).ToSize().Width;
            }

            // Route through "LowSec"
            if (routeThroughLowSec)
            {
                const string LowSecText = "Low Sec";
                DrawColoredText(e, LowSecText, Font, new Point(left, m_height), Color.DarkOrange, false);
                left += g.MeasureString(LowSecText, Font).ToSize().Width;
            }

            // Add comma when route through both "Null Sec" and "Low Sec"
            if ((routeThroughNullSec || routeThroughLowSec) && routeThroughHighSec)
            {
                const string CommaText = ", ";
                DrawText(e, String.Empty, CommaText, Font, false, left - Pad - DetailsPanel.Left - SecondIndentPosition);
                left += g.MeasureString(CommaText, Font).ToSize().Width;
            }

            // Route through "HighSec"
            if (routeThroughHighSec)
                DrawColoredText(e, "High Sec", Font, new Point(left, m_height), Color.Green, false);

            m_height += routeTextSize.Height + Pad;
        }

        /// <summary>
        /// Draws a colored text.
        /// </summary>
        /// <param name="e">The <see cref="System.Windows.Forms.PaintEventArgs"/> instance containing the event data.</param>
        /// <param name="text">The text.</param>
        /// <param name="font">The font.</param>
        /// <param name="point">The point.</param>
        /// <param name="color">The color.</param>
        /// <param name="increaseHeight">if set to <c>true</c> [increase height].</param>
        private void DrawColoredText(PaintEventArgs e, string text, Font font, PointF point, Color color,
            bool increaseHeight = true)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            SizeF textSize = e.Graphics.MeasureString(text, font);
            DrawColoredText(e, text, font, new RectangleF(point, textSize), color, increaseHeight);
        }

        /// <summary>
        /// Draws the colored text.
        /// </summary>
        /// <param name="e">The <see cref="System.Windows.Forms.PaintEventArgs"/> instance containing the event data.</param>
        /// <param name="text">The text.</param>
        /// <param name="font">The font.</param>
        /// <param name="rectangle">The rectangle.</param>
        /// <param name="color">The color.</param>
        /// <param name="increaseHeight">if set to <c>true</c> [increase height].</param>
        private void DrawColoredText(PaintEventArgs e, string text, Font font, RectangleF rectangle, Color color,
            bool increaseHeight = true)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (Brush secLevelBrush = new SolidBrush(color))
            {
                g.DrawString(text, font, secLevelBrush, rectangle);
            }

            if (increaseHeight)
                m_height += g.MeasureString(text, font).ToSize().Height + Pad;
        }

        /// <summary>
        /// Draws a text.
        /// </summary>
        /// <param name="e">The <see cref="System.Windows.Forms.PaintEventArgs"/> instance containing the event data.</param>
        /// <param name="labelText">The label text.</param>
        /// <param name="text">The text.</param>
        /// <param name="font">The font.</param>
        /// <param name="increaseHeight">if set to <c>true</c> [increase height].</param>
        /// <param name="position">The position.</param>
        private void DrawText(PaintEventArgs e, string labelText, string text, Font font, bool increaseHeight = true,
            int position = 0)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Size textSize = g.MeasureString(text, font).ToSize();

            // Draw the label text
            if (!String.IsNullOrEmpty(labelText))
                g.DrawString(labelText, font, Brushes.DimGray, DetailsPanel.Left + FirstIntendPosition, m_height);

            // Draw the contract's related info text
            g.DrawString(text, font, Brushes.Black, DetailsPanel.Left + SecondIndentPosition + position, m_height);

            if (increaseHeight)
                m_height += textSize.Height + Pad;
        }

        /// <summary>
        /// Draws the header.
        /// </summary>
        /// <param name="e">The <see cref="System.Windows.Forms.PaintEventArgs"/> instance containing the event data.</param>
        private void DrawHeader(PaintEventArgs e)
        {
            Image headerImage;
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Select image according to contract type
            switch (m_contract.ContractType)
            {
                case ContractType.Courier:
                    headerImage = ImageList.Images[1];
                    break;
                case ContractType.Auction:
                    headerImage = ImageList.Images[2];
                    break;
                case ContractType.ItemExchange:
                    headerImage = ImageList.Images[0];
                    break;
                default:
                    throw new NotImplementedException();
            }

            // Draw header image
            g.DrawImage(headerImage, new Rectangle(DetailsPanel.Left, DetailsPanel.Top,
                headerImage.Width, headerImage.Height));

            // Draw the header text
            Font headerTextFont = FontFactory.GetDefaultFont(10.25f);
            string headerText = $"{m_contract.ContractText} ({m_contract.ContractType.GetDescription()})";
            Size textSize = g.MeasureString(headerText, headerTextFont).ToSize();
            int imageWidth = headerImage.Width + Pad * 2;
            g.DrawString(headerText, headerTextFont, Brushes.Black,
                new Rectangle(DetailsPanel.Left + imageWidth,
                    headerImage.Height / 2 - textSize.Height,
                    DetailsPanel.Width - imageWidth, textSize.Height * 2));

            m_height = headerImage.Height;

            // Draw the lower line
            DrawLine(e);
        }

        /// <summary>
        /// Draws a line.
        /// </summary>
        /// <param name="e">The <see cref="System.Windows.Forms.PaintEventArgs"/> instance containing the event data.</param>
        private void DrawLine(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (Pen pen = new Pen(ForeColor))
            {
                e.Graphics.DrawLine(pen, DetailsPanel.Left + FirstIntendPosition, m_height,
                    DetailsPanel.Right - FirstIntendPosition, m_height);
            }
        }

        #endregion


        #region Route Drawing Methods

        /// <summary>
        /// Handles the Paint event of the RoutePanel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.PaintEventArgs"/> instance containing the event data.</param>
        private void RoutePanel_Paint(object sender, PaintEventArgs e)
        {
            if (m_route == null)
                return;

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            m_height = Pad;

            g.DrawString("Start Location:", Font, Brushes.Black, FirstIntendPosition, m_height);
            m_height += g.MeasureString("Start Location:", Font).ToSize().Height;

            int width = DrawSolarSystemText(e, m_route.First(), FirstIntendPosition + Pad);
            m_height += Pad * 2;

            width = m_route.Where(solarSystem => solarSystem != m_route.First() && solarSystem != m_route.Last()).Select(
                solarSystem => DrawSolarSystemText(e, solarSystem, FirstIntendPosition * 2)).Concat(new[] { width }).Max();
            m_height += Pad * 2;

            g.DrawString("Destination:", Font, Brushes.Black, FirstIntendPosition, m_height);
            m_height += g.MeasureString("Destination:", Font).ToSize().Height;

            width = Math.Max(width, DrawSolarSystemText(e, m_route.Last(), FirstIntendPosition + Pad));

            width += Pad;
            m_height += Pad * 2 + (width > RoutePanelParent.Width ? FirstIntendPosition * 2 + Pad : Pad);

            RoutePanel.Size = new Size(width, m_height);
        }

        /// <summary>
        /// Draws the solar system text.
        /// </summary>
        /// <param name="e">The <see cref="System.Windows.Forms.PaintEventArgs"/> instance containing the event data.</param>
        /// <param name="solarSystem">The solar system.</param>
        /// <param name="left">The left.</param>
        /// <returns>The width of the drawn text.</returns>
        private int DrawSolarSystemText(PaintEventArgs e, SolarSystem solarSystem, int left)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            string secLevelText = solarSystem.SecurityLevel.ToString("N1", CultureConstants.DefaultCulture);
            int intend = g.MeasureString(secLevelText, Font).ToSize().Width;
            Size systemTextSize = g.MeasureString(solarSystem.Name, Font).ToSize();
            DrawColoredText(e, secLevelText, Font, new Point(left, m_height),
                solarSystem.SecurityLevelColor, false);
            g.DrawString(solarSystem.Name, Font, Brushes.Black, left + intend, m_height);

            m_height += systemTextSize.Height;

            return left + intend + systemTextSize.Width + Pad;
        }


        #endregion


        #region Local Events

        /// <summary>
        /// Handles the Click event of the DetailsPanel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void DetailsPanel_Click(object sender, EventArgs e)
        {
            HideRoutePanel();
        }

        /// <summary>
        /// Handles the MouseClick event of the ItemImage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void ItemImage_MouseClick(object sender, MouseEventArgs e)
        {
            HideRoutePanel();
        }

        /// <summary>
        /// Handles the LinkClicked event of the CurrentToStartLinkLabel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.LinkLabelLinkClickedEventArgs"/> instance containing the event data.</param>
        private void CurrentToStartLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            m_route = m_characterLastLocationToStartRoute;
            ShowRoutePanel();
        }

        /// <summary>
        /// Handles the LinkClicked event of the CurrentToEndLinkLabel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.LinkLabelLinkClickedEventArgs"/> instance containing the event data.</param>
        private void CurrentToEndLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            m_route = m_characterLastLocationToEndRoute;
            ShowRoutePanel();
        }

        /// <summary>
        /// Handles the LinkClicked event of the StartToEndLinkLabel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.LinkLabelLinkClickedEventArgs"/> instance containing the event data.</param>
        private void StartToEndLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            m_route = GetStartToEndRoute;
            ShowRoutePanel();
        }

        /// <summary>
        /// Handles the Click event of the BidsButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void BidsButton_Click(object sender, EventArgs e)
        {
            m_buttonSwitch = !m_buttonSwitch;
            BidsButton.Text = m_buttonSwitch ? "Hide Bids" : " Show Bids";
        }

        #endregion


        #region Helper Methods

        /// <summary>
        /// Shows the route panel.
        /// </summary>
        private void ShowRoutePanel()
        {
            if (RoutePanelParent.Visible)
            {
                if (m_route.SequenceEqual(m_oldRoute))
                    return;

                m_oldRoute = m_route;
                RoutePanel.Invalidate();

                return;
            }

            int width = Width + RoutePanelParent.Width;
            MaximumSize = new Size(width, MaximumSize.Height);
            MinimumSize = new Size(width, MinimumSize.Height);
            RoutePanelParent.Visible = true;
            m_oldRoute = m_route;
        }

        /// <summary>
        /// Hides the route panel.
        /// </summary>
        private void HideRoutePanel()
        {
            if (!RoutePanelParent.Visible)
                return;

            RoutePanelParent.Visible = !RoutePanelParent.Visible;
            int width = Width - RoutePanelParent.Width;
            MaximumSize = new Size(width, MaximumSize.Height);
            MinimumSize = new Size(width, MinimumSize.Height);
        }


        #endregion


        #region Private Static Methods

        /// <summary>
        /// Gets the number format of an amount.
        /// </summary>
        /// <param name="number">The number.</param>
        /// <returns></returns>
        private static string GetNumberFormat(decimal number)
            => number - (long)number == 0 ? "{0:N0} ISK {1}" : "{0:N2} ISK {1}";

        #endregion


        #region Helper Class "ContractItemsListView"

        private sealed class ContractItemsListView : ListView
        {
            private readonly IEnumerable<ContractItem> m_list;
            private readonly Character m_character;
            private ColumnHeader m_sortCriteria;
            private bool m_sortAscending = true;
            private IContainer components;
            private ToolStripMenuItem m_showInBrowserMenuItem;


            #region Constructors

            /// <summary>
            /// Private Constructor.
            /// </summary>
            private ContractItemsListView()
            {
                InitializeComponent();
            }

            /// <summary>
            /// Construtor.
            /// </summary>
            /// <param name="character">The character.</param>
            /// <param name="items">The items.</param>
            internal ContractItemsListView(Character character, IEnumerable<ContractItem> items)
                : this()
            {
                m_list = items;
                m_sortCriteria = Columns[0];
                m_character = character;
            }

            #endregion


            #region Initializer

            /// <summary>
            /// Initializes the component.
            /// </summary>
            private void InitializeComponent()
            {
                components = new Container();
                ContextMenuStrip contextMenu = new ContextMenuStrip(components);
                m_showInBrowserMenuItem = new ToolStripMenuItem();
                contextMenu.SuspendLayout();

                contextMenu.Items.AddRange(new ToolStripItem[]
                {
                    m_showInBrowserMenuItem
                });
                contextMenu.Name = "contextMenu";
                contextMenu.Size = new Size(171, 48);
                contextMenu.Opening += contextMenu_Opening;

                m_showInBrowserMenuItem.Name = "m_showInBrowserMenuItem";
                m_showInBrowserMenuItem.Size = new Size(170, 22);
                m_showInBrowserMenuItem.Text = @"Show In Browser...";
                m_showInBrowserMenuItem.Click += showInBrowserMenuItem_Click;

                contextMenu.ResumeLayout(false);
                ColumnHeader chName = new ColumnHeader
                {
                    Text = @"Name",
                    Width = 120
                };

                ColumnHeader chQuantity = new ColumnHeader
                {
                    Text = @"Qty",
                    Width = 60,
                    TextAlign = HorizontalAlignment.Right
                };

                ColumnHeader chType = new ColumnHeader
                {
                    Text = @"Type",
                    Width = 60
                };

                Columns.AddRange(new[] { chName, chQuantity, chType });
                BorderStyle = BorderStyle.None;
                Size = new Size(0, 0);
                View = View.Details;
                Visible = false;
                FullRowSelect = true;
                HideSelection = false;
                MultiSelect = false;
                ContextMenuStrip = contextMenu;
                ColumnClick += ContractItemsListView_ColumnClick;
            }

            #endregion


            #region Inherited Events

            /// <summary>
            /// Clean up any resources being used.
            /// </summary>
            /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
            protected override void Dispose(bool disposing)
            {
                if (disposing)
                    components?.Dispose();

                base.Dispose(disposing);
            }

            /// <summary>
            /// When the control gets created, update the content.
            /// </summary>
            protected override void OnCreateControl()
            {
                base.OnCreateControl();

                UpdateContent();
                AdjustColumns();
            }

            /// <summary>
            /// Raises the <see cref="E:System.Windows.Forms.Control.Enter" /> event.
            /// </summary>
            /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
            /// <remarks>REmoves the focus cues around the item.</remarks>
            protected override void OnEnter(EventArgs e)
            {
                base.OnEnter(e);

                Message m = Message.Create(Handle, 295, new IntPtr(65537), IntPtr.Zero);
                WndProc(ref m);
            }

            /// <summary>
            /// Raises the <see cref="E:System.Windows.Forms.Control.MouseMove" /> event.
            /// </summary>
            /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs" /> that contains the event data.</param>
            protected override void OnMouseMove(MouseEventArgs e)
            {
                base.OnMouseMove(e);

                if (e.Button == MouseButtons.Right)
                    return;

                ListViewItem item = GetItemAt(e.X, e.Y);

                Cursor = item != null ? CustomCursors.ContextMenu : Cursors.Default;
            }

            /// <summary>
            /// Raises the <see cref="E:System.Windows.Forms.Control.MouseDown" /> event.
            /// </summary>
            /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs" /> that contains the event data.</param>
            protected override void OnMouseDown(MouseEventArgs e)
            {
                base.OnMouseDown(e);

                if (e.Button != MouseButtons.Right)
                    return;

                Cursor = Cursors.Default;
            }

            #endregion


            #region Local Event Handlers

            /// <summary>
            /// Handles the Opening event of the contextMenu control.
            /// </summary>
            /// <param name="sender">The source of the event.</param>
            /// <param name="e">The <see cref="CancelEventArgs"/> instance containing the event data.</param>
            private void contextMenu_Opening(object sender, CancelEventArgs e)
            {
                e.Cancel = SelectedItems.Count == 0;

                if (e.Cancel)
                    return;

                ContractItem contractItem = SelectedItems[0]?.Tag as ContractItem;


                if (contractItem?.Item == null)
                    return;

                Blueprint blueprint = StaticBlueprints.GetBlueprintByID(contractItem.Item.ID);
                Ship ship = contractItem.Item as Ship;
                Skill skill = m_character.Skills[contractItem.Item.ID];

                if (skill == Skill.UnknownSkill)
                    skill = null;

                string text = ship != null ? "Ship" : blueprint != null ? "Blueprint" : skill != null ? "Skill" : "Item";

                m_showInBrowserMenuItem.Text = $"Show In {text} Browser...";
            }

            /// <summary>
            /// Handles the Click event of the showInBrowserMenuItem control.
            /// </summary>
            /// <param name="sender">The source of the event.</param>
            /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
            private void showInBrowserMenuItem_Click(object sender, EventArgs e)
            {
                ContractItem contractItem = SelectedItems[0]?.Tag as ContractItem;

                if (contractItem?.Item == null)
                    return;

                Ship ship = contractItem.Item as Ship;
                Blueprint blueprint = StaticBlueprints.GetBlueprintByID(contractItem.Item.ID);
                Skill skill = m_character.Skills[contractItem.Item.ID];

                if (skill == Skill.UnknownSkill)
                    skill = null;

                PlanWindow planWindow = PlanWindow.ShowPlanWindow(m_character);

                if (ship != null)
                    planWindow.ShowShipInBrowser(ship);
                else if (blueprint != null)
                    planWindow.ShowBlueprintInBrowser(blueprint);
                else if (skill != null)
                    planWindow.ShowSkillInBrowser(skill);
                else
                    planWindow.ShowItemInBrowser(contractItem.Item);
            }

            #endregion


            #region Updating Methods

            /// <summary>
            /// Updates the content.
            /// </summary>
            private void UpdateContent()
            {
                BeginUpdate();
                try
                {
                    UpdateSort();

                    Items.Clear();

                    foreach (ContractItem contractItem in m_list)
                    {
                        ListViewItem lvItem = new ListViewItem(contractItem.Item.Name) { Tag = contractItem };

                        // Add enough subitems to match the number of columns
                        while (lvItem.SubItems.Count < Columns.Count + 1)
                        {
                            lvItem.SubItems.Add(String.Empty);
                        }

                        // Creates the subitems
                        for (int i = 0; i < Columns.Count; i++)
                        {
                            SetColumn(contractItem, lvItem.SubItems[i], Columns[i]);
                        }
                        Items.Add(lvItem);
                    }
                }
                finally
                {
                    EndUpdate();
                }
            }

            /// <summary>
            /// Adjusts the columns.
            /// </summary>
            private void AdjustColumns()
            {
                foreach (ColumnHeader column in Columns)
                {
                    column.Width = -2;

                    // Due to .NET design we need to prevent the last colummn to resize to the right end

                    // Return if it's not the last column and not set to auto-resize
                    if (column.Index != Columns.Count - 1)
                        continue;

                    const int Pad = 4;

                    // Calculate column header text width with padding
                    int columnHeaderWidth = TextRenderer.MeasureText(column.Text, Font).Width + Pad * 2;

                    // If there is an image assigned to the header, add its width with padding
                    if (SmallImageList != null && column.ImageIndex > -1)
                        columnHeaderWidth += SmallImageList.ImageSize.Width + Pad;

                    // Calculate the width of the header and the items of the column
                    int columnMaxWidth = column.ListView.Items.Cast<ListViewItem>().Select(
                        item => TextRenderer.MeasureText(item.SubItems[column.Index].Text, Font).Width).Concat(
                            new[] { columnHeaderWidth }).Max() + Pad + 1;

                    // Assign the width found
                    column.Width = columnMaxWidth;
                }
            }

            /// <summary>
            /// Updates the item sorter.
            /// </summary>
            private void UpdateSort()
            {
                ListViewItemSorter = new ListViewItemComparerByTag<ContractItem>(
                    new ContractItemComparer(m_sortCriteria, m_sortAscending));

                UpdateSortVisualFeedback();
            }

            /// <summary>
            /// Updates the sort feedback (the arrow on the header).
            /// </summary>
            private void UpdateSortVisualFeedback()
            {
                foreach (ColumnHeader columnHeader in Columns.Cast<ColumnHeader>())
                {
                    if (m_sortCriteria == columnHeader)
                        columnHeader.ImageIndex = m_sortAscending ? 0 : 1;
                    else
                        columnHeader.ImageIndex = 2;
                }
            }

            /// <summary>
            /// Updates the listview sub-item.
            /// </summary>
            /// <param name="contractItem">The contract item.</param>
            /// <param name="item">The item.</param>
            /// <param name="column">The column.</param>
            /// <exception cref="NotImplementedException"></exception>
            private static void SetColumn(ContractItem contractItem, ListViewItem.ListViewSubItem item, ColumnHeader column)
            {
                switch (column.Index)
                {
                    case 0:
                        item.Text = contractItem.Item.Name;
                        break;
                    case 1:
                        item.Text = contractItem.Quantity.ToString(CultureConstants.DefaultCulture);
                        break;
                    case 2:
                        item.Text = contractItem.Item.GroupName;
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }


            #region Local Events

            /// <summary>
            /// When the user clicks a column header, we update the sorting.
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void ContractItemsListView_ColumnClick(object sender, ColumnClickEventArgs e)
            {
                ListView lvItems = (ListView)sender;
                ColumnHeader column = lvItems.Columns[e.Column];
                if (m_sortCriteria == column)
                    m_sortAscending = !m_sortAscending;
                else
                {
                    m_sortCriteria = column;
                    m_sortAscending = true;
                }

                UpdateContent();
            }

            #endregion
        }

        #endregion


        #endregion
    }
}
