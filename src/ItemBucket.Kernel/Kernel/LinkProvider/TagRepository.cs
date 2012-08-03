using Sitecore.ItemBucket.Kernel.Kernel.Search;
using System;
using System.Collections.Generic;

namespace Sitecore.ItemBucket.Kernel.Common.Providers
{
    public class TagRepository : Repository<Tag>, ITagRepository, IDisposable
    {
        public TagRepository(IDataContextFactory dataContextFactory) : base(dataContextFactory)
        {
        }

        public IEnumerable<Tag> GetListOfTags()
        {
            return new List<Tag>();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="TagRepository"/> class. 
        /// </summary>
        ~TagRepository()
        {
            Dispose(false);
        }

        public IEnumerable<Tag> GetTags(string contains)
        {
            throw new NotImplementedException();
        }

        public Tag GetTagByValue(string value)
        {
            throw new NotImplementedException();
        }
    }
}
