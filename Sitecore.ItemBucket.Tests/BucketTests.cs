using Moq;
using NUnit.Framework;
using Sitecore.Data;

namespace Sitecore.ItemBucket.Tests
{
    class BucketTests
    {
        [TestFixture]
        public class SetupTestFixture
        {
            [Test]
            public void GetBucketItem()
            {
                Database db = Sitecore.Configuration.Factory.GetDatabase("master");
                var mock = new Mock<ItemAdapter>();
                mock.SetupGet(m => m["title"]).Returns("the title");
                var title = GetTitle(mock.Object);
                Assert.AreEqual("the title", title); 
                
            }

            [Test]
            public void IsBucket()
            {
                Database db = Sitecore.Configuration.Factory.GetDatabase("master");
                var mock = new Mock<ItemAdapter>();
                mock.SetupGet(m => m["IsBucket"]).Returns("1");
                var isBucket = GetBucket(mock.Object);
                Assert.AreEqual("1", isBucket);
            }

            public static string GetTitle(ItemAdapter item)
            {
                var fieldTitle = item["title"];

                if (string.IsNullOrEmpty(fieldTitle))
                    fieldTitle = item.Name;

                return fieldTitle;
            }


            public static string GetBucket(ItemAdapter item)
            {
                var fieldTitle = item["IsBucket"];

                if (string.IsNullOrEmpty(fieldTitle))
                    fieldTitle = item["IsBucket"];

                return fieldTitle;
            }
        }
    }

    public class ItemAdapter
    {
        private Sitecore.Data.Items.Item m_item = null;

        public ItemAdapter()
        {
        }

        public ItemAdapter(Sitecore.Data.Items.Item item)
        {
            m_item = item;
        }

        public virtual string this[string name]
        {
            get
            {
                return m_item[name];
            }
        }

        public virtual string Name
        {
            get
            {
                return m_item.Name;
            }
        }

        public virtual string IsBucket
        {
            get
            {
                return m_item.Fields["IsBucket"].Value;
            }
        }

        
    }
}
