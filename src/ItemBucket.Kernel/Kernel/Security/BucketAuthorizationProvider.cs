namespace Sitecore.ItemBucket.Kernel.Security
{
    using Sitecore.Security.AccessControl;
    using Sitecore.Security.Accounts;

    internal class BucketAuthorizationProvider : SqlServerAuthorizationProvider 
    {
        public BucketAuthorizationProvider() 
        { 
            this._itemHelper = new AuthHelper(); 
        } 
  
        private ItemAuthorizationHelper _itemHelper; 
        protected override ItemAuthorizationHelper ItemHelper 
        { 
            get
            {
                return this._itemHelper;
            } 

            set
            {
                this._itemHelper = value;
            } 
        } 
          
        protected override void AddAccessResultToCache(ISecurable entity, Account account, AccessRight accessRight, AccessResult accessResult, PropagationType propagationType) 
        { 
            if (accessRight.Name == BucketRights.MakeABucket) 
            { 
                return; 
            } 

            base.AddAccessResultToCache(entity, account, accessRight, accessResult, propagationType); 
        } 
    } 
}
