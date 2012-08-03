<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ItemDuplicator.aspx.cs" Inherits="Ninemsn.CMSPilot.Web.Ninemsn.ItemGenerator.ItemDuplicator" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    <asp:Literal runat="server" ID="litStatus" EnableViewState="false" /><br />
    <asp:TextBox runat="server" ID="txtPath" /><asp:Button runat="server" Text="Start" ID="btnStart"
            onclick="btnStart_Click" /><asp:Button runat="server" Text="Refresh" ID="btnRefresh" OnClick="btnRefresh_Click" />
    </div>
    </form>
</body>
</html>
