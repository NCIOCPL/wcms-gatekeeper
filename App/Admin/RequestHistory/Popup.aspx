<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Popup.aspx.cs" Inherits="GateKeeperAdmin.RequestHistory.Popup" %>


<html>
<head id="Head1" runat="server">
    <title>Document type counts</title>
    <link id="lnkStyleSheets" href="../stylesheets/gatekeeper.css" rel="stylesheet" type="text/css"  runat="server"/>
</head>
<body>
	<div class="gk-bannerwrap">
	    <div class="gk-banner-left"><a href="http://www.cancer.gov/"><asp:Image ID="imgNci" runat="server" ImageUrl="~/images/banner-nci.gif" AlternateText="National Cancer Institute" Width="290" Height="36"  BorderWidth="0"  ToolTip="National Cancer Institute" /></a></div>
    </div>
    <form id="form1" runat="server">
	<div id="idDataTabled" class="data-tabled" runat="server">
        <table cellpadding="4" cellspacing="0" style="margin-top: 10px">
        <tr>
          <th scope="col"><asp:Label ID="lblEntry" runat="server">Document Type</asp:Label></th>
          <th scope="col"><asp:Label ID="lblHistory" runat="server">Document Count</asp:Label></th>
        </tr>
    <asp:Repeater ID="rptTypeCount" runat="server">
	    <ItemTemplate>
                  <tr>
                    <td class="left"><%# Eval("Type")%></td>
                    <td><%# Eval("Value")%></td>
                  </tr>
	    </ItemTemplate>
	    <AlternatingItemTemplate>
                  <tr class="altrow">
                    <td class="left"><%# Eval("Type")%></td>
                    <td><%# Eval("Value")%></td>
                  </tr>
	    </AlternatingItemTemplate>
    </asp:Repeater>
    </table>
        <a id="aClose" href="#" onclick="javascript:window.close();">Close window</a>
    </div>
    </form>
</body>
</html>
