namespace Sitecore.ItemBucket.Kernel.Search
{
    public class FacetReturn
    {
        private string _title;

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public string Title
        {
            get
            {
                // legacy support... fallback to Type
                return _title ?? Type;
            }
            set
            {
                _title = value;
            }
        }

        /// <summary>
        /// The text to show for this facet
        /// </summary>
        public string KeyName { get; set; }

        /// <summary>
        /// The value, as stored in the search index, for this facet option
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// The number of results found for the facet
        /// </summary>
        public string Value { get; set; }
                
        /// <summary>
        /// The type of facet this is. 
        /// </summary>
        public string Type { get; set; }
        
        public string Template { get; set; }
    }
}
