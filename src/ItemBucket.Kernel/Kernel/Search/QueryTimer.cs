using System;
using System.Diagnostics;

namespace Sitecore.ItemBucket.Kernel.Kernel.Search
{
    internal class QueryTimer : IDisposable
    {
        private Stopwatch queryTimer;

        public QueryTimer()
        {
            queryTimer = new Stopwatch();
            queryTimer.Start();
        }

        public void Dispose()
        {
            queryTimer.Stop();
        }
    }
}
