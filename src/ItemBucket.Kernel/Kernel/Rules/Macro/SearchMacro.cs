using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Rules.RuleMacros;
using Sitecore.Shell.Applications.Dialogs.ItemLister;
using Sitecore.Text;
using Sitecore.Web.UI.Sheer;

namespace Sitecore.ItemBucket.Kernel.Kernel.Rules.Macro
{
    public class SearchMacro : IRuleMacro
    {
        // Methods
        public void Execute(XElement element, string name, UrlString parameters, string value)
        {
            Assert.ArgumentNotNull(element, "element");
            Assert.ArgumentNotNull(name, "name");
            Assert.ArgumentNotNull(parameters, "parameters");
            Assert.ArgumentNotNull(value, "value");
            SelectItemOptions options = new SelectItemOptions();
            if (!string.IsNullOrEmpty(value))
            {
                Item item = Client.ContentDatabase.GetItem(value);
                if (item != null)
                {
                    options.SelectedItem = item;
                }
            }
            string str = parameters["root"];
            if (!string.IsNullOrEmpty(str))
            {
                Item item2 = Client.ContentDatabase.GetItem(str);
                if (item2 != null)
                {
                    options.Root = item2;
                }
            }
            string str2 = parameters["selection"];
            if (!string.IsNullOrEmpty(str2))
            {
                options.IncludeTemplatesForSelection = SelectItemOptions.GetTemplateList(str2.Split(new char[] { '|' }));
            }
            string str3 = parameters["display"];
            if (!string.IsNullOrEmpty(str3))
            {
                options.IncludeTemplatesForDisplay = SelectItemOptions.GetTemplateList(str3.Split(new char[] { '|' }));
            }
            options.Title = "Select Item";
            options.Text = "Select the item to use in this rule.";
            options.Icon = "People/16x16/cube_blue.png";
            SheerResponse.ShowModalDialog(options.ToUrlString().ToString(), "1000", "700", "", true);
        }
    }
}
