namespace Sitecore.ItemBucket.Kernel.Forms.BucketLinkForm
{
    using System;
    using Sitecore.Data;
    using Sitecore.Data.Items;
    using Sitecore.Diagnostics;
    using Sitecore.Globalization;
    using Sitecore.IO;
    using Sitecore.Resources.Media;
    using Sitecore.Web;
    using Sitecore.Web.UI.HtmlControls;
    using Sitecore.Web.UI.Pages;
    using Sitecore.Web.UI.Sheer;
    using Sitecore.Web.UI.WebControls;

    internal class BucketLinkForm : DialogForm
    {
        // Fields
        protected DataContext InternalLinkDataContext;
        protected TreeviewEx InternalLinkTreeview;
        protected DataContext MediaDataContext;
        protected TreeviewEx MediaTreeview;

        /// <summary>
        /// Handle Message
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public override void HandleMessage(Message message)
        {
            Assert.ArgumentNotNull(message, "message");
            if (message.Name == "item:load")
            {
                this.LoadItem(message);
            }
            else
            {
                base.HandleMessage(message);
            }
        }

        private void LoadItem(Message message)
        {
            Assert.ArgumentNotNull(message, "message");
            var selectionItem = this.MediaTreeview.GetSelectionItem();
            var language = Context.Language;
            if (selectionItem != null)
            {
                language = selectionItem.Language;
            }

            var item = Client.ContentDatabase.GetItem(ID.Parse(message["id"]), language);
            if (item != null)
            {
                this.MediaDataContext.SetFolder(item.Uri);
                this.MediaTreeview.SetSelectedItem(item);
            }
        }

        /// <summary>
        /// On Cancel
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        protected override void OnCancel(object sender, EventArgs args)
        {
            Assert.ArgumentNotNull(sender, "sender");
            Assert.ArgumentNotNull(args, "args");
            if (this.Mode == "webedit")
            {
                base.OnCancel(sender, args);
            }
            else
            {
                SheerResponse.Eval("scCancel()");
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
          
            if (!Context.ClientPage.IsEvent)
            {
                this.Mode = WebUtil.GetQueryString("mo");
                try
                {
                    this.InternalLinkDataContext.GetFromQueryString();
                }
                catch
                {
                }

                try
                {
                    this.MediaDataContext.GetFromQueryString();
                }
                catch
                {
                }

                var queryString = WebUtil.GetQueryString("fo");
                if (queryString.Length > 0)
                {
                    if (queryString.EndsWith(".aspx", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!queryString.StartsWith("/sitecore"))
                        {
                            queryString = FileUtil.MakePath("/sitecore/content", queryString, '/');
                        }

                        if (queryString.EndsWith(".aspx"))
                        {
                            queryString = StringUtil.Left(queryString, queryString.Length - 5);
                        }

                        this.InternalLinkDataContext.Folder = queryString;
                    }
                    else if (ShortID.IsShortID(queryString))
                    {
                        queryString = ShortID.Parse(queryString).ToID().ToString();
                        var item = Client.ContentDatabase.GetItem(queryString);
                        if (item != null)
                        {
                            if (!item.Paths.IsMediaItem)
                            {
                                this.InternalLinkDataContext.Folder = queryString;
                            }
                            else
                            {
                                this.MediaDataContext.Folder = queryString;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// On Ok
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        protected override void OnOK(object sender, EventArgs args)
        {
            Assert.ArgumentNotNull(sender, "sender");
            Assert.ArgumentNotNull(args, "args");
        }

        /// <summary>
        /// Gets or sets Mode.
        /// </summary>
        protected string Mode
        {
            get
            {
                var str = StringUtil.GetString(this.ServerProperties["Mode"]);
                if (!string.IsNullOrEmpty(str))
                {
                    return str;
                }

                return "shell";
            }

            set
            {
                Assert.ArgumentNotNull(value, "value");
                this.ServerProperties["Mode"] = value;
            }
        }
    }
}
