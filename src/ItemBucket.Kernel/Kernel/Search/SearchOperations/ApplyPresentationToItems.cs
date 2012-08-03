using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.ItemBucket.Kernel.ItemExtensions.Axes;
using Sitecore.ItemBucket.Kernel.Kernel.Util;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Text;
using Sitecore.Web;
using Sitecore.Web.UI.Sheer;

namespace Sitecore.ItemBucket.Kernel.Kernel.Search.SearchOperations
{
    class ApplyPresentationToItems : Command
    {
        // Methods
        public override void Execute(CommandContext context)
        {
            Item item = context.Items[0];
            var parameters = new NameValueCollection();
            parameters["id"] = item.ID.ToString();
            parameters["language"] = item.Language.ToString();
            parameters["version"] = item.Version.ToString();
            parameters["database"] = item.Database.Name;
            parameters["searchString"] = context.Parameters.GetValues("url")[0].Replace("\"", "");
            Context.ClientPage.Start(this, "Run", parameters);
        }


        public override CommandState QueryState(CommandContext context)
        {
            Assert.ArgumentNotNull(context, "context");
            if (context.Items.Length != 1)
            {
                return CommandState.Hidden;
            }
            var item = context.Items[0];
            if (item.IsNotNull())
            {
                if (!base.HasField(item, FieldIDs.LayoutField))
                {
                    return CommandState.Hidden;
                }
                if (WebUtil.GetQueryString("mode") == "preview")
                {
                    return CommandState.Disabled;
                }
                if (!item.Access.CanWrite())
                {
                    return CommandState.Disabled;
                }
            }
            return base.QueryState(context);
        }

        protected void Run(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            if (args.IsPostBack)
            {
                if (args.HasResult)
                {
                    Database database = Factory.GetDatabase(args.Parameters["database"]);
                    Assert.IsNotNull(database, "Database \"" + args.Parameters["database"] + "\" not found.");
                    Item item = database.Items[args.Parameters["id"], Language.Parse(args.Parameters["language"]), Version.Parse(args.Parameters["version"])];
                    var searchStringModel = ExtractSearchQuery(args.Parameters["searchString"]);
                    var hitsCount = 0;
                    var listOfItems = item.Search(searchStringModel, out hitsCount).ToList();
                    Assert.IsNotNull(item, "item");
                    var result = args.Result;
                    foreach (var item1 in listOfItems.Select(sitecoreItem => sitecoreItem.GetItem()))
                    {
                        item1.Editing.BeginEdit();
                        if (item1.Name != "__Standard Values")
                        {
                            LayoutField.SetFieldValue(item1.Fields[FieldIDs.LayoutField], result);
                        }
                        else
                        {
                            item1[FieldIDs.LayoutField] = result;
                        }
                        item1.Editing.EndEdit();
                        Log.Audit(this, "Set layout details: {0}, layout: {1}", new [] { AuditFormatter.FormatItem(item1), result });
                    }
                }
            }
            else
            {
                var str2 = new UrlString(UIUtil.GetUri("control:LayoutDetails"));
                str2.Append("id", args.Parameters["id"]);
                str2.Append("la", args.Parameters["language"]);
                str2.Append("vs", args.Parameters["version"]);
                SheerResponse.ShowModalDialog(str2.ToString(), true);
                args.WaitForPostBack();
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
