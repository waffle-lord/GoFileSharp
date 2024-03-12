using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GoFileSharp.Controllers;
using GoFileSharp.Interfaces;

namespace GoFileSharp.Model.GoFileData.Wrappers
{
    /// <summary>
    /// A wrapper class for the GoFile <see cref="FileData"/>
    /// </summary>
    /// <remarks>The name is stupid, I know ...</remarks>
    public class GoFileFile : FileData
    {
        private GoFileController _api;

        public GoFileFile(FileData content, GoFileController controller) : base(content)
        {
            _api = controller;
        }

        private async Task<bool> SetOptionAndRefresh(IContentOption option)
        {
            var status = await _api.SetOption(GoFile.ApiToken, Id, option);

            if (status) await RefreshAsync();

            return status;
        }

        /// <summary>
        /// Refresh this files information
        /// </summary>
        /// <returns></returns>
        public async Task<bool> RefreshAsync()
        {
            var parent = await GoFile.GetFolderAsync(ParentFolderId, true);

            if (parent == null) return false;

            var thisFile = parent.FindFile(Name);

            if(thisFile == null) return false;

            base.Update(thisFile);

            return true;
        }

        /// <summary>
        /// Download this file
        /// </summary>
        /// <param name="destinationFile">The destination file to save to</param>
        /// <param name="overwrite">Whether or not to overwrite the destination file if it exists</param>
        /// <param name="progress">Progress to track the download with</param>
        /// <returns>Returns true if the file was downloaded, otherwise false</returns>
        public async Task<bool> DownloadAsync(FileInfo destinationFile, bool overwrite = false, IProgress<double> progress = null)
        {
            var result = await _api.DownloadFileAsync(DirectLinks.Values.First().Link, destinationFile, overwrite, progress);

            return result.IsOK;
        }

        /// <summary>
        /// Copy this file to a folder
        /// </summary>
        /// <param name="destinationFolder">The folder to copy this file to</param>
        /// <returns>Returns true if the file was copied, otherwise false</returns>
        public async Task<bool> CopyToAsync(GoFileFolder destinationFolder)
        {
            return await destinationFolder.CopyIntoAsync(new[] { this });
        }

        /// <summary>
        /// Delete this file
        /// </summary>
        /// <returns>Returns true if the file was deleted, otherwise false</returns>
        public async Task<DeleteInfo> DeleteAsync()
        {
            var result = await _api.DeleteContent(GoFile.ApiToken, new[] { this.Id });

            return result.Data ?? DeleteInfo.NoData();
        }

        /// <summary>
        /// Set this file's direct link option
        /// </summary>
        /// <param name="value">True to enable direct link, false to disable</param>
        /// <returns>Returns true is the option was updated successfully, otherwise false</returns>
        public async Task<bool> SetDirectLink(bool value) => await SetOptionAndRefresh(FileContentOption.DirectLink(value));
    }
}
