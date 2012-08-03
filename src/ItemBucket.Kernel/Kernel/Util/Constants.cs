//-----------------------------------------------------------------------
// <copyright file="Constants.cs" company="Sitecore A/S">
//     Sitecore A/S. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sitecore.ItemBucket.Kernel.Util
{
    using System;
    using Sitecore.Data;
    using Sitecore.Data.Items;
    using Sitecore.ItemBucket.Kernel.Kernel.Util;

    /// <summary>
    /// Item Bucket Constants
    /// </summary>
    public class Constants
    {
        /// <summary>
        /// Gets the name of the field that determines if an item is bucketable or not
        /// </summary>
        public static string BucketableField
        {
            get { return "Bucketable"; }
        }

        /// <summary>
        /// Gets "/"
        /// </summary>
        public static string ContentPathSeperator
        {
            get { return "/"; }
        }

        /// <summary>
        /// Gets "mo"
        /// </summary>
        public static string ModeQueryStringKeyName
        {
            get { return "mo"; }
        }

           /// <summary>
        /// Gets "vs"
        /// </summary>
        public static string VersionQueryStringKeyName
        {
            get { return "vs"; }
        }

        /// <summary>
        /// Gets "ri"
        /// </summary>
        public static string RibbonQueryStringKeyName
        {
            get { return "ri"; }
        }
      
        /// <summary>
        /// Gets the raw url of the relative path to the Content Editor
        /// </summary>
        public static string ContentEditorRawUrlAddress
        {
            get { return "/sitecore/shell/sitecore/content/Applications/Content%20Editor.aspx"; }
        }

        /// <summary>
        /// Gets the query string parameter for passing in an item id to an editor
        /// </summary>
        public static string OpenItemEditorQueryStringKeyName
        {
            get { return "fo"; }
        }

        /// <summary>
        /// Gets the Image Field Type name
        /// </summary>
        public static string ImageFieldType
        {
            get { return "Image"; }
        }

        /// <summary>
        /// Gets the Attachment Field Type name
        /// </summary>
        public static string AttachmentFieldType
        {
            get { return "attachment"; }
        }

        /// <summary>
        /// Gets the Default Image Path for search results
        /// </summary>
        public static string DefaultImagePath
        {
            get { return "./images/default.jpg"; }
        }

        /// <summary>
        /// Gets the name of the field tagged as Field Facets
        /// </summary>
        public static string IsTag
        {
            get { return "Is Facet"; }
        }

        /// <summary>
        /// Gets the Image Field Type name
        /// </summary>
        public static string ItemsPerPageText
        {
            get { return "Items Per Page Result"; }
        }

        /// <summary>
        /// Gets the value determining if an item should keep its descendant relationships when bucketed
        /// </summary>
        public static string ShouldNotOrganiseInBucket
        {
            get { return "ShouldNotOrganiseInBucket"; }
        }

        /// <summary>
        /// Gets the name of the field which determins if an item is a bucket
        /// </summary>
        public static string IsBucket
        {
            get { return "IsBucket"; }
        }

        /// <summary>
        /// Gets the field name that determines if a field should be displayed in the search results
        /// </summary>
        public static string IsSearchImage
        {
            get { return "Is Displayed in Search Results"; }
        }

        /// <summary>
        /// Gets the text used for running the bucketing process
        /// </summary>
        public static string BucketingText
        {
            get { return "Creating Bucket"; }
        }

        /// <summary>
        /// Gets the text used for running the bucketing process
        /// </summary>
        public static string UnBucketingText
        {
            get { return "Unbucketing"; }
        }

        /// <summary>
        /// Gets the Gutter warning hover tooltip
        /// </summary>
        public static string BucketGutterWarning
        {
            get { return "This item is a bucket and all items below this are hidden"; }
        }

        /// <summary>
        /// Gets the Sync bucket progress text
        /// </summary>
        public static string BucketingProgressText
        {
            get { return "Bucketing Sub Items"; }
        }

        /// <summary>
        /// Gets the Sync bucket progress text
        /// </summary>
        public static string UnBucketingProgressText
        {
            get { return "UnBucketing Sub Items"; }
        }

        /// <summary>
        /// Gets the name of the field that determines how search results will be opened
        /// </summary>
        public static string OpenSearchResult
        {
            get { return "Opening Search Results"; }
        }

        /// <summary>
        /// Gets the Search Editor Id
        /// </summary>
        public static string SearchEditor
        {
            get { return "{59F53BBB-D1F5-4E38-8EBA-0D73109BB59B}"; }
        }

        /// <summary>
        /// Gets the Facet Folder Id
        /// </summary>
        public static string FacetFolder
        {
            get { return "{0B42DC05-5CF9-4DD5-9A3A-8BDF11BE2B88}"; }
        }

        /// <summary>
        /// Gets the Facet Template Id
        /// </summary>
        public static string FacetTemplate
        {
            get { return "{9F63F3ED-BFB8-49A3-B370-1830E0A5920F}"; }
        }

        /// <summary>
        /// Gets the Id of the Folder that is used to create the bucket structures
        /// </summary>
        public static string BucketFolder
        {
            get { return "{ADB6CA4F-03EF-4F47-B9AC-9CE2BA53FF97}"; }
        }

        /// <summary>
        /// Gets the Tips Folder Id
        /// </summary>
        public static string TipsFolder
        {
            get { return "{C34856F2-E5EE-44A4-B72B-21B7ABF9F703}"; }
        }

        /// <summary>
        /// Gets the Folder Id for all the types of Filters that can be done through the UI
        /// </summary>
        public static string BucketSearchType
        {
            get { return "{7CE9FDA0-E6C1-4495-9740-9944656E893B}"; }
        }

        /// <summary>
        /// Gets the Id of the BucketGutterWarning
        /// </summary>
        public static string ContentEditorGutterFolder
        {
            get { return "{59F37069-3118-4151-8C01-5DA0EF12CB4E}"; }
        }

        /// <summary>
        /// Gets the Id of the Application node in the Core Database
        /// </summary>
        public static string CoreApplicationsFolder
        {
            get { return "{C74AC643-53C8-4F1E-9508-840CDC72AACA}"; }
        }

        /// <summary>
        /// Gets the Content Editor System Menu Id
        /// </summary>
        public static string ContentEditorSystemMenu
        {
            get { return "{FDBED83B-6F8E-405B-AAAB-22070085C9D4}"; }
        }

        /// <summary>
        /// Gets the Id of the folder containing the list of bulk operations that can be run on search results
        /// </summary>
        public static string ItemBucketsSearchOperationsFolder
        {
            get { return "{E5FF5407-BC94-41D1-A041-41B30E177B99}"; }
        }

        /// <summary>
        /// Gets the Id of the folder with all the drop down menu options available in the UI
        /// </summary>
        public static string DropDownMenuFolder
        {
            get { return "{00E978AE-076D-402F-B540-B1ACA5BC65A1}"; }
        }

        /// <summary>
        /// Gets the Tag Repository folder
        /// </summary>
        public static string TagFolder
        {
            get { return "{69C70EAB-9A64-4489-A74E-5B509EA5B4FF}"; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether debugging is enabled for a single search
        /// </summary>
        /// <remarks>If enabled, the system will Log all the queries and facets to the Log file for that one query</remarks>
        public static bool EnableTemporaryBucketDebug
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the Item Bucket main Settings Item
        /// </summary>
        public static Item SettingsItem
        {
            get
            {
                Database contentDatabase = Context.ContentDatabase;
                return contentDatabase.IsNull() ? Context.Database.GetItem("{376DA354-62FB-4066-ADE3-439419EAE7A8}") : contentDatabase.GetItem("{376DA354-62FB-4066-ADE3-439419EAE7A8}");
            }
        }

        /// <summary>
        /// Gets the Image Field Type name
        /// </summary>
        public class Index
        {
            /// <summary>
            /// Gets the default/fallback Index name which contains ONLY content
            /// </summary>
            public static string Name
            {
                get { return "itembuckets_buckets"; }
            }
        }
    }
}
