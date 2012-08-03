// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Optimise.cs" company="Sitecore">
//   Sitecore
// </copyright>
// <summary>
//   Defines the Optimise type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.ItemBucket.Kernel.Commands
{
    using Sitecore.Shell.Framework.Commands;

    /// <summary>
    /// Optimise the Index
    /// </summary>
    internal class Optimise : Command
    {
        /// <summary>
        /// Execute the Optimisation of an Index
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        public override void Execute(CommandContext context)
        {
            // TODO: This will currently only do this on the itembuckets_buckets index
            Sitecore.Search.SearchManager.GetIndex(Util.Constants.Index.Name).CreateUpdateContext().Optimize();
        }
    }
}
