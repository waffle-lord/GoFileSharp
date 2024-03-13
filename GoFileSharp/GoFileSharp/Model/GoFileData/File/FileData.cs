using System;
using System.Collections.Generic;
using GoFileSharp.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
        
        public string ServerSelected { get; set; }
        
        public Dictionary<string, DirectLink> DirectLinks { get; set; }
        

        public string Link { get; set; }

        protected void Update(FileData file)
        {
            Id = file.Id;
            Md5 = file.Md5;
            Name = file.Name;
            Size = file.Size;
            Type = file.Type;
            Link = file.Link;
            MimeType = file.MimeType;
            DirectLinks = file.DirectLinks;
            DirectLinks = file.DirectLinks;
            CreateTime = file.CreateTime;
            ServerSelected = file.ServerSelected;
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

        public static bool TryParse(JObject jObject, string parentId, out FileData fileData)
        {
            try
            {
                fileData = jObject.ToObject<FileData>();

                if (fileData != null) 
                    fileData.ParentFolderId = parentId;
                
                return fileData != null && fileData.Type == "file";
            }
            catch (Exception)
            {
                fileData = null;
                return false;
            }
        }
    }
}