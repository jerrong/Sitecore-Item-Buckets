using System;
using System.Text;
using System.Threading;
using Sitecore.ItemBucket.Kernel.Util;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;

namespace BulkMigrate
{
    public partial class CreateItems : System.Web.UI.Page
    {

        private int NumberOfItemsGlobal = 0;
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Run_Click(object sender, EventArgs e)
        {
            NumberOfItemsGlobal = Int32.Parse(NumberOfItems.Text);
            using (new Sitecore.Data.BulkUpdateContext())
            {
                StartJob();
            }
        }

        public void StartJob()
        {
            var jobOptions = new Sitecore.Jobs.JobOptions(
              "Import Items",
              "BulkImport",
              Sitecore.Context.Site.Name,
              this,
              "ProcessMethod",
              new object[] { "hi" });

            Sitecore.Jobs.JobManager.Start(jobOptions);
        }
        private Random random;


        public string GetRandomSentence(int wordCount)
        {
            this.random = new Random();
            string[] words = { "an", "afl", "or", "grand", "gran", "final", "is", "a", "Australian", "Football", "league", "nsw", "Victoria", "Melbourne", "Crows", "Lions", "Brisbane", "Collingwood", "its", "own", "Seven", "Fox", "nine", "Venue", "Player", "goal", "number", "winner", "ball", "kicked", "Aboriginal", "young", "2011", "2012", "2010", "AFL", "Sitecore", "points", "field", "match", "sport", "draft", "mark", "tickets" };

            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < wordCount; i++)
            {
                // Select a random word from the array
                builder.Append(words[random.Next(words.Length)]).Append(" ");
            }

            string sentence = builder.ToString().Trim();

            // Set the first letter of the first word in the sentenece to uppercase
            sentence = char.ToUpper(sentence[0]) + sentence.Substring(1);

            builder = new StringBuilder();
            builder.Append(sentence);

            return builder.ToString();
        }


        public string GetRandomGuid(int wordCount)
        {
            this.random = new Random();
            string[] words = { "{DC710F10-8A87-47A1-A764-31B0E9070E04}","{DDF80BA4-5F2B-4948-A806-2D4D292C5D46}", "{238ABBE6-56DB-47EF-B277-956B7E27AE4B}", "{57DF0953-87D9-48AF-ADBE-406588C3FC23}"};

            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < wordCount; i++)
            {
                // Select a random word from the array
                builder.Append(words[random.Next(words.Length)]).Append(" ");
            }

            string sentence = builder.ToString().Trim();

            // Set the first letter of the first word in the sentenece to uppercase
            sentence = char.ToUpper(sentence[0]) + sentence.Substring(1);

            builder = new StringBuilder();
            builder.Append(sentence);

            return builder.ToString();
        }

        public static Item GetDateFolderDestination(Item originalDestination, Database databaseName)
        {
            var dateFolder = DateTime.Now.Year + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day + "/" + DateTime.Now.Hour + "/" + DateTime.Now.Minute;
            var newPath = originalDestination.Paths.FullPath + "/" + dateFolder;
            if (databaseName.GetItem(newPath) != null)
            {
                return databaseName.GetItem(newPath);
            }
            TemplateItem templateItem = databaseName.Templates[new TemplateID(Config.ContainerTemplateId)];
            return databaseName.CreateItemPath(newPath, templateItem, templateItem);
        }



        //Create Year Folder
                //Month
                        //30 Days
                            //24 Hours
                                   //60 Minutes - > 60 Folders
                                        //1 -> 30 Children
                                    




        public void ProcessMethod(string message)
        {
            int RunningFolderCount = Int32.Parse(NumberOfItemsPerFolder.Text);
            String NewStartPath = StartPath.Text;
            int Level = 6;
            Database db = Factory.GetDatabase("master");
            int FolderCOunt = 0;
            for (int i = 0; i < NumberOfItemsGlobal; i++)
            {

                if (FolderCOunt == RunningFolderCount)
                {

                    FolderCOunt = 0;
                    //from 1 to 60 create a folder and create 30 items under all of them.
                    
                    Thread.Sleep(6000);
                }

                Item ii = ItemManager.CreateItem(GetRandomSentence(4),
                                                 GetDateFolderDestination(db.GetItem(NewStartPath), db),
                                                 new ID(this.DataTemplateId.Text)); //The template ID that you want to create millions of items of (this one is the included sample template ID)


                //Edit Item
                Item iii = ii.Versions.AddVersion();
                iii.Editing.BeginEdit();
                iii.Fields["Title"].Value = GetRandomSentence(4);
                iii.Fields["Text"].Value = GetRandomSentence(50);

              

               

                iii.Editing.EndEdit();
                FolderCOunt++;

            }


            //CreateFolderStructure(Factory.GetDatabase("master").GetItem(NewStartPath), 6,
            //  Int32.Parse(NumberOfItemsPerFolder.Text));


            //RecurceFolders(Factory.GetDatabase("master").GetItem(NewStartPath), 6, Int32.Parse(NumberOfItemsPerFolder.Text));
        }
        

        //private void CreateItem(string NewStartPath)
        //{
        //    Item ii = ItemManager.CreateItem(Guid.NewGuid().ToString(), Factory.GetDatabase("master").GetItem(NewStartPath),
        //                                     new ID("{1C033850-5303-4D82-BE20-FC5BBFFE1AD7}"));

        //    //Edit Item
        //    Item iii = ii.Versions.AddVersion();
        //    iii.Editing.BeginEdit();
        //    iii.Fields["Title"].Value = "Test Title";
        //    iii.Editing.EndEdit();
        //}

        //private void RecurceFolders(Item ParentItem, int Level, int RunningFolderCount)
        //{
        //    Item NewFolder = ItemManager.CreateItem(Guid.NewGuid().ToString(),
        //                               Factory.GetDatabase("master").GetItem(ParentItem.Paths.FullPath),
        //                               new ID("{A87A00B1-E6DB-45AB-8B54-636FEC3B5523}"));
        //    //Set the Item Creation Path
        //    String NewStartPath = NewFolder.Paths.FullPath;
        //    for (int i = 0; i < RunningFolderCount; i++)
        //    {
        //        //Create a Folder

        //        //Create Item
        //        Item ii = ItemManager.CreateItem(Guid.NewGuid().ToString(), Factory.GetDatabase("master").GetItem(NewStartPath),
        //                              new ID("{1C033850-5303-4D82-BE20-FC5BBFFE1AD7}"));

        //        //Edit Item
        //        Item iii = ii.Versions.AddVersion();
        //        iii.Editing.BeginEdit();
        //        iii.Fields["Title"].Value = "Test Title";
        //        iii.Editing.EndEdit();


        //        RunningFolderCount--;
        //        NumberOfItemsGlobal--;
        //        if (RunningFolderCount == 0 && NumberOfItemsGlobal >= 0)
        //        {
        //            NewStartPath = NewFolder.Paths.FullPath;
        //            RunningFolderCount = Int32.Parse(NumberOfItemsPerFolder.Text);
        //            RecurceFolders(NewFolder, Level - 1, RunningFolderCount);
        //        }
        //    }
        //}

        //public void Execute(int iterations)
        //{
        //    for (var i = 0; i < iterations; i++)
        //    {
        //        Thread.Sleep(200);
        //        if (Sitecore.Context.Job != null)
        //        {
        //            Sitecore.Context.Job.Status.Processed = i;
        //            Sitecore.Context.Job.Status.Messages.Add("Processed item " + i);
        //        }
        //    }
        //}
    }
}