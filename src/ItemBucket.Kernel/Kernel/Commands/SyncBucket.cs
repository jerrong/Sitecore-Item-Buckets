using Sitecore.ItemBucket.Kernel.Kernel.Util;
using Sitecore.SecurityModel;

namespace Sitecore.ItemBucket.Kernel.Commands
{
    using Sitecore.Data.Items;
    using Sitecore.Diagnostics;
    using Sitecore.ItemBucket.Kernel.ItemExtensions.Axes;
    using Sitecore.ItemBucket.Kernel.Managers;
    using Sitecore.ItemBucket.Kernel.Security;
    using Sitecore.Resources;
    using Sitecore.Shell.Framework.Commands;

    internal class SyncBucket : Command
    {
        public override void Execute(CommandContext context)
        {
            Assert.ArgumentNotNull(context, "context");
            Assert.IsNotNull(context.Items, "Context items list is null");
            var contextItem = context.Items[0];
            Util.SearchHelper.AddSearchTab(contextItem, contextItem.GetEditors());
            Shell.Applications.Dialogs.ProgressBoxes.ProgressBox.Execute(Util.Constants.BucketingText, Util.Constants.BucketingProgressText, Images.GetThemedImageSource("people/16x16/box_view.png"), this.StartProcess, new object[] { contextItem });
            Context.ClientPage.SendMessage(this, "item:load(id=" + contextItem.ID + ")");
            Context.ClientPage.SendMessage(this, "item:refreshchildren(id=" + contextItem.Parent.ID + ")");
        }

        private void StartProcess(params object[] parameters)
        {
            var contextItem = (Item)parameters[0];
            BucketManager.CreateBucket(contextItem);
            using (new EditContext(contextItem, SecurityCheck.Disable))
            {
                if (Context.Job.IsNotNull())
                {
                    Context.Job.Status.Messages.Add("Syncing " + contextItem.Paths.FullPath + " item");
                }
                if (!contextItem.IsBucketItemCheck())
                {
                    contextItem.IsBucketItemCheckBox().Checked = true;
                }

                Log.Info("Syncronisation Run on " + contextItem.ID + " bucket", this);
            }
        }

        public override CommandState QueryState(CommandContext context)
        {
            Error.AssertObject(context, "context");
            var item = context.Items[0];
            var bucketManager = new BucketSecurityManager(item);

            if (!bucketManager.IsAllowedToCreateBucket)
            {
                return CommandState.Disabled;
            }

            if (!item.Locking.HasLock())
            {
                return CommandState.Disabled;
            }

            return item.IsBucketItemCheck() ? CommandState.Enabled : CommandState.Disabled;
        }
    }
}
