using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Eventing;
using Sitecore.ItemBucket.Kernel.Managers;
using Sitecore.Web;

namespace Sitecore.ItemBucket.Kernel.Kernel.Managers
{
    public class BucketImportContext : IDisposable
    {
        // Fields
        private static object _lock = new object();
        private string _oldValue;
        private Item _item;
        protected static List<ThreadStart> _onLeave = new List<ThreadStart>();
        private const string ItemsKey = "BucketImportContext";
        private int m_disposed;

        // Events
        public static event ThreadStart OnLeave
        {
            add
            {
                lock (_lock)
                {
                    _onLeave.Add(value);
                }
            }
            remove
            {
                lock (_lock)
                {
                    _onLeave.Remove(value);
                }
            }
        }

        // Methods
        public BucketImportContext(Item bucketItem)
        {
            lock (_lock)
            {
                _item = bucketItem;
                this._oldValue = WebUtil.GetItemsString("BucketImportContext");
                WebUtil.SetItemsValue("BucketImportContext", "1");
            }
        }

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref this.m_disposed, 1, 0) == 0)
            {
                WebUtil.SetItemsValue("BucketImportContext", this._oldValue);
            }
            if (!IsActive)
            {
                lock (_lock)
                {
                    if (!IsActive)
                    {
                        foreach (ThreadStart start in _onLeave)
                        {
                            start();
                        }
                    }
                }
            }

            BucketManager.Sync(_item);
        }

        // Properties
        public static bool IsActive
        {
            get
            {
                return (WebUtil.GetItemsString("BucketImportContext") == "1");
            }
        }
    }


}
