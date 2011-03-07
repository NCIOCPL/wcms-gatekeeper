<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="EditUser.aspx.cs"  MasterPageFile="~/AdminToolMasterPage.Master" Inherits="GateKeeperAdmin.Administrator.AssignRoleToUser" %>


    <asp:Content ID="ManageUsersPageCnt" ContentPlaceHolderID="ContentPlaceHolderMaster" runat="server">  
     <!-- start breadcrumb navigation -->
    <div id="gk-home-contentwrap">
	<div class="gk-breadcrumbwrap" style="top: 160px;"><a class="gk-breadcrumb" href="../Home.aspx">Home</a> &gt; <a class="gk-breadcrumb" href="Default.aspx">Administrator</a> &gt; <span class="gk-breadcrumb-on">Edit User</span></div>
	<div class="gk-hr">&nbsp;</div>  
    
    <div class="data-tabled"  >
            <table>
                <caption><b> User Info</b>&nbsp;</caption>
                        <tr>
                                <td>
                                    &nbsp;Username&nbsp;</td>
                                <td>
                                    <asp:TextBox ID="txtUserName" Runat="server"  Enabled=false MaxLength="40" CssClass="size40_field" ></asp:TextBox>
                                </td>
                        </tr>
                    <tr>
                            <td>
                                &nbsp;Email
                            </td>
                            <td>
                                <asp:TextBox ID="txtEmail" Runat="server" MaxLength="200" CssClass="size40_field" ></asp:TextBox>&nbsp;
                            </td>
                     </tr>     
                     <tr>
                            <td>&nbsp;
                            </td> 
                            <td><asp:RegularExpressionValidator
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
    </div>
            <asp:Label ID="lblStatus" Runat="server" ForeColor="Red"></asp:Label>
        <br />
        <br />
        Assign Role to User<br />
        <asp:DropDownList ID="ddlRole" runat="server">
        </asp:DropDownList>
                            <asp:Button ID="btnAssign" Runat="server" Text="Assign" OnClick="btnAssign_Click"  /><br />
    </div>
    </asp:Content>

