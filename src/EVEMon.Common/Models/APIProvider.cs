using EVEMon.Common.Attributes;
using EVEMon.Common.Constants;
using EVEMon.Common.Extensions;
using EVEMon.Common.Serialization.Eve;
using EVEMon.Common.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Xsl;

namespace EVEMon.Common.Models
{
    /// <summary>
    /// Serializable class abstracting an API queries provider and its configuration.
    /// </summary>
    [EnforceUIThreadAffinity]
    public sealed class APIProvider
    {
        // Called when an ESI request completes
        public delegate void ESIRequestCallback<T>(EsiResult<T> result, object state);

        private static APIProvider s_ccpProvider;
        private static APIProvider s_ccpTestProvider;
        private static XslCompiledTransform s_rowsetsTransform;

        private readonly List<ESIMethod> m_methods;
        private bool m_supportsCompressedResponse;

        /// <summary>
        /// Default constructor.
        /// </summary>
        internal APIProvider()
        {
            m_methods = new List<ESIMethod>(ESIMethod.CreateDefaultSet());
            Url = new Uri("http://your-custom-API-provider.com");
            Name = "your provider's name";
        }


        #region Properties

        /// <summary>
        /// Returns the name of this APIConfiguration.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Returns the server host for this APIConfiguration.
        /// </summary>
        public Uri Url { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether the provider supports compressed responses.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the provider supports compressed responses; otherwise, <c>false</c>.
        /// </value>
        public bool SupportsCompressedResponse
        {
            get { return IsDefault || this == TestProvider || m_supportsCompressedResponse; }
            set { m_supportsCompressedResponse = value; }
        }

        /// <summary>
        /// Returns true if this APIConfiguration represents the default API service.
        /// </summary>
        public bool IsDefault => this == DefaultProvider;

        /// <summary>
        /// Gets the default API provider
        /// </summary>
        public static APIProvider DefaultProvider
            => s_ccpProvider ??
               (s_ccpProvider = new APIProvider
               {
                   Url = new Uri(NetworkConstants.ESIBase),
                   Name = "CCP"
               });

        /// <summary>
        /// Gets the test API provider
        /// </summary>
        public static APIProvider TestProvider
            => s_ccpTestProvider ??
               (s_ccpTestProvider = new APIProvider
               {
                   Url = new Uri(NetworkConstants.ESITestBase),
                   Name = "CCP Test API"
               });

        #endregion


        #region Helpers
        
        /// <summary>
        /// Returns the request method.
        /// </summary>
        /// <param name="requestMethod">An ESIMethodsEnum enumeration member specifying the method for which the URL is required.</param>
        private ESIMethod GetESIMethod(Enum requestMethod)
        {
            var esiMethod = m_methods.FirstOrDefault(method => method.Method.Equals(requestMethod));
            if (esiMethod == null)
                throw new InvalidOperationException("No ESI method found for " + requestMethod);
            return esiMethod;
        }

        /// <summary>
        /// Returns the full canonical ESI URL for the specified APIMethod as constructed from the Server and APIMethod properties.
        /// </summary>
        /// <param name="requestMethod">An APIMethodsEnum enumeration member specifying the method for which the URL is required.</param>
        /// <param name="getID1">The first numeric parameter for this URL, if needed.</param>
        /// <param name="getID2">The second numeric parameter for this URL, if needed.</param>
        /// <param name="getStr">The second string parameter for this URL, if needed. Overrides
        /// the numeric parameter if not null or empty.</param>
        /// <returns>A String representing the full URL path of the specified method.</returns>
        private Uri GetESIUrl(Enum requestMethod, long getID1, long getID2, string getStr)
        {
            string path;
            if (string.IsNullOrEmpty(getStr))
                path = string.Format(GetESIMethod(requestMethod).Path, getID1, getID2);
            else
                path = string.Format(GetESIMethod(requestMethod).Path, getID1, getStr);
            // Build the URI
            UriBuilder uriBuilder = new UriBuilder(NetworkConstants.ESIBase);
            uriBuilder.Path = Path.Combine(uriBuilder.Path, path);
            return uriBuilder.Uri;
        }
        
        #endregion


        #region Queries
        
        /// <summary>
        /// Query a public ESI method without arguments.
        /// </summary>
        /// <typeparam name="T">The type of the deserialization object.</typeparam>
        /// <param name="method"></param>
        /// <param name="callback">The callback to invoke once the query has been completed.</param>
        /// <param name="state">State to be passed to the callback when it is used.</param>
        /// <param name="eTag">The previous result's entity tag, if applicable.</param>
        public void QueryEsiAsync<T>(Enum method, ESIRequestCallback<T> callback,
            object state = null, string eTag = null) where T : class
        {
            QueryEsiAsync(method, callback, state, new EsiParams() { ETag = eTag });
        }

        /// <summary>
        /// Query a public ESI method with an ID argument.
        /// </summary>
        /// <typeparam name="T">The type of the deserialization object.</typeparam>
        /// <param name="method"></param>
        /// <param name="id">The ID to query.</param>
        /// <param name="callback">The callback to invoke once the query has been completed.</param>
        /// <param name="state">State to be passed to the callback when it is used.</param>
        /// <param name="eTag">The previous result's entity tag, if applicable.</param>
        public void QueryEsiAsync<T>(Enum method, long id, ESIRequestCallback<T> callback,
            object state = null, string eTag = null) where T : class
        {
            QueryEsiAsync(method, callback, state, new EsiParams() { ParamOne = id, ETag = eTag });
        }

        /// <summary>
        /// Query a public ESI method with an ID and string argument. Still uses GET.
        /// </summary>
        /// <typeparam name="T">The type of the deserialization object.</typeparam>
        /// <param name="method"></param>
        /// <param name="id">The ID to query.</param>
        /// <param name="data">The string query data for the second GET parameter.</param>
        /// <param name="callback">The callback to invoke once the query has been completed.</param>
        /// <param name="state">State to be passed to the callback when it is used.</param>
        /// <param name="eTag">The previous result's entity tag, if applicable.</param>
        public void QueryEsiAsync<T>(Enum method, long id, string data,
            ESIRequestCallback<T> callback, object state = null, string eTag = null) where T : class
        {
            QueryEsiAsync(method, callback, state, new EsiParams()
            {
                ParamOne = id, GetData = data, ETag = eTag
            });
        }

        /// <summary>
        /// Query a public ESI method with POST arguments.
        /// </summary>
        /// <typeparam name="T">The type of the deserialization object.</typeparam>
        /// <param name="method"></param>
        /// <param name="postData">The data to submit in the POST body.</param>
        /// <param name="callback">The callback to invoke once the query has been completed.</param>
        /// <param name="state">State to be passed to the callback when it is used.</param>
        /// <param name="eTag">The previous result's entity tag, if applicable.</param>
        public void QueryEsiAsync<T>(Enum method, string postData, ESIRequestCallback<T> callback,
            object state = null, string eTag = null) where T : class
        {
            QueryEsiAsync(method, callback, state, new EsiParams() { PostData = postData, ETag = eTag });
        }

        /// <summary>
        /// Query a public ESI method with an ID argument.
        /// </summary>
        /// <typeparam name="T">The type of the deserialization object.</typeparam>
        /// <param name="method"></param>
        /// <param name="id">The ID to query.</param>
        /// <param name="callback">The callback to invoke once the query has been completed.</param>
        /// <param name="state">State to be passed to the callback when it is used.</param>
        /// <param name="eTag">The previous result's entity tag, if applicable.</param>
        public void QueryEsiAsync<T>(Enum method, string token, long id, ESIRequestCallback<T> callback,
            object state = null, string eTag = null) where T : class
        {
            QueryEsiAsync(method, callback, state, new EsiParams()
            {
                Token = token, ParamOne = id, ETag = eTag
            });
        }

        /// <summary>
        /// Query a public ESI method with an ID and character argument.
        /// </summary>
        /// <typeparam name="T">The type of the deserialization object.</typeparam>
        /// <param name="method"></param>
        /// <param name="character">The character ID to query.</param>
        /// <param name="id">The ID to query.</param>
        /// <param name="callback">The callback to invoke once the query has been completed.</param>
        /// <param name="state">State to be passed to the callback when it is used.</param>
        /// <param name="eTag">The previous result's entity tag, if applicable.</param>
        public void QueryEsiAsync<T>(Enum method, string token, long character, long id,
            ESIRequestCallback<T> callback, object state = null, string eTag = null) where T : class
        {
            QueryEsiAsync(method, callback, state, new EsiParams()
            {
                Token = token, ParamOne = character, ParamTwo = id, ETag = eTag
            });
        }

        #endregion


        #region Querying helpers
        
        /// <summary>
        /// Asynchronously queries this method with the provided ID and HTTP POST data.
        /// </summary>
        /// <typeparam name="T">The subtype to deserialize (the deserialized type being <see cref="CCPAPIResult{T}" />).</typeparam>
        /// <param name="method">The method to query</param>
        /// <param name="callback">The callback to invoke once the query has been completed.</param>
        /// <param name="state">State to be passed to the callback when it is used.</param>
        /// <param name="data">The parameters to use for the request, including the token, arguments, and POST data.</param>
        /// <exception cref="System.ArgumentNullException">callback; The callback cannot be null.</exception>
        private void QueryEsiAsync<T>(Enum method, ESIRequestCallback<T> callback, object state,
            EsiParams data) where T : class
        {
            // Check callback not null
            callback.ThrowIfNull(nameof(callback), "The callback cannot be null.");

            // Lazy download
            Uri url = GetESIUrl(method, data.ParamOne, data.ParamTwo, data.GetData);

            Util.DownloadJsonAsync<T>(url, data.Token, SupportsCompressedResponse, data.PostData, eTag: data.ETag)
                .ContinueWith(task =>
                {
                    var esiResult = new EsiResult<T>(task.Result);

                    // Sync clock on the answer if necessary
                    var sync = esiResult.Result as ISynchronizableWithLocalClock;
                    if (sync != null)
                        sync.SynchronizeWithLocalClock(DateTime.UtcNow - esiResult.CurrentTime);

                    // Invokes the callback
                    Dispatcher.Invoke(() => callback.Invoke(esiResult, state));
                });
        }
        
        /// <summary>
        /// Gets the XSLT used for transforming rowsets into something deserializable by <see cref="System.Xml.Serialization.XmlSerializer"/>
        /// </summary>
        internal static XslCompiledTransform RowsetsTransform => s_rowsetsTransform ??
            (s_rowsetsTransform = Util.LoadXslt(Properties.Resources.RowsetsXSLT));

        #endregion


        /// <summary>
        /// Returns the configuration name as a String.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Name;


        #region Helper classes

        /// <summary>
        /// Simplifies
        /// </summary>
        private struct EsiParams
        {
            public long ParamOne;
            public long ParamTwo;
            public string GetData;
            public string PostData;
            public string Token;
            public string ETag;
        }

        #endregion

    }
}
