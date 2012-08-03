Sitecore-Item-Buckets
=====================

Manage Large Repositories of Content in Sitecore

Item Buckets is a Lucene.net/SOLR based framework for storing, querying
and scaling the Sitecore CMS to large content repositories.

This module allows users to store large repositories of hidden content 
within the Sitecore content tree that can be fetched efficiently with
a new Search UI.

This allows for very complex queries to run over large repositories of 
content and utilises the Lucene.net/SOLR indexes to return results in
a much faster way that the API/XPath/Sitecore Query etc.

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

