// -----------------------------------------------------------------------
// <copyright file="ItemExtensions.cs" company="Sitecore">
// Sitecore.
// </copyright>
// -----------------------------------------------------------------------

namespace Sitecore.ItemBucket.Kernel.Kernel.Util
{
    using Sitecore.Data.Items;

    /// <summary>
    /// Extension Method for an Item
    /// </summary>
    public static class ItemExtensions
    {
        /// <summary>
        /// Extension Method for marking an item as Hidden
        /// </summary>
        /// <param name="item">Item to Hide</param>
        public static void HideItem(this Item item)
        {
            item.Editing.BeginEdit();
            item.Appearance.Hidden = true;
            item.Editing.EndEdit();
        }
    }
}
