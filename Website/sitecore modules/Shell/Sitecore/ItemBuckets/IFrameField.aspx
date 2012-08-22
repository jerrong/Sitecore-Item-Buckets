<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="IFrameField.aspx.cs" Inherits="ItemBuckets.IFrameField" %>

<%@ Register TagPrefix="sc" Namespace="Sitecore.Web.UI.WebControls" Assembly="Sitecore.Kernel" %>
<%@ OutputCache Location="None" VaryByParam="none" %>
<%@ Import Namespace="Sitecore" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html lang="en" xml:lang="en" xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Welcome to Sitecore</title>
    <meta http-equiv="Content-Type" content="text/html; charset=UTF-8" />
    <meta name="CODE_LANGUAGE" content="C#" />
    <meta name="vs_defaultClientScript" content="JavaScript" />
    <meta name="vs_targetSchema" content="http://schemas.microsoft.com/intellisense/ie5" />
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
    <link rel="stylesheet" href="/sitecore%20modules/Shell/Sitecore/ItemBuckets/styles/jquery-ui.css" type="text/css" media="all" />
    <script type="text/javascript" src="/sitecore%20modules/Shell/Sitecore/ItemBuckets/Scripts/jquery.min.js"></script>    
    <script src="/sitecore%20modules/Shell/Sitecore/ItemBuckets/Scripts/jquery-ui.min.js" type="text/javascript"></script>   
    <link href="/sitecore%20modules/Shell/Sitecore/ItemBuckets/Styles/ItemBucket.css" rel="stylesheet" type="text/css" />
    <link href="/sitecore%20modules/Shell/Sitecore/ItemBuckets/Styles/SiteNew.css" rel="stylesheet" type="text/css" />
    <link href="/sitecore%20modules/Shell/Sitecore/ItemBuckets/Styles/ListNew.css" rel="stylesheet" type="text/css" />
    <link href="/sitecore%20modules/Shell/Sitecore/ItemBuckets/Styles/core.css" rel="stylesheet" type="text/css" />
    <script src="/sitecore%20modules/Shell/Sitecore/ItemBuckets/Scripts/date.js" type="text/javascript"></script>
    <script src="/sitecore%20modules/Shell/Sitecore/ItemBuckets/Scripts/IFrameItemBucket.js" type="text/javascript"></script>
    <script src="/sitecore%20modules/Shell/Sitecore/ItemBuckets/Scripts/jquery.tabSlideOut.v1.3.js" type="text/javascript"></script>
    <script src="/sitecore%20modules/Shell/Sitecore/ItemBuckets/Scripts/jquery.clippy.min.js" type="text/javascript"></script>
  

    <!--[if IE]>
     <link href="/sitecore%20modules/Shell/Sitecore/ItemBuckets/Styles/ItemBucket.ie.css" rel="stylesheet" type="text/css" />
    <![endif]-->
   


</head>
<body>
    <form id="mainform" method="post" runat="server">
    <div id="MainPanel">
        <div class="left">
            <div class="navAlpha slide-out-div">
            </div>
        </div>
        <div id="navBeta" class="slide-out-div2">
        </div>
        <div class="content">
            <div class="box">
                <div id="ui_element" class="sb_wrapper">
                   <span id="views"><a id="list" class="active list"></a><a id="grid" class="grid"></a></span>
                   <div class="sb_search_container">
                        <span class="sb_down">&nbsp;</span>
                        <ul class="token-input-list-facebook boxme">
                            <li class="token-input-input-token-facebook">
                                <input type="text" autocomplete="off" class="addition" style="outline: medium none;
                                    width: 200px;" id="token-input-demo-input-local" <%# HasLock ? "" : "disabled" %>/>
                            </li>                            
                        </ul>
                        <div id="slider" style="position: absolute; bottom: 20px; width: 180px; right: 60px;"></div>
                        <input class="sb_input" id="typesearch" type="text" style="display: none;" />
                        <input class="sb_clear" type="button" value="" tabindex="-1"/>   
                        <input class="sb_search" type="button" value="" />                                                                 
                    </div>
                    
                    <div class="sb_dropdown" style="display: none;">

                       <%-- <div class="sb_filter recent msg_head2">Recent Results</div>
                        <div class="msg_body2">
                        </div>
                        <div class="sb_filter commands msg_head3">Search Commands</div>
                        <div class="msg_body3">
                        </div>
                        <div class="sb_filter commands msg_head4">Recently Modified</div>
                        <div class="msg_body4">
                        </div>--%>
                    </div>
                </div>
            </div>
            <ul class="pageSection" <%# HasLock ? "" : "disabled" %>>
            <h2>Selected Item List</h2>
                <div id="savedIds" style="height: 50px;">
                </div>
                <div id="loadingSection" style="height: 50px;">
                </div>
                <div id="results" style="padding-top: 20px;padding-left: 94px;">
                </div>
            </ul>
        </div>
    </div>
    </form>
</body>
</html>
