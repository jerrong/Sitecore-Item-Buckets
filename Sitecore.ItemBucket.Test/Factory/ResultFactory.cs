using Sitecore.Data.Items;

namespace Sitecore.ItemBucket.Test.Factory
{
    class ResultFactory
    {
        public static string Create(Item item)
        {
            return item.Name;
        }
    }
}
