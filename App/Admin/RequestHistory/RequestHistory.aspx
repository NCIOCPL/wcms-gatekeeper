<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RequestHistory.aspx.cs" MasterPageFile="~/AdminToolMasterPage.Master" Inherits="GateKeeperAdmin.RequestHistory.RequestHistory" %>

<asp:Content ID="HomePageCnt" ContentPlaceHolderID="ContentPlaceHolderMaster" runat="server">

    <!-- start breadcrumb navigation -->
	<div class="gk-breadcrumbwrap"><a class="gk-breadcrumb" href="../Home.aspx">Home</a> &gt; <span class="gk-breadcrumb-on">Request History</span></div>
	
	<!-- start content body -->


	<!-- start filter action dropdown -->
	<div style="padding-left: 6px">
		<table cellpadding="2" cellspacing="0">
			<tr>
				<td nowrap="nowrap" style="font-size: 12px; font-family:Arial, Helvetica, sans-serif">Display Request Submitted in the last</td>
				<td> 
				    <asp:DropDownList ID="NumMonthDropDown" runat="server" CssClass="gk-dropdown" Width="50"></asp:DropDownList>
				</td>
				<td> month </td>
				<td><asp:ImageButton ID="ActionGoButton" runat="server" ImageUrl="~/images/go-gray.gif" OnClick="ActionGoButton_Click" /></td>
				<td> | </td>				
				<td><asp:ImageButton ID="ClearFiltersButton" runat="server" ImageUrl="~/images/btn-clear-filters.gif" OnClick="ClearFiltersButton_Click"/></td>
				<td><asp:Image runat="server" ImageUrl="~/images/spacer.gif" width="340" height="1" /></td>
				<td nowrap="nowrap"><strong>Search for all requests with CDRID:</strong></td>
				<td><asp:TextBox id="CdrIdTextBox" runat="server"></asp:TextBox></td>
				<td><asp:ImageButton ID="CdrGoButton" runat="server" ImageUrl="~/images/btn-go.gif" OnClick="CDRGoButton_Click"/></td>
			</tr>
		</table>
	</div>
	
		<!-- start tabled data content -->	
	<asp:Label ID="RequestDocCount" runat="server" Visible="false"></asp:Label>

	<div class="data-tabled" >	
		<table border="0" cellpadding="4" cellspacing="0">
        <tr>
          <th class="th1" scope="col"><asp:LinkButton ID="ReqDateLinkBtn"  runat="server" OnClick="LinkBtn_Click">Request Date</asp:LinkButton></th> 
          <th scope="col"><asp:LinkButton ID="ReqIdLinkBtn"  runat="server" OnClick="LinkBtn_Click">Request ID</asp:LinkButton></th> 
          <th scope="col">Description</th> 
          <th scope="col">Request<br />Type</th> 
          <th scope="col">
                <asp:LinkButton ID="ReqStatusLinkBtn"  runat="server" OnClick="LinkBtn_Click">Request <br />Status</asp:LinkButton><br />			
	    	    <asp:DropDownList ID="RequestStatusDropDown" runat="server" cssclass="filter-dropdown"  AutoPostBack="true" OnSelectedIndexChanged="DropDown_SelectedIndexChanged"></asp:DropDownList>
	      </th>  
          <th scope="col">
                <asp:LinkButton ID="PubDestinationLinkBtn"  runat="server" OnClick="LinkBtn_Click">Publishing Destination</asp:LinkButton><br />			
				<asp:DropDownList ID="PubDestinationDropDown" runat="server" CssClass="filter-dropdown" AutoPostBack="true" OnSelectedIndexChanged="DropDown_SelectedIndexChanged"></asp:DropDownList>	 
		  </th>
		  <th scope="col">Source</th>
		  <th scope="col"># of Documents</th>
		  <th scope="col">#&nbsp;Promoted to <br />Staging</th>
		  <th scope="col">#&nbsp;Promoted to <br />Preview</th>
		  <th scope="col">#&nbsp;Promoted to Live</th>
		  <th scope="col">Errors/ Warnings</th> 
		  <th scope="col">Batch <br />History</th>  
		  <th scope="col"><asp:LinkButton ID="ExternalReqIdLinkBtn"  runat="server" OnClick="LinkBtn_Click">External ID</asp:LinkButton></th> 
        </tr>

				

	<asp:Repeater id="ReqRepeater" runat="server">
	<HeaderTemplate>
	</HeaderTemplate>
	<ItemTemplate>
              <tr>
                <td class="date"><%# Eval("initiateDate")%></td>
                <td><a href="RequestDetails.aspx?reqId=<%# Eval("RequestId")%>"><%# Eval("requestID")%></a></td>            
                <td class="left"><%# Eval("description")%></td>
                <td><%# Eval("requestType")%></td>
                <td><%# Eval("status")%></td>
                <td><%# Eval("publicationTarget")%></td>
                <td><%# Eval("source")%></td>
                <td><%# Eval("TotalDoc")%><br /> [ +<%# Eval("TotalExport")%> | -<%# Eval("TotalRemove")%> ]</td>
                <td><%# Eval("TotalStaging") %></td>
                <td><%# Eval("TotalPreview")%></td>
                <td><%# Eval("TotalLive")%></td>
                <td><a href="RequestDetails.aspx?reqId=<%# Eval("RequestId")%>"><%# Eval("TotalError")%>/<%# Eval("TotalWarning").ToString()%></a></td>
                <td><a href="RequestBatchHistory.aspx?reqId=<%# Eval("RequestId")%>">view</a></td>
                <td class="left"><%# Eval("ExternalRequestID")%></td>
              </tr>
	</ItemTemplate>
	<AlternatingItemTemplate>
              <tr class="altrow">
                <td class="date"><%# Eval("initiateDate")%></td>
                <td><a href="RequestDetails.aspx?reqId=<%# Eval("RequestId")%>"><%# Eval("requestID")%></a></td>            
                <td class="left"><%# Eval("description")%></td>
                <td><%# Eval("requestType")%></td>
                <td><%# Eval("status")%></td>
                <td><%# Eval("publicationTarget")%></td>
                <td><%# Eval("source")%></td>
                <td><%# Eval("TotalDoc")%><br /> [ +<%# Eval("TotalExport")%> | -<%# Eval("TotalRemove")%> ]</td>  
                <td><%# Eval("TotalStaging") %></td>
                <td><%# Eval("TotalPreview")%></td>
                <td><%# Eval("TotalLive")%></td>
                <td><a href="RequestDetails.aspx?reqId=<%# Eval("RequestId")%>"><%# Eval("TotalError")%>/<%# Eval("TotalWarning").ToString()%></a></td>
                <td><a href="RequestBatchHistory.aspx?reqId=<%# Eval("RequestId")%>">view</a></td>
                <td class="left"><%# Eval("ExternalRequestID")%></td>
              </tr>
	</AlternatingItemTemplate>
	<FooterTemplate>
	</FooterTemplate>
	</asp:Repeater>
	</table>
	</div>

	
</asp:Content>