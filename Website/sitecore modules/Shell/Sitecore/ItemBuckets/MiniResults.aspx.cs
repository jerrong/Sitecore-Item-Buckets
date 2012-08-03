using System;
using System.Collections.Specialized;
using System.Web;
using Sitecore;
using Sitecore.Collections;
using Sitecore.Data.Items;
using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.ItemBucket.Kernel.Kernel.Util;
using Sitecore.ItemBucket.Kernel.Managers;
using Sitecore.Web;

namespace ItemBuckets
{
    public partial class MiniResults : System.Web.UI.Page
    {
       private string Filter = "";

        private string _ID = string.IsNullOrEmpty(WebUtil.GetQueryString("id"))
                                 ? WebUtil.ExtractUrlParm("id", HttpContext.Current.Request.Url.Query)
                                 : WebUtil.ExtractUrlParm("id", HttpContext.Current.Request.Url.Query);
      
       protected string Id
       {
           get { return _ID; }
           set { _ID = value; }
       }

        protected void Page_Load(object sender, EventArgs e)
        {

            try
            {
                var indexOfRo = System.Web.HttpContext.Current.Request.UrlReferrer.Query.IndexOf("fo=");
                var requestString =
                    System.Web.HttpUtility.ParseQueryString(
                        System.Web.HttpContext.Current.Request.UrlReferrer.Query.Substring(indexOfRo));
                //requestString = StringUtil.GetNameValues(requestString[0], '=', '&');
                var refinements = new SafeDictionary<string>();
                if (requestString["FieldsFilter"] != null)
                {
                    var splittedFields = StringUtil.GetNameValues(requestString["FieldsFilter"], ':', ',');
                    foreach (string key in splittedFields.Keys)
                    {
                        refinements.Add(key, splittedFields[key]);
                    }
                }

                var locationFilter = requestString["StartSearchLocation"];
                if (locationFilter.IsNotNull())
                {
                    if (locationFilter.StartsWith("query:"))
                    {
                        locationFilter = locationFilter.Replace("->", "=");
                        Item itemArray;
                        string query = locationFilter.Substring(6);
                        bool flag = query.StartsWith("fast:");
                        Opcode opcode = null;
                        if (!flag)
                        {
                            QueryParser.Parse(query);
                        }
                        if (flag || (opcode is Root))
                        {
                            itemArray =
                                Sitecore.Context.Item.Database.SelectSingleItem(query);
                        }
                        else
                        {
                            itemArray = Sitecore.Context.Item.Axes.SelectSingleItem(query);
                        }

                        locationFilter = itemArray.ID.ToString();
                        
                    }
                }

                var pageSize = requestString["PageSize"];


                var locationFinal = (locationFilter.IsNullOrEmpty() ? Sitecore.Context.ContentDatabase.GetItem(requestString["id"]).GetParentBucketItemOrRootOrSelf().ID.ToString() : locationFilter);
                _ID = locationFinal;
                Filter = "location=" +
                         locationFinal +

                         "&text=" + requestString["FullTextQuery"] +
                         "&language=" + requestString["Language"] +
                         "&pageSize=" + (pageSize.IsNullOrEmpty() ? 20 : Int32.Parse(pageSize)) +

                         "&sort=" + requestString["SortField"];

                if (requestString["TemplateFilter"].IsNotNull())
                {

                    Filter += "&template=" + requestString["TemplateFilter"];
                }

                
            }
            catch(Exception exc)
            {
                Log.Error("Failed to Resolve Rich Text Editor Source", exc, this);

            }
            finally
            {
                if (!Id.IsNullOrEmpty())
                {
                    Page.Response.Write(
                        "<style>.token-input-list-facebook.boxme {background-image: url(/temp/IconCache/" +
                        Sitecore.Context.ContentDatabase.GetItem(Id).Appearance.Icon +
                        ");background-size:16px 16px;background-position: 2% 50%;background-repeat: no-repeat;}</style>");
                }
                var script = "<script type='text/javascript' language='javascript'>var filterForSearch='" + Filter +
                             "';</script>";
                Page.Response.Write(script);
            }



        }
    }
}