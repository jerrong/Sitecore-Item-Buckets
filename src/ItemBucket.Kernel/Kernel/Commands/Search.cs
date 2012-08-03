// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Search.cs" company="Sitecore">
//   Sitecore
// </copyright>
// <summary>
//   Defines the Search type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.ItemBucket.Kernel.Commands
{
    using System.Collections.Specialized;
    using System.Linq;
    using Sitecore.Diagnostics;
    using Sitecore.ItemBucket.Kernel.ItemExtensions.Axes;
    using Sitecore.ItemBucket.Kernel.Kernel.Util;
    using Sitecore.Shell.Framework.Commands;
    using Sitecore.Web.UI.Sheer;

    /// <summary>
    /// Add a Search Tab to an Item
    /// </summary>
    internal class Search : Command
    {
        /// <summary>
        /// Detect if lots of items are being added to the Index
        /// </summary>
        public delegate void BucketBulkItemAddedEventSubscriber();

        /// <summary>
        /// Execute Process
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
        /// Run the Command
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>
        protected void Run(ClientPipelineArgs args)
        {
            var context = DeserializeItems(args.Parameters["items"]);
            Assert.IsNotNull(context[0], "Context items list is null");
            if (context.Any())
            {
                var contextItem = context[0];
                Util.SearchHelper.AddSearchTab(contextItem, contextItem.GetEditors());
                Log.Info(contextItem.ID + " Item now has a Search Panel", this);
                SheerResponse.Eval("scContent.onEditorTabClick(null, null, '" + Util.Constants.SearchEditor + "')");
            }
        }
    }
}