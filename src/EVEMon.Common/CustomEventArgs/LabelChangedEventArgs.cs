using EVEMon.Common.Models;
using System;
using System.Collections.Generic;

namespace EVEMon.Common.CustomEventArgs
{
    public sealed class LabelChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <param name="allLabels">The list of all valid labels.</param>
        public LabelChangedEventArgs(Character character, IEnumerable<string> allLabels)
        {
            Character = character;
            AllLabels = allLabels;
        }

        /// <summary>
        /// Gets the character related to this event.
        /// </summary>
        public Character Character { get; }

        /// <summary>
        /// Gets the list of all valid labels, computed once to save resources.
        /// </summary>
        public IEnumerable<string> AllLabels { get; }
    }
}
