namespace Sitecore.ItemBucket.Kernel.Util
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;

    using Sitecore.Configuration;
    using Sitecore.Data;
    using Sitecore.ItemBucket.Kernel.Kernel.Interfaces;
    using Sitecore.ItemBucket.Kernel.Kernel.Util;
    using Sitecore.Search;

    /// <summary>
    /// A small implementation of the Sitecore Item Class
    /// </summary>
    sealed public class SitecoreItem : IItem
    {
        public SitecoreItem(string id, string language, string version, string databaseName) : this(new ItemUri(ID.Parse(id), Globalization.Language.Parse(language), Data.Version.Parse(version), databaseName))
        {
            this.IsMeta = false;
            this.Name = this.Fields[BuiltinFields.Name];
        }

        public SitecoreItem(string id, string language, string version, string databaseName, bool isMeta) : this(new ItemUri(ID.Parse(id), Globalization.Language.Parse(language), Data.Version.Parse(version), databaseName))
        {
            this.Name = this.Fields[BuiltinFields.Name];
            this.IsMeta = isMeta;
        }

        public SitecoreItem(string itemUri) : this(ItemUri.Parse(itemUri))
        {
        }

        public SitecoreItem(ItemUri itemUri)
        {
            this.Fields = new NameValueCollection();
            this.RenderedFields = new List<string>();
            this.Uri = itemUri;
            this.Name = this.Fields[BuiltinFields.Name];
            this.Fields.Add(BuiltinFields.Language, this.Uri.Language.Name);
            this.Fields.Add(SearchFieldIDs.Version, this.Uri.Version.Number.ToString());
        }

        public NameValueCollection Fields
        {
            get; set;
        }

        public ItemUri Uri
        {
            get; set;
        }

        public string ImagePath
        {
            get; set;
        }

        public string Cre
        {
            get; set;
        }

        public string CreBy
        {
            get; set;
        }

        public string Content
        {
            get; set;
        }

        public string Bucket
        {
            get; set;
        }

        public bool IsClone
        {
            get
            {
                return this.GetItem() != null ? this.GetItem().IsClone : false;
            }
        }

        public bool IsMeta
        {
            get; set;
        }

        public List<string> RenderedFields
        {
            get; set;
        }

        public string ItemId
        {
            get
            {
                return this.Uri.ItemID.ToString();
            }
        }

        public string Name
        {
            get; set;
        }

        public string Version
        {
            get
            {
                return this.Uri.Version.Number.ToString();
            }
        }

        public string Language
        {
            get
            {
                return this.Uri.Language.Name;
            }
        }

        public string TemplateName
        {
            get; set;
        }

        public string Path { get; set; }

        public string CreatedBy
        {
            get
            {
                return this.Fields[SearchFieldIDs.CreatedBy];
            }
        }

        public string Created
        {
            get
            {
                return this.Fields[SearchFieldIDs.Created];
            }
        }

        public string Updated
        {
            get
            {
                return this.Fields[SearchFieldIDs.Updated];
            }
        }

        public string Editor
        {
            get
            {
                return this.Fields[SearchFieldIDs.Editor];
            }
        }

        /// <summary>
        /// Get the Item object from the SitecoreItem object
        /// </summary>
        public Data.Items.Item GetItem()
        {
            var db = Factory.GetDatabase(this.Uri.DatabaseName);
            return db.IsNotNull() ? db.GetItem(this.Uri.ItemID, this.Uri.Language, this.Uri.Version) : null;
        }

        public override string ToString()
        {
            return this.Fields.Keys.Cast<string>().Aggregate(string.Format("{0}, {1}, {2}", this.Uri.ItemID, this.Uri.Language, this.Uri.Version), (current, key) => current + (", " + this.Fields[key]));
        }
    }
}
