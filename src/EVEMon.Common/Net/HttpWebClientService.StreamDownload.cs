using EVEMon.Common.Enumerations;
using EVEMon.Common.Extensions;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace EVEMon.Common.Net
{
    static partial class HttpWebClientService
    {
        private const string StreamAccept = "application/json;q=0.9,text/plain;q=0.8,*/*;q=0.5";

        /// <summary>
        /// Passed to DownloadStreamAsync to process the stream into actual data.
        /// </summary>
        /// <typeparam name="T">The type to decode.</typeparam>
        /// <param name="stream">The input stream to read.</param>
        /// <param name="responseCode">The HTTP response code returned by the server.</param>
        /// <returns>The decoded value, or default(T) if none could be parsed.</returns>
        public delegate T ParseDataDelegate<T>(Stream stream, int responseCode);

        /// <summary>
        /// Asynchronously downloads an object (streaming) from the specified url.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="acceptEncoded">if set to <c>true</c> accept encoded response.</param>
        /// <param name="postData">The post data. If null, GET will be used.</param>
        /// <param name="parser">The function which will parse the stream.</param>
        /// <param name="token">The ESI token, or null if none is used.</param>
        /// <param name="eTag">The previous result's entity tag, if applicable.</param>
        public static async Task<DownloadResult<T>> DownloadStreamAsync<T>(Uri url,
            ParseDataDelegate<T> parser, bool acceptEncoded = false,
            HttpPostData postData = null, string token = null, string eTag = null)
        {
            string urlValidationError;
            if (!IsValidURL(url, out urlValidationError))
                throw new ArgumentException(urlValidationError);

            HttpClientServiceRequest request = new HttpClientServiceRequest();
            request.AuthToken = token;
            request.AcceptEncoded = acceptEncoded;
            request.DataCompression = postData?.Compression ?? DataCompression.None;
            try
            {
                var response = await request.SendAsync(url, (postData == null) ?
                    HttpMethod.Get : HttpMethod.Post, postData, StreamAccept, eTag).
                    ConfigureAwait(false);

                using (response)
                {
                    var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                    return GetResult(url, stream, parser, response);
                }
            }
            catch (HttpWebClientServiceException ex)
            {
                return new DownloadResult<T>(default(T), ex);
            }
        }

        /// <summary>
        /// Helper method to return an object from the completed request stream.
        /// </summary>
        /// <param name="requestBaseUrl">The request base URL.</param>
        /// <param name="stream">The stream.</param>
        /// <param name="parser">The function which will parse the stream.</param>
        /// <param name="response">The response from the server.</param>
        /// <returns>The parsed object.</returns>
        private static DownloadResult<T> GetResult<T>(Uri requestBaseUrl, Stream stream,
            ParseDataDelegate<T> parser, HttpResponseMessage response)
        {
            T result = default(T);
            HttpWebClientServiceException error = null;
            int responseCode = (int)response.StatusCode;
            DateTime serverTime = response.Headers.ServerTimeUTC();
            DateTime? expires = response.Content?.Headers?.ExpiresTimeUTC();


            if (stream == null)
                // No stream (can this happen)?
                error = HttpWebClientServiceException.Exception(requestBaseUrl,
                    new ArgumentNullException(nameof(stream)));
            else if (response.StatusCode != HttpStatusCode.NotModified)
                // If not modified, there is no data to parse
                // Otherwise, attempt to invoke parser
                result = parser.Invoke(Util.ZlibUncompress(stream), responseCode);

            return new DownloadResult<T>(result, error, responseCode, serverTime,
                expires, response.Headers.ETag?.Tag);
        }
    }
}
