using System;
using Sitecore.Collections;
using Sitecore.Data;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;
using Sitecore.ItemBucket.Kernel.ItemExtensions.Axes;
using Sitecore.ItemBucket.Kernel.Kernel.ItemExtensions.Axes;
using Sitecore.ItemBucket.Kernel.Managers;

namespace Sitecore.ItemBucket.Kernel.Kernel.Tests
{
    public static class Tests
    {
        public static void Run()
        {
           // Test 1 - Get All Items Under Home of Template "Sample Item"
            int hitCount;
            var HomeDescendantsOfTypeSampleItem = BucketManager.Search(Sitecore.Context.ContentDatabase.GetItem("{110D559F-DEA5-42EA-9C1C-8A5DF7E70EF9}"), out hitCount, templates: "{76036F5E-CBCE-46D1-AF0A-4143F9B557AA}");
            //Test 2 - Get All Items Under Tim Folder that have the Title Field Starting with the Word Tim

            var TimDescendantsWithTitleOfTime =
            BucketManager.Search(Sitecore.Context.ContentDatabase.GetItem("{27812FBA-F5E6-41B9-9DDE-4A82AE81496C}"), new SafeDictionary<string> { { "_links", "GUID" } }, out hitCount);
            Assert.AreEqual(hitCount, 3, "");
            //Test 3

            var RepositoryFolderWithNameOfTim =
            BucketManager.Search(Sitecore.Context.ContentDatabase.GetItem("{346C12DF-8FC7-4B97-8570-9D26F78240F2}"), new SafeDictionary<string> { { "_name", "Tim" } }, out hitCount);
            Assert.AreEqual(hitCount, 3, "");

            var gowriQuery = BucketManager.Search(Sitecore.Context.ContentDatabase.GetItem("{346C12DF-8FC7-4B97-8570-9D26F78240F2}"), new SafeDictionary<string> { { "authorid", "6801" } }, out hitCount);

           // Test 4 - Get All Items Under Home of Template "Sample Item"
            var HomeDescendantsOfTypeArticleWithTimContainedWithinIt =
               BucketManager.Search(Sitecore.Context.ContentDatabase.GetItem("{110D559F-DEA5-42EA-9C1C-8A5DF7E70EF9}"), out hitCount,
                                     templates: "{14633DB7-360E-447F-808B-B71128628009}", text: "Tim");

            Assert.AreEqual(hitCount, 3, "");
            //Test 6 - Items under Home that contain the word Tim, sort by Name
            var TimItemsSortedByName =
                BucketManager.Search(Sitecore.Context.ContentDatabase.GetItem("{110D559F-DEA5-42EA-9C1C-8A5DF7E70EF9}"), out hitCount, text: "Tim", sortField: "_name");
            Assert.AreEqual(hitCount, 3, "");
           // Test 7 - Items under Home that contain the word Tim, sort by Name
            var TimItemsOfTypeSampleItemSortedByName =
                BucketManager.Search(Sitecore.Context.ContentDatabase.GetItem("{110D559F-DEA5-42EA-9C1C-8A5DF7E70EF9}"), out hitCount, text: "Tim", sortField: "_name", templates: "{76036F5E-CBCE-46D1-AF0A-4143F9B557AA}");
            Assert.AreEqual(hitCount, 3, "");
            //Test 8 - Items under Home that contain the word Tim, sort by Name
            var ItemsUnderHomeContainingBrisbane =
                BucketManager.Search(Sitecore.Context.ContentDatabase.GetItem("{110D559F-DEA5-42EA-9C1C-8A5DF7E70EF9}"), out hitCount, text: "Brisbane");
            Assert.AreEqual(hitCount, 3, "");
            //Test 10 - Sort by unknown fieldname
            var GetVersion3OfItem =
                BucketManager.Search(Sitecore.Context.ContentDatabase.GetItem("{110D559F-DEA5-42EA-9C1C-8A5DF7E70EF9}"), out hitCount, id: "{344E1BED-B68C-4E13-9689-97BB7797D844}");
            Assert.AreEqual(hitCount, 3, "");
           

            var results = Sitecore.Context.ContentDatabase.GetItem("{44D7C0DC-0151-45D8-BAF5-45EB82B24C5A}").Search(out hitCount, text: "hfosf", numberOfItemsToReturn: 200, pageNumber: 1);
            Assert.AreEqual(hitCount, 3, "");
          
        }

        public static void RunExtensionMethods()
        {
           //Test 1 - Get All Items Under Home of Template "Sample Item"
            var hitsCount = 0;
            var HomeDescendantsOfTypeSampleItem =
                Sitecore.Context.ContentDatabase.GetItem("{110D559F-DEA5-42EA-9C1C-8A5DF7E70EF9}").Search(out hitsCount,
                                     templates: "{76036F5E-CBCE-46D1-AF0A-4143F9B557AA}");

            //Test 2 - Get All Items Under Tim Folder that have the Title Field Starting with the Word Tim

            var TimDescendantsWithTitleOfTime =
            Sitecore.Context.ContentDatabase.GetItem("{346C12DF-8FC7-4B97-8570-9D26F78240F2}").Search(new SafeDictionary<string> { { "title", "Tim" } }, out hitsCount);

            //Test 3

            var RepositoryFolderWithNameOfTim =
            Sitecore.Context.ContentDatabase.GetItem("{346C12DF-8FC7-4B97-8570-9D26F78240F2}").Search(new SafeDictionary<string> { { "_name", "Tim" } },out hitsCount);

            //Test 4 - Get All Items Under Home of Template "Sample Item"
            var HomeDescendantsOfTypeArticleWithTimContainedWithinIt =
               BucketManager.Search(Sitecore.Context.ContentDatabase.GetItem("{110D559F-DEA5-42EA-9C1C-8A5DF7E70EF9}"), out hitsCount,
                                     templates: "{14633DB7-360E-447F-808B-B71128628009}", text: "Tim");

            //Test 6 - Items under Home that contain the word Tim, sort by Name
            var TimItemsSortedByName =
                Sitecore.Context.ContentDatabase.GetItem("{110D559F-DEA5-42EA-9C1C-8A5DF7E70EF9}").Search(out hitsCount, text: "Tim", sortField: "_name");

            //Test 7 - Items under Home that contain the word Tim, sort by Name
            var TimItemsOfTypeSampleItemSortedByName =
                Sitecore.Context.ContentDatabase.GetItem("{110D559F-DEA5-42EA-9C1C-8A5DF7E70EF9}").Search(out hitsCount, text: "Tim", sortField: "_name", templates: "{76036F5E-CBCE-46D1-AF0A-4143F9B557AA}");

            //Test 8 - Items under Home that contain the word Tim, sort by Name
            var ItemsUnderHomeContainingBrisbane =
                Sitecore.Context.ContentDatabase.GetItem("{110D559F-DEA5-42EA-9C1C-8A5DF7E70EF9}").Search(out hitsCount, text: "Brisbane");

            //Test 10 - Sort by unknown fieldname
            var GetVersion3OfItem =
                Sitecore.Context.ContentDatabase.GetItem("{110D559F-DEA5-42EA-9C1C-8A5DF7E70EF9}").Search(out hitsCount, id: "{344E1BED-B68C-4E13-9689-97BB7797D844}");

            //Test 10 - Sort by unknown fieldname
            var ComplexSearch =
                Sitecore.Context.ContentDatabase.GetItem("{110D559F-DEA5-42EA-9C1C-8A5DF7E70EF9}").Search(out hitsCount, 
                                                                                                          startDate:
                                                                                                              "03/12/2012",
                                                                                                          endDate:
                                                                                                              "03/26/2012",
                                                                                                          numberOfItemsToReturn
                                                                                                              : 60,
                                                                                                          language: "en",
                                                                                                          sortField:
                                                                                                              "title");

            //Shanee Tests

            var items1 = BucketManager.Search(Sitecore.Context.ContentDatabase.GetItem("{110D559F-DEA5-42EA-9C1C-8A5DF7E70EF9}"), out hitsCount, numberOfItemsToReturn: 5, pageNumber: 1, templates: "{76036F5E-CBCE-46D1-AF0A-4143F9B557AA}");
            var items2 = BucketManager.Search(Sitecore.Context.ContentDatabase.GetItem("{110D559F-DEA5-42EA-9C1C-8A5DF7E70EF9}"), out hitsCount, templates: "{76036F5E-CBCE-46D1-AF0A-4143F9B557AA}", sortField: "title");
            var items3 = BucketManager.Search(Sitecore.Context.ContentDatabase.GetItem("{110D559F-DEA5-42EA-9C1C-8A5DF7E70EF9}"), new SafeDictionary<string> { { "_name", "Tim" } }, out hitsCount, templates: "{76036F5E-CBCE-46D1-AF0A-4143F9B557AA}", sortField: "_name");

            var items4 = BucketManager.Search(Sitecore.Context.ContentDatabase.GetItem("{110D559F-DEA5-42EA-9C1C-8A5DF7E70EF9}"), out hitsCount, text: "Tim", templates: "{76036F5E-CBCE-46D1-AF0A-4143F9B557AA}", sortField: "_name");

            //New Test Folder Tests

            var hitCount1 = 0;
            //Get me two items that have the word tim in it
            var items5 = BucketManager.Search(Sitecore.Context.ContentDatabase.GetItem("{0412204C-FF6B-47B8-99C4-E471B55BDAB8}"), out hitCount1, numberOfItemsToReturn: 2, pageNumber: 1, text: "Tim");

            //Get me all items that have the word tim in it
            var hitCount2 = 0;
            var items6 = BucketManager.Search(Sitecore.Context.ContentDatabase.GetItem("{0412204C-FF6B-47B8-99C4-E471B55BDAB8}"), out hitCount2, numberOfItemsToReturn: 2000, pageNumber: 1, sortField: "_name", text: "Tim");

            var hitCount3 = 0;
            //Get me all items that are named tim
            var items7 = BucketManager.Search(Sitecore.Context.ContentDatabase.GetItem("{0412204C-FF6B-47B8-99C4-E471B55BDAB8}"), new SafeDictionary<string> { { "_name", "Tim" } }, out hitCount3, sortField: "_name");

            var hitCount4 = 0;
            //Get me all items that have the text tim in it but are of template sample item
            var items8 = BucketManager.Search(Sitecore.Context.ContentDatabase.GetItem("{0412204C-FF6B-47B8-99C4-E471B55BDAB8}"), out hitCount4, text: "Tim", templates: "{76036F5E-CBCE-46D1-AF0A-4143F9B557AA}", sortField: "_name", sortDirection: "asc");

            var hitCount5 = 0;
            //Get me all items that have the text tim in it but are of template sample item
            var items9 = BucketManager.Search(Sitecore.Context.ContentDatabase.GetItem("{0412204C-FF6B-47B8-99C4-E471B55BDAB8}"), out hitCount5, text: "*Tim", sortField: "title");

            var hitCount6 = 0;
            //Get me all items that have the text tim in it but are of template sample item
            var items10 = BucketManager.Search(Sitecore.Context.ContentDatabase.GetItem("{0412204C-FF6B-47B8-99C4-E471B55BDAB8}"), out hitCount6, text: "Tim*", sortField: "title");
  
            var hitCount7 = 0;
            //Get me all items that have the text tim in it but are of template sample item
            var items11 = BucketManager.Search(Sitecore.Context.ContentDatabase.GetItem("{0412204C-FF6B-47B8-99C4-E471B55BDAB8}"), out hitCount7, templates: "{76036F5E-CBCE-46D1-AF0A-4143F9B557AA}|{14633DB7-360E-447F-808B-B71128628009}", sortField: "_name");

            var hitCount8 = 0;
            //Get me all items that have the text tim in it but are of template sample item
            var items112 = BucketManager.Search(Sitecore.Context.ContentDatabase.GetItem("{0412204C-FF6B-47B8-99C4-E471B55BDAB8}"), out hitCount8, templates: "{76036F5E-CBCE-46D1-AF0A-4143F9B557AA}|{14633DB7-360E-447F-808B-B71128628009}", sortField: "_name", sortDirection: "desc", numberOfItemsToReturn: 2, pageNumber:2, language: "de");

            var hitCount9 = 0;
            //Get me all items that have the text tim in it but are of template sample item
            var items113 = BucketManager.Search(Sitecore.Context.ContentDatabase.GetItem("{0412204C-FF6B-47B8-99C4-E471B55BDAB8}"), out hitCount9, templates: "{76036F5E-CBCE-46D1-AF0A-4143F9B557AA}|{14633DB7-360E-447F-808B-B71128628009}", sortField: "_name", sortDirection: "desc", numberOfItemsToReturn: 2, pageNumber: 2, itemName: "Tim");

            var hitCount10 = 0;
            //Get me all items that have the text tim in it but are of template sample item
            var items114 = BucketManager.Search(Sitecore.Context.ContentDatabase.GetItem("{0412204C-FF6B-47B8-99C4-E471B55BDAB8}"), out hitCount10, sortField: "_name", sortDirection: "desc", numberOfItemsToReturn: 20, pageNumber: 1, startDate: "04/30/2012", endDate: "5/05/2012");
       
        }


       public static void RunCreationOfItemsTest()
       {
           using (new BulkUpdateContext())
           {

               BucketManager.CreateBucket(Sitecore.Context.ContentDatabase.GetItem("{44D7C0DC-0151-45D8-BAF5-45EB82B24C5A}"));

               for (int i = 0; i < 10; i++)
               {
                   var currItem = ItemManager.CreateItem(DateTime.Now.ToLongTimeString(),
                                                         Sitecore.Context.ContentDatabase.GetItem(
                                                             "{44D7C0DC-0151-45D8-BAF5-45EB82B24C5A}"),
                                                         new ID("{8B046BC6-4AAF-4652-A1B7-06A6199AEDCE}"));
                   currItem.Editing.BeginEdit();
                   currItem.Versions.AddVersion();
                   currItem.Fields["Title"].Value = "hfosf";
                   currItem.Editing.EndEdit();
               }
               
               BucketManager.CreateBucket(Sitecore.Context.ContentDatabase.GetItem("{44D7C0DC-0151-45D8-BAF5-45EB82B24C5A}"));
           }
       }
    }
}
