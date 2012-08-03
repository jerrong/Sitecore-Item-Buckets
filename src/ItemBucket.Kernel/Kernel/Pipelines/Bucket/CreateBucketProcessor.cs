using Sitecore.SecurityModel;

namespace Sitecore.ItemBucket.Kernel.Pipelines.Bucket
{
    using System.Linq;

    using Sitecore.Data.Fields;
    using Sitecore.Data.Items;
    using Sitecore.Diagnostics;
    using Sitecore.Events;
    using Sitecore.ItemBucket.Kernel.ItemExtensions.Axes;
    using Sitecore.ItemBucket.Kernel.Kernel.Pipelines;
    using Sitecore.ItemBucket.Kernel.Managers;
    using Sitecore.Resources;
    using System.Threading.Tasks;

    public class CreateBucketProcessor
    {
        public void CreateBucket(BucketArgs args)
        {
            Event.RaiseEvent("item:bucketing:starting", args, this);
            var contextItem = args.Item;
            MultilistField editors = contextItem.Fields["__Editors"];
            using (new EditContext(contextItem, SecurityCheck.Disable))
            {
                if (!editors.Items.Contains(Util.Constants.SearchEditor))
                {
                    var tempEditors = editors.GetItems();
                    tempEditors.ToList().ForEach(tempEditor => editors.Remove(tempEditor.ID.ToString()));
                    editors.Add(Util.Constants.SearchEditor);
                    tempEditors.ToList().ForEach(tempEditor => editors.Add(tempEditor.ID.ToString()));
                }
            }

            Shell.Applications.Dialogs.ProgressBoxes.ProgressBox.Execute(Util.Constants.BucketingText, Util.Constants.BucketingProgressText, Images.GetThemedImageSource("Business/16x16/chest_add.png"), this.StartProcess, new object[] { contextItem });
            Context.ClientPage.SendMessage(this, "item:load(id=" + contextItem.ID + ")");
            Context.ClientPage.SendMessage(this, "item:refreshchildren(id=" + contextItem.Parent.ID + ")");
        }

        /// <summary>
        /// Method for Creating Bucket
        /// </summary>
        /// <param name="parameters">
        /// The parameters.
        /// </param>
        private void StartProcess(params object[] parameters)
        {
            var contextItem = (Item)parameters[0];
            BucketManager.CreateBucket(contextItem);
            using (new EditContext(contextItem, SecurityCheck.Disable))
            {
                if (!contextItem.IsBucketItemCheck())
                {
                    contextItem.IsBucketItemCheckBox().Checked = true;
                }

                if (contextItem.HasChildren)
                {
                    Parallel.ForEach(contextItem.GetChildren(), (child, state, i) =>
                    {
                        using (new EditContext(child, SecurityCheck.Disable))
                        {
                            if (child.TemplateID == Util.Config.ContainerTemplateId)
                            {
                                child.Appearance.Hidden = true;
                            }
                        }
                    });
                }

                Event.RaiseEvent("item:bucketing:ended", parameters, this);
                Log.Info("Created Bucket Item for " + contextItem.ID, this);
            }
        }
    }
}
