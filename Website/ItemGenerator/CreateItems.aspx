<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CreateItems.aspx.cs" Inherits="BulkMigrate.CreateItems" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <div>
            <label for="NumberOfItems">
                Number of Items to Create:</label>
            <asp:TextBox ID="NumberOfItems" runat="server"></asp:TextBox>
        </div>
        <div>
            <label for="NumberOfItemsPerFolder">
                Number of Items Per Folder:</label>
            <asp:TextBox ID="NumberOfItemsPerFolder" runat="server"></asp:TextBox>
        </div>
         <div>
            <label for="StartPath">
                Start Path (e.g. /sitecore/content/Home/xxx):</label>
            <asp:TextBox ID="StartPath" runat="server"></asp:TextBox>
        </div>
        <div>
            <label for="StartPath">
                Template ID (e.g. {FBD90082-A3D8-4057-9BA4-FAB5ADFD6C37} for Cleo Slideshow):</label>
            <asp:TextBox ID="DataTemplateId" runat="server" Text="{FBD90082-A3D8-4057-9BA4-FAB5ADFD6C37}"></asp:TextBox>
        </div>
        <asp:Button ID="Run" runat="server" Text="Run" onclick="Run_Click" />
    </div>
    </form>
</body>
</html>
