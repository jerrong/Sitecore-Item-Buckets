namespace Sitecore.ItemBucket.Kernel.Security
{
    using System.Collections.Specialized;

    using Sitecore.Security.AccessControl;

    internal class BucketAccessRight : AccessRight
    {
        public BucketAccessRight(string name) : base(name)
        {
        }

        public string BucketFieldName
        {
            get; private set;
        }

        public override void Initialize(NameValueCollection config)
        {
            base.Initialize(config);
            this.BucketFieldName = config["BucketFieldName"];
        }
    }
}
