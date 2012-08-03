<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Results.aspx.cs" Inherits="ItemBuckets.Results" CodePage="65001" %>
<%@ Register TagPrefix="sc" Namespace="Sitecore.Web.UI.WebControls" Assembly="Sitecore.Kernel" %>
<%@ OutputCache Location="None" VaryByParam="none" %>
<%@ Import Namespace="ItemBucket.Kernel.Kernel.Util" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html lang="en" xml:lang="en" xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Welcome to Sitecore</title>
    <meta http-equiv="Content-Type" content="text/html; charset=UTF-8" />
    <meta name="CODE_LANGUAGE" content="C#" />
    <meta name="vs_defaultClientScript" content="JavaScript" />
    <meta name="vs_targetSchema" content="http://schemas.microsoft.com/intellisense/ie5" />

    <link rel="stylesheet" href="http://loopj.com/jquery-tokeninput/styles/token-input.css"
        type="text/css" />
    <link rel="stylesheet" href="http://ajax.googleapis.com/ajax/libs/jqueryui/1.8.16/themes/base/jquery-ui.css"
        type="text/css" media="all" />
    <script type="text/javascript" src="https://ajax.googleapis.com/ajax/libs/jquery/1.5.1/jquery.min.js"></script>
    <script src="http://ajax.googleapis.com/ajax/libs/jqueryui/1.8.16/jquery-ui.min.js"
        type="text/javascript"></script>
    <link href="/Styles/ItemBucket.css" rel="stylesheet" type="text/css" />
    <script src="/Scripts/ItemBucket.js" type="text/javascript"></script>
</head>
<body>
    <form id="mainform" method="post" runat="server">
    <div id="MainPanel">
        <%--  <sc:Placeholder Key="main" runat="server" />--%>
        <div class="content">
            <div class="box">
                <div id="ui_element" class="sb_wrapper">
                    <p>
                        <span class="sb_down"></span>
                        <ul class="token-input-list-facebook boxme">
                            <li class="token-input-input-token-facebook">
                                <input type="text" autocomplete="off" class="addition" style="outline: medium none;
                                    width: 100%;" id="token-input-demo-input-local" />
                            </li>
                        </ul>
                        <input class="sb_input" id="typesearch" type="text" style="display: none;" />
                        <input class="sb_search" type="submit" value="" />
                    </p>
                    <ul class="sb_dropdown" style="display: none;">
                        <li class="sb_filter  msg_head">Suggested Keywords</li>
                        <div class="msg_body">
                            <li><a class="command" href="#">Balh</a></li>
                            <li><a class="command" href="#">Balh</a></li>
                            <li><a class="command" href="#">DDD</a></li>
                            <li><a class="command" href="#">Hello</a></li>
                            <li><a class="command" href="#">Me</a></li>
                            <li><a class="command" href="#">Tim</a></li>
                        </div>
                        <li class="sb_filter  msg_head1">Filter your search</li>
                        <div class="msg_body1">
                            <li>
                                <input type="checkbox" /><label for="all"><strong>All Categories</strong></label></li>
                            <li>
                                <input type="checkbox" /><label for="Automotive">Automotive</label></li>
                            <li>
                                <input type="checkbox" /><label for="Baby">Baby</label></li>
                            <li>
                                <input type="checkbox" /><label for="Beauty">Beautys</label></li>
                            <li>
                                <input type="checkbox" /><label for="Books">Books</label></li>
                            <li>
                                <input type="checkbox" /><label for="Cell">Cell Service</label></li>
                        </div>
                        <li class="sb_filter recent msg_head2">Recent Results</li>
                        <div class="msg_body2">
                            <li><a href="#"><span style="background: url('/images/pin.png') no-repeat left center;
                                padding: 0px 18px;">Name</span></a></li>
                            <li><a href="#"><span style="background: url('/images/pin-on.png') no-repeat left center;
                                padding: 0px 18px;">Name</span></a></li><li><a href="#"><span style="background: url('/images/pin.png') no-repeat left center;
                                    padding: 0px 18px;">Name</span></a></li><li><a href="#"><span style="background: url('/images/pin.png') no-repeat left center;
                                        padding: 0px 18px;">Name</span></a></li>
                        </div>
                        <li class="sb_filter commands msg_head3">Search Commands</li>
                        <div class="msg_body3">
                            <li><a class="command" href="#">text:</a>
                                <%--                   <ul>
                                    <li>page up/down - previous/next month </li>
                                    <li>ctrl+page up/down - previous/next year </li>
                                    <li>ctrl+home - current month or open when closed </li>
                                    <li>ctrl+left/right - previous/next day </li>
                                    <li>ctrl+up/down - previous/next week </li>
                                    <li>enter - accept the selected date </li>
                                    <li>ctrl+end - close and erase the date </li>
                                    <li>escape - close the datepicker without selection </li>
                                </ul>--%>
                            </li>
                            <li><a class="command" href="#">tag:</a></li>
                            <li><a class="command" href="#">filetype:</a></li>
                            <li><a class="command" href="#">template:</a></li>
                            <li><a class="command" href="#">start:</a></li>
                            <li><a class="command" href="#">end:</a></li>
                        </div>
                        <li class="sb_filter commands msg_head4">Top Searches</li>
                        <div class="msg_body4">
                            <li><a class="topsearch" href="#">Something</a></li>
                            <li><a class="topsearch" href="#">Else</a></li>
                            <li><a class="topsearch" href="#">And</a></li>
                            <li><a class="topsearch" href="#">Lorem</a></li>
                            <li><a class="topsearch" href="#">Ipsum</a></li>
                            <li><a class="topsearch" href="#">Optum</a></li>
                        </div>
                    </ul>
                </div>
            </div>
        </div>
    </div>
    <asp:TextBox ID="TextBox1" runat="server"></asp:TextBox>
    <asp:Button ID="Button1" runat="server" Text="Button" onclick="Button1_Click" />
    <div class="results">
             <asp:ListView runat="server" ID="SearchResults">
            <LayoutTemplate>
                <ul class="menu">
                    <asp:PlaceHolder ID="itemPlaceholder" runat="server" />
                </ul>
            </LayoutTemplate>
            <ItemTemplate>
                <li>
                    <%# ((SitecoreItem)Container.DataItem).Name %></li>
            </ItemTemplate>
        </asp:ListView>
    </div>
    </form>
</body>
</html>
