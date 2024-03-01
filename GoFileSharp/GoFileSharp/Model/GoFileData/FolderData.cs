using System;
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
        
        /// <summary>
        /// This property has no meaning at this time, but it exists on the API
        /// </summary>
        /// <remarks>You probably want to use <see cref="DirectLink"/> instead</remarks>
        public string[] DirectLinks { get; set; }

        public long CreateTime { get; set; }
        
        public string Description { get; set; }
        
        public bool Password { get; set; }
        
        public long Expire { get; set; }
        
        public string Tags { get; set; }

        public string[] Childs { get; set; }

        public string Code { get; set; }

        [JsonProperty("public")]
        public bool IsPublic { get; set; }
        
        public bool IsRoot { get; set; } = false;
    }
}