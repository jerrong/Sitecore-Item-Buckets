using System.Collections.Generic;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Diagnostics;
using Sitecore.Search;
using SearchConfiguration = Sitecore.ItemBuckets.BigData.RamDirectory.SearchConfiguration;

public static class InMemorySearchManager
{
    
    /// <summary> 
    /// New Search Configuration for In Memory Indexes
    /// </summary>
    private static SearchConfiguration _configuration;

    // Methods
    public static ILuceneIndex GetIndex(string id)
    {
        ILuceneIndex index;
        Assert.IsNotNullOrEmpty(id, "id");
        if (SearchConfiguration.Indexes.TryGetValue(id, out index))
        {
            return index;
        }
        return null;
    }

    public static object GetObject(SearchResult hit)
    {
        Assert.ArgumentNotNull(hit, "hit");
        string url = hit.Url;
        if ((url != null) && url.StartsWith("sitecore:"))
        {
            return Database.GetItem(Assert.ResultNotNull<ItemUri>(ItemUri.Parse(url), "Url is not a parseable URI"));
        }
        return null;
    }

    public static void Initialize()
    {
        _configuration = SearchConfiguration;
    }

    // Properties
    public static int IndexCount
    {
        get
        {
            return SearchConfiguration.Indexes.Count;
        }
    }

    public static IEnumerable<ILuceneIndex> Indexes
    {
        get
        {
            return _configuration.Indexes.Values;
        }
    }

    private static SearchConfiguration SearchConfiguration
    {
        get
        {
            if (_configuration == null)
            {
                _configuration = Factory.CreateObject("search/inmemoryconfiguration", true) as SearchConfiguration;
            }
            return _configuration;
        }
    }

    public static ILuceneIndex SystemIndex
    {
        get
        {
            return GetIndex("system");
        }
    }
}


