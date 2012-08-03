namespace Sitecore.ItemBucket.Kernel.Commands
{
    using System.Collections.Specialized;
    using Sitecore.Diagnostics;
    using Sitecore.ItemBucket.Kernel.ItemExtensions.Axes;
    using Sitecore.ItemBucket.Kernel.Kernel.Pipelines;
    using Sitecore.ItemBucket.Kernel.Security;
    using Sitecore.Shell.Framework.Commands;

    /// <summary>
    /// Unmake a Bucket
    /// </summary>
    internal class UnMakeBucket : Command
    {
        public override void Execute(CommandContext context)
        {
            Assert.ArgumentNotNull(context, "context");
            Assert.IsNotNull(context.Items, "Context items list is null");
            var parameters = new NameValueCollection();
            Context.ClientPage.Start("uiUnBucketItems", new BucketArgs(context.Items[0], parameters));
        }

        public override CommandState QueryState(CommandContext context)
        {
            Error.AssertObject(context, "context");
            var item = context.Items[0];
            var bucketManager = new BucketSecurityManager(item);

            if (!bucketManager.IsUnMakeBucketAllowed)
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