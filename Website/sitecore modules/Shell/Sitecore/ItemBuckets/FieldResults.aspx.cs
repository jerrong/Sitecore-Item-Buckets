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
    public partial class FieldResults : System.Web.UI.Page
    {
        private string _ID = string.IsNullOrEmpty(WebUtil.GetQueryString("id"))
                                ? WebUtil.ExtractUrlParm("id", HttpContext.Current.Request.Url.Query)
                                : WebUtil.ExtractUrlParm("id", HttpContext.Current.Request.Url.Query);

        protected string Id
        {
            get { return _ID; }
            set { _ID = value; }
        }
        private string Filter = "";
        private string thisIsWorkBox = "";
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {


                var indexOfRo = System.Web.HttpContext.Current.Request.UrlReferrer.Query.IndexOf("ro=");
                var requestString = new NameValueCollection();
                if (indexOfRo > 0)
                {
                    requestString =
                        System.Web.HttpUtility.ParseQueryString(
                            System.Web.HttpContext.Current.Request.UrlReferrer.Query.Substring(indexOfRo));
                    requestString = StringUtil.GetNameValues(requestString[0], '=', '&');

                }

                else if (System.Web.HttpContext.Current.Request.UrlReferrer.Query.Contains("xmlcontrol=AddFromTemplate") || System.Web.HttpContext.Current.Request.UrlReferrer.Query.Contains("xmlcontrol=SetMasters") || System.Web.HttpContext.Current.Request.UrlReferrer.Query.Contains("xmlcontrol=SetBaseTemplates"))
                {
                    requestString["StartSearchLocation"] = "{3C1715FE-6A13-4FCF-845F-DE308BA9741D}";
                    requestString["TemplateFilter"] = "{AB86861A-6030-46C5-B394-E8F99E8B87DB}";
                }

                else if (System.Web.HttpContext.Current.Request.UrlReferrer.Query.Contains("xmlcontrol=CopyDeviceTo"))
                {
                    requestString["StartSearchLocation"] = "{11111111-1111-1111-1111-111111111111}";
            
                }

                else if (System.Web.HttpContext.Current.Request.UrlReferrer.Query.Contains("xmlcontrol=Sitecore.Shell.Applications.Dialogs.SelectItem"))
                {
                    requestString["StartSearchLocation"] = Sitecore.ItemIDs.RootID.ToString();
                }
              
                var refinements = new SafeDictionary<string>();
                 if (requestString["FieldsFilter"] != null)
                 {
                     var splittedFields = StringUtil.GetNameValues(requestString["FieldsFilter"], ':', ',');
                     foreach (string key in splittedFields.Keys)
                     {
                         refinements.Add(key, splittedFields[key]);
                     }
                 }
                

                 else if (System.Web.HttpContext.Current.Request.UrlReferrer.AbsolutePath == "/sitecore/shell/sitecore/content/Applications/Workbox")
                 {
                     requestString["Custom"] = "__workflow\\ state|*";
                     requestString["StartSearchLocation"] = Sitecore.ItemIDs.RootID.ToString();
                     thisIsWorkBox = "true";
                 }

                 else if (System.Web.HttpContext.Current.Request.UrlReferrer.AbsolutePath == "/sitecore/shell/Applications/Templates/Change%20template.aspx")
                 {
                     requestString["StartSearchLocation"] = "{3C1715FE-6A13-4FCF-845F-DE308BA9741D}";
                     requestString["TemplateFilter"] = "{AB86861A-6030-46C5-B394-E8F99E8B87DB}";
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

                 var locationFinal = (locationFilter.IsNullOrEmpty() ? Sitecore.Context.ContentDatabase.GetItem(requestString["id"]).GetParentBucketItemOrRootOrSelf().ID.ToString(): locationFilter);
                 _ID = locationFinal;
                 Filter = "location=" +
                          locationFinal +

                          "&text=" + requestString["FullTextQuery"] +
                          "&language=" + requestString["Language"] +
                          "&pageSize=" + (pageSize.IsEmpty() ? 20 : Int32.Parse(pageSize)) +
                          "&custom=" + requestString["Custom"] +
                          "&sort=" + requestString["SortField"];

                 if (requestString["TemplateFilter"].IsNotNull())
                 {

                     Filter += "&template=" + requestString["TemplateFilter"];
                 }
            

            }
            catch (Exception exc)
            {

                Log.Error("Failed to Resolve Field Value", exc, this);
            }
            finally
            {
                if (!Id.IsNullOrEmpty())
                {
                    var item = Sitecore.Context.ContentDatabase.GetItem(Id);
                    Page.Response.Write(
                        "<style>.token-input-list-facebook.boxme {background-image: url(/temp/IconCache/" + (item.IsNotNull() ? item.Appearance.Icon : "") +
                        ");background-size:16px 16px;background-position: 2% 50%;background-repeat: no-repeat;}</style>");
                }
                var script = "<script type='text/javascript' language='javascript'>var filterForSearch='" + Filter +
                             "';var workBox='" + thisIsWorkBox +"';</script>";
                Page.Response.Write(script);
            }

        }

    }
}