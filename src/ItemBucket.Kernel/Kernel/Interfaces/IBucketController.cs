namespace Sitecore.ItemBucket.Kernel.Kernel.Interfaces
{
    using System.Collections.Generic;

    /// <summary>
    /// If you would like to implement a new User Interface to talk to the Item Bucket Kernel, you must implement an IBucketController
    /// </summary>
    /// <returns>IEnumerable of IItem which is a list of objects that implement IItem e.g. Item, SitecoreItem</returns>
    public interface IBucketController
    {
        /// <summary>
        /// Controller Interface
        /// </summary>
        /// <param name="query">
        /// The query.
        /// </param>
        /// <returns>
        /// IEnumerable Collection of IItem
        /// </returns>
        IEnumerable<IItem> SearchResults(IBucketSearchQuery query);
    }
}
