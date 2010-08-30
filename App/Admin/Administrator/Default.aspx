<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" MasterPageFile="~/AdminToolMasterPage.Master" Inherits="GateKeeperAdmin.Administrator.ResetCache" %>
 <asp:Content ID="ManageUsersPageCnt" ContentPlaceHolderID="ContentPlaceHolderMaster" runat="server">  
 <!-- start breadcrumb navigation -->

	<div id="gk-home-contentwrap">	
	<div class="gk-breadcrumbwrap" style="top: 160px;"><a class="gk-breadcrumb" href="../Home.aspx">Home</a> &gt; <span class="gk-breadcrumb-on">Administrator</span></div>
    <div class="gk-hr">&nbsp;</div>
        <asp:Label ID="lblStatus" runat="server" ForeColor="red" ></asp:Label>
	  <div id ="AdminBoxDiv" runat="server"> 
	     <div class="gk-rtcol-home" style="height:111px">	
	    <h2>Users/Roles Management</h2>
				<ul class="gk-bullet">
					<li><a href="ManageUsers.aspx">Manage Users</a></li><li><a href="ManageRoles.aspx">Manage Roles</a></li></ul>
		</div>
          <div class="gk-rtcol-home" style="height:111px">
              <h2>Other Tools</h2>
				<ul class="gk-bullet">
					<li><a href="EventLogViewer.aspx">View Eventlog</a></li><li><a href="../Reports/ViewLocation.aspx">View Document Locations</a></li></ul>
          </div>
          </div>
	  <div class="gk-ltcol-home" style="height:auto">		
         <div class="gk-hr">&nbsp;</div>
         <div class="gk-hr">&nbsp;</div>
	    
        <div style=" margin-left:5px">
		<strong>Cache Management</strong><br /><br />
        <asp:Button ID="btnResetLiveCache" runat="server" Text="Reset Live Cache" OnClick="btnResetLiveCache_Click" />
            <asp:Button ID="btnResetPreviewCache" runat="server" Text="Reset Preview Cache" OnClick="btnResetPreviewCache_Click" /><br /><br />
        <br />
        <strong>Pretty URL Management</strong><br /><br />
       <asp:Button ID="btnResetCGPU" runat="server" Text="Reset CancerGov Pretty URL" OnClick="btnResetCGPU_Click" />
       <asp:Button ID="btnResetCGSPU" runat="server" Text="Reset CancerGovStagingPretty URL" OnClick="btnResetCGSPU_Click" />
        <br />
        <br />
      
         <table  id="ImportDoc" runat="server" width="520">
         <tr>
            <th colspan="2" align="left">
                Import Document</th>
        </tr> 
        <tr>
            <td>
               File Name</td>
            <td style="width: 495px"> 
                <asp:FileUpload ID="FileUploadImport" runat="server" Width="448px" /></td>
        </tr>  
        <tr>
            <td colspan="2" align="center">
                <asp:Button ID="btnImport" Runat="server" Text="Import" OnClick="btnImport_Click" />&nbsp;
            </td>
        </tr>
    </table>
        
    </div>
	</div>

    <div class="gk-ltcol-home">
	    <p><strong>Open Requests</strong></p>
	    <div class="data-tabled" >
		    <table border="0" cellpadding="4" cellspacing="0">

                <tr>
                  <th class="th1" scope="col">Request Date</th> 
                  <th scope="col">Request ID</th> 
		          <th scope="col">External ID</th> 
                  <th scope="col">Description</th> 
                  <th scope="col">RequestType</th> 
		          <th scope="col">Source</th>
                  <th scope="col">&nbsp;</th>
                </tr>


	            <asp:Repeater id="ReqRepeater" runat="server">
	                <HeaderTemplate>
	                </HeaderTemplate>
	                <ItemTemplate>
                      <tr>
                        <td class="date"><%# Eval("initiateDate")%></td>
                        <td><a href="RequestDetails.aspx?reqId=<%# Eval("RequestId")%>"><%# Eval("requestID") %></a></td>            
                        <td class="left"><%# Eval("ExternalRequestID")%></td>
                        <td class="left"><%# Eval("description")%></td>
                        <td><%# Eval("requestType")%></td>
                        <td><%# Eval("source")%></td>
                        <td><asp:Button ID="btnAbort" Runat="server" Text="Abort" OnClick="btnAbort_Click"
                                OnClientClick="return ConfirmRequestAbort()"
                                CommandArgument=<%# Eval("ExternalRequestID") + "|" + Eval("source") %> />
                            </td>
                      </tr>
	                </ItemTemplate>
	                <AlternatingItemTemplate>
                      <tr class="altrow">
                        <td class="date"><%# Eval("initiateDate")%></td>
                        <td><a href="RequestDetails.aspx?reqId=<%# Eval("RequestId")%>"><%# Eval("requestID") %></a></td>            
                        <td class="left"><%# Eval("ExternalRequestID")%></td>
                        <td class="left"><%# Eval("description")%></td>
                        <td><%# Eval("requestType")%></td>
                        <td><%# Eval("source")%></td>
                        <td><asp:Button ID="btnAbort" Runat="server" Text="Abort" OnClick="btnAbort_Click"
                                OnClientClick="return ConfirmRequestAbort()"
                                CommandArgument=<%# Eval("ExternalRequestID") + "|" + Eval("source") %> />
                            </td>
                      </tr>
    	            </AlternatingItemTemplate>
	                <FooterTemplate>
	                </FooterTemplate>
    	        </asp:Repeater>
	        </table>
	    </div>
	</div>
   
    

    </div>
 </asp:Content>

