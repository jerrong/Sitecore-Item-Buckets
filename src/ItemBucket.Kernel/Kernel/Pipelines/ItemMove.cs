using Sitecore.SecurityModel;

namespace Sitecore.ItemBucket.Kernel.Pipelines
{
    using System;

    using Sitecore.Data.Items;
    using Sitecore.Data.Managers;
    using Sitecore.Diagnostics;
    using Sitecore.Events;
    using Sitecore.ItemBucket.Kernel.Managers;
    using Sitecore.Resources;
    using Sitecore.Shell.Framework.Pipelines;
    using Sitecore.Web.UI.Sheer;

    public class ItemMove : MoveItems
    {
        public new void Execute(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            var contentDatabase = Context.ContentDatabase;
            if (BucketManager.IsBucket(contentDatabase.GetItem(args.Parameters["target"])))
            {
                var item = contentDatabase.GetItem(args.Parameters["items"]);
                Event.RaiseEvent("item:bucketing:moving", args, this);
                Shell.Applications.Dialogs.ProgressBoxes.ProgressBox.Execute("Moving Items", "Moving Items", Images.GetThemedImageSource("Business/16x16/chest_add.png"), this.StartProcess, new object[] { item, BucketManager.CreateAndReturnDateFolderDestination(contentDatabase.GetItem(args.Parameters["target"]), DateTime.Now) });
                Event.RaiseEvent("item:bucketing:moved", args, this);
                Log.Info("Item " + item.ID + " has been moved to another bucket", this);
                Context.ClientPage.SendMessage(this, "item:refreshchildren(id=" + item.Parent.ID + ")");
                args.AbortPipeline();
            }
        }

        private void StartProcess(params object[] parameters)
        {
            var item = (Item)parameters[0];
            var topParent = (Item)parameters[1];
            using (new EditContext(item, SecurityCheck.Disable))
            {
                ItemManager.MoveItem(item, topParent);
            }
        }
    }
}