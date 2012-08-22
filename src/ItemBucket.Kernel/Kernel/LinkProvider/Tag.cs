// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Tag.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the Tag type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------


using System;

namespace Sitecore.ItemBucket.Kernel.Common.Providers
{
    using Sitecore.ItemBucket.Kernel.Kernel.Interfaces;
    [Serializable]
    public class Tag : ITag
    {
        private Tag()
        {
            
        }
        public Tag(string displayText, string value)
        {
            DisplayText = displayText;
            DisplayValue = value;
        }

        public string DisplayText { get; set; }
        public string DisplayValue { get; set; }
    }
}
