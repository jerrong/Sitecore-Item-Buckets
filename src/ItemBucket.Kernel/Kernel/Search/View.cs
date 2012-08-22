// -----------------------------------------------------------------------
// <copyright file="View.cs" company="Microsoft">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Sitecore.ItemBucket.Kernel.Kernel.Search
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    [Serializable]
    public class View
    {
        public string ViewName;

        public string HeaderTemplate;

        public string ItemTemplate;

        public string FooterTemplate;

        public string Icon;

        public bool IsDefault;

        public override string ToString()
        {
            return this.ViewName;
        }
    }
}
