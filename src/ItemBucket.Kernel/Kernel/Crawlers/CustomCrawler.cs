namespace  Sitecore.ItemBucket.Kernel.Crawlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml;

    using Lucene.Net.Documents;

    using Sitecore.Collections;
    using Sitecore.Data;
    using Sitecore.Data.Items;
    using Sitecore.Diagnostics;
    using Sitecore.ItemBucket.Kernel.Kernel.Crawlers;
    using Sitecore.ItemBucket.Kernel.Kernel.Util;
    using Sitecore.ItemBucket.Kernel.Search;
    using Sitecore.ItemBucket.Kernel.Util;
    using Sitecore.Search;
    using Sitecore.Xml;

    using LuceneField = Lucene.Net.Documents.Field;
    using SCField = Sitecore.Data.Fields.Field;

    /// <summary>
    /// Custom Buckets Crawler
    /// </summary>
    public class CustomCrawler : Sitecore.BigData.Crawler
    {
        #region Fields
        private List<BaseDynamicField> _dynamicFields = new List<BaseDynamicField>();
        private List<CustomField> _customFields = new List<CustomField>();
        private SafeDictionary<string, string> _fieldCrawlers = new SafeDictionary<string, string>();
        private readonly SafeDictionary<string, bool> _fieldFilter = new SafeDictionary<string, bool>();
        private SafeDictionary<string, SearchField> _fieldTypes = new SafeDictionary<string, SearchField>();
        private bool _hasFieldExcludes;
        private bool _hasFieldIncludes;

        #endregion

        /// <summary>
        /// Exclude a Particular Field
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        public void ExcludeField(string value)
        {
            Assert.IsTrue(ID.IsID(value), "fieldId parameter is not a valid GUID");
            this._hasFieldExcludes = true;
            this._fieldFilter[value] = false;
        }

        /// <summary>
        /// Include a Particular Field
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        public void IncludeField(string value)
        {
            Assert.IsTrue(ID.IsID(value), "fieldId parameter is not a valid GUID");
            this._hasFieldIncludes = true;
            this._fieldFilter[value] = true;
        }
        
        /// <summary>
        /// Add all field types to index
        /// </summary>
        /// <param name="configNode">
        /// The config node.
        /// </param>
        public virtual void AddFieldTypes(XmlNode configNode)
        {
            Assert.ArgumentNotNull(configNode, "configNode");
            var fieldName = XmlUtil.GetAttribute("name", configNode);
            var storageType = XmlUtil.GetAttribute("storageType", configNode);
            var indexType = XmlUtil.GetAttribute("indexType", configNode);
            var vectorType = XmlUtil.GetAttribute("vectorType", configNode);
            var boost = XmlUtil.GetAttribute("boost", configNode);
            var searchField = new SearchField(storageType, indexType, vectorType, boost);
            this.FieldTypes.Add(fieldName.ToLowerInvariant(), searchField);
        }

        /// <summary>
        /// Parses a configuration entry for a custom field and adds it to a collection of custom fields.
        /// </summary>
        /// <param name="node">Configuration entry</param>
        public void AddCustomField(XmlNode node)
        {
            var field = CustomField.ParseConfigNode(node);
            if (field == null)
            {
                throw new InvalidOperationException("Could not parse custom field entry: " + node.OuterXml);
            }

            this._customFields.Add(field);
        }

        #region Override Methods

        /// <summary>
        /// Index version of Item
        /// </summary>
        /// <param name="item">
        /// The item.
        /// </param>
        /// <param name="latestVersion">
        /// The latest version.
        /// </param>
        /// <param name="context">
        /// The context.
        /// </param>
        protected override void IndexVersion(Item item, Item latestVersion, IndexUpdateContext context)
        {
            if (item.Template.IsNotNull())
            {
                base.IndexVersion(item, latestVersion, context);
            }
            else
            {
                Log.Warn(string.Format("Custom Database Crawler: Cannot update item version. Reason: Template is NULL in item '{0}'.", item.Paths.FullPath), this);
            }
        }

        /// <summary>
        /// Override for Adding every field to the Index
        /// </summary>
        /// <param name="document">
        /// The document.
        /// </param>
        /// <param name="item">
        /// The item.
        /// </param>
        /// <param name="versionSpecific">
        /// The version specific.
        /// </param>
        protected override void AddAllFields(Document document, Item item, bool versionSpecific)
        {
            Assert.ArgumentNotNull(document, "document");
            Assert.ArgumentNotNull(item, "item");
            var fields = this.FilteredFields(item);
            fields.ForEach(field => this.ProcessField(field, document));
            this.AddCustomFields(document, item); 
            this.ProcessDynamicFields(document, item);
        }

        /// <summary>
        /// Process the Field to be added to the index
        /// </summary>
        /// <param name="field">
        /// The field.
        /// </param>
        /// <param name="document">
        /// The document.
        /// </param>
        protected virtual void ProcessField(SCField field, Document document)
        {
            var value = ExtendedFieldCrawlerFactory.GetFieldCrawlerValue(field, this.FieldCrawlers);
        
            if (value.IsNullOrEmpty())
            {
                return;
            }

            if (field.TypeKey == "integer" || field.TypeKey == "number")
            {
                value = value.PadLeft(8, '0');
            }

            value = IdHelper.ProcessGUIDs(value);
            this.ProcessField(document, field.Key, value, this.GetStorageType(field), this.GetIndexType(field), this.GetVectorType(field));
            if (this.GetIndexType(field) == LuceneField.Index.TOKENIZED)
            {
                this.ProcessField(document, BuiltinFields.Content, value, LuceneField.Store.NO, LuceneField.Index.TOKENIZED);
            }
        }

        #endregion

        #region Config Methods

        /// <summary>
        /// Creates a Lucene field.
        /// </summary>
        /// <param name="fieldKey">Field name</param>
        /// <param name="fieldValue">Field value</param>
        /// <param name="storeType">Storage option</param>
        /// <param name="indexType">Index type</param>
        /// <param name="boost">Boosting parameter</param>
        /// <returns>Fieldable Type</returns>
        private static Fieldable CreateField(string fieldKey, string fieldValue, Field.Store storeType, Field.Index indexType, float boost)
        {
            var field = new Field(fieldKey, fieldValue, storeType, indexType);
            field.SetBoost(boost);
            return field;
        }

        /// <summary>
        /// Loops through the collection of custom fields and adds them to fields collection of each indexed item.
        /// </summary>
        /// <param name="document">Lucene document</param>
        /// <param name="item">Sitecore data item</param>
        private void AddCustomFields(Document document, Item item)
        {
            foreach (var field in this._customFields)
            {
                document.Add(CreateField(field.LuceneFieldName, field.GetFieldValue(item, field.Formating), field.StorageType, field.IndexType, this.Boost));
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Create a Field
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="storageType">
        /// The storage type.
        /// </param>
        /// <param name="indexType">
        /// The index type.
        /// </param>
        /// <param name="vectorType">
        /// The vector type.
        /// </param>
        /// <param name="boost">
        /// The boost.
        /// </param>
        /// <returns>
        /// Abstract Field
        /// </returns>
        protected AbstractField CreateField(string name, string value, LuceneField.Store storageType, LuceneField.Index indexType, LuceneField.TermVector vectorType, float boost)
        {
            var field = new LuceneField(name, value, storageType, indexType, vectorType);
            field.SetBoost(boost);
            return field;
        }

        /// <summary>
        /// Filter Fields
        /// </summary>
        /// <param name="item">
        /// The item.
        /// </param>
        /// <returns>
        /// List of Sitecore Fields
        /// </returns>
        protected virtual List<SCField> FilteredFields(Item item)
        {
            var filteredFields = new List<SCField>();
            if (this.IndexAllFields)
            {
                item.Fields.ReadAll();
                filteredFields.AddRange(item.Fields);
            }
            else if (this.HasFieldIncludes)
            {
                filteredFields.AddRange((from p in this.FieldFilter where p.Value select p).Select(includeFieldId => item.Fields[ID.Parse(includeFieldId.Key)]));
            }

            if (this.HasFieldExcludes)
            {
                foreach (SCField field in item.Fields)
                {
                    this.FilterFields(filteredFields, field);
                }
            }
       
            return filteredFields.Where(f => !string.IsNullOrEmpty(f.Key)).ToList();
        }

        /// <summary>
        /// Filter Fields
        /// </summary>
        /// <param name="filteredFields">
        /// The filtered fields.
        /// </param>
        /// <param name="field">
        /// The field.
        /// </param>
        private void FilterFields(List<SCField> filteredFields, SCField field)
        {
            var fieldKey = field.ID.ToString();
            if (!(!this.FieldFilter.ContainsKey(fieldKey) ? true : this.FieldFilter[fieldKey]))
            {
                filteredFields.Remove(field);
            }
        }

        /// <summary>
        /// Get Index Type
        /// </summary>
        /// <param name="field">
        /// The field.
        /// </param>
        /// <returns>
        /// Lucene Index
        /// </returns>
        protected LuceneField.Index GetIndexType(SCField field)
        {
            if (this.FieldTypes.ContainsKey(field.TypeKey))
            {
                object searchField = this.FieldTypes[field.TypeKey];
                if (searchField is SearchField)
                {
                    return (searchField as SearchField).IndexType;
                }
            }
            return LuceneField.Index.UN_TOKENIZED;
        }

        /// <summary>
        /// Get Storage Type
        /// </summary>
        /// <param name="field">
        /// The field.
        /// </param>
        /// <returns>
        /// Store Type
        /// </returns>
        protected LuceneField.Store GetStorageType(SCField field)
        {
            if (this.FieldTypes.ContainsKey(field.TypeKey))
            {
                var searchField = this.FieldTypes[field.TypeKey];
                return searchField.StorageType;
            }

            return LuceneField.Store.NO;
        }

        /// <summary>
        /// Get Vector Type
        /// </summary>
        /// <param name="field">
        /// The field.
        /// </param>
        /// <returns>
        /// Term Vector
        /// </returns>
        protected LuceneField.TermVector GetVectorType(SCField field)
        {
            if (this.FieldTypes.ContainsKey(field.TypeKey))
            {
                var searchField = this.FieldTypes[field.TypeKey];
                return searchField.VectorType;
            }

            return LuceneField.TermVector.NO;
        }

        /// <summary>
        /// Process Dynamic Fields
        /// </summary>
        /// <param name="document">
        /// The document.
        /// </param>
        /// <param name="item">
        /// The item.
        /// </param>
        protected virtual void ProcessDynamicFields(Document document, Item item)
        {
            foreach (var dynamicField in this.DynamicFields)
            {
                var fieldValue = dynamicField.ResolveValue(item);

                if (fieldValue.IsNull())
                {
                    this.ProcessField(document, dynamicField.FieldKey, fieldValue, dynamicField.StorageType, dynamicField.IndexType, dynamicField.VectorType, dynamicField.Boost);
                }
            }
        }

        /// <summary>
        /// Process Field
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        /// <param name="fieldKey">
        /// The field key.
        /// </param>
        /// <param name="fieldValue">
        /// The field value.
        /// </param>
        /// <param name="storage">
        /// The storage.
        /// </param>
        /// <param name="index">
        /// The index.
        /// </param>
        protected virtual void ProcessField(Document doc, string fieldKey, string fieldValue, LuceneField.Store storage, LuceneField.Index index)
        {
            this.ProcessField(doc, fieldKey, fieldValue, storage, index, LuceneField.TermVector.NO);
        }

        /// <summary>
        /// Process Field
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        /// <param name="fieldKey">
        /// The field key.
        /// </param>
        /// <param name="fieldValue">
        /// The field value.
        /// </param>
        /// <param name="storage">
        /// The storage.
        /// </param>
        /// <param name="index">
        /// The index.
        /// </param>
        /// <param name="vector">
        /// The vector.
        /// </param>
        protected virtual void ProcessField(Document doc, string fieldKey, string fieldValue, LuceneField.Store storage, LuceneField.Index index, LuceneField.TermVector vector)
        {
            this.ProcessField(doc, fieldKey, fieldValue, storage, index, vector, 1f);
        }

        /// <summary>
        /// Process Field
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        /// <param name="fieldKey">
        /// The field key.
        /// </param>
        /// <param name="fieldValue">
        /// The field value.
        /// </param>
        /// <param name="storage">
        /// The storage.
        /// </param>
        /// <param name="index">
        /// The index.
        /// </param>
        /// <param name="vector">
        /// The vector.
        /// </param>
        /// <param name="boost">
        /// The boost.
        /// </param>
        protected virtual void ProcessField(Document doc, string fieldKey, string fieldValue, LuceneField.Store storage, LuceneField.Index index, LuceneField.TermVector vector, float boost)
        {
            if ((!fieldKey.IsNullOrEmpty() && !fieldValue.IsNullOrEmpty())
               && (index != LuceneField.Index.NO || storage != LuceneField.Store.NO))
            {
                doc.Add(CreateField(fieldKey, fieldValue.ToLowerInvariant(), storage, index, vector, boost));
            }
        }

        #endregion

        /// <summary>
        /// Gets DynamicFields.
        /// </summary>
        protected List<BaseDynamicField> DynamicFields
        {
            get
            {
                return this._dynamicFields;
            }
        }

        /// <summary>
        /// Gets FieldCrawlers.
        /// </summary>
        protected SafeDictionary<string, string> FieldCrawlers
        {
            get
            {
                return this._fieldCrawlers;
            }
        }

        /// <summary>
        /// Gets FieldFilter.
        /// </summary>
        protected SafeDictionary<string, bool> FieldFilter
        {
            get
            {
                return this._fieldFilter;
            }
        }

        /// <summary>
        /// Gets FieldTypes.
        /// </summary>
        protected SafeDictionary<string, SearchField> FieldTypes
        {
            get
            {
                return this._fieldTypes;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether HasFieldExcludes.
        /// </summary>
        protected bool HasFieldExcludes
        {
            get
            {
                return this._hasFieldExcludes;
            }

            set
            {
                this._hasFieldExcludes = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether HasFieldIncludes.
        /// </summary>
        protected bool HasFieldIncludes
        {
            get
            {
                return this._hasFieldIncludes;
            }

            set
            {
                this._hasFieldIncludes = value;
            }
        }
    }
}
