using System;
using System.Collections.Generic;
using System.Text;
using GoFileSharp.Controllers;
using GoFileSharp.Model.GoFileData;

/* TODO:
 * [ ] Download file
 * [ ] copy file
 * [ ] delete file
 */

namespace GoFileSharp.Model.GoFileData.Wrappers
{
    /// <summary>
    /// A wrapper class for the GoFile <see cref="FileData"/>
    /// </summary>
    /// <remarks>The name is stupid, I know ...</remarks>
    public class GoFileFile : FileData
    {
        private GoFileController _api;
        public GoFileFile(FileData file, GoFileController controller) : base(file)
        {
            _api = controller;
        }
    }
}
