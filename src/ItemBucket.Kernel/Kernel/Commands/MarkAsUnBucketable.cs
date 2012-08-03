// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MarkAsUnBucketable.cs" company="Sitecore">
//   Sitecore
// </copyright>
// <summary>
//   Defines the MarkAsUnBucketable type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Sitecore.SecurityModel;

namespace Sitecore.ItemBucket.Kernel.Commands
{
    using System.Collections.Specialized;
    using Sitecore.Data.Fields;
    using Sitecore.Data.Items;
    using Sitecore.Diagnostics;
    using Sitecore.ItemBucket.Kernel.Kernel.Util;
    using Sitecore.ItemBucket.Kernel.Util;
    using Sitecore.Shell.Framework.Commands;
    using Sitecore.Web.UI.Sheer;

    /// <summary>
    /// Make Template not allowed within Bucket
    /// </summary>
    internal class MarkAsUnBucketable : Command
    {
        /// <summary>
        /// Execute the Command
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        public override void Execute(CommandContext context)
        {
            Assert.ArgumentNotNull(context, "context");
            if (context.CheckCommandContextForItemCount(1))
            {
                var parameters = new NameValueCollection();
                parameters["items"] = this.SerializeItems(context.Items);
                Context.ClientPage.Start(this, "Run", parameters);
            }
        }

        /// <summary>
        /// Query State
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <returns>
        /// Command State
        /// </returns>
        public override CommandState QueryState(CommandContext context)
        {
            Item item = context.Items[0];
            if (item.IsNotNull() && item.Fields[Constants.BucketableField].IsNotNull())
            {
                if (!((CheckboxField)item.Fields[Constants.BucketableField]).Checked)
                {
                    return CommandState.Disabled;
                }

                if (((CheckboxField)item.Fields[Constants.BucketableField]).Checked)
                {
                    return CommandState.Enabled;
                }

                if (!this.HasField(item, ((CheckboxField)item.Fields[Constants.BucketableField]).InnerField.ID))
                {
                    return CommandState.Hidden;
                }

                if (!CanWriteField(item, ((CheckboxField)item.Fields[Constants.BucketableField]).InnerField.ID))
                {
                    return CommandState.Disabled;
                }
            }

            return base.QueryState(context);
        }

        /// <summary>
        /// Run Command on another Thread
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>
        protected void Run(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            if (SheerResponse.CheckModified())
            {
                if (args.IsNotNull())
                {
                    var items = DeserializeItems(args.Parameters["items"]);
                    var item = items[0];
                    using (new EditContext(item, SecurityCheck.Disable))
                    {
                        SearchHelper.SetTemplateAsBucketable(items);
                    }

                    Log.Info(item.ID + " Template can now be placed into Buckets", this);
                }
            }
        }
    }
}
