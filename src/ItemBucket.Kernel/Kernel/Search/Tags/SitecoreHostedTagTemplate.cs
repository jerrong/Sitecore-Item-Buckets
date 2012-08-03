using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Data.Fields;
using Sitecore.ItemBucket.Kernel.Common.Providers;
using Sitecore.ItemBucket.Kernel.ItemExtensions.Axes;
using Sitecore.ItemBucket.Kernel.Kernel.Util;

namespace Sitecore.ItemBucket.Kernel.Kernel.Search.Tags
{
    public class SitecoreHostedTagTemplate : ITagRepository
    {
        public IEnumerable<Tag> All()
        {
            var tagParent = Context.ContentDatabase.IsNull()
                                            ? Context.Database.GetItem(((ReferenceField)Sitecore.ItemBucket.Kernel.Util.Constants.SettingsItem.Fields["Tag Parent"]).TargetItem.ID)
                                            : Context.ContentDatabase.GetItem((
                                                (ReferenceField)Sitecore.ItemBucket.Kernel.Util.Constants.SettingsItem.Fields["Tag Parent"]).TargetItem.ID);
            var hitsCount = 0;
            return tagParent.Search(out hitsCount, location: "{380E56C2-801A-486F-BA5C-4E545701C146}", templates: "{EED0DE0E-F391-41E4-9BED-BF314D5DB5F4}").Select(itemreturn => new Tag(itemreturn.GetItem().Fields["Tag Name"].Value, itemreturn.GetItem().ID.ToString()));
        }

        public Tag Single(Func<Tag, bool> exp)
        {
            var tagParent = Context.ContentDatabase.IsNull()
                                            ? Context.Database.GetItem(((ReferenceField)Sitecore.ItemBucket.Kernel.Util.Constants.SettingsItem.Fields["Tag Parent"]).TargetItem.ID)
                                            : Context.ContentDatabase.GetItem((
                                                (ReferenceField)Sitecore.ItemBucket.Kernel.Util.Constants.SettingsItem.Fields["Tag Parent"]).TargetItem.ID);
            var hitsCount = 0;
            return tagParent.Search(out hitsCount, location: tagParent.ID.ToString()).Select(itemreturn => new Tag(itemreturn.GetItem().Name, itemreturn.GetItem().ID.ToString())).Single();
        }

        public Tag First(Func<Tag, bool> exp)
        {
            var tagParent = Context.ContentDatabase.IsNull()
                                       ? Context.Database.GetItem(((ReferenceField)Sitecore.ItemBucket.Kernel.Util.Constants.SettingsItem.Fields["Tag Parent"]).TargetItem.ID)
                                       : Context.ContentDatabase.GetItem((
                                           (ReferenceField)Sitecore.ItemBucket.Kernel.Util.Constants.SettingsItem.Fields["Tag Parent"]).TargetItem.ID);
            var hitsCount = 0;
            return tagParent.Search(out hitsCount, location: tagParent.ID.ToString()).Select(itemreturn => new Tag(itemreturn.GetItem().Name, itemreturn.GetItem().ID.ToString())).First();
        }

        public IEnumerable<Tag> GetTags(string contains)
        {
            var tagParent = Context.ContentDatabase.IsNull()
                                      ? Context.Database.GetItem(((ReferenceField)Sitecore.ItemBucket.Kernel.Util.Constants.SettingsItem.Fields["Tag Parent"]).TargetItem.ID)
                                      : Context.ContentDatabase.GetItem(((ReferenceField)Sitecore.ItemBucket.Kernel.Util.Constants.SettingsItem.Fields["Tag Parent"]).TargetItem.ID);
            var hitsCount = 0;
            return tagParent.Search(out hitsCount, location: tagParent.ID.ToString(), text: contains).Select(itemreturn => new Tag(itemreturn.GetItem().Name, itemreturn.GetItem().ID.ToString()));
        }

        public Tag GetTagByValue(string value)
        {
            var tagParent = Context.ContentDatabase.IsNull()
                                          ? Context.Database.GetItem(((ReferenceField)Sitecore.ItemBucket.Kernel.Util.Constants.SettingsItem.Fields["Tag Parent"]).TargetItem.ID)
                                          : Context.ContentDatabase.GetItem(((ReferenceField)Sitecore.ItemBucket.Kernel.Util.Constants.SettingsItem.Fields["Tag Parent"]).TargetItem.ID);
            var hitsCount = 0;
            return tagParent.Search(out hitsCount, location: tagParent.ID.ToString(), id: value).Select(itemreturn => new Tag(itemreturn.GetItem().Name, itemreturn.GetItem().ID.ToString())).First();

        }
    }
}
