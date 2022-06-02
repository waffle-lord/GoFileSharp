using GoFileSharp.Interfaces;
using Newtonsoft.Json;

namespace GoFileSharp.Model.GoFileData
{
    public class FolderData : IContent
    {
        public string Id { get; set; }

        public string Type { get; set; }

        public string Name { get; set; }

        [JsonProperty("parentFolder")]
        public string ParentFolderId { get; set; }

        public long CreateTime { get; set; }

        public string[] Childs { get; set; }

        public string Code { get; set; }

        [JsonProperty("public")]
        public bool IsPublic { get; set; }

        public bool IsRoot { get; set; } = false;
    }
}