using System;
using System.Collections.Generic;
using EVEMon.Common.Enumerations;
using EVEMon.Common.SettingsObjects;

namespace EVEMon.Common.Models.Comparers
{
    /// <summary>
    /// Performs a comparison between two <see cref="IndustryJob" /> types.
    /// </summary>
    public sealed class IndustryJobComparer : Comparer<IndustryJob>
    {
        private readonly IndustryJobColumn m_column;
        private readonly bool m_isAscending;

        /// <summary>
        /// Initializes a new instance of the <see cref="IndustryJobComparer"/> class.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="isAscending">Is ascending flag.</param>
        public IndustryJobComparer(IndustryJobColumn column, bool isAscending)
        {
            m_column = column;
            m_isAscending = isAscending;
        }

        /// <summary>
        /// Performs a comparison of two objects of the <see cref="IndustryJob" /> type and returns a value
        /// indicating whether one object is less than, equal to, or greater than the other.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>
        /// Less than zero
        /// <paramref name="x"/> is less than <paramref name="y"/>.
        /// Zero
        /// <paramref name="x"/> equals <paramref name="y"/>.
        /// Greater than zero
        /// <paramref name="x"/> is greater than <paramref name="y"/>.
        /// </returns>
        public override int Compare(IndustryJob x, IndustryJob y)
        {
            if (m_isAscending)
                return CompareCore(x, y);

            return -CompareCore(x, y);
        }

        /// <summary>
        /// Performs a comparison of two objects of the <see cref="IndustryJob" /> type and returns a value
        /// indicating whether one object is less than, equal to, or greater than the other.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>
        /// Less than zero
        /// <paramref name="x"/> is less than <paramref name="y"/>.
        /// Zero
        /// <paramref name="x"/> equals <paramref name="y"/>.
        /// Greater than zero
        /// <paramref name="x"/> is greater than <paramref name="y"/>.
        /// </returns>
        private int CompareCore(IndustryJob x, IndustryJob y)
        {
            switch (m_column)
            {
                case IndustryJobColumn.State:
                    return x.State == JobState.Active ? x.ActiveJobState.CompareTo(y.ActiveJobState) : x.State.CompareTo(y.State);
                case IndustryJobColumn.TTC:
                    return x.EndDate.CompareTo(y.EndDate);
                case IndustryJobColumn.InstalledItem:
                    return String.Compare(x.InstalledItem.Name, y.InstalledItem.Name, StringComparison.CurrentCulture);
                case IndustryJobColumn.InstalledItemType:
                    return String.Compare(x.InstalledItem.MarketGroup.Name, y.InstalledItem.MarketGroup.Name,
                                          StringComparison.CurrentCulture);
                case IndustryJobColumn.OutputItem:
                    return String.Compare(x.OutputItem.Name, y.OutputItem.Name, StringComparison.CurrentCulture);
                case IndustryJobColumn.OutputItemType:
                    return String.Compare(x.OutputItem.MarketGroup.Name, y.OutputItem.MarketGroup.Name,
                                          StringComparison.CurrentCulture);
                case IndustryJobColumn.Activity:
                    return x.Activity.CompareTo(y.Activity);
                case IndustryJobColumn.InstallTime:
                    return x.InstalledTime.CompareTo(y.InstalledTime);
                case IndustryJobColumn.EndTime:
                    return x.EndDate.CompareTo(y.EndDate);
                case IndustryJobColumn.Location:
                    // null is allowed here
                    return String.Compare(x.FullLocation, y.FullLocation, StringComparison.CurrentCulture);
                case IndustryJobColumn.Region:
                    return x.SolarSystem.Constellation.Region.CompareTo(y.SolarSystem.Constellation.Region);
                case IndustryJobColumn.SolarSystem:
                    // SolarSystem is not null even if location is unknown
                    return x.SolarSystem.CompareTo(y.SolarSystem);
                case IndustryJobColumn.Installation:
                    return String.Compare(x.Installation, y.Installation, StringComparison.CurrentCulture);
                case IndustryJobColumn.IssuedFor:
                    return x.IssuedFor.CompareTo(y.IssuedFor);
                case IndustryJobColumn.LastStateChange:
                    return x.LastStateChange.CompareTo(y.LastStateChange);
                case IndustryJobColumn.Cost:
                    return x.Cost.CompareTo(y.Cost);
                case IndustryJobColumn.Probability:
                    return x.Probability.CompareTo(y.Probability);
                default:
                    return 0;
            }
        }
    }
}
