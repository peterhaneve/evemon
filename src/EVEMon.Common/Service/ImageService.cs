using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using EVEMon.Common.Constants;
using EVEMon.Common.Enumerations;
using EVEMon.Common.Helpers;
using EVEMon.Common.Net;

namespace EVEMon.Common.Service
{
    public static class ImageService
    {
        /// <summary>
        /// Gets the image server base URI.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public static Uri GetImageServerBaseUri(string path) => new Uri(
            NetworkConstants.EVEImageServerBase + path);

        /// <summary>
        /// Asynchronously downloads a character portrait from its ID.
        /// </summary>
        /// <param name="charId"></param>
        internal static async Task<Image> GetCharacterImageAsync(long charId)
        {
            string path = string.Format(CultureConstants.InvariantCulture,
                NetworkConstants.CCPPortraits, charId, (int)EveImageSize.x128);

            return await GetImageAsync(GetImageServerBaseUri(path), false).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously downloads an alliance image.
        /// </summary>
        /// <param name="pictureBox">The picture box.</param>
        /// <param name="allianceID">The alliance ID.</param>
        public static Task GetAllianceImageAsync(PictureBox pictureBox, long allianceID)
        {
            string path = string.Format(CultureConstants.InvariantCulture, NetworkConstants.
                CCPAllianceLogo, allianceID, pictureBox.Width);
            return GetImageAsync(pictureBox, path);
        }

        /// <summary>
        /// Asynchronously downloads a corporation image.
        /// </summary>
        /// <param name="pictureBox">The picture box.</param>
        /// <param name="corporationID">The corporation ID.</param>
        public static Task GetCorporationImageAsync(PictureBox pictureBox, long corporationID)
        {
            string path = string.Format(CultureConstants.InvariantCulture, NetworkConstants.
                CCPCorporationLogo, corporationID, pictureBox.Width);
            return GetImageAsync(pictureBox, path);
        }

        /// <summary>
        /// Called when image gets downloaded.
        /// </summary>
        /// <param name="pictureBox">The picture box.</param>
        /// <param name="path">The path.</param>
        private static async Task GetImageAsync(PictureBox pictureBox, string path)
        {
            Image image = await GetImageAsync(GetImageServerBaseUri(path)).ConfigureAwait(false);
            pictureBox.Image = image ?? pictureBox.InitialImage;
            pictureBox.Update();
        }

        /// <summary>
        /// Asynchronously downloads an image from the provided url.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="useCache">if set to <c>true</c> [use cache].</param>
        public static async Task<Image> GetImageAsync(Uri url, bool useCache = true)
        {
            DownloadResult<Image> result;

            // Cache not to be used ?
            if (!useCache)
            {
                result = await HttpWebClientService.DownloadImageAsync(url).ConfigureAwait(false);
                return GetImage(result);
            }

            Image image = GetImageFromCache(GetCacheName(url));
            if (image != null)
                return image;

            // Downloads the image and adds it to cache
            result = await HttpWebClientService.DownloadImageAsync(url).ConfigureAwait(false);
            image = GetImage(result);

            if (image != null)
                await AddImageToCacheAsync(image, GetCacheName(url)).ConfigureAwait(false);

            return image;
        }

        /// <summary>
        /// Asynchronously gets the character image from cache.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="directory">The directory.</param>
        /// <returns></returns>
        internal static Image GetImageFromCache(string filename, string directory = null)
        {
            // First check whether the image exists in cache
            EveMonClient.EnsureCacheDirInit();
            string cacheFileName = Path.Combine(directory ?? EveMonClient.EVEMonImageCacheDir,
                filename);

            if (!File.Exists(cacheFileName))
                return null;

            try
            {
                // Load the data into a MemoryStream before returning the image to avoid file
                // locking
                Image image;
                byte[] imageBytes = File.ReadAllBytes(cacheFileName);
                using (MemoryStream stream = new MemoryStream(imageBytes))
                {
                    image = Image.FromStream(stream);
                }
                return image;
            }
            catch (ArgumentException e)
            {
                ExceptionHandler.LogException(e, false);
                FileHelper.DeleteFile(cacheFileName);
            }
            catch (IOException e)
            {
                ExceptionHandler.LogException(e, false);
            }
            catch (UnauthorizedAccessException e)
            {
                ExceptionHandler.LogException(e, false);
            }

            return null;
        }

        /// <summary>
        /// Callback used when images are downloaded.
        /// </summary>
        /// <param name="result">The result.</param>
        private static Image GetImage(DownloadResult<Image> result)
        {
            if (result.Error == null)
                return result.Result;

            if (result.Error.Status == HttpWebClientServiceExceptionStatus.Timeout)
                EveMonClient.Trace(result.Error.Message);
            else
                ExceptionHandler.LogException(result.Error, true);

            return null;
        }

        /// <summary>
        /// Adds the image to the cache.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="filename">The filename.</param>
        /// <param name="directory">The directory.</param>
        /// <returns></returns>
        internal static async Task AddImageToCacheAsync(Image image, string filename,
            string directory = null)
        {
            // Saves the image file
            try
            {
                // Write this image to the cache file
                EveMonClient.EnsureCacheDirInit();
                string cacheFileName = Path.Combine(directory ?? EveMonClient.
                    EVEMonImageCacheDir, filename);
                await FileHelper.OverwriteOrWarnTheUserAsync(cacheFileName,
                    async fs =>
                    {
                        ((Image)image.Clone()).Save(fs, ImageFormat.Png);
                        await fs.FlushAsync();
                        return true;
                    }).ConfigureAwait(false);
            }
            catch (IOException ex)
            {
                // Anything but "file in use"
                if (ex.HResult != -2147024864)
                    ExceptionHandler.LogException(ex, true);
            }
            catch (Exception ex)
            {
                ExceptionHandler.LogRethrowException(ex);
                throw;
            }
        }

        /// <summary>
        /// From a given url, computes a cache file name.
        /// </summary>
        private static string GetCacheName(Uri url)
        {
            Stream stream = Util.GetMemoryStream(Encoding.UTF8.GetBytes(url.AbsoluteUri));
            string md5Sum = Util.CreateMD5(stream);
            // Extensions are no longer part of the requested URLs
            return string.Concat(md5Sum, ".png");
        }
    }
}
