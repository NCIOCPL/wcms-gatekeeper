<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RequestDataHistory.aspx.cs" MasterPageFile="~/AdminToolMasterPage.Master" Inherits="GateKeeperAdmin.RequestHistory.RequestDataHistory" %>

<asp:Content ID="HomePageCnt" ContentPlaceHolderID="ContentPlaceHolderMaster" runat="server">
<!-- start breadcrumb navigation -->
			<div class="gk-breadcrumbwrap" style="top: 160px;"><a class="gk-breadcrumb" href="../Home.aspx">Home</a> &gt; <a class="gk-breadcrumb" href="RequestHistory.aspx">Request History</a> &gt; 
			<a class="gk-breadcrumb" href="RequestDetails.aspx?reqid=<%=ReqID%>">Request ID: <%=ReqID%></a> &gt; 
			<a class="gk-breadcrumb" href="RequestCDRID.aspx?reqdataid=<%=ReqDataID%>&reqid=<%=ReqID%>">Document Details</a> &gt; 
			<span class="gk-breadcrumb-on">RequestData ID: <asp:Label ID="RequestIdLbl" runat="server"></asp:Label></span></div>
			
			<!-- start content body -->

<div id="ScheduleDiv" runat="server">

	<!-- start tabled data content -->			
	<div id="idDataTabled" class="data-tabled" runat="server">

    <table cellpadding="4" cellspacing="0">
        <tr>
          <th scope="col"><asp:Label ID="lbtnBatchId" runat="server">Batch ID</asp:Label></th> 
          <th scope="col"><asp:Label ID="lblBatchName" runat="server">Batch Name</asp:Label></th> 
          <th scope="col"><asp:Label ID="lbtnHistDate" runat="server">Date</asp:Label></th> 
          <th scope="col"><asp:Label ID="lblEntry" runat="server">History</asp:Label></th> 
          <th scope="col"><asp:Label ID="lbtnHistoryType" runat="server">History Type</asp:Label><br />			
		    <span style="padding-left: 1px;">
    	        <asp:DropDownList ID="dropHistoryType" runat="server" cssclass="filter-dropdown"  AutoPostBack="true" OnSelectedIndexChanged="dropHistoryType_SelectedIndexChanged"></asp:DropDownList>
                <br />
                <asp:CheckBox ID="chkDebug" runat="server" AutoPostBack="True" Text="Show Debug Info" ToolTip="Show Debug Type" OnCheckedChanged="chkDebug_CheckedChanged" /></span></th>  
        </tr>
	<asp:Repeater id="rptDataHistory" runat="server">
	<HeaderTemplate>
	</HeaderTemplate>
	<ItemTemplate>
              <tr>
                <td><%# Eval("BatchID")%></td>
                <td class="left"><%# Eval("BatchName")%></td>
                <td class="date"><%# Eval("EntryDateTime")%></td>
                <td class="left"><%# Eval("Entry").ToString()%></td>
                <td><%# Eval("EntryType").ToString()%></td>
              </tr>
	</ItemTemplate>
	<AlternatingItemTemplate>
              <tr class="altrow">
                <td><%# Eval("BatchID")%></td>
                <td class="left"><%# Eval("BatchName")%></td>
                <td class="date"><%# Eval("EntryDateTime")%></td>
                <td class="left"><%# Eval("Entry").ToString()%></td>
                <td><%# Eval("EntryType").ToString()%></td>
              </tr>
	</AlternatingItemTemplate>
	<FooterTemplate>
	</FooterTemplate>
	</asp:Repeater>
	</table>
    </div>
</div>
</asp:Content>

