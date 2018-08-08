using EVEMon.Common.Extensions;
using System.Collections.Generic;

namespace EVEMon.Common.Service
{
    /// <summary>
    /// A class used to provide ID lookup services.
    /// </summary>
    internal abstract class IDToObjectProvider<T, X> where T : class where X : class
    {
        /// <summary>
        /// Reference to the master cache list.
        /// </summary>
        protected readonly IDictionary<long, IDInformation<T, X>> m_cache;

        /// <summary>
        /// List of IDs awaiting query. No duplicates allowed, and in ascending order for
        /// the picky API calls that need it that way.
        /// </summary>
        protected readonly SortedDictionary<long, X> m_pendingIDs;

        /// <summary>
        /// Is a query currently running?
        /// </summary>
        protected volatile bool m_queryPending;

        protected IDToObjectProvider(IDictionary<long, IDInformation<T, X>> cache)
        {
            cache.ThrowIfNull(nameof(cache));

            m_cache = cache;
            m_pendingIDs = new SortedDictionary<long, X>();
            m_queryPending = false;
        }

        /// <summary>
        /// Creates a new IDInformation object for this provider.
        /// </summary>
        /// <param name="id">The ID which needs information.</param>
        /// <param name="value">The prefilled/prefetched value, or default(T) if it is not
        /// yet known</param>
        /// <returns>A new instance of ID information for the specified ID.</returns>
        protected abstract IDInformation<T, X> CreateIDInfo(long id, T value);

        /// <summary>
        /// Evict as many IDs as can be handled at once from m_pendingIDs and update
        /// m_cache with the new mappings. Call OnLookupComplete in callback.
        /// </summary>
        protected abstract void FetchIDs();

        /// <summary>
        /// Checks (without locking) to see if the ID needs to be queried. Inserts a new
        /// record into the table if needed with blank information.
        /// </summary>
        /// <param name="id">The ID to check.</param>
        /// <param name="extra">The data to use while making the request.</param>
        /// <param name="value">The result which is currently known for this ID.</param>
        /// <returns>true if the ID needs to be queried, or false otherwise</returns>
        private bool IsNeeded(long id, X extra, out T value)
        {
            bool needsUpdate;
            IDInformation<T, X> currentInfo;
            m_cache.TryGetValue(id, out currentInfo);
            if (currentInfo != null)
            {
                // Check if request was attempted
                needsUpdate = !currentInfo.RequestAttempted(extra);
                value = currentInfo.Value;
            }
            else
            {
                // Never seen before, force an update
                needsUpdate = true;
                value = default(T);
                m_cache.Add(id, CreateIDInfo(id, value));
            }
            return needsUpdate;
        }

        /// <summary>
        /// Convert the ID to an object.
        /// </summary>
        /// <param name="id">The ID (type depends on implementation)</param>
        /// <param name="bypass">true to bypass the Prefetch filter, or false (default) to
        /// use it (recommended in most cases)</param>
        /// <returns>The object, or null if no item with this ID exists</returns>
        public T LookupID(long id, bool bypass = false)
        {
            return LookupID(id, bypass, default(X));
        }

        /// <summary>
        /// Convert the ID to an object. This is the raw version of LookupID which accepts the
        /// value of X provided by a subclass that uses the parameter.
        /// </summary>
        /// <param name="id">The ID (type depends on implementation)</param>
        /// <param name="bypass">true to bypass the Prefetch filter, or false to use it
        /// (recommended in most cases)</param>
        /// <param name="extra">The extra data to associate with this request</param>
        /// <returns>The object, or null if no item with this ID exists</returns>
        protected T LookupID(long id, bool bypass, X extra)
        {
            T value;
            bool needsUpdate;
            // Thread safety
            lock (m_cache)
            {
                // Queue update if necessary
                needsUpdate = (bypass || (value = Prefetch(id)) == default(T)) && IsNeeded(id,
                    extra, out value) && QueueID(id, extra);
            }
            if (needsUpdate)
                // No query running and a new one needs to be started; note that new queries
                // will be started even for IDs in the cache if they need to be updated
                FetchIDs();
            return value;
        }

        /// <summary>
        /// Convert the IDs to objects.
        /// </summary>
        /// <param name="id">The ID (type depends on implementation)</param>
        /// <returns>The objects, with null for each item where no match exists</returns>
        public IEnumerable<T> LookupAllID(IEnumerable<long> ids)
        {
            T value;
            bool start = false;
            var ret = new LinkedList<T>();
            // Thread safety
            lock (m_cache)
            {
                foreach (var id in ids)
                {
                    // Queue update if necessary
                    if ((value = Prefetch(id)) == default(T) && IsNeeded(id, default(X),
                        out value) && QueueID(id)) start = true;
                    ret.AddLast(value);
                }
            }
            // One query for many IDs
            if (start)
                FetchIDs();
            return ret;
        }

        /// <summary>
        /// Called by subclasses when an ID lookup completes, whether entries remain or not.
        /// </summary>
        protected void OnLookupComplete()
        {
            bool done = false;

            // No more?
            lock (m_pendingIDs)
            {
                done = m_pendingIDs.Count <= 0;
                if (done)
                    m_queryPending = false;
            }

            if (done)
                // Tell everyone we have updates
                TriggerEvent();
            else
                // Go again
                FetchIDs();
        }

        /// <summary>
        /// Called before any item is added to the queue; if the item can be resolved locally
        /// with no cache lookup cheaply, this method should do it.
        /// </summary>
        /// <param name="id">The ID.</param>
        /// <returns>The value that should be returned for this ID, or null if no local match
        /// is found.</returns>
        protected virtual T Prefetch(long id)
        {
            return default(T);
        }

        /// <summary>
        /// Adds a predefined ID and object to this provider. It will add the entry to the
        /// cache and mark it as requested so that this ID will not be sent upstream.
        /// </summary>
        /// <param name="id">The ID.</param>
        /// <param name="value">The value that should be returned for this ID.</param>
        public void Prefill(long id, T value)
        {
            var info = CreateIDInfo(id, value);
            // Overwrite if it exists
            if (m_cache.ContainsKey(id))
                m_cache[id] = info;
            else
                m_cache.Add(id, info);
        }

        /// <summary>
        /// Starts querying for an ID lookup with whatever is in the list.
        /// </summary>
        private bool QueueID(long id, X extra = null)
        {
            // Need to add to the requirements list
            bool startQuery = false;

            lock (m_pendingIDs)
            {
                if (m_pendingIDs.ContainsKey(id))
                {
                    // If there is an entry without a value, add the extra value
                    if (extra != null && m_pendingIDs[id] == null)
                        m_pendingIDs[id] = extra;
                }
                else
                    m_pendingIDs.Add(id, extra);

                if (!m_queryPending)
                {
                    m_queryPending = true;
                    startQuery = true;
                }
            }

            return startQuery;
        }

        /// <summary>
        /// Triggers the proper EVEMon event when updates are completed.
        /// </summary>
        protected abstract void TriggerEvent();
    }

    /// <summary>
    /// Describes an object with information about an ID and what has been done to attempt it.
    /// </summary>
    internal interface IDInformation<T, X> where T : class where X : class
    {
        /// <summary>
        /// The ID which was used to fetch this information.
        /// </summary>
        long ID { get; }

        /// <summary>
        /// The information retrieved, or default(T) if the request has failed or not yet
        /// been attempted.
        /// </summary>
        T Value { get; }

        /// <summary>
        /// Called when a request completes or fails.
        /// </summary>
        /// <param name="result">The request result, or default(T) if it failed.</param>
        void OnRequestComplete(T result);

        /// <summary>
        /// Called when a request begins for the specified ID.
        /// </summary>
        /// <param name="extra">The extra data to be used on the request.</param>
        void OnRequestStart(X extra);

        /// <summary>
        /// Returns true if a request was already attempted using this information. If true,
        /// another request will not be attempted (unless the request is currently in the
        /// queue).
        /// 
        /// Should return false if the ID was loaded from a cache.
        /// </summary>
        /// <param name="extra">The extra information to use for the request.</param>
        /// <returns>Whether a request was attempted already in this session.</returns>
        bool RequestAttempted(X extra);
    }
}
