using System;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Version = Sitecore.Data.Version;
namespace Sitecore.ItemBucket.Test
{
    class ContentItem : Item
    {

        public ContentItem(FieldList fieldList, string itemName = "default")
            : base(new ID(new Guid()), new ItemData(new ItemDefinition(new ID(new Guid()), itemName, new ID(new Guid()), new ID(new Guid())), Language.Invariant, new Version(1), fieldList), new Database("web"))
        {

        }

    }
}
