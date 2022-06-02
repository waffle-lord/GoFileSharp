using GoFileSharp.Interfaces;
using System.Collections.Generic;

namespace GoFileSharp.Model.GoFileData
{
    public class ContentInfo
    {
        public string Id { get; set; }

        public string Type { get; set; }

        public string Name { get; set; }

        public string ParentFolderId { get; set; }

        public long CreateTime { get; set; }

        public string[] Childs { get; set; }

        public string Code { get; set; }

        public bool IsPublic { get; set; }

        public bool IsRoot { get; set; } = false;

        public List<IContent> Contents { get; set; } = new List<IContent>();

        public ContentInfo()
        {
        }

        public ContentInfo(ContentInfo content)
        {
            Id = content.Id;
            Type = content.Type;
            Name = content.Name;
            Code = content.Code;
            IsRoot = content.IsRoot;
            Childs = content.Childs;
            IsPublic = content.IsPublic;
            Contents = content.Contents;
            CreateTime = content.CreateTime;
            ParentFolderId = content.ParentFolderId;
        }
    }
}
