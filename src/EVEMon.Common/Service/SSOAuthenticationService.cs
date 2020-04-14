using EVEMon.Common.Constants;
using EVEMon.Common.Extensions;
using EVEMon.Common.Helpers;
using EVEMon.Common.Net;
using EVEMon.Common.Serialization;
using EVEMon.Common.Serialization.Esi;
using EVEMon.Common.Threading;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Security.Cryptography;
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
        /// <returns>an instance of SSOAuthenticationService, or null if the settings are
        /// blank</returns>
        public static SSOAuthenticationService GetInstance()
        {
            string id = Settings.SSOClientID, secret = Settings.SSOClientSecret;
            string scopes = Settings.APIMethodAccess == Enumerations.CCPAPI.CCPAPIMethodsEnum.BasicCharacterFeatures ? NetworkConstants.SSOScopesBasic : NetworkConstants.SSOScopes;
            SSOAuthenticationService authService;

            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(secret))
                authService = new SSOAuthenticationService(NetworkConstants.SSODefaultAppID, null, scopes);
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
        public static void GetTokenInfo(string token, Action<JsonResult<EsiAPITokenInfo>>
            callback)
        {
            var url = new Uri(NetworkConstants.SSOBase + NetworkConstants.SSOCharID);
            Util.DownloadJsonAsync<EsiAPITokenInfo>(url, new RequestParams()
            {
                Authentication = token
            }).ContinueWith((result) => Dispatcher.Invoke(() =>
            {
                // Run the callback on the dispatcher thread
                callback?.Invoke(result.Result);
            }));
        }

        /// <summary>
        /// The SSO client ID.
        /// </summary>
        private readonly string m_clientID;

        /// <summary>
        /// The Base-64 encoded challenge string for PKCE authentication.
        /// </summary>
        private readonly string m_codeChallenge;

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
            m_clientID = clientID;
            var rnd = new RNGCryptoServiceProvider();
            byte[] cc = new byte[32];
            rnd.GetBytes(cc);
            m_codeChallenge = Util.URLSafeBase64(cc);
            m_scopes = scopes;
            m_secret = secret;
        }

        /// <summary>
        /// Retrieves a token from the server with the specified authentication data.
        /// </summary>
        /// <param name="data">The POST data, either an auth code or a refresh token.</param>
        /// <param name="callback">A callback to receive the new token.</param>
        /// <param name="isJWT">true if a JWT response is expected, or false if a straight JSON response is expected.</param>
        private void FetchToken(string data, Action<AccessResponse> callback, bool isJWT)
        {
            var obtained = DateTime.UtcNow;
            var url = new Uri(NetworkConstants.SSOBaseV2 + NetworkConstants.SSOToken);
            var rp = new RequestParams()
            {
                Content = data,
                Method = HttpMethod.Post
            };
            if (!string.IsNullOrEmpty(m_secret))
                // Non-PKCE
                rp.Authentication = GetBasicAuthHeader();
            HttpWebClientService.DownloadStringAsync(url, rp).ContinueWith((result) =>
            {
                AccessResponse response = null;
                DownloadResult<string> taskResult;
                string encodedToken;
                // It must be completed or failed if ContinueWith is reached
                if (result.IsFaulted)
                    ExceptionHandler.LogException(result.Exception, true);
                else if ((taskResult = result.Result) != null)
                {
                    // Log HTTP error if it occurred
                    if (taskResult.Error != null)
                        ExceptionHandler.LogException(taskResult.Error, true);
                    else if (!string.IsNullOrEmpty(encodedToken = taskResult.Result))
                        // For some reason the JWT token is not returned according to the ESI
                        // spec
                        response = TokenFromString(encodedToken, false, obtained);
                }
                Dispatcher.Invoke(() => callback?.Invoke(response));
            });
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
        /// Starts obtaining a new access token from the refresh token.
        /// </summary>
        /// <param name="refreshToken">The refresh token.</param>
        /// <param name="callback">A callback to receive the new token.</param>
        public void GetNewToken(string refreshToken, Action<AccessResponse> callback)
        {
            refreshToken.ThrowIfNull(nameof(refreshToken));
            string data;
            if (string.IsNullOrEmpty(m_secret))
                // PKCE
                data = string.Format(NetworkConstants.PostDataRefreshPKCE, WebUtility.
                    UrlEncode(refreshToken), m_clientID);
            else
                data = string.Format(NetworkConstants.PostDataRefreshToken, WebUtility.
                    UrlEncode(refreshToken));
            FetchToken(data, callback, false);
        }

        /// <summary>
        /// Starts verifying the authentication code.
        /// </summary>
        /// <param name="authCode">The code to verify.</param>
        /// <param name="callback">A callback to receive the tokens.</param>
        public void VerifyAuthCode(string authCode, Action<AccessResponse> callback)
        {
            authCode.ThrowIfNull(nameof(authCode));
            bool isPKCE = string.IsNullOrEmpty(m_secret);
            string data;
            if (isPKCE)
                // PKCE
                data = string.Format(NetworkConstants.PostDataAuthPKCE, WebUtility.UrlEncode(
                    authCode), m_clientID, m_codeChallenge);
            else
                data = string.Format(NetworkConstants.PostDataAuthToken, WebUtility.
                    UrlEncode(authCode));
            FetchToken(data, callback, isPKCE);
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
            string url;
            if (string.IsNullOrEmpty(m_secret))
                // PKCE
                url = string.Format(NetworkConstants.SSOLoginPKCE, WebUtility.UrlEncode(
                    redirect), state, m_scopes, m_clientID, Util.SHA256Base64(Encoding.ASCII.
                    GetBytes(m_codeChallenge)));
            else
                url = string.Format(NetworkConstants.SSOLogin, WebUtility.UrlEncode(redirect),
                    state, m_scopes, m_clientID);
            Util.OpenURL(new Uri(NetworkConstants.SSOBaseV2 + url));
        }

        /// <summary>
        /// Creates a token from a JWT or regular access response.
        /// </summary>
        /// <param name="data">The token data from the server.</param>
        /// <param name="isJWT">true if a JWT response is expected, or false if a straight JSON response is expected.</param>
        /// <param name="obtained">The time when this token was first obtained</param>
        /// <returns>The token, or null if none could be parsed.</returns>
        private AccessResponse TokenFromString(string data, bool isJWT, DateTime obtained)
        {
            AccessResponse response = null;
            if (isJWT)
            {
                JwtSecurityToken token;
                try
                {
                    token = new JwtSecurityToken(data);
                }
                catch (ArgumentException e)
                {
                    // JwtSecurityToken constructor throws this exception if the token is
                    // invalid
                    ExceptionHandler.LogException(e, true);
                    token = null;
                }
                if (token != null)
                {
                    var intendedURI = new Uri(NetworkConstants.SSOBaseV2);
                    string issuer = token.Issuer;
                    // Validate ISSuer
                    if (issuer == intendedURI.Host || issuer == intendedURI.GetLeftPart(
                            UriPartial.Authority))
                        response = Util.DeserializeJson<AccessResponse>(token.RawPayload);
                    else
                        EveMonClient.Trace("Rejecting invalid SSO token issuer: " + issuer);
                }
            }
            else
                response = Util.DeserializeJson<AccessResponse>(data);
            if (response != null)
                // Initialize time since deserializer does not call the constructor
                response.Obtained = obtained;
            return response;
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
