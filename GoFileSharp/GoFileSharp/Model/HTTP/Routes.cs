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
    }
}
