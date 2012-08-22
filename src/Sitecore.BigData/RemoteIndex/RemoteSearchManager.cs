using System.Collections.Generic;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Diagnostics;
using Sitecore.ItemBuckets.BigData.RamDirectory;
using Sitecore.Search;

public static class RemoteSearchManager
{
    // Fields
    private static RemoteIndexSearchConfiguration _configuration;

    // Methods
    public static ILuceneIndex GetIndex(string id)
    {
        ILuceneIndex index;
        Assert.IsNotNullOrEmpty(id, "id");
        if (RemoteSearchSearchConfiguration.Indexes.TryGetValue(id, out index))
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
        _configuration = RemoteSearchSearchConfiguration;
    }

    // Properties
    public static int IndexCount
    {
        get
        {
            return RemoteSearchSearchConfiguration.Indexes.Count;
        }
    }

    public static IEnumerable<ILuceneIndex> Indexes
    {
        get
        {
            return _configuration.Indexes.Values;
        }
    }

    private static RemoteIndexSearchConfiguration RemoteSearchSearchConfiguration
    {
        get
        {
            if (_configuration == null)
            {
                _configuration = Factory.CreateObject("search/remoteconfiguration", true) as RemoteIndexSearchConfiguration;
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


