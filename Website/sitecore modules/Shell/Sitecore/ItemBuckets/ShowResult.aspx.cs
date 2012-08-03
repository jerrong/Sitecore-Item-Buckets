using System;

namespace ItemBuckets
{
    using System.Web;
    using Sitecore.StringExtensions;

    using Sitecore.Web;

    public partial class ShowResult : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Id.IsNullOrEmpty())
            {
                Page.Response.Write("<style>.token-input-list-facebook.boxme {background-image: url(/temp/IconCache/" +
                                    Sitecore.Context.ContentDatabase.GetItem(Id).Appearance.Icon +
                                    ");background-size:16px 16px;background-position: 2% 50%;background-repeat: no-repeat;}</style>");
            } 
        }
        protected string Id
        {
            get { return string.IsNullOrEmpty(WebUtil.GetQueryString("id")) ? WebUtil.ExtractUrlParm("id", HttpContext.Current.Request.Url.Query) : WebUtil.ExtractUrlParm("id", HttpContext.Current.Request.Url.Query); }
        }

    }
}