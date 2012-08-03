namespace Sitecore.ItemBucket.Kernel.Pipelines
{
    using System.Linq;

    using Sitecore.Diagnostics;
    using Sitecore.ItemBucket.Kernel.ItemExtensions.Axes;
    using Sitecore.ItemBucket.Kernel.Kernel.Util;
    using Sitecore.Pipelines.HttpRequest;

    public class ItemSearchResolver : HttpRequestProcessor
    {
        private static readonly string[] Sites = { "shell", "login", "admin", "service", "modules_shell", "modules_website", "scheduler", "system", "publisher" };

        public override void Process(HttpRequestArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            if (Context.Item.IsNotNull() || Context.Database.IsNull() || args.Url.ItemPath.Length == 0 || Sites.Contains(Context.Site.Name))
            {
                return;
            }

            var urlAnatomy = args.Url.FilePathWithQueryString.Split(new[] { '/' });
            if (urlAnatomy[urlAnatomy.Length - 2].Length == 4)
            {
                var item = Context.Database.GetItem(string.Format("{0}/{1}/{2}", Context.Site.StartPath, urlAnatomy[urlAnatomy.Length - 4], urlAnatomy[urlAnatomy.Length - 3].Replace("-", "/")));
                if (item.IsNotNull())
                {
                    int hitsCount;
                    var enumerable = item.Search(out hitsCount, id: string.Format("{{{0}", urlAnatomy[urlAnatomy.Length - 2].ToUpper()));
                    // TODO: Search should be made to yield its results. That will make this perform much better.
                    if (enumerable.Any())
                    {
                        Context.Item = enumerable.Single().GetItem();
                    }
                }
            }
        }
    }
}
