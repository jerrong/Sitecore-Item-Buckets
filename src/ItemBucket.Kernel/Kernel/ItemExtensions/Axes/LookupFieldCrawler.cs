namespace Sitecore.ItemBucket.Kernel.ItemExtensions.Axes
{
    using Sitecore.Data.Fields;
    using Sitecore.ItemBucket.Kernel.Kernel.Util;
    using Sitecore.Search.Crawlers.FieldCrawlers;

    /// <summary>
    /// </summary>
    internal class LookupFieldCrawler : FieldCrawlerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LookupFieldCrawler"/> class.
        /// </summary>
        /// <param name="field">
        /// The field.
        /// </param>
        public LookupFieldCrawler(Field field) : base(field)
        {
        }

        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        public override string GetValue()
        {
            var targetItem = new LookupField(_field).TargetItem;
            return targetItem.IsNotNull() ? targetItem.Name.ToLowerInvariant() : string.Empty;
        }
    }
}
