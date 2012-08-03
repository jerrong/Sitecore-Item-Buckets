namespace Sitecore.ItemBucket.Kernel.Kernel.Forms.BucketLinkForm
{
    using System.Collections.Specialized;
    using System.IO;
    using Sitecore.Diagnostics;
    using Sitecore.Shell.Framework.Commands;
    using Sitecore.Web.UI.Sheer;
    using Sitecore.Web.UI.WebControls;
    using Sitecore.Web.UI.XamlSharp.Continuations;

    internal class RollbackEmpty : Command, ISupportsContinuation
    {
        // Methods
        public override void Execute(CommandContext context)
        {
            Assert.ArgumentNotNull(context, "context");
            var parameters = new NameValueCollection();
            var args = new ClientPipelineArgs(parameters);
            ContinuationManager.Current.Start(this, "Run", args);
        }

        protected void Run(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            if (args.IsPostBack)
            {
                if (args.Result == "yes")
                {
                    Directory.Delete(string.Format("{0}/ItemSync", Configuration.Settings.SerializationFolder), true);
                    SheerResponse.SetLocation(string.Empty);
                }
            }
            else
            {
                SheerResponse.Confirm("Are you sure you want to permanently delete all the saved snapshots?");
                args.WaitForPostBack();
            }
        }
    }
}
