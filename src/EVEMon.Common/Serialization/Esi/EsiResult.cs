using System;
using EVEMon.Common.Net;

namespace EVEMon.Common.Serialization.Eve
{
    /// <summary>
    /// Wraps the results returned from an ESI request.
    /// </summary>
    public sealed class EsiResult<T> : JsonResult<T>, IAPIResult
    {

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public EsiResult(int responseCode, T result = default(T)) : base(responseCode, result)
        {
            CachedUntil = Expires ?? CurrentTime;
        }
        
        /// <summary>
        /// Constructor from an http exception
        /// </summary>
        /// <param name="exception">The exception.</param>
        public EsiResult(HttpWebClientServiceException exception) : base(exception)
        {
            CachedUntil = Expires ?? CurrentTime;
        }
        
        /// <summary>
        /// Constructor from wrapping a JSON result.
        /// </summary>
        /// <param name="wrapped">The result to wrap.</param>
        public EsiResult(JsonResult<T> wrapped) : base(wrapped)
        {
            CachedUntil = wrapped.Expires ?? wrapped.CurrentTime;
        }

        #endregion


        #region Properties

        public DateTime CachedUntil;

        public int ErrorCode => ResponseCode;

        #endregion
        
    }
}
