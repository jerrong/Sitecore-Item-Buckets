// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IndexSearcher.cs" company="Sitecore A/S">
//   Copyright (C) 2013 by Sitecore A/S
// </copyright>
// <summary>
//   Defines the IndexSearcher type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.ItemBucket.Kernel.Util
{
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.Linq;

  using Lucene.Net.Analysis.Standard;
  using Lucene.Net.Index;
  using Lucene.Net.Search;

  using Microsoft.Practices.ServiceLocation;

  using Sitecore.BigData;
  using Sitecore.Data;
  using Sitecore.Data.Items;
  using Sitecore.Diagnostics;
  using Sitecore.ItemBucket.Kernel.Kernel.Search;
  using Sitecore.ItemBucket.Kernel.Kernel.Search.SOLR.SOLRItems;
  using Sitecore.ItemBucket.Kernel.Kernel.Util;
  using Sitecore.ItemBucket.Kernel.Managers;
  using Sitecore.ItemBucket.Kernel.Search;
  using Sitecore.ItemBuckets.BigData.RamDirectory;
  using Sitecore.ItemBuckets.BigData.RemoteIndex;
  using Sitecore.Search;

  using SolrNet;

  /// <summary>
  /// Represents the IndexSearcher.
  /// </summary>
  public class IndexSearcher : IDisposable
  {
    /// <summary>
    /// The searcher index name
    /// </summary>
    private readonly string searcherIndexName;

    /// <summary>
    /// Initializes a new instance of the <see cref="IndexSearcher"/> class.
    /// </summary>
    /// <param name="indexId">The index id.</param>
    public IndexSearcher(string indexId)
    {
      this.searcherIndexName = indexId;
      Index = SearchManager.GetIndex(BucketManager.GetContextIndex(Context.Item));
    }

    /// <summary>
    /// Gets or sets the index.
    /// </summary>
    /// <value>
    /// The index.
    /// </value>
    public static ILuceneIndex Index { get; set; }

    /// <summary>
    /// Constructs the query.
    /// </summary>
    /// <param name="param">The parameter.</param>
    /// <returns>The Boolean query.</returns>
    public static BooleanQuery ContructQuery(DateRangeSearchParam param)
    {
      string indexName = BucketManager.GetContextIndex(Context.ContentDatabase.GetItem(param.LocationIds));

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
        Index = SearchManager.GetIndex(indexName);
      }

      if (Index.IsNotNull())
      {
        var globalQuery = new CombinedQuery();
        string fullTextQuery = param.FullTextQuery; // .TrimStart('*').TrimEnd('*') + "*";
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
        SearcherMethods.ApplyRefinements(globalQuery, param.Refinements, param.Occurance);
        SearcherMethods.ApplyLatestVersion(globalQuery);

        if (Config.ExcludeContextItemFromResult)
        {
          SearcherMethods.ApplyContextItemRemoval(globalQuery, param.LocationIds);
        }

        var translator = new QueryTranslator(Index);
        BooleanQuery booleanQuery = translator.ConvertCombinedQuery(globalQuery);
        BooleanClause.Occur innerOccurance = translator.GetOccur(param.Occurance);

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

    /// <summary>
    /// Runs the query.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="pageSize">Size of the page.</param>
    /// <param name="pageNumber">The page number.</param>
    /// <param name="sortField">The sort field.</param>
    /// <param name="sortDirection">The sort direction.</param>
    /// <returns>The key value pair of the running query.</returns>
    public virtual KeyValuePair<int, List<SitecoreItem>> RunQuery(
      Query query, int pageSize, int pageNumber, string sortField, string sortDirection)
    {
      var items = new List<SitecoreItem>();
      if (Config.EnableBucketDebug || Constants.EnableTemporaryBucketDebug)
      {
        Log.Info("Bucket Debug Query: " + query, this);
      }

      if (Config.SOLREnabled == "true")
      {
        GetValue(query, items);
        return new KeyValuePair<int, List<SitecoreItem>>(items.Count, items);
      }

      if (Index is Index)
      {
        (Index as Index).Analyzer = new StandardAnalyzer(Consts.StopWords);
      }

      int hitCount;
      using (var context = new SortableIndexSearchContext(Index))
      {
        BooleanQuery.SetMaxClauseCount(Config.LuceneMaxClauseCount);
        bool sortingDir = sortDirection != "asc";
        SearchHits searchHits = string.IsNullOrWhiteSpace(sortField)
                                  ? context.Search(query)
                                  : context.Search(query, new Sort(sortField, sortingDir));
        if (searchHits.IsNull())
        {
          return new KeyValuePair<int, List<SitecoreItem>>();
        }

        hitCount = searchHits.Length;
        if (pageSize == 0 || pageNumber < 1)
        {
          pageSize = searchHits.Length;
          pageNumber = 1;
        }

        SearchResultCollection resultCollection = searchHits.FetchResults((pageNumber - 1) * pageSize, pageSize);

        SearchHelper.GetItemsFromSearchResult(resultCollection, items);
      }

      return new KeyValuePair<int, List<SitecoreItem>>(hitCount, items);
    }

    /// <summary>
    /// Runs the query.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="pageSize">Size of the page.</param>
    /// <param name="pageNumber">The page number.</param>
    /// <returns>The key value pair of the running query.</returns>
    public virtual KeyValuePair<int, List<SitecoreItem>> RunQuery(Query query, int pageSize, int pageNumber)
    {
      return this.RunQuery(query, pageSize, pageNumber, null, null);
    }

    /// <summary>
    /// Runs the facet.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="showAllVersions">if set to <c>true</c> [show all versions].</param>
    /// <param name="faceted">if set to <c>true</c> [is facet].</param>
    /// <param name="pageSize">Size of the page.</param>
    /// <param name="pageNumber">The page number.</param>
    /// <param name="termName">Name of the term.</param>
    /// <param name="termValue">The term value.</param>
    /// <param name="queryBase">The query base.</param>
    /// <param name="locationFilter">The location filter.</param>
    /// <returns>The dictionary of running facet.</returns>
    public virtual Dictionary<string, int> RunFacet(
      Query query,
      bool showAllVersions,
      bool faceted,
      int pageSize,
      int pageNumber,
      string termName,
      List<string> termValue,
      BitArray queryBase,
      string locationFilter)
    {
      return this.RunFacet(
        query, showAllVersions, faceted, false, pageSize, pageNumber, termName, termValue, queryBase, locationFilter);
    }

    /// <summary>
    /// Runs the facet.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="showAllVersions">if set to <c>true</c> [show all versions].</param>
    /// <param name="faceted">if set to <c>true</c> [is facet].</param>
    /// <param name="lookupId">if set to <c>true</c> [is id lookup].</param>
    /// <param name="pageSize">Size of the page.</param>
    /// <param name="pageNumber">The page number.</param>
    /// <param name="termName">Name of the term.</param>
    /// <param name="termValue">The term value.</param>
    /// <param name="queryBase">The query base.</param>
    /// <returns>The dictionary of the running facet.</returns>
    public virtual Dictionary<string, int> RunFacet(
      Query query,
      bool showAllVersions,
      bool faceted,
      bool lookupId,
      int pageSize,
      int pageNumber,
      string termName,
      List<string> termValue,
      BitArray queryBase)
    {
      return this.RunFacet(
        query, showAllVersions, faceted, false, pageSize, pageNumber, termName, termValue, queryBase, string.Empty);
    }

    /// <summary>
    /// Runs the facet.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="showAllVersions">if set to <c>true</c> [show all versions].</param>
    /// <param name="faceted">if set to <c>true</c> [is facet].</param>
    /// <param name="lookupId">if set to <c>true</c> [is id lookup].</param>
    /// <param name="pageSize">Size of the page.</param>
    /// <param name="pageNumber">The page number.</param>
    /// <param name="termName">Name of the term.</param>
    /// <param name="termValue">The term value.</param>
    /// <param name="queryBase">The query base.</param>
    /// <param name="locationFilter">The location filter.</param>
    /// <returns>The dictionary of the running facet.</returns>
    public virtual Dictionary<string, int> RunFacet(
      Query query,
      bool showAllVersions,
      bool faceted,
      bool lookupId,
      int pageSize,
      int pageNumber,
      string termName,
      List<string> termValue,
      BitArray queryBase,
      string locationFilter)
    {
      var runningCOunt = new Dictionary<string, int>();
      Database db = Context.ContentDatabase ?? Context.Database;
      string indexName = BucketManager.GetContextIndex(db.GetItem(locationFilter));

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
        Index = SearchManager.GetIndex(indexName);
      }

      using (var context = new SortableIndexSearchContext(Index))
      {
        if (Config.EnableBucketDebug || Constants.EnableTemporaryBucketDebug)
        {
          Log.Info("Using: " + indexName, this);
          Log.Info("Bucket Facet Original Debug Query: " + query, this);
        }

        foreach (string terms in termValue)
        {
          QueryFilter genreQueryFilter = this.GenreQueryFilter(query, faceted, lookupId, termName, terms);

          if (Config.EnableBucketDebug || Constants.EnableTemporaryBucketDebug)
          {
            Log.Info("Bucket Facet Debug Query: " + genreQueryFilter, this);
          }

          PreparedQuery preparedQuery = context.PrepareQueryForFacetsUse(query);
          Hits searchHits = context.Searcher.Search(preparedQuery.Query, genreQueryFilter);
          int numberOfResults = searchHits.Length();
          BitArray genreBitArray = genreQueryFilter.Bits(context.Searcher.GetIndexReader());

          var tempSearchArray = queryBase.Clone() as BitArray;
          if (tempSearchArray.Length == genreBitArray.Length)
          {
            BitArray combinedResults = tempSearchArray.And(genreBitArray);

            int cardinality = SearchHelper.GetCardinality(combinedResults);

            if (cardinality > 0)
            {
              if (!faceted)
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
                  runningCOunt.Add(terms, numberOfResults);
                }
              }
            }
          }
        }
      }

      return runningCOunt;
    }

    /// <summary>
    /// Genres the query filter.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="faceted">if set to <c>true</c> [is facet].</param>
    /// <param name="lookupId">if set to <c>true</c> [is id lookup].</param>
    /// <param name="termName">Name of the term.</param>
    /// <param name="terms">The terms.</param>
    /// <returns>The query filter.</returns>
    public virtual QueryFilter GenreQueryFilter(
      Query query, bool faceted, bool lookupId, string termName, string terms)
    {
      Guid newGuid;
      bool validGuid = Guid.TryParse(terms, out newGuid);

      string tempTerms = terms;
      if (validGuid)
      {
        tempTerms = IdHelper.NormalizeGuid(terms, true);
      }

      QueryFilter genreQueryFilter;
      if (!faceted)
      {
        if (termName == "_language" || lookupId)
        {
          string termValueParse = terms.Split('|')[0].ToLowerInvariant();
          if (lookupId)
          {
            termValueParse = IdHelper.NormalizeGuid(termValueParse, true);
          }

          genreQueryFilter = new QueryFilter(new TermQuery(new Term(termName.ToLowerInvariant(), termValueParse)));
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
              new TermQuery(new Term(terms.Split('|')[0].ToLowerInvariant(), termName.ToLowerInvariant())));
        }
      }
      else
      {
        if (termName == "__created by")
        {
          genreQueryFilter = new QueryFilter(new TermQuery(new Term(termName, tempTerms)));
        }
        else
        {
          if (Config.ExcludeContextItemFromResult)
          {
            if (termName == "_path")
            {
              var term = new BooleanQuery();
              term.Add(new TermQuery(new Term(termName, tempTerms.ToLowerInvariant())), BooleanClause.Occur.MUST);
              term.Add(
                new TermQuery(new Term(BuiltinFields.ID, tempTerms.ToLowerInvariant())), BooleanClause.Occur.MUST_NOT);
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
        string dateStart = terms.Split('|')[0];
        string typeOfDate = terms.Split('|')[1];
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
        SearcherMethods.AddDateRangeQuery(
          boolQuery,
          new DateRangeSearchParam.DateRangeField(termName, DateTime.Parse(dateStart), dateEnd)
            {
              InclusiveEnd = true,
              InclusiveStart = true
            },
          BooleanClause.Occur.MUST);
        genreQueryFilter = new QueryFilter(boolQuery);
        if (Config.EnableBucketDebug || Constants.EnableTemporaryBucketDebug)
        {
          Log.Info("Search Clauses Number: " + boolQuery.Clauses().Count, this);
        }
      }

      return genreQueryFilter;
    }

    /// <summary>
    /// Dispose the Index Searcher
    /// </summary>
    public virtual void Dispose()
    {
    }

    /// <summary>
    /// Runs the query.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <returns>The key value pair of the running query.</returns>
    internal virtual KeyValuePair<int, List<SitecoreItem>> RunQuery(QueryBase query)
    {
      var translator = new QueryTranslator(Index);
      Query luceneQuery = translator.Translate(query);
      return this.RunQuery(luceneQuery, 20, 0);
    }

    /// <summary>
    /// Runs the query.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="numberOfResults">The number of results.</param>
    /// <returns>The key value pair of the running query.</returns>
    internal virtual KeyValuePair<int, List<SitecoreItem>> RunQuery(QueryBase query, int numberOfResults)
    {
      var translator = new QueryTranslator(Index);
      Query luceneQuery = translator.Translate(query);
      return this.RunQuery(luceneQuery, numberOfResults, 0);
    }

    /// <summary>
    /// Runs the query.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="numberOfResults">The number of results.</param>
    /// <param name="pageNumber">The page number.</param>
    /// <param name="sortField">The sort field.</param>
    /// <param name="sortDirection">The sort direction.</param>
    /// <returns>The key value pair of the running query.</returns>
    internal virtual KeyValuePair<int, List<SitecoreItem>> RunQuery(
      QueryBase query, int numberOfResults, int pageNumber, string sortField, string sortDirection)
    {
      var translator = new QueryTranslator(Index);
      Query luceneQuery = translator.Translate(query);
      return this.RunQuery(luceneQuery, numberOfResults, pageNumber, sortField, sortDirection);
    }

    /// <summary>
    /// Gets the items via term query.
    /// </summary>
    /// <param name="fieldName">Name of the field.</param>
    /// <param name="fieldValue">The field value.</param>
    /// <returns>The key value pair of the query.</returns>
    internal virtual KeyValuePair<int, List<SitecoreItem>> GetItemsViaTermQuery(string fieldName, string fieldValue)
    {
      var query = new TermQuery(new Term(fieldName, fieldValue));
      return this.RunQuery(query, 20, 0);
    }

    /// <summary>
    /// Gets the items via field query.
    /// </summary>
    /// <param name="fieldName">Name of the field.</param>
    /// <param name="fieldValue">The field value.</param>
    /// <returns>The key value pair of the query.</returns>
    internal virtual KeyValuePair<int, List<SitecoreItem>> GetItemsViaFieldQuery(string fieldName, string fieldValue)
    {
      var query = new FieldQuery(fieldName, fieldValue);
      return this.RunQuery(query);
    }

    /// <summary>
    /// Gets the items via field query.
    /// </summary>
    /// <param name="fieldName">Name of the field.</param>
    /// <param name="fieldValue">The field value.</param>
    /// <param name="numberOfResults">The number of results.</param>
    /// <returns>The key value pair of the query.</returns>
    internal virtual KeyValuePair<int, List<SitecoreItem>> GetItemsViaFieldQuery(
      string fieldName, string fieldValue, int numberOfResults)
    {
      var query = new FieldQuery(fieldName, fieldValue);
      return this.RunQuery(query, numberOfResults);
    }

    /// <summary>
    /// Gets the items.
    /// </summary>
    /// <param name="param">The parameter.</param>
    /// <returns>The key value pair of the running query.</returns>
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

      return this.RunQuery(globalQuery, param.PageSize, param.PageNumber, param.SortByField, param.SortDirection);
    }

    /// <summary>
    /// Gets the items.
    /// </summary>
    /// <param name="param">The parameter.</param>
    /// <returns>The key value pair of the running query.</returns>
    internal virtual KeyValuePair<int, List<SitecoreItem>> GetItems(DateRangeSearchParam param)
    {
      Database db = Context.ContentDatabase ?? Context.Database;
      if (db != null)
      {
        string indexName = this.searcherIndexName;
        if (string.IsNullOrEmpty(this.searcherIndexName))
        {
          Item item = db.GetItem(param.LocationIds);
          indexName = BucketManager.GetContextIndex(item.IsNotNull() ? item : db.GetItem(ItemIDs.RootID));
        }

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
          Index = SearchManager.GetIndex(indexName);
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
                SearcherMethods.ApplyFullTextClause(globalQuery, param.FullTextQuery);
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

          // Hack!!!!!
          if (!param.Refinements.ContainsKey("__workflow state"))
          {
            SearcherMethods.ApplyRefinements(globalQuery, param.Refinements, param.Occurance);
          }

          SearcherMethods.ApplyLatestVersion(globalQuery);

          if (Config.ExcludeContextItemFromResult)
          {
            SearcherMethods.ApplyContextItemRemoval(globalQuery, param.LocationIds);
          }

          SearcherMethods.ApplyNameFilter(globalQuery, param.ItemName);
          var translator = new QueryTranslator(Index);
          BooleanQuery booleanQuery = translator.ConvertCombinedQuery(globalQuery);
          BooleanClause.Occur innerOccurance = translator.GetOccur(param.Occurance);

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
            SearcherMethods.AddFieldValueClause(
              booleanQuery, "__workflow state", param.Refinements["__workflow state"], QueryOccurance.Should);
          }

          if (Config.EnableBucketDebug || Constants.EnableTemporaryBucketDebug)
          {
            Log.Info("Search Clauses Number: " + booleanQuery.Clauses().Count, this);
          }

          if (!param.SortByField.IsNullOrEmpty())
          {
            return this.RunQuery(booleanQuery, param.PageSize, param.PageNumber, param.SortByField, param.SortDirection);
          }

          return param.PageNumber != 0
                   ? this.RunQuery(booleanQuery, param.PageSize, param.PageNumber)
                   : this.RunQuery(booleanQuery, 20, 0);
        }
      }

      return new KeyValuePair<int, List<SitecoreItem>>();
    }

    /// <summary>
    /// Gets the items.
    /// </summary>
    /// <param name="param">The parameter.</param>
    /// <returns>The key value pair of the items.</returns>
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

    /// <summary>
    /// Determines whether [contains items by fields] [the specified ids].
    /// </summary>
    /// <param name="ids">The ids.</param>
    /// <param name="fieldName">Name of the field.</param>
    /// <param name="fieldValue">The field value.</param>
    /// <returns>
    ///   <c>true</c> if [contains items by fields] [the specified ids]; otherwise, <c>false</c>.
    /// </returns>
    internal virtual bool ContainsItemsByFields(string ids, string fieldName, string fieldValue)
    {
      var globalQuery = new CombinedQuery();
      SearcherMethods.ApplyIdFilter(globalQuery, BuiltinFields.ID, ids);
      SearcherMethods.AddFieldValueClause(globalQuery, fieldName, fieldValue, QueryOccurance.Must);
      return this.RunQuery(globalQuery).Value.Any();
    }

    /// <summary>
    /// Gets the value.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="items">The items.</param>
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
  }
}