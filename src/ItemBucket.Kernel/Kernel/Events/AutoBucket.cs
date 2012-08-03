namespace Sitecore.ItemBucket.Kernel.Events
{
    using System;
    using System.Collections.Specialized;
    using Sitecore.Configuration;
    using Sitecore.Data.Items;
    using Sitecore.Diagnostics;
    using Sitecore.Events;
    using Sitecore.ItemBucket.Kernel.Kernel.Util;
    using Sitecore.ItemBucket.Kernel.Util;
    using Sitecore.Web.UI.Sheer;

    /// <summary>
    /// Auto Bucket Command
    /// </summary>
    internal class AutoBucket
    {
        #region Properties

        /// <summary>
        /// Gets BucketTriggerCount.
        /// </summary>
        private static int BucketTriggerCount
        {
            get
            {
                return Settings.GetIntSetting("BucketTriggerCount", 60);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Execute Auto Bucket
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        public void Execute(object sender, EventArgs args)
        {
            if (Context.ClientPage.IsNotNull())
            {
                var item = Event.ExtractParameter(args, 0) as Item;
                Error.AssertItem(item, "Item");

                if (Web.WebUtil.GetQueryString("mode").IsNullOrEmpty() &&
                    Web.WebUtil.GetQueryString("sc_pagesite").IsNullOrEmpty())
                {
                    if (item.IsNotNull())
                    {
                        if (item.Parent.Children.Count >= BucketTriggerCount && item.Parent.Paths.FullPath.ToLower() != Context.Site.StartPath.ToLower())
                        {
                            if (item.TemplateID != Config.BucketTemplateId
                                && item.Parent.TemplateID != Config.BucketTemplateId)
                            {
                                var cpa = new ClientPipelineArgs();
                                if (!cpa.IsPostBack)
                                {
                                    var parameters = new NameValueCollection();
                                    parameters["id"] = item.ID.ToString();
                                    Context.ClientPage.Start(this, "Run", parameters);
                                }
                            }
                            else
                            {
                                return;
                            }
                        }
                    }
                }
            }
            else
            {
                return;
            }
        }

        /// <summary>
        /// Run Command
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>
        protected void Run(ClientPipelineArgs args)
        {
            if (args.IsPostBack)
            {
                if (args.HasResult)
                {
                    if (args.Result == "yes")
                    {
                        var parentId = Context.ContentDatabase.GetItem(args.Parameters["id"]).Parent.ID;
                        Context.ClientPage.SendMessage(this, "item:bucket(id=" + parentId + ")");
                        Context.ClientPage.SendMessage(this, "item:refreshchildren(id=" + parentId + ")");
                        args.Result = string.Empty;
                    }
                    else
                    {
                        args.Result = string.Empty;
                        args.IsPostBack = false;
                        return;
                    }
                }
            }
            else
            {
                Context.ClientPage.ClientResponse.Confirm("You have reached the recommended sibling item count and it is recommended to turn the parent item into a bucket. Continue?");
                args.WaitForPostBack();
            }
        }

        #endregion
    }
}
