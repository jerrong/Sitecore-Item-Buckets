namespace Sitecore.BigData
{
    public static class Consts
    {
        public static string[] StopWords
        {
            get
            {
                return Sitecore.Configuration.Factory.GetConfigNode("settings/setting[@name=\"stopwords\"]").InnerText.Split(',');
            }
        }
    }
}
