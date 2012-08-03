// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SetFieldAsTag.cs" company="Sitecore">
//   Sitecore
// </copyright>
// <summary>
//   Defines the SetFieldAsTag type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Sitecore.SecurityModel;

namespace Sitecore.ItemBucket.Kernel.Commands
{
    using System.Collections.Specialized;
    using Sitecore.Data.Fields;
    using Sitecore.Data.Items;
    using Sitecore.Diagnostics;
    using Sitecore.Globalization;
    using Sitecore.ItemBucket.Kernel.Kernel.Util;
    using Sitecore.Shell.Framework.Commands;
    using Sitecore.Web.UI.Sheer;

    /// <summary>
    /// Set Field as a Facet
    /// </summary>
    internal class SetFieldAsTag : Command
    {
        #region Public Override Methods
        /// <summary>
        /// Sets any field as being a facet in which Sitecore will categorise results on
        /// </summary>
        /// <param name="context">Command Context</param>
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
        /// Get the Header Text
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
            if (item.IsNotNull() && item.Fields[Util.Constants.IsTag].IsNotNull())
            {
                if (!((CheckboxField)item.Fields[Util.Constants.IsTag]).Checked)
                {
                    return Translate.Text("Make Tag");
                }
            }

            return Translate.Text("Unmake Tag");
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
            Assert.ArgumentNotNull(context, "context");
            if (context.CheckCommandContextForItemCount(0))
            {
                return CommandState.Disabled;
            }
            var item = context.Items[0];
            if (item.IsNotNull() && item.Fields[Util.Constants.IsTag].IsNotNull())
            {
                var isFacet = ((CheckboxField)item.Fields[Util.Constants.IsTag]).InnerField.ID;
                if (!this.HasField(item, isFacet))
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

                if (!CanWriteField(item, isFacet))
                {
                    return CommandState.Disabled;
                }
            }

            return base.QueryState(context);
        }

        #endregion

        /// <summary>
        /// Run Field Command
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>
        protected void Run(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            if (SheerResponse.CheckModified())
            {
                var items = DeserializeItems(args.Parameters["items"]);
                var item = items[0];
                if (item.TemplateID == TemplateIDs.TemplateField)
                {
                    using (new EditContext(item, SecurityCheck.Disable))
                    {
                        if (item.Fields[Util.Constants.IsTag].IsNotNull())
                        {
                            ((CheckboxField)item.Fields[Util.Constants.IsTag]).Checked = !((CheckboxField)item.Fields[Util.Constants.IsTag]).Checked;
                            Log.Info(item + " Field has been marked as a tag and will show up in Facet Results", this);
                        }
                    }
                }
            }
        }
    }
}
