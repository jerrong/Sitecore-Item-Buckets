namespace Sitecore.ItemBucket.Kernel.FieldTypes
{
    using System.Web;
    using System.Web.UI;
    using Sitecore.Diagnostics;
    using Sitecore.Pipelines;
    using Sitecore.StringExtensions;

    /// <summary>
    /// Inject Scripts when initialising the Application
    /// </summary>
    public class InjectScripts
    {
        public void Process(PipelineArgs args)
        {
            if (Context.ClientPage.IsEvent)
            {
                return;
            }

            var context = HttpContext.Current;
            if (context == null)
            {
                return;
            }

            var page = context.Handler as Page;
            if (page == null)
            {
                return;
            }

            Assert.IsNotNull(page.Header, "Content Editor <head> tag is missing runat='value'");

            var scripts = new[]
            {
              "/sitecore/shell/Controls/Lib/Scriptaculous/Scriptaculous.js", 
              "/sitecore/shell/Controls/Lib/Scriptaculous/builder.js",
              "/sitecore/shell/Controls/Lib/Scriptaculous/effects.js", 
              "/sitecore/shell/Controls/Lib/Scriptaculous/dragdrop.js", 
              "/sitecore/shell/Controls/Lib/Scriptaculous/slider.js" 
            };

            foreach (var script in scripts)
            {
                page.Header.Controls.Add(new LiteralControl("<script type='text/javascript' language='javascript' src='{0}'></script>".FormatWith(script)));
            }
        }
    }
}
