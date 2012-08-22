using System.IO;
using Sitecore.ItemBuckets.BigData.RemoteIndex;
using Sitecore.SecurityModel;

namespace ItemBuckets
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Script.Services;
    using System.Web.Services;

    using Lucene.Net.Index;
    using Lucene.Net.Search;

    using Sitecore.Collections;
    using Sitecore.Configuration;
    using Sitecore.Data;
    using Sitecore.Data.Fields;
    using Sitecore.Data.Items;
    using Sitecore.Data.Managers;
    using Sitecore.ItemBucket.Kernel.Common.Providers;
    using Sitecore.ItemBucket.Kernel.ItemExtensions.Axes;
    using Sitecore.ItemBucket.Kernel.Kernel.Search;
    using Sitecore.ItemBucket.Kernel.Kernel.Util;
    using Sitecore.ItemBucket.Kernel.Search;
    using Sitecore.ItemBucket.Kernel.Util;
    using Sitecore.Search;
    using Sitecore.Web;

    using IndexSearcher = Sitecore.ItemBucket.Kernel.Util.IndexSearcher;

    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    [ScriptService]
    public class ItemBucketService : WebService
    {
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public List<string> GetAuthors(string tagChars)
        {
           if (tagChars.Length >= 2)
           {
               var authors =
                   GetAuthorsFromIndex().Where(
                       user =>
                       user.StartsWith(tagChars.ToLower()) ||
                       user.Substring(user.IndexOf("\\", StringComparison.Ordinal)).TrimStart('\\').StartsWith(
                           tagChars.ToLower()) || user.IndexOf(tagChars, System.StringComparison.Ordinal) > 0).ToList();
               return authors;
           }
           else
           {
                return new List<string>();
           }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public List<string> GetNames(string tagChars)
        {
           if (tagChars.Length >= 3)
           {
               var currentItem = Sitecore.Context.ContentDatabase.GetItem(WebUtil.ExtractUrlParm("id", HttpContext.Current.Request.UrlReferrer.AbsoluteUri));
               int hitCount;
               return currentItem.Search(out hitCount, text: tagChars + "*").Select(item => item.GetItem().Name).Distinct().ToList();
           }
           else
           {
                return new List<string>();
           }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string QueryServer()
        {
            return Sitecore.ItemBucket.Kernel.Util.Config.QueryServerAddress;
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetMediaPath(string id)
        {
            return Sitecore.Context.ContentDatabase.GetItem(id).Paths.MediaPath;
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetMediaPathSource(string id)
        {
            return Sitecore.Context.ContentDatabase.GetItem(id).Paths.FullPath;
        }


        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public View[] GetViews()
        {
            var views = new List<View>();
            //var parentPath = Sitecore.Context.ContentDatabase.GetItem("{3B750F26-520E-4B33-852A-9633C54706BE}").Children.Where(item => ((CheckboxField)item.Fields["Enabled"]).Checked);
            var parentPath = ((MultilistField)Sitecore.Context.ContentDatabase.GetItem(WebUtil.ExtractUrlParm("id", HttpContext.Current.Request.UrlReferrer.AbsoluteUri)).Fields["Enabled Views"]).GetItems();
            foreach (Item item in parentPath)
            {
                views.Add(new View
                {
                    ViewName = item.Fields["Name"].Value,
                    FooterTemplate = item.Fields["Footer Template"].Value,
                    HeaderTemplate = item.Fields["Header Template"].Value,
                    ItemTemplate = item.Fields["Item Template"].Value,
                    IsDefault = ((CheckboxField)item.Fields["Default"]).Checked,
                    Icon = item.Fields["Icon"].Value,
                });
            }

            return views.ToArray();
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public View GetView(string viewName)
        {
            var viewItem = Sitecore.Context.ContentDatabase.GetItem("{3B750F26-520E-4B33-852A-9633C54706BE}")
                   .Children.Where(view => view.Name == viewName)
                   .Select(view => new View
                               {
                                   ViewName = view.Fields["Name"].Value,
                                   FooterTemplate = view.Fields["Footer Template"].Value,
                                   HeaderTemplate = view.Fields["Header Template"].Value,
                                   ItemTemplate = view.Fields["Item Template"].Value,
                                   IsDefault = ((CheckboxField)view.Fields["Default"]).Checked,
                                   Icon = view.Fields["Icon"].Value
                               });
            if (viewItem.Any())
            {
                return viewItem.First();
            }
            return null;
        }

         [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetDefault()
        {
            var viewItem =  Sitecore.Context.ContentDatabase.GetItem(WebUtil.ExtractUrlParm("id", HttpContext.Current.Request.UrlReferrer.AbsoluteUri));
             return viewItem.Fields["Default View"].Value;
        }
         
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public List<string> GetFileType(string tagChars)
        {
            return tagChars.Length >= 1
                       ? GetExtensionsFromIndex().Where(user => user.StartsWith(tagChars.ToLower())).ToList()
                       : new List<string>();
        }

        [WebMethod]
        public List<string> GetBuckets()
        {
            int hitsCount = default(int);
            return Sitecore.Context.ContentDatabase.GetItem(Sitecore.ItemIDs.RootID).Search(new SafeDictionary<string> { { "isbucket", "1" } }, out hitsCount, numberOfItemsToReturn: 200, location: Sitecore.ItemIDs.RootID.ToString()).Select(t => t.GetItem().Name + "|" + t.GetItem().ID.ToString()).ToList();
        }


        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public List<string> GetFields(string tagChars)
        {
            return tagChars.Length >= 1
                       ? GetAllIndexFields().Where(user => user.StartsWith(tagChars.ToLower())).ToList()
                       : new List<string>();
        }


        private static IEnumerable<string> GetExtensionsFromIndex()
        {
            var terms = new List<string>();
            using (var context = new SortableIndexSearchContext(SearchManager.GetIndex("itembuckets_medialibrary")))
            {
                var termsByField = context.Searcher.GetIndexReader().Terms(new Term("extension", string.Empty));
                while (termsByField.Next())
                {
                    if (termsByField.Term().Field() == "extension")
                    {
                        terms.Add(termsByField.Term().Text());
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return terms;
        }


        private static IEnumerable<string> GetAllIndexFields()
        {
            var terms = new List<string>();
            using (var context = new SortableIndexSearchContext(SearchManager.GetIndex(Constants.Index.Name)))
            {
                var fieldsInIndex = context.Searcher.GetIndexReader().GetFieldNames(IndexReader.FieldOption.ALL).OfType<string>();
                terms.AddRange(fieldsInIndex);
            }
            return terms;
        }

        private static IEnumerable<string> GetAuthorsFromIndex()
        {
            var terms = new List<string>();
            using (var context = new SortableIndexSearchContext(SearchManager.GetIndex(Constants.Index.Name)))
            {
                var termsByField = context.Searcher.GetIndexReader().Terms(new Term("__created by", string.Empty));
                while (termsByField.Next())
                {
                    if (termsByField.Term().Field() == "__created by")
                    {
                        terms.Add(termsByField.Term().Text());
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return terms;
        }

        [WebMethod]
        public List<string> GetTips(int number)
        {
            return Sitecore.Context.ContentDatabase.GetItem(Constants.TipsFolder).Children.RandomSample(number, false).Select(u => u.Fields["Tip Text"].Value).ToList();
        }

        [WebMethod]
        public List<string> GetFields()
        {
            int hitsCount = default(int);
            return Sitecore.Context.ContentDatabase.GetItem(Sitecore.ItemIDs.TemplateRoot).Search(new SafeDictionary<string> { { Constants.IsTag.ToLower(), "1" } }, out hitsCount, numberOfItemsToReturn: 200, location: Sitecore.ItemIDs.TemplateRoot.ToString()).Select(t => t.GetItem().Name.ToLowerInvariant()).ToList();
        }

        [WebMethod]
        public string GetFieldValues(string FieldName)
        {
            if (HttpContext.Current.Request.UrlReferrer != null)
            {
                var fieldItem = Sitecore.Context.ContentDatabase.GetItem(WebUtil.ExtractUrlParm("id", HttpContext.Current.Request.UrlReferrer.AbsoluteUri));
                var parsedIDs = fieldItem.Fields[FieldName].Value.Split('|');
                var stringReturn = parsedIDs.Where(itmId => itmId.IsNotEmpty()).Aggregate(string.Empty, (current, itmId) => current + Sitecore.Context.ContentDatabase.GetItem(itmId).Name + "|" + itmId + "|");
                stringReturn = stringReturn.TrimEnd('|');
                return stringReturn;
            }

            return string.Empty;
        }

        [WebMethod]
        public string SaveFieldValues(string newId, string FieldName)
        {
            if (HttpContext.Current.Request.UrlReferrer != null)
            {
                var fieldItem = Sitecore.Context.ContentDatabase.GetItem(WebUtil.ExtractUrlParm("id", HttpContext.Current.Request.UrlReferrer.AbsoluteUri));
                using (new EditContext(fieldItem, SecurityCheck.Disable))
                {
                    if (fieldItem.Fields[FieldName].Value.IsEmpty())
                    {
                        fieldItem.Fields[FieldName].Value = fieldItem.Fields[FieldName].Value + newId;
                    }
                    else
                    {
                        fieldItem.Fields[FieldName].Value = fieldItem.Fields[FieldName].Value.TrimEnd('|') + "|" + newId;
                    }
                }

                var parsedIDs = fieldItem.Fields[FieldName].Value.Split('|');
                var stringReturn = parsedIDs.Where(itmId => itmId.IsNotEmpty()).Aggregate(string.Empty, (current, itmId) => current + Sitecore.Context.ContentDatabase.GetItem(itmId).Name + "|" + itmId + "|");
                stringReturn = stringReturn.TrimEnd('|');
                return stringReturn;
            }

            return string.Empty;
        }

        [WebMethod]
        public string RemoveFieldValues(string newId, string FieldName)
        {
            if (HttpContext.Current.Request.UrlReferrer != null)
            {
                var fieldItem = Sitecore.Context.ContentDatabase.GetItem(WebUtil.ExtractUrlParm("id", HttpContext.Current.Request.UrlReferrer.AbsoluteUri));

                using (new EditContext(fieldItem, SecurityCheck.Disable))
                {
                    if (fieldItem.Fields[FieldName].Value.IsNotEmpty())
                    {
                        fieldItem.Fields[FieldName].Value = fieldItem.Fields[FieldName].Value.Replace(newId, string.Empty).TrimEnd('|').TrimStart('|');
                    }
                }

                var parsedIDs = fieldItem.Fields[FieldName].Value.Split('|');
                var stringReturn = parsedIDs.Where(itmId => itmId.IsNotEmpty()).Aggregate(string.Empty, (current, itmId) => current + Sitecore.Context.ContentDatabase.GetItem(itmId).Name + "|" + itmId + "|");
                stringReturn = stringReturn.TrimEnd('|');
                return stringReturn;
            }

            return string.Empty;
        }

        [WebMethod]
        public void SaveClosedTabs(string ids)
        {
            if (ClientContext.GetValue("RecentlyOpenedTabs").IsNull())
            {
                ClientContext.SetValue("RecentlyOpenedTabs", string.Empty);
            }
            string[] parsedIds = IdHelper.ParseId(ids);
            
            foreach (var id in parsedIds)
            {
                //if (!ClientContext.GetValue("RecentlyOpenedTabs").ToString().Contains("|" + id + "|"))
                //{
                   
                //}
            }

            ClientContext.SetValue("RecentlyOpenedTabs",
                                          ClientContext.GetValue("RecentlyOpenedTabs") + "|" + "Closed Tabs (" + ids + ")" +
                                          "|");
        }


        [WebMethod]
        public List<string> GetRecent()
        {
            using (var searcher = new IndexSearcher(Constants.Index.Name))
            {
                var query = new RangeQuery(new Term("__smallUpdatedDate", DateTime.Now.AddDays(-1).ToString("yyyyMMdd")), new Term("__smallUpdatedDate", DateTime.Now.ToString("yyyyMMdd")), true);
                var ret = searcher.RunQuery(query, 10, 0).Value.Select(i => i.GetItem().Name + "|" + i.GetItem().ID.ToString()).ToList();
                ret.Reverse();
                return ret.Take(10).ToList();
            }
        }

        [WebMethod]
        public List<DropDown> RunLookup()
        {
            var facets = Sitecore.Context.ContentDatabase.GetItem(Constants.DropDownMenuFolder).Axes.GetDescendants().Where(t => t.TemplateID == new TemplateID(new ID(Constants.FacetTemplate)));
            return facets.Select(facet => new DropDown(facet.Fields["List Name"].Value, ((ISearchDropDown)Activator.CreateInstance(Type.GetType(facet.Fields["Type"].Value))).Process(), facet.Fields["Display Text"].Value, facet.Fields["__Icon"].Value)).ToList();
        }
        [WebMethod]
        public List<string> GenericCall(string ServiceName)
        {
            var ret = new List<string>();
            var facets = Sitecore.Context.ContentDatabase.GetItem(Constants.DropDownMenuFolder).Axes.GetDescendants().Where(t => t.TemplateID == new TemplateID(new ID(Constants.FacetTemplate))).Where(i => i.Fields["List Name"].Value.Replace(" ", string.Empty) == ServiceName.Replace(" ", string.Empty));
            ret.AddRange(((ISearchDropDown)Activator.CreateInstance(Type.GetType(facets.FirstOrDefault().Fields["Type"].Value))).Process());
            return ret;
        }

        [WebMethod]
        public List<string> GetSearchTypes()
        {
            return Sitecore.Context.ContentDatabase.GetItem(Constants.BucketSearchType).Axes.GetDescendants().Select(i => i.Fields["Name"].Value).ToList();
        }

        [WebMethod]
        public List<string> GetCommon()
        {
            var list = ClientContext.GetValue("Searches").ToString().Split(',').ToList();
            list.Reverse();
            return list.Take(20).ToList();
        }

        [WebMethod]
        public List<string> GetLanguages()
        {
            return LanguageManager.GetLanguages(Sitecore.Context.ContentDatabase).Select(u => u.Name).ToList();
        }

        [WebMethod]
        public List<SitecoreItem> Run(List<SearchStringModel> currentSearchFilter)
        {
            int hitsCount = default(int);
            return Sitecore.Context.Item.Search(currentSearchFilter, out hitsCount).ToList();
        }

        [WebMethod]
        public List<string> GetTemp()
        {
            int hitsCount = default(int);
            return Sitecore.Context.ContentDatabase.GetItem(Sitecore.ItemIDs.TemplateRoot).Search(new SafeDictionary<string> { { "bucketable", "1" } }, out hitsCount, numberOfItemsToReturn: 200, location: Sitecore.ItemIDs.TemplateRoot.ToString()).Select(t => t.GetItem().Name).ToList();
        }

        [WebMethod]
        public List<Tag> GetTag(string tagChars)
        {
            var tags = new List<Tag>();
            var repositories =
                Sitecore.Context.ContentDatabase.GetItem(Constants.TagFolder).Children;
            foreach (Item repository in repositories.Where(item => ((CheckboxField)item.Fields["Enabled"]).Checked))
            {
                var activationContext = Type.GetType(repository.Fields["Type"].Value);
                var objectInstance = (ITagRepository)Activator.CreateInstance(activationContext);
                tags.AddRange(objectInstance.GetTags(tagChars));
            }

            return tags.ToList();
        }

        [WebMethod]
        public List<string> GetSites()
        {
            return Sitecore.Sites.SiteManager.GetSites().Select(s => s.Name).ToList(); // + " -> " + s.Properties["hostName"]
        }

        [WebMethod]
        public List<string> GetTemplates(string currentCharacters)
        {
            int hitsCount = default(int);
            return Sitecore.Context.ContentDatabase.GetItem(Sitecore.ItemIDs.TemplateRoot).Search(
                                                                                           new SafeDictionary<string> { { "bucketable", "1" } },
                                                                                           out hitsCount,
                                                                                           location: Sitecore.ItemIDs.TemplateRoot.ToString(),
                                                                                           numberOfItemsToReturn: 200,
                                                                                           text: currentCharacters).Select(item => item.Name).ToList();
        }
    }

    public class DropDown
    {
        public string Name;
        public List<string> Values;
        public string DisplayText;
        public string Icon;

        public DropDown(string name, List<string> values, string displayText, string icon)
        {
            Name = name;
            Values = values;
            DisplayText = displayText;
            Icon = icon;
        }
    }
}

