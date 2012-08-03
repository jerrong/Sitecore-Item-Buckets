using Sitecore.SecurityModel;

namespace Sitecore.ItemBucket.Kernel.Pipelines
{
    using System;

    using Sitecore.Data.Items;
    using Sitecore.Data.Managers;
    using Sitecore.Diagnostics;
    using Sitecore.Events;
    using Sitecore.ItemBucket.Kernel.Kernel.Util;
    using Sitecore.ItemBucket.Kernel.Managers;
    using Sitecore.Resources;
    using Sitecore.Shell.Framework.Pipelines;

    public class ItemCopy : CopyItems
    {
        public new void Execute(CopyItemsArgs args)
        {
            Event.RaiseEvent("item:bucketing:copying", args, this);
            Assert.ArgumentNotNull(args, "args");
            var items = GetItems(args);
            Item currentItem = null;
            if (items[0].IsNotNull())
            {
                currentItem = items[0];
            }

            Error.AssertItem(items[0], "Item");
            var database = GetDatabase(args);
            var topParent = database.GetItem(args.Parameters["destination"]);
            if (BucketManager.IsBucket(topParent))
            {
                Shell.Applications.Dialogs.ProgressBoxes.ProgressBox.Execute("Copying Items", "Copying Items", Images.GetThemedImageSource("Business/16x16/chest_add.png"), this.StartProcess, new object[] { currentItem, BucketManager.CreateAndReturnDateFolderDestination(topParent, DateTime.Now), true });
                
                if (currentItem.IsNotNull())
                {
                    Log.Info("Item " + currentItem.ID + " has been copied to another bucket", this);
                }

                Event.RaiseEvent("item:bucketing:copied", args, this);
                args.AbortPipeline();
            }
        }

        private void StartProcess(params object[] parameters)
        {
            var contextItem = (Item)parameters[0];
            var topParent = (Item)parameters[1];
            var recurse = (bool)parameters[2];
            using (new EditContext(contextItem, SecurityCheck.Disable))
            {
                ItemManager.CopyItem(contextItem, BucketManager.CreateAndReturnDateFolderDestination(topParent, DateTime.Now), recurse);
            }
        }
    }
}