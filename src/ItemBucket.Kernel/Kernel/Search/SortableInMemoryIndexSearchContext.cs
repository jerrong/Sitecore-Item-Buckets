namespace Sitecore.ItemBucket.Kernel.Kernel.Search
{
    using Lucene.Net.Index;
    using Lucene.Net.Store;

    using Sitecore.Diagnostics;
    using Sitecore.Search;

    public class SortableInMemoryIndexSearchContext : IndexSearchContext
    {
        // Fields
        private LockScope _scope;
        private IndexWriter _writer;

        // Methods
        public SortableInMemoryIndexSearchContext(ILuceneIndex index)
        {
            Assert.ArgumentNotNull(index, "index");
            var idx = new RAMDirectory(index.Directory);
            this._scope = new LockScope(idx);
            this._writer = index.CreateWriter(false);
        }

        public void Dispose()
        {
            this._scope.Dispose();
        }
    }
}