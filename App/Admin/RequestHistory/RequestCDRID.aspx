<%@ Page Language="C#" MasterPageFile="~/AdminToolMasterPage.Master" AutoEventWireup="true" CodeBehind="RequestCDRID.aspx.cs" Inherits="GateKeeperAdmin.RequestHistory.RequestCDRID" Title="Gatekeeper Admin" %>
<%@ Register Src="~/RequestHistory/RequestDataSummary.ascx" TagName="RDSummary" TagPrefix="Request" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolderMaster" runat="server">

<!-- start breadcrumb navigation -->
			<div class="gk-breadcrumbwrap"><a class="gk-breadcrumb" href="../Home.aspx">Home</a> &gt; <a class="gk-breadcrumb" href="RequestHistory.aspx">Request History</a> &gt; <a class="gk-breadcrumb" href="RequestDetails.aspx?ReqId=<%=RequestID%>">Request ID: <%=RequestID%></a> &gt; <span class="gk-breadcrumb-on">CDR ID: <%=CdrID%></span></div>
			<div class="gk-hr">&nbsp;</div>
			
<!-- start inline bulleted list -->			
				<div class="gk-inline-box">
			        <h2>This Request</h2> <p style="display:inline;">(RequestID: <%=RequestID%>)</p>
					<ul class="gk-bullet-inline">
						<li><strong>Request Data ID:</strong>&nbsp; <asp:Label ID="RequestDataIdLbl" runat="server" EnableViewState="true"></asp:Label></li>
						<li><strong>CDR ID:</strong>&nbsp; <asp:Label ID="CDRIDLbl" runat="server" EnableViewState="true"></asp:Label></li>
                        <li><strong>CDR Version:</strong>&nbsp; <asp:Label ID="CDRVersionLbl" runat="server" EnableViewState="true"></asp:Label></li>		
						<li><strong>Packet Number:</strong>&nbsp; <asp:Label ID="PacketNumLbl" runat="server"></asp:Label></li>
                        <li><strong>Date Received:</strong>&nbsp; <asp:Label ID="DateLbl" runat="server" EnableViewState="true"></asp:Label></li>		
					</ul>
					<ul class="gk-bullet-inline">
                        <li><strong>Request Type:</strong>&nbsp; <asp:Label ID="ActionTypeLbl" runat="server"></asp:Label></li>
						<li><strong>Document Location:</strong>&nbsp; <asp:Label ID="LocationLbl" runat="server"></asp:Label></li>
					    <li><strong>Document Type:</strong>&nbsp; <asp:Label ID="DocTypeLbl" runat="server"></asp:Label></li>
					</ul>
					<ul class="gk-bullet-inline">
						<li><strong>Status:</strong>&nbsp; <asp:Label ID="StatusLbl" runat="server"></asp:Label></li>
						<li><strong>Dependency Status:</strong>&nbsp; <asp:Label ID="DependencyStatusLbl" runat="server"></asp:Label></li>
						<li><strong>Group ID:</strong>&nbsp; <asp:Label ID="GroupIdLbl" runat="server"></asp:Label></li>
					    <li><strong>View: </strong><a href="ViewXML.aspx?reqDataID=<%=ReqDataID%>">Gatekeeper XML</a></li>	
					    <li><strong>View: </strong><a href="RequestDataHistory.aspx?reqDataID=<%=ReqDataID%>&reqid=<%=RequestID%>">Gatekeeper Document History</a></li>			
					</ul>

				</div>
				
				
				<!-- start content body -->

	<div class="gk-contentwrap" style=" position: absolute; top: 346px;">

<!-- start actions form -->		
                <div id="ScheduleActionsDiv" runat="server">
				<div style="float:left;  width: 495px">
					<div class="gk-box-large">	 
						<h3>Schedule Actions</h3>
						<div style="padding: 0 12px;">
							<table width="476" border="0" cellspacing="0" cellpadding="0">
								<tr>
									<td nowrap="nowrap">Batch Name:&nbsp; </td>
									<td colspan="2"><asp:TextBox id="BatchNameTxtBox" style="width: 396px" runat="server"></asp:TextBox>
                                    </td>
                                    <td>
                                    <asp:RequiredFieldValidator ID="valrBatchName" runat="server" ControlToValidate="BatchNameTxtBox"
                                        ErrorMessage="You must enter a Batch Name" ValidationGroup="RequestCDRIDGroup">*</asp:RequiredFieldValidator>
</td>
								</tr>
								<tr>
									<td style="border-bottom: 1px solid #bcbcbc; padding:8px 0 3px 0;"><strong>Schedule</strong></td>
									<td style="border-bottom: 1px solid #bcbcbc;padding: 8px 0 3px 36px;"><strong>Action</strong></td>
								</tr>
								<tr>
									<td colspan="2"><div style="font-size: 4px; height: 4px; line-height: 4px;"></div></td>
								</tr>
								<tr>
									<td style="padding: 2px 0 2px 16px;"><label id="lblPushChkbx1">
									    <asp:CheckBox ID="PushToStagingPreviewChkbx" runat="server" />
									</label></td>
									<td nowrap="nowrap" style="padding-left: 36px;">Push to Staging and Preview</td>
								</tr>
								<tr>
									<td style="padding: 2px 0 2px 16px;"><label id="lblPushChkbx2">
										<asp:CheckBox ID="PushToLiveChkbx" runat="server" />
									</label></td>
									<td nowrap="nowrap" style="padding-left: 36px;">Push from Preview to Live </td>
                                    <asp:CustomValidator ID="valxCheckBoxes" runat="server" ClientValidationFunction="ValidateCDRID"
                                        ErrorMessage="You must select at least one Checkbox" ValidationGroup="RequestCDRIDGroup" Display="None" Text="">*</asp:CustomValidator>
                                    <asp:ValidationSummary ID="valsSummary" runat="server" ShowMessageBox="true"
                                        ShowSummary="false" ValidationGroup="RequestCDRIDGroup" />
								</tr>
								<tr>
									<td style="border-bottom: 1px solid #bcbcbc;"><asp:Image runat="server" ImageUrl="~/images/spacer.gif" width="80" height="1" /></td>
									<td style="border-bottom: 1px solid #bcbcbc;"><asp:Image runat="server" ImageUrl="~/images/spacer.gif" width="220" height="8" /></td>
								</tr>
								<tr>
									<td colspan="2"><div style="font-size: 8px; height: 8px; line-height: 8px;"></div></td>
								</tr>
								<tr>
									<td colspan="2"><asp:ImageButton runat="server" ImageUrl="~/images/btn-queue-actions.gif" ValidationGroup="RequestCDRIDGroup" OnClick="GoButton_Click" /></td>
								</tr>
							</table>
							
						</div>
					</div>
					</div>
		</div>	
				
		<div style="clear: both;"></div>
	
<!-- start gatekeeper db link farm -->					
				<div style="float:left;  width: 195px">
					<div class="gk-boxed-linkfarm">
					    <Request:RDSummary ID="GateKeeperBox" runat="server" sectionLabel="GateKeeper DB" />
						<p align="center"><strong>(Reflects most recent version)</strong></p>
					</div>
				</div>	

<!-- start staging db link farm -->					
				<div style="float:left;  width: 195px">
					<div class="gk-boxed-linkfarm">	 
					    <Request:RDSummary ID="StagingBox" runat="server" sectionLabel="Staging DB" />
					</div>
				</div>	

<!-- start preview db link farm -->				
			    <div style="float: left; width: 195px;">
				     <div class="gk-boxed-linkfarm">	 
				        <Request:RDSummary ID="PreviewBox" runat="server" sectionLabel="Preview DB" />
				    </div>		
			    </div>

<!-- start live db link farm -->						
			<div style="float: left; width: 195px;">
					<div class="gk-boxed-linkfarm">	 
				        <Request:RDSummary ID="LiveBox" runat="server" sectionLabel="Live DB" />
					</div>
			</div>
		</div>
</asp:Content>
