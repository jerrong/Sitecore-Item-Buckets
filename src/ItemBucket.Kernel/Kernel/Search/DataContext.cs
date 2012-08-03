namespace Sitecore.ItemBucket.Kernel.Kernel.Search
{
    public class DataContext : IDataContextFactory
    {
        #region IDataContextFactory Members
        private System.Data.Linq.DataContext dt;

        public System.Data.Linq.DataContext Context
        {
            get
            {
                if (this.dt == null)
                {
                    this.dt = new System.Data.Linq.DataContext(string.Empty);
                }

                return this.dt;
            }
        }

        public void SaveAll()
        {
            this.dt.SubmitChanges();
        }

        #endregion
    }
}
