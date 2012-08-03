namespace Sitecore.ItemBucket.BigData.Crawlers
{
    using System;
    using System.Collections.Generic;
    using System.Xml;
    using Lucene.Net.Documents;

    using Sitecore.Configuration;
    using Sitecore.Data;
    using Sitecore.Data.Items;
    using Sitecore.Data.Managers;
    using Sitecore.Diagnostics;
    using Sitecore.Events;
    using Sitecore.Globalization;
    using Sitecore.ItemBuckets.BigData.RamDirectory;
    using Sitecore.Search;
    using Sitecore.Search.Crawlers;
    using Sitecore.SecurityModel;
    using Sitecore.Xml;

    /// <summary>
    /// Crawler that you should use if you have a lot of data and want to increase index rebuild time
    /// </summary>
    public class RamCrawler : DatabaseCrawler, IRamCrawler
    {

        private Database _database;
        private InMemoryIndex _index;
        private Item _root;
        
        /// <summary>
        /// Stores the Remove Filters
        /// </summary>
        private Dictionary<string, string> filters = new Dictionary<string, string>();

        /// <summary>
        /// Method to be able to remove the special fields within Sitecore. You may not need these within every index.
        /// </summary>
        /// <param name="configNode">
        /// The config node.
        /// </param>
        public virtual void RemoveSpecialFields(XmlNode configNode)
        {
            Assert.ArgumentNotNull(configNode, "configNode");
            var filterType = XmlUtil.GetAttribute("type", configNode);
            var filtervalue = XmlUtil.GetValue(configNode);
            this.filters.Add(filtervalue, filterType);
        }

        /// <summary>
        /// Is Hidden Check
        /// </summary>
        /// <param name="item">
        /// The item.
        /// </param>
        /// <returns>
        /// True if Hidden
        /// </returns>
        public virtual bool IsHidden(Item item)
        {
            Assert.ArgumentNotNull(item, "item");
            return item.Appearance.Hidden || ((item.Parent != null) && this.IsHidden(item.Parent));
        }

        /// <summary>
        /// AddSpecial Fields Override
        /// </summary>
        /// <param name="document">
        /// The document.
        /// </param>
        /// <param name="item">
        /// The item.
        /// </param>
        protected override void AddSpecialFields(Document document, Item item)
        {
            Assert.ArgumentNotNull(document, "document");
            Assert.ArgumentNotNull(item, "item");
            document.Add(this.CreateTextField(BuiltinFields.Name, item.Name));
            document.Add(this.CreateDataField(BuiltinFields.Name, item.Name));
            this.DetectRemovalFilterAndProcess(document, item, "DisplayName", BuiltinFields.Name, (itm) => item.Appearance.DisplayName);
            this.DetectRemovalFilterValueField(document, item, "Icon", BuiltinFields.Icon, itm => itm.Appearance.Icon);
            this.DetectRemovalFilterAndProcess(document, item, "Creator", BuiltinFields.Creator, itm => itm.Statistics.CreatedBy);
            this.DetectRemovalFilterAndProcess(document, item, "Editor", BuiltinFields.Editor, itm => itm.Statistics.UpdatedBy);
            this.DetectRemovalFilterAndProcess(document, item, "AllTemplates", BuiltinFields.AllTemplates, this.GetAllTemplates);
            this.DetectRemovalFilterAndProcess(document, item, "TemplateName", BuiltinFields.TemplateName, itm => itm.TemplateName);
            if (this.DetectRemoval("Hidden"))
            {
                if (this.IsHidden(item))
                {
                    this.DetectRemovalFilterValueField(document, item, "Hidden", BuiltinFields.Hidden, itm => "1");
                }
            }

            this.DetectRemovalFilterValueField(document, item, "Created", BuiltinFields.Created, itm => item[FieldIDs.Created]);
            this.DetectRemovalFilterValueField(document, item, "Updated", BuiltinFields.Updated, itm => item[FieldIDs.Updated]);
            this.DetectRemovalFilterAndProcess(document, item, "Path", BuiltinFields.Path, this.GetItemPath);
            this.DetectRemovalFilterAndProcess(document, item, "Links", BuiltinFields.Links, this.GetItemLinks);
            var tags = this.Tags;
            if (tags.Length > 0)
            {
                document.Add(this.CreateTextField(BuiltinFields.Tags, tags));
                document.Add(this.CreateDataField(BuiltinFields.Tags, tags));
            }
        }

        /// <summary>
        /// Detect Removal or Field
        /// </summary>
        /// <param name="filterName">
        /// The filter name.
        /// </param>
        /// <returns>
        /// If the field should be removed from the index at either an item/template/both level
        /// </returns>
        private bool DetectRemoval(string filterName)
        {
            if (this.filters.ContainsKey(filterName))
            {
                switch (this.filters[filterName])
                {
                    case "both":
                        return false;
                    case "template":
                        return true;
                    case "item":
                        return true;
                    default:
                        return true;
                }
            }

            return true;
        }

        /// <summary>
        /// Remove Field from Index
        /// </summary>
        /// <param name="document">
        /// The document.
        /// </param>
        /// <param name="item">
        /// The item.
        /// </param>
        /// <param name="filterName">
        /// The filter name.
        /// </param>
        /// <param name="builtinFieldsName">
        /// The builtin fields name.
        /// </param>
        /// <param name="valueFunc">
        /// The value func.
        /// </param>
        private void DetectRemovalFilterAndProcess(Document document, Item item, string filterName, string builtinFieldsName, Func<Item, string> valueFunc)
        {
            if (this.filters.ContainsKey(filterName))
            {
                switch (this.filters[filterName])
                {
                    case "both":
                        break;
                    case "template":
                        if (!TemplateManager.IsTemplate(item))
                        {
                            document.Add(this.CreateTextField(builtinFieldsName, valueFunc(item)));
                        }

                        break;
                    case "item":
                        if (TemplateManager.IsTemplate(item))
                        {
                            document.Add(this.CreateTextField(builtinFieldsName, valueFunc(item)));
                        }

                        break;
                }
            }
            else
            {
                document.Add(this.CreateTextField(builtinFieldsName, valueFunc(item)));
            }
        }

        /// <summary>
        /// Detect Remove Filter Value
        /// </summary>
        /// <param name="document">
        /// The document.
        /// </param>
        /// <param name="item">
        /// The item.
        /// </param>
        /// <param name="filterName">
        /// The filter name.
        /// </param>
        /// <param name="builtinFieldsName">
        /// The builtin fields name.
        /// </param>
        /// <param name="valueFunc">
        /// The value func.
        /// </param>
        private void DetectRemovalFilterValueField(Document document, Item item, string filterName, string builtinFieldsName, Func<Item, string> valueFunc)
        {
            if (this.filters.ContainsKey(filterName))
            {
                switch (this.filters[filterName])
                {
                    case "both":
                        break;
                    case "template":
                        if (!TemplateManager.IsTemplate(item))
                        {
                            document.Add(this.CreateValueField(builtinFieldsName, valueFunc(item)));
                        }

                        break;
                    case "item":
                        if (TemplateManager.IsTemplate(item))
                        {
                            document.Add(this.CreateValueField(builtinFieldsName, valueFunc(item)));
                        }

                        break;
                }
            }
            else
            {
                document.Add(this.CreateValueField(builtinFieldsName, valueFunc(item)));
            }
        }

        public void set_Database(string value)
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
        
        public void set_Root(string value)
        {
            Assert.ArgumentNotNullOrEmpty(value, "value");
            Assert.IsNotNull(this._database, "database is not set yet");
            this._root = ItemManager.GetItem(value, Language.Invariant, Sitecore.Data.Version.Latest, this._database, SecurityCheck.Disable);
        }

        public void Initialize(InMemoryIndex index)
        {
            Assert.ArgumentNotNull(index, "index");
            Assert.IsNotNull(index, "index");
            this._index = index;
            this.set_Database("master");
            this.set_Root("/sitecore/content/Hello");
            IndexingManager.Provider.OnUpdateItem += new EventHandler(this.Provider_OnUpdateItem);
            IndexingManager.Provider.OnRemoveItem += new EventHandler(this.Provider_OnRemoveItem);
            IndexingManager.Provider.OnRemoveVersion += new EventHandler(this.Provider_OnRemoveVersion);

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
                Sitecore.Data.Version version = SitecoreEventArgs.GetObject(e, 3) as Sitecore.Data.Version;
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


    }
}
