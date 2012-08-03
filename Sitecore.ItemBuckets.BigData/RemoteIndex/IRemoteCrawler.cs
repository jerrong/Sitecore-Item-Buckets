using Sitecore.Search;

namespace Sitecore.ItemBuckets.BigData.RemoteIndex
{
    public interface IRemoteCrawler
    {
        // Methods
        void Add(IndexUpdateContext context);
        void Initialize(RemoteIndex index);
    }
}
