namespace Sitecore.ItemBucket.Kernel.Search
{
    using System.Collections.Generic;

    using Sitecore.ItemBucket.Kernel.Util;

    public class FullSearch
    {
        public IEnumerable<SitecoreItem> items;
        public List<List<FacetReturn>> facets;
        public List<Tip> tips;
        public int PageNumbers;
        public string launchType;
        public string SearchTime;
        public string SearchCount;
        public int CurrentPage;
        public string Location;
    }
}
