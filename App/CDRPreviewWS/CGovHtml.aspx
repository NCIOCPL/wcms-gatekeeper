<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CGovHtml.aspx.cs" Inherits="CDRPreviewWS.CGovHtml" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="header">
    <title>CDR Preview</title>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta name="content-language" content="<%= currentLanguage %>" />
   
    <link href="<%=serverUrl%>/PublishedContent/Styles/nvcg.css" rel="stylesheet" />
    
    <script src="<% =currentHost %>/CDRPreviewWS/common/popevents.js" type="text/javascript"></script>    
    <script src="http://ajax.googleapis.com/ajax/libs/jquery/1.10.1/jquery.min.js" type="text/javascript"></script>
    <script src="http://ajax.googleapis.com/ajax/libs/jqueryui/1.11.1/jquery-ui.min.js" type="text/javascript"></script>
    <script src="<%=serverUrl%>/PublishedContent/js/modernizr.custom.2.7.1.js" type="text/javascript"></script>
    <script src="<%=serverUrl%>/PublishedContent/js/NCIGeneralJS.js" type="text/javascript"></script>
    <script src="https://ajax.googleapis.com/ajax/libs/swfobject/2.2/swfobject.js" type="text/javascript"></script>
    <script src="<% =currentHost %>/CDRPreviewWS/common/wcmsAudio.js" type="text/javascript"></script>
    <script src="<%=serverUrl%>/PublishedContent/js/jquery-scrolltofixed.js" type="text/javascript"></script>
    <script src="<%=serverUrl%>/PublishedContent/js/jquery-accessibleMegaMenu.js" type="text/javascript"></script>
    <script src="<%=serverUrl%>/PublishedContent/js/jQuery.headroom.js" type="text/javascript"></script>
    <script src="<%=serverUrl%>/PublishedContent/js/all.js" type="text/javascript"></script>
    <script src="<%=serverUrl%>/PublishedContent/js/STOC.js" type="text/javascript"></script>
    <script src="<%=serverUrl%>/PublishedContent/js/Enlarge.js" type="text/javascript"></script>
    <script src="<%=serverUrl%>/PublishedContent/js/PDQCIS.js" type="text/javascript"></script>
    <script src="<%=serverUrl%>/PublishedContent/js/routie.js" type="text/javascript"></script>
    
</head>
<body>
    
    <form id="form1" runat="server">
    <!-- CGov Container -->
    <div id="page">
        
        <!-- Content Header -->
        <div id="headerzone">
            <!-- Load Header zone content here with help of user control -->
            <% =contentHeader %>
        </div>
        <!-- Main Area -->
        <!-- Left Navigation and Content Area -->
        <div  class="main-content" id="content">
            
            <!-- Main Content Area -->
             <div class="row">
                <div id="nvcgSlSectionNav" class="medium-3 columns local-navigation"><div class="section-nav"><div class="level-0"><div><a>Left Nav Goes Here</a></div></div></div></div>
                <div class="medium-9 columns contentzone" id="main" tabindex="0" role="main">
                    <article>
                        <div id="cgvBody">
                            <% =result %>
                        </div>
                    </article>
                </div>
            </div>
            <!-- End Content Area -->
        </div>
        <!-- End Left Navigation and Content Area -->
        <!-- End Main Area -->
       
    </div>
    <!-- End CGovContainer-->
    </form>
</body>
</html>
