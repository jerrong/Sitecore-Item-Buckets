namespace Sitecore.ItemBucket.Kernel.Search
{
    using Sitecore.Data.Fields;
    using Sitecore.Search.Crawlers.FieldCrawlers;

    public class DefaultFieldCrawler : FieldCrawlerBase
    {
        public DefaultFieldCrawler(Field field) : base(field)
        {
        }
    }
}
