using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using System.Web.Services;
using System.Xml;
using ItemBucket.Kernel.Kernel.Search;
using ItemBucket.Kernel.Kernel.Util;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Web;
using Page = System.Web.UI.Page;
using ItemBucket.Kernel.Kernel.ItemExtensions.Axes;

namespace ItemBuckets
{
   

    public partial class Results : Page
    {
        List<SearchStringModel> searchQuery = new List<SearchStringModel>();
        protected void Page_Load(object sender, EventArgs e)
        {

            //XmlDocument myxml = new XmlDocument();
            //StreamReader reader = new StreamReader(Page.Request.InputStream);

            //string test;
            //test = reader.ReadToEnd();

            //JavaScriptSerializer jss = new JavaScriptSerializer();
            //myxml = jss.Deserialize<XmlDocument>(test);
     
            NameValueCollection nvc = Request.Form;
            try
            {
                for (int i = 0; i < nvc.Keys.Count; i++)
                {
                    if (i % 2 == 0)
                    {
                        var search = new SearchStringModel();
                        search.Type = nvc[nvc.Keys[i]];
                        search.Value = nvc[nvc.Keys[i + 1]];
                        searchQuery.Add(search);
                    }
                    i++;
                }
            }
            catch (Exception exc)
            {

            }

            //foreach (var searchTerm in nvc.Keys)
            //{
            //    var search = new SearchStringModel();
            //    search.type = searchTerm.ToString();
            //    search.value = nvc[searchTerm.ToString()];
            //    searchQuery.Add(search);
            //}

            if (searchQuery.Count > 0)
            {
                var items = new List<SitecoreItem>();
                var stopwatch = new Stopwatch();
                try
                {
                    stopwatch.Start();
                    items.AddRange(GetItems());
                    stopwatch.Stop();
                 
                    //SearchResults.DataSource = items;
                    //SearchResults.DataBind();//RenderItemDetails(items);
                }
                catch (Exception)
                {
                    //ResultLabel.Text = "There was an error running search.";
                    throw;
                }
                finally
                {
                    stopwatch.Stop();
                }
            }


        }

        [WebMethod()]
        public string Search()
        {
            NameValueCollection nvc = Request.Form;
            try
            {
                for (int i = 0; i < nvc.Keys.Count; i++)
                {
                    if (i % 2 == 0)
                    {
                        var search = new SearchStringModel();
                        search.Type = nvc[nvc.Keys[i]];
                        search.Value = nvc[nvc.Keys[i + 1]];
                        searchQuery.Add(search);
                    }
                    i++;
                }
            }
            catch (Exception exc)
            {

            }

            //foreach (var searchTerm in nvc.Keys)
            //{
            //    var search = new SearchStringModel();
            //    search.type = searchTerm.ToString();
            //    search.value = nvc[searchTerm.ToString()];
            //    searchQuery.Add(search);
            //}

            if (searchQuery.Count > 0)
            {
                var items = new List<SitecoreItem>();
                var stopwatch = new Stopwatch();
                try
                {
                    stopwatch.Start();
                    items.AddRange(GetItems());
                    stopwatch.Stop();
                    //SearchResults.DataSource = items;
                    //SearchResults.DataBind();//RenderItemDetails(items);
                }
                catch (Exception)
                {
                    //ResultLabel.Text = "There was an error running search.";
                    throw;
                }
                finally
                {
                    stopwatch.Stop();
                }
            }

            return "Search is done";
        }

        protected string IndexName
        {
            get { return Constants.Index.Name; }
        }

        protected string Lang
        {
            get { return WebUtil.GetQueryString("la"); }
        }

        protected string LocationFilter
        {
            get { return WebUtil.GetQueryString("id"); }
        }

        protected string Db
        {
            get { return WebUtil.GetQueryString("db"); }
        }

        protected Database ContentDatabase
        {
            get { return Factory.GetDatabase(Db) ?? Factory.GetDatabase("master"); }
        }

        protected List<SearchStringModel> FullTextQuery
        {
            get { return searchQuery; }
        }



        //protected virtual void PopulateTemplateListFilter()
        //{
        //    var templateContainer = ContentDatabase.GetItem("/sitecore/templates/User Defined");

        //    if (templateContainer == null) return;

        //    var query = "fast:" + templateContainer.Paths.FullPath + "//*[@@templateid='{AB86861A-6030-46C5-B394-E8F99E8B87DB}']";

        //    var templates = ContentDatabase.SelectItems(query);

        //    TemplateList.Items.Add(new ListItem { Selected = true, Text = "All", Value = "" });

        //    foreach (var template in templates)
        //    {
        //        TemplateList.Items.Add(new ListItem { Text = template.DisplayName, Value = template.ID.ToString() });
        //    }
        //}


        //protected virtual void PopulateFieldListFilter(ListBox templateList)
        //{
        //    var templateContainer = ContentDatabase.GetItem("/sitecore/templates/User Defined");

        //    if (templateContainer == null) return;

        //    var query = "fast:" + templateContainer.Paths.FullPath + "//*[@@templateid='{AB86861A-6030-46C5-B394-E8F99E8B87DB}']";

        //    var templates = ContentDatabase.SelectItems(query);
        //    var selectedTemplateItems = new List<Item>();

        //    foreach (ListItem templateListItem in templateList.Items)
        //    {
        //        if (templateListItem.Selected)
        //        {
        //            selectedTemplateItems.Add(Sitecore.Context.Database.GetItem(templateListItem.Value));
        //        }
        //    }

        //    FilteredFieldItems.Items.Add(new ListItem { Selected = true, Text = "All", Value = "" });

        //    foreach (var template in selectedTemplateItems)
        //    {
        //        foreach (Field field in template.Fields)
        //        {
        //            FilteredFieldItems.Items.Add(new ListItem { Text = field.DisplayName, Value = field.ID.ToString() });
        //        }
        //    }
        //}


        //protected virtual void PopulateTemplateFilter()
        //{
        //    var templateContainer = ContentDatabase.GetItem("/sitecore/templates/User Defined");

        //    if (templateContainer == null) return;

        //    var query = "fast:" + templateContainer.Paths.FullPath + "//*[@@templateid='{AB86861A-6030-46C5-B394-E8F99E8B87DB}']";

        //    var templates = ContentDatabase.SelectItems(query);

        //    TemplateList.Items.Add(new ListItem { Selected = true, Text = "All", Value = "" });

        //    foreach (var template in templates)
        //    {
        //        TemplateList.Items.Add(new ListItem { Text = template.DisplayName, Value = template.ID.ToString() });
        //    }
        //}

        protected virtual void RunButton_Click(object sender, EventArgs e)
        {
            var items = new List<SitecoreItem>();
            var stopwatch = new Stopwatch();
            try
            {
                stopwatch.Start();
                items.AddRange(GetItems());
                stopwatch.Stop();
                RenderItemDetails(items);
            }
            catch (Exception)
            {
                //ResultLabel.Text = "There was an error running search.";
                throw;
            }
            finally
            {
                stopwatch.Stop();
            }

            //RenderTimingSummary(items.Count, stopwatch.Elapsed);
        }

        protected virtual void RenderItemDetails(IEnumerable<SitecoreItem> SitecoreItems)
        {
            // ResultLabel.Text = String.Empty;


            // ResultLabel.Text = "<ol>";

            foreach (var SitecoreItem in SitecoreItems)
            {
                // ResultLabel.Text += String.Format(@"<li><a href=""/sitecore/shell/sitecore/content/Applications/Content Editor.aspx?id={0}&la={1}&fo={0}"")>{2}</a></li>", SitecoreItem.ItemID, SitecoreItem.Language, SitecoreItem.Name);

                //ResultLabel.Text += String.Format(@"<li><a onclick=""scForm.postRequest('','','','item:load(id={0})'); return false;"" href=""#"">{1}</a></li>", SitecoreItem.ItemID, SitecoreItem.Name);
                try
                {
                    //Literal1.Text = Literal1.Text +
                    //                "<li class=\"BlogPostArea\"><div class=\"BlogPostViews\"><span style=\"color: #ffffff;\">" +
                    //                SitecoreItem.Version +
                    //                "<br />views</span><br /><br />1<br />version/s</div><h5 class=\"BlogPostHeader\"><a href=\"#\">" +
                    //                SitecoreItem.Name + "</a></h5><div class=\"BlogPostContent\">" +
                    //                SitecoreItem.GetItem().Fields["Text"] +
                    //                "</div><div class=\"BlogPostFooter\"><div><a href=\"#\">" +
                    //                SitecoreItem.GetItem().Statistics.Created.ToShortDateString() +
                    //                "</a>by<a href=\"#\">" + SitecoreItem.GetItem().Statistics.CreatedBy +
                    //                "</a></div><div><span id=\"ctl00_ctl00_bhcr_bcr_bcr_ctl01_ctl02_ctl08_ctl01\">Filed under: <a href=\"#\" rel=\"tag\">Fasion</a>, <a href=\"#\" rel=\"tag\">Cleo</a>, <a href=\"#\" rel=\"tag\">Food</a></span><input type=\"hidden\" name=\"ctl00$ctl00$bhcr$bcr$bcr$ctl01$ctl02$ctl08$ctl01\" id=\"ctl00_ctl00_bhcr_bcr_bcr_ctl01_ctl02_ctl08_ctl01_State\" value=\"nochange\"/></div></div></li>";
                }
                catch (Exception exc)
                {


                }

                // ResultLabel.Text += String.Format(@"<li><a onclick=""scForm.getParentForm().postRequest('','','','search:launchresult(url={0})'); return false;"" href=""#"">{1}</a></li>", SitecoreItem.Uri.GetPathOrId(), SitecoreItem.Name);
            }

            //ResultLabel.Text += "</ol>";

        }


        public string GetEndDate(List<SearchStringModel> searchParams)
        {
            var templates = searchParams.Where(i => i.Type == "end");
            if (templates.Count() > 0)
                return templates.Single().Value;
            else
            {

                return "";
            }
        }

        public string GetStartDate(List<SearchStringModel> searchParams)
        {
            var templates = searchParams.Where(i => i.Type == "start");
            if (templates.Count() > 0)
                return templates.Single().Value;
            else
            {

                return "";
            }
        }
        public string GetText(List<SearchStringModel> searchParams)
        {
            var templates = searchParams.Where(i => i.Type == "search");
            var returnTemp = String.Empty;
            foreach (var temp in templates)
            {
                returnTemp = returnTemp + temp.Value + " ";

            }
            return returnTemp;
        }

        public string GetTemplates(List<SearchStringModel> searchParams)
        {
            var templates = searchParams.Where(i => i.Type == "template");
            var returnTemp = String.Empty;
            foreach (var temp in templates)
            {
                var template = Sitecore.Context.Database.GetItem("/sitecore/templates").Axes.GetDescendants().Where(i => i.Name == temp.Value);
                returnTemp = returnTemp + template + "|";

            }
            return returnTemp;
        }

        protected virtual List<SitecoreItem> GetItems()
        {
            var searchParam = new SearchParam
            {
                Language = Lang,
                TemplateIds = GetTemplates(FullTextQuery),
                LocationIds = LocationFilter,
                FullTextQuery = GetText(FullTextQuery),
                ShowAllVersions = false
            };

            //var dateRange = new DateRangeSearchParam
            //{
            //    Language = Lang,
            //    TemplateIds = GetTemplates(FullTextQuery),
            //    LocationIds = LocationFilter,
            //    FullTextQuery = GetText(FullTextQuery),
            //    ShowAllVersions = false,
            //    Ranges = new List<DateRangeSearchParam.DateRangeField>()
            //                                     {
            //                                         new DateRangeSearchParam.DateRangeField("__Created",
            //                                                                                 Convert.ToDateTime(GetStartDate(FullTextQuery)),
            //                                                                                 Convert.ToDateTime(GetEndDate(FullTextQuery)))
            //                                     }


            //};

            using (var searcher = new ItemBucket.Kernel.Kernel.Util.IndexSearcher(IndexName))
            {
                var iii = new List<Item>();
                iii = searcher.GetItems(searchParam).Cast<Item>().ToList();
                return searcher.GetItems(searchParam);
            }
        }

        protected virtual List<SitecoreItem> Simple()
        {
            var searchParam = new SearchParam
            {
            
                FullTextQuery = TextBox1.Text
            
            };

            //var dateRange = new DateRangeSearchParam
            //{
            //    Language = Lang,
            //    TemplateIds = GetTemplates(FullTextQuery),
            //    LocationIds = LocationFilter,
            //    FullTextQuery = GetText(FullTextQuery),
            //    ShowAllVersions = false,
            //    Ranges = new List<DateRangeSearchParam.DateRangeField>()
            //                                     {
            //                                         new DateRangeSearchParam.DateRangeField("__Created",
            //                                                                                 Convert.ToDateTime(GetStartDate(FullTextQuery)),
            //                                                                                 Convert.ToDateTime(GetEndDate(FullTextQuery)))
            //                                     }


            //};

            using (var searcher = new ItemBucket.Kernel.Kernel.Util.IndexSearcher(IndexName))
            {
                return searcher.GetItems(searchParam);
            }
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            var items = new List<SitecoreItem>();
            var stopwatch = new Stopwatch();
            try
            {
                stopwatch.Start();
                items.AddRange(Simple());
                stopwatch.Stop();
                //RenderItemDetails(items);
                SearchResults.DataSource = items;
                SearchResults.DataBind();//RenderItemDetails(items);
            }
            catch (Exception)
            {
                //ResultLabel.Text = "There was an error running search.";
                throw;
            }
            finally
            {
                stopwatch.Stop();
            }
        }

        //protected void TemplateListItems_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    PopulateFieldListFilter(TemplateListItems);
        //}

       

    }
}