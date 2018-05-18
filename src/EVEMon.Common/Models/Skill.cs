using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using EVEMon.Common.Attributes;
using EVEMon.Common.Data;
using EVEMon.Common.Enumerations;
using EVEMon.Common.Extensions;
using EVEMon.Common.Interfaces;
using EVEMon.Common.Serialization.Eve;

namespace EVEMon.Common.Models
{
    /// <summary>
    /// Represents a skill bound to a character, not only including the skill static data (represented by <see cref="StaticSkill" />)
    /// but also the number of SP and level the character has. 
    /// </summary>
    [EnforceUIThreadAffinity]
    public sealed class Skill : IStaticSkill
    {
        private const long s_maxLevel = 5L;

        private readonly List<SkillLevel> m_prereqs = new List<SkillLevel>();
        private long m_currentSkillPoints;
        // Previous dev of EVEMon made these longs, when patently unnecessary
        private long m_skillLevel;
        private long m_level;
        private bool m_owned;
        private bool m_known;

        private static Skill s_unknownSkill;


        #region Construction, initialization, exportation, updates

        /// <summary>
        /// Constructor for an unknown skill.
        /// </summary>
        private Skill()
        {
            StaticData = StaticSkill.UnknownStaticSkill;
            Group = SkillGroup.UnknownSkillGroup;
        }

        /// <summary>
        /// Internal constructor, only used for character creation and updates
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="group"></param>
        /// <param name="skill"></param>
        internal Skill(Character owner, SkillGroup group, StaticSkill skill)
        {
            Character = owner;
            StaticData = skill;
            Group = group;
        }

        /// <summary>
        /// Completes the initialization once all the character's skills have been initialized
        /// </summary>
        /// <param name="skills">The array of the character's skills.</param>
        public void CompleteInitialization(Skill[] skills)
        {
            m_prereqs.AddRange(StaticData.Prerequisites.Select(staticSkillLevel =>
                new SkillLevel(skills[staticSkillLevel.Skill.ArrayIndex], staticSkillLevel.Level)));
        }

        /// <summary>
        /// Imports and updates from the provided deserialization object
        /// </summary>
        /// <param name="src"></param>
        /// <param name="fromCCP"></param>
        internal void Import(SerializableCharacterSkill src, bool fromCCP)
        {
            m_owned = src.OwnsBook;
            m_known = fromCCP | src.IsKnown;
            SkillPoints = src.Skillpoints;
            LastConfirmedLvl = src.Level;
            m_level = Math.Min(s_maxLevel, src.Level);
        }

        /// <summary>
        /// Resets the skill before we reimport data
        /// </summary>
        /// <param name="importFromCCP">When true, we will update it with up-to-date data from CCP</param>
        internal void Reset(bool importFromCCP)
        {
            m_known = false;
            SkillPoints = 0;
            LastConfirmedLvl = 0;
            m_level = 0;

            // Are we reloading the settings ?
            if (!importFromCCP)
                m_owned = false;
        }

        /// <summary>
        /// Check if the input level is higher than the current level
        /// </summary>
        /// <param name="newLevel"></param>
        /// <returns></returns>
        internal bool HasBeenCompleted(QueuedSkill queuedSkill)
        {
            return Math.Min(queuedSkill.Level, s_maxLevel) > m_level;
        }

        /// <summary>
        /// Updates the skill to match the one from the queue
        /// </summary>
        internal void UpdateSkillProgress(QueuedSkill queuedSkill)
        {
            m_known = true;
            if (queuedSkill.IsCompleted)
            {
                m_level = Math.Min(queuedSkill.Level, s_maxLevel);
                SkillPoints = queuedSkill.EndSP;
            }
            else
            {
                m_level = queuedSkill.Level - 1;
                SkillPoints = queuedSkill.CurrentSP;
            }
        }

        /// <summary>
        /// Exports the skill to a serialization object
        /// </summary>
        /// <returns></returns>
        internal SerializableCharacterSkill Export()
        {
            SerializableCharacterSkill dest = new SerializableCharacterSkill
            {
                ID = StaticData.ID,
                Name = StaticData.Name,
                Level = m_level,
                Skillpoints = m_currentSkillPoints,
                OwnsBook = IsOwned,
                IsKnown = m_known
            };

            return dest;
        }

        /// <summary>
        /// Gets or sets true if the skill is owned.
        /// </summary>
        public bool IsOwned
        {
            get { return m_owned | (HasBookInAssets && !m_known); }
            set
            {
                m_owned = value;
                EveMonClient.OnCharacterUpdated(Character);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has book in assets.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has book in assets; otherwise, <c>false</c>.
        /// </value>
        public bool HasBookInAssets
        {
            get
            {
                CCPCharacter ccpCharacter = Character as CCPCharacter;
                return (ccpCharacter != null) && ccpCharacter.Assets.Any(asset => asset.Item != null && asset.Item.ID == ID);
            }
        }

        #endregion


        #region Core properties

        /// <summary>
        /// Gets the character this skill is bound to.
        /// </summary>
        public Character Character { get; }

        /// <summary>
        /// Gets the underlying static data.
        /// </summary>
        public StaticSkill StaticData { get; }

        /// <summary>
        /// Gets this skill's id.
        /// </summary>
        public int ID => StaticData.ID;

        /// <summary>
        /// Gets a zero-based index for skills (allow the use of arrays to optimize computations).
        /// </summary>
        public int ArrayIndex => StaticData.ArrayIndex;

        /// <summary>
        /// Gets this skill's name.
        /// </summary>
        public string Name => StaticData.Name;

        /// <summary>
        /// Gets this skill's description.
        /// </summary>
        public string Description => StaticData.Description;

        /// <summary>
        /// Gets whether this skill is known.
        /// </summary>
        public bool IsKnown => m_known || IsTraining;

        /// <summary>
        /// Gets the skill group this skill is part of.
        /// </summary>
        public SkillGroup Group { get; }

        /// <summary>
        /// Gets whether this skill and all its prereqs are trainable on a trial account.
        /// </summary>
        public bool IsTrainableOnTrialAccount => StaticData.IsTrainableOnTrialAccount;

        /// <summary>
        /// Gets true if this is a public skill.
        /// </summary>
        public bool IsPublic => StaticData.IsPublic;

        /// <summary>
        /// Gets the skill cost in ISK.
        /// </summary>
        public long Cost => StaticData.Cost;

        /// <summary>
        /// Gets a formatted display of the ISK cost.
        /// </summary>
        public string FormattedCost => StaticData.FormattedCost;

        /// <summary>
        /// Gets the primary attribute of this skill.
        /// </summary>
        public EveAttribute PrimaryAttribute => StaticData.PrimaryAttribute;

        /// <summary>
        /// Gets the secondary attribute of this skill.
        /// </summary>
        public EveAttribute SecondaryAttribute => StaticData.SecondaryAttribute;

        /// <summary>
        /// Gets the rank of this skill.
        /// </summary>
        public Int64 Rank => StaticData.Rank;

        /// <summary>
        /// Gets the current level of this skill, as gotten from CCP or possibly estimated by EVEMon according to training informations.
        /// </summary>
        public Int64 Level
        {
            get
            {
                m_skillLevel = LastConfirmedLvl;
                Int64 skillPointsToNextLevel = StaticData.GetPointsRequiredForLevel(Math.Min(LastConfirmedLvl + 1, 5));

                while (skillPointsToNextLevel > 0 && m_currentSkillPoints >= skillPointsToNextLevel && m_skillLevel < 5)
                {
                    m_skillLevel++;
                    skillPointsToNextLevel = StaticData.GetPointsRequiredForLevel(Math.Min(m_skillLevel + 1, 5));
                }

                return m_skillLevel;
            }
        }

        /// <summary>
        /// Gets the level gotten from CCP during the last update.
        /// </summary>
        public Int64 LastConfirmedLvl { get; private set; }

        /// <summary>
        /// Gets the skill's prerequisites
        /// </summary>
        public IEnumerable<SkillLevel> Prerequisites => m_prereqs;

        /// <summary>
        /// Gets all the prerequisites. I.e, for eidetic memory, it will return <c>{ instant recall IV }</c>.
        /// The order matches the hierarchy.
        /// </summary>
        /// <remarks>Please notice, they may be redundancies.</remarks>
        public IEnumerable<SkillLevel> AllPrerequisites => StaticData.AllPrerequisites.ToCharacter(Character);

        /// <summary>
        /// Gets the training speed.
        /// </summary>
        public int SkillPointsPerHour => (int)Math.Round(Character?.GetBaseSPPerHour(this) ?? 0);

        #endregion


        #region Helper properties and methods

        /// <summary>
        /// Gets the unknown skill.
        /// </summary>
        /// <value>
        /// The unknown skill.
        /// </value>
        public static Skill UnknownSkill => s_unknownSkill ?? (s_unknownSkill = new Skill());

        /// <summary>
        /// Return current Level in Roman.
        /// </summary>
        public string RomanLevel => GetRomanFromInt(Level);

        /// <summary>
        /// Gets true if the skill is queued.
        /// </summary>
        public bool IsQueued
        {
            get
            {
                CCPCharacter ccpCharacter = Character as CCPCharacter;

                // Current character isn't a CCP character, so can't have a Queue.
                if (ccpCharacter == null)
                    return false;

                SkillQueue skillQueue = ccpCharacter.SkillQueue;
                return skillQueue.Where(x => x.Skill != null).Any(skill => StaticData.ID == skill.Skill.ID);
            }
        }

        /// <summary>
        /// Gets true if the skill is currently in training.
        /// </summary>
        public bool IsTraining
        {
            get
            {
                CCPCharacter ccpCharacter = Character as CCPCharacter;
                return ccpCharacter != null && ccpCharacter.IsTraining && ccpCharacter.CurrentlyTrainingSkill != null &&
                       ccpCharacter.CurrentlyTrainingSkill.Skill == this;
            }
        }

        /// <summary>
        /// Gets the current skill points of this skill (possibly estimated for skills in training).
        /// </summary>
        public Int64 SkillPoints
        {
            get
            {
                // Is it in training ? Then we estimate the current SP
                if (!IsTraining)
                    return m_currentSkillPoints;

                CCPCharacter ccpCharacter = Character as CCPCharacter;
                return ccpCharacter?.CurrentlyTrainingSkill.CurrentSP ?? m_currentSkillPoints;
            }
            internal set { m_currentSkillPoints = value; }
        }

        /// <summary>
        /// Gets the completed fraction (between 0.0 and 1.0).
        /// </summary>
        public float FractionCompleted
        {
            get
            {
                // Lv 5 ? 
                if (m_level == 5)
                    return 1.0f;

                // Not partially trained ? Then it's 1.0
                Int64 levelSp = StaticData.GetPointsRequiredForLevel(m_level);
                if (SkillPoints <= levelSp)
                    return 0.0f;

                // Partially trained, let's compute the difference with the previous level
                float nextLevelSp = StaticData.GetPointsRequiredForLevel(m_level + 1);
                float fraction = (SkillPoints - levelSp) / (nextLevelSp - levelSp);

                return fraction <= 1 ? fraction : fraction % 1;
            }
        }

        /// <summary>
        /// Gets the percentage completion (between 0.0 and 100.0).
        /// </summary>
        public double PercentCompleted => FractionCompleted * 100;

        /// <summary>
        /// Gets whether this skill is partially trained (true) or fully trained (false).
        /// </summary>
        public bool IsPartiallyTrained
        {
            get
            {
                if (Level == 5)
                    return false;

                bool partialLevel = SkillPoints > StaticData.GetPointsRequiredForLevel(Level),
                    isNotFullyTrained = GetLeftPointsRequiredToLevel(Level + 1) != 0,
                    isPartiallyTrained = partialLevel && isNotFullyTrained;
                return isPartiallyTrained;
            }
        }

        /// <summary>
        /// Gets true if all the prerequisites are met.
        /// </summary>
        public bool ArePrerequisitesMet => m_prereqs.AreTrained();

        /// <summary>
        /// Converts an integer into a roman number.
        /// </summary>
        /// <param name="number">Number from 1 to 5.</param>
        /// <returns>Roman number string.</returns>
        public static string GetRomanFromInt(Int64 number)
        {
            switch (number)
            {
                case 1:
                    return "I";
                case 2:
                    return "II";
                case 3:
                    return "III";
                case 4:
                    return "IV";
                case 5:
                    return "V";
                default:
                    return "(nulla)";
            }
        }

        /// <summary>
        /// Converts a roman number into an integer.
        /// </summary>
        /// <param name="r">Roman number from I to V.</param>
        /// <returns>Integer number.</returns>
        public static int GetIntFromRoman(string r)
        {
            switch (r)
            {
                case "I":
                    return 1;
                case "II":
                    return 2;
                case "III":
                    return 3;
                case "IV":
                    return 4;
                case "V":
                    return 5;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Calculate the time to train this skill to the next level including prerequisites.
        /// </summary>
        /// <returns>Time it will take</returns>
        public TimeSpan GetLeftTrainingTimeToNextLevel => Level == 5 ? TimeSpan.Zero : GetLeftTrainingTimeToLevel(Level + 1);

        /// <summary>
        /// Returns the string representation of this skill (the name).
        /// </summary>
        /// <returns>
        /// The name of the skill.
        /// </returns>
        public override string ToString() => Name;

        /// <summary>
        /// Gets this skill's representation for the provided character.
        /// </summary>
        /// <param name="character"></param>
        /// <returns></returns>
        public Skill ToCharacter(Character character)
        {
            character.ThrowIfNull(nameof(character));

            return character.Skills[StaticData.ArrayIndex];
        }

        #endregion


        #region Computations

        /// <summary>
        /// Calculate the time it will take to train a certain amount of skill points.
        /// </summary>
        /// <param name="points">The amount of skill points.</param>
        /// <returns>Time it will take.</returns>
        public TimeSpan GetTimeSpanForPoints(Int64 points)
            => Character?.GetTimeSpanForPoints(this, points) ?? TimeSpan.Zero;

        /// <summary>
        /// Calculates the cumulative points required to reach the given level of this skill, starting from the current SP.
        /// </summary>
        /// <remarks>For a result starting from 0 SP, use the equivalent method on <see cref="StaticSkill"/>.</remarks>
        /// <param name="level">The level.</param>
        /// <returns>The required nr. of points.</returns>
        public Int64 GetLeftPointsRequiredToLevel(Int64 level)
        {
            Int64 result = StaticData.GetPointsRequiredForLevel(level) - SkillPoints;

            return result < 0 ? 0 : result;
        }

        /// <summary>
        /// Calculates the cumulative points required for the only level of this skill, including the current SP if the level is partially trained.
        /// </summary>
        /// <remarks>For a result not including the current SP, use the equivalent method on <see cref="StaticSkill"/>.</remarks>
        /// <param name="level">The level.</param>
        /// <returns>The required nr. of points.</returns>
        private Int64 GetLeftPointsRequiredForLevelOnly(int level)
        {
            if (level == 0)
                return 0;

            Int64 startSP = Math.Max(SkillPoints, StaticData.GetPointsRequiredForLevel(level - 1));
            Int64 result = StaticData.GetPointsRequiredForLevel(level) - startSP;

            return result < 0 ? 0 : result;
        }

        /// <summary>
        /// Calculates the time required to reach the given level of this skill, starting from the current SP.
        /// </summary>
        /// <remarks>For a result starting from 0 SP, use the equivalent method on <see cref="StaticSkill"/>.</remarks>
        /// <param name="level">The level.</param>
        /// <returns>The required time span.</returns>
        public TimeSpan GetLeftTrainingTimeToLevel(Int64 level) => GetTimeSpanForPoints(GetLeftPointsRequiredToLevel(level));

        /// <summary>
        /// Calculates the time required for the only level of this skill, including the current SP if the level is partially trained.
        /// </summary>
        /// <remarks>For a result not including the current SP, use the equivalent method on <see cref="StaticSkill"/>.</remarks>
        /// <param name="level">The level.</param>
        /// <returns>The required time span.</returns>
        public TimeSpan GetLeftTrainingTimeForLevelOnly(int level)
            => GetTimeSpanForPoints(GetLeftPointsRequiredForLevelOnly(level));

        #endregion


        #region Conversion operators

        /// <summary>
        /// Returns the static skill the provided skill is based on.
        /// </summary>
        /// <param name="skill"></param>
        /// <returns></returns>
        public static implicit operator StaticSkill(Skill skill) => skill?.StaticData;

        #endregion


        #region IStaticSkill Members

        Collection<StaticSkillLevel> IStaticSkill.Prerequisites => StaticData.Prerequisites;

        StaticSkillGroup IStaticSkill.Group => StaticData.Group;

        #endregion
    }
}
