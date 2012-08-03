namespace Sitecore.ItemBucket.Kernel.Kernel.Search.SearchOperations
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.IO;
    using System.Linq;

    using Sitecore.Configuration;
    using Sitecore.Data.Items;
    using Sitecore.Data.Serialization;
    using Sitecore.Diagnostics;
    using Sitecore.ItemBucket.Kernel.ItemExtensions.Axes;
    using Sitecore.ItemBucket.Kernel.Kernel.Util;
    using Sitecore.Pipelines;
    using Sitecore.Pipelines.GetItemPersonalizationVisibility;
    using Sitecore.SecurityModel;
    using Sitecore.Shell.Applications.Dialogs.ProgressBoxes;
    using Sitecore.Shell.Applications.WebEdit.Commands;
    using Sitecore.Shell.Framework.Commands;
    using Sitecore.Text;
    using Sitecore.Web;
    using Sitecore.Web.UI.Sheer;
    using Sitecore.Web.UI.WebControls;
    using Sitecore.Web.UI.XamlSharp.Continuations;

    class ApplyProfileCardsToAllItems : WebEditCommand, ISupportsContinuation
    {
        // Methods
        public override void Execute(CommandContext context)
        {
            Assert.ArgumentNotNull(context, "context");
            if (context.Items.Length == 1)
            {
                Item item = context.Items[0];
                if (!item.Appearance.ReadOnly && item.Access.CanWrite())
                {
                    var parameters = new NameValueCollection();
                    parameters["id"] = item.ID.ToString();
                    parameters["language"] = (Context.Language == null) ? item.Language.ToString() : Context.Language.ToString();
                    parameters["version"] = item.Version.ToString();
                    parameters["database"] = item.Database.Name;
                    parameters["isPageEditor"] = (context.Parameters["pageEditor"] == "1") ? "1" : "0";
                    parameters["searchString"] = context.Parameters.GetValues("url")[0].Replace("\"", string.Empty);
            
                    if (ContinuationManager.Current != null)
                    {
                        ContinuationManager.Current.Start(this, "Run", new ClientPipelineArgs(parameters));
                    }
                    else
                    {
                        Context.ClientPage.Start(this, "Run", parameters);
                    }
                }
            }
        }

        public override CommandState QueryState(CommandContext context)
        {
            Assert.ArgumentNotNull(context, "context");
            if (context.Items.Length != 1)
            {
                return CommandState.Disabled;
            }

            if (!Settings.Analytics.Enabled)
            {
                return CommandState.Hidden;
            }

            if (CorePipelineFactory.GetPipeline("getItemPersonalizationVisibility", string.Empty) == null)
            {
                return base.QueryState(context);
            }

            var args = new GetItemPersonalizationVisibilityArgs(true, context.Items[0]);

            using (new LongRunningOperationWatcher(200, "getItemPersonalizationVisibility", new string[0]))
            {
                CorePipeline.Run("getItemPersonalizationVisibility", args);
            }
            
            if (!args.Visible)
            {
                return CommandState.Hidden;
            }

            return CommandState.Enabled;
        }
        public void DumpItem(object[] param)
        {
            var path = string.Format("{0}//ItemSync//{1}{2}{3}{4}{5}{6}{7}", Settings.SerializationFolder, DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, DateTime.Now.Millisecond);
            var searchStringModel = ExtractSearchQuery(param[0].ToString());
            var tempItem = Factory.GetDatabase(param[2].ToString()).GetItem(param[1].ToString());
            int hitsCount;
            var listOfItems = tempItem.Search(searchStringModel, out hitsCount).ToList();
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            foreach (var sitecoreItem in listOfItems)
            {
                var item = sitecoreItem.GetItem();
                Assert.ArgumentNotNullOrEmpty(path, "path");
                Assert.ArgumentNotNull(item, "item");
                var itemPath = string.Format("{0}//{1}", path, item.Paths.FullPath);
                using (new SecurityDisabler())
                {
                    if (!Directory.Exists(itemPath))
                    {
                        Directory.CreateDirectory(itemPath);
                    }

                    using (var file = new StreamWriter(string.Format("{0}.item", itemPath.Replace("//", "\\")), true))
                    {
                        TextWriter writer = file;
                        try
                        {
                            ItemSynchronization.WriteItem(item, writer);
                        }
                        catch (Exception exception)
                        {
                            Log.Error(exception.Message, item);
                        }
                        finally
                        {
                            writer.Close();
                        }

                        writer.Close();
                    }
                }
            }
        }

        public void Execute(string query, string id, string database)
        {
            ProgressBox.Execute("Bulk Applying Profile Cards", this.GetName(), "Business/16x16/radar-chart.png", this.DumpItem, new object[] { query, id, database });
        }

        protected virtual string GetName()
        {
            return "Creating Snapshot Point";
        }
       
        protected void Run(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            if (SheerResponse.CheckModified())
            {
                if (args.IsPostBack)
                {
                    if (((Context.Page != null) && (Context.Page.Page != null)) && ((Context.Page.Page.Session["TrackingFieldModified"] as string) == "1"))
                    {
                        Context.Page.Page.Session["TrackingFieldModified"] = null;
                        if (args.Parameters["isPageEditor"] == "1")
                        {
                            Reload(WebEditCommand.GetUrl());
                        }
                        else if (AjaxScriptManager.Current != null)
                        {
                            AjaxScriptManager.Current.Dispatch("analytics:trackingchanged");
                        }
                        else
                        {
                            Context.ClientPage.SendMessage(this, "analytics:trackingchanged");
                            var tempItem = Factory.GetDatabase(args.Parameters["database"]).GetItem(args.Parameters["id"]);

                            var valueForAllOtherItems = tempItem.Fields["__tracking"].Value;
                            var searchStringModel = ExtractSearchQuery(args.Parameters["searchString"]);
                            int hitsCount;
                            var listOfItems = tempItem.Search(searchStringModel, out hitsCount).ToList();
                            Assert.IsNotNull(tempItem, "item");
                      
                            foreach (var sitecoreItem in listOfItems)
                            {
                                var item1 = sitecoreItem.GetItem();
                                Context.Job.Status.Messages.Add("Applying Profile Card to " + item1.Paths.FullPath + " item"); 
                                item1.Editing.BeginEdit();
                                item1["__tracking"] = valueForAllOtherItems;
                                item1.Editing.EndEdit();
                            }

                            this.Execute(args.Parameters["searchString"], tempItem.ID.ToString(), args.Parameters["database"]);
                            tempItem.Editing.BeginEdit();
                            tempItem.Fields["__tracking"].Value = Context.ClientData.GetValue("tempTrackingField").ToString();
                            tempItem.Editing.EndEdit();
                        }
                    }
                }
                else
                {
                    var tempItem = Factory.GetDatabase(args.Parameters["database"]).GetItem(args.Parameters["id"]);
                    Context.ClientData.SetValue("tempTrackingField", tempItem.Fields["__tracking"].Value);
                    var urlString = new UrlString("/sitecore/shell/~/xaml/Sitecore.Shell.Applications.Analytics.Personalization.ProfileCardsForm.aspx");
                    var handle = new UrlHandle();
                    handle["itemid"] = args.Parameters["id"];
                    handle["databasename"] = args.Parameters["database"];
                    handle["la"] = args.Parameters["language"];
                    handle.Add(urlString);
                    SheerResponse.ShowModalDialog(urlString.ToString(), "1000", "600", string.Empty, true);
                    args.WaitForPostBack();
                }
            }
        }

        private static List<SearchStringModel> ExtractSearchQuery(string searchQuery)
        {
            var searchStringModels = new List<SearchStringModel>();
            searchQuery = searchQuery.Replace("text:;", string.Empty);
            var terms = searchQuery.Split(';');
            for (var i = 0; i < terms.Length; i++)
            {
                if (!string.IsNullOrEmpty(terms[i]))
                {
                    searchStringModels.Add(new SearchStringModel
                    {
                        Type = terms[i].Split(':')[0],
                        Value = terms[i].Split(':')[1]
                    });
                }

                //Because of the String that is passed through I have to skip twice
            }
            return searchStringModels;
        }

        protected new virtual string GetUrl()
        {
            return "/sitecore/shell/~/xaml/Sitecore.Shell.Applications.Analytics.Personalization.ProfileCardsForm.aspx";
        }

        protected virtual void ShowDialog(string url)
        {
            Assert.ArgumentNotNull(url, "url");
            SheerResponse.ShowModalDialog(url, true);
        }
    }
}














