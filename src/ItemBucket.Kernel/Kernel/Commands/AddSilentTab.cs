// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AddSilentTab.cs" company="Sitecore">
//   Sitecore
// </copyright>
// <summary>
//   Add a new Content Editor Tab to Content Editor so that users can search for hidden content
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.ItemBucket.Kernel.Commands
{
    using Sitecore.Diagnostics;
    using Sitecore.Globalization;
    using Sitecore.ItemBucket.Kernel.Kernel.Commands;
    using Sitecore.Resources;
    using Sitecore.Shell.Framework.Commands;
    using Sitecore.Text;
    using Sitecore.Web;
    using Sitecore.Web.UI.Framework.Scripts;
    using Sitecore.Web.UI.Sheer;

    /// <summary>
    /// Add a new Content Editor Tab to Content Editor so that users can search for hidden content
    /// </summary>
    internal class AddSilentTab : BaseCommand
    {
        #region Public Overrides
        
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
                if (WebUtil.GetFormValue("scEditorTabs").Contains("contenteditor:launchtab") && WebUtil.GetFormValue("scEditorTabs").Contains(itemId))
                {
                    SheerResponse.Eval("scContent.onEditorTabClick(null, null, '" + itemId + "')");
                }
                else
                {
                    var urlString = new UrlString(Util.Constants.ContentEditorRawUrlAddress);
                    urlString.Add(Util.Constants.OpenItemEditorQueryStringKeyName, itemId);
                    context.Items[0].Uri.AddToUrlString(urlString);
                    UIUtil.AddContentDatabaseParameter(urlString);
                    urlString.Add(Util.Constants.ModeQueryStringKeyName, "preview");
                    AddLatestVersionToUrlString(urlString, itemId);
                    SheerResponse.Eval(new ShowEditorTab { Command = "contenteditor:launchtab", Header = Translate.Text(Context.ContentDatabase.GetItem(itemId).Name), Icon = Images.GetThemedImageSource("Applications/16x16/text_view.png"), Url = urlString.ToString(), Id = itemId, Closeable = true, Activate = false }.ToString());
                }
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

        #endregion

        #region Private Methods
        /// <summary>
        /// Overide Sitecore choosing which version to open and get the latest instead
        /// </summary>
        /// <param name="urlString">Raw Url String</param>
        /// <param name="itemId">Id of the Item that will have the its version checked</param>
        private static void AddLatestVersionToUrlString(UrlString urlString, string itemId)
        {
            urlString.Remove(Util.Constants.VersionQueryStringKeyName);
            urlString.Add(Util.Constants.VersionQueryStringKeyName, Context.ContentDatabase.GetItem(itemId).Versions.GetLatestVersion().Version.ToString());
        }
        #endregion
    }
}
