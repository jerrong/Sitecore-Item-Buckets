// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GoToClosestBucketParent.cs" company="Sitecore">
//   Sitecore
// </copyright>
// <summary>
//   Defines the GoToClosestBucketParent type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.ItemBucket.Kernel.Commands
{
    using Sitecore.Diagnostics;
    using Sitecore.ItemBucket.Kernel.Kernel.Util;
    using Sitecore.ItemBucket.Kernel.Managers;
    using Sitecore.Shell.Framework.Commands;

    /// <summary>
    /// Find closest ancestor that is a Bucket
    /// </summary>
    internal class GoToClosestBucketParent : Command
    {
        /// <summary>
        /// Execute the Command
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        public override void Execute(CommandContext context)
        {
            Assert.ArgumentNotNull(context, "context");
            Assert.IsNotNull(context.Items, "Context items list is null");
            if (context.Items.Length > 0)
            {
                var contextItem = context.Items[0];
                Context.ClientPage.SendMessage(this, "item:load(id=" + contextItem.GetParentSearchItemOrRoot().ID + ")");
            }
        }
    }
}
