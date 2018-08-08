using System;
using System.Collections.Generic;
using System.Linq;
using EVEMon.Common.Constants;
using EVEMon.Common.Data;
using EVEMon.Common.Enumerations;
using EVEMon.Common.Enumerations.CCPAPI;
using EVEMon.Common.Extensions;
using EVEMon.Common.Serialization.Eve;
using EVEMon.Common.Serialization.Settings;
using EVEMon.Common.Service;
using EVEMon.Common.Serialization.Esi;
using EVEMon.Common.Net;

namespace EVEMon.Common.Models
{
    public sealed class Contract
    {
        private readonly List<ContractItem> m_contractItems = new List<ContractItem>();
        private ContractState m_state;
        private Enum m_method;

        private string m_acceptor;
        private string m_assignee;
        private ICollection<ContractBid> m_bids;
        private ResponseParams m_bidsResponse;
        private bool m_bidsPending;
        private long m_endStationID;
        private string m_issuer;
        private bool m_itemsPending;
        private ResponseParams m_itemsResponse;
        private long m_startStationID;

        /// <summary>
        /// The maximum number of days after contract expires. Beyond this limit, we do not
        /// import contracts anymore.
        /// </summary>
        public const int MaxEndedDays = 7;


        #region Constructor

        /// <summary>
        /// Constructor from the API.
        /// </summary>
        /// <param name="ccpCharacter">The CCP character.</param>
        /// <param name="src">The source.</param>
        /// <exception cref="System.ArgumentNullException">src</exception>
        internal Contract(CCPCharacter ccpCharacter, SerializableContractListItem src)
        {
            src.ThrowIfNull(nameof(src));

            m_bidsResponse = m_itemsResponse = null;
            Character = ccpCharacter;
            PopulateContractInfo(src);
            m_state = GetState(src);
            m_bids = new LinkedList<ContractBid>();
        }

        /// <summary>
        /// Constructor from an object deserialized from the settings file.
        /// </summary>
        /// <param name="ccpCharacter">The CCP character.</param>
        /// <param name="src">The source.</param>
        internal Contract(CCPCharacter ccpCharacter, SerializableContract src)
        {
            src.ThrowIfNull(nameof(src));

            m_bidsResponse = m_itemsResponse = null;
            Character = ccpCharacter;
            ID = src.ContractID;
            IssuedFor = src.IssuedFor;
            if (IssuedFor == IssuedFor.Corporation)
                IssuerID = Character.CharacterID;
            m_state = src.ContractState;
            m_bids = new LinkedList<ContractBid>();
        }

        #endregion


        #region Properties

        /// <summary>
        /// Gets the character.
        /// </summary>
        public CCPCharacter Character { get; }

        /// <summary>
        /// Gets the ID.
        /// </summary>
        public long ID { get; private set; }

        /// <summary>
        /// Gets the issuer ID.
        /// </summary>
        public long IssuerID { get; private set; }

        /// <summary>
        /// Gets the assignee ID.
        /// </summary>
        public long AssigneeID { get; private set; }

        /// <summary>
        /// Gets the acceptor ID.
        /// </summary>
        public long AcceptorID { get; private set; }

        /// <summary>
        /// Gets the start station.
        /// </summary>
        public Station StartStation { get; private set; }

        /// <summary>
        /// Gets the end station.
        /// </summary>
        public Station EndStation { get; private set; }

        /// <summary>
        /// Gets the type of the contract.
        /// </summary>
        /// <value>
        /// The type of the contract.
        /// </value>
        public ContractType ContractType { get; private set; }

        /// <summary>
        /// Gets the status.
        /// </summary>
        public CCPContractStatus Status { get; private set; }

        /// <summary>
        /// Gets the description.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Gets the issued for.
        /// </summary>
        public IssuedFor IssuedFor { get; private set; }

        /// <summary>
        /// Gets the availability.
        /// </summary>
        public ContractAvailability Availability { get; private set; }

        /// <summary>
        /// Gets the issued.
        /// </summary>
        public DateTime Issued { get; private set; }

        /// <summary>
        /// Gets the expiration.
        /// </summary>
        public DateTime Expiration { get; private set; }

        /// <summary>
        /// Gets the accepted.
        /// </summary>
        public DateTime Accepted { get; private set; }

        /// <summary>
        /// Gets the duration.
        /// </summary>
        public int Duration { get; private set; }

        /// <summary>
        /// Gets the days to complete.
        /// </summary>
        public int DaysToComplete { get; private set; }

        /// <summary>
        /// Gets the completed.
        /// </summary>
        public DateTime Completed { get; private set; }

        /// <summary>
        /// Gets the price.
        /// </summary>
        public decimal Price { get; private set; }

        /// <summary>
        /// Gets the reward.
        /// </summary>
        public decimal Reward { get; private set; }

        /// <summary>
        /// Gets the collateral.
        /// </summary>
        public decimal Collateral { get; private set; }

        /// <summary>
        /// Gets the buyout.
        /// </summary>
        public decimal Buyout { get; private set; }

        /// <summary>
        /// Gets the volume.
        /// </summary>
        public decimal Volume { get; private set; }

        /// <summary>
        /// Gets the issuer.
        /// </summary>
        public string Issuer => m_issuer.IsEmptyOrUnknown() ? (m_issuer = EveIDToName.
            GetIDToName(IssuerID)) : m_issuer;

        /// <summary>
        /// Gets the assignee.
        /// </summary>
        public string Assignee => m_assignee.IsEmptyOrUnknown() ? (m_assignee = EveIDToName.
            GetIDToName(AssigneeID)) : m_assignee;

        /// <summary>
        /// Gets the acceptor.
        /// </summary>
        public string Acceptor => m_acceptor.IsEmptyOrUnknown() ? (m_acceptor = EveIDToName.
            GetIDToName(AcceptorID)) : m_acceptor;

        /// <summary>
        /// Gets the contract items.
        /// </summary>
        public IEnumerable<ContractItem> ContractItems => m_contractItems.Where(x => x.Item != null);

        /// <summary>
        /// Gets the contract bids.
        /// </summary>
        public IEnumerable<ContractBid> ContractBids => m_bids;

        /// <summary>
        /// Gets the contract text.
        /// </summary>
        /// <remarks>Keep this order of checking.</remarks>
        public string ContractText
        {
            get
            {
                if (ContractType == ContractType.Courier)
                {
                    string startName = StartStation.SolarSystemChecked?.Name ??
                        EveMonConstants.UnknownText;
                    string endName = EndStation.SolarSystemChecked?.Name ??
                        EveMonConstants.UnknownText;
                    return $"{startName} >> {endName} ({Math.Round(Volume)} m³)";
                }

                if (!m_contractItems.Any() || !ContractItems.Any())
                    return EveMonConstants.UnknownText;

                if (IsTrading)
                    return "[Want To Trade]";

                if (IsMultipleItems)
                    return "[Multiple Items]";

                if (IsBuyOnly)
                    return Reward == 0 ? "[Want A Gift]" : "[Want To Buy]";

                return m_contractItems.First().Item.Name + ((m_contractItems.First().
                    Quantity > 1) ? $" x {m_contractItems.First().Quantity}" : string.Empty);
            }
        }

        /// <summary>
        /// When true, the contract will be deleted unless it was found on the API feed.
        /// </summary>
        internal bool MarkedForDeletion { get; set; }

        /// <summary>
        /// Gets the contract state.
        /// </summary>
        public ContractState State
        {
            get
            {
                if (IsExpired && (m_state == ContractState.Created || m_state == ContractState.Assigned))
                    return ContractState.Expired;

                return m_state;
            }
        }

        /// <summary>
        /// Gets true if the contract is of "buy only" type.
        /// </summary>
        public bool IsBuyOnly => m_contractItems.All(x => !x.Included);

        /// <summary>
        /// Gets true if the contract is of "multiple items" type.
        /// </summary>
        public bool IsMultipleItems => m_contractItems.Count > 1 && m_contractItems.All(x => x.Included);

        /// <summary>
        /// Gets true if the contract is of "trading" type.
        /// </summary>
        public bool IsTrading
            => m_contractItems.Count > 1 && m_contractItems.Any(x => x.Included) && m_contractItems.Any(x => !x.Included);

        /// <summary>
        /// Gets true if the contract is not finished, canceled, expired, etc.
        /// </summary>
        public bool IsAvailable => (m_state == ContractState.Created || m_state == ContractState.Assigned) && !IsExpired;

        /// <summary>
        /// Gets true if the contract is expired or rejected but not yet deleted (needs attention).
        /// </summary>
        public bool NeedsAttention => m_state == ContractState.Expired || m_state == ContractState.Rejected || Overdue;

        /// <summary>
        /// Gets true if contract completion is ovedue.
        /// </summary>
        public bool Overdue => Status == CCPContractStatus.Overdue || (Status ==
            CCPContractStatus.InProgress && Accepted.AddDays(DaysToComplete) < DateTime.UtcNow);

        /// <summary>
        /// Gets true if contract naturally expired because of its duration.
        /// </summary>
        public bool IsExpired => Expiration < DateTime.UtcNow;

        /// <summary>
        /// Gets or sets a value indicating whether a notification for this contract have been send.
        /// </summary>
        /// <value><c>true</c> if a notification for this contract have been send; otherwise, <c>false</c>.</value>
        public bool NotificationSend { get; set; }

        #endregion


        #region Importation, Exportation

        /// <summary>
        /// Try to update this contract with a serialization object from the API.
        /// </summary>
        /// <param name="src">The serializable source.</param>
        /// <param name="endedContracts">The ended contracts.</param>
        /// <returns>True if import successful otherwise, false.</returns>
        internal bool TryImport(SerializableContractListItem src, ICollection<Contract> endedContracts)
        {
            // Note that, before a match is found, all contracts have been marked for deletion:
            // m_markedForDeletion == true
            bool matches = MatchesWith(src);
            if (matches)
            {
                // Prevent deletion
                MarkedForDeletion = false;
                // Contract is from a serialized object, so populate the missing info
                if (IssuerID == 0L || Issuer.IsEmptyOrUnknown())
                    PopulateContractInfo(src);
                // Update if modified
                ContractState state = GetState(src);
                if (state != m_state || NeedsAttention)
                {
                    if (state != m_state || Overdue)
                    {
                        m_state = state;
                        UpdateContractInfo(src);
                    }
                    // If the contract has just finished or expired, add it to the ended list
                    if (NeedsAttention || state == ContractState.Finished)
                        endedContracts.Add(this);
                }
            }
            return matches;
        }

        /// <summary>
        /// Populates the serialization object contract with the info from the API.
        /// </summary>
        /// <param name="src">The source.</param>
        /// <exception cref="System.ArgumentNullException">src</exception>
        private void PopulateContractInfo(SerializableContractListItem src)
        {
            ContractAvailability avail;
            ContractType type;
            src.ThrowIfNull(nameof(src));

            m_method = src.APIMethod;
            ID = src.ContractID;
            IssuerID = src.IssuerID;
            AssigneeID = src.AssigneeID;
            Description = src.Title.IsEmptyOrUnknown() ? "(None)" : src.Title;
            IssuedFor = src.ForCorp ? IssuedFor.Corporation : IssuedFor.Character;
            Issued = src.DateIssued;
            Expiration = src.DateExpired;
            Duration = (int)Math.Round(src.DateExpired.Subtract(src.DateIssued).TotalDays);
            DaysToComplete = src.NumDays;
            Price = src.Price;
            Reward = src.Reward;
            Collateral = src.Collateral;
            Buyout = src.Buyout;
            Volume = src.Volume;
            m_startStationID = src.StartStationID;
            m_endStationID = src.EndStationID;
            UpdateStation();
            UpdateContractInfo(src);

            // Parse availability and contract type
            if (!Enum.TryParse(src.Availability, true, out avail))
                avail = ContractAvailability.None;
            Availability = avail;
            if (!Enum.TryParse(src.Type, true, out type))
                type = ContractType.None;
            ContractType = type;
            // Issuer and assignee
            m_issuer = src.ForCorp ? Character.Corporation.Name : EveIDToName.GetIDToName(
                src.IssuerID);
            m_assignee = EveIDToName.GetIDToName(src.AssigneeID);
        }

        /// <summary>
        /// Updates the contract info.
        /// </summary>
        /// <param name="src">The source.</param>
        private void UpdateContractInfo(SerializableContractListItem src)
        {
            Accepted = src.DateAccepted;
            Completed = src.DateCompleted;
            AcceptorID = src.AcceptorID;
            m_acceptor = EveIDToName.GetIDToName(src.AcceptorID);
            Status = GetStatus(src);
            if (Overdue)
                Status = CCPContractStatus.Overdue;
        }

        /// <summary>
        /// Imports the contract items to a list.
        /// </summary>
        /// <param name="src">The source.</param>
        private void Import(IEnumerable<EsiContractItemsListItem> src)
        {
            foreach (EsiContractItemsListItem item in src)
                m_contractItems.Add(new ContractItem(item));
        }

        /// <summary>
        /// Imports the contract bids to a list.
        /// </summary>
        /// <param name="src">The source.</param>
        internal void Import(IEnumerable<EsiContractBidsListItem> src)
        {
            m_bids.Clear();
            // Add new bids to collection
            foreach (EsiContractBidsListItem item in src)
                m_bids.Add(new ContractBid(item));
        }

        /// <summary>
        /// Exports the given object to a serialization object.
        /// </summary>
        /// <returns></returns>
        internal SerializableContract Export() => new SerializableContract
        {
            ContractID = ID,
            ContractState = m_state,
            IssuedFor = IssuedFor
        };

        #endregion


        #region Querying Methods

        /// <summary>
        /// Gets the contract items or bids.
        /// </summary>
        private void GetContractData<T, U>(APIProvider.ESIRequestCallback<T> callback,
            ESIAPICorporationMethods methodCorp, ESIAPICharacterMethods methodPersonal,
            ResponseParams response) where T : List<U> where U : class
        {
            var cid = Character.Identity;
            ESIKey key;
            Enum method;
            long owner;
            // Special condition to identify corporation contracts in character query
            if (IssuedFor == IssuedFor.Corporation && ESIAPICorporationMethods.
                CorporationContracts.Equals(m_method))
            {
                key = cid.FindAPIKeyWithAccess(methodCorp);
                method = methodCorp;
                owner = Character.CorporationID;
            }
            else
            {
                key = cid.FindAPIKeyWithAccess(methodPersonal);
                method = methodPersonal;
                owner = Character.CharacterID;
            }
            if (key != null)
                EveMonClient.APIProviders.CurrentProvider.QueryPagedEsi<T, U>(method, callback,
                    new ESIParams(response, key.AccessToken)
                    {
                        ParamOne = owner,
                        ParamTwo = ID
                    }, method);
        }

        /// <summary>
        /// Processes the contract items.
        /// </summary>
        /// <param name="result">The result.</param>
        private void OnContractItemsDownloaded(EsiResult<EsiAPIContractItems> result,
            object apiMethod)
        {
            var methodEnum = (apiMethod as Enum) ?? ESIAPICharacterMethods.ContractItems;
            var target = Character;
            m_itemsResponse = result.Response;
            // Notify if an error occured
            if (target.ShouldNotifyError(result, methodEnum))
                EveMonClient.Notifications.NotifyContractItemsError(target, result);
            if (!result.HasError && result.HasData)
            {
                EveMonClient.Notifications.InvalidateCharacterAPIError(target);
                Import(result.Result);
                // Fire correct event type
                if (methodEnum is ESIAPICharacterMethods)
                    EveMonClient.OnCharacterContractItemsDownloaded(target);
                else
                    EveMonClient.OnCorporationContractItemsDownloaded(target);
            }
            m_itemsPending = false;
        }

        /// <summary>
        /// Processes the contract bids.
        /// </summary>
        private void OnContractBidsUpdated(EsiResult<EsiAPIContractBids> result, object
            apiMethod)
        {
            var methodEnum = (apiMethod as Enum) ?? ESIAPICharacterMethods.ContractBids;
            var target = Character;
            m_bidsResponse = result.Response;
            // Notify if an error occured
            if (target.ShouldNotifyError(result, methodEnum))
                EveMonClient.Notifications.NotifyContractBidsError(Character, result);
            if (!result.HasError)
            {
                EveMonClient.Notifications.InvalidateCharacterAPIError(target);
                Import(result.Result);
                // Fire correct event type
                if (methodEnum is ESIAPICharacterMethods)
                    EveMonClient.OnCharacterContractBidsDownloaded(target);
                else
                    EveMonClient.OnCorporationContractBidsDownloaded(target);
            }
            m_bidsPending = false;
        }

        #endregion


        #region Public Methods

        /// <summary>
        /// Updates the station.
        /// </summary>
        public void UpdateStation()
        {
            StartStation = EveIDToStation.GetIDToStation(m_startStationID, Character);
            EndStation = EveIDToStation.GetIDToStation(m_endStationID, Character);
        }

        /// <summary>
        /// Fetches the contract's items and bids.
        /// </summary>
        public void UpdateContractItems()
        {
            // Retrieve items
            if (ContractType != ContractType.Courier && m_contractItems.Count < 1 &&
                !m_itemsPending)
            {
                m_itemsPending = true;
                GetContractData<EsiAPIContractItems, EsiContractItemsListItem>(
                    OnContractItemsDownloaded, ESIAPICorporationMethods.
                    CorporationContractItems, ESIAPICharacterMethods.ContractItems,
                    m_itemsResponse);
            }
            if (ContractType == ContractType.Auction && !m_bidsPending)
            {
                m_bidsPending = true;
                GetContractData<EsiAPIContractBids, EsiContractBidsListItem>(
                    OnContractBidsUpdated, ESIAPICorporationMethods.CorporationContractBids,
                    ESIAPICharacterMethods.ContractBids, m_bidsResponse);
            }
        }

        #endregion


        #region Helper Methods

        /// <summary>
        /// Checks whether the given API object matches with this contract.
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        private bool MatchesWith(SerializableContractListItem src) => src.ContractID == ID;

        /// <summary>
        /// Gets the state of a contract.
        /// </summary>
        /// <param name="src">The source.</param>
        /// <returns></returns>
        private ContractState GetState(SerializableContractListItem src)
        {
            CCPContractStatus status = GetStatus(src);

            if (status == CCPContractStatus.Outstanding && IsExpired)
                return ContractState.Expired;

            switch (status)
            {
                case CCPContractStatus.Failed:
                    return ContractState.Failed;
                case CCPContractStatus.Canceled:
                    return ContractState.Canceled;
                case CCPContractStatus.Overdue:
                case CCPContractStatus.Outstanding:
                case CCPContractStatus.InProgress:
                    return IssuerID != Character.CharacterID ? ContractState.Assigned : ContractState.Created;
                case CCPContractStatus.Completed:
                case CCPContractStatus.CompletedByContractor:
                case CCPContractStatus.CompletedByIssuer:
                    return ContractState.Finished;
                case CCPContractStatus.Rejected:
                    return ContractState.Rejected;
                case CCPContractStatus.Deleted:
                    return ContractState.Deleted;
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets the contract status.
        /// </summary>
        /// <param name="src">The source.</param>
        /// <returns></returns>
        private static CCPContractStatus GetStatus(SerializableContractListItem src)
        {
            CCPContractStatus status;
            if (!Enum.TryParse(src.Status ?? string.Empty, true, out status))
                status = CCPContractStatus.None;
            return status;
        }

        #endregion
    }
}
