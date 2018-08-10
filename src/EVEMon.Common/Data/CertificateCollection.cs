using EVEMon.Common.Attributes;
using EVEMon.Common.Collections;
using EVEMon.Common.Models;
using System.Linq;

namespace EVEMon.Common.Data
{
    /// <summary>
    /// Represents a collection of certificates
    /// </summary>
    [EnforceUIThreadAffinity]
    public sealed class CertificateCollection : ReadonlyKeyedCollection<int, Certificate>
    {
        /// <summary>
        /// Constructor
        /// <param name="character">The character</param>
        /// </summary>
        internal CertificateCollection(Character character)
        {
            // Builds the list
            foreach (var certGroup in character.CertificateCategories)
                foreach (var certClass in certGroup)
                {
                    var certificate = certClass.Certificate;
                    Items[certificate.ID] = certificate;
                }
        }

        /// <summary>
        /// Gets a certificate from its ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Certificate this[int id] => GetByKey(id);

        /// <summary>
        /// Initializes the certificates.
        /// </summary>
        internal void Initialize()
        {
            while (Items.Values.Aggregate(false, (current, cert) => current | cert.
                TryUpdateCertificateStatus())) ;
        }
    }
}
