<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CGovHtml.aspx.cs" Inherits="CDRPreviewWS.CGovHtml" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml" lang="<%= currentLanguage %>">

     <head id="header">
      <meta http-equiv="X-UA-Compatible" content="IE=edge" />
      <meta charset="utf-8">
      <title>CDR Preview</title>
      <link rel="preconnect" crossorigin="" href="https://cdnjs.cloudflare.com" />
      <link rel="preconnect" crossorigin="" href="https://ajax.googleapis.com" />
      <link rel="preconnect" crossorigin="" href="https://fonts.gstatic.com" />
      <link rel="preconnect" href="https://static.cancer.gov" />
      <link id="gFonts" rel="stylesheet" href="https://fonts.googleapis.com/css?family=Noto+Sans:400,400i,700,700i" />
      <meta name="content-language" content="<%= currentLanguage %>">
      <meta name="english-linking-policy" content="<%=serverUrl%>/global/web/policies/exit">
      <meta name="espanol-linking-policy" content="<%=serverUrl%>/espanol/global/politicas/salda">
      <meta name="publishpreview" content="undefined">
      <link href="<%=serverUrl%>/PublishedContent/Styles/nvcg.css" rel="stylesheet">

      <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.1.1/jquery.min.js"
              type="text/javascript"></script>
      <script src="https://ajax.googleapis.com/ajax/libs/jqueryui/1.12.1/jquery-ui.min.js"
              type="text/javascript"></script>
      <script src="https://cdnjs.cloudflare.com/ajax/libs/headroom/0.9.3/headroom.min.js"
              type="text/javascript"></script>
      <script src="https://cdnjs.cloudflare.com/ajax/libs/jquery.touchswipe/1.6.18/jquery.touchSwipe.min.js"
              type="text/javascript"></script>
      <script src="<%=serverUrl%>/PublishedContent/js/Common.js"
              type="text/javascript"></script>
      <script src="<%=serverUrl%>/PublishedContent/js/ContentPage.js"
              type="text/javascript"></script>
      <script src="<%=serverUrl%>/PublishedContent/js/PDQPage.js"
              type="text/javascript"></script>
      <script src="<% =currentHost %>/CDRPreviewWS/common/popevents.js"
              type="text/javascript"></script>
      <script type="text/javascript">
        var NCIAnalytics = {
                displayAlerts: false,
                stringDelimiter: '|',
                fieldDelimiter: '~'
                };
      </script>
      <link href="<%=serverUrl%>/PublishedContent/Styles/PDQPage.css" rel="stylesheet" />
      <link href="<%=serverUrl%>/PublishedContent/Styles/InnerPage.css" rel="stylesheet" />
      <!-- 
        Temporary overwrite to prevent PP report for glossaries with
        images from floating text of the Spanish definition
      -->
      <style>
         dl.dictionary-list figure.image-left-medium { float: none; }
      </style>
    </head>

  <body class="pdqcancerinfosummary">
    
    <form id="form1" runat="server">
      <!-- CGov Container -->
      <div id="page">
        <!-- Empty Header element needed for JS calculations on Cancer.gov -->
        <div class="fixedtotop"><!-- dada --></div>
        
        <!-- Content Header -->
        <div id="headerzone">
            <!-- Load Header zone content here with help of user control -->
            <% =contentHeader %>
        </div>
        <!-- Main Area -->
        <!-- Left Navigation and Content Area -->
        <div  class="main-content" id="content">
            
              <!-- Main Content Area -->
              <div class="row general-page-body-container">
                <div id="nvcgSlSectionNav" 
                     class="medium-3 columns local-navigation">
                  <div class="section-nav">
                    <div class="level-0">
                      <div><a>Left Nav Goes Here</a></div>
                    </div>
                  </div>
                </div>
                <div class="medium-9 columns contentzone" 
                     id="main" tabindex="-1" role="main">
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
