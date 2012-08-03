namespace Sitecore.ItemBucket.Kernel.Rules
{
    using Sitecore.ItemBucket.Kernel.ItemExtensions.Axes;
    using Sitecore.ItemBucket.Kernel.Kernel.Util;
    using Sitecore.Rules.Conditions;
    using Sitecore.Rules.ContentEditorWarnings;

    public class WithinABucket<T> : WhenCondition<T> where T : ContentEditorWarningsRuleContext
    {
        protected override bool Execute(T ruleContext)
        {
            var item = ruleContext.Item;
            if (item.IsNull() || item.Parent.IsNull())
            {
                return false;
            }

            return item.Parent.IsBucketItemCheck();
        }
    }
}