using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Text.RegularExpressions;

using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Resources.Media;



namespace afl.sitecore_modules.Shell.ItemGenerator
{

    public partial class ScrapeData : System.Web.UI.Page
    {
        private StringBuilder sb2 = new StringBuilder();
        protected void Page_Load(object sender, EventArgs e)
        {
            WebClient w = new WebClient();
            string s = w.DownloadString("http://en.wikipedia.org/wiki/List_of_current_AFL_team_squads");
          
            StringBuilder sb = new StringBuilder();
            foreach (LinkItem i in LinkFinder.Find(s).GetRange(1000, 50))
            {
                try
                {
                    WebClient ww = new WebClient();
                    string ss = ww.DownloadString("http://en.wikipedia.org" + i.Href);
                    //sb.Append(i.Href);
                    //sb.Append("<br />"); 
                    sb2 = LinkFinder.CallMe(ss);
                }
                catch (Exception exc) { }
            }

            Literal1.Text = sb2.ToString();

        }
    }


}


public struct LinkItem
{
    public string Href;
    public string Text;

    public override string ToString()
    {
        return Href + "\n\t" + Text;
    }
}

static class LinkFinder
{
    private static StringBuilder sb2 = new StringBuilder();
    public static List<LinkItem> Find(string file)
    {
        List<LinkItem> list = new List<LinkItem>();

        // 1.
        // Find all matches in file.
        MatchCollection m1 = Regex.Matches(file, @"(<a.*?>.*?</a>)",
            RegexOptions.Singleline);

        // 2.
        // Loop over each match.
        foreach (Match m in m1)
        {
            string value = m.Groups[1].Value;
            LinkItem i = new LinkItem();

            // 3.
            // Get href attribute.
            Match m2 = Regex.Match(value, @"href=\""(.*?)\""",
            RegexOptions.Singleline);
            if (m2.Success)
            {
                i.Href = m2.Groups[1].Value;
            }

            // 4.
            // Remove inner tags from text.
            string t = Regex.Replace(value, @"\s*<.*?>\s*", "",
            RegexOptions.Singleline);
            i.Text = t;

            list.Add(i);
        }
        return list;
    }

    public static List<LinkItem> FindName(string file)
    {
        List<LinkItem> list = new List<LinkItem>();

        // 1.
        // Find all matches in file.
        MatchCollection m1 = Regex.Matches(file, @"(<table class=""infobox vcard"".*?>.*?</table>)",
            RegexOptions.Singleline);


        // 2.
        // Loop over each match.
        foreach (Match m in m1)
        {
            string value = m.Groups[1].Value;
            //  LinkItem i = new LinkItem();

            // 3.
            // Get href attribute.
            Match m2 = Regex.Match(value, @"<b>Full&nbsp;name</b>",
            RegexOptions.Singleline);
            //if (m2.Success)
            //{
            //    i.Href = m2.Groups[1].Value;
            //}

            //// 4.
            //// Remove inner tags from text.
            //string t = Regex.Replace(value, @"\s*<.*?>\s*", "",
            //RegexOptions.Singleline);
            //i.Text = t;

            //list.Add(i);
        }
        return list;
    }

    public static StringBuilder CallMe(string filePath)
    {
        //HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();

        //// There are various options, set as needed
        //htmlDoc.OptionFixNestedTags = true;

        //// filePath is a path to a file containing the html
        ////htmlDoc.Load(filePath);

        //htmlDoc.LoadHtml((filePath));



        //if (htmlDoc.DocumentNode != null)
        //{
        //    var inputs = from input in htmlDoc.DocumentNode.Descendants("table")
        //                 where input.Attributes["class"].Value == "infobox vcard"
        //                 select input;




        //    foreach (var input in inputs)
        //    {
        //        //input.Attributes["value"].Value;
        //        //foreach (var htmlNode in input.ChildNodes)
        //        //{
        //        try
        //        {
        //            HtmlNode nodes =
        //                input.Descendants().Where(d => d.Attributes["class"].Value == "image").FirstOrDefault();
        //        }
        //        catch (Exception exc)
        //        {
        //        }

        //        try
        //        {
        //            HtmlNode nodes2 =
        //                input.ChildNodes.Where(d => d.Attributes["class"].Value == "image").FirstOrDefault();
        //        }
        //        catch (Exception exc)
        //        {
        //        }

        //        String image = input.SelectSingleNode("//a[@class='image']").ChildNodes[0].GetAttributeValue("src", Boolean.TrueString);



        //        string FullName = "Default Name";
        //        string NickName = "NickName";
        //        string DOB = new DateTime(new Random().Next(1960, 1991), new Random().Next(1, 12),
        //                                  new Random().Next(1, 28)).ToShortDateString();
        //        string HeightWeight = "181 / 89";
        //        string Height = new Random().Next(150, 206).ToString();
        //        string Weight = new Random().Next(65, 130).ToString();
        //        string CurrentClub = GetRandomSentence(1);
        //        string PlayerNumber = new Random().Next(1, 99).ToString();
        //        string Goals = new Random().Next(12, 600).ToString();



        //        try
        //        {

        //             FullName = input.ChildNodes[7].ChildNodes[3].InnerText.Split(new char[] {'('})[0].Trim();
        //             //NickName = input.ChildNodes[9].ChildNodes[3].InnerText.Split(new char[] {'('})[0].Trim();
        //             //DOB = input.ChildNodes[11].ChildNodes[3].InnerText.Split(new char[] {'('})[0].Trim();
        //             //HeightWeight = input.ChildNodes[19].ChildNodes[3].InnerText.Split(new char[] {'('})[0].Trim();
        //             //Height = HeightWeight.Split(new char[] {'/'})[0].Trim();
        //             //Weight = HeightWeight.Split(new char[] {'/'})[1].Trim();
        //             //CurrentClub = input.ChildNodes[23].ChildNodes[3].InnerText.Split(new char[] {'('})[0].Trim();
        //             //PlayerNumber = input.ChildNodes[25].ChildNodes[3].InnerText.Split(new char[] {'('})[0].Trim();
        //             //Goals = new Random().Next(12, 600).ToString();
        //        }
        //        catch(Exception exc)
        //        {
                    

        //        }

        //        //string image = input.ChildNodes[3].GetAttributeValue("src", Boolean.TrueString);

        //        //WebRequest requestPic = WebRequest.Create(image);

        //        //WebResponse responsePic = requestPic.GetResponse();

        //        WebClient client = new WebClient();
        //        Stream stream = client.OpenRead(image);
        //        Image ii = Image.FromStream(stream);
        //        String Random = new Random().Next(0, 100000000).ToString();

        //        ii.Save(@"C:\inetpub\wwwroot\afl\Website\players\" + Random + ".jpg");


        //        //Image webImage = Image.FromStream(responsePic.GetResponseStream());


        //        MediaItem newItem = MediaManager.Creator.CreateFromFile(@"C:\inetpub\wwwroot\afl\Website\players\" + Random + ".jpg", new MediaCreatorOptions()
        //                                                       {

        //                                                           AlternateText = FullName,
        //                                                           Destination = "/sitecore/media library/Images/Players/All/" + Random

        //                                                       });
        //        Item NewPlayer = ItemManager.CreateItem(FullName,
        //                                                Factory.GetDatabase("master").GetItem(
        //                                                    "/sitecore/content/Home/Players"), new ID("{E9D03D8A-ACB6-41FA-8141-B08868A5FE37}"));

        //        NewPlayer.Editing.BeginEdit();
        //        NewPlayer.Fields["Name"].Value = FullName;
        //        NewPlayer.Fields["NickName"].Value = NickName;
        //        NewPlayer.Fields["DOB"].Value = DOB;
        //        NewPlayer.Fields["Height"].Value = Height;
        //        NewPlayer.Fields["Weight"].Value = Weight;
        //        NewPlayer.Fields["Team"].Value = CurrentClub;
        //        NewPlayer.Fields["Number"].Value = PlayerNumber;
        //        NewPlayer.Fields["Goals"].Value = Goals;
        //        ((ImageField)NewPlayer.Fields["Image"]).MediaID = newItem.ID;
        //        NewPlayer.Editing.EndEdit();







        //        //sb2.Append(htmlNode.InnerHtml);
        //        //}
        //        // John
        //    }

        //    //HtmlNode bodyNode = htmlDoc.DocumentNode.SelectSingleNode("//body");

        //    //if (bodyNode != null)
        //    //{
        //    //    // Do something with bodyNode
        //    //}
        //}

        //return sb2;

        return new StringBuilder();


    }
    private static Random random;

    public static string GetRandomSentence(int wordCount)
    {
        random = new Random();
        string[] words = { "Swans", "Lions", "Cats", "Bombers", "Lizards", "Collingwood", "Carlton", "Essendon", "Fremantle", "Geelong", "Hawthorne", "Melbourne", "St Kilda", "Richmond" };

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




}