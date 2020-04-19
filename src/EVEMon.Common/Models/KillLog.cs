using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using EVEMon.Common.Constants;
using EVEMon.Common.Data;
using EVEMon.Common.Enumerations;
using EVEMon.Common.Extensions;
using EVEMon.Common.Helpers;
using EVEMon.Common.Serialization.Eve;
using EVEMon.Common.Service;
using EVEMon.Common.Serialization.Esi;

namespace EVEMon.Common.Models
{
    public sealed class KillLog : IComparable<KillLog>
    {
        /// <summary>
        /// Occurs when kill log victim ship image updated.
        /// </summary>
        public event EventHandler KillLogVictimShipImageUpdated;


        #region Fields

        private readonly List<KillLogItem> m_items = new List<KillLogItem>();
        private Image m_image;

        #endregion


        #region Constructor

        /// <summary>
        /// Constructor from the API.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <param name="src">The source.</param>
        internal KillLog(Character character, SerializableKillLogListItem src)
        {
            Character = character;
            KillTime = src.KillTime;
            TimeSinceKill = DateTime.UtcNow.Subtract(src.KillTime);
            MoonID = src.MoonID;
            Victim = src.Victim;
            Attackers = src.Attackers;
            SolarSystem = StaticGeography.GetSolarSystemByID(src.SolarSystemID);

            m_items.AddRange(src.Items.Select(item => new KillLogItem(item)));

            Group = src.Victim.ID == character.CharacterID ? KillGroup.Losses : KillGroup.Kills;

            UpdateCharacterNames();
        }
        
        #endregion


        #region Public Properties

        /// <summary>
        /// Gets the character.
        /// </summary>
        /// <value>
        /// The character.
        /// </value>
        public Character Character { get; }

        /// <summary>
        /// Gets the solar system.
        /// </summary>
        public SolarSystem SolarSystem { get; }

        /// <summary>
        /// Gets the kill time.
        /// </summary>
        public DateTime KillTime { get; }

        /// <summary>
        /// Gets the time since kill.
        /// </summary>
        public TimeSpan TimeSinceKill { get; }

        /// <summary>
        /// Gets the moon ID.
        /// </summary>
        public int MoonID { get; }

        /// <summary>
        /// Gets the victim.
        /// </summary>
        public SerializableKillLogVictim Victim { get; }

        /// <summary>
        /// Gets the attackers.
        /// </summary>
        public IEnumerable<SerializableKillLogAttackersListItem> Attackers { get; }

        /// <summary>
        /// Gets the final blow attacker.
        /// </summary>
        public SerializableKillLogAttackersListItem FinalBlowAttacker => Attackers.Single(x => x.FinalBlow);

        /// <summary>
        /// Gets or sets the group.
        /// </summary>
        public KillGroup Group { get; }

        /// <summary>
        /// Gets the items.
        /// </summary>
        public IEnumerable<KillLogItem> Items => m_items;

        /// <summary>
        /// Gets the victim image.
        /// </summary>
        public Image VictimShipImage
        {
            get
            {
                if (m_image != null)
                    return m_image;

                GetVictimShipImageAsync().ConfigureAwait(false);

                return m_image ?? (m_image = GetDefaultImage());
            }
        }

        #endregion


        #region Helper Methods

        /// <summary>
        /// Exports this object to a serializable form.
        /// </summary>
        /// <returns>The SerializableKillLogListItem representing this object.</returns>
        public SerializableKillLogListItem Export()
        {
            var exported = new SerializableKillLogListItem()
            {
                KillTime = KillTime,
                MoonID = MoonID,
                SolarSystemID = SolarSystem?.ID ?? 0,
                Victim = Victim
            };
            // Export items
            foreach (var item in m_items)
                exported.Items.Add(item.Export());
            // Export attackers
            foreach (var attacker in Attackers)
                exported.Attackers.Add(attacker);
            return exported;
        }

        /// <summary>
        /// Gets the victim's ship image.
        /// </summary>
        private async Task GetVictimShipImageAsync()
        {
            Uri uri = ImageHelper.GetTypeImageURL(Victim.ShipTypeID);
            Image img = await ImageService.GetImageAsync(uri).ConfigureAwait(false);
            if (img != null) {
                m_image = img;
                // Notify the subscriber that we got the image
                KillLogVictimShipImageUpdated?.ThreadSafeInvoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Updates the names and corporations/alliances of victims and attackers.
        /// </summary>
        public void UpdateCharacterNames()
        {
            if (Victim != null)
            {
                // Update victim's info
                Victim.AllianceName = EveIDToName.GetIDToName(Victim.AllianceID);
                Victim.CorporationName = EveIDToName.GetIDToName(Victim.CorporationID);
                Victim.FactionName = EveIDToName.GetIDToName(Victim.FactionID);
                Victim.Name = EveIDToName.GetIDToName(Victim.ID);
            }
            if (Attackers != null)
                foreach (var attacker in Attackers)
                {
                    // Update attacker's info
                    attacker.AllianceName = EveIDToName.GetIDToName(attacker.AllianceID);
                    attacker.CorporationName = EveIDToName.GetIDToName(attacker.CorporationID);
                    attacker.FactionName = EveIDToName.GetIDToName(attacker.FactionID);
                    attacker.Name = EveIDToName.GetIDToName(attacker.ID);
                }
        }

        /// <summary>
        /// Gets the default image.
        /// </summary>
        /// <returns></returns>
        private static Bitmap GetDefaultImage() => new Bitmap(32, 32);

        #endregion


        #region Inherited Methods

        public int CompareTo(KillLog other)
        {
            // Default order should be recent first
            return -KillTime.CompareTo(other.KillTime);
        }

        #endregion

    }
}
