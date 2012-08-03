namespace Sitecore.ItemBucket.Kernel.Kernel.Forms.BucketLinkForm
{
    using System.Collections.Specialized;
    using System.IO;
    using Sitecore.Diagnostics;
    using Sitecore.Globalization;
    using Sitecore.Shell.Framework.Commands;
    using Sitecore.Text;
    using Sitecore.Web.UI.Sheer;
    using Sitecore.Web.UI.WebControls;
    using Sitecore.Web.UI.XamlSharp.Continuations;

    /// <summary>
    /// Rollback Delete Command
    /// </summary>
    public class RollbackDelete : Command, ISupportsContinuation
    {
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
                parameters["archivename"] = context.Parameters["archivename"];
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
                        Directory.Delete(str2, true);
                    }

                    SheerResponse.SetLocation(string.Empty);
                }
            }
            else
            {
                var str3 = new ListString(args.Parameters["item"]);
                if (str3.Count == 1)
                {
                    SheerResponse.Confirm(Translate.Text("Are you sure you want to permanently delete \"{0}\"?", new object[] { str3 }));
                }
                else
                {
                    SheerResponse.Confirm(Translate.Text("Are you sure you want to permanently delete these {0} items?", new object[] { str3.Count }));
                }

                args.WaitForPostBack();
            }
        }
    }
}
