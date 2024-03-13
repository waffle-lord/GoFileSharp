using System;
using System.Collections.Generic;
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
            var status = await _api.UpdateContent(GoFile.ApiToken, Id, option);

            if (status.IsOK) 
                await RefreshAsync();

            return status.IsOK;
        }

        /// <summary>
        /// Refresh this files information
        /// </summary>
        /// <returns></returns>
        public async Task<bool> RefreshAsync()
        {
            var parent = await GoFile.GetFolderAsync(ParentFolderId, true);

            if (parent == null) return false;

            var thisFile = parent.Children.First(x => x.Id == Id);

            if (thisFile != null && thisFile is FileData fileData)
            {
                base.Update(fileData);
                return true;
            }
            
            return false;
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
        public async Task<Dictionary<string, DeleteInfo>> DeleteAsync()
        {
            var result = await _api.DeleteContent(GoFile.ApiToken, new[] { this.Id });

            return result.Data ?? new Dictionary<string, DeleteInfo>();
        }


        /// <summary>
        /// Update the name of this file
        /// </summary>
        /// <param name="newName">The new name of the file</param>
        /// <returns>Returns true if the name was updated, otherwise false</returns>
        public async Task<bool> SetName(string newName) =>
            await SetOptionAndRefresh(FileContentOption.Name(newName));

        /// <summary>
        /// Add a direct link to this file
        /// </summary>
        /// <param name="optionsBuilder">The options builder to use for link options</param>
        /// <returns></returns>
        public async Task<DirectLink?> AddDirectLink(DirectLinkOptionsBuilder optionsBuilder)
            => await AddDirectLink(optionsBuilder.Build());
        
        /// <summary>
        /// Add a direct link to this file
        /// </summary>
        /// <param name="options">The options to set on the link</param>
        /// <returns>A <see cref="DirectLink"/> or null if the link fails to be added</returns>
        public async Task<DirectLink?> AddDirectLink(DirectLinkOptions? options = null)
        {
            var response = await _api.AddDirectLink(GoFile.ApiToken, Id, options);

            if (response.IsOK) 
                await RefreshAsync();

            return response.Data;
        }
        
        // todo: update direct link
        // todo: delete direct link
    }
}
