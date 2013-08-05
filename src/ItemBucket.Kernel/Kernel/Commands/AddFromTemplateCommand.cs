//-----------------------------------------------------------------------
// <copyright file="AddFromTemplateCommand.cs" company="Sitecore A/S">
//     Sitecore A/S. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sitecore.ItemBucket.Kernel.Commands
{
  using System;

  using Sitecore.Data.Items;
  using Sitecore.Diagnostics;
  using Sitecore.Events;
  using Sitecore.ItemBucket.Kernel.Kernel.Util;
  using Sitecore.ItemBucket.Kernel.Managers;
  using Sitecore.ItemBucket.Kernel.Util;
  using Sitecore.Reflection;
  using Sitecore.Text;
  using Sitecore.Web.UI.Sheer;

  /// <summary>
  /// Extended Hook to capture all Add events.
  /// </summary>
  public class AddFromTemplateCommand : Data.Engines.DataCommands.AddFromTemplateCommand
  {
    /// <summary>
    /// Refresh the content tree with the new item opened.
    /// </summary>
    /// <param name="item">Item that is being added</param>
    /// <remarks>You will need to override this if you are running without HttpContext e.g. Unit Tests.</remarks>
    protected virtual void SetLocation(Item item)
    {
      if ((Context.GetSiteName() == "shell") && Context.ClientPage.IsNotNull() && !Client.Site.Notifications.Disabled)
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
    /// Determine the Destination Folder of the item if it is to be automatically categorized.
    /// </summary>
    /// <returns>The newly created Item.</returns>
    protected override Item DoExecute()
    {
      if (this.CanAddToBucket() && this.TemplateId != Config.BucketTemplateId)
      {
        Item newDestination = BucketManager.CreateAndReturnDateFolderDestination(this.Destination, DateTime.Now);
        newDestination = newDestination.Database.GetItem(newDestination.ID, this.Destination.Language);

        if (newDestination != null && !newDestination.Uri.Equals(this.Destination.Uri))
        {
          EventResult eventResult = Event.RaiseEvent("item:bucketing:adding", new object[] { this.NewId, this.ItemName, this.TemplateId, newDestination }, this);
          if (eventResult.Cancel)
          {
            Log.Info(string.Format("Event {0} was cancelled", "item:bucketing:adding"), this);
            return null;
          }

          Item item = Nexus.DataApi.AddFromTemplate(this.TemplateId, newDestination, this.ItemName, this.NewId);

          if (item.IsNotNull())
          {
            item.Editing.BeginEdit();
            item["__BucketParentRef"] = this.Destination.ID.ToString();
            item.Editing.EndEdit();
            this.SetLocation(item);
          }

          Event.RaiseEvent("item:bucketing:added", new object[] { item }, this);
          return item;
        }
      }

      return base.DoExecute();
    }

    /// <summary>
    /// Factory Method Override to create a new instance of AddFromTemplateCommand.
    /// </summary>
    /// <returns>The instance of AddFromTemplateCommand.</returns>
    protected override Data.Engines.DataCommands.AddFromTemplateCommand CreateInstance()
    {
      return new AddFromTemplateCommand();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// This checks if the item being added is of a template type that will be bucketed and
    /// if the creation destination is a bucket item itself.
    /// </summary>
    /// <returns>True if it can be added. False if it cannot.</returns>
    private bool CanAddToBucket()
    {
      return BucketManager.IsTemplateBucketable(this.TemplateId, this.Database)
             && BucketManager.IsBucket(this.Destination);
    }

    #endregion
  }
}
