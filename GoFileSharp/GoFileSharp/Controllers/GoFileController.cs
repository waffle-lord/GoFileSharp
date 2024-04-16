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
using GoFileSharp.Model;
using Newtonsoft.Json.Linq;

namespace GoFileSharp.Controllers
{
    /// <summary>
    /// A means to interact with the GoFile.io API
    /// </summary>
    public class GoFileController
    {
        private readonly string? _token;
        private readonly AccountType _accountType = AccountType.Guest;
        private HttpClient _client = new HttpClient();

        /// <summary>
        /// A GoFile API Controller
        /// </summary>
        /// <param name="token">The GoFile API token to use with this controller</param>
        /// <param name="timeout">The HttpClient timeout. Defaults to 1 hour</param>
        protected GoFileController(string? token = null, TimeSpan? timeout = null)
        {
            _client.Timeout = timeout ?? TimeSpan.FromHours(1);
            
            if (string.IsNullOrWhiteSpace(token))
            {
                return;
            }

            _token = token;

            var accountIdResponse = GetAccountId().GetAwaiter().GetResult();

            if (!accountIdResponse.IsOK || accountIdResponse.Data == null)
            {
                return;
            }

            var accountResponse = GetAccountDetails(accountIdResponse.Data.Id)
                .GetAwaiter().GetResult();

            if (!accountResponse.IsOK || accountResponse.Data == null)
            {
                return;
            }

            _accountType = accountResponse.Data.Tier;
        }

        /// <summary>
        /// Initialize a new GoFile API Controller
        /// </summary>
        /// <param name="token">The GoFile API token to use with requests</param>
        /// <param name="timeout">The timeout to use for the http client</param>
        /// <returns>A new <see cref="GoFileController"/> that is ready to use</returns>
        public static GoFileController Init(string? token = null, TimeSpan? timeout = null)
        {
            return new GoFileController(token, timeout);
        }
        
        private bool CheckAccountAllowed(AccountType minRequiredAccountType, out string message)
        {
            message = "ok";
            
            if (minRequiredAccountType == AccountType.Guest)
            {
                return true;
            }

            if (_accountType >= minRequiredAccountType && !string.IsNullOrWhiteSpace(_token))
            {
                return true;
            }

            message =
                $"A {minRequiredAccountType} account or higher is required for this call. Currently using a '{_accountType}' account";

            return false;
        }

        private async Task<GoFileResponse<T>> DeserializeResponse<T>(HttpResponseMessage response) where T : class
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<GoFileResponse<T>>(content)
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
        /// <remarks>This call does not require a GoFile account to use: Accessible as guest</remarks>
        public async Task<GoFileResponse<ServerInfo>> GetServersAsync(string? zoneId = null)
        {
            var serverRequest = GoFileRequest.GetServers(zoneId);

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
        /// <param name="noCache">Whether or not to use GoFile's cache</param>
        /// <param name="passwordHash">The SHA256 password hash to use if the content is password protected</param>
        /// <returns>Returns the response from GoFile with the content info or null</returns>
        /// <remarks>This call requires a GoFile Premium account or higher</remarks>
        public async Task<GoFileResponse<IContent>> GetContentAsync(string contentId, bool noCache = false, string? passwordHash = null)
        {
            if (!CheckAccountAllowed(AccountType.Premium, out string message))
            {
                return new GoFileResponse<IContent>() { Status = message};
            }
            
            var contentRequest = GoFileRequest.GetContents(_token, contentId, noCache, passwordHash);

            var contentResponse = await _client.SendAsync(contentRequest);

            if(contentResponse == null || contentResponse.Content == null)
            {
                return GetFailedResponseStatus<IContent>(contentResponse);
            }

            var content = await contentResponse.Content.ReadAsStringAsync();

            var response = JObject.Parse(content);

            var status = response["status"].Value<string>() ?? "Response failed";
            var data = response["data"].Value<JObject>();

            if(status != "ok" || data == null || !data.HasValues)
            {
                return new GoFileResponse<IContent>() { Status = status};
            }
            
            // todo: add file content here if it ever gets added. Currently only folders are allowed
            
            if (FolderData.TryParse(data, out FolderData folder))
            {
                return new GoFileResponse<IContent>() { Status = status, Data = folder };
            }

            return new GoFileResponse<IContent>() { Status = "Failed to parse data" };
        }

        /// <summary>
        /// Download a file
        /// </summary>
        /// <param name="directDownloadLink">The direct download link to the file</param>
        /// <param name="destinationFile">The destination file info</param>
        /// <param name="progress">A progress object to use to track download progress</param>
        /// <returns>Returns a GoFile response</returns>
        /// <remarks>This response is not deserialized from GoFile and is only used for consistency</remarks>
        public async Task<GoFileResponse<object>> DownloadFileAsync(string directDownloadLink, FileInfo destinationFile, bool overwrite = false, IProgress<double> progress = null)
        {
            try
            {
                if (destinationFile.Exists && !overwrite)
                    return new GoFileResponse<object>() { Status = $"File already exists: {destinationFile.FullName}" };

                Directory.CreateDirectory(destinationFile.Directory.FullName);

                using var fileStream = destinationFile.Open(FileMode.Create);

                await _client.DownloadDataAsync(directDownloadLink, fileStream, progress);

                destinationFile.Refresh();

                string status = "Download failed";

                if (destinationFile.Exists)
                    status = "ok";

                return new GoFileResponse<object>() { Status = status};
            }
            catch(Exception ex)
            {
                return new GoFileResponse<object>() { Status = ex.Message};
            }
        }

        /// <summary>
        /// Uploads a file to GoFile
        /// </summary>
        /// <param name="file">The file to upload</param>
        /// <param name="progress">A progress object to report progress updates to</param>
        /// <param name="folderId">The folder ID to upload into</param>
        /// <returns>The response from GoFile including the uploaded file info</returns>
        /// <remarks>This call does not require a GoFile account to use: Accessible as guest</remarks>
        public async Task<GoFileResponse<UploadInfo>> UploadFileAsync(FileInfo file, ServerZone zone, IProgress<double> progress = null, string folderId = null)
        {
            file.Refresh();

            if (!file.Exists)
            {
                new GoFileResponse<UploadInfo>() { Status = $"File does not exist: {file.FullName}" };
            }
            
            var serverResponse = await GetServersAsync(zone.GetDescription());

            if (!serverResponse.IsOK || serverResponse.Data == null)
            {
                return new GoFileResponse<UploadInfo>() { Status = serverResponse.Status };
            }

            bool keepTracking = true; //to start and stop the tracking thread

            try
            {
                using (FileStream fileToUpload = file.OpenRead())
                {
                    // this one isn't using GoFileRequest because of this stream
                    // maybe there is a better way, but I'm lazy so ... eh..
                    var form = new MultipartFormDataContent
                    {
                        { new StreamContent(fileToUpload), "file", file.Name }
                    };

                    if (_token != null)
                        form.Add(new StringContent(_token), "token");

                    if ((folderId != null))
                        form.Add(new StringContent(folderId), "folderId");

                    var uploadRequest = new HttpRequestMessage(HttpMethod.Post, Routes.PostUploadFile(serverResponse.Data.Servers[0].Name))
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
        /// Get the account id for the provided token
        /// </summary>
        /// <returns>The GoFile response with an account id</returns>
        /// <remarks>This call requires a GoFile Standard account or higher</remarks>
        public async Task<GoFileResponse<AccountId>> GetAccountId()
        {
            // we need the id to get the account details / tier, so only checking to make sure a token is provided here
            if (_token == null)
            {
                return new GoFileResponse<AccountId>() { Status = "A token is required for this call" };
            }
            
            var idRequest = GoFileRequest.GetAccountId(_token);

            var response = await _client.SendAsync(idRequest);
            
            if (response == null || response.Content == null)
            {
                return GetFailedResponseStatus<AccountId>(response);
            }

            return await DeserializeResponse<AccountId>(response);
        }

        /// <summary>
        /// Get the account details for the token provided
        /// </summary>
        /// <param name="accountId">The account ID to get details of</param>
        /// <returns>The GoFile response with the account details</returns>
        /// <remarks>This call requires a GoFile Standard account or higher</remarks>
        public async Task<GoFileResponse<AccountDetails>> GetAccountDetails(string? accountId = null)
        {
            // we need to get the account details / tier, so only checking to make sure a token is provided here
            if (_token == null)
            {
                return new GoFileResponse<AccountDetails>() { Status = "A token is required for this call" };
            }
            
            if (accountId == null)
            {
                var idResponse = await GetAccountId();
                
                accountId = idResponse.Data?.Id;

                if (!idResponse.IsOK || accountId == null)
                {
                    return new GoFileResponse<AccountDetails>() { Status = idResponse.Status };
                }
            }

            var accountRequest = GoFileRequest.GetAccountDetails(_token, accountId);

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
        /// <param name="parentFolderId">The parent folder Id to create the new folder in</param>
        /// <param name="folderName">The name of the new folder</param>
        /// <returns>The GoFile response with the created folder</returns>
        /// <remarks>This call requires a GoFile Standard account or higher</remarks>
        public async Task<GoFileResponse<FolderData>> CreateFolder(string parentFolderId, string? folderName = null)
        {
            if (!CheckAccountAllowed(AccountType.Standard, out string message))
            {
                return new GoFileResponse<FolderData>() { Status = message };
            }
            
            var createFolderRequest = GoFileRequest.CreateFolder(_token, parentFolderId, folderName);

            var createFolderResponse = await _client.SendAsync(createFolderRequest);

            if(createFolderRequest == null || createFolderRequest.Content == null)
            {
                return GetFailedResponseStatus<FolderData>(createFolderResponse);
            }

            return await DeserializeResponse<FolderData>(createFolderResponse);
        }

        /// <summary>
        /// Copy contents to a folder
        /// </summary>
        /// <param name="contentIds">The Ids of the content to copy</param>
        /// <param name="destinationFolderId">The folder to copy the contents into</param>
        /// <returns>Returns the response from GoFile. The response contains an empty data object</returns>
        /// <remarks>This call requires a GoFile Premium account or higher.</remarks>
        public async Task<GoFileResponse<object>> CopyContent(string[] contentIds, string destinationFolderId)
        {
            if (!CheckAccountAllowed(AccountType.Premium, out string message))
            {
                return new GoFileResponse<object>() { Status = message };
            }
            
            var copyRequest = GoFileRequest.CopyContents(_token, contentIds, destinationFolderId);

            var copyResponse = await _client.SendAsync(copyRequest);

            if(copyResponse == null || copyResponse.Content == null)
            {
                return GetFailedResponseStatus<object>(copyResponse);
            }

            return await DeserializeResponse<object>(copyResponse);
        }

        /// <summary>
        /// Delete contents
        /// </summary>
        /// <param name="contentIds">The content Ids to delete</param>
        /// <returns>The response from GoFile with a dictionary of deletion info</returns>
        /// <remarks>This call requires a GoFile Standard account or higher</remarks>
        public async Task<GoFileResponse<Dictionary<string, DeleteInfo>>> DeleteContent(string[] contentIds)
        {
            if (!CheckAccountAllowed(AccountType.Standard, out string message))
            {
                return new GoFileResponse<Dictionary<string, DeleteInfo>>() { Status = message };
            }
            
            var deleteRequest = GoFileRequest.DeleteContent(_token, contentIds);

            var deleteResponse = await _client.SendAsync(deleteRequest);

            if(deleteResponse == null || deleteResponse.Content == null)
            {
                return GetFailedResponseStatus<Dictionary<string, DeleteInfo>>(deleteResponse);
            }
            
            return await DeserializeResponse<Dictionary<string, DeleteInfo>>(deleteResponse);
        }

        /// <summary>
        /// Set a file or folder option
        /// </summary>
        /// <param name="contentId">The Id of the content to update</param>
        /// <param name="option">The option to set</param>
        /// <returns>Returns the response from GoFile. The response has an empty data object</returns>
        /// <remarks>This call requires a GoFile Standard account or higher</remarks>
        public async Task<GoFileResponse<object>> UpdateContent(string contentId, IContentOption option)
        {
            if (!CheckAccountAllowed(AccountType.Standard, out string message))
            {
                return new GoFileResponse<object>() { Status = message };
            }
            
            var setOptionRequset = GoFileRequest.UpdateContent(_token, contentId, option.OptionName, option.Value);

            var setOptionResponse = await _client.SendAsync(setOptionRequset);

            if(setOptionResponse == null || setOptionResponse.Content == null)
            {
                return GetFailedResponseStatus<object>(setOptionResponse);
            }

            return await DeserializeResponse<object>(setOptionResponse);
        }

        /// <summary>
        /// Add a direct link to content
        /// </summary>
        /// <param name="contentId">The id of the content to add the link to</param>
        /// <param name="optionsBuilder">The link options builder to use for link options</param>
        /// <returns>The response from GoFile with the direct link info</returns>
        /// <remarks>This call requires a GoFile Premium account or higher</remarks>
        public async Task<GoFileResponse<DirectLink>> AddDirectLink(string contentId,
            DirectLinkOptionsBuilder optionsBuilder)
            => await AddDirectLink(contentId, optionsBuilder.Build());
        
        /// <summary>
        /// Add a direct link to content
        /// </summary>
        /// <param name="contentId">The id of the content to add the link to</param>
        /// <param name="options">The link options to set on the new link</param>
        /// <returns>The response from GoFile with the direct link info</returns>
        /// <remarks>This call requires a GoFile Premium account or higher</remarks>
        public async Task<GoFileResponse<DirectLink>> AddDirectLink(string contentId, DirectLinkOptions? options = null)
        {
            if (!CheckAccountAllowed(AccountType.Premium, out string message))
            {
                return new GoFileResponse<DirectLink>() { Status = message };
            }
            
            var addLinkRequest = GoFileRequest.CreateDirectLink(_token, contentId, options?.ExpireTime?.ToUnixTimeSeconds(),
                options?.SourceIpsAllowed, options?.DomainsAllowed, options?.Auth);

            var addLinkResponse = await _client.SendAsync(addLinkRequest);

            if (addLinkResponse == null || addLinkResponse.Content == null)
            {
                return GetFailedResponseStatus<DirectLink>(addLinkResponse);
            }

            return await DeserializeResponse<DirectLink>(addLinkResponse);
        }

        /// <summary>
        /// Update a direct link to content
        /// </summary>
        /// <param name="contentId">The id of the content to update the link to</param>
        /// <param name="directLinkId">The direct link id to update</param>
        /// <param name="options">The options to update on the link</param>
        /// <returns>The response from GoFile with the updated direct link info</returns>
        /// <remarks>This call requires a GoFile Premium account or higher</remarks>
        public async Task<GoFileResponse<DirectLink>> UpdateDirectLink(string contentId, string directLinkId, DirectLinkOptions options)
        {
            if (!CheckAccountAllowed(AccountType.Premium, out string message))
            {
                return new GoFileResponse<DirectLink>() { Status = message };
            }
            
            var updateLinkRequest = GoFileRequest.UpdateDirectLink(_token, contentId, directLinkId, options.ExpireTime?.ToUnixTimeSeconds(), 
                options.SourceIpsAllowed, options.DomainsAllowed, options.Auth);

            var updateLinkResponse = await _client.SendAsync(updateLinkRequest);

            if (updateLinkResponse == null || updateLinkResponse.Content == null)
            {
                return GetFailedResponseStatus<DirectLink>(updateLinkResponse);
            }

            return await DeserializeResponse<DirectLink>(updateLinkResponse);
        }

        /// <summary>
        /// Remove a direct link from content
        /// </summary>
        /// <param name="contentId">The id of the content to remove the link to</param>
        /// <param name="directLinkId">The direct link id to remove</param>
        /// <returns>The response from GoFile on if the deletion was successful</returns>
        /// <remarks>The response has an empty data object on it</remarks>
        /// <remarks>This call requires a GoFile Premium account or higher</remarks>
        public async Task<GoFileResponse<object>> RemoveDirectLink(string contentId, string directLinkId)
        {
            if (CheckAccountAllowed(AccountType.Premium, out string message))
            {
                return new GoFileResponse<object>() { Status = message };
            }
            
            var deleteLinkRequest = GoFileRequest.DeleteDirectLink(_token, contentId, directLinkId);

            var deleteLinkResponse = await _client.SendAsync(deleteLinkRequest);

            if (deleteLinkResponse == null || deleteLinkResponse.Content == null)
            {
                return GetFailedResponseStatus<object>(deleteLinkResponse);
            }

            return await DeserializeResponse<object>(deleteLinkResponse);
        }
    }
}