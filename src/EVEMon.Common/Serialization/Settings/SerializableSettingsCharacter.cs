using System;
using System.Xml.Serialization;
using EVEMon.Common.Serialization.Eve;

namespace EVEMon.Common.Serialization.Settings
{
    /// <summary>
    /// Represents a base for character serialization in the settings.
    /// </summary>
    public class SerializableSettingsCharacter : SerializableCharacterSheetBase
    {
        [XmlAttribute("guid")]
        public Guid Guid { get; set; }

        [XmlAttribute("label")]
        public string Label { get; set; }

        [XmlElement("implants")]
        public SerializableImplantSetCollection ImplantSets { get; set; }
    }
}
