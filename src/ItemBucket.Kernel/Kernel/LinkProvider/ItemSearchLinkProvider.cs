namespace Sitecore.ItemBucket.Kernel.LinkProvider
{
    using Sitecore.ItemBucket.Kernel.ItemExtensions.Axes;
    using Sitecore.ItemBucket.Kernel.Managers;
    using Sitecore.Links;

    public class ItemSearchLinkProvider : LinkProvider
    {
        public override string GetItemUrl(Data.Items.Item item, UrlOptions options)
        {
            if (item != null)
            {
                return BucketManager.IsItemContainedWithinBucket(item, Context.Database)
                           ? item.ShortUrl()
                           : base.GetItemUrl(item, options);
            }

            return string.Empty;
        }
    }
}
