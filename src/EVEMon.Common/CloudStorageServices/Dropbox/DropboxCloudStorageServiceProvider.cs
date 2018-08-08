using System;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Dropbox.Api;
using Dropbox.Api.Files;
using Dropbox.Api.Users;
using EVEMon.Common.Constants;
using EVEMon.Common.Net;
using EVEMon.Common.Serialization;

namespace EVEMon.Common.CloudStorageServices.Dropbox
{
    public sealed class DropboxCloudStorageServiceProvider : CloudStorageServiceProvider
    {
        private SerializableAPIResult<SerializableAPICredentials> m_result =
                new SerializableAPIResult<SerializableAPICredentials>();

        #region Properties

        /// <summary>
        /// Gets the name of the provider.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public override string Name => "Dropbox";

        /// <summary>
        /// Gets the authentication steps.
        /// </summary>
        /// <value>
        /// The authentication steps.
        /// </value>
        public override AuthenticationSteps AuthSteps => AuthenticationSteps.Two;

        /// <summary>
        /// Gets a value indicating whether this <see cref="CloudStorageServiceProvider" /> is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if enabled; otherwise, <c>false</c>.
        /// </value>
        protected override bool Enabled => true;

        /// <summary>
        /// Gets a value indicating whether the provider API credentials are stored.
        /// </summary>
        /// <value>
        /// <c>true</c> if the provider API credentials are stored; otherwise, <c>false</c>.
        /// </value>
        public override bool HasCredentialsStored
            => !String.IsNullOrEmpty(DropboxCloudStorageServiceSettings.Default.AccessToken);

        /// <summary>
        /// Gets the settings.
        /// </summary>
        /// <value>
        /// The settings.
        /// </value>
        protected override ApplicationSettingsBase Settings => DropboxCloudStorageServiceSettings.Default;

        /// <summary>
        /// Gets the refferal link.
        /// </summary>
        /// <value>
        /// The refferal link.
        /// </value>
        public override Uri RefferalLink => new Uri("https://www.dropbox.com/referrals/NTEyOTk1Njg4MDk?src=app9-891961");

        /// <summary>
        /// Gets the logo.
        /// </summary>
        /// <value>
        /// The logo.
        /// </value>
        public override Image Logo => CloudStorageServiceResources.DropboxLogo;

        #endregion


        #region Implementation Methods

        /// <summary>
        /// Asynchronously checks that the provider authentication with credentials is valid.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="apiKey">The API key.</param>
        protected override Task<SerializableAPIResult<SerializableAPICredentials>>
            CheckProviderAuthWithCredentialsIsValidAsync(uint userId, string apiKey)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Asynchronously requests the provider an authentication code.
        /// </summary>
        protected override Task<SerializableAPIResult<SerializableAPICredentials>> RequestProviderAuthCodeAsync()
        {
            m_result = new SerializableAPIResult<SerializableAPICredentials>();

            try
            {
                Uri authorizeUri = DropboxOAuth2Helper.GetAuthorizeUri(
                    Util.Decrypt(DropboxCloudStorageServiceSettings.Default.AppKey,
                        CultureConstants.InvariantCulture.NativeName));

                Util.OpenURL(authorizeUri);
            }
            catch (Exception exc)
            {
                m_result.Error = new SerializableAPIError { ErrorMessage = exc.Message };
            }

            return Task.FromResult(m_result);
        }

        /// <summary>
        /// Asynchronously checks the provider authentication code.
        /// </summary>
        /// <param name="code">The code.</param>
        protected override async Task<SerializableAPIResult<SerializableAPICredentials>> CheckProviderAuthCodeAsync(
            string code)
        {
            if (m_result == null)
                m_result = new SerializableAPIResult<SerializableAPICredentials>();

            try
            {
                OAuth2Response response = await DropboxOAuth2Helper.ProcessCodeFlowAsync(code,
                    Util.Decrypt(DropboxCloudStorageServiceSettings.Default.AppKey,
                        CultureConstants.InvariantCulture.NativeName),
                    Util.Decrypt(DropboxCloudStorageServiceSettings.Default.AppSecret,
                        CultureConstants.InvariantCulture.NativeName)).ConfigureAwait(false);

                await CheckAuthenticationAsync().ConfigureAwait(false);

                if (!m_result.HasError)
                    DropboxCloudStorageServiceSettings.Default.AccessToken = response.AccessToken;
            }
            catch (OAuth2Exception exc)
            {
                m_result.Error = new SerializableAPIError { ErrorMessage = exc.Message };
            }
            catch (Exception exc)
            {
                m_result.Error = new SerializableAPIError { ErrorMessage = exc.Message };
            }

            return m_result;
        }

        /// <summary>
        /// Asynchronously checks the authentication.
        /// </summary>
        protected override async Task<SerializableAPIResult<SerializableAPICredentials>> CheckAuthenticationAsync()
        {
            if (m_result == null)
                m_result = new SerializableAPIResult<SerializableAPICredentials>();

            if (!HasCredentialsStored)
                return m_result;
            try
            {
                InitializeCertPinning();
                using (DropboxClient client = GetClient())
                {
                    await client.Users.GetCurrentAccountAsync().ConfigureAwait(false);
                }
            }
            catch (ApiException<GetAccountError> exc)
            {
                m_result.Error = new SerializableAPIError { ErrorMessage = exc.Message };
            }
            catch (AuthException exc)
            {
                m_result.Error = new SerializableAPIError { ErrorMessage = exc.Message };

                if (exc.ErrorResponse.IsInvalidAccessToken && HasCredentialsStored)
                    await ResetSettingsAsync().ConfigureAwait(false);
            }
            catch (BadInputException exc)
            {
                m_result.Error = new SerializableAPIError { ErrorMessage = exc.Message };
            }
            catch (Exception exc)
            {
                m_result.Error = new SerializableAPIError { ErrorMessage = exc.Message };
            }
            finally
            {
                ClearCertPinning();
            }

            return m_result;
        }

        /// <summary>
        /// Asynchronously revokes the authorization.
        /// </summary>
        /// <returns></returns>
        protected override Task<SerializableAPIResult<SerializableAPICredentials>> RevokeAuthorizationAsync()
            => Task.FromResult(new SerializableAPIResult<SerializableAPICredentials>());

        /// <summary>
        /// Asynchronously uploads the file.
        /// </summary>
        /// <returns></returns>
        protected override async Task<SerializableAPIResult<CloudStorageServiceAPIFile>> UploadFileAsync()
        {
            SerializableAPIResult<CloudStorageServiceAPIFile> result = new SerializableAPIResult<CloudStorageServiceAPIFile>();

            try
            {
                byte[] content = Util.GZipCompress(SettingsFileContentByteArray).ToArray();
                CommitInfo commitInfo = new CommitInfo($"/{SettingsFileNameWithoutExtension}",
                    WriteMode.Overwrite.Instance);

                InitializeCertPinning();
                using (DropboxClient client = GetClient())
                using (Stream stream = Util.GetMemoryStream(content))
                {
                    await client.Files.UploadAsync(commitInfo, stream).ConfigureAwait(false);
                    return result;
                }
            }
            catch (ApiException<UploadError> ex)
            {
                result.Error = new SerializableAPIError { ErrorMessage = ex.Message };
            }
            catch (AuthException exc)
            {
                IsAuthenticated = false;
                result.Error = new SerializableAPIError { ErrorMessage = exc.Message };
            }
            catch (BadInputException exc)
            {
                IsAuthenticated = false;
                result.Error = new SerializableAPIError { ErrorMessage = exc.Message };
            }
            catch (Exception exc)
            {
                result.Error = new SerializableAPIError { ErrorMessage = exc.Message };
            }
            finally
            {
                ClearCertPinning();
            }

            return result;
        }

        /// <summary>
        /// Asynchronously downloads the file.
        /// </summary>
        /// <returns></returns>
        protected override async Task<SerializableAPIResult<CloudStorageServiceAPIFile>> DownloadFileAsync()
        {
            SerializableAPIResult<CloudStorageServiceAPIFile> result = new SerializableAPIResult<CloudStorageServiceAPIFile>();

            try
            {
                InitializeCertPinning();
                DownloadArg arg = new DownloadArg($"/{SettingsFileNameWithoutExtension}");
                using (DropboxClient client = GetClient())
                {
                    Task<Stream> response = await client.Files.DownloadAsync(arg)
                        .ContinueWith(async task => await task.Result.GetContentAsStreamAsync());
                    return await GetMappedAPIFileAsync(result, response.Result);
                }
            }
            catch (ApiException<DownloadError> ex)
            {
                result.Error = new SerializableAPIError { ErrorMessage = ex.Message };
            }
            catch (AuthException exc)
            {
                IsAuthenticated = false;
                result.Error = new SerializableAPIError { ErrorMessage = exc.Message };
            }
            catch (BadInputException exc)
            {
                IsAuthenticated = false;
                result.Error = new SerializableAPIError { ErrorMessage = exc.Message };
            }
            catch (Exception exc)
            {
                result.Error = new SerializableAPIError { ErrorMessage = exc.Message };
            }
            finally
            {
                ClearCertPinning();
            }

            return result;
        }

        #endregion


        #region Helper Methods

        /// <summary>
        /// Initializes the cert pinning.
        /// </summary>
        private static void InitializeCertPinning()
        {
            ServicePointManager.ServerCertificateValidationCallback = EveMonClient.IsDebugBuild
                ? HttpWebClientService.DummyCertificateValidationCallback
                : (RemoteCertificateValidationCallback)DropboxCertificateValidationCallback;
        }

        /// <summary>
        /// De-initializes the cert pinning.
        /// </summary>
        private static void ClearCertPinning()
        {
            // Default value is null
            ServicePointManager.ServerCertificateValidationCallback = null;
        }

        /// <summary>
        /// The Dropbox certificate validation callback.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="certificate">The certificate.</param>
        /// <param name="chain">The chain.</param>
        /// <param name="sslpolicyerrors">The sslpolicyerrors.</param>
        /// <returns></returns>
        private static bool DropboxCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain,
            SslPolicyErrors sslpolicyerrors)
        {
            // http://babbacom.com/?p=300 - fix security hole
            if (sslpolicyerrors != SslPolicyErrors.None || chain == null)
                return false;
            int n = chain.ChainElements.Count;
            if (n == 0)
                return false;
            X509ChainElement root = chain.ChainElements[n - 1];
            string publicKey = root.Certificate.GetPublicKeyString();

            return DropboxCertHelper.IsKnownRootCertPublicKey(publicKey);
        }

        /// <summary>
        /// Gets the client.
        /// </summary>
        /// <returns></returns>
        private static DropboxClient GetClient()
        {
            return new DropboxClient(DropboxCloudStorageServiceSettings.Default.AccessToken,
                userAgent: HttpWebClientServiceState.UserAgent);
        }

        #endregion
    }
}
