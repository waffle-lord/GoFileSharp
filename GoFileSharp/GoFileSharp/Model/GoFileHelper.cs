using System;
using System.Linq;
using System.Threading.Tasks;
using GoFileSharp.Controllers;
using GoFileSharp.Interfaces;
using GoFileSharp.Model.GoFileData;
using GoFileSharp.Model.GoFileData.Wrappers;

namespace GoFileSharp.Model
{
    public static class GoFileHelper
    {
        public static async Task<GoFileFile?> TryGetUplaodedFile(UploadInfo uploadInfo, GoFileOptions options, GoFileController api, string? passwordHash = null)
        {
            // NOTE: This is mainly due to GoFile folder data not updating immediately after an upload :(
            // up to 1 min to try and get uploaded file
            TimeSpan interval = TimeSpan.FromSeconds(10);
            int maxTries = 10;

            IContent? uploadedContent = null;

            while (maxTries > 0)
            {
                var parentFolder = await api.GetContentAsync(uploadInfo.ParentFolderId, options.ApiToken, true, passwordHash);

                if (!parentFolder.IsOK || parentFolder.Data == null) 
                    return null;

                if (parentFolder.Data is FolderData folder)
                {
                    uploadedContent = folder.Children.SingleOrDefault(x => x.Id == uploadInfo.FileId);

                    if (uploadedContent != null)
                        break;

                    maxTries--;

                }

                await Task.Delay(interval);
            }

            if (uploadedContent == null) return null;

            if (uploadedContent is FileData uploadedFile)
            {
                return new GoFileFile(uploadedFile, options, api);
            }

            return null;
        }
    }
}