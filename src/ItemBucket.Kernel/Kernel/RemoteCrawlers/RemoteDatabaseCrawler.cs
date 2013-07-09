// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemoteDatabaseCrawler.cs" company="Sitecore A/S">
//   Copyright (C) 2013 by Sitecore A/S
// </copyright>
// <summary>
//   Defines the RemoteDatabaseCrawler type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.ItemBucket.Kernel.Kernel.RemoteCrawlers
{
  using System;
  using System.Collections.Generic;
  using System.Text;
  using System.Text.RegularExpressions;

  using Lucene.Net.Documents;
  using Lucene.Net.Index;
  using Lucene.Net.Search;

  using Sitecore.Collections;
  using Sitecore.Common;
  using Sitecore.Configuration;
  using Sitecore.Data;
  using Sitecore.Data.Items;
  using Sitecore.Data.Managers;
  using Sitecore.Data.Templates;
  using Sitecore.Diagnostics;
  using Sitecore.Events;
  using Sitecore.Globalization;
  using Sitecore.ItemBuckets.BigData.RemoteIndex;
  using Sitecore.Links;
  using Sitecore.Search;
  using Sitecore.Search.Crawlers;
  using Sitecore.Search.Crawlers.FieldCrawlers;
  using Sitecore.SecurityModel;

  using Field = Sitecore.Data.Fields.Field;
  using Version = Sitecore.Data.Version;

  /// <summary>
  /// Represents the Remote Database Crawler for Lucene indexes.
  /// </summary>
  public class RemoteDatabaseCrawler : BaseCrawler, IRemoteCrawler
  {
    /// <summary>
    /// The template filter
    /// </summary>
    protected readonly Dictionary<string, bool> TemplateFilter = new Dictionary<string, bool>();

    /// <summary>
    /// The shorten GUID
    /// </summary>
    private static readonly Regex ShortenGuid = new Regex("[{}-]", RegexOptions.Compiled);

    /// <summary>
    /// The has excludes
    /// </summary>
    private bool hasExcludes;

    /// <summary>
    /// The has includes
    /// </summary>
    private bool hasIncludes;
    
    /// <summary>
    /// The target database
    /// </summary>
    private Database targetDatabase;

    /// <summary>
    /// The remote index
    /// </summary>
    private RemoteIndex remoteIndex;

    /// <summary>
    /// The index all fields
    /// </summary>
    private bool indexAllFields = true;

    /// <summary>
    /// The monitor changes
    /// </summary>
    private bool monitorChanges = true;

    /// <summary>
    /// The root
    /// </summary>
    private Item root;
  
    /// <summary>
    /// The field type list
    /// </summary>
    private List<string> fieldTypeList =
      new List<string>(
        new[]
          {
              "Single-Line Text", "Rich Text", "Multi-Line Text", "text", "rich text", "html", "memo", "Word Document" 
          });

    /// <summary>
    /// Gets or sets the database.
    /// </summary>
    /// <value>
    /// The database.
    /// </value>
    public string Database
    {
      get
      {
        if (this.targetDatabase != null)
        {
          return this.targetDatabase.Name;
        }

        return null;
      }

      set
      {
        Assert.ArgumentNotNullOrEmpty(value, "value");

        this.targetDatabase = Factory.GetDatabase(value);

        Assert.IsNotNull(this.targetDatabase, "Database " + value + " does not exist");

        using (new SecurityDisabler())
        {
          this.root = this.targetDatabase.GetRootItem();
        }

        Assert.IsNotNull(this.root, "Root item is not defined for " + value + " database.");
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether [index all fields].
    /// </summary>
    /// <value>
    ///   <c>true</c> if [index all fields]; otherwise, <c>false</c>.
    /// </value>
    public bool IndexAllFields
    {
      get
      {
        return this.indexAllFields;
      }

      set
      {
        this.indexAllFields = value;
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether [monitor changes].
    /// </summary>
    /// <value>
    ///   <c>true</c> if [monitor changes]; otherwise, <c>false</c>.
    /// </value>
    public bool MonitorChanges
    {
      get
      {
        return this.monitorChanges;
      }

      set
      {
        this.monitorChanges = value;
      }
    }

    /// <summary>
    /// Gets or sets the root.
    /// </summary>
    /// <value>
    /// The root.
    /// </value>
    public string Root
    {
      get
      {
        if (this.root != null)
        {
          return this.root.ID.ToString();
        }

        return null;
      }

      set
      {
        Assert.ArgumentNotNullOrEmpty(value, "value");
        Assert.IsNotNull(this.targetDatabase, "database is not set yet");

        this.root = ItemManager.GetItem(value, Language.Invariant, Version.Latest, this.targetDatabase, SecurityCheck.Disable);
      }
    }

    /// <summary>
    /// Gets or sets the text field types.
    /// </summary>
    /// <value>
    /// The text field types.
    /// </value>
    protected List<string> TextFieldTypes
    {
      get
      {
        return this.fieldTypeList;
      }

      set
      {
        Assert.ArgumentNotNull(value, "value");

        this.fieldTypeList = value;
      }
    }

    /// <summary>
    /// Adds the specified context.
    /// </summary>
    /// <param name="context">The context.</param>
    public void Add(IndexUpdateContext context)
    {
      Assert.ArgumentNotNull(context, "context");

      this.AddTree(this.root, context);
    }

    /// <summary>
    /// Adds the item.
    /// </summary>
    /// <param name="item">The item.</param>
    public void AddItem(Item item)
    {
      Assert.ArgumentNotNull(item, "item");

      if (this.IsMatch(item))
      {
        using (IndexUpdateContext context = this.remoteIndex.CreateUpdateContext())
        {
          this.AddItem(item, context);
          context.Commit();
        }
      }
    }

    /// <summary>
    /// Adds the type of the text field.
    /// </summary>
    /// <param name="type">The type.</param>
    public void AddTextFieldType(string type)
    {
      Assert.ArgumentNotNullOrEmpty(type, "type");

      this.fieldTypeList.Add(type);
    }

    /// <summary>
    /// Adds the tree.
    /// </summary>
    /// <param name="rootItem">The root item.</param>
    public void AddTree(Item rootItem)
    {
      Assert.ArgumentNotNull(rootItem, "rootItem");

      if (rootItem.Axes.IsDescendantOf(this.root))
      {
        using (IndexUpdateContext context = this.remoteIndex.CreateUpdateContext())
        {
          this.AddTree(rootItem, context);
          context.Commit();
        }
      }
    }

    /// <summary>
    /// Adds the version.
    /// </summary>
    /// <param name="version">The version.</param>
    public void AddVersion(Item version)
    {
      Assert.ArgumentNotNull(version, "version");

      if (this.IsMatch(version))
      {
        using (IndexUpdateContext context = this.remoteIndex.CreateUpdateContext())
        {
          this.AddVersion(version, context);
          context.Commit();
        }
      }
    }

    /// <summary>
    /// Deletes the item.
    /// </summary>
    /// <param name="item">The item.</param>
    public void DeleteItem(Item item)
    {
      Assert.ArgumentNotNull(item, "item");

      if (this.IsMatch(item))
      {
        using (IndexDeleteContext context = this.remoteIndex.CreateDeleteContext())
        {
          this.DeleteItem(item.ID, context);
          context.Commit();
        }
      }
    }

    /// <summary>
    /// Deletes the tree.
    /// </summary>
    /// <param name="rootItem">The root item.</param>
    public void DeleteTree(Item rootItem)
    {
      Assert.ArgumentNotNull(rootItem, "rootItem");

      if (rootItem.Axes.IsDescendantOf(this.root))
      {
        using (IndexDeleteContext context = this.remoteIndex.CreateDeleteContext())
        {
          this.DeleteTree(rootItem.ID, context);
          context.Commit();
        }
      }
    }

    /// <summary>
    /// Deletes the version.
    /// </summary>
    /// <param name="version">The version.</param>
    public void DeleteVersion(Item version)
    {
      Assert.ArgumentNotNull(version, "version");

      if (this.IsMatch(version))
      {
        using (IndexDeleteContext context = this.remoteIndex.CreateDeleteContext())
        {
          this.DeleteVersion(version.ID, version.Language.ToString(), version.Version.ToString(), context);
          context.Commit();
        }
      }
    }

    /// <summary>
    /// Excludes the template.
    /// </summary>
    /// <param name="value">The value.</param>
    public void ExcludeTemplate(string value)
    {
      Assert.ArgumentNotNullOrEmpty(value, "value");

      this.hasExcludes = true;
      this.TemplateFilter[value] = false;
    }

    /// <summary>
    /// Includes the template.
    /// </summary>
    /// <param name="value">The value.</param>
    public void IncludeTemplate(string value)
    {
      Assert.ArgumentNotNullOrEmpty(value, "value");

      this.hasIncludes = true;
      this.TemplateFilter[value] = true;
    }

    /// <summary>
    /// Initializes the specified index.
    /// </summary>
    /// <param name="index">The index.</param>
    public void Initialize(RemoteIndex index)
    {
      Assert.ArgumentNotNull(index, "index");
      Assert.IsNotNull(index, "index");

      this.remoteIndex = index;
      
      Assert.IsNotNull(this.targetDatabase, "Database is not defined");
      Assert.IsNotNull(this.root, "Root item is not defined");
      
      IndexingManager.Provider.OnUpdateItem += this.Provider_OnUpdateItem;
      IndexingManager.Provider.OnRemoveItem += this.Provider_OnRemoveItem;
      IndexingManager.Provider.OnRemoveVersion += this.Provider_OnRemoveVersion;
    }

    /// <summary>
    /// Removes the type of the text field.
    /// </summary>
    /// <param name="type">The type.</param>
    public void RemoveTextFieldType(string type)
    {
      Assert.ArgumentNotNullOrEmpty(type, "type");

      this.fieldTypeList.Remove(type);
    }

    /// <summary>
    /// Updates the database.
    /// </summary>
    /// <param name="database">The database.</param>
    public void UpdateDatabase(Database database)
    {
      Assert.ArgumentNotNull(database, "database");
     
      Item rootItem = database.GetRootItem();
      if (rootItem != null)
      {
        this.UpdateTree(rootItem);
      }
    }

    /// <summary>
    /// Updates the item.
    /// </summary>
    /// <param name="item">The item.</param>
    public void UpdateItem(Item item)
    {
      Assert.ArgumentNotNull(item, "item");
      if (this.IsMatch(item))
      {
        this.DeleteItem(item);
        this.AddItem(item);
      }
    }

    /// <summary>
    /// Updates the tree.
    /// </summary>
    /// <param name="rootItem">The root.</param>
    public void UpdateTree(Item rootItem)
    {
      Assert.ArgumentNotNull(rootItem, "rootItem");

      if (rootItem.Axes.IsDescendantOf(this.root))
      {
        this.DeleteTree(rootItem);
        this.AddTree(rootItem);
      }
    }

    /// <summary>
    /// Updates the version.
    /// </summary>
    /// <param name="version">The version.</param>
    public void UpdateVersion(Item version)
    {
      Assert.ArgumentNotNull(version, "version");

      if (this.IsMatch(version))
      {
        this.DeleteVersion(version);
        this.AddVersion(version);
      }
    }

    /// <summary>
    /// Adds all fields.
    /// </summary>
    /// <param name="document">The document.</param>
    /// <param name="item">The item.</param>
    /// <param name="versionSpecific">if set to <c>true</c> [version specific].</param>
    protected virtual void AddAllFields(Document document, Item item, bool versionSpecific)
    {
      Assert.ArgumentNotNull(document, "document");
      Assert.ArgumentNotNull(item, "item");

      foreach (Field field in item.Fields)
      {
        if (!string.IsNullOrEmpty(field.Key))
        {
          bool tokenize = this.IsTextField(field);
          FieldCrawlerBase fieldCrawler = FieldCrawlerFactory.GetFieldCrawler(field);
          Assert.IsNotNull(fieldCrawler, "fieldCrawler");
          if (this.IndexAllFields)
          {
            document.Add(this.CreateField(field.Key, fieldCrawler.GetValue(), tokenize, 1f));
          }

          if (tokenize)
          {
            document.Add(this.CreateField(BuiltinFields.Content, fieldCrawler.GetValue(), true, 1f));
          }
        }
      }
    }

    /// <summary>
    /// Adds the item.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <param name="context">The context.</param>
    protected virtual void AddItem(Item item, IndexUpdateContext context)
    {
      Assert.ArgumentNotNull(item, "item");
      Assert.ArgumentNotNull(context, "context");

      if (this.IsMatch(item))
      {
        foreach (Language language in item.Languages)
        {
          Item latestVersion = item.Database.GetItem(item.ID, language, Version.Latest);
          if (latestVersion != null)
          {
            foreach (Item item3 in latestVersion.Versions.GetVersions(false))
            {
              this.IndexVersion(item3, latestVersion, context);
            }
          }
        }
      }
    }

    /// <summary>
    /// Adds the item identifiers.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <param name="document">The document.</param>
    [Obsolete("Use AddVersionIdentifiers")]
    protected void AddItemIdentifiers(Item item, Document document)
    {
      Assert.ArgumentNotNull(item, "item");
      Assert.ArgumentNotNull(document, "document");

      document.Add(this.CreateValueField(BuiltinFields.Database, item.Database.Name));
      document.Add(this.CreateValueField(BuiltinFields.ID, ShortID.Encode(item.ID)));
      document.Add(this.CreateValueField(BuiltinFields.Language, "neutral"));
      document.Add(this.CreateTextField(BuiltinFields.Template, ShortID.Encode(item.TemplateID)));
      document.Add(this.CreateDataField(BuiltinFields.Url, new ItemUri(item.ID, item.Database).ToString(), 0f));
      document.Add(this.CreateDataField(BuiltinFields.Group, ShortID.Encode(item.ID), 0f));
    }

    /// <summary>
    /// Adds the match criteria.
    /// </summary>
    /// <param name="query">The query.</param>
    protected virtual void AddMatchCriteria(BooleanQuery query)
    {
      Assert.ArgumentNotNull(query, "query");

      query.Add(new TermQuery(new Term(BuiltinFields.Database, this.root.Database.Name)), BooleanClause.Occur.MUST);
      query.Add(new TermQuery(new Term(BuiltinFields.Path, ShortID.Encode(this.root.ID))), BooleanClause.Occur.MUST);
      
      if (this.hasIncludes || this.hasExcludes)
      {
        foreach (var pair in this.TemplateFilter)
        {
          query.Add(
            new TermQuery(new Term(BuiltinFields.Template, ShortID.Encode(pair.Key))),
            pair.Value ? BooleanClause.Occur.SHOULD : BooleanClause.Occur.MUST_NOT);
        }
      }
    }

    /// <summary>
    /// Adds the special fields.
    /// </summary>
    /// <param name="document">The document.</param>
    /// <param name="item">The item.</param>
    protected virtual void AddSpecialFields(Document document, Item item)
    {
      Assert.ArgumentNotNull(document, "document");
      Assert.ArgumentNotNull(item, "item");

      string displayName = item.Appearance.DisplayName;
      Assert.IsNotNull(displayName, "Item's display name is null.");

      document.Add(this.CreateTextField(BuiltinFields.Name, item.Name));
      document.Add(this.CreateDataField(BuiltinFields.Name, item.Name));
      document.Add(this.CreateTextField(BuiltinFields.Name, displayName));
      document.Add(this.CreateValueField(BuiltinFields.Icon, item.Appearance.Icon));
      document.Add(this.CreateTextField(BuiltinFields.Creator, item.Statistics.CreatedBy));
      document.Add(this.CreateTextField(BuiltinFields.Editor, item.Statistics.UpdatedBy));
      document.Add(this.CreateTextField(BuiltinFields.AllTemplates, this.GetAllTemplates(item)));
      document.Add(this.CreateTextField(BuiltinFields.TemplateName, item.TemplateName));

      if (this.IsHidden(item))
      {
        document.Add(this.CreateValueField(BuiltinFields.Hidden, "1"));
      }

      document.Add(this.CreateValueField(BuiltinFields.Created, item[FieldIDs.Created]));
      document.Add(this.CreateValueField(BuiltinFields.Updated, item[FieldIDs.Updated]));
      document.Add(this.CreateTextField(BuiltinFields.Path, this.GetItemPath(item)));
      document.Add(this.CreateTextField(BuiltinFields.Links, this.GetItemLinks(item)));

      if (this.Tags.Length > 0)
      {
        document.Add(this.CreateTextField(BuiltinFields.Tags, this.Tags));
        document.Add(this.CreateDataField(BuiltinFields.Tags, this.Tags));
      }
    }
    
    /// <summary>
    /// Adds the tree.
    /// </summary>
    /// <param name="rootItem">The root item.</param>
    /// <param name="context">The context.</param>
    protected void AddTree(Item rootItem, IndexUpdateContext context)
    {
      Assert.ArgumentNotNull(rootItem, "rootItem");
      Assert.ArgumentNotNull(context, "context");

      using (new LimitMemoryContext(true))
      {
        this.AddItem(rootItem, context);
        var list = new List<ID>();
        foreach (Item item in rootItem.GetChildren(ChildListOptions.IgnoreSecurity))
        {
          list.Add(item.ID);
        }

        foreach (ID id in list)
        {
          Item item2 = rootItem.Database.GetItem(id);
          Assert.IsNotNull(item2, "Child item was not found.");
          this.AddTree(item2, context);
        }
      }
    }
    
    /// <summary>
    /// Adds the version.
    /// </summary>
    /// <param name="version">The version.</param>
    /// <param name="context">The context.</param>
    protected virtual void AddVersion(Item version, IndexUpdateContext context)
    {
      Assert.ArgumentNotNull(version, "version");
      Assert.ArgumentNotNull(context, "context");

      Item latestVersion = version.Database.GetItem(version.ID, version.Language, Version.Latest);
      if (latestVersion != null)
      {
        this.IndexVersion(version, latestVersion, context);
      }
    }

    /// <summary>
    /// Adds the version identifiers.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <param name="latestVersion">The latest version.</param>
    /// <param name="document">The document.</param>
    protected void AddVersionIdentifiers(Item item, Item latestVersion, Document document)
    {
      Assert.ArgumentNotNull(item, "item");
      Assert.ArgumentNotNull(latestVersion, "latestVersion");
      Assert.ArgumentNotNull(document, "document");
      
      document.Add(this.CreateValueField(BuiltinFields.Database, item.Database.Name));
      document.Add(
        this.CreateValueField(
          BuiltinFields.ID, this.GetItemId(item.ID, item.Language.ToString(), item.Version.ToString())));
      document.Add(this.CreateValueField(BuiltinFields.Language, item.Language.ToString()));
      document.Add(this.CreateTextField(BuiltinFields.Template, ShortID.Encode(item.TemplateID)));
      
      if (item.Version.Number == latestVersion.Version.Number)
      {
        document.Add(this.CreateValueField(BuiltinFields.LatestVersion, "1"));
      }

      document.Add(this.CreateDataField(BuiltinFields.Url, item.Uri.ToString()));
      document.Add(this.CreateDataField(BuiltinFields.Group, ShortID.Encode(item.ID)));
    }

    /// <summary>
    /// Adjusts the boost.
    /// </summary>
    /// <param name="document">The document.</param>
    /// <param name="item">The item.</param>
    protected virtual void AdjustBoost(Document document, Item item)
    {
      Assert.ArgumentNotNull(document, "document");
      Assert.ArgumentNotNull(item, "item");

      document.SetBoost(this.Boost);
    }

    /// <summary>
    /// Deletes the item.
    /// </summary>
    /// <param name="itemId">The item id.</param>
    /// <param name="context">The context.</param>
    protected virtual void DeleteItem(ID itemId, IndexDeleteContext context)
    {
      Assert.ArgumentNotNull(itemId, "itemId");
      Assert.ArgumentNotNull(context, "context");

      context.DeleteDocuments(context.Search(new PreparedQuery(this.GetItemQuery(itemId))).Ids);
    }
    
    /// <summary>
    /// Deletes the tree.
    /// </summary>
    /// <param name="rootId">The root id.</param>
    /// <param name="context">The context.</param>
    protected void DeleteTree(ID rootId, IndexDeleteContext context)
    {
      Assert.ArgumentNotNull(rootId, "rootId");
      Assert.ArgumentNotNull(context, "context");

      context.DeleteDocuments(context.Search(new PreparedQuery(this.GetDescendantsQuery(rootId))).Ids);
    }
    
    /// <summary>
    /// Deletes the version.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <param name="language">The language.</param>
    /// <param name="version">The version.</param>
    /// <param name="context">The context.</param>
    protected virtual void DeleteVersion(ID id, string language, string version, IndexDeleteContext context)
    {
      Assert.ArgumentNotNull(id, "id");
      Assert.ArgumentNotNull(language, "language");
      Assert.ArgumentNotNullOrEmpty(version, "version");
      Assert.ArgumentNotNull(context, "context");

      context.DeleteDocuments(context.Search(new PreparedQuery(this.GetVersionQuery(id, language, version))).Ids);
    }

    /// <summary>
    /// Gets all templates.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>The string of all item's templates.</returns>
    protected string GetAllTemplates(Item item)
    {
      Assert.ArgumentNotNull(item, "item");
      Assert.IsNotNull(item.Template, "Item's template is null.");

      var builder = new StringBuilder();
      builder.Append(ShortID.Encode(item.TemplateID));
      builder.Append(" ");

      foreach (TemplateItem item2 in item.Template.BaseTemplates)
      {
        builder.Append(ShortID.Encode(item2.ID));
        builder.Append(" ");
      }

      return builder.ToString();
    }

    /// <summary>
    /// Gets the descendants query.
    /// </summary>
    /// <param name="itemId">The item ID.</param>
    /// <returns>The query of descendants.</returns>
    protected virtual Query GetDescendantsQuery(ID itemId)
    {
      Assert.ArgumentNotNull(itemId, "itemID");

      var query = new BooleanQuery();
      query.Add(new TermQuery(new Term(BuiltinFields.Path, ShortID.Encode(itemId))), BooleanClause.Occur.MUST);
      this.AddMatchCriteria(query);
      return query;
    }

    /// <summary>
    /// Gets the descendants query.
    /// </summary>
    /// <param name="rootItem">The root.</param>
    /// <returns>The query of descendants.</returns>
    protected Query GetDescendantsQuery(Item rootItem)
    {
      Assert.ArgumentNotNull(rootItem, "rootItem");

      return this.GetDescendantsQuery(rootItem.ID);
    }

    /// <summary>
    /// Gets the item ID.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <param name="language">The language.</param>
    /// <param name="version">The version.</param>
    /// <returns>The string of the item's ID.</returns>
    protected string GetItemId(ID id, string language, string version)
    {
      Assert.ArgumentNotNull(id, "id");
      Assert.ArgumentNotNull(language, "language");
      Assert.ArgumentNotNull(version, "version");

      return ShortID.Encode(id) + language + "%" + version;
    }

    /// <summary>
    /// Gets the item links.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>The string of the item's links.</returns>
    protected string GetItemLinks(Item item)
    {
      Assert.ArgumentNotNull(item, "item");

      var builder = new StringBuilder();
      foreach (ItemLink link in item.Links.GetAllLinks(false))
      {
        builder.Append(" ");
        builder.Append(ShortID.Encode(link.TargetItemID));
      }

      return builder.ToString();
    }

    /// <summary>
    /// Gets the item path.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>The string of the item's path.</returns>
    protected string GetItemPath(Item item)
    {
      Assert.ArgumentNotNull(item, "item");

      return ShortenGuid.Replace(item.Paths.LongID.Replace('/', ' '), string.Empty);
    }

    /// <summary>
    /// Gets the item query.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <returns>The query of the item.</returns>
    protected virtual Query GetItemQuery(ID id)
    {
      Assert.ArgumentNotNull(id, "id");

      var query = new BooleanQuery();
      query.Add(new PrefixQuery(new Term(BuiltinFields.ID, this.GetWildcardItemId(id))), BooleanClause.Occur.MUST);
      this.AddMatchCriteria(query);
      return query;
    }

    /// <summary>
    /// Gets the item query.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>The query of the item.</returns>
    protected Query GetItemQuery(Item item)
    {
      Assert.ArgumentNotNull(item, "item");

      return this.GetItemQuery(item.ID);
    }

    /// <summary>
    /// Gets the version query.
    /// </summary>
    /// <param name="version">The version.</param>
    /// <returns>The query of the version.</returns>
    protected Query GetVersionQuery(Item version)
    {
      Assert.ArgumentNotNull(version, "version");

      return this.GetVersionQuery(version.ID, version.Language.ToString(), version.Version.ToString());
    }

    /// <summary>
    /// Gets the version query.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <param name="language">The language.</param>
    /// <param name="version">The version.</param>
    /// <returns>The query of the version.</returns>
    protected virtual Query GetVersionQuery(ID id, string language, string version)
    {
      Assert.ArgumentNotNull(id, "id");
      Assert.ArgumentNotNullOrEmpty(language, "language");
      Assert.ArgumentNotNullOrEmpty(version, "version");

      var query = new BooleanQuery();
      query.Add(
        new TermQuery(new Term(BuiltinFields.ID, this.GetItemId(id, language, version))), BooleanClause.Occur.MUST);
      this.AddMatchCriteria(query);
      return query;
    }

    /// <summary>
    /// Gets the wildcard item ID.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <returns>The ID of the wildcard item.</returns>
    protected string GetWildcardItemId(ID id)
    {
      Assert.ArgumentNotNull(id, "id");

      return ShortID.Encode(id);
    }

    /// <summary>
    /// Indexes the shared data.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <param name="context">The context.</param>
    [Obsolete("Shared fields are moved into item versions")]
    protected void IndexSharedData(Item item, IndexUpdateContext context)
    {
      Assert.ArgumentNotNull(item, "item");
      Assert.ArgumentNotNull(context, "context");

      var document = new Document();
      this.AddItemIdentifiers(item, document);
      this.AddAllFields(document, item, false);
      this.AddSpecialFields(document, item);
      this.AdjustBoost(document, item);
      context.AddDocument(document);
    }

    /// <summary>
    /// Indexes the version.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <param name="latestVersion">The latest version.</param>
    /// <param name="context">The context.</param>
    protected virtual void IndexVersion(Item item, Item latestVersion, IndexUpdateContext context)
    {
      Assert.ArgumentNotNull(item, "item");
      Assert.ArgumentNotNull(latestVersion, "latestVersion");
      Assert.ArgumentNotNull(context, "context");

      var document = new Document();
      this.AddVersionIdentifiers(item, latestVersion, document);
      this.AddAllFields(document, item, true);
      this.AddSpecialFields(document, item);
      this.AdjustBoost(document, item);
      context.AddDocument(document);
    }
    
    /// <summary>
    /// Determines whether the specified item is match.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>
    ///   <c>true</c> if the specified item is match; otherwise, <c>false</c>.
    /// </returns>
    protected virtual bool IsMatch(Item item)
    {
      Assert.ArgumentNotNull(item, "item");

      if (!this.root.Axes.IsAncestorOf(item))
      {
        return false;
      }

      if (!this.hasIncludes && !this.hasExcludes)
      {
        return true;
      }

      bool flag;
      if (!this.TemplateFilter.TryGetValue(item.TemplateID.ToString(), out flag))
      {
        return !this.hasIncludes;
      }

      return flag;
    }

    /// <summary>
    /// Determines whether [is text field] [the specified field].
    /// </summary>
    /// <param name="field">The field.</param>
    /// <returns>
    ///   <c>true</c> if [is text field] [the specified field]; otherwise, <c>false</c>.
    /// </returns>
    protected virtual bool IsTextField(Field field)
    {
      Assert.ArgumentNotNull(field, "field");

      if (!this.TextFieldTypes.Contains(field.Type))
      {
        return false;
      }

      TemplateField templateField = field.GetTemplateField();

      return (templateField == null) || !templateField.ExcludeFromTextSearch;
    }

    /// <summary>
    /// Determines whether the specified item is hidden.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>
    ///   <c>true</c> if the specified item is hidden; otherwise, <c>false</c>.
    /// </returns>
    private bool IsHidden(Item item)
    {
      Assert.ArgumentNotNull(item, "item");

      return item.Appearance.Hidden || ((item.Parent != null) && this.IsHidden(item.Parent));
    }

    /// <summary>
    /// Handles the OnRemoveItem event of the Provider control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void Provider_OnRemoveItem(object sender, EventArgs e)
    {
      Assert.ArgumentNotNull(sender, "sender");
      Assert.ArgumentNotNull(e, "e");
      
      var database = SitecoreEventArgs.GetObject(e, 0) as Database;
      if ((database != null) && (database.Name == this.targetDatabase.Name))
      {
        ID iD = SitecoreEventArgs.GetID(e, 1);
        Assert.IsNotNull(iD, "ID is not passed to RemoveItem handler");
        using (IndexDeleteContext context = this.remoteIndex.CreateDeleteContext())
        {
          this.DeleteItem(iD, context);
          context.Commit();
        }
      }
    }

    /// <summary>
    /// Handles the OnRemoveVersion event of the Provider control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void Provider_OnRemoveVersion(object sender, EventArgs e)
    {
      Assert.ArgumentNotNull(sender, "sender");
      Assert.ArgumentNotNull(e, "e");
      
      var database = SitecoreEventArgs.GetObject(e, 0) as Database;
      if ((database != null) && (database.Name == this.targetDatabase.Name))
      {
        ID id = SitecoreEventArgs.GetID(e, 1);
        Assert.IsNotNull(id, "ID is not passed to RemoveVersion handler");
        
        var language = SitecoreEventArgs.GetObject(e, 2) as Language;
        Assert.IsNotNull(language, "Language is not passed to RemoveVersion handler");
        
        var version = SitecoreEventArgs.GetObject(e, 3) as Version;
        Assert.IsNotNull(version, "Version is not passed to RemoveVersion handler");
        
        using (IndexDeleteContext context = this.remoteIndex.CreateDeleteContext())
        {
          this.DeleteVersion(id, language.ToString(), version.ToString(), context);
          context.Commit();
        }
      }
    }

    /// <summary>
    /// Handles the OnUpdateItem event of the Provider control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void Provider_OnUpdateItem(object sender, EventArgs e)
    {
      Assert.ArgumentNotNull(sender, "sender");
      Assert.ArgumentNotNull(e, "e");

      var database = SitecoreEventArgs.GetObject(e, 0) as Database;
      if ((database != null) && (database.Name == this.targetDatabase.Name))
      {
        var item = SitecoreEventArgs.GetObject(e, 1) as Item;
        if (item != null)
        {
          this.UpdateItem(item);
        }
      }
    }

    /// <summary>
    /// Represents the Limit Memory Context.
    /// </summary>
    private class LimitMemoryContext : Switcher<bool, LimitMemoryContext>
    {
      /// <summary>
      /// Initializes a new instance of the <see cref="LimitMemoryContext"/> class.
      /// </summary>
      public LimitMemoryContext()
        : base(true)
      {
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="LimitMemoryContext"/> class.
      /// </summary>
      /// <param name="limit">if set to <c>true</c> [limit].</param>
      public LimitMemoryContext(bool limit)
        : base(limit)
      {
      }
    }
  }
}