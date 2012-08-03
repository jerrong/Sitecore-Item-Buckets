namespace Sitecore.ItemBucket.Kernel.Common.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Sitecore;
    using Sitecore.Data.Items;
    using Sitecore.ItemBucket.Kernel.Kernel.Search;
    using Sitecore.ItemBucket.Kernel.Util;
    using Sitecore.Links;

    /// <summary>
    /// Link Provider for use with Bucketed Items
    /// </summary>
    /// <example>
    /// /sitecore/content/Home/Products/2012/12/13/08/12/12/Item Name will convert to http://sitename/Products/Item Name-20121213081212
    /// </example>
    public class NewsLinkProvider : LinkProvider
    {
        public override string GetItemUrl(Item item, UrlOptions options)
        {
            var datedLinkProvider = new DatedLinkProvider(new TagRepository(new DataContext()));
            Func<Item, UrlOptions, string> defaultUrlGenerator = base.GetItemUrl;
            return datedLinkProvider.GetItemUrl(item, options, defaultUrlGenerator).FirstOrLazy(() => defaultUrlGenerator(item, options));
        }
     
        private class DatedLinkProvider
        {
            private readonly TagRepository _tagRepository;

            public DatedLinkProvider(TagRepository tagRepository)
            {
                this._tagRepository = tagRepository;
            }

            public IEnumerable<string> GetItemUrl(Item item, UrlOptions options, Func<Item, UrlOptions, string> defaultUrlGenerator)
            {
                IOrderedEnumerable<Item> orderByDescending = item.Axes.GetAncestors().OrderByDescending(a => a.Axes.Level); //This can be done quicker
                var orderedAncestors = orderByDescending.Take(5);
                var match = (from ancestor in orderedAncestors select new { ancestor.Name, ancestor.TemplateID, Item = ancestor }).ToList();

                // Match wil contain [day,month,year,first-section]
                // The parents of the item should be in the order folder (year), folder (month), folder (day), section
                var templatesEqual = (from f in match select f.TemplateID).SequenceEqual(new[]
                            {
                               Config.ContainerTemplateId, Config.ContainerTemplateId, Config.ContainerTemplateId, Config.ContainerTemplateId, Config.ContainerTemplateId
                            });

                // The date folders should have lengths 2 for day, 2 for month, 4 for year
                var dateFolderNameLengthsEqual = (from f in match select f.Name.Length).Take(5).SequenceEqual(new[] { 2, 2, 2, 2, 4 });

                if (dateFolderNameLengthsEqual && templatesEqual)
                {
                    var section = orderByDescending.Skip(5).Reverse();
              
                    var appendedSection = default(string);
                    foreach (var sec in section)
                    {
                        if (sec.ID == Sitecore.ItemIDs.ContentRoot || sec.ID == Sitecore.ItemIDs.RootID)
                        {
                            continue;
                        }
                        else if(sec.Paths.FullPath.ToLowerInvariant() == Sitecore.Context.Site.StartPath)
                        {
                            continue;
                        }
                        appendedSection += "/" + sec.Name;
                    }
                    var urlParts = AddDatePart(item, match.Select(m => m.Name).Take(5).Reverse());
                    yield return appendedSection + "/" + string.Join("/", urlParts);
                }
            }

           
            private static IEnumerable<string> AddDatePart(Item item, IEnumerable<string> folderNames)
            {
                var dateFolderAbbrev = string.Join(string.Empty, folderNames);
                var itemUrlPart = string.Format("{0}-{1}", MainUtil.EncodeName(item.Name), dateFolderAbbrev);
                yield return itemUrlPart;
            }
        }
    }
}