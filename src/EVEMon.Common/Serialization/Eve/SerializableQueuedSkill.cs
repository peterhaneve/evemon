using System;
using System.Xml.Serialization;
using EVEMon.Common.Extensions;

namespace EVEMon.Common.Serialization.Eve
{
    public sealed class SerializableQueuedSkill : ISynchronizableWithLocalClock
    {
        [XmlAttribute("typeID")]
        public int ID { get; set; }

        [XmlAttribute("level")]
        public int Level { get; set; }

        [XmlAttribute("startSP")]
        public int StartSP { get; set; }

        [XmlAttribute("endSP")]
        public int EndSP { get; set; }

        [XmlAttribute("startTime")]
        public string CCPStartTime { get; set; }

        [XmlAttribute("endTime")]
        public string CCPEndTime { get; set; }

        [XmlIgnore]
        public DateTime StartTime
        {
            get { return CCPStartTime.TimeStringToDateTime(); }
            set { CCPStartTime = value.DateTimeToTimeString(); }
        }

        [XmlIgnore]
        public DateTime EndTime
        {
            get { return CCPEndTime.TimeStringToDateTime(); }
            set { CCPEndTime = value.DateTimeToTimeString(); }
        }

        // When the skill queue is paused, startTime and endTime are empty in the XML document
        // As a result, the serialization leaves the DateTime with its default value
        [XmlIgnore]
        public bool IsTraining
        {
            get { return EndTime != DateTime.MinValue; }
        }

        #region ISynchronizableWithLocalClock Members

        /// <summary>
        /// Synchronizes the stored times with local clock
        /// </summary>
        /// <param name="drift"></param>
        void ISynchronizableWithLocalClock.SynchronizeWithLocalClock(TimeSpan drift)
        {
            if (!String.IsNullOrEmpty(CCPStartTime))
                StartTime -= drift;

            if (!String.IsNullOrEmpty(CCPEndTime))
                EndTime -= drift;
        }

        #endregion
    }
}