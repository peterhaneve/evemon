using EVEMon.Common.Constants;
using EVEMon.Common.Data;
using EVEMon.Common.Enumerations;
using EVEMon.Common.Enumerations.CCPAPI;
using EVEMon.Common.Extensions;
using EVEMon.Common.Serialization.Esi;
using EVEMon.Common.Service;

namespace EVEMon.Common.Models
{
    public sealed class Asset
    {
        private readonly CCPCharacter m_character;
        private readonly EveProperty m_volumeProperty = StaticProperties.GetPropertyByID(DBConstants.VolumePropertyID);
        private long m_locationID;
        private string m_flag;
        private string m_fullLocation;
        private SolarSystem m_solarSystem;


        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Asset" /> class.
        /// </summary>
        /// <param name="src">The source.</param>
        /// <param name="character">The owning character.</param>
        /// <exception cref="System.ArgumentNullException">src</exception>
        internal Asset(EsiAssetListItem src, CCPCharacter character)
        {
            src.ThrowIfNull(nameof(src));

            int flagID = EveFlag.GetFlagID(src.EVEFlag);
            LocationID = src.LocationID;
            Quantity = src.Quantity;
            Item = StaticItems.GetItemByID(src.TypeID);
            FlagID = (short)flagID;
            m_character = character;
            m_flag = EveFlag.GetFlagText(flagID);
            TypeOfBlueprint = GetTypeOfBlueprint(src.RawQuantity);
            Container = string.Empty;
            Volume = GetVolume();
            TotalVolume = Quantity * Volume;
        }

        #endregion


        #region Properties

        /// <summary>
        /// Gets the location ID.
        /// </summary>
        internal long LocationID
        {
            get { return m_locationID; }
            set
            {
                if (m_locationID == value)
                    return;

                m_locationID = value;
                // Force update the full location, solar system, station
                m_solarSystem = null;
                m_fullLocation = string.Empty;
            }
        }

        /// <summary>
        /// Gets the full celestial path of the item's location.
        /// </summary>
        public string FullLocation {
            get
            {
                UpdateLocation();
                return m_fullLocation;
            }
        }

        /// <summary>
        /// Gets the solar system.
        /// </summary>
        public SolarSystem SolarSystem
        {
            get
            {
                UpdateLocation();
                return m_solarSystem;
            }
        }

        /// <summary>
        /// Gets the item.
        /// </summary>
        public Item Item { get; }

        /// <summary>
        /// Gets the container.
        /// </summary>
        public string Container { get; internal set; }

        /// <summary>
        /// Gets the quantity.
        /// </summary>
        public long Quantity { get; }

        /// <summary>
        /// Gets the eve flag identifier.
        /// </summary>
        private short FlagID { get; }

        /// <summary>
        /// Gets the flag.
        /// </summary>
        public string Flag
        {
            get
            {
                if (m_flag.IsEmptyOrUnknown())
                    m_flag = EveFlag.GetFlagText(FlagID);

                return m_flag;
            }
        }

        /// <summary>
        /// Gets the type of the blueprint.
        /// </summary>
        public string TypeOfBlueprint { get; }

        /// <summary>
        /// Gets the location.
        /// </summary>
        public string Location { get; private set; }

        /// <summary>
        /// Gets the jumps count.
        /// </summary>
        public int Jumps { get; internal set; }

        /// <summary>
        /// Gets the jumps text.
        /// </summary>
        public string JumpsText => Jumps == -1 ? string.Empty :
            $"{Jumps} jump{(Jumps != 1 ? "s" : string.Empty)}";

        /// <summary>
        /// Gets the volume.
        /// </summary>
        public double Volume { get; }

        /// <summary>
        /// Gets the total volume.
        /// </summary>
        public double TotalVolume { get; }

        /// <summary>
        /// Gets the price.
        /// </summary>
        public double Price => (TypeOfBlueprint != BlueprintType.Copy.ToString()) ?
            (Settings.MarketPricer.Pricer != null ? Settings.MarketPricer.Pricer.
            GetPriceByTypeID(Item.ID) : 0.0) : 0.0;

        /// <summary>
        /// Gets the cost.
        /// </summary>
        public double Cost => Price * Quantity;

        #endregion


        #region Helper Methods

        /// <summary>
        /// Gets the type of the blueprint.
        /// </summary>
        /// <param name="rawQuantity">The raw quantity.</param>
        /// <returns></returns>
        private string GetTypeOfBlueprint(int rawQuantity) => (Item != null &&
            StaticBlueprints.GetBlueprintByID(Item.ID) != null && !Item.MarketGroup.BelongsIn(
            DBConstants.AncientRelicsMarketGroupID)) ? (rawQuantity == -2 ?
            BlueprintType.Copy.ToString() : BlueprintType.Original.ToString()) : string.Empty;

        /// <summary>
        /// Gets the volume.
        /// </summary>
        /// <returns></returns>
        private double GetVolume() => (Item != null && m_volumeProperty != null) ?
            m_volumeProperty.GetNumericValue(Item) : 0d;

        /// <summary>
        /// Updates the location.
        /// </summary>
        /// <returns></returns>
        public void UpdateLocation()
        {
            // If location not already determined
            if (m_locationID != 0L && (m_solarSystem == null || m_solarSystem.ID == 0 ||
                m_fullLocation.IsEmptyOrUnknown()))
            {
                var station = EveIDToStation.GetIDToStation(m_locationID, m_character);
                if (station == null)
                {
                    SolarSystem sys;
                    if (m_locationID < int.MaxValue && (sys = StaticGeography.
                        GetSolarSystemByID((int)m_locationID)) != null)
                    {
                        // In space
                        m_solarSystem = sys;
                        m_fullLocation = sys.FullLocation;
                    }
                    else
                    {
                        // In an inaccessible citadel, or one that is not yet loaded
                        m_solarSystem = new SolarSystem();
                        m_fullLocation = EveMonConstants.UnknownText;
                    }
                }
                else
                {
                    // Station known
                    m_solarSystem = station.SolarSystem;
                    m_fullLocation = station.FullLocation;
                }
                string locationStr = m_locationID.ToString(CultureConstants.InvariantCulture);
                Location = (station == null ? (m_solarSystem == null ? locationStr :
                    m_solarSystem.Name) : station.Name);
            }
        }

        #endregion

    }
}
