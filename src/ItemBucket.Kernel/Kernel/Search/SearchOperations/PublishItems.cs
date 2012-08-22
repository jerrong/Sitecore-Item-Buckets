using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.ItemBucket.Kernel.ItemExtensions.Axes;
using Sitecore.ItemBucket.Kernel.Kernel.Util;
using Sitecore.Publishing;
using Sitecore.SecurityModel;
using Sitecore.Shell.Framework;
using Sitecore.Shell.Framework.Commands;

namespace Sitecore.ItemBucket.Kernel.Kernel.Search.SearchOperations
{
    internal class PublishItems : Command
    {
        // Methods
        public override void Execute(CommandContext context)
        {
            var searchStringModel = ExtractSearchQuery(context.Parameters.GetValues("url")[0].Replace("\"", ""));
            int hitsCount;
            var listOfItems = context.Items[0].Search(searchStringModel, out hitsCount).ToList();
            foreach (var item in listOfItems)
            {
                PublishItem(item.GetItem());
            }
        }

        private void PublishItem(Item item)
        {
            PublishManager.PublishItem(item, this.GetTargets(item), new Language[] {item.Language}, false, false);
        }

        private Database[] GetTargets(Item item)
        {
            using (new SecurityDisabler())
            {
                Item item2 = item.Database.Items["/sitecore/system/publishing targets"];
                if (item2 != null)
                {
                    var list = new ArrayList();
                    foreach (Item item3 in item2.Children)
                    {
                        string name = item3["Target database"];
                        if (name.Length > 0)
                        {
                            Database database = Factory.GetDatabase(name, false);
                            if (database != null)
                            {
                                list.Add(database);
                            }
                            else
                            {
                                Log.Warn("Unknown database in PublishAction: " + name, this);
                            }
                        }
                    }
                    return (list.ToArray(typeof (Database)) as Database[]);
                }
            }
            return new Database[0];
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
