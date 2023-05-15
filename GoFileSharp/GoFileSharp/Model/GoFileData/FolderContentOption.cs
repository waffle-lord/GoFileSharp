using GoFileSharp.Interfaces;
using System;
using System.Collections.Generic;

namespace GoFileSharp.Model.GoFileData
{
    public class FolderContentOption : IContentOption
    {
        public string OptionName { get; private set; }
        public string Value { get; private set; }

        protected FolderContentOption(string optionName, string value)
        {
            OptionName = optionName;
            Value = value;
        }

        public static FolderContentOption Public(bool value) => new FolderContentOption("public", value.ToString().ToLower());

        public static FolderContentOption Password(string password) => new FolderContentOption("password", password);

        public static FolderContentOption Description(string description) => new FolderContentOption("description", description);

        public static FolderContentOption Expire(DateTimeOffset expireDate) => new FolderContentOption("expire", expireDate.ToUnixTimeMilliseconds().ToString());

        public static FolderContentOption Tags(List<string> tags) => new FolderContentOption("tags", string.Join(',', tags));
    }
}
