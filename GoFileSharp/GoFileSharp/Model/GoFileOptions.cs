namespace GoFileSharp.Model
{
    public class GoFileOptions
    {
        /// <summary>
        /// The token to use with API calls
        /// </summary>
        public string ApiToken = null;

        /// <summary>
        /// The preferred server zone to use for file uploads
        /// </summary>
        /// <remarks>Defaults to 'none' which will use the first server returned from GoFile</remarks>
        public ServerZone PreferredZone = ServerZone.Any;
    }
}