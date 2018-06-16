using System;

namespace EVEMon.Common.Net
{
    /// <summary>
    /// Container class to return the result of an asynchronous string download
    /// </summary>
    public class DownloadResult<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DownloadResult{T}"/> class.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="error">The error.</param>
        /// <param name="responseCode">The server response code.</param>
        public DownloadResult(T result, HttpWebClientServiceException error, int responseCode = 0)
        {
            Error = error;
            Result = result;
            ResponseCode = responseCode;
            ServerTime = DateTime.UtcNow;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DownloadResult{T}"/> class.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="error">The error.</param>
        /// <param name="responseCode">The server response code.</param>
        /// <param name="serverTime">The time on the server.</param>
        /// <param name="eTag">The previous result's entity tag, if applicable.</param>
        public DownloadResult(T result, HttpWebClientServiceException error, int responseCode,
            DateTime serverTime, DateTime? expires = null, string eTag = null)
        {
            Error = error;
            Result = result;
            ResponseCode = responseCode;
            ServerTime = serverTime;
            Expires = expires;
            ETag = eTag;
        }

        /// <summary>
        /// Gets or sets the response code.
        /// </summary>
        /// <value>The response code.</value>
        public int ResponseCode { get; }

        /// <summary>
        /// Gets or sets the result.
        /// </summary>
        /// <value>The result.</value>
        public T Result { get; }

        /// <summary>
        /// Gets or sets the error.
        /// </summary>
        /// <value>The error.</value>
        public HttpWebClientServiceException Error { get; }

        /// <summary>
        /// Gets the server time.
        /// </summary>
        /// <value>The time on the server, in UTC.</value>
        public DateTime ServerTime { get; }

        /// <summary>
        /// The time at which the result expires
        /// </summary>
        public DateTime? Expires { get; }

        /// <summary>
        /// Entity Tag value - used for If-None-Match header
        /// https://developers.eveonline.com/blog/article/esi-etag-best-practices
        /// </summary>
        public string ETag { get; }
    }
}
