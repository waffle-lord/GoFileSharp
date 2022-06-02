using Newtonsoft.Json;

namespace GoFileSharp.Model.GoFileData
{
    public class UploadInfo
    {
        public string DownloadPage { get; set; }

        public string Code { get; set; }

        [JsonProperty("parentFolder")]
        public string ParentFolderId { get; set; }

        public string FileId { get; set; }

        public string FileName { get; set; }

        public string Md5 { get; set; }
    }
}