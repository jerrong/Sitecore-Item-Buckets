//-----------------------------------------------------------------------
// <copyright file="BucketManager.cs" company="Sitecore A/S">
//     Sitecore A/S. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Threading.Tasks;
using System.Web;
using Lucene.Net.Search;
using Sitecore.Events;
using Sitecore.ItemBucket.Kernel.Kernel.Search;
using Sitecore.ItemBuckets.BigData.RamDirectory;
using Sitecore.ItemBuckets.BigData.RemoteIndex;
using Sitecore.Search;
using Sitecore.Sites;
using Sitecore.Web;

namespace Sitecore.ItemBucket.Kernel.Managers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using Sitecore.Caching;
    using Sitecore.Collections;
    using Sitecore.Data;
    using Sitecore.Data.Fields;
    using Sitecore.Data.Items;
    using Sitecore.Data.Managers;
    using Sitecore.Diagnostics;
    using Sitecore.ItemBucket.Kernel.ItemExtensions.Axes;
    using Sitecore.ItemBucket.Kernel.Kernel.Util;
    using Sitecore.ItemBucket.Kernel.Search;
    using Sitecore.ItemBucket.Kernel.Templates;
    using Sitecore.ItemBucket.Kernel.Util;
    using Sitecore.SecurityModel;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// This is your entry point for doing most things with the Item Buckets. This Manager allows you to search, create, move etc. with Bucket Items
    /// </summary>
    public static class BucketManager
    {

        /// <summary>
        /// Gets LocationFilter.
        /// </summary>
        private static string LocationFilter
        {
            get
            {
                return HttpContext.Current.Request.UrlReferrer.IsNull() ? Context.Item.ID.ToString() : WebUtil.ExtractUrlParm("id", HttpContext.Current.Request.UrlReferrer.AbsoluteUri);
            }
        }

        /// <summary>
        /// This method is an extension of Item which will return the first ancestor that is a Bucket
        /// </summary>
        /// <returns>The Bucket Item</returns>
        /// <param name="item">The Item which will act as the starting point for recurssing its ancestors to find either a bucket and if not, then return the root path of the site</param>
        /// <example> var BucketContainer = Sitecore.Context.Item.GetParentBucketItemOrRoot()</example>
        public static Item GetParentBucketItemOrSiteRoot(this Item item)
        {
            return item.Axes.GetAncestors().AsParallel().Where(IsBucket).DefaultIfEmpty(Context.ContentDatabase.GetItem(Context.Site.RootPath)).First();
        }

        /// <summary>
        /// This method is an extension of Item which will return the first ancestor that is a Bucket
        /// </summary>
        /// <returns>The Bucket Item</returns>
        /// <param name="item">The Item which will act as the starting point for recurssing its ancestors to find either a bucket and if not, then return the direct parent of the item</param>
        /// <example> var BucketContainer = Sitecore.Context.Item.GetParentBucketItemOrRoot()</example>
        public static Item GetParentBucketItemOrParent(this Item item)
        {
            return item.Axes.GetAncestors().AsParallel().Where(IsBucket).DefaultIfEmpty(item.Parent).First();
        }

        /// <summary>
        /// This method is an extension of Item which will return the first ancestor that is a Bucket
        /// </summary>
        /// <returns>The Bucket Item</returns>
        /// <param name="item">The Item which will act as the starting point for recurssing its ancestors to find either a bucket and if not, then return the root path of the site</param>
        /// <example> var BucketContainer = Sitecore.Context.Item.GetParentBucketItemOrRoot()</example>
        public static Item GetParentBucketItemOrRootOrSelf(this Item item)
        {
            return item.IsABucket() ? item : item.Axes.GetAncestors().AsParallel().Where(IsBucket).DefaultIfEmpty(Context.ContentDatabase.GetItem(Context.Site.RootPath)).First();
        }

        /// <summary>
        /// This method is an extension of Item which will return the first ancestor that is a Bucket
        /// </summary>
        /// <returns>The Bucket Item</returns>
        /// /// <param name="item">The Item which will act as the starting point for recurssing its ancestors to find either a item with a Search Interface (not necessarily a bucket) and if not, then return the root path of the site</param>
        /// <example> var BucketContainer = Sitecore.Context.Item.GetParentBucketItemOrRoot()</example>
        public static Item GetParentSearchItemOrRoot(this Item item)
        {
            return item.Axes.GetAncestors().Where(items => items.GetEditors().Items.Contains(Constants.SearchEditor)).DefaultIfEmpty(Context.ContentDatabase.GetItem(Context.Site.RootPath)).Last();
        }

        /// <summary>
        /// Given an Item, this will return the name of the Index that it will use to search on
        /// </summary>
        /// <returns>The Name of the Index</returns>
        /// <param name="item">The item which will be used to determine (based off the item path) which index will be used for the search</param>
        public static string GetContextIndex(Item item)
        {
            Contract.Requires(item.IsNotNull());
            
            if (item.IsNotNull())
            {
                RemoteSearchManager.Initialize();
                foreach (var index in from index in RemoteSearchManager.Indexes
                                      let indexConfigurationNode =
                                          Configuration.Factory.GetConfigNode(
                                              "/sitecore/search/remoteconfiguration/indexes/index[@id='" + (index as RemoteIndex).Name +
                                              "']/locations/ItemBucketSearch/Root")
                                      where indexConfigurationNode != null
                                      where item.Paths.FullPath.StartsWith(indexConfigurationNode.InnerText)
                                      select index)
                {
                    return (index as RemoteIndex).Name;
                }

                InMemorySearchManager.Initialize();
                foreach (var index in from index in InMemorySearchManager.Indexes
                                      let indexConfigurationNode =
                                          Configuration.Factory.GetConfigNode(
                                              "/sitecore/search/inmemoryconfiguration/indexes/index[@id='" + (index as InMemoryIndex).Name +
                                              "']/locations/ItemBucketSearch/Root")
                                      where indexConfigurationNode != null
                                      where item.Paths.FullPath.StartsWith(indexConfigurationNode.InnerText)
                                      select index)
                {
                    return (index as InMemoryIndex).Name;
                }

                foreach (var index in from index in Sitecore.Search.SearchManager.Indexes
                                      let indexConfigurationNode =
                                          Configuration.Factory.GetConfigNode(
                                              "/sitecore/search/configuration/indexes/index[@id='" + index.Name +
                                              "']/locations/ItemBucketSearch/Root")
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
        /// <returns>List of Results of Type IEnumerable List of SitecoreItem (which implements IItem)</returns>
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
        /// <param name="numberOfItemsToReturn">0-XXXXXX (The bigger this number is the less performant it will be)</param>
        /// <param name="pageNumber">Go directly to a Page of results</param>
        /// <example>BucketManager.Search(Sitecore.Context.Item, text: "Tim", templates: "TemplateGUID")</example>
        /// <example>BucketManager.Search(Sitecore.Context.Item, text: "Tim", templates: "TemplateGUID", sortField: "_name")</example>
        public static IEnumerable<SitecoreItem> Search(Item startLocationItem, out int hitCount, string relatedIds = "", string indexName = "itembuckets_buckets", string text = "", string templates = "", string location = "", string language = "en", string id = "", string sortField = "", string sortDirection = "", string itemName = "", string startDate = "", string endDate = "", int numberOfItemsToReturn = 20, int pageNumber = 1)
        {
            Contract.Requires(startLocationItem.IsNotNull());

            using (var searcher = new IndexSearcher(indexName))
            {
                var culture = CultureInfo.CreateSpecificCulture("en-US");
                var startDateOut = DateTime.Now;
                var endDateOut = DateTime.Now.AddDays(1);
                var startFlag = true;
                var endFlag = true;
                if (!DateTime.TryParse(startDate, culture, DateTimeStyles.None, out startDateOut))
                {
                    startDateOut = DateTime.Now;
                    startFlag = false;
                }

                if (!DateTime.TryParse(endDate, culture, DateTimeStyles.None, out endDateOut))
                {
                    endDateOut = DateTime.Now.AddDays(1);
                    endFlag = false;
                }

                if (startLocationItem.IsNull())
                {
                    Log.Warn("You are trying to run an Search on an item that has a start location of null", null);
                    hitCount = 0;
                    return new List<SitecoreItem>();
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
                                              PageNumber = pageNumber
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
                                                             InclusiveStart = true, InclusiveEnd = true
                                                         }
                                                 };
                }

                var keyValuePair = searcher.GetItems(dateSearchParam);
                hitCount = keyValuePair.Key;
                return keyValuePair.Value;
            }
        }

        /// <summary>
        /// An extension of Item that allows you to launch a Search from an item
        /// </summary>
        /// <returns>List of Results of Type IEnumerable List of SitecoreItem (which implements IItem)</returns>
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
        /// <param name="numberOfItemsToReturn">0-XXXXXX (The bigger this number is the less performant it will be)</param>
        /// <example>BucketManager.Search(Sitecore.Context.Item, text: "Tim", templates: "TemplateGUID")</example>
        /// <example>BucketManager.Search(Sitecore.Context.Item, text: "Tim", relatedIds: "ItemGUID", sortField: "_name")</example>
        public static IEnumerable<SitecoreItem> Search(Item startLocationItem, SafeDictionary<string> refinements, out int hitCount, string relatedIds = "", string indexName = "itembuckets_buckets", string text = "", string templates = "", string location = "", string language = "en", string id = "", string sortField = "", string sortDirection = "", string itemName = "", string startDate = "", string endDate = "", int numberOfItemsToReturn = 20)
        {
            using (var searcher = new IndexSearcher(indexName))
            {
                var culture = CultureInfo.CreateSpecificCulture("en-US");
                var startDateOut = DateTime.Now;
                var endDateOut = DateTime.Now.AddDays(1);
                var startFlag = true;
                var endFlag = true;
                if (!DateTime.TryParse(startDate, culture, DateTimeStyles.None, out startDateOut))
                {
                    startDateOut = DateTime.Now;
                    startFlag = false;
                }

                if (!DateTime.TryParse(endDate, culture, DateTimeStyles.None, out endDateOut))
                {
                    endDateOut = DateTime.Now.AddDays(1);
                    endFlag = false;
                }

                if (startLocationItem.IsNull())
                {
                    Log.Warn("You are trying to run an Search on an item that has a start location of null", null);
                    hitCount = 0;
                    return new List<SitecoreItem>();
                }

                var dateSearchParam = new DateRangeSearchParam
                                          {
                                              ItemName = itemName,
                                              FullTextQuery = text,
                                              RelatedIds = relatedIds,
                                              TemplateIds = templates,
                                              LocationIds = startLocationItem.ID.ToString(),
                                              Language = language,
                                              SortDirection = sortDirection,
                                              Refinements = refinements,
                                              ID = id,
                                              SortByField = sortField,
                                              PageSize = numberOfItemsToReturn
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
                                                             InclusiveStart = true, InclusiveEnd = true
                                                         }
                                                 };
                }

                var keyValuePair = searcher.GetItems(dateSearchParam);
                hitCount = keyValuePair.Key;
                return keyValuePair.Value;
            }
        }

        /// <summary>
        /// An extension of Item that allows you to launch a Search from an item
        /// </summary>
        /// <returns>List of Results of Type IEnumerable List of SitecoreItem (which implements IItem)</returns>
        /// <param name="startLocationItem">The start location of the search</param>
        /// <param name="hitCount">This will output the hitCount of the search</param>
        /// <param name="currentSearchString">The raw JSON Parse query</param>
        /// <param name="indexName">Force query to run on a particular index</param>
        /// <param name="sortField">Sort query by field (must be in index)</param>
        /// <param name="sortDirection">Sort in either "asc" or "desc"</param>
        /// <example>BucketManager.Search(Sitecore.Context.Item, SearchModel)</example>
        public static IEnumerable<SitecoreItem> Search(Item startLocationItem, out int hitCount, List<SearchStringModel> currentSearchString, string indexName = "itembuckets_buckets", string sortField = "", string sortDirection = "")
        {
            var refinements = new SafeDictionary<string>();
            var searchStringModels = SearchHelper.GetTags(currentSearchString);

            if (searchStringModels.Count > 0)
            {
                foreach (var ss in searchStringModels)
                {
                    var query = ss.Value;
                    if (query.Contains("tagid="))
                    {
                        query = query.Split('|')[1].Replace("tagid=", string.Empty);
                    }
                    var db = Context.ContentDatabase ?? Context.Database;
                    refinements.Add("_tags", db.GetItem(query).ID.ToString());
                }
            }
            using (var searcher = new IndexSearcher(indexName))
            {
                var keyValuePair = searcher.GetItems(new DateRangeSearchParam { FullTextQuery = SearchHelper.GetText(currentSearchString), RelatedIds = string.Empty, SortDirection = sortDirection, TemplateIds = SearchHelper.GetTemplates(currentSearchString), LocationIds = startLocationItem.ID.ToString(), SortByField = sortField, Refinements = refinements});
                hitCount = keyValuePair.Key;
                return keyValuePair.Value;
            }
        }

        /// <summary>
        /// An extension of Item that allows you to launch a Search from an item
        /// </summary>
        /// <returns>List of Results of Type IEnumerable List of SitecoreItem (which implements IItem)</returns>
        /// <param name="startLocationItem">The start location of the search</param>
        /// <param name="queryParser">The raw JSON Parse query</param>
        /// <param name="hitCount">This will output the hitCount of the search</param>
        /// <param name="indexName">Force query to run on a particular index</param>
        public static IEnumerable<SitecoreItem> Search(Item startLocationItem, SearchParam queryParser, out int hitCount, string indexName = "itembuckets_buckets")
        {
            using (var searcher = new IndexSearcher(indexName))
            {
                var keyValuePair = searcher.GetItems(queryParser);
                hitCount = keyValuePair.Key;
                return keyValuePair.Value;
            }
        }


        /// <summary>
        /// An extension of Item that allows you to launch a Search from an item
        /// </summary>
        /// <returns>List of Results of Type IEnumerable List of SitecoreItem (which implements IItem)</returns>
        /// <param name="startLocationItem">The start location of the search</param>
        /// <param name="queryParser">The raw JSON Parse query</param>
        /// <param name="hitCount">This will output the hitCount of the search</param>
        /// <param name="indexName">Force query to run on a particular index</param>
        public static IEnumerable<SitecoreItem> Search(Query rawLuceneQuery, out int hitCount, int pageSize = 20, int pageNumber = 1, string indexName = "itembuckets_buckets")
        {
            using (var searcher = new IndexSearcher(indexName))
            {
                var keyValuePair = searcher.RunQuery(rawLuceneQuery, pageSize, pageNumber);
                hitCount = keyValuePair.Key;
                return keyValuePair.Value;
            }
        }

        /// <summary>
        /// Given an Item, this will determine if this Item is a Bucket Container
        /// </summary>
        /// <returns>Returns true if this item is a Bucket</returns>
        /// <param name="item">Item being checked to see if it is a bucket or not</param>
        public static bool IsBucket(Item item)
        {
            Contract.Requires(item.IsNotNull());

            return item.IsBucketItemCheck();
        }

        /// <summary>
        /// Based off a Template ID, when an Item is Created, Will it be bucketed.
        /// </summary>
        /// <returns>If true then it is going to be bucketed</returns>
        /// <param name="templateId">Template Id</param>
        /// <param name="database">Context Database</param>
        public static bool IsTemplateBucketable(ID templateId, Database database)
        {
            Contract.Requires(templateId.IsNotNull());
            Contract.Requires(database.IsNotNull());

            var template = database.GetTemplate(templateId).IsNull() ? database.GetItem(templateId).Template : database.GetTemplate(templateId);
            return template.IsBucketTemplateCheck();
        }

        /// <summary>
        /// Given and item, it will determine if it lives within a Bucket Container
        /// </summary>
        /// <returns>If true then the item in question lives within a bucket and is hidden from the UI</returns>
        /// <param name="item">Item Id</param>
        /// <param name="database">Context Database</param>
        public static bool IsItemContainedWithinBucket(Item item, Database database)
        {
            Contract.Requires(item.IsNotNull());
            Contract.Requires(database.IsNotNull());

            return item.Axes.GetAncestors().Any(a => a.IsBucketItemCheck());
        }

        /// <summary>
        /// The item that is passed to this method will now be made into a Bucket and all items under it will be automatically organised and hidden.
        /// </summary>
        /// <param name="item">The item that is being turned into a Bucket</param>
        public static void CreateBucket(Item item)
        {
            CreateBucket(item, (itm) => { });
        }

        /// <summary>
        /// The item that is passed to this method will now be made into a Bucket and all items under it will be automatically organised and hidden.
        /// </summary>
        /// <param name="item">The item that is being turned into a Bucket</param>
        public static void AddSearchTabToItem(Item item)
        {
            MultilistField editors = item.Fields["__Editors"];
            using (new EditContext(item, SecurityCheck.Disable))
            {
                if (!editors.Items.Contains(Constants.SearchEditor))
                {
                    var tempEditors = editors.GetItems();
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

            var bucketableItems = item.Children.ToList().Where(child => child.Template.IsBucketTemplateCheck());

            using(new EditContext(item, SecurityCheck.Disable))
            {
                item.IsBucketItemCheckBox().Checked = true;
            }
            long count = 0;
            
            foreach (var child in bucketableItems)
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
        /// <param name="itm">
        /// The itm.
        /// </param>
        /// <param name="currentSearchString">
        /// The current Search String.
        /// </param>
        /// <param name="hitCount">
        /// The hit Count.
        /// </param>
        /// <param name="indexName">
        /// The index Name.
        /// </param>
        /// <param name="sortField">
        /// The sort Field.
        /// </param>
        /// <param name="sortDirection">
        /// The sort Direction.
        /// </param>
        /// <param name="pageSize">
        /// The page Size.
        /// </param>
        /// <param name="pageNumber">
        /// The page Number.
        /// </param>
        /// <param name="parameters">
        /// The parameters.
        /// </param>
        /// <returns>
        /// IEnumreable List of Results that have been typed to a smaller version of the Item Object
        /// </returns>
        public static IEnumerable<SitecoreItem> FullSearch(Item itm, List<SearchStringModel> currentSearchString, out int hitCount, string indexName = "itembuckets_buckets", string sortField = "", string sortDirection = "", int pageSize = 0, int pageNumber = 0, object[] parameters = null)
        {
            var startDate = DateTime.Now;
            var endDate = DateTime.Now.AddDays(1);
            var locationSearch = LocationFilter;
            var refinements = new SafeDictionary<string>();
            var searchStringModels = SearchHelper.GetTags(currentSearchString);

            if (searchStringModels.Count > 0)
            {
                foreach (var ss in searchStringModels)
                {
                    var query = ss.Value;
                    if (query.Contains("tagid="))
                    {
                        query = query.Split('|')[1].Replace("tagid=", string.Empty);
                    }
                    var db = Context.ContentDatabase ?? Context.Database;
                    refinements.Add("_tags", db.GetItem(query).ID.ToString());
                }
            }

            var author = SearchHelper.GetAuthor(currentSearchString);


            var languages = SearchHelper.GetLanguages(currentSearchString);
            if (languages.Length > 0)
            {
                refinements.Add("_language", languages);
            }

            var references = SearchHelper.GetReferences(currentSearchString);

            var custom = SearchHelper.GetCustom(currentSearchString);
            if (custom.Length > 0)
            {
                var customSearch = custom.Split('|');
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

            var search = SearchHelper.GetField(currentSearchString);
            if (search.Length > 0)
            {
                var customSearch = search;
                refinements.Add(customSearch, SearchHelper.GetText(currentSearchString));
            }

            var fileTypes = SearchHelper.GetFileTypes(currentSearchString);
            if (fileTypes.Length > 0)
            {
                refinements.Add("extension", SearchHelper.GetFileTypes(currentSearchString));
            }

            var s = SearchHelper.GetSite(currentSearchString);
            if (s.Length > 0)
            {
                SiteContext siteContext = SiteContextFactory.GetSiteContext(SiteManager.GetSite(s).Name);
                var db = Context.ContentDatabase ?? Context.Database;
                var startItemId = db.GetItem(siteContext.StartPath);
                locationSearch = startItemId.ID.ToString();
            }

            var culture = CultureInfo.CreateSpecificCulture("en-US");
            var startFlag = true;
            var endFlag = true;
            if (SearchHelper.GetStartDate(currentSearchString).Any())
            {
                if (!DateTime.TryParse(SearchHelper.GetStartDate(currentSearchString), culture, DateTimeStyles.None, out startDate))
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
                var location = SearchHelper.GetLocation(currentSearchString, locationSearch);
                var locationIdFromItem = itm != null ? itm.ID.ToString() : string.Empty;
                var rangeSearch = new DateRangeSearchParam
                {
                    ID = SearchHelper.GetID(currentSearchString).IsEmpty() ? SearchHelper.GetRecent(currentSearchString) : SearchHelper.GetID(currentSearchString),
                    ShowAllVersions = false,
                    FullTextQuery = SearchHelper.GetText(currentSearchString),
                    Refinements = refinements,
                    RelatedIds = references.Any() ? references : string.Empty,
                    SortDirection = sortDirection,
                    TemplateIds = SearchHelper.GetTemplates(currentSearchString),
                    LocationIds = location == string.Empty ? locationIdFromItem : location,
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
                                                 new DateRangeSearchParam.DateRangeField(SearchFieldIDs.CreatedDate, startDate, endDate)
                                                     {
                                                         InclusiveStart = true, InclusiveEnd = true
                                                     }
                                             };
                }

                var returnResult = searcher.GetItems(rangeSearch);
                hitCount = returnResult.Key;
                return returnResult.Value;
            }
        }

        /// <summary>
        /// Using a strongly types List of SearchStringModel, you can run a search based off a JSON String
        /// </summary>
        /// <param name="itm">
        /// The itm.
        /// </param>
        /// <param name="currentSearchString">
        /// The current Search String.
        /// </param>
        /// <param name="hitCount">
        /// The hit Count.
        /// </param>
        /// <param name="indexName">
        /// The index Name.
        /// </param>
        /// <param name="sortField">
        /// The sort Field.
        /// </param>
        /// <param name="sortDirection">
        /// The sort Direction.
        /// </param>
        /// <param name="pageSize">
        /// The page Size.
        /// </param>
        /// <param name="pageNumber">
        /// The page Number.
        /// </param>
        /// <param name="parameters">
        /// The parameters.
        /// </param>
        /// <returns>
        /// IEnumreable List of Results that have been typed to a smaller version of the Item Object
        /// </returns>
        public static IEnumerable<SitecoreItem> FullSearch(List<SearchStringModel> currentSearchString, out int hitCount, string indexName = "itembuckets_buckets", string sortField = "", string sortDirection = "", int pageSize = 0, int pageNumber = 0, object[] parameters = null)
        {
            int hitCountInner = 0;
            hitCount = hitCountInner;
            return FullSearch(Sitecore.Context.Item, currentSearchString, out hitCountInner, indexName: indexName,
                              pageNumber: pageNumber, sortDirection: sortDirection, pageSize: pageSize,
                              sortField: sortField, parameters: parameters);
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

        public static void MoveItemIntoBucket(Item source, Item target)
        {
           ItemManager.MoveItem(source, CreateAndReturnDateFolderDestination(target, DateTime.Now));
        }

        public static void CopyItemIntoBucket(Item source, Item target, bool deep)
        {
            ItemManager.CopyItem(source, CreateAndReturnDateFolderDestination(target, DateTime.Now), deep);
        }

        public static void CopyItemIntoBucket(Item source, Item target)
        {
            CopyItemIntoBucket(source, target, true);
        }

        public static void DuplicateItemIntoBucket(Item source, Item target, bool deep)
        {
            CopyItemIntoBucket(source, target, deep);
        }

        public static void DuplicateItemIntoBucket(Item source, Item target)
        {
            DuplicateItemIntoBucket(source, target, true);
        }

        public static void CloneItemIntoBucket(Item source, Item target, bool deep)
        {
            source.CloneTo(CreateAndReturnDateFolderDestination(target, DateTime.Now), deep);
        }

        public static void CloneItemIntoBucket(Item source, Item target)
        {
            CloneItemIntoBucket(source, target, true);
        }

        internal static void ShowAllSubFolders(Item contextItem)
        {
            Parallel.ForEach(contextItem.Children, (item, state, i) =>
            {
                foreach (var child in item.Children.ToList())
                {
                    ShowAllSubFolders(child);
                }
                if (Context.Job.IsNotNull())
                {
                    Context.Job.Status.Messages.Add("Making " + item.Paths.FullPath + " visible");
                }
                foreach (var subChild in item.Children.ToList())
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
                                if (item.TemplateID.ToString() != Util.Constants.BucketFolder)
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
        /// <returns>This will return the destination parent Item that hosts the new Item</returns>
        /// <param name="topParent">Gets the root of where this item will be created</param>
        /// <param name="childItemCreationDateTime">Determins the folder that the item will be created within</param>
        internal static Item CreateAndReturnDateFolderDestination(Item topParent, DateTime childItemCreationDateTime)
        {
            Contract.Requires(topParent.IsNotNull());
            Contract.Requires(childItemCreationDateTime.IsNotNull());

            var database = topParent.Database;
            var dateFolder = childItemCreationDateTime.ToString(Config.BucketFolderPath);
            var destinationFolderPath = topParent.Paths.FullPath + Constants.ContentPathSeperator + dateFolder;
            Item destinationFolderItem;
            
            // TODO: Use the Path Cache to determine if the path exists instead of looking it up on the item everytime I create an item (will be noticed if programmatically adding items)
            if ((destinationFolderItem = database.GetItem(destinationFolderPath)).IsNull())
            {
                var containerTemplate = database.Templates[new TemplateID(Config.ContainerTemplateId)];
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
        /// <returns>This will return the created Item</returns>
        /// <param name="topParent">Gets the root of where this item will be created</param>
        /// <param name="itemToMove">Determines the item that is moving</param>
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
            foreach (var child in item.Children.ToList())
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
        /// Hides an Item
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
        /// Moves an Item to a Date Format Folder. Will create the folder based off the Date if the folder does not exist.
        /// </summary>
        /// <param name="topParent">Destination Folder</param>
        /// <param name="toMove">Item that is being moved</param>
        private static void MoveItemToDateFolder(Item topParent, Item toMove)
        {
            Contract.Requires(topParent.IsNotNull());
            Contract.Requires(toMove.IsNotNull());

            foreach (var item in toMove.Children.ToList())
            {
                MoveItemToDateFolder(topParent, item);
            }

            if (ShouldMoveToDateFolder(toMove))
            {
                using (new EditContext(toMove, SecurityCheck.Disable))
                {
                    if (toMove.Fields["__BucketParentRef"].IsNotNull())
                    {
                        toMove.Fields["__BucketParentRef"].Value = toMove.Parent.ID.ToString();
                    }
                }

                var destinationFolder = CreateAndReturnDateFolderDestination(topParent, toMove);
                var returnValue = ItemManager.MoveItem(toMove, destinationFolder);

                // TODO: This should be handled by MoveItem. Remove this check once Logging is placed into ItemManager code.
                if (!returnValue)
                {
                    Log.Error("There has been an issue moving the item from " + toMove + " to " + destinationFolder, returnValue);
                }
            }
            else if (ShouldDeleteInCreationOfBucket(toMove))
            {
                toMove.Delete();
            }
        }

        /// <summary>
        /// Determines if an item should move with its parent or not
        /// </summary>
        /// <returns>True if moving, False if not moving</returns>
        /// <param name="toMove">Item that is being moved</param>
        private static bool ShouldMoveToDateFolder(Item toMove)
        {
            return !ShouldDeleteInCreationOfBucket(toMove) && (toMove.Fields[Constants.ShouldNotOrganiseInBucket].IsNull()

                   || 

                   !((CheckboxField)toMove.Parent.Fields[Constants.ShouldNotOrganiseInBucket]).Checked);
        }

        /// <summary>
        /// Determines if an item should be deleted on creation of a bucket
        /// </summary>
        /// <returns>True if deleting, False if not deleting</returns>
        /// <param name="item">Item that is being moved</param>
        private static bool ShouldDeleteInCreationOfBucket(Item item)
        {
            Contract.Requires(item.IsNotNull());
            return item.TemplateID.ToString().Equals(Constants.BucketFolder);
        }

        private static string GetLocationOverride(List<SearchStringModel> _searchQuery)
        {
            return SearchHelper.GetLocation(_searchQuery).Any() ? SearchHelper.GetLocation(_searchQuery) : (SearchHelper.GetSite(_searchQuery).Any() ? getSiteIdFromName(SearchHelper.GetSite(_searchQuery)) : Sitecore.Context.Item.ID.ToString());
        }

        private static string getSiteIdFromName(string name)
        {
            SiteContext siteContext = SiteContextFactory.GetSiteContext(SiteManager.GetSite(name).Name);
            var db = Context.ContentDatabase ?? Context.Database;
            return db.GetItem(siteContext.StartPath).ID.ToString();
        }

        public static List<List<FacetReturn>> GetFacets(List<SearchStringModel> _searchQuery)
        {
            var ret = new List<List<FacetReturn>>();
            var facets = Context.ContentDatabase.GetItem(Constants.FacetFolder).Children;
            foreach (Item facet in facets)
            {
                if (facet.Fields["Enabled"].Value == "1")
                {
                    dynamic type = Activator.CreateInstance(Type.GetType(facet.Fields["Type"].Value));
                    if ((type as IFacet).IsNotNull())
                    {
                        var locationOverride = GetLocationOverride(_searchQuery);
                        using (var context = new SortableIndexSearchContext(SearchManager.GetIndex(BucketManager.GetContextIndex(Context.ContentDatabase.GetItem(locationOverride)))))
                        {

                            var query = SearchHelper.GetBaseQuery(_searchQuery, locationOverride);
                            var queryBase = IndexSearcher.ContructQuery(query);
                            var searchBitArray = new QueryFilter(queryBase).Bits(context.Searcher.GetIndexReader());
                            var res = ((IFacet)type).Filter(queryBase, _searchQuery, locationOverride, searchBitArray);
                            ret.Add(res);
                        }
                    }
                }
            }

            return ret;
        }
    }
}
