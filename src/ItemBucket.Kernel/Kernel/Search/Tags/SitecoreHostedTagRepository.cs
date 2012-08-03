namespace Sitecore.ItemBucket.Kernel.Kernel.Search.Tags
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Sitecore.Data.Fields;
    using Sitecore.ItemBucket.Kernel.Common.Providers;
    using Sitecore.ItemBucket.Kernel.ItemExtensions.Axes;
    using Sitecore.ItemBucket.Kernel.Kernel.Util;

    public class SitecoreHostedTagRepository : ITagRepository
    {
        public IEnumerable<Tag> All()
        {
            var tagParent = Context.ContentDatabase.IsNull()
                                            ? Context.Database.GetItem(((ReferenceField)ItemBucket.Kernel.Util.Constants.SettingsItem.Fields["Tag Parent"]).TargetItem.ID)
                                            : Context.ContentDatabase.GetItem((
                                                (ReferenceField)ItemBucket.Kernel.Util.Constants.SettingsItem.Fields["Tag Parent"]).TargetItem.ID);
            int hitsCount;
            return tagParent.Search(out hitsCount, location: tagParent.ID.ToString(), templates: "{58DA2398-0F91-4989-AB76-78DAC905E775}").Select(itemreturn => new Tag(itemreturn.GetItem().Name, itemreturn.GetItem().ID.ToString()));
        }

        public Tag Single(Func<Tag, bool> exp)
        {
            var tagParent = Context.ContentDatabase.IsNull()
                                            ? Context.Database.GetItem(((ReferenceField)ItemBucket.Kernel.Util.Constants.SettingsItem.Fields["Tag Parent"]).TargetItem.ID)
                                            : Context.ContentDatabase.GetItem((
                                                (ReferenceField)ItemBucket.Kernel.Util.Constants.SettingsItem.Fields["Tag Parent"]).TargetItem.ID);
            int hitsCount;
            return tagParent.Search(out hitsCount, location: tagParent.ID.ToString()).Select(itemreturn => new Tag(itemreturn.GetItem().Name, itemreturn.GetItem().ID.ToString())).Single();
        }

        public Tag First(Func<Tag, bool> exp)
        {
            var tagParent = Context.ContentDatabase.IsNull()
                                       ? Context.Database.GetItem(((ReferenceField)ItemBucket.Kernel.Util.Constants.SettingsItem.Fields["Tag Parent"]).TargetItem.ID)
                                       : Context.ContentDatabase.GetItem((
                                           (ReferenceField)ItemBucket.Kernel.Util.Constants.SettingsItem.Fields["Tag Parent"]).TargetItem.ID);
            int hitsCount;
            return tagParent.Search(out hitsCount, location: tagParent.ID.ToString()).Select(itemreturn => new Tag(itemreturn.GetItem().Name, itemreturn.GetItem().ID.ToString())).First();
        }

        public IEnumerable<Tag> GetTags(string contains)
        {
            var tagParent = Context.ContentDatabase.IsNull()
                                      ? Context.Database.GetItem(((ReferenceField)ItemBucket.Kernel.Util.Constants.SettingsItem.Fields["Tag Parent"]).TargetItem.ID)
                                      : Context.ContentDatabase.GetItem((
                                           (ReferenceField)ItemBucket.Kernel.Util.Constants.SettingsItem.Fields["Tag Parent"]).TargetItem.ID);
            int hitsCount;
            return tagParent.Search(out hitsCount, location: tagParent.ID.ToString(), text: contains).Select(itemreturn => new Tag(itemreturn.GetItem().Name, itemreturn.GetItem().ID.ToString()));
        }

        public Tag GetTagByValue(string value)
        {
            var tagParent = Context.ContentDatabase.IsNull()
                                          ? Context.Database.GetItem(((ReferenceField)ItemBucket.Kernel.Util.Constants.SettingsItem.Fields["Tag Parent"]).TargetItem.ID)
                                          : Context.ContentDatabase.GetItem((
                                           (ReferenceField)ItemBucket.Kernel.Util.Constants.SettingsItem.Fields["Tag Parent"]).TargetItem.ID);
            int hitsCount;
            return tagParent.Search(out hitsCount, location: tagParent.ID.ToString(), id: value).Select(itemreturn => new Tag(itemreturn.GetItem().Name, itemreturn.GetItem().ID.ToString())).First();
        }
    }
}
