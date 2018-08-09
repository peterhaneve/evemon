using System;
using System.Collections.Generic;
using System.Linq;
using EVEMon.Common.Attributes;
using EVEMon.Common.Collections;
using EVEMon.Common.Constants;
using EVEMon.Common.Data;
using EVEMon.Common.Enumerations;
using EVEMon.Common.Enumerations.CCPAPI;
using EVEMon.Common.Extensions;
using EVEMon.Common.Interfaces;
using EVEMon.Common.Models.Collections;
using EVEMon.Common.Serialization.Eve;
using EVEMon.Common.Serialization.Settings;
using EVEMon.Common.SettingsObjects;
using EVEMon.Common.Serialization.Esi;
using EVEMon.Common.Service;
using System.Globalization;

namespace EVEMon.Common.Models
{
    /// <summary>
    /// Represents a base class for character
    /// </summary>
    [EnforceUIThreadAffinity]
    public abstract class Character : BaseCharacter
    {
        // Character name
        private string m_name;
        private string m_label;

        // Attributes
        private readonly CharacterAttribute[] m_attributes = new CharacterAttribute[5];

        // Skill Point Caching
        private DateTime m_skillPointTotalUpdated = DateTime.MinValue;
        private long m_lastSkillPointTotal;

        #region Initialization

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="identity">The identity for this character</param>
        /// <param name="guid">The unique identifier.</param>
        /// <exception cref="System.ArgumentNullException">identity</exception>
        protected Character(CharacterIdentity identity, Guid guid)
        {
            identity.ThrowIfNull(nameof(identity));

            CharacterID = identity.CharacterID;
            m_name = identity.CharacterName;

            Identity = identity;
            Guid = guid;
            m_label = string.Empty;

            Corporation = new Corporation(this);

            SkillGroups = new SkillGroupCollection(this);
            Skills = new SkillCollection(this);

            UpdateAccountStatus();

            EmploymentHistory = new EmploymentRecordCollection(this);
            ImplantSets = new ImplantSetCollection(this);
            Plans = new PlanCollection(this);
            CertificateCategories = new CertificateCategoryCollection(this);
            CertificateClasses = new CertificateClassCollection(this);
            Certificates = new CertificateCollection(this);
            MasteryShips = new MasteryShipCollection(this);

            for (int i = 0; i < m_attributes.Length; i++)
            {
                m_attributes[i] = new CharacterAttribute(this, (EveAttribute)i);
            }

            UISettings = new CharacterUISettings();
        }

        /// <summary>
        /// Updates the character's account status based on the last known status and the
        /// current skill queue / training times.
        /// </summary>
        /// <param name="statusType">The current account status</param>
        public void UpdateAccountStatus(AccountStatus.AccountStatusType statusType =
            AccountStatus.AccountStatusType.Unknown)
        {
            var skill = CurrentlyTrainingSkill;

            if (skill != null && skill.IsTraining)
            {
                // Try to determine account status based on training time
                var hoursToTrain = (skill.EndTime - skill.StartTime).TotalHours;
                var spToTrain = skill.EndSP - skill.StartSP;
                if (hoursToTrain > 0 && spToTrain > 0)
                {
                    // spPerHour must be greater than zero since numerator and denominator are
                    var spPerHour = spToTrain / hoursToTrain;
                    var rate = (int)Math.Round(GetOmegaSPPerHour(skill.Skill) / spPerHour, 0);
                    switch (rate)
                    {
                    case 1:
                        statusType = AccountStatus.AccountStatusType.Omega;
                        break;
                    case 2:
                        statusType = AccountStatus.AccountStatusType.Alpha;
                        break;
                    default:
                        statusType = AccountStatus.AccountStatusType.Unknown;
                        break;
                    }
                }
            }

            CharacterStatus = new AccountStatus(statusType);
        }

        #endregion


        #region Bio

        /// <summary>
        /// Gets a global identifier for this character.
        /// </summary>
        public Guid Guid { get; }

        /// <summary>
        /// Gets the identity for this character.
        /// </summary>
        public CharacterIdentity Identity { get; protected set; }

        /// <summary>
        /// Gets or sets true if the character is monitored and displayed on the main window.
        /// </summary>
        public bool Monitored
        {
            get { return EveMonClient.MonitoredCharacters.Contains(this); }
            set { EveMonClient.MonitoredCharacters.OnCharacterMonitoringChanged(this, value); }
        }

        /// <summary>
        /// Gets the ID for this character.
        /// </summary>
        public long CharacterID { get; protected set; }

        /// <summary>
        /// Gets or sets the source's name.
        /// By default, it's the character's name but it may be overriden to help distinct tabs on the main window.
        /// </summary>
        public string Name
        {
            get { return m_name; }
            set
            {
                if (m_name == value)
                    return;

                m_name = value;
                EveMonClient.OnCharacterUpdated(this);
            }
        }

        /// <summary>
        /// Gets the home station identifier.
        /// </summary>
        public long HomeStationID { get; private set; }

        /// <summary>
        /// Gets an adorned name, with (file), (url) or (cached) labels.
        /// </summary>
        public virtual string AdornedName => m_name;

        /// <summary>
        /// Gets the character's birthday.
        /// </summary>
        public DateTime Birthday { get; private set; }

        /// <summary>
        /// Gets the character's race.
        /// </summary>
        public string Race { get; private set; }

        /// <summary>
        /// Gets the character's bloodline.
        /// </summary>
        public string Bloodline { get; private set; }

        /// <summary>
        /// Gets the character's ancestry.
        /// </summary>
        public string Ancestry { get; private set; }

        /// <summary>
        /// Gets the character's gender.
        /// </summary>
        public string Gender { get; private set; }

        /// <summary>
        /// Gets or sets the corporation.
        /// </summary>
        /// <value>The corporation.</value>
        public Corporation Corporation { get; }

        /// <summary>
        /// Gets the id of the character's corporation.
        /// </summary>
        public long CorporationID { get; internal set; }

        /// <summary>
        /// Gets the name of the character's corporation.
        /// </summary>
        public string CorporationName { get; internal set; }

        /// <summary>
        /// Gets the name of the character's alliance.
        /// </summary>
        public string AllianceName { get; internal set; }

        /// <summary>
        /// Gets the id of the character's alliance.
        /// </summary>
        public long AllianceID { get; internal set; }

        /// <summary>
        /// Gets the name of the character's warfare faction.
        /// </summary>
        public string FactionName { get; internal set; }

        /// <summary>
        /// Gets the id of the character's warfare faction.
        /// </summary>
        public int FactionID { get; internal set; }

        /// <summary>
        /// Gets the free skill points.
        /// </summary>
        public int FreeSkillPoints { get; private set; }

        /// <summary>
        /// Gets the jump clone creation date.
        /// </summary>
        public DateTime JumpCloneLastJumpDate { get; private set; }

        /// <summary>
        /// Gets the available remaps.
        /// </summary>
        public short AvailableReMaps { get; private set; }

        /// <summary>
        /// Gets the last remap date.
        /// </summary>
        public DateTime LastReMapDate { get; private set; }

        /// <summary>
        /// Gets the last remap timed.
        /// </summary>
        public DateTime LastReMapTimed { get; private set; }

        /// <summary>
        /// Gets the remote station date.
        /// </summary>
        public DateTime RemoteStationDate { get; private set; }

        /// <summary>
        /// Gets or sets the jump activation date.
        /// </summary>
        public DateTime JumpActivationDate { get; private set; }

        /// <summary>
        /// Gets or sets the jump fatigue date.
        /// </summary>
        public DateTime JumpFatigueDate { get; private set; }

        /// <summary>
        /// Gets or sets the jump last update date.
        /// </summary>
        public DateTime JumpLastUpdateDate { get; private set; }

        /// <summary>
        /// Gets the current character's wallet balance.
        /// </summary>
        public decimal Balance { get; private set; }

        /// <summary>
        /// Gets true when the character is in a NPC corporation, false otherwise.
        /// </summary>
        public bool IsInNPCCorporation => CorporationID < int.MaxValue &&
            StaticGeography.GetCorporationByID((int)CorporationID) != null;

        #endregion


        #region Info

        /// <summary>
        /// Gets or sets the character's custom label. This is used for display only.
        /// </summary>
        public string Label
        {
            get
            {
                return m_label;
            }
            set
            {
                m_label = value ?? string.Empty;
                EveMonClient.OnCharacterLabelChanged(this);
            }
        }

        /// <summary>
        /// Generates a prefix to be used on the character's name in the overview and tab list
        /// when the character has a custom label.
        /// </summary>
        public string LabelPrefix => m_label.IsEmptyOrUnknown() ? string.Empty : "[" + m_label + "] ";

        /// <summary>
        /// Gets the character's ship name.
        /// </summary>
        public string ShipName { get; private set; }

        /// <summary>
        /// Gets the character's ship type name.
        /// </summary>
        public string ShipTypeName { get; private set; }

        /// <summary>
        /// Gets the character's last known location.
        /// </summary>
        public SerializableLocation LastKnownLocation { get; private set; }

        /// <summary>
        /// Gets the character's security status.
        /// </summary>
        public double SecurityStatus { get; private set; }

        /// <summary>
        /// Gets or sets the character's employment history.
        /// </summary>
        public EmploymentRecordCollection EmploymentHistory { get; }

        /// <summary>
        /// Gets the character's last known station location.
        /// </summary>
        public Station LastKnownStation
        {
            get
            {
                var loc = LastKnownLocation;
                if (loc == null)
                    return null;
                int id = loc.StationID;
                // If this is a CCP character, allow usage of ESI key to find citadel info
                return EveIDToStation.GetIDToStation(id != 0 ? id : loc.StructureID, this as
                    CCPCharacter);
            }
        }

        /// <summary>
        /// Gets the character's last known solar system location.
        /// </summary>
        public SolarSystem LastKnownSolarSystem => StaticGeography.GetSolarSystemByID(
            LastKnownLocation?.SolarSystemID ?? 0);

        /// <summary>
        /// Gets Alpha/Omega status for this character.
        /// </summary>
        public AccountStatus CharacterStatus { get; private set; }
        #endregion


        #region Certificates

        /// <summary>
        /// Gets the collection of certificate categories.
        /// </summary>
        public CertificateCategoryCollection CertificateCategories { get; }

        /// <summary>
        /// Gets the collection of certificate classes.
        /// </summary>
        public CertificateClassCollection CertificateClasses { get; }

        /// <summary>
        /// Gets the collection of certificates.
        /// </summary>
        public CertificateCollection Certificates { get; }

        #endregion


        #region Masteries

        /// <summary>
        /// Gets the collection of mastery ships.
        /// </summary>
        public MasteryShipCollection MasteryShips { get; }

        #endregion


        #region Attributes

        /// <summary>
        /// Gets the base attribute value for the given attribute.
        /// </summary>
        /// <param name="attribute">The attribute to retrieve.</param>
        /// <returns></returns>
        protected override ICharacterAttribute GetAttribute(EveAttribute attribute) => m_attributes[(int)attribute];

        #endregion


        #region Implants

        /// <summary>
        /// Gets the implants sets of the character and its clones.
        /// </summary>
        public ImplantSetCollection ImplantSets { get; }

        /// <summary>
        /// Gets the current implants' bonuses.
        /// </summary>
        public ImplantSet CurrentImplants => ImplantSets.Current;

        #endregion


        #region Skills

        /// <summary>
        /// Gets the collection of skills.
        /// </summary>
        public SkillCollection Skills { get; }

        /// <summary>
        /// Gets the collection of skill groups.
        /// </summary>
        public SkillGroupCollection SkillGroups { get; }

        /// <summary>
        /// Gets the total skill points for this character.
        /// </summary>
        /// <returns></returns>
        protected override Int64 TotalSkillPoints
        {
            get
            {
                // We only do the calculation once every second to avoid
                // excessive CPU utilization.
                if (m_skillPointTotalUpdated > DateTime.UtcNow.AddSeconds(-1))
                    return m_lastSkillPointTotal;

                m_lastSkillPointTotal = Skills.Sum(skill => skill.SkillPoints);
                m_skillPointTotalUpdated = DateTime.UtcNow;

                return m_lastSkillPointTotal;
            }
        }

        /// <summary>
        /// Gets the number of skills this character knows.
        /// </summary>
        public int KnownSkillCount => Skills.Count(skill => skill.IsKnown);

        /// <summary>
        /// Gets the number of skills currently known at the same level than the one specified.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <returns></returns>
        public int GetSkillCountAtLevel(int level) => Skills.Count(skill => skill.IsKnown && skill.LastConfirmedLvl == level);

        /// <summary>
        /// Gets the level of the given skill.
        /// </summary>
        /// <param name="skill"></param>
        /// <returns></returns>
        public override Int64 GetSkillLevel(StaticSkill skill)
        {
            skill.ThrowIfNull(nameof(skill));

            return Skills[skill.ID].Level;
        }

        /// <summary>
        /// Gets the level of the given skill.
        /// </summary>
        /// <param name="skill">The skill.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">skill</exception>
        public override Int64 GetSkillPoints(StaticSkill skill)
        {
            skill.ThrowIfNull(nameof(skill));

            return Skills[skill.ID].SkillPoints;
        }

        /// <summary>
        /// Gets the adjusted base skill points per hour
        /// based upon skill, attributes, and account status.
        /// </summary>
        /// <param name="skill">The skill.</param>
        /// <returns>Skill points earned per hour when training this skill</returns>
        public override float GetBaseSPPerHour(StaticSkill skill)
        {
            return CharacterStatus.TrainingRate * base.GetBaseSPPerHour(skill);
        }

        #endregion


        #region Plans

        /// <summary>
        /// Gets the collection of plans.
        /// </summary>
        public PlanCollection Plans { get; }

        #endregion


        #region Training

        /// <summary>
        /// Gets true when the character is currently training, false otherwise.
        /// </summary>
        public virtual bool IsTraining => false;

        /// <summary>
        /// Gets the skill currently in training.
        /// </summary>
        public virtual QueuedSkill CurrentlyTrainingSkill => null;

        #endregion


        #region Location Info

        /// <summary>
        /// Gets the active ship description.
        /// </summary>
        /// <returns></returns>
        public string GetActiveShipText()
        {
            string shipText = !String.IsNullOrEmpty(ShipTypeName) && !String.IsNullOrEmpty(ShipName)
                ? $"{ShipTypeName} [{ShipName}]" : EveMonConstants.UnknownText;
            return $"Active Ship: {shipText}";
        }

        /// <summary>
        /// Gets the last known location description.
        /// </summary>
        /// <returns></returns>
        public string GetLastKnownLocationText()
        {
            if (LastKnownLocation == null)
                return EveMonConstants.UnknownText;

            // Show the tooltip on when the user provides api key
            ESIKey apiKey = Identity.FindAPIKeyWithAccess(ESIAPICharacterMethods.Location);
            if (apiKey == null)
                return EveMonConstants.UnknownText;

            // Check if in an NPC station or in an outpost
            var system = (LastKnownStation?.SolarSystem) ?? LastKnownSolarSystem;

            // Not in a solar system ??? Then show default location
            return system != null ? $"{system.FullLocation} ({system.SecurityLevel:N1})"
                : "Lost in space";
        }

        /// <summary>
        /// Gets the last known docked description.
        /// </summary>
        /// <returns></returns>
        public string GetLastKnownDockedText()
        {
            if (LastKnownLocation == null)
                return EveMonConstants.UnknownText;
            
            // Show the tooltip on when the user provides api key
            ESIKey apiKey = Identity.FindAPIKeyWithAccess(ESIAPICharacterMethods.Location);
            if (apiKey == null)
                return EveMonConstants.UnknownText;

            Station station = LastKnownStation;
            
            // Not in any station ?
            if (station == null)
                return String.Empty;

            return station.Name;
        }

        #endregion


        #region Importation / exportation

        /// <summary>
        /// Create a serializable character sheet for this character.
        /// </summary>
        /// <returns></returns>
        public abstract SerializableSettingsCharacter Export();

        /// <summary>
        /// Fetches the data to the given serialization object, used by inheritors.
        /// </summary>
        /// <param name="serial">The serial.</param>
        /// <exception cref="System.ArgumentNullException">serial</exception>
        protected void Export(SerializableSettingsCharacter serial)
        {
            serial.ThrowIfNull(nameof(serial));

            serial.Guid = Guid;
            serial.ID = Identity.CharacterID;
            serial.Name = m_name;
            serial.HomeStationID = HomeStationID;
            serial.Birthday = Birthday;
            serial.Race = Race;
            serial.BloodLine = Bloodline;
            serial.Ancestry = Ancestry;
            serial.Gender = Gender;
            serial.CorporationName = CorporationName;
            serial.CorporationID = CorporationID;
            serial.AllianceName = AllianceName;
            serial.AllianceID = AllianceID;
            serial.FreeSkillPoints = FreeSkillPoints;
            serial.FreeRespecs = AvailableReMaps;
            serial.CloneJumpDate = JumpCloneLastJumpDate;
            serial.LastRespecDate = LastReMapDate;
            serial.LastTimedRespec = LastReMapTimed;
            serial.RemoteStationDate = RemoteStationDate;
            serial.JumpActivationDate = JumpActivationDate;
            serial.JumpFatigueDate = JumpFatigueDate;
            serial.JumpLastUpdateDate = JumpLastUpdateDate;
            serial.Balance = Balance;

            // Info
            serial.Label = m_label;
            serial.ShipName = ShipName;
            serial.ShipTypeName = ShipTypeName;
            serial.SecurityStatus = SecurityStatus;
            serial.LastKnownLocation = LastKnownLocation;

            // Employment History
            serial.EmploymentHistory.AddRange(EmploymentHistory.Export());

            // Attributes
            serial.Attributes.Intelligence = Intelligence.Base;
            serial.Attributes.Perception = Perception.Base;
            serial.Attributes.Willpower = Willpower.Base;
            serial.Attributes.Charisma = Charisma.Base;
            serial.Attributes.Memory = Memory.Base;

            // Implants sets
            serial.ImplantSets = ImplantSets.Export();

            // Skills
            serial.Skills.AddRange(Skills.Export());
        }

        /// <summary>
        /// Imports data from the given character sheet informations.
        /// </summary>
        /// <param name="serial">The serialized character sheet</param>
        internal void Import(CCPAPIResult<SerializableAPICharacterSheet> serial)
        {
            Import(serial.Result);
            EveMonClient.OnCharacterUpdated(this);
        }

        /// <summary>
        /// Imports data from the given character sheet informations.
        /// </summary>
        /// <param name="serial">The serialized character sheet</param>
        internal void Import(EsiAPICharacterSheet serial)
        {
            // Import from ESI
            m_name = serial.Name;
            Birthday = serial.Birthday;
            Race = serial.Race.ToString().UnderscoresToDashes();
            Bloodline = serial.BloodLine.ToString().UnderscoresToDashes();
            Ancestry = serial.Ancestry.ToString().UnderscoresToSpaces();
            Gender = serial.Gender.ToTitleCase();
            CorporationID = serial.CorporationID;
            AllianceID = serial.AllianceID;
            FactionID = serial.FactionID;
            SecurityStatus = serial.SecurityStatus;
            // Enable bypass since we would have a circular loop otherwise
            CorporationName = EveIDToName.GetIDToName(CorporationID, true);
            AllianceName = EveIDToName.GetIDToName(AllianceID, true);
            FactionName = EveIDToName.GetIDToName(FactionID);
        }

        /// <summary>
        /// Imports data from the given character sheet informations.
        /// </summary>
        /// <param name="serial">The serialized character sheet</param>
        /// <exception cref="System.ArgumentNullException">serial</exception>
        protected void Import(SerializableSettingsCharacter serial)
        {
            serial.ThrowIfNull(nameof(serial));

            Import((SerializableCharacterSheetBase)serial);

            // Implants
            ImplantSets.Import(serial.ImplantSets);
        }

        /// <summary>
        /// Imports data from the given character sheet informations.
        /// </summary>
        /// <param name="serial">The serialized character sheet</param>
        private void Import(SerializableAPICharacterSheet serial)
        {
            Import((SerializableCharacterSheetBase)serial);
            // Implants
            if (serial.Implants.Any() || serial.JumpClones.Any())
                ImplantSets.Import(serial);
        }

        /// <summary>
        /// Imports data from the given character account balance.
        /// </summary>
        /// <param name="result">The serialized character account balance</param>
        internal void Import(string result)
        {
            decimal balance;
            if (decimal.TryParse(result, NumberStyles.AllowDecimalPoint, CultureInfo.
                    InvariantCulture, out balance))
                Balance = balance;
        }

        /// <summary>
        /// Imports data from the given character location.
        /// </summary>
        /// <param name="location">The serialized character location</param>
        internal void Import(EsiAPILocation location)
        {
            LastKnownLocation = location.ToXMLItem();
        }

        /// <summary>
        /// Imports data from the given character ship information.
        /// </summary>
        /// <param name="ship">The serialized character ship information</param>
        internal void Import(EsiAPIShip ship)
        {
            ShipName = ship.ShipName;
            ShipTypeName = StaticItems.GetItemName(ship.ShipTypeID);
        }

        /// <summary>
        /// Imports data from the given character jump fatigue.
        /// </summary>
        /// <param name="fatigue">The serialized character jump fatigue</param>
        internal void Import(EsiAPIJumpFatigue fatigue)
        {
            JumpLastUpdateDate = fatigue.LastUpdate;
            JumpFatigueDate = fatigue.FatigueExpires;
            JumpActivationDate = fatigue.LastJump;
        }

        /// <summary>
        /// Imports data from the given character clone information.
        /// </summary>
        /// <param name="clones">The serialized character clone information</param>
        internal void Import(EsiAPIClones clones)
        {
            // Information about clone jumping and clone moving
            JumpCloneLastJumpDate = clones.LastCloneJump;
            RemoteStationDate = clones.LastStationChange;
            HomeStationID = clones.HomeLocation.LocationID;
            ImplantSets.Import(clones);
        }

        /// <summary>
        /// Imports data from the given character attribute information.
        /// </summary>
        /// <param name="attribs">The serialized character attribute information</param>
        internal void Import(EsiAPIAttributes attribs)
        {
            // Remap info
            DateTime lastRespec = DateTime.MinValue, nextRespec = attribs.RemapCooldownDate;
            if (nextRespec > DateTime.MinValue)
                lastRespec = nextRespec.Subtract(TimeSpan.FromDays(365.0));
            LastReMapTimed = lastRespec;
            AvailableReMaps = attribs.BonusRemaps;
            LastReMapDate = attribs.LastRemap;

            SetAttribute(EveAttribute.Intelligence, attribs.Intelligence);
            SetAttribute(EveAttribute.Perception, attribs.Perception);
            SetAttribute(EveAttribute.Willpower, attribs.Willpower);
            SetAttribute(EveAttribute.Charisma, attribs.Charisma);
            SetAttribute(EveAttribute.Memory, attribs.Memory);
        }

        /// <summary>
        /// Attributes include current implants! Therefore, subtract the information
        /// about current implants since those were fetched with Implants beforehand.
        /// </summary>
        /// <param name="attribute">The attribute to set.</param>
        /// <param name="value">The value reported by Attributes ESI call.</param>
        private void SetAttribute(EveAttribute attribute, int value)
        {
            m_attributes[(int)attribute].Base = value - CurrentImplants[attribute]?.Bonus ?? 0;
        }

        /// <summary>
        /// Imports data from the given skills information.
        /// </summary>
        /// <param name="skills">The serialized character skill information</param>
        internal void Import(EsiAPISkills skills, EsiAPISkillQueue queue)
        {
            var newSkills = new LinkedList<SerializableCharacterSkill>();
            DateTime uselessDate = DateTime.UtcNow;
            FreeSkillPoints = skills.UnallocatedSP;

            // Keep track of the current skill queue's completed skills, as ESI does not
            // transfer them to the skills list until you login
            var dict = new Dictionary<long, SerializableQueuedSkill>();
            if (queue != null)
                foreach (var queuedSkill in queue.CreateSkillQueue())
                {
                    // If the skill is completed or currently training, we need it later to
                    // copy the progress over to the imported skills
                    if (queuedSkill.IsCompleted || queuedSkill.IsTraining)
                    {
                        if (!dict.ContainsKey(queuedSkill.ID))
                            dict.Add(queuedSkill.ID, queuedSkill);
                        else
                            dict[queuedSkill.ID] = queuedSkill;
                    }
                }
            // Convert skills to EVE format
            foreach (var skill in skills.Skills)
            {
                // Check if the skill is in the queue, and completed at a higher level or has
                // higher current SP
                if (dict.ContainsKey(skill.ID))
                {
                    var queuedSkill = dict[skill.ID];
                    if (queuedSkill.IsCompleted)
                    {
                        // Queued skill is completed, so make sure the imported skill is
                        // updated
                        skill.ActiveLevel = Math.Max(skill.ActiveLevel, queuedSkill.Level);
                        skill.Level = Math.Max(skill.Level, queuedSkill.Level);
                        skill.Skillpoints = Math.Max(skill.Skillpoints, queuedSkill.EndSP);
                    }
                    else if (queuedSkill.IsTraining)
                    {
                        // Queued skill is currently training - use QueuedSkill class to
                        // calculate the CurrentSP of the skill
                        var tempSkill = new QueuedSkill(this, queuedSkill, ref uselessDate);
                        skill.Skillpoints = Math.Max(skill.Skillpoints, tempSkill.CurrentSP);
                    }
                }
                newSkills.AddLast(skill.ToXMLItem());
            }
            Skills.Import(newSkills, true);
        }

        /// <summary>
        /// Imports data from the given implants information.
        /// </summary>
        /// <param name="implants">The serialized implant information</param>
        internal void Import(List<int> implants)
        {
            // Implants
            var newImplants = new LinkedList<SerializableNewImplant>();
            foreach (int implant in implants)
                newImplants.AddLast(new SerializableNewImplant()
                {
                    ID = implant,
                    Name = StaticItems.GetItemName(implant)
                });
            CurrentImplants.Import(newImplants);
        }

        /// <summary>
        /// Imports data from the given employment history information.
        /// </summary>
        /// <param name="history">The serialized employment history information</param>
        internal void Import(EsiAPIEmploymentHistory history)
        {
            EmploymentHistory.Import(history.ToXMLItem());
        }

        /// <summary>
        /// Imports data from the given character sheet informations.
        /// </summary>
        /// <param name="serial">The serialized character sheet</param>
        private void Import(SerializableCharacterSheetBase serial)
        {
            // Bio
            m_name = serial.Name;
            HomeStationID = serial.HomeStationID;
            Birthday = serial.Birthday;
            Race = serial.Race;
            Bloodline = serial.BloodLine;
            Ancestry = serial.Ancestry;
            Gender = serial.Gender;
            CorporationName = serial.CorporationName;
            CorporationID = serial.CorporationID;
            AllianceName = serial.AllianceName;
            AllianceID = serial.AllianceID;
            FreeSkillPoints = serial.FreeSkillPoints;
            AvailableReMaps = serial.FreeRespecs;
            JumpCloneLastJumpDate = serial.CloneJumpDate;
            LastReMapDate = serial.LastRespecDate;
            LastReMapTimed = serial.LastTimedRespec;
            RemoteStationDate = serial.RemoteStationDate;
            JumpActivationDate = serial.JumpActivationDate;
            JumpFatigueDate = serial.JumpFatigueDate;
            JumpLastUpdateDate = serial.JumpLastUpdateDate;
            Balance = serial.Balance;

            var settingsChar = serial as SerializableSettingsCharacter;
            if (settingsChar != null)
            {
                // Info
                m_label = settingsChar.Label ?? string.Empty;
                ShipName = settingsChar.ShipName;
                ShipTypeName = settingsChar.ShipTypeName;
                SecurityStatus = settingsChar.SecurityStatus;
                LastKnownLocation = settingsChar.LastKnownLocation;

                // Employment History
                EmploymentHistory.Import(settingsChar.EmploymentHistory);
            }

            // Attributes
            m_attributes[(int)EveAttribute.Intelligence].Base = serial.Attributes.Intelligence;
            m_attributes[(int)EveAttribute.Perception].Base = serial.Attributes.Perception;
            m_attributes[(int)EveAttribute.Willpower].Base = serial.Attributes.Willpower;
            m_attributes[(int)EveAttribute.Charisma].Base = serial.Attributes.Charisma;
            m_attributes[(int)EveAttribute.Memory].Base = serial.Attributes.Memory;

            // Skills
            Skills.Import(serial.Skills, serial is SerializableAPICharacterSheet);

            // Certificates
            Certificates.Initialize();

            // Masteries
            MasteryShips.Initialize();
        }

        /// <summary>
        /// Imports the given plans.
        /// </summary>
        /// <param name="plans"></param>
        internal void ImportPlans(IEnumerable<SerializablePlan> plans)
        {
            Plans.Import(plans);
        }

        /// <summary>
        /// Export the plans to the given list.
        /// </summary>
        /// <param name="list"></param>
        internal void ExportPlans(List<SerializablePlan> list)
        {
            list.AddRange(Plans.Select(plan => plan.Export()));
        }

        #endregion


        /// <summary>
        /// Gets the UI settings for this character.
        /// </summary>
        public CharacterUISettings UISettings { get; internal set; }

        /// <summary>
        /// Gets a unique hashcode for this character.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() => Guid.GetHashCode();

        /// <summary>
        /// Gets the name of the character.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => m_name;
    }
}
