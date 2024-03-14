using System;
using System.ComponentModel;

namespace GoFileSharp.Extensions
{
    public static class EnumExtensions
    {
        public static string? GetDescription(this Enum e)
        {
            var memberInfo = e.GetType().GetMember(e.ToString())[0];

            var blah = memberInfo.GetCustomAttributes(typeof(DescriptionAttribute), inherit: false);

            var descAttribute = blah.Length > 0 ? blah[0] as DescriptionAttribute : null;

            return descAttribute?.Description;
        }
    }
}