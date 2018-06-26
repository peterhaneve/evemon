#define HAMMERTIME

using EVEMon.Common.Collections;
using EVEMon.Common.Constants;
using EVEMon.Common.Data;
using EVEMon.Common.Enumerations.CCPAPI;
using EVEMon.Common.Helpers;
using EVEMon.Common.Models;
using EVEMon.Common.Serialization;
using EVEMon.Common.Serialization.Datafiles;
using EVEMon.Common.Serialization.Esi;
using EVEMon.Common.Serialization.Eve;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using HammertimeStructureList = System.Collections.Generic.Dictionary<string, EVEMon.Common.
    Serialization.Hammertime.HammertimeStructure>;

namespace EVEMon.Common.Service
{
    public static class EveIDToStation
    {
        private const string Filename = "ConquerableStationList";

        // Cache used to return all data, this is saved and loaded into the file
        private static readonly Dictionary<long, SerializableOutpost> s_cacheList =
            new Dictionary<long, SerializableOutpost>();

        // Provider for conquerable stations (NPC stations go through staticgeography)
        private static readonly IDToObjectProvider<SerializableOutpost, string> s_conq =
            new ConquerableStationProvider(s_cacheList);

        // Provider for citadels
        private static readonly CitadelStationProvider s_cita = new CitadelStationProvider(
            s_cacheList);

        private static bool s_savePending;
        private static DateTime s_lastSaveTime;

        /// <summary>
        /// Static Constructor.
        /// </summary>
        static EveIDToStation()
        {
            EveMonClient.TimerTick += EveMonClient_TimerTick;
        }

        /// <summary>
        /// Handles the TimerTick event of the EveMonClient control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private static async void EveMonClient_TimerTick(object sender, EventArgs e)
        {
            await UpdateOnOneSecondTickAsync();
        }
        
        /// <summary>
        /// Gets the station information from its ID. Works on NPC, conquerable, and citadel stations.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>The station information</returns>
        internal static Station GetIDToStation(long id, CCPCharacter character = null)
        {
            var station = StaticGeography.GetStationByID(id);
            if (station == null && id != 0L)
            {
                SerializableOutpost serStation = null;

                // Citadels have ID over maximum int value
                if (id < int.MaxValue)
                    serStation = s_conq.LookupID(id);
                else
                    serStation = s_cita.LookupIDESI(id, character);

                if (serStation != null)
                    station = new Station(serStation);
            }
            return station;
        }
        
        /// <summary>
        /// Initializes the cache from file.
        /// </summary>
        public static void InitializeFromFile()
        {
            // Quit if the client has been shut down
            if (EveMonClient.Closed || s_cacheList.Any())
                return;
            var cache = LocalXmlCache.Load<SerializableStationList>(Filename, true);
            // Add the data to the cache
            if (cache != null)
                Import(cache.Stations);
        }
        
        /// <summary>
        /// Imports the data from the query result.
        /// </summary>
        /// <param name="entities">The entities.</param>
        private static void Import(IEnumerable<SerializableOutpost> entities)
        {
            if (entities != null)
            {
                foreach (SerializableOutpost entity in entities)
                {
                    if (entity == null) continue;
                    long id = entity.StationID;

                    // Add the query result to our cache list if it doesn't exist already
                    if (!s_cacheList.ContainsKey(id))
                        s_cacheList.Add(id, entity);
                }
            }
        }

        /// <summary>
        /// Every timer tick, checks whether we should save the list every 10s.
        /// 
        /// While this looks similar to EveIDToName.UpdateOnOneSecondTickAsync, the methods
        /// are static, so there is little to gain by inheriting from an abstract superclass.
        /// </summary>
        private static Task UpdateOnOneSecondTickAsync()
        {
            // Is a save requested and is the last save older than 10s ?
            if (s_savePending && DateTime.UtcNow > s_lastSaveTime.AddSeconds(10))
                return SaveImmediateAsync();

            return Task.CompletedTask;
        }
        
        /// <summary>
        /// Saves this cache list to a file.
        /// </summary>
        public static async Task SaveImmediateAsync()
        {
            // Save in file
            await LocalXmlCache.SaveAsync(Filename, Util.SerializeToXmlDocument(Export()));

            // Reset savePending flag
            s_lastSaveTime = DateTime.UtcNow;
            s_savePending = false;
        }

        /// <summary>
        /// Exports the cache list to a serializable object.
        /// </summary>
        /// <returns></returns>
        private static SerializableStationList Export()
        {
            var serial = new SerializableStationList();

            lock (s_cacheList)
            {
                serial.Stations.AddRange(s_cacheList.Values);
            }

            return serial;
        }

        /// <summary>
        /// Provides station ID lookups. Uses the NPC/conquerable station endpoint.
        /// </summary>
        private class ConquerableStationProvider : IDToObjectProvider<SerializableOutpost,
            string>
        {
            public ConquerableStationProvider(IDictionary<long, SerializableOutpost>
                cacheList) : base(cacheList) { }

            protected override void FetchIDs()
            {
                long id = 0L;
                lock (m_pendingIDs)
                {
                    using (var it = m_pendingIDs.GetEnumerator())
                    {
                        // Fetch the next ID if it is available
                        if (it.MoveNext())
                        {
                            id = it.Current.Key;
                            m_pendingIDs.Remove(id);
                        }
                    }
                }
                if (id != 0L)
                    EveMonClient.APIProviders.CurrentProvider.QueryEsi<EsiAPIStation>(
                        ESIAPIGenericMethods.StationInfo, OnQueryStationUpdated,
                        new ESIParams()
                        {
                            ParamOne = id
                        }, id);
            }

            private void OnQueryStationUpdated(EsiResult<EsiAPIStation> result, object ignore)
            {
                // Bail if there is an error
                if (result.HasError)
                {
                    EveMonClient.Notifications.NotifyStationQueryError(result);
                    m_queryPending = false;
                }
                else
                {
                    EveMonClient.Notifications.InvalidateAPIError();
                    if (result.HasData)
                        lock (s_cacheList)
                        {
                            // Add resulting names to the cache; duplicates should not occur,
                            // but guard against them defensively
                            var station = result.Result.ToSerializableOutpost();
                            long id = station.StationID;

                            if (s_cacheList.ContainsKey(id))
                                s_cacheList[id] = station;
                            else
                                s_cacheList.Add(id, station);
                            m_requested.Add(id);
                        }
                    OnLookupComplete();
                }
            }

            protected override void TriggerEvent() {
                EveMonClient.OnConquerableStationListUpdated();
            }
        }

        /// <summary>
        /// Provides citadel ID lookups. Uses the citadel info endpoint.
        /// </summary>
        private class CitadelStationProvider : IDToObjectProvider<SerializableOutpost, ESIKey>
        {
            /// <summary>
            /// Holds information about the query
            /// </summary>
            private class CitadelQueryInfo
            {
                public CitadelQueryInfo(long citadelId, ESIKey key)
                {
                    this.citadelId = citadelId;
                    this.key = key;
                }
                public long citadelId;
                public ESIKey key;
            }
#if HAMMERTIME
            private ISet<long> FailedHammertimeIds = new HashSet<long>();
#endif
            private IDictionary<long, ISet<long>> FailedEsiIds = new Dictionary<long, ISet<long>>();
            public CitadelStationProvider(IDictionary<long, SerializableOutpost> cacheList) :
                base(cacheList) { }

            protected override void FetchIDs()
            {
                long id = 0L;
                ESIKey esiKey = null;

                lock (m_pendingIDs)
                {
                    using (var it = m_pendingIDs.GetEnumerator())
                    {
                        // Fetch the next ID if it is available
                        if (it.MoveNext())
                        {
                            id = it.Current.Key;
                            esiKey = it.Current.Value;
                            m_pendingIDs.Remove(id);
                        }
                    }
                }
                if (id != 0L)
                {
                    lock (FailedEsiIds)
                    {
                        if (esiKey != null && (!FailedEsiIds.ContainsKey(id) || !FailedEsiIds[id].Contains(esiKey.ID)))
                        {
                            // Query ESI for the citadel information
                            // No response is given because keeping a list of all previous
                            // responses just for their expiry / etag is not feasible
                            EveMonClient.APIProviders.CurrentProvider.QueryEsi<EsiAPIStructure>(
                                ESIAPIGenericMethods.CitadelInfo, OnQueryStationUpdatedEsi,
                                new ESIParams(null, esiKey.AccessToken)
                                {
                                    ParamOne = id
                                }, new CitadelQueryInfo(id, esiKey));
                        }
                        else
                        {
#if HAMMERTIME
                            lock (FailedHammertimeIds)
                                if (!FailedHammertimeIds.Contains(id))
                                    LoadCitadelInformationFromHammertimeAPI(id);
                                else
                                    OnLookupComplete();
#else
                            // we move on to the next request
                            OnLookupComplete();
#endif
                        }
                    }
                }
                else // this should not happen but lets be sure
                {
                    // move on to the next request
                    OnLookupComplete();
                }
            }

#if HAMMERTIME
            /// <summary>
            /// Downloads citadel data from the Hammertime Citadel Hunt project.
            /// Avoids some access and API key problems on private citadels.
            /// </summary>
            /// <param name="id">The citadel ID</param>
            private void LoadCitadelInformationFromHammertimeAPI(long id)
            {
                var url = new Uri(string.Format(NetworkConstants.HammertimeCitadel, id));
                Util.DownloadJsonAsync<HammertimeStructureList>(url, null).ContinueWith((task) =>
                {
                    OnQueryStationUpdated(task, id);
                });
            }

            private void OnQueryStationUpdated(Task<JsonResult<HammertimeStructureList>>
                result, long id)
            {
                JsonResult<HammertimeStructureList> jsonResult;

                // Bail if there is an error
                if (result.IsFaulted || (jsonResult = result.Result).HasError)
                {
                    EveMonClient.Notifications.NotifyCitadelQueryError(null);
                    m_queryPending = false;
                    return;
                }
                EveMonClient.Notifications.InvalidateAPIError();
                // Should only have one result, with an integer key
                var citInfo = jsonResult.Result;
                if (citInfo.Count == 1)
                    AddToCache(id, citInfo.Values.First().ToXMLItem(id));
                else
                {
                    lock(FailedHammertimeIds)
                        FailedHammertimeIds.Add(id);
                    // this creates an other id lookup if there are ids pending
                    // triggers events etc
                    OnLookupComplete();
                }
            }
#endif

            private void OnQueryStationUpdatedEsi(EsiResult<EsiAPIStructure> result,
                object idObject)
            {
                CitadelQueryInfo info = (idObject as CitadelQueryInfo);

                // Bail if there is an error
                if (result.HasError || info.citadelId == 0L)
                {
                    EveMonClient.Notifications.NotifyCitadelQueryError(result);
                    m_queryPending = false;
#if HAMMERTIME
                    lock (FailedHammertimeIds)
                    {
                        if (!FailedHammertimeIds.Contains(info.citadelId))
                            LoadCitadelInformationFromHammertimeAPI(info.citadelId);
                        else
                        {
#endif
                            // Requested but failed
                            lock (FailedEsiIds)
                            {
                                ISet<long> keySet = null;
                                if (FailedEsiIds.ContainsKey(info.citadelId))
                                {
                                    keySet = FailedEsiIds[info.citadelId];
                                }
                                else
                                {
                                    keySet = new HashSet<long>();
                                    FailedEsiIds.Add(info.citadelId, keySet);
                                }
                                keySet.Add(info.key.ID);
                            }
                            // do start a new round if there is more ids
                            // if needed trigger event
                            OnLookupComplete();
#if HAMMERTIME
                        }
                    }
#endif
                    return;
                }

                EveMonClient.Notifications.InvalidateAPIError();

                AddToCache(info.citadelId, result.Result.ToXMLItem(info.citadelId));
            }

            /// <summary>
            /// Convert the ID to an object.
            /// </summary>
            /// <param name="id">The ID</param>
            /// <param name="character">The character making the request</param>
            /// <param name="bypass">true to bypass the Prefetch filter, or false (default) to
            /// use it (recommended in most cases)</param>
            /// <returns>The object, or null if no item with this ID exists</returns>
            public SerializableOutpost LookupIDESI(long id, CCPCharacter character, bool
                bypass = false)
            {
                ESIKey key = null;
                if (character != null)
                    key = character.Identity.FindAPIKeyWithAccess(ESIAPICharacterMethods.
                        CitadelInfo);
                return LookupID(id, bypass, key);
            }

            private void AddToCache(long id, SerializableOutpost station)
            {
                lock (s_cacheList)
                {
                    // Add resulting names to the cache; duplicates should not occur, but
                    // guard against them defensively
                    if (s_cacheList.ContainsKey(id))
                        s_cacheList[id] = station;
                    else
                        s_cacheList.Add(id, station);
                    m_requested.Add(id);
                }
                OnLookupComplete();
            }

            protected override void TriggerEvent()
            {
                EveMonClient.OnConquerableStationListUpdated();
                s_savePending = true;
            }
        }
    }
}
