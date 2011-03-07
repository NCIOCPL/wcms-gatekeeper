<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RequestBatchHistory.aspx.cs" MasterPageFile="~/AdminToolMasterPage.Master" Inherits="GateKeeperAdmin.RequestHistory.RequestBatchHistory" %>

<asp:Content ID="HomePageCnt" ContentPlaceHolderID="ContentPlaceHolderMaster" runat="server">
<!-- start breadcrumb navigation -->
			<div class="gk-breadcrumbwrap" style="top: 160px;"><a class="gk-breadcrumb" href="../Home.aspx">Home</a> &gt; <a class="gk-breadcrumb" href="RequestHistory.aspx">Request History</a> &gt; <span class="gk-breadcrumb-on">Request ID: <asp:Label ID="RequestIdLbl" runat="server"></asp:Label></span></div>

			<!-- start content body -->
<div id="ScheduleDiv" runat="server">
	<!-- start Request Batch History tabled data content -->			
	<div id="idDataTabled" class="data-tabled" runat="server">
        <table cellpadding="4" cellspacing="0"">
        <tr>
          <th scope="col"><asp:Label ID="lblEntry" runat="server">Batch ID</asp:Label></th>
          <th scope="col"><asp:Label ID="lblBatchName" runat="server">Batch Name</asp:Label></th>
          <th scope="col"><asp:Label ID="lblStatus" runat="server">Status</asp:Label></th>
          <th scope="col"><asp:Label ID="lblDateTime" runat="server">Date</asp:Label></th> 
          <th scope="col"><asp:Label ID="lblUserName" runat="server">User Name</asp:Label></th>
          <th scope="col"><asp:Label ID="lblHistory" runat="server">Batch History</asp:Label></th>
        </tr>
					
	<asp:Repeater id="rptRequestBatchHistory" runat="server">
	<HeaderTemplate>
	</HeaderTemplate>
	<ItemTemplate>
              <tr>
                <td><a href="RequestDetails.aspx?reqID=<%=ReqID%>&batchid=<%# Eval("BatchID")%>"><%# Eval("BatchID")%></a></td>
                <td><%# Eval("BatchName")%></td>
                <td><%# Eval("Status")%></td>
                <td class="date"><%# Eval("EntryDate")%></td>
                <td><%# Eval("UserName")%></td>
                <td><a href="BatchHistory.aspx?batchid=<%# Eval("BatchID")%>&reqID=<%=ReqID%>">View Batch History</a></td>
             </tr>
	</ItemTemplate>
	<AlternatingItemTemplate>
              <tr class="altrow">
                <td><a href="RequestDetails.aspx?reqID=<%=ReqID%>&batchid=<%# Eval("BatchID")%>"><%# Eval("BatchID")%></a></td>
                 <td><%# Eval("BatchName")%></td>
                <td><%# Eval("Status")%></td>
                <td class="date"><%# Eval("EntryDate")%></td>
                <td><%# Eval("UserName")%></td>
                <td><a href="BatchHistory.aspx?batchid=<%# Eval("BatchID")%>&reqID=<%=ReqID%>">View Batch History</a></td>
             </tr>
	</AlternatingItemTemplate>
	<FooterTemplate>
	</FooterTemplate>
	</asp:Repeater>
    </table>
    </div>
</div>

</asp:Content>

