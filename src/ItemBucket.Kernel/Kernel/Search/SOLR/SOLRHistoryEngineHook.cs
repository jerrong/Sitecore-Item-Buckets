using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Xml;
using Sitecore.Collections;
using Sitecore.Data;
using Sitecore.Data.Engines;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace Sitecore.ItemBucket.Kernel.Kernel.Search.SOLR
{
    public class SOLRHistoryEngineHook
    {
        // Fields
        private Database m_database;
        private bool m_saveDotNetCallStack;
        private HistoryStorage m_storage;

        // Events
        public event EventHandler<HistoryAddedEventArgs> AddedEntry;

        // Methods
        public SOLRHistoryEngineHook(Database database)
        {
            Assert.ArgumentNotNull(database, "database");
            this.m_database = database;
        }

        private void AddEntry(HistoryCategory category, HistoryAction action, Item item, ID oldParentId,
                              string additionalInfo)
        {
            HistoryStorage storage = this.Storage;
            if (storage != null)
            {
                HistoryEntry entry = new HistoryEntry(category, action, item, oldParentId, additionalInfo);
                if (this.SaveDotNetCallStack)
                {
                    entry.TaskStack = new StackTrace().ToString();
                }
                storage.AddEntry(entry);
                MainUtil.RaiseEvent<HistoryAddedEventArgs>(this.AddedEntry, this,
                                                           new HistoryAddedEventArgs(entry, this.Database));
                SendSolrAdd(category, action, item, oldParentId, additionalInfo);
            }
        }

        public static bool SendSolrAdd(HistoryCategory category, HistoryAction action, Item item, ID oldParentId,
                              string additionalInfo)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("/solr/insert");
            httpWebRequest.ContentType = "text/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(item);

                streamWriter.Write(json);
            }
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var responseText = streamReader.ReadToEnd();
                //Now you have your response.
                //or false depending on information in the response
                return true;
            }
        }

        public void Cleanup()
        {
            HistoryStorage storage = this.Storage;
            if (storage != null)
            {
                storage.Cleanup();
            }
        }

        public HistoryEntryCollection GetHistory(DateTime from, DateTime to)
        {
            HistoryStorage storage = this.Storage;
            if (storage != null)
            {
                return storage.GetHistory(from, to);
            }
            return new HistoryEntryCollection();
        }

        public void RegisterItemCopied(Item copiedItem, Item sourceItem)
        {
            string additionalInfo =
                string.Concat(new object[] {"copy: ", copiedItem.Paths.Path, " (", copiedItem.ID, ")"});
            this.AddEntry(HistoryCategory.Item, HistoryAction.Copied, sourceItem, null, additionalInfo);
        }

        public void RegisterItemCreated(Item item)
        {
            this.AddEntry(HistoryCategory.Item, HistoryAction.Created, item, null, string.Empty);
        }

        public void RegisterItemDeleted(Item item, ID oldParentId)
        {
            this.AddEntry(HistoryCategory.Item, HistoryAction.Deleted, item, oldParentId, string.Empty);
        }

        public virtual void RegisterItemMoved(Item item, ID oldParentId)
        {
            string additionalInfo =
                string.Concat(new object[] {"new parent: ", item.Paths.ParentPath, " (", item.ParentID, ")"});
            this.AddEntry(HistoryCategory.Item, HistoryAction.Moved, item, oldParentId, additionalInfo);
        }

        public void RegisterItemSaved(Item item, ItemChanges changes)
        {
            this.AddEntry(HistoryCategory.Item, HistoryAction.Saved, item, null, string.Empty);
        }

        public void RegisterVersionAdded(Item item)
        {
            this.AddEntry(HistoryCategory.Item, HistoryAction.AddedVersion, item, null, string.Empty);
        }

        public virtual void RegisterVersionRemoved(Item item)
        {
            this.AddEntry(HistoryCategory.Item, HistoryAction.RemovedVersion, item, null, string.Empty);

        }

        // Properties
        public Database Database
        {
            get { return this.m_database; }
        }

        public bool SaveDotNetCallStack
        {
            get { return this.m_saveDotNetCallStack; }
            set { this.m_saveDotNetCallStack = value; }
        }

        public HistoryStorage Storage
        {
            get { return this.m_storage; }
            set { this.m_storage = value; }
        }
    }
}
