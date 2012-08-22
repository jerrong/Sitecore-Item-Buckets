namespace Sitecore.ItemBucket.Kernel.Search.SearchDropdowns
{
    using System.Collections.Generic;
    using System.Linq;

    using Sitecore.Configuration;
    using Sitecore.ItemBucket.Kernel.Kernel.Util;

    internal class RecentTabs : ISearchDropDown
    {
        public List<string> Process()
        {
            var searches = ClientContext.GetValue("RecentlyOpenedTabs");
            if (searches.IsNotNull())
            {
                var list = searches.ToString().Split('|').ToList().Where(s => s != string.Empty).Where(guidCheck => guidCheck.IsGuid()).Where(item => Context.ContentDatabase.GetItem(item).IsNotNull()).Select(i => Context.ContentDatabase.GetItem(i).Name + "|" + i);
                var combinedResults =
                    searches.ToString().Split('|').ToList().Where(s => s != string.Empty).Where(
                        items => items.StartsWith("Closed Tabs"));
                list = list.Concat(combinedResults);
                
                list.Reverse();
                return list.Where(item => item != string.Empty).Take(20).ToList();
            }

            return new List<string> { "You have not recently opened any items in a new tab" };
        }
    }
}
