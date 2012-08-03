namespace Sitecore.ItemBucket.Kernel.FieldTypes
{
    using System;
    using System.Linq;

    using Sitecore.Diagnostics;
    using Sitecore.Shell.Applications.ContentEditor;
    using Sitecore.Shell.Applications.ContentEditor.RichTextEditor;
    using Sitecore.Web.UI.Sheer;

    /// <summary>
    /// Bucket Rich Text Editor Control
    /// </summary>
    internal sealed class BucketRichText : RichText
    {
        // Fields
        private string handle;
        private string itemVersion;
        private bool setValueOnPreRender;

        /// <summary>
        /// Initializes a new instance of the <see cref="BucketRichText"/> class.
        /// </summary>
        public BucketRichText()
        {
            this.Class = "scContentControlHtml";
            this.Activation = true;
            this.AllowTransparency = false;
            this.Attributes["tabindex"] = "-1";
        }

        /// <summary>
        /// Gets or sets Source.
        /// </summary>
        public new string Source
        {
            get
            {
                return this.GetViewStateString("Source").Split('&')[0];
            }

            set
            {
                Assert.ArgumentNotNull(value, "value");
                this.SetViewStateString("Source", value);
            }
        }

        /// <summary>
        /// Edit Text Button
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>
        private new void EditText(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            if (!this.Disabled)
            {
                if (args.IsPostBack)
                {
                    if ((args.Result != null) && (args.Result != "undefined"))
                    {
                        this.UpdateHtml(args);
                    }
                }
                else
                {
                    var url2 = new RichTextEditorUrl
                    {
                        Conversion = RichTextEditorUrl.HtmlConversion.DoNotConvert,
                        Disabled = this.Disabled,
                        FieldID = this.FieldID,
                        ID = this.ID,
                        ItemID = this.ItemID,
                        Language = this.ItemLanguage,
                        Mode = string.Empty,
                        Source = this.Source,
                        Url = "/sitecore/shell/Controls/Rich Text Editor/EditorPage.aspx",
                        Value = this.Value,
                        Version = this.ItemVersion
                    };
                    var urlReturn = url2.GetUrl().ToString();
                    if (this.GetViewStateString("Source").Contains("&"))
                    {
                        urlReturn = this.GetViewStateString("Source").Split('&').Aggregate(urlReturn, (current, queryString) => current + "&" + queryString);
                    }

                    this.handle = url2.Handle;
                    var id = MainUtil.GetMD5Hash(this.Source + this.ItemLanguage);
                    SheerResponse.Eval(string.Concat(new object[] { "scContent.editRichText(\"", urlReturn, "\", \"", id.ToShortID(), "\", ", StringUtil.EscapeJavascriptString(this.Value), ")" }));
                    args.WaitForPostBack();
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
            if (!Sitecore.Context.ClientPage.IsEvent)
            {
                var url2 = new RichTextEditorUrl
                {
                    Conversion = RichTextEditorUrl.HtmlConversion.DoNotConvert,
                    Disabled = this.Disabled,
                    FieldID = this.FieldID,
                    ID = this.ID,
                    ItemID = this.ItemID,
                    Language = this.ItemLanguage,
                    Mode = "ContentEditor",
                    Source = this.Source,
                    Url = string.Empty,
                    Value = this.Value,
                    Version = this.ItemVersion
                };
                var urlReturn = url2.GetUrl().ToString();
                if (this.GetViewStateString("Source").Contains("&"))
                {
                    urlReturn = this.GetViewStateString("Source").Split('&').Aggregate(urlReturn, (current, queryString) => current + "&" + queryString);
                }

                this.handle = url2.Handle;
                this.setValueOnPreRender = true;
                this.SourceUri = urlReturn;
            }
        }
    }
}
