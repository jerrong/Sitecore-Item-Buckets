// -----------------------------------------------------------------------
// <copyright file="ExtendedShellForm.cs" company="Microsoft">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Sitecore.ItemBucket.Kernel.Kernel.Forms
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    using Sitecore.Configuration;
    using Sitecore.Data;
    using Sitecore.Data.Items;
    using Sitecore.ItemBucket.Kernel.Kernel.Util;
    using Sitecore.Jobs;
    using Sitecore.Security.AccessControl;
    using Sitecore.Security.Accounts;
    using Sitecore.SecurityModel;
    using Sitecore.Shell.Applications;
    using Sitecore.Web.UI.HtmlControls;
    using Sitecore.Web.UI.Sheer;
    using Sitecore.Workflows;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class ExtendedShellForm : ShellForm
    {
        void HandleJobStateChangeEvent(object sender, EventArgs a)
        {
            var item = Factory.GetDatabase("core").GetItem("{21783FF0-6856-4DDD-AD42-FA16D9E13268}");
            using (new EditContext(item, SecurityCheck.Disable))
            {
                item.Appearance.Icon = "Business/16x16/data.png";
            }
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (Context.ClientPage.ClientQueryString.Contains("RunningJobs=refresh"))
            {
                HttpContext.Current.Response.Clear();
                HttpContext.Current.Response.Write("<img src=\"/temp/IconCache/" + (JobManager.GetJobs().Any() ? this.GetJobIcon(JobState.Running) : this.GetJobIcon(JobState.Finished)) + "\" width=\"16\" height=\"16\" align=\"middle\" class=\"\" style=\"margin:0px 1px 0px 1px\" alt=\"Running Jobs\" border=\"0\">");
                HttpContext.Current.Response.Flush();
                HttpContext.Current.Response.End();
            }

            //if (Context.ClientPage.ClientQueryString.Contains("WorkflowNotifications=refresh"))
            //{
            //    HttpContext.Current.Response.Clear();
            //    HttpContext.Current.Response.Write("<img src=\"/temp/IconCache/" + (this.GetWorkFlowItems(10).Any() ? "Network/32x32/inbox_into.png" : "network/32x32/inbox.png") + "\" width=\"16\" height=\"16\" align=\"middle\" class=\"\" style=\"margin:0px 1px 0px 1px\" alt=\"Running Jobs\" border=\"0\">");
            //    HttpContext.Current.Response.Flush();
            //    HttpContext.Current.Response.End();
            //}
        }

        private IEnumerable<Item> GetWorkFlowItems(int numberOfItems)
        {
            var currentUser = Context.User;
            
            var workFlowList = Context.ContentDatabase.WorkflowProvider.GetWorkflows();
            var index = 0;
            var workFlowItems = new List<Item>();
            foreach (var iwf in workFlowList)
            {
                WorkflowState[] wss = iwf.GetStates();
                foreach (WorkflowState ws in wss)
                {
                    var its = iwf.GetItems(ws.StateID);
                    foreach (var workflowItemInState in its)
                    {
                        if (index != numberOfItems)
                        {
                            var i = Context.ContentDatabase.GetItem(workflowItemInState.ItemID);
                            var isNotNull = Context.Workflow.GetWorkflow(i).GetState(i);
                            if (isNotNull.IsNotNull())
                            {
                                if (!isNotNull.FinalState)
                                {
                                    var ia = new ItemAccess(i);
                                    if (ia.CanRead())
                                    {
                                        workFlowItems.Add(i);
                                        index++;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return workFlowItems;
        }

        private string GetJobIcon(JobState jobState)
        {
            switch (jobState)
            {
                case JobState.Running:
                    return "applications/16x16/gear_run.png";
                case JobState.Queued:
                    return "Applications/16x16/gear_view.png";
                case JobState.Initializing:
                    return "Applications/16x16/gear_connection.png";
                case JobState.Unknown:
                    return "Applications/16x16/gear_warning.png";
                default:
                    return "Applications/16x16/gear.png";
            }
        }

        protected void WorkflowNotifications()
        {
            var contextMenu = new Menu();
            SheerResponse.DisableOutput();
            try
            {
                foreach (var str2 in GetWorkFlowItems(10))
                {
                    if (Context.Workflow.GetWorkflow(str2).GetState(str2).IsNotNull())
                    {
                        var item3 = new MenuItem();
                        contextMenu.Controls.Add(item3);
                        var workflowState = str2;
                        item3.Header = str2.Name + " ("
                                       + Context.Workflow.GetWorkflow(workflowState).GetState(workflowState).DisplayName
                                       + ")";
                        item3.Icon = StringUtil.GetString(this.GetWorkflowIcon(workflowState));
                        item3.Click = "HaltJob(\"" + str2.Name + "\")";
                        item3.Checked = false;
                    }
                }
            }
            finally
            {
                SheerResponse.EnableOutput();
            }

            SheerResponse.ShowContextMenu("WorkflowNotifications", "above", contextMenu);
        }

        private string GetWorkflowIcon(Item workflowState)
        {
            switch (Context.Workflow.GetWorkflow(workflowState).GetState(workflowState).FinalState)
            {
                case true:
                    return Context.Workflow.GetWorkflow(workflowState).GetState(workflowState).Icon;
                default:
                    return Context.Workflow.GetWorkflow(workflowState).GetState(workflowState).Icon;
            }
        }

        protected void RunningJobs()
        {
            var contextMenu = new Menu();
            SheerResponse.DisableOutput();
            try
            {
                foreach (var str2 in JobManager.GetJobs())
                {
                    var item3 = new MenuItem();
                    contextMenu.Controls.Add(item3);
                    var jobState = str2.Status.State;
                    item3.Header = str2.Name + " (" + jobState + ")";
                    item3.Icon = StringUtil.GetString(this.GetJobIcon(jobState));
                    item3.Click = "HaltJob(\"" + str2.Name + "\")";
                    item3.Checked = false;
                    str2.Finished += this.HandleJobStateChangeEvent;
                }
            }
            finally
            {
                SheerResponse.EnableOutput();
            }

            SheerResponse.ShowContextMenu("RunningJobs", "above", contextMenu);
        }
    }
}
