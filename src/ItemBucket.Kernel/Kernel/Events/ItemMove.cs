namespace Sitecore.ItemBucket.Kernel.Events
{
    using System;
    using Sitecore.Data;
    using Sitecore.Data.Items;
    using Sitecore.Diagnostics;
    using Sitecore.Events;
    using Sitecore.ItemBucket.Kernel.Kernel.Util;

    /// <summary>
    /// Item Move Event
    /// </summary>
    public class ItemMove
    {
        /// <summary>
        /// Execute the Item Move
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        public void Execute(object sender, EventArgs args)
        {
            var item = Event.ExtractParameter(args, 0) as Item;
            var movedFromId = Event.ExtractParameter(args, 1) as ID;
            Error.AssertNotNull(movedFromId, "MovedFromID");
            Error.AssertItem(item, "Item");
            if (item.IsNotNull())
            {
                Error.AssertItem(Context.ContentDatabase.GetItem(item.Parent.ID), "movedFromFolderItem");
            }
        }
    }
}