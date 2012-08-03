// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BucketUsageForm.cs" company="Sitecore">
//   Sitecore
// </copyright>
// <summary>
//   Show Bucket Information
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.ItemBucket.Kernel.Commands
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Web.UI;
    using Sitecore.Diagnostics;
    using Sitecore.ItemBucket.Kernel.Util;
    using Sitecore.Web.UI.HtmlControls;
    using Sitecore.Web.UI.Pages;

    /// <summary>
    /// Show Bucket Information
    /// </summary>
    internal class BucketUsageForm : DialogForm
    {
        #region Fields
        protected Border AvailableSpace;
        protected Border ContentLimit;
        protected Border Buckets;
        protected Border FreeSpaceRemaining;
        protected Border FreeSpaceRemainingBar;
        protected Border FreeSpaceRemainingPercentage;
        protected Border SystemUsage;
        protected Literal BucketTable;

        #endregion

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

            this.RenderBucketCount();
            this.RenderBuckets();
            this.RenderIndexList();
        }

        /// <summary>
        /// Render the Bucket Details
        /// </summary>
        private void RenderBuckets()
        {
            var writer = new HtmlTextWriter(new StringWriter());
            writer.Write("<table cellpadding=\"4\" cellspacing=\"0\" border=\"0\">");
            var indexSize = 0;
            foreach (var index in Sitecore.Search.SearchManager.Indexes)
            {
                indexSize = indexSize + Sitecore.Search.SearchManager.GetIndex(index.Name).GetDocumentCount();
            }

            writer.Write("<tr>");
            writer.Write("<td align=\"right\">");
            writer.Write("Number of items existing withing all Buckets: ");
            writer.Write("</td>");
            writer.Write("<td align=\"right\">");
            writer.Write(indexSize);
            writer.Write(' ');
            writer.Write("Items");
            writer.Write("</td>");
            writer.Write("</tr>");
            this.Buckets.Controls.Add(new LiteralControl(writer.InnerWriter.ToString()));
        }

        /// <summary>
        /// Determine Bucket Count and Render Output
        /// </summary>
        private void RenderBucketCount()
        {
            var writer = new HtmlTextWriter(new StringWriter());
            writer.Write("<table cellpadding=\"4\" cellspacing=\"0\" border=\"0\">");
            var items = new List<SitecoreItem>();

            foreach (var index in Sitecore.Search.SearchManager.Indexes)
            {
                using (var searcher = new IndexSearcher(index.Name))
                {
                    items.AddRange(searcher.GetItemsViaFieldQuery("isbucket", "1").Value);
                }
            }

            var bucketCount = items.Count;
            writer.Write("<tr>");
            writer.Write("<td align=\"right\">");
            writer.Write("Number of Buckets: ");
            writer.Write("</td>");
            writer.Write("<td align=\"right\">");
            writer.Write(bucketCount);
            writer.Write(' ');
            writer.Write("Items");
            writer.Write("</td>");
            writer.Write("</tr>");
            this.Buckets.Controls.Add(new LiteralControl(writer.InnerWriter.ToString()));
        }

        /// <summary>
        /// Render all the indexes to the Page
        /// </summary>
        private void RenderIndexList()
        {
            var writer = new HtmlTextWriter(new StringWriter());
            writer.Write("<table cellpadding=\"4\" cellspacing=\"0\" border=\"0\">");
            writer.Write("<tr>");
            writer.Write("<td align=\"right\">");
            writer.Write("Current Indexes: ");
            writer.Write("</td>");
            foreach (var index in Sitecore.Search.SearchManager.Indexes)
            {
                writer.Write("<td align=\"right\">");
                writer.Write("<strong>" + index.Name + "</strong>");
                writer.Write(" :");
                writer.Write(index.GetDocumentCount());
                writer.Write("</td>");
            }

            var stringtoreturn = "<thead><tr><td></td>";
            stringtoreturn = stringtoreturn + " </tr></thead><tbody>";
            foreach (var index in Sitecore.Search.SearchManager.Indexes)
            {
                stringtoreturn = stringtoreturn + "  <tr><th scope=\"row\">" + index.Name + "</th><td>" + index.GetDocumentCount() + "</td></tr>";
            }

            stringtoreturn = stringtoreturn + "</tbody>";
            this.BucketTable.Text = stringtoreturn;
            writer.Write("</tr>");
            this.Buckets.Controls.Add(new LiteralControl(writer.InnerWriter.ToString()));
        }
    }
}
