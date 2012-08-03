Sitecore-Item-Buckets
=====================

Manage Large Repositories of Content in Sitecore

Item Buckets is a Lucene.net/SOLR based framework for storing, querying
and scaling the Sitecore CMS to large content repositories.

This module allows users to store large repositories of hidden content a
within the Sitecore content tree that can be fetched efficiently with
a new Search UI.

This allows for very complex queries to run over large repositories of 
content and utilises the Lucene.net/SOLR indexes to return results in
a much faster way that the API/XPath/Sitecore Query etc.

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

* You now can search for content under it (even “non-bucketable” items)
* You can now use the new Bucket API with those items
* All items are now auto-organised for you into logical format
* You can now have items living below other items that act as embedded items
* You have a repository that can store millions of items and not hinder the UI.

Why Bucket?
=====================
The Item Buckets addresses the management of large amounts of items within the 
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
3. Sitecore.ItemBucket.BigData (optional)

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



