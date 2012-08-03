// -----------------------------------------------------------------------
// <copyright file="InMemoryIndex.cs" company="Microsoft">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Sitecore.ItemBuckets.BigData.RamDirectory
{
    using System.Collections.Generic;

    using Lucene.Net.Analysis;
    using Lucene.Net.Index;
    using Lucene.Net.Search;
    using Lucene.Net.Store;

    using Sitecore.Configuration;
    using Sitecore.Diagnostics;
    using Sitecore.IO;
    using Sitecore.Search;


    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class InMemoryIndex : ILuceneIndex
    {
        // Fields
        private Analyzer _analyzer;
        private readonly List<IRamCrawler> _crawlers = new List<IRamCrawler>();
        private readonly RAMDirectory _directory;
        private readonly string _folder;
        private readonly string _name;

        // Methods
        public InMemoryIndex(string name, string folder)
        {
            this._name = name;
            this._folder = folder;
            this._directory = this.CreateDirectory(FileUtil.MapPath(FileUtil.MakePath(Settings.IndexFolder, folder)));
        }

        public void AddCrawler(IRamCrawler crawler)
        {
            crawler.Initialize(this);
            this._crawlers.Add(crawler);
        }

        public IndexDeleteContext CreateDeleteContext()
        {
            return new IndexDeleteContext(this);
        }

        protected virtual RAMDirectory CreateDirectory(string folder)
        {
            FileUtil.EnsureFolder(folder);
            RAMDirectory directory = new RAMDirectory(folder);
            using (new IndexLocker(directory.MakeLock("write.lock")))
            {
                if (!IndexReader.IndexExists(directory))
                {
                    new IndexWriter(directory, this._analyzer, true).Close();
                }
            }
            return directory;
        }

        protected virtual IndexReader CreateReader()
        {
            return Assert.ResultNotNull<IndexReader>(IndexReader.Open(this._directory), "Could not create reader for index " + this.Name);
        }

        public IndexSearchContext CreateSearchContext()
        {
            return new IndexSearchContext(this);
        }

        protected virtual IndexSearcher CreateSearcher(bool close)
        {
            if (close)
            {
                return new IndexSearcher(this.Directory);
            }
            return new IndexSearcher(this.CreateReader());
        }

        public IndexUpdateContext CreateUpdateContext()
        {
            return new IndexUpdateContext(this);
        }

        protected virtual IndexWriter CreateWriter(bool recreate)
        {
            using (new IndexLocker(this._directory.MakeLock("write.lock")))
            {
                recreate |= !IndexReader.IndexExists(this._directory);
                return new IndexWriter(this._directory, this._analyzer, recreate);
            }
        }

        public int GetDocumentCount()
        {
            int num;
            IndexSearcher searcher = this.CreateSearcher(true);
            try
            {
                num = searcher.Reader.NumDocs();
            }
            finally
            {
                searcher.Close();
            }
            return num;
        }

        public void Rebuild()
        {
            using (IndexUpdateContext context = this.CreateUpdateContext())
            {
                foreach (IRamCrawler crawler in this._crawlers)
                {
                    crawler.Add(context);
                }
                context.Optimize();
                context.Commit();
            }
        }

        private void Reset()
        {
            this.CreateWriter(true).Close();
        }

        IndexSearcher ILuceneIndex.CreateSearcher(bool close)
        {
            return this.CreateSearcher(close);
        }

        IndexWriter ILuceneIndex.CreateWriter(bool recreate)
        {
            return this.CreateWriter(recreate);
        }

        // Properties
        public Analyzer Analyzer
        {
            get
            {
                return this._analyzer;
            }
            set
            {
                this._analyzer = value;
            }
        }

        public Directory Directory
        {
            get
            {
                return this._directory;
            }
        }

        public string Name
        {
            get
            {
                return this._name;
            }
        }
    }
}
