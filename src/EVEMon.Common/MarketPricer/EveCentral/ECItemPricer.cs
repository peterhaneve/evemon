using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EVEMon.Common.Collections;
using EVEMon.Common.Constants;
using EVEMon.Common.Data;
using EVEMon.Common.Net;
using EVEMon.Common.Serialization.EveCentral.MarketPricer;
using EVEMon.Common.Service;

namespace EVEMon.Common.MarketPricer.EveCentral
{
    public sealed class ECItemPricer : ItemPricer
    {
        #region Fields

        private const string Filename = "ec_item_prices";

        private static Queue<int> s_queue;
        private static List<int> s_queryMonitorList;
        private static int s_queryCounter;
        private static bool s_queryPending;
        private static int s_queryStep;

        #endregion


        /// <summary>
        /// Gets the name.
        /// </summary>
        public override string Name => "EVEMarketer";

        /// <summary>
        /// Gets a value indicating whether this <see cref="ItemPricer" /> is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if enabled; otherwise, <c>false</c>.
        /// </value>
        protected override bool Enabled => EveMonClient.IsDebugBuild;

        /// <summary>
        /// Gets the price by type ID.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public override double GetPriceByTypeID(int id)
        {
            // Ensure list importation
            EnsureImportation();

            double result;
            PriceByItemID.TryGetValue(id, out result);
            return result;
        }

        /// <summary>
        /// Ensures the importation.
        /// </summary>
        private void EnsureImportation()
        {
            // Quit if query is pending
            if (s_queryPending)
                return;

            // Check the selected provider
            if (!String.IsNullOrWhiteSpace(SelectedProviderName))
            {
                if (SelectedProviderName != Name)
                {
                    Loaded = false;
                    CachedUntil = DateTime.MinValue;
                    SelectedProviderName = Name;
                }
            }
            else
                SelectedProviderName = Name;

            string file = LocalXmlCache.GetFileInfo(Filename).FullName;

            if ((!Loaded && !File.Exists(file)) || (Loaded && CachedUntil < DateTime.UtcNow))
            {
                Task.WhenAll(GetPricesAsync());
                return;
            }

            // Exit if we have already imported the list
            if (Loaded)
                return;
            
            if (File.Exists(file))
                LoadFromFile(file);
            else
            {
                Loaded = true;
                CachedUntil = DateTime.UtcNow.AddHours(1);
                PriceByItemID.Clear();
            }
        }

        /// <summary>
        /// Loads from file.
        /// </summary>
        /// <param name="file">The file.</param>
        private void LoadFromFile(string file)
        {
            CachedUntil = File.GetLastWriteTimeUtc(file).AddDays(1);

            // Deserialize the xml file
            SerializableECItemPrices result = Util.DeserializeXmlFromFile<SerializableECItemPrices>(file);

            // In case the file is an old one, we try to get a fresh copy
            if (result == null || CachedUntil < DateTime.UtcNow)
            {
                Task.WhenAll(GetPricesAsync());
                return;
            }

            PriceByItemID.Clear();
            Loaded = false;

            // Import the data
            Import(result.ItemPrices);
        }

        /// <summary>
        /// Imports the specified item prices.
        /// </summary>
        /// <param name="itemPrices">The item prices.</param>
        private static void Import(IEnumerable<SerializableECItemPriceListItem> itemPrices)
        {
            if (!PriceByItemID.Any())
                EveMonClient.Trace("begin");

            foreach (SerializableECItemPriceListItem item in itemPrices)
            {
                PriceByItemID[item.ID] = item.Prices.Average;
            }

            if (((s_queue == null) || (s_queue.Count == 0)) && !s_queryPending)
                Loaded = true;

            if (Loaded)
                EveMonClient.Trace("done");
        }

        /// <summary>
        /// Gets the prices asynchronous.
        /// </summary>
        /// Gets the item prices list.
        protected override async Task GetPricesAsync()
        {
            // Quit if query is pending
            if (s_queryPending)
                return;

            s_queryPending = true;

            PriceByItemID.Clear();
            Loaded = false;
            s_queryStep = 100;

            IOrderedEnumerable<int> marketItems = StaticItems.AllItems
                .Where(item =>
                    !item.MarketGroup.BelongsIn(DBConstants.RootNonMarketGroupID) &&
                    !item.MarketGroup.BelongsIn(DBConstants.BlueprintRootNonMarketGroupID) &&
                    !item.MarketGroup.BelongsIn(DBConstants.UniqueDesignsRootNonMarketGroupID))
                .Select(item => item.ID)
                .OrderBy(id => id);

            s_queue = new Queue<int>(marketItems);
            s_queryMonitorList = marketItems.ToList();

            EveMonClient.Trace("begin");

            await QueryIDs();
        }

        /// <summary>
        /// Queries the ids.
        /// </summary>
        /// <returns></returns>
        private async Task QueryIDs()
        {
            var idsToQuery = new List<int>();
            var url = new Uri($"{NetworkConstants.EVECentralBaseUrl}{NetworkConstants.EVECentralAPIItemPrices}");

            while (s_queue.Count > 0)
            {
                idsToQuery.Clear();
                for (int i = 0; i < s_queryStep; i++)
                {
                    idsToQuery.Add(s_queue.Dequeue());

                    if (s_queue.Count == 0)
                        break;
                }

                s_queryCounter++;

                DownloadResult<SerializableECItemPrices> result =
                    await Util.DownloadXmlAsync<SerializableECItemPrices>(url,
                        postData: GetPostData(idsToQuery), acceptEncoded: true);
                OnPricesDownloaded(result);
            }
        }

        /// <summary>
        /// Gets the post data.
        /// </summary>
        /// <param name="idsToQuery">The ids to query.</param>
        /// <returns></returns>
        private static string GetPostData(IReadOnlyCollection<int> idsToQuery)
        {
            StringBuilder sb = new StringBuilder();

            foreach (int i in idsToQuery)
            {
                sb.Append($"typeid={i}");

                if (idsToQuery.Last() != i)
                    sb.Append("&");
            }

            SolarSystem jitaSolarSystem = StaticGeography.GetSolarSystemByName("Jita");

            if (jitaSolarSystem != null)
                sb.Append($"&usesystem={jitaSolarSystem.ID}");

            return sb.ToString();
        }

        /// <summary>
        /// Called when prices downloaded.
        /// </summary>
        /// <param name="result">The result.</param>
        private void OnPricesDownloaded(DownloadResult<SerializableECItemPrices> result)
        {
            if (CheckQueryStatus(result))
                return;

            if (EveMonClient.IsDebugBuild)
                EveMonClient.Trace($"Remaining ids: {String.Join(", ", s_queryMonitorList)}", printMethod: false);

            Loaded = true;
            CachedUntil = DateTime.UtcNow.AddDays(1);

            // Reset query pending flag
            s_queryPending = false;

            EveMonClient.Trace("done");

            EveMonClient.OnPricesDownloaded(null, String.Empty);

            // Save the file in cache
            SaveAsync(Filename, Util.SerializeToXmlDocument(Export())).ConfigureAwait(false);
        }

        /// <summary>
        /// Checks the query status.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        private bool CheckQueryStatus(DownloadResult<SerializableECItemPrices> result)
        {
            s_queryCounter--;

            if (result == null || result.Error != null)
            {
                if (result?.Error != null)
                {
                    EveMonClient.Trace(result.Error.Message);

                        // Abort further attempts
                    if (result.Error.Status == HttpWebClientServiceExceptionStatus.Timeout ||
                        result.Error.Status == HttpWebClientServiceExceptionStatus.ServerError)
                    {
                        s_queue.Clear();

                        // Set a retry
                        Loaded = true;
                        CachedUntil = DateTime.UtcNow.AddHours(1);

                        // Reset query pending flag
                        s_queryPending = false;
                        EveMonClient.OnPricesDownloaded(null, String.Empty);

                        // We return 'true' to avoid saving a file
                        return true;
                    }

                    // If it's a 'Bad Request' just return 
                    // We'll check those items later on a lower query step
                    if (result.Error.Status == HttpWebClientServiceExceptionStatus.Exception &&
                        result.Error.Message.Contains("400 (Bad Request)") && s_queue.Count != 0)
                    {
                        return true;
                    }

                    // If we are done set the proper flags
                    if (!s_queryMonitorList.Any() || s_queryStep <= 1)
                    {
                        Loaded = true;
                        EveMonClient.Trace("ECItemPricer.Import - done", printMethod: false);
                        return false;
                    }
                }
            }

            // When the query succeeds import the data and remove the ids from the monitoring list
            if (result?.Result != null)
            {
                foreach (SerializableECItemPriceListItem item in result.Result.ItemPrices)
                {
                    s_queryMonitorList.Remove(item.ID);
                }

                Import(result.Result.ItemPrices);
            }

            // If all items where queried we are done
            if (s_queryCounter == 0 && s_queue.Count == 0 && s_queryStep <= 1)
                return false;

            // If there are still items in queue just continue
            if (s_queryCounter != 0 || !s_queryMonitorList.Any() || s_queue.Count != 0)
                return true;

            // if there are ids still to query repeat the query on a lower query step
            s_queryStep = s_queryStep / 2;
            s_queue = new Queue<int>(s_queryMonitorList);

            Task.WhenAll(QueryIDs());

            return true;
        }

        /// <summary>
        /// Exports the cache list to a serializable object.
        /// </summary>
        /// <returns></returns>
        private static SerializableECItemPrices Export()
        {
            IEnumerable<SerializableECItemPriceListItem> entitiesList = PriceByItemID
                .OrderBy(x => x.Key)
                .Select(
                    item =>
                        new SerializableECItemPriceListItem
                        {
                            ID = item.Key,
                            Prices = new SerializableECItemPriceItem { Average = item.Value }
                        });

            SerializableECItemPrices serial = new SerializableECItemPrices();
            serial.ItemPrices.AddRange(entitiesList);

            return serial;
        }
    }
}
