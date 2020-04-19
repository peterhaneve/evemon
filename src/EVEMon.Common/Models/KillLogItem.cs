using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using EVEMon.Common.Constants;
using EVEMon.Common.Data;
using EVEMon.Common.Enumerations;
using EVEMon.Common.Extensions;
using EVEMon.Common.Helpers;
using EVEMon.Common.Serialization.Eve;
using EVEMon.Common.Service;

namespace EVEMon.Common.Models
{
    public class KillLogItem
    {
        /// <summary>
        /// Occurs when kill log item image updated.
        /// </summary>
        public event EventHandler KillLogItemImageUpdated;


        #region Fields

        private readonly List<KillLogItem> m_items = new List<KillLogItem>();
        private readonly int m_typeID;
        private Image m_image;

        #endregion


        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="KillLogItem"/> class.
        /// </summary>
        /// <param name="src">The source.</param>
        /// <param name="isInContainer">if set to <c>true</c> item is in container.</param>
        internal KillLogItem(SerializableKillLogItemListItem src, bool isInContainer = false)
        {
            m_typeID = src.TypeID;
            EVEFlag = src.EVEFlag;
            QtyDestroyed = src.QtyDestroyed;
            QtyDropped = src.QtyDropped;
            Singleton = src.Singleton;
            Item = StaticItems.GetItemByID(src.TypeID);

            FittingContentGroup = GetFittingContentGroup();
            IsInContainer = isInContainer;

            m_items.AddRange(src.Items.Select(item => new KillLogItem(item, true)));
        }

        #endregion


        #region Properties

        /// <summary>
        /// Gets the EVE flag.
        /// </summary>
        public short EVEFlag { get; }

        /// <summary>
        /// Gets the qty dropped.
        /// </summary>
        public int QtyDropped { get; }

        /// <summary>
        /// Gets the qty destroyed.
        /// </summary>
        public int QtyDestroyed { get; }

        /// <summary>
        /// Gets the singleton.
        /// </summary>
        public byte Singleton { get; }

        /// <summary>
        /// Gets a value indicating whether the item is in a container.
        /// </summary>
        public bool IsInContainer { get; }

        /// <summary>
        /// Gets the fitting content group.
        /// </summary>
        public KillLogFittingContentGroup FittingContentGroup { get; }

        /// <summary>
        /// Gets the items.
        /// </summary>
        public IEnumerable<KillLogItem> Items => m_items;

        /// <summary>
        /// Gets the price.
        /// </summary>
        public double Price => Settings.MarketPricer.Pricer != null
            ? Settings.MarketPricer.Pricer.GetPriceByTypeID(m_typeID)
            : 0;

        /// <summary>
        /// Gets the inventory text.
        /// </summary>
        public string InventoryText
        {
            get
            {
                switch (FittingContentGroup)
                {
                    case KillLogFittingContentGroup.Implant:
                    case KillLogFittingContentGroup.DroneBay:
                    case KillLogFittingContentGroup.Cargo:
                        return EveFlag.GetFlagText(EVEFlag);
                    default:
                        return string.Empty;
                }
            }
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public Item Item { get; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name => Item?.Name ?? EveMonConstants.UnknownText;

        /// <summary>
        /// Gets the victim image.
        /// </summary>
        public Image ItemImage
        {
            get
            {
                if (m_image != null)
                    return m_image;

                GetItemImageAsync().ConfigureAwait(false);

                return m_image ?? (m_image = GetDefaultImage());
            }
        }

        #endregion


        #region Helper Methods

        /// <summary>
        /// Exports this object to a serializable form.
        /// </summary>
        /// <returns>The SerializableKillLogItemListItem representing this object.</returns>
        public SerializableKillLogItemListItem Export()
        {
            var exported = new SerializableKillLogItemListItem()
            {
                EVEFlag = EVEFlag,
                QtyDestroyed = QtyDestroyed,
                QtyDropped = QtyDropped,
                Singleton = Singleton,
                TypeID = m_typeID
            };
            // Recursively export contained items
            foreach (var item in m_items)
                exported.Items.Add(item.Export());
            return exported;
        }

        /// <summary>
        /// Gets the fiiting and content group.
        /// </summary>
        /// <returns></returns>
        private KillLogFittingContentGroup GetFittingContentGroup()
        {
            switch (EVEFlag)
            {
                case 0:
                    return KillLogFittingContentGroup.Other;
                case 5:
                    return KillLogFittingContentGroup.Cargo;
                case 11:
                case 12:
                case 13:
                case 14:
                case 15:
                case 16:
                case 17:
                case 18:
                    return KillLogFittingContentGroup.LowSlot;
                case 19:
                case 20:
                case 21:
                case 22:
                case 23:
                case 24:
                case 25:
                case 26:
                    return KillLogFittingContentGroup.MediumSlot;
                case 27:
                case 28:
                case 29:
                case 30:
                case 31:
                case 32:
                case 33:
                case 34:
                    return KillLogFittingContentGroup.HighSlot;
                case 87:
                    return KillLogFittingContentGroup.DroneBay;
                case 88:
                    return KillLogFittingContentGroup.Booster;
                case 89:
                    return KillLogFittingContentGroup.Implant;
                case 92:
                case 93:
                case 94:
                case 95:
                case 96:
                case 97:
                case 98:
                case 99:
                    return KillLogFittingContentGroup.RigSlot;
                case 125:
                case 126:
                case 127:
                case 128:
                case 129:
                case 130:
                case 131:
                case 132:
                    return KillLogFittingContentGroup.SubsystemSlot;
                default:
                    return KillLogFittingContentGroup.None;
            }
        }

        /// <summary>
        /// Gets the item image.
        /// </summary>
        private async Task GetItemImageAsync()
        {
            Uri uri = ImageHelper.GetTypeImageURL(m_typeID);
            Image img = await ImageService.GetImageAsync(uri).ConfigureAwait(false);
            if (img != null) {
                m_image = img;
                // Notify the subscriber that we got the image
                KillLogItemImageUpdated?.ThreadSafeInvoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets the default image.
        /// </summary>
        /// <returns></returns>
        private static Bitmap GetDefaultImage() => new Bitmap(24, 24);

        #endregion
    }
}
