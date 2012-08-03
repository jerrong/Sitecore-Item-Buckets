namespace Sitecore.ItemBucket.Kernel.Util
{
    using System;

    using Lucene.Net.Analysis;
    using Lucene.Net.QueryParsers;
    using Lucene.Net.Search;

    using Sitecore.Diagnostics;
    using Sitecore.ItemBucket.Kernel.Kernel.Util;
    using Sitecore.Search;

    public class QueryTranslator
    {
        private Analyzer _analyzer;

        protected QueryTranslator() { }

        public QueryTranslator(ILuceneIndex index)
        {
            Assert.ArgumentNotNull(index, "index");
            this.Initialize(index, true);
        }

        protected void Initialize(ILuceneIndex index, bool close)
        {
            Assert.ArgumentNotNull(index, "index");
            this._analyzer = index.Analyzer;
            Assert.IsNotNull(this._analyzer, "Failed to request analyzer from the index");
        }

        public virtual Query Translate(QueryBase query)
        {
            var fieldQuery = query as FieldQuery;
            if (fieldQuery.IsNotNull())
            {
                return this.ConvertFieldQuery(fieldQuery);
            }

            var combinedQuery = query as CombinedQuery;
            if (combinedQuery.IsNotNull())
            {
                return this.ConvertCombinedQuery(combinedQuery);
            }

            var fullTextQuery = query as FullTextQuery;
            if (fullTextQuery.IsNull())
            {
                throw new Exception("Unknown query type");
            }

            Assert.IsNotNull(fullTextQuery.Query, "Full text query is empty");
            Assert.IsNotNullOrEmpty(fullTextQuery.Query.Trim(), "Full text query is empty");

            return this.InternalParse(fullTextQuery.Query);
        }

        protected virtual Query ConvertFieldQuery(FieldQuery query)
        {
            try
            {
                return this.InternalParse(query.FieldValue, Escape(query.FieldName));
            }
            catch (Exception exc)
            {
                Log.Info("Tried to convert unescaped field but failed. If this continues, considering change the name of the FieldName for ", exc);
                return this.InternalParse(Escape(query.FieldValue), Escape(query.FieldName));
            }
        }

        public static string Escape(string query)
        {
            return QueryParser.Escape(query);
        }

        public virtual BooleanQuery ConvertCombinedQuery(CombinedQuery query)
        {
            var booleanQuery = new BooleanQuery();
            foreach (var clause in query.Clauses)
            {
                this.TranslateCombinedQuery(clause, booleanQuery);
            }

            return booleanQuery;
        }

        private void TranslateCombinedQuery(QueryClause clause, BooleanQuery booleanQuery)
        {
            var translatedQuery = this.Translate(clause.Query);
            if (translatedQuery.IsNotNull())
            {
                booleanQuery.Add(translatedQuery, this.GetOccur(clause.Occurance));
            }
        }

        public virtual BooleanClause.Occur GetOccur(QueryOccurance occurance)
        {
            switch (occurance)
            {
                case QueryOccurance.Must:
                    return BooleanClause.Occur.MUST;

                case QueryOccurance.MustNot:
                    return BooleanClause.Occur.MUST_NOT;

                case QueryOccurance.Should:
                    return BooleanClause.Occur.SHOULD;
            }

            throw new Exception("Unknown occurance");
        }

        protected Query InternalParse(string query)
        {
            Assert.ArgumentNotNullOrEmpty(query, "query");
            return this.InternalParse(query, BuiltinFields.Content);
        }

        protected virtual Query InternalParse(string query, string defaultField)
        {
            Assert.ArgumentNotNullOrEmpty(query, "query");
            Assert.ArgumentNotNullOrEmpty(defaultField, "defaultField");
            try
            {
                return Assert.ResultNotNull(new QueryParser(defaultField, this._analyzer).Parse(query), "Query parser returned null reference");
            }
            catch (ParseException)
            {
                return Assert.ResultNotNull(new QueryParser(defaultField, this._analyzer).Parse(Escape(query)), "Query parser returned null reference");
            }
        }
    }
}
