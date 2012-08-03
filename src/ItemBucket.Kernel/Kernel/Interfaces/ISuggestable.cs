using System;
using System.Collections.Generic;


namespace Sitecore.ItemBucket.Kernel.Kernel.Interfaces
{
    public interface ISuggestable
    {
        IEnumerable<string> Fetch();
    }
}
