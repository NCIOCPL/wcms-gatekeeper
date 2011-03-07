<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TabsControl.ascx.cs" Inherits="GateKeeperAdmin.UserControls.TabsControl" %>

<!-- start main, horizontal navigation buttons -->	
			<div class="gk-mainnav-bkg">
				<ul class="gk-mainnav"> 
					<li class="gk-home"><asp:HyperLink ID="HomeTab" NavigateUrl="~/Home.aspx" ToolTip="Home" runat="server"></asp:HyperLink></li>
					<li class="gk-history"><asp:HyperLink NavigateUrl="~/RequestHistory/RequestHistory.aspx" ToolTip="Request History" id="ReqHistTab" runat="server"/></li>
					<li class="gk-activities"><asp:HyperLink NavigateUrl="~/ProcessingActivities/Activities.aspx" ToolTip="Processing Activities" id="ActivitiesTab" runat="server"/></li>
					<li class="gk-reports"><asp:HyperLink NavigateUrl="~/Reports/ViewReports.aspx" ToolTip="Data Reports" id="ReportsTab" runat="server"/></li>
					<li class="gk-admin"><asp:HyperLink NavigateUrl="~/Administrator/Default.aspx" ToolTip="Administrators" id="AdminTab" runat="server"/></li>
				</ul>
			</div>