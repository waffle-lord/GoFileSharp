using System;

namespace GoFileSharp.Model.GoFileData
{
    public class DirectLinkOptions
    {
        public DateTimeOffset? ExpireTime { get; set; }
        public string[]? SourceIpsAllowed { get; set; }
        public string[]? DomainsAllowed { get; set; }
        public string[]? Auth { get; set; }
    }
}