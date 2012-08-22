using System;
using Sitecore.Diagnostics;
using Sitecore.Events.Hooks;
using Sitecore.ItemBucket.Kernel.Kernel.Search.SOLR;
using Sitecore.ItemBucket.Kernel.Kernel.Search.SOLR.SOLRItems;
using Sitecore.ItemBucket.Kernel.Util;
using Sitecore.Search;
using SolrNet;

namespace Sitecore.ItemBucket.Kernel.Kernel.Hooks
{
    public class QueryWarmUp : IHook
    {
        public QueryWarmUp()
        {
            
        }

        /// <summary>
        /// Implement this and run Bucket Queries to warm up the Lucene Cache.
        /// </summary>
        public virtual void Initialize()
        {
            if (Config.SOLREnabled == "true")
            {
                try
                {
                    Startup.Init<SOLRItem>(Config.SOLRServiceLocation);
                    
                    foreach (var index in SearchManager.Indexes)
                    {
                        if (index.Name == "itembuckets_templates")
                        {
                            Startup.Init<SolrTemplateItem>(Config.SOLRServiceLocation + "/" + index.Name);
                        }
                        if (index.Name == "itembuckets_buckets")
                        {
                            Startup.Init<SolrBucketItem>(Config.SOLRServiceLocation + "/" + index.Name);
                        }
                        if (index.Name == "itembuckets_sitecore")
                        {
                            Startup.Init<SolrSitecoreItem>(Config.SOLRServiceLocation + "/" + index.Name);
                        }
                        if (index.Name == "itembuckets_layoutsfolder")
                        {
                            Startup.Init<SolrLayoutItem>(Config.SOLRServiceLocation + "/" + index.Name);
                        }
                        if (index.Name == "itembuckets_systemfolder")
                        {
                            Startup.Init<SolrSystemItem>(Config.SOLRServiceLocation + "/" + index.Name);
                        }
                        if (index.Name == "itembuckets_medialibrary")
                        {
                            Startup.Init<SolrMediaItem>(Config.SOLRServiceLocation + "/" + index.Name);
                        }
                    }

                }
                catch (Exception exc) { }
            }

            Log.Audit("Query Warm Up Run", this);
            //Place warmup queries here
        }
    }
}
