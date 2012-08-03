// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseCommand.cs" company="Sitecore">
//   Sitecore
// </copyright>
// <summary>
//   Command Extension
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.ItemBucket.Kernel.Kernel.Commands
{
    using Sitecore.Diagnostics;
    using Sitecore.ItemBucket.Kernel.Kernel.Util;
    using Sitecore.Shell.Framework.Commands;

    /// <summary>
    /// Extends Command
    /// </summary>
    internal abstract class BaseCommand : Command
    {
        #region Public Virtual Methods
        /// <summary>
        /// Extends the built in Sitecore Command so that base Query State can always be run but I can extend this to add my overriden behaviour
        /// </summary>
        /// <returns>Command State</returns>
        /// <param name="context">Context of Command</param>
        public new virtual CommandState QueryState(CommandContext context)
        {
            Assert.ArgumentNotNull(context, "context");
            var item = context.Items[0];
            if (item.IsNotNull())
            {
                if (context.Items.Length == 0)
                {
                    return CommandState.Disabled;
                }

                if (item.Appearance.ReadOnly)
                {
                    return CommandState.Disabled;
                }

                if (!item.Access.CanWrite())
                {
                    return CommandState.Disabled;
                }

                if (IsLockedByOther(item))
                {
                    return CommandState.Disabled;
                }
            }

            return base.QueryState(context);
        }
        #endregion
        
        #region Public Virtual Methods
        /// <summary>
        /// Allows you to extend BaseCommand and override the default Execute bahvious that is specified in the Command Class
        /// </summary>
        /// <param name="context">Command Context</param>
        public abstract override void Execute(CommandContext context);
       
        #endregion
    }
}
