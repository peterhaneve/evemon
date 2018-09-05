using System;
using System.Collections.Generic;
using EVEMon.Common.Data;
using EVEMon.Common.Enumerations;
using EVEMon.Common.Enumerations.CCPAPI;
using EVEMon.Common.Extensions;
using EVEMon.Common.Serialization.Settings;
using EVEMon.Common.Service;
using EVEMon.Common.Serialization.Esi;

namespace EVEMon.Common.Models
{
    /// <summary>
    /// A base class for market orders.
    /// </summary>
    public abstract class MarketOrder
    {
        /// <summary>
        /// The maximum number of days after expiration. Beyond this limit, we do not import
        /// orders anymore.
        /// </summary>
        public const int MaxExpirationDays = 7;

        private readonly CCPCharacter m_character;
        private OrderState m_state;
        private long m_stationID;


        #region Constructors

        /// <summary>
        /// Constructor from the API.
        /// </summary>
        /// <param name="src">The source.</param>
        /// <param name="issuedFor">Whether the order was issued for a corporation or a
        /// character.</param>
        /// <param name="character">The owning character.</param>
        /// <exception cref="System.ArgumentNullException">src</exception>
            protected MarketOrder(EsiOrderListItem src, IssuedFor issuedFor,
                CCPCharacter character)
        {
            src.ThrowIfNull(nameof(src));

            PopulateOrderInfo(src, issuedFor);

                // veg Fix #1
                issuedFor = src.IsCorporation || src.IssuedBy == 0  ? IssuedFor.Corporation : IssuedFor.Character;

            LastStateChange = DateTime.UtcNow;
            m_character = character;
            m_state = GetState(src);
        }

        /// <summary>
        /// Constructor from an object deserialized from the settings file.
        /// </summary>
        /// <param name="src">The source.</param>
        /// <param name="character">The owning character.</param>
        /// <exception cref="System.ArgumentNullException">src</exception>
        protected MarketOrder(SerializableOrderBase src, CCPCharacter character)
        {
            src.ThrowIfNull(nameof(src));

            ID = src.OrderID;
            UnitaryPrice = src.UnitaryPrice;
            RemainingVolume = src.RemainingVolume;
            Issued = src.Issued;
            IssuedFor = src.IssuedFor == IssuedFor.None ? IssuedFor.Character : src.IssuedFor;
            LastStateChange = src.LastStateChange;
            m_character = character;
            m_state = src.State;
        }

        #endregion


        #region Properties

        /// <summary>
        /// When true, the order will be deleted unless it was found on the API feed.
        /// </summary>
        internal bool MarkedForDeletion { get; set; }

        /// <summary>
        /// Gets the order state.
        /// </summary>
        public OrderState State
        {
            get
            {
                if (IsExpired && (m_state == OrderState.Active || m_state == OrderState.Modified))
                    return OrderState.Expired;

                return m_state;
            }
        }

        /// <summary>
        /// Gets the order ID.
        /// </summary>
        public long ID { get; private set; }

        /// <summary>
        /// Gets or sets the owner ID.
        /// </summary>
        /// <value>The owner ID.</value>
        public long OwnerID { get; internal set; }

        /// <summary>
        /// Gets the item.
        /// </summary>
        public Item Item { get; private set; }

        /// <summary>
        /// Gets the station where this order is located.
        /// </summary>
        public Station Station { get; private set; }

        /// <summary>
        /// Gets the intial volume.
        /// </summary>
        public int InitialVolume { get; private set; }

        /// <summary>
        /// Gets the remaining volume.
        /// </summary>
        public int RemainingVolume { get; private set; }

        /// <summary>
        /// Gets the minimum sell/buy threshold volume.
        /// </summary>
        public int MinVolume { get; private set; }

        /// <summary>
        /// Gets the duration, in days, of the order.
        /// </summary>
        public int Duration { get; private set; }

        /// <summary>
        /// Gets the unitary price.
        /// </summary>
        public decimal UnitaryPrice { get; private set; }

        /// <summary>
        /// Gets the total price.
        /// </summary>
        public decimal TotalPrice => UnitaryPrice * RemainingVolume;

        /// <summary>
        /// Gets the time (UTC) this order was expired.
        /// </summary>
        public DateTime Issued { get; private set; }

        /// <summary>
        /// Gets for which the order was issued.
        /// </summary>
        public IssuedFor IssuedFor { get; private set; }

        /// <summary>
        /// Gets the estimated expiration time.
        /// </summary>
        public DateTime Expiration => Issued.AddDays(Duration);

        /// <summary>
        /// Gets the last state change.
        /// </summary>
        public DateTime LastStateChange { get; private set; }

        /// <summary>
        /// Gets true if order naturally expired because of its duration.
        /// </summary>
        public bool IsExpired => Expiration < DateTime.UtcNow;

        /// <summary>
        /// Gets true if the order is not fulfilled, canceled, expired, etc.
        /// </summary>
        public bool IsAvailable => (m_state == OrderState.Active || m_state == OrderState.Modified) && !IsExpired;

        #endregion


        #region Importation, Exportation

        /// <summary>
        /// Exports the given object to a serialization object.
        /// </summary>
        /// <returns></returns>
        public abstract SerializableOrderBase Export();

        /// <summary>
        /// Fetches the data to the given source.
        /// </summary>
        /// <param name="src"></param>
        protected SerializableOrderBase Export(SerializableOrderBase src)
        {
            src.ThrowIfNull(nameof(src));

            src.OrderID = ID;
            src.State = m_state;
            src.UnitaryPrice = UnitaryPrice;
            src.RemainingVolume = RemainingVolume;
            src.Issued = Issued;
            src.IssuedFor = IssuedFor;
            src.LastStateChange = LastStateChange;

            return src;
        }

        /// <summary>
        /// Try to update this order with a serialization object from the API.
        /// </summary>
        /// <param name="src"></param>
        /// <param name="issuedFor">Whether the order was issued for a corporation or a
        /// character.</param>
        /// <param name="endedOrders"></param>
        /// <returns></returns>
        internal bool TryImport(EsiOrderListItem src, IssuedFor issuedFor,
            ICollection<MarketOrder> endedOrders)
        {
            // Note that, before a match is found, all orders have been marked for deletion:
            // m_markedForDeletion == true
            // Checks whether ID is the same (IDs can be recycled ?)
            if (!MatchesWith(src))
                return false;
            // Prevent deletion
            MarkedForDeletion = false;
            // Update infos (if ID is the same it may have been modified either by the market 
            // or by the user [modify order] so we update the orders info that are changeable)
            if (IsModified(src))
            {
                if (Item != null)
                {
                    // If it is a buy order, escrow may have changed
                    var buyOrder = this as BuyOrder;
                    if (src.IsBuyOrder && buyOrder != null)
                        buyOrder.Escrow = src.Escrow;
                    UnitaryPrice = src.UnitaryPrice;
                    RemainingVolume = src.RemainingVolume;
                    Issued = src.Issued;
                }
                LastStateChange = DateTime.UtcNow;
                m_state = OrderState.Modified;
            }
            else if (m_state == OrderState.Modified)
            {
                LastStateChange = DateTime.UtcNow;
                m_state = OrderState.Active;
            }
            // Order is from a serialized object, so populate the missing info
            if (Item == null)
                PopulateOrderInfo(src, issuedFor);
            OrderState state = GetState(src);
            if (m_state == OrderState.Modified || state == m_state)
                return true;
            // It has either expired or fulfilled
            m_state = state;
            LastStateChange = DateTime.UtcNow;
            // Should we notify it to the user ?
            if (state == OrderState.Expired || state == OrderState.Fulfilled)
                endedOrders.Add(this);
            return true;
        }

        /// <summary>
        /// Populates the serialization object order with the info from the API.
        /// </summary>
        /// <param name="src">The source.</param>
        /// <param name="ownerID">The owner of this order.</param>
        /// <param name="issuedFor">Whether the order was issued for a corporation or a
        /// character.</param>
        private void PopulateOrderInfo(EsiOrderListItem src, IssuedFor issuedFor)
        {
            OwnerID = src.IssuedBy;
            ID = src.OrderID;
            Item = StaticItems.GetItemByID(src.ItemID);
            UnitaryPrice = src.UnitaryPrice;
            InitialVolume = src.InitialVolume;
            RemainingVolume = src.RemainingVolume;
            MinVolume = src.MinVolume;
            Duration = src.Duration;
            Issued = src.Issued;
            // veg Fix #2
            IssuedFor = src.IsCorporation || src.IssuedBy == 0 ? IssuedFor.Corporation : IssuedFor.Character;
            m_stationID = src.StationID;
            UpdateStation();

            var buyOrder = this as BuyOrder;
            if (src.IsBuyOrder && buyOrder != null)
            {
                buyOrder.Escrow = src.Escrow;
                buyOrder.Range = src.Range;
            }
        }

        #endregion


        #region Public Methods

        /// <summary>
        /// Updates the station.
        /// </summary>
        public void UpdateStation()
        {
            Station = EveIDToStation.GetIDToStation(m_stationID, m_character);
        }

        #endregion


        #region Helper Methods

        /// <summary>
        /// Gets the state of an order.
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        private static OrderState GetState(EsiOrderListItem src)
        {
            switch ((CCPOrderState)src.State)
            {
                case CCPOrderState.Closed:
                case CCPOrderState.Canceled:
                case CCPOrderState.CharacterDeleted:
                    return OrderState.Canceled;
                case CCPOrderState.Pending:
                case CCPOrderState.Opened:
                    return OrderState.Active;
                case CCPOrderState.ExpiredOrFulfilled:
                    return src.RemainingVolume == 0 ? OrderState.Fulfilled : OrderState.Expired;
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Checks whether the given API object matches with this order.
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        private bool MatchesWith(EsiOrderListItem src) => src.OrderID == ID;

        /// <summary>
        /// Checks whether the given API object has been modified.
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        private bool IsModified(EsiOrderListItem src) => src.RemainingVolume != 0 &&
            ((src.UnitaryPrice != UnitaryPrice && src.Issued != Issued) || src.
            RemainingVolume != RemainingVolume);

        #endregion
    }
}
