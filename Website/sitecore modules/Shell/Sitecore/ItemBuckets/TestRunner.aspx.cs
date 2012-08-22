using System;
using System.Diagnostics;
using System.Linq;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Sitecore.Collections;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.ItemBucket.Kernel.ItemExtensions.Axes;
using Sitecore.ItemBucket.Kernel.Kernel.ItemExtensions.Axes;
using Sitecore.ItemBucket.Kernel.Kernel.Util;
using Sitecore.ItemBucket.Kernel.Managers;


namespace Sitecore.ItemBucket.UI
{
    public partial class TestRunner : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //Burn Up
            var itemCreated = BurnUp();
            //Bucket Manager Tests
            long runningCounter = default(long);
            long runningExtensionCounter = default(long);
            for (int i = 0; i < 5; i++)
            {
                runningCounter += Run(itemCreated);
            }
            //Item Extension Tests
            for (int i = 0; i < 5; i++)
            {
                runningExtensionCounter += RunExtensionMethods(itemCreated); ;
            }
            
            //Creating Item Tests
            //RunCreationOfItemsTest();
            TearDown(itemCreated);
            Page.Response.Write("Average BucketManager Run : " + runningCounter / 5);
            Page.Response.Write("Average Extension Method Run : " + runningExtensionCounter / 5);
        }

        private void TearDown(Item itm)
        {
           // ItemManager.DeleteItem(itm);
        }

        private Item BurnUp()
        {
            var itm = Factory.GetDatabase("master").GetItem("/sitecore/content/Test Stub");
            if (itm.IsNotNull())
            {
                return itm;
                //ItemManager.DeleteItem(itm);
            }
            return RunCreationOfItemsTest();
        }

        private long Run(Item itm)
        {

            var stopWatch = new Stopwatch();
            stopWatch.Start();
            // Test 1 - Get All Items Under Home of Template "Sample Item"
            int hitCount;
            var itemsss = BucketManager.Search(new TermQuery(new Term("_name", "Tim")), out hitCount);

           
            var HomeDescendantsOfTypeSampleItem = BucketManager.Search(Factory.GetDatabase("master").GetItem(itm.ID.ToString()), out hitCount, templates: "{76036F5E-CBCE-46D1-AF0A-4143F9B557AA}");
           
            Page.Response.Write("1:  Get All Items Under Test Stub of Template Sample Item " + HomeDescendantsOfTypeSampleItem.Count() + " Items, Expecting 10</br>");
            //Test 2 - Get All Items Under Tim Folder that have the Title Field Starting with the Word Tim
            var TimDescendantsWithTitleOfTime =
            BucketManager.Search(Factory.GetDatabase("master").GetItem(itm.ID.ToString()), new SafeDictionary<string> { { "title", "Tim" } }, out hitCount);
            Page.Response.Write("2:  Get All Items Under Test Stub where the Title is Tim" + TimDescendantsWithTitleOfTime.Count() + " Items, Expecting 10</br>");
            //Test 3

            var RepositoryFolderWithNameOfTim =
            BucketManager.Search(Factory.GetDatabase("master").GetItem(itm.ID.ToString()), new SafeDictionary<string> { { "_name", "Tim" } }, out hitCount);
            Page.Response.Write("3:  Get All Items Under Test Stub where the Name of the Item is Tim" + RepositoryFolderWithNameOfTim.Count() + " Items, Expecting 10</br>");
            // Test 4 - Get All Items Under Home of Template "Sample Item"
            var HomeDescendantsOfTypeArticleWithTimContainedWithinIt =
               BucketManager.Search(Factory.GetDatabase("master").GetItem(itm.ID.ToString()), out hitCount,
                                     templates: "{76036F5E-CBCE-46D1-AF0A-4143F9B557AA}", text: "Tim");

            Page.Response.Write("4:  Get All Items Under Test Stub where the Item contains the text Tim and is based off the Sample item Template" + HomeDescendantsOfTypeArticleWithTimContainedWithinIt.Count() + " Items, Expecting 10</br>");

            //Test 6 - Items under Home that contain the word Tim, sort by Name
            var TimItemsSortedByName =
                BucketManager.Search(Factory.GetDatabase("master").GetItem(itm.ID.ToString()), out hitCount, text: "Tim", sortField: "_name");
            Page.Response.Write(TimItemsSortedByName.Count() + "</br>");
            // Test 7 - Items under Home that contain the word Tim, sort by Name
            var TimItemsOfTypeSampleItemSortedByName =
                BucketManager.Search(Factory.GetDatabase("master").GetItem(itm.ID.ToString()), out hitCount, text: "Tim", sortField: "_name", templates: "{76036F5E-CBCE-46D1-AF0A-4143F9B557AA}");
            Page.Response.Write(TimItemsOfTypeSampleItemSortedByName.Count() + "</br>");
            //Test 8 - Items under Home that contain the word Tim, sort by Name
            var ItemsUnderHomeContainingBrisbane =
                BucketManager.Search(Factory.GetDatabase("master").GetItem(itm.ID.ToString()), out hitCount, text: "Brisbane");
            Page.Response.Write(ItemsUnderHomeContainingBrisbane.Count() + "</br>");
            //Test 10 - Sort by unknown fieldname
            var GetVersion3OfItem =
                BucketManager.Search(Factory.GetDatabase("master").GetItem(itm.ID.ToString()), out hitCount, id: "{344E1BED-B68C-4E13-9689-97BB7797D844}");

            Page.Response.Write(GetVersion3OfItem.Count() + "</br>");

            var results = Factory.GetDatabase("master").GetItem(itm.ID.ToString()).Search(out hitCount, text: "hfosf", numberOfItemsToReturn: 200, pageNumber: 1);
            Page.Response.Write(results.Count() + "</br>");
            stopWatch.Stop();
            return stopWatch.ElapsedMilliseconds;

        }

        private long RunExtensionMethods(Item itm)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            //Test 1 - Get All Items Under Home of Template "Sample Item"
            var hitsCount = 0;
            var HomeDescendantsOfTypeSampleItem =
                Factory.GetDatabase("master").GetItem(itm.ID.ToString()).Search(out hitsCount,
                                     templates: "{76036F5E-CBCE-46D1-AF0A-4143F9B557AA}");
            Page.Response.Write(HomeDescendantsOfTypeSampleItem.Count() + "</br>");
            //Test 2 - Get All Items Under Tim Folder that have the Title Field Starting with the Word Tim

            var TimDescendantsWithTitleOfTime =
            Factory.GetDatabase("master").GetItem(itm.ID.ToString()).Search(new SafeDictionary<string> { { "title", "Tim" } }, out hitsCount);
            Page.Response.Write(TimDescendantsWithTitleOfTime.Count() + "</br>");
            //Test 3

            var RepositoryFolderWithNameOfTim =
            Factory.GetDatabase("master").GetItem(itm.ID.ToString()).Search(new SafeDictionary<string> { { "_name", "Tim" } }, out hitsCount);
            Page.Response.Write(RepositoryFolderWithNameOfTim.Count() + "</br>");
            //Test 4 - Get All Items Under Home of Template "Sample Item"
            var HomeDescendantsOfTypeArticleWithTimContainedWithinIt =
               BucketManager.Search(Factory.GetDatabase("master").GetItem(itm.ID.ToString()), out hitsCount,
                                     templates: "{14633DB7-360E-447F-808B-B71128628009}", text: "Tim");
            Page.Response.Write(HomeDescendantsOfTypeArticleWithTimContainedWithinIt.Count() + "</br>");
            //Test 6 - Items under Home that contain the word Tim, sort by Name
            var TimItemsSortedByName =
                Factory.GetDatabase("master").GetItem(itm.ID.ToString()).Search(out hitsCount, text: "Tim", sortField: "_name");
            Page.Response.Write(TimItemsSortedByName.Count() + "</br>");
            //Test 7 - Items under Home that contain the word Tim, sort by Name
            var TimItemsOfTypeSampleItemSortedByName =
                Factory.GetDatabase("master").GetItem(itm.ID.ToString()).Search(out hitsCount, text: "Tim", sortField: "_name", templates: "{76036F5E-CBCE-46D1-AF0A-4143F9B557AA}");
            Page.Response.Write(TimItemsOfTypeSampleItemSortedByName.Count() + "</br>");
            //Test 8 - Items under Home that contain the word Tim, sort by Name
            var ItemsUnderHomeContainingBrisbane =
                Factory.GetDatabase("master").GetItem(itm.ID.ToString()).Search(out hitsCount, text: "Brisbane");
            Page.Response.Write(ItemsUnderHomeContainingBrisbane.Count() + "</br>");
            //Test 10 - Sort by unknown fieldname
            var GetVersion3OfItem =
                Factory.GetDatabase("master").GetItem(itm.ID.ToString()).Search(out hitsCount, id: "{344E1BED-B68C-4E13-9689-97BB7797D844}");
            Page.Response.Write(GetVersion3OfItem.Count() + "</br>");
            //Test 10 - Sort by unknown fieldname
            var ComplexSearch =
                Factory.GetDatabase("master").GetItem(itm.ID.ToString()).Search(out hitsCount,
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
            Page.Response.Write(ComplexSearch.Count() + "</br>");
            var items1 = BucketManager.Search(Factory.GetDatabase("master").GetItem(itm.ID.ToString()), out hitsCount, numberOfItemsToReturn: 5, pageNumber: 1, templates: "{76036F5E-CBCE-46D1-AF0A-4143F9B557AA}");
            Page.Response.Write(items1.Count() + "</br>");
            var items2 = BucketManager.Search(Factory.GetDatabase("master").GetItem(itm.ID.ToString()), out hitsCount, templates: "{76036F5E-CBCE-46D1-AF0A-4143F9B557AA}", sortField: "title");
            Page.Response.Write(items2.Count() + "</br>");
            var items3 = BucketManager.Search(Factory.GetDatabase("master").GetItem(itm.ID.ToString()), new SafeDictionary<string> { { "_name", "Tim" } }, out hitsCount, templates: "{76036F5E-CBCE-46D1-AF0A-4143F9B557AA}", sortField: "_name");
            Page.Response.Write(items3.Count() + "</br>");
            var items4 = BucketManager.Search(Factory.GetDatabase("master").GetItem(itm.ID.ToString()), out hitsCount, text: "Tim", templates: "{76036F5E-CBCE-46D1-AF0A-4143F9B557AA}", sortField: "_name");
            Page.Response.Write(items4.Count() + "</br>");

            //New Test Folder Tests

            var hitCount1 = 0;
            //Get me two items that have the word tim in it
            var items5 = BucketManager.Search(Factory.GetDatabase("master").GetItem(itm.ID.ToString()), out hitCount1, numberOfItemsToReturn: 2, pageNumber: 1, text: "Tim");
            Page.Response.Write(items5.Count() + "</br>");
            //Get me all items that have the word tim in it
            var hitCount2 = 0;
            var items6 = BucketManager.Search(Factory.GetDatabase("master").GetItem(itm.ID.ToString()), out hitCount2, numberOfItemsToReturn: 2000, pageNumber: 1, sortField: "_name", text: "Tim");
            Page.Response.Write(items6.Count() + "</br>");
            var hitCount3 = 0;
            //Get me all items that are named tim
            var items7 = BucketManager.Search(Factory.GetDatabase("master").GetItem(itm.ID.ToString()), new SafeDictionary<string> { { "_name", "Tim" } }, out hitCount3, sortField: "_name");
            Page.Response.Write(items7.Count() + "</br>");
            var hitCount4 = 0;
            //Get me all items that have the text tim in it but are of template sample item
            var items8 = BucketManager.Search(Factory.GetDatabase("master").GetItem(itm.ID.ToString()), out hitCount4, text: "Tim", templates: "{76036F5E-CBCE-46D1-AF0A-4143F9B557AA}", sortField: "_name", sortDirection: "asc");
            Page.Response.Write(items8.Count() + "</br>");
            var hitCount5 = 0;
            //Get me all items that have the text tim in it but are of template sample item
            var items9 = BucketManager.Search(Factory.GetDatabase("master").GetItem(itm.ID.ToString()), out hitCount5, text: "*Tim", sortField: "title");
            Page.Response.Write(items9.Count() + "</br>");
            var hitCount6 = 0;
            //Get me all items that have the text tim in it but are of template sample item
            var items10 = BucketManager.Search(Factory.GetDatabase("master").GetItem(itm.ID.ToString()), out hitCount6, text: "Tim*", sortField: "title");
            Page.Response.Write(items10.Count() + "</br>");
            var hitCount7 = 0;
            //Get me all items that have the text tim in it but are of template sample item
            var items11 = BucketManager.Search(Factory.GetDatabase("master").GetItem(itm.ID.ToString()), out hitCount7, templates: "{76036F5E-CBCE-46D1-AF0A-4143F9B557AA}|{14633DB7-360E-447F-808B-B71128628009}", sortField: "_name");
            Page.Response.Write(items11.Count() + "</br>");
            var hitCount8 = 0;
            //Get me all items that have the text tim in it but are of template sample item
            var items112 = BucketManager.Search(Factory.GetDatabase("master").GetItem(itm.ID.ToString()), out hitCount8, templates: "{76036F5E-CBCE-46D1-AF0A-4143F9B557AA}|{14633DB7-360E-447F-808B-B71128628009}", sortField: "_name", sortDirection: "desc", numberOfItemsToReturn: 2, pageNumber: 2, language: "de");
            Page.Response.Write(items112.Count() + "</br>");
            var hitCount9 = 0;
            //Get me all items that have the text tim in it but are of template sample item
            var items113 = BucketManager.Search(Factory.GetDatabase("master").GetItem(itm.ID.ToString()), out hitCount9, templates: "{76036F5E-CBCE-46D1-AF0A-4143F9B557AA}|{14633DB7-360E-447F-808B-B71128628009}", sortField: "_name", sortDirection: "desc", numberOfItemsToReturn: 2, pageNumber: 2, itemName: "Tim");
            Page.Response.Write(items113.Count() + "</br>");
            var hitCount10 = 0;
            //Get me all items that have the text tim in it but are of template sample item
            var items114 = BucketManager.Search(Factory.GetDatabase("master").GetItem(itm.ID.ToString()), out hitCount10, sortField: "_name", sortDirection: "desc", numberOfItemsToReturn: 20, pageNumber: 1, startDate: "04/30/2012", endDate: "5/05/2012");
            Page.Response.Write(items114.Count() + "</br>");
            stopWatch.Stop();

            //All the movies where Tim is the director or the producer, but only where the move was in the 90's and starred JOhhny Depp and Helen Bonhem-Carter in the same movie
            int queryHits = 0;
            var res = new BucketQuery().WhereFieldValueIs("Director", "Tim")
                                       .WhereFieldValueIs("Producer", "Tim")
                                       .Starting(new DateTime(1990, 1, 1))
                                       .Ending(new DateTime(1999, 12, 31))
                                       .WhereFieldValueIs("Actors", "Johnny Depp")
                                       .WhereFieldValueIs("Actors", "Helen Bonham Carter").Run(itm, 200);

            return stopWatch.ElapsedMilliseconds;
        }


        private Item RunCreationOfItemsTest()
        {
            

                var itemToReturn = Factory.GetDatabase("master").GetItem("{0DE95AE4-41AB-4D01-9EB0-67441B7C2450}").Add("Test Stub",
                                                                                                    new TemplateID(
                                                                                                        TemplateIDs.
                                                                                                            Folder));

                for (int i = 0; i < 10; i++)
                {
                    var currItem = ItemManager.CreateItem("Tim " + DateTime.Now.ToLongTimeString(), itemToReturn,
                                                          new ID("{76036F5E-CBCE-46D1-AF0A-4143F9B557AA}"));
                    currItem.Editing.BeginEdit();
                    currItem.Versions.AddVersion();
                    currItem.Fields["Title"].Value = "Tim";
                    currItem.Editing.EndEdit();
                }

                BucketManager.CreateBucket(itemToReturn);
                return itemToReturn;
            


        }

    }
}