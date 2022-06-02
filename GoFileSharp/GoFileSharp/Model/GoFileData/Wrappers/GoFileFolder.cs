using System;
using System.Collections.Generic;
using System.Text;
using GoFileSharp.Controllers;
using GoFileSharp.Model.GoFileData;

/* TODO:
 * [ ] Download folder
 * [ ] upload to folder
 * [ ] copy folder
 * [ ] delete folder
 */

namespace GoFileSharp.Model.GoFileData.Wrappers
{
    /// <summary>
    /// A wrapper class for the GoFile <see cref="ContentInfo"/>
    /// </summary>
    public class GoFileFolder : ContentInfo
    {
        private GoFileController _api;

        public GoFileFolder(ContentInfo content, GoFileController controller) : base(content)
        {
            _api = controller;
        }
    }
}
