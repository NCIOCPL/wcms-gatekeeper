<%@ Page Language="C#" MasterPageFile="~/AdminToolMasterPage.Master" AutoEventWireup="true" CodeBehind="Error.aspx.cs" Inherits="GateKeeperAdmin.Error" Title="Untitled Page" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolderMaster" runat="server">
    				<div class="gk-inline-box" style="width: 600px" >	    
					<div class="gk-box-large">	 						
						<div style="padding: 40px 40px;">
							<table width="530" border="0" cellspacing="0" cellpadding="0">
							    <tr>
                                    <td><img src="/images/spacer.gif" width="7" height="30" alt="" border="0"/></td>
                                    <td><img src="/images/spacer.gif" width="7" height="30" alt="" border="0"/></td>
                                </tr>
							    <tr> 
							        <td><img src="/images/spacer.gif" width="30" height="1" alt="" border="0"/></td>
							        <td><b>Errors Occurred:</b></td> 
							        </tr>
							    <tr>
                                    <td><img src="/images/spacer.gif" width="7" height="10" alt="" border="0"/></td>
                                    <td><img src="/images/spacer.gif" width="7" height="10" alt="" border="0"/></td>
                                </tr>
								<tr>
								    <td><img src="/images/spacer.gif" width="30" height="1" alt="" border="0"/></td>
									<td><asp:Label ID="MesTextLbl" runat="server" /></td>
								</tr>
							</table>			
						</div>
                    </div>
				    </div>
</asp:Content>
