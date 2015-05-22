<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CGovHtml.aspx.cs" Inherits="CDRPreviewWS.CGovHtml" %>

<!DOCTYPE html><html xmlns="http://www.w3.org/1999/xhtml">
<head id="header">
    <title>CDR Preview</title>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta name="content-language" content="<%= currentLanguage %>" />
    <meta name="english-linking-policy" content="<%=serverUrl%>/global/web/policies/exit" />
    <meta name="espanol-linking-policy" content="<%=serverUrl%>/espanol/global/politicas/salda" />
    <meta name="publishpreview" content="undefined" />
   
    <link href="<%=serverUrl%>/PublishedContent/Styles/nvcg.css" rel="stylesheet" />
    <!--[if lt IE 9]>
<script src="<%=serverUrl%>/PublishedContent/js/respond.js"></script>
<![endif]-->
    
    <!-- IE8 Polyfills -->
<!--[if lt IE 9]>
<script src="<%=serverUrl%>/PublishedContent/js/ie8-polyfills.js"></script>
<![endif]-->

    <script src="<% =currentHost %>/CDRPreviewWS/common/popevents.js" type="text/javascript"></script>    
    <script src="http://ajax.googleapis.com/ajax/libs/jquery/1.10.1/jquery.min.js" type="text/javascript"></script>
    <script src="http://ajax.googleapis.com/ajax/libs/jqueryui/1.11.1/jquery-ui.min.js" type="text/javascript"></script>
    <script src="<%=serverUrl%>/PublishedContent/js/modernizr.custom.2.7.1.js" type="text/javascript"></script>
    <script src="https://ajax.googleapis.com/ajax/libs/swfobject/2.2/swfobject.js" type="text/javascript"></script>
  
    <script src="<%=serverUrl%>/PublishedContent/js/jquery-scrolltofixed.js" type="text/javascript"></script>
    <script src="<%=serverUrl%>/PublishedContent/js/jquery-accessibleMegaMenu.js" type="text/javascript"></script>
    <script src="<%=serverUrl%>/PublishedContent/js/jQuery.headroom.js" type="text/javascript"></script>
    <script src="<%=serverUrl%>/PublishedContent/js/all.js" type="text/javascript"></script>
    <script src="<%=serverUrl%>/PublishedContent/js/STOC.js" type="text/javascript"></script>
    <script src="<%=serverUrl%>/PublishedContent/js/Enlarge.js" type="text/javascript"></script>
    <script src="<%=serverUrl%>/PublishedContent/js/PDQCIS.js" type="text/javascript"></script>
    <script src="<%=serverUrl%>/PublishedContent/js/routie.js" type="text/javascript"></script>
    <script src="<%=serverUrl%>/PublishedContent/js/nci-util.js" type="text/javascript"></script>
    <script src="<%=serverUrl%>/PublishedContent/js/jquery.jplayer.min.js" type="text/javascript"></script>

<!-- This script is only for ON THIS PAGE for CTGovProtocols
Once the PDQCIS has been refactored to address both summaries and protocols with one javascript this can be removed -->
    <script type="text/javascript">
        // This file is for the PDQ Cancer Information Summary UX functionality
        $(function() {
            //(function($) {
            // Add the container DIV to insert the SectionNav list
            // as the first element of the DIV.summary-sections container
            var topTocDiv = "<div id='pdq-toptoc' class='toptoc'></div>";
            $(".summary-sections > section:eq( 0 )").before(topTocDiv);


            // JQuery Function: stoc()
            // This function creates the table of contents for the article and
            // for the individual sections.
            // Document level TOC also includes a title 'ON THIS PAGE' and all
            // TOCs are wrapped in a <nav> element.
            // The TOC starts on H-level 3 and goes 2 levels deep for sections.
            // The TOC starts on H-level 2 and goes 3 levels deep for the article.
            // ------------------------------------------------------------------
            // Function to create the TOC (Table Of Contents)
            // ------------------------------------------------------------------
            $.fn.stoc = function(options) {
                //console.log("In stoc start");
                //Our default options
                var defaults = {
                    search: "body",        //where we will search for titles
                    depth: 6,              //how many hN should we search
                    start: 1,              //which hN will be the first (and after it we
                    //go just deeper)
                    stocTitle: "Contents", //what to display before our box
                    listType: "ul",        //could be ul or ol
                    tocTitleEn: "Table of content for this section",
                    tocTitleEs: "Tabla de contenidos para esta secci&#243;n",
                    beforeText: "", // can add <span class="text-class">
                    afterText: "" // can add </span> to match beforeText

                };


                //let's extend our plugin with default or user options when defined
                var options = $.extend(defaults, options);

                // Select the language tag to pick the proper text for headings
                // TBD:  Are KeyPoints H3 or H4???
                // ------------------------------------------------------------
                if ($('meta[name="content-language"]').attr('content') == 'es')
                    defaults.stocTitle = '<h6>' + defaults.tocTitleEs + '</h6>';
                else
                    defaults.stocTitle = '<h6>' + defaults.tocTitleEn + '</h6>';

                // If the title string is empty don't put out the H-tages
                if (defaults.stocTitle == '<h6></h6>') {
                    defaults.stocTitle = "";
                }

                // Need to identify if this is a HP or patient summary.  If it's a
                // patient summary we'll create KeyPoint boxes.
                // KeyPoint titles are H-tags with a type='keypoint' attribute
                if ($("h3[type='keypoint']").length
           + $("h4[type='keypoint']").length > 0) {
                    var kp = 1;
                }
                else {
                    var kp = 0;
                }


                return this.each(function() {
                    //"cache" our target and search objects
                    obj = $(this); //target
                    src = $(options.search); //search

                    // if container is not found.
                    if (!src || 0 === src.length) {
                        return;
                    }

                    //let's declare some variables. We need this var declaration to
                    //create them as local variables (not global)
                    var appHTML = "",
            tagNumber = 0,
            txt = "",
            id = "",
            beforeTxt = options.beforeText,
            afterTxt = options.afterText,
            previous = options.start,
            start = options.start,
            depth = options.depth,
            i = 0,
            srcTags = "h" + options.start,
            cacheHN = "";

                    // Turn off the KeyPoint header (coming from the TOC box) (VE)
                    if (kp == 1 && options.search != 'article')
                        options.stocTitle = "";

                    //which tags we will search
                    while (depth > 1) {
                        start++; //we will just get our start level and numbers higher than it
                        srcTags = srcTags + ", h" + start;
                        depth--; //since went one level up, our depth will go one level down
                    }
                    // if the target is not found
                    var found = src.find(srcTags);
                    if (!found || 0 === found.length) {
                        return;
                    }

                    found.each(function() {
                        //we will cache our current H element
                        cacheHN = $(this);
                        //if we are on h1, 2, 3...
                        tagNumber = (cacheHN.get(0).tagName).substr(1);

                        //sets the needed id to the element
                        //if it doesn't have one, of course
                        // --------------------------------------------------
                        id = cacheHN.attr('id');
                        if (id == "" || typeof id === "undefined") {
                            id = "stoc_h" + tagNumber + "_" + i;
                            cacheHN.attr('id', id);
                        }
                        //our current text
                        // using html() instead of text() since there could be markup
                        txt = cacheHN.html();

                        // Suppressing certain headings from TOC
                        // The KeyPoint headings are only displayed in the KeyPoint
                        // boxes (section level TOC) but not in the document
                        // level TOC.  That means we'll have to suppress the KP
                        // headings when searching on the body or article level but
                        // need to include them at the section level.
                        // Note:
                        //   The prototype is using a different section-level
                        //   structure.  A top-level section acts like an article
                        //   and the search needs to be adjusted.
                        // --------------------------------------------------------
                        if (options.search == 'body' || options.search == 'article') {
                            hAttr = cacheHN.attr("type");
                        }
                        else {
                            hAttr = '';
                        }

                        // There are certain headings that should never be included
                        // in the TOC, i.e. Key Points, References, Nav Headings.
                        // These are identified by a "do-not-show='toc'" attribute.
                        // ---------------------------------------------------------
                        var tocDNS = cacheHN.attr("do-not-show");

                        //if (txt != 'References' 
                        //        && txt.substring(0, 14) != 'Key Points for'
                        //        && txt.substring(0, 10) != 'Bibliograf'
                        //        && txt.substring(0, 14) != 'Puntos importa'
                        //        && hAttr != 'keypoint'
                        //        && txt != 'Sections') {
                        if (tocDNS != "toc") {
                            switch (true) {                //with switch(true) we can do
                                //comparisons in each case   
                                case (tagNumber > previous): //it means that we went down
                                    //one level (e.g. from h2 to h3)
                                    appHTML = appHTML + "<" + options.listType + ">"
                                          + "<li>"
                                          + beforeTxt
                                          + "<a href=\"#link/" + id + "\">"
                                          + txt
                                          + "</a>";
                                    previous = tagNumber;
                                    break;
                                case (tagNumber == previous): //it means that stay on the
                                    //same level (e.g. h3 and
                                    //stay on it)
                                    appHTML = appHTML + "</li>"
                                          + "<li>"
                                          + beforeTxt
                                          + "<a href=\"#link/" + id + "\">"
                                          + txt
                                          + "</a>";
                                    break;
                                case (tagNumber < previous): //it means that we went up but
                                    //we don't know how much levels
                                    //(e.g. from h3 to h2)
                                    while (tagNumber != previous) {
                                        appHTML = appHTML + "</" + options.listType + ">"
                                              + "</li>";
                                        previous--;
                                    }
                                    appHTML = appHTML + "<li>"
                                          + beforeTxt
                                          + "<a href=\"#link/" + id + "\">"
                                          + txt
                                          + "</a>";
                                    break;
                            }
                        }
                        i++;
                    });
                    //corrects our last item, because it may have some opened ul's
                    while (tagNumber != options.start && tagNumber > 0) {
                        appHTML = appHTML + "</" + options.listType + ">";
                        tagNumber--;
                    }

                    // Clean up our Percussion workaround entry
                    // We had to include text within the empty container div to prevent
                    // Percussion from messing up the divs.  Setting text to blank now
                    // ----------------------------------------------------------------
                    //if ( $("div.#pdq-toc-article").length == 1
                    //                         && $("div.keyPoints").length == 0 ) {
                    //    $("div.#_toc_section").text("");
                    //    }

                    //append our html to our object
                    appHTML = options.stocTitle
                  + "<" + options.listType + ">"
                  + appHTML
                  + "</" + options.listType + ">";

                    // In the special case when we encounter a patient summary that
                    // does contain citations (citations are only contained in HP
                    // summaries except for one CAM summary) we need to suppress
                    // the display of the TOC header without list items.
                    emptyHTML = options.stocTitle
                  + "<" + options.listType + ">"
                  + "</" + options.listType + ">";
                    if (appHTML != emptyHTML) {
                        var hideDocTOC = "";
                        if (options.search == 'article') hideDocTOC = " hide";
                        appHTML = "<nav class='on-this-page"
                      + hideDocTOC
                      + "'>"
                      + appHTML
                      + "</nav>"
                        obj.append(appHTML);
                    }

                });
                //console.log("In stoc End");
            };



            // *** END Functions *** ****************************************

            // Creating the TOC for CTGovProtocols
            var tocArticle = "<div id='pdq-toc-protocol'></div>";
            if ($("protocol").length > 0) {
                $("article h1").after(tocArticle);
            }
            
            // Then insert the list
            // The default TOC header for the document level TOC is 'ON THIS PAGE'
            $("#pdq-toc-protocol").stoc({ search: "article",
                start: 2, depth: 3,
                tocTitleEn: "ON THIS PAGE",
                tocTitleEs: "En esta p&#225;gina"
            });
            $("#pdq-toc-protocol nav").removeClass("hide");

            // Section to setup re-routing of URLs
            // -----------------------------------
            routie({
                'all': function() {
                    routie('section/all');
                },
                'section/:sid': function(sid) {
                    $(document).scrollTop(0);

                    if (sid == 'all') {
                        $("section.hide").removeClass("hide")
                             .addClass("show");
                        $("#pdq-toptoc li.selected").removeClass("selected");
                        $("#pdq-toptoc li.viewall").addClass("selected");

                        // Hide all the TOCs (section and doc level)
                        $("div nav.on-this-page").addClass("hide");
                        // ... and then just show the doc level TOC
                        $("#pdq-toc-article nav.on-this-page").removeClass("hide")
                                                  .addClass("show");
                        // ... and hide the Previous/Next navigation links
                        $("div.next-link").addClass("hide");
                        $("div.previous-link").addClass("hide");
                    }
                    else {
                        // Display of SummarySections
                        $("section.show").removeClass("show")
                             .addClass("hide");
                        $("section#" + sid).removeClass("hide")
                             .addClass("show");

                        // Highlighting of the section nav elements
                        var thisSection = $("section.show").children("h2")
                                              .attr("id");
                        $("#pdq-toptoc li.selected").removeClass("selected");
                        $("#pdq-toptoc li > span[show=" + thisSection + "]").closest("li")
                                                            .addClass("selected");

                        // Show all the TOCs (section and doc level)
                        $("div nav.on-this-page").removeClass("hide");
                        // ... and then hide just the doc level TOC
                        $("#pdq-toc-article nav.on-this-page").removeClass("show")
                                                  .addClass("hide");
                        // ... and show the Previous/Next navigation links
                        $("div.next-link").removeClass("hide");
                        $("div.previous-link").removeClass("hide");
                    }
                },
                'link/:rid': function(rid) {
                    // Hide all open sections
                    $(".summary-sections section.show").removeClass("show")
                                           .addClass("hide");
                    // Find parent (top level section) of current element
                    $("#" + rid).closest("section.hide").removeClass("hide")
                                          .addClass("show");
                    $("#pdq-toptoc li.selected").removeClass("selected");
                    var thisSection = $("section.show").children("h2")
                                           .attr("id");
                    $("#pdq-toptoc li.selected").removeClass("selected");
                    $("#pdq-toptoc li > span[show=" + thisSection + "]").closest("li")
                                                        .addClass("selected");
                    $("#" + rid)[0].scrollIntoView();

                    // According to Bryan positioning the link target below the 
                    // sticky menu is not part of this story
                    // ------------------------------------------------------------
                    // var myPos = Math.floor( $("div.nav-search-bar").position().top);
                    // var myHeight = Math.floor( $("div.nav-search-bar").height() );
                    // console.log("dada3");
                    // console.log(myPos);
                    // console.log(myHeight);
                    // $("#"+rid)[0].scrollIntoView();
                    // document.body.scrollTop -= myPos//+myHeight;
                },

                ':lid': function(lid) {
                    var goodLink = $("#" + lid);
                    console.log(goodLink);
                    if (goodLink.length == 0) {
                        routie('section/all');
                    }
                    else {
                        routie('link/' + lid);
                    };
                }
            });

        });

</script>
</head>
<body class="pdqcancerinfosummary">
    
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
