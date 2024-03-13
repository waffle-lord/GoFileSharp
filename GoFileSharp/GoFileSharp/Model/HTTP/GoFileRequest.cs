using System.IO;
using System.Net.Http;

namespace GoFileSharp.Model.HTTP
{
    public static class GoFileRequest
    {
        public static HttpRequestMessage GetServers(string? zoneId = null)
        {
            return new HttpRequestMessage(HttpMethod.Get, Routes.GetServers(zoneId ?? ""));
        }

        public static HttpRequestMessage CreateFolder(string token, string parentFolderId, string? folderName = null)
        {
            return new GoFileRequestBuilder(HttpMethod.Post, Routes.PostCreateFolder())
                .WithBearerToken(token)
                .AddRequiredParam("parentFolderId", parentFolderId)
                .AddOptionalParam("folderName", folderName)
                .BuildAsJson();
        }

        public static HttpRequestMessage UpdateContent(string token, string contentId, string attribute, string attributeValue)
        {
            return new GoFileRequestBuilder(HttpMethod.Put, Routes.PutContentsUpdate(contentId))
                .WithBearerToken(token)
                .AddRequiredParam("attribute", attribute)
                .AddRequiredParam("attributeValue", attributeValue)
                .BuildAsJson();
        }

        public static HttpRequestMessage DeleteContent(string token, string[] contentIds)
        {
            return new GoFileRequestBuilder(HttpMethod.Delete, Routes.DeleteContents())
                .WithBearerToken(token)
                .AddRequiredParam("contentsId", string.Join(',', contentIds))
                .BuildAsJson();
        }

        public static HttpRequestMessage GetContents(string token, string contentId, bool noCache = false, string? passwordHash = null)
        {
            return new HttpRequestMessage(HttpMethod.Get, 
                Routes.GetContent(token, contentId, noCache, passwordHash ?? ""));
        }

        public static HttpRequestMessage CreateDirectLink(string token, string contentId, long? expireTimeAsUnixSeconds = null, string[]? sourceIpsAllowed = null, string[]? domainsAllowed = null, string[]? auth = null)
        {
            return new GoFileRequestBuilder(HttpMethod.Post, Routes.PostDirectLink(contentId))
                .WithBearerToken(token)
                .AddOptionalParam("expireTime", expireTimeAsUnixSeconds)
                .AddOptionalParam("sourceIpsAllowed", sourceIpsAllowed)
                .AddOptionalParam("domainsAllowed", domainsAllowed)
                .AddOptionalParam("auth", auth)
                .BuildAsJson();
        }

        public static HttpRequestMessage UdpateDirectLink(string token, string contentId, string directLinkId,
            long? expireTimeAsUnixSeconds = null, string[]? sourceIpsAllowed = null, string[]? domainsAllowed = null,
            string[]? auth = null)
        {
            return new GoFileRequestBuilder(HttpMethod.Put, Routes.PutContentsDirectLink(contentId, directLinkId))
                .WithBearerToken(token)
                .AddOptionalParam("expireTime", expireTimeAsUnixSeconds)
                .AddOptionalParam("sourceIpsAllowed", sourceIpsAllowed)
                .AddOptionalParam("domainsAllowed", domainsAllowed)
                .AddOptionalParam("auth", auth)
                .BuildAsJson();
        }

        public static HttpRequestMessage DeleteDirectLink(string token, string contentId, string directLinkId)
        {
            return new GoFileRequestBuilder(HttpMethod.Delete, Routes.DeleteContentsDirectLink(contentId, directLinkId))
                .WithBearerToken(token)
                .BuildAsJson();
        }

        public static HttpRequestMessage CopyContents(string token, string[] contentsId, string folderId)
        {
            return new GoFileRequestBuilder(HttpMethod.Post, Routes.PostContentsCopy())
                .WithBearerToken(token)
                .AddRequiredParam("contentsId", string.Join(',', contentsId))
                .AddRequiredParam("folderId", folderId)
                .BuildAsJson();
        }

        public static HttpRequestMessage MoveContents(string token, string[] contentsId, string folderId)
        {
            return new GoFileRequestBuilder(HttpMethod.Put, Routes.PutContentsMove())
                .WithBearerToken(token)
                .AddRequiredParam("contentsId", string.Join(',', contentsId))
                .AddRequiredParam("folderId", folderId)
                .BuildAsJson();
        }

        public static HttpRequestMessage GetAccountId(string token)
        {
            return new HttpRequestMessage(HttpMethod.Get, Routes.GetAccountId(token));
        }

        public static HttpRequestMessage GetAccountDetails(string token, string accountId)
        {
            return new HttpRequestMessage(HttpMethod.Get, Routes.GetAccountDetails(token, accountId));
        }

        public static HttpRequestMessage ResetToken(string token, string accountsId)
        {
            return new HttpRequestMessage(HttpMethod.Post, Routes.PostAccountResetToken(token, accountsId));
        }
    }
}