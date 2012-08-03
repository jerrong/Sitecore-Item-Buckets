namespace Sitecore.ItemBucket.Kernel.Kernel.Forms.BucketLinkForm
{
    using Sitecore.Diagnostics;
    using Sitecore.Shell.Framework.Commands;
    using Sitecore.Web.UI.Sheer;

    internal class RollbackRefresh : Command
    {
        public override void Execute(CommandContext context)
        {
            Assert.ArgumentNotNull(context, "context");
            SheerResponse.SetLocation(string.Empty);
        }
    }
}
