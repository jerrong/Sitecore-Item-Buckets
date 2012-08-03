using System;
using Sitecore.Data.Events;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Events;
using Sitecore.Rules;
using Sitecore.SecurityModel;

namespace Sitecore.ItemBucket.Kernel.Security.Rules
{
    class ItemEventResolver
    {

        protected void OnItemSaved(object sender, EventArgs args)
        {
            Assert.ArgumentNotNull(sender, "sender");
            Assert.ArgumentNotNull(args, "args");
            var item = Event.ExtractParameter(args, 0) as Item;
            if (item != null)
            {
                RunItemSavedRules(item);
            }
        }

        protected void OnItemSavedRemote(object sender, EventArgs args)
        {
            Assert.ArgumentNotNull(sender, "sender");
            Assert.ArgumentNotNull(args, "args");
            var args2 = args as ItemSavedRemoteEventArgs;
            if (args2 != null)
            {
                RunItemSavedRules(args2.Item);
            }
        }

        private static void RunItemSavedRules(Item item)
        {
            RunRules(item, "/sitecore/system/Settings/Rules/Security Rules/Rules");
        }

        private static void RunRules(Item item, string path)
        {
            Item item2;
            Assert.ArgumentNotNull(path, "path");
            using (new SecurityDisabler())
            {
                item2 = item.Database.GetItem(path);
                if (item2 == null)
                {
                    return;
                }
            }
            var context2 = new RuleContext
            {
                Item = item
            };
            var ruleContext = context2;
            var rules = RuleFactory.GetRules<RuleContext>(item2, "Rule");
            if (rules != null)
            {
                rules.Run(ruleContext);
            }
        }



    }
}
