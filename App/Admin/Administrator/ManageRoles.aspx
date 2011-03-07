<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ManageRoles.aspx.cs" MasterPageFile="~/AdminToolMasterPage.Master" Inherits="GateKeeperAdmin.Administrator.ManagerRoles" %>

<asp:Content ID="ManageUsersPageCnt" ContentPlaceHolderID="ContentPlaceHolderMaster" runat="server">   
 <!-- start breadcrumb navigation -->
	 <div id="gk-home-contentwrap">
	<div class="gk-breadcrumbwrap" style="top: 160px;"><a class="gk-breadcrumb" href="../Home.aspx">Home</a> &gt; <a class="gk-breadcrumb" href="Default.aspx">Administrator</a> &gt; <span class="gk-breadcrumb-on">Manage Roles</span></div>
	<div class="gk-hr">&nbsp;</div> 
 
    <div class="data-tabled"  >
        <asp:GridView ID="grdItem"  Width="300" Runat="server"  AutoGenerateColumns=False  OnRowDeleting="grdItem_RowDeleting">
                 <Columns>   
                    <asp:BoundField ReadOnly="True" HeaderText="Role Name" DataField="Role" ></asp:BoundField>
                    <asp:CommandField ShowDeleteButton="True" ButtonType=Image DeleteImageUrl="~/Images/delete.gif" HeaderText="Delete"></asp:CommandField>
                 </Columns>
        </asp:GridView>
        <hr />
    </div>
        <div>
        <br />
        <asp:Label ID="lblStatus" Runat="server" ForeColor="Red"></asp:Label>
            <table  width=330>
                <caption><b> Add Role Info</b>&nbsp;</caption>
                <tr>
                        <td>
                            &nbsp;Role name&nbsp;</td>
                        <td>
                            <asp:TextBox ID="txtRoleName" Runat="server"  MaxLength="256" CssClass="size40_field" ></asp:TextBox>
                            <br />
                            <asp:RegularExpressionValidator ID="RegularExpressionValidator1" runat="server" ControlToValidate="txtRoleName"
                                EnableTheming="False" ErrorMessage="Must be a valid role name" ValidationExpression="\w+"></asp:RegularExpressionValidator></td>
                </tr>
                <tr>
                    <td colspan=2 align=center>
                            <asp:Button ID="btnAdd" Runat="server" Text="Add" OnClick="btnAdd_Click"  />&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                            <asp:Button ID="btnCancel" Runat="server" Text="Cancel" OnClick="btnCancel_Click" />&nbsp;
                    </td>
                </tr>
            </table>
        </div>
        </div>
</asp:Content>
