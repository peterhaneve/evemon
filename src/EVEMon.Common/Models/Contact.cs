using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using EVEMon.Common.Data;
using EVEMon.Common.Enumerations;
using EVEMon.Common.Extensions;
using EVEMon.Common.Helpers;
using EVEMon.Common.Service;
using EVEMon.Common.Serialization.Esi;

namespace EVEMon.Common.Models
{
    public sealed class Contact
    {
        public event EventHandler ContactImageUpdated;


        #region Fields

        private readonly long m_contactID;
        private readonly ContactType m_contactType;
        private Image m_image;
        private string m_contactName;

        #endregion


        /// <summary>
        /// Constructor from the API.
        /// </summary>
        /// <param name="src"></param>
        internal Contact(EsiContactListItem src)
        {
            m_contactID = src.ContactID;
            m_contactName = EveIDToName.GetIDToName(m_contactID);
            IsInWatchlist = src.InWatchlist;
            Standing = src.Standing;
            Group = src.Group == ContactGroup.Personal && StaticGeography.AllAgents.Any(
                x => x.ID == m_contactID) ? ContactGroup.Agent : src.Group;

            switch (src.Group)
            {
            case ContactGroup.Corporate:
                m_contactType = ContactType.Corporation;
                break;
            case ContactGroup.Alliance:
                m_contactType = ContactType.Alliance;
                break;
            default:
                m_contactType = ContactType.Character;
                break;
            }
        }


        #region Public Properties

        /// <summary>
        /// Gets the name of the contact.
        /// </summary>
        /// <value>
        /// The name of the contact.
        /// </value>
        public string Name => (m_contactName.IsEmptyOrUnknown()) ? (m_contactName =
            EveIDToName.GetIDToName(m_contactID)) : m_contactName;

        /// <summary>
        /// Gets a value indicating whether the contact is in the watchlist.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the contact is in the watchlist; otherwise, <c>false</c>.
        /// </value>
        public bool IsInWatchlist { get; }

        /// <summary>
        /// Gets the standing.
        /// </summary>
        public double Standing { get; }

        /// <summary>
        /// Gets the group.
        /// </summary>
        public ContactGroup Group { get; }


        /// <summary>
        /// Gets the entity image.
        /// </summary>
        public Image EntityImage
        {
            get
            {
                if (m_image != null)
                    return m_image;

                GetImageAsync().ConfigureAwait(false);

                return m_image ?? (m_image = GetDefaultImage());
            }
        }

        #endregion


        #region Helper Methods

        /// <summary>
        /// Gets the entity image.
        /// </summary>
        private async Task GetImageAsync()
        {
            Image img = await ImageService.GetImageAsync(GetImageUrl()).ConfigureAwait(false);
            if (img != null)
            {
                m_image = img;
                ContactImageUpdated?.ThreadSafeInvoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets the default image.
        /// </summary>
        /// <returns></returns>
        private Image GetDefaultImage()
        {
            switch (m_contactType)
            {
                case ContactType.Character:
                    return Properties.Resources.DefaultCharacterImage32;
                case ContactType.Corporation:
                    return Properties.Resources.DefaultCorporationImage32;
                case ContactType.Alliance:
                    return Properties.Resources.DefaultAllianceImage32;
            }
            return new Bitmap(32, 32);
        }

        /// <summary>
        /// Gets the image URL.
        /// </summary>
        /// <returns></returns>
        private Uri GetImageUrl()
        {
            Uri uri;
            switch (m_contactType) {
            case ContactType.Corporation:
                uri = ImageHelper.GetCorporationImageURL(m_contactID);
                break;
            case ContactType.Alliance:
                uri = ImageHelper.GetAllianceImageURL(m_contactID);
                break;
            case ContactType.Character:
            default:
                uri = ImageHelper.GetPortraitUrl(m_contactID);
                break;
            }
            return uri;
        }

        #endregion
    }
}
