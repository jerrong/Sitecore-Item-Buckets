namespace Sitecore.ItemBucket.Kernel.FieldTypes
{
    using System.Linq;

    using Sitecore.Collections;
    using Sitecore.Data.Items;
    using Sitecore.Diagnostics;
    using Sitecore.ItemBucket.Kernel.ItemExtensions.Axes;
    using Sitecore.ItemBucket.Kernel.Kernel.Util;
    using Sitecore.Pipelines.GetLookupSourceItems;

    /// <summary>
    /// Allows for Lucene Syntax to populate Fields that use an Item[]
    /// </summary>
    internal class LuceneQuery
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
            if (args.Source.StartsWith("lucene:"))
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
        /// Reflect the Class Name in the Source
        /// </summary>
        /// <param name="templateSource">
        /// The template source.
        /// </param>
        /// <returns>
        /// </returns>
        private static Item[] RunEnumeration(string templateSource, Item sourceItem)
        {
            templateSource = templateSource.Replace("lucene:", string.Empty);
            var commands = templateSource.Split(';');
            var refinements = new SafeDictionary<string>();

            foreach (var command in commands)
            {
                if (!command.IsNullOrEmpty())
                {
                    var commandSplit = command.Split(':');
                    if (commandSplit.Length == 2)
                    {
                        refinements.Add(commandSplit[0], commandSplit[1]);
                    }
                }
            }

            if (refinements.ContainsKey("location"))
            {
                int hitsCount;
                var items = Context.ContentDatabase.GetItem(refinements["location"]).Search(refinements, out hitsCount);
                return items.ToList().Select(x => x.GetItem()).ToArray();
            }
            else
            {
                int hitsCount;
                var items = sourceItem.Search(refinements, out hitsCount);
                return items.ToList().Select(x => x.GetItem()).ToArray();
            }
        }
    }
}

