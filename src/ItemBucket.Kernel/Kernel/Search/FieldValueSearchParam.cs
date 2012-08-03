namespace Sitecore.ItemBucket.Kernel.Search
{
    using Sitecore.Collections;
    using Sitecore.Search;

    public class FieldValueSearchParam : SearchParam
    {
        public FieldValueSearchParam()
        {
            this.Refinements = new SafeDictionary<string>();
        }

        public QueryOccurance Occurance { get; set; }
    }
}
