<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="RequestDataSummary.ascx.cs" Inherits="GateKeeperAdmin.RequestHistory.RequestDataSummary" %>

<h2><asp:Label ID="sectionLabel" runat="server"></asp:Label></h2>
<ul class="gk-no-bullet">
	<li>CDR ID: <asp:Label ID="Cdrid" runat="server"></asp:Label></li>
	<li>Request ID:  <asp:Label ID="RequestIdLabel" runat="server" EnableViewState="true"></asp:Label></li>								
	<li>Date Received: <asp:Label ID="DateLabel" runat="server" EnableViewState="true"></asp:Label></li>
	<li>CDR Version: <asp:Label ID="CDRVersionLabel" runat="server" EnableViewState="true"></asp:Label></li>
	<li>View: <asp:Label id="XMLLink" Visible="false" runat="server">XML</asp:Label></li>
	<li>View: <asp:HyperLink id="DocumentHistoryLink" Visible="false" runat="server">Document History</asp:HyperLink></li>
</ul>
