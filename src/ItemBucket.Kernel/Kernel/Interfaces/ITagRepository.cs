namespace Sitecore.ItemBucket.Kernel.Common.Providers
{
    using System.Collections.Generic;

    using Sitecore.ItemBucket.Kernel.Kernel.Interfaces;

    /// <summary>
    /// To have the Item Buckets be able to facet or filter by tags, you can implement this ITagRepository Interface to retrieve tags from anywhere.
    /// Meaning that tags don't have to be stored in Sitecore as Items.
    /// </summary>
    /// <returns>IItem</returns>
    public interface ITagRepository : IRepository<Tag>
    {
        /// <summary>
        /// </summary>
        /// <param name="contains">
        /// The contains.
        /// </param>
        /// <returns>
        /// </returns>
        IEnumerable<Tag> GetTags(string contains);

        /// <summary>
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// </returns>
        Tag GetTagByValue(string value);
    }
}
