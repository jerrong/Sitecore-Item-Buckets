namespace Sitecore.ItemBucket.Kernel.Commands
{
    using System;
    using Sitecore.Shell.Framework.Commands;
    using Sitecore.Web.UI.HtmlControls;
    [Serializable]
    
    internal class ToggleBucket : Command
    {
        public override void Execute(CommandContext context)
        {

        }

        public override string GetClick(CommandContext context, string click)
        {
            return "BucketItems_Click";
        }
        
        public override CommandState QueryState(CommandContext context)
        {
            return !ShowHiddenItems ? CommandState.Enabled : CommandState.Down;
        }

        public static bool ShowHiddenItems
        {
            get
            {
                return Registry.GetBool("/Current_User/UserOptions.View.ShowBucketItems", Context.IsAdministrator);
            }

            set
            {
                Registry.SetBool("/Current_User/UserOptions.View.ShowBucketItems", value);
            }
        }
    }
}




