using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using GoFileSharp.Model.GoFileData;

namespace GoFileSharp.Model.HTTP
{
    public static class GoFileRequest
    {
        public static HttpRequestMessage GetServers(string zoneId)
        {
            return new HttpRequestMessage(HttpMethod.Get, Routes.GetServers(zoneId));
        }

        public static HttpRequestMessage UploadFile(FileInfo file, string server, string token = "", string folderId = "")
        {
            var request = new HttpRequestMessage(HttpMethod.Post, Routes.PostUploadFile(server));

            var requestParams = new Dictionary<string, string>
            {
                { "file", file.FullName },
            };

            if (!string.IsNullOrWhiteSpace(token))
            {
                requestParams.Add("token", token);
            }

            if (!string.IsNullOrWhiteSpace(folderId))
            {
                requestParams.Add("folderId", folderId);
            }

            request.Content = new FormUrlEncodedContent(requestParams);

            return request;
        }

        public static HttpRequestMessage CreateFolder(string token, string parentFolderId, string folderName = "")
        {
            var request = new HttpRequestMessage(HttpMethod.Post, Routes.PostCreateFolder());

            var requestParams = new Dictionary<string, string>
            {
                { "token", token },
                { "parentFolderId", parentFolderId }
            };

            if (!string.IsNullOrWhiteSpace(folderName))
            {
                requestParams.Add("folderName", folderName);
            }

            request.Content = new FormUrlEncodedContent(requestParams);

            return request;
        }

        public static HttpRequestMessage UpdateContent(string token, string contentId, string attribute, string attributeValue)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, Routes.PutContentsUpdate(contentId));

            var requestParams = new Dictionary<string, string>
            {
                { "token", token },
                { "attribute", attribute },
                { "attributeValue", attributeValue }
            };

            request.Content = new FormUrlEncodedContent(requestParams);

            return request;
        }

        public static HttpRequestMessage DeleteContent(string token, string[] contentIds)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, Routes.DeleteContents());

            var requestParams = new Dictionary<string, string>
            {
                { "token", token },
                { "contentsId", string.Join(',', contentIds) }
            };

            request.Content = new FormUrlEncodedContent(requestParams);

            return request;
        }

        public static HttpRequestMessage GetContents(string token, string contentId, bool noCache = false, string passwordHash = "")
        {
            return new HttpRequestMessage(HttpMethod.Get, 
                Routes.GetContent(token, contentId, noCache, passwordHash));
        }

        public static HttpRequestMessage CreateDirectLink(string token, string contentId, long expireTimeAsUnixSeconds = 0, string[] sourceIpsAllowed = null, string[] domainsAllowed = null, string[] auth = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, Routes.PostDirectLink(contentId));

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            
            // var directLink = new DirectLink();
            //
            // if (expireTimeAsUnixSeconds > 0)
            // {
            //     
            // }
            //
            // if (sourceIpsAllowed)
            // {
            //     
            // }
            //
            // if (domainsAllowed)
            // {
            //     
            // }
            //
            // if (auth != null && auth.Length > 0)
            // {
            //     
            // }

            return request;
        }
        
    }
}