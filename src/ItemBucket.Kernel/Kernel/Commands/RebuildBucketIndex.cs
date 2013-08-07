// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RebuildBucketIndex.cs" company="Sitecore A/S">
//   Copyright (C) 2013 by Sitecore A/S
// </copyright>
// <summary>
//   Defines the RebuildBucketIndex type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.ItemBucket.Kernel.Commands
{
  using System;
  using System.Diagnostics;
  using System.Web.UI;

  using Sitecore.Diagnostics;
  using Sitecore.Globalization;
  using Sitecore.ItemBuckets.BigData.RamDirectory;
  using Sitecore.ItemBuckets.BigData.RemoteIndex;
  using Sitecore.Jobs;
  using Sitecore.Search;
  using Sitecore.Text;
  using Sitecore.Web.UI.HtmlControls;
  using Sitecore.Web.UI.Pages;

  using Checkbox = Sitecore.Shell.Applications.ContentEditor.Checkbox;
  using Control = Sitecore.Web.UI.HtmlControls.Control;

  /// <summary>
  ///   Rebuilding Index Form
  /// </summary>
  internal class RebuildBucketIndex : WizardForm
  {
    /// <summary>
    /// The rebuild timer
    /// </summary>
    private static readonly Stopwatch RebuildTimer = new Stopwatch();

    /// <summary>
    /// The error text
    /// </summary>
    private Memo errorText;

    /// <summary>
    /// The indexes
    /// </summary>
    private Border indexes;

    /// <summary>
    /// The result text
    /// </summary>
    private Memo resultText;

    /// <summary>
    ///   Gets or sets IndexMap.
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
    ///   Detecting the Page Changing
    /// </summary>
    /// <param name="page">
    ///   The page.
    /// </param>
    /// <param name="oldPage">
    ///   The old page.
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
    /// <param name="page">The page.</param>
    /// <param name="newpage">The new page.</param>
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
      Job job = JobManager.GetJob(Handle.Parse(str));
      if (job == null)
      {
        this.Active = "Retry";
        this.NextButton.Disabled = true;
        this.BackButton.Disabled = false;
        this.CancelButton.Disabled = false;
        this.errorText.Value = "Job has finished unexpectedly";
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
        string str2 = job.Status.State == JobState.Running
                        ? Translate.Text(
                          "Processed {0} items. ", new object[] { job.Status.Processed, job.Status.Total })
                        : Translate.Text("Queued.");

        if (job.IsDone)
        {
          RebuildTimer.Stop();
          this.Active = "LastPage";
          this.BackButton.Disabled = true;
          this.resultText.Value = "Finished in : " + (RebuildTimer.ElapsedMilliseconds / 1000) + " seconds";
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
    /// <param name="e">The e.</param>
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
    /// Starts Building indexes
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

      var options2 = new JobOptions(
        "RebuildSearchIndex", "index", Client.Site.Name, new Builder(str.ToString()), "Build")
                       {
                         AfterLife =
                           TimeSpan.FromMinutes(
                             1.0),
                         ContextUser =
                           Context.User
                       };
      JobOptions options = options2;
      Job job = JobManager.Start(options);
      Context.ClientPage.ServerProperties["handle"] = job.Handle.ToString();
      Context.ClientPage.ClientResponse.Timer("CheckStatus", 500);

      var options3 = new JobOptions(
        "RebuildSearchIndex", "index", Client.Site.Name, new Builder(str.ToString()), "RemoteBuild")
                       {
                         AfterLife =
                           TimeSpan
                           .FromMinutes(
                             1.0),
                         ContextUser =
                           Context.User
                       };
      JobOptions options4 = options3;
      Job job2 = JobManager.Start(options4);
      Context.ClientPage.ServerProperties["handle"] = job2.Handle.ToString();
      Context.ClientPage.ClientResponse.Timer("CheckStatus", 500);

      var memoryOption = new JobOptions(
        "RebuildSearchIndex", "index", Client.Site.Name, new Builder(str.ToString()), "InMemoryBuild")
                           {
                             AfterLife =
                               TimeSpan
                               .FromMinutes(
                                 1.0),
                             ContextUser =
                               Context.User
                           };

      Job job3 = JobManager.Start(memoryOption);
      Context.ClientPage.ServerProperties["handle"] = job3.Handle.ToString();
      Context.ClientPage.ClientResponse.Timer("CheckStatus", 500);
    }

    /// <summary>
    /// Builds the Checkboxes to select indexes
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="header">The header.</param>
    /// <param name="selected">The selected.</param>
    /// <param name="indexMap">The index map.</param>
    private void BuildIndexCheckbox(string name, string header, ListString selected, ListString indexMap)
    {
      Assert.ArgumentNotNull(name, "name");
      Assert.ArgumentNotNull(header, "header");
      Assert.ArgumentNotNull(selected, "selected");
      Assert.ArgumentNotNull(indexMap, "indexMap");
      var child = new Checkbox();
      this.indexes.Controls.Add(child);
      child.ID = Control.GetUniqueID("dk_");
      child.Header = header;
      child.Value = name;
      child.Checked = selected.Contains(name);
      indexMap.Add(child.ID);
      indexMap.Add(name);
      this.indexes.Controls.Add(new LiteralControl("<br />"));
    }

    /// <summary>
    /// This is the process that Runs the Index Rebuild
    /// </summary>
    private void BuildIndexes()
    {
      var selected = new ListString(Registry.GetString("/Current_User/Rebuild Search Index/Selected"));
      var indexMap = new ListString();
      foreach (Index str3 in SearchManager.Indexes)
      {
        this.BuildIndexCheckbox(str3.Name, str3.Name, selected, indexMap);
      }

      RemoteSearchManager.Initialize();
      foreach (RemoteIndex str3 in RemoteSearchManager.Indexes)
      {
        this.BuildIndexCheckbox(str3.Name, str3.Name, selected, indexMap);
      }

      InMemorySearchManager.Initialize();
      foreach (InMemoryIndex str3 in InMemorySearchManager.Indexes)
      {
        this.BuildIndexCheckbox(str3.Name, str3.Name, selected, indexMap);
      }
      
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
      /// Initializes a new instance of the <see cref="Builder" /> class.
      /// </summary>
      /// <param name="indexname">The index name.</param>
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
        RebuildTimer.Start();
        Job job = Context.Job;
        if (job != null)
        {
          try
          {
            foreach (string str in this.indexNames)
            {
              if (!str.EndsWith("_remote") && !str.EndsWith("_inmemory"))
              {
                Index index = SearchManager.GetIndex(str);
                if (index != null)
                {
                  index.Rebuild();
                }

                JobStatus status = job.Status;
                status.Processed += 1L;
              }
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

      /// <summary>
      /// Remotes the build.
      /// </summary>
      protected void RemoteBuild()
      {
        Job job = Context.Job;
        if (job != null)
        {
          try
          {
            // Copy PsExec to Target Address
            foreach (string str in this.indexNames)
            {
              if (str.EndsWith("_remote"))
              {
                var index = RemoteSearchManager.GetIndex(str) as RemoteIndex;
                if (index != null)
                {
                  index.Rebuild();
                }

                JobStatus status = job.Status;
                status.Processed += 1L;
              }
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

      /// <summary>
      /// Ins the memory build.
      /// </summary>
      protected void InMemoryBuild()
      {
        Job job = Context.Job;
        if (job != null)
        {
          try
          {
            foreach (string str in this.indexNames)
            {
              if (str.EndsWith("_inmemory"))
              {
                var index = InMemorySearchManager.GetIndex(str) as InMemoryIndex;
                if (index != null)
                {
                  index.Rebuild();
                }

                JobStatus status = job.Status;
                status.Processed += 1L;
              }
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