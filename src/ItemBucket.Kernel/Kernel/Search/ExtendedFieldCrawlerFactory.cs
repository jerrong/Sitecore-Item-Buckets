namespace Sitecore.ItemBucket.Kernel.Search
{
    using Sitecore.Collections;
    using Sitecore.Data.Fields;
    using Sitecore.Diagnostics;
    using Sitecore.ItemBucket.Kernel.Kernel.Util;
    using Sitecore.Reflection;
    using Sitecore.Search.Crawlers.FieldCrawlers;

    public class ExtendedFieldCrawlerFactory
    {
        public static string GetFieldCrawlerValue(Field field, SafeDictionary<string, string> fieldCrawlers)
        {
            Assert.IsNotNull(field, "Field was not supplied");
            Assert.IsNotNull(fieldCrawlers, "Field Crawler collection is not specified");

            if (fieldCrawlers.ContainsKey(field.TypeKey))
            {
                var fieldCrawlerType = fieldCrawlers[field.TypeKey];

                if (!fieldCrawlerType.IsNullOrEmpty())
                {
                    var fieldCrawler = ReflectionUtil.CreateObject(fieldCrawlerType, new object[] { field });

                    if (fieldCrawler.IsNotNull() && fieldCrawler is FieldCrawlerBase)
                    {
                        return (fieldCrawler as FieldCrawlerBase).GetValue();
                    }
                }
            }

            return new DefaultFieldCrawler(field).GetValue();
        }
    }
}
