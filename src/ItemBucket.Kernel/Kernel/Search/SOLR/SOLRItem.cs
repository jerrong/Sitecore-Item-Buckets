using System.Collections;
using SolrNet.Attributes;

namespace Sitecore.ItemBucket.Kernel.Kernel.Search.SOLR
{
    public abstract class SOLRItem : ISOLRItem
    {
        [SolrUniqueKey("_group")]
        public string Group { get; set; }

        [SolrField("_name")]
        public ArrayList Name { get; set; }

        [SolrField("_url")]
        public string Url { get; set; }
    }

    public interface  ISOLRItem
    {
        [SolrUniqueKey("_group")]
        string Group { get; set; }

        [SolrField("_name")]
        ArrayList Name { get; set; }

        [SolrField("_url")]
        string Url { get; set; }
    }


}
