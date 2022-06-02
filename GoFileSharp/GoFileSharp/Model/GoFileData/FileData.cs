using GoFileSharp.Interfaces;
using Newtonsoft.Json;

namespace GoFileSharp.Model.GoFileData
{
    public class FileData : IContent
    {
        public string Id { get; set; }

        public string Type { get; set; }

        public string Name { get; set; }

        [JsonProperty("parentFolder")]
        public string ParentFolderId { get; set; }

        public long CreateTime { get; set; }

        public long Size { get; set; }

        public long DownloadCount { get; set; }

        public string Md5 { get; set; }

        public string MimeType { get; set; }

        [JsonProperty("serverchoosen")] //typo in API
        public string ServerChosen { get; set; }

        public string DirectLink { get; set; }

        public string Link { get; set; }

        public FileData()
        {
        }

        public FileData(FileData file)
        {
            Id = file.Id;
            Name = file.Name;
            Size = file.Size;
            CreateTime = file.CreateTime;
            ServerChosen = file.ServerChosen;
            DirectLink = file.DirectLink;
            DownloadCount = file.DownloadCount;
            ParentFolderId = file.ParentFolderId;
        }
    }
}