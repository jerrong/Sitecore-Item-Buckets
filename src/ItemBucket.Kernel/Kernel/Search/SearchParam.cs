// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SearchParam.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the SearchParam type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------


namespace Sitecore.ItemBucket.Kernel.Search
{
    using Sitecore.Collections;

    public class SearchParam
    {
        public string RelatedIds
        {
            get; set;
        }

        public string TemplateIds
        {
            get; set;
        }

        public string LocationIds
        {
            get; set;
        }

        public string ID
        {
            get; set;
        }

        public string FullTextQuery
        {
            get; set;
        }

        public bool ShowAllVersions
        {
            get; set;
        }

        public string Language
        {
            get; set;
        }

        public string Author
        {
            get; set;
        }

        public string SortByField
        {
            get; set;
        }

        public string ItemName
        {
            get; set;
        }

        public int PageNumber
        {
            get; set;
        }

        public int PageSize
        {
            get; set;
        }

        public bool IsFacet
        {
            get; set;
        }

        public string SortDirection
        {
            get; set;
        }

        public SafeDictionary<string> Refinements
        {
            get; set;
        }

        public SearchParam()
        {
            this.Refinements = new SafeDictionary<string>();
        }
    }
}