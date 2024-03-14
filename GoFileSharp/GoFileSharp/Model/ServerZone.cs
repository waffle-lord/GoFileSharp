using System.ComponentModel;

namespace GoFileSharp.Model
{
    public enum ServerZone
    {
        Any,
        [Description("na")]
        NorthAmerica,
        [Description("eu")]
        Europe
    }
}