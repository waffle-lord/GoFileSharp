using System;
using System.Collections.Generic;
using System.Text;

namespace GoFileSharp.Model.GoFileData
{
    public class AccountDetails
    {
        public string Token { get; set; }

        public string Email { get; set; }

        public string Tier { get; set; }

        public string RootFolder { get; set; }

        public ulong FoldersCount { get; set; }

        public ulong FilesCount { get; set; }

        public ulong TotalSize { get; set; }
        
        public ulong DownloadCount { get; set; }
    }
}