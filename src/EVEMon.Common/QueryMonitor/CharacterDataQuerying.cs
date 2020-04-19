using System;
using System.Linq;
using System.Collections.Generic;
using EVEMon.Common.Enumerations;
using EVEMon.Common.Enumerations.CCPAPI;
using EVEMon.Common.Extensions;
using EVEMon.Common.Helpers;
using EVEMon.Common.Interfaces;
using EVEMon.Common.Models;
using EVEMon.Common.Service;
using EVEMon.Common.Serialization.Esi;
using EVEMon.Common.Serialization.Eve;
using EVEMon.Common.Net;

namespace EVEMon.Common.QueryMonitor
{
    internal sealed class CharacterDataQuerying
    {
        #region Fields

        private readonly CharacterQueryMonitor<EsiAPISkillQueue> m_charSkillQueueMonitor;
        private readonly CharacterQueryMonitor<EsiAPISkills> m_charSkillsMonitor;
        private readonly CharacterQueryMonitor<EsiAPIMarketOrders> m_charMarketOrdersMonitor;
        private readonly QueryMonitor<EsiAPIContracts> m_charContractsMonitor;
        private readonly CharacterQueryMonitor<EsiAPIIndustryJobs> m_charIndustryJobsMonitor;
        private readonly List<IQueryMonitorEx> m_characterQueryMonitors;
        private readonly List<IQueryMonitorEx> m_basicFeaturesMonitors;
        private readonly CCPCharacter m_ccpCharacter;
        private bool m_characterSheetUpdating = false;

        // Responses from the attribute results since we handle it manually
        private ResponseParams m_attrResponse;
        // Result from the character skill queue to handle a pathological case where skill
        // queues were not-modified but need to be re-imported due to a skills list change
        private EsiAPISkillQueue m_lastQueue;
        // Responses from the market order history results since we handle it manually
        private ResponseParams m_orderHistoryResponse;

        #endregion


        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="CharacterDataQuerying"/> class.
        /// </summary>
        /// <param name="ccpCharacter">The CCP character.</param>
        internal CharacterDataQuerying(CCPCharacter ccpCharacter)
        {
            var notifiers = EveMonClient.Notifications;
            m_ccpCharacter = ccpCharacter;
            m_characterQueryMonitors = new List<IQueryMonitorEx>();
            m_attrResponse = null;
            m_orderHistoryResponse = null;
            m_lastQueue = null;

            // Add the monitors in an order as they will appear in the throbber menu
            CharacterSheetMonitor = new CharacterQueryMonitor<EsiAPICharacterSheet>(
                ccpCharacter, ESIAPICharacterMethods.CharacterSheet, OnCharacterSheetUpdated,
                notifiers.NotifyCharacterSheetError);
            m_characterQueryMonitors.Add(CharacterSheetMonitor);
            // Location
            m_characterQueryMonitors.Add(new CharacterQueryMonitor<EsiAPILocation>(
                ccpCharacter, ESIAPICharacterMethods.Location, OnCharacterLocationUpdated,
                notifiers.NotifyCharacterLocationError));
            // Clones
            m_characterQueryMonitors.Add(new CharacterQueryMonitor<EsiAPIClones>(
                ccpCharacter, ESIAPICharacterMethods.Clones, OnCharacterClonesUpdated,
                notifiers.NotifyCharacterClonesError));
            // Implants
            m_characterQueryMonitors.Add(new CharacterQueryMonitor<List<int>>(
                ccpCharacter, ESIAPICharacterMethods.Implants, OnCharacterImplantsUpdated,
                OnCharacterImplantsFailed, true));
            // Ship
            m_characterQueryMonitors.Add(new CharacterQueryMonitor<EsiAPIShip>(
                ccpCharacter, ESIAPICharacterMethods.Ship, OnCharacterShipUpdated,
                notifiers.NotifyCharacterShipError));
            // Skills
            m_charSkillsMonitor = new CharacterQueryMonitor<EsiAPISkills>(
                ccpCharacter, ESIAPICharacterMethods.Skills, OnCharacterSkillsUpdated,
                notifiers.NotifyCharacterSkillsError);
            m_characterQueryMonitors.Add(m_charSkillsMonitor);
            // Skill queue
            m_charSkillQueueMonitor = new CharacterQueryMonitor<EsiAPISkillQueue>(
                ccpCharacter, ESIAPICharacterMethods.SkillQueue, OnSkillQueueUpdated,
                notifiers.NotifySkillQueueError);
            m_characterQueryMonitors.Add(m_charSkillQueueMonitor);
            // Employment history
            m_characterQueryMonitors.Add(new CharacterQueryMonitor<EsiAPIEmploymentHistory>(
                ccpCharacter, ESIAPICharacterMethods.EmploymentHistory,
                OnCharacterEmploymentUpdated, notifiers.NotifyCharacterEmploymentError));
            // Standings
            m_characterQueryMonitors.Add(new PagedQueryMonitor<EsiAPIStandings,
                EsiStandingsListItem>(new CharacterQueryMonitor<EsiAPIStandings>(
                ccpCharacter, ESIAPICharacterMethods.Standings, OnStandingsUpdated,
                notifiers.NotifyCharacterStandingsError) { QueryOnStartup = true }));
            // Contacts
            m_characterQueryMonitors.Add(new PagedQueryMonitor<EsiAPIContactsList,
                EsiContactListItem>(new CharacterQueryMonitor<EsiAPIContactsList>(ccpCharacter,
                ESIAPICharacterMethods.ContactList, OnContactsUpdated,
                notifiers.NotifyCharacterContactsError) { QueryOnStartup = true }));
            // Factional warfare
            m_characterQueryMonitors.Add(new CharacterQueryMonitor<EsiAPIFactionalWarfareStats>(
                ccpCharacter, ESIAPICharacterMethods.FactionalWarfareStats,
                OnFactionalWarfareStatsUpdated, notifiers.
                NotifyCharacterFactionalWarfareStatsError) { QueryOnStartup = true });
            // Medals
            m_characterQueryMonitors.Add(new PagedQueryMonitor<EsiAPIMedals,
                EsiMedalsListItem>(new CharacterQueryMonitor<EsiAPIMedals>(ccpCharacter,
                ESIAPICharacterMethods.Medals, OnMedalsUpdated,
                notifiers.NotifyCharacterMedalsError) { QueryOnStartup = true }));
            // Kill log
            m_characterQueryMonitors.Add(new PagedQueryMonitor<EsiAPIKillLog,
                EsiKillLogListItem>(new CharacterQueryMonitor<EsiAPIKillLog>(ccpCharacter,
                ESIAPICharacterMethods.KillLog, OnKillLogUpdated,
                notifiers.NotifyCharacterKillLogError) { QueryOnStartup = true }));
            // Assets
            m_characterQueryMonitors.Add(new PagedQueryMonitor<EsiAPIAssetList,
                EsiAssetListItem>(new CharacterQueryMonitor<EsiAPIAssetList>(ccpCharacter,
                ESIAPICharacterMethods.AssetList, OnAssetsUpdated,
                notifiers.NotifyCharacterAssetsError) { QueryOnStartup = true }));
            // Market orders
            m_charMarketOrdersMonitor = new CharacterQueryMonitor<EsiAPIMarketOrders>(
                ccpCharacter, ESIAPICharacterMethods.MarketOrders, OnMarketOrdersUpdated,
                notifiers.NotifyCharacterMarketOrdersError) { QueryOnStartup = true };
            m_characterQueryMonitors.Add(m_charMarketOrdersMonitor);
            // Contracts
            m_charContractsMonitor = new PagedQueryMonitor<EsiAPIContracts,
                EsiContractListItem>(new CharacterQueryMonitor<EsiAPIContracts>(ccpCharacter,
                ESIAPICharacterMethods.Contracts, OnContractsUpdated,
                notifiers.NotifyCharacterContractsError) { QueryOnStartup = true });
            m_characterQueryMonitors.Add(m_charContractsMonitor);
            // Wallet journal
            m_characterQueryMonitors.Add(new PagedQueryMonitor<EsiAPIWalletJournal,
                EsiWalletJournalListItem>(new CharacterQueryMonitor<EsiAPIWalletJournal>(
                ccpCharacter, ESIAPICharacterMethods.WalletJournal, OnWalletJournalUpdated,
                notifiers.NotifyCharacterWalletJournalError) { QueryOnStartup = true }));
            // Wallet balance
            m_characterQueryMonitors.Add(new CharacterQueryMonitor<string>(
                ccpCharacter, ESIAPICharacterMethods.AccountBalance, OnWalletBalanceUpdated,
                notifiers.NotifyCharacterBalanceError));
            // Wallet transactions
            m_characterQueryMonitors.Add(new PagedQueryMonitor<EsiAPIWalletTransactions,
                EsiWalletTransactionsListItem>(new CharacterQueryMonitor<
                EsiAPIWalletTransactions>(ccpCharacter, ESIAPICharacterMethods.
                WalletTransactions, OnWalletTransactionsUpdated, notifiers.
                NotifyCharacterWalletTransactionsError) { QueryOnStartup = true }));
            // Industry
            m_charIndustryJobsMonitor = new CharacterQueryMonitor<EsiAPIIndustryJobs>(
                ccpCharacter, ESIAPICharacterMethods.IndustryJobs, OnIndustryJobsUpdated,
                notifiers.NotifyCharacterIndustryJobsError) { QueryOnStartup = true };
            m_characterQueryMonitors.Add(m_charIndustryJobsMonitor);
            // Research points
            m_characterQueryMonitors.Add(new CharacterQueryMonitor<EsiAPIResearchPoints>(
                ccpCharacter, ESIAPICharacterMethods.ResearchPoints, OnResearchPointsUpdated,
                notifiers.NotifyCharacterResearchPointsError) { QueryOnStartup = true });
            // Mail
            m_characterQueryMonitors.Add(new CharacterQueryMonitor<EsiAPIMailMessages>(
                ccpCharacter, ESIAPICharacterMethods.MailMessages, OnEVEMailMessagesUpdated,
                notifiers.NotifyEVEMailMessagesError) { QueryOnStartup = true });
            // Mailing lists
            m_characterQueryMonitors.Add(new CharacterQueryMonitor<EsiAPIMailingLists>(
                ccpCharacter, ESIAPICharacterMethods.MailingLists, OnEveMailingListsUpdated,
                    notifiers.NotifyMailingListsError));
            // Notifications
            m_characterQueryMonitors.Add(new CharacterQueryMonitor<EsiAPINotifications>(
                ccpCharacter, ESIAPICharacterMethods.Notifications, OnEVENotificationsUpdated,
                notifiers.NotifyEVENotificationsError) { QueryOnStartup = true });
            // Calendar
            m_characterQueryMonitors.Add(new CharacterQueryMonitor<EsiAPICalendarEvents>(
                ccpCharacter, ESIAPICharacterMethods.UpcomingCalendarEvents,
                OnUpcomingCalendarEventsUpdated, notifiers.
                NotifyCharacterUpcomingCalendarEventsError) { QueryOnStartup = true });
            // PI
            m_characterQueryMonitors.Add(new CharacterQueryMonitor<EsiAPIPlanetaryColoniesList>(
                ccpCharacter, ESIAPICharacterMethods.PlanetaryColonies,
                OnPlanetaryColoniesUpdated, notifiers.
                NotifyCharacterPlanetaryColoniesError) { QueryOnStartup = true });
            // LP
            m_characterQueryMonitors.Add(new CharacterQueryMonitor<EsiAPILoyality>(
                ccpCharacter, ESIAPICharacterMethods.LoyaltyPoints,
                OnLoyaltyPointsUpdated, notifiers.
                NotifyCharacterLoyaltyPointsError) { QueryOnStartup = true });
            m_characterQueryMonitors.ForEach(monitor => ccpCharacter.QueryMonitors.Add(monitor));

            // Enumerate the basic feature monitors into a separate list
            m_basicFeaturesMonitors = new List<IQueryMonitorEx>(m_characterQueryMonitors.Count);
            long basicFeatures = (long)CCPAPIMethodsEnum.BasicCharacterFeatures;
            foreach (var monitor in m_characterQueryMonitors)
            {
                long method = (long)(ESIAPICharacterMethods)monitor.Method;
                if (method == (method & basicFeatures))
                {
                    m_basicFeaturesMonitors.Add(monitor);
                    // If force update is selected, update basic features only
                    if (ccpCharacter.ForceUpdateBasicFeatures)
                        monitor.ForceUpdate(true);
                }
            }

            EveMonClient.TimerTick += EveMonClient_TimerTick;
        }

        #endregion


        #region Properties

        /// <summary>
        /// Gets the character sheet monitor.
        /// </summary>
        /// <value>The character sheet monitor.</value>
        internal CharacterQueryMonitor<EsiAPICharacterSheet> CharacterSheetMonitor { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the character market orders have been queried.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the character market orders have been queried; otherwise, <c>false</c>.
        /// </value>
        internal bool CharacterMarketOrdersQueried => !m_charMarketOrdersMonitor.IsUpdating;

        /// <summary>
        /// Gets or sets a value indicating whether the character contracts have been queried.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the character contracts have been queried; otherwise, <c>false</c>.
        /// </value>
        internal bool CharacterContractsQueried => !m_charContractsMonitor.IsUpdating;

        /// <summary>
        /// Gets or sets a value indicating whether the character industry jobs have been queried.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the character industry jobs have been queried; otherwise, <c>false</c>.
        /// </value>
        internal bool CharacterIndustryJobsQueried => !m_charIndustryJobsMonitor.IsUpdating;

        #endregion


        #region Dispose

        /// <summary>
        /// Called when the object gets disposed.
        /// </summary>
        internal void Dispose()
        {
            EveMonClient.TimerTick -= EveMonClient_TimerTick;

            // Unsubscribe events in monitors
            foreach (var monitor in m_characterQueryMonitors)
                monitor.Dispose();
        }

        #endregion


        #region Querying
        
        /// <summary>
        /// Check if any character sheet related query monitors are still running, and trigger
        /// events if they are all completed.
        /// </summary>
        private void FinishCharacterSheetUpdated()
        {
            // Check if all CharacterSheet related query monitors have completed
            if (!m_characterQueryMonitors.Any(monitor => (ESIAPICharacterMethods.
                CharacterSheet.Equals(monitor.Method) || monitor.Method.HasParent(
                ESIAPICharacterMethods.CharacterSheet)) && monitor.Status == QueryStatus.
                Updating))
            {
                m_characterSheetUpdating = false;
                var target = m_ccpCharacter;
                // Character may have been deleted since we queried
                if (target != null)
                {
                    // Finally all done!
                    EveMonClient.Notifications.InvalidateCharacterAPIError(target);
                    EveMonClient.OnCharacterUpdated(target);
                    EveMonClient.OnCharacterInfoUpdated(target);
                    EveMonClient.OnCharacterImplantSetCollectionChanged(target);
                    // Save character information locally
                    var doc = Util.SerializeToXmlDocument(target.Export());
                    LocalXmlCache.SaveAsync(target.Name, doc).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Processes the queried character's character sheet information.
        /// </summary>
        /// <param name="result"></param>
        private void OnCharacterSheetUpdated(EsiAPICharacterSheet result)
        {
            // Flag that we are waiting for character sheet operations to finish
            if (!m_characterSheetUpdating)
                m_characterSheetUpdating = true;

            var target = m_ccpCharacter;
            // Character may have been deleted since we queried
            if (target != null)
                target.Import(result);
        }

        /// <summary>
        /// Processes the queried character's location.
        /// </summary>
        /// <param name="result"></param>
        private void OnCharacterLocationUpdated(EsiAPILocation result)
        {
            var target = m_ccpCharacter;
            // Character may have been deleted since we queried
            if (target != null)
                target.Import(result);
        }

        /// <summary>
        /// Processes the queried character's clones.
        /// </summary>
        /// <param name="result"></param>
        private void OnCharacterClonesUpdated(EsiAPIClones result)
        {
            var target = m_ccpCharacter;
            // Character may have been deleted since we queried
            if (target != null)
                target.Import(result);
        }

        /// <summary>
        /// Processes the queried character's implants.
        /// </summary>
        /// <param name="result"></param>
        private void OnCharacterImplantsUpdated(List<int> result)
        {
            var target = m_ccpCharacter;
            // Character may have been deleted since we queried
            if (target != null)
            {
                if (result != null)
                    target.Import(result);
                QueryAttributesAsync(target);
            }
        }

        /// <summary>
        /// Notifies the user if character implants could not be queried, but continues to
        /// query the attributes even if this occurs.
        /// </summary>
        /// <param name="character"></param>
        /// <param name="result"></param>
        private void OnCharacterImplantsFailed(CCPCharacter character, EsiResult<List<int>>
            result)
        {
            EveMonClient.Notifications.NotifyCharacterImplantsError(character, result);
            var target = m_ccpCharacter;
            // Character may have been deleted since we queried
            if (target != null)
                QueryAttributesAsync(target);
        }

        /// <summary>
        /// Queries the character's attributes. Called on success or failure of implant
        /// import as attributes must be done second.
        /// </summary>
        private void QueryAttributesAsync(CCPCharacter target)
        {
            // This is only invoked where the character has already been checked against null
            ESIKey esiKey = target.Identity.FindAPIKeyWithAccess(ESIAPICharacterMethods.
                Attributes);
            if (esiKey != null && !EsiErrors.IsErrorCountExceeded)
                EveMonClient.APIProviders.CurrentProvider.QueryEsi<EsiAPIAttributes>(
                    ESIAPICharacterMethods.Attributes, OnCharacterAttributesUpdated,
                    new ESIParams(m_attrResponse, esiKey.AccessToken)
                    {
                        ParamOne = target.CharacterID
                    });
        }

        /// <summary>
        /// Processes the queried character's attributes.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="ignore"></param>
        private void OnCharacterAttributesUpdated(EsiResult<EsiAPIAttributes> result,
            object ignore)
        {
            var target = m_ccpCharacter;
            m_attrResponse = result.Response;
            // Character may have been deleted since we queried
            if (target != null && target.Monitored)
            {
                if (target.ShouldNotifyError(result, ESIAPICharacterMethods.Attributes))
                    EveMonClient.Notifications.NotifyCharacterAttributesError(target, result);
                if (!result.HasError && result.HasData)
                    target.Import(result.Result);
            }
        }

        /// <summary>
        /// Processes the queried character's ship.
        /// </summary>
        /// <param name="result"></param>
        private void OnCharacterShipUpdated(EsiAPIShip result)
        {
            var target = m_ccpCharacter;
            // Character may have been deleted since we queried
            if (target != null)
                target.Import(result);
        }

        /// <summary>
        /// Processes the queried character's skills.
        /// </summary>
        /// <param name="result"></param>
        private void OnCharacterSkillsUpdated(EsiAPISkills result)
        {
            var target = m_ccpCharacter;
            // Character may have been deleted since we queried
            if (target != null)
                target.Import(result, m_lastQueue);
        }

        /// <summary>
        /// Processes the queried character's employment history.
        /// </summary>
        /// <param name="result"></param>
        private void OnCharacterEmploymentUpdated(EsiAPIEmploymentHistory result)
        {
            var target = m_ccpCharacter;
            // Character may have been deleted since we queried
            if (target != null)
                target.Import(result);
        }
        
        /// <summary>
        /// Processes the queried character's skill queue information.
        /// </summary>
        /// <param name="result"></param>
        private void OnSkillQueueUpdated(EsiAPISkillQueue result)
        {
            var target = m_ccpCharacter;
            // Character may have been deleted since we queried
            if (target != null)
            {
                m_lastQueue = result;
                target.SkillQueue.Import(result.CreateSkillQueue());
                // Check if the character has less than the threshold queue length
                if (target.IsTraining && target.SkillQueue.LessThanWarningThreshold)
                    EveMonClient.Notifications.NotifySkillQueueThreshold(target,
                        Settings.UI.MainWindow.SkillQueueWarningThresholdDays);
                else
                    EveMonClient.Notifications.InvalidateSkillQueueThreshold(target);
            }
            else
                m_lastQueue = null;
        }

        /// <summary>
        /// Processes the queried character's standings information.
        /// </summary>
        /// <param name="result"></param>
        private void OnStandingsUpdated(EsiAPIStandings result)
        {
            var target = m_ccpCharacter;
            // Character may have been deleted since we queried
            if (target != null)
            {
                // Import the data
                target.Standings.Import(result);
                // Fires the event regarding standings update
                EveMonClient.OnCharacterStandingsUpdated(target);
            }
        }

        /// <summary>
        /// Processes the queried character's factional warfare statistic information.
        /// </summary>
        /// <param name="result"></param>
        private void OnFactionalWarfareStatsUpdated(EsiAPIFactionalWarfareStats result)
        {
            var target = m_ccpCharacter;
            int factionID = result.FactionID;
            // Character may have been deleted since we queried
            if (target != null)
            {
                // Update the enlisted in factional warfare flag
                if (factionID != 0)
                {
                    target.IsFactionalWarfareNotEnlisted = false;
                    target.FactionalWarfareStats = new FactionalWarfareStats(result);
                }
                else
                    target.IsFactionalWarfareNotEnlisted = true;
                // Fires the event regarding factional warfare stats update
                EveMonClient.OnCharacterFactionalWarfareStatsUpdated(target);
            }
        }

        /// <summary>
        /// Processes the queried character's assets information.
        /// </summary>
        /// <param name="result"></param>
        private void OnAssetsUpdated(EsiAPIAssetList result)
        {
            var target = m_ccpCharacter;
            // Character may have been deleted since we queried
            if (target != null)
                TaskHelper.RunCPUBoundTaskAsync(() => target.Assets.Import(result)).
                    ContinueWith(_ =>
                    {
                        EveMonClient.OnCharacterAssetsUpdated(target);
                    }, EveMonClient.CurrentSynchronizationContext);
        }

        /// <summary>
        /// Processes the queried character's market orders. Called from the history fetch on
        /// success or failure, but merges the original orders too.
        /// </summary>
        /// <param name="result"></param>
        /// <remarks>This method is sensitive to which "issued for" orders gets queried first</remarks>
        private void OnMarketOrdersCompleted(EsiResult<EsiAPIMarketOrders> result,
            object regularOrders)
        {
            var target = m_ccpCharacter;
            // Character may have been deleted since we queried
            if (target != null && regularOrders is EsiAPIMarketOrders orders)
            {
                var endedOrders = new LinkedList<MarketOrder>();
                var allOrders = new EsiAPIMarketOrders();
                m_orderHistoryResponse = result.Response;
                // Ignore the If-Modified-Since and cache timer on order history to ensure
                // that old orders are not wiped out
                if (m_orderHistoryResponse != null)
                {
                    m_orderHistoryResponse.Expires = null;
                    m_orderHistoryResponse.ETag = null;
                }
                // Add normal orders first
                if (orders != null)
                    allOrders.AddRange(orders);
                // Add result second
                if (result != null && !result.HasError && result.Result != null)
                    allOrders.AddRange(result.Result);
                allOrders.SetAllIssuedBy(target.CharacterID);
                target.CharacterMarketOrders.Import(allOrders, IssuedFor.Character,
                    endedOrders);
                EveMonClient.OnCharacterMarketOrdersUpdated(target, endedOrders);
                allOrders.Clear();
                // Notify if either one failed
                if (result != null && result.HasError)
                    EveMonClient.Notifications.NotifyCharacterMarketOrdersError(target,
                        result);
            }
        }

        /// <summary>
        /// Queries the character's historical market orders. The orders get imported even if
        /// fetching the historical orders fail.
        /// </summary>
        private void OnMarketOrdersUpdated(EsiAPIMarketOrders result)
        {
            var target = m_ccpCharacter;
            // Character may have been deleted since we queried
            if (target != null)
            {
                var esiKey = target.Identity.FindAPIKeyWithAccess(ESIAPICharacterMethods.
                    MarketOrders);
                if (esiKey != null && !EsiErrors.IsErrorCountExceeded)
                    EveMonClient.APIProviders.CurrentProvider.QueryEsi<EsiAPIMarketOrders>(
                        ESIAPICharacterMethods.MarketOrdersHistory, OnMarketOrdersCompleted,
                        new ESIParams(m_orderHistoryResponse, esiKey.AccessToken)
                        {
                            ParamOne = target.CharacterID
                        }, result);
                else
                    OnMarketOrdersCompleted(null, result);
            }
        }

        /// <summary>
        /// Processes the queried character's contracts.
        /// </summary>
        /// <param name="result"></param>
        /// <remarks>This method is sensitive to which "issued for" contracts gets queried first</remarks>
        private void OnContractsUpdated(EsiAPIContracts result)
        {
            var target = m_ccpCharacter;
            // Character may have been deleted since we queried
            if (target != null)
            {
                foreach (var contract in result)
                    contract.APIMethod = ESIAPICharacterMethods.Contracts;
                var endedContracts = new List<Contract>();
                target.CharacterContracts.Import(result, endedContracts);
                EveMonClient.OnCharacterContractsUpdated(target, endedContracts);
            }
        }

        /// <summary>
        /// Processes the queried character's wallet balance.
        /// </summary>
        /// <param name="result"></param>
        private void OnWalletBalanceUpdated(string result)
        {
            var target = m_ccpCharacter;
            // Character may have been deleted since we queried
            if (target != null)
            {
                target.Import(result);
                // Notify for insufficient balance
                target.NotifyInsufficientBalance();
            }
        }

        /// <summary>
        /// Processes the queried character's wallet journal information.
        /// </summary>
        /// <param name="result"></param>
        private void OnWalletJournalUpdated(EsiAPIWalletJournal result)
        {
            var target = m_ccpCharacter;
            // Character may have been deleted since we queried
            if (target != null)
            {
                target.WalletJournal.Import(result.ToXMLItem().WalletJournal);
                EveMonClient.OnCharacterWalletJournalUpdated(target);
            }
        }

        /// <summary>
        /// Processes the queried character's wallet transactions information.
        /// </summary>
        /// <param name="result"></param>
        private void OnWalletTransactionsUpdated(EsiAPIWalletTransactions result)
        {
            var target = m_ccpCharacter;
            // Character may have been deleted since we queried
            if (target != null)
            {
                target.WalletTransactions.Import(result.ToXMLItem().WalletTransactions);
                EveMonClient.OnCharacterWalletTransactionsUpdated(target);
            }
        }

        /// <summary>
        /// Processes the queried character's personal industry jobs.
        /// </summary>
        /// <param name="result"></param>
        /// <remarks>This method is sensitive to which "issued for" jobs gets queried first</remarks>
        private void OnIndustryJobsUpdated(EsiAPIIndustryJobs result)
        {
            var target = m_ccpCharacter;
            // Character may have been deleted since we queried
            if (target != null)
            {
                target.CharacterIndustryJobs.Import(result, IssuedFor.Character);
                EveMonClient.OnCharacterIndustryJobsUpdated(target);
            }
        }

        /// <summary>
        /// Processes the queried character's research points.
        /// </summary>
        /// <param name="result"></param>
        private void OnResearchPointsUpdated(EsiAPIResearchPoints result)
        {
            var target = m_ccpCharacter;
            // Character may have been deleted since we queried
            if (target != null)
            {
                target.ResearchPoints.Import(result);
                EveMonClient.OnCharacterResearchPointsUpdated(target);
            }
        }

        /// <summary>
        /// Processes the queried character's EVE mail messages.
        /// </summary>
        /// <param name="result"></param>
        private void OnEVEMailMessagesUpdated(EsiAPIMailMessages result)
        {
            var target = m_ccpCharacter;
            // Character may have been deleted since we queried
            if (target != null)
            {
                target.EVEMailMessages.Import(result.ToXMLItem().Messages);
                // Notify on new messages
                int newMessages = target.EVEMailMessages.NewMessages;
                if (newMessages != 0)
                    EveMonClient.Notifications.NotifyNewEVEMailMessages(target, newMessages);
            }
        }

        /// <summary>
        /// Processes the queried character's EVE mailing lists
        /// </summary>
        /// <param name="result"></param>
        private void OnEveMailingListsUpdated(EsiAPIMailingLists result)
        {
            var target = m_ccpCharacter;
            // Character may have been deleted since we queried
            if (target != null)
            {
                target.EVEMailingLists.Import(result);
            }
        }

        /// <summary>
        /// Processes the queried character's EVE notifications.
        /// </summary>
        /// <param name="result"></param>
        private void OnEVENotificationsUpdated(EsiAPINotifications result)
        {
            var target = m_ccpCharacter;
            // Character may have been deleted since we queried
            if (target != null)
            {
                target.EVENotifications.Import(result);
                // Notify on new notifications
                int newNotify = target.EVENotifications.NewNotifications;
                if (newNotify != 0)
                    EveMonClient.Notifications.NotifyNewEVENotifications(target, newNotify);
            }
        }

        /// <summary>
        /// Processes the queried character's contact list.
        /// </summary>
        /// <param name="result"></param>
        private void OnContactsUpdated(EsiAPIContactsList result)
        {
            var target = m_ccpCharacter;
            // Character may have been deleted since we queried
            if (target != null)
            {
                // TODO Corp and alliance contacts
                target.Contacts.Import(result);
                EveMonClient.OnCharacterContactsUpdated(target);
            }
        }

        /// <summary>
        /// Processes the queried character's medals.
        /// </summary>
        /// <param name="result"></param>
        private void OnMedalsUpdated(EsiAPIMedals result)
        {
            var target = m_ccpCharacter;
            // Character may have been deleted since we queried
            if (target != null)
            {
                target.CharacterMedals.Import(result, true);
                EveMonClient.OnCharacterMedalsUpdated(target);
            }
        }

        /// <summary>
        /// Processes the queried character's kill log.
        /// </summary>
        /// <param name="result"></param>
        private void OnKillLogUpdated(EsiAPIKillLog result)
        {
            var target = m_ccpCharacter;
            // Character may have been deleted since we queried
            if (target != null)
            {
                target.KillLog.Import(result);
                EveMonClient.OnCharacterKillLogUpdated(m_ccpCharacter);
            }
        }

        /// <summary>
        /// Processes the queried character's upcoming calendar events.
        /// </summary>
        /// <param name="result"></param>
        private void OnUpcomingCalendarEventsUpdated(EsiAPICalendarEvents result)
        {
            var target = m_ccpCharacter;
            // Character may have been deleted since we queried
            if (target != null)
            {
                target.UpcomingCalendarEvents.Import(result);
                EveMonClient.OnCharacterUpcomingCalendarEventsUpdated(target);
            }
        }

        /// <summary>
        /// Processes the queried character's planetary colonies.
        /// </summary>
        /// <param name="result"></param>
        private void OnPlanetaryColoniesUpdated(EsiAPIPlanetaryColoniesList result)
        {
            var target = m_ccpCharacter;
            // Character may have been deleted since we queried
            if (target != null)
            {
                // Invalidate previous notifications
                EveMonClient.Notifications.InvalidateCharacterPlanetaryPinCompleted(target);

                target.PlanetaryColonies.Import(result);
                EveMonClient.OnCharacterPlanetaryColoniesUpdated(target);
            }
        }

        /// <summary>
        /// Processes the queried character's loyalty point information.
        /// </summary>
        /// <param name="result"></param>
        private void OnLoyaltyPointsUpdated(EsiAPILoyality result)
        {
            var target = m_ccpCharacter;
            // Character may have been deleted since we queried
            if (target != null)
            {
                // Import the data
                target.LoyaltyPoints.Import(result);
                // Fires the event regarding standings update
                EveMonClient.OnCharacterLoyaltyPointsUpdated(target);
            }
        }

        #endregion


        #region Event Handlers

        /// <summary>
        /// Handles the TimerTick event of the EveMonClient control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void EveMonClient_TimerTick(object sender, EventArgs e)
        {
            // If character is monitored enable the basic feature monitoring
            foreach (var monitor in m_basicFeaturesMonitors)
                monitor.Enabled = m_ccpCharacter.Monitored;
            if (m_characterSheetUpdating)
                FinishCharacterSheetUpdated();
        }

        #endregion

    }
}
