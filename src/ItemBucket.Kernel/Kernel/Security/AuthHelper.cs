namespace Sitecore.ItemBucket.Kernel.Security
{
    using Sitecore.ItemBucket.Kernel.Kernel.Util;
    using Sitecore.Security.AccessControl;
    using Sitecore.Security.Accounts;

    internal class AuthHelper : ItemAuthorizationHelper
    {
        protected override AccessResult GetItemAccess(Data.Items.Item item, Account account, AccessRight accessRight, PropagationType propagationType)
        {
            var result = base.GetItemAccess(item, account, accessRight, propagationType);
            if (result.IsNull() || result.Permission != AccessPermission.Allow)
            {
                return result;
            }

            if (accessRight.Name != BucketRights.MakeABucket)
            {
                return result;
            }

            var right = accessRight as BucketAccessRight;
            if (right.IsNotNull())
            {
                result = this.GetItemAccess(item, account, right);
            }

            return result;
        }

        protected virtual AccessResult GetItemAccess(Data.Items.Item item, Account account, BucketAccessRight right)
        {
            var ex = new AccessExplanation("This item can be a bucket");
            return new AccessResult(AccessPermission.Allow, ex);
        }
    }
}
