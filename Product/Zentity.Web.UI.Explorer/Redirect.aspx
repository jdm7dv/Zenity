<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Redirect.aspx.cs" Inherits="Zentity.Web.UI.Explorer.Redirect" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:Label ID="lblError" Font-Bold="true" Font-Size="Medium" ForeColor="Red" runat="server" Text="An error occurred processing your request. Kindly go back to the previous page and try again." Visible="false"></asp:Label>
    </div>
    </form>
</body>
</html>
