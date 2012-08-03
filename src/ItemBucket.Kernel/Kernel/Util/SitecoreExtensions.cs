namespace Sitecore.ItemBucket.Kernel.Kernel.Util
{
    using System;

    using Sitecore.Shell.Framework.Commands;

    public static class SitecoreExtensions
    {
        public static bool CheckCommandContextForItemCount(this CommandContext context, int numberOfItems)
        {
            return context.Items.Length == numberOfItems;
        }

        public static bool Between<T>(this T actual, T lower, T upper) where T : IComparable<T>
        {
            return actual.CompareTo(lower) >= 0 && actual.CompareTo(upper) < 0;
        }

        public static bool IsEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        public static bool IsNotEmpty(this string value)
        {
            return value.IsEmpty() == false;
        }

        public static bool IsNumeric(this string value)
        {
            float output;
            return float.TryParse(value, out output);
        }
    }
}
