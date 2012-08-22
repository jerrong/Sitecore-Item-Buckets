namespace Sitecore.ItemBucket.Kernel.Kernel.Rules
{
    using Sitecore.ItemBucket.Kernel.ItemExtensions.Axes;
    using Sitecore.ItemBucket.Kernel.Kernel.Util;
    using Sitecore.Rules.Conditions;
    using Sitecore.Rules.ContentEditorWarnings;

    public class TaggedWith<T> : WhenCondition<T> where T : ContentEditorWarningsRuleContext
    {
        private string tagId;

        protected override bool Execute(T ruleContext)
        {
            var item = ruleContext.Item;
            var field = item.Fields["tags"];

            if (field.IsNotNull())
            {
                if (field.Value.Contains(tagId))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
