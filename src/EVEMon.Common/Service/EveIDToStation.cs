#define HAMMERTIME

using EVEMon.Common.Constants;
using EVEMon.Common.Data;
using EVEMon.Common.Enumerations.CCPAPI;
using EVEMon.Common.Models;
using EVEMon.Common.Serialization;
using EVEMon.Common.Serialization.Datafiles;
using EVEMon.Common.Serialization.Esi;
using EVEMon.Common.Serialization.Eve;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using HammertimeStructureList = System.Collections.Generic.Dictionary<string, EVEMon.Common.
    Serialization.Hammertime.HammertimeStructure>;
using CitadelIDInfo = EVEMon.Common.Service.IDInformation<EVEMon.Common.Serialization.Eve.
    SerializableOutpost, EVEMon.Common.Models.ESIKey>;

namespace EVEMon.Common.Service
{
    /// <summary>
    /// Converts citadel and NPC station IDs to the correct information (including names).
    /// </summary>
    public static class EveIDToStation
    {
        private const string Filename = "ConquerableStationList";

        // Cache used to return all data, this is saved and loaded into the file
        private static readonly Dictionary<long, CitadelIDInfo> s_cacheList =
            new Dictionary<long, CitadelIDInfo>();
        
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
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event
        /// data.</param>
        private static async void EveMonClient_TimerTick(object sender, EventArgs e)
        {
            await UpdateOnOneSecondTickAsync();
        }
        
        /// <summary>
        /// Gets the station information from its ID. Works on NPC stations and citadels.
        /// Since conquerable stations were converted, no attempt is made to check the ESI
        /// station endpoint online as all stations should (TM) be in the SDE...
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>The station information</returns>
        internal static Station GetIDToStation(long id, CCPCharacter character = null)
        {
            var station = StaticGeography.GetStationByID(id);
            if (station == null && id > int.MaxValue)
            {
                // Citadels have ID over maximum int value
                var serStation = s_cita.LookupIDESI(id, character);
                if (serStation != null)
                    station = new Station(serStation);
                else
                    station = Station.CreateInaccessible(id);
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
            foreach (SerializableOutpost entity in entities)
                s_cita.Prefill(entity.StationID, entity);
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
                foreach (var station in s_cacheList.Values)
                {
                    var result = station.Value;
                    // Only add stations which have been successfully fetched
                    if (result != null)
                        serial.Stations.Add(result);
                }
            }

            return serial;
        }
        
        /// <summary>
        /// Provides citadel ID lookups. Uses the citadel info endpoint.
        /// </summary>
        private class CitadelStationProvider : IDToObjectProvider<SerializableOutpost, ESIKey>
        {
            public CitadelStationProvider(IDictionary<long, CitadelIDInfo> cacheList) :
                base(cacheList)
            {
            }

            protected override CitadelIDInfo CreateIDInfo(long id, SerializableOutpost value)
            {
                return new CitadelInfo(id, value);
            }

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
                    CitadelIDInfo info;
                    lock (m_cache)
                    {
                        m_cache.TryGetValue(id, out info);
                    }
                    // info should never be null at this stage
                    if (esiKey != null)
                    {
                        info.OnRequestStart(esiKey);
                        // Query ESI for the citadel information
                        // No response is given because requests are only made to ESI once per
                        // key per session
                        EveMonClient.APIProviders.CurrentProvider.QueryEsi<EsiAPIStructure>(
                            ESIAPIGenericMethods.CitadelInfo, OnQueryStationUpdatedEsi,
                            new ESIParams(null, esiKey.AccessToken)
                            {
                                ParamOne = id
                            }, info);
                        EveMonClient.Trace("ESI lookup for {0:D} using {1}", id, esiKey);
                    }
#if HAMMERTIME
                    else
                        // Only run hammer if we did not try it already
                        LoadCitadelInformationFromHammertimeAPI(info);
#endif
                }
            }
            
#if HAMMERTIME
            /// <summary>
            /// Downloads citadel data from the Hammertime Citadel Hunt project.
            /// Avoids some access and API key problems on private citadels.
            /// </summary>
            /// <param name="info">The citadel ID information</param>
            private void LoadCitadelInformationFromHammertimeAPI(CitadelIDInfo info)
            {
                var url = new Uri(string.Format(NetworkConstants.HammertimeCitadel, info.ID));
                info.OnRequestStart(null);
                Util.DownloadJsonAsync<HammertimeStructureList>(url).ContinueWith((task) =>
                {
                    OnQueryStationUpdated(task, info);
                });
            }

            private void OnQueryStationUpdated(Task<JsonResult<HammertimeStructureList>>
                result, CitadelIDInfo info)
            {
                var jsonResult = result.Result;
                // Bail if there is an error
                if (result.IsFaulted || jsonResult == null)
                    EveMonClient.Notifications.NotifyCitadelQueryError(null);
                else if (jsonResult.HasError)
                {
                    // Provide some more debugging info with the actual failed response
                    var fakeResult = new EsiResult<EsiAPIStructure>(jsonResult.Response);
                    EveMonClient.Notifications.NotifyCitadelQueryError(fakeResult);
                }
                else
                {
                    EveMonClient.Notifications.InvalidateAPIError();
                    // Should only have one result, with an integer key
                    var hammerData = jsonResult.Result;
                    if (hammerData.Count == 1)
                        info.OnRequestComplete(hammerData.Values.First().ToXMLItem(info.ID));
                    else
                    {
                        EveMonClient.Trace("Citadel Hunt failed for {0:D}", info.ID);
                        info.OnRequestComplete(null);
                    }
                }
                OnLookupComplete();
            }
#endif

            private void OnQueryStationUpdatedEsi(EsiResult<EsiAPIStructure> result,
                object reqInfo)
            {
                var info = reqInfo as CitadelIDInfo;
                if (info == null)
                    throw new ArgumentException("Invalid argument for citadel ID info");
                // Consider trying hammertime if there is an error
                if (result.HasError)
                {
#if HAMMERTIME
                    LoadCitadelInformationFromHammertimeAPI(info);
#else
                    EveMonClient.Notifications.NotifyCitadelQueryError(result);
                    info.OnRequestComplete(null);
                    OnLookupComplete();
#endif
                }
                else
                {
                    EveMonClient.Notifications.InvalidateAPIError();
                    info.OnRequestComplete(result.Result.ToXMLItem(info.ID));
                    OnLookupComplete();
                }
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
                var key = character?.Identity?.FindAPIKeyWithAccess(ESIAPICharacterMethods.
                    CitadelInfo);
                return LookupID(id, bypass, key);
            }
            
            protected override void TriggerEvent()
            {
                EveMonClient.OnConquerableStationListUpdated();
                s_savePending = true;
            }
        }

        /// <summary>
        /// A class storing the state of an ESI/Hammertime station request, and if it failed,
        /// the methods which have been tried so far.
        /// </summary>
        private sealed class CitadelInfo : IDInformation<SerializableOutpost, ESIKey>
        {
            public long ID { get; }

            /// <summary>
            /// The station result if successful.
            /// </summary>
            public SerializableOutpost Value { get; private set; }

            /// <summary>
            /// The ESI keys which have already been tried.
            /// </summary>
            private readonly ISet<ESIKey> esiAttempts;

            /// <summary>
            /// True if hammertime was tried.
            /// </summary>
            private bool hammerAttempt;

            /// <summary>
            /// Constructor around an existing citadel result.
            /// </summary>
            /// <param name="id">The citadel ID.</param>
            /// <param name="station">The citadel value fetched.</param>
            public CitadelInfo(long id, SerializableOutpost station)
            {
                esiAttempts = new HashSet<ESIKey>();
                hammerAttempt = false;
                ID = id;
                Value = station;
            }

            public void OnRequestComplete(SerializableOutpost result)
            {
                if (Value == null || result != null)
                    // Avoid overwriting cached result with failed result
                    Value = result;
            }

            public void OnRequestStart(ESIKey extra)
            {
                if (extra == null)
                    hammerAttempt = true;
                else
                    esiAttempts.Add(extra);
            }

            public bool RequestAttempted(ESIKey extra)
            {
                if (extra == null)
                    return hammerAttempt;
                else
                    return esiAttempts.Contains(extra);
            }

            public override string ToString()
            {
                return string.Format("{0:D} => {1}", ID, Value);
            }
        }
    }
}
