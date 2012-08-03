using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Sitecore.Data.Managers;
using Sitecore.ItemBucket.Kernel.Search;
using Sitecore.ItemBucket.Kernel.Util;

namespace Sitecore.ItemBucket.Kernel.Kernel.Search.Facets
{
    class LanguageFacet : IFacet
    {
        public List<FacetReturn> Filter(Lucene.Net.Search.Query query, List<Util.SearchStringModel> searchQuery, string locationFilter, System.Collections.BitArray baseQuery)
        {
            var stopWatch = new Stopwatch();
            Diagnostics.Log.Info("Start Language Facets took : " + stopWatch.ElapsedMilliseconds + "ms", this);
            stopWatch.Start();
            var returnFacets = this.GetSearch(query, LanguageManager.GetLanguages(Sitecore.Context.ContentDatabase).Select(language => language.CultureInfo.TwoLetterISOLanguageName).ToList(), searchQuery, locationFilter, baseQuery).Select(
                       facet =>
                       new FacetReturn
                       {
                           KeyName = facet.Key,
                           Value = facet.Value.ToString(),
                           Type = "language",
                           ID = facet.Key
                       });

            stopWatch.Stop();
            Diagnostics.Log.Info("End Language Facets took : " + stopWatch.ElapsedMilliseconds + "ms", this);
            return returnFacets.ToList();
        }

        public Dictionary<string, int> GetSearch(Lucene.Net.Search.Query query, List<string> filter, List<Util.SearchStringModel> searchQuery, string locationFilter, System.Collections.BitArray baseQuery)
        {
            using (var searcher = new IndexSearcher(ItemBucket.Kernel.Util.Constants.Index.Name))
            {
                var results = searcher.RunFacet(query, false, false, 0, 0, "_language", filter, baseQuery, locationFilter);
                return results;
            }
        }
    }
}
