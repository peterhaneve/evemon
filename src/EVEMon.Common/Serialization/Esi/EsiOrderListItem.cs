using System;
using EVEMon.Common.Extensions;
using System.Runtime.Serialization;
using EVEMon.Common.Constants;

namespace EVEMon.Common.Serialization.Esi
{
    /// <summary>
    /// Represents a market order
    /// </summary>
    [DataContract]
    public sealed class EsiOrderListItem
    {
        private DateTime issued;

        public EsiOrderListItem()
        {
            issued = DateTime.MinValue;
        }

        /// <summary>
        /// Unique order ID for this order. Note that these are not guaranteed to be unique forever, they can recycle. 
        /// But they are unique for the purpose of one data pull. 
        /// </summary>
        [DataMember(Name = "order_id")]
        public long OrderID { get; set; }

        /// <summary>
        /// The item ID.
        /// </summary>
        [DataMember(Name = "type_id")]
        public int ItemID { get; set; }

        /// <summary>
        /// The location ID.
        /// </summary>
        [DataMember(Name = "location_id")]
        public long StationID { get; set; }

        /// <summary>
        /// The initial volume of the order.
        /// </summary>
        [DataMember(Name = "volume_total")]
        public int InitialVolume { get; set; }

        /// <summary>
        /// The remaining volume of the order.
        /// </summary>
        [DataMember(Name = "volume_remain")]
        public int RemainingVolume { get; set; }

        /// <summary>
        /// The minimum volume a buyer can buy.
        /// </summary>
        [DataMember(Name = "min_volume")]
        public int MinVolume { get; set; }

        /// <summary>
        /// 0 = open/active, 1 = closed, 2 = expired (or fulfilled), 3 = cancelled, 4 = pending, 5 = character deleted.
        /// </summary>
        public int State { get; set; }

        [DataMember(Name = "state", EmitDefaultValue = false, IsRequired = false)]
        private string StateJson
        {
            get
            {
                switch (State)
                {
                    case 1:
                        return "closed";
                    case 2:
                        return "expired";
                    case 3:
                        return "cancelled";
                    default:
                        // Active in ESI is simply not shown
                        return default(string);
                }
            }
            set
            {
                switch (value)
                {
                    case "active":
                    case "":
                    case null:
                        State = 0;
                        break;
                    case "closed":
                        State = 1;
                        break;
                    case "expired":
                        State = 2;
                        break;
                    case "cancelled":
                        State = 3;
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// The range this order is good for. For sell orders, this is always 32767. 
        /// For buy orders, allowed values are: -1 = station, 0 = solar system, 1 = 1 jump, 2 = 2 jumps, ..., 32767 = region.
        /// </summary>
        [DataMember(Name = "range")]
        private string RangeJson
        {
            get
            {
                int jumps = Range;
                if (jumps <= -1)
                    return "station";
                if (jumps == 0)
                    return "solarsystem";
                if (jumps >= 41)
                    return "region";
                return jumps.ToString();
            }
            set
            {
                int jumps;
                // Converts to legacy XML value
                switch (value)
                {
                    case "station":
                        Range = -1;
                        break;
                    case "solarsystem":
                        Range = 0;
                        break;
                    case "region":
                        Range = EveConstants.RegionRange;
                        break;
                    default:
                        // Cannot actually fail, but the exception would suck
                        if (value.TryParseInv(out jumps) && jumps > 0 && jumps < EveConstants.
                                RegionRange)
                            Range = jumps;
                        break;
                }
            }
        }

        [IgnoreDataMember]
        public int Range { get; set; }

        /// <summary>
        /// Which division this order is using as its account. 
        /// Always 0 for characters, within 1 to 7 for corporations.
        /// </summary>
        [DataMember(Name = "wallet_division", EmitDefaultValue = false, IsRequired = false)]
        public int Division { get; set; }

        /// <summary>
        /// How many days this order is good for. Expiration is issued + duration in days. 
        /// </summary>
        [DataMember(Name = "duration")]
        public int Duration { get; set; }

        /// <summary>
        /// How much ISK is in escrow, for buy orders.
        /// </summary>
        [DataMember(Name = "escrow", EmitDefaultValue = false, IsRequired = false)]
        public decimal Escrow { get; set; }

        /// <summary>
        /// The cost per unit for this order.
        /// </summary>
        [DataMember(Name = "price")]
        public decimal UnitaryPrice { get; set; }

        /// <summary>
        /// True if this is a buy order, false otherwise.
        /// </summary>
        [DataMember(Name = "is_buy_order")]
        public bool IsBuyOrder { get; set; }

        // veg Fix #3

        /// <summary>
        /// True if this is a corporation order, false otherwise.
        /// </summary>
        [DataMember(Name = "is_corporation")]
        public bool IsCorporation { get; set; }

        /// <summary>
        /// The character ID who issued this order.
        /// </summary>
        [DataMember(Name = "issued_by", EmitDefaultValue = false, IsRequired = false)]
        public long IssuedBy { get; set; }

        /// <summary>
        /// Whether this order is for a corporation, made on behalf of a player.
        /// This is slightly different from contract and industry job behavior (CCP why?)
        ///  [I can tell that these endpoints were written by different people with slightly
        ///  different specs]
        /// </summary>
        [DataMember(Name = "is_corporation", EmitDefaultValue = false, IsRequired = false)]
        public bool IsCorporation { get; set; }

        /// <summary>
        /// The time this order was issued.
        /// </summary>
        [IgnoreDataMember]
        public DateTime Issued
        {
            get
            {
                return issued;
            }
        }

        [DataMember(Name = "issued")]
        private string IssuedJson
        {
            get { return issued.DateTimeToTimeString(); }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    issued = value.TimeStringToDateTime();
            }
        }
    }
}
