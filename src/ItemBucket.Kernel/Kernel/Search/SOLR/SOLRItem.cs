using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SolrNet.Attributes;

namespace Sitecore.ItemBucket.Kernel.Kernel.Search.SOLR
{
    public class SOLRItem
    {
        [SolrUniqueKey("_group")]
        public string Group { get; set; }

        [SolrField("_name")]
        public ArrayList Name { get; set; }

        [SolrField("_url")]
        public string Url { get; set; }
    }
}
