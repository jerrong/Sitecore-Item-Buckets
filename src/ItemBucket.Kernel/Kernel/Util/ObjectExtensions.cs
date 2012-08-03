// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ObjectExtensions.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the ObjectExtensions type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.ItemBucket.Kernel.Kernel.Util
{
    using System;

    public static class ObjectExtensions
    {
        public static T IfNull<T>(this T obj, Action action) where T : class
        {
            if (obj == null)
            {
                action();
            }

            return obj;
        }

        public static bool IsNull(this object o)
        {
            return o == null;
        }

        public static bool IsNotNull(this object o)
        {
            return o != null;
        }

        public static T IfNull<T>(this T obj, Func<T> func) where T : class
        {
            if (obj == null)
            {
                return func();
            }

            return obj;
        }

        public static T IfNotNull<T>(this T obj, Action<T> action) where T : class
        {
            if (obj != null)
            {
                action(obj);
            }

            return obj;
        }

        public static T IfNotNull<T>(this T obj, Action action) where T : class
        {
            if (obj != null)
            {
                action();
            }

            return obj;
        }
    }
}
