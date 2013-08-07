// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BucketManager.cs" company="Sitecore A/S">
//   Copyright (C) 2013 by Sitecore A/S
// </copyright>
// <summary>
//   Defines the BucketManager class, which is an entry point for doing most things with the Item Buckets. 
//   This Manager allows you to search, create, move etc. with Bucket Items.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.ItemBucket.Kernel.Managers
{
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.Diagnostics.Contracts;
  using System.Globalization;
  using System.Linq;
  using System.Threading;
  using System.Threading.Tasks;
  using System.Web;

  using Lucene.Net.Search;

  using Sitecore.Collections;
  using Sitecore.Configuration;
  using Sitecore.Data;
  using Sitecore.Data.Fields;
  using Sitecore.Data.Items;
  using Sitecore.Data.Managers;
  using Sitecore.Diagnostics;
  using Sitecore.ItemBucket.Kernel.ItemExtensions.Axes;
  using Sitecore.ItemBucket.Kernel.Kernel.Search;
  using Sitecore.ItemBucket.Kernel.Kernel.Util;
  using Sitecore.ItemBucket.Kernel.Search;
  using Sitecore.ItemBucket.Kernel.Templates;
  using Sitecore.ItemBucket.Kernel.Util;
  using Sitecore.ItemBuckets.BigData.RamDirectory;
  using Sitecore.ItemBuckets.BigData.RemoteIndex;
  using Sitecore.Search;
  using Sitecore.SecurityModel;
  using Sitecore.Sites;
  using Sitecore.Web;

  using IndexSearcher = Sitecore.ItemBucket.Kernel.Util.IndexSearcher;

  /// <summary>
  /// Defines the BucketManager class, which is an entry point for doing most things with the Item Buckets. 
  /// This Manager allows you to search, create, move etc. with Bucket Items.
  /// </summary>
  public static class BucketManager
  {
    /// <summary>
    /// Gets LocationFilter.
    /// </summary>
    /// <value>
    /// The location filter.
    /// </value>
    private static string LocationFilter
    {
      get
      {
        return (HttpContext.Current.Request.UrlReferrer == null)
                 ? Context.Item.ID.ToString()
                 : WebUtil.ExtractUrlParm("id", HttpContext.Current.Request.UrlReferrer.AbsoluteUri);
      }
    }

    /// <summary>
    /// This method is an extension of Item which will return the first ancestor that is a Bucket
    /// </summary>
    /// <param name="item">The Item which will act as the starting point for recursing its ancestors to find either a bucket and if not, then return the root path of the site</param>
    /// <returns>
    /// The Bucket Item
    /// </returns>
    /// <example>
    /// This sample shows how to call the <see cref="GetParentBucketItemOrSiteRoot"/> method.
    /// <code>
    /// var BucketContainer = Sitecore.Context.Item.GetParentBucketItemOrRoot()
    /// </code>
    /// </example>
    public static Item GetParentBucketItemOrSiteRoot(this Item item)
    {
      return
        item.Axes.GetAncestors()
            .AsParallel()
            .Where(IsBucket)
            .DefaultIfEmpty(Context.ContentDatabase.GetItem(Context.Site.RootPath))
            .First();
    }

    /// <summary>
    /// This method is an extension of Item which will return the first ancestor that is a Bucket
    /// </summary>
    /// <param name="item">The Item which will act as the starting point for recursing its ancestors to find either a bucket and if not, then return the direct parent of the item</param>
    /// <returns>
    /// The Bucket Item
    /// </returns>
    /// <example>var BucketContainer = Sitecore.Context.Item.GetParentBucketItemOrRoot()</example>
    public static Item GetParentBucketItemOrParent(this Item item)
    {
      return item.Axes.GetAncestors().AsParallel().Where(IsBucket).DefaultIfEmpty(item.Parent).First();
    }

    /// <summary>
    /// This method is an extension of Item which will return the first ancestor that is a Bucket
    /// </summary>
    /// <param name="item">The Item which will act as the starting point for recursing its ancestors to find either a bucket and if not, then return the root path of the site</param>
    /// <returns>
    /// The Bucket Item
    /// </returns>
    /// <example> var BucketContainer = Sitecore.Context.Item.GetParentBucketItemOrRoot()</example>
    public static Item GetParentBucketItemOrRootOrSelf(this Item item)
    {
      return item.IsABucket()
               ? item
               : item.Axes.GetAncestors()
                     .AsParallel()
                     .Where(IsBucket)
                     .DefaultIfEmpty(Context.ContentDatabase.GetItem(Context.Site.RootPath))
                     .First();
    }

    /// <summary>
    /// This method is an extension of Item which will return the first ancestor that is a Bucket
    /// </summary>
    /// <param name="item">The Item which will act as the starting point for recursing its ancestors to find either a item with a Search Interface (not necessarily a bucket) and if not, then return the root path of the site</param>
    /// <returns>
    /// The Bucket Item
    /// </returns>
    /// <example> var BucketContainer = Sitecore.Context.Item.GetParentBucketItemOrRoot()</example>
    public static Item GetParentSearchItemOrRoot(this Item item)
    {
      return
        item.Axes.GetAncestors()
            .Where(items => items.GetEditors().Items.Contains(Constants.SearchEditor))
            .DefaultIfEmpty(Context.ContentDatabase.GetItem(Context.Site.RootPath))
            .Last();
    }

    /// <summary>
    /// Gets the nearest parent bucket item or site root.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>The bucket item.</returns>
    public static Item GetNearestParentBucketItemOrSiteRoot(this Item item)
    {
      return
        item.Axes.GetAncestors()
            .AsParallel()
            .Where(IsBucket)
            .DefaultIfEmpty(Context.ContentDatabase.GetItem(Context.Site.RootPath))
            .Last();
    }

    /// <summary>
    /// Given an Item, this will return the name of the Index that it will use to search on
    /// </summary>
    /// <param name="item">The item which will be used to determine (based off the item path) which index will be used for the search</param>
    /// <returns>
    /// The Name of the Index
    /// </returns>
    public static string GetContextIndex(Item item)
    {
      Contract.Requires(item.IsNotNull());

      if (item.IsNotNull() && Config.Scaling.Enabled)
      {
        RemoteSearchManager.Initialize();
        foreach (ILuceneIndex index in from index in RemoteSearchManager.Indexes
                                       let indexConfigurationNode =
                                         Factory.GetConfigNode(
                                           "/sitecore/search/remoteconfiguration/indexes/index[@id='"
                                           + (index as RemoteIndex).Name + "']/locations/ItemBucketSearch/Root")
                                       where indexConfigurationNode != null
                                       where item.Paths.FullPath.StartsWith(indexConfigurationNode.InnerText)
                                       select index)
        {
          return (index as RemoteIndex).Name;
        }

        InMemorySearchManager.Initialize();
        foreach (ILuceneIndex index in from index in InMemorySearchManager.Indexes
                                       let indexConfigurationNode =
                                         Factory.GetConfigNode(
                                           "/sitecore/search/inmemoryconfiguration/indexes/index[@id='"
                                           + (index as InMemoryIndex).Name + "']/locations/ItemBucketSearch/Root")
                                       where indexConfigurationNode != null
                                       where item.Paths.FullPath.StartsWith(indexConfigurationNode.InnerText)
                                       select index)
        {
          return (index as InMemoryIndex).Name;
        }

        foreach (Index index in from index in SearchManager.Indexes
                                let indexConfigurationNode =
                                  Factory.GetConfigNode(
                                    "/sitecore/search/configuration/indexes/index[@id='" + index.Name
                                    + "']/locations/ItemBucketSearch/Root")
                                where indexConfigurationNode != null
                                where item.Paths.FullPath.StartsWith(indexConfigurationNode.InnerText)
                                select index)
        {
          return index.Name;
        }
      }

      return "itembuckets_buckets";
    }

    /// <summary>
    /// An extension of Item that allows you to launch a Search from an item
    /// </summary>
    /// <param name="startLocationItem">The start location of the search</param>
    /// <param name="hitCount">This will output the hitCount of the search</param>
    /// <param name="relatedIds">Pipe delimited string of Id to query by Links to and from items</param>
    /// <param name="indexName">Force query to run on a particular index</param>
    /// <param name="text">The raw text query</param>
    /// <param name="templates">Pipe delimited string of Id of Templates</param>
    /// <param name="location">Override the location of the search with an Id</param>
    /// <param name="language">Query by the two letter ISO country code</param>
    /// <param name="id">Query by ID</param>
    /// <param name="sortField">Sort query by field (must be in index)</param>
    /// <param name="sortDirection">Sort in either "asc" or "desc"</param>
    /// <param name="itemName">Query by item name</param>
    /// <param name="startDate">mm/dd/yyyy format of start date</param>
    /// <param name="endDate">mm/dd/yyyy format of end date</param>
    /// <param name="numberOfItemsToReturn">0-XXXXXX (The bigger this number is the less performing it will be)</param>
    /// <param name="pageNumber">Go directly to a Page of results</param>
    /// <returns>
    /// List of Results of Type IEnumerable List of SitecoreItem (which implements IItem)
    /// </returns>
    /// <example>BucketManager.Search(Sitecore.Context.Item, text: "Tim", templates: "TemplateGUID")</example>
    /// <example>BucketManager.Search(Sitecore.Context.Item, text: "Tim", templates: "TemplateGUID", sortField: "_name")</example>
    public static IEnumerable<SitecoreItem> Search(
      Item startLocationItem,
      out int hitCount,
      string relatedIds = "",
      string indexName = "itembuckets_buckets",
      string text = "",
      string templates = "",
      string location = "",
      string language = "en",
      string id = "",
      string sortField = "",
      string sortDirection = "",
      string itemName = "",
      string startDate = "",
      string endDate = "",
      int numberOfItemsToReturn = 20,
      int pageNumber = 1)
    {
      Contract.Requires(startLocationItem.IsNotNull());

      using (var searcher = new IndexSearcher(indexName))
      {
        DateRangeSearchParam dateSearchParam = GetFullParameters(
          startLocationItem,
          new SafeDictionary<string>(),
          relatedIds,
          indexName = "itembuckets_buckets",
          text,
          templates,
          location,
          language,
          id,
          sortField,
          sortDirection,
          itemName,
          startDate,
          endDate,
          numberOfItemsToReturn,
          pageNumber,
          QueryOccurance.Must);
        if (dateSearchParam.IsNull())
        {
          hitCount = 0;
          return new List<SitecoreItem>();
        }

        return GetItemsFromSearcher(searcher, dateSearchParam, out hitCount);
      }
    }

    /// <summary>
    /// Searches the with parameter occurrence.
    /// </summary>
    /// <param name="startLocationItem">The start location item.</param>
    /// <param name="refinements">The refinements.</param>
    /// <param name="hitCount">The hit count.</param>
    /// <param name="relatedIds">The related ids.</param>
    /// <param name="indexName">Name of the index.</param>
    /// <param name="text">The text.</param>
    /// <param name="templates">The templates.</param>
    /// <param name="location">The location.</param>
    /// <param name="language">The language.</param>
    /// <param name="id">The id.</param>
    /// <param name="sortField">The sort field.</param>
    /// <param name="sortDirection">The sort direction.</param>
    /// <param name="itemName">Name of the item.</param>
    /// <param name="startDate">The start date.</param>
    /// <param name="endDate">The end date.</param>
    /// <param name="numberOfItemsToReturn">The number of items to return.</param>
    /// <param name="pageNumber">The page number.</param>
    /// <param name="customParametersOccurance">The custom parameters occurrence.</param>
    /// <returns></returns>
    public static IEnumerable<SitecoreItem> SearchWithParameterOccurance(
      Item startLocationItem,
      SafeDictionary<string> refinements,
      out int hitCount,
      string relatedIds = "",
      string indexName = "itembuckets_buckets",
      string text = "",
      string templates = "",
      string location = "",
      string language = "en",
      string id = "",
      string sortField = "",
      string sortDirection = "",
      string itemName = "",
      string startDate = "",
      string endDate = "",
      int numberOfItemsToReturn = 20,
      int pageNumber = 1,
      QueryOccurance customParametersOccurance = QueryOccurance.Must)
    {
      Contract.Requires(startLocationItem.IsNotNull());

      using (var searcher = new IndexSearcher(indexName))
      {
        DateRangeSearchParam dateSearchParam = GetFullParameters(
          startLocationItem,
          refinements,
          relatedIds,
          indexName = "itembuckets_buckets",
          text,
          templates,
          location,
          language,
          id,
          sortField,
          sortDirection,
          itemName,
          startDate,
          endDate,
          numberOfItemsToReturn,
          pageNumber,
          customParametersOccurance);
        if (dateSearchParam.IsNull())
        {
          hitCount = 0;
          return new List<SitecoreItem>();
        }
        return GetItemsFromSearcher(searcher, dateSearchParam, out hitCount);
      }
    }
    
    /// <summary>
    /// An extension of Item that allows you to launch a Search from an item
    /// </summary>
    /// <param name="startLocationItem">The start location of the search</param>
    /// <param name="refinements">A collection of refinements to the query</param>
    /// <param name="hitCount">This will output the hitCount of the search</param>
    /// <param name="relatedIds">Pipe delimited string of Id to query by Links to and from items</param>
    /// <param name="indexName">Force query to run on a particular index</param>
    /// <param name="text">The raw text query</param>
    /// <param name="templates">Pipe delimited string of Id of Templates</param>
    /// <param name="location">Override the location of the search with an Id</param>
    /// <param name="language">Query by the two letter ISO country code</param>
    /// <param name="id">Query by ID</param>
    /// <param name="sortField">Sort query by field (must be in index)</param>
    /// <param name="sortDirection">Sort in either "asc" or "desc"</param>
    /// <param name="itemName">Query by item name</param>
    /// <param name="startDate">mm/dd/yyyy format of start date</param>
    /// <param name="endDate">mm/dd/yyyy format of end date</param>
    /// <param name="numberOfItemsToReturn">0-XXXXXX (The bigger this number is the less performing it will be)</param>
    /// <param name="pageNumber">The page number.</param>
    /// <returns>
    /// List of Results of Type IEnumerable List of SitecoreItem (which implements IItem)
    /// </returns>
    /// <example>BucketManager.Search(Sitecore.Context.Item, text: "Tim", templates: "TemplateGUID")</example>
    /// <example>BucketManager.Search(Sitecore.Context.Item, text: "Tim", relatedIds: "ItemGUID", sortField: "_name")</example>
    public static IEnumerable<SitecoreItem> Search(
      Item startLocationItem,
      SafeDictionary<string> refinements,
      out int hitCount,
      string relatedIds = "",
      string indexName = "itembuckets_buckets",
      string text = "",
      string templates = "",
      string location = "",
      string language = "en",
      string id = "",
      string sortField = "",
      string sortDirection = "",
      string itemName = "",
      string startDate = "",
      string endDate = "",
      int numberOfItemsToReturn = 20,
      int pageNumber = 1)
    {
      using (var searcher = new IndexSearcher(indexName))
      {
        DateRangeSearchParam dateSearchParam = GetFullParameters(
          startLocationItem,
          refinements,
          relatedIds,
          indexName = "itembuckets_buckets",
          text,
          templates,
          location,
          language,
          id,
          sortField,
          sortDirection,
          itemName,
          startDate,
          endDate,
          numberOfItemsToReturn,
          pageNumber,
          QueryOccurance.Must);

        if (dateSearchParam.IsNull())
        {
          hitCount = 0;
          return new List<SitecoreItem>();
        }

        return GetItemsFromSearcher(searcher, dateSearchParam, out hitCount);
      }
    }

    /// <summary>
    /// An extension of Item that allows you to launch a Search from an item
    /// </summary>
    /// <param name="startLocationItem">The start location of the search</param>
    /// <param name="hitCount">This will output the hitCount of the search</param>
    /// <param name="currentSearchString">The raw JSON Parse query</param>
    /// <param name="indexName">Force query to run on a particular index</param>
    /// <param name="sortField">Sort query by field (must be in index)</param>
    /// <param name="sortDirection">Sort in either "asc" or "desc"</param>
    /// <returns>
    /// List of Results of Type IEnumerable List of SitecoreItem (which implements IItem)
    /// </returns>
    /// <example>BucketManager.Search(Sitecore.Context.Item, SearchModel)</example>
    public static IEnumerable<SitecoreItem> Search(
      Item startLocationItem,
      out int hitCount,
      List<SearchStringModel> currentSearchString,
      string indexName = "itembuckets_buckets",
      string sortField = "",
      string sortDirection = "")
    {
      string locationIdFromItem = startLocationItem != null ? startLocationItem.ID.ToString() : string.Empty;
      DateRangeSearchParam rangeSearch = SearchHelper.GetSearchSettings(currentSearchString, locationIdFromItem);
      if (!sortField.IsNullOrEmpty())
      {
        rangeSearch.SortByField = sortField;
      }
      if (!sortDirection.IsNullOrEmpty())
      {
        rangeSearch.SortDirection = sortDirection;
      }
      using (var searcher = new IndexSearcher(indexName))
      {
        KeyValuePair<int, List<SitecoreItem>> returnResult = searcher.GetItems(rangeSearch);
        hitCount = returnResult.Key;
        return returnResult.Value;
      }
    }

    /// <summary>
    /// An extension of Item that allows you to launch a Search from an item
    /// </summary>
    /// <param name="startLocationItem">The start location of the search</param>
    /// <param name="queryParser">The raw JSON Parse query</param>
    /// <param name="hitCount">This will output the hitCount of the search</param>
    /// <param name="indexName">Force query to run on a particular index</param>
    /// <returns>
    /// List of Results of Type IEnumerable List of SitecoreItem (which implements IItem)
    /// </returns>
    public static IEnumerable<SitecoreItem> Search(Item startLocationItem, SearchParam queryParser, out int hitCount, string indexName = "itembuckets_buckets")
    {
      using (var searcher = new IndexSearcher(indexName))
      {
        KeyValuePair<int, List<SitecoreItem>> keyValuePair = searcher.GetItems(queryParser);
        hitCount = keyValuePair.Key;
        return keyValuePair.Value;
      }
    }

    /// <summary>
    /// An extension of Item that allows you to launch a Search from an item
    /// </summary>
    /// <param name="rawLuceneQuery">The raw Lucene query.</param>
    /// <param name="hitCount">This will output the hitCount of the search</param>
    /// <param name="pageSize">Size of the page.</param>
    /// <param name="pageNumber">The page number.</param>
    /// <param name="indexName">Force query to run on a particular index</param>
    /// <returns>
    /// List of Results of Type IEnumerable List of SitecoreItem (which implements IItem)
    /// </returns>
    public static IEnumerable<SitecoreItem> Search(
      Query rawLuceneQuery,
      out int hitCount,
      int pageSize = 20,
      int pageNumber = 1,
      string indexName = "itembuckets_buckets")
    {
      using (var searcher = new IndexSearcher(indexName))
      {
        KeyValuePair<int, List<SitecoreItem>> keyValuePair = searcher.RunQuery(rawLuceneQuery, pageSize, pageNumber);
        hitCount = keyValuePair.Key;
        return keyValuePair.Value;
      }
    }

    /// <summary>
    /// Given an Item, this will determine if this Item is a Bucket Container
    /// </summary>
    /// <param name="item">Item being checked to see if it is a bucket or not</param>
    /// <returns>
    /// Returns true if this item is a Bucket
    /// </returns>
    public static bool IsBucket(Item item)
    {
      Contract.Requires(item.IsNotNull());

      return item.IsBucketItemCheck();
    }

    /// <summary>
    /// Based off a Template ID, when an Item is Created, Will it be bucketed.
    /// </summary>
    /// <param name="templateId">Template Id</param>
    /// <param name="database">Context Database</param>
    /// <returns>
    /// If true then it is going to be bucketed
    /// </returns>
    public static bool IsTemplateBucketable(ID templateId, Database database)
    {
      Contract.Requires(templateId.IsNotNull());
      Contract.Requires(database.IsNotNull());

      TemplateItem template = database.GetTemplate(templateId).IsNull()
                                ? database.GetItem(templateId).Template
                                : database.GetTemplate(templateId);
      return template.IsBucketTemplateCheck();
    }

    /// <summary>
    /// Given and item, it will determine if it lives within a Bucket Container
    /// </summary>
    /// <param name="item">Item Id</param>
    /// <param name="database">Context Database</param>
    /// <returns>
    /// If true then the item in question lives within a bucket and is hidden from the UI
    /// </returns>
    public static bool IsItemContainedWithinBucket(Item item, Database database)
    {
      Contract.Requires(item.IsNotNull());
      Contract.Requires(database.IsNotNull());

      return item.Axes.GetAncestors().Any(a => a.IsBucketItemCheck());
    }

    /// <summary>
    /// The item that is passed to this method will now be made into a Bucket and all items under it will be automatically organized and hidden.
    /// </summary>
    /// <param name="item">The item that is being turned into a Bucket</param>
    public static void CreateBucket(Item item)
    {
      CreateBucket(item, (itm) => { });
    }

    /// <summary>
    /// The item that is passed to this method will now be made into a Bucket and all items under it will be automatically organized and hidden.
    /// </summary>
    /// <param name="item">The item that is being turned into a Bucket</param>
    public static void AddSearchTabToItem(Item item)
    {
      MultilistField editors = item.Fields["__Editors"];
      using (new EditContext(item, SecurityCheck.Disable))
      {
        if (!editors.Items.Contains(Constants.SearchEditor))
        {
          Item[] tempEditors = editors.GetItems();
          tempEditors.ToList().ForEach(tempEditor => editors.Remove(tempEditor.ID.ToString()));
          editors.Add(Constants.SearchEditor);
          tempEditors.ToList().ForEach(tempEditor => editors.Add(tempEditor.ID.ToString()));
        }
      }
    }

    /// <summary>
    /// The item that is passed to this method will now be made into a Bucket and all items under it will be automatically organised and hidden.
    /// </summary>
    /// <param name="item">The item that is being turned into a Bucket</param>
    /// <param name="callBack">Callback function that gets run once the Bucket Process has finised</param>
    public static void CreateBucket(Item item, Action<Item> callBack)
    {
      Contract.Requires(item.IsNotNull());

      IEnumerable<Item> bucketableItems = item.Children.ToList().Where(child => child.Template.IsBucketTemplateCheck());

      using (new EditContext(item, SecurityCheck.Disable))
      {
        item.IsBucketItemCheckBox().Checked = true;
      }

      long count = 0;

      foreach (Item child in bucketableItems)
      {
        if (Context.Job.IsNotNull())
        {
          Context.Job.Status.Processed = count;
        }

        if (ShouldDeleteInCreationOfBucket(child))
        {
          child.Children.ToList().ForEach(MakeIntoBucket);
          if (Context.Job.IsNotNull())
          {
            Context.Job.Status.Messages.Add("Deleting item " + child.Paths.FullPath);
          }
        }
        else
        {
          MoveItemToDateFolder(item, child);
          if (Context.Job.IsNotNull())
          {
            Context.Job.Status.Messages.Add("Moving item " + child.Paths.FullPath);
          }
        }

        count++;
      }

      using (new SecurityDisabler())
      {
        item.GetChildren().ToList().ForEach(HideItem);
      }

      callBack(item);
    }

    /// <summary>
    /// All Items under the current bucket will Sync and update. This is useful if you have recently imported many content items or if you have made new templates "Bucketable"
    /// </summary>
    /// <param name="item">The Bucket that is being Synced</param>
    public static void Sync(Item item)
    {
      if (item.IsABucket())
      {
        CreateBucket(item);
      }
    }

    /// <summary>
    /// Using a strongly types List of SearchStringModel, you can run a search based off a JSON String
    /// </summary>
    /// <param name="item">The item.</param>
    /// <param name="currentSearchString">The current Search String.</param>
    /// <param name="hitCount">The hit Count.</param>
    /// <param name="indexName">The index Name.</param>
    /// <param name="sortField">The sort Field.</param>
    /// <param name="sortDirection">The sort Direction.</param>
    /// <param name="pageSize">The page Size.</param>
    /// <param name="pageNumber">The page Number.</param>
    /// <param name="parameters">The parameters.</param>
    /// <returns>
    /// IEnumreable List of Results that have been typed to a smaller version of the Item Object
    /// </returns>
    public static IEnumerable<SitecoreItem> FullSearch(
      Item item,
      List<SearchStringModel> currentSearchString,
      out int hitCount,
      string indexName = "itembuckets_buckets",
      string sortField = "",
      string sortDirection = "",
      int pageSize = 0,
      int pageNumber = 0,
      object[] parameters = null)
    {
      DateTime startDate = DateTime.Now;
      DateTime endDate = DateTime.Now.AddDays(1);
      string locationSearch = LocationFilter;
      var refinements = new SafeDictionary<string>();
      List<SearchStringModel> searchStringModels = SearchHelper.GetTags(currentSearchString);

      if (searchStringModels.Count > 0)
      {
        foreach (SearchStringModel ss in searchStringModels)
        {
          string query = ss.Value;
          if (query.Contains("tagid="))
          {
            query = query.Split('|')[1].Replace("tagid=", string.Empty);
          }

          Database db = Context.ContentDatabase ?? Context.Database;
          refinements.Add("_tags", db.GetItem(query).ID.ToString());
        }
      }

      string author = SearchHelper.GetAuthor(currentSearchString);

      string languages = SearchHelper.GetLanguages(currentSearchString);
      if (languages.Length > 0)
      {
        refinements.Add("_language", languages);
      }

      string references = SearchHelper.GetReferences(currentSearchString);

      string custom = SearchHelper.GetCustom(currentSearchString);
      if (custom.Length > 0)
      {
        string[] customSearch = custom.Split('|');
        if (customSearch.Length > 0)
        {
          try
          {
            refinements.Add(customSearch[0], customSearch[1]);
          }
          catch (Exception exc)
          {
            Log.Error("Could not parse the custom search query", exc);
          }
        }
      }

      string search = SearchHelper.GetField(currentSearchString);
      if (search.Length > 0)
      {
        string customSearch = search;
        refinements.Add(customSearch, SearchHelper.GetText(currentSearchString));
      }

      string fileTypes = SearchHelper.GetFileTypes(currentSearchString);
      if (fileTypes.Length > 0)
      {
        refinements.Add("extension", SearchHelper.GetFileTypes(currentSearchString));
      }

      string s = SearchHelper.GetSite(currentSearchString);
      if (s.Length > 0)
      {
        SiteContext siteContext = SiteContextFactory.GetSiteContext(SiteManager.GetSite(s).Name);
        Database db = Context.ContentDatabase ?? Context.Database;
        Item startItemId = db.GetItem(siteContext.StartPath);
        locationSearch = startItemId.ID.ToString();
      }

      CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
      bool startFlag = true;
      bool endFlag = true;
      if (SearchHelper.GetStartDate(currentSearchString).Any())
      {
        if (
          !DateTime.TryParse(
            SearchHelper.GetStartDate(currentSearchString), culture, DateTimeStyles.None, out startDate))
        {
          startDate = DateTime.Now;
        }

        startFlag = false;
      }

      if (SearchHelper.GetEndDate(currentSearchString).Any())
      {
        if (!DateTime.TryParse(SearchHelper.GetEndDate(currentSearchString), culture, DateTimeStyles.None, out endDate))
        {
          endDate = DateTime.Now.AddDays(1);
        }

        endFlag = false;
      }

      using (var searcher = new IndexSearcher(indexName))
      {
        string location = SearchHelper.GetLocation(currentSearchString, locationSearch);
        string locationIdFromItem = item != null ? item.ID.ToString() : string.Empty;
        var rangeSearch = new DateRangeSearchParam
                            {
                              ID =
                                SearchHelper.GetID(currentSearchString).IsEmpty()
                                  ? SearchHelper.GetRecent(currentSearchString)
                                  : SearchHelper.GetID(currentSearchString),
                              ShowAllVersions = false,
                              FullTextQuery = SearchHelper.GetText(currentSearchString),
                              Refinements = refinements,
                              RelatedIds = references.Any() ? references : string.Empty,
                              SortDirection = sortDirection,
                              TemplateIds = SearchHelper.GetTemplates(currentSearchString),
                              LocationIds =
                                location == string.Empty ? locationIdFromItem : location,
                              Language = languages,
                              SortByField = sortField,
                              PageNumber = pageNumber,
                              PageSize = pageSize,
                              Author = author == string.Empty ? string.Empty : author,
                            };

        if (!startFlag || !endFlag)
        {
          rangeSearch.Ranges = new List<DateRangeSearchParam.DateRangeField>
                                 {
                                   new DateRangeSearchParam.DateRangeField(
                                     SearchFieldIDs.CreatedDate,
                                     startDate,
                                     endDate)
                                     {
                                       InclusiveStart = true,
                                       InclusiveEnd = true
                                     }
                                 };
        }

        KeyValuePair<int, List<SitecoreItem>> returnResult = searcher.GetItems(rangeSearch);
        hitCount = returnResult.Key;
        return returnResult.Value;
      }
    }

    /// <summary>
    /// Using a strongly types List of SearchStringModel, you can run a search based off a JSON String
    /// </summary>
    /// <param name="currentSearchString">The current Search String.</param>
    /// <param name="hitCount">The hit Count.</param>
    /// <param name="indexName">The index Name.</param>
    /// <param name="sortField">The sort Field.</param>
    /// <param name="sortDirection">The sort Direction.</param>
    /// <param name="pageSize">The page Size.</param>
    /// <param name="pageNumber">The page Number.</param>
    /// <param name="parameters">The parameters.</param>
    /// <returns>
    /// IEnumreable List of Results that have been typed to a smaller version of the Item Object
    /// </returns>
    public static IEnumerable<SitecoreItem> FullSearch(
      List<SearchStringModel> currentSearchString,
      out int hitCount,
      string indexName = "itembuckets_buckets",
      string sortField = "",
      string sortDirection = "",
      int pageSize = 0,
      int pageNumber = 0,
      object[] parameters = null)
    {
      int hitCountInner = 0;
      hitCount = hitCountInner;
      return FullSearch(
        Context.Item,
        currentSearchString,
        out hitCountInner,
        indexName: indexName,
        pageNumber: pageNumber,
        sortDirection: sortDirection,
        pageSize: pageSize,
        sortField: sortField,
        parameters: parameters);
    }

    /// <summary>
    /// All Items under the current bucket will Sync and update. This is useful if you have recently imported many content items or if you have made new templates "Bucketable"
    /// </summary>
    /// <param name="item">The Bucket that is being Synced</param>
    public static void Unbucket(Item item)
    {
      if (item.IsABucket())
      {
        ShowAllSubFolders(item);
        item.IsBucketItemCheckBox().Checked = false;
      }
    }

    /// <summary>
    /// Moves the item into bucket.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="target">The target.</param>
    public static void MoveItemIntoBucket(Item source, Item target)
    {
      ItemManager.MoveItem(source, CreateAndReturnDateFolderDestination(target, DateTime.Now));
    }

    /// <summary>
    /// Copies the item into bucket.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="target">The target.</param>
    /// <param name="deep">if set to <c>true</c> [deep].</param>
    public static void CopyItemIntoBucket(Item source, Item target, bool deep)
    {
      ItemManager.CopyItem(source, CreateAndReturnDateFolderDestination(target, DateTime.Now), deep);
    }

    /// <summary>
    /// Copies the item into bucket.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="target">The target.</param>
    public static void CopyItemIntoBucket(Item source, Item target)
    {
      CopyItemIntoBucket(source, target, true);
    }

    /// <summary>
    /// Duplicates the item into bucket.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="target">The target.</param>
    /// <param name="deep">if set to <c>true</c> [deep].</param>
    public static void DuplicateItemIntoBucket(Item source, Item target, bool deep)
    {
      CopyItemIntoBucket(source, target, deep);
    }

    /// <summary>
    /// Duplicates the item into bucket.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="target">The target.</param>
    public static void DuplicateItemIntoBucket(Item source, Item target)
    {
      DuplicateItemIntoBucket(source, target, true);
    }

    /// <summary>
    /// Clones the item into bucket.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="target">The target.</param>
    /// <param name="deep">if set to <c>true</c> [deep].</param>
    public static void CloneItemIntoBucket(Item source, Item target, bool deep)
    {
      source.CloneTo(CreateAndReturnDateFolderDestination(target, DateTime.Now), deep);
    }

    /// <summary>
    /// Clones the item into bucket.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="target">The target.</param>
    public static void CloneItemIntoBucket(Item source, Item target)
    {
      CloneItemIntoBucket(source, target, true);
    }

    /// <summary>
    /// Gets the facets.
    /// </summary>
    /// <param name="searchQuery">The _search query.</param>
    /// <returns>The list of the facets.</returns>
    public static List<List<FacetReturn>> GetFacets(List<SearchStringModel> searchQuery)
    {
      var ret = new List<List<FacetReturn>>();
      ChildList facets = Context.ContentDatabase.GetItem(Constants.FacetFolder).Children;
      foreach (Item facet in facets)
      {
        if (facet.Fields["Enabled"].Value == "1")
        {
          dynamic type = Activator.CreateInstance(Type.GetType(facet.Fields["Type"].Value));
          if ((type as IFacet).IsNotNull())
          {
            string locationOverride = GetLocationOverride(searchQuery);
            using (
              var context =
                new SortableIndexSearchContext(
                  SearchManager.GetIndex(GetContextIndex(Context.ContentDatabase.GetItem(locationOverride)))))
            {
              DateRangeSearchParam query = SearchHelper.GetBaseQuery(searchQuery, locationOverride);
              BooleanQuery queryBase = IndexSearcher.ContructQuery(query);
              BitArray searchBitArray = new QueryFilter(queryBase).Bits(context.Searcher.GetIndexReader());
              List<FacetReturn> res = ((IFacet)type).Filter(queryBase, searchQuery, locationOverride, searchBitArray);
              ret.Add(res);
            }
          }
        }
      }

      return ret;
    }

    /// <summary>
    /// Shows all sub folders.
    /// </summary>
    /// <param name="contextItem">The context item.</param>
    internal static void ShowAllSubFolders(Item contextItem)
    {
      Parallel.ForEach(
        contextItem.Children,
        (item, state, i) =>
          {
            ShowAllSubFolders(item);

            if (Context.Job.IsNotNull())
            {
              Context.Job.Status.Messages.Add("Making " + item.Paths.FullPath + " visible");
            }

            foreach (Item subChild in item.Children.ToList())
            {
              using (new EditContext(subChild, SecurityCheck.Disable))
              {
                ((CheckboxField)subChild.Fields["__Hidden"]).Checked = false;
              }

              using (new EditContext(subChild, SecurityCheck.Disable))
              {
                subChild.IsBucketItemCheckBox().Checked = false;

                if (subChild.Fields["__BucketParentRef"].IsNotNull())
                {
                  if (((LookupField)subChild.Fields["__BucketParentRef"]).TargetItem.IsNotNull())
                  {
                    ItemManager.MoveItem(subChild, ((LookupField)subChild.Fields["__BucketParentRef"]).TargetItem);
                  }
                  else
                  {
                    if (item.TemplateID.ToString() != Constants.BucketFolder)
                    {
                      ItemManager.MoveItem(subChild, contextItem.GetParentBucketItemOrParent());
                    }
                  }
                }
              }
            }
          });
    }

    /// <summary>
    /// Given a destination Item, this will create the structures on this item to host unstructured data
    /// </summary>
    /// <param name="topParent">Gets the root of where this item will be created</param>
    /// <param name="childItemCreationDateTime">Determines the folder that the item will be created within</param>
    /// <returns>
    /// This will return the destination parent Item that hosts the new Item
    /// </returns>
    internal static Item CreateAndReturnDateFolderDestination(Item topParent, DateTime childItemCreationDateTime)
    {
      Contract.Requires(topParent.IsNotNull());
      Contract.Requires(childItemCreationDateTime.IsNotNull());

      Database database = topParent.Database;
      string dateFolder = childItemCreationDateTime.ToString(Config.BucketFolderPath);
      DateTimeFormatInfo dateTimeInfo = Thread.CurrentThread.CurrentCulture.DateTimeFormat;

      if (dateTimeInfo.DateSeparator != Constants.ContentPathSeperator)
      {
        Log.Info(
          "ItemBuckets. DateTimeFormat inconsistency. Current date separator is " + dateTimeInfo.DateSeparator
          + " and time separator is " + dateTimeInfo.TimeSeparator + ". Relative path to folder is " + dateFolder,
          new object());
        dateFolder =
          dateFolder.Replace(dateTimeInfo.DateSeparator, Constants.ContentPathSeperator)
                    .Replace(dateTimeInfo.TimeSeparator, Constants.ContentPathSeperator);
      }

      string destinationFolderPath = topParent.Paths.FullPath + Constants.ContentPathSeperator + dateFolder;
      Item destinationFolderItem;

      // TODO: Use the Path Cache to determine if the path exists instead of looking it up on the item everytime I create an item (will be noticed if programmatically adding items)
      if ((destinationFolderItem = database.GetItem(destinationFolderPath)).IsNull())
      {
        TemplateItem containerTemplate = database.Templates[new TemplateID(Config.ContainerTemplateId)];
        destinationFolderItem = database.CreateItemPath(destinationFolderPath, containerTemplate, containerTemplate);
      }

      using (new SecurityDisabler())
      {
        topParent.GetChildren().ToList().ForEach(HideItem);
      }

      Contract.Ensures(destinationFolderItem.IsNotNull());

      return destinationFolderItem;
    }

    /// <summary>
    /// Creates a Date formatted Path to an item and then returns the item being created
    /// </summary>
    /// <param name="topParent">Gets the root of where this item will be created</param>
    /// <param name="itemToMove">Determines the item that is moving</param>
    /// <returns>
    /// This will return the created Item
    /// </returns>
    internal static Item CreateAndReturnDateFolderDestination(Item topParent, Item itemToMove)
    {
      Contract.Requires(topParent.IsNotNull());
      Contract.Requires(itemToMove.IsNotNull());

      return CreateAndReturnDateFolderDestination(topParent, itemToMove.Statistics.Created);
    }

    /// <summary>
    /// Turns an item into a Bucket
    /// </summary>
    /// <param name="item">Gets the root of where this item will be created</param>
    internal static void MakeIntoBucket(Item item)
    {
      Contract.Requires(item.IsNotNull());

      // TODO: Change to use Tail Recursion to save on CPU and memory
      foreach (Item child in item.Children.ToList())
      {
        if (ShouldDeleteInCreationOfBucket(child))
        {
          child.Children.ToList().ForEach(MakeIntoBucket);
        }
        else
        {
          MoveItemToDateFolder(item, child);
        }
      }
    }

    /// <summary>
    /// Hides an item.
    /// </summary>
    /// <param name="firstLevelChild">Gets the root of where this item will be created</param>
    private static void HideItem(Item firstLevelChild)
    {
      Contract.Requires(firstLevelChild.IsNotNull());

      if (firstLevelChild.Template.IsBucketTemplateCheck() || firstLevelChild.TemplateID == Config.BucketTemplateId)
      {
        firstLevelChild.HideItem();
      }
    }

    /// <summary>
    /// Moves the item to a Date Format Folder. Will create the folder based off the Date if the folder does not exist.
    /// </summary>
    /// <param name="topParent">Destination Folder</param>
    /// <param name="movedItemve">The item that is being moved.</param>
    private static void MoveItemToDateFolder(Item topParent, Item movedItem)
    {
      Contract.Requires(topParent.IsNotNull());
      Contract.Requires(movedItem.IsNotNull());

      foreach (Item item in movedItem.Children.ToList())
      {
        MoveItemToDateFolder(topParent, item);
      }

      if (ShouldMoveToDateFolder(movedItem))
      {
        using (new EditContext(movedItem, SecurityCheck.Disable))
        {
          if (movedItem.Fields["__BucketParentRef"].IsNotNull())
          {
            movedItem.Fields["__BucketParentRef"].Value = movedItem.Parent.ID.ToString();
          }
        }

        Item destinationFolder = CreateAndReturnDateFolderDestination(topParent, movedItem);
        bool returnValue = ItemManager.MoveItem(movedItem, destinationFolder);

        // TODO: This should be handled by MoveItem. Remove this check once Logging is placed into ItemManager code.
        if (returnValue == false)
        {
          Log.Error("There has been an issue moving the item from " + movedItem + " to " + destinationFolder, returnValue);
        }
      }
      else if (ShouldDeleteInCreationOfBucket(movedItem))
      {
        movedItem.Delete();
      }
    }

    /// <summary>
    /// Determines if an item should move with its parent or not
    /// </summary>
    /// <param name="movedItem">The item that is being moved</param>
    /// <returns>True if moving, False if not moving.</returns>
    private static bool ShouldMoveToDateFolder(Item movedItem)
    {
      return !ShouldDeleteInCreationOfBucket(movedItem)
             && (movedItem.Fields[Constants.ShouldNotOrganiseInBucket].IsNull()
                 || !((CheckboxField)movedItem.Parent.Fields[Constants.ShouldNotOrganiseInBucket]).Checked);
    }

    /// <summary>
    /// Determines if an item should be deleted on creation of a bucket
    /// </summary>
    /// <param name="item">Item that is being moved</param>
    /// <returns>True if deleting, False if not deleting.</returns>
    private static bool ShouldDeleteInCreationOfBucket(Item item)
    {
      Contract.Requires(item.IsNotNull());
      return item.TemplateID.ToString().Equals(Constants.BucketFolder);
    }

    /// <summary>
    /// Gets the location override.
    /// </summary>
    /// <param name="searchQuery">The search query.</param>
    /// <returns>The location.</returns>
    private static string GetLocationOverride(List<SearchStringModel> searchQuery)
    {
      return SearchHelper.GetLocation(searchQuery).Any()
               ? SearchHelper.GetLocation(searchQuery)
               : (SearchHelper.GetSite(searchQuery).Any()
                    ? GetSiteIdFromName(SearchHelper.GetSite(searchQuery))
                    : Context.Item.ID.ToString());
    }

    /// <summary>
    /// Gets the name of the site id from.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns>The id of the site.</returns>
    private static string GetSiteIdFromName(string name)
    {
      SiteContext siteContext = SiteContextFactory.GetSiteContext(SiteManager.GetSite(name).Name);
      Database db = Context.ContentDatabase ?? Context.Database;
      return db.GetItem(siteContext.StartPath).ID.ToString();
    }
    
    /// <summary>
    /// Gets the full parameters.
    /// </summary>
    /// <param name="startLocationItem">The start location item.</param>
    /// <param name="refinements">The refinements.</param>
    /// <param name="relatedIds">The related ids.</param>
    /// <param name="p">The p.</param>
    /// <param name="text">The text.</param>
    /// <param name="templates">The templates.</param>
    /// <param name="location">The location.</param>
    /// <param name="language">The language.</param>
    /// <param name="id">The id.</param>
    /// <param name="sortField">The sort field.</param>
    /// <param name="sortDirection">The sort direction.</param>
    /// <param name="itemName">Name of the item.</param>
    /// <param name="startDate">The start date.</param>
    /// <param name="endDate">The end date.</param>
    /// <param name="numberOfItemsToReturn">The number of items to return.</param>
    /// <param name="pageNumber">The page number.</param>
    /// <param name="customParametersOccurance">The custom parameters occurrence.</param>
    /// <returns>The date range search parameters.</returns>
    private static DateRangeSearchParam GetFullParameters(
      Item startLocationItem,
      SafeDictionary<string> refinements,
      string relatedIds,
      string p,
      string text,
      string templates,
      string location,
      string language,
      string id,
      string sortField,
      string sortDirection,
      string itemName,
      string startDate,
      string endDate,
      int numberOfItemsToReturn,
      int pageNumber,
      QueryOccurance customParametersOccurance)
    {
      CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");

      DateTime startDateOut;
      bool startFlag = true;
      if (!DateTime.TryParse(startDate, culture, DateTimeStyles.None, out startDateOut))
      {
        startDateOut = DateTime.Now;
        startFlag = false;
      }

      DateTime endDateOut;
      bool endFlag = true;
      if (!DateTime.TryParse(endDate, culture, DateTimeStyles.None, out endDateOut))
      {
        endDateOut = DateTime.Now.AddDays(1);
        endFlag = false;
      }

      if (startLocationItem.IsNull())
      {
        Log.Warn("You are trying to run an Search on an item that has a start location of null", null);
        return null;
      }

      var dateSearchParam = new DateRangeSearchParam
      {
        ItemName = itemName,
        FullTextQuery = text,
        RelatedIds = relatedIds,
        TemplateIds = templates,
        LocationIds = startLocationItem.ID.ToString(),
        Language = language,
        ID = id,
        SortDirection = sortDirection,
        SortByField = sortField,
        PageSize = numberOfItemsToReturn,
        PageNumber = pageNumber,
        Refinements = refinements,
        Occurance = customParametersOccurance
      };

      if (startFlag || endFlag)
      {
        dateSearchParam.Ranges = new List<DateRangeSearchParam.DateRangeField>
                                   {
                                     new DateRangeSearchParam.DateRangeField(
                                       SearchFieldIDs.CreatedDate,
                                       startDateOut,
                                       endDateOut)
                                       {
                                         InclusiveStart = true,
                                         InclusiveEnd = true
                                       }
                                   };
      }

      return dateSearchParam;
    }

    /// <summary>
    /// Gets the items from searcher.
    /// </summary>
    /// <param name="searcher">The searcher.</param>
    /// <param name="dateSearchParam">The date search parameter.</param>
    /// <param name="hitCount">The hit count.</param>
    /// <returns>The list of the items.</returns>
    private static IEnumerable<SitecoreItem> GetItemsFromSearcher(
      IndexSearcher searcher, DateRangeSearchParam dateSearchParam, out int hitCount)
    {
      KeyValuePair<int, List<SitecoreItem>> keyValuePair = searcher.GetItems(dateSearchParam);
      hitCount = keyValuePair.Key;
      return keyValuePair.Value;
    }
  }
}