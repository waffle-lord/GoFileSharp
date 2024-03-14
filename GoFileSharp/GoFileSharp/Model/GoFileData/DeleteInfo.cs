using System.Collections.Generic;
using System.Linq;

namespace GoFileSharp.Model.GoFileData
{
    public class DeleteInfo
    {
        public bool IsOk() => Status == "ok";
        public string Status { get; set; }
        public Dictionary<string, DeleteInfo> Data { get; set; }
    }
}