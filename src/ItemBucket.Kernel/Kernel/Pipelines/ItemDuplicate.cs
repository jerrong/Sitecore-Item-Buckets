using Sitecore.ItemBucket.Kernel.Templates;

namespace Sitecore.ItemBucket.Kernel.Pipelines
{
    using System;

    using Sitecore.Configuration;
    using Sitecore.Data.Managers;
    using Sitecore.Diagnostics;
    using Sitecore.Events;
    using Sitecore.Globalization;
    using Sitecore.ItemBucket.Kernel.Kernel.Util;
    using Sitecore.ItemBucket.Kernel.Managers;
    using Sitecore.Shell.Framework.Pipelines;
    using Sitecore.Web.UI.Sheer;

    public class ItemDuplicate : DuplicateItem
    {
        public new void Execute(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            var database = Factory.GetDatabase(args.Parameters["database"]);
            Assert.IsNotNull(database, args.Parameters["database"]);
            var str = args.Parameters["id"];
            var item = database.Items[str];
            if (item.IsNull())
            {
                SheerResponse.Alert("Item not found.", new string[0]);
                args.AbortPipeline();
            }
            else
            {
                var parent = item.Parent;
                if (parent.IsNull())
                {
                    SheerResponse.Alert("Cannot duplicate the root item.", new string[0]);
                    args.AbortPipeline();
                }
                else if (parent.Access.CanCreate())
                {
                    Log.Audit(this, "Duplicate item: {0}", new[] { AuditFormatter.FormatItem(item) });

                    var parentBucketOfItem = item.GetParentBucketItemOrSiteRoot();
                    if (BucketManager.IsBucket(parentBucketOfItem))
                    {
                       
                        var duplicatedItem = Context.Workflow.DuplicateItem(item, args.Parameters["name"]);
                        var newDestination = BucketManager.CreateAndReturnDateFolderDestination(parentBucketOfItem, DateTime.Now);
                        if (!item.Template.IsBucketTemplateCheck())
                        {
                            newDestination = parentBucketOfItem;
                        }
                        Event.RaiseEvent("item:bucketing:duplicating", args, this);
                        ItemManager.MoveItem(duplicatedItem, newDestination);
                        Event.RaiseEvent("item:bucketing:duplicated", args, this);
                        Log.Info("Item " + duplicatedItem.ID + " has been duplicated to another bucket", this);
                    }
                    else
                    {
                        Context.Workflow.DuplicateItem(item, args.Parameters["name"]);
                    }
                }
                else
                {
                    SheerResponse.Alert(Translate.Text("You do not have permission to duplicate \"{0}\".", new object[] { item.DisplayName }), new string[0]);
                    args.AbortPipeline();
                }
            }

            args.AbortPipeline();
        }
    }
}
