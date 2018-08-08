using System;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace EVEMon.Common.Net
{
    public static partial class HttpWebClientService
    {
        /// <summary>
        /// Initializes the <see cref="HttpWebClientService"/> class.
        /// </summary>
        static HttpWebClientService()
        {
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.DefaultConnectionLimit = 10;
#if false
            // To debug trust failure issues
            if (EveMonClient.IsDebugBuild)
                ServicePointManager.ServerCertificateValidationCallback = DummyCertificateValidationCallback;
#endif
        }

        /// <summary>
        /// A dummy certificate validation callback.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="certificate">The certificate.</param>
        /// <param name="chain">The chain.</param>
        /// <param name="sslpolicyerrors">The sslpolicyerrors.</param>
        /// <returns></returns>
        internal static bool DummyCertificateValidationCallback(object sender, X509Certificate
            certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors) => true;

        /// <summary>
        /// Gets the web client.
        /// </summary>
        /// <returns></returns>
        public static WebClient GetWebClient() => new WebClient
        {
            Proxy = HttpClientServiceRequest.GetWebProxy()
        };

        /// <summary>
        /// Gets the HTTP client.
        /// </summary>
        /// <param name="httpClientHandler">The HTTP client handler.</param>
        /// <returns></returns>
        public static HttpClient GetHttpClient(HttpClientHandler httpClientHandler = null) =>
            httpClientHandler == null ? new HttpClient() : new HttpClient(httpClientHandler);

        /// <summary>
        /// Validates a Url as acceptable for an HttpWebServiceRequest.
        /// </summary>
        /// <param name="url">A url <see cref="string"/> for the request. The string must specify HTTP or HTTPS as its scheme.</param>
        /// <param name="errorMsg">Is url is invalid, contains a descriptive message of the reason</param>
        public static bool IsValidURL(Uri url, out string errorMsg)
        {
            if (String.IsNullOrWhiteSpace(url.AbsoluteUri))
            {
                errorMsg = "Url may not be null or an empty string.";
                return false;
            }

            if (!Uri.IsWellFormedUriString(url.AbsoluteUri, UriKind.Absolute))
            {
                errorMsg = $"\"{url}\" is not a well-formed URL.";

                return false;
            }

            try
            {
                if (url.Scheme != Uri.UriSchemeHttp && url.Scheme != Uri.UriSchemeHttps)
                {
                    errorMsg = $"The specified scheme ({url.Scheme}) is not supported.";

                    return false;
                }
            }
            catch (UriFormatException)
            {
                errorMsg = $"\"{url}\" is not a valid URL for an HTTP or HTTPS request.";

                return false;
            }

            errorMsg = String.Empty;
            return true;
        }
    }
}
