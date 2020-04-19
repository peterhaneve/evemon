using EVEMon.Common.Extensions;
using EVEMon.Common.Helpers;
using EVEMon.Common.Serialization.Eve;
using EVEMon.Common.Service;
using System;
using System.Drawing;
using System.Threading.Tasks;

namespace EVEMon.Common.Models
{
    public sealed class EmploymentRecord
    {
        public event EventHandler EmploymentRecordImageUpdated;


        #region Fields

        private readonly Character m_character;
        private readonly long m_corporationId;

        private string m_corporationName;
        private Image m_image;

        #endregion


        #region Constructor

        /// <summary>
        /// Constructor from the API.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <param name="src">The source.</param>
        /// <exception cref="System.ArgumentNullException">src</exception>
        public EmploymentRecord(Character character, SerializableEmploymentHistoryListItem src)
        {
            src.ThrowIfNull(nameof(src));

            m_character = character;
            m_corporationId = src.CorporationID;
            m_corporationName = string.IsNullOrWhiteSpace(src.CorporationName)
                ? EveIDToName.GetIDToName(src.CorporationID) : src.CorporationName;
            StartDate = src.StartDate;
        }

        /// <summary>
        /// Constructor from the settings.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <param name="src">The source.</param>
        /// <exception cref="System.ArgumentNullException">src</exception>
        public EmploymentRecord(Character character, SerializableEmploymentHistory src)
        {
            src.ThrowIfNull(nameof(src));

            m_character = character;
            m_corporationId = src.CorporationID;
            m_corporationName = src.CorporationName;
            StartDate = src.StartDate;
        }

        #endregion


        #region Public Properties

        /// <summary>
        /// Gets or sets the name of the corporation.
        /// </summary>
        /// <value>The name of the corporation.</value>
        public string CorporationName => m_corporationName.IsEmptyOrUnknown() ?
            (m_corporationName = EveIDToName.GetIDToName(m_corporationId)) : m_corporationName;

        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        /// <value>The start date.</value>
        public DateTime StartDate { get; }

        /// <summary>
        /// Gets the corporation image.
        /// </summary>
        /// <value>The corporation image.</value>
        public Image CorporationImage
        {
            get
            {
                if (m_image != null)
                    return m_image;

                GetImageAsync().ConfigureAwait(false);

                return m_image ?? (m_image = Properties.Resources.DefaultCorporationImage32);
            }
        }

        #endregion


        #region Helper Methods

        /// <summary>
        /// Gets the corporation image.
        /// </summary>
        private async Task GetImageAsync()
        {
            Uri uri = ImageHelper.GetCorporationImageURL(m_corporationId);
            Image img = await ImageService.GetImageAsync(uri).ConfigureAwait(false);
            if (img != null)
            {
                m_image = img;
                EmploymentRecordImageUpdated?.ThreadSafeInvoke(this, EventArgs.Empty);
            }
        }

        #endregion


        #region Export Method

        /// <summary>
        /// Exports the given object to a serialization object.
        /// </summary>
        public SerializableEmploymentHistory Export()
        {
            SerializableEmploymentHistory serial = new SerializableEmploymentHistory
            {
                CorporationID = m_corporationId,
                CorporationName = CorporationName,
                StartDate = StartDate
            };
            return serial;
        }

        #endregion

    }
}
