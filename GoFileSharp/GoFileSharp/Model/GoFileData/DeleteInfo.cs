using System.Collections.Generic;
using System.Linq;

namespace GoFileSharp.Model.GoFileData
{
    public class DeleteInfo
    {
        public bool IsOk;
        public readonly Dictionary<string, string> RemovalStatus;
        
        protected DeleteInfo(Dictionary<string, string> deletions)
        {
            RemovalStatus = deletions;
            if (RemovalStatus.Count > 0)
            {
                IsOk = RemovalStatus.Count(x => x.Value == "ok") == RemovalStatus.Count;
                return;
            }

            IsOk = false;
        }

        public static DeleteInfo WithData(Dictionary<string, string> data) => new DeleteInfo(data);
        public static DeleteInfo NoData() => new DeleteInfo(new Dictionary<string, string>());
    }
}