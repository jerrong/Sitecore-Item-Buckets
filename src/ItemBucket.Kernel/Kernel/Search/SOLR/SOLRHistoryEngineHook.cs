using System;
using Microsoft.Practices.ServiceLocation;
using Sitecore.Data;
using Sitecore.Data.Engines;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;
using Sitecore.Events;
using Sitecore.ItemBucket.Kernel.Kernel.Util;
using Sitecore.ItemBucket.Kernel.Util;
using SolrNet;

namespace Sitecore.ItemBucket.Kernel.Kernel.Search.SOLR
{
    public class SOLRHistoryEngineHook
    {
        // Fields
        private Database m_database;
        private ISolrOperations<SOLRItem> solr;

        // Events
        public event EventHandler<HistoryAddedEventArgs> AddedEntry;

        // Methods
        public SOLRHistoryEngineHook(Database database)
        {
            Assert.ArgumentNotNull(database, "database");
            this.m_database = database;
            if (ServiceLocator.Current.IsNotNull())
            {
                Startup.Init<SOLRItem>(Config.SOLRServiceLocation);
                solr = ServiceLocator.Current.GetInstance<ISolrOperations<SOLRItem>>();
                IndexingManager.Provider.OnRemoveItem += new EventHandler(Provider_OnRemoveItem);
                IndexingManager.Provider.OnUpdateItem += new EventHandler(Provider_OnUpdateItem);
            }
        }

        void Provider_OnUpdateItem(object sender, EventArgs e)
        {
            Assert.ArgumentNotNull(sender, "sender");
            Assert.ArgumentNotNull(e, "e");
            Database database = SitecoreEventArgs.GetObject(e, 0) as Database;
            if ((database != null) && (database.Name == this.m_database.Name))
            {
                ID iD = SitecoreEventArgs.GetID(e, 1);
                Assert.IsNotNull(iD, "ID is not passed to RemoveItem handler");
                solr.Delete(IdHelper.NormalizeGuid(iD.ToString(), false));
               // solr.Add(new SOLRItem());
                solr.Commit();
                solr.Optimize();
            }
        }

        void Provider_OnRemoveItem(object sender, EventArgs e)
        {
            Assert.ArgumentNotNull(sender, "sender");
            Assert.ArgumentNotNull(e, "e");
            Database database = SitecoreEventArgs.GetObject(e, 0) as Database;
            if ((database != null) && (database.Name == this.m_database.Name))
            {
                ID iD = SitecoreEventArgs.GetID(e, 1);
                Assert.IsNotNull(iD, "ID is not passed to RemoveItem handler");
                solr.Delete(IdHelper.NormalizeGuid(iD.ToString(), false));
                solr.Commit();
            }
        }
    }
}
