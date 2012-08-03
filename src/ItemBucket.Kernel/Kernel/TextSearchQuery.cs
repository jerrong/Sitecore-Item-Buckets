using Sitecore.ItemBucket.Kernel.Kernel.Interfaces;

namespace Sitecore.ItemBucket.Kernel.Kernel
{
    public class TextSearchQuery : IBucketSearchQuery
    {
        public string GetSearchType()
        {
            return "text";
        }

        public string GetSearchValue(string query)
        {
            return "";
        }
    }
}
