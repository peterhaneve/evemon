using System;
using System.Drawing;
using System.Threading.Tasks;
using EVEMon.Common.Constants;
using EVEMon.Common.Enumerations;
using EVEMon.Common.Extensions;
using EVEMon.Common.Helpers;
using EVEMon.Common.Service;
using EVEMon.Common.Serialization.Esi;

namespace EVEMon.Common.Models
{
    public sealed class Standing
    {
        public event EventHandler StandingImageUpdated;


        #region Fields

        private readonly long m_entityID;
        private readonly Character m_character;
        private string m_entityName;
        private Image m_image;

        #endregion


        #region Constructor

        /// <summary>
        /// Constructor from the API.
        /// </summary>
        /// <param name="character"></param>
        /// <param name="src"></param>
        internal Standing(Character character, EsiStandingsListItem src)
        {
            m_character = character;

            m_entityID = src.ID;
            m_entityName = EveMonConstants.UnknownText;
            StandingValue = src.StandingValue;
            Group = src.Group;
        }

        #endregion


        #region Public Properties

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string EntityName => m_entityName.IsEmptyOrUnknown() ?
            (m_entityName = EveIDToName.GetIDToName(m_entityID)) : m_entityName;

        /// <summary>
        /// Gets or sets the standing value.
        /// </summary>
        /// <value>The standing value.</value>
        public double StandingValue { get; }

        /// <summary>
        /// Gets or sets the group.
        /// </summary>
        /// <value>The group.</value>
        public StandingGroup Group { get; }

        /// <summary>
        /// Gets or sets the entity image.
        /// </summary>
        /// <value>The entity image.</value>
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

        /// <summary>
        /// Gets the effective standing.
        /// </summary>
        /// <value>The effective standing.</value>
        public double EffectiveStanding
        {
            get
            {
                int skillLevel = m_character.LastConfirmedSkillLevel((StandingValue < 0) ?
                    DBConstants.DiplomacySkillID : DBConstants.ConnectionsSkillID);
                return StandingValue + (10.0 - StandingValue) * (skillLevel * 0.04);
            }
        }

        /// <summary>
        /// Gets the standing status.
        /// </summary>
        /// <value>The status.</value>
        public static StandingStatus Status(double standing)
        {
            if (standing <= -5.5)
                return StandingStatus.Terrible;

            if (standing <= -0.5)
                return StandingStatus.Bad;

            if (standing < 0.5)
                return StandingStatus.Neutral;

            return standing < 5.5 ? StandingStatus.Good : StandingStatus.Excellent;
        }

        /// <summary>
        /// Gets the standing image.
        /// </summary>
        /// <param name="standing">The standing.</param>
        /// <returns></returns>
        public static Image GetStandingImage(int standing)
        {
            if (standing <= -5.5)
                return Properties.Resources.TerribleStanding;

            if (standing <= -0.5)
                return Properties.Resources.BadStanding;

            if (standing < 0.5)
                return Properties.Resources.NeutralStanding;

            return standing < 5.5 ? Properties.Resources.GoodStanding : Properties.Resources.ExcellentStanding;
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

                // Notify the subscriber that we got the image
                StandingImageUpdated?.ThreadSafeInvoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets the default image.
        /// </summary>
        /// <returns></returns>
        private Image GetDefaultImage()
        {
            switch (Group)
            {
                case StandingGroup.Agents:
                    return Properties.Resources.DefaultCharacterImage32;
                case StandingGroup.NPCCorporations:
                    return Properties.Resources.DefaultCorporationImage32;
                case StandingGroup.Factions:
                    return Properties.Resources.DefaultAllianceImage32;
            }
            return new Bitmap(32, 32);
        }

        /// <summary>
        /// Gets the image URL.
        /// </summary>
        private Uri GetImageUrl()
        {
            Uri uri;
            switch (Group)
            {
            case StandingGroup.NPCCorporations:
                uri = ImageHelper.GetCorporationImageURL(m_entityID);
                break;
            case StandingGroup.Factions:
                uri = ImageHelper.GetAllianceImageURL(m_entityID);
                break;
            case StandingGroup.Agents:
            default:
                uri = ImageHelper.GetPortraitUrl(m_entityID);
                break;
            }
            return uri;
        }

        #endregion
    }
}
