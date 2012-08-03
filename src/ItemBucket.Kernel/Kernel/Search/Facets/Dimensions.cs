using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Sitecore.ItemBucket.Kernel.Kernel.Util;
using Sitecore.ItemBucket.Kernel.Search;
using Sitecore.ItemBucket.Kernel.Util;

namespace Sitecore.ItemBucket.Kernel.Kernel.Search.Facets
{
    internal class Dimensions : IFacet
    {
        public List<FacetReturn> Filter(Lucene.Net.Search.Query query, List<Util.SearchStringModel> searchQuery, string locationFilter, System.Collections.BitArray baseQuery)
        {
            if (InAvailableLocations(locationFilter))
            {
                var listOfSizes = new List<string>()
                {
                    "[0 TO 300x200]",
                    "[300x200 TO 600x400]",
                    "[600x400 TO 1000x800]",
                    "[1000x800 TO 1400x1020]"
                };

                var stopWatch = new Stopwatch();
                if (Config.EnableBucketDebug || Sitecore.ItemBucket.Kernel.Util.Constants.EnableTemporaryBucketDebug)
                {
                    Diagnostics.Log.Info("Start Image Dimenstion Facet took : " + stopWatch.ElapsedMilliseconds + "ms", this);
                }
                stopWatch.Start();
                var returnFacets = this.GetSearch(query, listOfSizes, searchQuery, locationFilter, baseQuery).Select(
                           facet =>
                           new FacetReturn
                           {
                               KeyName = facet.Key,
                               Value = facet.Value.ToString(),
                               Type = "dimensions",
                               ID = facet.Key
                           });
                if (Config.EnableBucketDebug || Sitecore.ItemBucket.Kernel.Util.Constants.EnableTemporaryBucketDebug)
                {
                    stopWatch.Stop();
                    Diagnostics.Log.Info("End Image Dimension Facet took : " + stopWatch.ElapsedMilliseconds + "ms", this);
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


        public Dictionary<string, int> GetSearch(Lucene.Net.Search.Query query, List<string> filter, List<Util.SearchStringModel> searchQuery, string locationFilter, System.Collections.BitArray baseQuery)
        {
            using (var searcher = new IndexSearcher(ItemBucket.Kernel.Util.Constants.Index.Name))
            {
                var results = searcher.RunFacet(query, false, false, 0, 0, "dimensions", filter, baseQuery, locationFilter);
                return results;
            }
        }
    }
}
