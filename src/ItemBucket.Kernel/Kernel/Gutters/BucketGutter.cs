namespace Sitecore.ItemBucket.Kernel.Gutters
{
    using Sitecore.Data.Items;
    using Sitecore.Diagnostics;
    using Sitecore.ItemBucket.Kernel.ItemExtensions.Axes;
    using Sitecore.Shell.Applications.ContentEditor.Gutters;

    /// <summary>
    /// Bucket Gutter
    /// </summary>
    internal class BucketGutter : GutterRenderer
    {
        protected override GutterIconDescriptor GetIconDescriptor(Item item)
        {
            Assert.ArgumentNotNull(item, "item");
            if (!item.IsABucket())
            {
                return null;
            }

            var descriptor = new GutterIconDescriptor
                                 {
                                     Icon = "business/32x32/chest_add.png",
                                     Tooltip = Util.Constants.BucketGutterWarning
                                 };
            return descriptor;
        }
    }
}
