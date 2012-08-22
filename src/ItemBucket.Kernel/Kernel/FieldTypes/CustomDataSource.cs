using System.Linq;
using Sitecore.ItemBucket.Kernel.Kernel.ItemExtensions.Axes;
using Sitecore.ItemBucket.Kernel.Util;

namespace Sitecore.ItemBucket.Kernel.FieldTypes
{
    using System;

    using Sitecore.Data.Items;
    using Sitecore.Diagnostics;
    using Sitecore.ItemBucket.Kernel.Kernel.Util;
    using Sitecore.Pipelines.GetLookupSourceItems;

    /// <summary>
    /// Datasource Interface
    /// </summary>
    public interface IDataSource
    {
        /// <summary>
        /// Item Array to return
        /// </summary>
        /// <param name="itm">
        /// The itm.
        /// </param>
        /// <returns>
        /// Item Array
        /// </returns>
        Item[] ListQuery(Item itm);
    }

    /// <summary>
    /// Test Implementation of IDataSource
    /// </summary>
    public class BucketListQuery : IDataSource
    {
        /// <summary>
        /// List Children
        /// </summary>
        /// <param name="itm">
        /// The itm.
        /// </param>
        /// <returns>
        /// Item Array
        /// </returns>
        public Item[] ListQuery(Item itm)
        {
            return itm.Children.ToArray();
        }
    }

    /// <summary>
    /// Test Implementation of IDataSource
    /// </summary>
    public class LargeBucketListQuery : IDataSource
    {
        /// <summary>
        /// List Children
        /// </summary>
        /// <param name="itm">
        /// The itm.
        /// </param>
        /// <returns>
        /// Item Array
        /// </returns>
        public Item[] ListQuery(Item itm)
        {
            return new BucketQuery().WhereContentContains("Test").Run(itm, 10).Select(sitecoreItem => sitecoreItem.GetItem()).Where(i => i != null).ToArray();
        }
    }

    /// <summary>
    /// Custom DataSource to be able to run Lucene Queries to populate lists in fields
    /// </summary>
    internal class CustomDataSource
    {
        /// <summary>
        /// Process Method
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>
        public void Process(GetLookupSourceItemsArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            if (args.Source.StartsWith("code:"))
            {
                var itemArray = RunEnumeration(args.Source, args.Item);
                if (itemArray.IsNotNull() && (itemArray.Length > 0))
                {
                    args.Result.AddRange(itemArray);
                }

                args.AbortPipeline();
            }
        }

        /// <summary>
        /// Reflect the Possible Datasource
        /// </summary>
        /// <param name="templateSource">
        /// The template source.
        /// </param>
        /// <param name="itm">
        /// The itm.
        /// </param>
        /// <returns>
        /// Item Array
        /// </returns>
        private static Item[] RunEnumeration(string templateSource, Item itm)
        {
            templateSource = templateSource.Replace("code:", string.Empty);
            var classInstance = Activator.CreateInstance(Type.GetType(templateSource)) as IDataSource;
            return classInstance.IsNotNull() ? classInstance.ListQuery(itm) : new Item[] { };
        }
    }
}
