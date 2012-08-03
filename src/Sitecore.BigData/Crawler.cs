namespace Sitecore.BigData
{
    using System;
    using System.Collections.Generic;
    using System.Xml;

    using Lucene.Net.Documents;

    using Sitecore.Data.Items;
    using Sitecore.Data.Managers;
    using Sitecore.Diagnostics;
    using Sitecore.Search;
    using Sitecore.Search.Crawlers;
    using Sitecore.Xml;

    public class Crawler : DatabaseCrawler
    {
        private readonly Dictionary<string, string> filters = new Dictionary<string, string>();

        public virtual void RemoveSpecialFields(XmlNode configNode)
        {
            Assert.ArgumentNotNull(configNode, "configNode");
            var filterType = XmlUtil.GetAttribute("type", configNode);
            var filtervalue = XmlUtil.GetValue(configNode);
            this.filters.Add(filtervalue, filterType);
        }

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
                            document.Add(base.CreateValueField(builtinFieldsName, valueFunc(item)));
                        }
                        break;
                    case "item":
                        if (TemplateManager.IsTemplate(item))
                        {
                            document.Add(base.CreateValueField(builtinFieldsName, valueFunc(item)));
                        }
                        break;
                }
            }
            else
            {
                document.Add(base.CreateValueField(builtinFieldsName, valueFunc(item)));
            }
        }

        public virtual bool IsHidden(Item item)
        {
            Assert.ArgumentNotNull(item, "item");
            return (item.Appearance.Hidden || ((item.Parent != null) && this.IsHidden(item.Parent)));
        }
    }
}
