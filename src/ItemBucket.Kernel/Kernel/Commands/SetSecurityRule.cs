// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SetSecurityRule.cs" company="Sitecore">
//   Sitecore
// </copyright>
// <summary>
//   Defines the SetSecurityRule type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.ItemBucket.Kernel.Commands
{
    using System.Collections.Specialized;
    using Sitecore.Diagnostics;
    using Sitecore.Globalization;
    using Sitecore.ItemBucket.Kernel.Kernel.Util;
    using Sitecore.Shell.Framework.Commands;
    using Sitecore.StringExtensions;
    using Sitecore.Text;
    using Sitecore.Web.UI.Sheer;
    using Sitecore.Web.UI.WebControls;
    using Sitecore.Web.UI.XamlSharp.Continuations;

    /// <summary>
    /// Set Security Using a Lucene Query
    /// </summary>
    internal class SetSecurityRule : Command, ISupportsContinuation
    {
        /// <summary>
        /// Execute Rule
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        public override void Execute(CommandContext context)
        {
            Assert.ArgumentNotNull(context, "context");
            if (context.CheckCommandContextForItemCount(1))
            {
                var parameters = new NameValueCollection();
                parameters["items"] = this.SerializeItems(context.Items);
                parameters["domainname"] = context.Parameters["domainname"];
                parameters["accountname"] = context.Parameters["accountname"];
                parameters["accounttype"] = context.Parameters["accounttype"];
                parameters["fieldid"] = context.Parameters["fieldid"];
                var args = new ClientPipelineArgs(parameters);
                if (ContinuationManager.Current.IsNotNull())
                {
                    ContinuationManager.Current.Start(this, "Run", args);
                }
                else
                {
                    Context.ClientPage.Start(this, "Run", args);
                }
            }
        }

        /// <summary>
        /// Run Rule
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>
        protected void Run(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            var itemArray = DeserializeItems(args.Parameters["items"]);
            var item = itemArray[0];
            if (itemArray.Length == 0)
            {
                SheerResponse.Eval("alert('{0}');".FormatWith(new object[] { Translate.Text("The item may have been deleted by another user or you do not have permission to access the item.") }), new object[] { false });
                Context.ClientPage.SendMessage(this, "item:refresh");
            }
            else if (SheerResponse.CheckModified())
            {
                if (args.IsPostBack)
                {
                    if (AjaxScriptManager.Current.IsNotNull())
                    {
                        AjaxScriptManager.Current.Dispatch("itemsecurity:changed");
                    }
                    else
                    {
                        Context.ClientPage.SendMessage(this, "itemsecurity:changed");
                        Context.ClientPage.SendMessage(this, "item:refresh(id={0})".FormatWith(new object[] { item.ID.ToString() }));
                    }
                }
                else if (item.Appearance.ReadOnly)
                {
                    SheerResponse.Alert("You cannot edit the '{0}' item because it is protected.", new[] { item.DisplayName });
                }
                else if (!item.Access.CanWrite())
                {
                    SheerResponse.Alert("You cannot edit this item because you do not have write access to it.", new string[0]);
                }
                else if (!item.Access.CanAdmin())
                {
                    SheerResponse.Alert("You cannot set security for the '{0}' item because you do not have administrative access.", new[] { item.DisplayName });
                }
                else
                {
                    var urlString = new UrlString("/sitecore/shell/~/xaml/Sitecore.Shell.Applications.Security.ItemSecurityRuleEditor.aspx");
                    item.Uri.AddToUrlString(urlString);
                    urlString["do"] = StringUtil.GetString(new[] { args.Parameters["domainname"] });
                    urlString["ac"] = StringUtil.GetString(new[] { args.Parameters["accountname"] });
                    urlString["at"] = StringUtil.GetString(new[] { args.Parameters["accounttype"] });
                    urlString["fld"] = StringUtil.GetString(new[] { args.Parameters["fieldid"] });
                    SheerResponse.ShowModalDialog(urlString.ToString(), true);
                    args.WaitForPostBack();
                }
            }
        }
    }
}
