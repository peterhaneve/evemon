using EVEMon.Common.Attributes;
using EVEMon.Common.Collections;
using EVEMon.Common.Models;

namespace EVEMon.Common.Data
{
    /// <summary>
    /// Represents a certificate category.
    /// </summary>
    [EnforceUIThreadAffinity]
    public sealed class CertificateCategoryCollection : ReadonlyKeyedCollection<int, CertificateGroup>
    {
        /// <summary>
        /// Constructor for the character initialization.
        /// </summary>
        /// <param name="character"></param>
        internal CertificateCategoryCollection(Character character)
        {
            foreach (var srcCategory in StaticCertificates.AllGroups)
            {
                var category = new CertificateGroup(character, srcCategory);
                Items[category.ID] = category;
            }
        }
    }
}
