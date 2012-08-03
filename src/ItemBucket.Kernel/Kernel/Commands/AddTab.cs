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
    internal class AddTab : BaseCommand
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
                if (WebUtil.GetFormValue("scEditorTabs").Contains("contenteditor:launchtab") && WebUtil.GetFormValue("scEditorTabs").Contains(itemId))
                {
                    SheerResponse.Eval("scContent.onEditorTabClick(null, null, '" + itemId + "')");
                }
                else
                {
                    var urlString = new UrlString("/sitecore/shell/sitecore/content/Applications/Content%20Editor.aspx");
                    urlString.Add(Util.Constants.OpenItemEditorQueryStringKeyName, itemId);
                    TrackOpenTab(context);
                    context.Items[0].Uri.AddToUrlString(urlString);
                    UIUtil.AddContentDatabaseParameter(urlString);
                    urlString.Add(Util.Constants.ModeQueryStringKeyName, "preview");
                    urlString.Add(Util.Constants.RibbonQueryStringKeyName, "{D3A2D76F-02E6-49DE-BE90-D23C9771DC8D}");
                    var language = context.Parameters["la"].IsNull() ? Sitecore.Context.Language.CultureInfo.TwoLetterISOLanguageName : context.Parameters["la"];
                    urlString.Add("la", language);
                    AddLatestVersionToUrlString(urlString, itemId, language);
                    SheerResponse.Eval(new ShowEditorTab { Command = "contenteditor:launchtab", Header = Translate.Text(Context.ContentDatabase.GetItem(itemId).Name), Icon = Images.GetThemedImageSource("Applications/16x16/text_view.png"), Url = urlString.ToString(), Id = itemId, Closeable = true, Activate = Util.Constants.SettingsItem.Fields[Util.Constants.OpenSearchResult].Value == "New Tab Not Selected" ? false : true }.ToString());
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

        #region Private Methods

        /// <summary>
        /// Overide Sitecore choosing which version to open and get the latest instead
        /// </summary>
        /// <param name="urlString">Raw Url String</param>
        /// <param name="itemId">Id of the Item that will have the its version checked</param>
        private static void AddLatestVersionToUrlString(UrlString urlString, string itemId, string language)
        {
            urlString.Remove(Util.Constants.VersionQueryStringKeyName);
            urlString.Add(Util.Constants.VersionQueryStringKeyName, Context.ContentDatabase.GetItem(itemId, LanguageManager.GetLanguage(language)).Versions.GetLatestVersion().Version.ToString());
        }
        #endregion

        /// <summary>
        /// Tracks the opened tabs in the session
        /// </summary>
        /// <param name="context">Context of the Command</param>
        private static void TrackOpenTab(CommandContext context)
        {
            if (ClientContext.GetValue("RecentlyOpenedTabs").IsNull())
            {
                ClientContext.SetValue("RecentlyOpenedTabs", string.Empty);
            }

            if (!ClientContext.GetValue("RecentlyOpenedTabs").ToString().Contains("|" + context.Parameters[0] + "|"))
            {
                ClientContext.SetValue("RecentlyOpenedTabs", ClientContext.GetValue("RecentlyOpenedTabs") + "|" + context.Parameters[0] + "|");
            }
        }
    }
}
