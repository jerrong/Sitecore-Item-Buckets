using Lucene.Net.Index;
using Sitecore.Data.Items;
using Sitecore.ItemBucket.Kernel.Kernel.Search;
using Sitecore.Search;

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
            var facetFields = Context.ContentDatabase.GetItem(ItemIDs.TemplateRoot).Search(refinement, out hitsCount, location: ItemIDs.TemplateRoot.ToString(), numberOfItemsToReturn: 200, pageNumber: 1)
                //.Select(item => item.GetItem().Name + "|" + item.GetItem().ID.ToString())
                .ToList();
            
            facetFields.Sort((item, sitecoreItem) => System.String.Compare(item.Name, sitecoreItem.Name, System.StringComparison.Ordinal));
            var returnFacets = (from facetField in facetFields
                                let item = facetField.GetItem()
                                let values = GetValuesFromIndex(item.Name)
                                from facet in this.GetSearch(query, values, searchQuery, locationFilter, baseQuery, item.Name).Select(facet => new FacetReturn
                                    {
                                        KeyName = facet.Key, 
                                        Value = facet.Value.ToString(), 
                                        Type = item.Name.ToLower(), 
                                        ID = item.ID + "|" + facet.Key
                                    })
                                select facet).ToList();

            return returnFacets.ToList();
        }

        private List<string> GetValuesFromIndex(string indexName)
        {
            var terms = new List<string>();
            using (var context = new SortableIndexSearchContext(SearchManager.GetIndex(Util.Constants.Index.Name)))
            {

                var termsByField = context.Searcher.GetIndexReader().Terms(new Term(indexName.ToLower(), string.Empty));
                while (termsByField.Next())
                {
                    if (termsByField.Term().Field().Equals(indexName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        terms.Add(termsByField.Term().Text());
                    }
                    else
                    {
                        break;
                    }
                }
            }

            terms.Sort();
            return terms;
        }

        public Dictionary<string, int> GetSearch(Query query, List<string> filters, List<SearchStringModel> searchQuery, string locationFilter, BitArray baseQuery, string fieldName)
        {
            using (var searcher = new IndexSearcher(Constants.Index.Name))
            {
                var results = searcher.RunFacet(query, false, true, 0, 0, (fieldName ?? SearchHelper.GetText(searchQuery)).ToLower(), filters, baseQuery, locationFilter);
                return results;
            }
        }
    }
}
