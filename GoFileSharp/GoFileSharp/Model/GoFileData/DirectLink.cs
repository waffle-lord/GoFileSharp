using System.Collections.Generic;
using Newtonsoft.Json;

namespace GoFileSharp.Model.GoFileData
{
    public class DirectLink
    {
        public List<string> Auth { get; set; }
        public List<string> DomainsAllowed { get; set; }
        public ulong ExpireTime { get; set; }
        public bool IsReqLink { get; set; }
        public List<string> SourceIpsAllowed { get; set; }
        [JsonProperty("DirectLink")]
        public string Link { get; set; }
    }
}