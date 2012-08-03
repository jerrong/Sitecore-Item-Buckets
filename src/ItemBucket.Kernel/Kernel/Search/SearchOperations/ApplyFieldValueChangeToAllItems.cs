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
            var str = new UrlString(ItemBucket.Kernel.Util.Constants.ContentEditorRawUrlAddress);
            var searchStringModel = this.ExtractSearchQuery(context.Parameters.GetValues("url")[0].Replace("\"", string.Empty));
            int hitsCount;
            var listOfItems = context.Items[0].Search(searchStringModel, out hitsCount).ToList();
            Assert.IsNotNull(context.Items[0], "item");
            foreach (var item1 in listOfItems.Select(sitecoreItem => sitecoreItem.GetItem()))
            {
                item1.Editing.BeginEdit();
                item1.Editing.EndEdit();
            }

            str[ItemBucket.Kernel.Util.Constants.OpenItemEditorQueryStringKeyName] = listOfItems.First().GetItem().ID.ToString();
            str[ItemBucket.Kernel.Util.Constants.ModeQueryStringKeyName] = "preview";
            var features = GetFeatures();
            SheerResponse.Eval(string.Concat(new object[] { "window.open('", str, "', 'SitecoreWebEditEditor', '", features, "')" }));
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
