namespace Sitecore.ItemBucket.Kernel.Search
{
    using System.Collections;
    using System.Collections.Generic;

    using Lucene.Net.Search;

    using Sitecore.ItemBucket.Kernel.Kernel.Util;

    public interface IFacet
    {
        List<FacetReturn> Filter(Query query, List<SearchStringModel> searchQuery, string locationFilter, BitArray baseQuery);

        Dictionary<string, int> GetSearch(Query query, List<string> filter, List<SearchStringModel> searchQuery, string locationFilter, BitArray baseQuery);
    }
}


