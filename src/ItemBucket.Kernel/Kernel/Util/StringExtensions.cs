using Sitecore.ItemBucket.Kernel.Util;

namespace Sitecore.ItemBucket.Kernel.Kernel.Util
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;

    public static class StringExtensions
    {
        public static bool Matches(this string source, string compare)
        {
            return string.Equals(source, compare, StringComparison.OrdinalIgnoreCase);
        }

        public static string ToDelimitedList(this IEnumerable<string> list)
        {
            return ToDelimitedList(list, ",");
        }

        public static bool IsGuid(this string s)
        {
            return IdHelper.IsGuid(s);
        }

        public static string ToDelimitedList(this IEnumerable<string> list, string delimiter)
        {
            var sb = new StringBuilder();
            foreach (var s in list)
            {
                sb.Append(string.Concat(s, delimiter));
            }

            var result = sb.ToString();
            result = Chop(result);
            return result;
        }

        public static string Chop(this string sourceString)
        {
            return Chop(sourceString, 1);
        }

        public static string Chop(this string sourceString, int removeFromEnd)
        {
            var result = sourceString;
            if ((removeFromEnd > 0) && (sourceString.Length > removeFromEnd - 1))
            {
                result = result.Remove(sourceString.Length - removeFromEnd, removeFromEnd);
            }

            return result;
        }
     
        public static string Chop(this string sourceString, string backDownTo)
        {
            var removeDownTo = sourceString.LastIndexOf(backDownTo);
            var removeFromEnd = 0;
            if (removeDownTo > 0)
            {
                removeFromEnd = sourceString.Length - removeDownTo;
            }

            var result = sourceString;

            if (sourceString.Length > removeFromEnd - 1)
            {
                result = result.Remove(removeDownTo, removeFromEnd);
            }

            return result;
        }

        public static bool IsNullOrEmpty(this string sourceString)
        {
            return string.IsNullOrEmpty(sourceString);
        }
    }
}
