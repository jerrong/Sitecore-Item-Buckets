using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web.UI.Sheer;

namespace Sitecore.ItemBucket.Kernel.Kernel.Commands
{
    internal class SelectInTree : Command
    {
        /// <summary>
        /// Execute the Paste
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        ///    // Methods
        public override void Execute(CommandContext context)
        {
        }

        /// 
        public override string GetClick(CommandContext context, string click)
        {
            return "item:load(id=" + context.Items[0].ID.ToString() + ")";

        }

        public override CommandState QueryState(CommandContext context)
        {

            return CommandState.Enabled;

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
            //Item item = Sitecore.Context.ContentDatabase.GetItem(args.Parameters["id"]);
            //var pasteContent = Sitecore.Context.ClientData.GetValue("CurrentPaste");


        }
    }
}

