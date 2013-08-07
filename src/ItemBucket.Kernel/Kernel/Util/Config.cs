// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Config.cs" company="Sitecore A/S">
//   Copyright (C) 2013 by Sitecore A/S
// </copyright>
// <summary>
//   Defines the Config type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.ItemBucket.Kernel.Util
{
  using System.Collections.Generic;
  using System.Linq;

  using Sitecore.Configuration;
  using Sitecore.Data;

  /// <summary>
  /// Defines the Item Buckets settings class.
  /// </summary>
  public class Config
  {
    /// <summary>
    /// Gets the container template id.
    /// </summary>
    /// <value>
    /// The container template id.
    /// </value>
    public static ID ContainerTemplateId
    {
      get
      {
        return ID.Parse("{ADB6CA4F-03EF-4F47-B9AC-9CE2BA53FF97}");
      }
    }

    /// <summary>
    /// Gets the query server address.
    /// </summary>
    /// <value>
    /// The query server address.
    /// </value>
    public static string QueryServerAddress
    {
      get
      {
        return Settings.GetSetting("QueryServer", string.Empty);
      }
    }

    /// <summary>
    /// Gets the network drop point.
    /// </summary>
    /// <value>
    /// The network drop point.
    /// </value>
    public static string NetworkDropPoint
    {
      get
      {
        return Settings.GetSetting("NetworkDropPoint", string.Empty);
      }
    }

    /// <summary>
    /// Gets the remote indexing server.
    /// </summary>
    /// <value>
    /// The remote indexing server.
    /// </value>
    public static string RemoteIndexingServer
    {
      get
      {
        return Settings.GetSetting("RemoteIndexingServer", string.Empty);
      }
    }

    /// <summary>
    /// Gets the remote index location.
    /// </summary>
    /// <value>
    /// The remote index location.
    /// </value>
    public static string RemoteIndexLocation
    {
      get
      {
        return Settings.GetSetting("RemoteIndexLocation", string.Empty);
      }
    }

    /// <summary>
    /// Gets the SOLR enabled.
    /// </summary>
    /// <value>
    /// The SOLR enabled.
    /// </value>
    public static string SolrEnabled
    {
      get
      {
        return Settings.GetSetting("SOLREnabled", "false");
      }
    }

    /// <summary>
    /// Gets the SOLR service location.
    /// </summary>
    /// <value>
    /// The SOLR service location.
    /// </value>
    public static string SolrServiceLocation
    {
      get
      {
        return Settings.GetSetting("SOLRServiceLocation", "http://localhost:8983/solr");
      }
    }

    /// <summary>
    /// Gets the supported container templates.
    /// </summary>
    /// <value>
    /// The supported container templates.
    /// </value>
    public static IEnumerable<ID> SupportedContainerTemplates
    {
      get
      {
        return ID.ParseArray("{ADB6CA4F-03EF-4F47-B9AC-9CE2BA53FF97}|{FE5DD826-48C6-436D-B87A-7C4210C7413B}".ToLower());
      }
    }

    /// <summary>
    /// Gets the bucket folder path.
    /// </summary>
    /// <value>
    /// The bucket folder path.
    /// </value>
    public static string BucketFolderPath
    {
      get
      {
        return Settings.GetSetting("BucketFolderPath", "yyyy/MM/dd/HH/mm");
      }
    }

    /// <summary>
    /// Gets the name of the field that determines if an item is bucketable or not.
    /// </summary>
    /// <value>
    /// The bucketable field.
    /// </value>
    public static string BucketableField
    {
      get
      {
        return "Bucketable";
      }
    }

    /// <summary>
    /// Gets the bucket trigger count.
    /// </summary>
    /// <value>
    /// The bucket trigger count.
    /// </value>
    public static ID BucketTriggerCount
    {
      get
      {
        return ID.Parse(Settings.GetSetting("BucketTriggerCount", "100"));
      }
    }

    /// <summary>
    /// Gets the Lucene max clause count.
    /// </summary>
    /// <value>
    /// The Lucene max clause count.
    /// </value>
    public static int LuceneMaxClauseCount
    {
      get
      {
        return int.Parse(Settings.GetSetting("LuceneQueryClauseCount", "1024"));
      }
    }

    /// <summary>
    /// Gets a value indicating whether [enable tips].
    /// </summary>
    /// <value>
    ///   <c>true</c> if [enable tips]; otherwise, <c>false</c>.
    /// </value>
    public static bool EnableTips
    {
      get
      {
        string excludeTips = Settings.GetSetting("EnableSearchTips", "true");
        return excludeTips == "true";
      }
    }

    /// <summary>
    /// Gets the bucket template id.
    /// </summary>
    /// <value>
    /// The bucket template id.
    /// </value>
    public static ID BucketTemplateId
    {
      get
      {
        return ID.Parse(Settings.GetSetting("BucketTemplateId", "{ADB6CA4F-03EF-4F47-B9AC-9CE2BA53FF97}"));
      }
    }

    /// <summary>
    /// Gets a value indicating whether [exclude context item from result].
    /// </summary>
    /// <value>
    /// <c>true</c> if [exclude context item from result]; otherwise, <c>false</c>.
    /// </value>
    public static bool ExcludeContextItemFromResult
    {
      get
      {
        string excludeResults = Settings.GetSetting("ExcludeContextItemFromResult", "true");
        return excludeResults == "true";
      }
    }

    /// <summary>
    /// Gets a value indicating whether [enable bucket debug].
    /// </summary>
    /// <value>
    ///   <c>true</c> if [enable bucket debug]; otherwise, <c>false</c>.
    /// </value>
    public static bool EnableBucketDebug
    {
      get
      {
        string excludeResults = Settings.GetSetting("EnableBucketDebug", "true");
        return excludeResults == "true";
      }
    }

    /// <summary>
    /// Gets the domains for author facet.
    /// </summary>
    /// <value>
    /// The domains for author facet.
    /// </value>
    public static List<string> DomainsForAuthorFacet
    {
      get
      {
        string listOfDomains = Settings.GetSetting("AuthorFacetDomains");
        return listOfDomains.Split('|').ToList();
      }
    }

    /// <summary>
    /// Gets the secured items.
    /// </summary>
    /// <value>
    /// The secured items.
    /// </value>
    public static string SecuredItems
    {
      get
      {
        return Settings.GetSetting("SecuredItems");
      }
    }
  }
}