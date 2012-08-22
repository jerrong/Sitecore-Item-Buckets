// -----------------------------------------------------------------------
// <copyright file="SearchConfiguration.cs" company="Sitecore">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using Sitecore.Search;

namespace Sitecore.ItemBuckets.BigData.RamDirectory
{
    using System.Collections.Generic;

    using Sitecore.Diagnostics;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class RemoteIndexSearchConfiguration
    {
        // Fields
        private Dictionary<string, ILuceneIndex> _indexes = new Dictionary<string, ILuceneIndex>();

        // Methods
        public virtual void AddIndex(Sitecore.ItemBuckets.BigData.RemoteIndex.RemoteIndex index)
        {
            Assert.ArgumentNotNull(index, "index");
            this._indexes[index.Name] = index;
        }

        // Properties
        public Dictionary<string, ILuceneIndex> Indexes
        {
            get
            {
                return this._indexes;
            }
            set { _indexes = value; }
        }
    }
}
