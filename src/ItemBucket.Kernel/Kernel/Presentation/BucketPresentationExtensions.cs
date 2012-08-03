using Sitecore.ItemBucket.Kernel.Managers;

namespace Sitecore.ItemBucket.Kernel.Presentation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Sitecore.Data.Items;
    using Sitecore.ItemBucket.Kernel.ItemExtensions.Axes;
    using Sitecore.ItemBucket.Kernel.Kernel.Util;
    using Sitecore.ItemBucket.Kernel.Util;

    public class BucketPresentationExtensions
    {
        /// <summary>
        /// Given a datasource from a Sitecore Control, this will parse the string to turn it into a bucket query
        /// </summary>
        private static List<SearchStringModel> ParseDataSourceQuery(string dataSource)
        {
            var searches = dataSource.Replace("bucket:", string.Empty).Split(';');
            return (from searchStringModel in searches 
                    where !string.IsNullOrEmpty(searchStringModel)
                    select searchStringModel.Split(':')
                        into splitSearch
                        select new SearchStringModel
                        {
                            Type = splitSearch[0],
                            Value = splitSearch[1]
                        }).ToList();
        }

        /// <summary>
        /// Given a datasource from a Sitecore Control, this will parse the string to turn it into a bucket query and to run the query as well
        /// </summary>
        public static List<SitecoreItem> ParseDataSourceQueryForItems(string dataSource, Item item, int pageNumber, int pageSize, out int hits)
        {
            hits = 0;
            int hitCount = hits;
            return item.FullSearch(ParseDataSourceQuery(dataSource), out hitCount, pageNumber: pageNumber, pageSize: pageSize).ToList();
        }

        /// <summary>
        /// Given a datasource from a Sitecore Control, this will parse the string to turn it into a bucket query and to run the query as well using the Context Item
        /// </summary>
        public static List<SitecoreItem> ParseDataSourceQueryForItems(string dataSource, int pageNumber, int pageSize, out int hits)
        {
            hits = 0;
            int hitCount = hits;
            return BucketManager.FullSearch(ParseDataSourceQuery(dataSource), out hitCount, pageNumber: pageNumber, pageSize: pageSize).ToList();
        }
    }
}
