namespace Sitecore.ItemBucket.Kernel.Security
{
    using Sitecore.ItemBucket.Kernel.Kernel.Util;
    using Sitecore.Security.AccessControl;

    class BucketSecurityManager
    {
        public BucketSecurityManager(Data.Items.Item item)
        {
            this.ContextItem = item;
        }

        public Data.Items.Item ContextItem
        {
            get; private set;
        }
 
        public virtual bool IsAllowedToCreateBucket
        {
            get
            {
                var right = AccessRight.FromName(BucketRights.MakeABucket);
                if (right.IsNull())
                {
                    return false;
                }

                var allowed = AuthorizationManager.IsAllowed(this.ContextItem, right, Context.User);
                return allowed;
            }
        }

       public virtual Data.Items.Item UnMakeBucket()
        {
            if (!this.IsUnMakeBucketAllowed)
            {
                return null;
            }

            return null;
        }

        public virtual bool IsUnMakeBucketAllowed
        {
            get
            {
                var right = AccessRight.FromName(BucketRights.UnmakeBucket);
                if (right.IsNull())
                {
                    return false;
                }

                var allowed = AuthorizationManager.IsAllowed(this.ContextItem, right, Context.User);
                return allowed;
            }
        }
    }
}




