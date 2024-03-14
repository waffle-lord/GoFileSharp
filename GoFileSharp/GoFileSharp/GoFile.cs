using GoFileSharp.Controllers;
using GoFileSharp.Interfaces;
using GoFileSharp.Model.GoFileData;
using GoFileSharp.Model.GoFileData.Wrappers;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GoFileSharp.Model;

namespace GoFileSharp
{
    /// <summary>
    /// A wrapper to interact with the GoFile.io API.
    /// </summary>
    /// <remarks>If you want more direct access to the responses, create a <see cref="GoFileController"/> to use instead</remarks>
    public class GoFile
    {
        private readonly GoFileOptions _options = new GoFileOptions();
        private readonly GoFileController _api = new GoFileController();

        public GoFile(GoFileOptions? options = null)
        {
            if (options == null)
                return;
            
            _options = options;
        }

        /// <summary>
        /// Get <see cref="IContent"/> from an ID
        /// </summary>
        /// <param name="contentId"></param>
        /// <returns>Returns content of the id</returns>
        private async Task<IContent?> GetContentAsync(string contentId, bool noCache = false)
        {
            var response = await _api.GetContentAsync(contentId, _options.ApiToken, noCache);

            if(response is { IsOK: true, Data: { } data })
            {
                // todo: add file data here if it is ever added. Currently only folder are allowed
                
                if (data is FolderData folder) 
                    return new GoFileFolder(folder, _options, _api);
            }

            return null;
        }

        /// <summary>
        /// Get a folder object from an ID
        /// </summary>
        /// <param name="contentId"></param>
        /// <returns></returns>
        public async Task<GoFileFolder?> GetFolderAsync(string contentId, bool noCache = false)
        {
            var folder = await GetContentAsync(contentId, noCache);

            if(folder is GoFileFolder gofileFolder)
            {
                return gofileFolder;
            }

            return null;
        }
        
        // /// <summary>
        // /// Get a file object from an ID
        // /// </summary>
        // /// <param name="contentId"></param>
        // /// <returns></returns>
        // public static async Task<GoFileFile?> GetFile(string contentId, bool noCache = false)
        // {
        //     var file = await GetContentAsync(contentId, noCache);
        //
        //     if (file is GoFileFile gofileFile)
        //     {
        //         return gofileFile;
        //     }
        //
        //     return null;
        // }
        
        /// <summary>
        /// Upload a file to Gofile
        /// </summary>
        /// <param name="file">The file to upload</param>
        /// <param name="progress"></param>
        /// <returns>Returns the uploaded file</returns>
        /// <remarks>If the preferred zone option was set, the upload will use a server in that zone</remarks>
        public async Task<GoFileFile?> UploadFileAsync(FileInfo file, IProgress<double> progress = null, string folderId = null)
        {
            var uploadResponse = await _api.UploadFileAsync(file, _options.PreferredZone, _options.ApiToken, progress, folderId);

            if(!uploadResponse.IsOK || uploadResponse.Data == null)
            {
                return null;
            }

            return await GoFileHelper.TryGetUplaodedFile(uploadResponse.Data, _options, _api);
        }

        /// <summary>
        /// Get your account details
        /// </summary>
        /// <returns>Returns your account details</returns>
        public async Task<AccountDetails?> GetMyAccount()
        {
            var accountResponse = await _api.GetAccountDetails(_options.ApiToken);

            return accountResponse.Data;
        }

        /// <summary>
        /// Get the account's root folder
        /// </summary>
        /// <returns>Returns the root folder</returns>
        public async Task<GoFileFolder?> GetMyRootFolderAsync()
        {
            var accountDetailsResponse = await _api.GetAccountDetails(_options.ApiToken);

            if(!accountDetailsResponse.IsOK || accountDetailsResponse.Data == null)
            {
                return null;
            }

            return await GetFolderAsync(accountDetailsResponse.Data.RootFolder) ?? null;
        }
    }
}
