namespace Sitecore.ItemBucket.Kernel.Kernel.Pipelines
{
    using System;
    using System.Collections.Specialized;
    using System.Runtime.Serialization;

    using Sitecore.Configuration;
    using Sitecore.Data;
    using Sitecore.Data.Items;
    using Sitecore.ItemBucket.Kernel.Kernel.Util;
    using Sitecore.Web.UI.Sheer;

    [Serializable]
    public class BucketArgs : ClientPipelineArgs
    {
        protected BucketArgs(Item item)
        {
            this._Item = item;
        }

        public BucketArgs(Item item, NameValueCollection parameters) : base(parameters)
        {
            this._Item = item;
        }

        public BucketArgs(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this._ItemId = info.GetString("itemid");
            this._DatabaseName = info.GetString("databasename");
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("itemid", this._ItemId);
            info.AddValue("databasename", this._DatabaseName);
            base.GetObjectData(info, context);
        }

        private readonly string _ItemId;

        private readonly string _DatabaseName;

        private Item _Item;

        public Item Item
        {
            get
            {
                if (this._Item.IsNull())
                {
                    if (!this._ItemId.IsNullOrEmpty() && !this._DatabaseName.IsNullOrEmpty())
                    {
                        var database = Factory.GetDatabase(this._DatabaseName);
                        if (database.IsNotNull())
                        {
                            this._Item = database.GetItem(new ID(this._ItemId));
                        }
                    }
                }

                return this._Item;
            }
        }
    }
}