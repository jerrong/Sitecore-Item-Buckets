namespace Sitecore.ItemBucket.Kernel.Search.Facets
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using Lucene.Net.Search;

    using Sitecore.Collections;
    using Sitecore.ItemBucket.Kernel.ItemExtensions.Axes;
    using Sitecore.ItemBucket.Kernel.Kernel.Util;
    using Sitecore.ItemBucket.Kernel.Util;

    using IndexSearcher = Sitecore.ItemBucket.Kernel.Util.IndexSearcher;

    internal class TemplateFacet : IFacet
    {
        public List<FacetReturn> Filter(Query query, List<SearchStringModel> searchQuery, string locationFilter, BitArray baseQuery)
        {
            var refinement = new SafeDictionary<string> { { "bucketable", "1" } };
            int hitsCount;
            var templateSearchSelectToList = Context.ContentDatabase.GetItem(ItemIDs.TemplateRoot).Search(refinement, out hitsCount, location: ItemIDs.TemplateRoot.ToString()).OrderBy(i => i.GetItem().Name).Select(item => item.GetItem().ID.ToString()).ToList();
            var returnFacets = this.GetSearch(query, templateSearchSelectToList, searchQuery, locationFilter, baseQuery).Select(
                              facet =>
                              new FacetReturn
                                  {
                                      KeyName = Context.ContentDatabase.GetItem(facet.Key).Name,
                                      Value = facet.Value.ToString(),
                                      Type = "template",
                                      ID = facet.Key
                                  });
        
            return returnFacets.ToList();
        }

        public Dictionary<string, int> GetSearch(Query query, List<string> filters, List<SearchStringModel> searchQuery, string locationFilter, BitArray baseQuery)
        {
            using (var searcher = new IndexSearcher(Constants.Index.Name))
            {
                var results = searcher.RunFacet(query, false, true, 0, 0, "_template", filters, baseQuery,locationFilter);
                return results;
            }
        }
    }
}
