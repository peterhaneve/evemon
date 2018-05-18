using System;
using System.Linq;
using EVEMon.Common.Enumerations.CCPAPI;
using EVEMon.Common.Extensions;
using EVEMon.Common.Models;
using EVEMon.Common.Serialization.Eve;
using EVEMon.Common.Serialization;

namespace EVEMon.Common.QueryMonitor
{
    /// <summary>
    /// Represents a monitor for all queries related to characters and their corporations.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class CharacterQueryMonitor<T> : QueryMonitor<T> where T : class
    {
        private readonly CCPCharacter m_character;
        private ESIKey m_apiKey;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="character">The character to monitor.</param>
        /// <param name="method">The method to use.</param>
        /// <param name="onSuccess">An action to call on success.</param>
        /// <param name="onFailure">The callback to use upon failure.</param>
        internal CharacterQueryMonitor(CCPCharacter character, Enum method, Action<T>
            onSuccess, NotifyErrorCallback onFailure) : base(method, (result) =>
            {
                // Character may have been set to not be monitored
                if (character.Monitored)
                {
                    if (character.ShouldNotifyError(result, method))
                        onFailure.Invoke(character, result);
                    if (!result.HasError)
                        onSuccess.Invoke(result.Result);
                }

                foreach(var monitor in character.QueryMonitors.Where(
                    monitor => monitor.Method.HasParent(method)))
                {
                    character.QueryMonitors.Query(monitor.Method);
                }
            })
        {
            m_character = character;
        }

        /// <summary>
        /// Gets the required API key information are known.
        /// </summary>
        /// <returns>False if an API key was required and not found.</returns>
        protected override bool HasAPIKey => m_character.Identity.ESIKeys.Any();

        /// <summary>
        /// Gets a value indicating whether this monitor has access to data.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this monitor has access; otherwise, <c>false</c>.
        /// </value>
        public override bool HasAccess
        {
            get
            {
                m_apiKey = m_character.Identity.FindAPIKeyWithAccess((ESIAPICharacterMethods)Method);
                return m_apiKey != null;
            }
        }
        
        /// <summary>
        /// Performs the query to the provider, passing the required arguments.
        /// </summary>
        /// <param name="provider">The API provider to use.</param>
        /// <param name="callback">The callback invoked on the UI thread after a result has been queried.</param>
        /// <exception cref="System.ArgumentNullException">provider</exception>
        protected override void QueryAsyncCore(APIProvider provider, APIProvider.ESIRequestCallback<T> callback)
        {
            provider.ThrowIfNull(nameof(provider));

            provider.QueryEsiAsync(Method, m_apiKey.AccessToken, m_character.CharacterID, callback);
        }
    }
}
