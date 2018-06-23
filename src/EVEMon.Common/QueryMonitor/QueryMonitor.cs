using System;
using EVEMon.Common.Attributes;
using EVEMon.Common.Enumerations;
using EVEMon.Common.Enumerations.UISettings;
using EVEMon.Common.Extensions;
using EVEMon.Common.Interfaces;
using EVEMon.Common.Models;
using EVEMon.Common.Net;
using EVEMon.Common.Serialization.Eve;
using EVEMon.Common.SettingsObjects;

namespace EVEMon.Common.QueryMonitor
{
    /// <summary>
    /// This class monitors a querying process. It provides services for autoupdating, update
    /// notification, and querying character data.
    /// </summary>
    [EnforceUIThreadAffinity]
    public class QueryMonitor<T> : IQueryMonitorEx, INetworkChangeSubscriber where T : class
    {
        // Matches the error reporting methods in GlobalNotificationCollection
        internal delegate void NotifyErrorCallback(CCPCharacter character, EsiResult<T> result);

        private readonly Action<EsiResult<T>> m_onUpdated;

        private bool m_forceUpdate;
        private bool m_isCanceled;
        protected ResponseParams m_lastResponse;
        private bool m_retryOnForceUpdateError;


        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="callback">The callback.</param>
        /// <exception cref="System.ArgumentNullException">callback;@The callback cannot be null.</exception>
        internal QueryMonitor(Enum method, Action<EsiResult<T>> callback)
        {
            // Check callback not null
            callback.ThrowIfNull(nameof(callback), "The callback cannot be null.");

            LastUpdate = DateTime.MinValue;
            m_forceUpdate = true;
            m_lastResponse = null;
            m_onUpdated = callback;
            Method = method;
            Enabled = false;
            QueryOnStartup = false;

            NetworkMonitor.Register(this);

            EveMonClient.TimerTick += EveMonClient_TimerTick;
        }

        #endregion


        #region Properties

        /// <summary>
        /// Gets true if the query is enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets true whether the monitor has to do a query on application startup.
        /// </summary>
        public bool QueryOnStartup { get; set; }

        /// <summary>
        /// Gets the API method monitored by this instance.
        /// </summary>
        public Enum Method { get; }

        /// <summary>
        /// Gets the last time this instance was updated (UTC).
        /// </summary>
        public DateTime LastUpdate { get; private set; }

        /// <summary>
        /// Gets the status of the query.
        /// </summary>
        public QueryStatus Status { get; private set; }

        /// <summary>
        /// Gets true when the API provider is not CCP or the cache timer has expired.
        /// </summary>
        public bool CanForceUpdate
        {
            get
            {
                if (EveMonClient.APIProviders.CurrentProvider != APIProvider.DefaultProvider &&
                    EveMonClient.APIProviders.CurrentProvider != APIProvider.TestProvider)
                    return true;

                return DateTime.UtcNow > (LastResult?.CachedUntil ?? NextUpdate);
            }
        }

        /// <summary>
        /// Gets the next time this instance should be updated (UTC), based on both the CCP cache time and the user preferences.
        /// </summary>
        public DateTime NextUpdate
        {
            get
            {
                // If there was an error on last try, we use the cached time
                if (LastResult != null && LastResult.HasError)
                    return LastResult.CachedUntil;

                // No error ? Then we compute the next update according to the settings
                UpdatePeriod period = UpdatePeriod.Never;
                if (Settings.Updates.Periods.ContainsKey(Method.ToString()))
                    period = Settings.Updates.Periods[Method.ToString()];

                if (period == UpdatePeriod.Never)
                    return DateTime.MaxValue;

                DateTime nextUpdate = LastUpdate.Add(period.ToDuration());

                // If CCP "cached until" is greater than what we computed, return CCP cached time
                if (LastResult != null && LastResult.CachedUntil > nextUpdate)
                    return LastResult.CachedUntil;

                return nextUpdate;
            }
        }

        /// <summary>
        /// Gets the last result queried from the API provider.
        /// </summary>
        public EsiResult<T> LastResult { get; private set; }

        /// <summary>
        /// Gets true whether the method is curently being requeried.
        /// </summary>
        public bool IsUpdating { get; private set; }

        /// <summary>
        /// Gets true when the monitor encountered an error on last try.
        /// </summary>
        public bool HasError => LastResult != null && LastResult.HasError;

        /// <summary>
        /// Gets true if this monitor has access to data.
        /// </summary>
        public virtual bool HasAccess => true;

        /// <summary>
        /// Gets the required API key information are known.
        /// </summary>
        /// <returns>False if an API key was required and not found.</returns>
        protected virtual bool HasESIKey => true;

        #endregion


        #region  Event Handlers


        /// <summary>
        /// Handles the TimerTick event of the EveMonClient control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void EveMonClient_TimerTick(object sender, EventArgs e)
        {
            UpdateOnOneSecondTick();
        }

        #endregion


        /// <summary>
        /// Called when the object gets disposed.
        /// </summary>
        public void Dispose()
        {
            EveMonClient.TimerTick -= EveMonClient_TimerTick;
        }

        /// <summary>
        /// Manually updates this monitor with the provided data, like if it has just been
        /// updated from CCP.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <remarks>
        /// This method does not fire any event.
        /// </remarks>
        internal void UpdateWith(EsiResult<T> result)
        {
            LastResult = result;
            LastUpdate = DateTime.UtcNow;
        }

        /// <summary>
        /// Forces an update.
        /// </summary>
        internal void ForceUpdate(bool retryOnError)
        {
            m_forceUpdate = true;
            m_retryOnForceUpdateError |= retryOnError;
        }

        /// <summary>
        /// Updates on every second.
        /// </summary>
        private void UpdateOnOneSecondTick()
        {
            // Are we already updating?
            if (!IsUpdating)
            {
                m_isCanceled = false;
                if (!Enabled)
                    // Monitor is disabled
                    Status = QueryStatus.Disabled;
                else if (!NetworkMonitor.IsNetworkAvailable)
                    // No network connection
                    Status = QueryStatus.NoNetwork;
                else if (!HasESIKey)
                    // No valid ESI key
                    Status = QueryStatus.NoESIKey;
                else if (!HasAccess)
                    // This ESI key does not have access
                    Status = QueryStatus.NoAccess;
                else if (!m_forceUpdate && NextUpdate > DateTime.UtcNow)
                    // Is it an auto-update test?
                    // If not due time yet, quits
                    Status = QueryStatus.Pending;
                else
                {
                    // Start the update
                    IsUpdating = true;
                    Status = QueryStatus.Updating;
                    QueryAsyncCore(EveMonClient.APIProviders.CurrentProvider, OnQueried);
                }
            }
        }

        /// <summary>
        /// Performs the query to the provider, passing the required arguments.
        /// </summary>
        /// <param name="provider">The API provider to use.</param>
        /// <param name="callback">The callback invoked on the UI thread after a result has
        /// been queried.</param>
        /// <exception cref="System.ArgumentNullException">provider</exception>
        protected virtual void QueryAsyncCore(APIProvider provider, APIProvider.
            ESIRequestCallback<T> callback)
        {
            provider.ThrowIfNull(nameof(provider));

            provider.QueryEsi(Method, callback, new ESIParams(m_lastResponse));
        }

        /// <summary>
        /// Occurs when a new result has been queried.
        /// </summary>
        /// <param name="result">The downloaded result</param>
        private void OnQueried(EsiResult<T> result, object state)
        {
            IsUpdating = false;
            Status = QueryStatus.Pending;

            // Do we need to retry the force update ?
            m_forceUpdate = m_retryOnForceUpdateError && result.HasError;

            if (!m_isCanceled)
            {
                // Updates the stored data
                m_retryOnForceUpdateError = false;
                LastUpdate = DateTime.UtcNow;
                LastResult = result;
                m_lastResponse = result.Response;
                // Notify subscribers
                if (result.HasData)
                    m_onUpdated?.Invoke(result);
            }
        }

        /// <summary>
        /// Resets the monitor with the given last update time.
        /// </summary>
        /// <param name="lastUpdate">The UTC time of the last update.</param>
        private void Reset(DateTime lastUpdate)
        {
            Cancel();
            LastUpdate = lastUpdate;
            LastResult = null;
        }

        /// <summary>
        /// Cancels the running update.
        /// </summary>
        private void Cancel()
        {
            m_isCanceled = true;
            m_forceUpdate = false;
        }

        /// <summary>
        /// Set the network availability.
        /// </summary>
        protected bool SetNetworkStatus { get; set; }


        #region Overridden Methods

        /// <summary>
        /// Gets the bound method header.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Method.HasHeader() ? Method.GetHeader() : Method.
            ToString();

        #endregion


        #region Interfaces implementations

        bool INetworkChangeSubscriber.SetNetworkStatus
        {
            get { return SetNetworkStatus; }
            set { SetNetworkStatus = value; }
        }

        void IQueryMonitorEx.Reset(DateTime lastUpdate)
        {
            Reset(lastUpdate);
        }

        void IQueryMonitorEx.ForceUpdate(bool retryOnError)
        {
            ForceUpdate(retryOnError);
        }

        IAPIResult IQueryMonitor.LastResult => LastResult;

        #endregion

    }
}
