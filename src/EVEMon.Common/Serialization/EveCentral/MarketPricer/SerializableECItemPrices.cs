using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace EVEMon.Common.Serialization.EveCentral.MarketPricer
{
    [XmlRoot("exec_api")]
    public sealed class SerializableECItemPrices
    {
        private readonly Collection<SerializableECItemPriceListItem> m_itemPrices;

        public SerializableECItemPrices()
        {
            m_itemPrices = new Collection<SerializableECItemPriceListItem>();
        }

        [XmlArray("marketstat")]
        [XmlArrayItem("type")]
        public Collection<SerializableECItemPriceListItem> ItemPrices => m_itemPrices;
    }
}
