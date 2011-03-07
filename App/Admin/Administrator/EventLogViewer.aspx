<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/AdminToolMasterPage.Master" CodeBehind="EventLogViewer.aspx.cs" Inherits="GateKeeperAdmin.Administrator.EventLogViewer" %>
<%@ Register Assembly="NCILibrary.Web.UI.WebControls" Namespace="NCI.Web.UI.WebControls" TagPrefix="NCI" %>

<asp:Content ID="HomePageCnt" ContentPlaceHolderID="ContentPlaceHolderMaster" runat="server">
    <asp:DropDownList ID="dropLogNames" runat="server" AutoPostBack="true" 
        OnSelectedIndexChanged="dropLogNames_SelectedIndexChanged" style="margin-top:10px">
    </asp:DropDownList>
<div>&nbsp;</div>
    <NCI:EventLogViewer ID="elvViewer" runat="server" Log="GateKeeper"/>
</asp:Content>
