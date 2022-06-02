using Newtonsoft.Json;

namespace GoFileSharp.Interfaces
{
    public interface IContent
    {
        public string Id { get; set; }

        public string Type { get; set; }

        public string Name { get; set; }

        [JsonProperty("parentFolder")]
        public string ParentFolderId { get; set; }

        public long CreateTime { get; set; }
    }
}