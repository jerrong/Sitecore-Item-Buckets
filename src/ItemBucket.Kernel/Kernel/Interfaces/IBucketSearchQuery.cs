namespace Sitecore.ItemBucket.Kernel.Kernel.Interfaces
{
    /// <summary>
    /// If you would like to implement a new search filter then you must implement an IBucketSearchQuery
    /// </summary>
    /// <returns>IBucketSearch Interface</returns>
    public interface IBucketSearchQuery
    {
        /// <summary>
        /// Search Type
        /// </summary>
        /// <returns>
        /// Type of Search
        /// </returns>
        string GetSearchType();

        /// <summary>
        /// Search Value
        /// </summary>
        /// <param name="query">
        /// The query.
        /// </param>
        /// <returns>
        /// Value of Search
        /// </returns>
        string GetSearchValue(string query);


    }
}
