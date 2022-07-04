using GoFileSharp.Controllers;
using GoFileSharp.Model.GoFileData;
using GoFileSharp.Model.GoFileData.Wrappers;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


/* TODO:
 * [X] GetContent
 * [ ] UploadFile
 * [ ] GetMyFolder (gets the user's root folder, maybe a better name for this idk ...)
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
        /// Get a <see cref="GoFileFolder"/> and it's contents
        /// </summary>
        /// <param name="contentId"></param>
        /// <returns>Returns the folder object or null</returns>
        public static async Task<GoFileFolder?> GetContent(string contentId)
        {
            if (ApiToken == null) return null;

            var response = await _api.GetContentAsync(contentId, ApiToken);

            if(response.IsOK && response.Data != null)
            {
                return new GoFileFolder(response.Data, _api);
            }

            return null;
        }

        public static async Task<GoFileFile?> UploadFileAsync(FileInfo file, IProgress<double> progress = null)
        {
            var uploadResponse = await _api.UploadFileAsync(file, ApiToken, progress);

            if(!uploadResponse.IsOK || uploadResponse.Data == null)
            {
                return null;
            }

            var parentFolder = await GetContent(uploadResponse.Data.ParentFolderId);

            if (parentFolder == null) return null;

            var uploadedContent = parentFolder.Contents.SingleOrDefault(x => x.Id == uploadResponse.Data.FileId);

            if(uploadedContent is FileData uploadedFile)
            {
                return new GoFileFile(uploadedFile, _api);
            }

            return null;
        }
    }
}
