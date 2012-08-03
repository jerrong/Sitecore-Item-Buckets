namespace Sitecore.ItemBucket.Kernel.Kernel.Forms
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using ComponentArt.Web.UI;

    using Sitecore.Common;
    using Sitecore.Diagnostics;
    using Sitecore.Extensions;
    using Sitecore.Shell.Framework.Commands;
    using Sitecore.Web;
    using Sitecore.Web.UI.Grids;
    using Sitecore.Web.UI.XamlSharp.Xaml;

    /// <summary>
    /// SnapShot Details
    /// </summary>
    public class SnapshotPoint
    {
        public string SnapShotId
        {
            get; set;
        }

        public string ItemPathToTrack
        {
            get; set;
        }
    }

    /// <summary>
    /// Item Rollback
    /// </summary>
    public class ItemBucketsRollbackForm : XamlMainControl, IHasCommandContext
    {
        protected Grid Items;

        /// <summary>
        /// Form Construction
        /// </summary>
        /// <returns>
        /// </returns>
        private IPageable<SnapshotPoint> GetSnapshotEntries()
        {
            var listOfReturn = new List<SnapshotPoint>();

            foreach (var directory in Directory.GetDirectories(Configuration.Settings.SerializationFolder + "/ItemSync"))
            {
                listOfReturn.Add(new SnapshotPoint
                                     {
                                         ItemPathToTrack = directory,
                                         SnapShotId = directory
                                     });
            }

            return new Pageable<SnapshotPoint>((pageIndex, pageSize) => listOfReturn, () => listOfReturn, () => listOfReturn.Count());
        }

        protected override void OnInit(EventArgs e)
        {
            Assert.ArgumentNotNull(e, "e");
            base.OnInit(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            Assert.ArgumentNotNull(e, "e");
            base.OnLoad(e);
            this.RepositoryName = StringUtil.GetString(new[] { WebUtil.GetQueryString("an", "recyclebin") });
            var archiveEntries = this.GetSnapshotEntries();
            ComponentArtGridHandler<SnapshotPoint>.Manage(this.Items, new GridSource<SnapshotPoint>(archiveEntries), !AjaxScriptManager.IsEvent);
            this.Items.LocalizeGrid();
        }

        CommandContext IHasCommandContext.GetCommandContext()
        {
            var itemNotNull = Client.GetItemNotNull("/sitecore/content/Applications/Archives/Item Bucket Roll Back/Ribbon", Client.CoreDatabase);
            var context = new CommandContext();
            context.Parameters["item"] = GridUtil.GetSelectedValue("Items");
            context.Parameters["archivename"] = this.RepositoryName;
            context.RibbonSourceUri = itemNotNull.Uri;
            return context;
        }

        /// <summary>
        /// Gets or sets RepositoryName.
        /// </summary>
        public string RepositoryName
        {
            get
            {
                return StringUtil.GetString(this.ViewState["ArchiveName"]);
            }

            set
            {
                Assert.ArgumentNotNull(value, "value");
                this.ViewState["ArchiveName"] = value;
            }
        }
    }
}

