using EVEMon.Common.Constants;
using EVEMon.Common.Extensions;
using EVEMon.Common.Serialization;
using EVEMon.Common.Serialization.Esi;
using EVEMon.Common.Threading;
using System;
using System.Net;
using System.Runtime.Serialization;
using System.Text;

namespace EVEMon.Common.Service
{
    /// <summary>
    /// Uses the built-in TcpListener class to create a server to receive responses for SSO
    /// authentication.
    /// </summary>
    public sealed class SSOAuthenticationService
    {
        /// <summary>
        /// Creates an instance of SSOAuthenticationService with the current settings.
        /// </summary>
        /// <returns>an instance of SSOAuthenticationService, or null if the settings are blank</returns>
        public static SSOAuthenticationService GetInstance()
        {
            string id = Settings.SSOClientID, secret = Settings.SSOClientSecret, scopes = Settings.SSOScopes;
            SSOAuthenticationService authService;

            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(scopes))
                authService = null;
            else
                authService = new SSOAuthenticationService(id, secret, scopes);
            return authService;
        }

        /// <summary>
        /// Starts obtaining information about the character used to authenticate the specified
        /// token.
        /// </summary>
        /// <param name="token">The auth token used.</param>
        /// <param name="callback">A callback to receive the token info.</param>
        public static void GetTokenInfo(string token, Action<JsonResult<EsiAPITokenInfo>> callback)
        {
            Util.DownloadJsonAsync<EsiAPITokenInfo>(new Uri(NetworkConstants.SSOBase +
                NetworkConstants.SSOCharID), token).ContinueWith((result) =>
                Dispatcher.Invoke(() => callback?.Invoke(result.Result)));
        }
        
        /// <summary>
        /// The SSO client ID.
        /// </summary>
        private readonly string m_clientID;

        /// <summary>
        /// List of scopes that are to be used.
        /// </summary>
        private readonly string m_scopes;

        /// <summary>
        /// The SSO client secret.
        /// </summary>
        private readonly string m_secret;

        public SSOAuthenticationService(string clientID, string secret, string scopes)
        {
            if (string.IsNullOrEmpty(clientID))
                throw new ArgumentNullException("clientID");
            if (string.IsNullOrEmpty(scopes))
                throw new ArgumentNullException("scopes");
            if (string.IsNullOrEmpty(secret))
                throw new ArgumentNullException("secret");
            m_clientID = clientID;
            m_scopes = scopes;
            m_secret = secret;
        }

        /// <summary>
        /// Retrieves a token from the server with the specified authentication data.
        /// </summary>
        /// <param name="data">The POST data, either an auth code or a refresh token.</param>
        /// <param name="callback">A callback to receive the new token.</param>
        private void FetchToken(string data, Action<JsonResult<AccessResponse>> callback)
        {
            var obtained = DateTime.UtcNow;

            // URL is the same for both
            var url = new Uri(NetworkConstants.SSOBase + NetworkConstants.SSOToken);
            Util.DownloadJsonAsync<AccessResponse>(url, GetBasicAuthHeader(), postData: data).
                ContinueWith((result) =>
                {
                    var taskResult = result.Result;
                    if (taskResult != null)
                        // Initialize time since the deserializer does not call the constructor
                        taskResult.Result.Obtained = obtained;
                    Dispatcher.Invoke(() => callback?.Invoke(taskResult));
                }
            );
        }

        /// <summary>
        /// Starts obtaining a new access token from the refresh token.
        /// </summary>
        /// <param name="refreshToken">The refresh token.</param>
        /// <param name="callback">A callback to receive the new token.</param>
        public void GetNewToken(string refreshToken, Action<JsonResult<AccessResponse>> callback)
        {
            refreshToken.ThrowIfNull(nameof(refreshToken));
            FetchToken(string.Format(NetworkConstants.PostDataWithRefreshToken, WebUtility.
                UrlEncode(refreshToken)), callback);
        }

        /// <summary>
        /// Starts verifying the authentication code.
        /// </summary>
        /// <param name="authCode">The code to verify.</param>
        /// <param name="callback">A callback to receive the tokens.</param>
        public void VerifyAuthCode(string authCode, Action<JsonResult<AccessResponse>> callback)
        {
            authCode.ThrowIfNull(nameof(authCode));
            FetchToken(string.Format(NetworkConstants.PostDataWithAuthToken, WebUtility.
                UrlEncode(authCode)), callback);
        }

        /// <summary>
        /// Retrieves the "basic" authentication header used when retrieving tokens.
        /// </summary>
        /// <returns>The correct Basic authentication header with client ID/secret.</returns>
        private string GetBasicAuthHeader()
        {
            return "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(m_clientID +
                ":" + m_secret));
        }
        
        /// <summary>
        /// Spawns a browser for the user to log in; the port is the location of the local
        /// web server which receives the response, the state is used to stop XSRF
        /// (make it random!)
        /// </summary>
        /// <param name="state">The random state parameter used to stop cross-site forgery.</param>
        /// <param name="port">The port used for the response.</param>
        public void SpawnBrowserForLogin(string state, int port)
        {
            string redirect = string.Format(NetworkConstants.SSORedirect, port);
            string url = string.Format(NetworkConstants.SSOBase + NetworkConstants.SSOLogin,
                WebUtility.UrlEncode(redirect), state, m_scopes, m_clientID);
            Util.OpenURL(new Uri(url));
        }
    }

    /// <summary>
    /// Template class for verification responses from SSO authentication.
    /// </summary>
    [DataContract]
    public sealed class AccessResponse
    {
        [DataMember(Name = "access_token")]
        public string AccessToken { get; set; }
        [DataMember(Name = "refresh_token")]
        public string RefreshToken { get; set; }
        [DataMember(Name = "expires_in")]
        private int ExpiresIn { get; set; }
        [IgnoreDataMember]
        public DateTime ExpiryUTC
        {
            get
            {
                return Obtained + TimeSpan.FromSeconds(ExpiresIn);
            }
        }
        // This is apparently not set when deserialized, added a method to initialize it
        [IgnoreDataMember]
        public DateTime Obtained { get; set; }

        public AccessResponse()
        {
            AccessToken = string.Empty;
            RefreshToken = string.Empty;
            Obtained = DateTime.UtcNow;
        }
    }
}
