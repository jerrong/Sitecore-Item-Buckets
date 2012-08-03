using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Sitecore.Jobs;

namespace Ninemsn.CMSPilot.Web.Ninemsn.ItemGenerator
{
    public partial class ItemDuplicator : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void btnRefresh_Click(object sender, EventArgs args)
        {

        }

        public class Builder
        {
            private string _indexName;

            public Builder(string indexName)
            {
                _indexName = indexName;
            }

            public void Rebuild()
            {
                var index = Sitecore.Search.SearchManager.GetIndex(_indexName);
                if (index != null)
                {
                    index.Rebuild();
                }
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            var duplicateJob = Sitecore.Jobs.JobManager.GetJob("DuplicateItem");
            if (duplicateJob != null)
            {
                litStatus.Text = "Duplicate job is " + duplicateJob.Status.State.ToString();
            }
            if (duplicateJob == null)
            {
                btnStart.Enabled = true;
            }
            else
            {
                btnStart.Enabled = duplicateJob.Status.State == JobState.Finished || duplicateJob.Status.State == JobState.Unknown;
            }
        }

        protected void btnStart_Click(object sender, EventArgs e)
        {
            var options = new Sitecore.Jobs.JobOptions("DuplicateItem", "duplicate", Sitecore.Client.Site.Name, new Duplicator(txtPath.Text), "Duplicate");
            options.AfterLife = TimeSpan.FromMinutes(1.0);
            Sitecore.Jobs.JobManager.Start(options);
        }

        public class Duplicator
        {
            private string _itemPath;

            public Duplicator(string itemPath)
            {
                _itemPath = itemPath;
            }

            public void Duplicate()
            {
                var item = Sitecore.Context.ContentDatabase.GetItem(_itemPath);
                if(item != null)
                {
                    item.Duplicate(item.Name + "duplicate");
                }
            }
        }
    }
}