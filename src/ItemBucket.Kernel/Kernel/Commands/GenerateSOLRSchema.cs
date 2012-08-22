// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RebuildBucketIndex.cs" company="Sitecore">
//   Sitecore
// </copyright>
// <summary>
//   Defines the RebuildBucketIndex type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Diagnostics;
using System.IO;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Sitecore.ItemBucket.Kernel.Kernel.Search;
using Sitecore.Search.Crawlers;

namespace Sitecore.ItemBucket.Kernel.Commands
{
    using System;
    using System.Web.UI;
    using Sitecore.Diagnostics;
    using Sitecore.Globalization;
    using System.Linq;
    using Sitecore.Jobs;
    using Sitecore.Search;
    using Sitecore.Text;
    using Sitecore.Web.UI.HtmlControls;
    using Sitecore.Web.UI.Pages;
    using Checkbox = Sitecore.Shell.Applications.ContentEditor.Checkbox;
    using Control = Sitecore.Web.UI.HtmlControls.Control;

    /// <summary>
    /// Rebuilding Index Form
    /// </summary>
    internal class GenerateSOLRSchema : WizardForm
    {
        protected Memo ErrorText;
        protected Border Indexes;
        protected Memo ResultText;
        private static Stopwatch rebuildTimer = new Stopwatch();
        private static string path = HttpContext.Current.Request.PhysicalApplicationPath;
        /// <summary>
        /// Gets or sets IndexMap.
        /// </summary>
        public string IndexMap
        {
            get
            {
                return StringUtil.GetString(this.ServerProperties["IndexMap"]);
            }

            set
            {
                Assert.ArgumentNotNull(value, "value");
                this.ServerProperties["IndexMap"] = value;
            }
        }

        /// <summary>
        /// Detecting the Page Changing
        /// </summary>
        /// <param name="page">
        /// The page.
        /// </param>
        /// <param name="oldPage">
        /// The old page.
        /// </param>
        protected override void ActivePageChanged(string page, string oldPage)
        {
            Assert.ArgumentNotNull(page, "page");
            Assert.ArgumentNotNull(oldPage, "oldPage");
            base.ActivePageChanged(page, oldPage);
            this.NextButton.Header = "Next >";
            if (page == "Database")
            {
                this.NextButton.Header = "Rebuild >";
            }

            if (page == "Rebuilding")
            {
                this.NextButton.Disabled = true;
                this.BackButton.Disabled = true;
                this.CancelButton.Disabled = true;
                Context.ClientPage.ClientResponse.Timer("StartRebuilding", 10);
            }
        }

        /// <summary>
        /// Detect Active Page Change
        /// </summary>
        /// <param name="page">
        /// The page.
        /// </param>
        /// <param name="newpage">
        /// The newpage.
        /// </param>
        /// <returns>
        /// True / False
        /// </returns>
        protected override bool ActivePageChanging(string page, ref string newpage)
        {
            Assert.ArgumentNotNull(page, "page");
            Assert.ArgumentNotNull(newpage, "newpage");
            if ((page == "Retry") && (newpage == "Rebuilding"))
            {
                newpage = "Database";
                this.NextButton.Disabled = false;
                this.BackButton.Disabled = false;
                this.CancelButton.Disabled = false;
            }

            return base.ActivePageChanging(page, ref newpage);
        }

        /// <summary>
        /// Status Update
        /// </summary>
        protected void CheckStatus()
        {
            var str = Context.ClientPage.ServerProperties["handle"] as string;
            Assert.IsNotNullOrEmpty(str, "handle");
            var job = JobManager.GetJob(Handle.Parse(str));
            if (job == null)
            {
                this.Active = "Retry";
                this.NextButton.Disabled = true;
                this.BackButton.Disabled = false;
                this.CancelButton.Disabled = false;
                this.ErrorText.Value = "Job has finished unexpectedly";
            }
            else if (job.Status.Failed)
            {
                this.Active = "Retry";
                this.NextButton.Disabled = true;
                this.BackButton.Disabled = false;
                this.CancelButton.Disabled = false;
            }
            else
            {
                var str2 = job.Status.State == JobState.Running ? Translate.Text("Processed {0} items. ", new object[] { job.Status.Processed, job.Status.Total }) : Translate.Text("Queued.");

                if (job.IsDone)
                {
                    rebuildTimer.Stop();
                    this.Active = "LastPage";
                    this.BackButton.Disabled = true;
                    this.ResultText.Value = "Finished in : " + (rebuildTimer.ElapsedMilliseconds / 1000) + " seconds. Your file is under /sitecore modules/Shell/Sitecore/ItemBuckets/Services/";

                }
                else
                {
                    Context.ClientPage.ClientResponse.SetInnerHtml("Status", str2);
                    Context.ClientPage.ClientResponse.Timer("CheckStatus", 500);
                }
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
                this.BuildIndexes();
            }
        }

        /// <summary>
        /// Starts Building Indexes
        /// </summary>
        protected void StartRebuilding()
        {
            var str = new ListString();
            var str2 = new ListString(this.IndexMap);
            foreach (string str3 in Context.ClientPage.ClientRequest.Form.Keys)
            {
                if (!string.IsNullOrEmpty(str3) && str3.StartsWith("dk_"))
                {
                    int index = str2.IndexOf(str3);
                    if (index >= 0)
                    {
                        str.Add(str2[index + 1]);
                    }
                }
            }

            Registry.SetString("/Current_User/Rebuild Search Index/Selected", str.ToString());

           
                var options2 = new JobOptions("RebuildSearchIndex", "index", Client.Site.Name,
                                              new Builder(str.ToString()), "Build")
                {
                    AfterLife = TimeSpan.FromMinutes(1.0),
                    ContextUser = Context.User
                };
                var options = options2;
                var job = JobManager.Start(options);
                Context.ClientPage.ServerProperties["handle"] = job.Handle.ToString();
                Context.ClientPage.ClientResponse.Timer("CheckStatus", 500);
            
       
        }

        /// <summary>
        /// Builds the Checkboxes to select indexes
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="header">
        /// The header.
        /// </param>
        /// <param name="selected">
        /// The selected.
        /// </param>
        /// <param name="indexMap">
        /// The index map.
        /// </param>
        private void BuildIndexCheckbox(string name, string header, ListString selected, ListString indexMap)
        {
            Assert.ArgumentNotNull(name, "name");
            Assert.ArgumentNotNull(header, "header");
            Assert.ArgumentNotNull(selected, "selected");
            Assert.ArgumentNotNull(indexMap, "indexMap");
            var child = new Checkbox();
            this.Indexes.Controls.Add(child);
            child.ID = Control.GetUniqueID("dk_");
            child.Header = header;
            child.Value = name;
            child.Checked = true;//selected.Contains(name);
            indexMap.Add(child.ID);
            indexMap.Add(name);
            this.Indexes.Controls.Add(new LiteralControl("<br />"));
        }

        /// <summary>
        /// This is the process that Runs the Index Rebuild
        /// </summary>
        private void BuildIndexes()
        {
            var selected = new ListString(Registry.GetString("/Current_User/Rebuild Search Index/Selected"));
            var indexMap = new ListString();
            foreach (var str3 in SearchManager.Indexes)
            {
                this.BuildIndexCheckbox(str3.Name, str3.Name, selected, indexMap);
            }


            //this.BuildIndexCheckbox("Select All", "Select All", selected, indexMap);
            //this.BuildIndexCheckbox("Deselect All", "Deselect All", selected, indexMap);
            this.BuildIndexCheckbox("$system", "Quick search index", selected, indexMap);
            this.IndexMap = indexMap.ToString();
        }

        /// <summary>
        /// Builder Job Class
        /// </summary>
        private class Builder
        {
            /// <summary>
            /// List of Index Names
            /// </summary>
            private readonly ListString indexNames;

            /// <summary>
            /// Initializes a new instance of the <see cref="Sitecore.ItemBucket.Kernel.Commands.RebuildBucketIndex.Builder"/> class.
            /// </summary>
            /// <param name="indexname">
            /// The indexname.
            /// </param>
            public Builder(string indexname)
            {
                Assert.ArgumentNotNull(indexname, "indexname");
                this.indexNames = new ListString(indexname);
            }

            /// <summary>
            /// Build Index
            /// </summary>
            protected void Build()
            {
                rebuildTimer.Start();
                var job = Context.Job;
                if (job != null)
                {
                    try
                    {
                        foreach (var str in this.indexNames)
                        {
                            //var index = SearchManager.GetIndex(str);
                            //if (index != null)
                            //{
                            //    index.Rebuild();
                            //}
                            ExportIndexSchema(str);

                            var status = job.Status;
                            status.Processed += 1L;
                        }
                    }
                    catch (Exception exception)
                    {
                        job.Status.Failed = true;
                        job.Status.Messages.Add(exception.ToString());
                    }

                    job.Status.State = JobState.Finished;
                }
            }

            private void ExportIndexSchema(string str)
            {
                //Load xml
                XDocument xdoc = XDocument.Load(path + @"\sitecore modules\Shell\Sitecore\ItemBuckets\Services\schema_template.xml");
                var index = SearchManager.GetIndex(str);
                if (index != null)
                {

                    using (var context = new SortableIndexSearchContext(index))
                    {
                        //Run query
                        foreach (var field in context.Searcher.GetIndexReader().GetFieldNames(IndexReader.FieldOption.ALL))
                        {
                            xdoc.Element("schema").Element("fields").Add(new XElement("field", new XAttribute("name", field),
                                                                             new XAttribute("type", "string"),
                                                                             new XAttribute("indexed", "true"),
                                                                             new XAttribute("stored", "true")));
                        }

                    }
                }
                var WriteFileStream = new XmlTextWriter(path + @"\sitecore modules\Shell\Sitecore\ItemBuckets\Services\schema.xml", Encoding.UTF8);
             
                xdoc.Save(WriteFileStream);
                WriteFileStream.Close();
            }

            protected void RemoteBuild()
            {
                var job = Context.Job;
                if (job != null)
                {
                    try
                    {
                        var serverAddress = Util.Config.RemoteIndexingServer;
                        //Copy PsExec to Target Address

                        foreach (var str in this.indexNames)
                        {
                            var index = SearchManager.GetIndex(str);
                            if (index != null)
                            {

                                using (IndexUpdateContext context = new IndexUpdateContext(index))
                                {
                                    foreach (ICrawler crawler in SearchManager.Indexes.Where(indexType => indexType.GetType() == Type.GetType("Sitecore.ItemBuckets.BigData.RemoteIndex.RemoteIndex, Sitecore.ItemBuckets.BigData")))
                                    {
                                        crawler.Add(context);
                                    }
                                    context.Optimize();
                                    context.Commit();
                                }

                            }

                            var status = job.Status;
                            status.Processed += 1L;
                        }
                    }
                    catch (Exception exception)
                    {
                        job.Status.Failed = true;
                        job.Status.Messages.Add(exception.ToString());
                    }

                    job.Status.State = JobState.Finished;
                }
            }


        }
    }
}