using System;
using EVEMon.Common.Constants;
using EVEMon.Common.Enumerations;
using EVEMon.Common.Service;

namespace EVEMon.Common.Helpers
{
    public static class ImageHelper
    {
        private const string NO_TYPE = "";

        /// <summary>
        /// Gets the URL for the image of the specified alliance.
        /// </summary>
        /// <param name="id">the ID for the given type.</param>
        /// <param name="size">the image size.</param>
        /// <returns></returns>
        public static Uri GetAllianceImageURL(long id, int size = (int)EveImageSize.x32)
        {
            return ImageService.GetImageServerBaseUri(string.Format(CultureConstants.
                InvariantCulture, NetworkConstants.CCPAllianceLogo, id, size));
        }

        /// <summary>
        /// Gets the URL for the image of the specified corporation.
        /// </summary>
        /// <param name="id">the ID for the given type.</param>
        /// <param name="size">the image size.</param>
        /// <returns></returns>
        public static Uri GetCorporationImageURL(long id, int size = (int)EveImageSize.x32)
        {
            return ImageService.GetImageServerBaseUri(string.Format(CultureConstants.
                InvariantCulture, NetworkConstants.CCPCorporationLogo, id, size));
        }

        /// <summary>
        /// Gets the URL for the image of the specified type.
        /// </summary>
        /// <param name="id">the ID for the given type.</param>
        /// <param name="size">the image size.</param>
        /// <returns></returns>
        public static Uri GetTypeImageURL(long id, int size = (int)EveImageSize.x32)
        {
            return ImageService.GetImageServerBaseUri(string.Format(CultureConstants.
                InvariantCulture, NetworkConstants.CCPTypeImage, id, size));
        }

        /// <summary>
        /// Gets the URL for the render of the specified type.
        /// </summary>
        /// <param name="id">the ID for the given type.</param>
        /// <param name="size">the image size.</param>
        /// <returns></returns>
        public static Uri GetTypeRenderURL(long id, int size = (int)EveImageSize.x32)
        {
            return ImageService.GetImageServerBaseUri(string.Format(CultureConstants.
                InvariantCulture, NetworkConstants.CCPTypeRender, id, size));
        }

        /// <summary>
        /// Gets the URL for the portrait of the specified character.
        /// </summary>
        /// <param name="id">the ID for the character.</param>
        /// <param name="size">the image size.</param>
        /// <returns></returns>
        public static Uri GetPortraitUrl(long id, int size = (int)EveImageSize.x32)
        {
            return ImageService.GetImageServerBaseUri(string.Format(CultureConstants.
                InvariantCulture, NetworkConstants.CCPPortraits, id, size));
        }
    }
}
