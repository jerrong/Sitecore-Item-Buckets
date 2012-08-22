<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AddTab.aspx.cs" Inherits="Sitecore.ItemBucket.sitecore_modules.Shell.Sitecore.ItemBuckets.AddTab" %>
<%@ Import Namespace="Sitecore" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <% if (UIUtil.IsIE())
       { %>
    <script src="/sitecore/shell/controls/InternetExplorer.js" type="text/javascript"></script>
    <% }
       else
       { %>
    <script src="/sitecore/shell/controls/Gecko.js" type="text/javascript"></script>
    <% } %>
    <script src="/sitecore/shell/controls/Sitecore.js" type="text/javascript"></script>
    <link rel="stylesheet" href="/sitecore%20modules/Shell/Sitecore/ItemBuckets/styles/token-input.css" type="text/css" />

    <script type="text/javascript" src="/sitecore%20modules/Shell/Sitecore/ItemBuckets/Scripts/jquery.min.js"></script>    
 
    <!--[if IE]>
     <link href="/sitecore%20modules/Shell/Sitecore/ItemBuckets/Styles/ItemBucket.ie.css" rel="stylesheet" type="text/css" />
    <![endif]-->
    
    <script type="text/javascript">

        var one = $('.scEditorTabIcon');
        var two = $('.scEditorTabHeaderActive');
        var three = $('.scRibbonEditorTabActive');


        $(".scRibbonEditorTabNormal", parent.document.body).click(function () {
            // Handler for .ready() called.

            if ($('.scEditorTabHeaderActive', parent.document.body)[0].firstChild.src.indexOf("/temp/IconCache/Applications/16x16/view_add.png") != -1) {
                 scForm.getParentForm().postRequest('', '', '', 'contenteditor:launchblanktab(url=' + '' + ')');
             }
            

        });
       // if ($('.scEditorTabHeaderActive').first().src.indexOf("/temp/IconCache/Applications/16x16/view_add.png") != -1) {
        //   alert('Hello');
      // }
       
    </script>
   
</head>
<body>
    <form id="form1" runat="server">
    <div>
  


    </div>
    </form>
</body>
</html>
