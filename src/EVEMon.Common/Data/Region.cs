using System;
using EVEMon.Common.Collections;
using EVEMon.Common.Constants;
using EVEMon.Common.Extensions;
using EVEMon.Common.Serialization.Datafiles;

namespace EVEMon.Common.Data
{
    /// <summary>
    /// Represents a region of the EVE universe.
    /// </summary>
    public sealed class Region : ReadonlyCollection<Constellation>, IComparable<Region>
    {
        # region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="src"></param>
        internal Region(SerializableRegion src)
            : base(src.Constellations.Count)
        {
            ID = src.ID;
            Name = src.Name;

            foreach (SerializableConstellation srcConstellation in src.Constellations)
            {
                Items.Add(new Constellation(this, srcConstellation));
            }
        }

        internal Region()
        {
            ID = 0;
            Name = EveMonConstants.UnknownText;
        }
        #endregion


        # region Public Properties

        /// <summary>
        /// Gets this object's id.
        /// </summary>
        public long ID { get; }

        /// <summary>
        /// Gets this object's name.
        /// </summary>
        public string Name { get; }

        #endregion


        #region Public Methods

        /// <summary>
        /// Compare two regions by their names.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">other</exception>
        public int CompareTo(Region other)
        {
            other.ThrowIfNull(nameof(other));

            return String.Compare(Name, other.Name, StringComparison.CurrentCulture);
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