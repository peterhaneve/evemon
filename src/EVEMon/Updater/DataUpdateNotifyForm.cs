using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using EVEMon.Common;
using EVEMon.Common.Controls;
using EVEMon.Common.CustomEventArgs;
using EVEMon.Common.Data;
using EVEMon.Common.Helpers;
using EVEMon.Common.Serialization.PatchXml;
using EVEMon.Common.Service;
using EVEMon.Common.Extensions;

namespace EVEMon.Updater
{
    public partial class DataUpdateNotifyForm : EVEMonForm
    {
        private readonly DataUpdateAvailableEventArgs m_args;
        private bool m_formClosing;

        /// <summary>
        /// Default constructor.
        /// </summary>
        private DataUpdateNotifyForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public DataUpdateNotifyForm(DataUpdateAvailableEventArgs args)
            : this()
        {
            m_args = args;
        }

        /// <summary>
        /// On load we update the informations.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            StringBuilder changedFiles = new StringBuilder();
            StringBuilder notes = new StringBuilder();

            foreach (SerializableDatafile versionDatafile in m_args.ChangedFiles)
            {
                changedFiles
                    .AppendLine($"Filename: {versionDatafile.Name.PadRight(35)}\tReleased: {versionDatafile.Date}");

                notes
                    .AppendLine(versionDatafile.Message)
                    .AppendLine();
            }
            tbFiles.Lines = changedFiles.ToString().TrimEnd(Environment.NewLine.ToCharArray()).Split(Environment.NewLine.ToCharArray());
            tbNotes.Lines = notes.ToString().TrimEnd(Environment.NewLine.ToCharArray()).Replace("\r", String.Empty).Split('\n');
        }

        /// <summary>
        /// Handles the FormClosing event of the DataUpdateNotifyForm control.
        /// </summary>
        /// <param name="e">The <see cref="FormClosingEventArgs"/> instance containing the event data.</param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (!Visible ||
                (e.CloseReason != CloseReason.ApplicationExitCall && e.CloseReason != CloseReason.TaskManagerClosing &&
                 e.CloseReason != CloseReason.WindowsShutDown))
            {
                return;
            }

            m_formClosing = true;
        }

        /// <summary>
        /// Occurs on "update" button click.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            DialogResult result = DialogResult.Yes;
            int changedFilesCount = m_args.ChangedFiles.Count, newCount = changedFilesCount;

            // Delete the EVE Flags xml file from cache to force a refetch on next startup
            FileHelper.DeleteFile(LocalXmlCache.GetFileInfo(EveFlag.Filename).FullName);

            while (newCount != 0 && result == DialogResult.Yes)
            {
                if (m_formClosing)
                    break;

                DownloadUpdates();
                newCount = m_args.ChangedFiles.Count;

                if (newCount == 0)
                    break;

                // One or more files failed
                string message = newCount +
                    $" file{(newCount.S())} failed to download, do you wish to try again?";

                result = MessageBox.Show(message, @"Failed Download", MessageBoxButtons.YesNo);
            }

            // If no files were updated, abort the update process
            DialogResult = newCount == changedFilesCount ? DialogResult.Abort : DialogResult.OK;
        }

        /// <summary>
        /// Downloads the updates.
        /// </summary>
        private void DownloadUpdates()
        {
            List<SerializableDatafile> datafiles = new List<SerializableDatafile>();

            // Copy the new datafiles to a new list
            datafiles.AddRange(m_args.ChangedFiles);

            // Show the download dialog, which will download the files
            using (DataUpdateDownloadForm form = new DataUpdateDownloadForm(datafiles))
            {
                form.ShowDialog();
            }

            foreach (SerializableDatafile versionDatafile in datafiles.Where(datafile => datafile.IsDownloaded))
            {
                string oldFilename = Path.Combine(EveMonClient.EVEMonDataDir, versionDatafile.Name);
                string tempFilename = $"{oldFilename}.tmp";

                Datafile downloadedDatafile = new Datafile(Path.GetFileName(tempFilename));

                if (versionDatafile.MD5Sum != null && versionDatafile.MD5Sum != downloadedDatafile.MD5Sum)
                {
                    FileHelper.DeleteFile(tempFilename);
                    continue;
                }

                UpdateManager.ReplaceDatafile(oldFilename, tempFilename);
                m_args.ChangedFiles.Remove(versionDatafile);
            }
        }

        /// <summary>
        /// Occurs on "remind me later" button click.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLater_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Abort;
        }
    }
}
