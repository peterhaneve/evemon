using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using EVEMon.Common.Constants;
using EVEMon.Common.Data;
using EVEMon.Common.Extensions;
using EVEMon.Common.Service;
using YamlDotNet.RepresentationModel;

namespace EVEMon.Common.Models.Extended
{
    /// <summary>
    /// A class for the internal parser of the EVE notification text.
    /// </summary>
    /// <remarks>
    /// This class is called when the fetching of the external parser fails for any reason.
    /// </remarks>
    internal class InternalEveNotificationTextParser : EveNotificationTextParser
    {
        /// <summary>
        /// Parses the implant list from a lost clone.
        /// </summary>
        /// <param name="typeIDs">The type IDs of lost implants</param>
        /// <returns>A string representation of the implants</returns>
        private static string BuildImplantList(YamlSequenceNode typeIDs)
        {
            string implants;
            if (!typeIDs.Any())
                implants = "None were in the clone";
            else
            {
                int type;
                var sb = new StringBuilder(256);
                // Add all valid implants to the string
                foreach (var typeID in typeIDs)
                    if (int.TryParse(typeID.ToString(), out type))
                        sb.AppendLine().AppendLine("Type: " + StaticItems.GetItemName(type));
                implants = sb.ToString();
            }
            return implants;
        }

        /// <summary>
        /// Parses the item list from a delivery. The format is an array of 2-element arrays,
        /// where element 0 is the quantity and element 1 is the type ID.
        /// </summary>
        /// <param name="typeIDs">The type IDs and quantities of delivered items</param>
        /// <returns></returns>
        private static string BuildItemList(YamlSequenceNode typeIDs)
        {
            string items;
            if (!typeIDs.Any())
                items = "No items were delivered";
            else
            {
                int type, qty;
                var sb = new StringBuilder(512);
                // Add all valid types to the string
                foreach (var typeAndQty in typeIDs)
                {
                    // Convert to array, proceed only if successful and has 2 elements
                    var array = (typeAndQty as YamlSequenceNode)?.Children;
                    if (array?.Count == 2 && int.TryParse(array[0].ToString(), out qty) &&
                            qty > 0 && int.TryParse(array[1].ToString(), out type) && type > 0)
                        sb.AppendLine(string.Format("{0}x {1}", qty.ToNumericString(0),
                            StaticItems.GetItemName(type)));
                }
                items = sb.ToString();
            }
            return items;
        }

        /// <summary>
        /// Parses the notification text.
        /// </summary>
        /// <param name="notification">The notification.</param>
        /// <param name="pair">The pair.</param>
        /// <param name="parsedDict">The parsed dictionary.</param>
        public override void Parse(EveNotification notification, KeyValuePair<YamlNode,
            YamlNode> pair, IDictionary<string, string> parsedDict)
        {
            string key = pair.Key.ToString(), value = pair.Value.ToString();
            long valueAsLong;
            decimal amount;
            double valueAsDouble;
            int typeID = notification.TypeID;
            DateTime timestamp = notification.SentDate;
            YamlSequenceNode typeIDs;
            // The value is often used as an int64 in the list below, simplify calculation
            if (!long.TryParse(value, out valueAsLong))
                valueAsLong = 0L;
            switch (key.ToUpperInvariant())
            {
            case "CHARID":
            case "SENDERCHARID":
            case "RECEIVERCHARID":
            case "OWNERID":
            case "LOCATIONOWNERID":
            case "DESTROYERID":
            case "INVOKINGCHARID":
            case "PODKILLERID":
            case "NEWCEOID":
            case "OLDCEOID":
            case "CORPID":
            case "VICTIMID":
            case "DECLAREDBYID":
            case "AGAINSTID":
            case "CREDITORID":
            case "FACTIONID":
            case "DEFENDERID":
            case "ENEMYID":
            case "AGGRESSORID":
            case "ALLYID":
            case "MERCID":
            case "AGGRESSORCORPID":
            case "AGGRESSORALLIANCEID":
                parsedDict[key] = EveIDToName.GetIDToName(valueAsLong);
                break;
            case "CLONESTATIONID":
            case "CORPSTATIONID":
            case "LOCATIONID":
            case "STRUCTUREID":
            case "EXTERNALID2":
                parsedDict[key] = EveIDToStation.GetIDToStation(valueAsLong)?.Name ??
                    EveMonConstants.UnknownText;
                break;
            case "SOLARSYSTEMID":
                // If it overflows the result will be invalid anyways
                parsedDict[key] = StaticGeography.GetSolarSystemName((int)valueAsLong);
                break;
            case "SHIPTYPEID":
            case "TYPEID":
            case "STRUCTURETYPEID":
            case "VICTIMSHIPTYPEID":
                // If it overflows the result will be invalid anyways
                parsedDict[key] = StaticItems.GetItemName((int)valueAsLong);
                break;
            case "MEDALID":
                var medal = notification.CCPCharacter.CharacterMedals.FirstOrDefault(x =>
                    (x.ID.ToString() == value));
                parsedDict[key] = medal?.Title ?? EveMonConstants.UnknownText;
                parsedDict.Add("medalDescription", medal?.Description ??
                    EveMonConstants.UnknownText);
                break;
            case "AMOUNT":
            case "ISKVALUE":
                // Format as ISK amount
                if (decimal.TryParse(value,NumberStyles.Number, CultureInfo.InvariantCulture, out amount))
                    parsedDict[key] = amount.ToString("N2");
                break;
            case "ENDDATE":
            case "STARTDATE":
            case "DECLOAKTIME":
            case "DESTRUCTTIME":
            case "TIMEFINISHED":
                parsedDict[key] = string.Format(CultureConstants.InvariantCulture,
                    "{0:dddd, MMMM d, yyyy HH:mm} (EVE Time)", valueAsLong.
                    WinTimeStampToDateTime());
                break;
            case "NOTIFICATION_CREATED":
                parsedDict[key] = string.Format(CultureConstants.InvariantCulture,
                    "{0:dddd, MMMM d, yyyy HH:mm} (EVE Time)", timestamp);
                break;
            case "DUEDATE":
            case "ISSUEDATE":
                parsedDict[key] = string.Format(CultureConstants.InvariantCulture,
                    "{0:dddd, MMMM d, yyyy} (EVE Time)", valueAsLong.WinTimeStampToDateTime());
                break;
            case "CAMPAIGNEVENTTYPE":
                switch (value)
                {
                case "1":
                    parsedDict[key] = "Territorial Claim Unit";
                    break;
                case "2":
                    parsedDict[key] = "Infrastructure Hub";
                    break;
                case "3":
                    parsedDict[key] = "Station";
                    break;
                default:
                    parsedDict[key] = EveMonConstants.UnknownText;
                    break;
                }
                break;
            case "TYPEIDS":
                typeIDs = pair.Value as YamlSequenceNode;
                if (typeIDs != null && (typeID == 56 || typeID == 57))
                    parsedDict[key] = BuildImplantList(typeIDs);
                break;
            case "LISTOFTYPESANDQTY":
                typeIDs = pair.Value as YamlSequenceNode;
                if (typeIDs != null)
                    parsedDict[key] = BuildItemList(typeIDs);
                break;
            case "ISHOUSEWARMINGGIFT":
                if (Convert.ToBoolean(pair.Value) && typeID == 34)
                    // Tritanium
                    parsedDict[key] = StaticItems.GetItemName(typeID);
                break;
            case "LEVEL":
                if (double.TryParse(value, out valueAsDouble))
                    parsedDict[key] = Standing.Status(valueAsDouble) + " Standing";
                break;
            case "SHIELDVALUE":
            case "ARMORVALUE":
            case "HULLVALUE":
                if (double.TryParse(value, out valueAsDouble))
                    parsedDict[key] = (valueAsDouble * 100.0).ToString("N0");
                break;
            }
        }
    }
}
