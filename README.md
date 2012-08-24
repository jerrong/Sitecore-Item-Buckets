Sitecore-Item-Buckets
=====================

This module allows you to manage large repositories of content in Sitecore

Item Buckets is a Lucene.net/SOLR based framework for storing, querying
and scaling the Sitecore CMS to large content repositories.

This module allows users to store large repositories of hidden content
within the Sitecore content tree that can be fetched efficiently with
a new Search UI.

This allows for very complex queries to run over large repositories of 
content and utilises the Lucene.net/SOLR indexes to return results in
a much faster way than the API/XPath/Sitecore Query etc.

Idea
=====================
A bucket is a container within the Content Tree that will store content
items. What differs this from a normal container is that all items 
stored within the container will be hidden and have a new Content 
Search tab to be able to look up the items within the container instead
of using the tree to navigate and select items. In addition to this, 
items in a bucket, will auto organise your content into folders and 
completely remove the parent to child relationship between items.

Having an item as a bucket brings many advantages including:

* You now can search for content under it (even "un-bucketable" items)
* You can now use the new Bucket API with those items
* All items are now auto-organised for you into logical format
* You can now have items living below other items that act as embedded items
* You have a repository that can store millions of items and not hinder the UI.

Why Bucket?
=====================
Sitecore Item Buckets addresses the management of large amounts of items within the 
content tree and being able to retrieve and work with them in a speedy and 
efficient manner. To decide if you should turn an item into a bucket, and 
in-turn, hiding all its descendants, is as simple as asking yourself if you
care about the structure of the data that lives under the bucket. For example,
if you had a Product Repository, Movie Repository or Tag Repository within 
the content tree, you would most likely want to just dump them all into a 
folder and when you want to work with a particular product, movie or tag, 
you would simply search for it and open it.

Item Buckets allows for connections to be made through semantics, not hierarchy. 
For example, traditionally for creating products you would put categories 
into the content tree and then place product items under those categories.
With item buckets, you can place all products in the one repository but tag
each product with what category they belong to.


Design
=====================

Item Buckets has been designed for flexibility of architecture and
extensiblity and is broken up into 3 main libraries:

1. Sitecore.ItemBucket.Kernel (mandatory)
2. Sitecore.ItemBucket.UI (optional)
3. Sitecore.ItemBucket.BigData (mandatory)

This abstraction allows you to run Item Buckets in the way that you want
for different envionrments:

* Single Server
* Multi Server
* Dedicated Indexing Server
* Dedicated Query Server
* Distributed SOLR Servers
* No UI, just API
* UI and API
* API and BigData
* API, BigData and UI


Build
-------------

Item Buckets comes with build scripts for installing into different
environments and you can configure your indexes to run locally, 
distributed, or replicated with SOLR.

You can install through Nuget.org or through the standard Sitecore 
package.

This source is targeted towards the current recommended release of Sitecore i.e. Sitecore 
6.5 Update 5.

Check the releases folder for a packaged build in other versions.

Installation
-------------

1) Sitecore.ItemBuckets as a Nuget Package

Either through Visual Studio 2010 or from Nuget.Org, search for Sitecore Item Buckets. 
It will provide you with 3 links. 1 is for the Kernel and the other is for the UI.
You will want to attach the Kernel to all projects that will be using the Item 
Buckets Search API. You will want to attach the UI package to any of your projects
that require the Item Buckets Search UI e.g. your Sitecore Website Project.
There is a 3rd optional package with is Sitecore Item Buckets Big Data. 

This can be attached to your projects if you plan on having millions of 
content items in the content tree. It also brings with it other features such as 
“in-memory” indexes and “remote-indexes”. By installing with the Nuget Package, 
you will be notified through Visual Studio when there is an update. You will have
the choice to accept the updates or continue on with the version you have.

``` Powershell
PM> Install-Package Sitecore.ItemBuckets
PM> Install-Package Sitecore.ItemBuckets.Client
``` 

2) Sitecore ItemBuckets as a Sitecore Package
You will get 3 packages for the Sitecore Item Buckets. The first is the Kernel. 
This needs to be installed in all environments. The second is the Sitecore Item
Buckets UI. This will only need to be installed in environments that need the 
UI e.g. Authoring, Development. Once you have downloaded the modules as a 
Sitecore Package, simply install through the Installation Wizard within the 
Desktop. It will ask you to override some content items. Please select “Override”
or “Merge” if you have already made customisations to the template that is being
modified.

A 3rd optional package can be installed which is the Sitecore Item Buckets Big
Data functionality. This can be installed alongside the Sitecore Item Buckets
Kernel.

The installation will fire some post installation steps and will run a Smart
Publish as well as rebuilding the new index that will be installed for you.
You will need to restart the client to see your changes.

You will also receive 3 update packages. You will need to install these as well as
these are the items associated with the package.

UI
=====================

The UI was designed for best use in Google Chrome. IE and Firefox are supported, but best experience
will be in Google Chrome.

API Overview
--------

You can use Lambda and Linq expressions to chain searches together.

Here's an example:

``` C#
var movies  = new BucketQuery().WhereContentContains("Dark")
                               .WhereTemplateIs("{D3335D0B-D84D-46AF-C620-A67A6022AB3F}")
                               .WhereLanguageIs(Sitecore.Context.Language)
                               .WhereTaggedWith("Tag ID for Tim Burton")
                               .WhereTaggedWith("Tag ID for Johnny Depp")
                               .WhereTaggedWith("Tag ID for Helen Bohnam-Carter")
                               .Starting(DateTime.Now.AddYears(-12))
                               .Ending(DateTime.Now)
                               .SortBy("_name")
                               .Page(1, 200, out hitCount);

```

You can use the BucketManager to run queries:

``` C#
var HomeDescendantsOfTypeSampleItem = BucketManager.Search(Sitecore.Context.Item, 
                                                           templates: "{76036F5E-CBCE-46D1-AF0A-4143F9B557AA}");
```

You can use the Extension Methods on the Item object to run queries:

``` C#
var HomeDescendantsOfTypeSampleItem = Sitecore.Context.Item.Search(templates: "{76036F5E-CBCE-46D1-AF0A-4143F9B557AA}");
```


Scalability Extensions
--------

Sitecore Item Buckets makes extensions to the following parts of the Sitecore CMS

* New Link Database Implementation
* New Scalable Workbox
* New Scalable Datasource Implementation
* New Publishing Pipelines
* many more...

Developer Extensions
--------
<strong>Extensions</strong>

<table>
  <tr>
    <th>Namespace</th><th>Class</th><th>Description</th>
  </tr>
  <tr>
    <td>Sitecore.ItemBucket.Kernel.ItemExtensions.Axes</td><td>BucketItemAxes</td><td>If you add this to your using statements in your CS file you will get extension method replacements for using GetChildren(), Children and Axes methods.</td>
  </tr>
  <tr>
    <td>Sitecore.ItemBucket.Kernel.ItemExtensions.Axes</td><td>ItemExtensions</td><td>This will allow you to have new extension methods and properties on an item object to simply be able to run queries on an item like so item.Search(“”) or item.FullSearch(“”)</td>
  </tr>
   <tr>
    <td>Sitecore.ItemBucket.Kernel.Managers</td><td>BucketManager</td><td>This is your main entry point for working with the Bucket containers. It contains methods such as IsBucket(), IsBucketItem(), GetParentBucket() etc. This also allows you to run Searches as well.</td>
  </tr>
   <tr>
    <td>Sitecore.ItemBuckets.Kernel.Search.Query</td><td>BucketQuery</td><td>The entry point for running Lambda or Linq expressions that get converted into Lucene queries.</td>
  </tr>
   <tr>
    <td>Sitecore.ItemBucket.Kernel.Util</td><td>SitecoreItem</td><td>To keep memory to a minimum, all searches will return a SitecoreItem which is a stripped-back representation of the Item class.</td>
  </tr>
   <tr>
    <td>Sitecore.ItemBucket.Kernel.Kernel.Hooks</td><td>QueryWarmUp</td><td>An abstract class that allows you to specify warm-up queries that run when Sitecore is initializing. This is useful to run common queries so that they are cached when requested another time. This sacrifices startup time for operation performance.</td>
  </tr>
   <tr>
    <td>Sitecore.ItemBucket.Kernel.Presentation</td><td>BucketPresentationExtensions</td><td>A helper class for converting a string datasource to a Bucket Query.</td>
  </tr>
</table>

<strong>Interfaces</strong>

<table>
  <tr>
    <th>Namespace</th><th>Interface</th><th>Description</th>
  </tr>
  <tr>
    <td>Sitecore.ItemBucket.Kernel.FieldTypes</td><td>IDataSource</td><td>Implement this interface if you would like to be able to have list field types within your Sitecore template take advantage of populating itself from a lucene query.</td>
  </tr>
  <tr>
    <td>Example of IDataSource Implementation</td><td>BucketListQuery</td>
    <td>
    
    public class BucketListQuery : IDataSource
    {
        public Item[] ListQuery(Item itm)
        {
            return itm.Children.ToArray();
        }
    }
    </td>
    
  </tr>
   <tr>
    <td>Sitecore.ItemBucket.Kernel.Kernel.Interfaces</td><td>IBucketController</td><td>If you would like to build your own UI layer that can send the Bucket Handler a request and receive back a list of items, implement this interface.</td>
  </tr>
   <tr>
    <td>Sitecore.ItemBucket.Kernel.Kernel.Interfaces</td><td>IBucketSearchQuery</td><td>There are many filters that come with the Item Buckets e.g. Author, Start Date, Text, Tags etc. If you would like to implement a new Filter then implement this interface.</td>
  </tr>
   <tr>
    <td>Sitecore.ItemBucket.Kernel.Kernel.Interfaces</td><td>ITag</td><td>A Tag Repository works with the ITag Interface. If you have Tags that are pulled from external systems then you will need to implement this interface to be able to use these tags to tag items and then search by them as well.</td>
  </tr>
   <tr>
    <td>Sitecore.ItemBucket.Kernel.Kernel.Interfaces</td><td>ITagRepository</td><td>Item Buckets comes with an implementation of a Tag Repository using Items within the content tree. If you have an existing Tag Repository and would like to use this to search for tagged content within Sitecore then you will need to implement a ITagRepository.</td>
  </tr>
   <tr>
    <td>Sitecore.ItemBucket.Kernel.Kernel.Interfaces</td><td>ISearchOperation</td><td>If you would like to introduce new actions to do on a list of search results, then you will need to implement this interface. For example, if you want to search for all items in the content tree that had $name in any of the fields and replace them, then you would could implement a new ISearchOperation so that authors could do this.</td>
  </tr>
    <tr>
    <td>Sitecore.ItemBucket.Kernel.Search</td><td>IFacet</td><td>Item Buckets ships with 5 different types of faceting.

1.                               Templates
2.	Fields
3.	Dates
4.	Locations
5.	Authors

If you would like to introduce your own faceting categories then you only need to implement the IFacet interface.
</td>
  </tr>
   <tr>
    <td>Sitecore.ItemBucket.Kernel.Search</td><td>ISearchDropDown</td><td>When running a search you will see a dropdown that is shown from the textbox where you enter your text. If you would like to add your own, firstly you will need to implement this interface. Secondly, you will need to add an item into the content tree (/sitecore/system/Modules/Item Buckets/Settings/Search Box Dropdown) to register this class so that it will show up in the drop down menu.</td>
  </tr>
</table>

Working with a Datasource in Code
------------------
A lot of websites are very similar and have similar requirements. Below is a list of example queries to retrieve items for common web controls.

Example 1: Side Menu (Get all descendants of type Template “Site Section”.)
Code
``` C#
//This will use the data source that is specified on a control to query the buckets for items.
var items = BucketManager.ParseDataSourceQueryForItems(((Sublayout) this.Parent).DataSource, 
                                                         Sitecore.Context.Item, 0, 20);

//This will use the string that is specified in the method\ query the buckets for items.
var items = BucketManager.ParseDataSourceQueryForItems((“<Insert Query Here>”, 
                                                        Sitecore.Context.Item, 0, 20);
```
Importing content into Buckets
-------------------

A common requirement with masses of content is to import content from many different sources. To be able to do this efficiently, you can use the following code snippet to import content into a bucket. It will disable subsystems from firing.
``` C#
  Item item = database.GetItem("/sitecore/content");
  using (new BucketImportContext(item))
  {
     //Disable History Engine
     //Disable Publishing Queue
     //Smart Links Database Rebuild
                    
     BucketManager.CreateBucket(item, (itm => BucketManager.AddSearchTabToItem(item)));
  }
```

Roadmap
-------------------

* Integration with Hadoop for Search Analysis of Log Files
* Custom Facet Controls e.g. Sliders, Colour Pickers, Calendars
* PerFieldWrapperAnalysis
* More Like This..
* Sounds Like...
* Greater Than or Less Than filters
* Stemming 
* Saved Searches e.g. "Items created today"
* Redesign of Index Rebuilding Wizard
* Bucket Treelist Field Type