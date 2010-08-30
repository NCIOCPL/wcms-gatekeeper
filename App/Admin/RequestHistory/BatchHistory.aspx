<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="BatchHistory.aspx.cs" MasterPageFile="~/AdminToolMasterPage.Master" Inherits="GateKeeperAdmin.RequestHistory.BatchHistory" %>

<asp:Content ID="HomePageCnt" ContentPlaceHolderID="ContentPlaceHolderMaster" runat="server">
<!-- start breadcrumb navigation -->
			<div class="gk-breadcrumbwrap" style="top: 100px;"><a class="gk-breadcrumb" href="../Home.aspx">Home</a> &gt; <a class="gk-breadcrumb" href="RequestHistory.aspx">Request History</a> &gt; <a class="gk-breadcrumb" href="RequestDetails.aspx?reqid=<%= RequestId %>">Request Details</a> &gt; <span class="gk-breadcrumb-on">Batch ID: <asp:Label ID="RequestIdLbl" runat="server"></asp:Label></span></div>
			
			<!-- start content body -->

<div id="ScheduleDiv" runat="server">

	<!-- start Batch Action tabled data content -->			
	<div id="idDataTabled" class="data-tabled" runat="server">
        <table cellpadding="4" cellspacing="0"">
            <tr>
                <th scope="col"><asp:Label ID="lblAction" runat="server">Scheduled Action</asp:Label></th>
                <th scope="col"><asp:Label ID="lblComplDate" runat="server">Completion Date</asp:Label></th>
            </tr>
            <asp:Repeater ID="rptBatchAction" runat="server" OnItemDataBound="rptBatchAction_ItemDataBound">
                <HeaderTemplate>
                </HeaderTemplate>
                <ItemTemplate>
                    <tr>
                        <td class="left"><%# Eval("Action").ToString()%></td>
                        <td class="date"><asp:Label ID="lblComplDateItem" runat="server" Text='<%# Eval("CompletionDate")%>'></asp:Label></td>
                    </tr>
                </ItemTemplate>
                <AlternatingItemTemplate>
                    <tr class="altrow">
                        <td class="left"><%# Eval("Action").ToString()%></td>
                        <td class="date"><asp:Label ID="lblComplDateItemAlt" Text='<%# Eval("CompletionDate")%>' runat="server"></asp:Label></td>
                    </tr>
                </AlternatingItemTemplate>
                <FooterTemplate>
                </FooterTemplate>
            </asp:Repeater>
        </table>

	<!-- start Batch tabled data content -->	        
        <table cellpadding="4" cellspacing="0"">
        <tr>
          <th scope="col"><asp:Label ID="lblEntry" runat="server">Entry</asp:Label></th> 
          <th scope="col"><asp:Label ID="lblDateTime" runat="server">Date</asp:Label></th> 
          <th scope="col"><asp:Label ID="lblUserName" runat="server">User Name</asp:Label></th> 
        </tr>
					
	<asp:Repeater id="rptBatchHistory" runat="server">
	<HeaderTemplate>
	</HeaderTemplate>
	<ItemTemplate>
              <tr>
                <td class="left" style="width: 200px"><%# Eval("Entry").ToString()%></td>
                <td class="date"><%# Eval("EntryDateTime")%></td>
                <td><%# Eval("UserName")%></td>
              </tr>
	</ItemTemplate>
	<AlternatingItemTemplate>
              <tr class="altrow">
                <td class="left" style="width: 200px"><%# Eval("Entry").ToString()%></td>
                <td class="date"><%# Eval("EntryDateTime")%></td>
                <td><%# Eval("UserName")%></td>
              </tr>
	</AlternatingItemTemplate>
	<FooterTemplate>
	</FooterTemplate>
	</asp:Repeater>
    </table>
    </div>

</div>
</asp:Content>