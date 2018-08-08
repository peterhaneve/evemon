using System;
using EVEMon.Common.Collections;
using EVEMon.Common.Extensions;
using EVEMon.Common.Models;
using EVEMon.Common.Serialization.Datafiles;
using EVEMon.Common.Serialization.Eve;

namespace EVEMon.Common.Data
{
    /// <summary>
    /// Represents a station inside the EVE universe.
    /// </summary>
    public class Station : ReadonlyCollection<Agent>, IComparable<Station>
    {
        /// <summary>
        /// Creates a station modelling an inaccessible citadel with the given ID.
        /// </summary>
        /// <param name="id">The citadel ID that could not be accessed.</param>
        /// <returns>A dummy station object that represents that structure.</returns>
        public static Station CreateInaccessible(long id)
        {
            return new Station(new SerializableOutpost()
            {
                CorporationID = 0,
                SolarSystemID = 0,
                StationID = id,
                StationName = "Inaccessible Structure",
                StationTypeID = 35832 // Astrahus
            });
        }

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Station"/> class.
        /// </summary>
        /// <param name="src">The source.</param>
        /// <exception cref="System.ArgumentNullException">src</exception>
        public Station(SerializableOutpost src)
        {
            src.ThrowIfNull(nameof(src));

            ID = src.StationID;
            Name = src.StationName;
            CorporationID = src.CorporationID;
            CorporationName = src.CorporationName;
            SolarSystem = StaticGeography.GetSolarSystemByID(src.SolarSystemID);
            FullLocation = GetFullLocation(SolarSystem, src.StationName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Station"/> class.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="src">The source.</param>
        /// <exception cref="System.ArgumentNullException">owner or src</exception>
        public Station(SolarSystem owner, SerializableStation src)
            : base(src?.Agents?.Count ?? 0)
        {
            owner.ThrowIfNull(nameof(owner));

            src.ThrowIfNull(nameof(src));

            ID = src.ID;
            Name = src.Name;
            CorporationID = src.CorporationID;
            CorporationName = src.CorporationName;
            SolarSystem = owner;
            ReprocessingStationsTake = src.ReprocessingStationsTake;
            ReprocessingEfficiency = src.ReprocessingEfficiency;
            FullLocation = GetFullLocation(owner, src.Name);

            if (src.Agents == null)
                return;

            foreach (SerializableAgent agent in src.Agents)
            {
                Items.Add(new Agent(this, agent));
            }
        }
        
        #endregion


        #region Public Properties
            
        /// <summary>
        /// Gets this object's id.
        /// </summary>
        public long ID { get; }

        /// <summary>
        /// Gets this object's name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets this object's corporation id.
        /// </summary>
        public int CorporationID { get; }

        /// <summary>
        /// Gets this object's corporation name.
        /// </summary>
        public string CorporationName { get; }

        /// <summary>
        /// Gets the solar system where this station is located.
        /// </summary>
        public SolarSystem SolarSystem { get; }

        /// <summary>
        /// Gets the solar system where this station is located. This accessor is checked
        /// and instead of returning null returns an empty solar system
        /// </summary>
        public SolarSystem SolarSystemChecked => SolarSystem ?? SolarSystem.UNKNOWN;

        /// <summary>
        /// Gets something like Region > Constellation > Solar System > Station.
        /// </summary>
        public string FullLocation { get; }

        /// <summary>
        /// Gets the base reprocessing efficiency of the station.
        /// </summary>
        public float ReprocessingEfficiency { get; }

        /// <summary>
        /// Gets the fraction of reprocessing products taken by the station.
        /// </summary>
        public float ReprocessingStationsTake { get; }

        #endregion


        #region Public Methods

        /// <summary>
        /// Compares this station with another one.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">other</exception>
        public int CompareTo(Station other)
        {
            other.ThrowIfNull(nameof(other));
            // Properly handle null SolarSystem, it should be equal to SolarSystem with ID = 0
            SolarSystem mine = SolarSystemChecked, theirs = other.SolarSystemChecked;
            return mine != theirs ? mine.CompareTo(theirs) : string.Compare(Name, other.Name,
                StringComparison.CurrentCulture);
        }

        #endregion


        #region Helper Methods

        /// <summary>
        /// Gets the station's full location.
        /// </summary>
        /// <param name="solarSystem">The solar system.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        private static string GetFullLocation(SolarSystem solarSystem, string name)
            => (solarSystem == null) ? string.Empty : $"{solarSystem.FullLocation} > {name}";
        
        #endregion


        #region Overridden Methods

        /// <summary>
        /// Gets the name of this object.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Name;

        #endregion
    }
}
