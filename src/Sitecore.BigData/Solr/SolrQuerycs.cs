using System.Collections.Generic;
using Microsoft.Practices.ServiceLocation;
using Sitecore.Data.Items;
using SolrNet;

namespace Sitecore.ItemBuckets.BigData.Solr
{
    class SolrQuerycs
    {
        ISolrOperations<SolrSitecoreItem> solr = ServiceLocator.Current.GetInstance<ISolrOperations<SolrSitecoreItem>>();
        public void AddQuery(Item item)
        {
            Startup.Init<SolrSitecoreItem>("http://localhost:8080/solr");

            solr.Add(new SolrSitecoreItem()
                         {
                             Group = "",
                             Name = item.Name,
                             Url = item.Uri.ToString()
                         });

            solr.Commit();
        }

        public IEnumerable<SolrSitecoreItem> Query(string rawQuery)
        {
            return solr.Query(new SolrQuery(rawQuery));
        }
    }
}
