using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Shell;
using Sitecore.Web;

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
    internal class FullScreen : Command
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
            return "javascript:scContent.toggleFolders();javascript:scContent.fullScreen();javascript:scContent.ribbonNavigatorButtonDblClick(Ribbon_Strip_ViewStrip, null, Ribbon_Strip_ViewStrip.id);";

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
