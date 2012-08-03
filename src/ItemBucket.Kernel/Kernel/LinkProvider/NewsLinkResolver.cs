using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.ItemBucket.Kernel.Kernel.Util;
using Sitecore.Pipelines.HttpRequest;
using Sitecore.Sites;
using Sitecore.ItemBucket.Kernel.Kernel.Search;

namespace Sitecore.ItemBucket.Kernel.Common.Providers
{
    public class NewsLinkResolver : HttpRequestProcessor
    {
        /// <summary>
        /// Link Resolver for use with Bucketed Items
        /// </summary>
        /// <example>
        ///  This will resolve the URL http://sitename/Products/Item Name-20121213081212 to /sitecore/content/Home/Products/2012/12/13/08/12/12/Item Name
        /// </example>
        public override void Process(HttpRequestArgs args)
        {
            if (Context.Item == null)
            {
                try
                {
                    var contentDatabase = Context.Database;
                    if (contentDatabase == null || Context.Site == null)
                    {
                        return;
                    }

                    var datedLinkResolver = new DatedLinkResolver(contentDatabase, Context.Site, new TagRepository(new DataContext()));
                    Context.Item = datedLinkResolver.Process(args.Url.FilePath);
                }
                catch (Exception e)
                {
                    Diagnostics.Log.Error(string.Format("Error resolving url {0}", args.Url), e, this);
                }
            }
        }

        public class DatedLinkResolver
        {
            private readonly Database database;

            private readonly SiteContext site;

            private readonly ITagRepository tagReadonlyRepository;

            public DatedLinkResolver(Database database, SiteContext site, ITagRepository tagReadonlyRepository)
            {
                this.database = database;
                this.site = site;
                this.tagReadonlyRepository = tagReadonlyRepository;
            }

            public Item Process(string url)
            {
                var dateAndArticleNameSegments = ParseFinalSegment(url).ToList();
                if (!dateAndArticleNameSegments.Any())
                {
                    return null;
                }

                var pathSegments = dateAndArticleNameSegments.First().Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
                var rewrittenDateSegments = dateAndArticleNameSegments.Skip(1).ToList();

                var itemToReturn = Sitecore.Context.Site.StartPath;

                if (pathSegments.Any())
                {
                    if (pathSegments.First() == "sitecore")
                    {
                        itemToReturn = "/" + string.Join("/", pathSegments);
                    }
                    else
                    {
                        itemToReturn = "/sitecore/content/" + string.Join("/", pathSegments);
                    }
                }

                foreach (var segment in rewrittenDateSegments)
                {
                    itemToReturn = itemToReturn + "/" + segment;
                }

                Item item = Sitecore.Context.Database.GetItem(itemToReturn);
                if (item.IsNotNull())
                {
                    return item;
                }


                itemToReturn = Sitecore.Context.Site.StartPath;

                if (pathSegments.Any())
                {
                    if (pathSegments.First() == "sitecore")
                    {
                        itemToReturn = "/" + string.Join("/", pathSegments);
                    }
                    else
                    {
                        itemToReturn = Sitecore.Context.Site.StartPath + "/" + string.Join("/", pathSegments);
                    }
                }
                item = Sitecore.Context.Database.GetItem(itemToReturn);
                if (item.IsNotNull())
                {
                    return item;
                }

                return Sitecore.Context.Item;
            }

            private static IEnumerable<string> ParseFinalSegment(string segment)
            {
                var r = new Regex(@"(?<start>.*)\/(?<itemname>.*)-(?<datefolders>(?<year>\d{4})(?<month>\d{2})(?<day>\d{2})(?<hour>\d{2})(?<minute>\d{2}))$");

                var matches = r.Matches(segment);
                if (matches.Count > 0)
                {
                    var m = matches[0];
                    yield return m.Groups["start"].Value;
                    yield return m.Groups["year"].Value;
                    yield return m.Groups["month"].Value;
                    yield return m.Groups["day"].Value;
                    yield return m.Groups["hour"].Value;
                    yield return m.Groups["minute"].Value;
                    yield return MainUtil.DecodeName(m.Groups["itemname"].Value);
                }
            }

            private static IEnumerable<Tag> GetAdditionalTags(Item itm)
            {
                return new List<Tag>();
            }

            private static Tag GetPrimaryTag(Item itm, ITagRepository tagRepository)
            {
                return new Tag("Test", "Test");
            }
        }
    }
}