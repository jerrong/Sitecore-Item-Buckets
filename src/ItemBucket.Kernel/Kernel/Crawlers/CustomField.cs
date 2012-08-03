using Sitecore.Diagnostics;

namespace Sitecore.ItemBucket.Kernel.Kernel.Crawlers
{
    using System;
    using System.Xml;
    using Lucene.Net.Documents;
    using Sitecore.Data;
    using Sitecore.Data.Items;
    using Sitecore.ItemBucket.Kernel.Util;
    using Sitecore.Xml;

    /// <summary>
    /// Custom Index Field
    /// </summary>
    public class CustomField
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomField"/> class.
        /// </summary>
        public CustomField()
        {
            this.FieldId = ID.Null;
            this.FieldName = string.Empty;
            this.LuceneFieldName = string.Empty;
        }

        /// <summary>
        /// Gets FieldId.
        /// </summary>
        public ID FieldId
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets FieldName.
        /// </summary>
        public string FieldName { get; private set; }

        /// <summary>
        /// Gets or sets Formating.
        /// </summary>
        public string Formating { get; set; }

        /// <summary>
        /// Gets or sets StorageType.
        /// </summary>
        public Field.Store StorageType { get; set; }

        /// <summary>
        /// Gets or sets IndexType.
        /// </summary>
        public Field.Index IndexType { get; set; }

        /// <summary>
        /// Gets LuceneFieldName.
        /// </summary>
        public string LuceneFieldName { get; private set; }

        /// <summary>
        /// Parse Config
        /// </summary>
        /// <param name="node">
        /// The node.
        /// </param>
        /// <returns>
        /// Custom Field
        /// </returns>
        public static CustomField ParseConfigNode(XmlNode node)
        {
            var field = new CustomField();
            var fieldName = XmlUtil.GetValue(node);
            if (ID.IsID(fieldName))
            {
                field.FieldId = ID.Parse(fieldName);
            }
            else
            {
                field.FieldName = fieldName;
            }

            field.LuceneFieldName = XmlUtil.GetAttribute("luceneName", node);
            field.StorageType = GetStorageType(node);
            field.IndexType = GetIndexType(node);
            field.Formating = GetFormattingType(node);

            if (!IsValidField(field))
            {
                return null;
            }

            return field;
        }

        /// <summary>
        /// Get Field Value
        /// </summary>
        /// <param name="item">
        /// The item.
        /// </param>
        /// <returns>
        /// </returns>
        public string GetFieldValue(Item item)
        {
            if (!ID.IsNullOrEmpty(this.FieldId))
            {
                return item[ID.Parse(this.FieldId)];
            }

            if (!string.IsNullOrEmpty(this.FieldName))
            {
                return item[this.FieldName];
            }

            return string.Empty;
        }

        /// <summary>
        /// Get Field Value Overload with Formatting
        /// </summary>
        /// <param name="item">
        /// The item.
        /// </param>
        /// <param name="formatting">
        /// The formatting.
        /// </param>
        /// <returns>
        /// String Formatted
        /// </returns>
        public string GetFieldValue(Item item, string formatting)
        {
            if (!formatting.Equals(string.Empty))
            {
                try
                {
                    if (!ID.IsNullOrEmpty(this.FieldId))
                    {
                        return XmlConvert.ToDateTime(item[ID.Parse(this.FieldId)].Split(':')[0], "yyyyMMddTHHmmss").ToString(formatting);
                    }

                    if (!string.IsNullOrEmpty(this.FieldName))
                    {
                        return XmlConvert.ToDateTime(item[this.FieldName].Split(':')[0], "yyyyMMddTHHmmss").ToString(formatting);
                    }
                }
                catch (Exception exc)
                {
                    Log.Error("Failed to Get Field Value", exc, this);
                    return string.Empty;
                }
            }

            if (!ID.IsNullOrEmpty(this.FieldId))
            {
                return item[ID.Parse(this.FieldId)];
            }

            if (!string.IsNullOrEmpty(this.FieldName))
            {
                var normalised = IdHelper.NormalizeGuid(item[this.FieldName]);
                return normalised;
            }
            return string.Empty;
        }

        /// <summary>
        /// Is a Valid Field
        /// </summary>
        /// <param name="field">
        /// The field.
        /// </param>
        /// <returns>
        /// </returns>
        private static bool IsValidField(CustomField field)
        {
            if ((!string.IsNullOrEmpty(field.FieldName) || !ID.IsNullOrEmpty(field.FieldId)) && !string.IsNullOrEmpty(field.LuceneFieldName))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get Formatting Type
        /// </summary>
        /// <param name="node">
        /// The node.
        /// </param>
        /// <returns>
        /// </returns>
        private static string GetFormattingType(XmlNode node)
        {
            return XmlUtil.GetAttribute("format", node);
        }

        /// <summary>
        /// Get Index Type
        /// </summary>
        /// <param name="node">
        /// The node.
        /// </param>
        /// <returns>
        /// Field Index
        /// </returns>
        private static Field.Index GetIndexType(XmlNode node)
        {
            var indexType = XmlUtil.GetAttribute("indexType", node);
            if (!string.IsNullOrEmpty(indexType))
            {
                switch (indexType.ToLowerInvariant())
                {
                    case "no":
                        return Field.Index.NO;
                    case "tokenized":
                        return Field.Index.TOKENIZED;
                    case "untokenized":
                        return Field.Index.UN_TOKENIZED;
                    case "nonorms":
                        return Field.Index.NO_NORMS;
                }
            }

            return Field.Index.TOKENIZED;
        }

        /// <summary>
        /// Get Storage Type
        /// </summary>
        /// <param name="node">
        /// The node.
        /// </param>
        /// <returns>
        /// </returns>
        private static Field.Store GetStorageType(XmlNode node)
        {
            var storage = XmlUtil.GetAttribute("storageType", node);
            if (!string.IsNullOrEmpty(storage))
            {
                switch (storage.ToLowerInvariant())
                {
                    case "no":
                        return Field.Store.NO;
                    case "yes":
                        return Field.Store.YES;
                    case "compress":
                        return Field.Store.COMPRESS;
                }
            }

            return Field.Store.NO;
        }
    }
}
