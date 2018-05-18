using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using EVEMon.Common.Attributes;
using EVEMon.Common.Collections;
using EVEMon.Common.Extensions;
using EVEMon.Common.Serialization.Eve;
using EVEMon.Common.Service;

namespace EVEMon.Common.Models
{
    /// <summary>
    /// Represents a character's skills queue.
    /// </summary>
    [EnforceUIThreadAffinity]
    public sealed class SkillQueue : ReadonlyCollection<QueuedSkill>
    {
        private readonly CCPCharacter m_character;
        private readonly DateTime m_startTime = DateTime.UtcNow;


        #region Constructor

        /// <summary>
        /// Default constructor, only used by <see cref="Character"/>
        /// </summary>
        /// <param name="character">The character this collection is bound to.</param>
        internal SkillQueue(CCPCharacter character)
        {
            m_character = character;

            EveMonClient.TimerTick += EveMonClient_TimerTick;
        }

        #endregion


        /// <summary>
        /// Called when the object gets disposed.
        /// </summary>
        internal void Dispose()
        {
            EveMonClient.TimerTick -= EveMonClient_TimerTick;
        }


        #region Properties

        /// <summary>
        /// Gets true when the character is currently training (non-empty and non-paused skill queue), false otherwise.
        /// </summary>
        public bool IsTraining => !IsPaused && Items.Any();

        /// <summary>
        /// Gets the last completed skill.
        /// </summary>
        public QueuedSkill LastCompleted { get; private set; }

        /// <summary>
        /// Gets the training end time (UTC).
        /// </summary>
        public DateTime EndTime => !Items.Any() ? DateTime.UtcNow : Items.Last().EndTime;

        /// <summary>
        /// Gets the skill currently in training.
        /// </summary>
        public QueuedSkill CurrentlyTraining => Items.FirstOrDefault();

        /// <summary>
        /// Gets true whether the skill queue is currently paused.
        /// </summary>
        public bool IsPaused { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the skill queue has less than the warning threshold worth of training.
        /// </summary>
        /// <value>
        /// <c>true</c> if the skill queue has less than the warning threshold worth of training; otherwise, <c>false</c>.
        /// </value>
        public bool LessThanWarningThreshold => EndTime <= DateTime.UtcNow.AddDays(Settings.UI.MainWindow.SkillQueueWarningThresholdDays);

        /// <summary>
        /// Gets the warning threshold time span.
        /// </summary>
        /// <value>
        /// The warning threshold time span.
        /// </value>
        public static TimeSpan WarningThresholdTimeSpan => TimeSpan.FromDays(Settings.UI.MainWindow.SkillQueueWarningThresholdDays);

        #endregion


        #region Update

        /// <summary>
        /// When the timer ticks, on every second, we update the skill.
        /// </summary>
        private void UpdateOnTimerTick()
        {
            var skillsCompleted = new LinkedList<QueuedSkill>();
            QueuedSkill skill;
            Skill skillTrained;

            // Pops all the completed skills
            while (Items.Count > 0)
            {
                skill = Items[0];
                skillTrained = skill.Skill;

                if (!skill.IsCompleted)
                {
                    // Still training, stop loop and update the skill (ESI doesn't move skills from skillqueue to skill list until you log in)
                    skillTrained?.UpdateSkillProgress(skill);
                    break;
                }

                // Check that it is in fact completed (ESI doesn't move skills from skillqueue to skill list until you log in)
                if (skill.IsCompleted && skillTrained != null && skillTrained.HasBeenCompleted(skill))
                {
                    // The skill has been completed
                    skillTrained.UpdateSkillProgress(skill);
                    skillsCompleted.AddLast(skill);
                    LastCompleted = skill;
                    // Send an email alert if configured
                    if (!Settings.IsRestoring && Settings.Notifications.SendMailAlert)
                        Emailer.SendSkillCompletionMail(Items, skill, m_character);
                }
                Items.RemoveAt(0);
            }

            if (skillsCompleted.Any() && !Settings.IsRestoring)
            {
                // Send a notification, only if skills were completed
                EveMonClient.Notifications.NotifySkillCompletion(m_character, skillsCompleted);
                EveMonClient.OnCharacterQueuedSkillsCompleted(m_character, skillsCompleted);
            }
        }

        #endregion


        #region Global Event Handlers

        /// <summary>
        /// Handles the TimerTick event of the EveMonClient control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void EveMonClient_TimerTick(object sender, EventArgs e)
        {
            if (IsPaused || !m_character.Monitored)
                return;

            UpdateOnTimerTick();
        }

        #endregion


        #region Importation/Exportation

        /// <summary>
        /// Generates a deserialization object.
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<SerializableQueuedSkill> Export() => Items.Select(skill => skill.Export());

        /// <summary>
        /// Imports data from a serialization object.
        /// </summary>
        /// <param name="serial"></param>
        internal void Import(IEnumerable<SerializableQueuedSkill> serial)
        {
            IsPaused = false;

            // If the queue is paused, CCP sends empty start and end time
            // So we base the start time on when the skill queue was started
            DateTime startTimeWhenPaused = m_startTime;

            // Imports the queued skills and checks whether they are paused
            Items.Clear();
            foreach (SerializableQueuedSkill serialSkill in serial)
            {
                if (!serialSkill.IsTraining)
                    IsPaused = true;

                // Creates the skill queue
                Items.Add(new QueuedSkill(m_character, serialSkill, ref startTimeWhenPaused));
            }

            // Update skills with the imported data
            UpdateOnTimerTick();

            // Fires the event regarding the character skill queue update
            EveMonClient.OnCharacterSkillQueueUpdated(m_character);
        }

        #endregion


        #region Helper Methods

        /// <summary>
        /// Gets an enumeration of rectangles a skill renders in within a specified rectangle.
        /// </summary>
        /// <param name="skill">Skill that exists within the queue</param>
        /// <param name="width">Width of the canvas</param>
        /// <param name="height">Height of the canvas</param>
        /// <returns>
        /// Rectangle representing the area within the visual
        /// queue the skill occupies.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">skill</exception>
        public IEnumerable<RectangleF> GetSkillRects(QueuedSkill skill, int width, int height)
        {
            skill.ThrowIfNull(nameof(skill));

            List<RectangleF> skillRects = new List<RectangleF>();

            TimeSpan endTimeSpan = EndTime.Subtract(DateTime.UtcNow);
            double totalSeconds = (endTimeSpan < WarningThresholdTimeSpan
                ? WarningThresholdTimeSpan
                : endTimeSpan).TotalSeconds;

            TimeSpan relativeStart = skill.StartTime.Subtract(DateTime.UtcNow);
            TimeSpan relativeFinish = skill.EndTime.Subtract(DateTime.UtcNow);
            double start = Math.Floor(relativeStart.TotalSeconds / totalSeconds * width);
            double afterOneDayFinish = Math.Floor(WarningThresholdTimeSpan.TotalSeconds / totalSeconds * width);
            double finish = Math.Floor(relativeFinish.TotalSeconds / totalSeconds * width);

            // If the start time is before now set it to zero
            if (start < 0)
                start = 0;

            // If the after one day finish time is after finish time set it to finish
            if (afterOneDayFinish > finish)
                afterOneDayFinish = finish;

            skillRects.Add(new RectangleF((float)start, 0, (float)(finish - start), height));
            skillRects.Add(new RectangleF((float)start, 0, (float)(afterOneDayFinish - start), height));
            skillRects.Add(new RectangleF((float)afterOneDayFinish, 0, (float)(finish - afterOneDayFinish), height));

            return skillRects;
        }

        /// <summary>
        /// Gets the width of a one day skill queue.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <returns></returns>
        public double GetOneDaySkillQueueWidth(int width)
        {
            double totalSeconds = EndTime.Subtract(DateTime.UtcNow).TotalSeconds;
            return  Math.Floor(WarningThresholdTimeSpan.TotalSeconds / totalSeconds * width);
        }

        #endregion
    }
}
