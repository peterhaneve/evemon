using System;
using System.Collections.Generic;
using EVEMon.Common.SettingsObjects;
using EVEMon.Common.Data;

namespace EVEMon.Common.Models.Comparers
{
    /// <summary>
    /// Performs a comparison between two <see cref="Contract"/> types.
    /// </summary>
    public sealed class ContractComparer : Comparer<Contract>
    {
        private readonly ContractColumn m_column;
        private readonly bool m_isAscending;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractComparer"/> class.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="isAscending">Is ascending flag.</param>
        public ContractComparer(ContractColumn column, bool isAscending)
        {
            m_column = column;
            m_isAscending = isAscending;
        }

        /// <summary>
        /// Performs a comparison of two objects of the <see cref="Contract" /> type and returns a value
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
        public override int Compare(Contract x, Contract y)
        {
            if (m_isAscending)
                return CompareCore(x, y);

            return -CompareCore(x, y);
        }

        /// <summary>
        /// Performs a comparison of two objects of the <see cref="Contract" /> type and returns a value
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
        private int CompareCore(Contract x, Contract y)
        {
            Station xStart = x?.StartStation, yStart = y?.StartStation, xEnd = x?.EndStation,
                yEnd = y?.EndStation;
            switch (m_column)
            {
            case ContractColumn.Status:
                return x.Status.CompareTo(y.Status);
            case ContractColumn.ContractText:
                return String.Compare(x.ContractText, y.ContractText, StringComparison.CurrentCulture);
            case ContractColumn.ContractType:
                return x.ContractType.CompareTo(y.ContractType);
            case ContractColumn.Issuer:
                return String.Compare(x.Issuer, y.Issuer, StringComparison.CurrentCulture);
            case ContractColumn.Assignee:
                return String.Compare(x.Assignee, y.Assignee, StringComparison.CurrentCulture);
            case ContractColumn.Issued:
                return x.Issued.CompareTo(y.Issued);
            case ContractColumn.Expiration:
                return x.Expiration.CompareTo(y.Expiration);
            case ContractColumn.Title:
                return String.Compare(x.Description, y.Description, StringComparison.CurrentCulture);
            case ContractColumn.Acceptor:
                return String.Compare(x.Acceptor, y.Acceptor, StringComparison.CurrentCulture);
            case ContractColumn.Availability:
                return x.Availability.CompareTo(y.Availability);
            case ContractColumn.Price:
                return x.Price.CompareTo(y.Price);
            case ContractColumn.Buyout:
                return x.Buyout.CompareTo(y.Buyout);
            case ContractColumn.Reward:
                return x.Reward.CompareTo(y.Reward);
            case ContractColumn.Collateral:
                return x.Collateral.CompareTo(y.Collateral);
            case ContractColumn.Volume:
                return x.Volume.CompareTo(y.Volume);
            case ContractColumn.StartLocation:
                return xStart.CompareTo(yStart);
            case ContractColumn.StartRegion:
                return xStart.SolarSystemChecked.Constellation.Region.CompareTo(yStart.
                    SolarSystemChecked.Constellation.Region);
            case ContractColumn.StartSolarSystem:
                return xStart.SolarSystemChecked.CompareTo(yStart.SolarSystemChecked);
            case ContractColumn.StartStation:
                return xStart.CompareTo(yStart);
            case ContractColumn.EndLocation:
                return xEnd.CompareTo(yEnd);
            case ContractColumn.EndRegion:
                return xEnd.SolarSystemChecked.Constellation.Region.CompareTo(yEnd.
                    SolarSystemChecked.Constellation.Region);
            case ContractColumn.EndSolarSystem:
                return xEnd.SolarSystemChecked.CompareTo(yEnd.SolarSystemChecked);
            case ContractColumn.EndStation:
                return xEnd.CompareTo(yEnd);
            case ContractColumn.Accepted:
                return x.Accepted.CompareTo(y.Accepted);
            case ContractColumn.Completed:
                return x.Completed.CompareTo(y.Completed);
            case ContractColumn.Duration:
                return x.Duration.CompareTo(y.Duration);
            case ContractColumn.DaysToComplete:
                return x.DaysToComplete.CompareTo(y.DaysToComplete);
            case ContractColumn.IssuedFor:
                return x.IssuedFor.CompareTo(y.IssuedFor);
            default:
                return 0;
            }
        }
    }
}
