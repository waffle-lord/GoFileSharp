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
    /// A wrapper class for the GoFile <see cref="ContentInfo"/>
    /// </summary>
    public class GoFileFolder : ContentInfo
    {
        private GoFileController _api;

        public GoFileFolder(ContentInfo content, GoFileController controller) : base(content)
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
        /// Refresh this folders information
        /// </summary>
        /// <returns></returns>
        public async Task<bool> RefreshAsync()
        {
            var thisFolder = await GoFile.GetFolderAsync(Id, true);

            if(thisFolder == null) return false;

            base.Update(thisFolder);

            return true;
        }

        /// <summary>
        /// Create a new subfolder in this folder
        /// </summary>
        /// <param name="folderName">The name of the new folder</param>
        /// <returns>Returns the newly created folder as a <see cref="GoFileFolder"/> object or null</returns>
        public async Task<GoFileFolder?> CreateFolderAsync(string folderName)
        {
            var createFolderResponse = await _api.CreateFolder(GoFile.ApiToken, Id, folderName);

            if(!createFolderResponse.IsOK || createFolderResponse.Data == null || createFolderResponse.Data.Id == null)
            {
                return null;
            }

            var contentInfo = await GoFile.GetFolderAsync(createFolderResponse.Data.Id);

            if(contentInfo == null)
            {
                return null;
            }

            return new GoFileFolder(contentInfo, _api);
        }

        /// <summary>
        /// Find a file by name
        /// </summary>
        /// <param name="Name">The name of the file</param>
        /// <returns>Returns the file as a <see cref="GoFileFile"/> object or null</returns>
        public GoFileFile? FindFile(string Name)
        {
            var fileContent = Contents.SingleOrDefault(x => x.Name == Name);

            if(fileContent is FileData file)
            {
                return new GoFileFile(file, _api);
            }

            return null;
        }

        /// <summary>
        /// Find a folder by name
        /// </summary>
        /// <param name="Name">The name of the folder</param>
        /// <returns>Returns the folder as a <see cref="GoFileFolder"/> object or null</returns>
        public async Task<GoFileFolder?> FindFolderAsync(string Name)
        {
            var folderContent = Contents.SingleOrDefault(x => x.Name == Name);

            if(folderContent is FolderData folderData)
            {
                var folder = await GoFile.GetFolderAsync(folderData.Id, true);

                if(folder != null)
                {
                    return new GoFileFolder(folder, _api);
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
            return await GoFile.UploadFileAsync(file, progress, Id);
        }

        /// <summary>
        /// Delete this folder
        /// </summary>
        /// <returns>Returns true if the folder was deleted, otherwise false</returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> DeleteAsync()
        {
            var response = await _api.DeleteContent(GoFile.ApiToken, new[] { Id });

            return response.IsOK;
        }

        /// <summary>
        /// Copy this folder to another folder
        /// </summary>
        /// <param name="destinationFolder">The folder to copy to</param>
        /// <returns>Returns true if this folder was copied into the destination folder, otherwise false</returns>
        public async Task<bool> CopyToAsync(GoFileFolder destinationFolder)
        {
            return await destinationFolder.CopyIntoAsync(new[] { this });
        }

        /// <summary>
        /// Copy content into this folder
        /// </summary>
        /// <param name="content">an array of content to copy</param>
        /// <returns>Returns true if the content was copied, otherwise false</returns>
        public async Task<bool> CopyIntoAsync(IContent[] content)
        {
            var contentIds = content.Select(x => x.Id).ToArray();

            var copyResponse = await _api.CopyContent(GoFile.ApiToken, contentIds, Id);

            return copyResponse.IsOK;
        }

        /// <summary>
        /// Set the tags for this folder
        /// </summary>
        /// <param name="tags">the tags to set on this folder</param>
        /// <returns>Returns true is the option was set, otherwise false</returns>
        public async Task<bool> SetTags(List<string> tags) => await SetOptionAndRefresh(FolderContentOption.Tags(tags));

        /// <summary>
        /// Set the password for this folder
        /// </summary>
        /// <param name="password"></param>
        /// <returns>Returns true is the option was set, otherwise false</returns>
        public async Task<bool> SetPassword(string password) => await SetOptionAndRefresh(FolderContentOption.Password(password));

        /// <summary>
        /// Set the expiration date of the folder
        /// </summary>
        /// <param name="date"></param>
        /// <returns>Returns true is the option was set, otherwise false</returns>
        public async Task<bool> SetExpire(DateTimeOffset date) => await SetOptionAndRefresh(FolderContentOption.Expire(date));

        /// <summary>
        /// Set the public flag of this folder
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Returns true is the option was set, otherwise false</returns>
        public async Task<bool> SetPublic(bool value) => await SetOptionAndRefresh(FolderContentOption.Public(value));

        /// <summary>
        /// Set the description of this folder
        /// </summary>
        /// <param name="description"></param>
        /// <returns>Returns true is the option was set, otherwise false</returns>
        public async Task<bool> SetDescription(string description) => await SetOptionAndRefresh(FolderContentOption.Description(description));
    }
}
