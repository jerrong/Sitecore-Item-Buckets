namespace Sitecore.ItemBucket.Kernel.Search
{
    using System.Collections.Generic;

    using Sitecore.Search;

    sealed public class NumericRangeSearchParam : SearchParam
    {
        public class NumericRangeField
        {
            public NumericRangeField() { }

            public NumericRangeField(string fieldName, int start, int end)
            {
                this.FieldName = fieldName.ToLowerInvariant();
                this.Start = start;
                this.End = end;
            }

            #region Properties

            public string FieldName { get; set; }
            public int Start { get; set; }
            public int End { get; set; }

            #endregion Properties
        }

        public List<NumericRangeField> Ranges { get; set; }

        public QueryOccurance Occurance { get; set; }
    }
}
