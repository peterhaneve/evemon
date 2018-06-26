using System;
using EVEMon.Common.Net;
using System.Net;

namespace EVEMon.Common.Serialization.Eve
{
    /// <summary>
    /// Wraps the results returned from an ESI request.
    /// </summary>
    public sealed class EsiResult<T> : JsonResult<T>, IAPIResult
    {

        #region Helpers

        /// <summary>
        /// Calculates the cached until time based on the response parameters. Uses the expires
        /// date if provided and in the future; otherwise calculates a default expiration to
        /// avoid spamming the server.
        /// </summary>
        /// <param name="response">The server response.</param>
        private static DateTime GetCacheTimerFromResponse(ResponseParams response)
        {
            DateTime cachedUntil;
            DateTimeOffset? expires = response.Expires;
            // If there was an error or no cache timer provided, retry after error cache time
            if (expires == null)
                cachedUntil = GetErrorCacheTime();
            else
            {
                expires = expires.AddSeconds(2.0);
                DateTimeOffset ccpCacheTime = ((DateTimeOffset)expires), serverTime =
                    response.Time ?? DateTimeOffset.UtcNow;
                // Ensure that cache date is not in the past
                if (ccpCacheTime < serverTime)
                    ccpCacheTime = serverTime.AddSeconds(10.0);
                cachedUntil = ccpCacheTime.UtcDateTime;
            }
            return cachedUntil;
        }

        /// <summary>
        /// Reports the time to be used for caching if an error occurs. This prevents spamming
        /// CCP on error requests and running out the error limit.
        /// </summary>
        /// <returns>The time when an error request expires and should be retried</returns>
        private static DateTime GetErrorCacheTime() => DateTime.UtcNow.AddMinutes(2.0);

        #endregion


        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public EsiResult(ResponseParams response, T result = default(T)) : base(response,
            result)
        {
            CachedUntil = GetCacheTimerFromResponse(response);
            HasData = response.ResponseCode != (int)HttpStatusCode.NotModified;
        }
        
        /// <summary>
        /// Constructor from an http exception
        /// </summary>
        /// <param name="exception">The exception.</param>
        public EsiResult(HttpWebClientServiceException exception) : base(exception)
        {
            CachedUntil = GetErrorCacheTime();
            HasData = false;
        }
        
        /// <summary>
        /// Constructor from wrapping a JSON result.
        /// </summary>
        /// <param name="wrapped">The result to wrap.</param>
        public EsiResult(JsonResult<T> wrapped) : base(wrapped)
        {
            CachedUntil = GetCacheTimerFromResponse(wrapped.Response);
            HasData = wrapped.ResponseCode != (int)HttpStatusCode.NotModified;
        }

        #endregion


        #region Properties

        public DateTime CachedUntil { get; }

        public int ErrorCode => ResponseCode;

        public bool HasData { get; }

        #endregion
        
    }
}
