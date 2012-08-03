namespace Sitecore.ItemBucket.Kernel.Rules.Actions
{
    using Sitecore.Diagnostics;
    using Sitecore.Rules.ConditionalRenderings;

    internal class SetDataSourceViaSearchQuery<T> : SetDataSource<T> where T : ConditionalRenderingsRuleContext
    {
        // Fields
        private string dataSource;

        // Methods
        public override void Apply(T ruleContext)
        {
            Assert.ArgumentNotNull(ruleContext, "ruleContext");
            this.Apply(ruleContext, this.DataSource);
        }

        // Properties
        public string DataSource
        {
            get
            {
                return this.dataSource ?? string.Empty;
            }

            set
            {
                Assert.ArgumentNotNull(value, "value");
                this.dataSource = value;
            }
        }
    }
}
