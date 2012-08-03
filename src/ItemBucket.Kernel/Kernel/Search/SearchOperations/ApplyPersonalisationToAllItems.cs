using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.ItemBucket.Kernel.ItemExtensions.Axes;
using Sitecore.ItemBucket.Kernel.Kernel.Util;
using Sitecore.Shell.Framework.Commands;
using Sitecore.StringExtensions;
using Sitecore.Text;
using Sitecore.Web;
using Sitecore.Web.UI.Sheer;
using Sitecore.Web.UI.WebControls;
using Sitecore.Web.UI.XamlSharp.Continuations;

namespace Sitecore.ItemBucket.Kernel.Kernel.Search.SearchOperations
{
    class ApplyCampaignsGoalsEventsToAllItems : Command, ISupportsContinuation
    {
        private List<SearchStringModel> ExtractSearchQuery(string searchQuery)
        {
            var searchStringModels = new List<SearchStringModel>();
            searchQuery = searchQuery.Replace("text:;", "");
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
            Assert.ArgumentNotNull(context, "context");
            if (context.Items.Length == 1)
            {
                var parameters = new NameValueCollection();
                parameters["items"] = base.SerializeItems(context.Items);
                parameters["fieldid"] = context.Parameters["fieldid"];
                parameters["searchString"] = context.Parameters.GetValues("url")[0].Replace("\"", string.Empty);
                var args = new ClientPipelineArgs(parameters);
                if (ContinuationManager.Current != null)
                {
                    ContinuationManager.Current.Start(this, "Run", args);
                }
                else
                {
                    Context.ClientPage.Start(this, "Run", args);
                }
            }
        }

        protected virtual string GetUrl()
        {
            return "/sitecore/shell/~/xaml/Sitecore.Shell.Applications.Analytics.TrackingField.aspx";
        }

        protected virtual void ShowDialog(string url)
        {
            Assert.ArgumentNotNull(url, "url");
            SheerResponse.ShowModalDialog(url, true);
        }

        [UsedImplicitly]
        protected void Run(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            Item item = base.DeserializeItems(args.Parameters["items"])[0];
            if (SheerResponse.CheckModified())
            {
                string str = args.Parameters["fieldid"];
                if (string.IsNullOrEmpty(str))
                {
                    str = "__Tracking";
                }
                if (args.IsPostBack)
                {
                    if (args.HasResult)
                    {

                        var searchStringModel = ExtractSearchQuery(args.Parameters["searchString"]);
                        var hitsCount = 0;
                        var listOfItems = item.Search(searchStringModel, out hitsCount).ToList();
                        Assert.IsNotNull(item, "item");
                        foreach (var sitecoreItem in listOfItems)
                        {
                            var item1 = sitecoreItem.GetItem();
                            item1.Editing.BeginEdit();
                            item1[str] = args.Result;
                            item1.Editing.EndEdit();

                        }
                        if (AjaxScriptManager.Current != null)
                        {
                            AjaxScriptManager.Current.Dispatch("analytics:trackingchanged");
                        }
                        else
                        {
                            Context.ClientPage.SendMessage(this, "analytics:trackingchanged");
                           // Context.ClientPage.SendMessage(this, "item:refresh(id={0})".FormatWith(new object[] { item.ID.ToString() }));
                        }
                    }
                }
                else if (item.Appearance.ReadOnly)
                {
                    SheerResponse.Alert("You cannot edit the '{0}' item because it is protected.", new string[] { item.DisplayName });
                }
                else if (!item.Access.CanWrite())
                {
                    SheerResponse.Alert("You cannot edit this item because you do not have write access to it.", new string[0]);
                }
                else
                {
                    UrlString urlString = new UrlString(this.GetUrl());
                    UrlHandle handle = new UrlHandle();
                    handle["tracking"] = item[str];
                    handle.Add(urlString);
                    this.ShowDialog(urlString.ToString());
                    args.WaitForPostBack();
                }
            }

        }
    }
}














