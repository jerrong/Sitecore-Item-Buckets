

using System.Collections;
using SolrNet.Attributes;

namespace Sitecore.ItemBucket.Kernel.Kernel.Search.SOLR.SOLRItems
{
    public class SolrTemplateItem : ISOLRItem
    {
        [SolrUniqueKey("_group")]
        public string Group { get; set; }

        [SolrField("_name")]
        public ArrayList Name { get; set; }

        [SolrField("_url")]
        public string Url { get; set; }
    }
    public class SolrBucketItem : ISOLRItem
    {
        [SolrUniqueKey("_group")]
        public string Group { get; set; }

        [SolrField("_name")]
        public ArrayList Name { get; set; }

        [SolrField("_url")]
        public string Url { get; set; }
    }
    public class SolrSitecoreItem : ISOLRItem
    {
        [SolrUniqueKey("_group")]
        public string Group { get; set; }

        [SolrField("_name")]
        public ArrayList Name { get; set; }

        [SolrField("_url")]
        public string Url { get; set; }
    }
    public class SolrMediaItem : ISOLRItem
    {
        [SolrUniqueKey("_group")]
        public string Group { get; set; }

        [SolrField("_name")]
        public ArrayList Name { get; set; }

        [SolrField("_url")]
        public string Url { get; set; }
    }
    public class SolrSystemItem : ISOLRItem
    {
        [SolrUniqueKey("_group")]
        public string Group { get; set; }

        [SolrField("_name")]
        public ArrayList Name { get; set; }

        [SolrField("_url")]
        public string Url { get; set; }
    }
    public class SolrLayoutItem : ISOLRItem
    {
        [SolrUniqueKey("_group")]
        public string Group { get; set; }

        [SolrField("_name")]
        public ArrayList Name { get; set; }

        [SolrField("_url")]
        public string Url { get; set; }
    }
}
