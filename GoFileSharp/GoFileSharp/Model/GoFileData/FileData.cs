using GoFileSharp.Interfaces;
using GoFileSharp.Model.GoFileData.Wrappers;
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
        
        /// <summary>
        /// This property has no meaning at this time, but it exists on the API
        /// </summary>
        /// <remarks>You probably want to use <see cref="DirectLink"/> instead</remarks>
        public string[] DirectLinks { get; set; }
        public string DirectLink { get; set; }

        public string Link { get; set; }

        public void Update(FileData file)
        {
            Id = file.Id;
            Md5 = file.Md5;
            Name = file.Name;
            Size = file.Size;
            Type = file.Type;
            Link = file.Link;
            MimeType = file.MimeType;
            DirectLink = file.DirectLink;
            DirectLinks = file.DirectLinks;
            CreateTime = file.CreateTime;
            ServerChosen = file.ServerChosen;
            DownloadCount = file.DownloadCount;
            ParentFolderId = file.ParentFolderId;
        }

        public FileData()
        {
        }

        public FileData(FileData file)
        {
            Update(file);
        }
    }
}