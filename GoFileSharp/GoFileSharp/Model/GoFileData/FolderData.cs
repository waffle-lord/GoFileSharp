using System;
using System.Collections.Generic;
using GoFileSharp.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GoFileSharp.Model.GoFileData
{
    public class FolderData : IContent
    {
        private string _id;
        
        public bool IsOwner { get; set; }

        public string Id
        {
            get => _id ?? FolderId;
            set => _id = value;
        }
        
        // api returns folderId when creating a folder instead of id
        [JsonProperty]
        private string FolderId { get; set; }

        public string Type { get; set; }

        public string Name { get; set; }
        
        public string Code { get; set; }
        
        [JsonProperty("parentFolder")]
        public string ParentFolderId { get; set; }
        
        public long CreateTime { get; set; }
        
        public bool IsRoot { get; set; } = false;
        
        [JsonProperty("public")] 
        public bool IsPublic { get; set; }
        
        public ulong TotalDownloadCount { get; set; }
        
        public ulong TotalSize { get; set; }
        
        public Dictionary<string, DirectLink> DirectLinks { get; set; }

        public string Description { get; set; }

        [JsonProperty("password")] 
        public bool HasPassword { get; set; }

        public long Expire { get; set; }

        public string Tags { get; set; }
        
        public string[] ChildrenIds { get; set; }
        
        [JsonProperty("children")] 
        private Dictionary<string, object> ChildrenDictionary { get; set; } = new Dictionary<string, object>();
        
        [JsonIgnore]
        public List<IContent> Children { get; set; } = new List<IContent>();
        
        protected void Update(FolderData folder)
        {
            IsOwner = folder.IsOwner;
            Id = folder.Id;
            Type = folder.Type;
            Name = folder.Name;
            ParentFolderId = folder.ParentFolderId;
            DirectLinks = folder.DirectLinks;
            CreateTime = folder.CreateTime;
            Description = folder.Description;
            HasPassword = folder.HasPassword;
            Expire = folder.Expire;
            Tags = folder.Tags;
            ChildrenIds = folder.ChildrenIds;
            Children = folder.Children;
            Code = folder.Code;
            IsPublic = folder.IsPublic;
            IsRoot = folder.IsRoot;
            TotalDownloadCount = folder.TotalDownloadCount;
            TotalSize = folder.TotalSize;
        }

        public FolderData()
        {
        }

        public FolderData(FolderData folder)
        {
            Update(folder);
        }

        private static IContent? GetContentData(JObject jObject)
        {
            if (FileData.TryParse(jObject, out FileData file))
                return file;

            if (FolderData.TryParse(jObject, out FolderData folder))
                return folder;

            return null;
        }

        public static bool TryParse(JObject jObject, out FolderData folderData)
        {
            try
            {
                folderData = jObject.ToObject<FolderData>();

                if (folderData == null || folderData.Type != "folder")
                    return false;

                foreach (object o in folderData.ChildrenDictionary.Values)
                {
                    var child = GetContentData(JObject.Parse(o.ToString() ?? ""));

                    if (child != null)
                    {
                        folderData.Children.Add(child);
                    }
                }

                return true;
            }
            catch (Exception)
            {
                folderData = null;
                return false;
            }
        }
    }
}