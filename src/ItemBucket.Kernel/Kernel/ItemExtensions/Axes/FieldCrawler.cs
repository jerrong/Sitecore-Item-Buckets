namespace Sitecore.ItemBucket.Kernel.ItemExtensions.Axes
{
    using Sitecore.Data.Fields;
    using Sitecore.ItemBucket.Kernel.Kernel.Util;
    using Sitecore.Search.Crawlers.FieldCrawlers;

    /// <summary>
    /// Custome Field Crawler
    /// </summary>
    internal class FieldCrawler : FieldCrawlerFactory
    {
        /// <summary>
        /// Get Field Crawler
        /// </summary>
        /// <param name="field">
        /// The field.
        /// </param>
        /// <returns>
        /// Field Crawler Base
        /// </returns>
        public static new FieldCrawlerBase GetFieldCrawler(Field field)
        {
            string fieldType;

            if ((fieldType = field.Type).IsNotNull() && (fieldType == "Droplink"))
            {
                return new LookupFieldCrawler(field);
            }

            return FieldCrawlerFactory.GetFieldCrawler(field);
        }
    }
}
