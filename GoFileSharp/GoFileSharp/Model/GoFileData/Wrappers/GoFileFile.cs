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
        private readonly GoFileOptions _options;
        private readonly GoFileController _api;

        public GoFileFile(FileData content, GoFileOptions options, GoFileController controller) : base(content)
        {
            _options = options;
            _api = controller;
        }

        private async Task<bool> SetOptionAndRefresh(IContentOption option)
        {
            var status = await _api.UpdateContent(_options.ApiToken, Id, option);

            if (status.IsOK) 
                await RefreshAsync();

            return status.IsOK;
        }

        /// <summary>
        /// Refresh this files information
        /// </summary>
        /// <param name="parentFolderPasswordHash">The SHA256 hash of the parent folder's password to use when refreshing this file</param>
        /// <returns></returns>
        /// <remarks>Automatic refreshes, like when using a Set method (SetNameAsync for example) will not refresh if a password is set. You will need to call this manually with the password hash</remarks>
        public async Task<bool> RefreshAsync(string? parentFolderPasswordHash = null)
        {
            var parent = await _api.GetContentAsync(ParentFolderId, _options.ApiToken, true, parentFolderPasswordHash);

            if (!parent.IsOK || parent.Data == null)
                return false;

            if (parent.Data is FolderData parentFolder)
            {

                var thisFile = parentFolder.Children.First(x => x.Id == Id);

                if (thisFile != null && thisFile is FileData fileData)
                {
                    base.Update(fileData);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Download this file using a specific direct link
        /// </summary>
        /// <param name="destinationFile">The destination file to save to</param>
        /// <param name="directLink">The direct link to use when downloading this file</param>
        /// <param name="overwrite">Whether or not to overwrite the destination file if it exists</param>
        /// <param name="progress">Pregress to track the download with</param>
        /// <returns>Returns true if the file was downloaded, otherwise false</returns>
        public async Task<bool> DownloadAsync(FileInfo destinationFile, DirectLink directLink, bool overwrite = false, IProgress<double> progress = null)
        {
            var result = await _api.DownloadFileAsync(directLink.Link, destinationFile, overwrite, progress);

            return result.IsOK;
        }
        
        /// <summary>
        /// Download this file using the first direct link found for this file
        /// </summary>
        /// <param name="destinationFile">The destination file to save to</param>
        /// <param name="overwrite">Whether or not to overwrite the destination file if it exists</param>
        /// <param name="progress">Progress to track the download with</param>
        /// <returns>Returns true if the file was downloaded, otherwise false</returns>
        public async Task<bool> DownloadAsync(FileInfo destinationFile, bool overwrite = false, IProgress<double> progress = null)
        {
            var result = await _api.DownloadFileAsync(DirectLinks.First().Link, destinationFile, overwrite, progress);

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
            var result = await _api.DeleteContent(_options.ApiToken, new[] { this.Id });

            return result.Data ?? new Dictionary<string, DeleteInfo>();
        }

        /// <summary>
        /// Update the name of this file
        /// </summary>
        /// <param name="newName">The new name of the file</param>
        /// <returns>Returns true if the name was updated, otherwise false</returns>
        public async Task<bool> SetNameAsync(string newName) =>
            await SetOptionAndRefresh(FileContentOption.Name(newName));

        /// <summary>
        /// Add a direct link to this file
        /// </summary>
        /// <param name="optionsBuilder">The options builder to use for link options</param>
        /// <returns>A <see cref="DirectLink"/> or null if the link fails to be added</returns>
        public async Task<DirectLink?> AddDirectLinkAsync(DirectLinkOptionsBuilder? optionsBuilder = null)
            => await AddDirectLinkAsync(optionsBuilder?.Build());
        
        private async Task<DirectLink?> AddDirectLinkAsync(DirectLinkOptions? options = null)
        {
            var response = await _api.AddDirectLink(_options.ApiToken, Id, options);

            if (response.IsOK) 
                await RefreshAsync();
            
            return response.Data;
        }
        
        /// <summary>
        /// Update a direct link on this file
        /// </summary>
        /// <param name="directLink">The direct link to update</param>
        /// <param name="optionsBuilder">The options builder to use to update the link</param>
        /// <returns>A <see cref="DirectLink"/> or null if the link fails to be updated</returns>
        public async Task<DirectLink?> UpdateDirectLinkAsync(DirectLink directLink, DirectLinkOptionsBuilder optionsBuilder)
            => await UpdateDirectLinkAsync(directLink.Id, optionsBuilder.Build());
        
        private async Task<DirectLink?> UpdateDirectLinkAsync(string directLinkId, DirectLinkOptions options)
        {
            var response = await _api.UpdateDirectLink(_options.ApiToken, Id, directLinkId, options);

            if (response.IsOK)
                await RefreshAsync();

            return response.Data;
        }

        /// <summary>
        /// Remove a direct link from this file
        /// </summary>
        /// <param name="directLink">The direct link to remove</param>
        /// <returns>Returns true if the link was removed, otherwise false</returns>
        public async Task<bool> RemoveDirectLinkAsync(DirectLink directLink) 
            => await RemoveDirectLinkAsync(directLink.Id);
        
        private async Task<bool> RemoveDirectLinkAsync(string directLinkId)
        {
            var response = await _api.RemoveDirectLink(_options.ApiToken, Id, directLinkId);

            if (response.IsOK)
                await RefreshAsync();

            return response.IsOK;
        }
    }
}
