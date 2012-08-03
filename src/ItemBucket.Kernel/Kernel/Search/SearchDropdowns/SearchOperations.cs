namespace Sitecore.ItemBucket.Kernel.Search.SearchDropdowns
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// </summary>
    internal class SearchOperations : ISearchDropDown
    {
        public List<string> Process()
        {
            return Context.ContentDatabase.GetItem(Util.Constants.ItemBucketsSearchOperationsFolder).Axes.GetDescendants().Where(operation => operation.TemplateID.ToString() == "{9F63F3ED-BFB8-49A3-B370-1830E0A5920F}").Select(i => i.Fields["List Name"].Value + "|" + i.Appearance.Icon).ToList();
        }
    }
}
