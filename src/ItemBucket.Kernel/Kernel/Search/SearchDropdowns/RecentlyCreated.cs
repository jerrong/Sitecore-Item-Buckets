namespace Sitecore.ItemBucket.Kernel.Kernel.Search.SearchDropdowns
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Lucene.Net.Index;
    using Lucene.Net.Search;

    using Sitecore.Collections;
    using Sitecore.Data.Managers;
    using Sitecore.ItemBucket.Kernel.Kernel.Util;
    using Sitecore.ItemBucket.Kernel.Search;
    using Sitecore.ItemBucket.Kernel.Util;

    using IndexSearcher = Sitecore.ItemBucket.Kernel.Util.IndexSearcher;

    internal class RecentlyCreated : ISearchDropDown
    {
        public List<string> Process()
        {
            using (var searcher = new IndexSearcher(Constants.Index.Name))
            {
                var query = new RangeQuery(new Term(SearchFieldIDs.CreatedDate, DateTime.Now.AddDays(-1).ToString("yyyyMMdd")), new Term(SearchFieldIDs.CreatedDate, DateTime.Now.AddDays(1).ToString("yyyyMMdd")), true);
                var ret = searcher.RunQuery(query, 10, 1).Value.Where(item => item.GetItem().IsNotNull()).Select(i => i.GetItem().Name + "|" + i.GetItem().ID.ToString()).ToList();
                ret.Reverse();
                return ret.Any() ? ret : new List<string> { "There have been no items recently modified within the last day" };
            }
        }
    }
}
