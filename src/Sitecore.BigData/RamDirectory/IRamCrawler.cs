// -----------------------------------------------------------------------
// <copyright file="IRamCrawler.cs" company="Microsoft">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Sitecore.ItemBuckets.BigData.RamDirectory
{
    using Sitecore.Search;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public interface IRamCrawler
    {
        // Methods
        void Add(IndexUpdateContext context);
        void Initialize(InMemoryIndex index);
    }
}
