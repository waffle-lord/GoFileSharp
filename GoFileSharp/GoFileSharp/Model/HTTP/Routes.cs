namespace GoFileSharp.Model.HTTP
{
    internal static class Routes
    {
        private static string _baseUrl = "https://api.gofile.io";

        internal static string GetServer()
        {
            return $"{_baseUrl}/getServer";
        }

        internal static string GetContent(string contentId, string token)
        {
            return $"{_baseUrl}/getContent?contentId={contentId}&token={token}";
        }

        internal static string UploadFile(string server)
        {
            return $"{_baseUrl}/uploadFile".Replace("api", server);
        }

        internal static string GetAccountDetails(string token)
        {
            return $"{_baseUrl}/getAccountDetails?token={token}&allDetails=true";
        }

        internal static string CreateFolder()
        {
            return $"{_baseUrl}/createFolder";
        }

        internal static string CopyContent()
        {
            return $"{_baseUrl}/copyContent";
        }

        internal static string DeleteContent()
        {
            return $"{_baseUrl}/deleteContent";
        }

        internal static string SetOption()
        {
            return $"{_baseUrl}/setOption";
        }
    }
}
