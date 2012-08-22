using System;
using System.Collections;
using System.Collections.Generic;
using Lucene.Net.Index;
using Sitecore.Data;
using Sitecore.Data.Items;
using System.Linq;
using Sitecore.Globalization;
using Sitecore.ItemBucket.Kernel.Kernel.Util;
using Sitecore.ItemBucket.Kernel.Managers;
using Sitecore.ItemBucket.Kernel.Util;
using Sitecore.ItemBucket.Kernel.ItemExtensions.Axes;
using Sitecore.Security.Accounts;
using Sitecore.Sites;
using ItemSearch = Sitecore.ItemBucket.Kernel.ItemExtensions;

namespace Sitecore.ItemBucket.Kernel.Kernel.ItemExtensions.Axes
{
    /// <summary>
    /// Entry point for running Linq based Bucket queries
    /// </summary>
    public class BucketQuery : List<String>
    {

    }

    public static class ItemSearchMethods
    {
        /// <summary>
        /// Search for Items based off a Template ID
        /// </summary>
        public static BucketQuery WhereTemplateIs(this BucketQuery query, string templateId)
        {
            query.Add("template:" + templateId);
            return query;
        }

        /// <summary>
        /// Search for Items based off a Template ID
        /// </summary>
        public static BucketQuery WhereTemplateIs(this BucketQuery query, TemplateID templateId)
        {
            query.Add("template:" + templateId.ID);
            return query;
        }

        /// <summary>
        /// Deferred Execution of the Search
        /// </summary>
        public static IEnumerable<SitecoreItem> Run(this BucketQuery query, out int numberOfHits)
        {
            var returnQuery = query.Select(subQuery => new SearchStringModel()
            {
                Type = subQuery.Split(':')[0],
                Value = subQuery.Split(':')[1]
            }).ToList();
            int hitCount = 0;
            numberOfHits = hitCount;
            return BucketManager.Search(Sitecore.Context.Item, out hitCount, returnQuery);
        }

        public static IEnumerable<SitecoreItem> Run(this BucketQuery query, Item startLocationItem, out int numberOfHits)
        {
            var returnQuery = query.Select(subQuery => new SearchStringModel()
            {
                Type = subQuery.Split(':')[0],
                Value = subQuery.Split(':')[1]
            }).ToList();
            int hitCount = 0;
            numberOfHits = hitCount;
            return BucketManager.Search(startLocationItem, out hitCount, returnQuery);
        }

        /// <summary>
        /// Deferred Execution of the Search
        /// </summary>
        public static IEnumerable<SitecoreItem> Skip(this BucketQuery query, int count, out int numberOfHits, int skipToPage)
        {
            var returnQuery = query.Select(subQuery => new SearchStringModel()
            {
                Type = subQuery.Split(':')[0],
                Value = subQuery.Split(':')[1]
            }).ToList();
            int hitCount = 0;
            numberOfHits = hitCount;
            return Sitecore.Context.Item.Search(returnQuery, out hitCount, numberOfItems: count, pageNumber: skipToPage);
        }

        /// <summary>
        /// Deferred Execution of the Search
        /// </summary>
        public static IEnumerable<SitecoreItem> TopResults(this BucketQuery query, int count, out int numberOfHits)
        {
            var returnQuery = query.Select(subQuery => new SearchStringModel()
            {
                Type = subQuery.Split(':')[0],
                Value = subQuery.Split(':')[1]
            }).ToList();
            int hitCount = 0;
            numberOfHits = hitCount;
            return Sitecore.Context.Item.Search(returnQuery, out hitCount,numberOfItems: count, pageNumber:1);
        }

        /// <summary>
        /// Deferred Execution of the Search
        /// </summary>
        public static IEnumerable<SitecoreItem> TakeResults(this BucketQuery query, int count, out int numberOfHits)
        {
            var returnQuery = query.Select(subQuery => new SearchStringModel()
            {
                Type = subQuery.Split(':')[0],
                Value = subQuery.Split(':')[1]
            }).ToList();
            int hitCount = 0;
            numberOfHits = hitCount;
            return Sitecore.Context.Item.Search(returnQuery, out hitCount, numberOfItems: count, pageNumber: 1);
        }
        /// <summary>
        /// Deferred Execution of the Search
        /// </summary>
        public static IEnumerable<SitecoreItem> First(this BucketQuery query)
        {
            var returnQuery = query.Select(subQuery => new SearchStringModel()
            {
                Type = subQuery.Split(':')[0],
                Value = subQuery.Split(':')[1]
            }).ToList();
            int hitCount = 0;
 
            return Sitecore.Context.Item.Search(returnQuery, out hitCount, numberOfItems: 1, pageNumber: 1);
        }

        /// <summary>
        /// Deferred Execution of the Search
        /// </summary>
        public static bool AnyResults(this BucketQuery query)
        {
            var returnQuery = query.Select(subQuery => new SearchStringModel()
            {
                Type = subQuery.Split(':')[0],
                Value = subQuery.Split(':')[1]
            }).ToList();
            int hitCount = 0;
            var blah = new List<String>();
            Sitecore.Context.Item.Search(returnQuery, out hitCount, numberOfItems: 1, pageNumber: 1);
            return hitCount > 0;
        }
        /// <summary>
        /// Given a query, it will return the results for a particular page
        /// </summary>
        /// <returns>Returns a page of results. Default is 20 items per page</returns>
        /// <paramref name="page">Page Number</paramref>
        public static IEnumerable<SitecoreItem> Page(this BucketQuery query, int page, out int numberOfHits)
        {
           
            int hitCount = 0;
            numberOfHits = hitCount;
            return Page(query, page, 20, out hitCount);
        }

        /// <summary>
        /// Given a query, it will return the results for a particular page and allow you to set the number of items per page.
        /// </summary>
        /// <returns>Returns a page of results of an arbitary number</returns>
        /// <paramref name="page">Page Number</paramref>
        public static IEnumerable<SitecoreItem> Page(this BucketQuery query, int page, int numberOfItemsPerPage, out int numberOfHits)
        {
            var returnQuery = query.Select(subQuery => new SearchStringModel()
            {
                Type = subQuery.Split(':')[0],
                Value = subQuery.Split(':')[1]
            }).ToList();
            int hitCount = 0;
            numberOfHits = hitCount;
            return Sitecore.Context.Item.Search(returnQuery, out hitCount, pageNumber: page, numberOfItems: numberOfItemsPerPage);
        }

        /// <summary>
        /// Deferred Execution of the Search
        /// </summary>
        public static IEnumerable<SitecoreItem> Run(this BucketQuery query, Item startLocationItem, int numberOfHits)
        {
            var returnQuery = query.Select(subQuery => new SearchStringModel()
                                                           {
                                                               Type = subQuery.Split(':')[0], Value = subQuery.Split(':')[1]
                                                           }).ToList();
            int hitCount = 0;
            numberOfHits = hitCount;
            return startLocationItem.Search(returnQuery, out hitCount);
        }

        /// <summary>
        /// Search for Items that contain text
        /// </summary>
        public static BucketQuery WhereContentContains(this BucketQuery query, string searchString)
        {
            query.Add("text:" + searchString);
            return query;
        }

        /// <summary>
        /// Search for Items are tagged by a particular Tag ID
        /// </summary>
        public static BucketQuery WhereTaggedWith(this BucketQuery query, ID tagId)
        {
            query.Add("tag:" + tagId);
            return query;
        }

        /// <summary>
        /// Search for Items are tagged by a particular Tag ID
        /// </summary>
        public static BucketQuery BoostContent(this BucketQuery query, string searchString, int boostValue)
        {
            query.Add("text:" + searchString + "^" + boostValue.ToString());
            return query;
        }

        /// <summary>
        /// Search for Items are tagged by a particular list of Tag IDs
        /// </summary>
        public static BucketQuery WhereTaggedWith(this BucketQuery query, IEnumerable<ID> tagIds)
        {
            query.AddRange(tagIds.Select(tag => "tag:" + tag));
            return query;
        }

        /// <summary>
        /// Search for Items are by a particular Author
        /// </summary>
        public static BucketQuery WhereAuthorIs(this BucketQuery query, string author)
        {
            query.Add("author:" + author);
            return query;
        }

        /// <summary>
        /// Search for Items are by a particular Author
        /// </summary>
        public static BucketQuery WhereAuthorIs(this BucketQuery query, User author)
        {
            query.Add("author:" + author.Name);
            return query;
        }

        /// <summary>
        /// Search for Items have a specific value for a field
        /// </summary>
        public static BucketQuery WhereFieldValueIs(this BucketQuery query, string fieldName, string fieldValue)
        {
            query.Add("custom:" + fieldName + "|" + fieldValue);
            return query;
        }

        /// <summary>
        /// Search for Items that are by a particular list of Authors
        /// </summary>
        public static BucketQuery WhereAuthorIs(this BucketQuery query, IEnumerable<User> authors)
        {
            query.AddRange(authors.Select(author => "author:" + author.Name));
            return query;
        }

        /// <summary> 
        /// Search for Items that contain a list of string values
        /// </summary>
        public static BucketQuery WhereContentContains(this BucketQuery query, IEnumerable<string> searchStrings)
        {
            query.AddRange(searchStrings.Select(@string => "text:" + @string));
            return query;
        }

        /// <summary> 
        /// Search for Items that have a particular Item Name
        /// </summary>
        public static BucketQuery WhereItemNameIs(this BucketQuery query, string itemName)
        {
            query.Add("itemName:" + itemName);
            return query;
        }

        /// <summary> 
        /// Search for Items that belong to a list of Item Names
        /// </summary>
        public static BucketQuery WhereItemNameIs(this BucketQuery query, IEnumerable<string> searchStrings)
        {
            query.AddRange(searchStrings.Select(@string => "itemName:" + @string));
            return query;
        }

        /// <summary> 
        /// Search for Items within a particular location
        /// </summary>
        public static BucketQuery WhereLocationIs(this BucketQuery query, string locationId)
        {
            query.Add("location:" + locationId);
            return query;
        }

        /// <summary> 
        /// Search for Items within a particular location
        /// </summary>
        public static BucketQuery WhereLocationIs(this BucketQuery query, ID locationId)
        {
            query.Add("location:" + locationId);
            return query;
        }

        /// <summary> 
        /// Search for Items within a particular language
        /// </summary>
        public static BucketQuery WhereLanguageIs(this BucketQuery query, string language)
        {
            query.Add("language:" + language);
            return query;
        }

        /// <summary> 
        /// Search for Items within a particular language
        /// </summary>
        public static BucketQuery WhereLanguageIs(this BucketQuery query, Language language)
        {
            query.Add("language:" + language.CultureInfo.TwoLetterISOLanguageName);
            return query;
        }

        /// <summary> 
        /// Search for Items within a particular language
        /// </summary>
        public static BucketQuery WhereLanguageIs(this BucketQuery query, IEnumerable<Language> language)
        {
            query.AddRange(language.Select(lang => "language:" + lang.CultureInfo.TwoLetterISOLanguageName));
            return query;
        }

        /// <summary> 
        /// Search for Items up to a certain Creation Date
        /// </summary>
        public static BucketQuery Ending(this BucketQuery query, DateTime endDate)
        {
            query.Add("end:" + endDate.ToString("MM/dd/yyyy"));
            return query;
        }

        /// <summary> 
        /// Search for Items beginning from a certain Creation Date
        /// </summary>
        public static BucketQuery Starting(this BucketQuery query, DateTime startDate)
        {
            query.Add("end:" + startDate.ToString("MM/dd/yyyy"));
            return query;
        }

        /// <summary> 
        /// Search for Items within a particular Site
        /// </summary>
        public static BucketQuery ForSite(this BucketQuery query, Site site)
        {
            query.Add("site:" + site.Name);
            return query;
        }

        /// <summary> 
        /// Sort your results by a Particular Field. Do not sort by fields that DO NOT exist
        /// </summary>
        public static BucketQuery SortBy(this BucketQuery query, string fieldName)
        {
            query.Add("sort:" + fieldName);
            return query;
        }

        /// <summary> 
        /// Set the operation between two query types
        /// </summary>
        public static BucketQuery And(this BucketQuery query)
        {
            query.Add("+");
            return query;
        }

        /// <summary> 
        /// Negate the next Query
        /// </summary>
        public static BucketQuery Not(this BucketQuery query)
        {
            query.Add("-");
            return query;
        }

        public static BucketQuery Or(this BucketQuery query)
        {
            return query;
        }

        /// <summary> 
        /// Sort by a particular field name and Run the query
        /// </summary>
        public static IEnumerable<SitecoreItem> SortBy(this BucketQuery query, string fieldName, SortDirection sortDirection, out int numberOfHits, int page, int numberOfItemsPerPage)
        {
            query.Add("sort:" + fieldName);
            var returnQuery = query.Select(subQuery => new SearchStringModel()
            {
                Type = subQuery.Split(':')[0],
                Value = subQuery.Split(':')[1]
            }).ToList();
            int hitCount = 0;
            numberOfHits = hitCount;
            return Sitecore.Context.Item.Search(returnQuery, out hitCount, pageNumber: page, numberOfItems: numberOfItemsPerPage, sortDirection: sortDirection == SortDirection.Ascending ? "asc" : "desc");
        }
    }
}
