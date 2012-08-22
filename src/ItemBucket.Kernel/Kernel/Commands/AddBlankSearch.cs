using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Configuration;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.ItemBucket.Kernel.Kernel.Util;
using Sitecore.Resources;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Text;
using Sitecore.Web;
using Sitecore.Web.UI.Framework.Scripts;
using Sitecore.Web.UI.Sheer;



        // --------------------------------------------------------------------------------------------------------------------
// <copyright file="AddTab.cs" company="Sitecore">
//   Sitecore
// </copyright>
// <summary>
//   Add a new Content Editor Tab to Content Editor so that users can search for hidden content
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Sitecore.Data.Managers;

namespace Sitecore.ItemBucket.Kernel.Commands
{
    using Sitecore.Configuration;
    using Sitecore.Diagnostics;
    using Sitecore.Globalization;
    using Sitecore.ItemBucket.Kernel.Kernel.Commands;
    using Sitecore.ItemBucket.Kernel.Kernel.Util;
    using Sitecore.Resources;
    using Sitecore.Shell.Framework.Commands;
    using Sitecore.Text;
    using Sitecore.Web;
    using Sitecore.Web.UI.Framework.Scripts;
    using Sitecore.Web.UI.Sheer;

    /// <summary>
    /// Add a new Content Editor Tab to Content Editor so that users can search for hidden content
    /// </summary>
    internal class AddBlankSearch : BaseCommand
    {
        /// <summary>
        /// Add a new Content Editor Tab to Content Editor so that users can search for hidden content
        /// </summary>
        /// <param name="context">Context of Call</param>
        public override void Execute(CommandContext context)
        {
            Assert.ArgumentNotNull(context, "context");
            if (context.Items.Length == 1)
            {
                var itemId = context.Parameters[0];
                //if (WebUtil.GetFormValue("scEditorTabs").Contains("contenteditor:launchtab") && WebUtil.GetFormValue("scEditorTabs").Contains(itemId))
                //{
                //    SheerResponse.Eval("scContent.onEditorTabClick(null, null, '" + itemId + "')");
                //}
                //else
                //{
                
                      var urlString = new UrlString("/sitecore%20modules/Shell/Sitecore/ItemBuckets/ShowResult.aspx");
                      urlString.Add(Util.Constants.OpenItemEditorQueryStringKeyName, itemId);
                 
                      context.Items[0].Uri.AddToUrlString(urlString);
                      UIUtil.AddContentDatabaseParameter(urlString);
                      urlString.Add(Util.Constants.ModeQueryStringKeyName, "preview");
                      urlString.Add(Util.Constants.RibbonQueryStringKeyName, "{D3A2D76F-02E6-49DE-BE90-D23C9771DC8D}");
                      var language = context.Parameters["la"].IsNull() ? Sitecore.Context.Language.CultureInfo.TwoLetterISOLanguageName : context.Parameters["la"];
                      urlString.Add("la", language);
                  
        //SheerResponse.Eval(new ShowEditorTab { Command = "contenteditor:launchtab", Header = "Another Search", Icon = Images.GetThemedImageSource("Applications/16x16/text_view.png"), Url = urlString.ToString(), Id = Id, Closeable = true, Activate = true }.ToString());

                    SheerResponse.Eval(new ShowEditorTab { Command = "contenteditor:launchblanktab", Header = "Search " + DateTime.Now.ToLongTimeString(), Icon = Images.GetThemedImageSource("Applications/16x16/text_view.png"), Url = urlString.ToString(), Id = new Random().Next(0, 99999999).ToString(), Closeable = true, Activate = true }.ToString());
               // }
            }
        }

        /// <summary>
        /// Determins when the table will be disabled or not
        /// </summary>
        /// <param name="context">Context of Call</param>
        /// <returns>Command State</returns>
        public override CommandState QueryState(CommandContext context)
        {
            Assert.ArgumentNotNull(context, "context");
            if (context.Items.Length != 1)
            {
                return CommandState.Disabled;
            }

            var item = context.Items[0];
            return !this.HasField(item, FieldIDs.LayoutField) ? CommandState.Hidden : base.QueryState(context);
        }

        

        
    }
}
