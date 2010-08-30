<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ViewLocation.aspx.cs" MasterPageFile="~/AdminToolMasterPage.Master" Inherits="GateKeeperAdmin.Reports.ViewLocation" %>

<asp:Content ID="ViewLocationCnt" ContentPlaceHolderID="ContentPlaceHolderMaster" runat="server">
<div>&nbsp;</div>
<div id="ScheduleDiv" runat="server">
<asp:Panel DefaultButton="ibtnGo" runat="server">
<table cellpadding="2" cellspacing="0">
	<tr>
	    <td>
               Search by CdrID <asp:TextBox ID="txtCdrId" Width="60px" runat="server"></asp:TextBox>
	    </td>
        <td valign="bottom">
            <asp:ImageButton ID="ibtnGo" runat="server" ImageUrl="~/images/btn-go.gif" OnClick="ibtnGo_Click"/>
        </td>
    </tr>
</table>
</asp:Panel>
    <div>&nbsp;</div>
	<div id="idDataTabled" class="data-tabled" runat="server">
        <table cellpadding="4" cellspacing="0"">
            <tr>
                <th rowspan="2" scope="col"><asp:Label ID="lblCdrid" runat="server">Cdr Id</asp:Label></th>
                <th rowspan="2" scope="col"><asp:Label ID="lblDocType" runat="server">DocType</asp:Label><br />
				<asp:DropDownList ID="dropDocType" runat="server" CssClass="filter-dropdown" AutoPostBack="true" OnSelectedIndexChanged="dropDocType_SelectedIndexChanged"></asp:DropDownList></th>
                <th colspan="2" scope="col"><asp:Label ID="lblGK" runat="server">GateKeeper</asp:Label></th>
                <th colspan="2" scope="col"><asp:Label ID="lblStaging" runat="server">Staging</asp:Label></th>
                <th colspan="2" scope="col"><asp:Label ID="lblPreview" runat="server">Preview</asp:Label></th>
                <th colspan="2" scope="col"><asp:Label ID="lblLive" runat="server">Live</asp:Label></th>
            </tr>
            <tr>
                <th scope="col"><asp:Label ID="lblGKReqId" runat="server">Request ID</asp:Label></th>
                <th scope="col"><asp:Label ID="lblGKDate" runat="server">Date/Time</asp:Label></th>
                <th scope="col"><asp:Label ID="lblStagingReqId" runat="server">Request ID</asp:Label></th>
                <th scope="col"><asp:Label ID="lblStagingDate" runat="server">Date/Time</asp:Label></th>
                <th scope="col"><asp:Label ID="lblPreviewReqId" runat="server">Request ID</asp:Label></th>
                <th scope="col"><asp:Label ID="lblPreviewDate" runat="server">Date/Time</asp:Label></th>
                <th scope="col"><asp:Label ID="lblLiveReqId" runat="server">Request ID</asp:Label></th>
                <th scope="col"><asp:Label ID="lblLiveDate" runat="server">Date/Time</asp:Label></th>
            </tr>
                <asp:Repeater ID="rptLocation" runat="server">
                <HeaderTemplate>
                </HeaderTemplate>
                <ItemTemplate>
                    <tr>
                        <td><%# Eval("CdrId")%></td>
                        <td><%# Eval("DocType")%></td>
                        <td>
                            <%# (bool)Eval("IsPresentInGateKeeper") ? "<a href='../RequestHistory/RequestDetails.aspx?reqId=" + Eval("GKReqId") + "'>" + Eval("GKReqId") + "</a>" : "Not Present"%>
                        </td>
                        <td class="date"><%# (bool)Eval("HasDateInGateKeeper") ? Eval("GKDate").ToString() : "Not Present"%></td>
                        <td>
                            <%# (bool)Eval("IsPresentInStaging") ? "<a href='../RequestHistory/RequestDetails.aspx?reqId=" + Eval("StagingReqId") + "'>" + Eval("StagingReqId") + "</a>" : "Not Present"%>
                        </td>
                        <td class="date"><%# (bool)Eval("HasDateInStaging") ? Eval("StagingDate").ToString() : "Not Present"%></td>
                        <td>
                            <%# (bool)Eval("IsPresentInPreview") ? "<a href='../RequestHistory/RequestDetails.aspx?reqId=" + Eval("PreviewReqId") + "'>" + Eval("PreviewReqId") + "</a>" : "Not Present"%>
                        </td>
                        <td class="date"><%# (bool)Eval("HasDateInPreview") ? Eval("PreviewDate").ToString() : "Not Present"%></td>
                        <td>
                            <%# (bool)Eval("IsPresentInLive") ? "<a href='../RequestHistory/RequestDetails.aspx?reqId=" + Eval("LiveReqId") + "'>" + Eval("LiveReqId") + "</a>" : "Not Present"%>
                        </td>
                        <td class="date"><%# (bool)Eval("HasDateInLive") ? Eval("LiveDate").ToString() : "Not Present"%></td>
                    </tr>
                </ItemTemplate>
                <AlternatingItemTemplate>
                    <tr class="altrow">
                        <td><%# Eval("CdrId")%></td>
                        <td><%# Eval("DocType")%></td>
                        <td>
                            <%# (bool)Eval("IsPresentInGateKeeper") ? "<a href='../RequestHistory/RequestDetails.aspx?reqId=" + Eval("GKReqId") + "'>" + Eval("GKReqId") + "</a>" : "Not Present"%>
                        </td>
                        <td class="date"><%# (bool)Eval("HasDateInGateKeeper") ? Eval("GKDate").ToString() : "Not Present"%></td>
                        <td>
                            <%# (bool)Eval("IsPresentInStaging") ? "<a href='../RequestHistory/RequestDetails.aspx?reqId=" + Eval("StagingReqId") + "'>" + Eval("StagingReqId") + "</a>" : "Not Present"%>
                        </td>
                        <td class="date"><%# (bool)Eval("HasDateInStaging") ? Eval("StagingDate").ToString() : "Not Present"%></td>
                        <td>
                            <%# (bool)Eval("IsPresentInPreview") ? "<a href='../RequestHistory/RequestDetails.aspx?reqId=" + Eval("PreviewReqId") + "'>" + Eval("PreviewReqId") + "</a>" : "Not Present"%>
                        </td>
                        <td class="date"><%# (bool)Eval("HasDateInPreview") ? Eval("PreviewDate").ToString() : "Not Present"%></td>
                        <td>
                            <%# (bool)Eval("IsPresentInLive") ? "<a href='../RequestHistory/RequestDetails.aspx?reqId=" + Eval("LiveReqId") + "'>" + Eval("LiveReqId") + "</a>" : "Not Present"%>
                        </td>
                        <td class="date"><%# (bool)Eval("HasDateInLive") ? Eval("LiveDate").ToString() : "Not Present"%></td>
                    </tr>
                </AlternatingItemTemplate>
                <FooterTemplate>
                </FooterTemplate>
            </asp:Repeater>
        </table>

    </div>
</div>
</asp:Content>
