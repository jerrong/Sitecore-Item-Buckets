using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.ItemBucket.Kernel.Kernel.Managers;
using Sitecore.ItemBucket.Kernel.Managers;
using Sitecore.SecurityModel;

namespace Sitecore.ItemBucket.Tests
{
    public class DatabaseTests
    {
        [TestFixture]
        public class SetupTestFixture
        {
            [Test]
            public void DatabaseGet()
            {
                Database database = global::Sitecore.Configuration.Factory.GetDatabase("master");
                Assert.IsNotNull(database);
            }
            [Test]
            public void GetItem()
            {
                Database database = global::Sitecore.Configuration.Factory.GetDatabase("master");
                Assert.IsNotNull(database);

                Item item = database.GetItem("/sitecore/content");
                using (new BucketImportContext(item))
                {
                    //Disable History Engine
                    //Disable Publishing Queue
                    //Smart Links Database Rebuild

                    BucketManager.CreateBucket(item, (itm => BucketManager.AddSearchTabToItem(item)));
                }
                Assert.IsNotNull(item);
                Assert.AreEqual("content", item.Name);
            }

            
        }
    }
}
