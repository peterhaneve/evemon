using System.Linq;
using EVEMon.Common.Collections;
using EVEMon.Common.Models;
using EVEMon.Common.Serialization.Datafiles;

namespace EVEMon.Common.Data
{
    /// <summary>
    /// Represents a mastery ship.
    /// </summary>
    public sealed class MasteryShip : ReadonlyCollection<Mastery>
    {

        #region Constructor

        /// <summary>
        /// Deserialization constructor.
        /// </summary>
        /// <param name="src">The source.</param>
        /// <param name="ship">The ship.</param>
        internal MasteryShip(SerializableMasteryShip src, Ship ship)
            : base(src?.Masteries.Count ?? 0)
        {
            if (src == null)
                return;

            Ship = ship;

            foreach (SerializableMastery mastery in src.Masteries)
            {
                Items.Add(new Mastery(this, mastery));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MasteryShip"/> class.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <param name="masteryShip">The mastery ship.</param>
        internal MasteryShip(Character character, MasteryShip masteryShip)
            : base(masteryShip?.Count ?? 0)
        {
            if (masteryShip == null)
                return;

            Character = character;
            Ship = masteryShip.Ship;

            foreach (Mastery mastery in masteryShip)
            {
                Items.Add(new Mastery(character, mastery));
            }
        }

        #endregion


        #region Public Properties

        /// <summary>
        /// Gets the character this certificate is bound to.
        /// </summary>
        public Character Character { get; }

        /// <summary>
        /// Gets the ship.
        /// </summary>
        public Ship Ship { get; }

        /// <summary>
        /// Gets the highest trained mastery level.
        /// May be null if no level has been trained.
        /// </summary>
        public Mastery HighestTrainedLevel => Items.LastOrDefault(level => level.IsTrained);

        #endregion

        /// <summary>
        /// Gets the level.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <returns></returns>
        public Mastery GetLevel(int level) => Items.FirstOrDefault(mastery => mastery.Level == level);

        /// <summary>
        /// Initializes the mastery.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">character</exception>
        internal void Initialize()
        {
            while (true)
            {
                bool updatedAnything = Items
                    .Aggregate(false, (current, mastery) => current || mastery.TryUpdateMasteryStatus());

                if (!updatedAnything)
                    break;
            }
        }

    }
}
