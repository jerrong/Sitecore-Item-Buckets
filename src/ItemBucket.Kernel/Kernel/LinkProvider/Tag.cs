// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Tag.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the Tag type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------


namespace Sitecore.ItemBucket.Kernel.Common.Providers
{
    using Sitecore.ItemBucket.Kernel.Kernel.Interfaces;

    public class Tag : ITag
    {
        public Tag(string displayText, string value)
        {
            DisplayText = displayText;
            DisplayValue = value;
        }

        public string DisplayText { get; set; }
        public string DisplayValue { get; set; }
    }
}
