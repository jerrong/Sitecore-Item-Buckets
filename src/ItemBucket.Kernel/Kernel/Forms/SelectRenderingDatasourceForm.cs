using System;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Resources;
using Sitecore.Shell.Applications.Dialogs;
using Sitecore.Shell.Applications.Dialogs.ItemLister;
using Sitecore.Shell.Applications.Dialogs.SelectCreateItem;
using Sitecore.StringExtensions;
using Sitecore.Web.UI;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Sheer;
using Sitecore.Web.UI.WebControls;

namespace Sitecore.ItemBucket.Kernel.Kernel.Forms
{
    public class SelectRenderingDatasourceForm : SelectCreateItemForm
    {
        // Fields
        protected TreeviewEx CloneDestination;
        protected const string CloneMode = "Clone";
        protected Edit CloneName;
        protected Border CloneOption;
        protected DataContext CloneParentDataContext;
        protected Border CloneSection;



        protected Border SearchOption;
        protected Border SearchSection;
        protected Edit ItemLink;

        protected TreeviewEx CreateDestination;
        protected ThemedImage CreateIcon;
        protected Border CreateOption;
        protected DataContext CreateParentDataContext;
        protected Border CreateSection;
        protected Literal Information;
        protected Edit NewDatasourceName;
        protected Literal SectionHeader;
        protected Border SelectOption;
        protected Scrollbox SelectSection;
        protected Border Warnings;

        // Methods
        protected override void ChangeMode(string mode)
        {
            Assert.ArgumentNotNull(mode, "mode");
            base.ChangeMode(mode);
            if (!UIUtil.IsIE())
            {
                SheerResponse.Eval("scForm.browser.initializeFixsizeElements();");
            }
        }

        private void CloneDatasource()
        {
            Item selectionItem = this.CloneDestination.GetSelectionItem();
            if (selectionItem == null)
            {
                SheerResponse.Alert("Parent not found", new string[0]);
            }
            else
            {
                string str2;
                string name = this.CloneName.Value;
                if (!this.ValidateNewItemName(name, out str2))
                {
                    SheerResponse.Alert(str2, new string[0]);
                }
                else
                {
                    Item currentDatasourceItem = this.CurrentDatasourceItem;
                    Assert.IsNotNull(currentDatasourceItem, "currentDatasource");
                    if (selectionItem.Paths.LongID.StartsWith(currentDatasourceItem.Paths.LongID))
                    {
                        SheerResponse.Alert("An item cannot be copied below itself.", new string[0]);
                    }
                    else
                    {
                        name = ItemUtil.GetCopyOfName(selectionItem, name);
                        Item selectedItem = currentDatasourceItem.CloneTo(selectionItem, name, true);
                        this.SetDialogResult(selectedItem);
                        SheerResponse.CloseWindow();
                    }
                }
            }
        }

        protected void CloneDestination_Change()
        {
            Item selectionItem = this.CloneDestination.GetSelectionItem();
            this.SetControlsForCloning(selectionItem);
        }

        

        private void CreateDatasource()
        {
            Item selectionItem = this.CreateDestination.GetSelectionItem();
            if (selectionItem == null)
            {
                SheerResponse.Alert("Select an item first.", new string[0]);
            }
            else
            {
                string str2;
                string name = this.NewDatasourceName.Value;
                if (!this.ValidateNewItemName(name, out str2))
                {
                    SheerResponse.Alert(str2, new string[0]);
                }
                else
                {
                    Item item2;
                    Language contentLanguage = this.ContentLanguage;
                    if ((contentLanguage != null) && (contentLanguage != selectionItem.Language))
                    {
                        selectionItem = selectionItem.Database.GetItem(selectionItem.ID, contentLanguage) ?? selectionItem;
                    }
                    if ((this.Prototype != null) && (this.Prototype.TemplateID == TemplateIDs.BranchTemplate))
                    {
                        item2 = selectionItem.Add(name, (BranchItem)this.Prototype);
                    }
                    else
                    {
                        item2 = selectionItem.Add(name, (TemplateItem)this.Prototype);
                    }
                    if (item2 != null)
                    {
                        this.SetDialogResult(item2);
                    }
                    SheerResponse.CloseWindow();
                }
            }
        }

        protected void CreateDestination_Change()
        {
            Item selectionItem = this.CreateDestination.GetSelectionItem();
            this.SetControlsForCreating(selectionItem);
        }

        private void DisableCreateOption()
        {
            this.CreateOption.Disabled = true;
            this.CreateOption.Class = "option-disabled";
            this.CreateOption.Click = "javascript:void(0);";
            this.CreateIcon.Src = Images.GetThemedImageSource(this.CreateIcon.Src, ImageDimension.id32x32, true);
        }

        protected override void OnLoad(EventArgs e)
        {
            Assert.ArgumentNotNull(e, "e");
            base.OnLoad(e);
            if (!Context.ClientPage.IsEvent)
            {
                this.SelectOption.Click = string.Format("ChangeMode(\"{0}\")", "Select");
                this.CreateOption.Click = string.Format("ChangeMode(\"{0}\")", "Create");
                this.CloneOption.Click = string.Format("ChangeMode(\"{0}\")", "Clone");
                this.SearchOption.Click = string.Format("ChangeMode(\"{0}\")", "Search");
                SelectDatasourceOptions options = SelectItemOptions.Parse<SelectDatasourceOptions>();
                if (options.DatasourcePrototype == null)
                {
                    this.DisableCreateOption();
                }
                else
                {
                    this.Prototype = options.DatasourcePrototype;
                }
                if (!string.IsNullOrEmpty(options.DatasourceItemDefaultName))
                {
                    this.NewDatasourceName.Value = this.GetNewItemDefaultName(options.DatasourceRoot, options.DatasourceItemDefaultName);
                }
                if (options.ContentLanguage != null)
                {
                    this.ContentLanguage = options.ContentLanguage;
                }
                if (!string.IsNullOrEmpty(options.CurrentDatasource))
                {
                    this.CurrentDatasourcePath = options.CurrentDatasource;
                    if (options.DatasourceRoot != null)
                    {
                        string copyOfName = string.Empty;
                        if (!string.IsNullOrEmpty(options.DatasourceItemDefaultName))
                        {
                            copyOfName = ItemUtil.GetCopyOfName(options.DatasourceRoot, options.DatasourceItemDefaultName);
                        }
                        if (this.CurrentDatasourceItem != null)
                        {
                            copyOfName = this.CloneName.Value = ItemUtil.GetCopyOfName(options.DatasourceRoot, this.CurrentDatasourceItem.Name);
                        }
                        this.CloneName.Value = copyOfName;
                    }
                }
                else
                {
                    this.CloneOption.Visible = false;
                }
                this.SetDataContexts();
                Item folder = base.DataContext.GetFolder();
                this.SetControlsForSelection(folder);
                this.SetSectionHeader();
            }
        }

        protected override void OnOK(object sender, EventArgs args)
        {
            Assert.ArgumentNotNull(sender, "sender");
            Assert.ArgumentNotNull(args, "args");
            string currentMode = this.CurrentMode;
            if (currentMode != null)
            {
                if (!(currentMode == "Select"))
                {
                    if (!(currentMode == "Clone"))
                    {
                        if (currentMode == "Create")
                        {
                            this.CreateDatasource();
                        }
                        else if (currentMode =="Search")
                        {
                            var selectionItem = Sitecore.Context.ContentDatabase.GetItem(ItemLink.Value);
                            if (selectionItem != null)
                            {
                                this.SetDialogResult(selectionItem);
                                SheerResponse.CloseWindow();
                            }
                        }
                        return;
                    }
                }
                else
                {
                    Item selectionItem = base.Treeview.GetSelectionItem();
                    if (selectionItem != null)
                    {
                        this.SetDialogResult(selectionItem);
                    }
                    else
                    {

                    }
                    SheerResponse.CloseWindow();
                    return;
                }
                this.CloneDatasource();
            }
        }

        private void SetControlsForCloning(Item item)
        {
            this.SetControlsForCreating(item);
        }

        private void SetControlsForCreating(Item item)
        {
            string str;
            this.Warnings.Visible = false;
            SheerResponse.SetAttribute(this.Warnings.ID, "title", string.Empty);
            if (!this.CanCreateItem(item, out str))
            {
                base.OK.Disabled = true;
                this.Information.Text = Translate.Text(str);
                this.Warnings.Visible = true;
            }
            else
            {
                base.OK.Disabled = false;
            }
        }


        private void SetControlsForSearching(Item item)
        {
            string str;
            this.Warnings.Visible = false;
            SheerResponse.SetAttribute(this.Warnings.ID, "title", string.Empty);
            if (!this.CanCreateItem(item, out str))
            {
                base.OK.Disabled = true;
                this.Information.Text = Translate.Text(str);
                this.Warnings.Visible = true;
            }
            else
            {
                base.OK.Disabled = false;
            }
        }

        private void SetControlsForSelection(Item item)
        {
            this.Warnings.Visible = false;
            SheerResponse.SetAttribute(this.Warnings.ID, "title", string.Empty);
            if (item != null)
            {
                if (!this.IsSelectable(item))
                {
                    base.OK.Disabled = true;
                    string str = StringUtil.Clip(item.DisplayName, 20, true);
                    string str2 = Translate.Text("The '{0}' item is not a valid selection.".FormatWith(new object[] { str }));
                    this.Information.Text = str2;
                    this.Warnings.Visible = true;
                    SheerResponse.SetAttribute(this.Warnings.ID, "title", Translate.Text("The data source must be a '{0}' item.".FormatWith(new object[] { this.TemplateNamesString })));
                }
                else
                {
                    this.Information.Text = string.Empty;
                    base.OK.Disabled = false;
                }
            }
        }

        protected override void SetControlsOnModeChange()
        {
            base.SetControlsOnModeChange();
            string currentMode = this.CurrentMode;
            if (currentMode != null)
            {
                if (!(currentMode == "Select"))
                {
                    if (currentMode == "Clone")
                    {
                        this.CloneOption.Class = "selected";
                        if (!this.CreateOption.Disabled)
                        {
                            this.CreateOption.Class = string.Empty;
                        }
                        this.SelectOption.Class = string.Empty;
                        this.SearchOption.Class = string.Empty;
                        this.SearchSection.Visible = false;
                        this.SelectSection.Visible = false;
                        this.CloneSection.Visible = true;
                        this.CreateSection.Visible = false;
                        this.SetControlsForCloning(this.CloneDestination.GetSelectionItem());
                        SheerResponse.Eval(string.Format("selectItemName('{0}')", this.CloneName.ID));
                    }
                    else if (currentMode == "Create")
                    {
                        this.CloneOption.Class = string.Empty;
                        this.SelectSection.Visible = false;
                        this.SearchOption.Class = string.Empty;
                        this.SearchSection.Visible = false;
                        this.CloneSection.Visible = false;
                        this.CreateSection.Visible = true;
                        this.SetControlsForCreating(this.CreateDestination.GetSelectionItem());
                        SheerResponse.Eval(string.Format("selectItemName('{0}')", this.NewDatasourceName.ID));
                    }
                    else if (currentMode == "Search")
                    {
                        this.SearchOption.Class = "selected";
                        if (!this.CreateOption.Disabled)
                        {
                            this.CreateOption.Class = string.Empty;
                        }
                        this.CloneOption.Class = string.Empty;
                        this.SelectSection.Visible = false;
                        this.SearchSection.Visible = true;
                        this.CloneSection.Visible = false;
                        this.CreateSection.Visible = false;
                        this.SetControlsForSearching(this.CreateDestination.GetSelectionItem());
                        SheerResponse.Eval(string.Format("selectItemName('{0}')", this.NewDatasourceName.ID));
                    }
                }
                else
                {
                    this.CloneOption.Class = string.Empty;
                    this.SelectSection.Visible = true;
                    this.CloneSection.Visible = false;
                    this.CreateSection.Visible = false;
                    this.SearchSection.Visible = false;
                    this.SetControlsForSelection(base.Treeview.GetSelectionItem());
                }
            }
            this.SetSectionHeader();
        }

        private void SetDataContexts()
        {
            if (!string.IsNullOrEmpty(base.DataContext.Root))
            {
                this.CloneParentDataContext.Root = base.DataContext.Root;
                this.CreateParentDataContext.Root = base.DataContext.Root;
                this.CloneParentDataContext.Folder = base.DataContext.Root;
                this.CreateParentDataContext.Folder = base.DataContext.Root;
            }
            if (!string.IsNullOrEmpty(base.DataContext.DataViewName))
            {
                this.CloneParentDataContext.DataViewName = base.DataContext.DataViewName;
                this.CreateParentDataContext.DataViewName = base.DataContext.DataViewName;
            }
            if (!string.IsNullOrEmpty(base.DataContext.Filter))
            {
                this.CloneParentDataContext.Filter = base.DataContext.Filter;
                this.CreateParentDataContext.Filter = base.DataContext.Filter;
            }
            Item currentDatasourceItem = this.CurrentDatasourceItem;
            if (currentDatasourceItem != null)
            {
                Item root = base.DataContext.GetRoot();
                if ((root != null) && currentDatasourceItem.Paths.IsDescendantOf(root))
                {
                    base.DataContext.Folder = currentDatasourceItem.Paths.FullPath;
                }
            }
        }

        private void SetSectionHeader()
        {
            string currentMode = this.CurrentMode;
            if (currentMode != null)
            {
                if (!(currentMode == "Select"))
                {
                    if (!(currentMode == "Create"))
                    {
                        if (currentMode == "Clone")
                        {
                            this.SectionHeader.Text = Translate.Text("Clone the current content item.");
                        }
                        return;
                    }
                }
                else if (!(currentMode == "Search"))
                {
                    this.SectionHeader.Text = Translate.Text("Search for content items");
                }
                else
                {
                    this.SectionHeader.Text = Translate.Text("Select an existing content item.");
                    return;
                }
                this.SectionHeader.Text = Translate.Text("Create a new content item.");
            }
        }

        protected void Treeview_Click()
        {
            this.SetControlsForSelection(base.Treeview.GetSelectionItem());
        }

        // Properties
        private Language ContentLanguage
        {
            get
            {
                return (base.ServerProperties["cont_language"] as Language);
            }
            set
            {
                base.ServerProperties["cont_language"] = value;
            }
        }

        protected override Control CreateOptionControl
        {
            get
            {
                return this.CreateOption;
            }
        }

        private Item CurrentDatasourceItem
        {
            get
            {
                string currentDatasourcePath = this.CurrentDatasourcePath;
                if (!string.IsNullOrEmpty(currentDatasourcePath))
                {
                    return Client.ContentDatabase.GetItem(currentDatasourcePath);
                }
                return null;
            }
        }

        private string CurrentDatasourcePath
        {
            get
            {
                return (base.ServerProperties["current_datasource"] as string);
            }
            set
            {
                base.ServerProperties["current_datasource"] = value;
            }
        }

        private Item Prototype
        {
            get
            {
                ItemUri prototypeUri = this.PrototypeUri;
                if (prototypeUri != null)
                {
                    return Database.GetItem(prototypeUri);
                }
                return null;
            }
            set
            {
                Assert.IsNotNull(value, "value");
                base.ServerProperties["template_item"] = value.Uri;
            }
        }

        private ItemUri PrototypeUri
        {
            get
            {
                return (base.ServerProperties["template_item"] as ItemUri);
            }
        }

        protected override Control SelectOptionControl
        {
            get
            {
                return this.SelectOption;
            }
        }
    }





}
