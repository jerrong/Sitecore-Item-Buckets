using Sitecore.ItemBucket.Kernel.Kernel.Search.SOLR;
using Sitecore.ItemBucket.Kernel.Kernel.Search.SOLR.SOLRItems;
using Sitecore.SecurityModel;
using SolrNet;

namespace Sitecore.ItemBucket.Kernel.Util
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using Sitecore.Collections;
    using Sitecore.Data;
    using Sitecore.Data.Fields;
    using Sitecore.Data.Items;
    using Sitecore.Diagnostics;
    using Sitecore.ItemBucket.Kernel.ItemExtensions.Axes;
    using Sitecore.ItemBucket.Kernel.Kernel.Util;
    using Sitecore.ItemBucket.Kernel.Search;
    using Sitecore.Search;
    using Sitecore.Sites;

    using Field = Lucene.Net.Documents.Field;

    public class SearchHelper
    {
        public static void SetTemplateAsBucketable(IList<Item> items)
        {
            ((CheckboxField)items[0].Fields[Constants.BucketableField]).Checked = !((CheckboxField)items[0].Fields[Constants.BucketableField]).Checked;
        }

        public static void AddSearchTab(Item contextItem, MultilistField editors)
        {
            using (new EditContext(contextItem, SecurityCheck.Disable))
            {
                if (!editors.Items.Contains(Constants.SearchEditor))
                {
                    editors.Add(Constants.SearchEditor);
                }
            }
        }

        private static string ParseSearch(IEnumerable<SearchStringModel> searchParams, string filter)
        {
            return searchParams.Where(i => i.Type == filter).Any() ? searchParams.Where(i => i.Type == filter).First().Value : string.Empty;
        }

        public static string GetLocation(List<SearchStringModel> searchParams)
        {
            return ParseSearch(searchParams, "location");
        }

        public static string GetDebug(List<SearchStringModel> searchParams)
        {
            return ParseSearch(searchParams, "debug");
        }

        public static string GetEndDate(List<SearchStringModel> searchParams)
        {
            return ParseSearch(searchParams, "end");
        }

        public static string GetRecent(List<SearchStringModel> searchParams)
        {
            return ParseSearch(searchParams, "recent");
        }
        public static string GetSort(List<SearchStringModel> searchParams)
        {
            return ParseSearch(searchParams, "sort");
        }

        public static string GetOrderDirection(List<SearchStringModel> searchParams)
        {
            return ParseSearch(searchParams, "orderby");
        }

        public static string GetID(List<SearchStringModel> searchParams)
        {
            return ParseSearch(searchParams, "id");
        }

        public static string GetCustom(List<SearchStringModel> searchParams)
        {
            return ParseSearch(searchParams, "custom");
        }

        public static string GetPageSize(List<SearchStringModel> searchParams)
        {
            return ParseSearch(searchParams, "pageSize");
        }

        public static string GetStartDate(List<SearchStringModel> searchParams)
        {
            return ParseSearch(searchParams, "start");
        }

        public static string GetAuthor(List<SearchStringModel> searchParams)
        {
            return ParseSearch(searchParams, "author");
        }

        public static string GetField(List<SearchStringModel> searchParams)
        {
            return ParseSearch(searchParams, "field");
        }

        public static string GetFileTypes(List<SearchStringModel> searchParams)
        {
            return ParseSearch(searchParams, "filetype");
        }

        public static string GetSite(List<SearchStringModel> searchParams)
        {
            return ParseSearch(searchParams, "site");
        }

        public static string GetLocation(List<SearchStringModel> searchParams, string @default)
        {
            return searchParams.Where(i => i.Type == "location").Any() ? searchParams.Where(i => i.Type == "location").Single().Value : @default;
        }

        public static List<SearchStringModel> GetTags(List<SearchStringModel> searchParams)
        {
            return searchParams.Where(i => i.Type == "tag").Any() ? searchParams.Where(i => i.Type == "tag").ToList() : new List<SearchStringModel>();
        }

        public static string GetText(List<SearchStringModel> searchParams)
        {
            return searchParams.Where(i => i.Type == "text").Aggregate(string.Empty, (current, temp) => current + temp.Value + " ").Trim();
        }

        public static string GetItemName(List<SearchStringModel> searchParams)
        {
            return searchParams.Where(i => i.Type == "itemName").Aggregate(string.Empty, (current, temp) => current + temp.Value + " ").Trim();
        }

        public static string GetVersion(List<SearchStringModel> searchParams)
        {
            return searchParams.Where(i => i.Type == "version").Aggregate(string.Empty, (current, temp) => current + temp.Value + " ").Trim();
        }

        public static string GetLanguages(List<SearchStringModel> searchParams)
        {
            return searchParams.Where(i => i.Type == "language").Select(output => output.Value).ToDelimitedList();
        }

        public static string GetReferences(List<SearchStringModel> searchParams)
        {
            return searchParams.Where(i => i.Type == "ref").Select(output => output.Value).ToDelimitedList();
        }

        public static DateRangeSearchParam GetBaseQuery(List<SearchStringModel> _searchQuery, string locationFilter)
        {
            var returnResult = GetSearchSettings(_searchQuery,locationFilter);
            returnResult.IsFacet = true;

            return returnResult;
        }

        public static string GetTemplates(List<SearchStringModel> searchParams)
        {
            var templates = searchParams.Where(i => i.Type == "template");
            var newGuid = new Guid();
            var returnString = string.Empty;
            if (templates.Any())
            {
                foreach (var template in templates)
                {
                    if (template.Value.Contains('|'))
                    {
                        foreach (var templateName in template.Value.Split('|'))
                        {
                            returnString = returnString + templateName + "|";
                        }
                    }
                    else
                    {
                        var isGuid = Guid.TryParse(template.Value, out newGuid);
                        if (template.Value.IsEmpty())
                        {
                            return string.Empty;
                        }

                        if (!isGuid)
                        {
                            if (Context.ContentDatabase != null)
                            {
                                int hitsCount;
                                var templateSearch = Context.ContentDatabase.GetItem(ItemIDs.TemplateRoot).Search(new SafeDictionary<string> { { "_name", template.Value }, { "bucketable", "1" } }, out hitsCount, location: ItemIDs.TemplateRoot.ToString());
                                if (templateSearch.Any())
                                {
                                    returnString = returnString + templateSearch.First().ItemId + "|";
                                }
                            }
                        }
                        else
                        {
                            if (templates.Any())
                            {
                                returnString = returnString + (Context.ContentDatabase ?? Context.Database).GetItem(template.Value).ID + "|";
                            }
                        }
                    }
                }

                return returnString.TrimEnd('|');
            }

            return string.Empty;
        }

        public static void GetItemsFromSearchResult(IEnumerable<SearchResult> searchResults, List<SitecoreItem> items)
        {
            foreach (var result in searchResults)
            {
                var uriField = result.Document.GetField(BuiltinFields.Url);
                if (uriField.IsNotNull() && !uriField.StringValue().IsNullOrEmpty())
                {
                    AssignFieldValues(result, uriField, items);
                }
            }
        }

        public static void GetItemsFromSearchResultFromSOLR(SolrQueryResults<SOLRItem> searchResults, List<SitecoreItem> items)
        {
            foreach (var result in searchResults)
            {
                var uriField = result.Url;
                if (uriField.IsNotNull() && !uriField.IsNullOrEmpty())
                {
                    AssignFieldValues(result, uriField, items);
                }
            }
        }


        public static void GetItemsFromSearchResultFromSOLR(SolrQueryResults<SolrBucketItem> searchResults, List<SitecoreItem> items)
        {
            foreach (var result in searchResults)
            {
                var uriField = result.Url;
                if (uriField.IsNotNull() && !uriField.IsNullOrEmpty())
                {
                    AssignFieldValues(result, uriField, items);
                }
            }
        }

        private static void AssignFieldValues(SolrBucketItem result, string uriField, List<SitecoreItem> items)
        {

            var itemInfo = new SitecoreItem(new ItemUri(uriField));
            try
            {
                foreach (
                    Sitecore.Data.Fields.Field field in
                        Sitecore.Context.ContentDatabase.GetItem(new ItemUri(result.Url).ItemID).Fields)
                {
                    itemInfo.Fields[field.Name] = field.Value;
                }

                items.Add(itemInfo);
            }
            catch (Exception exc)
            {

            }
        }

        public static void GetItemsFromSearchResultFromSOLR(SolrQueryResults<SolrTemplateItem> searchResults, List<SitecoreItem> items)
        {
            foreach (var result in searchResults)
            {
                var uriField = result.Url;
                if (uriField.IsNotNull() && !uriField.IsNullOrEmpty())
                {
                    AssignFieldValues(result, uriField, items);
                }
            }
        }

        private static void AssignFieldValues(SolrTemplateItem result, string uriField, List<SitecoreItem> items)
        {

            var itemInfo = new SitecoreItem(new ItemUri(uriField));
            try
            {
                foreach (
                    Sitecore.Data.Fields.Field field in
                        Sitecore.Context.ContentDatabase.GetItem(new ItemUri(result.Url).ItemID).Fields)
                {
                    itemInfo.Fields[field.Name] = field.Value;
                }

                items.Add(itemInfo);
            }
            catch (Exception exc)
            {

            }
        }

        public static void GetItemsFromSearchResultFromSOLR(SolrQueryResults<SolrSitecoreItem> searchResults, List<SitecoreItem> items)
        {
            foreach (var result in searchResults)
            {
                var uriField = result.Url;
                if (uriField.IsNotNull() && !uriField.IsNullOrEmpty())
                {
                    AssignFieldValues(result, uriField, items);
                }
            }
        }

        private static void AssignFieldValues(SolrSitecoreItem result, string uriField, List<SitecoreItem> items)
        {

            var itemInfo = new SitecoreItem(new ItemUri(uriField));
            try
            {
                foreach (
                    Sitecore.Data.Fields.Field field in
                        Sitecore.Context.ContentDatabase.GetItem(new ItemUri(result.Url).ItemID).Fields)
                {
                    itemInfo.Fields[field.Name] = field.Value;
                }

                items.Add(itemInfo);
            }
            catch (Exception exc)
            {

            }
        }

        public static void GetItemsFromSearchResultFromSOLR(SolrQueryResults<SolrMediaItem> searchResults, List<SitecoreItem> items)
        {
            foreach (var result in searchResults)
            {
                var uriField = result.Url;
                if (uriField.IsNotNull() && !uriField.IsNullOrEmpty())
                {
                    AssignFieldValues(result, uriField, items);
                }
            }
        }

        private static void AssignFieldValues(SolrMediaItem result, string uriField, List<SitecoreItem> items)
        {

            var itemInfo = new SitecoreItem(new ItemUri(uriField));
            try
            {
                foreach (
                    Sitecore.Data.Fields.Field field in
                        Sitecore.Context.ContentDatabase.GetItem(new ItemUri(result.Url).ItemID).Fields)
                {
                    itemInfo.Fields[field.Name] = field.Value;
                }

                items.Add(itemInfo);
            }
            catch (Exception exc)
            {

            }
        }

        public static void GetItemsFromSearchResultFromSOLR(SolrQueryResults<SolrLayoutItem> searchResults, List<SitecoreItem> items)
        {
            foreach (var result in searchResults)
            {
                var uriField = result.Url;
                if (uriField.IsNotNull() && !uriField.IsNullOrEmpty())
                {
                    AssignFieldValues(result, uriField, items);
                }
            }
        }

        private static void AssignFieldValues(SolrLayoutItem result, string uriField, List<SitecoreItem> items)
        {

            var itemInfo = new SitecoreItem(new ItemUri(uriField));
            try
            {
                foreach (
                    Sitecore.Data.Fields.Field field in
                        Sitecore.Context.ContentDatabase.GetItem(new ItemUri(result.Url).ItemID).Fields)
                {
                    itemInfo.Fields[field.Name] = field.Value;
                }

                items.Add(itemInfo);
            }
            catch (Exception exc)
            {

            }
        }

        public static void GetItemsFromSearchResultFromSOLR(SolrQueryResults<SolrSystemItem> searchResults, List<SitecoreItem> items)
        {
            foreach (var result in searchResults)
            {
                var uriField = result.Url;
                if (uriField.IsNotNull() && !uriField.IsNullOrEmpty())
                {
                    AssignFieldValues(result, uriField, items);
                }
            }
        }

        private static void AssignFieldValues(SolrSystemItem result, string uriField, List<SitecoreItem> items)
        {

            var itemInfo = new SitecoreItem(new ItemUri(uriField));
            try
            {
                foreach (
                    Sitecore.Data.Fields.Field field in
                        Sitecore.Context.ContentDatabase.GetItem(new ItemUri(result.Url).ItemID).Fields)
                {
                    itemInfo.Fields[field.Name] = field.Value;
                }

                items.Add(itemInfo);
            }
            catch (Exception exc)
            {

            }
        }

        private static void AssignFieldValues(SOLRItem result, string uriField, List<SitecoreItem> items)
        {
        
            var itemInfo = new SitecoreItem(new ItemUri(uriField));
            try
            {
                foreach (
                    Sitecore.Data.Fields.Field field in
                        Sitecore.Context.ContentDatabase.GetItem(new ItemUri(result.Url).ItemID).Fields)
                {
                    itemInfo.Fields[field.Name] = field.Value;
                }

                items.Add(itemInfo);
            }
            catch(Exception exc)
            {
                
            }
        }

        private static void AssignFieldValues(SearchResult result, Field uriField, List<SitecoreItem> items)
        {
            var itemInfo = new SitecoreItem(new ItemUri(uriField.StringValue()));
            foreach (Field field in result.Document.GetFields())
            {
                itemInfo.Fields[field.Name()] = field.StringValue();
            }

            items.Add(itemInfo);
        }

        private static readonly byte[] BitsSetArray256 = new byte[] { 0, 1, 1, 2, 1, 2, 2, 3, 1, 2, 2, 3, 2, 3, 3, 4, 1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5, 1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5, 2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5, 2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7, 1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5, 2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7, 2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7, 3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7, 4, 5, 5, 6, 5, 6, 6, 7, 5, 6, 6, 7, 6, 7, 7, 8 };

        public static int GetCardinality(BitArray bitArray)
        {
            var array = (uint[])bitArray.GetType().GetField("m_array", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(bitArray);
            return array.Sum(t => BitsSetArray256[t & 0xFF] + BitsSetArray256[(t >> 8) & 0xFF] + BitsSetArray256[(t >> 16) & 0xFF] + BitsSetArray256[(t >> 24) & 0xFF]);
        }

        public static SafeDictionary<string> GetTagRefinements(List<SearchStringModel> currentSearchString)
        {
            var refinements = new SafeDictionary<string>();
            var tagsList = new List<string>();
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
                    string result = "";
                    var db = Context.ContentDatabase ?? Context.Database;
                    if (ID.IsID(query))
                    {
                        Item tagItem = db.GetItem(query);
                        if (tagItem != null)
                        {
                            result = db.GetItem(query).ID.ToShortID().ToString();
                        }
                    }
                    else
                    {
                        Item tagParentItem = ((ReferenceField)ItemBucket.Kernel.Util.Constants.SettingsItem.Fields["Tag Parent"]).TargetItem;
                        if (tagParentItem != null)
                        {
                            Item tagItem = tagParentItem.Axes.GetChild(query);
                            if (tagItem != null)
                            {
                                result = tagItem.ID.ToShortID().ToString();
                            }
                        }
                    }
                    if (!String.IsNullOrEmpty(result))
                    {
                        tagsList.Add(result);
                    }
                }
                if (tagsList.Count > 0)
                {
                    string refinementValue = "(" + tagsList[0];
                    tagsList.RemoveAt(0);
                    foreach (string part in tagsList)
                    {
                        refinementValue += " AND " + part;
                    }
                    refinementValue += ")";
                    string tagFieldName = ItemBucket.Kernel.Util.Constants.SettingsItem.Fields["Tag FieldName"].Value;
                    refinements.Add(tagFieldName, refinementValue);
                }
            }
            return refinements;
        }
        public static List<DateRangeSearchParam.DateRangeField> GetDateRefinements(string startDate, string endDate)
        {
            var culture = CultureInfo.CreateSpecificCulture("en-US");
            var startDateOut = DateTime.MinValue;
            var endDateOut = DateTime.Now.AddDays(1);  
            var dateIsSet = false;
            if (startDate != null && startDate.Any())
            {
                dateIsSet = true;
                if (!DateTime.TryParse(startDate, culture, DateTimeStyles.None, out startDateOut))
                {
                    startDateOut = DateTime.Now;                    
                }
            }
            if (endDate != null && endDate.Any())
            {
                dateIsSet = true;
                if (!DateTime.TryParse(endDate, culture, DateTimeStyles.None, out endDateOut))
                {
                    endDateOut = DateTime.Now.AddDays(1);                    
                }                
            }
            if (dateIsSet)
            {
                return new List<DateRangeSearchParam.DateRangeField>
                                                 {
                                                     new DateRangeSearchParam.DateRangeField(SearchFieldIDs.CreatedDate, startDateOut, endDateOut)
                                                         {
                                                             InclusiveStart = true, InclusiveEnd = true
                                                         }
                                                 };
            }
            return null;
        }
        public static DateRangeSearchParam GetSearchSettings(List<SearchStringModel> currentSearchString, string locationFilter)
        {            
            var locationSearch = locationFilter;
            var refinements = new SafeDictionary<string>();
            refinements = GetTagRefinements(currentSearchString);

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
           
            var location = SearchHelper.GetLocation(currentSearchString, locationSearch);
            
            var rangeSearch = new DateRangeSearchParam
            {
                ID = SearchHelper.GetID(currentSearchString).IsEmpty() ? SearchHelper.GetRecent(currentSearchString) : SearchHelper.GetID(currentSearchString),
                ShowAllVersions = false,
                FullTextQuery = SearchHelper.GetText(currentSearchString),
                Refinements = refinements,
                RelatedIds = references.Any() ? references : string.Empty,
                TemplateIds = SearchHelper.GetTemplates(currentSearchString),
                LocationIds = location,
                Language = languages,
                Author = author == string.Empty ? string.Empty : author,
                ItemName = SearchHelper.GetItemName(currentSearchString),
                Ranges = GetDateRefinements(SearchHelper.GetStartDate(currentSearchString),SearchHelper.GetEndDate(currentSearchString))
            };
           
            return rangeSearch;
        }
    }
}
