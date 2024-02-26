using GoFileSharp.Controllers;
using GoFileSharp.Interfaces;
using GoFileSharp.Model.GoFileData;
using GoFileSharp.Model.GoFileData.Wrappers;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


/* TODO:
 * [X] GetContent
 * [X] UploadFile
 * [X] GetMyFolder (gets the user's root folder, maybe a better name for this idk ...)
 * [ ] ...
 * [ ] probably should offer some kind of logging... but I'm kind of lazy so idk ...
 */

namespace GoFileSharp
{
    /// <summary>
    /// A wrapper to interact with the GoFile.io API. You can set the <see cref="ApiToken"/> to use it with all requests (it is required for some)
    /// </summary>
    /// <remarks>If you want more direct access to the responses, create a static <see cref="GoFileController"/> to use instead</remarks>
    public static class GoFile
    {
        /// <summary>
        /// The token to use with API calls
        /// </summary>
        public static string ApiToken = null;
        private static GoFileController _api = new GoFileController();

        /// <summary>
        /// Get <see cref="IContent"/> from an ID
        /// </summary>
        /// <param name="contentId"></param>
        /// <returns>Returns content of the id</returns>
        private static async Task<IContent?> GetContent(string contentId, bool noCache = false)
        {
            if (ApiToken == null) return null;

            var response = await _api.GetContentAsync(contentId, ApiToken, noCache);

            if(response.IsOK && response.Data != null)
            {
                if(response.Data is ContentInfo folder)
                {
                    return new GoFileFolder(folder, _api);
                }

                if(response.Data is FileData file)
                {
                    return new GoFileFile(file, _api);
                }
            }

            return null;
        }

        /// <summary>
        /// Get a folder object from an ID
        /// </summary>
        /// <param name="contentId"></param>
        /// <returns></returns>
        public static async Task<GoFileFolder?> GetFolder(string contentId, bool noCache = false)
        {
            var folder = await GetContent(contentId, noCache);

            if(folder is GoFileFolder gofileFolder)
            {
                return gofileFolder;
            }

            return null;
        }

        // todo: getting files via content ID on the API is not allowed?
        // /// <summary>
        // /// Get a file object from an ID
        // /// </summary>
        // /// <param name="contentId"></param>
        // /// <returns></returns>
        // public static async Task<GoFileFile?> GetFile(string contentId, bool noCache = false)
        // {
        //     var file = await GetContent(contentId, noCache);
        //
        //     if (file is GoFileFile gofileFile)
        //     {
        //         return gofileFile;
        //     }
        //
        //     return null;
        // }

        private static async Task<GoFileFile?> TryGetUplaodedFile(UploadInfo uploadInfo)
        {
            // NOTE: This is mainly due to GoFile folder data not updating immediately after an upload :(
            // up to 1 min to try and get uploaded file
            TimeSpan interval = TimeSpan.FromSeconds(10);
            int maxTries = 10;

            IContent? uploadedContent = null;

            while (maxTries > 0)
            {
                var parentFolder = await GetFolder(uploadInfo.ParentFolderId, true);

                if (parentFolder == null) return null;

                uploadedContent = parentFolder.Contents.SingleOrDefault(x => x.Id == uploadInfo.FileId);

                if (uploadedContent != null) break;

                maxTries--;

                await Task.Delay(interval);
            }

            if (uploadedContent == null) return null;

            if (uploadedContent is FileData uploadedFile)
            {
                return new GoFileFile(uploadedFile, _api);
            }

            return null;
        }

        /// <summary>
        /// Upload a file to Gofile
        /// </summary>
        /// <param name="file">The file to upload</param>
        /// <param name="progress"></param>
        /// <returns>Returns the uploaded file</returns>
        public static async Task<GoFileFile?> UploadFileAsync(FileInfo file, IProgress<double> progress = null, string folderId = null)
        {
            var uploadResponse = await _api.UploadFileAsync(file, ApiToken, progress, folderId);

            if(!uploadResponse.IsOK || uploadResponse.Data == null)
            {
                return null;
            }

            return await TryGetUplaodedFile(uploadResponse.Data);
        }

        /// <summary>
        /// Get the account's root folder
        /// </summary>
        /// <returns>Returns the root folder</returns>
        public static async Task<GoFileFolder?> GetMyRootFolderAsync()
        {
            var accountDetailsResponse = await _api.GetAccountDetails(ApiToken);

            if(!accountDetailsResponse.IsOK || accountDetailsResponse.Data == null)
            {
                return null;
            }

            return await GetFolder(accountDetailsResponse.Data.RootFolder) ?? null;
        }
    }
}
