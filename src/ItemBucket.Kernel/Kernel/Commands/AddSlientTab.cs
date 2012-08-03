using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.ItemBucket.Kernel.Kernel.Commands;
using Sitecore.Resources;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Text;
using Sitecore.Web;
using Sitecore.Web.UI.Framework.Scripts;
using Sitecore.Web.UI.Sheer;

namespace Sitecore.ItemBucket.Kernel.Commands
{
    class AddSilentTab : BaseCommand
    {
        #region Public Overrides
        /// <summary>
        /// Add a new Content Editor Tab to Content Editor so that users can search for hidden content
        /// </summary>
        /// <returns>Void</returns>
        public override void Execute(CommandContext context)
        {
            Assert.ArgumentNotNull(context, "context");
            if (context.Items.Length == 1)
            {
                if (WebUtil.GetFormValue("scEditorTabs").Contains("contenteditor:launchtab") && WebUtil.GetFormValue("scEditorTabs").Contains(context.Parameters[0]))
                {
                    SheerResponse.Eval("scContent.onEditorTabClick(null, null, '" + context.Parameters[0] + "')");
                }
                else
                {
                    var urlString = new UrlString("/sitecore/shell/sitecore/content/Applications/Content%20Editor.aspx");
                    urlString.Add("fo", context.Parameters[0]);
                    context.Items[0].Uri.AddToUrlString(urlString);
                    UIUtil.AddContentDatabaseParameter(urlString);
                    //Open the new tab without the content tree showing
                    urlString.Add("mo", "preview");
                    AddLatestVersionToUrlString(context, urlString);
                    SheerResponse.Eval(new ShowEditorTab { Command = "contenteditor:launchtab", Header = Translate.Text(Context.ContentDatabase.GetItem(context.Parameters[0]).Name), Icon = Images.GetThemedImageSource("Applications/16x16/text_view.png"), Url = urlString.ToString(), Id = context.Parameters[0], Closeable = true, Activate = false}.ToString());
                    SheerResponse.Eval("scContent.onEditorTabClick('scEditorTabHeaderActive', 'scEditorTabHeaderActive', '" + "{59F53BBB-D1F5-4E38-8EBA-0D73109BB59B}" + "')");
                }
            }
        }

        public override CommandState QueryState(CommandContext context)
        {
            Assert.ArgumentNotNull(context, "context");
            if (context.Items.Length != 1)
            {
                return CommandState.Disabled;
            }
            var item = context.Items[0];
            return !HasField(item, FieldIDs.LayoutField) ? CommandState.Hidden : base.QueryState(context);
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// Overide Sitecore choosing which version to open and get the latest instead
        /// </summary>
        /// <returns>Void</returns>
        private static void AddLatestVersionToUrlString(CommandContext context, UrlString urlString)
        {
            urlString.Remove("vs");
            urlString.Add("vs", Context.ContentDatabase.GetItem(context.Parameters[0]).Versions.GetLatestVersion().
                                    Version.ToString());
        }
        #endregion
    }
}
