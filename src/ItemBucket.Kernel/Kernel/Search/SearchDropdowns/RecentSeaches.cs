namespace Sitecore.ItemBucket.Kernel.Search.SearchDropdowns
{
    using System.Collections.Generic;
    using System.Linq;

    using Sitecore.Configuration;
    using Sitecore.ItemBucket.Kernel.Kernel.Util;

    internal class MyRecentSearches : ISearchDropDown
    {
        public List<string> Process()
        {
            var searches = ClientContext.GetValue("Searches");
            if (searches.IsNotNull())
            {
                var list = searches.ToString().Split('|').ToList();
                list.Reverse();
                return list.Where(item => item != string.Empty).Take(20).ToList();
            }

            return new List<string> { "You have no recent searches available" };
        }
    }
}
