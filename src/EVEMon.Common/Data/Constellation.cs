using System;
using EVEMon.Common.Collections;
using EVEMon.Common.Constants;
using EVEMon.Common.Extensions;
using EVEMon.Common.Serialization.Datafiles;

namespace EVEMon.Common.Data
{
    /// <summary>
    /// Represents a constellation of the EVE universe.
    /// </summary>
    public sealed class Constellation : ReadonlyCollection<SolarSystem>, IComparable<Constellation>
    {
        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="region">The region.</param>
        /// <param name="src">The source.</param>
        /// <exception cref="System.ArgumentNullException">src</exception>
        public Constellation(Region region, SerializableConstellation src)
            : base(src?.Systems.Count ?? 0)
        {
            src.ThrowIfNull(nameof(src));

            ID = src.ID;
            Name = src.Name;
            Region = region;

            foreach (SerializableSolarSystem srcSystem in src.Systems)
            {
                Items.Add(new SolarSystem(this, srcSystem));
            }
        }

        public Constellation()
        {
            ID = 0;
            Name = EveMonConstants.UnknownText;
            Region = new Region();
        }
        #endregion


        #region Public Properties

        /// <summary>
        /// Gets this object's id.
        /// </summary>
        public int ID { get; }

        /// <summary>
        /// Gets this object's name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the region where this constellation is located.
        /// </summary>
        public Region Region { get; }

        /// <summary>
        /// Gets something like Region > Constellation.
        /// </summary>
        public string FullLocation => $"{Region.Name} > {Name}";

        #endregion


        #region Helper Methods

        /// <summary>
        /// Compares this system with another one.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">other</exception>
        public int CompareTo(Constellation other)
        {
            other.ThrowIfNull(nameof(other));

            return Region != other.Region
                ? Region.CompareTo(other.Region)
                : String.Compare(Name, other.Name, StringComparison.CurrentCulture);
        }

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