using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.ItemBucket.Kernel.Search;
using Sitecore.ItemBucket.Kernel.Util;

namespace Sitecore.ItemBucket.Kernel.Kernel.Search.Facets
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

    internal class FileSize : IFacet
    {
        public List<FacetReturn> Filter(Query query, List<SearchStringModel> searchQuery, string locationFilter, BitArray baseQuery)
        {
            if (InAvailableLocations(locationFilter))
            {
                var listOfSizes = new List<string>()
                {
                    "[0 TO 60000]",
                    "[60000 TO 800000]",
                    "[800000 TO 9000000]",
                };

                var stopWatch = new Stopwatch();
                if (Config.EnableBucketDebug || Sitecore.ItemBucket.Kernel.Util.Constants.EnableTemporaryBucketDebug)
                {
                    Diagnostics.Log.Info("Start File Size Facet took : " + stopWatch.ElapsedMilliseconds + "ms", this);
                }
                stopWatch.Start();
                var returnFacets = this.GetSearch(query, listOfSizes, searchQuery, locationFilter, baseQuery).Select(
                           facet =>
                           new FacetReturn
                           {
                               KeyName = facet.Key,
                               Value = facet.Value.ToString(),
                               Type = "file size",
                               ID = facet.Key
                           });
                if (Config.EnableBucketDebug || Sitecore.ItemBucket.Kernel.Util.Constants.EnableTemporaryBucketDebug)
                {
                    stopWatch.Stop();
                    Diagnostics.Log.Info("End File Size Facet took : " + stopWatch.ElapsedMilliseconds + "ms", this);
                }

                return returnFacets.ToList();
            }

            return new List<FacetReturn>();
        }

        private static bool InAvailableLocations(string locationFilter)
        {
            if (Sitecore.Context.ContentDatabase.GetItem(locationFilter).IsNotNull())
            {

                return Sitecore.Context.ContentDatabase.GetItem(ItemIDs.MediaLibraryRoot).Axes.IsAncestorOf(
                    Sitecore.Context.ContentDatabase.GetItem(locationFilter))
                       ||
                       Sitecore.Context.ContentDatabase.GetItem(ItemIDs.RootID).Axes.IsAncestorOf(
                           Sitecore.Context.ContentDatabase.GetItem(locationFilter));
            }
            else
            {
                return false;
            }
        }

        public Dictionary<string, int> GetSearch(Query query, List<string> filter, List<SearchStringModel> searchQuery, string locationFilter, BitArray baseQuery)
        {
            using (var searcher = new IndexSearcher(ItemBucket.Kernel.Util.Constants.Index.Name))
            {
                var results = searcher.RunFacet(query, false, false, 0, 0, "size", filter, baseQuery, locationFilter);
                return results;
            }
        }
    }
}
