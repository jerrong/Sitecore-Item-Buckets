namespace Sitecore.ItemBucket.Kernel.Kernel.Forms.BucketLinkForm
{
    using System.Collections.Specialized;
    using Sitecore.Data.Serialization;
    using Sitecore.Diagnostics;
    using Sitecore.Globalization;
    using Sitecore.Shell.Framework.Commands;
    using Sitecore.Text;
    using Sitecore.Web.UI.Sheer;
    using Sitecore.Web.UI.WebControls;
    using Sitecore.Web.UI.XamlSharp.Continuations;

    class RollbackRestore : Command, ISupportsContinuation
    {
        // Methods
        public override void Execute(CommandContext context)
        {
            Assert.ArgumentNotNull(context, "context");
            var str = context.Parameters["item"];
            if (string.IsNullOrEmpty(str))
            {
                SheerResponse.Alert("Select an item first.", new string[0]);
            }
            else
            {
                var parameters = new NameValueCollection();
                parameters["item"] = str;
                var args = new ClientPipelineArgs(parameters);
                ContinuationManager.Current.Start(this, "Run", args);
            }
        }

        protected void Run(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            if (args.IsPostBack)
            {
                if (args.Result == "yes")
                {
                    var str = new ListString(args.Parameters["item"]);
                    foreach (string str2 in str)
                    {
                        Manager.LoadItem(str2, new LoadOptions { Database = Context.ContentDatabase, ForceUpdate = true });
                    }

                    SheerResponse.SetLocation(string.Empty);
                }
            }
            else
            {
                var str3 = new ListString(args.Parameters["item"]);
                SheerResponse.Confirm(str3.Count == 1 ? Translate.Text("Do you want to restore \"{0}\"?", new object[] { str3 }) : Translate.Text("Do you want to restore these {0} items?", new object[] { str3.Count }));
                args.WaitForPostBack();
            }
        }
    }
}
