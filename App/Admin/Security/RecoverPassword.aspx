<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RecoverPassword.aspx.cs"  MasterPageFile="~/AdminToolMasterPage.Master" Inherits="GateKeeperAdmin.Security.RecoverPassword" %>

<asp:Content ID="ManageUsersPageCnt" ContentPlaceHolderID="ContentPlaceHolderMaster" runat="server">    
    <div id="gk-home-contentwrap">
        <asp:PasswordRecovery ID="PasswordRecovery1" runat="server" >
        </asp:PasswordRecovery>
    </div>
</asp:Content>


