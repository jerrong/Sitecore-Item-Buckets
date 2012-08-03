using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.ItemBucket.Kernel.ItemExtensions.Axes;
using Sitecore.ItemBucket.Kernel.Kernel.Util;
using Sitecore.Shell.Framework;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web.UI.Sheer;

namespace Sitecore.ItemBucket.Kernel.Kernel.Search.SearchOperations
{
    class DeleteCommand : Command
    {
        // Methods
        public override void Execute(CommandContext context)
        {
            var searchStringModel = ExtractSearchQuery(context.Parameters.GetValues("url")[0].Replace("\"", ""));
            int hitsCount;
            var listOfItems = context.Items[0].Search(searchStringModel, out hitsCount).ToList();
            Items.Delete(listOfItems.Select(i => i.GetItem()).ToArray());
        }

        public override CommandState QueryState(CommandContext context)
        {
            Error.AssertObject(context, "context");
            if (context.Items.Length != 1)
            {
                return CommandState.Disabled;
            }
            Item item = context.Items[0];
            if (item.IsNotNull())
            {
                if (item.Appearance.ReadOnly)
                {
                    return CommandState.Disabled;
                }
                if (!item.Access.CanRead())
                {
                    return CommandState.Disabled;
                }
            }
            return base.QueryState(context);
        }

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
    }
}