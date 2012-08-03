namespace Sitecore.ItemBucket.Kernel.Events
{
    using System;
    using System.Collections.Generic;
    using Sitecore.Data.Items;
    using Sitecore.Diagnostics;
    using Sitecore.Events;
    using Sitecore.ItemBucket.Kernel.Kernel.Util;
    using Sitecore.ItemBucket.Kernel.Managers;
    using Sitecore.Web.UI.Sheer;

    /// <summary>
    /// Hook for Adding Events
    /// </summary>
    public class AddingVersions
    {
        /// <summary>
        /// Temporary Items
        /// </summary>
        private List<Item> listToTemporarilyShow = new List<Item>();

        /// <summary>
        /// Execute Event
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
            Error.AssertItem(item, "Item");
            var clientPipelineArgs = args as ClientPipelineArgs;
            if (clientPipelineArgs.IsNotNull())
            {
                clientPipelineArgs.Parameters.Add("root", item.ID.ToString());
            }

            //this.LoopThroughAscendants(item, item.GetParentBucketItemOrSiteRoot());
            //this.listToTemporarilyShow.ForEach(HideItem);
        }

        /// <summary>
        /// Hide Item
        /// </summary>
        /// <param name="itemsToShow">
        /// The items to show.
        /// </param>
        private static void HideItem(Item itemsToShow)
        {
            itemsToShow.HideItem();
        }

        /// <summary>
        /// Loop Ancestors
        /// </summary>
        /// <param name="itm">
        /// The itm.
        /// </param>
        /// <param name="parent">
        /// The parent.
        /// </param>
        /// <returns>
        /// Item that matched
        /// </returns>
        private Item LoopThroughAscendants(Item itm, Item parent)
        {
            if (itm.ID == parent.ID)
            {
                return itm;
            }

            this.listToTemporarilyShow.Add(itm);
            return this.LoopThroughAscendants(itm.Parent, parent);
        }
    }
}