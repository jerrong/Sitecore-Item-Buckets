namespace Sitecore.ItemBucket.Kernel.FieldTypes
{
    using System;
    using System.Xml;
    using Sitecore.Configuration;
    using Sitecore.Data;
    using Sitecore.Data.Items;
    using Sitecore.Diagnostics;
    using Sitecore.ItemBucket.Kernel.Kernel.Util;
    using Sitecore.ItemBucket.Kernel.Util;
    using Sitecore.Links;
    using Sitecore.Web;
    using Sitecore.Web.UI.HtmlControls;
    using Sitecore.Web.UI.Pages;
    using Sitecore.Web.UI.Sheer;
    using Sitecore.Web.UI.WebControls;
    using Sitecore.Xml;

    /// <summary>
    /// Bucket Link Form
    /// </summary>
    public class BucketLinkForm : DialogForm
    {
        // Fields
        protected Edit Anchor;
        protected Edit Class;
        protected Panel CustomLabel;
        protected Edit CustomTarget;
        protected DataContext InternalLinkDataContext;
        protected Edit Querystring;
        protected Combobox Target;
        protected Edit Text;
        protected Edit ItemLink;
        protected Edit Title;
        protected TreeviewEx Treeview;

        /// <summary>
        /// On Change of the List Box
        /// </summary>
        protected void OnListboxChanged()
        {
            if (this.Target.Value == "Custom")
            {
                this.CustomTarget.Disabled = false;
                this.CustomLabel.Disabled = false;
            }
            else
            {
                this.CustomTarget.Value = string.Empty;
                this.CustomTarget.Disabled = true;
                this.CustomLabel.Disabled = true;
            }
        }

        /// <summary>
        /// On Load
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected override void OnLoad(EventArgs e)
        {
            Assert.ArgumentNotNull(e, "e");
            base.OnLoad(e);
            if (Context.ClientPage.IsEvent)
            {
                return;
            }
            
            this.InternalLinkDataContext.GetFromQueryString();
            this.CustomTarget.Disabled = true;
            this.CustomLabel.Disabled = true;
            var xml = StringUtil.GetString(new[] { WebUtil.GetQueryString("va"), "<link/>" });
            var queryString = WebUtil.GetQueryString("ro");
            var document = XmlUtil.LoadXml(xml);
            if (document == null)
            {
                this.CheckQueryStringLength(queryString);
            }

            var node = document.SelectSingleNode("/link");
            if (node == null)
            {
                this.CheckQueryStringLength(queryString);
            }

            var attribute = XmlUtil.GetAttribute("text", node);
            var str4 = XmlUtil.GetAttribute("anchor", node);
            var str5 = XmlUtil.GetAttribute("target", node);
            var str6 = string.Empty;
            var str7 = XmlUtil.GetAttribute("class", node);
            var str8 = XmlUtil.GetAttribute("querystring", node);
            var str9 = XmlUtil.GetAttribute("title", node);
            var url = XmlUtil.GetAttribute("url", node);
            var str12 = str5;
            if (str12 != null)
            {
                if (str12.IsNotEmpty() && str12 != "_self")
                {
                    if (str12 == "_blank")
                    {
                        str5 = "New";
                    }
                    else
                    {
                        str6 = str5;
                        str5 = "Custom";
                        this.CustomTarget.Background = "window";
                        this.CustomTarget.Disabled = false;
                        this.CustomLabel.Disabled = false;
                    }
                }
                else
                {
                    str5 = "Self";
                }
            }

            this.Text.Value = attribute;
            this.Anchor.Value = str4;
            this.Target.Value = str5;
            this.CustomTarget.Value = str6;
            this.Class.Value = str7;
            this.Querystring.Value = str8;
            this.Title.Value = str9;
            var str11 = XmlUtil.GetAttribute("id", node);
            if (string.IsNullOrEmpty(str11) || !IdHelper.IsGuid(str11))
            {
                this.SetFolderFromUrl(node, url);
            }
            else
            {
                var itemId = new ID(str11);
                if (Client.ContentDatabase.GetItem(itemId) == null)
                {
                    this.SetFolderFromUrl(node, url);
                }
                else
                {
                    var uri = new ItemUri(itemId, Client.ContentDatabase);
                    this.InternalLinkDataContext.SetFolder(uri);
                }
            }
        }

        /// <summary>
        /// Check Query String Length
        /// </summary>
        /// <param name="queryString">
        /// The query string.
        /// </param>
        private void CheckQueryStringLength(string queryString)
        {
            if (queryString.Length > 0)
            {
                this.InternalLinkDataContext.Root = queryString;
            }
        }

        protected void ReturnQuery()
        {
            var str2 = string.Empty;


            Context.ClientPage.ClientResponse.SetDialogValue(this.ItemLink.Value);
            SheerResponse.CloseWindow();
        }

        /// <summary>
        /// Upload Image
        /// </summary>
        protected void UploadImage()
        {
            var str2 = string.Empty;
            var selectionItem = Context.ContentDatabase.GetItem(this.ItemLink.Value);
            if (selectionItem == null)
            {
                Context.ClientPage.ClientResponse.Alert("Select an item.");
                return;
            }

            var path = selectionItem.Paths.Path;
            if (path.StartsWith("/sitecore/content"))
            {
                path = path.Substring(0x11);
            }

            if (LinkManager.AddAspxExtension)
            {
                path = path + ".aspx";
            }

            var str4 = this.Target.Value;
            if (str4 != null)
            {
                str2 = str4 != "Self" ? (str4 == "New" ? "_blank" : this.CustomTarget.Value) : string.Empty;
            }
          
            var str3 = this.Querystring.Value;
            if (str3.StartsWith("?"))
            {
                str3 = str3.Substring(1);
            }

            var packet = new Packet("link", new string[0]);
            SetAttribute(packet, "text", this.Text);
            SetAttribute(packet, "linktype", "internal");
            SetAttribute(packet, "url", path);
            SetAttribute(packet, "anchor", this.Anchor);
            SetAttribute(packet, "querystring", this.Anchor);
            SetAttribute(packet, "title", this.Title);
            SetAttribute(packet, "class", this.Class);
            SetAttribute(packet, "querystring", str3);
            SetAttribute(packet, "target", str2);
            SetAttribute(packet, "id", selectionItem.ID.ToString());
            Context.ClientPage.ClientResponse.SetDialogValue(packet.OuterXml);
            SheerResponse.CloseWindow();
        }

        /// <summary>
        /// OnOk
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        protected override void OnOK(object sender, EventArgs args)
        {
            var str2 = string.Empty;
            Assert.ArgumentNotNull(sender, "sender");
            Assert.ArgumentNotNull(args, "args");
            var selectionItem = Context.ContentDatabase.GetItem(this.ItemLink.Value);
            if (selectionItem == null)
            {
                Context.ClientPage.ClientResponse.Alert("Select an item.");
                return;
            }

            var path = selectionItem.Paths.Path;
            if (path.StartsWith("/sitecore/content"))
            {
                path = path.Substring(0x11);
            }

            if (LinkManager.AddAspxExtension)
            {
                path = path + ".aspx";
            }

            var str4 = this.Target.Value;
            if (str4 != null)
            {
                str2 = str4 != "Self" ? (str4 == "New" ? "_blank" : this.CustomTarget.Value) : string.Empty;
            }
     
            var str3 = this.Querystring.Value;
            if (str3.StartsWith("?"))
            {
                str3 = str3.Substring(1);
            }

            var packet = new Packet("link", new string[0]);
            SetAttribute(packet, "text", this.Text);
            SetAttribute(packet, "linktype", "internal");
            SetAttribute(packet, "url", path);
            SetAttribute(packet, "anchor", this.Anchor);
            SetAttribute(packet, "querystring", this.Anchor);
            SetAttribute(packet, "title", this.Title);
            SetAttribute(packet, "class", this.Class);
            SetAttribute(packet, "querystring", str3);
            SetAttribute(packet, "target", str2);
            SetAttribute(packet, "id", selectionItem.ID.ToString());
            Context.ClientPage.ClientResponse.SetDialogValue(packet.OuterXml);
            base.OnOK(sender, args);
        }

        /// <summary>
        /// Set Attribute
        /// </summary>
        /// <param name="packet">
        /// The packet.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="control">
        /// The control.
        /// </param>
        private static void SetAttribute(Packet packet, string name, Control control)
        {
            Assert.ArgumentNotNull(packet, "packet");
            Assert.ArgumentNotNullOrEmpty(name, "name");
            Assert.ArgumentNotNull(control, "control");
            if (control.Value.Length > 0)
            {
                SetAttribute(packet, name, control.Value);
            }
        }

        /// <summary>
        /// Set Attirbute
        /// </summary>
        /// <param name="packet">
        /// The packet.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        private static void SetAttribute(Packet packet, string name, string value)
        {
            Assert.ArgumentNotNull(packet, "packet");
            Assert.ArgumentNotNullOrEmpty(name, "name");
            Assert.ArgumentNotNull(value, "value");
            packet.SetAttribute(name, value);
        }

        /// <summary>
        /// Set Folder From Url
        /// </summary>
        /// <param name="link">
        /// The link.
        /// </param>
        /// <param name="url">
        /// The url.
        /// </param>
        private void SetFolderFromUrl(XmlNode link, string url)
        {
            if (XmlUtil.GetAttribute("linktype", link) != "internal")
            {
                url = "/sitecore/content" + Settings.DefaultItem;
            }

            if (url.Length == 0)
            {
                url = "/sitecore/content";
            }

            if (!url.StartsWith("/sitecore"))
            {
                url = "/sitecore/content" + url;
            }

            this.InternalLinkDataContext.Folder = url;
        }
    }
}
