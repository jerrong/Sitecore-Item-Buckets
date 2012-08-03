namespace Sitecore.ItemBucket.Kernel.FieldTypes
{
    using System;
    using System.Collections.Specialized;
    using Sitecore.Diagnostics;
    using Sitecore.IO;
    using Sitecore.ItemBucket.Kernel.Util;
    using Sitecore.Shell.Applications.ContentEditor;
    using Sitecore.Text;
    using Sitecore.Web.UI.HtmlControls;
    using Sitecore.Web.UI.Sheer;

    /// <summary>
    /// Bucket Link Control for Searching Large List of Items
    /// </summary>
    public sealed class BucketLink : Edit, IContentField
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BucketLink"/> class.
        /// </summary>
        public BucketLink()
        {
            this.Class = "scContentControl";
            this.Activation = true;
        }

        /// <summary>
        /// Clear Link
        /// </summary>
        private void ClearLink()
        {
            if (this.Value.Length > 0)
            {
                this.SetModified();
            }

            this.XmlValue = new XmlValue(string.Empty, "link");
            this.Value = string.Empty;
            Sitecore.Context.ClientPage.ClientResponse.SetAttribute(this.ID, "value", string.Empty);
        }

        /// <summary>
        /// Method Invoked via Reflection on Request -> DO NOT REMOVE
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>
        private void InsertLink(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            if (args.IsPostBack)
            {
                if (!string.IsNullOrEmpty(args.Result) && (args.Result != "undefined"))
                {
                    this.XmlValue = new XmlValue(args.Result, "link");
                    this.Value = this.XmlValue.GetAttribute("url");
                    this.SetModified();
                    Sitecore.Context.ClientPage.ClientResponse.SetAttribute(this.ID, "value", this.Value);
                    SheerResponse.Eval("scContent.startValidators()");
                }
            }
            else
            {
                var str = new UrlString(args.Parameters["url"]);
                str.Append("va", this.XmlValue.ToString());
                str.Append("ro", this.Source);
                Sitecore.Context.ClientPage.ClientResponse.ShowModalDialog(str.ToString(), "1000", "700", "", true);
                args.WaitForPostBack();
            }
        }

        /// <summary>
        /// Follow
        /// </summary>
        private void Follow()
        {
            var xmlValue = this.XmlValue;
            var attribute = xmlValue.GetAttribute("linktype");
            if (attribute != null)
            {
                if (attribute != "internal" && attribute != "media")
                {
                    if (attribute != "external" && attribute != "mailto")
                    {
                        if (attribute != "anchor")
                        {
                            if (attribute == "javascript")
                            {
                                SheerResponse.Alert("You cannot follow a Javascript link.", new string[0]);
                            }

                            return;
                        }

                        SheerResponse.Alert("You cannot follow an Anchor link.", new string[0]);
                        return;
                    }
                }
                else
                {
                    var str2 = xmlValue.GetAttribute("id");
                    if (!string.IsNullOrEmpty(str2))
                    {
                        Sitecore.Context.ClientPage.SendMessage(this, "item:load(id=" + str2 + ")");
                    }

                    return;
                }

                var str3 = xmlValue.GetAttribute("url");
                if (!string.IsNullOrEmpty(str3))
                {
                    SheerResponse.Eval("window.open('" + str3 + "', '_blank', 'width=700','height=500' )");
                }
            }
        }

        /// <summary>
        /// Get Value of Xml
        /// </summary>
        /// <returns>
        /// Xml Value
        /// </returns>
        public string GetValue()
        {
            return this.XmlValue.ToString();
        }

        /// <summary>
        /// Handle Client Side Events
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public override void HandleMessage(Message message)
        {
            Assert.ArgumentNotNull(message, "message");
            base.HandleMessage(message);
            if (message["id"] == this.ID)
            {
                switch (message.Name)
                {
                    case "contentlink:internallink":
                        this.Insert("/sitecore/shell/Applications/Dialogs/Internal link.aspx");
                        return;

                    case "contentlink:bucketlink":
                        this.Insert("/sitecore/shell/Applications/Dialogs/Bucket link.aspx");
                        return;

                    case "contentlink:media":
                        this.Insert("/sitecore/shell/Applications/Dialogs/Media link.aspx");
                        return;

                    case "contentlink:externallink":
                        this.Insert("/sitecore/shell/Applications/Dialogs/External link.aspx");
                        return;

                    case "contentlink:anchorlink":
                        this.Insert("/sitecore/shell/Applications/Dialogs/Anchor link.aspx");
                        return;

                    case "contentlink:mailto":
                        this.Insert("/sitecore/shell/Applications/Dialogs/Mail link.aspx");
                        return;

                    case "contentlink:javascript":
                        this.Insert("/sitecore/shell/Applications/Dialogs/Javascript link.aspx");
                        return;

                    case "contentlink:follow":
                        this.Follow();
                        return;

                    case "contentlink:clear":
                        this.ClearLink();
                        return;
                }
            }
        }

        /// <summary>
        /// Insert new URL
        /// </summary>
        /// <param name="url">
        /// The url.
        /// </param>
        private void Insert(string url)
        {
            Assert.ArgumentNotNull(url, "url");
            Sitecore.Context.ClientPage.Start(this, "InsertLink", new NameValueCollection { { "url", url } });
        }

        /// <summary>
        /// Load Post Data
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// True or False
        /// </returns>
        protected override bool LoadPostData(string value)
        {
            Assert.ArgumentNotNull(value, "value");
            var flag = base.LoadPostData(value);
            if (flag)
            {
                if (string.IsNullOrEmpty(this.Value))
                {
                    this.ClearLink();
                    return true;
                }

                var xmlValue = this.XmlValue;
                if (this.Value == xmlValue.GetAttribute("url"))
                {
                    return flag;
                }

                xmlValue.SetAttribute("url", this.Value);
                if (xmlValue.GetAttribute("linktype").Length == 0)
                {
                    xmlValue.SetAttribute("linktype", this.Value.IndexOf("://") >= 0 ? "external" : "internal");
                }

                var str = string.Empty;
                if ((xmlValue.GetAttribute("linktype") == "internal") || (xmlValue.GetAttribute("linktype") == "media"))
                {
                    var attribute = xmlValue.GetAttribute("url");
                    if (!string.IsNullOrEmpty(attribute))
                    {
                        if (!attribute.StartsWith("/sitecore", StringComparison.CurrentCultureIgnoreCase) && !IdHelper.IsGuid(attribute))
                        {
                            attribute = FileUtil.MakePath(xmlValue.GetAttribute("linktype") == "internal" ? "/sitecore/content" : "/sitecore/media library", attribute, '/');
                        }

                        var item = Client.ContentDatabase.GetItem(attribute);
                        if (item != null)
                        {
                            str = item.ID.ToString();
                        }
                    }
                }

                xmlValue.SetAttribute("id", str);
                this.XmlValue = xmlValue;
            }

            return flag;
        }

        /// <summary>
        /// Pre Render
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected override void OnPreRender(EventArgs e)
        {
            Assert.ArgumentNotNull(e, "e");
            base.OnPreRender(e);
            this.ServerProperties["Value"] = this.ServerProperties["Value"];
            this.ServerProperties["XmlValue"] = this.ServerProperties["XmlValue"];
            this.ServerProperties["Source"] = this.ServerProperties["Source"];
        }

        /// <summary>
        /// Set Field as Modified
        /// </summary>
        protected override void SetModified()
        {
            base.SetModified();
            if (this.TrackModified)
            {
                Sitecore.Context.ClientPage.Modified = true;
            }
        }

        /// <summary>
        /// Set Value
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        public void SetValue(string value)
        {
            Assert.ArgumentNotNull(value, "value");
            this.XmlValue = new XmlValue(value, "link");
            this.Value = this.XmlValue.GetAttribute("url");
        }

        /// <summary>
        /// Gets or sets Source.
        /// </summary>
        public string Source
        {
            get
            {
                return this.GetViewStateString("Source");
            }

            set
            {
                Assert.ArgumentNotNull(value, "value");
                var str = MainUtil.UnmapPath(value);
                if (str.EndsWith("/"))
                {
                    str = str.Substring(0, str.Length - 1);
                }

                this.SetViewStateString("Source", str);
            }
        }

        /// <summary>
        /// Gets or sets XmlValue.
        /// </summary>
        private XmlValue XmlValue
        {
            get
            {
                return new XmlValue(this.GetViewStateString("XmlValue"), "link");
            }

            set
            {
                Assert.ArgumentNotNull(value, "value");
                this.SetViewStateString("XmlValue", value.ToString());
            }
        }
    }
}
