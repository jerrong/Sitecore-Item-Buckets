using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Sitecore.Collections;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Clones;
using Sitecore.Data.Events;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Data.Validators;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.IO;
using Sitecore.ItemBucket.Kernel.Kernel.Util;
using Sitecore.Layouts;
using Sitecore.Links;
using Sitecore.Pipelines;
using Sitecore.Pipelines.Save;
using Sitecore.Pipelines.Search;
using Sitecore.Reflection;
using Sitecore.Resources;
using Sitecore.Search;
using Sitecore.SecurityModel;
using Sitecore.Shell;
using Sitecore.Shell.Applications.ContentEditor;
using Sitecore.Shell.Applications.ContentEditor.Galleries;
using Sitecore.Shell.Applications.ContentEditor.Gutters;
using Sitecore.Shell.Applications.ContentEditor.Pipelines.GetContentEditorFields;
using Sitecore.Shell.Applications.ContentManager;
using Sitecore.Shell.Applications.ContentManager.Sidebars;
using Sitecore.Shell.Framework;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Sites;
using Sitecore.StringExtensions;
using Sitecore.Text;
using Sitecore.Web;
using Sitecore.Web.Configuration;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Sheer;
using Sitecore.Web.UI.WebControls.Ribbons;
using Sitecore.Web.UI.XmlControls;
using Sitecore.Workflows;
using Sitecore.Xml;
using ContextMenu = Sitecore.Shell.Framework.ContextMenu;
using Control = Sitecore.Web.UI.HtmlControls.Control;
using Tree = Sitecore.Shell.Applications.ContentManager.Sidebars.Tree;
using ValidatorCollection = Sitecore.Data.Validators.ValidatorCollection;

namespace Sitecore.ItemBucket.Kernel.Kernel.Forms
{
    using Sitecore.ItemBucket.Kernel.ItemExtensions.Axes;

    class ExtendedContentEditorForm : ContentEditorForm
    {
        // Fields
        private ContextMenu _contextMenu;
        private bool _hasPendingUpdate;
        private ItemUri _lastFolder;
        private ItemUri _pendingUpdateItemUri;
        private Ribbon _ribbon;
        private Sidebar _sidebar;
        private string _validatorsKey;
        protected HtmlGenericControl Body;
        protected HtmlTableRow BottomBorder;
        protected PlaceHolder BrowserTitle;
        protected Border ContentEditor;
        protected DataContext ContentEditorDataContext;
        protected PlaceHolder ContentTreePlaceholder;
        protected PlaceHolder DocumentType;
        protected PlaceHolder Pager;
        protected PlaceHolder RibbonPlaceholder;
        protected HtmlTableRow SearchPanel;
        protected PlaceHolder Stylesheets;
        protected HtmlAnchor SystemMenu;
        protected PlaceHolder WindowButtonsPlaceholder;


        // Methods
        private void AcceptNotification(Message message, Item item)
        {
            Notification notification = this.GetNotification(message, item);
            if ((notification != null) && (item != null))
            {
                notification.Accept(item);
            }
        }

        protected ValidatorCollection BuildValidators(ValidatorsMode mode, Item folder)
        {
            Assert.ArgumentNotNull(folder, "folder");
            SafeDictionary<string, string> controls = new SafeDictionary<string, string>();
            foreach (FieldInfo info in this.FieldInfo.Values)
            {
                controls[info.FieldID.ToString()] = info.ID;
            }
            return ValidatorManager.BuildValidators(mode, this.ValidatorsKey, folder, controls);
        }

        private bool CanBeShown(Item strip)
        {
            if (strip.ID.ToString() == "{BB2FBB55-EBC8-4724-9A1F-37BCFED0370D}")
            {
                return true;
            }
            foreach (Item item in strip.Children)
            {
                Item item2 = item;
                if (item.TemplateID == TemplateIDs.Reference)
                {
                    item2 = item2.Database.Items[item2["Reference"]];
                }
                if ((item2 != null) && item2.HasChildren)
                {
                    return true;
                }
            }
            return false;
        }

        private void CheckModified(Message message)
        {
            Assert.ArgumentNotNull(message, "message");
            message.Result = false;
            foreach (FieldInfo info in this.FieldInfo.Values)
            {
                var control = Context.ClientPage.FindSubControl(info.ID);
                if (control != null)
                {
                    string str;
                    if (control is IContentField)
                    {
                        str = (control as IContentField).GetValue();
                    }
                    else
                    {
                        str = ReflectionUtil.GetProperty(control, "Value").ToString();
                    }
                    switch (info.Type.ToLowerInvariant())
                    {
                        case "html":
                        case "rich text":
                            str = XHtml.Convert(str);
                            break;
                    }
                    if (Crc.CRC(str) != info.Crc)
                    {
                        message.Result = true;
                        break;
                    }
                }
            }
        }

        protected void Close()
        {
            Windows.Close(CloseMethod.Logout);
        }

        protected void ClosePreviewWindow()
        {
            SheerResponse.CloseWindow();
        }

        private Notification CreateItemVersionNotification(Message message)
        {
            ID id;
            Language language;
            Sitecore.Data.Version version;
            Assert.ArgumentNotNull(message, "message");
            ItemVersionNotification notification = new ItemVersionNotification();
            if (!ID.TryParse(message["id"], out id))
            {
                return null;
            }
            if (!Language.TryParse(message["language"], out language))
            {
                return null;
            }
            if (!Sitecore.Data.Version.TryParse(message["version"], out version))
            {
                return null;
            }
            string str = message["database"];
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }
            notification.VersionUri = new ItemUri(id, id.ToString(), language, version, str);
            return notification;
        }

        protected void Drop(string data)
        {
            Assert.ArgumentNotNull(data, "data");
            if (data.StartsWith("sitecore:"))
            {
                data = data.Substring(9);
                if (data.StartsWith("item:"))
                {
                    data = data.Substring(5);
                    int length = data.LastIndexOf(",");
                    if (length > 0)
                    {
                        string iD = GetID(StringUtil.Left(data, length));
                        string path = GetID(StringUtil.Mid(data, length + 1));
                        Item target = Client.ContentDatabase.GetItem(path);
                        Item item = Client.ContentDatabase.GetItem(iD);
                        if ((target != null) && (item != null))
                        {
                            Items.DragTo(item, target, Context.ClientPage.ClientRequest.CtrlKey, !Context.ClientPage.ClientRequest.ShiftKey, !Context.ClientPage.ClientRequest.AltKey);
                        }
                        else if (item == null)
                        {
                            SheerResponse.Alert("The source item could not be found.\n\nIt may have been deleted by another user.", new string[0]);
                        }
                        else
                        {
                            SheerResponse.Alert("The target item could not be found.\n\nIt may have been deleted by another user.", new string[0]);
                        }
                    }
                }
            }
        }

        protected void DropAndSort(string data)
        {
            DragAction into;
            Assert.ArgumentNotNullOrEmpty(data, "data");
            string[] strArray = data.Split(new char[] { '|' });
            Assert.IsTrue(strArray.Length == 3, "Part count in DropAndSort data");
            string str = strArray[0];
            string str2 = strArray[1];
            string str3 = strArray[2];
            bool ctrlKey = Context.ClientPage.ClientRequest.CtrlKey;
            bool confirm = !Context.ClientPage.ClientRequest.ShiftKey;
            Item target = Client.ContentDatabase.GetItem(ShortID.Parse(str3).ToID());
            Item item = Client.ContentDatabase.GetItem(ShortID.Parse(str).ToID());
            string str4 = str2;
            if (str4 != null)
            {
                if (!(str4 == "before"))
                {
                    if (str4 == "into")
                    {
                        into = DragAction.Into;
                        goto Label_00F1;
                    }
                    if (str4 == "after")
                    {
                        into = DragAction.After;
                        goto Label_00F1;
                    }
                }
                else
                {
                    into = DragAction.Before;
                    goto Label_00F1;
                }
            }
            throw new Exception("Unexpected drag action '{0}'. Only 'before', 'after' and 'into' are allowed.".FormatWith(new object[] { str2 }));
        Label_00F1:
            if ((target != null) && (item != null))
            {
                Items.DragTo(item, target, ctrlKey, confirm, into);
            }
            else if (item == null)
            {
                SheerResponse.Alert("The source item could not be found.\n\nIt may have been deleted by another user.", new string[0]);
            }
            else
            {
                SheerResponse.Alert("The target item could not be found.\n\nIt may have been deleted by another user.", new string[0]);
            }
        }

        protected void DropInRichText(string data)
        {
            Assert.ArgumentNotNull(data, "data");
            if (!string.IsNullOrEmpty(data) && data.StartsWith("sitecore:"))
            {
                data = data.Substring(9);
                data.StartsWith("item:");
            }
        }

        protected void EntireTree_Click()
        {
            ClientPipelineArgs currentPipelineArgs = Context.ClientPage.CurrentPipelineArgs as ClientPipelineArgs;
            Assert.IsNotNull(currentPipelineArgs, typeof(ClientPipelineArgs));
            if (SheerResponse.CheckModified())
            {
                UserOptions.View.ShowEntireTree = !UserOptions.View.ShowEntireTree;
                Item folder = this.ContentEditorDataContext.GetFolder();
                if (folder != null)
                {
                    UrlString urlString = new UrlString(WebUtil.GetRawUrl());
                    folder.Uri.AddToUrlString(urlString);
                    urlString["pa" + WebUtil.GetQueryString("pa", "0")] = folder.Uri.ToString();
                    urlString["ras"] = "ViewStrip";
                    SheerResponse.SetLocation(urlString.ToString());
                }
                else
                {
                    SheerResponse.SetLocation(string.Empty);
                }
            }
        }

        protected void FieldMenu_Click(string id)
        {
            Assert.ArgumentNotNullOrEmpty(id, "id");
            Item fieldMenu = this.GetFieldMenu(id);
            if (fieldMenu != null)
            {
                ChildList children = fieldMenu.Children;
                if (children.Count > 0)
                {
                    Item item2 = children[0];
                    if (item2 != null)
                    {
                        string message = item2["Message"].Replace("$Target", id);
                        Context.ClientPage.SendMessage(this, message);
                    }
                }
            }
        }

        protected void FieldMenu_DropDown(string id)
        {
            Assert.ArgumentNotNullOrEmpty(id, "id");
            Item fieldMenu = this.GetFieldMenu(id);
            if (fieldMenu != null)
            {

                SheerResponse.DisableOutput();
                var menu2 = new Sitecore.Web.UI.HtmlControls.Menu
                {
                    ID = "M"
                };
                var control = menu2;
                CommandContext commandContext = new CommandContext(this.ContentEditorDataContext.GetFolder());
                control.AddFromDataSource(fieldMenu, id, commandContext);
                SheerResponse.EnableOutput();
                SheerResponse.ShowPopup(id + "_menu", "below", control);
            }
        }

        public static Item GetContextItem()
        {
            DataContext context = Client.Page.Page.FindControl("ContentEditorDataContext") as DataContext;
            if (context == null)
            {
                return null;
            }
            return context.GetFolder();
        }

        private static ContextMenu GetContextMenu()
        {
            ContextMenu result = new ContextMenu
            {
                ID = "ContextMenu"
            };
            return Assert.ResultNotNull<ContextMenu>(result);
        }

        private Item GetCurrentItem(Message message)
        {
            Assert.ArgumentNotNull(message, "message");
            string str = message["id"];
            Item folder = this.ContentEditorDataContext.GetFolder();
            if (folder == null)
            {
                return null;
            }
            if (!string.IsNullOrEmpty(str))
            {
                return Client.ContentDatabase.GetItem(str, folder.Language);
            }
            return folder;
        }

        private Item GetFieldMenu(string id)
        {
            Assert.ArgumentNotNullOrEmpty(id, "id");
            FieldInfo info = this.FieldInfo[id] as FieldInfo;
            if (info == null)
            {
                return null;
            }
            Item item = Client.ContentDatabase.GetItem(info.ItemID);
            if (item == null)
            {
                return null;
            }
            Field field = item.Fields[info.FieldID];
            Item fieldTypeItem = FieldTypeManager.GetFieldTypeItem(field);
            if (fieldTypeItem == null)
            {
                return null;
            }
            return fieldTypeItem.Children["Menu"];
        }

        private static NameValueCollection GetFieldValues()
        {
            NameValueCollection values = new NameValueCollection();
            foreach (string str in HttpContext.Current.Request.Form.Keys)
            {
                if (!string.IsNullOrEmpty(str) && str.StartsWith("SearchOptionsName"))
                {
                    string str2 = StringUtil.Mid(str, 0x11);
                    string str3 = HttpContext.Current.Request.Form["SearchOptionsName" + str2];
                    string str4 = HttpContext.Current.Request.Form["SearchOptionsValue" + str2];
                    if (!string.IsNullOrEmpty(str3.Trim()) && !string.IsNullOrEmpty(str4.Trim()))
                    {
                        values[str3] = str4;
                    }
                }
            }
            return values;
        }

        private static string GetID(string id)
        {
            Assert.ArgumentNotNull(id, "id");
            int num = id.LastIndexOf("_");
            if (num >= 0)
            {
                id = StringUtil.Mid(id, num + 1);
            }
            if (ShortID.IsShortID(id))
            {
                id = ShortID.Decode(id);
            }
            return id;
        }

        private Notification GetNotification(Message message, Item item)
        {
            ID id;
            Assert.ArgumentNotNull(message, "message");
            Assert.ArgumentNotNull(item, "item");
            if (item == null)
            {
                return null;
            }
            if (item.Database.NotificationProvider == null)
            {
                return null;
            }
            string str = message["notification"];
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }
            if (string.Compare(str, "itemversionnotification", true) == 0)
            {
                return this.CreateItemVersionNotification(message);
            }
            if (!ID.TryParse(str, out id))
            {
                return null;
            }
            return item.Database.NotificationProvider.GetNotification(id);
        }

        private QueryBase GetQuery()
        {
            QueryBase base2 = null;
            NameValueCollection fieldValues = GetFieldValues();
            string str = StringUtil.GetString(new string[] { Context.ClientPage.ClientRequest.Form["TreeSearch"] });
            if (str == "Search")
            {
                str = string.Empty;
            }
            str = str.Trim();
            if (!string.IsNullOrEmpty(str))
            {
                base2 = new FullTextQuery(str);
            }
            if (fieldValues.Count <= 0)
            {
                return base2;
            }
            CombinedQuery query = new CombinedQuery();
            foreach (string str2 in fieldValues.AllKeys)
            {
                string fieldValue = fieldValues[str2];
                if (fieldValue.Length > 0)
                {
                    query.Add(new FieldQuery(str2.ToLowerInvariant(), fieldValue), QueryOccurance.Should);
                }
            }
            return query;
        }

        private static Ribbon GetRibbon()
        {
            Ribbon result = new Ribbon
            {
                ID = "Ribbon"
            };
            return Assert.ResultNotNull<Ribbon>(result);
        }

        private static Packet GetSavePacket(Hashtable fieldInfo)
        {
            Assert.ArgumentNotNull(fieldInfo, "fieldInfo");
            Packet result = new Packet();
            foreach (FieldInfo info in fieldInfo.Values)
            {
                var control = Context.ClientPage.FindSubControl(info.ID);
                if (control != null)
                {
                    string str;
                    if (control is IContentField)
                    {
                        str = StringUtil.GetString(new string[] { (control as IContentField).GetValue() });
                    }
                    else
                    {
                        str = StringUtil.GetString(ReflectionUtil.GetProperty(control, "Value"));
                    }
                    if (str != "__#!$No value$!#__")
                    {
                        result.StartElement("field");
                        result.SetAttribute("itemid", info.ItemID.ToString());
                        result.SetAttribute("language", info.Language.ToString());
                        result.SetAttribute("version", info.Version.ToString());
                        result.SetAttribute("itemrevision", info.Revision.ToString());
                        switch (info.Type.ToLowerInvariant())
                        {
                            case "rich text":
                            case "html":
                                str = str.TrimEnd(new char[] { ' ' });
                                break;
                        }
                        result.SetAttribute("fieldid", info.FieldID.ToString());
                        result.AddElement("value", str, new string[0]);
                        result.EndElement();
                    }
                }
            }
            return Assert.ResultNotNull<Packet>(result);
        }

        private static Sidebar GetSidebar()
        {
            Sidebar result = new Tree
            {
                ID = "Tree"
            };
            return Assert.ResultNotNull<Sidebar>(result);
        }

        private Language GetTranslatingLanguage(Item folder)
        {
            Assert.ArgumentNotNull(folder, "folder");
            Language language = folder.Language;
            if (this.TranslatingLanguage.Length > 0)
            {
                language = Language.Parse(this.TranslatingLanguage);
            }
            return language;
        }

        private static string GetUri(Item item)
        {
            Assert.ArgumentNotNull(item, "item");
            return item.Uri.ToString().Replace("?", "%26");
        }

        private static UrlString GetUrl(Item item)
        {
            Assert.ArgumentNotNull(item, "item");
            UrlString result = new UrlString(UIUtil.GetUri(item["Source"]));
            result["mo"] = WebUtil.GetQueryString("mo");
            if (WebUtil.GetQueryString("we") == "1")
            {
                result["we"] = "1";
            }
            result["he"] = item["Header"];
            return Assert.ResultNotNull<UrlString>(result);
        }

        protected ValidatorCollection GetValidators()
        {
            Assert.IsTrue(UserOptions.ContentEditor.ShowValidatorBar, "Validator bar is switched off in Content Editor.");
            return ValidatorManager.GetValidators(ValidatorsMode.ValidatorBar, this.ValidatorsKey);
        }

        protected void Gutter_ContextMenu()
        {
            var contextMenu = new Sitecore.Web.UI.HtmlControls.Menu();
            SheerResponse.DisableOutput();
            Item item = Context.Database.GetItem(Sitecore.ItemBucket.Kernel.Util.Constants.ContentEditorGutterFolder);
            Assert.IsNotNull(item, typeof(Item), "Item \"/sitecore/content/Applications/Content Editor/Gutters\" not found", new object[0]);
            List<ID> activeRendererIDs = GutterManager.GetActiveRendererIDs();
            foreach (Item item2 in item.Children)
            {
                contextMenu.Add("M" + item2.ID.ToShortID(), item2["Header"], item2.Appearance.Icon, string.Empty, "SetGutterRenderer(\"" + item2.ID + "\")", activeRendererIDs.Contains(item2.ID), string.Empty, MenuItemType.Check);
            }
            contextMenu.AddDivider();
            contextMenu.Add("__Refresh", "Refresh", "Applications/16x16/refresh.png", string.Empty, "Gutter_Refresh", false, string.Empty, MenuItemType.Normal);
            SheerResponse.EnableOutput();
            SheerResponse.ShowContextMenu(Context.ClientPage.ClientRequest.Control, string.Empty, contextMenu);
        }

        protected void Gutter_Refresh()
        {
            Item item;
            Item item2;
            this.ContentEditorDataContext.GetState(out item2, out item);
            this._sidebar.ChangeRoot(item2, item);
        }

        public override void HandleMessage(Message message)
        {
            Assert.ArgumentNotNull(message, "message");
            switch (message.Name)
            {
                case "datacontext:changed":
                    if (message["id"] == "ContentEditorDataContext")
                    {
                        this.OnDataContextChanged(message);
                    }
                    break;

                case "item:iscontenteditor":
                    message.Result = true;
                    break;

                case "item:isediting":
                    message.Result = this.IsEditing(message["id"]);
                    break;

                case "item:load":
                case "item:versionadded":
                    this.LoadItem(message);
                    break;

                case "item:modified":
                    this.CheckModified(message);
                    break;

                case "item:save":
                    this.Save(message);
                    return;

                case "item:updated":
                    if ((this.ContentEditorDataContext.CurrentItem != null) && (this.ContentEditorDataContext.CurrentItem.ID.ToString() == message["id"]))
                    {
                        this.LoadItem(message);
                    }
                    return;

                case "item:workflowhistory":
                    this.Workflow_History();
                    return;

                case "item:refreshchildren":
                    this.Tree_Refresh(message["id"]);
                    break;

                case "item:checkedin":
                case "item:checkedout":
                case "item:refresh":
                case "item:templatefieldadded":
                case "item:templatechanged":
                case "item:templatefielddeleted":
                    this.ContentEditorDataContext.Refresh();
                    break;

                case "shell:useroptionschanged":
                    SheerResponse.CheckModified(false);
                    SheerResponse.SetLocation(string.Empty);
                    return;

                case "item:updategutter":
                    this.UpdateGutter(message["id"]);
                    break;

                case "contenteditor:switchto":
                    this.SwitchTo(HttpUtility.UrlDecode(message["target"]));
                    break;

                case "contenteditor:showvalidationresult":
                    this.ShowValidationResult(this.GetCurrentItem(message));
                    return;

                case "notification:accept":
                    this.AcceptNotification(message, this.GetCurrentItem(message));
                    this.Reload();
                    return;

                case "notification:reject":
                    this.RejectNotification(message, this.GetCurrentItem(message));
                    this.Reload();
                    return;
            }
            if (!string.IsNullOrEmpty(this.FrameName))
            {
                message.Arguments.Add("frameName", this.FrameName);
            }
            Dispatcher.Dispatch(message, this.GetCurrentItem(message));
        }

        private static void HandleSections(UrlString sections)
        {
            Assert.ArgumentNotNull(sections, "sections");
            UrlString str = new UrlString(Registry.GetString("/Current_User/Content Editor/Sections/Collapsed"));
            foreach (string str2 in sections.Parameters.Keys)
            {
                str[HttpUtility.UrlDecode(str2)] = sections[str2];
            }
            Registry.SetString("/Current_User/Content Editor/Sections/Collapsed", str.ToString());
            SheerResponse.SetAttribute("scSections", "value", string.Empty);
        }

        protected void HiddenItems_Click()
        {
            bool flag = !UserOptions.View.ShowHiddenItems;
            UserOptions.View.ShowHiddenItems = flag;
            Item folder = this.ContentEditorDataContext.GetFolder();
            if ((folder != null) && (flag || !folder.Appearance.Hidden))
            {
                UrlString urlString = new UrlString(WebUtil.GetRawUrl());
                folder.Uri.AddToUrlString(urlString);
                urlString["pa" + WebUtil.GetQueryString("pa", "0")] = folder.Uri.ToString();
                urlString["ras"] = "ViewStrip";
                SheerResponse.SetLocation(urlString.ToString());
            }
            else
            {
                SheerResponse.SetLocation(string.Empty);
            }
        }

        private bool IsEditing(string id)
        {
            Assert.ArgumentNotNull(id, "id");
            Item folder = this.ContentEditorDataContext.GetFolder();
            if ((folder == null) || !(folder.ID.ToString() == id))
            {
                return false;
            }
            return (folder.State.CanSave() && !MainUtil.GetBool(Context.ClientPage.ServerProperties["ContentReadOnly"], true));
        }

        private void ItemCopiedNotification(object sender, ItemCopiedEventArgs args)
        {
            Assert.ArgumentNotNull(sender, "sender");
            Assert.ArgumentNotNull(args, "args");
            Item folder = this.ContentEditorDataContext.GetFolder();
            if (Settings.ContentEditor.ClassicFrameEventPropagation || ((folder != null) && ((folder.ID == args.Copy.ID) || (folder.ID == args.Copy.ParentID))))
            {
                SheerResponse.Eval("scContent.postMessage(\"" + ("notification:itemcopied(source=" + GetUri(args.Source) + ",copy=" + GetUri(args.Copy) + ")") + "\")");
            }
        }

        private void ItemCreatedNotification(object sender, ItemCreatedEventArgs args)
        {
            Assert.ArgumentNotNull(sender, "sender");
            Assert.ArgumentNotNull(args, "args");
            Item folder = this.ContentEditorDataContext.GetFolder();
            if (Settings.ContentEditor.ClassicFrameEventPropagation || ((folder != null) && ((folder.ID == args.Item.ID) || (folder.ID == args.Item.ParentID))))
            {
                SheerResponse.Eval("scContent.postMessage(\"" + ("notification:itemadded(item=" + GetUri(args.Item) + ")") + "\")");
            }
        }

        private void ItemDeletedNotification(object sender, ItemDeletedEventArgs args)
        {
            Assert.ArgumentNotNull(sender, "sender");
            Assert.ArgumentNotNull(args, "args");
            if ((WebUtil.GetQueryString("mo") == "preview") && (args.Item.ID.ToString() == WebUtil.GetQueryString("fo")))
            {
                SheerResponse.SetAttribute("scPostAction", "value", "back:" + Sitecore.Globalization.Translate.Text("The current item has been deleted.\n\nThe browser will now go back to the previous page.\n\nIf the previous page has been cached by the browser,\nthe latest changes may not be visible. In that case\nclick the Refresh button to reload the page."));
            }
            Item folder = this.ContentEditorDataContext.GetFolder();
            if (Settings.ContentEditor.ClassicFrameEventPropagation || ((folder != null) && ((folder.ID == args.Item.ID) || (folder.ID == args.ParentID))))
            {
                SheerResponse.Eval("scContent.postMessage(\"" + string.Concat(new object[] { "notification:itemdeleted(item=", GetUri(args.Item), ",parentid=", args.ParentID, ",index=", args.Index, ")" }) + "\")");
            }
        }

        private void ItemMovedNotification(object sender, ItemMovedEventArgs args)
        {
            Assert.ArgumentNotNull(sender, "sender");
            Assert.ArgumentNotNull(args, "args");
            if ((WebUtil.GetQueryString("mo") == "preview") && (args.Item.ID.ToString() == WebUtil.GetQueryString("fo")))
            {
                UrlOptions defaultOptions = UrlOptions.DefaultOptions;
                defaultOptions.ShortenUrls = false;
                SheerResponse.SetAttribute("scPostAction", "value", "goto:" + LinkManager.GetItemUrl(args.Item, defaultOptions));
            }
            Item folder = this.ContentEditorDataContext.GetFolder();
            if (Settings.ContentEditor.ClassicFrameEventPropagation || ((folder != null) && (((folder.ID == args.Item.ID) || (folder.ID == args.OldParentID)) || (folder.ID == args.Item.ParentID))))
            {
                string str = string.Concat(new object[] { "notification:itemmoved(item=", GetUri(args.Item), ",oldparentid=", args.OldParentID, ")" });
                Context.ClientPage.ClientResponse.Eval("scContent.postMessage(\"" + str + "\")");
            }
        }

        private void ItemRenamedNotification(object sender, ItemRenamedEventArgs args)
        {
            Assert.ArgumentNotNull(sender, "sender");
            Assert.ArgumentNotNull(args, "args");
            if ((WebUtil.GetQueryString("mo") == "preview") && (args.Item.ID.ToString() == WebUtil.GetQueryString("fo")))
            {
                UrlOptions defaultOptions = UrlOptions.DefaultOptions;
                defaultOptions.ShortenUrls = false;
                SheerResponse.SetAttribute("scPostAction", "value", "goto:" + LinkManager.GetItemUrl(args.Item, defaultOptions));
            }
            Item folder = this.ContentEditorDataContext.GetFolder();
            if (Settings.ContentEditor.ClassicFrameEventPropagation || ((folder != null) && ((folder.ID == args.Item.ID) || (folder.ID == args.Item.ParentID))))
            {
                SheerResponse.Eval("scContent.postMessage(\"" + string.Concat(new object[] { "notification:itemrenamed(item=", GetUri(args.Item), ",oldname=", args.OldName, ")" }) + "\")");
            }
        }

        private void ItemSavedNotification(object sender, ItemSavedEventArgs args)
        {
            Assert.ArgumentNotNull(sender, "sender");
            Assert.ArgumentNotNull(args, "args");
            Item folder = this.ContentEditorDataContext.GetFolder();
            if (Settings.ContentEditor.ClassicFrameEventPropagation || ((folder != null) && ((folder.ID == args.Item.ID) || (folder.ID == args.Item.ParentID))))
            {
                SheerResponse.Eval("scContent.postMessage(\"" + ("notification:itemsaved(item=" + GetUri(args.Item) + ")") + "\")");
            }
            UpdateGutter(args.Item);
        }


        protected void LoadItem(Message message)
        {
            Assert.ArgumentNotNull(message, "message");
            string path = StringUtil.GetString(new string[] { message["id"] });
            string name = StringUtil.GetString(new string[] { message["language"] });
            string str3 = StringUtil.GetString(new string[] { message["version"] });
            string str4 = StringUtil.GetString(new string[] { message["root"] });
            this.DisableHistory = StringUtil.GetString(new string[] { message["disablehistory"] }) == "1";
            if (name.Length == 0)
            {
                name = this.ContentEditorDataContext.Language.ToString();
            }
            ItemUri uri = new ItemUri(path, Sitecore.Globalization.Language.Parse(name), Sitecore.Data.Version.Parse(str3), Client.ContentDatabase.Name);
            Item item = Database.GetItem(uri);
            if (item == null)
            {
                SheerResponse.Alert("The selected item could not be found.\n\nIt may have been deleted by another user.\n\nSelect another item.", new string[0]);
            }
            else
            {
                //if (string.IsNullOrEmpty(str4))
                //{
                //    Item root = this.ContentEditorDataContext.GetRoot();
                //    if (((item.ID != root.ID) && !root.Axes.IsAncestorOf(item)) || IsHidden(item, root))
                //    {
                //        SheerResponse.Alert("The item that you are attempting to open is stored in a location that is not visible in the content tree.\n\nItem: {0}", new string[] { item.Paths.Path });
                //        return;
                //    }
                //}
                if (message["force"] == "1")
                {
                    if (!string.IsNullOrEmpty(str4))
                    {
                        this.ContentEditorDataContext.SetRootAndFolder(str4, uri);
                    }
                    else
                    {
                        this.ContentEditorDataContext.SetFolder(uri);
                    }
                }
                else
                {
                    this.LoadItem(uri, str4);
                }
            }
        }

        protected void LoadItem(string id)
        {
            Assert.ArgumentNotNullOrEmpty(id, "id");
            if (ShortID.IsShortID(id))
            {
                id = ShortID.Decode(id);
            }
            ItemUri uri = new ItemUri(id, this.ContentEditorDataContext.Language, Context.ContentDatabase);
            this.LoadItem(uri, string.Empty);
        }

        protected void LoadItem(ItemUri uri, string root)
        {
            Assert.ArgumentNotNull(uri, "uri");
            Assert.ArgumentNotNull(root, "root");
            if (Database.GetItem(uri) == null)
            {
                SheerResponse.Alert("The selected item could not be found.\n\nIt may have been deleted by another user.\n\nSelect another item.", new string[0]);
            }
            else
            {
                NameValueCollection values2 = new NameValueCollection();
                values2.Add("uri", uri.ToString());
                values2.Add("root", root);
                values2.Add("disablehistory", this.DisableHistory ? "1" : "0");
                NameValueCollection parameters = values2;
                Context.ClientPage.Start(this, "LoadItemPipeline", parameters);
            }
        }

        protected void LoadItem(string id, string language, string version)
        {
            Assert.ArgumentNotNullOrEmpty(id, "id");
            Assert.ArgumentNotNull(language, "language");
            Assert.ArgumentNotNull(version, "version");
            ItemUri uri = new ItemUri(id, Language.Parse(language), Sitecore.Data.Version.Parse(version), Context.ContentDatabase);
            this.LoadItem(uri, string.Empty);
        }

        protected void LoadItemPipeline(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            if (SheerResponse.CheckModified())
            {
                this.DisableHistory = args.Parameters["disablehistory"] == "1";
                ItemUri uri = ItemUri.Parse(args.Parameters["uri"]);
                string root = args.Parameters["root"];
                if (root.Length > 0)
                {
                    this.ContentEditorDataContext.SetRootAndFolder(root, uri);
                }
                else
                {
                    this.ContentEditorDataContext.SetFolder(uri);
                }
                Context.ClientPage.Modified = false;
            }
        }

        protected void NavigatorMenu_DropDown()
        {
            Item item;
            Item item2;
            this.ContentEditorDataContext.GetState(out item2, out item);
            if (item != null)
            {
                Hashtable fieldInfo = this.FieldInfo;
                Editor.Sections sections = new Editor.Sections();
                GetContentEditorFieldsArgs args = new GetContentEditorFieldsArgs(item)
                {
                    Sections = sections,
                    FieldInfo = new Hashtable(),
                    ShowDataFieldsOnly = !UserOptions.ContentEditor.ShowSystemFields
                };
                using (new LongRunningOperationWatcher(Settings.Profiling.RenderFieldThreshold, "getContentEditorFields pipeline[id={0}]", new string[] { item.ID.ToString() }))
                {
                    CorePipeline.Run("getContentEditorFields", args);
                }
                HtmlTextWriter writer = new HtmlTextWriter(new StringWriter());
                writer.Write("<table class=\"scEditorHeaderNavigatorMenu\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" onclick=\"javascript:return scForm.Content.scrollTo(this, event);\">");
                foreach (Editor.Section section in sections)
                {
                    writer.Write("<tr><td><a id=\"Nav_" + section.ControlID + "\" href=\"#\" class=\"scEditorHeaderNavigatorSection\">" + section.DisplayName + "<a></td></tr>");
                    foreach (Editor.Field field in section.Fields)
                    {
                        string controlID = field.ControlID;
                        foreach (FieldInfo info in fieldInfo.Values)
                        {
                            if (info.FieldID == field.TemplateField.ID)
                            {
                                controlID = info.ID;
                                break;
                            }
                        }
                        string str2 = WebUtil.SafeEncode(StringUtil.GetString(new string[] { field.TemplateField.GetTitle(Context.Language), field.TemplateField.Name }));
                        writer.Write("<tr><td><a id=\"Nav_" + controlID + "\" href=\"#\" class=\"scEditorHeaderNavigatorField\">" + str2 + "<a></td></tr>");
                    }
                }
                writer.Write("</table>");
                SheerResponse.ShowPopup("ContentEditorNavigator", "below", writer.InnerWriter.ToString());
            }
        }

        protected void OnDataContextChanged(Message message)
        {
            Assert.ArgumentNotNull(message, "message");
            Sidebar sidebar = this.Sidebar;
            if ((sidebar != null) && sidebar.OnDataContextChanged(this.ContentEditorDataContext, message))
            {
                Item folder = this.ContentEditorDataContext.GetFolder();
                if ((folder != null) && (!this.HasPendingUpdate || (this.PendingUpdateItemUri.ItemID == folder.ID)))
                {
                    this.PendingUpdateItemUri = folder.Uri;
                    this.HasPendingUpdate = true;
                }
            }
        }

        protected void OnInit(EventArgs e)
        {
            Assert.ArgumentNotNull(e, "e");
            this._sidebar = GetSidebar();
            this._ribbon = GetRibbon();
            this._contextMenu = GetContextMenu();
        }

        protected override void OnLoad(EventArgs e)
        {
            Item folder;
            Assert.ArgumentNotNull(e, "e");

            Assert.CanRunApplication("Content Editor");
            if (!Context.ClientPage.IsEvent)
            {
                string str2;
                AttributeCollection attributes;
                this.SetDocumentType();
                this.SetWebEditStylesheet();
                (attributes = this.Body.Attributes)["class"] = attributes["class"] + " " + UIUtil.GetBrowserClassString();
                this.ContentEditorDataContext.GetFromQueryString();
                this.ContentEditorDataContext.BeginUpdate();
                string queryString = WebUtil.GetQueryString("pa" + WebUtil.GetQueryString("pa", "0"));
                if (!string.IsNullOrEmpty(queryString))
                {
                    folder = Database.GetItem(ItemUri.Parse(HttpUtility.UrlDecode(queryString)));
                    if (folder != null)
                    {
                        this.ContentEditorDataContext.SetFolder(folder.Uri);
                    }
                }
                Database contentDatabase = Context.ContentDatabase;
                Item item2 = this.ContentEditorDataContext.GetRoot();
                if (item2 != null)
                {
                    contentDatabase = item2.Database;
                }
                this.ResolveLanguage(contentDatabase);
                this.ContentEditorDataContext.EndUpdate();
                folder = this.ContentEditorDataContext.GetFolder();
                this.CurrentRoot = this.ContentEditorDataContext.GetRoot().ID.ToString();
                this.ValidatorsKey = "VK_" + ID.NewID.ToShortID();
                int num = Settings.Validators.AutomaticUpdate ? Settings.Validators.UpdateDelay : 0;
                var control = Context.ClientPage.FindControl("ContentEditorForm");
                Assert.IsNotNull(control, "Form \"ContentEditorForm\" not found.");
                control.Controls.Add(new LiteralControl("<input type=\"hidden\" id=\"__CurrentItem\" name=\"__CurrentItem\" value=\"" + folder.Uri + "\"/>"));
                control.Controls.Add(new LiteralControl("<input type=\"hidden\" id=\"scValidatorsKey\" name=\"scValidatorsKey\" value=\"" + this.ValidatorsKey + "\"/>"));
                control.Controls.Add(new LiteralControl("<input type=\"hidden\" id=\"scValidatorsUpdateDelay\" name=\"scValidatorsUpdateDelay\" value=\"" + num + "\"/>"));
                if (WebUtil.GetQueryString("mo") == "preview")
                {
                    str2 = "Shell";
                }
                else
                {
                    str2 = "CE_" + ID.NewID.ToShortID();
                }
                control.Controls.Add(new LiteralControl(string.Format("<input id=\"__FRAMENAME\" name=\"__FRAMENAME\" type=\"hidden\" value=\"{0}\"/>", str2)));
                this.FrameName = str2;
                this.RenderPager();
                this.RenderButtons();
                if (!UserOptions.ContentEditor.ShowSearchPanel)
                {
                    this.SearchPanel.Style["display"] = "none";
                }
                this.BrowserTitle.Controls.Add(new LiteralControl("<title>" + Client.Site.BrowserTitle + " - Sitecore Content Editor</title>"));
                PlaceHolder holder = Context.ClientPage.FindControl("scLanguage") as PlaceHolder;
                Assert.IsNotNull(holder, "language");
                holder.Controls.Add(new LiteralControl("<input type=\"hidden\" id=\"scLanguage\" name=\"scLanguage\" value=\"" + folder.Language + "\" />"));
                var child = this.Body.FindControl("RadSpell");
                holder.Controls.Add(child);
            }
            Item root = this.ContentEditorDataContext.GetRoot();
            folder = this.ContentEditorDataContext.GetFolder();
            //if (!Sitecore.Context.Item.IsABucket())
            //{
            this.Sidebar.Initialize(this, folder, root);
            //}
            SiteContext site = Client.Site;
            site.Notifications.ItemDeleted += new ItemDeletedDelegate(this.ItemDeletedNotification);
            site.Notifications.ItemMoved += new ItemMovedDelegate(this.ItemMovedNotification);
            site.Notifications.ItemRenamed += new ItemRenamedDelegate(this.ItemRenamedNotification);
            site.Notifications.ItemCopied += new ItemCopiedDelegate(this.ItemCopiedNotification);
            site.Notifications.ItemCreated += new ItemCreatedDelegate(this.ItemCreatedNotification);
            site.Notifications.ItemSaved += new ItemSavedDelegate(this.ItemSavedNotification);
            this._lastFolder = folder.Uri;
            string formValue = WebUtil.GetFormValue("scSections");
            if (!string.IsNullOrEmpty(formValue))
            {
                HandleSections(new UrlString(formValue));
            }
            GalleryManager.SetGallerySizes();
        }

        protected void OnPreRendered(EventArgs e)
        {
            Assert.ArgumentNotNull(e, "e");
            Item folder = this.ContentEditorDataContext.GetFolder();
            if (this.HasPendingUpdate)
            {
                if (this.PendingUpdateItemUri != null)
                {
                    this.ContentEditorDataContext.DisableEvents();
                    this.ContentEditorDataContext.SetFolder(this.PendingUpdateItemUri);
                    this.ContentEditorDataContext.EnableEvents();
                }
                this.Update();
            }
            else
            {
                this.Sidebar.Update(folder.ID, false);
            }
        }

        [Obsolete]
        protected void Open(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            if (args.IsPostBack)
            {
                if (args.HasResult)
                {
                    Context.ClientPage.SendMessage(this, "item:load(id=" + args.Result + ")");
                }
            }
            else
            {
                Item folder = this.ContentEditorDataContext.GetFolder();
                if (folder != null)
                {
                    Dialogs.BrowseItem("Open Item.", string.Empty, "Core3/16x16/open_document.png", "Open", "/", folder.ID + "/");
                    args.WaitForPostBack();
                }
            }
        }

        protected void RawValues_Click()
        {
            ClientPipelineArgs currentPipelineArgs = Context.ClientPage.CurrentPipelineArgs as ClientPipelineArgs;
            Assert.IsNotNull(currentPipelineArgs, typeof(ClientPipelineArgs));
            if (SheerResponse.CheckModified())
            {
                UserOptions.ContentEditor.ShowRawValues = !UserOptions.ContentEditor.ShowRawValues;
                this.Refresh();
            }
        }

        protected void Refresh()
        {
            this.ContentEditorDataContext.Refresh();
        }

        private void RejectNotification(Message message, Item item)
        {
            Notification notification = this.GetNotification(message, item);
            if ((notification != null) && (item != null))
            {
                notification.Reject(item);
            }
        }

        protected void Reload()
        {
            Item folder = this.ContentEditorDataContext.GetFolder();
            if (folder != null)
            {
                Context.ClientPage.SendMessage(this, string.Concat(new object[] { "item:load(id=", folder.ID, ",language=", folder.Language, ",version=", folder.Version, ")" }));
            }
        }

        private void RenderButtons()
        {
            string text = string.Empty;
            string queryString = WebUtil.GetQueryString("mo");
            if (WebUtil.GetQueryString("wb") != "0")
            {
                if ((queryString != "template") && (queryString != "templateworkspace"))
                {
                    text = text + new ImageBuilder { ID = "SwitchMenu", Src = "Images/Window Management/Switch_blue.png", Alt = Sitecore.Globalization.Translate.Text("Change Application."), Class = "scWindowManagementButton", OnClick = @"javascript:return scForm.postEvent(this,event,'javascript:scForm.postEvent(this,evt,\'SwitchMenu_Click\')')", RollOver = true }.ToString();
                }
                if (((queryString != "preview") && (queryString != "popup")) && (!State.Client.UsesBrowserWindows && (WebUtil.GetQueryString("il") != "1")))
                {
                    ImageBuilder builder2 = new ImageBuilder
                    {
                        Src = "Images/Window Management/Minimize.png",
                        Alt = Sitecore.Globalization.Translate.Text("Minimize"),
                        Class = "scWindowManagementButton",
                        OnClick = "javascript:return scForm.postEvent(this,event, 'javascript:scWin.minimizeWindow()')",
                        RollOver = true
                    };
                    text = text + builder2.ToString();
                    builder2 = new ImageBuilder
                    {
                        Src = "Images/Window Management/Maximize.png",
                        Alt = Sitecore.Globalization.Translate.Text("Maximize/Restore"),
                        Class = "scWindowManagementButton",
                        OnClick = "javascript:return scForm.postEvent(this,event,'javascript:scWin.maximizeWindow()')",
                        RollOver = true
                    };
                    text = text + builder2.ToString();
                    text = text + new ImageBuilder { Src = "Images/Window Management/Close_red.png", Alt = Sitecore.Globalization.Translate.Text("Close"), Class = "scWindowManagementCloseButton", Width = 0x26, OnClick = "javascript:return scForm.postEvent(this,event,'javascript:scWin.closeWindow()')", RollOver = true }.ToString();
                }
                this.WindowButtonsPlaceholder.Controls.Add(new LiteralControl(text));
            }
        }

        private void RenderCategory(HtmlTextWriter output, SearchResultCategoryCollection category)
        {
            Assert.ArgumentNotNull(output, "output");
            Assert.ArgumentNotNull(category, "category");
            HtmlTextWriter writer = new HtmlTextWriter(new StringWriter());
            foreach (SearchResult result in (IEnumerable<SearchResult>)category)
            {
                Item item = result.GetObject<Item>();
                if (item != null)
                {
                    writer.Write(string.Concat(new object[] { "<a href=\"#\" class=\"scSearchLink\" title=\"", item.Paths.ContentPath, "\" onclick='javascript:return scForm.invoke(\"item:load(id=", item.ID, ",language=", item.Language, ",version=", item.Version, ")\")'>", Images.GetImage(item.Appearance.Icon, 0x10, 0x10, "absmiddle", "0px 4px 0px 0px"), item.DisplayName, "</a>" }));
                }
            }
            string str = writer.InnerWriter.ToString();
            if (str.Length != 0)
            {
                output.Write("<tr>");
                output.Write("<td width='33%' class='scSearchCategory'>" + category.Name + "</td>");
                output.Write("<td class='scSearchResults'>");
                output.Write(str);
                output.Write("</td>");
                output.Write("</tr>");
            }
        }

        private void RenderEditor(Item item, Item root, Control parent, bool showEditor)
        {
            Assert.ArgumentNotNull(item, "item");
            Assert.ArgumentNotNull(root, "root");
            Assert.ArgumentNotNull(parent, "parent");
            ((Registry.GetString("/Current_User/Content Editor/Translate") == "on") ? new Translator() : new Editor()).Render(item, root, this.FieldInfo, parent, showEditor);
            if (Context.ClientPage.IsEvent)
            {
                ClientCommand command = SheerResponse.SetInnerHtml("ContentEditor", parent);
                command.Attributes["preserveScrollTop"] = "true";
                command.Attributes["preserveScrollElement"] = "EditorPanel";
            }
        }

        private void RenderPager()
        {
            string queryString = WebUtil.GetQueryString("mo");
            if ((UserOptions.ContentEditor.ShowPages && (queryString != "template")) && ((queryString != "templateworkspace") && (WebUtil.GetQueryString("pager") != "0")))
            {
                HtmlTextWriter writer = new HtmlTextWriter(new StringWriter());
                writer.Write("<tr><td class=\"scPager scDockBottom\">");
                Item itemNotNull = Client.GetItemNotNull("/sitecore/content/Applications/Content Editor/Applications", Client.CoreDatabase);
                int @int = MainUtil.GetInt(WebUtil.GetQueryString("pa", "0"), 0);
                ChildList children = itemNotNull.Children;
                for (int i = 0; i < children.Count; i++)
                {
                    Item item = children[i];
                    string uniqueID = Control.GetUniqueID("T");
                    UrlString url = GetUrl(item);
                    url.Add("pa", i.ToString());
                    url.Add("pa0", WebUtil.GetQueryString("pa0"));
                    url.Add("pa1", WebUtil.GetQueryString("pa1"));
                    string str4 = StringUtil.EscapeQuote("SwitchTo(\"" + url + "\")");
                    writer.Write("<a id=\"button_");
                    writer.Write(uniqueID);
                    writer.Write("\" class=\"");
                    writer.Write((i == @int) ? "scPagerButtonActive" : "scPagerButtonNormal");
                    writer.Write("\" href=\"#\" onclick=\"javascript:return scForm.postEvent(this,event,'" + str4 + "')\"");
                    writer.Write(">");
                    writer.Write(item["Header"]);
                    writer.Write("</a>");
                }
                writer.Write("</td></tr>");
                this.Pager.Controls.Add(new LiteralControl(writer.InnerWriter.ToString()));
                this.BottomBorder.Visible = false;
            }
        }

        protected void ResetRibbonStrips()
        {
            UserOptions.ContentEditor.VisibleStrips = string.Empty;
            this.Reload();
        }

        protected void RefreshItem()
        {
    
            this.Reload();
        }

        private void ResolveLanguage(Database database)
        {
            Assert.ArgumentNotNull(database, "database");
            Language contentLanguage = null;
            string queryString = WebUtil.GetQueryString("la");
            if (!string.IsNullOrEmpty(queryString))
            {
                contentLanguage = Language.Parse(queryString);
            }
            if ((contentLanguage == null) && !string.IsNullOrEmpty(Context.User.Profile.ContentLanguage))
            {
                contentLanguage = Language.Parse(Context.User.Profile.ContentLanguage);
                if (ItemUtil.IsNull(LanguageManager.GetLanguageItemId(contentLanguage, database)))
                {
                    contentLanguage = null;
                }
            }
            if (contentLanguage == null)
            {
                contentLanguage = Context.ContentLanguage;
                if (ItemUtil.IsNull(LanguageManager.GetLanguageItemId(contentLanguage, database)))
                {
                    contentLanguage = null;
                }
            }
            if (contentLanguage == null)
            {
                contentLanguage = Context.Language;
                if (ItemUtil.IsNull(LanguageManager.GetLanguageItemId(contentLanguage, database)))
                {
                    contentLanguage = null;
                }
            }
            if (contentLanguage == null)
            {
                LanguageCollection languages = LanguageManager.GetLanguages(database);
                if ((languages != null) && (languages.Count > 0))
                {
                    contentLanguage = languages[0];
                }
            }
            Assert.IsNotNull(contentLanguage, "Could resolve language");
            this.ContentEditorDataContext.Language = contentLanguage;
        }

        protected void Save(Message message)
        {
            Assert.ArgumentNotNull(message, "message");
            SaveArgs args = new SaveArgs(GetSavePacket(this.FieldInfo).XmlDocument);
            if (message["loadid"] != null)
            {
                args.PostAction = "item:load(id=" + message["loadid"] + ",language=" + message["language"] + ",version=" + message["version"] + ",force=" + message["force"] + ")";
            }
            else if (message["postaction"] != null)
            {
                args.PostAction = message["postaction"];
            }
            Context.ClientPage.Start("saveUI", (ClientPipelineArgs)args);
            if (args.Error.Length > 0)
            {
                SheerResponse.Alert(args.Error, new string[0]);
            }
        }

        protected void SaveFieldSize(string templateFieldId, string value)
        {
            int @int = MainUtil.GetInt(value, -1);
            if (@int != -1)
            {
                Registry.SetInt("/Current_User/Content Editor/Field Size/" + templateFieldId, @int);
            }
        }

        protected void SetComparingVersions()
        {
            string str = StringUtil.GetString(new string[] { Context.ClientPage.ClientRequest.Form["ComparingVersion1"] });
            string str2 = StringUtil.GetString(new string[] { Context.ClientPage.ClientRequest.Form["ComparingVersion2"] });
            if ((str != this.ComparingVersion1) || (str2 != this.ComparingVersion2))
            {
                this.ComparingVersion1 = str;
                this.ComparingVersion2 = str2;
                this.Refresh();
            }
        }

        private void SetDocumentType()
        {
            string text = "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.0 Transitional//EN\">";
            DeviceCapabilities capabilities = Client.Device.Capabilities;
            if (capabilities != null)
            {
                text = StringUtil.GetString(new string[] { capabilities.GetDefaultDocumentType(), text });
            }
            this.DocumentType.Controls.Add(new LiteralControl(text));
        }

        protected void SetGutterRenderer(string id)
        {
            Assert.ArgumentNotNullOrEmpty(id, "id");
            GutterManager.ToggleActiveRendererID(ID.Parse(id));
            this.Gutter_Refresh();
        }

        protected void SetTranslatingLanguage()
        {
            Item currentItem = this.GetCurrentItem(Message.Empty);
            if (currentItem != null)
            {
                Language language = Language.Parse(Context.ClientPage.ClientRequest.Form["TranslatingLanguage"]);
                Language translatingLanguage = this.GetTranslatingLanguage(currentItem);
                if (language != translatingLanguage)
                {
                    this.ComparingVersion1 = Sitecore.Data.Version.Latest.ToString();
                    this.ComparingVersion2 = Sitecore.Data.Version.Latest.ToString();
                    this.TranslatingLanguage = language.ToString();
                    this.Refresh();
                }
            }
        }

        private void SetWebEditStylesheet()
        {
            if (WebUtil.GetQueryString("we") == "1")
            {
                string contentEditorStylesheet = Settings.WebEdit.ContentEditorStylesheet;
                if (!string.IsNullOrEmpty(contentEditorStylesheet))
                {
                    this.Stylesheets.Controls.Add(new LiteralControl("<link href=\"" + contentEditorStylesheet + "\" rel=\"stylesheet\" />"));
                }
            }
        }

        protected void ShowRibbonContextMenu()
        {
            var contextMenu = new Sitecore.Web.UI.HtmlControls.Menu();
            Database database = Factory.GetDatabase("core", false);
            Assert.IsNotNull(database, typeof(Database));
            Item item = database.GetItem(ItemIDs.DefaultRibbon);
            Assert.IsNotNull(item, typeof(Ribbon));
            ListString str = new ListString(UserOptions.ContentEditor.VisibleStrips);
            SheerResponse.DisableOutput();
            foreach (Item item2 in item.Children)
            {
                Item strip = item2;
                if (strip.TemplateID == TemplateIDs.Reference)
                {
                    strip = strip.Database.Items[strip["Reference"]];
                }
                if (this.CanBeShown(strip))
                {
                    bool check = strip["Hidden by default"] != "1";
                    if (str.Count > 0)
                    {
                        check = str.Contains(strip.ID.ToString());
                    }
                    contextMenu.Add("S" + strip.ID.ToShortID(), strip["Header"], string.Empty, string.Empty, "ToggleRibbonStrip(\"" + strip.ID + "\")", check, string.Empty, MenuItemType.Check);
                }
            }
            contextMenu.AddDivider();
            contextMenu.Add("__Reset", "Reset", string.Empty, string.Empty, "ResetRibbonStrips", false, string.Empty, MenuItemType.Normal);
            CommandState hidden = CommandState.Hidden;
            Command command = CommandManager.GetCommand("ribbon:customize");
            if (command != null)
            {
                hidden = CommandManager.QueryState(command, CommandContext.Empty);
            }
            if ((hidden == CommandState.Enabled) || (hidden == CommandState.Down))
            {
                contextMenu.Add("__Customize", "Customize", string.Empty, string.Empty, "ribbon:customize", false, string.Empty, MenuItemType.Normal);
            }
            SheerResponse.EnableOutput();
            SheerResponse.ShowContextMenu(Context.ClientPage.ClientRequest.Control, "contextmenu", contextMenu);
        }

        protected void ShowTabContextMenu()
        {
            var contextMenu = new Sitecore.Web.UI.HtmlControls.Menu();
            Database database = Factory.GetDatabase("master", false);
            Assert.IsNotNull(database, typeof(Database));
            Item item = database.GetItem("{DB14B596-67DD-4C93-BE7C-2BDBADEB64C2}"); //Tab Options
            Assert.IsNotNull(item, typeof(Ribbon));
            ListString str = new ListString(UserOptions.ContentEditor.VisibleStrips);
            SheerResponse.DisableOutput();
            foreach (Item item2 in item.Children)
            {
                Item strip = item2;

                contextMenu.Add("S" + strip.ID.ToShortID(), strip.Name, string.Empty, string.Empty, "CloseTabs(\"" + strip.ID + "\")", false, string.Empty, MenuItemType.Check);

            }
            contextMenu.AddDivider();
            contextMenu.Add("__Refresh", "Refresh", string.Empty, string.Empty, "RefreshItem", false, string.Empty, MenuItemType.Normal);
            
            SheerResponse.EnableOutput();
            SheerResponse.ShowContextMenu(Context.ClientPage.ClientRequest.Control, "contextmenu", contextMenu);
        }


        protected void ShowValidationResult(Item folder)
        {
            Assert.ArgumentNotNull(folder, "folder");
            ValidatorCollection validators = this.BuildValidators(ValidatorsMode.ValidateButton, folder);
            ValidatorOptions options = new ValidatorOptions(true);
            ValidatorManager.Validate(validators, options);
            IFormatter formatter = new BinaryFormatter();
            MemoryStream serializationStream = new MemoryStream();
            formatter.Serialize(serializationStream, validators);
            serializationStream.Close();
            UrlHandle handle = new UrlHandle();
            handle["validators"] = Convert.ObjectToBase64(serializationStream.ToArray());
            UrlString urlString = new UrlString("/sitecore/shell/~/xaml/Sitecore.Shell.Applications.ContentEditor.Dialogs.ValidationResult.aspx");
            handle.Add(urlString);
            SheerResponse.ShowModalDialog(urlString.ToString());
        }

        protected void StandardFields_Click()
        {
            UserOptions.ContentEditor.ShowSystemFields = !UserOptions.ContentEditor.ShowSystemFields;
            SheerResponse.CheckModified(false);
            this.Refresh();
        }

        protected void SwitchMenu_Click()
        {
            var parent = new Sitecore.Web.UI.HtmlControls.Menu();
            Item item = Client.CoreDatabase.GetItem(Sitecore.ItemBucket.Kernel.Util.Constants.CoreApplicationsFolder);
            Assert.IsNotNull(item, "Item \"/sitecore/content/Applications/Content Editor/Applications\" not found");
            ChildList children = item.Children;
            for (int i = 0; i < children.Count; i++)
            {
                Item item2 = children[i];
                UrlString url = GetUrl(item2);
                url.Add("pa", i.ToString());
                url.Add("pa0", WebUtil.GetQueryString("pa0"));
                url.Add("pa1", WebUtil.GetQueryString("pa1"));
                var control = new Sitecore.Web.UI.HtmlControls.MenuItem();
                Context.ClientPage.AddControl(parent, control);
                control.Header = item2["Header"];
                control.Icon = item2.Appearance.Icon;
                control.Click = "SwitchTo(\"" + url + "\")";
            }
            SheerResponse.ShowPopup("SwitchMenu", "below-right", parent);
        }

        protected void SwitchTo(string target)
        {
            Assert.ArgumentNotNull(target, "target");
            UrlString str = new UrlString(target);
            Item folder = this.ContentEditorDataContext.GetFolder();
            if (folder != null)
            {
                str["pa" + WebUtil.GetQueryString("pa", "0")] = HttpUtility.UrlEncode(folder.Uri.ToString());
            }
            SheerResponse.SetInnerHtml("scPostActionText", string.Empty);
            SheerResponse.SetLocation(str.ToString());
        }

        protected void SystemMenu_Click()
        {
            var contextMenu = new Sitecore.Web.UI.HtmlControls.Menu();
            SheerResponse.DisableOutput();
            Item item = Client.CoreDatabase.GetItem(Sitecore.ItemBucket.Kernel.Util.Constants.ContentEditorSystemMenu);
            if (item != null)
            {
                contextMenu.AddFromDataSource(item, string.Empty);
                SheerResponse.EnableOutput();
                SheerResponse.ShowContextMenu("SystemMenu", "below", contextMenu);
            }
        }

        protected void ToggleRibbonStrip(string id)
        {
            Assert.ArgumentNotNull(id, "id");
            ListString str = new ListString(UserOptions.ContentEditor.VisibleStrips);
            if (str.Count == 0)
            {
                Database database = Factory.GetDatabase("core", false);
                Assert.IsNotNull(database, typeof(Database));
                Item item = database.GetItem(ItemIDs.DefaultRibbon);
                Assert.IsNotNull(item, typeof(Ribbon));
                foreach (Item item2 in item.Children)
                {
                    Item item3 = item2;
                    if (item3.TemplateID == TemplateIDs.Reference)
                    {
                        item3 = item3.Database.Items[item3["Reference"]];
                    }
                    if (item3["Hidden by default"] != "1")
                    {
                        str.Add(item3.ID.ToString());
                    }
                }
            }
            if (str.Contains(id))
            {
                if (str.Count > 1)
                {
                    str.Remove(id);
                }
                else
                {
                    SheerResponse.Alert("You cannot remove the last strip from the ribbon.", new string[0]);
                }
            }
            else
            {
                str.Add(id);
            }
            UserOptions.ContentEditor.VisibleStrips = str.ToString();
            this.Reload();
        }

        // Methods
        private static Item CreateFavorite(Item item)
        {
            var favorites = Client.CoreDatabase.GetItem("{C39B817C-37D6-4E9D-A8D3-4426550C7125}");
            Assert.ArgumentNotNull(favorites, "favorites");
            Assert.ArgumentNotNull(item, "item");
            ID itemId = new ID("{6073E5B2-8B53-4A0F-8353-C2DE393932AE}");
            TemplateItem item2 = Client.CoreDatabase.GetItem(itemId);
            Assert.IsNotNull(item2, "Template \"Favorite\" not found.");
            return favorites.Add(item.Name, item2);
        }

        protected void BookmarkAll()
        {
            Item item = Sitecore.Context.Item;

            if (item.IsNotNull())
            {
                CreateFavorite(item);
            }
        }

        protected void CloseTabs(string id)
        {
            Assert.ArgumentNotNull(id, "id");

            if (id == "{A26CAF92-CE90-4756-887D-BAF8736975F3}") //Close All
            {
                
                SheerResponse.Eval("javascript:scContent.closeAllEditorTab('" + id + "');");
            }
            else if (id == "{C8AE7385-B923-452C-8FBF-5B99678F9586}") //Close All But This
            {
                SheerResponse.Eval("javascript:scContent.closeAllButActiveEditorTab('" + id + "');");
            }
            else if (id == "{16DBEEB1-443D-4187-8FFA-81DAF5A2A336}")
            {
                SheerResponse.Eval(
                    "javascript:return scForm.postEvent(this,event,'favorites:add(id='" + id + "')')");

            }
            else //Close All To Right
            {
                SheerResponse.Eval("javascript:scContent.closeAllToRightEditorTab('" + id + "');");
            }

            //ListString str = new ListString(UserOptions.ContentEditor.VisibleStrips);
            //if (str.Count == 0)
            //{
            //    Database database = Factory.GetDatabase("core", false);
            //    Assert.IsNotNull(database, typeof(Database));
            //    Item item = database.GetItem(ItemIDs.DefaultRibbon);
            //    Assert.IsNotNull(item, typeof(Ribbon));
            //    foreach (Item item2 in item.Children)
            //    {
            //        Item item3 = item2;
            //        if (item3.TemplateID == TemplateIDs.Reference)
            //        {
            //            item3 = item3.Database.Items[item3["Reference"]];
            //        }
            //        if (item3["Hidden by default"] != "1")
            //        {
            //            str.Add(item3.ID.ToString());
            //        }
            //    }
            //}
            //if (str.Contains(id))
            //{
            //    if (str.Count > 1)
            //    {
            //        str.Remove(id);
            //    }
            //    else
            //    {
            //        SheerResponse.Alert("You cannot remove the last strip from the ribbon.", new string[0]);
            //    }
            //}
            //else
            //{
            //    str.Add(id);
            //}
            //UserOptions.ContentEditor.VisibleStrips = str.ToString();
            //this.Reload();
        }

        protected void ToggleSection(string sectionName, string collapsed)
        {
            Assert.ArgumentNotNullOrEmpty(sectionName, "sectionName");
            Assert.ArgumentNotNull(collapsed, "collapsed");
            UrlString str = new UrlString(Registry.GetString("/Current_User/Content Editor/Sections/Collapsed"));
            str[sectionName] = (collapsed == "1") ? "0" : "1";
            Registry.SetString("/Current_User/Content Editor/Sections/Collapsed", str.ToString());
            this.Reload();
        }

        protected void ToggleTranslation(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            if (SheerResponse.CheckModified())
            {
                string str = (Registry.GetString("/Current_User/Content Editor/Translate") == "on") ? string.Empty : "on";
                Registry.SetString("/Current_User/Content Editor/Translate", str);
                this.Refresh();
            }
        }

        protected void Translate(string fieldID)
        {
            Assert.ArgumentNotNull(fieldID, "fieldID");
            Item currentItem = this.GetCurrentItem(Message.Empty);
            if (currentItem != null)
            {
                Language translatingLanguage = this.GetTranslatingLanguage(currentItem);
                if (translatingLanguage != null)
                {
                    Item item2 = currentItem.Database.Items[currentItem.ID, translatingLanguage];
                    if (item2 != null)
                    {
                        UrlString str = new UrlString(UIUtil.GetUri("control:WorldLingo"));
                        str.Append("ph", item2[fieldID]);
                        str.Append("src", item2.Language.ToString());
                        str.Append("trg", currentItem.Language.ToString());
                        SheerResponse.ShowModalDialog(str.ToString());
                    }
                }
            }
        }

        protected void Translate_Click()
        {
            Context.ClientPage.Start(this, "ToggleTranslation");
        }

        protected void Tree_ContextMenu(string id)
        {
            Assert.ArgumentNotNull(id, "id");
            id = GetID(id);
            Item item = this.ContentEditorDataContext.GetItem(id);
            if (item != null)
            {
                CommandContext context = new CommandContext(item);
                var contextMenu = this.ContextMenu.Build(context);
                SheerResponse.DisableOutput();
                contextMenu.AddDivider();
                contextMenu.Add("__Refresh", "Refresh", "Applications/16x16/refresh.png", string.Empty, "Tree_Refresh(\"" + id + "\")", false, string.Empty, MenuItemType.Normal);
                SheerResponse.EnableOutput();
                SheerResponse.ShowContextMenu(Context.ClientPage.ClientRequest.Control, string.Empty, contextMenu);
            }
        }

        protected void Tree_Refresh(string id)
        {
            Assert.ArgumentNotNull(id, "id");
            Item folder = this.ContentEditorDataContext.GetFolder();
            if (folder != null)
            {
                ID itemId = ID.Parse(GetID(id));
                Tree sidebar = this.Sidebar as Tree;
                Assert.IsNotNull(sidebar, typeof(Tree));
                Item item = folder.Database.GetItem(itemId, folder.Language);
                if (item != null)
                {
                    sidebar.FolderItem = item;
                    sidebar.Refresh(itemId);
                }
            }
        }

        protected void TreeSearch_Click()
        {
            Assert.IsNotNull(SearchManager.SystemIndex, "index");
            HtmlTextWriter output = new HtmlTextWriter(new StringWriter());
            QueryBase query = this.GetQuery();
            if (query != null)
            {
                try
                {
                    SearchArgs args2 = new SearchArgs(query)
                    {
                        Type = SearchType.ContentEditor,
                        Limit = 0x19,
                        Root = this.ContentEditorDataContext.GetFolder()
                    };
                    SearchArgs args = args2;

                    using (new LongRunningOperationWatcher(200, "search pipeline", new string[0]))
                    {
                        CorePipeline.Run("search", args);
                    }
                   
                    if (args.Result.Count == 0)
                    {
                        output.Write("<div style=\"padding:8px 0px 0px;font-style:italic\" align=\"center\">");
                        output.Write(Sitecore.Globalization.Translate.Text("There are no matches."));
                    }
                    else
                    {
                        output.Write("<table cellpadding='0' cellspacing='0' height='100%' width='100%'>");
                        string descendantsTitle = Sitecore.Globalization.Translate.Text("Subitems");
                        SearchResultCategoryCollection category = Enumerable.SingleOrDefault<SearchResultCategoryCollection>(args.Result.Categories, (Func<SearchResultCategoryCollection, bool>)(c => (c.Name == descendantsTitle)));
                        if (category != null)
                        {
                            this.RenderCategory(output, category);
                        }
                        foreach (SearchResultCategoryCollection categorys2 in args.Result.Categories)
                        {
                            if ((categorys2.Count != 0) && !(categorys2.Name == descendantsTitle))
                            {
                                this.RenderCategory(output, categorys2);
                            }
                        }
                        output.Write("<tr class='filler'><td height='100%' class='scSearchCategory'></td><td class='scSearchResults'></td></tr>");
                        output.Write("</table>");
                    }
                }
                catch (Exception exception)
                {
                    output = new HtmlTextWriter(new StringWriter());
                    output.Write(Sitecore.Globalization.Translate.Text("An error occured while searching. Rephrase the query."));
                    output.Write("<br/><br/>");
                    output.Write(exception.Message);
                }
            }
            else
            {
                output.Write(Sitecore.Globalization.Translate.Text("Enter a search query."));
            }
            Item folder = this.ContentEditorDataContext.GetFolder();
            Assert.IsNotNull(folder, "current item");
            string str = StringUtil.Clip(folder.DisplayName, 40, true);
            SheerResponse.SetInnerHtml("SearchResultsHeader", Sitecore.Globalization.Translate.Text("Search Results ({0})", new object[] { str }));
            SheerResponse.SetInnerHtml("SearchResult", output.InnerWriter.ToString());
            SheerResponse.SetStyle("ContentTreeHolder", "display", "none");
            SheerResponse.SetStyle("SearchResultHolder", "display", string.Empty);
        }

        protected void TreeSearchOptionName_Click()
        {
            string control = Context.ClientPage.ClientRequest.Control;
            string str2 = StringUtil.Mid(control, 20);
            SheerResponse.DisableOutput();
            var menu = new Sitecore.Web.UI.HtmlControls.Menu();
            Hashtable fieldInfo = this.FieldInfo;
            bool flag = false;
            SortedList list = new SortedList(StringComparer.Ordinal);
            foreach (FieldInfo info in fieldInfo.Values)
            {
                Item item = Client.ContentDatabase.GetItem(info.FieldID);
                if (item != null)
                {
                    if (!list.ContainsKey(item.Name))
                    {
                        list.Add(item.Name, item.Key);
                    }
                    flag = true;
                }
            }
            if (flag)
            {
                foreach (string str3 in list.Keys)
                {
                    menu.Add("F" + list[str3], str3, string.Empty, string.Empty, "javascript:scContent.changeSearchCriteria(\"" + str2 + "\",\"" + str3 + "\")", false, string.Empty, MenuItemType.Normal);
                }
                menu.AddDivider();
            }
            menu.Add("Remove", "Remove", string.Empty, string.Empty, "javascript:scContent.removeSearchCriteria(\"" + str2 + "\")", false, string.Empty, MenuItemType.Normal);
            SheerResponse.EnableOutput();
            SheerResponse.ShowPopup(control, "below", menu);
        }

        private void Update()
        {
            Item item;
            Item item2;
            this.ContentEditorDataContext.GetState(out item2, out item);
            if (item != null)
            {
                RecentDocuments.AddToRecentDocuments(item.Uri);
                if (!this.DisableHistory)
                {
                    History.Add(item.Uri);
                }
                bool isCurrentItemChanged = ((this._lastFolder == null) || (item.Uri != this._lastFolder)) || !Context.ClientPage.IsEvent;
                bool showEditor = isCurrentItemChanged || (Context.ClientPage.ClientRequest.Form["scShowEditor"] != "0");
                this.UpdateEditor(item, item2, showEditor);
                this.UpdateTree(item);
                this.UpdateRibbon(item, isCurrentItemChanged, showEditor);
                UpdateGutter(item);
                SheerResponse.SetAttribute("__CurrentItem", "value", item.Uri.ToString());
                SheerResponse.Eval("scContentEditorUpdated()");
                Context.ClientPage.Modified = false;
            }
        }

        private void UpdateEditor(Item folder, Item root, bool showEditor)
        {
            Assert.ArgumentNotNull(folder, "folder");
            Assert.ArgumentNotNull(root, "root");
            Border control = new Border();
            this.ContentEditor.Controls.Clear();
            control.ID = "Editors";
            Context.ClientPage.AddControl(this.ContentEditor, control);
            SheerResponse.SetAttribute("scShowEditor", "value", showEditor ? "1" : "0");
            if (Context.ClientPage.IsEvent)
            {
                SheerResponse.SetAttribute("scLanguage", "value", folder.Language.ToString());
            }
            this.RenderEditor(folder, root, control, showEditor);
            this.UpdateValidatorBar(folder, control);
        }

        private static void UpdateGutter(Item folder)
        {
            Assert.ArgumentNotNull(folder, "folder");
            string str = GutterManager.Render(GutterManager.GetRenderers(), folder);
            SheerResponse.SetInnerHtml("Gutter" + folder.ID.ToShortID(), str);
        }

        private void UpdateGutter(string id)
        {
            Item item;
            Item item2;
            Assert.ArgumentNotNullOrEmpty(id, "id");
            this.ContentEditorDataContext.GetState(out item2, out item);
            Item folder = item.Database.GetItem(id);
            if (folder != null)
            {
                UpdateGutter(folder);
            }
        }

        private void UpdateRibbon(Item folder, bool isCurrentItemChanged, bool showEditor)
        {
            Assert.ArgumentNotNull(folder, "folder");
            CommandContext context = new CommandContext(folder);
            context.Parameters["ShowEditor"] = showEditor ? "1" : "0";
            context.Parameters["Ribbon.RenderTabs"] = "true";
            Ribbon ctl = this.Ribbon;
            if (ctl != null)
            {
                ctl.CommandContext = context;
                ctl.PreserveActiveStrip = !isCurrentItemChanged;
                ctl.ActiveStrip = WebUtil.GetQueryString("ras");
                ctl.CustomizeStrips = true;
                ctl.ShowContextualTabs = folder.TemplateID != TemplateIDs.Template;
                string text = HtmlUtil.RenderControl(ctl);
                if (!Context.ClientPage.IsEvent)
                {
                    this.RibbonPlaceholder.Controls.Add(new LiteralControl(text));
                }
                else
                {
                    Sidebar sidebar = this.Sidebar;
                    sidebar.Update(folder.ID, true);
                    sidebar.SetActiveItem(folder.ID);
                    this.RibbonPlaceholder.Controls.Clear();
                    SheerResponse.Redraw();
                    SheerResponse.SetInnerHtml("RibbonPanel", text);
                }
                string renderedActiveStrip = ctl.GetRenderedActiveStrip();
                if (!string.IsNullOrEmpty(renderedActiveStrip))
                {
                    SheerResponse.SetAttribute("scActiveRibbonStrip", "value", renderedActiveStrip);
                }
            }
        }

        private void UpdateTree(Item folder)
        {
            Item rootItem;
            Assert.ArgumentNotNull(folder, "folder");
            if (UserOptions.View.ShowEntireTree && (WebUtil.GetQueryString("ro").Length == 0))
            {
                rootItem = folder.Database.GetRootItem();
            }
            else
            {
                rootItem = this.ContentEditorDataContext.GetRoot();
            }
            if (this.CurrentRoot != rootItem.ID.ToString())
            {
                this.Sidebar.ChangeRoot(rootItem, folder);
                this.CurrentRoot = rootItem.ID.ToString();
            }
        }

        private void UpdateValidatorBar(Item folder, Border parent)
        {
            Assert.ArgumentNotNull(folder, "folder");
            Assert.ArgumentNotNull(parent, "parent");
            if (UserOptions.ContentEditor.ShowValidatorBar)
            {
                ValidatorCollection validators = this.BuildValidators(ValidatorsMode.ValidatorBar, folder);
                ValidatorOptions options = new ValidatorOptions(false);
                ValidatorManager.Validate(validators, options);
                string str = ValidatorBarFormatter.RenderValidationResult(validators);
                bool flag = str.IndexOf("Applications/16x16/bullet_square_grey.png") >= 0;
                if (Context.ClientPage.IsEvent)
                {
                    SheerResponse.Eval("scContent.clearValidatorTimeouts()");
                    SheerResponse.SetInnerHtml("ValidatorPanel", str);
                    SheerResponse.SetAttribute("scHasValidators", "value", (validators.Count > 0) ? "1" : string.Empty);
                    SheerResponse.Eval("scContent.updateFieldMarkers()");
                    if (flag)
                    {
                        SheerResponse.Eval("window.setTimeout(\"scContent.updateValidators()\", " + Settings.Validators.UpdateFrequency + ")");
                    }
                    SheerResponse.Redraw();
                }
                else
                {
                    var control = parent.FindControl("ValidatorPanel");
                    if (control != null)
                    {
                        control.Controls.Add(new LiteralControl(str));
                        Context.ClientPage.FindControl("ContentEditorForm").Controls.Add(new LiteralControl("<input type=\"hidden\" id=\"scHasValidators\" name=\"scHasValidators\" value=\"" + ((validators.Count > 0) ? "1" : string.Empty) + "\"/>"));
                        if (flag)
                        {
                            control.Controls.Add(new LiteralControl("<script type=\"text/javascript\" language=\"javascript\">window.setTimeout('scContent.updateValidators()', " + Settings.Validators.UpdateFrequency + ")</script>"));
                        }
                        control.Controls.Add(new LiteralControl("<script type=\"text/javascript\" language=\"javascript\">scContent.updateFieldMarkers()</script>"));
                    }
                }
            }
        }

        protected void UpdateValidators()
        {
            Assert.IsTrue(UserOptions.ContentEditor.ShowValidatorBar, "Validator bar is switched off in Content Editor.");
            string formValue = WebUtil.GetFormValue("scValidatorsKey");
            ValidatorCollection validators = ValidatorManager.GetValidators(ValidatorsMode.ValidatorBar, formValue);
            ValidatorManager.UpdateValidators(validators);
            string text = ValidatorBarFormatter.RenderValidationResult(validators);
            SheerResponse.Eval(string.Format("scContent.renderValidators({0},{1})", StringUtil.EscapeJavascriptString(text), Settings.Validators.UpdateFrequency));
        }

        protected string UpdateValidators(Item folder)
        {
            Assert.ArgumentNotNull(folder, "folder");
            Assert.IsTrue(UserOptions.ContentEditor.ShowValidatorBar, "Validator bar is switched off in Content Editor.");
            ValidatorCollection validators = this.BuildValidators(ValidatorsMode.ValidatorBar, folder);
            ValidatorOptions options = new ValidatorOptions(false);
            ValidatorManager.Validate(validators, options);
            return ValidatorBarFormatter.RenderValidationResult(validators);
        }

        protected void ValidateItem()
        {
            Assert.IsTrue(UserOptions.ContentEditor.ShowValidatorBar, "Validator bar is switched off in Content Editor.");
            string formValue = WebUtil.GetFormValue("scValidatorsKey");
            ValidatorCollection validators = ValidatorManager.GetValidators(ValidatorsMode.ValidatorBar, formValue);
            ValidatorOptions options = new ValidatorOptions(false);
            ValidatorManager.Validate(validators, options);
            string text = ValidatorBarFormatter.RenderValidationResult(validators);
            SheerResponse.Eval(string.Format("scContent.renderValidators({0},{1})", StringUtil.EscapeJavascriptString(text), Settings.Validators.UpdateFrequency));
        }

        protected void Validator_ContextMenu(string markerId)
        {
            Assert.ArgumentNotNull(markerId, "markerId");
            Assert.IsTrue(UserOptions.ContentEditor.ShowValidatorBar, "Validator bar is switched off in Content Editor.");
            var byMarkerID = ValidatorManager.GetValidators(ValidatorsMode.ValidatorBar, this.ValidatorsKey).GetByMarkerID(markerId);
            if (byMarkerID != null)
            {
                SheerResponse.DisableOutput();
                var contextMenu = new Sitecore.Web.UI.HtmlControls.Menu();
                if (byMarkerID.HasMenu)
                {
                    Assert.IsNotNull(Context.ContentDatabase, "Content database not found.");
                    Assert.IsNotNull(byMarkerID.ValidatorID, "Validator ID is null.");
                    Item item = Context.ContentDatabase.GetItem(byMarkerID.ValidatorID);
                    if (item != null)
                    {
                        contextMenu.AddFromDataSource(item, byMarkerID.MarkerID);
                        contextMenu.AddDivider();
                    }
                }
                contextMenu.Add("Suppress Validation Rule", string.Empty, string.Format("Validator_SuppressValidator(\"{0}\")", markerId));
                SheerResponse.EnableOutput();
                SheerResponse.ShowContextMenu(Context.ClientPage.ClientRequest.Control, string.Empty, contextMenu);
            }
        }

        protected void Validator_SuppressValidator(string markerId)
        {
            Assert.ArgumentNotNull(markerId, "markerId");
            Assert.IsTrue(UserOptions.ContentEditor.ShowValidatorBar, "Validator bar is switched off in Content Editor.");
            ValidatorCollection validators = ValidatorManager.GetValidators(ValidatorsMode.ValidatorBar, this.ValidatorsKey);
            var byMarkerID = validators.GetByMarkerID(markerId);
            if (byMarkerID != null)
            {
                Assert.IsNotNull(byMarkerID.ItemUri, "Item URI is null.");
                Item item = Database.GetItem(byMarkerID.ItemUri);
                if (item != null)
                {
                    ListString str = new ListString(item["__Suppressed Validation Rules"]);
                    Assert.IsNotNull(byMarkerID.ValidatorID, "Validator ID is null.");
                    if (!str.Contains(byMarkerID.ValidatorID.ToString()))
                    {
                        str.Add(byMarkerID.ValidatorID.ToString());
                        using (new SecurityDisabler())
                        {
                            item.Editing.BeginEdit();
                            item["__Suppressed Validation Rules"] = str.ToString();
                            item.Editing.EndEdit();
                        }
                        validators.Remove(byMarkerID);
                        WebUtil.SetSessionValue(this.ValidatorsKey + ValidatorsMode.ValidatorBar, validators);
                        SheerResponse.Eval("scContent.validate()");
                    }
                }
            }
        }

        protected void Workflow_History()
        {
            XmlControl webControl = Resource.GetWebControl("WorkboxHistory") as XmlControl;
            Assert.IsNotNull(webControl, "History not found");
            Item folder = this.ContentEditorDataContext.GetFolder();
            if (folder != null)
            {
                IWorkflowProvider workflowProvider = Client.ContentDatabase.WorkflowProvider;
                if ((workflowProvider != null) && (workflowProvider.GetWorkflows().Length > 0))
                {
                    IWorkflow workflow = workflowProvider.GetWorkflow(folder);
                    if (workflow != null)
                    {
                        webControl["ItemID"] = folder.ID.ToString();
                        webControl["Language"] = folder.Language.ToString();
                        webControl["Version"] = folder.Version.ToString();
                        webControl["WorkflowID"] = workflow.WorkflowID;
                        Context.ClientPage.ClientResponse.ShowPopup(Context.ClientPage.ClientRequest.Control, "below", webControl);
                    }
                }
            }
        }

        // Properties
        public string ComparingVersion1
        {
            get
            {
                return StringUtil.GetString(base.ServerProperties["ComparingVersion1"]);
            }
            set
            {
                Assert.ArgumentNotNull(value, "value");
                base.ServerProperties["ComparingVersion1"] = value;
            }
        }

        public string ComparingVersion2
        {
            get
            {
                return StringUtil.GetString(base.ServerProperties["ComparingVersion2"]);
            }
            set
            {
                Assert.ArgumentNotNull(value, "value");
                base.ServerProperties["ComparingVersion2"] = value;
            }
        }

        public ContextMenu ContextMenu
        {
            get
            {
                return this._contextMenu;
            }
        }

        public string CurrentRoot
        {
            get
            {
                return StringUtil.GetString(base.ServerProperties["Root"]);
            }
            set
            {
                Assert.ArgumentNotNull(value, "value");
                base.ServerProperties["Root"] = value;
            }
        }

        private bool DisableHistory { get; set; }

        public Hashtable FieldInfo
        {
            get
            {
                Hashtable hashtable = base.ServerProperties["Info"] as Hashtable;
                if (hashtable == null)
                {
                    hashtable = new Hashtable(5);
                    base.ServerProperties["Info"] = hashtable;
                }
                return hashtable;
            }
        }

        private string FrameName
        {
            get
            {
                return (base.ServerProperties["FrameName"] as string);
            }
            set
            {
                Assert.ArgumentNotNull(value, "value");
                base.ServerProperties["FrameName"] = value;
            }
        }

        private bool HasPendingUpdate
        {
            get
            {
                return this._hasPendingUpdate;
            }
            set
            {
                this._hasPendingUpdate = value;
            }
        }

        private ItemUri PendingUpdateItemUri
        {
            get
            {
                return this._pendingUpdateItemUri;
            }
            set
            {
                Assert.ArgumentNotNull(value, "value");
                this._pendingUpdateItemUri = value;
            }
        }

        public Ribbon Ribbon
        {
            get
            {
                return this._ribbon;
            }
        }

        public Sidebar Sidebar
        {
            get
            {
                return this._sidebar;
            }
        }

        public string TranslatingLanguage
        {
            get
            {
                return StringUtil.GetString(base.ServerProperties["TranslatingLanguage"]);
            }
            set
            {
                Assert.ArgumentNotNull(value, "value");
                base.ServerProperties["TranslatingLanguage"] = value;
            }
        }

        private static bool IsHidden(Item item, Item root)
        {
            Assert.ArgumentNotNull(item, "item");
            Assert.ArgumentNotNull(item, "root");
            if (!UserOptions.View.ShowHiddenItems)
            {
                do
                {
                    if (item.ID == root.ID)
                    {
                        return false;
                    }
                    if (item.Appearance.Hidden)
                    {
                        return true;
                    }
                    item = item.Parent;
                }
                while (item != null);
            }
            return false;
        }






        private string ValidatorsKey
        {
            get
            {
                if (string.IsNullOrEmpty(this._validatorsKey))
                {
                    this._validatorsKey = WebUtil.GetFormValue("scValidatorsKey");
                }
                return this._validatorsKey;
            }
            set
            {
                Assert.ArgumentNotNull(value, "value");
                this._validatorsKey = value;
            }
        }
    }
}




