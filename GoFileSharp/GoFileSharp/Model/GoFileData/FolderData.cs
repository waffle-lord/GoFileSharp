using System;
using System.Collections.Generic;
using GoFileSharp.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GoFileSharp.Model.GoFileData
{
    public class FolderData : IContent
    {
        public string Id { get; set; }

        public string Type { get; set; }

        public string Name { get; set; }

        [JsonProperty("parentFolder")] public string ParentFolderId { get; set; }

        /// <summary>
        /// This property has no meaning at this time, but it exists on the API
        /// </summary>
        /// <remarks>You probably want to use <see cref="DirectLink"/> instead</remarks>
        public string[] DirectLinks { get; set; }

        public long CreateTime { get; set; }

        public string Description { get; set; }

        [JsonProperty("password")] public bool HasPassword { get; set; }

        public long Expire { get; set; }

        public string Tags { get; set; }

        [JsonProperty("childs")] public string[] ChildIds { get; set; }

        public string Code { get; set; }

        [JsonProperty] private Dictionary<string, object> Contents { get; set; } = new Dictionary<string, object>();
        public List<IContent> Children { get; set; } = new List<IContent>();

        [JsonProperty("public")] public bool IsPublic { get; set; }

        public bool IsRoot { get; set; } = false;

        protected void Update(FolderData folder)
        {
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
            ChildIds = folder.ChildIds;
            Children = folder.Children;
            Code = folder.Code;
            IsPublic = folder.IsPublic;
            IsRoot = folder.IsRoot;
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

                foreach (object o in folderData.Contents.Values)
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