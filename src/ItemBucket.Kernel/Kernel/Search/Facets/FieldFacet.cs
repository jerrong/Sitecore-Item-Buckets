using Lucene.Net.Index;
using Sitecore.ItemBucket.Kernel.Kernel.Search;
using Sitecore.ItemBucket.Kernel.Util;
using Sitecore.Search;

namespace Sitecore.ItemBucket.Kernel.Search.Facets
{
    using System;
    using System.Linq;
    using System.Collections;
    using System.Collections.Generic;
    using Lucene.Net.Search;

    using Sitecore.Collections;
    using Sitecore.ItemBucket.Kernel.Kernel.Util;
    using Sitecore.ItemBucket.Kernel.ItemExtensions.Axes;

    internal class FieldFacet : IFacet
    {
        public List<FacetReturn> Filter(Query query, List<SearchStringModel> searchQuery, string locationFilter, BitArray baseQuery)
        {
            var refinement = new SafeDictionary<string> { { "is facet", "1" } };
            
            int hitsCount;
            var facetFields =
                Context.ContentDatabase.GetItem(ItemIDs.TemplateRoot)
                    .Search(refinement, out hitsCount, location: ItemIDs.TemplateRoot.ToString(),
                            numberOfItemsToReturn: 2000, pageNumber: 1)
                    .ToList()
                    .Select((item, sitecoreItem) =>
                            new
                                {
                                    FieldId = item.ItemId,
                                    Facet = new Facet(item.Name)
                                })

                    .ToList();
            facetFields.Sort((f1, f2) => System.String.Compare(f1.Facet.FieldName, f2.Facet.FieldName, System.StringComparison.Ordinal));
            
            var returnFacets = (from facetField in facetFields
                                from facet in facetField.Facet.GetValues(query, locationFilter, baseQuery).Select(facet => new FacetReturn
                                    {
                                        KeyName = facet.Key, 
                                        Value = facet.Value.ToString(),
                                        Type = facetField.Facet.FieldName.ToLower(), 
                                        ID = facetField.FieldId + "|" + facet.Key
                                    })
                                select facet).ToList();

            return returnFacets.ToList();
        }
    }
}
