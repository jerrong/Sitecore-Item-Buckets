using Lucene.Net.Analysis.Standard;
using Microsoft.Practices.ServiceLocation;
using Sitecore.BigData;
using Sitecore.ItemBucket.Kernel.Kernel.Search.SOLR;
using Sitecore.ItemBucket.Kernel.Kernel.Search.SOLR.SOLRItems;
using Sitecore.ItemBuckets.BigData.RamDirectory;
using Sitecore.ItemBuckets.BigData.RemoteIndex;
using SolrNet;
using SolrNet.Commands.Parameters;
using SolrNet.Impl;

namespace Sitecore.ItemBucket.Kernel.Util
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using Lucene.Net.Index;
    using Lucene.Net.Search;

    using Sitecore.Diagnostics;
    using Sitecore.Events;
    using Sitecore.ItemBucket.Kernel.Kernel.Search;
    using Sitecore.ItemBucket.Kernel.Kernel.Util;
    using Sitecore.ItemBucket.Kernel.Managers;
    using Sitecore.ItemBucket.Kernel.Search;
    using Sitecore.Search;

    public class IndexSearcher : IDisposable
    {
        #region ctor

        public IndexSearcher(string indexId)
        {
            Index = SearchManager.GetIndex(BucketManager.GetContextIndex(Sitecore.Context.Item));
        }

        #endregion ctor

        #region Properties

        public static ILuceneIndex Index
        {
            get;
            set;
        }

        #endregion Properties

        #region Query Runner Methods

        public virtual KeyValuePair<int, List<SitecoreItem>> RunQuery(Query query, int pageSize, int pageNumber, string sortField, string sortDirection)
        {
            var items = new List<SitecoreItem>();
            int hitCount;
            if (Config.EnableBucketDebug || Constants.EnableTemporaryBucketDebug)
            {
                Log.Info("Bucket Debug Query: " + query, this);
            }

            if (Config.SOLREnabled == "true")
            {
                GetValue(query, items);
                return new KeyValuePair<int, List<SitecoreItem>>(items.Count, items);
            }
            else
            {
                if(Index is Index)
                {
                    (Index as Index).Analyzer = new StandardAnalyzer(Consts.StopWords);
                }
                using (var context = new SortableIndexSearchContext(Index))
                {
                    BooleanQuery.SetMaxClauseCount(Config.LuceneMaxClauseCount);
                    var sortingDir = sortDirection == "asc" ? false : true;
                    var searchHits = string.IsNullOrWhiteSpace(sortField)
                                         ? context.Search(query)
                                         : context.Search(query, new Sort(sortField, sortingDir));
                    if (searchHits.IsNull())
                    {
                        return new KeyValuePair<int, List<SitecoreItem>>();
                    }

                    hitCount = searchHits.Length;
                    if (pageSize == 0)
                    {
                        pageSize = searchHits.Length;
                    }

                    if (pageNumber == 1)
                    {
                        pageNumber = 1;
                    }


                    var resultCollection = new SearchResultCollection();

                    resultCollection = searchHits.FetchResults((pageNumber - 1) * pageSize, pageSize);


                    SearchHelper.GetItemsFromSearchResult(resultCollection, items);
                }
                return new KeyValuePair<int, List<SitecoreItem>>(hitCount, items);
            }
        }

        private static void GetValue(Query query, List<SitecoreItem> items)
        {
            if (Index is Index)
            {
                if ((Index as Index).Name == "itembuckets_templates")
                {
                    var solr = ServiceLocator.Current.GetInstance<ISolrOperations<SolrTemplateItem>>();
                    SolrQueryResults<SolrTemplateItem> remoteSearch = solr.Query(new SolrQuery(query.ToString()));
                    SearchHelper.GetItemsFromSearchResultFromSOLR(remoteSearch, items);
                }
                if ((Index as Index).Name == "itembuckets_buckets")
                {
                    var solr = ServiceLocator.Current.GetInstance<ISolrOperations<SolrBucketItem>>();
                    SolrQueryResults<SolrBucketItem> remoteSearch = solr.Query(new SolrQuery(query.ToString()));
                    SearchHelper.GetItemsFromSearchResultFromSOLR(remoteSearch, items);
                }
                if ((Index as Index).Name == "itembuckets_sitecore")
                {
                    var solr = ServiceLocator.Current.GetInstance<ISolrOperations<SolrSitecoreItem>>();
                    SolrQueryResults<SolrSitecoreItem> remoteSearch = solr.Query(new SolrQuery(query.ToString()));
                    SearchHelper.GetItemsFromSearchResultFromSOLR(remoteSearch, items);
                }
                if ((Index as Index).Name == "itembuckets_layoutsfolder")
                {
                    var solr = ServiceLocator.Current.GetInstance<ISolrOperations<SolrLayoutItem>>();
                    SolrQueryResults<SolrLayoutItem> remoteSearch = solr.Query(new SolrQuery(query.ToString()));
                    SearchHelper.GetItemsFromSearchResultFromSOLR(remoteSearch, items);
                }
                if ((Index as Index).Name == "itembuckets_systemfolder")
                {
                    var solr = ServiceLocator.Current.GetInstance<ISolrOperations<SolrSystemItem>>();
                    SolrQueryResults<SolrSystemItem> remoteSearch = solr.Query(new SolrQuery(query.ToString()));
                    SearchHelper.GetItemsFromSearchResultFromSOLR(remoteSearch, items);
                }
                if ((Index as Index).Name == "itembuckets_medialibrary")
                {
                    var solr = ServiceLocator.Current.GetInstance<ISolrOperations<SolrMediaItem>>();
                    SolrQueryResults<SolrMediaItem> remoteSearch = solr.Query(new SolrQuery(query.ToString()));
                    SearchHelper.GetItemsFromSearchResultFromSOLR(remoteSearch, items);
                }
            }

            
        }

        public virtual KeyValuePair<int, List<SitecoreItem>> RunQuery(Query query, int pageSize, int pageNumber)
        {
            return this.RunQuery(query, pageSize, pageNumber, null, null);
        }

        public virtual Dictionary<string, int> RunFacet(Query query, bool showAllVersions, bool isFacet, int pageSize, int pageNumber, string termName, List<string> termValue, BitArray queryBase, string locationFilter)
        {
            return RunFacet(query, showAllVersions, isFacet, false, pageSize, pageNumber, termName, termValue, queryBase, locationFilter);
        }

        public virtual Dictionary<string, int> RunFacet(Query query, bool showAllVersions, bool isFacet, bool isIdLookup, int pageSize, int pageNumber, string termName, List<string> termValue, BitArray queryBase)
        {
            return RunFacet(query, showAllVersions, isFacet, false, pageSize, pageNumber, termName, termValue, queryBase, "");
        }

        public virtual Dictionary<string, int> RunFacet(Query query, bool showAllVersions, bool isFacet, bool isIdLookup, int pageSize, int pageNumber, string termName, List<string> termValue, BitArray queryBase, string locationFilter)
        {

            var runningCOunt = new Dictionary<string, int>();
            var db = Context.ContentDatabase ?? Sitecore.Context.Database;
            var indexName = BucketManager.GetContextIndex(db.GetItem(locationFilter));
      
            if (indexName.EndsWith("_remote"))
            {
                Index = RemoteSearchManager.GetIndex(indexName) as RemoteIndex;
            }
            else if (indexName.EndsWith("_inmemory"))
            {
                Index = InMemorySearchManager.GetIndex(indexName) as InMemoryIndex;
            }
            else
            {
                Index = SearchManager.GetIndex(indexName) as Index;
            }
            using (var context = new SortableIndexSearchContext(Index))
            {
                if (Config.EnableBucketDebug || Constants.EnableTemporaryBucketDebug)
                {
                    Log.Info("Using: " + indexName, this);
                    Log.Info("Bucket Facet Original Debug Query: " + query, this);
                }

                foreach (var terms in termValue)
                {
                    var genreQueryFilter = GenreQueryFilter(query, isFacet, isIdLookup, termName, terms);
                    var tempSearchArray = queryBase.Clone() as BitArray;
                    if (Config.EnableBucketDebug || Constants.EnableTemporaryBucketDebug)
                    {
                        Log.Info("Bucket Facet Debug Query: " + genreQueryFilter, this);
                    }

                    BitArray genreBitArray = genreQueryFilter.Bits(context.Searcher.GetIndexReader());
                    if (tempSearchArray.Length == genreBitArray.Length)
                    {
                        BitArray combinedResults = tempSearchArray.And(genreBitArray);

                        var cardinality = SearchHelper.GetCardinality(combinedResults);

                        if (cardinality > 0)
                        {
                            if (!isFacet)
                            {
                                if (!runningCOunt.ContainsKey(terms))
                                {
                                    runningCOunt.Add(terms, cardinality);
                                }
                            }
                            else
                            {
                                if (!runningCOunt.ContainsKey(terms))
                                {
                                    runningCOunt.Add(terms, cardinality);
                                }
                            }
                        }
                    }
                }
            }

            return runningCOunt;
        }

        public virtual QueryFilter GenreQueryFilter(Query query, bool isFacet, bool isIdLookup, string termName, string terms)
        {
            var tempTerms = terms;
            var newGuid = new Guid();
            var isGuid = Guid.TryParse(terms, out newGuid);
            if (isGuid)
            {
                tempTerms = IdHelper.NormalizeGuid(terms, true);
            }

            var genreQueryFilter = new QueryFilter(query);
            if (!isFacet)
            {
                if (termName == "_language" || isIdLookup)
                {
                    var termValueParse = terms.Split('|')[0].ToLowerInvariant();
                    if (isIdLookup)
                    {
                        termValueParse = IdHelper.NormalizeGuid(termValueParse, true);
                    }
                    genreQueryFilter =
                        new QueryFilter(
                            new TermQuery(new Term(termName.ToLowerInvariant(), termValueParse)));
                }
                else if (termName == "size" || termName == "dimensions")
                {
                    var term = new BooleanQuery();
                    term.Add(new TermQuery(new Term(termName, terms)), BooleanClause.Occur.MUST);
                    genreQueryFilter = new QueryFilter(term);
                }
                else
                {
                    genreQueryFilter =
                        new QueryFilter(
                            new TermQuery(new Term(terms.Split('|')[0].ToLowerInvariant(),
                                                   termName.ToLowerInvariant())));
                }
            }
            else
            {
                if (termName == "__created by")
                {
                    genreQueryFilter =
                        new QueryFilter(new TermQuery(new Term(termName, tempTerms)));
                }
                else
                {
                    if (Config.ExcludeContextItemFromResult)
                    {
                        if (termName == "_path")
                        {
                            var term = new BooleanQuery();
                            term.Add(new TermQuery(new Term(termName, tempTerms.ToLowerInvariant())), BooleanClause.Occur.MUST);
                            term.Add(new TermQuery(new Term(BuiltinFields.ID, tempTerms.ToLowerInvariant())),
                                     BooleanClause.Occur.MUST_NOT);
                            genreQueryFilter = new QueryFilter(term);
                        }
                        else
                        {
                            var term = new TermQuery(new Term(termName, tempTerms.ToLowerInvariant()));
                            genreQueryFilter = new QueryFilter(term);
                        }
                    }
                    else
                    {
                        var term = new TermQuery(new Term(termName, tempTerms.ToLowerInvariant()));
                        genreQueryFilter = new QueryFilter(term);
                    }
                }
            }
            if (termName == "__smallCreatedDate")
            {
                var dateStart = terms.Split('|')[0];
                var typeOfDate = terms.Split('|')[1];
                var dateEnd = new DateTime();
                if (typeOfDate == "Within a Day")
                {
                    dateEnd = DateTime.Now;
                }
                if (typeOfDate == "Within a Week")
                {
                    dateEnd = DateTime.Now.AddDays(-1);
                }
                if (typeOfDate == "Within a Month")
                {
                    dateEnd = DateTime.Now.AddDays(-7);
                }
                if (typeOfDate == "Within a Year")
                {
                    dateEnd = DateTime.Now.AddMonths(-1);
                }
                if (typeOfDate == "Older")
                {
                    dateEnd = DateTime.Now.AddYears(-1);
                }

                var boolQuery = new BooleanQuery(true);
                SearcherMethods.AddDateRangeQuery(boolQuery,
                                                  new DateRangeSearchParam.DateRangeField(termName, DateTime.Parse(dateStart),
                                                                                          dateEnd)
                                                      {InclusiveEnd = true, InclusiveStart = true}, BooleanClause.Occur.MUST);
                genreQueryFilter = new QueryFilter(boolQuery);
                if (Config.EnableBucketDebug || Constants.EnableTemporaryBucketDebug)
                {
                    Log.Info("Search Clauses Number: " + boolQuery.Clauses().Count, this);
                }
            }
            return genreQueryFilter;
        }

        internal virtual KeyValuePair<int, List<SitecoreItem>> RunQuery(QueryBase query)
        {
            var translator = new QueryTranslator(Index);
            var luceneQuery = translator.Translate(query);
            return this.RunQuery(luceneQuery, 20, 0);
        }


        internal virtual KeyValuePair<int, List<SitecoreItem>> RunQuery(QueryBase query, int numberOfResults)
        {
            var translator = new QueryTranslator(Index);
            var luceneQuery = translator.Translate(query);
            return this.RunQuery(luceneQuery, numberOfResults, 0);
        }

        #endregion

        #region Searching Methods

        internal virtual KeyValuePair<int, List<SitecoreItem>> GetItemsViaTermQuery(string fieldName, string fieldValue)
        {
            var query = new TermQuery(new Term(fieldName, fieldValue));
            return this.RunQuery(query, 20, 0);
        }

        internal virtual KeyValuePair<int, List<SitecoreItem>> GetItemsViaFieldQuery(string fieldName, string fieldValue)
        {
            var query = new FieldQuery(fieldName, fieldValue);
            return this.RunQuery(query);
        }

        internal virtual KeyValuePair<int, List<SitecoreItem>> GetItemsViaFieldQuery(string fieldName, string fieldValue, int numberOfResults)
        {
            var query = new FieldQuery(fieldName, fieldValue);
            return this.RunQuery(query, numberOfResults);
        }

        internal virtual KeyValuePair<int, List<SitecoreItem>> GetItems(SearchParam param)
        {
            var globalQuery = new CombinedQuery();
            SearcherMethods.ApplyLanguageClause(globalQuery, param.Language);
            if (!string.IsNullOrWhiteSpace(param.FullTextQuery))
            {
                SearcherMethods.ApplyFullTextClause(globalQuery, param.FullTextQuery);
            }

            SearcherMethods.ApplyRelationFilter(globalQuery, param.RelatedIds);
            SearcherMethods.ApplyTemplateFilter(globalQuery, param.TemplateIds);
            SearcherMethods.ApplyLocationFilter(globalQuery, param.LocationIds);
            SearcherMethods.ApplyRefinements(globalQuery, param.Refinements, QueryOccurance.Must);

            return this.RunQuery(globalQuery);
        }

        internal virtual KeyValuePair<int, List<SitecoreItem>> GetItems(DateRangeSearchParam param)
        {
            var db = Context.ContentDatabase ?? Sitecore.Context.Database;
            if (db != null)
            {
                var indexName = Util.Constants.Index.Name;
                var item = db.GetItem(param.LocationIds);
                indexName = BucketManager.GetContextIndex(item.IsNotNull() ? item : db.GetItem(Sitecore.ItemIDs.RootID));

                if (indexName.EndsWith("_remote"))
                {
                    Index = RemoteSearchManager.GetIndex(indexName) as RemoteIndex;
                }
                else if (indexName.EndsWith("_inmemory"))
                {
                    Index = InMemorySearchManager.GetIndex(indexName) as InMemoryIndex;
                }
                else
                {
                    Index = SearchManager.GetIndex(indexName) as Index;
                }

                if (Index.IsNotNull())
                {
                    var globalQuery = new CombinedQuery();
                    SearcherMethods.ApplyLanguageClause(globalQuery, param.Language);
                    if (!string.IsNullOrWhiteSpace(param.FullTextQuery))
                    {
                        if (!param.FullTextQuery.StartsWith("*"))
                        {
                            if (param.FullTextQuery != "*All*" && param.FullTextQuery != "*" && param.FullTextQuery != "**")
                            {
                                SearcherMethods.ApplyFullTextClause(globalQuery, param.FullTextQuery, indexName);
                            }
                        }
                    }

                    SearcherMethods.ApplyRelationFilter(globalQuery, param.RelatedIds);
                    SearcherMethods.ApplyTemplateFilter(globalQuery, param.TemplateIds);
                    SearcherMethods.ApplyTemplateNotFilter(globalQuery);
                    SearcherMethods.ApplyIDFilter(globalQuery, param.ID);
                    if (param.LocationIds.Contains("|"))
                    {
                        SearcherMethods.ApplyCombinedLocationFilter(globalQuery, param.LocationIds);
                    }
                    else
                    {
                        SearcherMethods.ApplyLocationFilter(globalQuery, param.LocationIds);
                    }
                    if (!param.Refinements.ContainsKey("__workflow state")) //Hack!!!!!
                    {
                        SearcherMethods.ApplyRefinements(globalQuery, param.Refinements, QueryOccurance.Should);
                    }
                    SearcherMethods.ApplyLatestVersion(globalQuery);

                    if (Config.ExcludeContextItemFromResult)
                    {
                        SearcherMethods.ApplyContextItemRemoval(globalQuery, param.LocationIds);
                    }

                    var translator = new QueryTranslator(Index);
                    var booleanQuery = translator.ConvertCombinedQuery(globalQuery);
                    var innerOccurance = translator.GetOccur(param.Occurance);

                    if (!string.IsNullOrWhiteSpace(param.FullTextQuery))
                    {
                        if (param.FullTextQuery.StartsWith("*"))
                        {
                            if (param.FullTextQuery != "*All*" && param.FullTextQuery != "*" && param.FullTextQuery != "**")
                            {
                                SearcherMethods.ApplyFullTextClause(booleanQuery, param.FullTextQuery, indexName);
                            }
                        }
                    }

                    SearcherMethods.ApplyAuthor(booleanQuery, param.Author);
                    SearcherMethods.ApplyDateRangeSearchParam(booleanQuery, param, innerOccurance);
                    if (param.Refinements.ContainsKey("__workflow state"))
                    {
                        SearcherMethods.AddFieldValueClause(booleanQuery, "__workflow state", param.Refinements["__workflow state"], QueryOccurance.Should);
                    }
                    if (Config.EnableBucketDebug || Constants.EnableTemporaryBucketDebug)
                    {
                        Log.Info("Search Clauses Number: " + booleanQuery.Clauses().Count, this);
                    }

                    if (!param.SortByField.IsNullOrEmpty())
                    {
                        return this.RunQuery(booleanQuery, param.PageSize, param.PageNumber, param.SortByField, param.SortDirection);
                    }

                    return param.PageNumber != 0 ? this.RunQuery(booleanQuery, param.PageSize, param.PageNumber) : this.RunQuery(booleanQuery, 20, 0);
                }
            }

            return new KeyValuePair<int, List<SitecoreItem>>();
        }

        public static BooleanQuery ContructQuery(DateRangeSearchParam param)
        {
            var indexName = BucketManager.GetContextIndex(Context.ContentDatabase.GetItem(param.LocationIds));

            if (indexName.EndsWith("_remote"))
            {
                Index = RemoteSearchManager.GetIndex(indexName) as RemoteIndex;
            }
            else if (indexName.EndsWith("_inmemory"))
            {
                Index = InMemorySearchManager.GetIndex(indexName) as InMemoryIndex;
            }
            else
            {
                Index = SearchManager.GetIndex(indexName) as Index;
            }
            if (Index.IsNotNull())
            {
                var globalQuery = new CombinedQuery();
                var fullTextQuery = param.FullTextQuery; // .TrimStart('*').TrimEnd('*') + "*";
                SearcherMethods.ApplyLanguageClause(globalQuery, param.Language);
                if (!string.IsNullOrWhiteSpace(fullTextQuery))
                {
                    if (!fullTextQuery.StartsWith("*"))
                    {
                        SearcherMethods.ApplyFullTextClause(globalQuery, fullTextQuery, indexName);
                    }
                }

                SearcherMethods.ApplyRelationFilter(globalQuery, param.RelatedIds);
                SearcherMethods.ApplyTemplateFilter(globalQuery, param.TemplateIds);
                SearcherMethods.ApplyTemplateNotFilter(globalQuery);
                SearcherMethods.ApplyIDFilter(globalQuery, param.ID);
                SearcherMethods.ApplyLocationFilter(globalQuery, param.LocationIds);
                SearcherMethods.ApplyRefinements(globalQuery, param.Refinements, QueryOccurance.Should);
                SearcherMethods.ApplyLatestVersion(globalQuery);

                if (Config.ExcludeContextItemFromResult)
                {
                    SearcherMethods.ApplyContextItemRemoval(globalQuery, param.LocationIds);
                }

                var translator = new QueryTranslator(Index);
                var booleanQuery = translator.ConvertCombinedQuery(globalQuery);
                var innerOccurance = translator.GetOccur(param.Occurance);

                if (!string.IsNullOrWhiteSpace(fullTextQuery))
                {
                    if (fullTextQuery.StartsWith("*"))
                    {
                        SearcherMethods.ApplyFullTextClause(booleanQuery, fullTextQuery, indexName);
                    }
                }

                SearcherMethods.ApplyAuthor(booleanQuery, param.Author);
                SearcherMethods.ApplyDateRangeSearchParam(booleanQuery, param, innerOccurance);
                if (Config.EnableBucketDebug || Constants.EnableTemporaryBucketDebug)
                {
                    Log.Info("Search Clauses Number: " + booleanQuery.Clauses().Count, booleanQuery);
                }

                return booleanQuery;
            }

            return null;
        }

        internal virtual KeyValuePair<int, List<SitecoreItem>> GetItems(FieldValueSearchParam param)
        {
            var globalQuery = new CombinedQuery();

            SearcherMethods.ApplyLanguageClause(globalQuery, param.Language);
            if (!string.IsNullOrWhiteSpace(param.FullTextQuery))
            {
                SearcherMethods.ApplyFullTextClause(globalQuery, param.FullTextQuery);
            }
            SearcherMethods.ApplyRelationFilter(globalQuery, param.RelatedIds);
            SearcherMethods.ApplyTemplateFilter(globalQuery, param.TemplateIds);
            SearcherMethods.ApplyLocationFilter(globalQuery, param.LocationIds);
            SearcherMethods.ApplyRefinements(globalQuery, param.Refinements, param.Occurance);

            return this.RunQuery(globalQuery);
        }

        internal virtual bool ContainsItemsByFields(string ids, string fieldName, string fieldValue)
        {
            var globalQuery = new CombinedQuery();
            SearcherMethods.ApplyIdFilter(globalQuery, BuiltinFields.ID, ids);
            SearcherMethods.AddFieldValueClause(globalQuery, fieldName, fieldValue, QueryOccurance.Must);
            return this.RunQuery(globalQuery).Value.Any();
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Dispose the Index Searcher
        /// </summary>
        public virtual void Dispose()
        {
            Index = null;
        }

        #endregion
    }
}
