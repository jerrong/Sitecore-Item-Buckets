using Sitecore.Events.Hooks;
using Sitecore.ItemBucket.Kernel.Util;
using SolrNet;

namespace Sitecore.ItemBucket.Kernel.Kernel.Hooks
{
    public abstract class QueryWarmUp : IHook
    {
        /// <summary>
        /// Implement this and run Bucket Queries to warm up the Lucene Cache.
        /// </summary>
        public virtual void Initialize()
        {
            if (Config.SOLREnabled == "true")
            {
               Startup.Init<SitecoreItem>("http://localhost:8983/solr");
            }
            //Place warmup queries here
        }
    }
}
