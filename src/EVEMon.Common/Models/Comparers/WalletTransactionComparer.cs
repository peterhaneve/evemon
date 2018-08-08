using System;
using System.Collections.Generic;
using EVEMon.Common.SettingsObjects;

namespace EVEMon.Common.Models.Comparers
{
    public sealed class WalletTransactionComparer : Comparer<WalletTransaction>
    {
        private readonly WalletTransactionColumn m_column;
        private readonly bool m_isAscending;

        /// <summary>
        /// Initializes a new instance of the <see cref="WalletTransactionComparer"/> class.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="isAscending">Is ascending flag.</param>
        public WalletTransactionComparer(WalletTransactionColumn column, bool isAscending)
        {
            m_column = column;
            m_isAscending = isAscending;
        }

        /// <summary>
        /// Performs a comparison of two objects of the <see cref="WalletTransaction" /> type and returns a value
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
        public override int Compare(WalletTransaction x, WalletTransaction y)
        {
            if (m_isAscending)
                return CompareCore(x, y);

            return -CompareCore(x, y);
        }

        /// <summary>
        /// Performs a comparison of two objects of the <see cref="WalletTransaction" /> type and returns a value
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
        private int CompareCore(WalletTransaction x, WalletTransaction y)
        {
            switch (m_column)
            {
            case WalletTransactionColumn.Date:
                return x.Date.CompareTo(y.Date);
            case WalletTransactionColumn.ItemName:
                return string.Compare(x.ItemName, y.ItemName, StringComparison.CurrentCulture);
            case WalletTransactionColumn.Price:
                return x.Price.CompareTo(y.Price);
            case WalletTransactionColumn.Quantity:
                return x.Quantity.CompareTo(y.Quantity);
            case WalletTransactionColumn.Credit:
                return x.Credit.CompareTo(y.Credit);
            case WalletTransactionColumn.Client:
                return string.Compare(x.ClientName, y.ClientName,
                    StringComparison.CurrentCulture);
            case WalletTransactionColumn.Location:
                return x.Station.CompareTo(y.Station);
            case WalletTransactionColumn.Region:
                return x.Station.SolarSystemChecked.Constellation.Region.CompareTo(y.
                    Station.SolarSystemChecked.Constellation.Region);
            case WalletTransactionColumn.SolarSystem:
                return x.Station.SolarSystemChecked.CompareTo(y.Station.SolarSystemChecked);
            case WalletTransactionColumn.Station:
                return x.Station.CompareTo(y.Station);
            case WalletTransactionColumn.TransactionFor:
                return x.TransactionFor.CompareTo(y.TransactionFor);
            case WalletTransactionColumn.JournalID:
                return x.JournalID.CompareTo(y.JournalID);
            default:
                return 0;
            }
        }
    }
}
