using Sitecore.Sites;

namespace ItemBuckets.Services
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.Web.SessionState;

    using Lucene.Net.Search;

    using Newtonsoft.Json;

    using Sitecore;
    using Sitecore.Collections;
    using Sitecore.Configuration;
    using Sitecore.Data;
    using Sitecore.Data.Fields;
    using Sitecore.Data.Items;
    using Sitecore.ItemBucket.Kernel.ItemExtensions.Axes;
    using Sitecore.ItemBucket.Kernel.Kernel.Search;
    using Sitecore.ItemBucket.Kernel.Kernel.Util;
    using Sitecore.ItemBucket.Kernel.Managers;
    using Sitecore.ItemBucket.Kernel.Search;
    using Sitecore.ItemBucket.Kernel.Util;
    using Sitecore.Resources.Media;
    using Sitecore.Search;
    using Sitecore.Web;

    using Constants = Sitecore.ItemBucket.Kernel.Util.Constants;
    using IndexSearcher = Sitecore.ItemBucket.Kernel.Util.IndexSearcher;

    /// <summary>
    /// Search End Point
    /// </summary>
    public class Search : IHttpHandler, IRequiresSessionState
    {
        #region Private Variables

        private string _pageNumber = "1";

        private int _itemsPerPage;

        private readonly List<SearchStringModel> _searchQuery = new List<SearchStringModel>();

        private bool _runFacet;

        private Stopwatch _stopwatch;

        private bool isJSONP;

        private string callback;

        #endregion

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            context.Response.ContentEncoding = Encoding.UTF8;
            isJSONP = false;
            this._itemsPerPage = 20; // Constants.SettingsItem.Fields[Constants.ItemsPerPageText].Value.IsNumeric() ? int.Parse(Constants.SettingsItem.Fields[Constants.ItemsPerPageText].Value) : 20;
            this.ExtractSearchQuery(context.Request.QueryString);
            this._stopwatch = new Stopwatch();
            this._stopwatch.Start();
            this.StoreUserContextSearches();
            int hitCount;
            var debugMode = SearchHelper.GetDebug(this._searchQuery).IsNotEmpty();
            if (debugMode)
            {
                if (!Config.EnableBucketDebug)
                {
                    Constants.EnableTemporaryBucketDebug = true;
                }
            }

            this._itemsPerPage = SearchHelper.GetPageSize(this._searchQuery).IsNumeric() ? int.Parse(SearchHelper.GetPageSize(this._searchQuery)) : 20;
            var items = Context.Item.FullSearch(this._searchQuery, out hitCount, pageNumber: int.Parse(this._pageNumber), pageSize: this._itemsPerPage, sortField: SearchHelper.GetSort(this._searchQuery), sortDirection: SearchHelper.GetOrderDirection(this._searchQuery));
            var itemsCount = hitCount;
            var pageNumbers = ((itemsCount % this._itemsPerPage) == 0) ? (itemsCount / this._itemsPerPage) : ((itemsCount / this._itemsPerPage) + 1);
            var currentPage = int.Parse(this._pageNumber);
            var startItemIdx = (currentPage - 1) * this._itemsPerPage;
            if (startItemIdx >= itemsCount)
            {
                currentPage = 1;
            }

            if (!this._runFacet)
            {
                if (items.IsNotNull())
                {
                    var refinement = new SafeDictionary<string> { { Constants.IsSearchImage.ToLowerInvariant(), "1" } };
                    int hitsCount;
                    var templateSearch = Context.ContentDatabase.GetItem(ItemIDs.TemplateRoot).Search(refinement, out hitsCount, location: ItemIDs.TemplateRoot.ToString(), numberOfItemsToReturn: 200, pageNumber:1);
                    var showFieldsQuick = templateSearch.Select(i => FieldTypeManager.GetTemplateFieldItem(new Field(i.GetItem().ID, i.GetItem())));
                    foreach (var sitecoreItem in items)
                    {
                        var innerItem = sitecoreItem.GetItem();
                        if (innerItem != null)
                        {
                            sitecoreItem.TemplateName = innerItem.TemplateName;
                            sitecoreItem.Name = innerItem.Name;
                            var parentBucketItemOrParent = innerItem.GetParentBucketItemOrParent();
                            if (parentBucketItemOrParent.IsNotNull())
                            {
                                sitecoreItem.Bucket = parentBucketItemOrParent.Name;
                            }
                            else
                            {
                                sitecoreItem.Bucket = "sitecore";
                            }
                            sitecoreItem.Cre = innerItem.Statistics.Created.ToString();
                            //sitecoreItem.Language = innerItem.Language.CultureInfo.TwoLetterISOLanguageName;
                            sitecoreItem.Path = innerItem.Paths.IsMediaItem
                                                    ? innerItem.Paths.MediaPath
                                                    : innerItem.Paths.FullPath;
                            sitecoreItem.CreBy = innerItem.Statistics.CreatedBy != string.Empty ? innerItem.Statistics.CreatedBy : innerItem.Statistics.UpdatedBy;

                            foreach (var fieldItem in showFieldsQuick)
                            {
                                if (innerItem.Fields[fieldItem.Name].IsNotNull())
                                {
                                    if (fieldItem.Type != "Thumbnail" && fieldItem.Type.ToLowerInvariant() != Constants.AttachmentFieldType && fieldItem.Type != Constants.ImageFieldType || fieldItem.Type == "Multilist" || fieldItem.Type == "Checkbox")
                                    {
                                        if (fieldItem.Type == "Multilist" || fieldItem.Type == "Checkbox")
                                        {
                                            sitecoreItem.Content += "<p><strong>" + fieldItem.Name + ":</strong> " + FieldParser.ParseIdsForFieldFriendlyValue(innerItem, fieldItem.Name) +
                                                                   "</p><br/>";
                                        }
                                        else
                                        {
                                            sitecoreItem.Content += "<p><strong>" + fieldItem.Name + ":</strong> " + innerItem.Fields[fieldItem.Name].Value +
                                                                    "</p><br/>";
                                        }
                                    }

                                    if (fieldItem.Type.ToLowerInvariant() == Constants.AttachmentFieldType || fieldItem.Type == Constants.ImageFieldType || fieldItem.Type == "Multilist" || fieldItem.Type == "Thumbnail")
                                    {
                                        if ((innerItem.Fields[fieldItem.Name] != null) && (((ImageField)innerItem.Fields[fieldItem.Name]).MediaItem != null))
                                        {
                                            sitecoreItem.ImagePath = MediaManager.GetMediaUrl(((ImageField)innerItem.Fields[fieldItem.Name]).MediaItem);
                                        }

                                        if (innerItem.Fields[fieldItem.Name].Type.ToLowerInvariant() == Constants.AttachmentFieldType)
                                        {
                                            sitecoreItem.ImagePath = MediaManager.GetMediaUrl(new MediaItem(innerItem));
                                        }

                                        if ((innerItem.Fields[fieldItem.Name] != null) && (((ThumbnailField)innerItem.Fields[fieldItem.Name]).MediaItem != null))
                                        {
                                            sitecoreItem.ImagePath = MediaManager.GetMediaUrl(((ThumbnailField)innerItem.Fields[fieldItem.Name]).MediaItem);
                                        }
                                        else if (fieldItem.Type == "Multilist")
                                        {
                                            if (sitecoreItem.ImagePath.IsNullOrEmpty())
                                            {
                                                var multiPath = FieldParser.ParseIdsForImage(innerItem, fieldItem.Name);
                                                if (multiPath.IsNotNull())
                                                {
                                                    sitecoreItem.ImagePath = MediaManager.GetMediaUrl(multiPath);
                                                }

                                            }
                                        }
                                    }
                                   
                                }
                            }

                            if (sitecoreItem.Content.IsNullOrEmpty())
                            {
                                sitecoreItem.Content = string.Empty;
                            }

                            if (sitecoreItem.ImagePath.IsNullOrEmpty())
                            {
                                sitecoreItem.ImagePath = "/temp/IconCache/" + sitecoreItem.GetItem().Template.Icon;
                            }
                        }
                    }
                }
            }

            if (Config.SecuredItems == "hide")
            {
                items = items.RemoveWhere(item => item.Name.IsNull());
            }

            if (!this._runFacet)
            {
                context.Response.Write(callback + "(" + JsonConvert.SerializeObject(new FullSearch
                                                                       {
                                                                           PageNumbers = pageNumbers,
                                                                           items = items,
                                                                           tips = GetRandomTips((!Config.EnableTips || Sitecore.Context.User.Profile.GetCustomProperty("Tips Enabled") == "")? 0 : 2),
                                                                           launchType = GetEditorLaunchType(),
                                                                           SearchTime = this.SearchTime,
                                                                           SearchCount = itemsCount.ToString(),
                                                                           CurrentPage = currentPage,
                                                                           Location = Context.ContentDatabase.GetItem(this.LocationFilter).IsNotNull() ? Context.ContentDatabase.GetItem(this.LocationFilter).Name : "Unknown"
                                                                       }) + ")");

                this._stopwatch.Stop();
                if (Config.EnableBucketDebug || Sitecore.ItemBucket.Kernel.Util.Constants.EnableTemporaryBucketDebug)
                {
                    Sitecore.Diagnostics.Log.Info("Search Took : " + this._stopwatch.ElapsedMilliseconds + "ms", this);
                }
            }
            else
            {
                if (SearchHelper.GetID(this._searchQuery).Count() == 0)
                {
                    if (itemsCount > 0)
                    {
                        context.Response.Write(callback + "(" + JsonConvert.SerializeObject(new FullSearch
                                                                               {
                                                                                   PageNumbers = pageNumbers,
                                                                                   facets = this.GetFacets(),
                                                                                   SearchCount = itemsCount.ToString(),
                                                                                   CurrentPage = currentPage
                                                                               }) + ")");
                    }
                    else
                    {
                        context.Response.Write(JsonConvert.SerializeObject(new FullSearch
                        {
                            PageNumbers = pageNumbers,
                            facets = new List<List<FacetReturn>>(),
                            SearchCount = itemsCount.ToString(),
                            CurrentPage = currentPage
                        }));
                    }
                }
            }

            if (debugMode)
            {
                Constants.EnableTemporaryBucketDebug = false;
            }
        }

        private static string GetEditorLaunchType()
        {
            return Constants.SettingsItem.Fields[Constants.OpenSearchResult].Value == "New Tab" ? "contenteditor:launchtab" : "search:launchresult";
        }

        private static List<Tip> GetRandomTips(int count)
        {
            var tagFolder = Context.ContentDatabase.GetItem(Constants.TipsFolder);
            if (tagFolder.IsNotNull())
            {
                return tagFolder.Children.RandomSample(count, false).Select(i =>
                        new Tip
                        {
                            TipName = i.Fields["Tip Title"].Value,
                            TipText = i.Fields["Tip Text"].Value
                        }).ToList();
            }

            return new List<Tip>
            { 
                new Tip
                {
                        TipName = "Could Not Retrieve Tips",
                        TipText = "Could Not Retrieve Tips"
                }
           };
        }

        private void StoreUserContextSearches()
        {
            var searchQuery = this._searchQuery.Where(query => !string.IsNullOrEmpty(query.Value)).Aggregate(string.Empty, (current, query) => current + query.Type + ":" + query.Value + ";");

            if (ClientContext.GetValue("Searches").IsNull())
            {
                ClientContext.SetValue("Searches", string.Empty);
            }

            if (!ClientContext.GetValue("Searches").ToString().Contains("|" + searchQuery + "|"))
            {
                ClientContext.SetValue("Searches", ClientContext.GetValue("Searches") + "|" + searchQuery + "|");
            }
        }

        private void ExtractSearchQuery(NameValueCollection searchQuery)
        {
            var fromField = false;
            if (searchQuery.Keys[0] == "fromBucketListField")
            {
                fromField = true;
            }

            this.ExtractFacetTagFromSearchQuery(searchQuery);
            for (var i = 0; i < searchQuery.Keys.Count; i++)
            {
                if (searchQuery.Keys[i] == "callback")
                {
                    isJSONP = true;
                    callback = searchQuery[searchQuery.Keys[i]];
                    continue;
                }
                if (searchQuery.Keys[i] != "pageNumber")
                {
                    if (searchQuery.Keys[i] == "fromBucketListField" || searchQuery.Keys[i] == "filterText")
                    {
                        this._searchQuery.Add(new SearchStringModel
                        {
                            Type = "text",
                            Value = searchQuery[i]
                        });
                    }
                    else
                    {
                        if (fromField)
                        {
                            this.BuildSearchStringModelFromFieldQuery(searchQuery, i);
                        }
                        else
                        {
                            this.BuildSearchStringModelFromQuery(searchQuery, i);
                        }
                    }
                }
                else if (searchQuery.Keys[i] == "pageSize")
                {
                    this.ExtractPageSizeFromQuery(searchQuery, i);
                }
                else
                {
                    this.ExtractPageNumberFromQuery(searchQuery, i);
                }

                if (!fromField)
                {
                    i++;
                }
            }
        }

        private void ExtractPageSizeFromQuery(NameValueCollection searchQuery, int i)
        {
            var pageSize = searchQuery[searchQuery.Keys[i]];
            if (pageSize == "0" || pageSize == string.Empty)
            {
                this._itemsPerPage = 20;
            }
            else
            {
                this._itemsPerPage = int.Parse(pageSize);
            }
        }

        private void ExtractPageNumberFromQuery(NameValueCollection searchQuery, int i)
        {
            this._pageNumber = searchQuery[searchQuery.Keys[i]];
            if (this._pageNumber == "0")
            {
                this._pageNumber = "1";
            }
        }

        private void BuildSearchStringModelFromFieldQuery(NameValueCollection searchQuery, int i)
        {
            if (searchQuery.Keys[i] == "pageNumber")
            {
                this.ExtractPageNumberFromQuery(searchQuery, i);
            }
            else
            {
                this._searchQuery.Add(new SearchStringModel
                                     {
                                         Type = searchQuery.Keys[i],
                                         Value = searchQuery[searchQuery.Keys[i]]
                                     });
            }
        }

        private void BuildSearchStringModelFromQuery(NameValueCollection searchQuery, int i)
        {
           // if (i % 2 == 0)
           // {
                if (searchQuery[searchQuery.Keys[i]] != "Query")
                {
                    if (searchQuery.Keys[i] != "_")
                    {
                        this._searchQuery.Add(new SearchStringModel
                                             {
                                                 Type = searchQuery[searchQuery.Keys[i]],
                                                 Value = searchQuery[searchQuery.Keys[i + 1]]
                                             });
                    }
                }
            //}
        }

        private void ExtractFacetTagFromSearchQuery(NameValueCollection searchQuery)
        {
            if (searchQuery["type"].IsNotNull())
            {
                this._runFacet = searchQuery["type"] != "Query";
            }
        }

        private List<List<FacetReturn>> GetFacets()
        {
            var ret = new List<List<FacetReturn>>();
            var facets = Context.ContentDatabase.GetItem(Constants.FacetFolder).Children;
            foreach (Item facet in facets)
            {
                if (facet.Fields["Enabled"].Value == "1")
                {
                    var type = Activator.CreateInstance(Type.GetType(facet.Fields["Type"].Value));
                    if ((type as IFacet).IsNotNull())
                    {
                        var locationOverride = GetLocationOverride();
                        using (var context = new SortableIndexSearchContext(SearchManager.GetIndex(BucketManager.GetContextIndex(Context.ContentDatabase.GetItem(locationOverride)))))
                        {

                            var query = SearchHelper.GetBaseQuery(this._searchQuery, locationOverride);
                            var queryBase = IndexSearcher.ContructQuery(query);
                            var searchBitArray = new QueryFilter(queryBase).Bits(context.Searcher.GetIndexReader());
                            var res = ((IFacet)type).Filter(queryBase, this._searchQuery, locationOverride, searchBitArray);
                            ret.Add(res);
                        }
                    }
                }
            }

            return ret;
        }

        private string GetLocationOverride()
        {
            return SearchHelper.GetLocation(this._searchQuery).Any() ? SearchHelper.GetLocation(this._searchQuery) : (SearchHelper.GetSite(this._searchQuery).Any() ? getSiteIdFromName(SearchHelper.GetSite(this._searchQuery)) : this.LocationFilter);
        }

        private string getSiteIdFromName(string name)
        {
            SiteContext siteContext = SiteContextFactory.GetSiteContext(SiteManager.GetSite(name).Name);
            var db = Context.ContentDatabase ?? Context.Database;
            return db.GetItem(siteContext.StartPath).ID.ToString();
        }

        protected string Lang
        {
            get { return WebUtil.GetQueryString("la"); }
        }

        protected string LocationFilter
        {
            get { return string.IsNullOrEmpty(WebUtil.GetQueryString("id")) ? WebUtil.ExtractUrlParm("id", HttpContext.Current.Request.UrlReferrer.AbsoluteUri) : WebUtil.ExtractUrlParm("id", HttpContext.Current.Request.UrlReferrer.AbsoluteUri); }
        }

        protected string Db
        {
            get { return WebUtil.GetQueryString("db"); }
        }

        protected string SearchTime
        {
            get { return this._stopwatch.Elapsed.ToString(); }
        }

        protected Database ContentDatabase
        {
            get { return Factory.GetDatabase(this.Db) ?? Context.ContentDatabase; }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}