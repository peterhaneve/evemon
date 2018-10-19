using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using EVEMon.Common.Collections;
using EVEMon.Common.Constants;
using EVEMon.Common.Enumerations.UISettings;
using EVEMon.Common.Exceptions;
using EVEMon.Common.Extensions;
using EVEMon.Common.Helpers;
using EVEMon.Common.Models;
using Google;
using Google.Apis.Auth.OAuth2.Responses;

namespace EVEMon.Common.ExternalCalendar
{
    public static class ExternalCalendar
    {
        /// <summary>
        /// Gets a value indicating whether MSOutlook is installed.
        /// </summary>
        /// <value><c>true</c> if MSOutlook is installed; otherwise, <c>false</c>.</value>
        public static bool OutlookInstalled => OutlookCalendarEvent.OutlookApplication != null;

        /// <summary>
        /// Gets true if the Outlook calendar exist.
        /// </summary>
        /// <param name="useDefaultCalendar">if set to <c>true</c> [use default].</param>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public static bool OutlookCalendarExist(bool useDefaultCalendar, string path)
            => OutlookCalendarEvent.OutlookCalendarExist(useDefaultCalendar, path);

        /// <summary>
        /// Process the selected character skill queue into the selected calendar.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <exception cref="System.ArgumentNullException">character</exception>
        public static async Task UpdateCalendar(CCPCharacter character)
        {
            character.ThrowIfNull(nameof(character));

            SkillQueue skillQueue = character.SkillQueue;

            foreach (QueuedSkill queuedSkill in skillQueue)
            {
                bool lastSkillInQueue = queuedSkill == skillQueue.Last();
                int queuePosition = skillQueue.IndexOf(queuedSkill) + 1;

                // Continue if user has selected 'Last Queued Skill Only'
                if (Settings.Calendar.LastQueuedSkillOnly && !lastSkillInQueue)
                    continue;

                try
                {
                    if (Settings.Calendar.Provider == CalendarProvider.Outlook && OutlookInstalled)
                        await DoOutlookAppointmentAsync(queuedSkill, queuePosition, lastSkillInQueue);

                    if (Settings.Calendar.Provider == CalendarProvider.Google)
                        await DoGoogleAppointmentAsync(queuedSkill, queuePosition, lastSkillInQueue);
                }
                catch (Exception ex)
                {
                    ExceptionHandler.LogRethrowException(ex);
                    throw;
                }
            }
        }

        /// <summary>
        /// Process the queue into MS Outlook.
        /// </summary>
        /// <param name="queuedSkill">The queued skill.</param>
        /// <param name="queuePosition">The queue position.</param>
        /// <param name="lastSkillInQueue">if set to <c>true</c> skill is the last in queue.</param>
        private static async Task DoOutlookAppointmentAsync(QueuedSkill queuedSkill, int queuePosition, bool lastSkillInQueue)
        {
            // Get the calendar
            if (!OutlookCalendarEvent.OutlookCalendarExist(Settings.Calendar.UseOutlookDefaultCalendar))
            {
                MessageBox.Show(@"Outlook calendar does not exist. Please check your settings.", @"Outlook Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Set the subject to the character name and the skill and level in queue for uniqueness sake
            OutlookCalendarEvent outlookAppointmentFilter = new OutlookCalendarEvent
            {
                StartDate = DateTime.Now.AddDays(-40),
                EndDate = DateTime.Now.AddDays(100),
                Subject = string.Format(
                    CultureConstants.DefaultCulture,
                    "{0} - {1} {2}",
                    queuedSkill.Owner.Name,
                    queuedSkill.SkillName,
                    Skill.GetRomanFromInt(queuedSkill.Level))
            };


            // Pull the list of appointments, hopefully we should either get 1 or none back
            await outlookAppointmentFilter.ReadEventsAsync();

            // If there is an appointment, get the first one
            bool foundAppointment = false;
            if (outlookAppointmentFilter.ItemCount > 0)
                foundAppointment = outlookAppointmentFilter.GetEvent();

            // Update the appointment we may have pulled or the new one
            // Set the appointment length to 5 minutes, starting at the estimated completion date and time
            // Reminder value was already validated
            // Use the values from the screen as these may differ what the user has set for defaults
            outlookAppointmentFilter.StartDate = queuedSkill.EndTime.ToLocalTime();
            outlookAppointmentFilter.EndDate = queuedSkill.EndTime.ToLocalTime().AddMinutes(5);
            outlookAppointmentFilter.ItemReminder = Settings.Calendar.UseReminding;
            outlookAppointmentFilter.AlternateReminder = Settings.Calendar.UseAlternateReminding;
            outlookAppointmentFilter.EarlyReminder = Settings.Calendar.EarlyReminding;
            outlookAppointmentFilter.LateReminder = Settings.Calendar.LateReminding;
            outlookAppointmentFilter.Minutes = Settings.Calendar.RemindingInterval;

            await outlookAppointmentFilter.AddOrUpdateEventAsync(foundAppointment, queuePosition, lastSkillInQueue);
        }

        /// <summary>
        /// Process the queue into Google calendar.
        /// </summary>
        /// <param name="queuedSkill">The queued skill.</param>
        /// <param name="queuePosition">The queue position.</param>
        /// <param name="lastSkillInQueue">if set to <c>true</c> skill is the last in queue.</param>
        private static async Task DoGoogleAppointmentAsync(QueuedSkill queuedSkill, int queuePosition, bool lastSkillInQueue)
        {
            try
            {
                // Set the subject to the character name and the skill and level in queue for uniqueness sakes
                GoogleCalendarEvent googleAppointmentFilter = new GoogleCalendarEvent
                {
                    StartDate = DateTime.Now.AddDays(-40),
                    EndDate = DateTime.Now.AddDays(100),
                    Subject = string.Format(
                        CultureConstants.DefaultCulture,
                        "{0} - {1} {2}",
                        queuedSkill.Owner.Name,
                        queuedSkill.SkillName,
                        Skill.GetRomanFromInt(queuedSkill.Level))
                };

                // Pull the list of appointments, hopefully we should either get 1 or none back
                await googleAppointmentFilter.ReadEventsAsync();

                // If there is are appointments, see if any match the subject
                bool foundAppointment = false;
                if (googleAppointmentFilter.ItemCount > 0)
                    foundAppointment = googleAppointmentFilter.GetEvent();

                // Update the appointment we may have pulled or the new one
                // Set the appointment length to 5 minutes, starting at the estimated completion date and time
                // Reminder interval was already validated
                // Use the values from the screen as these may differ what the user has set for defaults
                googleAppointmentFilter.StartDate = queuedSkill.EndTime.ToLocalTime();
                googleAppointmentFilter.EndDate = queuedSkill.EndTime.ToLocalTime().AddMinutes(5);
                googleAppointmentFilter.ItemReminder = Settings.Calendar.UseReminding;
                googleAppointmentFilter.AlternateReminder = Settings.Calendar.UseAlternateReminding;
                googleAppointmentFilter.EarlyReminder = Settings.Calendar.EarlyReminding;
                googleAppointmentFilter.LateReminder = Settings.Calendar.LateReminding;
                googleAppointmentFilter.Minutes = Settings.Calendar.RemindingInterval;
                googleAppointmentFilter.ReminderMethod = Settings.Calendar.GoogleEventReminder;

                await googleAppointmentFilter.AddOrUpdateEventAsync(foundAppointment, queuePosition, lastSkillInQueue);
            }
            catch (TokenResponseException ex)
            {
                MessageBox.Show(ex.Error.ErrorDescription, @"Google Calendar");
            }
            catch (GoogleApiException ex)
            {
                MessageBox.Show(ex.Error.Message, @"Google Calendar");
            }
            catch (APIException ex)
            {
                MessageBox.Show(ex.Message, ex.ErrorCode ?? @"Google Calendar");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, @"Google Calendar");
            }
        }
    }
}