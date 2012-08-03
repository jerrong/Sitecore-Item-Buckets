namespace Sitecore.ItemBucket.Kernel.Kernel.Search
{
    public interface IDataContextFactory
    {
        System.Data.Linq.DataContext Context { get; }
        void SaveAll();
    }
}
