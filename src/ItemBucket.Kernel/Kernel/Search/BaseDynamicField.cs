namespace Sitecore.ItemBucket.Kernel.Search
{
    using Sitecore.Data.Items;
    using Sitecore.ItemBucket.Kernel.Util;

    public abstract class BaseDynamicField : SearchField
    {
        public string FieldKey { get; set; }

        public abstract string ResolveValue(Item item);
    }
}
