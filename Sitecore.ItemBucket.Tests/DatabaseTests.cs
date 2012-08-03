using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Sitecore.Data;
using Sitecore.Data.Items;

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
                Assert.IsNotNull(item);
                Assert.AreEqual("content", item.Name);
            }
        }
    }
}
