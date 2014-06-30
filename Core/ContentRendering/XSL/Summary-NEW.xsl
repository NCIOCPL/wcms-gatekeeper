<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method = "html"/>
  <xsl:param                      name = "section"/>
  <xsl:param                      name = "default.table.width" 
                                select = "''"/>

  <xsl:template match="/Summary">
   <xsl:variable                  name = "page"
                                select = "SummarySection[$section]"/>

<html>
 <head>
  <title>
   <xsl:text>WCMS Summary: </xsl:text>
   <xsl:value-of             select = "SummaryTitle"/>
  </title>
  <link   href="http://www.cancer.gov/PublishedContent/Styles/nvcg.css" 
          type="text/css" rel="StyleSheet" />
  <script type="text/javascript" 
           src="http://ajax.googleapis.com/ajax/libs/jquery/1.5.1/jquery.min.js"></script>
  <script type="text/javascript">
// wrap a div with overflow: auto around all tables in the body 
$(document).ready(function() { 
$('.contentzone table').wrap('<div style="overflow: auto;"></div>'); 
});
</script>
 </head>
 <body>
  <div class="contentzone">
  <article>
   <xsl:apply-templates     select = "SummaryMetaData"/>
   <xsl:apply-templates     select = "SummaryTitle"/>

   <!--
   We have to loop over each top section in order to set the numbering
   for the References section
   This is also where we're creating the KeyPoints box and the TOC
   =================================================================== -->
   <xsl:for-each                select = "$page">
    <xsl:variable                 name = "topSection">
     <xsl:text>section_</xsl:text><xsl:number/>
    </xsl:variable>

    <section>

    <!-- The section title of the top SummarySection has to be
         printed above the keypoints box or the TOC -->
    <xsl:apply-templates       select = "Title"/>

    <!-- KeyPoints Box -->
    <xsl:choose>
     <xsl:when                    test = "../descendant::KeyPoint">
      <xsl:call-template          name = "keypointsbox"/>
     </xsl:when>
    <!-- Table of Contents -->
     <xsl:when                    test = "SummarySection/Title">
      <div class="on-this-page">
       <h4>Table of Contents for this page</h4>
       <!--
       <xsl:call-template          name = "toc"/>
       -->
       <xsl:if                     test = "SummarySection[Title]">
       <ul>
       <xsl:apply-templates      select = "SummarySection"
                                   mode = "toc"/>
       </ul>
       </xsl:if>
      </div>
     </xsl:when>
    </xsl:choose>

    <xsl:apply-templates        select = "*[not(self::Title)]">
     <xsl:with-param              name = "topSection"
                                select = "$topSection"/>
    </xsl:apply-templates>

    </section>
   </xsl:for-each>
  </article>
  </div>
  </body>
</html>
  </xsl:template>

  <!--
  Template to display basic meta data information
  *** for testing only ***
  ================================================================ -->
  <xsl:template                  match = "SummaryMetaData">
    <div style="background-color: yellow;">
    <p>
     <xsl:apply-templates     select="../@id"/>
     <xsl:text>;</xsl:text>
     <xsl:apply-templates     select="SummaryType"/>
     <xsl:text>;</xsl:text>
     <xsl:apply-templates     select="SummaryAudience"/>
     <xsl:text>;</xsl:text>
     <xsl:apply-templates     select="SummaryLanguage"/>
    </p>
    </div>
  </xsl:template>

  <!--
  Template to remove display of SectionMetaData on any level
  ================================================================ -->
  <xsl:template                  match = "SectMetaData">
    <!-- This template is empty -->
  </xsl:template>

  <!--
  ================================================================ -->
  <xsl:template                  match = "SummaryTitle">
    <!-- h1><xsl:apply-templates/></h1 -->
    <h1>
     <xsl:value-of                select = "substring-before(.,
                                                      '(PDQ')"/>
     <xsl:text>(PDQ</xsl:text>
     <xsl:element                  name = "sup">
      <xsl:text>&#174;</xsl:text>
     </xsl:element>
     <xsl:text>)</xsl:text>
    </h1>
  </xsl:template>

  <!--
  Template for Top SummarySections
  ================================================================ -->
  <xsl:template                  match = "SummarySection">
   <xsl:param                     name = "topSection" 
                                select = "'sub'"/>
    <xsl:apply-templates>
     <xsl:with-param          name = "topSection"
                            select = "$topSection"/>
    </xsl:apply-templates>
  </xsl:template>

  <!--
  Display the Keypoints as Titles within the text
  ================================================================ -->
  <xsl:template                  match = "KeyPoint">
   <xsl:element                   name = "h3">
    <xsl:attribute                name = "id">
     <xsl:value-of              select = "@id"/>
    </xsl:attribute>
    <xsl:apply-templates/>
   </xsl:element>
  </xsl:template>


  <!--
  Create the section titles
  ================================================================ -->
  <xsl:template                  match = "Title">
   <xsl:param                     name = "topSection" 
                                select = "'title'"/>
   <xsl:choose>
    <xsl:when                     test = "$topSection = 'title'">
     <h3><xsl:apply-templates/></h3>
    </xsl:when>
    <xsl:otherwise>
     <h4><xsl:apply-templates/></h4>
    </xsl:otherwise>
   </xsl:choose>
  </xsl:template>

  <!--
  ================================================================ -->
  <xsl:template                  match = "Para">
   <xsl:param                     name = "topSection" 
                                select = "'para'"/>
   <xsl:element                   name = "p">
    <xsl:attribute                name = "id">
     <xsl:value-of              select = "@id"/>
    </xsl:attribute>
    <xsl:attribute                name = "tabindex">
     <xsl:text>0</xsl:text>
    </xsl:attribute>
    <xsl:apply-templates>
     <xsl:with-param          name = "topSection"
                            select = "$topSection"/>
    </xsl:apply-templates>
   </xsl:element>
  </xsl:template>

  <!--
  ================================================================ -->
  <xsl:template                  match = "ItemizedList">
   <xsl:param                     name = "topSection" 
                                select = "'il'"/>
   <xsl:element                   name = "ul">
    <xsl:attribute                name = "id">
     <xsl:value-of              select = "@id"/>
    </xsl:attribute>
    <xsl:apply-templates>
     <xsl:with-param          name = "topSection"
                            select = "$topSection"/>
    </xsl:apply-templates>
   </xsl:element>
  </xsl:template>

  <!--
  ================================================================ -->
  <xsl:template                  match = "OrderedList">
   <xsl:param                     name = "topSection" 
                                select = "'ol'"/>
   <xsl:element                   name = "ol">
    <xsl:attribute                name = "id">
     <xsl:value-of              select = "@id"/>
    </xsl:attribute>
    <xsl:apply-templates>
     <xsl:with-param          name = "topSection"
                            select = "$topSection"/>
    </xsl:apply-templates>
   </xsl:element>
  </xsl:template>

  <!--
  ================================================================ -->
  <xsl:template                  match = "ListItem">
   <xsl:param                     name = "topSection" 
                                select = "'li'"/>
   <li>
    <xsl:apply-templates>
     <xsl:with-param          name = "topSection"
                            select = "$topSection"/>
    </xsl:apply-templates>
   </li>
  </xsl:template>

  <!--
  Template to display the citations in the Reference Section
  ================================================================ -->
  <xsl:template                  match = "ReferenceSection">
   <xsl:param                     name = "topSection" 
                                select = "'ref'"/>
   <h3>References</h3>

    <ol>
     <xsl:for-each              select = "Citation">
      <li>
       
       <xsl:element               name = "a">
        <xsl:attribute            name = "name">
         <xsl:value-of          select = "$topSection"/>
         <xsl:text>.</xsl:text>
         <xsl:value-of          select = "@idx"/>
        </xsl:attribute>
        <!-- Little Problem -->
        <xsl:text> </xsl:text>
       </xsl:element>

       <xsl:value-of            select = "."/>
       <xsl:if                    test = "@PMID">
        <xsl:element              name = "a">
         <xsl:attribute           name = "href">
          <xsl:text>http://www.ncbi.nlm.nih.gov/entrez/query.fcgi</xsl:text>
          <xsl:text>?cmd=Retrieve&amp;db=PubMed&amp;list_uids=</xsl:text>
          <xsl:value-of          select = "@PMID"/>
          <xsl:text>&amp;dopt=Abstract</xsl:text>
         </xsl:attribute>
         <xsl:text>[PUBMED Abstract]</xsl:text>
        </xsl:element>
       </xsl:if>
      </li>
     </xsl:for-each>
    </ol>
  </xsl:template>

  <!--
  ================================================================ -->
  <xsl:template                  match = "Reference">
   <xsl:param                     name = "topSection" 
                                select = "'cit'"/>
    <xsl:text>[</xsl:text>
    <xsl:element                  name = "a">
     <xsl:attribute               name = "href">
      <xsl:text>#</xsl:text>
      <xsl:value-of             select = "$topSection"/>
      <xsl:text>.</xsl:text>
      <xsl:value-of             select = "@refidx"/>
     </xsl:attribute>
     <xsl:value-of             select = "@refidx"/>
    </xsl:element>
    <xsl:text>]</xsl:text>
  </xsl:template>

  <!--
  ================================================================ -->
  <xsl:template                  match = "Strong">
    <strong><xsl:apply-templates/></strong>
  </xsl:template>

  <!--
  ================================================================ -->
  <xsl:template                  match = "GlossaryTermRef">
   <xsl:element                   name = "a">
    <xsl:attribute                name = "class">
     <xsl:text>definition</xsl:text>
    </xsl:attribute>
    <xsl:attribute                name = "href">
     <!--
     Next Line For Testing on DEV only !!! 
     ===================================== -->
     <xsl:text>http://www.cancer.gov</xsl:text>
     <xsl:text>/Common/PopUps/popDefinition.aspx?id=</xsl:text>
     <xsl:value-of              select = "number(
                                           substring-after(@href, 'CDR'))"/>
     <xsl:text>&amp;version=Patient&amp;language=English</xsl:text>
    </xsl:attribute>
    <xsl:attribute                name = "onclick">
     <xsl:text>javascript:popWindow('defbyid','</xsl:text>
     <xsl:value-of              select = "@href"/>
     <xsl:text>&amp;version=Patient&amp;language=English'); </xsl:text>
     <xsl:text>return(false);</xsl:text>
    </xsl:attribute>
    <xsl:apply-templates/>
   </xsl:element>
  </xsl:template>

  <!--
  ================================================================ -->
  <xsl:template                  match = "SummaryRef">
   <xsl:element                   name = "a">
    <xsl:attribute                name = "href">
     <!--
     Next Line For Testing on DEV only !!! 
     ===================================== -->
     <xsl:text>http://www.cancer.gov</xsl:text>
     <xsl:value-of              select = "@url"/>
    </xsl:attribute>
    <xsl:apply-templates/>
   </xsl:element>
  </xsl:template>

  <!--
  ================================================================ -->
  <xsl:template                  match = "ExternalRef">
   <xsl:element                   name = "a">
    <xsl:attribute                name = "href">
     <!--
     Next Line For Testing on DEV only !!! 
     ===================================== -->
     <xsl:value-of              select = "@xref"/>
    </xsl:attribute>
    <xsl:apply-templates/>
   </xsl:element>
  </xsl:template>

  <!--
  ================================================================ -->
  <xsl:template                  match = "MediaLink">
   <xsl:element                   name = "figure">
    <xsl:attribute                name = "class">
     <xsl:text>image-center</xsl:text>
    </xsl:attribute>

   <!--
   Display the 'Enlarge' button
   ============================= -->
   <xsl:element                   name = "a">
    <xsl:attribute                name = "class">
     <xsl:text>article-image-enlarge</xsl:text>
    </xsl:attribute>
     <xsl:attribute                name = "target">
      <xsl:text>_blank</xsl:text>
     </xsl:attribute>
    <xsl:attribute                name = "href">
     <!--
     Next Line For Testing on DEV only !!! 
     ===================================== -->
     <xsl:text>http://www.cancer.gov</xsl:text>
     <xsl:text>/images/cdr/live/CDR</xsl:text>
     <xsl:value-of              select = "number(
                                           substring-after(@ref, 'CDR'))"/>
     <xsl:text>.jpg</xsl:text>
    </xsl:attribute>
    <xsl:text>Enlarge</xsl:text>
   </xsl:element>

    <!--
    Display the Image
    ============================= -->
    <xsl:element                  name = "img">
     <xsl:attribute               name = "__id">
      <xsl:value-of             select = "@id"/>
     </xsl:attribute>
     <xsl:attribute               name = "alt">
      <xsl:value-of             select = "@alt"/>
     </xsl:attribute>
     <xsl:attribute               name = "border">
      <xsl:value-of             select = "'1'"/>
     </xsl:attribute>
     <xsl:attribute               name = "src">
     <!--
     Next Line For Testing on DEV only !!! 
     ===================================== -->
     <xsl:text>http://www.cancer.gov</xsl:text>
     <xsl:text>/images/cdr/live/CDR</xsl:text>
     <xsl:value-of              select = "number(
                                           substring-after(@ref, 'CDR'))"/>
     <xsl:text>.jpg</xsl:text>
     </xsl:attribute>
    </xsl:element>


    <xsl:element                 name = "figcaption">
     <xsl:element                 name = "div">
      <xsl:attribute              name = "class">
       <xsl:text>caption-container</xsl:text>
      </xsl:attribute>
 
      <xsl:apply-templates/>
     </xsl:element>
    </xsl:element>
   </xsl:element>
  </xsl:template>


<!-- *************** TABLES ********************************** -->
  <!--
  Template for Tables
  ================================================================ -->
  <xsl:template                  match = "Table">
   <!-- Provide the 'Enlarge' link -->
   <xsl:element                   name = "p">
    <xsl:attribute                name = "class">
     <xsl:text>table-enlarge</xsl:text>
    </xsl:attribute>

    <xsl:element                  name = "a">
     <xsl:attribute               name = "href">
      <xsl:text>/DADA</xsl:text>
     </xsl:attribute>
     <xsl:text>Enlarge</xsl:text>
    </xsl:element>
   </xsl:element>

   <!-- Display the Table -->
   <xsl:apply-templates        select = "TGroup"/>
  </xsl:template>

  <!--
  Template for Table Caption/Title
  ================================================================ -->
  <xsl:template                  match = "Title"
                                  mode = "table">
   <xsl:element                   name = "caption">
    <xsl:apply-templates/>
   </xsl:element>
  </xsl:template>

  <!--
  Template for colgroup
  ================================================================ -->
  <xsl:template                  match = "TGroup">
   <xsl:param  name = "numCols" select="count(child::ColSpec)"/>
   <xsl:element                   name = "colgroup">
    <xsl:for-each               select = "ColSpec">
     <xsl:element                 name = "col">
      <xsl:attribute              name = "width">
       <xsl:value-of            select = "@ColWidth"/>
      </xsl:attribute>
     </xsl:element>
    </xsl:for-each>
   </xsl:element>

   <xsl:apply-templates         select = "THead"/>
   <xsl:apply-templates         select = "TFoot"/>
   <xsl:apply-templates         select = "TBody"/>
  </xsl:template>

  <!--
  Template for Table Caption/Title
  ================================================================ -->
  <xsl:template                  match = "THead">

   <xsl:element                   name = "thead">
    <xsl:apply-templates        select = "Row">
     <xsl:with-param              name = "type"
                                select = "'header'"/>
    </xsl:apply-templates>
   </xsl:element>
  </xsl:template>

  <!--
  Template for Table Caption/Title
  ================================================================ -->
  <xsl:template                  match = "TFoot">

   <xsl:element                   name = "tfoot">
    <xsl:apply-templates        select = "Row">
     <xsl:with-param              name = "type"
                                select = "'footer'"/>
    </xsl:apply-templates>
   </xsl:element>
  </xsl:template>

  <!--
  Template for Table Caption/Title
  ================================================================ -->
  <xsl:template                  match = "TBody">
   <xsl:element                   name = "tbody">
    <xsl:apply-templates        select = "Row"/>
   </xsl:element>
  </xsl:template>

  <!--
  Template for Table Caption/Title
  ================================================================ -->
  <xsl:template                  match = "Row">
   <xsl:param                     name = "type"/>
   <xsl:param                     name = "numCols" 
                                select = "count(child::entry)"/>
                                
   <xsl:element                   name = "tr">
    <xsl:apply-templates        select = "entry">
     <xsl:with-param              name = "type"
                                select = "$type"/>
     <xsl:with-param              name = "numCols"
                                select = "$numCols"/>
    </xsl:apply-templates>
   </xsl:element>
  </xsl:template>

  <!--
  Template for Table Caption/Title
  ================================================================ -->
  <xsl:template                  match = "entry">
   <xsl:param                     name = "type"
                                select = "''"/>
   <xsl:param                     name = "numCols"/>

   <xsl:choose>
    <xsl:when                     test = "$type = 'header'">
     <xsl:element                 name = "th">
      <xsl:attribute              name = "scope">
       <xsl:text>col</xsl:text>
      </xsl:attribute>
      <xsl:apply-templates/>
     </xsl:element>
    </xsl:when>
    <xsl:when                     test = "$type = 'footer'">
     <xsl:element                 name = "td">
      <xsl:if                     test = "$numCols = '1'
                                          and
                                          ../../../@Cols > $numCols">
       <xsl:attribute             name = "colspan">
        <xsl:value-of           select = "../../../@Cols"/>
       </xsl:attribute>
      </xsl:if>
      <xsl:apply-templates/>
     </xsl:element>
    </xsl:when>
    <xsl:otherwise>
     <xsl:element                 name = "td">
      <xsl:apply-templates/>
     </xsl:element>
    </xsl:otherwise>
   </xsl:choose>
  </xsl:template>


  <!--
  ================================================================ -->
  <xsl:template                  match = "Superscript">
   <xsl:element                   name = "sup">
    <xsl:apply-templates/>
   </xsl:element>
  </xsl:template>

  <!--
  ================================================================ -->
  <xsl:template                  match = "Subscript">
   <xsl:element                   name = "sub">
    <xsl:apply-templates/>
   </xsl:element>
  </xsl:template>



<!--
========================================================================
    NAMED TEMPLATES
======================================================================== -->
  <!--
  Template to create the Keypoint box
  ================================================================ -->
  <xsl:template                   name = "keypointsbox">
   <xsl:if test = "descendant::KeyPoint">
   <div class="keyPoints">
    <h4>Key Points for This Section</h4>
    <ul>
    <xsl:for-each               select = "descendant::KeyPoint">
     <li>
      <xsl:element                name = "a">
       <xsl:attribute             name = "href">
        <xsl:text>#</xsl:text>
        <xsl:value-of           select = "@id"/>
       </xsl:attribute>
       <xsl:value-of            select = "."/>
      </xsl:element>
     </li>
    </xsl:for-each>
    </ul>
   </div>
   </xsl:if>
  </xsl:template>

  <!--
  Template to create the TOC for a page
  ================================================================ -->
  <!-- *** to do:  need to have secondary UL inside LI *** -->
  <!-- <SummarySection> with <Title> children becomes <ul> -->
  <xsl:template match="SummarySection[Title]" mode="toc">
       <li>
   <!-- A1  --><xsl:apply-templates select="Title"  mode="toc"/>

   <xsl:if test = "SummarySection[Title]">
    <ul>
     <!-- A2 --> <xsl:apply-templates select="SummarySection[Title]" mode = "toc2"/>
    </ul>
   </xsl:if>
       </li>
  </xsl:template>

  <!--
  Template to create the TOC for a page
  ================================================================ -->
  <xsl:template match="SummarySection[Title]" mode="toc2">
    <!-- B2 --> <xsl:apply-templates select="Title"  mode="toc"/>
  </xsl:template>

  <!-- <SummarySection> without <Title> children is not handled -->
  <!--
  Template to create the TOC for a page
  ================================================================ -->
  <xsl:template match="SummarySection[not(Title)]" mode = "toc"/>

  <!--
  ================================================================ -->
  <xsl:template                  match = "Title" mode = "toc">
      <!-- B1 --> <xsl:element                name = "a">
        <xsl:attribute             name = "href">
         <xsl:text>#</xsl:text>
         <xsl:value-of          select = "../@id"/>
        </xsl:attribute>
        <xsl:value-of            select = "."/>
       </xsl:element>
  </xsl:template>


  <!-- <Title> with @name becomes <li> -->
  <!--
  Template to create the TOC for a page
  ================================================================ -->
  <xsl:template match="TitleXXX" mode="toc">
    <li>
      <xsl:value-of select ="." />
    </li>
  </xsl:template>

  <!--
  Template to create the TOC for a page
  ================================================================ -->
  <xsl:template                  match = "SummarySectionXXX"
                                  mode = "toc">
   <ul>
    <xsl:apply-templates        select = "Title"
                                  mode = "toc"/>
   </ul>
  </xsl:template>


<!--
========================================================================
    Template for CALS to HTML Conversion 
    (code adapted from 
    http://www.biglist.com/lists/xsl-list/archives/200202/msg00666.html)
======================================================================== -->
<!--
Template for Creating a table (from CALS)
============================================================== -->
<xsl:template match="TGroup">
   <xsl:element                   name = "table">
    <xsl:attribute                name = "class">
     <xsl:text>table-default</xsl:text>
    </xsl:attribute>

    <xsl:if test="@PgWide=1">
      <xsl:attribute name="width">100%</xsl:attribute>
    </xsl:if>
    <xsl:if test="TGroup/@Align">
      <xsl:attribute name="align">
        <xsl:value-of select="TGroup/@Align"/>
      </xsl:attribute>
    </xsl:if>
    <xsl:choose>
      <xsl:when test="@Frame='TOPBOT'">
        <xsl:attribute name="style">border-top:thin solid black; border-bottom:thin solid black;</xsl:attribute>
      </xsl:when>
      <!-- Should be coming from CSS 
      <xsl:when test="@Frame='None'">
        <xsl:attribute name="border">0</xsl:attribute>
      </xsl:when>
      -->
      <!-- Should be coming from CSS 
      -->
      <xsl:otherwise>
        <xsl:attribute name="border">1</xsl:attribute>
      </xsl:otherwise>
    </xsl:choose>

    <xsl:variable name="colgroup">
      <colgroup>
        <xsl:call-template name="generate.colgroup">
          <xsl:with-param name="cols" select="@Cols"/>
        </xsl:call-template>
      </colgroup>
    </xsl:variable>

    <xsl:variable name="table.width">
      <xsl:choose>
        <xsl:when test="$default.table.width = ''">
          <xsl:text>100%</xsl:text>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$default.table.width"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <!-- Remove attribute for Table width; coming from class
    <xsl:attribute name="width">
       <xsl:value-of select="$table.width"/>
    </xsl:attribute>
    -->


    <xsl:apply-templates        select = "../Title"
                                  mode = "table"/>


    <xsl:copy-of select="$colgroup"/>

    <xsl:apply-templates/>

   </xsl:element>
</xsl:template>

<xsl:template match="ColSpec"></xsl:template>

<xsl:template match="SpanSpec"></xsl:template>

<!-- 
=====================================================================
===================================================================== -->
<xsl:template                    match = "THead | 
                                          TFoot">
  <xsl:element                    name = "{name(.)}">
    <!-- Only display Align attribute if it's not center. Coming from CSS -->
    <xsl:if                       test = "@Align">
      <xsl:choose>
       <xsl:when                  test = "not(@Align = 'Center')">
        <xsl:attribute              name = "align">
          <xsl:value-of           select = "@Align"/>
        </xsl:attribute>
       </xsl:when>
      </xsl:choose>
    </xsl:if>
    <xsl:if                       test = "@Char">
      <xsl:attribute              name = "char">
        <xsl:value-of           select = "@Char"/>
      </xsl:attribute>
    </xsl:if>
    <xsl:if                       test = "@Charoff">
      <xsl:attribute              name = "charoff">
        <xsl:value-of           select = "@Charoff"/>
      </xsl:attribute>
    </xsl:if>
    <xsl:if                       test = "@Valign">
      <xsl:attribute              name = "valign">
        <xsl:value-of           select = "@Valign"/>
      </xsl:attribute>
    </xsl:if>

    <xsl:apply-templates/>
  </xsl:element>
</xsl:template>


<!-- 
=====================================================================
===================================================================== -->
<xsl:template match="TBody">
  <tbody>
    <xsl:if test="@Align">
      <xsl:attribute name="align">
        <xsl:value-of select="@Align"/>
      </xsl:attribute>
    </xsl:if>
    <xsl:if test="@Char">
      <xsl:attribute name="char">
        <xsl:value-of select="@Char"/>
      </xsl:attribute>
    </xsl:if>
    <xsl:if test="@Charoff">
      <xsl:attribute name="charoff">
        <xsl:value-of select="@Charoff"/>
      </xsl:attribute>
    </xsl:if>
    <xsl:if test="@Valign">
      <xsl:attribute name="valign">
        <xsl:value-of select="@Valign"/>
      </xsl:attribute>
    </xsl:if>

    <xsl:apply-templates/>
  </tbody>
</xsl:template>

<xsl:template match="Row">
  <tr>
    <xsl:if test="@Align">
      <xsl:attribute name="align">
        <xsl:value-of select="@Align"/>
      </xsl:attribute>
    </xsl:if>
    <xsl:if test="@Char">
      <xsl:attribute name="char">
        <xsl:value-of select="@Char"/>
      </xsl:attribute>
    </xsl:if>
    <xsl:if test="@Charoff">
      <xsl:attribute name="charoff">
        <xsl:value-of select="@Charoff"/>
      </xsl:attribute>
    </xsl:if>
    <xsl:if test="@Valign">
      <xsl:attribute name="valign">
        <xsl:value-of select="@Valign"/>
      </xsl:attribute>
    </xsl:if>
    <xsl:apply-templates/>
  </tr>
</xsl:template>

<xsl:template match="THead/Row/entry">
  <xsl:call-template name="process.cell">
    <xsl:with-param name="cellgi">th</xsl:with-param>
  </xsl:call-template>
</xsl:template>

<xsl:template match="TBody/Row/entry">
  <xsl:call-template name="process.cell">
    <xsl:with-param name="cellgi">td</xsl:with-param>
  </xsl:call-template>
</xsl:template>

<xsl:template match="TFoot/Row/entry">
  <xsl:call-template name="process.cell">
    <xsl:with-param name="cellgi">td</xsl:with-param>
  </xsl:call-template>
</xsl:template>

<xsl:template name="process.cell">
  <xsl:param name="cellgi">td</xsl:param>


  <xsl:variable name="empty.cell" select="count(node()) = 0"/>

  <xsl:variable name="entry.colnum">
    <xsl:call-template name="entry.colnum"/>
  </xsl:variable>

  <xsl:if test="$entry.colnum != ''">
    <xsl:variable name="prev.entry" select="preceding-sibling::*[1]"/>
    <xsl:variable name="prev.ending.colnum">
      <xsl:choose>
        <xsl:when test="$prev.entry">
          <xsl:call-template name="entry.ending.colnum">
            <xsl:with-param name="entry" select="$prev.entry"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:otherwise>0</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:call-template name="add-empty-entries">
      <xsl:with-param name="number">
        <xsl:choose>
          <xsl:when test="$prev.ending.colnum = ''">0</xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$entry.colnum - $prev.ending.colnum - 1"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:with-param>
    </xsl:call-template>
  </xsl:if>

  <xsl:element name="{$cellgi}">

  <xsl:if test="@SpanName">
  	<xsl:variable name="namest"
select="ancestor::TGroup/SpanSpec[@SpanName=./@SpanName]/@NameSt"/>
	<xsl:variable name="nameend"
select="ancestor::TGroup/SpanSpec[@SpanName=./@SpanName]/@NameEnd"/>
	<xsl:variable name="colst"
select="ancestor::*[ColSpec/@ColName=$namest]/ColSpec[@ColName=$namest]/@ColNum"/>
	<xsl:variable name="colend"
select="ancestor::*[ColSpec/@ColName=$nameend]/ColSpec[@ColName=$nameend]/@ColNum"/>
	<xsl:attribute name="colspan"><xsl:value-of 
    select="number($colend) - number($colst) + 1"/></xsl:attribute>
  </xsl:if>

    <xsl:if test="@MoreRows">
      <xsl:attribute name="rowspan">
        <xsl:value-of select="@MoreRows+1"/>
      </xsl:attribute>
    </xsl:if>
    <xsl:if test="@NameSt">
      <xsl:attribute name="colspan">
        <xsl:call-template name="calculate.colspan"/>
      </xsl:attribute>
    </xsl:if>
    <xsl:if                       test = "@Align">
      <xsl:choose>
       <xsl:when                  test = "not(@Align = 'Center')">
        <xsl:attribute              name = "align">
          <xsl:value-of           select = "@Align"/>
        </xsl:attribute>
       </xsl:when>
      </xsl:choose>
      <xsl:attribute name="scope">
        <xsl:value-of select="'col'"/>
      </xsl:attribute>
    </xsl:if>
    <xsl:if test="@Char">
      <xsl:attribute name="char">
        <xsl:value-of select="@Char"/>
      </xsl:attribute>
    </xsl:if>
    <xsl:if test="@Charoff">
      <xsl:attribute name="charoff">
        <xsl:value-of select="@Charoff"/>
      </xsl:attribute>
    </xsl:if>
    <xsl:if test="@Valign">
      <xsl:attribute name="valign">
        <xsl:value-of select="@Valign"/>
      </xsl:attribute>
    </xsl:if>

	<xsl:if test="@RowSep='1'">
		<xsl:attribute name="style">border-bottom:thin solid black</xsl:attribute>
	</xsl:if>

    <xsl:if test="not(preceding-sibling::*)
                  and ancestor::Row/@id">
      <a name="{ancestor::Row/@id}"/>
    </xsl:if>

    <xsl:if test="@id">
      <a name="{@id}"/>
    </xsl:if>

    <!-- Process Cell Content (entry element) -->
    <xsl:choose>
      <xsl:when test="$empty.cell">
        <xsl:text>&#160;</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:apply-templates/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:element>
</xsl:template>

<xsl:template name="add-empty-entries">
  <xsl:param name="number" select="'0'"/>
  <xsl:choose>
    <xsl:when test="$number &lt;= 0"></xsl:when>
    <xsl:otherwise>
      <td>&#160;</td>
      <xsl:call-template name="add-empty-entries">
        <xsl:with-param name="number" select="$number - 1"/>
      </xsl:call-template>
    </xsl:otherwise>
  </xsl:choose>
</xsl:template>


<xsl:template name="entry.colnum">
  <xsl:param name="entry" select="."/>

  <xsl:choose>
    <xsl:when test="$entry/@ColName">
      <xsl:variable name="colname" select="$entry/@ColName"/>
      <xsl:variable name="colspec"

select="$entry/ancestor::TGroup/ColSpec[@ColName=$colname]"/>
      <xsl:call-template name="colspec.colnum">
        <xsl:with-param name="colspec" select="$colspec"/>
      </xsl:call-template>
    </xsl:when>
    <xsl:when test="$entry/@NameSt">
      <xsl:variable name="namest" select="$entry/@NameSt"/>
      <xsl:variable name="colspec"
select="$entry/ancestor::TGroup/ColSpec[@ColName=$namest]"/>
      <xsl:call-template name="colspec.colnum">
        <xsl:with-param name="colspec" select="$colspec"/>
      </xsl:call-template>
    </xsl:when>
    <xsl:when test="count($entry/preceding-sibling::*) = 0">1</xsl:when>
    <xsl:otherwise>
      <xsl:variable name="pcol">
        <xsl:call-template name="entry.ending.colnum">
          <xsl:with-param name="entry"
select="$entry/preceding-sibling::*[1]"/>
        </xsl:call-template>
      </xsl:variable>
      <xsl:value-of select="$pcol + 1"/>
    </xsl:otherwise>
  </xsl:choose>
</xsl:template>


<xsl:template name="entry.ending.colnum">
  <xsl:param name="entry" select="."/>

  <xsl:choose>
    <xsl:when test="$entry/@ColName">
      <xsl:variable name="colname" select="$entry/@ColName"/>
      <xsl:variable name="colspec"
select="$entry/ancestor::TGroup/ColSpec[@ColName=$colname]"/>
      <xsl:call-template name="colspec.colnum">
        <xsl:with-param name="colspec" select="$colspec"/>
      </xsl:call-template>
    </xsl:when>
    <xsl:when test="$entry/@NameEnd">
      <xsl:variable name="nameend" select="$entry/@NameEnd"/>
      <xsl:variable name="colspec"
select="$entry/ancestor::TGroup/ColSpec[@ColName=$nameend]"/>
      <xsl:call-template name="colspec.colnum">
        <xsl:with-param name="colspec" select="$colspec"/>
      </xsl:call-template>
    </xsl:when>
    <xsl:when test="count($entry/preceding-sibling::*) = 0">1</xsl:when>
    <xsl:otherwise>
      <xsl:variable name="pcol">
        <xsl:call-template name="entry.ending.colnum">
          <xsl:with-param name="entry"
select="$entry/preceding-sibling::*[1]"/>
        </xsl:call-template>
      </xsl:variable>
      <xsl:value-of select="$pcol + 1"/>
    </xsl:otherwise>
  </xsl:choose>
</xsl:template>


<xsl:template name="colspec.colnum">
  <xsl:param name="colspec" select="."/>
  <xsl:choose>
    <xsl:when test="$colspec/@ColNum">
      <xsl:value-of select="$colspec/@ColNum"/>
    </xsl:when>
    <xsl:when test="$colspec/preceding-sibling::ColSpec">
      <xsl:variable name="prec.colspec.colnum">
        <xsl:call-template name="colspec.colnum">
          <xsl:with-param name="colspec"
                          select="$colspec/preceding-sibling::ColSpec[1]"/>
        </xsl:call-template>
      </xsl:variable>
      <xsl:value-of select="$prec.colspec.colnum + 1"/>
    </xsl:when>
    <xsl:otherwise>1</xsl:otherwise>
  </xsl:choose>
</xsl:template>

<xsl:template name="generate.colgroup">
  <xsl:param name="cols" select="1"/>
  <xsl:param name="count" select="1"/>
  <xsl:choose>
    <xsl:when test="$count>$cols"></xsl:when>
    <xsl:otherwise>
      <xsl:call-template name="generate.col">
        <xsl:with-param name="countcol" select="$count"/>
      </xsl:call-template>
      <xsl:call-template name="generate.colgroup">
        <xsl:with-param name="cols" select="$cols"/>
        <xsl:with-param name="count" select="$count+1"/>
      </xsl:call-template>
    </xsl:otherwise>
  </xsl:choose>
</xsl:template>

<xsl:template name="generate.col">
  <xsl:param name="countcol">1</xsl:param>
  <xsl:param name="colspecs" select="./ColSpec"/>
  <xsl:param name="count">1</xsl:param>
  <xsl:param name="colnum">1</xsl:param>

  <xsl:choose>
    <xsl:when test="$count>count($colspecs)">
      <col/>
    </xsl:when>
    <xsl:otherwise>
      <xsl:variable name="colspec" select="$colspecs[$count=position()]"/>
      <xsl:variable name="colspec.colnum">
        <xsl:choose>
          <xsl:when test="$colspec/@ColNum">
            <xsl:value-of select="$colspec/@ColNum"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$colnum"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:choose>
        <xsl:when test="$colspec.colnum=$countcol">
          <col>
            <xsl:if test="$colspec/@Align">
              <xsl:attribute name="align">
                <xsl:value-of select="$colspec/@Align"/>
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="$colspec/@Char">
              <xsl:attribute name="char">
                <xsl:value-of select="$colspec/@Char"/>
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="$colspec/@Charoff">
              <xsl:attribute name="charoff">
                <xsl:value-of select="$colspec/@Charoff"/>
              </xsl:attribute>
            </xsl:if>
            <!-- VE start -->
            <xsl:if test="$colspec/@ColWidth">
              <xsl:attribute name="width">
                <xsl:value-of select="$colspec/@ColWidth"/>
              </xsl:attribute>
            </xsl:if>
            <!-- VE end  -->
          </col>
        </xsl:when>
        <xsl:otherwise>
          <xsl:call-template name="generate.col">
            <xsl:with-param name="countcol" select="$countcol"/>
            <xsl:with-param name="colspecs" select="$colspecs"/>
            <xsl:with-param name="count" select="$count+1"/>
            <xsl:with-param name="colnum">
              <xsl:choose>
                <xsl:when test="$colspec/@ColNum">
                  <xsl:value-of select="$colspec/@ColNum + 1"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="$colnum + 1"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:with-param>
           </xsl:call-template>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:otherwise>
  </xsl:choose>

</xsl:template>

<xsl:template name="colspec.colwidth">
  <!-- when this macro is called, the current context must be an entry -->
  <xsl:param name="colname"></xsl:param>
  <!-- .. = Row, ../.. = THead|TBody, ../../.. = TGroup -->
  <xsl:param name="colspecs" select="../../../../TGroup/ColSpec"/>
  <xsl:param name="count">1</xsl:param>
  <xsl:choose>
    <xsl:when test="$count>count($colspecs)"></xsl:when>
    <xsl:otherwise>
      <xsl:variable name="colspec" select="$colspecs[$count=position()]"/>
      <xsl:choose>
        <xsl:when test="$colspec/@ColName=$colname">
          <xsl:value-of select="$colspec/@ColWidth"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:call-template name="colspec.colwidth">
            <xsl:with-param name="colname" select="$colname"/>
            <xsl:with-param name="colspecs" select="$colspecs"/>
            <xsl:with-param name="count" select="$count+1"/>
          </xsl:call-template>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:otherwise>
  </xsl:choose>
</xsl:template>

<xsl:template name="calculate.colspan">
  <xsl:param name="entry" select="."/>
  <xsl:variable name="namest" select="$entry/@NameSt"/>
  <xsl:variable name="nameend" select="$entry/@NameEnd"/>

  <xsl:variable name="scol">
    <xsl:call-template name="colspec.colnum">
      <xsl:with-param name="colspec"
select="$entry/ancestor::TGroup/ColSpec[@ColName=$namest]"/>
    </xsl:call-template>
  </xsl:variable>
  <xsl:variable name="ecol">
    <xsl:call-template name="colspec.colnum">
      <xsl:with-param name="colspec"

select="$entry/ancestor::TGroup/ColSpec[@ColName=$nameend]"/>
    </xsl:call-template>
  </xsl:variable>
  <xsl:value-of select="$ecol - $scol + 1"/>
</xsl:template>


</xsl:stylesheet>
