using GoFileSharp.Interfaces;
using GoFileSharp.Model.GoFileData;
using Newtonsoft.Json;
using System;

namespace GoFileSharp.Builders
{
    internal class ContentInfoBuilder
    {
        ProxyContentInfo _info;

        public ContentInfoBuilder(ProxyContentInfo proxyContent)
        {
            _info = proxyContent;
        }

        private IContent? GetContentData(string json)
        {
            try
            {
                var data = JsonConvert.DeserializeObject<FileData>(json, new JsonSerializerSettings() { MissingMemberHandling = MissingMemberHandling.Error });

                return data;
            }
            catch (Exception)
            {
                //objec is not a file
            }

            try
            {
                var data = JsonConvert.DeserializeObject<FolderData>(json, new JsonSerializerSettings() { MissingMemberHandling = MissingMemberHandling.Error });

                return data;
            }
            catch(Exception)
            {
                //object is not a folder
            }

            return null;
        }

        public ContentInfo? Build()
        {
            if (_info == null) return null;

            ContentInfo content = new ContentInfo();

            content.Id = _info.Id;
            content.Name = _info.Name;
            content.Type = _info.Type;
            content.ParentFolderId = _info.ParentFolderId;
            content.Code = _info.Code;
            content.CreateTime = _info.CreateTime;
            content.IsPublic = _info.IsPublic;
            content.IsRoot = _info.IsRoot;
            content.Childs = _info.Childs;

            foreach (object o in _info.Contents.Values)
            {
                var child = GetContentData(o.ToString() ?? "");

                if(child != null)
                {
                    content.Contents.Add(child);
                }
            }

            return content;
        }
    }
}
