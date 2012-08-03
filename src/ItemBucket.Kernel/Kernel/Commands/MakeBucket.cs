// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MakeBucket.cs" company="Sitecore">
//   Sitecore
// </copyright>
// <summary>
//   Defines the MakeBucket type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.ItemBucket.Kernel.Commands
{
    using System.Collections.Specialized;
    using Sitecore.Diagnostics;
    using Sitecore.ItemBucket.Kernel.ItemExtensions.Axes;
    using Sitecore.ItemBucket.Kernel.Kernel.Pipelines;
    using Sitecore.ItemBucket.Kernel.Security;
    using Sitecore.Shell.Framework.Commands;

    /// <summary>
    /// Make A Bucket Command
    /// </summary>
    internal class MakeBucket : Command
    {
        #region Public Override Methods
        /// <summary>
        /// Create a Bucket out of an item
        /// </summary>
        /// <param name="context">Command Context</param>
        public override void Execute(CommandContext context)
        {
            Assert.ArgumentNotNull(context, "context");
            var items = context.Items;
            Assert.IsNotNull(items, "Context items list is null");
            Context.ClientPage.Start("uiBucketItems", new BucketArgs(items[0], new NameValueCollection()));
        }

        /// <summary>
        /// Query State
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <returns>
        /// Command State
        /// </returns>
        public override CommandState QueryState(CommandContext context)
        {
            Error.AssertObject(context, "context");

            var item = context.Items[0];
            if (!new BucketSecurityManager(item).IsAllowedToCreateBucket)
            {
                return CommandState.Disabled;
            }

            if (!item.Locking.HasLock())
            {
                return CommandState.Disabled;
            }

            return item.IsBucketItemCheck() ? CommandState.Disabled : CommandState.Enabled;
        }

        #endregion
    }
}
