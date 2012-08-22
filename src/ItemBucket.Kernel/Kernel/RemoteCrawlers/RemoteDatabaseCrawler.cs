using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Sitecore.Collections;
using Sitecore.Common;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Data.Templates;
using Sitecore.Diagnostics;
using Sitecore.Events;
using Sitecore.Globalization;
using Sitecore.ItemBuckets.BigData.RemoteIndex;
using Sitecore.Links;
using Sitecore.Search;
using Sitecore.Search.Crawlers;
using Sitecore.Search.Crawlers.FieldCrawlers;
using Sitecore.SecurityModel;
using Version = System.Version;

namespace Sitecore.ItemBucket.Kernel.Kernel.RemoteCrawlers
{
    public class RemoteDatabaseCrawler : BaseCrawler, IRemoteCrawler
    {
        // Fields
        private Database _database;
        protected bool _hasExcludes;
        protected bool _hasIncludes;
        private RemoteIndex _index;
        private bool _indexAllFields = true;
        private bool _monitorChanges = true;
        private Item _root;
        protected static Regex _shortenGuid = new Regex("[{}-]", RegexOptions.Compiled);
        protected readonly Dictionary<string, bool> _templateFilter = new Dictionary<string, bool>();
        private List<string> _textFieldTypes = new List<string>(new string[] { "Single-Line Text", "Rich Text", "Multi-Line Text", "text", "rich text", "html", "memo", "Word Document" });

        // Methods
        public void Add(IndexUpdateContext context)
        {
            Assert.ArgumentNotNull(context, "context");
            this.AddTree(this._root, context);
        }

        protected virtual void AddAllFields(Document document, Item item, bool versionSpecific)
        {
            Assert.ArgumentNotNull(document, "document");
            Assert.ArgumentNotNull(item, "item");
            foreach (Sitecore.Data.Fields.Field field in item.Fields)
            {
                if (!string.IsNullOrEmpty(field.Key))
                {
                    bool tokenize = this.IsTextField(field);
                    FieldCrawlerBase fieldCrawler = FieldCrawlerFactory.GetFieldCrawler(field);
                    Assert.IsNotNull(fieldCrawler, "fieldCrawler");
                    if (this.IndexAllFields)
                    {
                        document.Add(base.CreateField(field.Key, fieldCrawler.GetValue(), tokenize, 1f));
                    }
                    if (tokenize)
                    {
                        document.Add(base.CreateField(BuiltinFields.Content, fieldCrawler.GetValue(), true, 1f));
                    }
                }
            }
        }

        public void AddItem(Item item)
        {
            Assert.ArgumentNotNull(item, "item");
            if (this.IsMatch(item))
            {
                using (IndexUpdateContext context = this._index.CreateUpdateContext())
                {
                    this.AddItem(item, context);
                    context.Commit();
                }
            }
        }

        protected virtual void AddItem(Item item, IndexUpdateContext context)
        {
            Assert.ArgumentNotNull(item, "item");
            Assert.ArgumentNotNull(context, "context");
            if (this.IsMatch(item))
            {
                foreach (Language language in item.Languages)
                {
                    Item latestVersion = item.Database.GetItem(item.ID, language, Sitecore.Data.Version.Latest);
                    if (latestVersion != null)
                    {
                        foreach (Item item3 in latestVersion.Versions.GetVersions(false))
                        {
                            this.IndexVersion(item3, latestVersion, context);
                        }
                    }
                }
            }
        }

        [Obsolete("Use AddVersionIdentifiers")]
        protected void AddItemIdentifiers(Item item, Document document)
        {
            Assert.ArgumentNotNull(item, "item");
            Assert.ArgumentNotNull(document, "document");
            document.Add(base.CreateValueField(BuiltinFields.Database, item.Database.Name));
            document.Add(base.CreateValueField(BuiltinFields.ID, ShortID.Encode(item.ID)));
            document.Add(base.CreateValueField(BuiltinFields.Language, "neutral"));
            document.Add(base.CreateTextField(BuiltinFields.Template, ShortID.Encode(item.TemplateID)));
            document.Add(base.CreateDataField(BuiltinFields.Url, new ItemUri(item.ID, item.Database).ToString(), 0f));
            document.Add(base.CreateDataField(BuiltinFields.Group, ShortID.Encode(item.ID), 0f));
        }

        protected virtual void AddMatchCriteria(BooleanQuery query)
        {
            Assert.ArgumentNotNull(query, "query");
            query.Add(new TermQuery(new Term(BuiltinFields.Database, this._root.Database.Name)), BooleanClause.Occur.MUST);
            query.Add(new TermQuery(new Term(BuiltinFields.Path, ShortID.Encode(this._root.ID))), BooleanClause.Occur.MUST);
            if (this._hasIncludes || this._hasExcludes)
            {
                foreach (KeyValuePair<string, bool> pair in this._templateFilter)
                {
                    query.Add(new TermQuery(new Term(BuiltinFields.Template, ShortID.Encode(pair.Key))), pair.Value ? BooleanClause.Occur.SHOULD : BooleanClause.Occur.MUST_NOT);
                }
            }
        }

        protected virtual void AddSpecialFields(Document document, Item item)
        {
            Assert.ArgumentNotNull(document, "document");
            Assert.ArgumentNotNull(item, "item");
            string displayName = item.Appearance.DisplayName;
            Assert.IsNotNull(displayName, "Item's display name is null.");
            document.Add(base.CreateTextField(BuiltinFields.Name, item.Name));
            document.Add(base.CreateDataField(BuiltinFields.Name, item.Name));
            document.Add(base.CreateTextField(BuiltinFields.Name, displayName));
            document.Add(base.CreateValueField(BuiltinFields.Icon, item.Appearance.Icon));
            document.Add(base.CreateTextField(BuiltinFields.Creator, item.Statistics.CreatedBy));
            document.Add(base.CreateTextField(BuiltinFields.Editor, item.Statistics.UpdatedBy));
            document.Add(base.CreateTextField(BuiltinFields.AllTemplates, this.GetAllTemplates(item)));
            document.Add(base.CreateTextField(BuiltinFields.TemplateName, item.TemplateName));
            if (this.IsHidden(item))
            {
                document.Add(base.CreateValueField(BuiltinFields.Hidden, "1"));
            }
            document.Add(base.CreateValueField(BuiltinFields.Created, item[FieldIDs.Created]));
            document.Add(base.CreateValueField(BuiltinFields.Updated, item[FieldIDs.Updated]));
            document.Add(base.CreateTextField(BuiltinFields.Path, this.GetItemPath(item)));
            document.Add(base.CreateTextField(BuiltinFields.Links, this.GetItemLinks(item)));
            if (base.Tags.Length > 0)
            {
                document.Add(base.CreateTextField(BuiltinFields.Tags, base.Tags));
                document.Add(base.CreateDataField(BuiltinFields.Tags, base.Tags));
            }
        }

        public void AddTextFieldType(string type)
        {
            Assert.ArgumentNotNullOrEmpty(type, "type");
            this._textFieldTypes.Add(type);
        }

        public void AddTree(Item root)
        {
            Assert.ArgumentNotNull(root, "root");
            if (root.Axes.IsDescendantOf(this._root))
            {
                using (IndexUpdateContext context = this._index.CreateUpdateContext())
                {
                    this.AddTree(root, context);
                    context.Commit();
                }
            }
        }

        protected void AddTree(Item root, IndexUpdateContext context)
        {
            Assert.ArgumentNotNull(root, "root");
            Assert.ArgumentNotNull(context, "context");
            using (new LimitMemoryContext(true))
            {
                this.AddItem(root, context);
                List<ID> list = new List<ID>();
                foreach (Item item in root.GetChildren(ChildListOptions.IgnoreSecurity))
                {
                    list.Add(item.ID);
                }
                foreach (ID id in list)
                {
                    Item item2 = root.Database.GetItem(id);
                    Assert.IsNotNull(item2, "Child item was not found.");
                    this.AddTree(item2, context);
                }
            }
        }

        private class LimitMemoryContext : Switcher<bool, LimitMemoryContext>
        {
            // Methods
            public LimitMemoryContext()
                : base(true)
            {
            }

            public LimitMemoryContext(bool limit)
                : base(limit)
            {
            }
        }



        public void AddVersion(Item version)
        {
            Assert.ArgumentNotNull(version, "version");
            if (this.IsMatch(version))
            {
                using (IndexUpdateContext context = this._index.CreateUpdateContext())
                {
                    this.AddVersion(version, context);
                    context.Commit();
                }
            }
        }

        protected virtual void AddVersion(Item version, IndexUpdateContext context)
        {
            Assert.ArgumentNotNull(version, "version");
            Assert.ArgumentNotNull(context, "context");
            Item latestVersion = version.Database.GetItem(version.ID, version.Language, Sitecore.Data.Version.Latest);
            if (latestVersion != null)
            {
                this.IndexVersion(version, latestVersion, context);
            }
        }

        protected void AddVersionIdentifiers(Item item, Item latestVersion, Document document)
        {
            Assert.ArgumentNotNull(item, "item");
            Assert.ArgumentNotNull(latestVersion, "latestVersion");
            Assert.ArgumentNotNull(document, "document");
            document.Add(base.CreateValueField(BuiltinFields.Database, item.Database.Name));
            document.Add(base.CreateValueField(BuiltinFields.ID, this.GetItemID(item.ID, item.Language.ToString(), item.Version.ToString())));
            document.Add(base.CreateValueField(BuiltinFields.Language, item.Language.ToString()));
            document.Add(base.CreateTextField(BuiltinFields.Template, ShortID.Encode(item.TemplateID)));
            if (item.Version.Number == latestVersion.Version.Number)
            {
                document.Add(base.CreateValueField(BuiltinFields.LatestVersion, "1"));
            }
            document.Add(base.CreateDataField(BuiltinFields.Url, item.Uri.ToString()));
            document.Add(base.CreateDataField(BuiltinFields.Group, ShortID.Encode(item.ID)));
        }

        protected virtual void AdjustBoost(Document document, Item item)
        {
            Assert.ArgumentNotNull(document, "document");
            Assert.ArgumentNotNull(item, "item");
            document.SetBoost(base.Boost);
        }

        public void DeleteItem(Item item)
        {
            Assert.ArgumentNotNull(item, "item");
            if (this.IsMatch(item))
            {
                using (IndexDeleteContext context = this._index.CreateDeleteContext())
                {
                    this.DeleteItem(item.ID, context);
                    context.Commit();
                }
            }
        }

        protected virtual void DeleteItem(ID itemId, IndexDeleteContext context)
        {
            Assert.ArgumentNotNull(itemId, "itemId");
            Assert.ArgumentNotNull(context, "context");
            context.DeleteDocuments(context.Search(new PreparedQuery(this.GetItemQuery(itemId))).Ids);
        }

        public void DeleteTree(Item root)
        {
            Assert.ArgumentNotNull(root, "root");
            if (root.Axes.IsDescendantOf(this._root))
            {
                using (IndexDeleteContext context = this._index.CreateDeleteContext())
                {
                    this.DeleteTree(root.ID, context);
                    context.Commit();
                }
            }
        }

        protected void DeleteTree(ID rootId, IndexDeleteContext context)
        {
            Assert.ArgumentNotNull(rootId, "rootId");
            Assert.ArgumentNotNull(context, "context");
            context.DeleteDocuments(context.Search(new PreparedQuery(this.GetDescendantsQuery(rootId))).Ids);
        }

        public void DeleteVersion(Item version)
        {
            Assert.ArgumentNotNull(version, "version");
            if (this.IsMatch(version))
            {
                using (IndexDeleteContext context = this._index.CreateDeleteContext())
                {
                    this.DeleteVersion(version.ID, version.Language.ToString(), version.Version.ToString(), context);
                    context.Commit();
                }
            }
        }

        protected virtual void DeleteVersion(ID id, string language, string version, IndexDeleteContext context)
        {
            Assert.ArgumentNotNull(id, "id");
            Assert.ArgumentNotNull(language, "language");
            Assert.ArgumentNotNullOrEmpty(version, "version");
            Assert.ArgumentNotNull(context, "context");
            context.DeleteDocuments(context.Search(new PreparedQuery(this.GetVersionQuery(id, language, version))).Ids);
        }

        public void ExcludeTemplate(string value)
        {
            Assert.ArgumentNotNullOrEmpty(value, "value");
            this._hasExcludes = true;
            this._templateFilter[value] = false;
        }

        protected string GetAllTemplates(Item item)
        {
            Assert.ArgumentNotNull(item, "item");
            Assert.IsNotNull(item.Template, "Item's template is null.");
            StringBuilder builder = new StringBuilder();
            builder.Append(ShortID.Encode(item.TemplateID));
            builder.Append(" ");
            foreach (TemplateItem item2 in item.Template.BaseTemplates)
            {
                builder.Append(ShortID.Encode(item2.ID));
                builder.Append(" ");
            }
            return builder.ToString();
        }

        protected virtual Query GetDescendantsQuery(ID itemID)
        {
            Assert.ArgumentNotNull(itemID, "itemID");
            BooleanQuery query = new BooleanQuery();
            query.Add(new TermQuery(new Term(BuiltinFields.Path, ShortID.Encode(itemID))), BooleanClause.Occur.MUST);
            this.AddMatchCriteria(query);
            return query;
        }

        protected Query GetDescendantsQuery(Item root)
        {
            Assert.ArgumentNotNull(root, "root");
            return this.GetDescendantsQuery(root.ID);
        }

        protected string GetItemID(ID id, string language, string version)
        {
            Assert.ArgumentNotNull(id, "id");
            Assert.ArgumentNotNull(language, "language");
            Assert.ArgumentNotNull(version, "version");
            return (ShortID.Encode(id) + language + "%" + version);
        }

        protected string GetItemLinks(Item item)
        {
            Assert.ArgumentNotNull(item, "item");
            StringBuilder builder = new StringBuilder();
            foreach (ItemLink link in item.Links.GetAllLinks(false))
            {
                builder.Append(" ");
                builder.Append(ShortID.Encode(link.TargetItemID));
            }
            return builder.ToString();
        }

        protected string GetItemPath(Item item)
        {
            Assert.ArgumentNotNull(item, "item");
            return _shortenGuid.Replace(item.Paths.LongID.Replace('/', ' '), string.Empty);
        }

        protected virtual Query GetItemQuery(ID id)
        {
            Assert.ArgumentNotNull(id, "id");
            BooleanQuery query = new BooleanQuery();
            query.Add(new PrefixQuery(new Term(BuiltinFields.ID, this.GetWildcardItemID(id))), BooleanClause.Occur.MUST);
            this.AddMatchCriteria(query);
            return query;
        }

        protected Query GetItemQuery(Item item)
        {
            Assert.ArgumentNotNull(item, "item");
            return this.GetItemQuery(item.ID);
        }

        protected Query GetVersionQuery(Item version)
        {
            Assert.ArgumentNotNull(version, "version");
            return this.GetVersionQuery(version.ID, version.Language.ToString(), version.Version.ToString());
        }

        protected virtual Query GetVersionQuery(ID id, string language, string version)
        {
            Assert.ArgumentNotNull(id, "id");
            Assert.ArgumentNotNullOrEmpty(language, "language");
            Assert.ArgumentNotNullOrEmpty(version, "version");
            BooleanQuery query = new BooleanQuery();
            query.Add(new TermQuery(new Term(BuiltinFields.ID, this.GetItemID(id, language, version))), BooleanClause.Occur.MUST);
            this.AddMatchCriteria(query);
            return query;
        }

        protected string GetWildcardItemID(ID id)
        {
            Assert.ArgumentNotNull(id, "id");
            return ShortID.Encode(id);
        }

        public void IncludeTemplate(string value)
        {
            Assert.ArgumentNotNullOrEmpty(value, "value");
            this._hasIncludes = true;
            this._templateFilter[value] = true;
        }

        [Obsolete("Shared fields are moved into item versions")]
        protected void IndexSharedData(Item item, IndexUpdateContext context)
        {
            Assert.ArgumentNotNull(item, "item");
            Assert.ArgumentNotNull(context, "context");
            Document document = new Document();
            this.AddItemIdentifiers(item, document);
            this.AddAllFields(document, item, false);
            this.AddSpecialFields(document, item);
            this.AdjustBoost(document, item);
            context.AddDocument(document);
        }

        protected virtual void IndexVersion(Item item, Item latestVersion, IndexUpdateContext context)
        {
            Assert.ArgumentNotNull(item, "item");
            Assert.ArgumentNotNull(latestVersion, "latestVersion");
            Assert.ArgumentNotNull(context, "context");
            Document document = new Document();
            this.AddVersionIdentifiers(item, latestVersion, document);
            this.AddAllFields(document, item, true);
            this.AddSpecialFields(document, item);
            this.AdjustBoost(document, item);
            context.AddDocument(document);
        }

        public void Initialize(RemoteIndex index)
        {
            Assert.ArgumentNotNull(index, "index");
            Assert.IsNotNull(index, "index");
            this._index = index;
            Assert.IsNotNull(this._database, "Database is not defined");
            Assert.IsNotNull(this._root, "Root item is not defined");
            IndexingManager.Provider.OnUpdateItem += new EventHandler(this.Provider_OnUpdateItem);
            IndexingManager.Provider.OnRemoveItem += new EventHandler(this.Provider_OnRemoveItem);
            IndexingManager.Provider.OnRemoveVersion += new EventHandler(this.Provider_OnRemoveVersion);
        }

        private bool IsHidden(Item item)
        {
            Assert.ArgumentNotNull(item, "item");
            return (item.Appearance.Hidden || ((item.Parent != null) && this.IsHidden(item.Parent)));
        }

        protected virtual bool IsMatch(Item item)
        {
            bool flag;
            Assert.ArgumentNotNull(item, "item");
            if (!this._root.Axes.IsAncestorOf(item))
            {
                return false;
            }
            if (!this._hasIncludes && !this._hasExcludes)
            {
                return true;
            }
            if (!this._templateFilter.TryGetValue(item.TemplateID.ToString(), out flag))
            {
                return !this._hasIncludes;
            }
            return flag;
        }

        protected virtual bool IsTextField(Sitecore.Data.Fields.Field field)
        {
            Assert.ArgumentNotNull(field, "field");
            if (!this.TextFieldTypes.Contains(field.Type))
            {
                return false;
            }
            TemplateField templateField = field.GetTemplateField();
            return ((templateField == null) || !templateField.ExcludeFromTextSearch);
        }

        private void Provider_OnRemoveItem(object sender, EventArgs e)
        {
            Assert.ArgumentNotNull(sender, "sender");
            Assert.ArgumentNotNull(e, "e");
            Database database = SitecoreEventArgs.GetObject(e, 0) as Database;
            if ((database != null) && (database.Name == this._database.Name))
            {
                ID iD = SitecoreEventArgs.GetID(e, 1);
                Assert.IsNotNull(iD, "ID is not passed to RemoveItem handler");
                using (IndexDeleteContext context = this._index.CreateDeleteContext())
                {
                    this.DeleteItem(iD, context);
                    context.Commit();
                }
            }
        }

        private void Provider_OnRemoveVersion(object sender, EventArgs e)
        {
            Assert.ArgumentNotNull(sender, "sender");
            Assert.ArgumentNotNull(e, "e");
            Database database = SitecoreEventArgs.GetObject(e, 0) as Database;
            if ((database != null) && (database.Name == this._database.Name))
            {
                ID iD = SitecoreEventArgs.GetID(e, 1);
                Assert.IsNotNull(iD, "ID is not passed to RemoveVersion handler");
                Language language = SitecoreEventArgs.GetObject(e, 2) as Language;
                Assert.IsNotNull(language, "Language is not passed to RemoveVersion handler");
                Version version = SitecoreEventArgs.GetObject(e, 3) as Version;
                Assert.IsNotNull(version, "Version is not passed to RemoveVersion handler");
                using (IndexDeleteContext context = this._index.CreateDeleteContext())
                {
                    this.DeleteVersion(iD, language.ToString(), version.ToString(), context);
                    context.Commit();
                }
            }
        }

        private void Provider_OnUpdateItem(object sender, EventArgs e)
        {
            Assert.ArgumentNotNull(sender, "sender");
            Assert.ArgumentNotNull(e, "e");
            Database database = SitecoreEventArgs.GetObject(e, 0) as Database;
            if ((database != null) && (database.Name == this._database.Name))
            {
                Item item = SitecoreEventArgs.GetObject(e, 1) as Item;
                if (item != null)
                {
                    this.UpdateItem(item);
                }
            }
        }

        public void RemoveTextFieldType(string type)
        {
            Assert.ArgumentNotNullOrEmpty(type, "type");
            this._textFieldTypes.Remove(type);
        }

        public void UpdateDatabase(Database database)
        {
            Assert.ArgumentNotNull(database, "database");
            Item rootItem = database.GetRootItem();
            if (rootItem != null)
            {
                this.UpdateTree(rootItem);
            }
        }

        public void UpdateItem(Item item)
        {
            Assert.ArgumentNotNull(item, "item");
            if (this.IsMatch(item))
            {
                this.DeleteItem(item);
                this.AddItem(item);
            }
        }

        public void UpdateTree(Item root)
        {
            Assert.ArgumentNotNull(root, "root");
            if (root.Axes.IsDescendantOf(this._root))
            {
                this.DeleteTree(root);
                this.AddTree(root);
            }
        }

        public void UpdateVersion(Item version)
        {
            Assert.ArgumentNotNull(version, "version");
            if (this.IsMatch(version))
            {
                this.DeleteVersion(version);
                this.AddVersion(version);
            }
        }

        // Properties
        public string Database
        {
            get
            {
                if (this._database != null)
                {
                    return this._database.Name;
                }
                return null;
            }
            set
            {
                Assert.ArgumentNotNullOrEmpty(value, "value");
                this._database = Factory.GetDatabase(value);
                Assert.IsNotNull(this._database, "Database " + value + " does not exist");
                using (new SecurityDisabler())
                {
                    this._root = this._database.GetRootItem();
                }
                Assert.IsNotNull(this._root, "Root item is not defined for " + value + " database.");
            }
        }

        public bool IndexAllFields
        {
            get
            {
                return this._indexAllFields;
            }
            set
            {
                this._indexAllFields = value;
            }
        }

        public bool MonitorChanges
        {
            get
            {
                return this._monitorChanges;
            }
            set
            {
                this._monitorChanges = value;
            }
        }

        public string Root
        {
            get
            {
                if (this._root != null)
                {
                    return this._root.ID.ToString();
                }
                return null;
            }
            set
            {
                Assert.ArgumentNotNullOrEmpty(value, "value");
                Assert.IsNotNull(this._database, "database is not set yet");
                this._root = ItemManager.GetItem(value, Language.Invariant, Sitecore.Data.Version.Latest, this._database, SecurityCheck.Disable);
            }
        }

        protected List<string> TextFieldTypes
        {
            get
            {
                return this._textFieldTypes;
            }
            set
            {
                Assert.ArgumentNotNull(value, "value");
                this._textFieldTypes = value;
            }
        }
    }




}
