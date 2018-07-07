using System.Runtime.Serialization;

namespace EVEMon.Common.Serialization.Esi
{
    [DataContract]
    public sealed class EsiAssetListItem : EsiLocationBase
    {
        [DataMember(Name = "item_id")]
        public long ItemID { get; set; }

        [DataMember(Name = "type_id")]
        public int TypeID { get; set; }

        // Maximum stack size is int32
        [DataMember(Name = "quantity")]
        public int Quantity { get; set; }

        [DataMember(Name = "location_flag")]
        public string EVEFlag { get; set; }

        [DataMember(Name = "is_singleton")]
        public bool Singleton { get; set; }

        [DataMember(Name = "is_blueprint_copy", IsRequired = false, EmitDefaultValue = false)]
        public bool IsBPC { get; set; }
        
        // LocationID:
        // <60 000 000 = solar system in space
        // 60 014 861..60 014 928 = immensea
        // 61 000 000..66 000 000 = outpost
        // 66 000 000..67 000 000 = station, subtract 6 000 001 for station ID
        // 67 000 000..68 000 000 = outpost, subtract 6 000 000 for station ID
        // >68 000 000 = citadel
    }
}
