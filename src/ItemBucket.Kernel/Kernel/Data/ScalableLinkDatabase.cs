using System;
using System.Collections;
using System.Linq;
using Sitecore.Collections;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.SqlServer;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.ItemBucket.Kernel.Kernel.ItemExtensions.Axes;
using Sitecore.ItemBucket.Kernel.Kernel.Util;
using Sitecore.ItemBucket.Kernel.Managers;
using Sitecore.Links;
using Sitecore.ItemBucket.Kernel.ItemExtensions.Axes;
using Sitecore.SecurityModel;

namespace Sitecore.ItemBucket.Kernel.Kernel.Data
{
    /// <summary>
    /// A helper class that allows a developer to find connections between items. This uses the Bucket Indexes to do the looks rather than the Sitecore API
    /// </summary>
    public class ScalableLinkDatabase : SqlServerLinkDatabase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString">
        /// The name of the connection string in your ConnectionStrings.config file
        /// </param>
        public ScalableLinkDatabase(string connectionString) : base(connectionString) { }

        private void AddItemIDs(Item item, Hashtable itemIDs)
        {
            itemIDs[item.ID] = string.Empty;
            foreach (Item item2 in item.GetChildren(ChildListOptions.None))
            {
                this.AddItemIDs(item2, itemIDs);
            }
        }

        public virtual ItemLink[] GetItemVersionReferrers(Item version)
        {
            Assert.ArgumentNotNull(version, "version");
            return this.GetReferrers(version);
        }

        [Obsolete("Deprecated - Use GetReferrers instead.")]
        public ItemLink[] GetReferers(Item item)
        {
            Assert.ArgumentNotNull(item, "item");
            return this.GetReferrers(item);
        }

        public override ItemLink[] GetReferrers(Item item)
        {
            return this.GetReferrers(item, true, 1);
        }

        public override ItemLink[] GetReferrers(Item item, bool deep)
        {
            return this.GetReferrers(item, true, 1);
        }

        /// <summary>
        /// Get Refererres by Page
        /// </summary>
        /// <param name="item">
        /// This is the item where the referrers will be looked up on
        /// </param>
        /// <param name="deep">
        /// Specify true if you want to look at all connections
        /// </param>
        /// <param name="page">
        /// By default, links will come back 20 links at a time. This page parameter will bring back the page of 20 within the return ItemLinks
        /// </param>
        public ItemLink[] GetReferrers(Item item, bool deep, int page)
        {
            if (!BulkUpdateContext.IsActive)
            {
                var db = Factory.GetDatabase("master");
                int hitCount;
                var query = db.GetItem(Sitecore.ItemIDs.RootID).Search(new SafeDictionary<string> { { "_links", "" } }, out hitCount, pageNumber: page);
                if (query.IsNotNull())
                {
                    return query.Select(innerItem => new ItemLink(item, item.ID, innerItem.GetItem(), innerItem.GetItem().Paths.FullPath)).ToArray();
                }
            }

            return new ItemLink[] { };
        }

        public bool HasExternalReferers(Item item, bool deep)
        {
            Assert.ArgumentNotNull(item, "item");
            return this.HasExternalReferrers(item, deep);
        }

        private bool HasExternalReferrers(ItemLink[] referrers, Hashtable tree)
        {
            return referrers.Any(link => !tree.ContainsKey(link.SourceItemID));
        }

        public virtual bool HasExternalReferrers(Item item, bool deep)
        {
            Assert.ArgumentNotNull(item, "item");
            Database database = item.Database;
            Hashtable itemIDs = new Hashtable();
            this.AddItemIDs(item, itemIDs);
            if (!deep)
            {
                return this.HasExternalReferrers(this.GetReferrers(item), itemIDs);
            }
            foreach (ID id in itemIDs.Keys)
            {
                Item item2 = database.Items[id];
                if (item2 != null)
                {
                    ItemLink[] referrers = this.GetReferrers(item2);
                    if (this.HasExternalReferrers(referrers, itemIDs))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        protected virtual bool ItemExists(ID itemId, string itemPath, Language itemLanguage, Sitecore.Data.Version itemVersion, Database database)
        {
            Assert.ArgumentNotNull(itemId, "itemId");
            Assert.ArgumentNotNull(database, "database");
            Assert.ArgumentNotNull(itemLanguage, "itemLanguage");
            Assert.ArgumentNotNull(itemVersion, "itemVersion");
            Item itemFromPartialPath = null;
            if (!itemId.IsNull)
            {
                itemFromPartialPath = database.GetItem(itemId);
                if (itemFromPartialPath == null)
                {
                    return false;
                }
            }
            else if (!string.IsNullOrEmpty(itemPath))
            {
                itemFromPartialPath = ItemUtil.GetItemFromPartialPath(itemPath, database);
            }
            if (itemFromPartialPath != null)
            {
                if (itemLanguage == Language.Invariant)
                {
                    return true;
                }

                itemFromPartialPath = itemFromPartialPath.Database.GetItem(itemFromPartialPath.ID, itemLanguage);
                if (itemFromPartialPath == null)
                {
                    return false;
                }

                if (itemVersion == Sitecore.Data.Version.Latest)
                {
                    return (itemFromPartialPath.Versions.Count > 0);
                }

                foreach (Sitecore.Data.Version version in itemFromPartialPath.Versions.GetVersionNumbers())
                {
                    if (version.Number == itemVersion.Number)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public virtual void Rebuild(Database database)
        {
            Assert.ArgumentNotNull(database, "database");
            using (new SecurityDisabler())
            {
                Item rootItem = database.GetRootItem(Context.Language);
                Assert.IsNotNull(rootItem, "No root item in database: " + database.Name);
                this.RebuildItem(rootItem);
            }
            this.Compact(database);
        }

        public virtual void DeferredRebuild(DateTime startDate, DateTime endDate, Database database)
        {
            Assert.ArgumentNotNull(database, "database");
            using (new SecurityDisabler())
            {
                int hitCount;
                Item rootItem = database.GetRootItem(Context.Language);
                var items = new BucketQuery().Starting(startDate).Ending(endDate).Run(out hitCount);
                var pages = hitCount/20;

                for (int i = 0; i <= pages; i++ ) 
                {
                    var pagedResults = new BucketQuery().Starting(startDate).Ending(endDate).Page(i, out hitCount);
                    foreach (var itm in pagedResults)
                    {
                        this.UpdateReferences(itm.GetItem());
                    }
                   
                }
            }

            this.Compact(database);
        }

        private void RebuildItem(Item item)
        {
            this.UpdateReferences(item);
            foreach (Item item2 in item.GetChildren(ChildListOptions.None))
            {
                this.RebuildItem(item2);
            }
        }

        protected virtual bool TargetExists(ID targetID, string targetPath, Database database)
        {
            Assert.ArgumentNotNull(targetID, "targetID");
            Assert.ArgumentNotNull(targetPath, "targetPath");
            Assert.ArgumentNotNull(database, "database");
            if (!targetID.IsNull)
            {
                return (database.GetItem(targetID) != null);
            }
            return (!string.IsNullOrEmpty(targetPath) && (ItemUtil.GetItemFromPartialPath(targetPath, database) != null));
        }

        protected virtual void UpdateItemVersionLinks(Item item, ItemLink[] links)
        {
            Assert.ArgumentNotNull(item, "item");
            Assert.ArgumentNotNull(links, "links");
        }

        public virtual void UpdateItemVersionReferences(Item item)
        {
            Assert.ArgumentNotNull(item, "item");
            ItemLink[] allLinks = item.Links.GetAllLinks();
            this.UpdateItemVersionLinks(item, allLinks);
        }

        public virtual void UpdateReferences(Item item)
        {
            Assert.ArgumentNotNull(item, "item");
            ItemLink[] allLinks = item.Links.GetAllLinks();
            this.UpdateLinks(item, allLinks);
        }

        public override int GetReferenceCount(Item item)
        {
            return GetReferenceCount(item);
        }

        /// <summary>
        /// Get References by Page
        /// </summary>
        /// <param name="item">
        /// This is the item where the referrers will be looked up on
        /// </param>
        /// <param name="deep">
        /// Specify true if you want to look at all connections
        /// </param>
        /// <param name="page">
        /// By default, links will come back 20 links at a time. This page parameter will bring back the page of 20 within the return ItemLinks
        /// </param>
        public override ItemLink[] GetReferences(Item item)
        {
            if (!BulkUpdateContext.IsActive)
            {
                var db = Factory.GetDatabase("master");
                int hitCount;
                var query = db.GetItem(Sitecore.ItemIDs.RootID).Search(new SafeDictionary<string> { { "_links", "" } }, out hitCount, pageNumber: 1);
                if (query.IsNotNull())
                {
                    return query.Select(innerItem => new ItemLink(item, item.ID, innerItem.GetItem(), innerItem.GetItem().Paths.FullPath)).ToArray();
                }
            }
            return new ItemLink[] { };
        }
    }
}
