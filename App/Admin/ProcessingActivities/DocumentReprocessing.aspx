<%@ Page Language="C#" MasterPageFile="~/AdminToolMasterPage.Master" AutoEventWireup="true"
    CodeBehind="DocumentReprocessing.aspx.cs" Inherits="GateKeeperAdmin.ProcessingActivities.DocumentReprocessing" %>

<asp:Content ID="ViewLocationCnt" ContentPlaceHolderID="ContentPlaceHolderMaster"
    runat="server">
    <!-- start breadcrumb navigation -->
    <div class="gk-breadcrumbwrap">
        <a class="gk-breadcrumb" href="../Home.aspx">Home</a> &gt; <span class="gk-breadcrumb-on">
            Reprocess Documents</span></div>
    <div style="padding-left: 6px; margin-bottom: 10px">
        <table cellpadding="6" cellspacing="0">
            <tr>
                <td>
                    Select Document Type:
                </td>
                <td>
                    <asp:DropDownList ID="DocTypeDropDown" runat="server" CssClass="filter-dropdown"
                        AutoPostBack="true" OnSelectedIndexChanged="DropDown_SelectedIndexChanged">
                    </asp:DropDownList>
                </td>
                <td>
                    |
                </td>
                <td valign="bottom">
                    <asp:ImageButton ID="ibtnGo" runat="server" ImageUrl="~/images/btn-go.gif" OnClick="ibtnGo_Click" />
                </td>
                <td>
                    |
                </td>
                <td>
                    <asp:CustomValidator ID="cvIsConfirmed" runat="server" ClientValidationFunction="checkBoxSelected"
                        Display="Dynamic" ErrorMessage="Please confirm by checking 'Yes, I really want to do this'" ValidationGroup="header">*</asp:CustomValidator>
                    <asp:CustomValidator ID="cvIsConfirmed2" runat="server" ClientValidationFunction="checkBoxSelected"
                        Display="Dynamic" ErrorMessage="Please confirm by checking 'Yes, I really want to do this'" ValidationGroup="vgSelectedInList">*</asp:CustomValidator>
                        
                    <asp:CheckBox runat="server" ID="chkReallyWant" Text=" Yes, I really want to do this." />
                </td>
                <td>
                    |
                </td>
                <td>
                    <asp:ImageButton ID="btnScheduleSelected" runat="server" ImageUrl="~/images/gk-schedule-selected.gif"
                        OnClick="btnScheduleSelected_Click" ValidationGroup="vgSelectedInList"    />
                </td>
                <td>
                    |
                </td>
                <td>
                    <asp:ImageButton ID="btnScheduleAll" runat="server" ImageUrl="~/images/gk-schedule-all.gif"
                        OnClick="btnScheduleAll_Click" ValidationGroup="header" />
                </td>
            </tr>
        </table>
    </div>
    <asp:Panel class="data-tabled" runat="server" ID="pnlResults">
        <table border="0" cellpadding="4" cellspacing="0" id="tblMain">
            <tr>
                <th class="th1" scope="col">
                    <asp:LinkButton ID="lnkSelectAll" runat="server" CssClass="redtext" OnClick="SelectAll_Click">Select All</asp:LinkButton><br />
                    <asp:LinkButton ID="lnkClearAll" runat="server" CssClass="redtext" OnClick="ClearAll_Click">Clear All</asp:LinkButton><br />
                </th>
                <th scope="col">
                    Request Date
                </th>
                <th scope="col">
                    Request Id
                </th>
                <th scope="col">
                    External Id
                </th>
                <th scope="col">
                    DTD Version
                </th>
                <th scope="col">
                    CDR ID
                </th>
                <th scope="col" style="width:350px">
                    Title
                </th>
            </tr>
            <asp:Repeater ID="rptRequestsWithCurrentDTD" runat="server">
                <HeaderTemplate>
                </HeaderTemplate>
                <ItemTemplate>
                    <tr>
                        <td>
                            <asp:CheckBox ID="ChkBox" runat="server" /><asp:Label ID="GroupIdLbl" runat="server"
                                Visible="false" Text='<%# Eval("GroupId")%>'></asp:Label>
                        </td>
                        <td>
                            <%# Eval("CompleteReceivedTime")%>
                        </td>
                        <td><asp:Label ID="RequestDataIdLbl" runat="server" Visible="false" Text='<%# Eval("RequestDataID")%>'></asp:Label>
                        <asp:Label ID="RequestIdLbl" runat="server" Visible="false" Text='<%# Eval("RequestID")%>'></asp:Label>
                            <a href="../RequestHistory/RequestDetails.aspx?reqId=<%# Eval("RequestID")%>">
                                <%# Eval("RequestID")%></a>
                        </td>
                        <td>
                            <%# Eval("ExternalRequestID")%>
                        </td>
                        <td>
                            <%# Eval("GateKeeperDTDVersion")%>
                        </td>
                        <td>
                            <a href="../RequestHistory/RequestCDRID.aspx?reqdataid=<%# Eval("RequestDataID") %>&reqid=<%# Eval("RequestID")%>">
                                <%# Eval("CdrId")%></a>
                        </td>
                        <td class="left"> 
                            <%# Eval("Title")%>
                        </td>
                    </tr>
                </ItemTemplate>
                <FooterTemplate>
                </FooterTemplate>
            </asp:Repeater>
            <asp:Repeater ID="rptRequestsWithOlderDTD" runat="server">
                <HeaderTemplate>
                    <table cellpadding="4" cellspacing="0"">
                        <tr>
                            <th scope="col">
                                GK RequestId
                            </th>
                            <th scope="col">
                                GK DTD Version
                            </th>
                        </tr>
                </HeaderTemplate>
                <ItemTemplate>
                    <tr>
                        <td class="left">
                            <asp:Label ID="lblTableName" runat="server" Text='<%# Eval("GKReqId")%>'></asp:Label>
                        </td>
                        <td>
                            <%# Eval("GateKeeperDTDVersion")%>
                        </td>
                    </tr>
                </ItemTemplate>
                <AlternatingItemTemplate>
                    <tr class="altrow">
                        <td class="left">
                            <asp:Label ID="lblTableName" runat="server" Text='<%# Eval("GKReqId")%>'></asp:Label>
                        </td>
                        <td>
                            <%# Eval("GateKeeperDTDVersion")%>
                        </td>
                    </tr>
                </AlternatingItemTemplate>
                <FooterTemplate>
                    </table>
                </FooterTemplate>
            </asp:Repeater>
        </table>
    </asp:Panel>
    <asp:CustomValidator ID="chIsSelectedInList" runat="server" ClientValidationFunction="checkBoxSelectedInList"
        ErrorMessage="Please make a selection from the list and then click 'Schedule Selected'."
        ValidationGroup="vgSelectedInList" ></asp:CustomValidator>
        
    <asp:ValidationSummary ID="ValidationSummary2" runat="server" ValidationGroup="header"
        ShowMessageBox="true" ShowSummary="false" />        
        
    <asp:ValidationSummary ID="ValidationSummary1" runat="server" ValidationGroup="vgSelectedInList"
        ShowMessageBox="true" ShowSummary="false" />
</asp:Content>
