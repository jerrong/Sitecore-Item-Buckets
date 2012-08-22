using Lucene.Net.Search;

namespace Sitecore.ItemBucket.Kernel.ItemExtensions.Axes
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Web;

    using global::ItemBucket.Kernel.ItemExtensions.Axes;

    using Sitecore.Collections;
    using Sitecore.Data.Fields;
    using Sitecore.Data.Items;
    using Sitecore.Diagnostics;
    using Sitecore.ItemBucket.Kernel.Kernel.Util;
    using Sitecore.ItemBucket.Kernel.Managers;
    using Sitecore.ItemBucket.Kernel.Search;
    using Sitecore.ItemBucket.Kernel.Util;
    using Sitecore.Sites;
    using Sitecore.StringExtensions;
    using Sitecore.Web;

    using IndexSearcher = Sitecore.ItemBucket.Kernel.Util.IndexSearcher;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// This is a group of handy extension methods to be easily able to search through content from the API
    /// </summary>
    public static class ItemExtensions
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
        /// Get Bucket Item Axes Override to do fast Ancestor/Descendant Lookups
        /// </summary>
        /// <param name="itm">
        /// The itm.
        /// </param>
        /// <returns>
        /// Bucket Item Axes
        /// </returns>
        internal static BucketItemAxes GetBucketItemAxes(this Item itm)
        {
            return new BucketItemAxes(itm);
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
        public static IEnumerable<SitecoreItem> FullSearch(this Item itm, List<SearchStringModel> currentSearchString, out int hitCount, string indexName = "itembuckets_buckets", string sortField = "", string sortDirection = "", int pageSize = 0, int pageNumber = 0, object[] parameters = null)
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

        internal static CheckboxField IsBucketItemCheckBox(this Item itm)
        {
            return itm.Fields[Constants.IsBucket];
        }

        internal static bool IsBucketItemCheck(this Item itm)
        {
            if (itm.IsNotNull())
            {
                if (itm.Fields[Constants.IsBucket].IsNotNull())
                {
                    return itm.Fields[Constants.IsBucket].Value.Equals("1");
                }
            }

            return false;
        }

        internal static string ShortUrl(this Item item)
        {
            return "/{0}/{1}/{2}/{3}".FormatWith(item.GetParentBucketItemOrSiteRoot().Name.ToLower(), item.Statistics.Created.ToString("yyyy-MM-dd"), item.ID.Guid.ToString().Substring(0, 4), item.Name.ToLowerInvariant());
        }

        internal static MultilistField GetEditors(this Item itm)
        {
            return itm.Fields["__Editors"];
        }

        /// <summary>
        /// Using a strongly types List of SearchStringModel, you can run a search based off a JSON String
        /// </summary>
        /// <returns>IEnumreable List of Results that have been typed to a smaller version of the Item Object</returns>
        public static IEnumerable<SitecoreItem> Search(this Item itm, SafeDictionary<string> refinements, out int hitCount,
            string relatedIds = "",
            string indexName = "itembuckets_buckets", 
            string text = "", 
            string templates = "", 
            string location = "", 
            string language = "en", 
            string id = "", 
            string sortField = "", 
            string itemName = "", 
            string startDate = "",
            int pageNumber = 0,
            string endDate = "", 
            string sortDirection = "",
            int numberOfItemsToReturn = 20, 
            object[] parameters = null)
        {
            if (itm.IsNull())
            {
                Log.Warn("You are trying to run an Item Extension on an item that is null", "");
                hitCount = 0;
                return new List<SitecoreItem>();
            }

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

                var dateSearchParam = new DateRangeSearchParam
                                          {
                                              ItemName = itemName,
                                              FullTextQuery = text,
                                              RelatedIds = relatedIds,
                                              TemplateIds = templates,
                                              LocationIds = location.IsEmpty() ? itm.ID.ToString() : location,
                                              Language = language,
                                              SortDirection = sortDirection,
                                              Refinements = refinements,
                                              ID = id,
                                              SortByField = sortField,
                                              PageSize = numberOfItemsToReturn,
                                              PageNumber = pageNumber 
                                          };
                if (startFlag || endFlag)
                {
                    dateSearchParam.Ranges = new List<DateRangeSearchParam.DateRangeField>
                                                 {
                                                     new DateRangeSearchParam.DateRangeField(SearchFieldIDs.CreatedDate, startDateOut, endDateOut)
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
        /// Using a strongly types List of SearchStringModel, you can run a search based off a JSON String
        /// </summary>
        /// <returns>IEnumreable List of Results that have been typed to a smaller version of the Item Object</returns>
        public static IEnumerable<SitecoreItem> Search(this Item itm, out int hitCount, string relatedIds = "", string indexName = "itembuckets_buckets", string text = "", string templates = "", string location = "", string sortDirection = "", string language = "en", string id = "", string sortField = "", string itemName = "", string startDate = "", string endDate = "", int numberOfItemsToReturn = 20, int pageNumber = 0)
        {
            var culture = CultureInfo.CreateSpecificCulture("en-US");
            var startDateOut = new DateTime();
            var endDateOut = new DateTime();
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

            using (var searcher = new IndexSearcher(indexName))
            {
                var dateSearchParam = new DateRangeSearchParam
                                          {
                                              ItemName = itemName,
                                              FullTextQuery = text,
                                              RelatedIds = relatedIds,
                                              TemplateIds = templates,
                                              LocationIds = location.IsEmpty() ? itm.ID.ToString() : location,
                                              Language = language,
                                              ID = id,
                                              SortDirection = sortDirection,
                                              SortByField = sortField,
                                              PageSize = numberOfItemsToReturn,
                                              PageNumber = pageNumber
                                          };
                var keyValuePair = searcher.GetItems(dateSearchParam);

                if (startFlag || endFlag)
                {
                    dateSearchParam.Ranges = new List<DateRangeSearchParam.DateRangeField>
                                                 {
                                                     new DateRangeSearchParam.DateRangeField(SearchFieldIDs.CreatedDate, startDateOut, endDateOut)
                                                         {
                                                             InclusiveStart = true, InclusiveEnd = true
                                                         }
                                                 };
                }

                hitCount = keyValuePair.Key;
                return keyValuePair.Value;
            }
        }

        /// <summary>
        /// Using a strongly types List of SearchStringModel, you can run a search based off a JSON String
        /// </summary>
        /// <returns>IEnumreable List of Results that have been typed to a smaller version of the Item Object</returns>
        public static IEnumerable<SitecoreItem> Search(this Item itm, List<SearchStringModel> currentSearchString, out int hitCount, string indexName = "itembuckets_buckets", string sortField = "", string sortDirection = "", int numberOfItems = 20, int pageNumber = 0)
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
                var sitecoreItems = searcher.GetItems(new DateRangeSearchParam()
                    {
                        FullTextQuery = SearchHelper.GetText(currentSearchString), 
                        RelatedIds = string.Empty, 
                        TemplateIds = SearchHelper.GetTemplates(currentSearchString), 
                        LocationIds = itm.ID.ToString(), 
                        Refinements = refinements,
                        SortByField = sortField,
                        PageNumber = pageNumber,
                        PageSize = numberOfItems
                    });

                hitCount = sitecoreItems.Key;
                return sitecoreItems.Value;
            }
        }

        /// <summary>
        /// Using a strongly types List of SearchStringModel, you can run a search based off a JSON String
        /// </summary>
        /// <returns>IEnumreable List of Results that have been typed to a smaller version of the Item Object</returns>
        public static IEnumerable<SitecoreItem> Search(this Item itm, SearchParam queryParser, out int hitCount, string indexName = "itembuckets_buckets")
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
        public static IEnumerable<SitecoreItem> Search(this Item itm, Query rawLuceneQuery, out int hitCount, int pageSize = 20, int pageNumber = 1, string indexName = "itembuckets_buckets")
        {
            using (var searcher = new IndexSearcher(indexName))
            {
                var keyValuePair = searcher.RunQuery(rawLuceneQuery, pageSize, pageNumber);
                hitCount = keyValuePair.Key;
                return keyValuePair.Value;
            }
        }

        /// <summary>
        /// Using a strongly types List of SearchStringModel, you can run a search based off a JSON String
        /// </summary>
        /// <returns>IEnumreable List of Results that have been typed to a smaller version of the Item Object</returns>
        public static bool IsABucket(this Item item)
        {
            Contract.Requires(item.IsNotNull());

            var field = item.Fields[Constants.IsBucket];
            return field.IsNotNull() && "1".Equals(field.Value);
        }

        //public static IEnumerable<SitecoreItem> Search(this Item itm, dynamic relatedIds = null, dynamic indexName = null, dynamic text = null, dynamic templates = null, dynamic location = null, dynamic language = null, dynamic id = null, dynamic sortField = null, dynamic itemName = null, dynamic startDate = null, dynamic endDate = null, int numberOfItemsToReturn = 20)
        //{
           
        //    var culture = CultureInfo.CreateSpecificCulture("en-US");
        //    var startDateOut = new DateTime();
        //    var endDateOut = new DateTime();
        //    if (!DateTime.TryParse(startDate, culture, DateTimeStyles.None, out startDateOut))
        //        startDateOut = DateTime.MinValue;
        //    if (!DateTime.TryParse(endDate, culture, DateTimeStyles.None, out endDateOut))
        //        endDateOut = DateTime.MaxValue;
        //    if (indexName == null)
        //    {
        //        indexName = "buckets";
        //    }

        //    Nullable.GetUnderlyingType(itemName.PropertyType);

        //    itemName = itemName.IsNull() ? "" : itemName.Search + "|" + itemName.BooleanOperation;
        //    text = text.IsNull() ? "" : text.Search + "|" + text.BooleanOperation;
        //    relatedIds = relatedIds.IsNull() ? "" : relatedIds.Search + "|" + relatedIds.BooleanOperation;
        //    templates = templates.IsNull() ? "" : templates.Search + "|" + templates.BooleanOperation;
        //    language = language.IsNull() ? "" : language.Search + "|" + language.BooleanOperation;
        //    id = id.IsNull() ? "" : id.Search + "|" + id.BooleanOperation;
        //    sortField = sortField.IsNull() ? "" : sortField.Search + "|" + sortField.BooleanOperation;

        //    using (var searcher = new IndexSearcher(indexName))
        //    {
        //        return searcher.GetItems(new DateRangeSearchParam
        //        {
        //            ItemName = itemName,
        //            FullTextQuery = text,
        //            RelatedIds = relatedIds,
        //            TemplateIds = templates,
        //            Language =  language,
        //            ID = id,
        //            SortByField = sortField,
        //            PageSize = numberOfItemsToReturn,
        //            Ranges = new List<DateRangeSearchParam.DateRangeField> {
        //                                         new DateRangeSearchParam.DateRangeField(SearchFieldIDs.CreatedDate,
        //                                                                                 startDateOut,
        //                                                                                 endDateOut) {InclusiveStart = true, InclusiveEnd = true}}
        //        }).Value;
        //    }
        //}
    }
}