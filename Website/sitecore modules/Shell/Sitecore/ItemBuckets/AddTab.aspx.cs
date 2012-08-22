using System;
using System.Web;
using Sitecore.Globalization;
using Sitecore.Resources;
using Sitecore.Text;
using Sitecore.Web;
using Sitecore.Web.UI.Framework.Scripts;
using Sitecore.Web.UI.Sheer;

namespace Sitecore.ItemBucket.sitecore_modules.Shell.Sitecore.ItemBuckets
{
    public partial class AddTab : System.Web.UI.Page
    {
       
        protected void Page_Load(object sender, EventArgs e)
        {
            //var urlString = new UrlString("/sitecore%20modules/Shell/Sitecore/ItemBuckets/ShowResult.aspx");

            //SheerResponse.Eval(new ShowEditorTab { Command = "contenteditor:launchtab", Header = "Another Search", Icon = Images.GetThemedImageSource("Applications/16x16/text_view.png"), Url = urlString.ToString(), Id = Id, Closeable = true, Activate = true }.ToString());

        }
    }
}