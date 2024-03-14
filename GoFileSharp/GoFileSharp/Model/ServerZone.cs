using System.ComponentModel;

namespace GoFileSharp.Model
{
    public enum ServerZone
    {
        None,
        [Description("na")]
        NorthAmerica,
        [Description("eu")]
        Europe
    }
}