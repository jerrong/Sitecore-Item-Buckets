// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Rebuild.cs" company="Sitecore">
//   Sitecore
// </copyright>
// <summary>
//   Defines the Rebuild type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.ItemBucket.Kernel.Commands
{
    using Sitecore.Events;
    using Sitecore.Globalization;
    using Sitecore.Shell.Framework;
    using Sitecore.Shell.Framework.Commands;

    /// <summary>
    /// Rebuild Indexes
    /// </summary>
    internal class Rebuild : Command
    {
        /// <summary>
        /// Execute the Rebuild
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        public override void Execute(CommandContext context)
        {
            Event.RaiseEvent("item:bucketing:index:rebuilding", new object[] { }, this);
            Windows.RunUri("control:Bucket.Rebuild", "Business/16x16/data_information.png", Translate.Text("Rebuild Indexes"));
            Event.RaiseEvent("item:bucketing:index:rebuilt", new object[] { }, this);
        }
    }
}
