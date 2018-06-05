using System.Xml.Serialization;

namespace EVEMon.Common.Serialization.EveCentral.MarketPricer
{
    public sealed class SerializableECItemPriceListItem
    {
        [XmlAttribute("id")]
        public int ID { get; set; }

        [XmlElement("sell")]
        public SerializableECItemPriceItem Prices { get; set; }
    }
}
