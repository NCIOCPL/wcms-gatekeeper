<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="LoginControl.ascx.cs" Inherits="GateKeeperAdmin.WebControls.LoginControl" %>


	<div class="gk-login-table">
		    <div id="LoginDiv" runat="server">		
                <asp:Login ID="Login1" runat="server">
                    <LayoutTemplate>
                    <table cellpadding="0" cellspacing="2" width="100%">
    	                <tr align="center">
			                    <td nowrap="nowrap" valign="top" style="padding-right: 4px;"><strong>Sign In:</strong></td>
	                            <td  valign="top"><asp:TextBox ID="UserName" runat="server"></asp:TextBox></td>   
			                    <td valign="top" width="10"><asp:RequiredFieldValidator ID="UserNameRequired" runat="server" ControlToValidate="UserName"
                                                        ErrorMessage="User Name is required." ToolTip="User Name is required." ValidationGroup="ctl00$Login1">*</asp:RequiredFieldValidator></td>
			                    <td  valign="top"><asp:TextBox ID="Password" runat="server" TextMode="Password"></asp:TextBox></td>
                                <td valign="top" width="10"><asp:RequiredFieldValidator ID="PasswordRequired" runat="server" ControlToValidate="Password"
                                                    ErrorMessage="Password is required." ToolTip="Password is required." ValidationGroup="ctl00$Login1">*</asp:RequiredFieldValidator></td>

			                    <td>
			                     <asp:ImageButton ID="LoginButton" runat="server"  ToolTip="Go" ImageUrl="~/images/btn-go.gif" CommandName="Login" Text="Log In" ValidationGroup="ctl00$Login1" />
			                    </td>
			                    <td width="840" >
                                    <a runat=server id="GetPassword" href="~/Security/RecoverPassword.aspx">Forgot your password?</a> </td>
			                    <td valign="top" class="gk-helpbtn-padding"><a href="#"><asp:Image runat="server" ImageUrl="~/images/btn-help.gif" AlternateText="Help" title="Help" width="47" height="17" borderwidth="0" /></a></td>
		                </tr>
                        <tr>
                            <td class="error" colspan="8">
                                <asp:Literal ID="FailureText" runat="server" EnableViewState="False"></asp:Literal>
                            </td>
                        </tr>
                        </table>
                    </LayoutTemplate>
                </asp:Login>
			</div>
			<div id="LoggedinDiv" runat="server" visible="false">
			<table cellpadding="0" cellspacing="4" width="100%">
            	<tr>
			    <td nowrap="nowrap"><strong>Logged in as: </strong><asp:LoginName ID="LoginName1" runat="server" /></td>
			    <td>|</td>
			    <td nowrap="nowrap"><asp:LoginStatus ID="LoginStatus1" runat="server" /></td>
			    <td>|</td>
			    <td nowrap="nowrap"><a runat=server href="~/Security/ResetPassword.aspx">Reset Password/Change Email</a> </td>
			    <td width="340" >&nbsp;</td>
			    <td><a href="#"><asp:Image runat="server" ImageUrl="~/images/btn-help.gif" AlternateText="Help" title="Help" width="47" height="17"  BorderWidth="0" /></a></td>
			</tr>
			</table>
			</div>
	</div>







