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

    internal class FieldFacet : IFacet
    {
        public List<FacetReturn> Filter(Query query, List<SearchStringModel> searchQuery, string locationFilter, BitArray baseQuery)
        {
            var refinement = new SafeDictionary<string> { { "is facet", "1" } };
            
            int hitsCount;
            var bucketableTemplatesSelectToList = Context.ContentDatabase.GetItem(ItemIDs.TemplateRoot).Search(refinement, out hitsCount, location: ItemIDs.TemplateRoot.ToString(), numberOfItemsToReturn: 200, pageNumber: 1).Select(item => item.GetItem().Name + "|" + item.GetItem().ID.ToString()).ToList();
            
            bucketableTemplatesSelectToList.Sort();
            
            var returnFacets = this.GetSearch(query, bucketableTemplatesSelectToList, searchQuery, locationFilter, baseQuery).Select(
                          facet =>
                          new FacetReturn
                          {
                              KeyName = facet.Key.Split('|')[0],
                              Value = facet.Value.ToString(),
                              Type = "field",
                              ID = facet.Key.Split('|')[1] + "|" + Context.ContentDatabase.GetItem(facet.Key.Split('|')[1]).Template.ID
                          });
          
            return returnFacets.ToList();
        }

        public Dictionary<string, int> GetSearch(Query query, List<string> filters, List<SearchStringModel> searchQuery, string locationFilter, BitArray baseQuery)
        {
            using (var searcher = new IndexSearcher(Constants.Index.Name))
            {
                var results = searcher.RunFacet(query, false, false, 0, 0, SearchHelper.GetText(searchQuery), filters, baseQuery, locationFilter);
                return results;
            }
        }
    }
}
