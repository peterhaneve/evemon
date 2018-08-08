using System;
using System.Collections.Generic;
using EVEMon.Common.SettingsObjects;

namespace EVEMon.Common.Models.Comparers
{
    /// <summary>
    /// Performs a comparison between two <see cref="MarketOrder" /> types.
    /// </summary>
    public sealed class MarketOrderComparer : Comparer<MarketOrder>
    {
        private readonly MarketOrderColumn m_column;
        private readonly bool m_isAscending;

        /// <summary>
        /// Initializes a new instance of the <see cref="MarketOrderComparer"/> class.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="isAscending">Is ascending flag.</param>
        public MarketOrderComparer(MarketOrderColumn column, bool isAscending)
        {
            m_column = column;
            m_isAscending = isAscending;
        }

        /// <summary>
        /// Performs a comparison of two objects of the <see cref="MarketOrder" /> type and returns a value
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
        public override int Compare(MarketOrder x, MarketOrder y)
        {
            if (m_isAscending)
                return CompareCore(x, y);

            return -CompareCore(x, y);
        }

        /// <summary>
        /// Performs a comparison of two objects of the <see cref="MarketOrder" /> type and returns a value
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
        private int CompareCore(MarketOrder x, MarketOrder y)
        {
            BuyOrder buyOrderX = x as BuyOrder;
            BuyOrder buyOrderY = y as BuyOrder;

            switch (m_column)
            {
            case MarketOrderColumn.Duration:
                return x.Duration.CompareTo(y.Duration);
            case MarketOrderColumn.Expiration:
                return x.Expiration.CompareTo(y.Expiration);
            case MarketOrderColumn.InitialVolume:
                return x.InitialVolume.CompareTo(y.InitialVolume);
            case MarketOrderColumn.Issued:
                return x.Issued.CompareTo(y.Issued);
            case MarketOrderColumn.IssuedFor:
                return x.IssuedFor.CompareTo(y.IssuedFor);
            case MarketOrderColumn.Item:
                return string.Compare(x.Item.Name, y.Item.Name,
                    StringComparison.CurrentCulture);
            case MarketOrderColumn.ItemType:
                return string.Compare(x.Item.MarketGroup.Name, y.Item.MarketGroup.Name,
                    StringComparison.CurrentCulture);
            case MarketOrderColumn.Location:
                return x.Station.CompareTo(y.Station);
            case MarketOrderColumn.MinimumVolume:
                return x.MinVolume.CompareTo(y.MinVolume);
            case MarketOrderColumn.Region:
                return x.Station.SolarSystemChecked.Constellation.Region.CompareTo(y.Station.
                    SolarSystemChecked.Constellation.Region);
            case MarketOrderColumn.RemainingVolume:
                return x.RemainingVolume.CompareTo(y.RemainingVolume);
            case MarketOrderColumn.SolarSystem:
                return x.Station.SolarSystemChecked.CompareTo(y.Station.SolarSystemChecked);
            case MarketOrderColumn.Station:
                return x.Station.CompareTo(y.Station);
            case MarketOrderColumn.TotalPrice:
                return x.TotalPrice.CompareTo(y.TotalPrice);
            case MarketOrderColumn.UnitaryPrice:
                return x.UnitaryPrice.CompareTo(y.UnitaryPrice);
            case MarketOrderColumn.Volume:
                // Compare the percent left
                return ((double)x.RemainingVolume / x.InitialVolume).CompareTo((double)y.
                    RemainingVolume / y.InitialVolume);
            case MarketOrderColumn.LastStateChange:
                return x.LastStateChange.CompareTo(y.LastStateChange);
            case MarketOrderColumn.OrderRange:
                // Compare applies only to BuyOrder 
                return buyOrderX?.Range.CompareTo(buyOrderY?.Range) ?? 0;
            case MarketOrderColumn.Escrow:
                // Compare applies only to BuyOrder 
                return buyOrderX?.Escrow.CompareTo(buyOrderY?.Escrow) ?? 0;
            default:
                return 0;
            }
        }
    }
}
