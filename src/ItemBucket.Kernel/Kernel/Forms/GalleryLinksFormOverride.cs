namespace Sitecore.ItemBucket.Kernel.Kernel.Forms
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Web.UI;

    using Sitecore.Collections;
    using Sitecore.Configuration;
    using Sitecore.Data.Items;
    using Sitecore.Diagnostics;
    using Sitecore.Globalization;
    using Sitecore.Links;
    using Sitecore.Resources;
    using Sitecore.Shell;
    using Sitecore.Shell.Applications.ContentManager.Galleries.Links;
    using Sitecore.Web.UI.HtmlControls;
    using Sitecore.Web.UI.Sheer;

    internal class GalleryLinksFormOverride : GalleryLinksForm
    {
        // Fields
        protected Scrollbox Links;

        // Methods
        public override void HandleMessage(Message message)
        {
            Assert.ArgumentNotNull(message, "message");
            this.Invoke(message, true);
            message.CancelBubble = true;
            message.CancelDispatch = true;
        }

        private bool IsHidden(Item item)
        {
            while (item != null)
            {
                if (item.Appearance.Hidden)
                {
                    return true;
                }

                item = item.Parent;
            }

            return false;
        }

        protected override void OnLoad(EventArgs e)
        {
            Assert.ArgumentNotNull(e, "e");
            if (!Context.ClientPage.IsEvent)
            {
                var result = new StringBuilder();
                var itemFromQueryString = UIUtil.GetItemFromQueryString(Context.ContentDatabase);
                if (itemFromQueryString != null)
                {
                    var linkDatabase = Globals.LinkDatabase;
                    var referrers = linkDatabase.GetReferrers(itemFromQueryString);
                    var list = new List<Pair<Item, ItemLink>>();
                    foreach (var link in referrers)
                    {
                        var database = Factory.GetDatabase(link.SourceDatabaseName, false);
                        if (database != null)
                        {
                            var item = database.Items[link.SourceItemID];
                            if (((item == null) || !this.IsHidden(item)) || UserOptions.View.ShowHiddenItems)
                            {
                                list.Add(new Pair<Item, ItemLink>(item, link));
                            }
                        }
                    }

                    if (list.Count > 0)
                    {
                        this.RenderReferrers(result, list);
                    }

                    referrers = linkDatabase.GetReferences(itemFromQueryString);
                    list = new List<Pair<Item, ItemLink>>();
                    foreach (var link2 in referrers)
                    {
                        var database3 = Factory.GetDatabase(link2.TargetDatabaseName, false);
                        if (database3 != null)
                        {
                            var item3 = database3.Items[link2.TargetItemID];
                            if (((item3 == null) || !this.IsHidden(item3)) || UserOptions.View.ShowHiddenItems)
                            {
                                list.Add(new Pair<Item, ItemLink>(item3, link2));
                            }
                        }
                    }

                    if (list.Count > 0)
                    {
                        this.RenderReferences(result, list);
                    }
                }

                if (result.Length == 0)
                {
                    result.Append(Translate.Text("This item has no references."));
                }

                this.Links.Controls.Add(new LiteralControl(result.ToString()));
            }
        }

        private void RenderReferences(StringBuilder result, List<Pair<Item, ItemLink>> references)
        {
            result.Append("<div style=\"font-weight:bold;padding:2px 0px 4px 0px\">" + Translate.Text("References:") + "</div>");
            foreach (Pair<Item, ItemLink> pair in references)
            {
                var item = pair.Part1;
                var link = pair.Part2;
                if (item == null)
                {
                    result.Append(string.Format("<div class=\"scLink\">{0} {1}: {2}, {3}</div>", new object[] { Images.GetImage("Applications/16x16/error.png", 0x10, 0x10, "absmiddle", "0px 4px 0px 0px"), Translate.Text("Not found"), link.TargetDatabaseName, link.TargetItemID }));
                }
                else
                {
                    result.Append(string.Concat(new object[] { "<a href=\"#\" class=\"scLink\" onclick='javascript:return scForm.invoke(\"item:load(id=", item.ID, ",language=", item.Language, ",version=", item.Version, ")\")'>", Images.GetImage(item.Appearance.Icon, 0x10, 0x10, "absmiddle", "0px 4px 0px 0px"), item.DisplayName, " - [", item.Paths.Path, "]</a>" }));
                }
            }
        }

        private void RenderReferrers(StringBuilder result, List<Pair<Item, ItemLink>> referrers)
        {
            result.Append("<div style=\"font-weight:bold;padding:2px 0px 4px 0px\">" + Translate.Text("Referrers:") + "</div>");
            foreach (Pair<Item, ItemLink> pair in referrers)
            {
                var item = pair.Part1;
                var link = pair.Part2;
                if (item == null)
                {
                    result.Append(string.Format("<div class=\"scLink\">{0} {1}: {2}, {3}</div>", new object[] { Images.GetImage("Applications/16x16/error.png", 0x10, 0x10, "absmiddle", "0px 4px 0px 0px"), Translate.Text("Not found"), link.SourceDatabaseName, link.SourceItemID }));
                }
                else
                {
                    result.Append(string.Concat(new object[] { "<a href=\"#\" class=\"scLink\" onclick='javascript:return scForm.invoke(\"item:load(id=", item.ID, ",language=", item.Language, ",version=", item.Version, ")\")'>", Images.GetImage(item.Appearance.Icon, 0x10, 0x10, "absmiddle", "0px 4px 0px 0px"), item.DisplayName }));
                    if (item.Fields.Contains(link.SourceFieldID))
                    {
                        var field = item.Fields[link.SourceFieldID];
                        if (field.HasValue)
                        {
                            result.Append(" - ");
                            result.Append(field.DisplayName);
                        }
                    }

                    result.Append(" - [" + item.Paths.Path + "]</a>");
                }
            }
        }
    }
}
