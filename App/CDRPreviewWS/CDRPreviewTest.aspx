<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CDRPreviewTest.aspx.cs" Inherits="CDRPreviewWS.CDRPreviewTest" EnableEventValidation="false" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>CDR Preview</title>
        <meta http-equiv="content-type" content="text/html;charset=UTF-8" />
        <link rel="stylesheet" href="http://www.cancer.gov/stylesheets/nci.css" type="text/css"/>
		<link rel="stylesheet" href="http://www.cancer.gov/stylesheets/nci_general_browsers.css" type="text/css"/>
        <script language="JavaScript" src="http://www.cancer.gov/scripts/popEvents.js" type="text/jscript"></script>
 </head>
<body>
    <form id="form1" runat="server">
        <div>
        <asp:Panel ID="Panel1" runat="server" Height="250px" Width="500px">
            RequestID:&nbsp;
            <asp:TextBox ID="requestID" runat="server"></asp:TextBox>
            &nbsp; &nbsp; &nbsp;&nbsp;<br />
            &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;<br />
            CDRID: &nbsp; &nbsp; &nbsp;<asp:TextBox ID="cdrID" runat="server"></asp:TextBox>
            &nbsp; &nbsp; &nbsp; DocumentType:
            <asp:DropDownList ID="dropDownList" runat="server">
                <asp:ListItem>Summary</asp:ListItem>
                <asp:ListItem Value="Protocol_HP">Protocol_HP</asp:ListItem>
                <asp:ListItem Value="Protocol_Patient">Protocol_Patient</asp:ListItem>
                <asp:ListItem>CTGovProtocol</asp:ListItem>
                <asp:ListItem>GlossaryTerm</asp:ListItem>
                <asp:ListItem>DrugInfoSummary</asp:ListItem>
                <asp:ListItem>GeneticsProfessional</asp:ListItem>
            </asp:DropDownList><br />
            <br />
            &nbsp;<asp:Button ID="btnPreview" runat="server" Height="30px" OnClick="btnPreview_Click"
                Style="position: static" Text="Preview" Width="126px" />
      </asp:Panel>
    </div>

    <br />
    <div align="center">
        <table width="100%" cellspacing="0" cellpadding="0" border="0">
           <tr><td><%=result%></td></tr>
        </table>
    </div>
    </form>
</body>
</html>
