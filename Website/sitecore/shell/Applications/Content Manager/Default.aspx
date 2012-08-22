<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Sitecore.Shell.Applications.ContentManager.ContentEditorPage" %>

<%@ Import Namespace="Sitecore" %>
<%@ Import Namespace="Sitecore.Globalization" %>
<%@ Register TagPrefix="sc" Namespace="Sitecore.Web.UI.HtmlControls" Assembly="Sitecore.Kernel" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<asp:placeholder id="DocumentType" runat="server" />
<html>
<head runat="server">
       <script type="text/JavaScript" language="javascript" src="/sitecore/shell/Controls/Lib/jQuery/jquery.noconflict.js"></script>

    <!--[if !IE]> -->
    <script type="text/javascript" src="/sitecore%20modules/Shell/Sitecore/ItemBuckets/Scripts/fullscreen.js"></script>  
    <!--<![endif]-->
    <asp:placeholder id="BrowserTitle" runat="server" />
    <sc:Stylesheet runat="server" Src="Content Manager.css" DeviceDependant="true" />
    <sc:Stylesheet runat="server" Src="Ribbon.css" DeviceDependant="true" />
    <asp:placeholder id="Stylesheets" runat="server" />
    <script type="text/JavaScript" language="javascript" src="/sitecore/shell/controls/SitecoreObjects.js"></script>
    <script type="text/JavaScript" language="javascript" src="/sitecore/shell/controls/SitecoreKeyboard.js"></script>
    <script type="text/JavaScript" language="javascript" src="/sitecore/shell/controls/SitecoreModifiedHandling.js"></script>
    <script type="text/JavaScript" language="javascript" src="/sitecore/shell/controls/SitecoreVSplitter.js"></script>
    <script type="text/JavaScript" language="javascript" src="/sitecore/shell/controls/SitecoreWindow.js"></script>
    <script type="text/JavaScript" language="javascript" src="/sitecore/shell/Applications/Content Manager/Content Editor.js"></script>
    <script type="text/JavaScript" language="javascript" src="/sitecore/shell/controls/TreeviewEx/TreeviewEx.js"></script>
    <script type="text/JavaScript" language="javascript" src="/sitecore/shell/Controls/Lib/Scriptaculous/Scriptaculous.js"></script>
    <script type="text/JavaScript" language="javascript" src="/sitecore/shell/Controls/Lib/Scriptaculous/Effects.js"></script>
    <script type="text/JavaScript" language="javascript" src="/sitecore/shell/Controls/Lib/Scriptaculous/DragDrop.js"></script>

    <script type="text/JavaScript" language="javascript" src="/sitecore/shell/Applications/Analytics/Personalization/Carousel/jquery.jcarousel.min.js"></script>
    <link href="/sitecore/shell/Applications/Analytics/Personalization/Carousel/skin.css"
        rel="Stylesheet"></link>
    <script type="text/JavaScript" language="javascript" src="/sitecore/shell/Applications/Analytics/Personalization/Tooltip.js"></script>
    <style type="text/css">
        .scRibbonNavigator
        {
            margin-left: 44px;
        }
    </style>
</head>
<body runat="server" id="Body" class="scWindowBorder1" onmousedown="javascript:scWin.mouseDown(this, event)"
    onmousemove="javascript:scWin.mouseMove(this, event)" onmouseup="javascript:scWin.mouseUp(this, event)">
    <form id="ContentEditorForm" runat="server">
    <sc:CodeBeside runat="server" Type="Sitecore.ItemBucket.Kernel.Kernel.Forms.ExtendedContentEditorForm,Sitecore.ItemBucket.Kernel" />
    <sc:DataContext runat="server" ID="ContentEditorDataContext" />
    <sc:RegisterKey runat="server" KeyCode="120" Click="system:publish" />
    <asp:PlaceHolder ID="scLanguage" runat="server" />
    <input type="hidden" id="scActiveRibbonStrip" name="scActiveRibbonStrip" />
    <input type="hidden" id="scEditorTabs" name="scEditorTabs" />
    <input type="hidden" id="scActiveEditorTab" name="scActiveEditorTab" />
    <input type="hidden" id="scPostAction" name="scPostAction" />
    <input type="hidden" id="scShowEditor" name="scShowEditor" />
    <input type="hidden" id="scSections" name="scSections" />
    <div id="outline" class="scOutline" style="display: none">
    </div>
    <span id="scPostActionText" style="display: none">
        <sc:Literal Text="The main window could not be updated due to the current browser security settings. You must click the Refresh button yourself to view the changes."
            runat="server" />
    </span>
    <asp:ScriptManager ID="ScriptManager1" runat="server" />
    <telerik:RadSpell ID="RadSpell" runat="server" />
    <telerik:RadToolTipManager runat="server" ID="ToolTipManager" class="scRadTooltipManager" />
    <iframe id="overlayWindow" src="/sitecore/shell/Controls/Rich Text Editor/EditorWindow.aspx"
        style="position: absolute; width: 100%; height: 100%; top: 0px; left: 0px; right: 0px;
        bottom: 0px; display: none; z-index: 999; border: none" frameborder="0" allowtransparency="allowtransparency">
    </iframe>
    <a id="SystemMenu" runat="server" href="#" class="scSystemMenu" onclick="javascript:return scForm.postEvent(this, event, 'SystemMenu_Click')"
        ondblclick="javascript:return scForm.invoke('contenteditor:close')"></a>
    <table class="scPanel scWindowBorder2" cellpadding="0" cellspacing="0" border="0"
        onactivate="javascript:scWin.activate(this, event)">
        <tr>
            <td class="scCaption scWindowHandle scDockTop" ondblclick="javascript:scWin.maximizeWindow()"
                valign="top">
                <div id="CaptionTopLine">
                    <img src="/sitecore/images/blank.gif" width="1" height="1" alt="" /></div>
                <div class="scSystemButtons">
                    <asp:PlaceHolder ID="WindowButtonsPlaceholder" runat="server" />
                </div>
                <div id="RibbonPanel" onclick="javascript:scContent.onEditorClick(this, event)">
                    <asp:PlaceHolder ID="RibbonPlaceholder" runat="server" />
                </div>
            </td>
        </tr>
        <tr>
            <td id="MainPanel" class="scDockMain" onclick="javascript:scContent.onEditorClick(this, event)"
                valign="top">
                <table class="scPanel" cellpadding="0" cellspacing="0" border="0">
                    <tr>
                        <td class="scWindowBorder3" width="2">
                            <img src="/sitecore/images/blank.gif" class="scWindowBorder4" alt="" />
                        </td>
                        <td id="ContentTreePanel" valign="top" width="250" style="display: none; height: 100%">
                            <div class="scFixSize scKeepFixSize" style="height: 100%">
                                <table width="100%" height="100%" cellpadding="0" cellspacing="0" border="0">
                                    <tr id="SearchPanel" runat="server">
                                        <td style="background: #e9e9e9; border-bottom: 1px solid #4c4c4c">
                                            <table width="100%" cellpadding="0" cellspacing="0" border="0">
                                                <tr>
                                                    <td style="width: 100%; overflow: hidden; padding: 1px 0px 1px 2px" runat="server">
                                                        <input id="TreeSearch" class="scSearchInput scIgnoreModified" style="color: #999999"
                                                            value="<%=Translate.Text(Texts.SEARCH) %>" onkeydown="javascript:if(event.keyCode==13)return scForm.postEvent(this,event,'TreeSearch_Click',true)"
                                                            onfocus="javascript:scContent.watermarkFocus(this,event)" onblur="javascript:scContent.watermarkBlur(this,event)" />
                                                    </td>
                                                    <td style="padding: 0px 0px 0px 0px">
                                                        <a href="#" class="scSearchButton" onclick="javascript:return scForm.postEvent(this,event,'TreeSearch_Click',true)">
                                                            <sc:ThemedImage runat="server" Src="Applications/16x16/view.png" Width="16" Height="16"
                                                                Margin="0px 1px 0px 1px" />
                                                        </a>
                                                    </td>
                                                    <td style="padding: 0px 2px 0px 0px">
                                                        <a href="#" class="scSearchOptionsButton" onclick="javascript:Element.toggle('TreeSearchOptions'); if (typeof(scGeckoRelayout) != 'undefined') scGeckoRelayout();">
                                                            <sc:ThemedImage runat="server" Src="Images/Down.png" Width="16" Height="16" Margin="0px 1px 0px 1px" />
                                                        </a>
                                                    </td>
                                                </tr>
                                            </table>
                                            <table id="TreeSearchOptions" width="100%" cellpadding="0" cellspacing="0" border="0"
                                                style="display: none; table-layout: fixed; padding: 4px 2px 2px 2px; border-top: 1px solid #414851">
                                                <tr>
                                                    <td>
                                                        <table id="SearchOptionsList" width="100%" cellpadding="0" cellspacing="0" border="0"
                                                            onkeydown="javascript:if(event.keyCode==13)return scForm.postEvent(this,event,'TreeSearch_Click',true)">
                                                            <col align="right" />
                                                            <col width="100%" />
                                                            <tr>
                                                                <td class="scSearchOptionsNameContainer">
                                                                    <a id="SearchOptionsControl0" href="#" class="scSearchOptionName" onclick="javascript:return scForm.postEvent(this,event,'TreeSearchOptionName_Click',true)">
                                                                        <sc:Literal Text="Name:" runat="server" />
                                                                    </a>
                                                                </td>
                                                                <td class="scSearchOptionsValueContainer">
                                                                    <input id="SearchOptionsValue0" class="scSearchOptionsInput scIgnoreModified" /><input
                                                                        id="SearchOptionsName0" type="hidden" value="_name" />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td valign="top" style="padding: 12px 0px 0px 0px">
                                                                    <a href="#" class="scSearchAddCriteria" onclick="javascript:return scContent.addSearchCriteria(this,event)">
                                                                        <sc:ThemedImage Src="Applications/16x16/view_add.png" Width="16" Height="16" runat="server"
                                                                            Align="absmiddle" Style="margin: 0px 4px 0px 0px" />
                                                                        <sc:Literal Text="Add Criteria" runat="server" />
                                                                    </a>
                                                                </td>
                                                                <td valign="top" class="scSearchOptionsValueContainer scSearchAddCriteriaInput" runat="server">
                                                                    <input id="SearchOptionsAddCriteria" class="scSearchOptionsInput scIgnoreModified"
                                                                        style="color: #999999" value="<%=Translate.Text(Texts.FIELD1) %>" onkeydown="javascript:if(event.keyCode==13)return scContent.addSearchCriteria(this,event)"
                                                                        onfocus="javascript:scContent.watermarkFocus(this,event)" onblur="javascript:scContent.watermarkBlur(this,event)" />
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                    <tr id="SearchResultHolder" style="display: none">
                                        <td height="100%">
                                            <table width="100%" height="100%" cellpadding="0" cellspacing="0" border="0" style="table-layout: fixed">
                                                <tr>
                                                    <td class="scSearchHeader">
                                                        <a href="#" style="float: right" onclick="javascript:return scContent.closeSearch(this,event)">
                                                            <sc:ThemedImage runat="server" Src="Images/close.png" Width="16" Height="16" Margin="0px 4px 0px 0px"
                                                                RollOver="true" />
                                                        </a>
                                                        <sc:ThemedImage runat="server" Src="Applications/16x16/view.png" Width="16" Height="16"
                                                            Align="absmiddle" />
                                                        <span id="SearchResultsHeader" />
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td height="100%">
                                                        <div id="SearchResult" style="background: white; height: 100%; overflow: auto">
                                                        </div>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                    <tr id="ContentTreeHolder">
                                        <td valign="top" style="height: 100%">
                                            <div class="scContentTreeContainer" style="height: 100%">
                                                <asp:PlaceHolder ID="ContentTreePlaceholder" runat="server" />
                                            </div>
                                        </td>
                                    </tr>
                                </table>
                            </div>
                            <!-- /scFixSize -->
                        </td>
                        <td id="ContentTreeSplitter" class="scSplitter" width="7" onmousedown="javascript:return scContent.mouseDown(this, event)"
                            onmousemove="javascript:return scContent.mouseMove(this, event)" onmouseup="javascript:return scContent.mouseUp(this, event)">
                            <img src="/sitecore/images/blank.gif" class="scSplitterFill" alt="" />
                        </td>
                        <td id="MainContent" valign="top" width="100%" height="100%" style="height: 100%">
                            <sc:Border ID="ContentEditor" runat="server" Class="scEditor" />
                        </td>
                        <td class="scWindowBorder3" width="2">
                            <img src="/sitecore/images/blank.gif" class="scWindowBorder4" alt="" />
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
        <asp:PlaceHolder ID="Pager" runat="server" />
        <tr runat="server" id="BottomBorder">
            <td height="2" class="scWindowBorder3">
                <img src="/sitecore/images/blank.gif" height="2" width="1" alt="" border="0" />
            </td>
        </tr>
    </table>
    <sc:KeyMap runat="server" />
    </form>
</body>
</html>
