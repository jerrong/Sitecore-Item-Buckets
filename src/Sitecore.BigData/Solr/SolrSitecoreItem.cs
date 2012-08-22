using SolrNet.Attributes;

namespace Sitecore.ItemBuckets.BigData.Solr
{
    class SolrSitecoreItem
    {
        [SolrUniqueKey("_group")]
        public string Group { get; set; }

        [SolrField("_name")]
        public string Name { get; set; }

        [SolrField("_url")]
        public string Url { get; set; }
    }
}
