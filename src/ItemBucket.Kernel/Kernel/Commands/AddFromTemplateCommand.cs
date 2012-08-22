//-----------------------------------------------------------------------
// <copyright file="AddFromTemplateCommand.cs" company="Sitecore A/S">
//     Sitecore A/S. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sitecore.ItemBucket.Kernel.Commands
{
    using System;

    using Sitecore.Data.Items;
    using Sitecore.Events;
    using Sitecore.ItemBucket.Kernel.Kernel.Util;
    using Sitecore.ItemBucket.Kernel.Managers;
    using Sitecore.Reflection;
    using Sitecore.Text;
    using Sitecore.Web.UI.Sheer;
    using Constants = Sitecore.ItemBucket.Kernel.Util.Constants;

    /// <summary>
    /// Extended Hook to capture all Add events
    /// </summary>
    public class AddFromTemplateCommand : Data.Engines.DataCommands.AddFromTemplateCommand
    {
        /// <summary>
        /// Refresh the content tree with the new item opened
        /// </summary>
        /// <param name="item">Item that is being added</param>
        /// <remarks>You will need to override this if you are running without HttpContext e.g. Unit Tests</remarks>
        protected virtual void SetLocation(Item item)
        {
            if (Context.ClientPage.IsNotNull())
            {
                var urlString = new UrlString(Constants.ContentEditorRawUrlAddress);
                urlString.Add(Constants.OpenItemEditorQueryStringKeyName, item.ID.ToString());
                item.Uri.AddToUrlString(urlString);
                UIUtil.AddContentDatabaseParameter(urlString);
                SheerResponse.SetLocation(urlString.ToString());
            }
        }

        #region Protected Overrides
        /// <summary>
        /// Determine the Destination Folder of the item if it is to be automatically categorised
        /// </summary>
        /// <returns>The newly created Item</returns>
        protected override Item DoExecute()
        {
            if (this.CanAddToBucket())
            {
                var newDestination = BucketManager.CreateAndReturnDateFolderDestination(Destination, DateTime.Now);
                if (newDestination.IsNotNull() && !newDestination.Uri.Equals(Destination.Uri))
                {
                    Event.RaiseEvent("item:bucketing:adding", new object[] { this.NewId, this.ItemName, this.TemplateId, newDestination }, this);

                    var item = Nexus.DataApi.AddFromTemplate(TemplateId, newDestination, ItemName, NewId);

                    if (newDestination.IsNotNull())
                    {
                        this.SetLocation(item);
                    }

                    Event.RaiseEvent("item:bucketing:added", new object[] { item }, this);
                    return item;
                }
            }

            return base.DoExecute();
        }

        /// <summary>
        /// Factory Method Override to create a new instance of AddFromTemplateCommand
        /// </summary>
        /// <returns>The instance of AddFromTemplateCommand</returns>
        protected override Data.Engines.DataCommands.AddFromTemplateCommand CreateInstance()
        {
            return new AddFromTemplateCommand();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// This checks if the item being added is of a template type that will be bucketed and 
        /// if the creation destination is a bucket item itself
        /// </summary>
        /// <returns>True if it can be added. False if it cannot.</returns>
        private bool CanAddToBucket()
        {
            return BucketManager.IsTemplateBucketable(this.TemplateId, this.Database) && BucketManager.IsBucket(this.Destination);
        }
        #endregion
    }
}
