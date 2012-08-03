namespace Sitecore.ItemBucket.Kernel.Templates
{
    using System.Collections.Generic;

    using Sitecore.Data.Fields;
    using Sitecore.Data.Items;
    using Sitecore.Data.Templates;
    using Sitecore.Diagnostics;
    using Sitecore.ItemBucket.Kernel.Kernel.Util;
    using Sitecore.ItemBucket.Kernel.Util;

    public static class TemplateExtensions
    {
        public static bool IsBucketTemplateCheck(this Item templateItem)
        {
            if (templateItem.Fields[Constants.BucketableField].IsNotNull())
            {
                return ((CheckboxField)templateItem.Fields[Constants.BucketableField]).Checked;
            }

            Log.Error("Cannot resolve looking up the Is Template Bucketable Checkbox for ", templateItem.Name);
            return false;
        }

        public static bool IsBucketTemplateCheck(this TemplateItem templateItem)
        {
            if (templateItem.InnerItem.Fields[Constants.BucketableField].IsNotNull())
            {
                return ((CheckboxField)templateItem.InnerItem.Fields[Constants.BucketableField]).Checked;
            }

            Log.Error("Cannot resolve looking up the Is Template Bucketable Checkbox for ", templateItem.Name);
            return false;
        }

        public static bool IsBucketTemplate(this Item templateItem)
        {
            return templateItem.Fields[Constants.BucketableField].IsNotNull() && ((CheckboxField)templateItem.Fields[Constants.BucketableField]).Checked;
        }

        public static TemplateField IsBucketTemplateCheck(this Template templateItem)
        {
            return templateItem.GetField(Constants.BucketableField);
        }

        public class TemplateEqulality : IEqualityComparer<SitecoreItem>
        {
            public bool Equals(SitecoreItem x, SitecoreItem y)
            {
                return x.GetItem().TemplateName.Equals(y.GetItem().TemplateName);
            }

            public int GetHashCode(SitecoreItem obj)
            {
                return obj.GetItem().TemplateName.GetHashCode();
            }
        }
    }
}
