﻿using System;

namespace EVEMon.Common.Models
{
    public class AccountStatus
    {
        public enum AccountStatusType { Unknown, Alpha, Omega };
        
        private const float trainingRateUnknown = 1.0f;
        private const float trainingRateAlpha = 0.5f;
        private const float trainingRateOmega = 1.0f;

        /// <summary>
        /// Creates an AccountStatus object with defined type
        /// </summary>
        /// <param name="statusType">Type (Alpha, Omega, Unknown).</param>
        public AccountStatus(AccountStatusType statusType)
        {
            CurrentStatus = statusType;
        }

        /// <summary>
        /// Spits out a friendly name for the Account Status
        /// </summary>
        public override string ToString()
        {
            string accountTypeName;

            switch (CurrentStatus)
            {
                case AccountStatusType.Unknown:
                    accountTypeName = "Unknown";
                    break;
                case AccountStatusType.Alpha:
                    accountTypeName = "Alpha";
                    break;
                case AccountStatusType.Omega:
                    accountTypeName = "Omega";
                    break;
                default:
                    accountTypeName = "Unknown";
                    break;
            }
            return accountTypeName;
        }

        public AccountStatusType CurrentStatus { get; private set; }

        public bool IsAlpha
        {
            get
            {
                return CurrentStatus == AccountStatusType.Alpha;
            }
        }

        /// <summary>
        /// Returns training rate adjusted for account status
        /// </summary>
        /// <returns>The skill point accrual rate (1.0 = base) modifier</returns>
        public float TrainingRate
        {
            get
            {
                float rate = trainingRateUnknown;
                switch (CurrentStatus)
                {
                    case AccountStatusType.Alpha:
                        rate = trainingRateAlpha;
                        break;
                    case AccountStatusType.Omega:
                        rate = trainingRateOmega;
                        break;
                    case AccountStatusType.Unknown:
                        rate = trainingRateUnknown;
                        break;
                }
                return rate;
            }
        }
    }
}
