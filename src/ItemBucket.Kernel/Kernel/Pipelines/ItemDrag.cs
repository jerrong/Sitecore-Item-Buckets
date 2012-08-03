using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;
using Sitecore.Events;
using Sitecore.ItemBucket.Kernel.Kernel.Util;
using Sitecore.ItemBucket.Kernel.Managers;
using Sitecore.Resources;
using Sitecore.SecurityModel;
using Sitecore.Shell.Framework;
using Sitecore.Shell.Framework.Pipelines;
using Sitecore.Web.UI.Sheer;

namespace Sitecore.ItemBucket.Kernel.Kernel.Pipelines
{
    public class ItemDrag : ItemOperation
    {
        public new void Execute(ClientPipelineArgs args)
        {
            Event.RaiseEvent("item:bucketing:dragInto", args, this);
            Assert.ArgumentNotNull(args, "args");
            Database database = GetDatabase(args);
            Item source = GetSource(args, database);
            Item target = GetTarget(args);
            if (args.Parameters["copy"] == "1")
            {

                if (BucketManager.IsBucket(target))
                {
                    Shell.Applications.Dialogs.ProgressBoxes.ProgressBox.Execute("Copying Items", "Copying Items",
                                                                                 Images.GetThemedImageSource(
                                                                                     "Business/16x16/chest_add.png"),
                                                                                 this.StartProcess,
                                                                                 new object[]
                                                                                     {
                                                                                         source,
                                                                                         BucketManager.
                                                                                             CreateAndReturnDateFolderDestination
                                                                                             (target, DateTime.Now), true, args
                                                                                     });

                    if (source.IsNotNull())
                    {
                        Log.Info("Item " + source.ID + " has been copied to another bucket", this);
                    }

                   
                    
               
                }

               
            }
            else
            {
                if (BucketManager.IsBucket(target))
                {
                    Shell.Applications.Dialogs.ProgressBoxes.ProgressBox.Execute("Moving Items", "Moving Items",
                                                                                 Images.GetThemedImageSource(
                                                                                     "Business/16x16/chest_add.png"),
                                                                                 this.StartMoveProcess,
                                                                                 new object[]
                                                                                     {
                                                                                         source,
                                                                                         BucketManager.
                                                                                             CreateAndReturnDateFolderDestination
                                                                                             (target, DateTime.Now), args
                                                                                     });
               
                }
            }


            Event.RaiseEvent("item:bucketing:dragged", args, this);
          
            //Assert.ArgumentNotNull(args, "args");
            //var items = GetItems(args);
            //Item currentItem = null;
            //if (items[0].IsNotNull())
            //{
            //    currentItem = items[0];
            //}

            //Error.AssertItem(items[0], "Item");
            //var database = GetDatabase(args);
            //var topParent = database.GetItem(args.Parameters["destination"]);
            //if (BucketManager.IsBucket(topParent))
            //{
            //    Shell.Applications.Dialogs.ProgressBoxes.ProgressBox.Execute("Copying Items", "Copying Items", Images.GetThemedImageSource("Business/16x16/chest_add.png"), this.StartProcess, new object[] { currentItem, BucketManager.CreateAndReturnDateFolderDestination(topParent, DateTime.Now), true });

            //    if (currentItem.IsNotNull())
            //    {
            //        Log.Info("Item " + currentItem.ID + " has been copied to another bucket", this);
            //    }

            //    Event.RaiseEvent("item:bucketing:copied", args, this);
            //    args.AbortPipeline();
            //}
         
        }


        private static Database GetDatabase(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            Database database = Factory.GetDatabase(args.Parameters["database"]);
            Error.Assert(database != null, "Database \"" + args.Parameters["database"] + "\" not found.");
            return database;
        }

        private static Item GetSource(ClientPipelineArgs args, Database database)
        {
            Assert.ArgumentNotNull(args, "args");
            Assert.ArgumentNotNull(database, "database");
            Item item = database.Items[args.Parameters["id"]];
            Assert.IsNotNull(item, typeof(Item), "ID:{0}", new object[] { args.Parameters["id"] });
            return item;
        }

        private static Item GetTarget(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            Item parent = GetDatabase(args).Items[args.Parameters["target"]];
            Assert.IsNotNull(parent, typeof(Item), "ID:{0}", new object[] { args.Parameters["target"] });
            if (args.Parameters["appendAsChild"] != "1")
            {
                parent = parent.Parent;
                Assert.IsNotNull(parent, typeof(Item), "ID:{0}.Parent", new object[] { args.Parameters["target"] });
            }
            return parent;
        }

        private static void SetSortorder(Item item, ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(item, "item");
            Assert.ArgumentNotNull(args, "args");
            if (args.Parameters["appendAsChild"] != "1")
            {
                Item target = GetDatabase(args).Items[args.Parameters["target"]];
                if (target != null)
                {
                    int sortorder = target.Appearance.Sortorder;
                    if (args.Parameters["sortAfter"] == "1")
                    {
                        Item nextSibling = target.Axes.GetNextSibling();
                        if (nextSibling == null)
                        {
                            sortorder += 100;
                        }
                        else
                        {
                            int num2 = nextSibling.Appearance.Sortorder;
                            if (Math.Abs((int)(num2 - sortorder)) >= 2)
                            {
                                sortorder += (num2 - sortorder) / 2;
                            }
                            else if (target.Parent != null)
                            {
                                sortorder = Resort(target, DragAction.After);
                            }
                        }
                    }
                    else
                    {
                        Item previousSibling = target.Axes.GetPreviousSibling();
                        if (previousSibling == null)
                        {
                            sortorder -= 100;
                        }
                        else
                        {
                            int num3 = previousSibling.Appearance.Sortorder;
                            if (Math.Abs((int)(num3 - sortorder)) >= 2)
                            {
                                sortorder -= (sortorder - num3) / 2;
                            }
                            else if (target.Parent != null)
                            {
                                sortorder = Resort(target, DragAction.Before);
                            }
                        }
                    }
                    SetItemSortorder(item, sortorder);
                }
            }
        }

        private static int Resort(Item target, DragAction dragAction)
        {
            Assert.ArgumentNotNull(target, "target");
            int num = 0;
            int num2 = 0;
            foreach (Item item in target.Parent.Children)
            {
                item.Editing.BeginEdit();
                item.Appearance.Sortorder = num2 * 100;
                item.Editing.EndEdit();
                if (item.ID == target.ID)
                {
                    num = (dragAction == DragAction.Before) ? ((num2 * 100) - 50) : ((num2 * 100) + 50);
                }
                num2++;
            }
            return num;
        }

        private static void SetItemSortorder(Item item, int sortorder)
        {
            Assert.ArgumentNotNull(item, "item");
            item.Editing.BeginEdit();
            item.Appearance.Sortorder = sortorder;
            item.Editing.EndEdit();
        }

        private void StartProcess(params object[] parameters)
        {
            var contextItem = (Item)parameters[0];
            var topParent = (Item)parameters[1];
            var recurse = (bool)parameters[2];
            var args = (ClientPipelineArgs)parameters[3];
            using (new EditContext(contextItem, SecurityCheck.Disable))
            {
                Log.Audit(this, "Copy item: {0} to {1}", new string[] { AuditFormatter.FormatItem(contextItem), AuditFormatter.FormatItem(topParent) });

                ItemManager.CopyItem(contextItem, topParent, recurse);
                args.AbortPipeline();
            }
        }

        private void StartMoveProcess(params object[] parameters)
        {
            var item = (Item)parameters[0];
            var topParent = (Item)parameters[1];
            var args = (ClientPipelineArgs)parameters[2];
            using (new EditContext(item, SecurityCheck.Disable))
            {
                Log.Audit(this, "Drag item: {0} to {1}", new string[] { AuditFormatter.FormatItem(item), AuditFormatter.FormatItem(topParent) });
            
                ItemManager.MoveItem(item, topParent);

                args.AbortPipeline();
            }

        }

    }
}


 
