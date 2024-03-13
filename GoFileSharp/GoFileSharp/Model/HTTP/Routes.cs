namespace GoFileSharp.Model.HTTP
{
    
    /* TODO: update API routes
     * [X] GET /servers
     * [X] GET /accounts/getId
     * [X] GET /accounts/{accountId}
     * [X] GET /contents/{contentId}
     * [X] POST /accounts/{accountId}/resetToken
     * [X] POST /contents/uploadFile
     * [X] POST /contents/createFolder
     * [X] POST /contents/copy
     * [-] POST /contents/{contentId}/copy - ignoring for now
     * [X] POST /contents/{contentId}/directLinks
     * [X] PUT /contents/move
     * [-] PUT /contents/{contentId}/move - ignoring for now
     * [X] PUT /contents/{contentId}/update
     * [X] PUT /contents/{contentId}/directLinks/{directLinkId}
     * [X] DELETE /contents
     * [-] DELETE /contents/{contentId} - ignoring for now
     * [X] DELETE /contents/{contentId}/directLinks/{directLinkId}
     */
    
    
    internal static class Routes
    {
        private static string _baseUrl = "https://api.gofile.io";

        #region GET
        internal static string GetServers(string zoneId)
        {
            return string.IsNullOrWhiteSpace(zoneId) 
                ? $"{_baseUrl}/servers" 
                : $"{_baseUrl}/servers?zone={zoneId}";
        }

        internal static string GetContent(string token, string contentId, bool noCache, string passwordHash)
        {
            string route = $"{_baseUrl}/contents/{contentId}?token={token}";

            if (noCache) route += "&cache=false";

            if (!string.IsNullOrWhiteSpace(passwordHash)) route += $"&password={passwordHash}";

            return route;
        }
        
        internal static string GetAccountId(string token)
        {
            return $"{_baseUrl}/accounts/getId?token={token}&allDetails=true";
        }
        
        internal static string GetAccountDetails(string token, string accountId)
        {
            return $"{_baseUrl}/accounts/{accountId}?token={token}";
        }
        #endregion
        
        #region PUT
        internal static string PutContentsUpdate(string contentId)
        {
            return $"{_baseUrl}/contents/{contentId}/update";
        }
        
        internal static string PutContentsMove()
        {
            return $"{_baseUrl}/contents/move";
        }

        internal static string PutContentsDirectLink(string contentId, string directLinkId)
        {
            return $"{_baseUrl}/contents/{contentId}/directlinks/{directLinkId}";
        }
        #endregion
        
        #region POST
        internal static string PostUploadFile(string server)
        {
            return $"{_baseUrl}/contents/uploadfile".Replace("api", server);
        }
        
        internal static string PostAccountResetToken(string token, string accountId)
        {
            return $"{_baseUrl}/accounts/{accountId}/resettoken?token={token}";
        }
        
        internal static string PostCreateFolder()
        {
            return $"{_baseUrl}/contents/createfolder";
        }

        internal static string PostContentsCopy()
        {
            return $"{_baseUrl}/contents/copy";
        }
        
        internal static string PostDirectLink(string contentId)
        {
            return $"{_baseUrl}/contents/{contentId}/directlinks";
        }
        #endregion
        
        #region DELETE
        internal static string DeleteContents()
        {
            return $"{_baseUrl}/contents";
        }

        internal static string DeleteContentsDirectLink(string contentId, string directLinkId)
        {
            return $"{_baseUrl}/contents/{contentId}/directlinks/{directLinkId}";
        }
        #endregion
    }
}
