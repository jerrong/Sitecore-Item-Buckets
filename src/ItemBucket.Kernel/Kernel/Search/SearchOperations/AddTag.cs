using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.ItemBucket.Kernel.ItemExtensions.Axes;
using Sitecore.ItemBucket.Kernel.Kernel.Util;
using Sitecore.Shell.Applications.WebEdit.Commands;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Text;
using Sitecore.Web;
using Sitecore.Web.Configuration;
using Sitecore.Web.UI.Sheer;
using Sitecore.Web.UI.WebControls;

namespace Sitecore.ItemBucket.Kernel.Kernel.Search.SearchOperations
{
    internal class AddTag : WebEditCommand
    {
        private List<SearchStringModel> ExtractSearchQuery(string searchQuery)
        {
            var searchStringModels = new List<SearchStringModel>();
            searchQuery = searchQuery.Replace("text:;", string.Empty);
            var terms = searchQuery.Split(';');
            for (var i = 0; i < terms.Length; i++)
            {
                if (!string.IsNullOrEmpty(terms[i]))
                {
                    searchStringModels.Add(new SearchStringModel
                                               {
                                                   Type = terms[i].Split(':')[0],
                                                   Value = terms[i].Split(':')[1]
                                               });
                }

                //Because of the String that is passed through I have to skip twice
            }

            return searchStringModels;
        }

        public override void Execute(CommandContext context)
        {
            Item item = context.Items[0];
            var parameters = new NameValueCollection();
            parameters["id"] = item.ID.ToString();
            parameters["language"] = (Context.Language == null) ? item.Language.ToString() : Context.Language.ToString();
            parameters["version"] = item.Version.ToString();
            parameters["database"] = item.Database.Name;
            parameters["isPageEditor"] = (context.Parameters["pageEditor"] == "1") ? "1" : "0";
            parameters["searchString"] = context.Parameters.GetValues("url")[0].Replace("\"", string.Empty);
            Context.ClientPage.Start(this, "Run", parameters);


        }

        private static string GetFeatures()
        {
            var str = "location=0,menubar=0,status=0,toolbar=0,resizable=1";
            var device = Context.Device;
            if (device != null)
            {
                var capabilities = device.Capabilities as SitecoreClientDeviceCapabilities;
                if (capabilities == null)
                {
                    return str;
                }

                if (capabilities.RequiresScrollbarsOnWindowOpen)
                {
                    str = str + ",scrollbars=1,dependent=1";
                }
            }

            return str;
        }

        protected void Run(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            if (SheerResponse.CheckModified())
            {
                if (args.IsPostBack)
                {

                    var tempItem = Factory.GetDatabase(args.Parameters["database"]).GetItem(args.Parameters["id"]);
          
                    var searchStringModel = ExtractSearchQuery(args.Parameters["searchString"]);
                    int hitsCount;
                    var listOfItems = tempItem.Search(searchStringModel, out hitsCount).ToList();
                    Assert.IsNotNull(tempItem, "item");

                    foreach (var sitecoreItem in listOfItems)
                    {
                        var item1 = sitecoreItem.GetItem();
                        if (item1.Fields["tags"].IsNotNull())
                        {
                            item1.Editing.BeginEdit();
                            if (item1.Fields["tags"].Value == string.Empty)
                            {
                                item1.Fields["tags"].Value = args.Result;
                            }
                            else
                            {
                                item1.Fields["tags"].Value = item1.Fields["tags"].Value + "|" + args.Result;
                            }
                            item1.Editing.EndEdit();
                        }
                    }
                }
                else
                {
                    var searchStringModel = ExtractSearchQuery(args.Parameters["searchString"]);
                    var tempItem = Factory.GetDatabase(args.Parameters["database"]).GetItem(args.Parameters["id"]);
                    int hitsCount;
                    var listOfItems = tempItem.Search(searchStringModel, out hitsCount).ToList();
                    if (hitsCount > 0)
                    {
                     
                        var urlString = new UrlString(ItemBucket.Kernel.Util.Constants.ContentEditorRawUrlAddress);
                        var handle = new UrlHandle();
                        handle["itemid"] = args.Parameters["id"];
                        handle["databasename"] = args.Parameters["database"];
                        handle["la"] = args.Parameters["language"];
                        handle.Add(urlString);
                        //SheerResponse.Input("Please enter the Tag ID", "Tag ID");
               
                        UrlString str2 = new UrlString("/sitecore/shell/Applications/Item browser.aspx");
                        str2.Append("ro", "/sitecore/content/Applications");
                        str2.Append("sc_content", Context.ContentDatabase.Name);

                        SheerResponse.ShowModalDialog(str2.ToString(), "1000", "700", "", true);

                        args.WaitForPostBack();
                    }
                }
            }
        }
    }
}
