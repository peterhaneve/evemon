using EVEMon.Common.Extensions;
using EVEMon.Common.Models;
using EVEMon.Common.Serialization;
using EVEMon.Common.Serialization.Esi;
using EVEMon.Common.Serialization.Eve;
using System;

namespace EVEMon.Common.CustomEventArgs
{
    public sealed class ESIKeyCreationEventArgs : EventArgs
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ESIKeyCreationEventArgs"/> class.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="refreshToken">The refresh token.</param>
        /// <param name="charInfo">The ESI key info.</param>
        /// <exception cref="System.ArgumentNullException">charInfo</exception>
        public ESIKeyCreationEventArgs(long id, ulong accessMask, string refreshToken, JsonResult<EsiAPITokenInfo> charInfo)
        {
            charInfo.ThrowIfNull(nameof(charInfo));

            ID = id;
            RefreshToken = refreshToken;
            AccessMask = accessMask;

            if (charInfo.HasError)
                CCPError = new CCPAPIError()
                {
                    ErrorCode = charInfo.ResponseCode,
                    ErrorMessage = charInfo.ErrorMessage ??
                        "No character result retrieved from ESI key"
                };
            else
            {
                EsiAPITokenInfo result = charInfo.Result;
                CCPError = null;
                long charId = result.CharacterID;
                string name = result.CharacterName;

                // Only one character per ESI key
                // Look for an existing character ID and update its name
                CharacterIdentity identity = EveMonClient.CharacterIdentities[charId];
                if (identity != null)
                    identity.CharacterName = name;
                else
                    // Create an identity if necessary
                    identity = EveMonClient.CharacterIdentities.Add(charId, name);
                Identity = identity;
            }
        }


        #endregion


        #region Properties

        /// <summary>
        /// Gets or sets the ID.
        /// </summary>
        /// <value>The ID.</value>
        public long ID { get; }

        /// <summary>
        /// Gets or sets the verification code.
        /// </summary>
        /// <value>The verification code.</value>
        public string RefreshToken { get; }

        /// <summary>
        /// Gets or sets the access mask.
        /// </summary>
        /// <value>The access mask.</value>
        public ulong AccessMask { get; }
        
        /// <summary>
        /// Gets or sets the expiration.
        /// </summary>
        /// <value>The expiration.</value>
        public DateTime Expiration { get; }
        
        /// <summary>
        /// Gets or sets the CCP error.
        /// </summary>
        /// <value>The CCP error.</value>
        public CCPAPIError CCPError { get; }
        
        /// <summary>
        /// Gets the identity available from this ESI key.
        /// </summary>
        public CharacterIdentity Identity { get; }

        #endregion


        #region Methods

        /// <summary>
        /// Creates or updates the ESI key.
        /// </summary>
        /// <returns></returns>
        public ESIKey CreateOrUpdate()
        {
            // Checks whether this ESI key already exists to update it
            ESIKey esiKey = EveMonClient.ESIKeys[ID];
            if (esiKey != null)
            {
                esiKey.Update(this);

                // Fires the event regarding the ESI key info update
                EveMonClient.OnESIKeyInfoUpdated(esiKey);
            }
            else
            {
                esiKey = new ESIKey(ID);
                esiKey.Update(this);
                EveMonClient.ESIKeys.Add(esiKey);
            }

            return esiKey;
        }

        #endregion

    }
}
