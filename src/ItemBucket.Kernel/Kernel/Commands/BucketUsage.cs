namespace Sitecore.ItemBucket.Kernel.Commands
{
    using Sitecore.Globalization;
    using Sitecore.ItemBucket.Kernel.Kernel.Commands;
    using Sitecore.Shell.Framework;
    using Sitecore.Shell.Framework.Commands;

    internal class BucketUsage : BaseCommand
    {
        /// <summary>
        /// Command used to determine how much your buckets are being used
        /// </summary>
        /// <param name="context">Commad Context</param>
        public override void Execute(CommandContext context)
        {
            Windows.RunUri("control:Bucket.Usage", "Business/16x16/data_information.png", Translate.Text("Bucket Usage"));
        }
    }
}
