using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using HtmlAgilityPack;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.ItemBucket.Kernel.Kernel.Util;
using Sitecore.Links;
using Sitecore.Shell.Applications.WebEdit.Commands;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Text;
using Sitecore.Web.UI.Sheer;

namespace Sitecore.ItemBucket.Kernel.Kernel.Forms.WebEdit
{
    public class Search : WebEditCommand
    {
        // Methods
        public override void Execute(CommandContext context)
        {
            Assert.ArgumentNotNull(context, "context");
            Context.ClientPage.Start(this, "Run", context.Parameters);
        }

        public override string GetHeader(CommandContext context, string header)
        {
            return "Search";
            //Assert.ArgumentNotNull(context, "context");
            //Assert.ArgumentNotNull(header, "header");
            //string str = base.GetHeader(context, header);
            //if (Settings.WebEdit.ShowNumberOfLockedItemsOnButton)
            //{
            //    Item[] itemArray = Client.ContentDatabase.SelectItems("fast://*[@__lock='%\"" + Context.User.Name + "\"%']");
            //    int length = 0;
            //    if (itemArray != null)
            //    {
            //        length = itemArray.Length;
            //    }
            //    if (length > 0)
            //    {
            //        object obj2 = str;
            //        str = string.Concat(new object[] { obj2, " (", length, ")" });
            //    }
            //}
            //return str;
        }

        protected static void Run(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            if (SheerResponse.CheckModified())
            {
                if (args.IsPostBack)
                {
                    SheerResponse.Eval("window.top.location.href=window.top.location.href");
                    var itemId = ParseForAttribute(args.Result, "id");
                    Item item = Sitecore.Context.ContentDatabase.GetItem(itemId);
                    if (item.IsNotNull())
                    {
                        var url =
                            new UrlString("http://" + HttpContext.Current.Request.Url.Host +
                                          LinkManager.GetItemUrl(item).Replace("/sitecore/shell", ""));
                        WebEditCommand.Reload(url);
                    }

                }
                else
                {
                    SheerResponse.ShowModalDialog(
                        new UrlString("/sitecore/shell/Applications/Dialogs/Bucket%20link.aspx?").ToString(), "1000", "700", "", true);
                    args.WaitForPostBack();
                }
            }
        }

        private static string ParseForAttribute(string value, string attibuteName)
        {
            
            var doc = new HtmlDocument();
            doc.LoadHtml(value);
            var node = doc.DocumentNode.SelectSingleNode("link");
            if (node.IsNotNull())
            {
                foreach (HtmlAttribute attr in node.Attributes)
                {
                    if (attr.Name == attibuteName)
                    {
                        return attr.Value;
                    }
                }
            }
            return string.Empty;
        }
    }
}
