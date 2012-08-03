namespace Sitecore.ItemBucket.Kernel.Search.SearchDropdowns
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// </summary>
    internal class SearchCommands : ISearchDropDown
    {
        public List<string> Process()
        {
            return Context.ContentDatabase.GetItem(Util.Constants.BucketSearchType).Axes.GetDescendants().Select(i => i.Fields["Name"].Value + "|" + i.Fields["Display Text"].Value + "|" + i.Appearance.Icon).ToList();
        }
    }
}
