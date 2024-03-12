using System;
using Newtonsoft.Json;

namespace GoFileSharp.Model.GoFileData
{
    public class Stats
    {
        [JsonIgnore] 
        public DateTime? Date { get; set; }
        public ulong CdnTraffic { get; set; }
        public ulong FilesCount { get; set; }
        public ulong Storage { get; set; }
    }
}