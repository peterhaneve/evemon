using System;
using System.Collections.Generic;
using System.Drawing;
using EVEMon.Common.Collections;
using EVEMon.Common.Enumerations;
using EVEMon.Common.Extensions;
using EVEMon.Common.Helpers;
using EVEMon.Common.Serialization.Datafiles;

namespace EVEMon.Common.Data
{
    /// <summary>
    /// Represents a solar system of the EVE universe.
    /// </summary>
    public sealed class SolarSystem : ReadonlyCollection<Station>, IComparable<SolarSystem>
    {
        /// <summary>
        /// The unknown solar system, with an empty location and ID of 0.
        /// </summary>
        public static readonly SolarSystem UNKNOWN = new SolarSystem();

        // Do not set this as readonly !
        private FastList<SolarSystem> m_jumps;

        // The planets in this system.
        private readonly FastList<Planet> m_planets;

        private readonly int m_x;
        private readonly int m_y;
        private readonly int m_z;


        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SolarSystem"/> class.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="src">The source.</param>
        /// <exception cref="System.ArgumentNullException">owner or src</exception>
        public SolarSystem(Constellation owner, SerializableSolarSystem src)
            : base(src?.Stations?.Count ?? 0)
        {
            owner.ThrowIfNull(nameof(owner));
            src.ThrowIfNull(nameof(src));

            ID = src.ID;
            Constellation = owner;
            Name = src.Name;
            SecurityLevel = src.SecurityLevel;
            FullLocation = $"{owner.FullLocation} > {src.Name}";
            m_jumps = new FastList<SolarSystem>(0);

            m_x = src.X;
            m_y = src.Y;
            m_z = src.Z;

            if (src.Stations != null)
                foreach (SerializableStation srcStation in src.Stations)
                    Items.Add(new Station(this, srcStation));

            if (src.Planets != null)
            {
                // Add planets
                m_planets = new FastList<Planet>(src.Planets.Count);
                foreach (SerializablePlanet srcPlanet in src.Planets)
                    m_planets.Add(new Planet(this, srcPlanet));
            }
            else
                m_planets = new FastList<Planet>(1);
        }

        public SolarSystem()
        {
            ID = 0;
            Constellation = new Constellation();
            SecurityLevel = 0.0F;
            FullLocation = "";
        }
        #endregion


        # region Public Properties

        /// <summary>
        /// Gets this object's id.
        /// </summary>
        public int ID { get; }

        /// <summary>
        /// Gets this object's name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the real security level, between -1.0 and +1.0
        /// </summary>
        public float SecurityLevel { get; }

        /// <summary>
        /// Gets the constellation this solar system is located.
        /// </summary>
        public Constellation Constellation { get; }

        /// <summary>
        /// Gets something like Region > Constellation > Solar System.
        /// </summary>
        public string FullLocation { get; }

        /// <summary>
        /// Gets the planets in this solar system.
        /// </summary>
        public ICollection<Planet> Planets { get; }

        /// <summary>
        /// Gets or sets the color of the security level.
        /// </summary>
        /// <value>The color of the security level.</value>
        public Color SecurityLevelColor
        {
            get
            {
                if (IsNullSec)
                    return Color.Red;

                return IsLowSec ? Color.DarkOrange : Color.Green;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this solar system is in high sec.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this solar system is in high sec; otherwise, <c>false</c>.
        /// </value>
        public bool IsHighSec => Math.Round(SecurityLevel, 1) >= 0.5;

        /// <summary>
        /// Gets a value indicating whether this solar system is in low sec.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this solar system is in low sec; otherwise, <c>false</c>.
        /// </value>
        public bool IsLowSec
        {
            get
            {
                double secLevel = Math.Round(SecurityLevel, 1);
                return secLevel > 0 && secLevel < 0.5;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this solar system is in null sec.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this solar system is in null sec; otherwise, <c>false</c>.
        /// </value>
        public bool IsNullSec => Math.Round(SecurityLevel, 1) <= 0;

        #endregion


        #region Public Methods

        /// <summary>
        /// Looks up a planet by its ID.
        /// </summary>
        /// <param name="planetID">The planet ID.</param>
        /// <returns>The planet, or null if the planet is not in this system.</returns>
        public Planet FindPlanetByID(int planetID)
        {
            Planet planet = null;
            // May look slow but there are only a few planets per system
            foreach (var srcPlanet in m_planets)
                if (srcPlanet.ID == planetID)
                {
                    planet = srcPlanet;
                    break;
                }
            return planet;
        }

        /// <summary>
        /// Gets the square distance with the given system.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">other</exception>
        public int GetSquareDistanceWith(SolarSystem other)
        {
            other.ThrowIfNull(nameof(other));

            int dx = m_x - other.m_x;
            int dy = m_y - other.m_y;
            int dz = m_z - other.m_z;

            return dx * dx + dy * dy + dz * dz;
        }

        /// <summary>
        /// Gets the solar systems within the given range.
        /// </summary>
        /// <param name="maxInclusiveNumberOfJumps">The maximum, inclusive, number of jumps from this system.</param>
        /// <returns></returns>
        public IEnumerable<SolarSystemRange> GetSystemsWithinRange(int maxInclusiveNumberOfJumps)
            => SolarSystemRange.GetSystemRangesFrom(this, maxInclusiveNumberOfJumps);

        /// <summary>
        /// Find the guessed shortest path using a A* (heuristic) algorithm.
        /// </summary>
        /// <param name="target">The target system.</param>
        /// <param name="criteria">The path searching criteria.</param>
        /// <param name="minSecurityLevel">The minimum, inclusive, real security level. Systems have levels between -1 and +1.</param>
        /// <param name="maxSecurityLevel">The maximum, inclusive, real security level. Systems have levels between -1 and +1.</param>
        /// <returns>
        /// The list of systems, beginning with this one and ending with the provided target.
        /// </returns>
        public IEnumerable<SolarSystem> GetFastestPathTo(SolarSystem target, PathSearchCriteria criteria,
            float minSecurityLevel = -1.0f, float maxSecurityLevel = 1.0f)
            => PathFinder.FindBestPath(this, target, criteria, minSecurityLevel, maxSecurityLevel);

        /// <summary>
        /// Gets the systems which have a jumpgate connection with his one.
        /// </summary>
        public IEnumerable<SolarSystem> Neighbors => m_jumps;

        #endregion


        # region Internal Methods

        /// <summary>
        /// Adds a neighbor with a jumpgate connection to this system.
        /// </summary>
        /// <param name="system"></param>
        internal void AddNeighbor(SolarSystem system)
        {
            m_jumps.Add(system);
        }

        /// <summary>
        /// Trims the neighbors list.
        /// </summary>
        internal void TrimNeighbors()
        {
            if (m_jumps.Capacity > m_jumps.Count)
                m_jumps.Trim();
        }

        #endregion


        #region Overridden Methods

        /// <summary>
        /// Gets the name of this object.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Name;

        /// <summary>
        /// Gets the ID of the object.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() => ID;

        #endregion


        #region Comparer Method

        /// <summary>
        /// Compares this system with another one.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">other</exception>
        public int CompareTo(SolarSystem other)
        {
            other.ThrowIfNull(nameof(other));

            return Constellation != other.Constellation
                ? Constellation.CompareTo(other.Constellation)
                : String.Compare(Name, other.Name, StringComparison.CurrentCulture);
        }

        #endregion

    }
}
