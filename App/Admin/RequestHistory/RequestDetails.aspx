<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RequestDetails.aspx.cs"  MasterPageFile="~/AdminToolMasterPage.Master" Inherits="GateKeeperAdmin.RequestHistory.RequestDetails" %>

<asp:Content ID="HomePageCnt" ContentPlaceHolderID="ContentPlaceHolderMaster" runat="server">

<!-- start breadcrumb navigation -->
			<div class="gk-breadcrumbwrap"><a class="gk-breadcrumb" href="../Home.aspx">Home</a> &gt; <a class="gk-breadcrumb" href="RequestHistory.aspx">Request History</a> &gt; <span class="gk-breadcrumb-on">Request ID: <asp:Label ID="RequestIdLbl" runat="server"></asp:Label><asp:Label ID="BatchIdLbl" runat="server"></asp:Label></span></div>
    <asp:Panel ID="pnlInfo" runat="server">			
        <div style="margin:3px 8px">Number of displayed documents: <asp:Label ID="lblNumEntries" runat="server"></asp:Label> | <a id="aPopup" href="#" runat="server">Number of each document type for Request <asp:Label ID="RequestIdLbl1" runat="server"></asp:Label></a></div>
    </asp:Panel>
    <div style="margin: 3px 8px; color: #0000dd; cursor: hand"></div>
        
			
			<!-- start content body -->

<div id="ScheduleDiv" runat="server">


<!-- start filter action dropdown -->
	 <div class="gk-action-filter" style="padding-left: 6px">
    	<div id="ActionsDiv" runat="server">
		<table cellpadding="4" cellspacing="0">
			<tr>
				<td><strong>Action:</strong></td>
				<td> 
				    <asp:DropDownList ID="ActionDropDown" runat="server" cssclass="gk-dropdown">
				    <asp:ListItem Value="-1" Text="" Selected="True"></asp:ListItem>
				    <asp:ListItem Value="1">Push To Preview</asp:ListItem>   
				    <asp:ListItem Value="2">Push To Live</asp:ListItem>   
   				    <asp:ListItem Value="3">Copy From Preview To Live</asp:ListItem>   
				    </asp:DropDownList>
                    <asp:CustomValidator ID="CustomValidator1" runat="server" ClientValidationFunction="ValidateDetails"
                        ControlToValidate="ActionDropDown" ErrorMessage="You must select an Action type" ValidationGroup="header">*</asp:CustomValidator>
                </td>
				<td><strong>  Batch Name: </strong><asp:TextBox ID="BatchNameTxt" runat="server"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="BatchNameTxt"
                        ErrorMessage="You must enter a Batch Name" ValidationGroup="header">*</asp:RequiredFieldValidator>
                    <asp:ValidationSummary ID="ValidationSummary1" runat="server" ValidationGroup="header" ShowMessageBox="true" ShowSummary="false" />
				</td>
				<td>
                    <asp:ImageButton ID="ActionGoButton" runat="server" ImageUrl="~/images/gk-schedule-selected.gif"
                        OnClick="ActionGoButton_Click" ValidationGroup="header" /></td>
				<td> | </td>
				<td>
                    <asp:ImageButton ID="ScheduleAllButton" runat="server" ImageUrl="~/images/gk-schedule-all.gif"
                        OnClick="ScheduleAllButton_Click" ValidationGroup="header" /></td>
				<td> | </td>
				<td><asp:ImageButton ID="ClearFiltersButton" runat="server" ImageUrl="~/images/btn-clear-filters.gif" OnClick="ClearFiltersButton_Click"/></td>
				<td> | </td>
				<td><asp:ImageButton ID="ExportButton" runat="server" ImageUrl="~/images/export.gif" OnClick="ExportButton_Click"/></td>
				
			</tr>
		</table>
		
	</div>
	<div id="NoActionsDiv" runat="server">
		<table cellpadding="4" cellspacing="0">
		<tr>
	        <td><asp:ImageButton ID="ImageButton1" runat="server" ImageUrl="~/images/btn-clear-filters.gif" OnClick="ClearFiltersButton_Click"/></td>
        </tr>
	    </table>
	</div>
	</div>
	<!-- start tabled data content -->	
	<asp:Label ID="RequestDocCount" runat="server" Visible="false"></asp:Label>

	<div class="data-tabled" >	
		<table border="0" cellpadding="4" cellspacing="0" id="tblMain">
        <tr>
          <th class="th1" scope="col">
<%--          <asp:HyperLink ID="SelectAll"  runat="server" CssClass="redtext" href="" OnClick="javascript:SelectOrClearAll(true); return (false);">Select All</asp:HyperLink><br />
--%>              
          <asp:LinkButton ID="lnkSelectAll" runat="server" CssClass="redtext" OnClick="SelectAll_Click">Select All</asp:LinkButton><br />
<%--              <asp:HyperLink ID="ClearAll" runat="server" CssClass="redtext" href="" OnClick="javascript:SelectOrClearAll(false); return (false);">Clear All</asp:HyperLink></th> 
--%>
          <asp:LinkButton ID="lnkClearAll" runat="server" CssClass="redtext" OnClick="ClearAll_Click">Clear All</asp:LinkButton><br />
          <th scope="col"><asp:LinkButton ID="PacketNumLinkBtn"  runat="server" OnClick="LinkBtn_Click">Packet <br />Number</asp:LinkButton></th> 
          <th scope="col"><asp:LinkButton ID="GroupIdLinkBtn"  runat="server" OnClick="LinkBtn_Click">Group ID</asp:LinkButton></th> 
          <th scope="col"><span style="padding-left: 8px;"><asp:LinkButton ID="CdrIdLinkBtn"  runat="server" OnClick="LinkBtn_Click">CDR ID</asp:LinkButton></span></th> 
          <th scope="col">
                <asp:LinkButton ID="ReqTypeLinkBtn"  runat="server">Request Type</asp:LinkButton><br />			
	    	    <asp:DropDownList ID="RequestTypeDropDown" runat="server" cssclass="filter-dropdown"  AutoPostBack="true" OnSelectedIndexChanged="DropDown_SelectedIndexChanged"></asp:DropDownList>
	      </th>  
          <th scope="col">
                <asp:LinkButton ID="DocTypeLinkBtn"  runat="server" OnClick="LinkBtn_Click">Document Type</asp:LinkButton><br />			
				<asp:DropDownList ID="DocTypeDropDown" runat="server" CssClass="filter-dropdown" AutoPostBack="true" OnSelectedIndexChanged="DropDown_SelectedIndexChanged"></asp:DropDownList>	 
		  </th>
		  <th scope="col">
		        <asp:LinkButton ID="DocStatusLinkBtn"  runat="server" OnClick="LinkBtn_Click">Document Status</asp:LinkButton><br /> 
		        <asp:DropDownList ID="StatusDropDown" runat="server" CssClass="filter-dropdown" AutoPostBack="true" OnSelectedIndexChanged="DropDown_SelectedIndexChanged"></asp:DropDownList>	 
          </th> 
		  <th scope="col">
		        <asp:LinkButton ID="DepStatusLinkBtn"  runat="server" OnClick="LinkBtn_Click">Dependency Status</asp:LinkButton><br /> 
			    <asp:DropDownList ID="DepStatusDropDown" runat="server" CssClass="filter-dropdown" AutoPostBack="true" OnSelectedIndexChanged="DropDown_SelectedIndexChanged"></asp:DropDownList>
		  </th> 
          <th class="lastcol" scope="col">
                <asp:LinkButton ID="LocationLnkBtn"  runat="server"  OnClick="LinkBtn_Click">Location</asp:LinkButton><br />
			    <asp:DropDownList ID="LocationDropDown" runat="server" CssClass="filter-dropdown" AutoPostBack="true" OnSelectedIndexChanged="DropDown_SelectedIndexChanged"></asp:DropDownList>
          </th>  
        </tr>

				

	<asp:Repeater id="ReqRepeater" OnItemDataBound="ReqRepeater_ItemDataBound" runat="server">
	<HeaderTemplate>
	</HeaderTemplate>
	<ItemTemplate>
              <tr>
                <td><asp:CheckBox id="ChkBox" runat="server"/><asp:Label ID="GroupIdLbl" runat="server" Visible="false" Text='<%# Eval("GroupID")%>'></asp:Label></td>
                <td><%# Eval("PacketNumber") %></td>
                <td><%# Eval("GroupID") %></td>
                <td><asp:Label ID="RequestDataIdLbl" runat="server" Visible="false" Text='<%# Eval("RequestDataID")%>'></asp:Label><a href="RequestCDRID.aspx?reqdataid=<%# Eval("RequestDataID") %>&reqid=<%=this.RequestID%>"><%# Eval("CdrID") %></a></td>
                <td><%# Eval("ActionType").ToString() %></td>
                <td><%# Eval("CDRDocType").ToString()%></td>
                <td><%# Eval("Status").ToString()%></td>
                <td><%# Eval("DependencyStatus").ToString()%></td>
                <td><%# Eval("Location").ToString()%></td>
              </tr>
	</ItemTemplate>
	<AlternatingItemTemplate>
              <tr class="altrow">
                <td><asp:CheckBox id="ChkBox" runat="server"/><asp:Label ID="GroupIdLbl" runat="server" Visible="false" Text='<%# Eval("GroupID")%>'></asp:Label></td>
                <td><%# Eval("PacketNumber") %></td>
                <td><%# Eval("GroupID") %></td>
                <td><asp:Label ID="RequestDataIdLbl" runat="server" Visible="false" Text='<%# Eval("RequestDataID")%>'></asp:Label><a href="RequestCDRID.aspx?reqdataid=<%# Eval("RequestDataID") %>&reqid=<%=this.RequestID%>"><%# Eval("CdrID") %></a></td>
                <td><%# Eval("ActionType").ToString() %></td>
                <td><%# Eval("CDRDocType").ToString()%></td>
                <td><%# Eval("Status").ToString()%></td>
                <td><%# Eval("DependencyStatus").ToString()%></td>
                <td><%# Eval("Location").ToString()%></td>
              </tr>
	</AlternatingItemTemplate>
	<FooterTemplate>
	</FooterTemplate>
	</asp:Repeater>
	</table>
	</div>



								
</div>
	
<!-- Confirmation screen -->
<div id="ConfirmationDiv" runat="server" visible="false">
	


	<table cellpadding="4" cellspacing="0">
		<tr>
			<td><strong>Schedule Action:  </strong><asp:Label Id="ActionLabel" runat="server"></asp:Label></td>
			<td> | </td>
            <td><strong>Batch Name: </strong><asp:Label id="BatchNameLbl" runat="server"></asp:Label></td>
			<td> &nbsp; | </td>
			<td><asp:ImageButton ID="ScheduleBttn" runat="server" ImageUrl="~/images/go-gray.gif" OnClick="ScheduleGoButton_Click" /></td>  
            
		</tr>
		<tr>
		</tr>
	</table>

	<!-- start tabled data content -->	
	
	<div class="data-tabled" >	

					
	<asp:Repeater id="RepeaterConfirm" runat="server">
	    <HeaderTemplate>
	    <table cellpadding="4" cellspacing="0">
          <thead>
            <tr>
              <th scope="col">Packet Number</th> 
              <th scope="col">Group ID</th> 
              <th scope="col">CDR ID</th> 
              <th scope="col">Request Type</th>  		
              <th scope="col">Document Type</th>
		      <th scope="col">Document Status</th> 
		      <th scope="col">Dependency Status</th> 
              <th class="lastcol" scope="col">Location</th>  
            </tr>
          </thead>	
			    <tbody>

	</HeaderTemplate>
	<ItemTemplate>
              <tr>
                <td><%# Eval("PacketNumber") %></td>
                <td><%# Eval("GroupID") %></td>
                <td><asp:Label ID="RequestDataIdLbl" runat="server" Visible="false" Text='<%# Eval("RequestDataID")%>'></asp:Label><a href="RequestCDRID.aspx?reqID=<%=this.RequestID%>&reqdataid=<%# Eval("RequestDataID") %>"><%# Eval("CdrID") %></a></td>
                <td><%# Eval("ActionType").ToString() %></td>
                <td><%# Eval("CDRDocType").ToString()%></td>
                <td><%# Eval("Status").ToString()%></td>
                <td><%# Eval("DependencyStatus").ToString()%></td>
                <td><%# Eval("Location").ToString()%></td>
              </tr>
	</ItemTemplate>
	<AlternatingItemTemplate>
                <td><%# Eval("PacketNumber") %></td>
                <td><%# Eval("GroupID") %></td>
                <td><asp:Label ID="RequestDataIdLbl" runat="server" Visible="false" Text='<%# Eval("RequestDataID")%>'></asp:Label><a href="RequestCDRID.aspx?reqID=<%=this.RequestID%>&reqdataid=<%# Eval("RequestDataID") %>"><%# Eval("CdrID") %></a></td>
                <td><%# Eval("ActionType").ToString() %></td>
                <td><%# Eval("CDRDocType").ToString()%></td>
                <td><%# Eval("Status").ToString()%></td>
                <td><%# Eval("DependencyStatus").ToString()%></td>
                <td><%# Eval("Location").ToString()%></td>
              </tr>
	</AlternatingItemTemplate>
	<FooterTemplate>
			</table>
	</FooterTemplate>
	</asp:Repeater>
	
	</div>
<!-- end tabled data content -->
</div>
</asp:Content>