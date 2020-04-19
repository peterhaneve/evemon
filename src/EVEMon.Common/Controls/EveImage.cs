using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using EVEMon.Common.Constants;
using EVEMon.Common.Data;
using EVEMon.Common.Enumerations;
using EVEMon.Common.Factories;
using EVEMon.Common.Service;

namespace EVEMon.Common.Controls
{
    /// <summary>
    /// Displays an image for a given EveObject.
    /// </summary>
    /// <remarks>
    /// Setting the PopUpEnabled property to true enables a pop-up
    /// window for EveObjects with a 256 x 256 image available, accessed
    /// via the user double-clicking the image. Image size must be
    /// set using the ImageSize property. The default Size property is
    /// overriden.
    /// </remarks>
    public partial class EveImage : UserControl
    {
        private readonly Timer m_timer = new Timer();
        private MouseEventArgs m_mouseEvent;

        /// <summary>
        /// Holds configuration data for different image types.
        /// </summary>
        private Dictionary<ImageType, ImageTypeData> m_imageTypeAttributes;

        private bool m_popUpActive;
        private EveImageSize m_imageSize;
        private EveImageSizeMode m_sizeMode;
        private Item m_item;


        #region Constructor

        /// <summary>
        /// Initialize the control.
        /// </summary>
        /// <remarks>
        /// The default image size is 64 x 64, with the image pop-up enabled.
        /// </remarks>
        public EveImage()
        {
            InitializeComponent();
            SetImageTypeAttributes();
            ImageSize = EveImageSize.x64;
            PopUpEnabled = true;
            ShowBlankImage();

            m_timer.Interval = 500;
            m_timer.Tick += m_timer_Tick;
        }

        #endregion


        #region Public Properties

        /// <summary>
        /// Gets or sets the state of the pop-up ability.
        /// </summary>
        public bool PopUpEnabled { get; set; }

        /// <summary>
        /// Gets or sets the item to display an image for.
        /// </summary>
        [Browsable(false), ReadOnly(true)]
        public Item EveItem
        {
            get { return m_item; }
            set
            {
                m_item = value;
                if (m_imageSize != EveImageSize.x0)
                   GetImageAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Gets or sets the size of the image in eve parlance.
        /// </summary>
        public EveImageSize ImageSize
        {
            get { return m_imageSize; }
            set
            {
                m_imageSize = value;

                if (m_sizeMode != EveImageSizeMode.AutoSize)
                    return;

                pbImage.Size = new Size((int)m_imageSize, (int)m_imageSize);
                Size = pbImage.Size;
            }
        }

        /// <summary>
        /// Gets or sets the size of the image in pixels.
        /// </summary>
        public new Size Size
        {
            get { return base.Size; }
            set
            {
                base.Size = m_sizeMode == EveImageSizeMode.AutoSize
                    ? new Size((int)m_imageSize, (int)m_imageSize)
                    : value;
            }
        }

        /// <summary>
        /// Gets or sets the size mode.
        /// </summary>
        /// <value>The size mode.</value>
        public EveImageSizeMode SizeMode
        {
            get { return m_sizeMode; }
            set
            {
                m_sizeMode = value;

                switch (value)
                {
                    case EveImageSizeMode.Normal:
                        pbImage.SizeMode = PictureBoxSizeMode.Normal;
                        break;
                    case EveImageSizeMode.AutoSize:
                        pbImage.Size = new Size((int)m_imageSize, (int)m_imageSize);
                        Size = pbImage.Size;
                        pbImage.SizeMode = PictureBoxSizeMode.AutoSize;
                        break;
                    case EveImageSizeMode.StretchImage:
                        pbImage.SizeMode = PictureBoxSizeMode.StretchImage;
                        break;
                }
            }
        }

        #endregion


        #region Private Methods

        /// <summary>
        /// Builds the m_ImageTypeAttributes dictionary.
        /// </summary>
        private void SetImageTypeAttributes()
        {
            m_imageTypeAttributes = new Dictionary<ImageType, ImageTypeData>();

            // Ships
            ArrayList validSizes = new ArrayList { EveImageSize.x32, EveImageSize.x64, EveImageSize.x128, EveImageSize.x256 };
            m_imageTypeAttributes.Add(ImageType.Ship,
                new ImageTypeData("Ships", "icons", ImageNameFrom.TypeID, validSizes));

            // Items
            validSizes = new ArrayList { EveImageSize.x16, EveImageSize.x32, EveImageSize.x64, EveImageSize.x128 };
            m_imageTypeAttributes.Add(ImageType.Item,
                new ImageTypeData("Items", "icons", ImageNameFrom.Icon, validSizes));

            // Drones
            validSizes = new ArrayList { EveImageSize.x32, EveImageSize.x64, EveImageSize.x128, EveImageSize.x256 };
            m_imageTypeAttributes.Add(ImageType.Drone,
                new ImageTypeData("Drones", "icons", ImageNameFrom.TypeID, validSizes));

            // Structures
            validSizes = new ArrayList { EveImageSize.x32, EveImageSize.x64, EveImageSize.x128, EveImageSize.x256 };
            m_imageTypeAttributes.Add(ImageType.Structure,
                new ImageTypeData("", "icons", ImageNameFrom.TypeID, validSizes));

            // Blueprints
            validSizes = new ArrayList { EveImageSize.x64 };
            m_imageTypeAttributes.Add(ImageType.Blueprint,
                new ImageTypeData("Blueprints", "icons", ImageNameFrom.TypeID, validSizes));
        }

        /// <summary>
        /// Gets the type of the image.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        private static ImageType GetImageType(Item item)
        {
            switch (item.Family)
            {
                case ItemFamily.Ship:
                    return ImageType.Ship;
                case ItemFamily.Drone:
                    return ImageType.Drone;
                case ItemFamily.StarbaseStructure:
                    return ImageType.Structure;
                case ItemFamily.Blueprint:
                    return ImageType.Blueprint;
                default:
                    return ImageType.Item;
            }
        }

        /// <summary>
        /// Renders a BackColor square as a placeholder for the image.
        /// </summary>
        private void ShowBlankImage()
        {
            Bitmap bmp;
            using (Bitmap tempBitmap = new Bitmap(pbImage.ClientSize.Width, pbImage.ClientSize.Height))
            {
                bmp = (Bitmap)tempBitmap.Clone();
            }

            using (Graphics g = Graphics.FromImage(bmp))
            using (SolidBrush brush = new SolidBrush(BackColor))
            {
                g.FillRectangle(brush, new Rectangle(0, 0, bmp.Width, bmp.Height));
            }

            pbImage.Image = bmp;
        }

        /// <summary>
        /// Retrieves image for the given EveObject.
        /// </summary>
        private async Task GetImageAsync()
        {
            // Reset flags and cursor
            m_popUpActive = false;
            toolTip.Active = false;
            pbImage.Cursor = Cursors.Default;

            if (m_item == null)
                return;

            ImageType imageType = GetImageType(m_item);
            ImageTypeData typeData = m_imageTypeAttributes[imageType];

            // Only display an image if the correct size is available
            if (!typeData.ValidSizes.Contains(m_imageSize))
                return;

            // Enable pop up if required
            if (PopUpEnabled && typeData.ValidSizes.Contains(EveImageSize.x256))
            {
                toolTip.Active = true;
                m_popUpActive = true;
                pbImage.Cursor = Cursors.Hand;
            }

            await GetImageFromCCPAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the image from CCP's image server.
        /// </summary>
        private async Task GetImageFromCCPAsync()
        {
            while (true)
            {
                Image img = await ImageService.GetImageAsync(GetImageUrl()).ConfigureAwait(false);
                GotImage(m_item.ID, img);
                break;
            }
        }

        /// <summary>
        /// Gets the image URL.
        /// </summary>
        /// <returns></returns>
        private Uri GetImageUrl()
        {
            int size = (int)m_imageSize;
            string path = string.Format(CultureConstants.InvariantCulture, (size > 64 ?
                NetworkConstants.CCPTypeRender : NetworkConstants.CCPTypeImage), m_item.ID,
                size);
            return ImageService.GetImageServerBaseUri(path);
        }

        /// <summary>
        /// Callback method for asynchronous web requests.
        /// </summary>
        /// <param name="id">EveObject id for retrieved image</param>
        /// <param name="image">Image object retrieved</param>
        private void GotImage(long id, Image image)
        {
            // Only display the image if the id matches the current EveObject
            if (image != null && m_item.ID == id)
            {
                pbImage.Image = image;

                // Draw the overlay icon
                if ((int)m_imageSize > 64)
                    DrawOverlayIcon();
            }
            else
                ShowBlankImage();
        }

        /// <summary>
        /// Draws the overlay icon.
        /// </summary>
        private void DrawOverlayIcon()
        {
            Bitmap overlayIcon = null;
            try
            {
                switch (m_item.MetaGroup)
                {
                    case ItemMetaGroup.T2:
                        overlayIcon = Properties.Resources.T2;
                        break;
                    case ItemMetaGroup.T3:
                        overlayIcon = Properties.Resources.T3;
                        break;
                    case ItemMetaGroup.Storyline:
                        overlayIcon = Properties.Resources.Storyline;
                        break;
                    case ItemMetaGroup.Deadspace:
                        overlayIcon = Properties.Resources.Deadspace;
                        break;
                    case ItemMetaGroup.Officer:
                        overlayIcon = Properties.Resources.Officer;
                        break;
                    case ItemMetaGroup.Faction:
                        overlayIcon = Properties.Resources.Faction;
                        break;
                    default:
                        overlayIcon = new Bitmap(16, 16);
                        break;
                }

                Image image = (Image)pbImage.Image.Clone();

                using (Graphics graph = Graphics.FromImage(image))
                {
                    graph.DrawImage(overlayIcon, 0, 0, (int)m_imageSize / 4, (int)m_imageSize / 4);
                }

                pbImage.Image = image;
            }
            finally
            {
                overlayIcon?.Dispose();
            }
        }

        #endregion


        #region Local Event Handlers

        /// <summary>
        /// Handles the Tick event of the m_timer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void m_timer_Tick(object sender, EventArgs e)
        {
            m_timer.Stop();

            if (!Enabled)
                return;

            // Actions according to mouse clicks
            switch (m_mouseEvent.Clicks)
            {
                case 1:
                {
                    base.OnMouseClick(m_mouseEvent);
                }
                    break;
                case 2:
                {
                    base.OnMouseDoubleClick(m_mouseEvent);

                    WindowsFactory.ShowByTag<EveImagePopUp, Item>(m_item);
                }
                    break;
            }
        }

        /// <summary>
        /// Event handler for image click.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void pbImage_MouseClick(object sender, MouseEventArgs e)
        {
            // If item does not support pop-up trigger the event immediately
            if (!m_popUpActive)
            {
                base.OnMouseClick(e);
                return;
            }

            // Store the single mouse click event
            m_mouseEvent = e;
            m_timer.Start();
        }

        /// <summary>
        /// Event handler for image double click.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void pbImage_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            // Display the pop up form if pop-ups are enabled and a suitable image can be retrieved
            // otherwise trigger a single mouse click event
            if (!m_popUpActive)
            {
                base.OnMouseClick(e);
                return;
            }

            // Store the double mouse click event
            m_mouseEvent = e;
            m_timer.Start();
        }

        #endregion


        #region Private Enumerations and Structs

        /// <summary>
        /// Identifies the image type being handled.
        /// </summary>
        private enum ImageType
        {
            Ship,
            Drone,
            Structure,
            Item,
            Blueprint
        }

        /// <summary>
        /// Indicates the source of the .png image name.
        /// </summary>
        private enum ImageNameFrom
        {
            TypeID,
            Icon
        };

        /// <summary>
        /// Defines configuration data for a specific ImageType.
        /// </summary>
        private struct ImageTypeData
        {
            internal readonly string LocalComponent;
            internal readonly string URLPath;
            internal readonly ImageNameFrom NameFrom;
            internal readonly ArrayList ValidSizes;

            internal ImageTypeData(string local, string url, ImageNameFrom name, ArrayList sizes)
            {
                LocalComponent = local;
                URLPath = url;
                NameFrom = name;
                ValidSizes = sizes;
            }
        }

        #endregion
    }
}
