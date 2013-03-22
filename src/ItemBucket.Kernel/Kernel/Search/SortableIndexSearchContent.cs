using Sitecore.ItemBucket.Kernel.Kernel.Util;

namespace Sitecore.ItemBucket.Kernel.Kernel.Search
{
    using System;

    using Lucene.Net.Search;

    using Sitecore.Search;

    public class SortableIndexSearchContext : IndexSearchContext, IDisposable
    {
        public SortableIndexSearchContext(ILuceneIndex index)
        {
            if (index.IsNotNull())
            Initialize(index, true);
        }
        public PreparedQuery PrepareQueryForFacetsUse(Query query){
            return base.Prepare(query, SearchContext.Empty);
        }
        public SortableIndexSearchContext(ILuceneIndex index, bool autoWarm) : this (index)
        {
           //TODO: Implement AutoWarm
        }
        public SearchHits Search(Query query, Sort sort)
        {
            //Fix for #384289. Previous code line returned PreparedQuery. PreparedQuery made FullText queries from all queries,
            //for example _name:test query became _name:test*. It produced wrong search results, so I decided to remove
            //PreparedQuery part. If something wrong noticed during usage, PreparedQuery should be return
            //and other logic should be created for this part.            
            //return Search(query, SearchContext.Empty, sort); - this returned PreparedQuery.
            return new SearchHits(Searcher.Search(query, sort));
        }

        public SearchHits NoPreparedSearch(Query query)
        {
            return new SearchHits(Searcher.Search(query));
        }

        public SearchHits Search(PreparedQuery query, Sort sort)
        {
            return new SearchHits(Searcher.Search(query.Query, sort));
        }

        public SearchHits Search(QueryBase query, Sort sort)
        {
            return Search(query, SearchContext.Empty, sort);
        }

        public SearchHits Search(string query, Sort sort)
        {
            return Search(query, SearchContext.Empty, sort);
        }

        public SearchHits Search(Query query, ISearchContext context, Sort sort)
        {
            return Search(Prepare(query, context), sort);
        }

        public SearchHits Search(QueryBase query, ISearchContext context, Sort sort)
        {
            return this.Search(Prepare(Translate(query), context), sort);
        }

        public SearchHits Search(string query, ISearchContext context, Sort sort)
        {
            return this.Search(Parse(query, context), sort);
        }

        void IDisposable.Dispose()
        {
            base.Dispose();
        }
    }
}
