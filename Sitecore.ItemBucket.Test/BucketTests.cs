using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.ItemBucket.Test.Factory;

namespace Sitecore.ItemBucket.Test
{
    namespace Sitecore.ItemBucket.Test
    {
        [TestClass]
        public class BucketTests
        {
            [TestMethod]
            public void CreateItemWithShowInMenuFalseShouldReturnEmptyString()
            {
                FieldList stubFieldList = new FieldList();
                Item stub = new ContentItem(stubFieldList);
                string navigationTitle = ResultFactory.Create(stub);
                Assert.IsNotNull(navigationTitle);
            }

            [TestMethod]
            public void CreateItemWithShowInMenuTrueNoNavigationTitleShouldReturnItemName()
            {
                FieldList stubFieldList = new FieldList();
                Item stub = new ContentItem(stubFieldList, "myItemName");
                string navigationTitle = ResultFactory.Create(stub);
                Assert.AreEqual("myItemName", navigationTitle);
            }

            [TestMethod]
            public void CreateItemWithShowInMenuTrueShouldReturnItemNavigationTitle()
            {
                FieldList stubFieldList = new FieldList();
                Item stub = new ContentItem(stubFieldList);
                string navigationTitle = ResultFactory.Create(stub);
                Assert.AreEqual("NavigationTitle", navigationTitle);
            }
        }
    }
}
