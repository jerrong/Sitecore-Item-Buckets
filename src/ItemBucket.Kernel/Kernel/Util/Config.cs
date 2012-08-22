namespace Sitecore.ItemBucket.Kernel.Util
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Sitecore.Configuration;
    using Sitecore.Data;

    public class Config
    {
        public static ID ContainerTemplateId
        {
            get
            {
                return ID.Parse("{ADB6CA4F-03EF-4F47-B9AC-9CE2BA53FF97}");
            }
        }

        public static string QueryServerAddress
        {
            get
            {
                return Settings.GetSetting("QueryServer", "");
            }
        }


        public static string NetworkDropPoint
        {
            get
            {
                return Settings.GetSetting("NetworkDropPoint", "");
            }
        }

        public static string RemoteIndexingServer
        {
            get
            {
                return Settings.GetSetting("RemoteIndexingServer", "");
            }
        }

        public static string RemoteIndexLocation
        {
            get
            {
                return Settings.GetSetting("RemoteIndexLocation", "");
            }
        }

        public static string SOLREnabled
        {
            get
            {
                return Settings.GetSetting("SOLREnabled", "false");
            }
        }

        public static string SOLRServiceLocation
        {
            get
            {
                return Settings.GetSetting("SOLRServiceLocation", "http://localhost:8983/solr");
            }
        }
        
        public static IEnumerable<ID> SupportedContainerTemplates
        {
            get
            {
                return ID.ParseArray("{ADB6CA4F-03EF-4F47-B9AC-9CE2BA53FF97}|{FE5DD826-48C6-436D-B87A-7C4210C7413B}".ToLower());
            }
        }

        public static string BucketFolderPath
        {
            get
            {
                return Settings.GetSetting("BucketFolderPath", "yyyy/MM/dd/HH/mm");
            }
        }

        /// <summary>
        /// Gets the name of the field that determines if an item is bucketable or not
        /// </summary>
        public static string BucketableField
        {
            get { return "Bucketable"; }
        }

        public static ID BucketTriggerCount
        {
            get
            {
                return ID.Parse(Settings.GetSetting("BucketTriggerCount", "100"));
            }
        }

        public static int LuceneMaxClauseCount
        {
            get
            {
                return Int32.Parse(Settings.GetSetting("LuceneQueryClauseCount", "1024"));
            }
        }
        public static bool EnableTips
        {
            get
            {
                var excludeTips = Settings.GetSetting("EnableSearchTips", "true");
                return excludeTips == "true";
            }
        }

        public static ID BucketTemplateId
        {
            get
            {
                return ID.Parse(Settings.GetSetting("BucketTemplateId", "{ADB6CA4F-03EF-4F47-B9AC-9CE2BA53FF97}"));
            }
        }

        public static bool ExcludeContextItemFromResult
        {
            get
            {
                var excludeResults = Settings.GetSetting("ExcludeContextItemFromResult", "true");
                return excludeResults == "true";
            }
        }

        public static bool EnableBucketDebug
        {
            get
            {
                var excludeResults = Settings.GetSetting("EnableBucketDebug", "true");
                return excludeResults == "true";
            }
        }


        public static List<String> DomainsForAuthorFacet
        {
            get
            {
                var listOfDomains = Settings.GetSetting("AuthorFacetDomains");
                return listOfDomains.Split('|').ToList();
            }
        }

        public static string SecuredItems
        {
            get
            {
                return Settings.GetSetting("SecuredItems");
            }
        }
    }
}
