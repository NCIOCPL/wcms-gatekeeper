<%@ Page Language="C#" MasterPageFile="~/AdminToolMasterPage.Master" AutoEventWireup="true" CodeBehind="Activities.aspx.cs" Inherits="GateKeeperAdmin.ProcessingActivities.Activities"  %>
<asp:Content ID="headContent" ContentPlaceHolderID="ContentPlaceHolderHead" runat="server">
    <meta http-equiv="Refresh" content="30"/>
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolderMaster" runat="server">

<!-- start breadcrumb navigation -->
			<div class="gk-breadcrumbwrap"><a class="gk-breadcrumb" href="../Home.aspx">Home</a> &gt; <span class="gk-breadcrumb-on">Processing Activities</span></div>

<!-- start content body -->


<!-- start tabs -->
				<div class="gk-tab-wrap">	
					<div id="ActiveTabDiv" runat="server">
					<ul class="gk-tabs">
						<li class="queue"><a class="on" href="#" title="Process Queue Contents"><span>Process Queue Contents</span></a></li>   
						<li class="transactions"><a href="Activities.aspx?tab=2" title="Failed Batches" style="padding-right: 8px;"><asp:Image ID="ErrorImage1" visible="false" runat="server" ImageUrl="~/images/gk-urgent.gif" ImageAlign="left" Width="28" Height="22" BorderWidth="0" /> <span style="margin-left: -18px">Failed Batches</span></a></li>
					</ul>
				    </div>
			        <div id="FailedTabDiv" runat="server" visible="false">
				    <ul class="gk-tabs">
						<li class="queue"><a href="Activities.aspx?tab=1" title="Process Queue Contents"><span>Process Queue Contents</span></a></li>
						<li class="transactions"><a class="on" href="#" title="Failed Batches"><asp:Image ID="ErrorImage2" Visible="false" runat="server" ImageUrl="~/images/gk-urgent.gif" ImageAlign="left" Width="28" Height="22" BorderWidth="0" /> <span style="padding-left: 20px;">Failed Batches</span></a></li>
					</ul>
					</div>
				</div>
				
				<!-- start tabled data content -->			
				<div class="gk-tabbed-container">
		 <div class="gk-action-filter" id="ActionsDiv" visible="false" runat="server">
		 <table cellpadding="0" cellspacing="4" border="0">
            <tr>
              <td nowrap="nowrap"><strong style="font-size: 12px; font-family:Arial, Helvetica, sans-serif;">Process Queue Contents:</strong></td>
              <td>
              <asp:ImageButton ID="ActivateButton" runat="server" ImageUrl="~/images/btn-activate.gif" ImageAlign="top" OnClick="ActivateButton_Click"/>
              <asp:ImageButton ID="DeactivateButton" runat="server" ImageUrl="~/images/btn-deactivate.gif" ImageAlign="top" Visible="false" OnClick="DeactivateButton_Click"/>
              </td>
              <td><asp:ImageButton ID="ClearButton" runat="server" ImageUrl="~/images/btn-clear.gif"  ImageAlign="top" OnClick="ClearButton_Click"/></td>
            </tr>
          </table>
          </div>

          <div class="gk-action-filter" style="display:inline; background-color:Yellow;" id="StatusDiv" visible="false" runat="server">
            <strong style="font-size: 12px; font-family:Arial, Helvetica, sans-serif;">The GateKeeper promotion system has been deactivated by an administrator. New batches will not be processed.</strong>
          </div>

          	<!-- start tabled data content -->		

				
	<div class="data-tabled" >
	<asp:Repeater id="BatchRepeater" runat="server">
	<HeaderTemplate>
				<table border="0" cellpadding="4" cellspacing="0">
					<tr>
                      <th scope="col">
                      <asp:LinkButton ID="lnkSelectAll" runat="server" CssClass="redtext" OnClick="SelectAll_Click">Select All</asp:LinkButton><br />
                      <asp:LinkButton ID="lnkClearAll" runat="server" CssClass="redtext" OnClick="ClearAll_Click">Clear All</asp:LinkButton><br />
                      <th scope="col">Batch ID</th> 
                      <th scope="col">Batch Name</th> 
                      <th scope="col">Batch Status </th>           
                      <th scope="col">Request ID</th> 
                      <th scope="col">Date Queued</th> 
                      <th scope="col">User Name</th> 
                      <th scope="col">Errors/<br />Warnings</th>   
					</tr>
	</HeaderTemplate>
	<ItemTemplate>
              <tr>
                <td><asp:CheckBox id="ChkBox" runat="server"/><asp:Label ID="BatchIdLbl" runat="server" Visible="false" Text='<%# Eval("BatchID")%>'></asp:Label></td>
                <td><a href="../RequestHistory/BatchHistory.aspx?batchId=<%# Eval("BatchID")%>&reqId=<%# Eval("RequestId")%>"><%# Eval("BatchID")%></a></td>
                <td class="left"><%# Eval("BatchName")%></td>
                <td><%# Eval("Status") %></a></td>
                <td><a href="../RequestHistory/RequestDetails.aspx?reqId=<%# Eval("RequestId")%>"><%# Eval("RequestId") %></a></td>
                <td class="date"><%# Eval("EntryDate")%></td>
                <td><%# Eval("UserName")%></td>
                <td><a href="../RequestHistory/RequestDetails.aspx?batchId=<%# Eval("BatchId")%>&reqId=<%# Eval("RequestId")%>"><%# Eval("ErrorCount")%>/<%# Eval("WarningCount").ToString()%></a></td>
              </tr>
	</ItemTemplate>
	<AlternatingItemTemplate>
              <tr class="altrow">
                <td><asp:CheckBox id="ChkBox" runat="server"/><asp:Label ID="BatchIdLbl" runat="server" Visible="false" Text='<%# Eval("BatchID")%>'></asp:Label></td>
                <td><a href="../RequestHistory/BatchHistory.aspx?batchId=<%# Eval("BatchID")%>&reqId=<%# Eval("RequestId")%>"><%# Eval("BatchID")%></a></td>
                <td class="left"><%# Eval("BatchName")%></td>
                <td><%# Eval("Status") %></a></td>
                <td><a href="../RequestHistory/RequestDetails.aspx?reqId=<%# Eval("RequestId")%>"><%# Eval("RequestId") %></a></td>
                <td class="date"><%# Eval("EntryDate")%></td>
                <td><%# Eval("UserName")%></td>
                <td><a href="../RequestHistory/RequestDetails.aspx?batchId=<%# Eval("BatchId")%>&reqId=<%# Eval("RequestId")%>"><%# Eval("ErrorCount")%>/<%# Eval("WarningCount").ToString()%></a></td>
              </tr>
	</AlternatingItemTemplate>
	<FooterTemplate>
			</table>
	</FooterTemplate>
	</asp:Repeater>      
    </div>
			

<!-- end content body -->

				
				
			</div>
</asp:Content>
