namespace Sitecore.ItemBucket.Kernel.Search.Facets
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using Lucene.Net.Search;

    using Sitecore.ItemBucket.Kernel.Kernel.Util;
    using Sitecore.ItemBucket.Kernel.Util;

    using IndexSearcher = Sitecore.ItemBucket.Kernel.Util.IndexSearcher;

    internal class LocationFacet : IFacet
    {
        public List<FacetReturn> Filter(Query query, List<SearchStringModel> searchQuery, string locationFilter, BitArray baseQuery)
        {
            var buckets = new List<SitecoreItem>();
            using (var searcher = new IndexSearcher(Constants.Index.Name))
            {
                if (locationFilter.IsNotEmpty())
                {
                    buckets.AddRange(
                        searcher.GetItemsViaFieldQuery("isbucket", "1", 200).Value.Where(item => item.GetItem().IsNotNull()).Where(
                            itm => Context.ContentDatabase.GetItem(locationFilter).Axes.IsAncestorOf(itm.GetItem())));
                }
            }

            var bucketsSelectToList = buckets.OrderBy(i => i.GetItem().Name).Select(item => item.GetItem().ID.ToString()).ToList();
          
            var returnFacets = this.GetSearch(query, bucketsSelectToList, searchQuery, locationFilter, baseQuery).Select(
                          facet =>
                          new FacetReturn
                          {
                              KeyName = Context.ContentDatabase.GetItem(facet.Key).Name,
                              Value = facet.Value.ToString(),
                              Type = "location",
                              ID = facet.Key
                          });

            return returnFacets.ToList();
        }

        public Dictionary<string, int> GetSearch(Query query, List<string> filters, List<SearchStringModel> searchQuery, string locationFilter, BitArray baseQuery)
        {
            using (var searcher = new IndexSearcher(Constants.Index.Name))
            {
                var results = searcher.RunFacet(query, false, true, 0, 0, "_path", filters, baseQuery, locationFilter);
                return results;
            }
        }
    }
}
