using Sitecore.ItemBucket.Kernel.Util;

namespace Sitecore.ItemBucket.Kernel.Search.Facets
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    using Lucene.Net.Index;
    using Lucene.Net.Search;

    using Sitecore.ItemBucket.Kernel.Kernel.Search;
    using Sitecore.ItemBucket.Kernel.Kernel.Util;
    using Sitecore.Search;

    using IndexSearcher = Sitecore.ItemBucket.Kernel.Util.IndexSearcher;

    internal class AuthorFacet : IFacet
    {
        public List<FacetReturn> Filter(Query query, List<SearchStringModel> searchQuery, string locationFilter, BitArray baseQuery)
        {
            var stopWatch = new Stopwatch();
            if (Config.EnableBucketDebug || Util.Constants.EnableTemporaryBucketDebug)
            {
                Diagnostics.Log.Info("Start Author Facets took : " + stopWatch.ElapsedMilliseconds + "ms", this);
            }
            stopWatch.Start();
            var returnFacets = this.GetSearch(query, this.GetAuthorsFromIndex().ToList(), searchQuery, locationFilter, baseQuery).Select(
                       facet =>
                       new FacetReturn
                       {
                           KeyName = facet.Key,
                           Value = facet.Value.ToString(),
                           Type = "author",
                           ID = facet.Key
                       });
            if (Config.EnableBucketDebug || Util.Constants.EnableTemporaryBucketDebug)
            {
                stopWatch.Stop();
                Diagnostics.Log.Info("End Author Facets took : " + stopWatch.ElapsedMilliseconds + "ms", this);
            }
            return returnFacets.ToList();
        }

        private IEnumerable<string> GetAuthorsFromIndex()
        {
            var terms = new List<string>();
            using (var context = new SortableIndexSearchContext(SearchManager.GetIndex(Util.Constants.Index.Name)))
            {
                var termsByField = context.Searcher.GetIndexReader().Terms(new Term("__created by", string.Empty));
                while (termsByField.Next())
                {
                    if (termsByField.Term().Field() == "__created by")
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

        public Dictionary<string, int> GetSearch(Query query, List<string> filter, List<SearchStringModel> searchQuery, string locationFilter, BitArray baseQuery)
        {
            using (var searcher = new IndexSearcher(Util.Constants.Index.Name))
            {
                var results = searcher.RunFacet(query, false, true, 0, 0, "__created by", filter, baseQuery, locationFilter);
                return results;
            }
        }
    }
}
