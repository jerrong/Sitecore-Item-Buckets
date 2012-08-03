namespace Sitecore.ItemBucket.Kernel.Events
{
    using System;
    using Sitecore.Data.Items;
    using Sitecore.Data.Managers;
    using Sitecore.Diagnostics;
    using Sitecore.ItemBucket.Kernel.Kernel.Util;
    using Sitecore.Web.UI.Sheer;

    /// <summary>
    /// Copy Item Event
    /// </summary>
    public class ItemCopy
    {
        /// <summary>
        /// Execute Item Copy
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        public void Execute(object sender, EventArgs args)
        {
            var item = Sitecore.Events.Event.ExtractParameter(args, 0) as Item;
            var copiedFromId = Sitecore.Events.Event.ExtractParameter(args, 1) as Item;
            Error.AssertNotNull(copiedFromId, "copiedFromId");
            Error.AssertItem(item, "Item");
            var cpa = new ClientPipelineArgs();
            if (item.IsNotNull())
            {
                cpa.Parameters.Add("id", item.ID.ToString());
            }

            if (copiedFromId.IsNotNull())
            {
                cpa.Parameters.Add("copiedFromId", copiedFromId.ID.ToString());
            }

            Context.ClientPage.Start(this, "DialogCreateRedirect", cpa);
        }

        /// <summary>
        /// Create Redirect
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>
        protected void DialogCreateRedirect(ClientPipelineArgs args)
        {
            var masterdb = Context.ContentDatabase;
            var item = masterdb.GetItem(args.Parameters["id"]);
            var copiedFromFolderItem = masterdb.GetItem(args.Parameters["copiedFromId"]);
            Error.AssertItem(item, "item");
            Error.AssertItem(copiedFromFolderItem, "copiedFromFolderItem");
            var copiedToFolderItem = copiedFromFolderItem;
            if (args.IsPostBack)
            {
                string res = args.Result;
                if (res == "yes")
                {
                    ItemManager.CopyItem(item, copiedToFolderItem, true);
                    Log.Info("Item " + item.ID + " has been copied to another bucket located here " + copiedToFolderItem.ID, this);
                }
                else
                {
                    args.Result = string.Empty;
                    args.IsPostBack = false;
                    return;
                }
            }
        }
    }
}