// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MarkImageFieldAsSearchResultsImage.cs" company="Sitecore">
//   Sitecore
// </copyright>
// <summary>
//   Defines the MarkImageFieldAsSearchResultsImage type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Sitecore.SecurityModel;

namespace Sitecore.ItemBucket.Kernel.Commands
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using Sitecore.Data.Fields;
    using Sitecore.Data.Items;
    using Sitecore.Diagnostics;
    using Sitecore.Globalization;
    using Sitecore.ItemBucket.Kernel.Kernel.Util;
    using Sitecore.Shell.Framework.Commands;
    using Sitecore.Web.UI.Sheer;

    /// <summary>
    /// Mark Image Field to show in Search Results
    /// </summary>
    internal class MarkImageFieldAsSearchResultsImage : Command
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
        /// Get the Command Header
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <param name="header">
        /// The header.
        /// </param>
        /// <returns>
        /// Header Text
        /// </returns>
        public override string GetHeader(CommandContext context, string header)
        {
            if (!context.CheckCommandContextForItemCount(1))
            {
                return base.GetHeader(context, header);
            }

            var item = context.Items[0];
            if (item.IsNotNull() && item.Fields[Util.Constants.IsSearchImage].IsNotNull())
            {
                if (!((CheckboxField)item.Fields[Util.Constants.IsSearchImage]).Checked)
                {
                    return Translate.Text("Image Result Field");
                }
            }

            return Translate.Text("Remove Image Result Field");
        }

        /// <summary>
        /// Get the Query State
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <returns>
        /// Command State
        /// </returns>
        public override CommandState QueryState(CommandContext context)
        {
            Assert.ArgumentNotNull(context, "context");
            if (context.CheckCommandContextForItemCount(0))
            {
                return CommandState.Disabled;
            }

            var item = context.Items[0];
            if (item.IsNotNull())
            {
                var isSearchImageFieldId = ((CheckboxField)item.Fields[Util.Constants.IsSearchImage]).InnerField.ID;
                if (!this.HasField(item, isSearchImageFieldId))
                {
                    return CommandState.Hidden;
                }

                if (item.Appearance.ReadOnly)
                {
                    return CommandState.Disabled;
                }

                if (!item.Access.CanWrite())
                {
                    return CommandState.Disabled;
                }

                if (IsLockedByOther(item))
                {
                    return CommandState.Disabled;
                }

                if (!CanWriteField(item, isSearchImageFieldId))
                {
                    return CommandState.Disabled;
                }

                if (item.Fields["Type"].Value != Util.Constants.ImageFieldType)
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
                    if ((item.TemplateID == TemplateIDs.TemplateField) && (item.Fields["Type"].Value == Util.Constants.ImageFieldType))
                    {
                        MarkAsSearchImageFieldInSearchResults(items);
                    }
                }
            }
        }

        /// <summary>
        /// Mark the item field as showing in the search results
        /// </summary>
        /// <param name="items">
        /// The items.
        /// </param>
        private static void MarkAsSearchImageFieldInSearchResults(IList<Item> items)
        {
            var item = items[0];
            using (new EditContext(item, SecurityCheck.Disable))
            {
                ((CheckboxField)item.Fields[Util.Constants.IsSearchImage]).Checked = !((CheckboxField)item.Fields[Util.Constants.IsSearchImage]).Checked;
            }
        }
    }
}
