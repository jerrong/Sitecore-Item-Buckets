// -----------------------------------------------------------------------
// <copyright file="Initialize.cs" company="Sitecore">
// Sitecore
// </copyright>
// -----------------------------------------------------------------------

namespace Sitecore.ItemBucket.Kernel.Kernel.Pipelines
{
    using Sitecore.Diagnostics;
    using Sitecore.Events.Hooks;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class Initialize : IHook
    {
        /// <summary>
        /// Hook for Initialising Sitecore Item Bucket items
        /// </summary>
        void IHook.Initialize()
        {
            Log.Audit("Writing Custom Cache", this);
        }
    }
}
