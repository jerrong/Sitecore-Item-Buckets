namespace Sitecore.ItemBucket.Kernel.Search.SearchDropdowns
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Lucene.Net.Index;
    using Lucene.Net.Search;

    using Sitecore.ItemBucket.Kernel.Kernel.Util;
    using Sitecore.ItemBucket.Kernel.Util;

    using IndexSearcher = Sitecore.ItemBucket.Kernel.Util.IndexSearcher;

    internal class RecentlyModified : ISearchDropDown
    {
        public List<string> Process()
        {
            using (var searcher = new IndexSearcher(Constants.Index.Name))
            {
                var query = new RangeQuery(new Term(SearchFieldIDs.UpdatedDate, DateTime.Now.AddDays(-1).ToString("yyyyMMdd")), new Term(SearchFieldIDs.UpdatedDate, DateTime.Now.AddDays(1).ToString("yyyyMMdd")), true);
                var ret = searcher.RunQuery(query, 10, 1, SearchFieldIDs.UpdatedDate, "desc").Value.Where(item => item.GetItem().IsNotNull()).Select(i => i.GetItem().Name + "|" + i.GetItem().ID.ToString()).ToList();
                return ret.Any() ? ret : new List<string> { "There have been no items recently created within the last day" };
            }
        }
    }
}
