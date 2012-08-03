namespace Sitecore.ItemBucket.Kernel.Pipelines
{
    using System.Collections;
    using System.Linq;

    using Sitecore.Data.Items;
    using Sitecore.Diagnostics;
    using Sitecore.Events;
    using Sitecore.ItemBucket.Kernel.ItemExtensions.Axes;
    using Sitecore.ItemBucket.Kernel.Kernel.Util;
    using Sitecore.ItemBucket.Kernel.Managers;
    using Sitecore.Shell.Framework.Pipelines;

    internal class BucketItemClone : CloneItems
    {
        // Methods
        public new void Execute(CopyItemsArgs args)
        {
            Event.RaiseEvent("item:bucketing:cloning", args, this);
            Assert.ArgumentNotNull(args, "args");
            var items = GetItems(args);
            if (args.IsNotNull())
            {
                var itemId = args.Parameters["destination"];
                if (itemId.IsNotNull())
                {
                    var database = GetDatabase(args);
                    if (database.GetItem(itemId).IsBucketItemCheck())
                    {
                        var list = new ArrayList();
                        foreach (var item3 in from item2 in items
                                              where item2.IsNotNull()
                                              let item = BucketManager.CreateAndReturnDateFolderDestination(database.GetItem(itemId), item2)
                                              let copyOfName = ItemUtil.GetCopyOfName(item, item2.Name)
                                              select item2.CloneTo(item, copyOfName, true))
                        {
                            list.Add(item3);
                        }

                        args.Copies = list.ToArray(typeof(Item)) as Item[];
                        Event.RaiseEvent("item:bucketing:cloned", args, this);
                        args.AbortPipeline();
                    }
                }
            }
        }
    }
}
