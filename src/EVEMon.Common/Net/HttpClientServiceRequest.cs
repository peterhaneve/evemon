using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using EVEMon.Common.Constants;
using EVEMon.Common.Enumerations;
using EVEMon.Common.Models;
using EVEMon.Common.SettingsObjects;

namespace EVEMon.Common.Net
{
    internal class HttpClientServiceRequest
    {
        internal string AuthToken { get; set; }

        internal bool AcceptEncoded
        {
            get
            {
                return m_acceptEncoded;
            }
            set
            {
                m_acceptEncoded = value;
            }
        }

        internal DataCompression DataCompression { get; set; }

        private static TimeSpan s_timeout;

        private HttpPostData m_postData;
        private DataCompression m_dataCompression;
        private HttpMethod m_method;
        private Uri m_url;
        private Uri m_referrer;

        private string m_accept;

        private int m_redirectsRemaining;
        private bool m_acceptEncoded;


        /// <summary>
        /// Initializes a new instance of the <see cref="HttpClientServiceRequest"/> class.
        /// </summary>
        public HttpClientServiceRequest()
        {
            m_redirectsRemaining = HttpWebClientServiceState.MaxRedirects;
            DataCompression = DataCompression.None;
            m_acceptEncoded = false;
            AuthToken = null;

            // Pull the timeout from the settings
            TimeSpan timeoutSetting = TimeSpan.FromSeconds(Settings.Updates.HttpTimeout);

            s_timeout = timeoutSetting < TimeSpan.FromSeconds(1) || timeoutSetting > TimeSpan.FromMinutes(5)
                ? TimeSpan.FromSeconds(20)
                : timeoutSetting;
        }

        /// <summary>
        /// Gets the base URL.
        /// </summary>
        /// <value>
        /// The base URL.
        /// </value>
        public Uri BaseUrl => m_url;

        /// <summary>
        /// Asynchronously sends a request to the specified url.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="method">The method.</param>
        /// <param name="postData">The post data.</param>
        /// <param name="accept">The accept.</param>
        /// <param name="eTag">The previous result's entity tag, if applicable.</param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> SendAsync(Uri url, HttpMethod method,
            HttpPostData postData, string accept, string eTag = null)
        {
            while (true)
            {
                // Store params
                m_url = url;
                m_accept = accept;
                m_postData = postData;
                m_method = postData == null || method == null ? HttpMethod.Get : method;
                m_dataCompression = postData == null ? DataCompression.None : DataCompression;

                HttpResponseMessage response = null;
                try
                {
                    HttpClientHandler httpClientHandler = GetHttpClientHandler();
                    HttpRequestMessage request = GetHttpRequest(AuthToken, postData?.ContentType, eTag);
                    response = await GetHttpResponseAsync(httpClientHandler, request).ConfigureAwait(false);

                    EnsureSuccessStatusCode(response);
                }
                catch (HttpWebClientServiceException)
                {
                    throw;
                }
                catch (HttpRequestException ex)
                {
                    if (ex.InnerException is WebException)
                        throw HttpWebClientServiceException.HttpWebClientException(url, ex.InnerException);

                    if (response == null)
                        throw HttpWebClientServiceException.Exception(url, ex);

                    if (response.StatusCode != HttpStatusCode.Redirect && response.StatusCode != HttpStatusCode.MovedPermanently)
                    {
                        throw HttpWebClientServiceException.HttpWebClientException(url, ex, response.StatusCode);
                    }
                }
                catch (WebException ex)
                {
                    // We should not get a WebException here but keep this as extra precaution
                    throw HttpWebClientServiceException.HttpWebClientException(url, ex);
                }
                catch (TaskCanceledException ex)
                {
                    // We throw a request timeout if the task gets cancelled due to the timeout setting
                    throw HttpWebClientServiceException.HttpWebClientException(url, new HttpRequestException(ex.Message),
                        HttpStatusCode.RequestTimeout);
                }
                catch (Exception ex)
                {
                    throw HttpWebClientServiceException.Exception(url, ex);
                }

                if (response.StatusCode != HttpStatusCode.Redirect && response.StatusCode != HttpStatusCode.MovedPermanently)
                {
                    return response;
                }

                // When the address has been redirected, connects to the redirection
                Uri target = response.Headers.Location;
                response.Dispose();

                if (m_redirectsRemaining-- <= 0)
                    throw HttpWebClientServiceException.RedirectsExceededException(m_url);

                m_referrer = m_url;
                m_url = new Uri(m_url, target);
                url = m_url;
            }
        }

        private static void EnsureSuccessStatusCode(HttpResponseMessage response)
        {
            if ((int)response.StatusCode < 100)
            {
                response.StatusCode = HttpStatusCode.OK;
                response.ReasonPhrase = "OK";
            }

            string contentTypeMediaType = response.Content?.Headers?.ContentType?.MediaType;
            bool isNotCCPWithXmlContent = response.RequestMessage.RequestUri.Host != APIProvider.DefaultProvider.Url.Host &&
                                       response.RequestMessage.RequestUri.Host != APIProvider.TestProvider.Url.Host &&
                                       contentTypeMediaType != null && !contentTypeMediaType.Contains("xml");

            if (isNotCCPWithXmlContent ||
                (response.Content?.Headers?.ContentLength == 0 && response.StatusCode != HttpStatusCode.NotModified))
                response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Gets the HTTP client handler.
        /// </summary>
        /// <returns></returns>
        private static HttpClientHandler GetHttpClientHandler()
            => new HttpClientHandler
            {
                AllowAutoRedirect = false,
                MaxAutomaticRedirections = HttpWebClientServiceState.MaxRedirects,
                Proxy = GetWebProxy()
            };

        /// <summary>
        /// Gets the web proxy.
        /// </summary>
        /// <returns></returns>
        internal static IWebProxy GetWebProxy()
        {
            if (!HttpWebClientServiceState.Proxy.Enabled)
                return WebRequest.DefaultWebProxy;

            WebProxy proxy = new WebProxy(HttpWebClientServiceState.Proxy.Host, HttpWebClientServiceState.Proxy.Port);
            switch (HttpWebClientServiceState.Proxy.Authentication)
            {
                case ProxyAuthentication.None:
                    proxy.UseDefaultCredentials = false;
                    proxy.Credentials = null;
                    break;
                case ProxyAuthentication.SystemDefault:
                    proxy.UseDefaultCredentials = true;
                    break;
                case ProxyAuthentication.Specified:
                    proxy.UseDefaultCredentials = false;
                    proxy.Credentials = new NetworkCredential(HttpWebClientServiceState.Proxy.Username,
                        Util.Decrypt(HttpWebClientServiceState.Proxy.Password,
                            HttpWebClientServiceState.Proxy.Username));
                    break;
            }
            return proxy;
        }

        /// <summary>
        /// Gets the HTTP response.
        /// </summary>
        /// <param name="httpClientHandler">The HTTP client handler.</param>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        private static async Task<HttpResponseMessage> GetHttpResponseAsync(HttpClientHandler httpClientHandler,
            HttpRequestMessage request)
        {
            using (HttpClient client = HttpWebClientService.GetHttpClient(httpClientHandler))
            {
                client.Timeout = s_timeout;
                return await client.SendAsync(request).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Gets the HTTP request.
        /// </summary>
        /// <param name="token">If not null, adds the specified ESI token to the headers.</param>
        /// <param name="dataContentType">The content type of the input data.</param>
        /// <param name="eTag">The previous result's entity tag, if applicable.</param>
        /// <returns>The HTTP request.</returns>
        private HttpRequestMessage GetHttpRequest(string token, string dataContentType, string eTag = null)
        {
            if (m_method == HttpMethod.Get && m_postData != null)
                m_url = new Uri($"{m_url.AbsoluteUri}?{m_postData}");

            var request = new HttpRequestMessage
            {
                RequestUri = m_url,
                Method = m_method,
            };
            if (token != null)
            {
                // If the token has a space, use that type of auth header
                string type = "Bearer";
                int index = token.IndexOf(' ');
                if (index > 0)
                {
                    type = token.Substring(0, index);
                    token = token.Substring(index + 1).TrimStart();
                }
                request.Headers.Authorization = new AuthenticationHeaderValue(type, token);
            }
            request.Headers.AcceptCharset.TryParseAdd("ISO-8859-1,utf-8;q=0.8,*;q=0.7");
            request.Headers.AcceptLanguage.TryParseAdd("en-us,en;q=0.5");
            request.Headers.Pragma.TryParseAdd("no-cache");
            request.Headers.UserAgent.TryParseAdd(HttpWebClientServiceState.UserAgent);
            request.Headers.Accept.ParseAdd(m_accept);

            if (!string.IsNullOrEmpty(eTag))
                request.Headers.IfNoneMatch.Add(new EntityTagHeaderValue(eTag));

            if (m_acceptEncoded)
                request.Headers.AcceptEncoding.ParseAdd("gzip,deflate;q=0.8");

            if (m_referrer != null)
                request.Headers.Referrer = m_referrer;

            if (m_postData == null || m_method == HttpMethod.Get)
                return request;

            request.Content = new ByteArrayContent(m_postData.Content.ToArray());
            if (!string.IsNullOrEmpty(dataContentType))
                request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(dataContentType);

            // If we are going to send a compressed request set the appropriate header
            if (Enum.IsDefined(typeof(DataCompression), m_dataCompression) && m_dataCompression != DataCompression.None)
            {
                request.Content.Headers.ContentEncoding.Add(m_dataCompression.ToString().ToLowerInvariant());
            }

            return request;
        }
    }
}
