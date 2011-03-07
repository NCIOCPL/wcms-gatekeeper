<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/AdminToolMasterPage.Master" CodeBehind="Home.aspx.cs" Inherits="GateKeeperAdmin.www.Home" %>


    <asp:Content ID="HomePageCnt" ContentPlaceHolderID="ContentPlaceHolderMaster" runat="server">

<!-- start content body -->
	
<!-- start content body banner -->
	<div id="gk-home-contentwrap" style="margin-top: 22px;"><asp:Image runat="server" ImageUrl="~/images/page-banner-home.jpg" AlternateText="Controls &amp; Monitors System Activities!" title="Controls &amp; Monitors System Activities!" width="721" height="170" />

<!-- start main content right column, administrators link farm -->
	  <div id ="AdminBoxDiv" runat="server"> 
	  <div class="gk-rtcol-home">	
			<h2>Administrators</h2>
				<ul class="gk-bullet">
					<li><a href="Administrator/Default.aspx">Reset Cache/Pretty URLs</a></li>
					<li><a href="Administrator/Default.aspx">Import</a></li>
					<li><a href="Administrator/ManageUsers.aspx">Manage Users</a></li>
					<li><a href="Administrator/ManageRoles.aspx">Manage Roles</a></li>
                    <li><a href="Administrator/EventLogViewer.aspx">View Eventlog</a></li>
                    <li><a href="Reports/ViewLocation.aspx">View Document Locations</a></li>
                </ul>
		</div>
		</div>

<!-- start main content left column -->			
	<div class="gk-ltcol-home">
<!-- start request of history link farm -->			
		<div class="gk-home180">
			<h1>Request of History</h1>
				<ul class="gk-bullet">
					<li><a href="RequestHistory/RequestHistory.aspx">View Request History</a></li>
					</ul>
		</div>	

<!-- start data reports link farm -->				
		<div class="gk-home168">
			<h1>Data Reports</h1>
				<ul class="gk-bullet">
					<li><a href="Reports/ViewReports.aspx">View Reports</a></li>
				</ul>
		</div>

<!-- start processing activities link farm -->						
		<div class="gk-home180">
			<h1>Processing Activities</h1>
				<ul class="gk-bullet">
					<li><a href="ProcessingActivities/Activities.aspx">View Processing Activities</a></li>
				</ul>
		</div>
	</div>
	</div>

    </asp:Content>

