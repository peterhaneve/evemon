﻿using EVEMon.XmlGenerator.Interfaces;
using System.Xml.Serialization;

namespace EVEMon.XmlGenerator.StaticData
{
    public sealed class IndustryActivity : IRelation
	{
		[XmlElement("typeID")]
        public int BlueprintTypeID { get; set; }

        [XmlElement("activityID")]
		public int ActivityID { get; set; }

        [XmlElement("time")]
		public int? Time { get; set; }

        #region IRelation Members

        /// <summary>
        /// Gets the left column value.
        /// </summary>
        /// <value>The left.</value>
        int IRelation.Left => BlueprintTypeID;

        /// <summary>
        /// Gets the center column value.
        /// </summary>
        /// <value>
        /// The center.
        /// </value>
        int IRelation.Center => 0;

        /// <summary>
        /// Gets the right column value.
        /// </summary>
        /// <value>The right.</value>
        int IRelation.Right => ActivityID;

        #endregion
    }
}
