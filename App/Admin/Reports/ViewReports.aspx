<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ViewReports.aspx.cs" MasterPageFile="~/AdminToolMasterPage.Master" Inherits="GateKeeperAdmin.Reports.ViewReports" %>

<asp:Content ID="ViewReportsCnt" ContentPlaceHolderID="ContentPlaceHolderMaster" runat="server">
    <div class="gk-breadcrumbwrap"><a class="gk-breadcrumb" href="../Home.aspx">Home</a> &gt; <span class="gk-breadcrumb-on">Data Reports</span></div>

<!-- start dropdownlist -->			
	<div style="padding-left: 6px; margin-bottom: 10px">
		<table cellpadding="2" cellspacing="0">
			<tr>
			    <td>
                    <asp:DropDownList ID="dropViewReports" runat="server" AutoPostBack="false" CssClass="filter-dropdown"></asp:DropDownList>
			    </td>
                <td valign="bottom">
                    <asp:ImageButton ID="ibtnGo" runat="server" ImageUrl="~/images/btn-go.gif" OnClick="ibtnGo_Click"/>
                </td>
            </tr>
		</table>
	</div>
  
			<!-- start content body -->
<div id="ScheduleDiv" runat="server">
	<!-- start Request Batch History tabled data content -->			
	<div id="idDataTabled" class="data-tabled" runat="server">
	<asp:Panel ID="pnlTitle" runat="server">
	<div style="margin-bottom:10px" runat="server"><asp:Label ID="lblTitle" runat="server" Font-Bold="True" Font-Size="Larger"></asp:Label><strong> </strong>
    </div>	
    </asp:Panel>
	<table cellpadding="4" cellspacing="0"">
        <tr>
          <th scope="col"><asp:Label ID="lblBatchName" runat="server">Table</asp:Label></th>
          <th scope="col"><asp:Label ID="lblStatus" runat="server">On Live</asp:Label></th>
          <th scope="col"><asp:Label ID="lblDateTime" runat="server">On Preview</asp:Label></th> 
          <th scope="col"><asp:Label ID="lblUserName" runat="server">On Staging</asp:Label></th>
        </tr>
					
	<asp:Repeater ID="rptViewReports" runat="server" OnItemDataBound="rptViewReports_ItemDataBound">
	<HeaderTemplate>
	</HeaderTemplate>
	<ItemTemplate>
              <tr>
                <td class="left"><asp:Label ID="lblTableName" runat="server" Text='<%# Eval("Table")%>'></asp:Label></td>
                <td><%# Eval("On Live")%></td>
                <td><%# Eval("On Preview")%></td>
                <td><%# Eval("On Staging")%></td>
             </tr>
	</ItemTemplate>
	<AlternatingItemTemplate>
              <tr class="altrow">
                <td class="left"><asp:Label ID="lblTableName" runat="server" Text='<%# Eval("Table")%>'></asp:Label></td>
                <td><%# Eval("On Live")%></td>
                <td><%# Eval("On Preview")%></td>
                <td><%# Eval("On Staging")%></td>
             </tr>
	</AlternatingItemTemplate>
	<FooterTemplate>
	</FooterTemplate>
	</asp:Repeater>
    </table>
    </div>
</div>
  


</asp:Content>
