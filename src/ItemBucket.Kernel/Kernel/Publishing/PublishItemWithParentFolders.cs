using System.Collections;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Publishing;
using Sitecore.SecurityModel;
using Sitecore.Workflows.Simple;

namespace Sitecore.ItemBucket.Kernel.Kernel.Publishing
{
    public class PublishItemWithParentFolders
    {
        public void Process(WorkflowPipelineArgs args)
        {
            var itemLinks = args.DataItem.Axes.GetAncestors();

            foreach (var itm in itemLinks)
            {
                if (itm != null && itm.Paths.IsMediaItem)
                {
                    PublishItem(itm);
                }
            }
        }

        private void PublishItem(Item item)
        {
            PublishManager.PublishItem(item, this.GetTargets(item), new Language[] { item.Language }, false, false);
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
                    return (list.ToArray(typeof(Database)) as Database[]);
                }
            }
            return new Database[0];
        }
    }
}
