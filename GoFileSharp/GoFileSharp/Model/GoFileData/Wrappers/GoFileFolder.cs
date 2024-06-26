﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GoFileSharp.Controllers;
using GoFileSharp.Interfaces;


namespace GoFileSharp.Model.GoFileData.Wrappers
{
    /// <summary>
    /// A wrapper class for the GoFile <see cref="FolderData"/>
    /// </summary>
    public class GoFileFolder : FolderData
    {
        private readonly GoFileController _api;
        private readonly GoFileOptions _options;

        public GoFileFolder(FolderData content, GoFileOptions options, GoFileController controller) : base(content)
        {
            _options = options;
            _api = controller;
        }

        private async Task<bool> SetOptionAndRefresh(IContentOption option)
        {
            var status = await _api.UpdateContent(Id, option);

            if (status.IsOK) 
                await RefreshAsync();

            return status.IsOK;
        }

        /// <summary>
        /// Refresh this folders information
        /// </summary>
        /// <param name="passwordHash">The SHA256 hash of the password set on this folder</param>
        /// <returns></returns>
        /// <remarks>Automatic refreshes, like when using a Set method (SetNameAsync for example) will not refresh if a password is set. You will need to call this manually with the password hash</remarks>
        public async Task<bool> RefreshAsync(string? passwordHash = null)
        {
            var thisFolder = await _api.GetContentAsync(Id, true, passwordHash);

            if(!thisFolder.IsOK || thisFolder.Data == null) 
                return false;

            if (thisFolder.Data is FolderData folder)
            {
                base.Update(folder);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Create a new subfolder in this folder
        /// </summary>
        /// <param name="folderName">The name of the new folder</param>
        /// <returns>Returns the newly created folder as a <see cref="GoFileFolder"/> object or null</returns>
        public async Task<GoFileFolder?> CreateFolderAsync(string folderName)
        {
            var createFolderResponse = await _api.CreateFolder(Id, folderName);

            if(!createFolderResponse.IsOK || createFolderResponse.Data == null)
            {
                return null;
            }

            return new GoFileFolder(createFolderResponse.Data, _options, _api);
        }

        /// <summary>
        /// Find a file by name
        /// </summary>
        /// <param name="fileName">The name of the file</param>
        /// <returns>Returns the file as a <see cref="GoFileFile"/> object or null</returns>
        public GoFileFile? FindFile(string fileName)
        {
            var fileContent = Children.SingleOrDefault(x => x.Name == fileName);

            if(fileContent is FileData file)
            {
                return new GoFileFile(file, _api);
            }

            return null;
        }

        /// <summary>
        /// Find a folder by name
        /// </summary>
        /// <param name="name">The name of the folder</param>
        /// <returns>Returns the folder as a <see cref="GoFileFolder"/> object or null</returns>
        public async Task<GoFileFolder?> FindFolderAsync(string name)
        {
            var folderContent = Children.SingleOrDefault(x => x.Name == name);
            
            if(folderContent is FolderData folderData)
            {
                var folderResponse = await _api.GetContentAsync(folderData.Id, true);

                if(folderResponse.IsOK && folderResponse.Data != null && folderResponse.Data is FolderData folder)
                {
                    return new GoFileFolder(folder, _options, _api);
                }
            }

            return null;
        }

        /// <summary>
        /// Upload a file into this folder
        /// </summary>
        /// <param name="file">The file to upload</param>
        /// <param name="progress">progress object for the upload</param>
        /// <returns>Returns the uplaoded file as a <see cref="GoFileFile"/> object</returns>
        public async Task<GoFileFile?> UploadIntoAsync(FileInfo file, IProgress<double> progress = null)
        {
            var response =  await _api.UploadFileAsync(file, _options.PreferredZone, progress, Id);
            
            if(!response.IsOK || response.Data == null)
            {
                return null;
            }

            return await GoFileHelper.TryGetUplaodedFile(response.Data, _api);
        }

        /// <summary>
        /// Delete this folder
        /// </summary>
        /// <returns>Returns true if the folder was deleted, otherwise false</returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<Dictionary<string, DeleteInfo>> DeleteAsync()
        {
            var response = await _api.DeleteContent(new[] { Id });

            return response.Data ?? new Dictionary<string, DeleteInfo>();
        }

        /// <summary>
        /// Copy this folder to another folder
        /// </summary>
        /// <param name="destinationFolder">The folder to copy to</param>
        /// <returns>Returns true if this folder was copied into the destination folder, otherwise false</returns>
        public async Task<bool> CopyToAsync(GoFileFolder destinationFolder)
        {
            return await destinationFolder.CopyIntoAsync(new IContent[] { this });
        }

        /// <summary>
        /// Copy content into this folder
        /// </summary>
        /// <param name="content">an array of content to copy</param>
        /// <returns>Returns true if the content was copied, otherwise false</returns>
        public async Task<bool> CopyIntoAsync(IContent[] content)
        {
            var contentIds = content.Select(x => x.Id).ToArray();

            var copyResponse = await _api.CopyContent(contentIds, Id);

            return copyResponse.IsOK;
        }

        /// <summary>
        /// Set the tags for this folder
        /// </summary>
        /// <param name="tags">the tags to set on this folder</param>
        /// <returns>Returns true is the option was set, otherwise false</returns>
        public async Task<bool> SetTagsAsync(List<string> tags) 
            => await SetOptionAndRefresh(FolderContentOption.Tags(tags));

        /// <summary>
        /// Set the password for this folder
        /// </summary>
        /// <param name="password"></param>
        /// <returns>Returns true is the option was set, otherwise false</returns>
        public async Task<bool> SetPasswordAsync(string password) 
            => await SetOptionAndRefresh(FolderContentOption.Password(password));

        /// <summary>
        /// Set the expiration date of the folder
        /// </summary>
        /// <param name="date"></param>
        /// <returns>Returns true is the option was set, otherwise false</returns>
        public async Task<bool> SetExpireAsync(DateTimeOffset date) 
            => await SetOptionAndRefresh(FolderContentOption.Expire(date));

        /// <summary>
        /// Set the public flag of this folder
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Returns true is the option was set, otherwise false</returns>
        public async Task<bool> SetPublicAsync(bool value) 
            => await SetOptionAndRefresh(FolderContentOption.Public(value));

        /// <summary>
        /// Set the description of this folder
        /// </summary>
        /// <param name="description"></param>
        /// <returns>Returns true is the option was set, otherwise false</returns>
        public async Task<bool> SetDescriptionAsync(string description) 
            => await SetOptionAndRefresh(FolderContentOption.Description(description));

        /// <summary>
        /// Update the name of this folder
        /// </summary>
        /// <param name="newName">The new name of the folder</param>
        /// <returns>Returns true if the name was updated, otherwise false</returns>
        public async Task<bool> SetNameAsync(string newName)
            => await SetOptionAndRefresh(FolderContentOption.Name(newName));
        
        /// <summary>
        /// Add a direct link to this folder
        /// </summary>
        /// <param name="optionsBuilder">The options builder to use for link options</param>
        /// <returns>A <see cref="DirectLink"/> or null if the link fails to be added</returns>
        public async Task<DirectLink?> AddDirectLinkAsync(DirectLinkOptionsBuilder? optionsBuilder = null)
            => await AddDirectLinkAsync(optionsBuilder?.Build());
        
        private async Task<DirectLink?> AddDirectLinkAsync(DirectLinkOptions? options = null)
        {
            var response = await _api.AddDirectLink(Id, options);

            if (response.IsOK) 
                await RefreshAsync();
            
            return response.Data;
        }
        
        /// <summary>
        /// Update a direct link on this folder
        /// </summary>
        /// <param name="directLink">The direct link to update</param>
        /// <param name="optionsBuilder">The options builder to use to update the link</param>
        /// <returns>A <see cref="DirectLink"/> or null if the link fails to be updated</returns>
        public async Task<DirectLink?> UpdateDirectLinkAsync(DirectLink directLink, DirectLinkOptionsBuilder optionsBuilder)
            => await UpdateDirectLinkAsync(directLink.Id, optionsBuilder.Build());
        
        private async Task<DirectLink?> UpdateDirectLinkAsync(string directLinkId, DirectLinkOptions options)
        {
            var response = await _api.UpdateDirectLink(Id, directLinkId, options);

            if (response.IsOK)
                await RefreshAsync();

            return response.Data;
        }

        /// <summary>
        /// Remove a direct link from this folder
        /// </summary>
        /// <param name="directLink">The direct link to remove</param>
        /// <returns>Returns true if the link was removed, otherwise false</returns>
        public async Task<bool> RemoveDirectLinkAsync(DirectLink directLink) 
            => await RemoveDirectLinkAsync(directLink.Id);
        
        private async Task<bool> RemoveDirectLinkAsync(string directLinkId)
        {
            var response = await _api.RemoveDirectLink(Id, directLinkId);

            if (response.IsOK)
                await RefreshAsync();

            return response.IsOK;
        }
    }
}
