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

namespace EVEMon.Common.Service
{
    /// <summary>
    /// Converts citadel and NPC station IDs to the correct information (including names).
    /// </summary>
    public static class EveIDToStation
    {
        private const string Filename = "ConquerableStationList";

        // Cache used to return all data, this is saved and loaded into the file
        private static readonly Dictionary<long, CitadelInfo> s_cacheList =
            new Dictionary<long, CitadelInfo>();
        
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
                var serStation = s_cita.LookupIDESI(id, character)?.Station;
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
            {
                long id = entity.StationID;

                // Add the query result to our cache list if it doesn't exist already
                if (!s_cacheList.ContainsKey(id))
                    s_cacheList.Add(id, new CitadelInfo(entity));
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
                foreach (var station in s_cacheList.Values)
                {
                    // Only add stations which have been successfully fetched
                    var result = station.Station;
                    if (result != null)
                        serial.Stations.Add(result);
                }
            }

            return serial;
        }
        
        /// <summary>
        /// Provides citadel ID lookups. Uses the citadel info endpoint.
        /// </summary>
        private class CitadelStationProvider : IDToObjectProvider<CitadelInfo, ESIKey>
        {
            // Tracks whether we actually got anywhere when a request was made
            private volatile bool madeProgress;

            public CitadelStationProvider(IDictionary<long, CitadelInfo> cacheList) :
                base(cacheList)
            {
                madeProgress = false;
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
                    // info.Station must be null here, because non-null meant it was added
                    // to m_requested and will thus not be seen in FetchIDs ever again
                    var info = GetInfoFor(id);
                    // If ESI key was tried before, fail immediately
                    if (esiKey != null && !info.ESIFailed.Contains(esiKey))
                    {
                        // Query ESI for the citadel information
                        // No response is given because requests are only made to ESI once per
                        // key per session
                        EveMonClient.APIProviders.CurrentProvider.QueryEsi<EsiAPIStructure>(
                            ESIAPIGenericMethods.CitadelInfo, OnQueryStationUpdatedEsi,
                            new ESIParams(null, esiKey.AccessToken)
                            {
                                ParamOne = id
                            }, new CitadelRequestInfo(id, esiKey));
                        EveMonClient.Trace("ESI lookup for {0:D} using {1}", id, esiKey);
                    }
#if HAMMERTIME
                    else if (!info.HammertimeFailed)
                        // Only run hammer if we did not try it already
                        LoadCitadelInformationFromHammertimeAPI(id);
#endif
                    else
                        // We tried hammer before and it failed; tried ESI and it failed
                        OnLookupComplete();
                }
            }

            /// <summary>
            /// Retrieves the previous request information for a citadel ID.
            /// </summary>
            /// <param name="id">The citadel ID</param>
            /// <returns>null if never requested before, or information about what was tried
            /// </returns>
            private CitadelInfo GetInfoFor(long id)
            {
                CitadelInfo info;
                lock (s_cacheList)
                {
                    if (!s_cacheList.TryGetValue(id, out info))
                    {
                        // Add a blank entry
                        info = new CitadelInfo();
                        s_cacheList.Add(id, info);
                    }
                }
                return info;
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
                Util.DownloadJsonAsync<HammertimeStructureList>(url).ContinueWith((task) =>
                {
                    OnQueryStationUpdated(task, id);
                });
            }

            private void OnQueryStationUpdated(Task<JsonResult<HammertimeStructureList>>
                result, long id)
            {
                JsonResult<HammertimeStructureList> jsonResult;
                bool failed = false;

                // Bail if there is an error
                if (result.IsFaulted || (jsonResult = result.Result).HasError)
                    failed = true;
                else
                {
                    EveMonClient.Notifications.InvalidateAPIError();
                    // Should only have one result, with an integer key
                    var citInfo = jsonResult.Result;
                    if (citInfo.Count == 1)
                    {
                        madeProgress = true;
                        AddToCache(id, new CitadelInfo(citInfo.Values.First().ToXMLItem(id)));
                    }
                    else
                    {
                        failed = true;
                        EveMonClient.Trace("Citadel Hunt failed for {0:D}", id);
                    }
                }
                if (failed)
                {
                    // We already tried ESI, and now hammer is a no-go as well
                    EveMonClient.Notifications.NotifyCitadelQueryError(null);
                    var failInfo = GetInfoFor(id);
                    failInfo.HammertimeFailed = true;
                    AddToCache(id, failInfo);
                }
            }
#endif

            private void OnQueryStationUpdatedEsi(EsiResult<EsiAPIStructure> result,
                object reqInfo)
            {
                var requestInfo = reqInfo as CitadelRequestInfo;
                if (requestInfo == null)
                    throw new InvalidOperationException("Wrong result type in citadel query");
                long id = requestInfo.ID;
                // Bail if there is an error
                if (result.HasError)
                {
                    var info = GetInfoFor(id);
                    // Mark the ESI key failure so it will not be retried
                    var esiKey = requestInfo.Key;
                    info.ESIFailed.Add(esiKey);
                    EveMonClient.Trace("ESI lookup failed for {0:D} blacklisting {1}", id,
                        esiKey);
#if HAMMERTIME
                    LoadCitadelInformationFromHammertimeAPI(id);
#else
                    EveMonClient.Notifications.NotifyCitadelQueryError(result);
                    // Requested but failed
                    AddToCache(id, info);
#endif
                }
                else
                {
                    EveMonClient.Notifications.InvalidateAPIError();
                    if (result.HasData)
                    {
                        madeProgress = true;
                        AddToCache(id, new CitadelInfo(result.Result.ToXMLItem(id)));
                    }
                    else
                        // Technically unreachable until we specify an ETag/If-Modified-Since
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
            public CitadelInfo LookupIDESI(long id, CCPCharacter character, bool
                bypass = false)
            {
                var key = character?.Identity?.FindAPIKeyWithAccess(ESIAPICharacterMethods.
                    CitadelInfo);
                return LookupID(id, bypass, key);
            }

            private void AddToCache(long id, CitadelInfo info)
            {
                lock (s_cacheList)
                {
                    // Add resulting info to the cache; duplicates should not occur, but
                    // guard against them defensively
                    if (s_cacheList.ContainsKey(id))
                        s_cacheList[id] = info;
                    else
                        s_cacheList.Add(id, info);
                    // Only add to requested list if successful since that will bar the code
                    // from ever checking it again
                    if (info.Station != null)
                        m_requested.Add(id);
                }
                OnLookupComplete();
            }

            protected override void TriggerEvent()
            {
                if (madeProgress)
                    EveMonClient.OnConquerableStationListUpdated();
                madeProgress = false;
                s_savePending = true;
            }
        }

        /// <summary>
        /// A class storing the state of an ESI/Hammertime station request, and if it failed,
        /// the methods which have been tried so far.
        /// </summary>
        private sealed class CitadelInfo
        {
            /// <summary>
            /// The ESI keys which have already been tried and failed.
            /// </summary>
            public ISet<ESIKey> ESIFailed { get; }

            /// <summary>
            /// True if hammertime was tried and failed.
            /// </summary>
            public bool HammertimeFailed { get; set; }

            /// <summary>
            /// The station result if successful.
            /// </summary>
            public SerializableOutpost Station { get; set; }

            /// <summary>
            /// Constructor around an existing station result.
            /// </summary>
            public CitadelInfo(SerializableOutpost station = null)
            {
                ESIFailed = new HashSet<ESIKey>();
                HammertimeFailed = false;
                Station = station;
            }

            public override string ToString()
            {
                return Station?.ToString() ?? EveMonConstants.UnknownText;
            }
        }

        /// <summary>
        /// Information about an ESI request (passed in the state field) for citadel info.
        /// </summary>
        private sealed class CitadelRequestInfo
        {
            /// <summary>
            /// The ESI key used to request.
            /// </summary>
            public ESIKey Key { get; }

            /// <summary>
            /// The ID requested.
            /// </summary>
            public long ID { get; }

            /// <summary>
            /// Wraps citadel request info in a single state object.
            /// </summary>
            /// <param name="id">The citadel ID</param>
            /// <param name="key">The ESI key used to request</param>
            public CitadelRequestInfo(long id, ESIKey key)
            {
                if (id == 0L)
                    throw new ArgumentException("id");
                if (key == null)
                    throw new ArgumentNullException("key");
                ID = id;
                Key = key;
            }

            public override string ToString()
            {
                return string.Format("ID #{0:D} using {1}", ID, Key);
            }
        }
    }
}
