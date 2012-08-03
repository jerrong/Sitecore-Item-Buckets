using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.ItemBucket.Kernel.ItemExtensions.Axes;
using Sitecore.ItemBucket.Kernel.Kernel.Util;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Text;
using Sitecore.Web;
using Sitecore.Web.UI.Sheer;
using Sitecore.Web.UI.WebControls;
using Sitecore.Web.UI.XamlSharp.Continuations;

namespace Sitecore.ItemBucket.Kernel.Kernel.Search.SearchOperations
{
    class ChangeTemplateForAllItems : Command, ISupportsContinuation
    {
        private List<SearchStringModel> ExtractSearchQuery(string searchQuery)
        {
            var searchStringModels = new List<SearchStringModel>();
            searchQuery = searchQuery.Replace("text:;", "");
            var terms = searchQuery.Split(';');
            for (var i = 0; i < terms.Length; i++)
            {
                if (!terms[i].IsNullOrEmpty())
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
                Item item = context.Items[0];
                NameValueCollection parameters = new NameValueCollection();
                Assert.IsNotNull(item, "item");
                parameters["id"] = item.ID.ToString();
                parameters["searchString"] = context.Parameters.GetValues("url")[0].Replace("\"", "");
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

        [UsedImplicitly]
        protected void Run(ClientPipelineArgs args)
        {
            Item item = Sitecore.Context.Database.GetItem(args.Parameters["id"]);
            if (SheerResponse.CheckModified())
            {
                if (args.IsPostBack)
                {
                    if (args.Result == "yes")
                    {
                        Context.ClientPage.SendMessage(this, "item:templatechanged");
                        var searchStringModel = ExtractSearchQuery(args.Parameters["searchString"]);
                        var hitsCount = 0;
                        var listOfItems = item.Search(searchStringModel, out hitsCount).ToList();
                        Assert.IsNotNull(item, "item");
                        foreach (var sitecoreItem in listOfItems)
                        {
                            var item1 = sitecoreItem.GetItem();
                            item1.Editing.BeginEdit();
                            item1.ChangeTemplate(listOfItems.First().GetItem().Template);
                            item1.Editing.EndEdit();
                        }
                    }
                }
                else
                {
                    string str = args.Parameters["id"];
                    Item item3 = Context.ContentDatabase.Items[str];
                    if (item3 == null)
                    {
                        SheerResponse.Alert("Item not found.", new string[0]);
                    }
                    else
                    {
                        UrlString str2 = new UrlString("/sitecore/shell/Applications/Templates/Change template.aspx");
                        var searchStringModel = ExtractSearchQuery(args.Parameters["searchString"]);
                        var hitsCount = 0;
                        var listOfItems = item.Search(searchStringModel, out hitsCount).ToList();
                        str2.Append("id", listOfItems.First().GetItem().ID.ToString());
                        Context.ClientPage.ClientResponse.ShowModalDialog(str2.ToString(), true);
                        args.WaitForPostBack();
                    }
                }
            }
        }
    }
}