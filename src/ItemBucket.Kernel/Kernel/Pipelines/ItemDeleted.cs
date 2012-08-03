namespace Sitecore.ItemBucket.Kernel.Pipelines
{
    using Sitecore.Diagnostics;
    using Sitecore.Events;
    using Sitecore.ItemBucket.Kernel.ItemExtensions.Axes;
    using Sitecore.ItemBucket.Kernel.Kernel.Util;
    using Sitecore.Shell.Framework.Pipelines;
    using Sitecore.Web.UI.Sheer;

    /// <summary>
    /// Deleted Item Event Pipeline
    /// </summary>
    internal class ItemDeleted : DeleteItems
    {
        public new void Execute(ClientPipelineArgs args)
        {
            var item = Context.ContentDatabase.GetItem(args.Parameters[1]);
            if (item.IsNotNull())
            {
                Error.AssertItem(item, "item");
                if (!args.IsPostBack)
                {
                    if (item.IsABucket())
                    {
                        new ClientResponse().Confirm("You are about to delete an item which is an item bucket with lots of hidden items below it. Are you sure you want to do this?");
                        args.WaitForPostBack();
                    }
                }
                else
                {
                    if (args.Result != "yes")
                    { 
                        args.AbortPipeline();
                        args.Result = string.Empty;
                        args.IsPostBack = false;
                        return;
                    }

                    Event.RaiseEvent("item:bucketing:deleting", args, this);
                }
            }
        }
    }
}
