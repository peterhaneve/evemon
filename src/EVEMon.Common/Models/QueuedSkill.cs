using System;
using System.Linq;
using EVEMon.Common.Attributes;
using EVEMon.Common.Serialization.Eve;

namespace EVEMon.Common.Models
{
    /// <summary>
    /// Represents a skill training.
    /// </summary>
    [EnforceUIThreadAffinity]
    public sealed class QueuedSkill
    {
        /// <summary>
        /// Deserialization constructor.
        /// </summary>
        /// <param name="character">The character for this training</param>
        /// <param name="serial">The serialization object for this training</param>
        /// <param name="startTimeWhenPaused">Training starttime when the queue is actually paused.
        /// Indeed, in such case, CCP returns empty start and end time, so we compute a "what if we start now" scenario.</param>
        internal QueuedSkill(Character character, SerializableQueuedSkill serial,
            ref DateTime startTimeWhenPaused)
        {
            Owner = character;
            StartSP = serial.StartSP;
            EndSP = serial.EndSP;
            Level = serial.Level;
            Skill = character.Skills[serial.ID];

            if (serial.IsTraining)
            {
                // Not paused, we should trust CCP
                StartTime = serial.StartTime;
                EndTime = serial.EndTime;
            }
            else
            {
                // StartTime and EndTime were empty on the serialization object if the skill was paused
                // So we compute a "what if we start now" scenario
                StartTime = startTimeWhenPaused;
                if (Skill != null)
                {
                    Skill.SkillPoints = StartSP;
                    startTimeWhenPaused += Skill.GetLeftTrainingTimeForLevelOnly(Level);
                }
                EndTime = startTimeWhenPaused;
            }
        }

        /// <summary>
        /// Gets the character training this.
        /// </summary>
        public Character Owner { get; }

        /// <summary>
        /// Gets the trained level.
        /// </summary>
        public int Level { get; }

        /// <summary>
        /// Gets the trained skill. May be null if the skill is not in our datafiles.
        /// </summary>
        public Skill Skill { get; }

        /// <summary>
        /// Gets the skill name, or "Unknown Skill" if the skill was not in our datafiles.
        /// </summary>
        public string SkillName => (Skill ?? Skill.UnknownSkill).Name;

        /// <summary>
        /// Gets the training start time (UTC).
        /// </summary>
        public DateTime StartTime { get; }

        /// <summary>
        /// Gets the time this training will be completed (UTC).
        /// </summary>
        public DateTime EndTime { get; }

        /// <summary>
        /// Gets the number of SP this skill had when the training started.
        /// </summary>
        public int StartSP { get; }

        /// <summary>
        /// Gets the number of SP this skill will have once the training is over.
        /// </summary>
        public int EndSP { get; }

        /// <summary>
        /// Gets the fraction completed, between 0 and 1.
        /// </summary>
        public float FractionCompleted => Skill == null ? 0.0f : (Skill == Skill.UnknownSkill ?
            (float)(1.0 - EndTime.Subtract(DateTime.UtcNow).TotalMilliseconds /
            EndTime.Subtract(StartTime).TotalMilliseconds) : Skill.FractionCompleted);

        /// <summary>
        /// Computes an estimation of the current SP.
        /// </summary>
        public int CurrentSP
        {
            get
            {
                int estimatedSP = (int)(EndSP - EndTime.Subtract(DateTime.UtcNow).TotalHours *
                    SkillPointsPerHour);
                return IsTraining ? Math.Max(estimatedSP, StartSP) : StartSP;
            }
        }

        /// <summary>
        /// Gets the rank.
        /// </summary>
        /// <value>
        /// The rank.
        /// </value>
        public long Rank
        {
            get
            {
                if (Skill != Skill.UnknownSkill)
                    return Skill.Rank;

                switch (Level)
                {
                    case 0:
                        return 0;
                    case 1:
                        return EndSP / 250;
                    case 2:
                        return EndSP / 1414;
                    case 3:
                        return EndSP / 8000;
                    case 4:
                        return EndSP / 45255;
                    case 5:
                        return EndSP / 256000;
                }
                return Skill.Rank;
            }
        }

        /// <summary>
        /// Gets the training speed.
        /// </summary>
        /// <returns></returns>
        public double SkillPointsPerHour => (Skill == Skill.UnknownSkill) ?
            Math.Ceiling((EndSP - StartSP) / EndTime.Subtract(StartTime).TotalHours) :
            Skill.SkillPointsPerHour;

        /// <summary>
        /// Computes the remaining time.
        /// </summary>
        /// <value>The remaining time.</value>
        /// <returns> Returns <see cref="TimeSpan.Zero"/> if already completed.</returns>
        public TimeSpan RemainingTime
        {
            get
            {
                TimeSpan left = EndTime.Subtract(DateTime.UtcNow);
                return left < TimeSpan.Zero ? TimeSpan.Zero : left;
            }
        }

        /// <summary>
        /// Gets true if the skill is currently in training.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the skill is training; otherwise, <c>false</c>.
        /// </value>
        public bool IsTraining
        {
            get
            {
                var ccpCharacter = Owner as CCPCharacter;
                return Skill.IsTraining || (ccpCharacter != null && ccpCharacter.SkillQueue.
                    IsTraining && ccpCharacter.SkillQueue.First() == this);
            }
        }

        /// <summary>
        /// Gets true if the training has been completed, false otherwise.
        /// </summary>
        public bool IsCompleted => EndTime <= DateTime.UtcNow;

        public override bool Equals(object obj)
        {
            var other = obj as QueuedSkill;
            string otherName = other?.SkillName;
            return otherName != null && otherName.Equals(SkillName, StringComparison.
                InvariantCulture) && StartSP == other.StartSP && EndSP == other.EndSP;
        }

        public override int GetHashCode()
        {
            return SkillName?.GetHashCode() ?? 0;
        }

        /// <summary>
        /// Generates a deserialization object.
        /// </summary>
        /// <returns></returns>
        internal SerializableQueuedSkill Export()
        {
            SerializableQueuedSkill skill = new SerializableQueuedSkill
            {
                ID = Skill?.ID ?? 0,
                Level = Level,
                StartSP = StartSP,
                EndSP = EndSP,
            };

            // CCP's API indicates paused training skill with missing start and end times
            // Mimicing them is ugly but necessary
            if (!Owner.IsTraining)
                return skill;

            skill.StartTime = StartTime;
            skill.EndTime = EndTime;

            return skill;
        }

        /// <summary>
        /// Gets a string representation of this skill.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"{SkillName} {Skill.GetRomanFromInt(Level)}";
    }
}
