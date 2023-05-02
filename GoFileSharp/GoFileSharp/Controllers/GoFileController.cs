using GoFileSharp.Builders;
using GoFileSharp.Model.GoFileData;
using GoFileSharp.Model.HTTP;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using GoFileSharp.Extensions;
using GoFileSharp.Interfaces;
using GoFileSharp.Model.GoFileData.Wrappers;

namespace GoFileSharp.Controllers
{
    /// <summary>
    /// A means to interact with the GoFile.io API
    /// </summary>
    public class GoFileController
    {
        private HttpClient _client = new HttpClient();

        /// <summary>
        /// A GoFile API Controller
        /// </summary>
        /// <param name="timeout">The HttpClient timeout. Defaults to 1 hour</param>
        public GoFileController(TimeSpan? timeout = null)
        {
            _client.Timeout = timeout ?? TimeSpan.FromHours(1);
        }

        private async Task<GoFileResponse<T>> DeserializeResponse<T>(HttpResponseMessage response) where T : class
        {
            return JsonConvert.DeserializeObject<GoFileResponse<T>>(await response.Content.ReadAsStringAsync())
                   ?? new GoFileResponse<T>() { Status = "Failed to deserialize response" };
        }

        private GoFileResponse<T> GetFailedResponseStatus<T>(HttpResponseMessage? response) where T : class
        {
            return new GoFileResponse<T>() { Status = response?.StatusCode.ToString() ?? "No response from GoFile" };
        }

        private void ProgressTracker(FileStream fileToTrack, IProgress<double> progress, ref bool keepTracking)
        {
            int prevPos = -1;
            while (keepTracking)
            {
                int pos = (int)Math.Round(100 * (fileToTrack.Position / (double)fileToTrack.Length));
                if (pos != prevPos)
                {
                    progress.Report(pos);
                }
                prevPos = pos;

                Thread.Sleep(100); //update every 100ms
            }
        }

        /// <summary>
        /// Get a server from GoFile
        /// </summary>
        /// <returns>Returns the response from GoFile with the server info or null</returns>
        public async Task<GoFileResponse<ServerInfo>> GetServerAsync()
        {
            var serverRequest = new HttpRequestMessage(HttpMethod.Get, Routes.GetServer());

            var serverResponse = await _client.SendAsync(serverRequest);

            if (serverResponse == null || serverResponse.Content == null)
            {
                return GetFailedResponseStatus<ServerInfo>(serverResponse);
            }

            return await DeserializeResponse<ServerInfo>(serverResponse);
        }

        /// <summary>
        /// Get the content of a folder from GoFile
        /// </summary>
        /// <param name="contentId">The contentId of the folder to request content info for</param>
        /// <param name="token">The token to use with this request</param>
        /// <returns>Returns the response from GoFile with the content info or null</returns>
        public async Task<GoFileResponse<IContent>> GetContentAsync(string contentId, string token)
        {
            var contentRequest = new HttpRequestMessage(HttpMethod.Get, Routes.GetContent(contentId, token));

            var contentResponse = await _client.SendAsync(contentRequest);

            if(contentResponse == null || contentResponse.Content == null)
            {
                return GetFailedResponseStatus<IContent>(contentResponse);
            }

            var proxyContentResponse = JsonConvert.DeserializeObject<GoFileResponse<ProxyContentInfo>>(await contentResponse.Content.ReadAsStringAsync());

            if(proxyContentResponse == null)
            {
                return new GoFileResponse<IContent>() { Status = "No response from GoFile" };
            }

            if(proxyContentResponse != null && !proxyContentResponse.IsOK || proxyContentResponse?.Data == null)
            {
                return new GoFileResponse<IContent>() { Status = proxyContentResponse.Status };
            }

            ContentInfoBuilder builder = new ContentInfoBuilder(proxyContentResponse.Data);

            return new GoFileResponse<IContent>() { Status = proxyContentResponse.Status, Data = builder.Build() };
        }

        /// <summary>
        /// Download a file
        /// </summary>
        /// <param name="directDownloadLink">The direct download link to the file</param>
        /// <param name="destinationFile">The destination file info</param>
        /// <param name="progress">A progress object to use to track download progress</param>
        /// <returns></returns>
        public async Task<GoFileResponse<ContentInfo>> DownloadFileAsync(string directDownloadLink, FileInfo destinationFile, bool overwrite = false, IProgress<double> progress = null)
        {
            try
            {
                if (destinationFile.Exists && !overwrite)
                    return new GoFileResponse<ContentInfo>() { Status = $"File already exists: {destinationFile.FullName}" };

                Directory.CreateDirectory(destinationFile.Directory.FullName);

                using var fileStream = destinationFile.Open(FileMode.Create);

                await _client.DownloadDataAsync(directDownloadLink, fileStream, progress);

                destinationFile.Refresh();

                string status = "Download failed";

                if (destinationFile.Exists)
                    status = "ok";

                return new GoFileResponse<ContentInfo>() { Status = status};
            }
            catch(Exception ex)
            {
                return new GoFileResponse<ContentInfo>() { Status = ex.Message};
            }
        }

        /// <summary>
        /// Uploads a file to GoFile
        /// </summary>
        /// <param name="file">The file to upload</param>
        /// <param name="token">The token to use with this request</param>
        /// <param name="progress">A progress object to report progress updates to</param>
        /// <returns></returns>
        public async Task<GoFileResponse<UploadInfo>> UploadFileAsync(System.IO.FileInfo file, string token = null, IProgress<double> progress = null, string folderId = null)
        {
            file.Refresh();

            if (!file.Exists)
            {
                new GoFileResponse<UploadInfo>() { Status = $"File does not exist: {file.FullName}" };
            }

            var serverResponse = await GetServerAsync();

            if (!serverResponse.IsOK || serverResponse.Data == null)
            {
                return new GoFileResponse<UploadInfo>() { Status = serverResponse.Status };
            }

            bool keepTracking = true; //to start and stop the tracking thread

            try
            {
                using (FileStream fileToUpload = file.OpenRead())
                {
                    var form = new MultipartFormDataContent
                    {
                        { new StreamContent(fileToUpload), "file", file.Name }
                    };

                    if (token != null)
                        form.Add(new StringContent(token), "token");

                    if ((folderId != null))
                        form.Add(new StringContent(folderId), "folderId");

                    var uploadRequest = new HttpRequestMessage(HttpMethod.Post, Routes.UploadFile(serverResponse.Data.Server))
                    {
                        Content = form
                    };

                    if (progress != null)
                    {
                        new Task(new Action(() => { ProgressTracker(fileToUpload, progress, ref keepTracking); })).Start();
                    }

                    var uploadResponse = await _client.SendAsync(uploadRequest);

                    if(uploadResponse == null || uploadResponse.Content == null)
                    {
                        return GetFailedResponseStatus<UploadInfo>(uploadResponse);
                    }

                    return await DeserializeResponse<UploadInfo>(uploadResponse);
                }
            }
            catch (Exception ex)
            {
                return new GoFileResponse<UploadInfo> { Status = ex.Message };
            }
            finally
            {
                keepTracking = false;
            }
        }

        /// <summary>
        /// Get the account details for the token provided
        /// </summary>
        /// <param name="token">The token to use with this request</param>
        /// <returns></returns>
        public async Task<GoFileResponse<AccountDetails>> GetAccountDetails(string token)
        {
            var accountRequest = new HttpRequestMessage(HttpMethod.Get, Routes.GetAccountDetails(token));

            var accountResponse = await _client.SendAsync(accountRequest);

            if(accountResponse == null || accountResponse.Content == null)
            {
                return GetFailedResponseStatus<AccountDetails>(accountResponse);
            }

            return await DeserializeResponse<AccountDetails>(accountResponse);
        }

        /// <summary>
        /// Create a folder on GoFile
        /// </summary>
        /// <param name="token">The token to use with this request</param>
        /// <param name="parentFolderId">The parent folder Id to create the new folder in</param>
        /// <param name="folderName">The name of the new folder</param>
        /// <returns></returns>
        public async Task<GoFileResponse<ContentInfo>> CreateFolder(string token, string parentFolderId, string folderName)
        {
            var createFolderRequest = new HttpRequestMessage(HttpMethod.Put, Routes.CreateFolder());

            var requestDictionary = new Dictionary<string, string>
            {
                { "token", token },
                { "parentFolderId", parentFolderId },
                { "folderName", folderName }
            };

            createFolderRequest.Content = new FormUrlEncodedContent(requestDictionary);

            var createFolderResponse = await _client.SendAsync(createFolderRequest);

            if(createFolderRequest == null || createFolderRequest.Content == null)
            {
                return GetFailedResponseStatus<ContentInfo>(createFolderResponse);
            }

            return await DeserializeResponse<ContentInfo>(createFolderResponse);
        }

        /// <summary>
        /// Copy contents to a folder
        /// </summary>
        /// <param name="token">The token to use with this request</param>
        /// <param name="contentIds">The Ids of the content to copy</param>
        /// <param name="destinationFolderId">The folder to copy the contents into</param>
        /// <returns></returns>
        public async Task<GoFileResponse<ContentInfo>> CopyContent(string token, string[] contentIds, string destinationFolderId)
        {
            var copyRequest = new HttpRequestMessage(HttpMethod.Put, Routes.CopyContent());

            var requestDictionary = new Dictionary<string, string>
            {
                { "token", token },
                { "contentsId", string.Join(',', contentIds) },
                { "folderIdDest", destinationFolderId }
            };

            copyRequest.Content = new FormUrlEncodedContent(requestDictionary);

            var copyResponse = await _client.SendAsync(copyRequest);

            if(copyResponse == null || copyResponse.Content == null)
            {
                return GetFailedResponseStatus<ContentInfo>(copyResponse);
            }

            return await DeserializeResponse<ContentInfo>(copyResponse);
        }

        /// <summary>
        /// Delete contents
        /// </summary>
        /// <param name="token">The token to use with this request</param>
        /// <param name="contentIds">The content Ids to delete</param>
        /// <returns></returns>
        public async Task<GoFileResponse<ContentInfo>> DeleteContent(string token, string[] contentIds)
        {
            var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, Routes.DeleteContent());

            var requestDictionary = new Dictionary<string, string>
            {
                { "token", token },
                { "contentsId", string.Join(',', contentIds) }
            };

            deleteRequest.Content = new FormUrlEncodedContent(requestDictionary);

            var deleteResponse = await _client.SendAsync(deleteRequest);

            if(deleteResponse == null || deleteResponse.Content == null)
            {
                return GetFailedResponseStatus<ContentInfo>(deleteResponse);
            }

            return await DeserializeResponse<ContentInfo>(deleteResponse);
        }

        /// <summary>
        /// Set a file or folder option
        /// </summary>
        /// <param name="token">The token to use with this request</param>
        /// <param name="contentId">The Id of the content to update</param>
        /// <param name="option">The option to set</param>
        /// <returns></returns>
        public async Task<bool> SetOption(string token, string contentId, IContentOption option)
        {
            var setOptionRequset = new HttpRequestMessage(HttpMethod.Put, Routes.SetOption());

            var requestDictionary = new Dictionary<string, string>
            {
                { "token", token },
                { "contentId", contentId },
                { "option", option.OptionName },
                { "value", option.Value }
            };

            setOptionRequset.Content = new FormUrlEncodedContent(requestDictionary);

            var setOptionResponse = await _client.SendAsync(setOptionRequset);

            if(setOptionResponse == null || setOptionResponse.Content == null)
            {
                return false;
            }

            var goFileResponse = await DeserializeResponse<object>(setOptionResponse);

            return goFileResponse.IsOK;
        }
    }
}