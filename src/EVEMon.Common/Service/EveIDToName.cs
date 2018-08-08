using EVEMon.Common.Collections;
using EVEMon.Common.Constants;
using EVEMon.Common.Data;
using EVEMon.Common.Enumerations.CCPAPI;
using EVEMon.Common.Extensions;
using EVEMon.Common.Models;
using EVEMon.Common.Serialization;
using EVEMon.Common.Serialization.Esi;
using EVEMon.Common.Serialization.Eve;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using StringIDInfo = EVEMon.Common.Service.IDInformation<string, string>;

namespace EVEMon.Common.Service
{
    public static class EveIDToName
    {
        private const string Filename = "EveIDToName";

        // Cache used to return all data, this is saved and loaded into the file
        private static readonly Dictionary<long, StringIDInfo> s_cacheList =
            new Dictionary<long, StringIDInfo>();

        // Provider for characters, corps, and alliances
        // Thank goodness for the consolidated names endpoint
        private static readonly GenericIDToNameProvider s_lookup =
            new GenericIDToNameProvider(s_cacheList);

        private static bool s_savePending;
        private static DateTime s_lastSaveTime;

        /// <summary>
        /// Static Constructor.
        /// </summary>
        static EveIDToName()
        {
            EveMonClient.TimerTick += EveMonClient_TimerTick;
        }

        #region Helpers

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
        /// Gets the character, corporation, or alliance name from its ID.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="bypass">false (default) to allow local lookup optimizations, true
        /// to force a query to ESI API (depending on local cache)</param>
        /// <returns>The entity name, or EveMonConstants.UnknownText if it is being queried.</returns>
        internal static string GetIDToName(long id, bool bypass = false)
        {
            return s_lookup.LookupID(id, bypass) ?? EveMonConstants.UnknownText;
        }

        /// <summary>
        /// Gets character, corporation, or alliance names from their IDs.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>The entity names, or null for each entry being queried.</returns>
        internal static IEnumerable<string> GetIDsToNames(IEnumerable<long> ids)
        {
            return s_lookup.LookupAllID(ids);
        }

        #endregion


        #region Importation/Exportation

        /// <summary>
        /// Initializes the cache from file.
        /// </summary>
        public static void InitializeFromFile()
        {
            // Quit if the client has been shut down
            if (EveMonClient.Closed || s_cacheList.Any())
                return;
            // Deserialize the file
            var cache = LocalXmlCache.Load<SerializableEveIDToName>(Filename, true);
            if (cache != null)
                // Add the data to the cache
                Import(cache.Entities.Select(entity => new SerializableCharacterNameListItem {
                    ID = entity.ID,
                    Name = entity.Name
                }));
            // For blank corporations and alliances
            s_lookup.Prefill(0L, "(None)");
        }
        
        /// <summary>
        /// Imports the data from the query result.
        /// </summary>
        /// <param name="entities">The entities.</param>
        private static void Import(IEnumerable<SerializableCharacterNameListItem> entities)
        {
            foreach (var entity in entities)
                s_lookup.Prefill(entity.ID, entity.Name);
        }

        /// <summary>
        /// Every timer tick, checks whether we should save the list every 10s.
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
        private static SerializableEveIDToName Export()
        {
            var serial = new SerializableEveIDToName();

            lock (s_cacheList)
            {
                serial.Entities.AddRange(s_cacheList.Select(item =>
                    new SerializableEveIDToNameListItem
                    {
                        ID = item.Key,
                        // Should never be null, but better than crashing...
                        Name = item.Value?.Value ?? EveMonConstants.UnknownText,
                    }));
            }

            return serial;
        }

        #endregion


        /// <summary>
        /// Provides character, corp, or alliance ID to name conversion. Uses the combined
        /// names endpoint.
        /// </summary>
        private sealed class GenericIDToNameProvider : IDToObjectProvider<string, string>
        {
            // Only this many IDs can be requested in one attempt
            private const int MAX_IDS = 250;

            public GenericIDToNameProvider(IDictionary<long, StringIDInfo> cacheList) :
                base(cacheList) { }

            protected override StringIDInfo CreateIDInfo(long id, string value)
            {
                return new GenericIDInformation(id, value);
            }

            protected override void FetchIDs()
            {
                var toDo = new LinkedList<long>();
                StringIDInfo info;
                lock (m_pendingIDs)
                {
                    // Take up to MAX_IDS of them, in sorted order
                    var enumerator = m_pendingIDs.GetEnumerator();
                    for (int i = 0; i < MAX_IDS && enumerator.MoveNext(); i++)
                        toDo.AddLast(enumerator.Current.Key);
                    foreach (long key in toDo)
                    {
                        m_pendingIDs.Remove(key);
                        m_cache.TryGetValue(key, out info);
                        info?.OnRequestStart(null);
                    }
                }
                string ids = "[ " + string.Join(",", toDo) + " ]";
                // Given the number of IDs we are requesting, it is very unlikely that the
                // eTag or expiration will be useful
                EveMonClient.APIProviders.CurrentProvider.QueryEsi<EsiAPICharacterNames>(
                    ESIAPIGenericMethods.CharacterName, OnQueryAPICharacterNameUpdated,
                    new ESIParams()
                    {
                        PostData = ids
                    });
            }

            private void OnQueryAPICharacterNameUpdated(EsiResult<EsiAPICharacterNames> result,
                object ignore)
            {
                StringIDInfo info;
                // Bail if there is an error
                if (result.HasError)
                    EveMonClient.Notifications.NotifyCharacterNameError(result);
                else
                {
                    EveMonClient.Notifications.InvalidateAPIError();
                    // This probably will never be false since we did not provide an etag
                    if (result.HasData)
                        lock (m_cache)
                        {
                            // Add resulting names to the cache; duplicates should not occur,
                            // but guard against them defensively
                            foreach (var namePair in result.Result)
                            {
                                m_cache.TryGetValue(namePair.ID, out info);
                                info?.OnRequestComplete(namePair.Name);
                            }
                        }
                }
                OnLookupComplete();
            }

            protected override string Prefetch(long id)
            {
                string name = null;

                if (id < int.MaxValue && id > int.MinValue && id != 0)
                {
                    int intId = (int)id;

                    // Check NPC corporations
                    var npcCorp = StaticGeography.GetCorporationByID(intId);
                    if (npcCorp != null)
                        name = npcCorp.Name;
                    else
                    {
                        // Check NPC factions
                        var npcFaction = StaticGeography.GetFactionByID(intId);
                        if (npcFaction != null)
                            name = npcFaction.Name;
                    }
                }
                // Try filling with a current character identity or corporation/alliance
                if (string.IsNullOrEmpty(name))
                    foreach (var character in EveMonClient.Characters)
                    {
                        string corpName = character.CorporationName, allianceName = character.
                            AllianceName;
                        if (character.CharacterID == id)
                        {
                            name = character.Name;
                            break;
                        }
                        if (character.CorporationID == id && !corpName.IsEmptyOrUnknown())
                        {
                            name = corpName;
                            break;
                        }
                        if (character.AllianceID == id && !allianceName.IsEmptyOrUnknown())
                        {
                            name = allianceName;
                            break;
                        }
                    }
                return name;
            }

            protected override void TriggerEvent() {
                EveMonClient.OnEveIDToNameUpdated();
                s_savePending = true;
            }
        }

        /// <summary>
        /// A simple implementation of IDInformation for maintaining ID state.
        /// </summary>
        private sealed class GenericIDInformation : IDInformation<string, string>
        {
            public long ID { get; }

            public string Value { get; private set; }

            /// <summary>
            /// True if a request has been attempted, or false otherwise.
            /// </summary>
            private bool requested;

            public GenericIDInformation(long id, string value)
            {
                ID = id;
                Value = value;
                requested = false;
            }

            public void OnRequestComplete(string result)
            {
                bool isBlank = string.IsNullOrWhiteSpace(result);
                if (Value == null || !isBlank)
                    // Avoid overwriting cached result with failed result
                    Value = isBlank ? null : result;
            }

            public void OnRequestStart(string extra)
            {
                requested = true;
            }

            public bool RequestAttempted(string extra)
            {
                return requested;
            }

            public override string ToString()
            {
                return string.Format("{0:D} => {1}", ID, Value);
            }
        }
    }
}
