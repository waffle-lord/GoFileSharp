﻿using GoFileSharp.Interfaces;

namespace GoFileSharp.Model.GoFileData
{
    public class FileContentOption : IContentOption
    {
        public string OptionName { get; private set; }

        public string Value { get; private set; }

        protected FileContentOption(string optionName, string value)
        {
            OptionName = optionName;
            Value = value;
        }

        public static FileContentOption Name(string newName) => new FileContentOption("name", newName);
    }
}
