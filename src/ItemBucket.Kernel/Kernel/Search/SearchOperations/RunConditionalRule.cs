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
    class RunConditionalRule : Command, ISupportsContinuation
    {
        // Methods
        public override void Execute(CommandContext context)
        {
            var searchStringModel = ExtractSearchQuery(context.Parameters.GetValues("url")[0].Replace("\"", ""));
            var hitsCount = 0;
            var listOfItems = context.Items[0].Search(searchStringModel, out hitsCount).ToList();
            var parameters = new NameValueCollection();
            parameters["searchString"] = context.Parameters.GetValues("url")[0].Replace("\"", "");
            if (ContinuationManager.Current != null)
            {
                ContinuationManager.Current.Start(this, "Run", new ClientPipelineArgs(parameters));
            }
            else
            {
                Context.ClientPage.Start(this, "Run", parameters);
            }
        }

        public override CommandState QueryState(CommandContext context)
        {
            Error.AssertObject(context, "context");
            if (context.Items.Length != 1)
            {
                return CommandState.Disabled;
            }
            Item item = context.Items[0];
            if (item.Appearance.ReadOnly)
            {
                return CommandState.Disabled;
            }
            if (!item.Access.CanRead())
            {
                return CommandState.Disabled;
            }
            return base.QueryState(context);
        }

        protected void Run(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            if (SheerResponse.CheckModified())
            {
                if (args.IsPostBack)
                {
                    if (((Context.Page != null) && (Context.Page.Page != null)) && ((Context.Page.Page.Session["TrackingFieldModified"] as string) == "1"))
                    {
                        Context.Page.Page.Session["TrackingFieldModified"] = null;
                         if (AjaxScriptManager.Current != null)
                        {
                            AjaxScriptManager.Current.Dispatch("analytics:trackingchanged");
                            AjaxScriptManager.Current.Dispatch("item:refresh(id={0})".FormatWith(new object[] { args.Parameters["id"] }));
                        }
                        else
                        {
                            Context.ClientPage.SendMessage(this, "analytics:trackingchanged");
                            Context.ClientPage.SendMessage(this, "item:refresh(id={0})".FormatWith(new object[] { args.Parameters["id"] }));
                        }
                    }
                }
                else
                {
                    var urlString = new UrlString("/sitecore/shell/~/xaml/Sitecore.Shell.Applications.Rules.RulesEditor.aspx");
                    var handle = new UrlHandle();
                    handle.Add(urlString);
                    SheerResponse.ShowModalDialog(urlString.ToString(), "1000", "600", string.Empty, true);
                    args.WaitForPostBack();
                }
            }
        }


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
    }
}
