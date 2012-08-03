//-----------------------------------------------------------------------
// <copyright file="DateRangeFacet.cs" company="Sitecore A/S">
//     Sitecore A/S. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sitecore.ItemBucket.Kernel.Search.Facets
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using Lucene.Net.Search;

    using Sitecore.ItemBucket.Kernel.Kernel.Util;
    using IndexSearcher = Sitecore.ItemBucket.Kernel.Util.IndexSearcher;
    
    /// <summary>
    /// Facet class implementation for Faceting by Date
    /// </summary>
    internal class DateRangeFacet : IFacet
    {
        /// <summary>
        /// Given a Lucene Query, this will build the facets and append them to find the Bit comparison of two queries.
        /// </summary>
        /// <returns>List of FacetReturn</returns>
        /// <param name="query">The initial query</param>
        /// <param name="searchQuery">The raw search query</param>
        /// <param name="locationFilter">The location used to determine which index to use</param>
        /// <param name="baseQuery">The initial queries bit array</param>
        public List<FacetReturn> Filter(Query query, List<SearchStringModel> searchQuery, string locationFilter, BitArray baseQuery)
        {
            var listOfDates = new Dictionary<DateTime, string>
                {
                    { DateTime.Now.AddDays(-1), "Within a Day" },
                    { DateTime.Now.AddDays(-7), "Within a Week" },
                    { DateTime.Now.AddMonths(-1), "Within a Month" },
                    { DateTime.Now.AddYears(-1), "Within a Year" },
                    { DateTime.Now.AddYears(-3), "Older" }
                };
            var returnFacets =
                          this.GetSearch(query, listOfDates.Select(date => date.Key.ToString() + "|" + date.Value).ToList(), searchQuery, locationFilter, baseQuery).Select(
                          facet =>
                          new FacetReturn
                              {
                                  KeyName = DecodeDateSearch(DateTime.Parse(facet.Key.Split('|')[0]), listOfDates),
                                  Value = facet.Value.ToString(),
                                  Type = "date range"
                              });
            return returnFacets.ToList();
        }

        /// <summary>
        /// Given a Lucene Query, this will build the facets and append them to find the Bit comparison of two queries.
        /// </summary>
        /// <returns>List of FacetReturn</returns>
        /// <param name="query">The initial query</param>
        /// <param name="filter">The facet filter</param>
        /// <param name="searchQuery">The raw search query</param>
        /// <param name="locationFilter">The location used to determine which index to use</param>
        /// <param name="baseQuery">The initial queries bit array</param>
        public Dictionary<string, int> GetSearch(Query query, List<string> filter, List<SearchStringModel> searchQuery, string locationFilter, BitArray baseQuery)
        {
            using (var searcher = new IndexSearcher(Util.Constants.Index.Name))
            {
                var results = searcher.RunFacet(query, false, true, 0, 0, "__smallCreatedDate", filter, baseQuery, locationFilter);
                return results;
            }
        }

        /// <summary>
        /// This will find the string name of the date facet based off the JSON parse query from the handler
        /// </summary>
        /// <returns>The name of the facet</returns>
        /// <value>Within a Year</value>
        /// <param name="dateTime">The JSON date</param>
        /// <param name="listOfDates">The available facets</param>
        private static string DecodeDateSearch(DateTime dateTime, Dictionary<DateTime, string> listOfDates)
        {
            return listOfDates.Where(date => date.Key.ToString() == dateTime.ToString()).First().Value;
        }
    }
}
