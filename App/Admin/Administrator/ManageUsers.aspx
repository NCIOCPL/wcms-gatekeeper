<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ManageUsers.aspx.cs" MasterPageFile="~/AdminToolMasterPage.Master" Inherits="GateKeeperAdmin.Administrator.ManageUsers" %>

    <asp:Content ID="ManageUsersPageCnt" ContentPlaceHolderID="ContentPlaceHolderMaster" runat="server">  
     <!-- start breadcrumb navigation -->
    <div id="gk-home-contentwrap">
	<div class="gk-breadcrumbwrap" style="top: 160px;"><a class="gk-breadcrumb" href="../Home.aspx">Home</a> &gt; <a class="gk-breadcrumb" href="Default.aspx">Administrator</a> &gt; <span class="gk-breadcrumb-on">Manage Users</span></div>
    <div class="gk-hr">&nbsp;</div>
    <div class="data-tabled"  >
        <asp:GridView ID="grdItem" Runat="server"  Width="600" AutoGenerateColumns="False">
                 <Columns>
                  <asp:templatefield>
                        <itemtemplate>
                        <asp:linkButton runat="server" id="Edit" text="Edit" commandname="Edit" commandargument='<%#Eval("UserName")%>' forecolor="black" oncommand="LinkButtonClick"/>
                        </itemtemplate>
                    </asp:templatefield> 
                    <asp:templatefield>
                        <itemtemplate>
                        <asp:linkButton runat="server" id="Delete" text="Delete" commandname="Delete" commandargument='<%#Eval("UserName")%>' forecolor="black" oncommand="LinkButtonClick"/>
                        </itemtemplate>
                    </asp:templatefield>
                    
                    <asp:BoundField ReadOnly="True" HeaderText="User Name" DataField="username"></asp:BoundField>
                    <asp:BoundField ReadOnly="True" HeaderText="Email" DataField="Email"></asp:BoundField>
                    <asp:BoundField ReadOnly="True" HeaderText="Create Date" DataField="CreationDate"></asp:BoundField>
                    <asp:BoundField ReadOnly="True" HeaderText="Last Login Date" DataField="LastLoginDate"></asp:BoundField>
                    <asp:BoundField ReadOnly="True" HeaderText="Role" DataField="Role" ></asp:BoundField>
                   <asp:templatefield>
                        <itemtemplate>
                        <asp:linkButton runat="server" id="DeleteRole" text="Delete Role" Visible='<% #Eval("Role").ToString() != "" %>' commandname="DeleteRole" commandargument='<%#Eval("UserName")%>' forecolor="black" oncommand="LinkButtonClick"/>
                        </itemtemplate>
                    </asp:templatefield>
                    
                  </Columns>
            </asp:GridView>
        <hr />
    </div>
            <br />
            <asp:Label ID="lblStatus" Runat="server" ForeColor="Red"></asp:Label>
            <table width="530">
                <caption>
                    <b>Add User</b></caption>
                <tr>
                    <td>
                        &nbsp;Username&nbsp;</td>
                    <td colspan="2">
                        <asp:TextBox ID="txtUserName" runat="server" CssClass="size40_field" MaxLength="40"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td>
                        &nbsp;Email
                    </td>
                    <td>
                        <asp:TextBox ID="txtEmail" runat="server" CssClass="size40_field" MaxLength="200"></asp:TextBox>
                    </td>
                    <td>    <asp:RegularExpressionValidator ID="RegularExpressionValidator1" runat="server" ControlToValidate="txtEmail"
                            ErrorMessage="Invalid email" ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"></asp:RegularExpressionValidator>
                    </td>
                </tr>
                <tr>
                    <td>
                        &nbsp;Password
                    </td>
                    <td colspan="2">
                        <asp:TextBox ID="txtPassword" runat="server" CssClass="size40_field" TextMode="Password" MaxLength="20"></asp:TextBox>
                    </td>
                </tr>
                 <tr>
                    <td>
                        &nbsp;Password Confirmation 
                    </td>
                    <td>
                        <asp:TextBox ID="txtPasswordConfirmation" runat="server" CssClass="size40_field" TextMode="Password" MaxLength="20"></asp:TextBox>
                     </td>
                    <td>    <asp:CompareValidator ID="CompareValidator1" runat="server" ErrorMessage="CompareValidator" ControlToCompare="txtPassword" ControlToValidate="txtPasswordConfirmation">Unmatched Passwords</asp:CompareValidator>
                        </td>
                </tr>
                <tr>
                    <td style="height: 22px">
                        &nbsp;Password Question</td>
                    <td colspan="2" style="height: 22px">
                        <asp:TextBox ID="txtPasswordQuestion" runat="server" CssClass="size60_field" MaxLength="256"></asp:TextBox>&nbsp;
                    </td>
                </tr>
                <tr>
                    <td>
                        &nbsp;Password Answer</td>
                    <td colspan="2">
                        <asp:TextBox ID="txtPasswordAnswer" runat="server" CssClass="size60_field" MaxLength="128"></asp:TextBox>&nbsp;
                    </td>
                </tr>
                <tr>
                    <td colspan="3" align=center>
                        <asp:Button ID="btnAdd" runat="server" OnClick="btnAdd_Click" Text="Add" />
                        &nbsp; &nbsp; &nbsp;&nbsp;
                        <asp:Button ID="btnCancel" runat="server" OnClick="btnCancel_Click" Text="Cancel" />&nbsp;
                    </td>
                </tr>
            </table>
            
        </div>
    </asp:Content>
