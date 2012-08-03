// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PasteToField.cs" company="Sitecore">
//   Sitecore
// </copyright>
// <summary>
//   Defines the PasteToField type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.ItemBucket.Kernel.Kernel.Commands
{
    using System.Collections.Specialized;
    using Sitecore.Data.Items;
    using Sitecore.Diagnostics;
    using Sitecore.Shell.Framework.Commands;
    using Sitecore.Web.UI.Sheer;

    /// <summary>
    /// Paste the Content of the Clipboard into a Field
    /// </summary>
    internal class PasteToField : Command
    {
        /// <summary>
        /// Execute the Paste
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        public override void Execute(CommandContext context)
        {
            if (context.Items.Length == 1)
            {
                var item = context.Items[0];
                var parameters = new NameValueCollection();
                parameters["id"] = item.ID.ToString();
                parameters["searchString"] = context.Parameters.GetValues("url")[0].Replace("\"", string.Empty);
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
            Error.AssertObject(context, "context");
            if (context.Items.Length != 1)
            {
                return CommandState.Hidden;
            }

            if (!UIUtil.IsIE())
            {
                return CommandState.Hidden;
            }

            return base.QueryState(context);
        }

        /// <summary>
        /// Run Paste in another Thread
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>
        protected void Run(ClientPipelineArgs args)
        {
            // TODO: Currently not implemented
            Item item = Sitecore.Context.ContentDatabase.GetItem(args.Parameters["id"]);
            var pasteContent = Sitecore.Context.ClientData.GetValue("CurrentPaste");
        }
    }
}
