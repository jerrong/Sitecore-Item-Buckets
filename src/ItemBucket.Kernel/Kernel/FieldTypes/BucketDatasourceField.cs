using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.ItemBucket.Kernel.Kernel.Util;
using Sitecore.Text;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Sheer;

namespace Sitecore.ItemBucket.Kernel.FieldTypes
{
    public class BucketInternalLink : Edit
    {
        // Methods
        public BucketInternalLink()
        {
            this.Class = "scContentControl";
            base.Activation = true;
        }

        protected virtual Database GetContentDatabase()
        {
            return Client.ContentDatabase;
        }

        public override void HandleMessage(Message message)
        {
            string str;
            base.HandleMessage(message);
            if ((message["id"] == this.ID) && ((str = message.Name) != null))
            {
                if (str == "contentExtension:pastequery")
                {
                    if (Sitecore.Context.ClientData.GetValue("CurrentPasteDatasource").IsNotNull())
                    {
                        this.Value = Sitecore.Context.ClientData.GetValue("CurrentPasteDatasource").ToString();
                    }
                }
                else if (str == "contentExtension:buildquery")
                {
                    Sitecore.Context.ClientPage.Start(this, "OpenSearch");
                }
                else if (!(str == "contentinternallink:open"))
                {
                    if (!(str == "contentinternallink:clear"))
                    {
                        return;
                    }
                }

                else
                {
                    Sitecore.Context.ClientPage.Start(this, "OpenLink");
                    return;
                }
                if (this.Value.Length > 0)
                {
                    this.SetModified();
                }
                //this.Value = string.Empty;
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            base.ServerProperties["Value"] = base.ServerProperties["Value"];
        }

        protected void OpenLink(ClientPipelineArgs args)
        {
            if (args.IsPostBack)
            {
                if (!string.IsNullOrEmpty(args.Result) && (args.Result != "undefined"))
                {
                    Item item = this.GetContentDatabase().Items[args.Result];
                    if (item != null)
                    {
                        if (this.Value != item.Paths.Path)
                        {
                            this.SetModified();
                        }
                        this.Value = item.Paths.Path;
                    }
                    else
                    {
                        if (this.Value.Length > 0)
                        {
                            this.SetModified();
                        }
                        SheerResponse.Alert("Item not found.", new string[0]);
                        this.Value = string.Empty;
                    }
                }
            }
            else
            {
                UrlString str = new UrlString("/sitecore/shell/Applications/Item browser.aspx");
                string str2 = this.Value;
                string str3 = this.Value;
                Item item2 = this.GetContentDatabase().Items[str2];
                if (item2 != null)
                {
                    str3 = item2.ID.ToString();
                }
                str.Append("db", this.GetContentDatabase().Name);
                str.Append("id", str3);
                str.Append("fo", str3);
                if (!string.IsNullOrEmpty(this.Source))
                {
                    str.Append("ro", this.Source);
                }
                SheerResponse.ShowModalDialog(str.ToString(), true);
                args.WaitForPostBack();
            }
        }

        protected void OpenSearch(ClientPipelineArgs args)
        {
            if (args.IsPostBack)
            {
                if (!string.IsNullOrEmpty(args.Result) && (args.Result != "undefined"))
                {
                    Item item = this.GetContentDatabase().Items[args.Result];
                    if (item != null)
                    {
                        if (this.Value != item.Paths.Path)
                        {
                            this.SetModified();
                        }
                        this.Value = item.Paths.Path;
                    }

                    else
                    {
                        this.SetModified();
                        var dirtyResult = args.Result;
                        this.Value = this.CleanResult(dirtyResult);
                    }
                }
            }
            else
            {
                UrlString str = new UrlString("/sitecore/shell/Applications/Dialogs/Bucket Internal Link.aspx");
                string str2 = this.Value;
                string str3 = this.Value;
                Item item2 = this.GetContentDatabase().Items[str2];
                if (item2 != null)
                {
                    str3 = item2.ID.ToString();
                }
                str.Append("db", this.GetContentDatabase().Name);
                str.Append("id", str3);
                str.Append("fo", str3);
                if (!string.IsNullOrEmpty(this.Source))
                {
                    str.Append("ro", this.Source);
                }
                SheerResponse.ShowModalDialog(str.ToString(), "900", "600", "", true);
                args.WaitForPostBack();
            }
        }

        private string CleanResult(string result)
        {
            if (result.Contains("tag:"))
            {
                result = result.Remove(result.IndexOf("tag:"), result.IndexOf("tagid=") - result.IndexOf("tag:"));
                result = result.Replace("tagid=", "tag:");
            }
            return result;
        }

        protected override void SetModified()
        {
            base.SetModified();
            if (base.TrackModified)
            {
                Sitecore.Context.ClientPage.Modified = true;
            }
        }

        // Properties
        public string Source
        {
            get
            {
                return base.GetViewStateString("Source");
            }
            set
            {
                string str = MainUtil.UnmapPath(value);
                if (str.EndsWith("/"))
                {
                    str = str.Substring(0, str.Length - 1);
                }
                base.SetViewStateString("Source", str);
            }
        }
    }





}
