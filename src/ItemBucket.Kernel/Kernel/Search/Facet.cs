using System;
using System.Collections;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Sitecore.ItemBucket.Kernel.Kernel.Search;
using Sitecore.Search;

namespace Sitecore.ItemBucket.Kernel.Search
{
    using System.Collections.Generic;

    using Sitecore.ItemBucket.Kernel.Util;

    public class Facet
    {
        public string FieldName { get; private set; }
        public string IndexName { get; set; }

        public Facet(string fieldName, string indexName = Util.Constants.Index.Name)
        {
            FieldName = fieldName;
            IndexName = indexName;
        }

        public IEnumerable<string> GetPossibleValues()
        {
            var terms = new List<string>();
            using (var context = new SortableIndexSearchContext(SearchManager.GetIndex(IndexName)))
            {
                var termsByField = context.Searcher.GetIndexReader().Terms(new Term(FieldName.ToLower(), string.Empty));
                while (termsByField.Next())
                {
                    if (termsByField.Term().Field().Equals(FieldName, StringComparison.InvariantCultureIgnoreCase))
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
            return terms.ToArray();
        }

        public Dictionary<string, int> GetValues(Query query = null, string locationFilter = null, BitArray baseQuery = null)
        {
            IEnumerable<string> possibleValues = this.GetPossibleValues();
            using (var searcher = new Util.IndexSearcher(IndexName))
            {
                var results = searcher.RunFacet(query, false, true, 0, 0, FieldName.ToLower(), possibleValues);
                return results;
            }
        }
    }
}
