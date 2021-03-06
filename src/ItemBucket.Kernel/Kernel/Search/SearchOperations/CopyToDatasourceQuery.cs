﻿using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.ItemBucket.Kernel.ItemExtensions.Axes;
using Sitecore.ItemBucket.Kernel.Kernel.Util;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web.UI.Sheer;

namespace Sitecore.ItemBucket.Kernel.Kernel.Search.SearchOperations
{
    public class CopyToDatasourceQuery : Command
    {
        // Methods
        public override void Execute(CommandContext context)
        {
            if (context.Items.Length == 1)
            {
                var item = context.Items[0];
                var parameters = new NameValueCollection();
                parameters["id"] = item.ID.ToString();
                parameters["searchString"] = context.Parameters.GetValues("url")[0].Replace("\"", "");
                Context.ClientPage.Start(this, "Run", parameters);
            }
        }

        public override CommandState QueryState(CommandContext context)
        {
            Error.AssertObject(context, "context");
            if (context.Items.Length != 1)
            {
                return CommandState.Hidden;
            }
            if (!UIUtil.IsIE())
            {
                return CommandState.Hidden;
            }
            return base.QueryState(context);
        }

        private List<SearchStringModel> ExtractSearchQuery(string searchQuery)
        {
            var searchStringModels = new List<SearchStringModel>();
            searchQuery = searchQuery.Replace("text:;", "");
            var terms = searchQuery.Split(';').Where(item => item.IsNotEmpty()).ToList();
            for (var i = 0; i < terms.Count; i++)
            {
                if (!terms[i].IsNullOrEmpty())
                {
                    var strings = terms[i].Split(':');
                    searchStringModels.Add(new SearchStringModel
                                               {
                                                   Type = strings[0],
                                                   Value = strings[1]
                                               });
                }
            }
            return searchStringModels;
        }

        protected void Run(ClientPipelineArgs args)
        {
            if (args.IsPostBack)
            {
                if (args.Result == "yes")
                {
                    var item = Context.ContentDatabase.GetItem(args.Parameters["id"]);
                    var copyString = string.Empty;
                    var searchStringModel = ExtractSearchQuery(args.Parameters["searchString"]);
                    copyString = searchStringModel.Aggregate(copyString, (current, stringModel) => current + stringModel.Type + ":" + stringModel.Value + ";");
                    Assert.IsNotNull(item, "item");
                    Sitecore.Context.ClientData.SetValue("CurrentPasteDatasource", copyString);
                    SheerResponse.Eval(string.Format("window.clipboardData.setData(\"Text\", \"{0}\")", copyString));
                }
            }
            else
            {
                SheerResponse.Confirm(Translate.Text("Do you want to copy the search to a datasource query?"));
                args.WaitForPostBack();
            }
        }
    }
}
