using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Lucene.Net.Index;
using Sitecore.ItemBucket.Kernel.Kernel.Util;
using Sitecore.ItemBucket.Kernel.Managers;
using Sitecore.ItemBucket.Kernel.Search;
using Sitecore.ItemBucket.Kernel.Util;
using Sitecore.Search;

namespace Sitecore.ItemBucket.Kernel.Kernel.Search.Facets
{
    internal class Extension : IFacet
    {
        public List<FacetReturn> Filter(Lucene.Net.Search.Query query, List<Util.SearchStringModel> searchQuery, string locationFilter, System.Collections.BitArray baseQuery)
        {
            if (InAvailableLocations(locationFilter))
            {
                var stopWatch = new Stopwatch();
                if (Config.EnableBucketDebug || Sitecore.ItemBucket.Kernel.Util.Constants.EnableTemporaryBucketDebug)
                {
                    Diagnostics.Log.Info("Start Extension Facet took : " + stopWatch.ElapsedMilliseconds + "ms",
                                         this);
                }
                stopWatch.Start();
                var returnFacets = this.GetSearch(query, GetFileExtensionsFromIndex().ToList(), searchQuery, locationFilter, baseQuery).Select(
                    facet =>
                    new FacetReturn
                        {
                            KeyName = facet.Key,
                            Value = facet.Value.ToString(),
                            Type = "extension",
                            ID = facet.Key
                        });
                if (Config.EnableBucketDebug || Sitecore.ItemBucket.Kernel.Util.Constants.EnableTemporaryBucketDebug)
                {
                    stopWatch.Stop();
                    Diagnostics.Log.Info("End Extension Facet took : " + stopWatch.ElapsedMilliseconds + "ms",
                                         this);
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

        private IEnumerable<string> GetFileExtensionsFromIndex()
        {
            var terms = new List<string>();
            //using (var context = new SortableIndexSearchContext(SearchManager.GetIndex(BucketManager.GetContextIndex(Sitecore.Context.ContentDatabase.GetItem(ItemIDs.MediaLibraryRoot)))))
            //{
            //    var termsByField = context.Searcher.GetIndexReader().Terms(new Term("extension", string.Empty));
            //    while (termsByField.Next())
            //    {
            //        if (termsByField.Term().Field() == "extension")
            //        {
            //            terms.Add(termsByField.Term().Text());
            //        }
            //        else
            //        {
            //            break;
            //        }
            //    }
            //}
            terms.Add("jpg");
            terms.Add("png");
            terms.Add("pdf");
            terms.Add("gif");
            terms.Add("tiff");
            terms.Add("doc");
            terms.Add("docx");
            terms.Add("ppt");
            terms.Add("pptx");
            terms.Add("xls");
            terms.Add("mp3");
            terms.Add("mp4");
            terms.Add("bmp");

            terms.Sort(); //TODO: Change this to sort in the lucene query
            return terms;
        }

        public Dictionary<string, int> GetSearch(Lucene.Net.Search.Query query, List<string> filter, List<Util.SearchStringModel> searchQuery, string locationFilter, System.Collections.BitArray baseQuery)
        {
            using (var searcher = new IndexSearcher(ItemBucket.Kernel.Util.Constants.Index.Name))
            {
                var results = searcher.RunFacet(query, false, true, 0, 0, "extension", filter, baseQuery, locationFilter);
                return results;
            }
        }
    }
}
