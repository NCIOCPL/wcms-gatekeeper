<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ResetPassword.aspx.cs" MasterPageFile="~/AdminToolMasterPage.Master" Inherits="GateKeeperAdmin.Security.ResetPassword" %>

<asp:Content ID="ManageUsersPageCnt" ContentPlaceHolderID="ContentPlaceHolderMaster" runat="server">    
    <div id="gk-home-contentwrap">
    <asp:ChangePassword ID="ChangePassword1" runat="server"   OnChangedPassword="Redirect"  ContinueButtonStyle-Width="0">
    </asp:ChangePassword>
       <br />
         <table  border=1>
                <caption><b> User Info</b>&nbsp;</caption>
                    <tr>
                            <td>
                                &nbsp;User name&nbsp;</td>
                            <td>
                                <asp:TextBox ID="txtUserName" Runat="server"  Enabled=false MaxLength="40" CssClass="size40_field" ></asp:TextBox>
                            </td>
                    </tr>
                    <tr>
                            <td>
                                &nbsp;Role&nbsp;</td>
                            <td>
                                <asp:TextBox ID="txtRole" Runat="server"  Enabled=false MaxLength="40" CssClass="size40_field" ></asp:TextBox>
                            </td>
                    </tr>
                    <tr>
                            <td>
                                &nbsp;Email
                            </td>
                            <td>
                                <asp:TextBox ID="txtEmail" Runat="server" MaxLength="200" CssClass="size40_field" ></asp:TextBox>&nbsp;<asp:RegularExpressionValidator
                                    ID="RegularExpressionValidator1" runat="server" ControlToValidate="txtEmail"
                                    ErrorMessage="Invalid email" ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"></asp:RegularExpressionValidator>
                            </td>
                     </tr> 
                <tr>
                    <td colspan=2 align=center>
                            &nbsp;&nbsp;&nbsp;
                            <asp:Button ID="btnUpdate" Runat="server" Text="Update" OnClick="btnUpdate_Click" />&nbsp;&nbsp;&nbsp;
                            <asp:Button ID="btnCancel" Runat="server" Text="Cancel" OnClick="btnCancel_Click" />&nbsp;
                    </td>
                </tr>
            </table>
            <br />
            <asp:Label ID="lblStatus" Runat="server" ForeColor="Red"></asp:Label>
    </div>
</asp:Content>
