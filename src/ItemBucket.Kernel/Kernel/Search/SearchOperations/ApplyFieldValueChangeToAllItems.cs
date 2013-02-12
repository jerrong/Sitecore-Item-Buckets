namespace Sitecore.ItemBucket.Kernel.Kernel.Search.SearchOperations
{
    using System.Collections.Generic;
    using System.Linq;

    using Sitecore.Diagnostics;
    using Sitecore.ItemBucket.Kernel.ItemExtensions.Axes;
    using Sitecore.ItemBucket.Kernel.Kernel.Util;
    using Sitecore.Shell.Applications.WebEdit.Commands;
    using Sitecore.Shell.Framework.Commands;
    using Sitecore.Text;
    using Sitecore.Web.Configuration;
    using Sitecore.Web.UI.Sheer;
    using Sitecore.Shell.Framework;
    using Sitecore.Shell.Data;
    using Sitecore.Data.Items;

    internal class ApplyFieldValueChangeToAllItems : WebEditCommand
    {
        private List<SearchStringModel> ExtractSearchQuery(string searchQuery)
        {
            var searchStringModels = new List<SearchStringModel>();
            searchQuery = searchQuery.Replace("text:;", string.Empty);
            var terms = searchQuery.Split(';');
            for (var i = 0; i < terms.Length; i++)
            {
                if (!string.IsNullOrEmpty(terms[i]))
                {
                    searchStringModels.Add(new SearchStringModel
                    {
                        Type = terms[i].Split(':')[0],
                        Value = terms[i].Split(':')[1]
                    });
                }

                //Because of the String that is passed through I have to skip twice
            }

            return searchStringModels;
        }

        public override void Execute(CommandContext context)
        {
            Assert.ArgumentNotNull(context, "context");

            var searchStringModel = this.ExtractSearchQuery(context.Parameters.GetValues("url")[0].Replace("\"", string.Empty));
            int hitsCount;
            var listOfItems = context.Items[0].Search(searchStringModel, out hitsCount).ToList();
            Assert.IsNotNull(listOfItems.First(), "item");

            Item item = listOfItems.First().GetItem();
            Open(item.ID.ToString(), Sitecore.Context.Language.Name, item.Versions.GetLatestVersion().Version.Number.ToString());
        }
        protected void Open(string id, string language, string version)
        {
            Assert.ArgumentNotNull(id, "id");
            Assert.ArgumentNotNull(language, "language");
            Assert.ArgumentNotNull(version, "version");
            string sectionID = RootSections.GetSectionID(id);
            UrlString str2 = new UrlString(ItemBucket.Kernel.Util.Constants.ContentEditorRawUrlAddress);
            str2.Append("ro", sectionID);
            str2.Append("fo", id);
            str2.Append("id", id);
            str2.Append("la", language);
            str2.Append("vs", version);
            str2.Append("mo", "popup");

            SheerResponse.ShowModalDialog(str2.ToString(), "960", "600", string.Empty, false);           
        }
        private static string GetFeatures()
        {
            var str = "location=0,menubar=0,status=0,toolbar=0,resizable=1";
            var device = Context.Device;
            if (device != null)
            {
                var capabilities = device.Capabilities as SitecoreClientDeviceCapabilities;
                if (capabilities == null)
                {
                    return str;
                }

                if (capabilities.RequiresScrollbarsOnWindowOpen)
                {
                    str = str + ",scrollbars=1,dependent=1";
                }
            }

            return str;
        }
    }
}
