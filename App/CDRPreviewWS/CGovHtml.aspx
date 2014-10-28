<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CGovHtml.aspx.cs" Inherits="CDRPreviewWS.CGovHtml" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="header">
    <title>CDR Preview</title>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <link href="<%=serverUrl%>/PublishedContent/Styles/nci.css" rel="stylesheet" />
    <link href="<%=serverUrl%>/PublishedContent/Styles/nci-new.css" rel="stylesheet" />
    <link href="<%=serverUrl%>/PublishedContent/Styles/nciplus.css" rel="stylesheet" />
    <link href="<%=serverUrl%>/PublishedContent/Styles/emergency_IE.css" rel="stylesheet" />
    <%--<link href="<%=serverUrl%>/PublishedContent/Styles/jquery-ui-1.8.5.custom.css" rel="stylesheet" />--%>

    <script src="<% =currentHost %>/CDRPreviewWS/common/popevents.js" type="text/javascript"></script>    
    <script src="http://ajax.googleapis.com/ajax/libs/jquery/1.5.1/jquery.min.js" type="text/javascript"></script>
    <script src="<%=serverUrl%>/PublishedContent/js/modernizr-1.7.min.js" type="text/javascript"></script>
    <script src="<%=serverUrl%>/PublishedContent/js/NCIGeneralJS.js" type="text/javascript"></script>
    <%--<script src="<%=serverUrl%>/PublishedContent/js/jquery.ui.position.js" type="text/javascript"></script>--%>
    <script src="https://ajax.googleapis.com/ajax/libs/swfobject/2.2/swfobject.js" type="text/javascript"></script>
    <script src="<% =currentHost %>/CDRPreviewWS/common/wcmsAudio.js" type="text/javascript"></script>
    <script src="<%=serverUrl%>/PublishedContent/js/STOC.js" type="text/javascript"></script>
    <script src="<%=serverUrl%>/PublishedContent/js/Enlarge.js" type="text/javascript"></script>
<!-- This is to make content width follow different rules for IE7 and below -->
<!--[if lt IE 8]>
<style type="text/css">
.contentzone-defaultTemplateContentContainer{
	width: 650px !important;
	padding-right: 50px !important;
}
.cgvBody-defaultTemplateContentContainer {
	padding-right: 0 !important;
  width: auto !important;
}
ul.ctpListPageList {
	width: 400px !important;
}
</style>
<![endif]-->
    
</head>
<body class="ncigeneral">
    <form id="form1" runat="server">
    <!-- CGov Container -->
    <div id="cgovContainer">
        <!-- Site Banner -->
        <div class="skip">
            <a title="Skip to content" href="#skiptocontent">Skip to content</a></div>
        <div id="cgvSiteBanner">
        </div>
        <div id="cgvMainNav">
        </div>
        <!-- Content Header -->
        <div id="headerzone">
            <!-- Load Header zone content here with help of user control -->
            <% =contentHeader %>
        </div>
        <!-- Main Area -->
        <!-- Left Navigation and Content Area -->
        <div id="mainContainer">
            <!-- Left Nav Column -->
            <div id="leftzone">
                <div id="cgvFindACancerTypeSlot" class="LeftNavSlot">
                </div>
                <div id="cgvLeftNav" class="LeftNavSlot">
                </div>
            </div>
            <!-- End Left Nav -->
            <!-- Main Content Area -->
            <div class="contentzone contentzone-defaultTemplateContentContainer">
                <a name="skiptocontent"></a>
                <article>
                    <div id="cgvBody" class="cgvBody-defaultTemplateContentContainer">
                        <% =result %>
                    </div>
                </article>
            </div>
            <!-- End Content Area -->
        </div>
        <!-- End Left Navigation and Content Area -->
        <!-- End Main Area -->
        <!-- Footer -->
        <div id="cgvFooter" removeifempty="false">
            
        </div>
        <!-- End Foooter-->
    </div>
    <!-- End CGovContainer-->
    </form>
</body>
</html>
