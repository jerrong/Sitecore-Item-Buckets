using System;
using System.Collections.Generic;
using Sitecore.Configuration;
using Sitecore.Data.Managers;
using Sitecore.Install.Framework;
using Sitecore.Search;

namespace Sitecore.ItemBucket.Kernel.Installer
{
    class PostSteps : IPostStep
    {
        void IPostStep.Run(ITaskOutput output, System.Collections.Specialized.NameValueCollection metaData)
        {
            Rebuild();
        }

        protected void Rebuild()
        {
            foreach (var index in GetSearchIndexes())
            {
                StartRebuildJob(index);
            }
        }

        private void StartRebuildJob(KeyValuePair<string, Index> index)
        {
            var options = new Jobs.JobOptions("Post Installation Index Build For Required Bucket Indexes", "Post Installation Indexing", Client.Site.Name, new Builder(index.Key), "Rebuild")
                              { AfterLife = TimeSpan.FromMinutes(1.0) };
            Jobs.JobManager.Start(options);
        }

        private IEnumerable<KeyValuePair<string, Index>> GetSearchIndexes()
        {
            var configuration = Factory.CreateObject("search/configuration", true) as SearchConfiguration;
            if (configuration != null) return configuration.Indexes;
            return new List<KeyValuePair<string, Index>>();
        }

        private class Builder
        {
            private string _indexName;

            public Builder(string indexName)
            {
                _indexName = indexName;
            }

            public void Rebuild()
            {
                var index = SearchManager.GetIndex(_indexName);
                if (index != null)
                {
                    index.Rebuild();
                }
            }
        }
    }
}
