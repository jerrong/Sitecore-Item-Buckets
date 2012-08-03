namespace ItemBucket.Kernel.ItemExtensions.Axes
{
    using System.Linq;

    using Sitecore;
    using Sitecore.Collections;
    using Sitecore.Data;
    using Sitecore.Data.Items;
    using Sitecore.Diagnostics;
    using Sitecore.ItemBucket.Kernel.ItemExtensions.Axes;
    using Sitecore.ItemBucket.Kernel.Kernel.Util;

    using Constants = Sitecore.ItemBucket.Kernel.Util.Constants;

    /// <summary>
    /// To deal with large amounts of content, the content is now unstructured however it is important to still be able to do relationa;
    /// queries on items. This is an override for the Axes Method on an Item to use the Lucene Index Instead.
    /// </summary>
    public class BucketItemAxes
    {
        private Item _item;

        /// <summary>
        /// Returns the default Bucket Index
        /// </summary>
        protected string IndexName
        {
            get { return Constants.Index.Name; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BucketItemAxes"/> class.
        /// </summary>
        /// <param name="item">
        /// The item.
        /// </param>
        public BucketItemAxes(Item item)
        {
            this._item = item;
        }

        protected string GetDisplayNameById(ID id)
        {
            return Context.ContentDatabase.GetItem(id.ToString()).DisplayName;
        }

        /// <summary>
        /// Looks up a Child Element of a Parent by ID
        /// </summary>
        /// <param name="childId">
        /// The ID of the child item
        /// </param>
        public Item GetChild(ID childId)
        {
            var refinement = new SafeDictionary<string> { { "_id", childId.Guid.ToString() } };
            int hitsCount;
            var result = this._item.Search(refinement, out hitsCount, location: this._item.ID.Guid.ToString());
            return result.IsNotNull() ? result.First().GetItem() : null;
        }

        /// <summary>
        /// Looks up a Child Element of a Parent by ID
        /// </summary>
        /// <param name="itemName">
        /// The item name to search for
        /// </param>
        public Item GetChild(string itemName)
        {
            var refinement = new SafeDictionary<string> { { "_name", itemName } };
            int hitsCount;
            var result = this._item.Search(refinement, out hitsCount, location: this._item.ID.Guid.ToString());
            return result.IsNotNull() ? result.First().GetItem() : null;
        }

        /// <summary>
        /// Looks up a Descendant Item of a Parent by name
        /// </summary>
        /// <param name="name">
        /// The item name to search for
        /// </param>
        public Item GetDescendant(string name)
        {
            var refinement = new SafeDictionary<string> { { "_name", name } };
            int hitsCount;
            var result = this._item.Search(refinement, out hitsCount, location: this._item.ID.Guid.ToString());
            return result.IsNotNull() ? result.First().GetItem() : null;
        }

        /// <summary>
        /// Checks if a particular item is a descendant of another item
        /// </summary>
        /// <param name="item">
        /// The possible Ancestor
        /// </param>
        public bool IsDescendantOf(Item item)
        {
            Assert.IsNotNull(item, "item");
            return item.Database.Name.Equals(this._item.Database.Name) && this._item.Paths.LongID.StartsWith(item.Paths.LongID);
        }

        /// <summary>
        /// Gets Level.
        /// </summary>
        public int Level
        {
            get
            {
                if (this._item.ID == this.Root.ID)
                {
                    return 0;
                }

                return this._item.Parent.Axes.Level + 1;
            }
        }

        /// <summary>
        /// Gets Root.
        /// </summary>
        public Item Root
        {
            get
            {
                return this._item.Database.GetRootItem(this._item.Language);
            }
        }
    }
}


