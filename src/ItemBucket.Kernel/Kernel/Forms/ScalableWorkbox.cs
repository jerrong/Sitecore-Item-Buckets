using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Exceptions;
using Sitecore.Globalization;
using Sitecore.Resources;
using Sitecore.Shell.Data;
using Sitecore.Shell.Feeds;
using Sitecore.Shell.Framework;
using Sitecore.Shell.Framework.CommandBuilders;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Text;
using Sitecore.Web;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Sheer;
using Sitecore.Web.UI.WebControls.Ribbons;
using Sitecore.Web.UI.XmlControls;
using Sitecore.Workflows;
using Version = Sitecore.Data.Version;

namespace Sitecore.ItemBucket.Kernel.Kernel.Forms
{
    public class ScalableWorkboxForm : BaseForm
    {
        // Fields
        private NameValueCollection _stateNames;
        protected Border Pager;
        protected Border RibbonPanel;
        protected Border States;
        protected Toolmenubutton ViewMenu;

        // Methods
        public void Comment(ClientPipelineArgs args)
        {
            if (!args.IsPostBack)
            {
                Context.ClientPage.ClientResponse.Input("Enter a comment:", string.Empty);
                args.WaitForPostBack();
            }
            else if (args.Result.Length > 0x7d0)
            {
                Context.ClientPage.ClientResponse.ShowError(new Exception(string.Format("The comment is too long.\n\nYou have entered {0} characters.\nA comment cannot contain more than 2000 characters.", args.Result.Length)));
                Context.ClientPage.ClientResponse.Input("Enter a comment:", string.Empty);
                args.WaitForPostBack();
            }
            else if (((args.Result != null) && (args.Result != "null")) && (args.Result != "undefined"))
            {
                IWorkflowProvider workflowProvider = Context.ContentDatabase.WorkflowProvider;
                if (workflowProvider != null)
                {
                    IWorkflow workflow = workflowProvider.GetWorkflow(Context.ClientPage.ServerProperties["workflowid"] as string);
                    if (workflow != null)
                    {
                        Item item = Context.ContentDatabase.Items[Context.ClientPage.ServerProperties["id"] as string, Language.Parse(Context.ClientPage.ServerProperties["language"] as string), Version.Parse(Context.ClientPage.ServerProperties["version"] as string)];
                        if (item != null)
                        {
                            try
                            {
                                workflow.Execute(Context.ClientPage.ServerProperties["command"] as string, item, args.Result, true, new object[0]);
                            }
                            catch (WorkflowStateMissingException)
                            {
                                SheerResponse.Alert("One or more items could not be processed because their workflow state does not specify the next step.", new string[0]);
                            }
                            Context.ClientPage.ClientResponse.SetLocation(string.Empty);
                        }
                    }
                }
            }
        }

        private void CreateCommand(IWorkflow workflow, WorkflowCommand command, Item item, XmlControl workboxItem)
        {
            XmlControl webControl = Resource.GetWebControl("WorkboxCommand") as XmlControl;
            webControl["Header"] = command.DisplayName;
            webControl["Icon"] = command.Icon;
            CommandBuilder builder = new CommandBuilder("workflow:send");
            builder.Add("id", item.ID.ToString());
            builder.Add("la", item.Language.Name);
            builder.Add("vs", item.Version.ToString());
            builder.Add("command", command.CommandID);
            builder.Add("wf", workflow.WorkflowID);
            builder.Add("ui", command.HasUI);
            builder.Add("suppresscomment", command.SuppressComment);
            webControl["Command"] = builder.ToString();
            workboxItem.AddControl(webControl);
        }

        private void CreateItem(IWorkflow workflow, Item item, Control control)
        {
            XmlControl webControl = Resource.GetWebControl("WorkboxItem") as XmlControl;
            control.Controls.Add(webControl);
            StringBuilder builder = new StringBuilder(" - (");
            Language language = item.Language;
            builder.Append(language.CultureInfo.DisplayName);
            builder.Append(", ");
            builder.Append(Translate.Text("version"));
            builder.Append(' ');
            builder.Append(item.Version.ToString());
            builder.Append(")");
            Assert.IsNotNull(webControl, "workboxItem");
            webControl["Header"] = item.DisplayName;
            webControl["Details"] = builder.ToString();
            webControl["Icon"] = item.Appearance.Icon;
            webControl["ShortDescription"] = item.Help.ToolTip;
            webControl["History"] = this.GetHistory(workflow, item);
            webControl["HistoryMoreID"] = Control.GetUniqueID("ctl");
            webControl["HistoryClick"] = "workflow:showhistory(id=" + item.ID.ToString() + ",la=" + item.Language.Name + ",vs=" + item.Version.ToString() + ",wf=" + workflow.WorkflowID + ")";
            webControl["PreviewClick"] = "Preview(\"" + item.ID.ToString() + "\", \"" + item.Language.ToString() + "\", \"" + item.Version.ToString() + "\")";
            webControl["Click"] = "Open(\"" + item.ID.ToString() + "\", \"" + item.Language.ToString() + "\", \"" + item.Version.ToString() + "\")";
            webControl["DiffClick"] = "Diff(\"" + item.ID.ToString() + "\", \"" + item.Language.ToString() + "\", \"" + item.Version.ToString() + "\")";
            webControl["Display"] = "none";
            string uniqueID = Control.GetUniqueID(string.Empty);
            webControl["CheckID"] = "check_" + uniqueID;
            webControl["HiddenID"] = "hidden_" + uniqueID;
            webControl["CheckValue"] = string.Concat(new object[] { item.ID, ",", item.Language, ",", item.Version });
            foreach (WorkflowCommand command in WorkflowFilterer.FilterVisibleCommands(workflow.GetCommands(item)))
            {
                this.CreateCommand(workflow, command, item, webControl);
            }
        }

        private void CreateNavigator(Section section, string id, int count)
        {
            Navigator child = new Navigator();
            section.Controls.Add(child);
            child.ID = id;
            child.Offset = 0;
            child.Count = count;
            child.PageSize = this.PageSize;
        }

        protected void Diff(string id, string language, string version)
        {
            UrlString str = new UrlString(UIUtil.GetUri("control:Diff"));
            str.Append("id", id);
            str.Append("la", language);
            str.Append("vs", version);
            str.Append("wb", "1");
            Context.ClientPage.ClientResponse.ShowModalDialog(str.ToString());
        }

        protected virtual void DisplayState(IWorkflow workflow, WorkflowState state, DataUri[] items, Control control, int offset, int pageSize)
        {
            if (items.Length > 0)
            {
                int length = offset + pageSize;
                if (length > items.Length)
                {
                    length = items.Length;
                }
                for (int i = offset; i < length; i++)
                {
                    DataUri uri = items[i];
                    Item item = Context.ContentDatabase.Items[uri];
                    if (item != null)
                    {
                        this.CreateItem(workflow, item, control);
                    }
                }
                Border child = new Border
                {
                    Background = "#e9e9e9"
                };
                control.Controls.Add(child);
                child.Margin = "0px 4px 0px 16px";
                child.Padding = "2px 8px 2px 8px";
                child.Border = "1px solid #999999";
                foreach (WorkflowCommand command in WorkflowFilterer.FilterVisibleCommands(workflow.GetCommands(state.StateID)))
                {
                    XmlControl webControl = Resource.GetWebControl("WorkboxCommand") as XmlControl;
                    webControl["Header"] = command.DisplayName + " " + Translate.Text("(selected)");
                    webControl["Icon"] = command.Icon;
                    webControl["Command"] = "workflow:sendselected(command=" + command.CommandID + ",ws=" + state.StateID + ",wf=" + workflow.WorkflowID + ")";
                    child.Controls.Add(webControl);
                    webControl = Resource.GetWebControl("WorkboxCommand") as XmlControl;
                    webControl["Header"] = command.DisplayName + " " + Translate.Text("(all)");
                    webControl["Icon"] = command.Icon;
                    webControl["Command"] = "workflow:sendall(command=" + command.CommandID + ",ws=" + state.StateID + ",wf=" + workflow.WorkflowID + ")";
                    child.Controls.Add(webControl);
                }
            }
        }

        protected virtual void DisplayStates(IWorkflow workflow, XmlControl placeholder)
        {
            this._stateNames = null;
            foreach (WorkflowState state in workflow.GetStates())
            {
                if (WorkflowFilterer.FilterVisibleCommands(workflow.GetCommands(state.StateID)).Length > 0)
                {
                    string str2;
                    DataUri[] items = this.GetItems(state, workflow);
                    string str = ShortID.Encode(workflow.WorkflowID) + "_" + ShortID.Encode(state.StateID);
                    Section section2 = new Section
                    {
                        ID = str + "_section"
                    };
                    Section control = section2;
                    control.Attributes["Width"] = "100%";
                    placeholder.AddControl(control);
                    int length = items.Length;
                    if (length <= 0)
                    {
                        str2 = Translate.Text("None");
                    }
                    else if (length == 1)
                    {
                        str2 = string.Format("1 {0}", Translate.Text("item"));
                    }
                    else
                    {
                        str2 = string.Format("{0} {1}", length, Translate.Text("items"));
                    }
                    str2 = string.Format("<span style=\"font-weight:normal\"> - ({0})</span>", str2);
                    control.Header = state.DisplayName + str2;
                    control.Icon = state.Icon;
                    if (Settings.ClientFeeds.Enabled)
                    {
                        FeedUrlOptions options2 = new FeedUrlOptions("/sitecore/shell/~/feed/workflowstate.aspx")
                        {
                            UseUrlAuthentication = true
                        };
                        FeedUrlOptions options = options2;
                        options.Parameters["wf"] = workflow.WorkflowID;
                        options.Parameters["st"] = state.StateID;
                        control.FeedLink = options.ToString();
                    }
                    control.Collapsed = length <= 0;
                    Border child = new Border();
                    control.Controls.Add(child);
                    child.ID = str + "_content";
                    this.DisplayState(workflow, state, items, child, 0, this.PageSize);
                    this.CreateNavigator(control, str + "_navigator", length);
                }
            }
        }

        protected virtual void DisplayWorkflow(IWorkflow workflow)
        {
            Context.ClientPage.ServerProperties["WorkflowID"] = workflow.WorkflowID;
            XmlControl webControl = Resource.GetWebControl("Pane") as XmlControl;
            Error.AssertXmlControl(webControl, "Pane");
            this.States.Controls.Add(webControl);
            Assert.IsNotNull(webControl, "pane");
            webControl["PaneID"] = this.GetPaneID(workflow);
            webControl["Header"] = workflow.Appearance.DisplayName;
            webControl["Icon"] = workflow.Appearance.Icon;
            FeedUrlOptions options2 = new FeedUrlOptions("/sitecore/shell/~/feed/workflow.aspx")
            {
                UseUrlAuthentication = true
            };
            FeedUrlOptions options = options2;
            options.Parameters["wf"] = workflow.WorkflowID;
            webControl["FeedLink"] = options.ToString();
            this.DisplayStates(workflow, webControl);
        }

        private string GetHistory(IWorkflow workflow, Item item)
        {
            WorkflowEvent[] history = workflow.GetHistory(item);
            if (history.Length > 0)
            {
                WorkflowEvent event2 = history[history.Length - 1];
                string user = event2.User;
                string name = Context.Domain.Name;
                if (user.StartsWith(name + @"\", StringComparison.OrdinalIgnoreCase))
                {
                    user = StringUtil.Mid(user, name.Length + 1);
                }
                user = StringUtil.GetString(new string[] { user, Translate.Text("Unknown") });
                string stateName = this.GetStateName(workflow, event2.OldState);
                string str5 = this.GetStateName(workflow, event2.NewState);
                return string.Format(Translate.Text("{0} changed from <b>{1}</b> to <b>{2}</b> on {3}."), new object[] { user, stateName, str5, DateUtil.FormatDateTime(event2.Date, "D", Context.User.Profile.Culture) });
            }
            return Translate.Text("No changes have been made.");
        }

        private DataUri[] GetItems(WorkflowState state, IWorkflow workflow)
        {
            ArrayList list = new ArrayList();
            DataUri[] items = workflow.GetItems(state.StateID);
            if (items != null)
            {
                foreach (DataUri uri in items)
                {
                    Item item = Context.ContentDatabase.Items[uri];
                    if ((((item != null) && item.Access.CanRead()) && (item.Access.CanReadLanguage() && item.Access.CanWriteLanguage())) && ((Context.IsAdministrator || item.Locking.CanLock()) || item.Locking.HasLock()))
                    {
                        list.Add(uri);
                    }
                }
            }
            return (list.ToArray(typeof(DataUri)) as DataUri[]);
        }

        private string GetPaneID(IWorkflow workflow)
        {
            return ("P" + Regex.Replace(workflow.WorkflowID, @"\W", string.Empty));
        }

        private string GetStateName(IWorkflow workflow, string stateID)
        {
            if (this._stateNames == null)
            {
                this._stateNames = new NameValueCollection();
                foreach (WorkflowState state in workflow.GetStates())
                {
                    this._stateNames.Add(state.StateID, state.DisplayName);
                }
            }
            return StringUtil.GetString(new string[] { this._stateNames[stateID], "?" });
        }

        public override void HandleMessage(Message message)
        {
            switch (message.Name)
            {
                case "workflow:send":
                    this.Send(message);
                    return;

                case "workflow:sendselected":
                    this.SendSelected(message);
                    return;

                case "workflow:sendall":
                    this.SendAll(message);
                    return;

                case "window:close":
                    Windows.Close();
                    return;

                case "workflow:showhistory":
                    ShowHistory(message, Context.ClientPage.ClientRequest.Control);
                    return;

                case "workbox:hide":
                    Context.ClientPage.SendMessage(this, "pane:hide(id=" + message["id"] + ")");
                    Context.ClientPage.ClientResponse.SetAttribute("Check_Check_" + message["id"], "checked", "false");
                    break;

                case "pane:hidden":
                    Context.ClientPage.ClientResponse.SetAttribute("Check_Check_" + message["paneid"], "checked", "false");
                    break;

                case "workbox:show":
                    Context.ClientPage.SendMessage(this, "pane:show(id=" + message["id"] + ")");
                    Context.ClientPage.ClientResponse.SetAttribute("Check_Check_" + message["id"], "checked", "true");
                    break;

                case "pane:showed":
                    Context.ClientPage.ClientResponse.SetAttribute("Check_Check_" + message["paneid"], "checked", "true");
                    break;
            }
            base.HandleMessage(message);
            string str = message["id"];
            if ((str != null) && (str.Length > 0))
            {
                string name = StringUtil.GetString(new string[] { message["language"] });
                string str3 = StringUtil.GetString(new string[] { message["version"] });
                Item item = Context.ContentDatabase.Items[str, Language.Parse(name), Version.Parse(str3)];
                if (item != null)
                {
                    Dispatcher.Dispatch(message, item);
                }
            }
        }

        private void Jump(object sender, Message message, int offset)
        {
            string control = Context.ClientPage.ClientRequest.Control;
            string workflowID = ShortID.Decode(control.Substring(0, 0x20));
            string stateID = ShortID.Decode(control.Substring(0x21, 0x20));
            control = control.Substring(0, 0x41);
            IWorkflowProvider workflowProvider = Context.ContentDatabase.WorkflowProvider;
            Error.Assert(workflowProvider != null, "Workflow provider for database \"" + Context.ContentDatabase.Name + "\" not found.");
            IWorkflow workflow = workflowProvider.GetWorkflow(workflowID);
            Error.Assert(workflow != null, "Workflow \"" + workflowID + "\" not found.");
            Assert.IsNotNull(workflow, "workflow");
            WorkflowState state = workflow.GetState(stateID);
            Error.Assert(state != null, "Workflow state \"" + stateID + "\" not found.");
            Border border = new Border
            {
                ID = control + "_content"
            };
            DataUri[] items = this.GetItems(state, workflow);
            this.DisplayState(workflow, state, items, border, offset, this.PageSize);
            Context.ClientPage.ClientResponse.SetOuterHtml(control + "_content", border);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!Context.ClientPage.IsEvent)
            {
                IWorkflowProvider workflowProvider = Context.ContentDatabase.WorkflowProvider;
                if (workflowProvider != null)
                {
                    foreach (IWorkflow workflow in workflowProvider.GetWorkflows())
                    {
                        this.DisplayWorkflow(workflow);
                    }
                }
                this.UpdateRibbon();
            }
            this.WireUpNavigators(Context.ClientPage);
        }

        protected void OnViewMenuClick()
        {
            Menu control = new Menu();
            IWorkflowProvider workflowProvider = Context.ContentDatabase.WorkflowProvider;
            if (workflowProvider != null)
            {
                foreach (IWorkflow workflow in workflowProvider.GetWorkflows())
                {
                    string paneID = this.GetPaneID(workflow);
                    string str2 = Registry.GetString("/Current_User/Panes/" + paneID);
                    control.Add(Control.GetUniqueID("ctl"), workflow.Appearance.DisplayName, workflow.Appearance.Icon, string.Empty, ((str2 != "hidden") ? "workbox:hide" : "workbox:show") + "(id=" + paneID + ")", str2 != "hidden", string.Empty, MenuItemType.Check);
                }
                if (control.Controls.Count > 0)
                {
                    control.AddDivider();
                }
                control.Add("Refresh", "Applications/16x16/refresh.png", "Refresh");
            }
            Context.ClientPage.ClientResponse.ShowPopup("ViewMenu", "below", control);
        }

        protected void Open(string id, string language, string version)
        {
            string sectionID = RootSections.GetSectionID(id);
            UrlString str2 = new UrlString();
            str2.Append("ro", sectionID);
            str2.Append("fo", id);
            str2.Append("id", id);
            str2.Append("la", language);
            str2.Append("vs", version);
            Windows.RunApplication("Content editor", str2.ToString());
        }

        protected void PageSize_Change()
        {
            string str = Context.ClientPage.ClientRequest.Form["PageSize"];
            int @int = MainUtil.GetInt(str, 10);
            this.PageSize = @int;
            this.Refresh();
        }

        protected void Pane_Toggle(string id)
        {
            id = "P" + Regex.Replace(id, @"\W", string.Empty);
            if (Registry.GetString("/Current_User/Panes/" + id) == "hidden")
            {
                Registry.SetString("/Current_User/Panes/" + id, "visible");
                Context.ClientPage.ClientResponse.SetStyle(id, "display", "");
            }
            else
            {
                Registry.SetString("/Current_User/Panes/" + id, "hidden");
                Context.ClientPage.ClientResponse.SetStyle(id, "display", "none");
            }
            SheerResponse.SetReturnValue(true);
        }

        protected void Preview(string id, string language, string version)
        {
            Context.ClientPage.SendMessage(this, "item:preview(id=" + id + ",language=" + language + ",version=" + version + ")");
        }

        protected void Refresh()
        {
            Context.ClientPage.ClientResponse.SetLocation(string.Empty);
        }

        private void Send(Message message)
        {
            IWorkflowProvider workflowProvider = Context.ContentDatabase.WorkflowProvider;
            if (workflowProvider != null)
            {
                string workflowID = message["wf"];
                IWorkflow workflow = workflowProvider.GetWorkflow(workflowID);
                if (workflow != null)
                {
                    Item item = Context.ContentDatabase.Items[message["id"], Language.Parse(message["la"]), Version.Parse(message["vs"])];
                    if (item != null)
                    {
                        if ((message["ui"] != "1") && !(message["suppresscomment"] == "1"))
                        {
                            Context.ClientPage.ServerProperties["id"] = message["id"];
                            Context.ClientPage.ServerProperties["language"] = message["la"];
                            Context.ClientPage.ServerProperties["version"] = message["vs"];
                            Context.ClientPage.ServerProperties["command"] = message["command"];
                            Context.ClientPage.ServerProperties["workflowid"] = workflowID;
                            Context.ClientPage.Start(this, "Comment");
                        }
                        else
                        {
                            try
                            {
                                workflow.Execute(message["command"], item, string.Empty, true, new object[0]);
                            }
                            catch (WorkflowStateMissingException)
                            {
                                SheerResponse.Alert("One or more items could not be processed because their workflow state does not specify the next step.", new string[0]);
                            }
                            Context.ClientPage.ClientResponse.SetLocation(string.Empty);
                        }
                    }
                }
            }
        }

        private void SendAll(Message message)
        {
            IWorkflowProvider workflowProvider = Context.ContentDatabase.WorkflowProvider;
            if (workflowProvider != null)
            {
                string workflowID = message["wf"];
                string stateID = message["ws"];
                IWorkflow workflow = workflowProvider.GetWorkflow(workflowID);
                if (workflow != null)
                {
                    WorkflowState state = workflow.GetState(stateID);
                    DataUri[] items = this.GetItems(state, workflow);
                    string comments = (state != null) ? state.DisplayName : string.Empty;
                    bool flag = false;
                    foreach (DataUri uri in items)
                    {
                        Item item = Context.ContentDatabase.Items[uri];
                        if (item != null)
                        {
                            try
                            {
                                workflow.Execute(message["command"], item, comments, true, new object[0]);
                            }
                            catch (WorkflowStateMissingException)
                            {
                                flag = true;
                            }
                        }
                    }
                    if (flag)
                    {
                        SheerResponse.Alert("One or more items could not be processed because their workflow state does not specify the next step.", new string[0]);
                    }
                    Context.ClientPage.ClientResponse.SetLocation(string.Empty);
                }
            }
        }

        private void SendSelected(Message message)
        {
            IWorkflowProvider workflowProvider = Context.ContentDatabase.WorkflowProvider;
            if (workflowProvider != null)
            {
                string workflowID = message["wf"];
                string str2 = message["ws"];
                IWorkflow workflow = workflowProvider.GetWorkflow(workflowID);
                if (workflow != null)
                {
                    int num = 0;
                    bool flag = false;
                    foreach (string str3 in Context.ClientPage.ClientRequest.Form.Keys)
                    {
                        if ((str3 != null) && str3.StartsWith("check_"))
                        {
                            string str4 = "hidden_" + str3.Substring(6);
                            string[] strArray = Context.ClientPage.ClientRequest.Form[str4].Split(new char[] { ',' });
                            Item item = Context.ContentDatabase.Items[strArray[0], Language.Parse(strArray[1]), Version.Parse(strArray[2])];
                            if (item != null)
                            {
                                WorkflowState state = workflow.GetState(item);
                                if (state.StateID == str2)
                                {
                                    try
                                    {
                                        workflow.Execute(message["command"], item, state.DisplayName, true, new object[0]);
                                    }
                                    catch (WorkflowStateMissingException)
                                    {
                                        flag = true;
                                    }
                                    num++;
                                }
                            }
                        }
                    }
                    if (flag)
                    {
                        SheerResponse.Alert("One or more items could not be processed because their workflow state does not specify the next step.", new string[0]);
                    }
                    if (num == 0)
                    {
                        Context.ClientPage.ClientResponse.Alert("There are no selected items.");
                    }
                    else
                    {
                        Context.ClientPage.ClientResponse.SetLocation(string.Empty);
                    }
                }
            }
        }

        private static void ShowHistory(Message message, string control)
        {
            XmlControl webControl = Resource.GetWebControl("WorkboxHistory") as XmlControl;
            webControl["ItemID"] = message["id"];
            webControl["Language"] = message["la"];
            webControl["Version"] = message["vs"];
            webControl["WorkflowID"] = message["wf"];
            Context.ClientPage.ClientResponse.ShowPopup(control, "below", webControl);
        }

        private void UpdateRibbon()
        {
            Ribbon ctl = new Ribbon
            {
                ID = "WorkboxRibbon",
                CommandContext = new CommandContext()
            };
            Item item = Context.Database.GetItem("/sitecore/content/Applications/Workbox/Ribbon");
            Error.AssertItemFound(item, "/sitecore/content/Applications/Workbox/Ribbon");
            ctl.CommandContext.RibbonSourceUri = item.Uri;
            this.RibbonPanel.InnerHtml = HtmlUtil.RenderControl(ctl);
        }

        private void WireUpNavigators(System.Web.UI.Control control)
        {
            foreach (Control control2 in control.Controls)
            {
                Navigator navigator = control2 as Navigator;
                if (navigator != null)
                {
                    navigator.Jump += new Navigator.NavigatorDelegate(this.Jump);
                    navigator.Previous += new Navigator.NavigatorDelegate(this.Jump);
                    navigator.Next += new Navigator.NavigatorDelegate(this.Jump);
                }
                this.WireUpNavigators(control2);
            }
        }

        // Properties
        public int PageSize
        {
            get
            {
                return Registry.GetInt("/Current_User/Workbox/Page Size", 10);
            }
            set
            {
                Registry.SetInt("/Current_User/Workbox/Page Size", value);
            }
        }
    }




}
