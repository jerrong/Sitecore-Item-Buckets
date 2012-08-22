using System;
using System.Web;
using System.Web.UI;
using Sitecore.Web;
using Sitecore.StringExtensions;
using Sitecore.Web.UI.WebControls;

namespace ItemBuckets
{
    public partial class IFrameField : System.Web.UI.Page
    {

        private string _ID = string.IsNullOrEmpty(WebUtil.GetQueryString("id"))
                               ? WebUtil.ExtractUrlParm("id", HttpContext.Current.Request.Url.Query)
                               : WebUtil.ExtractUrlParm("id", HttpContext.Current.Request.Url.Query);

        protected string Id
        {
            get { return _ID; }
            set { _ID = value; }
        }

        public bool HasLock
        {
            get { return Sitecore.Context.ContentDatabase.GetItem(Id).Locking.HasLock() || Sitecore.Context.User.IsAdministrator; }
           
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Id.IsNullOrEmpty())
            {
                Page.Response.Write(
                    "<style>.token-input-list-facebook.boxme {background-image: url(/temp/IconCache/" +
                    Sitecore.Context.ContentDatabase.GetItem(Id).Appearance.Icon +
                    ");background-size:16px 16px;background-position: 2% 50%;background-repeat: no-repeat;}</style>");


            }

            DataBind();
        }
    }
}