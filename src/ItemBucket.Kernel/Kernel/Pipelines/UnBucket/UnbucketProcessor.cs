using System.Linq;
using Sitecore.Data.Fields;
using Sitecore.Data.Managers;
using Sitecore.ItemBucket.Kernel.Managers;
using Sitecore.SecurityModel;

namespace Sitecore.ItemBucket.Kernel.Pipelines.UnBucket
{
    using Sitecore.Data.Items;
    using Sitecore.Diagnostics;
    using Sitecore.Events;
    using Sitecore.ItemBucket.Kernel.ItemExtensions.Axes;
    using Sitecore.ItemBucket.Kernel.Kernel.Pipelines;
    using Sitecore.ItemBucket.Kernel.Kernel.Util;
    using Sitecore.Resources;

    public class UnbucketProcessor
    {
        public void Unbucket(BucketArgs args)
        {
            Event.RaiseEvent("item:unbucketing:starting", args, this);
            var contextItem = args.Item;
            if (contextItem.IsNotNull())
            {
                Util.SearchHelper.AddSearchTab(contextItem, contextItem.GetEditors());
                Shell.Applications.Dialogs.ProgressBoxes.ProgressBox.Execute(Util.Constants.UnBucketingText, Util.Constants.UnBucketingText, Images.GetThemedImageSource("Business/32x32/chest_delete.png"), this.StartProcess, new object[] { contextItem });
                Context.ClientPage.SendMessage(this, "item:load(id=" + contextItem.ID + ")");
                Context.ClientPage.SendMessage(this, "item:refreshchildren(id=" + contextItem.Parent.ID + ")");
            }
        }

        

        private void StartProcess(params object[] parameters)
        {
            var contextItem = (Item)parameters[0];
            if (contextItem.IsNotNull())
            {
                BucketManager.ShowAllSubFolders(contextItem);
                foreach (var deleteFolder in contextItem.Children.Where(item => item.TemplateID.ToString() == Util.Constants.BucketFolder))
                {
                    ItemManager.DeleteItem(deleteFolder);
                }

                using (new EditContext(contextItem))
                {
                    contextItem.IsBucketItemCheckBox().Checked = false;
                    Log.Info("You have turned the " + contextItem.ID + " bucket into a normal structured item container", this);
                }
            }

            Event.RaiseEvent("item:unbucketing:ended", parameters, this);
        }
    }
}
