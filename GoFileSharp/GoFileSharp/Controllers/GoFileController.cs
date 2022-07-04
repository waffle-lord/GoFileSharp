using GoFileSharp.Builders;
using GoFileSharp.Model.GoFileData;
using GoFileSharp.Model.HTTP;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

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
            var requestServer = new HttpRequestMessage(HttpMethod.Get, Routes.GetServer());

            var result = await _client.SendAsync(requestServer);

            if (result == null || result.Content == null)
            {
                return new GoFileResponse<ServerInfo>() { Status = result?.StatusCode.ToString() ?? "No response from GoFile" };
            }

            return JsonConvert.DeserializeObject<GoFileResponse<ServerInfo>>(await result.Content.ReadAsStringAsync())
                   ?? new GoFileResponse<ServerInfo>() { Status = "Failed to deserialize response"};
        }

        /// <summary>
        /// Get the content of a folder from GoFile
        /// </summary>
        /// <param name="contentId">The contentId of the folder to request content info for</param>
        /// <param name="token">A GoFile API token to use with the request</param>
        /// <returns>Returns the response from GoFile with the content info or null</returns>
        public async Task<GoFileResponse<ContentInfo>> GetContentAsync(string contentId, string token)
        {
            var requestContent = new HttpRequestMessage(HttpMethod.Get, Routes.GetContent(contentId, token));

            var result = await _client.SendAsync(requestContent);

            if(result == null || result.Content == null)
            {
                return new GoFileResponse<ContentInfo>() { Status = result?.StatusCode.ToString() ?? "No response from GoFile" };
            }

            var response = JsonConvert.DeserializeObject<GoFileResponse<ProxyContentInfo>>(await result.Content.ReadAsStringAsync());

            if(response == null)
            {
                return new GoFileResponse<ContentInfo>() { Status = "No response from GoFile" };
            }

            if(response != null && !response.IsOK || response?.Data == null)
            {
                return new GoFileResponse<ContentInfo>() { Status = response.Status };
            }

            ContentInfoBuilder builder = new ContentInfoBuilder(response.Data);

            return new GoFileResponse<ContentInfo>() { Status = response.Status, Data = builder.Build() };
        }

        /// <summary>
        /// Uploads a file to GoFile
        /// </summary>
        /// <param name="file">The file to upload</param>
        /// <param name="token">The token to use for the uplaod</param>
        /// <param name="progress">A progress object to report progress updates to</param>
        /// <returns></returns>
        public async Task<GoFileResponse<UploadInfo>> UploadFileAsync(System.IO.FileInfo file, string token = null, IProgress<double> progress = null)
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
                    var form = new MultipartFormDataContent();

                    form.Add(new StreamContent(fileToUpload), "file", file.Name);

                    if (token != null)
                        form.Add(new StringContent(token), "token");

                    var uploadRequest = new HttpRequestMessage(HttpMethod.Post, Routes.UploadFile(serverResponse.Data.Server))
                    {
                        Content = form
                    };

                    if (progress != null)
                    {
                        new Task(new Action(() => { ProgressTracker(fileToUpload, progress, ref keepTracking); })).Start();
                    }

                    var result = _client.SendAsync(uploadRequest).Result.Content;

                    return JsonConvert.DeserializeObject<GoFileResponse<UploadInfo>>(await result.ReadAsStringAsync())
                           ?? new GoFileResponse<UploadInfo>() { Status = "Failed to deserialize response" };
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
    }
}