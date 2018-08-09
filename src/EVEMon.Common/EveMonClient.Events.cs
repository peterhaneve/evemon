using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using EVEMon.Common.CustomEventArgs;
using EVEMon.Common.Enumerations;
using EVEMon.Common.Extensions;
using EVEMon.Common.Models;
using EVEMon.Common.Notifications;
using EVEMon.Common.Serialization.PatchXml;

namespace EVEMon.Common
{
    public static partial class EveMonClient
    {
        #region Events firing

        /// <summary>
        /// Occurs every second.
        /// </summary>
        public static event EventHandler TimerTick;

        /// <summary>
        /// Occurs when the scheduler entries changed.
        /// </summary>
        public static event EventHandler SchedulerChanged;

        /// <summary>
        /// Occurs when the settings changed.
        /// </summary>
        public static event EventHandler SettingsChanged;

        /// <summary>
        /// Occurs when the collection of ESI Keys changed.
        /// </summary>
        public static event EventHandler ESIKeyCollectionChanged;

        /// <summary>
        /// Occurs when the ESI Keys monitored state changed.
        /// </summary>
        public static event EventHandler ESIKeyMonitoredChanged;

        /// <summary>
        /// Occurs when the collection of characters changed.
        /// </summary>
        public static event EventHandler CharacterCollectionChanged;

        /// <summary>
        /// Occurs when a character's label is changed.
        /// </summary>
        public static event EventHandler<LabelChangedEventArgs> CharacterLabelChanged;

        /// <summary>
        /// Occurs when the collection of monitored characters changed.
        /// </summary>
        public static event EventHandler MonitoredCharacterCollectionChanged;

        /// <summary>
        /// Occurs when the collection of a character implant set changed.
        /// </summary>
        public static event EventHandler<CharacterChangedEventArgs> CharacterImplantSetCollectionChanged;
        
        /// <summary>
        /// Occurs when an account status has been updated.
        /// </summary>
        public static event EventHandler AccountStatusUpdated;

        /// <summary>
        /// Occurs when the conquerable station list has been updated.
        /// </summary>
        public static event EventHandler ConquerableStationListUpdated;

        /// <summary>
        /// Occurs when the EVE factional warfare statistics has been updated.
        /// </summary>
        public static event EventHandler EveFactionalWarfareStatsUpdated;

        /// <summary>
        /// Occurs when the ESI key info have been updated.
        /// </summary>
        public static event EventHandler ESIKeyInfoUpdated;

        /// <summary>
        /// Occurs when the EveIDToName list has been updated.
        /// </summary>
        public static event EventHandler EveIDToNameUpdated;

        /// <summary>
        /// Occurs when the RefTypes list has been updated.
        /// </summary>
        public static event EventHandler RefTypesUpdated;

        /// <summary>
        /// Occurs when the NotificationRefTypes list has been updated.
        /// </summary>
        public static event EventHandler NotificationRefTypesUpdated;

        /// <summary>
        /// Occurs when the EveFlags list has been updated.
        /// </summary>
        public static event EventHandler EveFlagsUpdated;

        /// <summary>
        /// Occurs when the list of characters in an ESI key has been updated.
        /// </summary>
        public static event EventHandler<ESIKeyInfoChangedEventArgs> CharacterListUpdated;

        /// <summary>
        /// Occurs when one or many queued skills have been completed.
        /// </summary>
        public static event EventHandler<QueuedSkillsEventArgs> QueuedSkillsCompleted;

        /// <summary>
        /// Occurs when one of the character's collection of plans changed.
        /// </summary>
        public static event EventHandler<CharacterChangedEventArgs> CharacterPlanCollectionChanged;

        /// <summary>
        /// Occurs when a character's potrait has been updated.
        /// </summary>
        public static event EventHandler<CharacterChangedEventArgs> CharacterPortraitUpdated;

        /// <summary>
        /// Occurs when a character sheet has been updated.
        /// </summary>
        public static event EventHandler<CharacterChangedEventArgs> CharacterUpdated;

        /// <summary>
        /// Occurs when a character info has been updated.
        /// </summary>
        public static event EventHandler<CharacterChangedEventArgs> CharacterInfoUpdated;
        
        /// <summary>
        /// Occurs when a character skill queue has been updated.
        /// </summary>
        public static event EventHandler<CharacterChangedEventArgs> CharacterSkillQueueUpdated;

        /// <summary>
        /// Occurs when a character standings have been updated.
        /// </summary>
        public static event EventHandler<CharacterChangedEventArgs> CharacterStandingsUpdated;

        /// <summary>
        /// Occurs when a character factional warfare stats have been updated.
        /// </summary>
        public static event EventHandler<CharacterChangedEventArgs> CharacterFactionalWarfareStatsUpdated;

        /// <summary>
        /// Occurs when a character assets have been updated.
        /// </summary>
        public static event EventHandler<CharacterChangedEventArgs> CharacterAssetsUpdated;

        /// <summary>
        /// Occurs when both personal and corporation market orders of a character have been updated.
        /// </summary>
        public static event EventHandler<CharacterChangedEventArgs> MarketOrdersUpdated;

        /// <summary>
        /// Occurs when personal market orders of a character have been updated.
        /// </summary>
        public static event EventHandler<MarketOrdersEventArgs> CharacterMarketOrdersUpdated;

        /// <summary>
        /// Occurs when corporation market orders of a character have been updated.
        /// </summary>
        public static event EventHandler<MarketOrdersEventArgs> CorporationMarketOrdersUpdated;

        /// <summary>
        /// Occurs when both personal and corporation contracts of a character have been updated.
        /// </summary>
        public static event EventHandler<CharacterChangedEventArgs> ContractsUpdated;

        /// <summary>
        /// Occurs when personal contracts of a character have been updated.
        /// </summary>
        public static event EventHandler<ContractsEventArgs> CharacterContractsUpdated;

        /// <summary>
        /// Occurs when corporation contracts of a character have been updated.
        /// </summary>
        public static event EventHandler<ContractsEventArgs> CorporationContractsUpdated;

        /// <summary>
        /// Occurs when personal contract bids of a character have been updated.
        /// </summary>
        public static event EventHandler<CharacterChangedEventArgs> CharacterContractBidsDownloaded;

        /// <summary>
        /// Occurs when corporation contract bids of a character have been updated.
        /// </summary>
        public static event EventHandler<CharacterChangedEventArgs> CorporationContractBidsDownloaded;

        /// <summary>
        /// Occurs when items list of a character's contract have been downloaded.
        /// </summary>
        public static event EventHandler<CharacterChangedEventArgs> CharacterContractItemsDownloaded;

        /// <summary>
        /// Occurs when items list of a corporation's contract have been downloaded.
        /// </summary>
        public static event EventHandler<CharacterChangedEventArgs> CorporationContractItemsDownloaded;

        /// <summary>
        /// Occurs when a character wallet journal have been updated.
        /// </summary>
        public static event EventHandler<CharacterChangedEventArgs> CharacterWalletJournalUpdated;

        /// <summary>
        /// Occurs when a character walet transactions have been updated.
        /// </summary>
        public static event EventHandler<CharacterChangedEventArgs> CharacterWalletTransactionsUpdated;

        /// <summary>
        /// Occurs when industry jobs of a character have been updated.
        /// </summary>
        public static event EventHandler<CharacterChangedEventArgs> IndustryJobsUpdated;

        /// <summary>
        /// Occurs when industry jobs of a character have been updated.
        /// </summary>
        public static event EventHandler<CharacterChangedEventArgs> CharacterIndustryJobsUpdated;

        /// <summary>
        /// Occurs when industry jobs of a character have been updated.
        /// </summary>
        public static event EventHandler<CharacterChangedEventArgs> CorporationIndustryJobsUpdated;

        /// <summary>
        /// Occurs when the industry jobs of a character have been completed.
        /// </summary>
        public static event EventHandler<IndustryJobsEventArgs> CharacterIndustryJobsCompleted;

        /// <summary>
        /// Occurs when the industry jobs of a character have been completed.
        /// </summary>
        public static event EventHandler<IndustryJobsEventArgs> CorporationIndustryJobsCompleted;

        /// <summary>
        /// Occurs when the planetary pins of a character have been completed.
        /// </summary>
        public static event EventHandler<PlanetaryPinsEventArgs> CharacterPlaneteryPinsCompleted;

        /// <summary>
        /// Occurs when the research points of a character have been updated.
        /// </summary>
        public static event EventHandler<CharacterChangedEventArgs> CharacterResearchPointsUpdated;

        /// <summary>
        /// Occurs when the mail messages of a character have been updated.
        /// </summary>
        public static event EventHandler<CharacterChangedEventArgs> CharacterEVEMailMessagesUpdated;

        /// <summary>
        /// Occurs when the mailing list of a character have been updated.
        /// </summary>
        public static event EventHandler<CharacterChangedEventArgs> CharacterEVEMailingListsUpdated;

        /// <summary>
        /// Occurs when the body of a character EVE mail message has been downloaded.
        /// </summary>
        public static event EventHandler<CharacterChangedEventArgs> CharacterEVEMailBodyDownloaded;

        /// <summary>
        /// Occurs when the notifications of a character have been updated.
        /// </summary>
        public static event EventHandler<CharacterChangedEventArgs> CharacterEVENotificationsUpdated;
        
        /// <summary>
        /// Occurs when the text of a character contacts have been updated.
        /// </summary>
        public static event EventHandler<CharacterChangedEventArgs> CharacterContactsUpdated;

        /// <summary>
        /// Occurs when the text of a character medals have been updated.
        /// </summary>
        public static event EventHandler<CharacterChangedEventArgs> CharacterMedalsUpdated;

        /// <summary>
        /// Occurs when the text of a corporation medals have been updated.
        /// </summary>
        public static event EventHandler<CharacterChangedEventArgs> CorporationMedalsUpdated;

        /// <summary>
        /// Occurs when the text of a character upcoming calendar events have been updated.
        /// </summary>
        public static event EventHandler<CharacterChangedEventArgs> CharacterUpcomingCalendarEventsUpdated;

        /// <summary>
        /// Occurs when the text of a character calendar event attendees have been downloaded.
        /// </summary>
        public static event EventHandler<CharacterChangedEventArgs> CharacterCalendarEventAttendeesDownloaded;

        /// <summary>
        /// Occurs when the text of a character kill logs have been updated.
        /// </summary>
        public static event EventHandler<CharacterChangedEventArgs> CharacterKillLogUpdated;

        /// <summary>
        /// Occurs when the character planetary colony list has been updated.
        /// </summary>
        public static event EventHandler<CharacterChangedEventArgs> CharacterPlanetaryColoniesUpdated;

        /// <summary>
        /// Occurs when the character planetary colony layout has been updated.
        /// </summary>
        public static event EventHandler<CharacterChangedEventArgs> CharacterPlanetaryLayoutUpdated;
        
        /// <summary>
        /// Occurs when a plan's name changed.
        /// </summary>
        public static event EventHandler<PlanChangedEventArgs> PlanNameChanged;

        /// <summary>
        /// Occurs when a plan changed.
        /// </summary>
        public static event EventHandler<PlanChangedEventArgs> PlanChanged;

        /// <summary>
        /// Fired every time we ping the TQ server status (update pilots online count etc).
        /// </summary>
        public static event EventHandler<EveServerEventArgs> ServerStatusUpdated;

        /// <summary>
        /// Fired every time a notification (API errors, skill completed) is sent.
        /// </summary>
        public static event EventHandler<NotificationEventArgs> NotificationSent;

        /// <summary>
        /// Fired every time a notification (API errors, skill completed) is invalidated.
        /// </summary>
        public static event EventHandler<NotificationInvalidationEventArgs> NotificationInvalidated;

        /// <summary>
        /// Occurs when an application update is available.
        /// </summary>
        public static event EventHandler<UpdateAvailableEventArgs> UpdateAvailable;

        /// <summary>
        /// Occurs when a data files update is available.
        /// </summary>
        public static event EventHandler<DataUpdateAvailableEventArgs> DataUpdateAvailable;

        /// <summary>
        /// Occurs when the loadout feed updated.
        /// </summary>
        public static event EventHandler<LoadoutFeedEventArgs> LoadoutFeedUpdated;

        /// <summary>
        /// Occurs when the loadout updated.
        /// </summary>
        public static event EventHandler<LoadoutEventArgs> LoadoutUpdated;

        /// <summary>
        /// Occurs when item prices updated.
        /// </summary>
        public static event EventHandler ItemPricesUpdated;

        /// <summary>
        /// Fires the timer tick event to notify the subscribers.
        /// </summary>
        internal static void UpdateOnOneSecondTick()
        {
            if (Closed)
                return;

            // Fires the event for subscribers
            TimerTick?.ThreadSafeInvoke(null, EventArgs.Empty);
        }

        /// <summary>
        /// Called when settings changed.
        /// </summary>
        internal static void OnSettingsChanged()
        {
            if (Closed)
                return;

            Trace();
            Settings.Save();
            UpdateSettings();
            SettingsChanged?.ThreadSafeInvoke(null, EventArgs.Empty);
        }

        /// <summary>
        /// Called when the scheduler changed.
        /// </summary>
        internal static void OnSchedulerChanged()
        {
            if (Closed)
                return;

            Trace();
            Settings.Save();
            SchedulerChanged?.ThreadSafeInvoke(null, EventArgs.Empty);
        }

        /// <summary>
        /// Called when the ESI key collection changed.
        /// </summary>
        internal static void OnESIKeyCollectionChanged()
        {
            if (Closed)
                return;

            Trace();
            EveMonClient.Characters.UpdateAccountStatuses();
            Settings.Save();
            ESIKeyCollectionChanged?.ThreadSafeInvoke(null, EventArgs.Empty);
        }

        /// <summary>
        /// Called when the monitored state of an ESI key changed.
        /// </summary>
        internal static void OnESIKeyMonitoredChanged()
        {
            if (Closed)
                return;

            Trace();
            Settings.Save();
            ESIKeyMonitoredChanged?.ThreadSafeInvoke(null, EventArgs.Empty);
        }

        /// <summary>
        /// Called when the monitored characters changed.
        /// </summary>
        internal static void OnMonitoredCharactersChanged()
        {
            if (Closed)
                return;

            Trace();
            Settings.Save();
            MonitoredCharacterCollectionChanged?.ThreadSafeInvoke(null, EventArgs.Empty);
        }

        /// <summary>
        /// Called when the character collection changed.
        /// </summary>
        internal static void OnCharacterCollectionChanged()
        {
            if (Closed)
                return;

            Trace();
            Settings.Save();
            CharacterCollectionChanged?.ThreadSafeInvoke(null, EventArgs.Empty);
        }


        /// <summary>
        /// Called when the conquerable station list has been updated.
        /// </summary>
        internal static void OnConquerableStationListUpdated()
        {
            if (Closed)
                return;

            Trace();
            ConquerableStationListUpdated?.ThreadSafeInvoke(null, EventArgs.Empty);
        }

        /// <summary>
        /// Called when the EVE factional warfare statistics have been updated.
        /// </summary>
        internal static void OnEveFactionalWarfareStatsUpdated()
        {
            if (Closed)
                return;

            Trace();
            EveFactionalWarfareStatsUpdated?.ThreadSafeInvoke(null, EventArgs.Empty);
        }

        /// <summary>
        /// Called when the EveIDToName list has been updated.
        /// </summary>
        internal static void OnEveIDToNameUpdated()
        {
            if (Closed)
                return;

            Trace();
            EveIDToNameUpdated?.ThreadSafeInvoke(null, EventArgs.Empty);
        }

        /// <summary>
        /// Called when the RefTypes list has been updated.
        /// </summary>
        internal static void OnRefTypesUpdated()
        {
            if (Closed)
                return;

            Trace();
            RefTypesUpdated?.ThreadSafeInvoke(null, EventArgs.Empty);
        }

        /// <summary>
        /// Called when the NotificationRefTypes list has been updated.
        /// </summary>
        internal static void OnNotificationRefTypesUpdated()
        {
            if (Closed)
                return;

            Trace();
            NotificationRefTypesUpdated?.ThreadSafeInvoke(null, EventArgs.Empty);
        }

        /// <summary>
        /// Called when the EveFlags list has been updated.
        /// </summary>
        internal static void OnEveFlagsUpdated()
        {
            if (Closed)
                return;

            Trace();
            EveFlagsUpdated?.ThreadSafeInvoke(null, EventArgs.Empty);
        }

        /// <summary>
        /// Called when the ESI key info is updated.
        /// </summary>
        /// <param name="esiKey">The ESI key.</param>
        internal static void OnESIKeyInfoUpdated(ESIKey esiKey)
        {
            if (Closed)
                return;

            Trace(esiKey.ToString());
            Settings.Save();
            ESIKeyInfoUpdated?.ThreadSafeInvoke(null, EventArgs.Empty);
        }

        /// <summary>
        /// Called when an account status has been updated.
        /// </summary>
        /// <param name="esiKey">The ESI key.</param>
        internal static void OnAccountStatusUpdated(ESIKey esiKey)
        {
            if (Closed)
                return;

            Trace(esiKey.ToString());
            Characters.UpdateAccountStatuses();
            Settings.Save();
            AccountStatusUpdated?.ThreadSafeInvoke(null, EventArgs.Empty);
        }

        /// <summary>
        /// Called when the character list updated.
        /// </summary>
        /// <param name="esiKey">The ESI key.</param>
        internal static void OnCharacterListUpdated(ESIKey esiKey)
        {
            if (Closed)
                return;

            Trace(esiKey.ToString());
            Settings.Save();
            CharacterListUpdated?.ThreadSafeInvoke(null, new ESIKeyInfoChangedEventArgs(esiKey));
        }
        
        /// <summary>
        /// Called when the character implant set collection changed.
        /// </summary>
        internal static void OnCharacterImplantSetCollectionChanged(Character character)
        {
            if (Closed)
                return;

            Trace(character.Name);
            Settings.Save();
            CharacterImplantSetCollectionChanged?.ThreadSafeInvoke(null, new CharacterChangedEventArgs(character));
        }

        /// <summary>
        /// Called when the character sheet updated.
        /// </summary>
        /// <param name="character">The character.</param>
        internal static void OnCharacterUpdated(Character character)
        {
            if (Closed)
                return;

            Trace(character.Name);
            Settings.Save();
            CharacterUpdated?.ThreadSafeInvoke(null, new CharacterChangedEventArgs(character));
        }

        /// <summary>
        /// Called when the character info updated.
        /// </summary>
        /// <param name="character">The character.</param>
        internal static void OnCharacterInfoUpdated(Character character)
        {
            if (Closed)
                return;

            Trace(character.Name);
            Settings.Save();
            CharacterInfoUpdated?.ThreadSafeInvoke(null, new CharacterChangedEventArgs(character));
        }

        /// <summary>
        /// Called when the character label is changed via the UI.
        /// </summary>
        /// <param name="character">The character.</param>
        public static void OnCharacterLabelChanged(Character character)
        {
            if (Closed)
                return;

            Trace(character.Name);
            CharacterLabelChanged?.ThreadSafeInvoke(null, new LabelChangedEventArgs(character,
                Characters.GetKnownLabels()));
        }

        /// <summary>
        /// Called when the character skill queue updated.
        /// </summary>
        /// <param name="character">The character.</param>
        internal static void OnCharacterSkillQueueUpdated(Character character)
        {
            if (Closed)
                return;

            Trace(character.Name);
            character.UpdateAccountStatus();
            Settings.Save();
            CharacterSkillQueueUpdated?.ThreadSafeInvoke(null, new CharacterChangedEventArgs(character));
        }

        /// <summary>
        /// Called when the character queued skills completed.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <param name="skillsCompleted">The skills completed.</param>
        internal static void OnCharacterQueuedSkillsCompleted(Character character, IEnumerable<QueuedSkill> skillsCompleted)
        {
            if (Closed)
                return;

            Trace(character.Name);
            QueuedSkillsCompleted?.ThreadSafeInvoke(null, new QueuedSkillsEventArgs(character, skillsCompleted));
        }

        /// <summary>
        /// Called when the character standings updated.
        /// </summary>
        /// <param name="character">The character.</param>
        internal static void OnCharacterStandingsUpdated(Character character)
        {
            if (Closed)
                return;

            Trace(character.Name);
            CharacterStandingsUpdated?.ThreadSafeInvoke(null, new CharacterChangedEventArgs(character));
        }

        /// <summary>
        /// Called when the character factinal warfare stats updated.
        /// </summary>
        /// <param name="character">The character.</param>
        internal static void OnCharacterFactionalWarfareStatsUpdated(Character character)
        {
            if (Closed)
                return;

            Trace(character.Name);
            CharacterFactionalWarfareStatsUpdated?.ThreadSafeInvoke(null, new CharacterChangedEventArgs(character));
        }

        /// <summary>
        /// Called when the character assets updated.
        /// </summary>
        /// <param name="character">The character.</param>
        internal static void OnCharacterAssetsUpdated(Character character)
        {
            if (Closed)
                return;

            Trace(character.Name);
            CharacterAssetsUpdated?.ThreadSafeInvoke(null, new CharacterChangedEventArgs(character));
        }

        /// <summary>
        /// Called when both character and corporation issued market orders of a character updated.
        /// </summary>
        /// <param name="character">The character.</param>
        internal static void OnMarketOrdersUpdated(Character character)
        {
            if (Closed)
                return;

            Trace(character.Name);
            Settings.Save();
            MarketOrdersUpdated?.ThreadSafeInvoke(null, new CharacterChangedEventArgs(character));
        }

        /// <summary>
        /// Called when the personal market orders of a character updated.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <param name="endedOrders">The ended orders.</param>
        internal static void OnCharacterMarketOrdersUpdated(Character character, IEnumerable<MarketOrder> endedOrders)
        {
            if (Closed)
                return;

            Trace(character.Name);
            CharacterMarketOrdersUpdated?.ThreadSafeInvoke(null, new MarketOrdersEventArgs(character, endedOrders));
        }

        /// <summary>
        /// Called when both character and corporation issued contracts of a character updated.
        /// </summary>
        /// <param name="character">The character.</param>
        internal static void OnContractsUpdated(Character character)
        {
            if (Closed)
                return;

            Trace(character.Name);
            Settings.Save();
            ContractsUpdated?.ThreadSafeInvoke(null, new CharacterChangedEventArgs(character));
        }

        /// <summary>
        /// Called when the personal contracts of a character updated.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <param name="endedContracts">The ended contracts.</param>
        internal static void OnCharacterContractsUpdated(Character character, IEnumerable<Contract> endedContracts)
        {
            if (Closed)
                return;

            Trace(character.Name);
            CharacterContractsUpdated?.ThreadSafeInvoke(null, new ContractsEventArgs(character, endedContracts));
        }

        /// <summary>
        /// Called when the bid list of a personal contract has been downloaded.
        /// </summary>
        /// <param name="character">The character.</param>
        internal static void OnCharacterContractBidsDownloaded(Character character)
        {
            if (Closed)
                return;

            Trace(character.Name);
            CharacterContractBidsDownloaded?.ThreadSafeInvoke(null, new CharacterChangedEventArgs(character));
        }

        /// <summary>
        /// Called when the item list of a personal contract has been downloaded.
        /// </summary>
        /// <param name="character">The character.</param>
        internal static void OnCharacterContractItemsDownloaded(Character character)
        {
            if (Closed)
                return;

            Trace(character.Name);
            CharacterContractItemsDownloaded?.ThreadSafeInvoke(null, new CharacterChangedEventArgs(character));
        }

        /// <summary>
        /// Called when the character wallet journal updated.
        /// </summary>
        /// <param name="character">The character.</param>
        internal static void OnCharacterWalletJournalUpdated(Character character)
        {
            if (Closed)
                return;

            Trace(character.Name);
            CharacterWalletJournalUpdated?.ThreadSafeInvoke(null, new CharacterChangedEventArgs(character));
        }

        /// <summary>
        /// Called when the character wallet transcations updated.
        /// </summary>
        /// <param name="character">The character.</param>
        internal static void OnCharacterWalletTransactionsUpdated(Character character)
        {
            if (Closed)
                return;

            Trace(character.Name);
            CharacterWalletTransactionsUpdated?.ThreadSafeInvoke(null, new CharacterChangedEventArgs(character));
        }

        /// <summary>
        /// Called when both character and corporation issued industry jobs for a character updated.
        /// </summary>
        /// <param name="character">The character.</param>
        internal static void OnIndustryJobsUpdated(Character character)
        {
            if (Closed)
                return;

            Trace(character.Name);
            Settings.Save();
            IndustryJobsUpdated?.ThreadSafeInvoke(null, new CharacterChangedEventArgs(character));
        }

        /// <summary>
        /// Called when the character industry jobs for a character updated.
        /// </summary>
        /// <param name="character">The character.</param>
        internal static void OnCharacterIndustryJobsUpdated(Character character)
        {
            if (Closed)
                return;

            Trace(character.Name);
            CharacterIndustryJobsUpdated?.ThreadSafeInvoke(null, new CharacterChangedEventArgs(character));
        }

        /// <summary>
        /// Called when the corporation issued industry jobs for a character updated.
        /// </summary>
        /// <param name="character">The character.</param>
        internal static void OnCorporationIndustryJobsUpdated(Character character)
        {
            if (Closed)
                return;

            Trace(character.Name);
            CorporationIndustryJobsUpdated?.ThreadSafeInvoke(null, new CharacterChangedEventArgs(character));
        }

        /// <summary>
        /// Called when the character's industry jobs completed.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <param name="jobsCompleted">The jobs completed.</param>
        internal static void OnCharacterIndustryJobsCompleted(Character character, IEnumerable<IndustryJob> jobsCompleted)
        {
            if (Closed)
                return;

            Trace(character.Name);
            CharacterIndustryJobsCompleted?.ThreadSafeInvoke(null, new IndustryJobsEventArgs(character, jobsCompleted));
        }

        /// <summary>
        /// Called when the character's planetary pins completed.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <param name="pinsCompleted">The pins completed.</param>
        internal static void OnCharacterPlanetaryPinsCompleted(Character character, IEnumerable<PlanetaryPin> pinsCompleted)
        {
            if (Closed)
                return;

            Trace(character.Name);
            CharacterPlaneteryPinsCompleted?.ThreadSafeInvoke(null, new PlanetaryPinsEventArgs(character, pinsCompleted));
        }

        /// <summary>
        /// Called when the character research points updated.
        /// </summary>
        /// <param name="character">The character.</param>
        internal static void OnCharacterResearchPointsUpdated(Character character)
        {
            if (Closed)
                return;

            Trace(character.Name);
            CharacterResearchPointsUpdated?.ThreadSafeInvoke(null, new CharacterChangedEventArgs(character));
        }

        /// <summary>
        /// Called when the character EVE mail messages updated.
        /// </summary>
        /// <param name="character">The character.</param>
        internal static void OnCharacterEVEMailMessagesUpdated(Character character)
        {
            if (Closed)
                return;

            Trace(character.Name);
            Settings.Save();
            CharacterEVEMailMessagesUpdated?.ThreadSafeInvoke(null, new CharacterChangedEventArgs(character));
        }

        /// <summary>
        /// Called when the character EVE mailing list updated.
        /// </summary>
        /// <param name="character">The character.</param>
        internal static void OnCharacterEVEMailingListsUpdated(Character character)
        {
            if (Closed)
                return;

            Trace(character.Name);
            CharacterEVEMailingListsUpdated?.ThreadSafeInvoke(null, new CharacterChangedEventArgs(character));
        }

        /// <summary>
        /// Called when the character EVE mail message body downloaded.
        /// </summary>
        /// <param name="character">The character.</param>
        internal static void OnCharacterEVEMailBodyDownloaded(Character character)
        {
            if (Closed)
                return;

            Trace(character.Name);
            CharacterEVEMailBodyDownloaded?.ThreadSafeInvoke(null, new CharacterChangedEventArgs(character));
        }

        /// <summary>
        /// Called when the character EVE notifications updated.
        /// </summary>
        /// <param name="character">The character.</param>
        internal static void OnCharacterEVENotificationsUpdated(Character character)
        {
            if (Closed)
                return;

            Trace(character.Name);
            Settings.Save();
            CharacterEVENotificationsUpdated?.ThreadSafeInvoke(null, new CharacterChangedEventArgs(character));
        }
        
        /// <summary>
        /// Called when the character contacts updated.
        /// </summary>
        /// <param name="character">The character.</param>
        internal static void OnCharacterContactsUpdated(Character character)
        {
            if (Closed)
                return;

            Trace(character.Name);
            CharacterContactsUpdated?.ThreadSafeInvoke(null, new CharacterChangedEventArgs(character));
        }

        /// <summary>
        /// Called when the character medals updated.
        /// </summary>
        /// <param name="character">The character.</param>
        internal static void OnCharacterMedalsUpdated(Character character)
        {
            if (Closed)
                return;

            Trace(character.Name);
            CharacterMedalsUpdated?.ThreadSafeInvoke(null, new CharacterChangedEventArgs(character));
        }

        /// <summary>
        /// Called when the corporation medals updated.
        /// </summary>
        /// <param name="character">The character.</param>
        internal static void OnCorporationMedalsUpdated(Character character)
        {
            if (Closed)
                return;

            Trace(character.Name);
            CorporationMedalsUpdated?.ThreadSafeInvoke(null, new CharacterChangedEventArgs(character));
        }

        /// <summary>
        /// Called when the character upcoming calendar events updated.
        /// </summary>
        /// <param name="character">The character.</param>
        internal static void OnCharacterUpcomingCalendarEventsUpdated(Character character)
        {
            if (Closed)
                return;

            Trace(character.Name);
            CharacterUpcomingCalendarEventsUpdated?.ThreadSafeInvoke(null, new CharacterChangedEventArgs(character));
        }

        /// <summary>
        /// Called when the character calendar event attendees downloaded.
        /// </summary>
        /// <param name="character">The character.</param>
        internal static void OnCharacterCalendarEventAttendeesDownloaded(Character character)
        {
            if (Closed)
                return;

            Trace(character.Name);
            CharacterCalendarEventAttendeesDownloaded?.ThreadSafeInvoke(null, new CharacterChangedEventArgs(character));
        }

        /// <summary>
        /// Called when the character kill log updated.
        /// </summary>
        /// <param name="character">The character.</param>
        internal static void OnCharacterKillLogUpdated(Character character)
        {
            if (Closed)
                return;

            Trace(character.Name);
            CharacterKillLogUpdated?.ThreadSafeInvoke(null, new CharacterChangedEventArgs(character));
        }

        /// <summary>
        /// Called when the character planetary colonies updated.
        /// </summary>
        /// <param name="character">The character.</param>
        internal static void OnCharacterPlanetaryColoniesUpdated(Character character)
        {
            if (Closed)
                return;

            Trace(character.Name);
            CharacterPlanetaryColoniesUpdated?.ThreadSafeInvoke(null, new CharacterChangedEventArgs(character));
        }

        /// <summary>
        /// Called when the character planetary pins updated.
        /// </summary>
        /// <param name="character">The character.</param>
        internal static void OnCharacterPlanetaryLayoutUpdated(Character character)
        {
            if (Closed)
                return;

            Trace(character.Name);
            CharacterPlanetaryLayoutUpdated?.ThreadSafeInvoke(null, new CharacterChangedEventArgs(character));
        }
        
        /// <summary>
        /// Called when the character portrait updated.
        /// </summary>
        /// <param name="character">The character.</param>
        internal static void OnCharacterPortraitUpdated(Character character)
        {
            if (Closed)
                return;

            Trace(character.Name);
            CharacterPortraitUpdated?.ThreadSafeInvoke(null, new CharacterChangedEventArgs(character));
        }

        /// <summary>
        /// Called when the character plan collection changed.
        /// </summary>
        /// <param name="character">The character.</param>
        internal static void OnCharacterPlanCollectionChanged(Character character)
        {
            if (Closed)
                return;

            Trace(character.Name);
            Settings.Save();
            CharacterPlanCollectionChanged?.ThreadSafeInvoke(null, new CharacterChangedEventArgs(character));
        }

        /// <summary>
        /// Called when the corporation market orders of a character updated.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <param name="endedOrders">The ended orders.</param>
        internal static void OnCorporationMarketOrdersUpdated(Character character, IEnumerable<MarketOrder> endedOrders)
        {
            if (Closed)
                return;

            Trace(character.CorporationName);
            CorporationMarketOrdersUpdated?.ThreadSafeInvoke(null, new MarketOrdersEventArgs(character, endedOrders));
        }

        /// <summary>
        /// Called when the corporation contracts of a character updated.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <param name="endedContracts">The ended contracts.</param>
        internal static void OnCorporationContractsUpdated(Character character, IEnumerable<Contract> endedContracts)
        {
            if (Closed)
                return;

            Trace(character.CorporationName);
            CorporationContractsUpdated?.ThreadSafeInvoke(null, new ContractsEventArgs(character, endedContracts));
        }

        /// <summary>
        /// Called when the bid list of a corporation contract has been downloaded.
        /// </summary>
        /// <param name="character">The character.</param>
        internal static void OnCorporationContractBidsDownloaded(Character character)
        {
            if (Closed)
                return;

            Trace(character.CorporationName);
            CorporationContractBidsDownloaded?.ThreadSafeInvoke(null, new CharacterChangedEventArgs(character));
        }

        /// <summary>
        /// Called when the item list of a corporation contract has been downloaded.
        /// </summary>
        /// <param name="character">The character.</param>
        internal static void OnCorporationContractItemsDownloaded(Character character)
        {
            if (Closed)
                return;

            Trace(character.CorporationName);
            CorporationContractItemsDownloaded?.ThreadSafeInvoke(null, new CharacterChangedEventArgs(character));
        }

        /// <summary>
        /// Called when the character's corporation industry jobs completed.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <param name="jobsCompleted">The jobs completed.</param>
        internal static void OnCorporationIndustryJobsCompleted(Character character, IEnumerable<IndustryJob> jobsCompleted)
        {
            if (Closed)
                return;

            Trace(character.CorporationName);
            CorporationIndustryJobsCompleted?.ThreadSafeInvoke(null, new IndustryJobsEventArgs(character, jobsCompleted));
        }

        /// <summary>
        /// Called when a plan changed.
        /// </summary>
        /// <param name="plan">The plan.</param>
        internal static void OnPlanChanged(Plan plan)
        {
            if (Closed)
                return;

            Trace(plan.Name);
            Settings.Save();
            PlanChanged?.ThreadSafeInvoke(null, new PlanChangedEventArgs(plan));
        }

        /// <summary>
        /// Called when a plan name changed.
        /// </summary>
        /// <param name="plan">The plan.</param>
        internal static void OnPlanNameChanged(Plan plan)
        {
            if (Closed)
                return;

            Trace(plan.Name);
            Settings.Save();
            PlanNameChanged?.ThreadSafeInvoke(null, new PlanChangedEventArgs(plan));
        }

        /// <summary>
        /// Called when the server status updated.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="previousStatus">The previous status.</param>
        /// <param name="status">The status.</param>
        internal static void OnServerStatusUpdated(EveServer server, ServerStatus previousStatus, ServerStatus status)
        {
            if (Closed)
                return;

            Trace();
            ServerStatusUpdated?.ThreadSafeInvoke(null, new EveServerEventArgs(server, previousStatus, status));
        }

        /// <summary>
        /// Called when a notification is sent.
        /// </summary>
        /// <param name="notification">The notification.</param>
        internal static void OnNotificationSent(NotificationEventArgs notification)
        {
            if (Closed)
                return;

            Trace(notification.ToString());
            NotificationSent?.ThreadSafeInvoke(null, notification);
        }

        /// <summary>
        /// Called when a notification gets invalidated.
        /// </summary>
        /// <param name="args">The <see cref="EVEMon.Common.Notifications.NotificationInvalidationEventArgs"/> instance containing the event data.</param>
        internal static void OnNotificationInvalidated(NotificationInvalidationEventArgs args)
        {
            if (Closed)
                return;

            Trace();
            NotificationInvalidated?.ThreadSafeInvoke(null, args);
        }

        /// <summary>
        /// Called when an update is available.
        /// </summary>
        /// <param name="forumUrl">The forum URL.</param>
        /// <param name="installerUrl">The installer URL.</param>
        /// <param name="updateMessage">The update message.</param>
        /// <param name="currentVersion">The current version.</param>
        /// <param name="newestVersion">The newest version.</param>
        /// <param name="md5Sum">The MD5 sum.</param>
        /// <param name="canAutoInstall">if set to <c>true</c> [can auto install].</param>
        /// <param name="installArgs">The install args.</param>
        internal static void OnUpdateAvailable(Uri forumUrl, Uri installerUrl, string updateMessage,
            Version currentVersion, Version newestVersion, string md5Sum,
            bool canAutoInstall, string installArgs)
        {
            Trace($"({currentVersion} -> {newestVersion}, {canAutoInstall}, {installArgs})");
            UpdateAvailable?.ThreadSafeInvoke(null, new UpdateAvailableEventArgs(forumUrl, installerUrl, updateMessage, currentVersion,
                newestVersion, md5Sum, canAutoInstall, installArgs));
        }

        /// <summary>
        /// Called when data update is available.
        /// </summary>
        /// <param name="changedFiles">The changed files.</param>
        internal static void OnDataUpdateAvailable(Collection<SerializableDatafile> changedFiles)
        {
            Trace($"(ChangedFiles = {changedFiles.Count})");
            DataUpdateAvailable?.ThreadSafeInvoke(null, new DataUpdateAvailableEventArgs(changedFiles));
        }

        /// <summary>
        /// Called when we downloaded a loadouts feed from the provider.
        /// </summary>
        /// <param name="loadoutFeed">The loadout feed.</param>
        /// <param name="errorMessage">The error message.</param>
        internal static void OnLoadoutsFeedDownloaded(object loadoutFeed, string errorMessage)
        {
            LoadoutFeedUpdated?.ThreadSafeInvoke(null, new LoadoutFeedEventArgs(loadoutFeed, errorMessage));
        }

        /// <summary>
        /// Called when we downloaded a loadout from the provider.
        /// </summary>
        /// <param name="loadout">The loadout.</param>
        /// <param name="errorMessage">The error message.</param>
        internal static void OnLoadoutDownloaded(object loadout, string errorMessage)
        {
            LoadoutUpdated?.ThreadSafeInvoke(null, new LoadoutEventArgs(loadout, errorMessage));
        }

        /// <summary>
        /// Called when prices downloaded.
        /// </summary>
        /// <param name="pricesFeed">The prices feed.</param>
        /// <param name="errormessage">The errormessage.</param>
        internal static void OnPricesDownloaded(object pricesFeed, string errormessage)
        {
            ItemPricesUpdated?.ThreadSafeInvoke(null, EventArgs.Empty);
        }

        #endregion

    }
}
