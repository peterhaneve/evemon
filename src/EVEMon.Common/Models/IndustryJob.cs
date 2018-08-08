using EVEMon.Common.Constants;
using EVEMon.Common.Data;
using EVEMon.Common.Enumerations;
using EVEMon.Common.Enumerations.CCPAPI;
using EVEMon.Common.Extensions;
using EVEMon.Common.Serialization.Esi;
using EVEMon.Common.Serialization.Settings;
using EVEMon.Common.Service;
using System;

namespace EVEMon.Common.Models
{
    public sealed class IndustryJob
    {
        private long m_installedItemLocationID;

        /// <summary>
        /// The maximum number of days after job ended. Beyond this limit, we do not import jobs anymore.
        /// </summary>
        internal const int MaxEndedDays = 7;


        #region Constructor

        /// <summary>
        /// Constructor from the API.
        /// </summary>
        /// <param name="src">The source.</param>
        /// <param name="issuedFor">Whether this jobs was issued for the corporation or
        /// character.</param>
        /// <exception cref="System.ArgumentNullException">src</exception>
        internal IndustryJob(EsiJobListItem src, IssuedFor issuedFor)
        {
            src.ThrowIfNull(nameof(src));

            PopulateJobInfo(src, issuedFor);
            State = GetState(src);
            LastStateChange = DateTime.UtcNow;
            ActiveJobState = GetActiveJobState();
        }

        /// <summary>
        /// Constructor from an object deserialized from the settings file.
        /// </summary>
        /// <param name="src">The source.</param>
        /// <exception cref="System.ArgumentNullException">src</exception>
        internal IndustryJob(SerializableJob src)
        {
            src.ThrowIfNull(nameof(src));

            ID = src.JobID;
            State = src.State;
            StartDate = src.StartDate;
            EndDate = src.EndDate;
            PauseDate = src.PauseDate;
            LastStateChange = src.LastStateChange;
            IssuedFor = src.IssuedFor == IssuedFor.None ? IssuedFor.Character : src.IssuedFor;
            ActiveJobState = GetActiveJobState();
        }

        #endregion


        #region Properties

        /// <summary>
        /// Gets true if we have notified the user.
        /// </summary>
        public bool NotificationSend { get; set; }

        /// <summary>
        /// When true, the job will be deleted unless it was found on the API feed.
        /// </summary>
        internal bool MarkedForDeletion { get; set; }

        /// <summary>
        /// Gets or sets the jobs state.
        /// </summary>
        public JobState State { get; private set; }

        /// <summary>
        /// Gets or sets the active jobs state.
        /// </summary>
        public ActiveJobState ActiveJobState { get; set; }

        /// <summary>
        /// Gets the last state change.
        /// </summary>
        public DateTime LastStateChange { get; private set; }

        /// <summary>
        /// Gets the estimated time to completion.
        /// </summary>
        public string TTC
        {
            get
            {
                if (State == JobState.Paused)
                    return new DateTime(EndDate.Subtract(PauseDate).Ticks).
                        ToRemainingTimeDigitalDescription(DateTimeKind.Utc);
                if (State == JobState.Active && EndDate > DateTime.UtcNow)
                    return EndDate.ToRemainingTimeDigitalDescription(DateTimeKind.Utc);
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the job ID.
        /// </summary>
        public long ID { get; private set; }

        /// <summary>
        /// Gets or sets the installer ID.
        /// </summary>
        /// <value>The installer ID.</value>
        public long InstallerID { get; set; }

        /// <summary>
        /// Gets the installed item (can only be a blueprint).
        /// </summary>
        public Blueprint InstalledItem { get; private set; }

        /// <summary>
        /// Gets the output item (can be a blueprint or item).
        /// </summary>
        public Item OutputItem { get; private set; }

        /// <summary>
        /// Gets the job runs.
        /// </summary>
        public int Runs { get; private set; }

        /// <summary>
        /// Gets the job runs.
        /// </summary>
        public double Cost { get; private set; }

        /// <summary>
        /// Gets the job runs.
        /// </summary>
        public double Probability { get; private set; }

        /// <summary>
        /// Gets the successful runs.
        /// </summary>
        public int SuccessfulRuns { get; private set; }

        /// <summary>
        /// Gets the job activity.
        /// </summary>
        public BlueprintActivity Activity { get; private set; }

        /// <summary>
        /// Get the blueprint's type.
        /// </summary>
        public BlueprintType BlueprintType { get; private set; }

        /// <summary>
        /// Gets the job installed material efficiency level.
        /// </summary>
        public int InstalledME { get; private set; }

        /// <summary>
        /// Gets the job installed productivity level.
        /// </summary>
        public int InstalledTE { get; private set; }

        /// <summary>
        /// Gets the time the job was installed.
        /// </summary>
        public DateTime InstalledTime => StartDate;

        /// <summary>
        /// Gets the time the job begins.
        /// </summary>
        public DateTime StartDate { get; private set; }

        /// <summary>
        /// Gets the time the job ends.
        /// </summary>
        public DateTime EndDate { get; private set; }

        /// <summary>
        /// Gets the time the job was paused.
        /// </summary>
        public DateTime PauseDate { get; private set; }

        /// <summary>
        /// Gets where this job is installed.
        /// </summary>
        public string Installation { get; private set; }

        /// <summary>
        /// Gets the solar system where this job is located.
        /// </summary>
        public SolarSystem SolarSystem { get; private set; }

        /// <summary>
        /// Gets the job installation full celestrial path.
        /// </summary>
        public string FullLocation => $"{SolarSystem.FullLocation} > {Installation}";

        /// <summary>
        /// Gets for which the job was issued.
        /// </summary>
        public IssuedFor IssuedFor { get; private set; }

        /// <summary>
        /// Gets true if the job is active.
        /// </summary>
        public bool IsActive => State == JobState.Active;

        /// <summary>
        /// Checks whether the given API object matches with this job.
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        private bool MatchesWith(EsiJobListItem src) => src.JobID == ID;

        /// <summary>
        /// Checks whether the given API object has been modified.
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        private bool IsModified(EsiJobListItem src) => src.EndDate != EndDate ||
            src.PauseDate != PauseDate;

        #endregion


        #region Importation, Exportation

        /// <summary>
        /// Exports the given object to a serialization object.
        /// </summary>
        internal SerializableJob Export() => new SerializableJob
        {
            JobID = ID,
            State = State,
            StartDate = StartDate,
            EndDate = EndDate,
            PauseDate = PauseDate,
            IssuedFor = IssuedFor,
            LastStateChange = LastStateChange,
        };

        /// <summary>
        /// Try to update this job with a serialization object from the API.
        /// </summary>
        /// <param name="src">The serializable source.</param>
        /// <param name="issuedFor">Whether this jobs was issued for the corporation or
        /// character.</param>
        /// <param name="character">The character owning this job.</param>
        /// <returns>True if import sucessful otherwise, false.</returns>
        internal bool TryImport(EsiJobListItem src, IssuedFor issuedFor, CCPCharacter character)
        {
            bool matches = MatchesWith(src);
            // Note that, before a match is found, all jobs have been marked for deletion:
            // m_markedForDeletion == true
            if (matches)
            {
                MarkedForDeletion = false;
                // Update information (if ID is the same it may have been modified)
                if (IsModified(src))
                {
                    // Job is from a serialized object, so populate the missing info
                    if (InstalledItem == null)
                        PopulateJobInfo(src, issuedFor);
                    else
                    {
                        EndDate = src.EndDate;
                        PauseDate = src.PauseDate;
                    }
                    State = (PauseDate == DateTime.MinValue) ? JobState.Active : JobState.
                        Paused;
                    ActiveJobState = GetActiveJobState();
                    LastStateChange = DateTime.UtcNow;
                }
                // Job is from a serialized object, so populate the missing info
                if (InstalledItem == null)
                    PopulateJobInfo(src, issuedFor, character);
                var state = GetState(src);
                if (state != State)
                {
                    State = state;
                    LastStateChange = DateTime.UtcNow;
                }
            }
            return matches;
        }

        /// <summary>
        /// Populates the serialization object job with the info from the API.
        /// </summary>
        /// <param name="src">The source.</param>
        /// <param name="issuedFor">Whether this jobs was issued for the corporation or
        /// character.</param>
        /// <param name="character">The character owning this job.</param>
        private void PopulateJobInfo(EsiJobListItem src, IssuedFor issuedFor,
            CCPCharacter character = null)
        {
            ID = src.JobID;
            InstallerID = src.InstallerID;
            InstalledItem = StaticBlueprints.GetBlueprintByID(src.BlueprintTypeID);
            Runs = src.Runs;
            Cost = src.Cost;
            Probability = src.Probability;
            SuccessfulRuns = src.SuccessfulRuns;
            StartDate = src.StartDate;
            EndDate = src.EndDate;
            PauseDate = src.PauseDate;
            IssuedFor = issuedFor;
            m_installedItemLocationID = src.FacilityID;

            UpdateLocation(character);
            UpdateInstallation(character);

            if (Enum.IsDefined(typeof(BlueprintActivity), src.ActivityID))
                Activity = (BlueprintActivity)Enum.ToObject(typeof(BlueprintActivity),
                    src.ActivityID);

            OutputItem = GetOutputItem(src.ProductTypeID);
        }

        #endregion


        #region Helper Methods

        /// <summary>
        /// Gets the output item by its ID (can be a blueprint or an item).
        /// </summary>
        /// <param name="id">The itemID of the blueprint.</param>
        /// <returns>The output item from the bluperint.</returns>
        private Item GetOutputItem(int id)
        {
            switch (Activity)
            {
                case BlueprintActivity.Manufacturing:
                    return StaticBlueprints.GetBlueprintByID(InstalledItem.ID).ProducesItem ??
                        StaticItems.GetItemByID(0);
                case BlueprintActivity.ResearchingMaterialEfficiency:
                case BlueprintActivity.ResearchingTimeEfficiency:
                case BlueprintActivity.Copying:
                    return InstalledItem;
                case BlueprintActivity.Invention:
                case BlueprintActivity.ReverseEngineering:
                    return StaticBlueprints.GetBlueprintByID(id) ?? StaticItems.GetItemByID(0);
                default:
                    return StaticItems.GetItemByID(0);
            }
        }

        /// <summary>
        /// Gets the station.
        /// </summary>
        /// <param name="id">The ID of the installation.</param>
        /// <returns>Name of the installation.</returns>
        private string GetInstallation(long id, CCPCharacter character)
        {
            return EveIDToStation.GetIDToStation(id, character)?.Name ?? EveMonConstants.
                UnknownText;
        }

        /// <summary>
        /// Gets the state of a job.
        /// </summary>
        /// <param name="src">The serializable source.</param>
        /// <returns>State of the seriallzable job.</returns>
        private static JobState GetState(EsiJobListItem src)
        {
            switch (src.Status)
            {
                // Active States
                case CCPJobCompletedStatus.Active:
                    return JobState.Active;
                // Cancelled States
                case CCPJobCompletedStatus.Cancelled:
                    return JobState.Canceled;
                // Failed States
                case CCPJobCompletedStatus.Reverted:
                    return JobState.Failed;
                // Delivered States
                case CCPJobCompletedStatus.Delivered:
                    return JobState.Delivered;
                // Paused States
                case CCPJobCompletedStatus.Paused:
                    return JobState.Paused;
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets the state of an active job.
        /// </summary>
        /// <returns>State of an active job.</returns>
        private ActiveJobState GetActiveJobState()
        {
            if (State != JobState.Active)
                return ActiveJobState.None;

            if (StartDate > DateTime.UtcNow)
                return ActiveJobState.Pending;

            return EndDate > DateTime.UtcNow ? ActiveJobState.InProgress : ActiveJobState.Ready;
        }

        #endregion


        #region Public Methods

        /// <summary>
        /// Updates the installation.
        /// </summary>
        public void UpdateInstallation(CCPCharacter character)
        {
            Installation = GetInstallation(m_installedItemLocationID, character);
        }

        /// <summary>
        /// Updates the location.
        /// </summary>
        /// <returns></returns>
        public void UpdateLocation(CCPCharacter character)
        {
            // If location not already determined
            if (m_installedItemLocationID != 0L && (SolarSystem == null || SolarSystem.ID == 0))
            {
                var station = EveIDToStation.GetIDToStation(m_installedItemLocationID,
                    character);
                SolarSystem = station?.SolarSystem ?? SolarSystem.UNKNOWN;
            }
        }

        #endregion
    }
}
