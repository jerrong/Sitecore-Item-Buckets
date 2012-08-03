namespace Sitecore.ItemBucket.Kernel.Publishing
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;

    using Sitecore.Diagnostics;
    using Sitecore.Globalization;
    using Sitecore.ItemBucket.Kernel.Kernel.Util;
    using Sitecore.Publishing;
    using Sitecore.Publishing.Pipelines.Publish;
    using Sitecore.Publishing.Pipelines.PublishItem;

    public class ProcessQueue : PublishProcessor
    {
        private static PublishItemContext CreateItemContext(PublishingCandidate entry, PublishContext context)
        {
            Assert.ArgumentNotNull(entry, "entry");
            var context2 = PublishManager.CreatePublishItemContext(entry.ItemId, entry.PublishOptions);
            context2.Job = context.Job;
            context2.User = context.User;
            context2.PublishContext = context;
            return context2;
        }

        public override void Process(PublishContext context)
        {
            var watch = new Stopwatch();
            Log.Info("publish process started", this);
            watch.Start();
            Assert.ArgumentNotNull(context, "context");

            foreach (var enumerable in context.Queue)
            {
                this.ProcessEntries(enumerable, context, 0);
            }

            UpdateJobStatus(context);
            watch.Stop();
            Log.Info("publish process completed elapsed: " + watch.Elapsed, this);
        }

        protected virtual void ProcessEntries(IEnumerable<PublishingCandidate> entries, PublishContext context, int depth)
        {
            var level = depth - 5;
            if (level < 1)
            {
                level = 1;
            }

            Parallel.ForEach(entries, new ParallelOptions { MaxDegreeOfParallelism = level }, candidate => this.ProcessCandidate(candidate, context, depth));
        }

        private void ProcessCandidate(PublishingCandidate candidate, PublishContext context, int depth)
        {
            var result = PublishItemPipeline.Run(CreateItemContext(candidate, context));
            if (!this.SkipReferrers(result, context))
            {
                this.ProcessEntries(result.ReferredItems, context, depth + 1);
            }

            if (!SkipChildren(result, candidate))
            {
                this.ProcessEntries(candidate.ChildEntries, context, depth + 1);
            }
        }

        private static bool SkipChildren(PublishItemResult result, PublishingCandidate entry)
        {
            if (result.ChildAction == PublishChildAction.Skip)
            {
                return true;
            }

            if (result.ChildAction != PublishChildAction.Allow)
            {
                return false;
            }

            if ((entry.PublishOptions.Mode != PublishMode.SingleItem) && (result.Operation == PublishOperation.Created))
            {
                return false;
            }

            return !entry.PublishOptions.Deep;
        }

        protected virtual bool SkipReferrers(PublishItemResult result, PublishContext context)
        {
            return result.ReferredItems.Count == 0;
        }

        private static void UpdateJobStatus(PublishContext context)
        {
            var job = context.Job;
            if (job.IsNotNull())
            {
                job.Status.Messages.Add(string.Format("{0}{1}", Translate.Text("Items created: "), context.Statistics.Created));
                job.Status.Messages.Add(string.Format("{0}{1}", Translate.Text("Items deleted: "), context.Statistics.Deleted));
                job.Status.Messages.Add(string.Format("{0}{1}", Translate.Text("Items updated: "), context.Statistics.Updated));
                job.Status.Messages.Add(string.Format("{0}{1}", Translate.Text("Items skipped: "), context.Statistics.Skipped));
            }
        }
    }
}
