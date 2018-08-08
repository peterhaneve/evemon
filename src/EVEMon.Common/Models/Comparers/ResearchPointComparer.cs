using System;
using System.Collections.Generic;
using EVEMon.Common.SettingsObjects;

namespace EVEMon.Common.Models.Comparers
{
    /// <summary>
    /// Performs a comparison between two <see cref="ResearchPoint"/> types.
    /// </summary>
    public sealed class ResearchPointComparer : Comparer<ResearchPoint>
    {
        private readonly ResearchColumn m_column;
        private readonly bool m_isAscending;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResearchPointComparer"/> class.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="isAscending">Is ascending flag.</param>
        public ResearchPointComparer(ResearchColumn column, bool isAscending)
        {
            m_column = column;
            m_isAscending = isAscending;
        }

        /// <summary>
        /// Performs a comparison of two objects of the <see cref="ResearchPoint" /> type and returns a value
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
        public override int Compare(ResearchPoint x, ResearchPoint y)
        {
            if (m_isAscending)
                return CompareCore(x, y);

            return -CompareCore(x, y);
        }

        /// <summary>
        /// Performs a comparison of two objects of the <see cref="ResearchPoint" /> type and returns a value
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
        private int CompareCore(ResearchPoint x, ResearchPoint y)
        {
            switch (m_column)
            {
            case ResearchColumn.Agent:
                return string.Compare(x.AgentName, y.AgentName,
                    StringComparison.CurrentCulture);
            case ResearchColumn.Field:
                return string.Compare(x.Field, y.Field, StringComparison.CurrentCulture);
            case ResearchColumn.Level:
                return x.AgentLevel.CompareTo(y.AgentLevel);
            case ResearchColumn.CurrentRP:
                return x.CurrentRP.CompareTo(y.CurrentRP);
            case ResearchColumn.PointsPerDay:
                return x.PointsPerDay.CompareTo(y.PointsPerDay);
            case ResearchColumn.StartDate:
                return x.StartDate.CompareTo(y.StartDate);
            case ResearchColumn.Location:
                return x.Station.CompareTo(y.Station);
            case ResearchColumn.Region:
                return x.Station.SolarSystemChecked.Constellation.Region.CompareTo(y.Station.
                    SolarSystemChecked.Constellation.Region);
            case ResearchColumn.SolarSystem:
                return x.Station.SolarSystemChecked.CompareTo(y.Station.SolarSystemChecked);
            case ResearchColumn.Station:
                return x.Station.CompareTo(y.Station);
            default:
                return 0;
            }
        }
    }
}
