<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method = "html"/>
  <xsl:param    name = "section"/>

  <xsl:template match="/GlossaryTerm">
<html>
 <head>
  <title>
   <xsl:text>WCMS Glossary: </xsl:text>
   <xsl:value-of             select = "TermName"/>
  </title>

  <link href="http://www.cancer.gov/PublishedContent/Styles/nvcg.css" 
        type="text/css" rel="StyleSheet" />
 </head>
 <body>
  <div class="contentzone">
    <div style="background-color: yellow;">
    <p>
     <xsl:apply-templates     select="@id"/>
     <xsl:text>;</xsl:text>
     <xsl:apply-templates     select="TermName"/>
    </p>
    </div>

    <!--
    <xsl:for-each            select = "TermDefinition">
    -->
    <xsl:for-each               select = "TermDefinition">
    <div class="definition-term">
     <xsl:apply-templates       select = "../TermName"/>
     <xsl:apply-templates       select = "../MediaLink[@language = 'en' 
                                                 and 
                                                 @type = 'audio/mpeg']"
                                  mode = "audio"/>
    </div>

    <xsl:apply-templates       select = "DefinitionText"/>
    <xsl:apply-templates       select = "../RelatedInformation/
                                          RelatedExternalRef[@UseWith = 'en']"/>
    <xsl:choose>
     <xsl:when                   test = "Audience = 'Patient'">
      <xsl:apply-templates     select = "../MediaLink[@audience = 'Patients']
                                                  [@language = 'en']"/>
     </xsl:when>
     <xsl:otherwise>
      <xsl:apply-templates     select = "../MediaLink[@audience = 'Health_professionals']
                                                  [@language = 'en']"/>
     </xsl:otherwise>
    </xsl:choose>
    </xsl:for-each>

    <!-- Spanish Section -->
    <hr />
    <div class="definition-term">
     <xsl:apply-templates    select = "SpanishTermName"/>
     <xsl:apply-templates    select = "MediaLink[@language = 'es'
                                                 and
                                                 @type = 'audio/mpeg']"
                               mode = "audio"/>
    </div>
    <xsl:apply-templates    select = "SpanishTermDefinition/DefinitionText"/>
    <xsl:apply-templates    select = "RelatedInformation/
                                       RelatedExternalRef[@UseWith = 'es']"/>
    <xsl:apply-templates    select = "MediaLink[@audience = 'Patients'][@language = 'es']"/>

    <!--
    <xsl:apply-templates    select = "*[not(self::TermPronunciation)]
                                       [not(self::TermName)]
                                       [not(self::TermDefinition)]
                                       [not(self::DateFirstPublished)]
                                       [not(self::DateLastModified)]"/>
     -->
  </div>
 </body>
</html>
  </xsl:template>

  <!--
  Template to remove display of SectionMetaData
  ================================================================ -->
  <xsl:template                  match = "DefinitionText">
   <p>
    <xsl:apply-templates/>
   </p>
  </xsl:template>

  <!--
  ================================================================ -->
  <xsl:template                  match = "TermName |
                                          SpanishTermName">
    <strong><xsl:apply-templates/></strong>&#160;&#160;
  </xsl:template>

  <!--
  ================================================================ -->
  <xsl:template                  match = "MediaLink"
                                  mode = "audio">
   <xsl:element                 name = "a">
    <xsl:attribute              name = "href">
     <!--
     Next Line For Testing on DEV only !!! 
     ===================================== -->
     <xsl:text>http://www.cancer.gov</xsl:text>
     <xsl:text>/PublishedContent/Media/CDR/Media/</xsl:text>
     <xsl:choose>
      <xsl:when                 test = "@language = 'en'">
       <xsl:value-of            select = "number(
                                           substring-after(@ref, 'CDR'))"/>
      </xsl:when>
      <xsl:otherwise>
       <xsl:value-of            select = "number(
                                           substring-after(@ref, 'CDR'))"/>
      </xsl:otherwise>
     </xsl:choose>
     <xsl:choose>
      <xsl:when                test = "@type = 'audio/mpeg'">
       <xsl:text>.mp3</xsl:text>
      </xsl:when>
     </xsl:choose>
    </xsl:attribute>
    <!--
    Next Line For Testing on DEV only - hard coded Server name !!! 
    ===================================== -->
    <img width="16" height="16" class="definition-term-image"
         alt="listen" 
         src="http://www.cancer.gov/images/audio-icon.gif"></img>
   </xsl:element>

   <xsl:text>&#160;&#160;</xsl:text>
   <xsl:if                        test = "@language = 'en'">
    <xsl:value-of               select = "../TermPronunciation"/>
   </xsl:if>
   <!-- span style="background: yellow;">
    <xsl:text> XXX mpeg vs mp3 XXX</xsl:text>
   </span -->
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
     <xsl:text>definition-image</xsl:text>
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
    <xsl:choose>
     <xsl:when                    test = "@language = 'en'">
      <xsl:text>Enlarge</xsl:text>
     </xsl:when>
     <xsl:otherwise>
      <xsl:text>Ampliar</xsl:text>
     </xsl:otherwise>
    </xsl:choose>
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


    <!-- We're not displaying Captions on Glossary Term Thumbs -->
    <!-- 
    <xsl:element                 name = "figcaption">
     <xsl:element                 name = "div">
      <xsl:attribute              name = "class">
       <xsl:text>caption-container</xsl:text>
      </xsl:attribute>
 
      <xsl:apply-templates/>
     </xsl:element>
    </xsl:element>
    -->
   </xsl:element>
  </xsl:template>


  <!--
  ================================================================ -->
  <xsl:template                  match = "RelatedExternalRef[@UseWith = 'en']">
   <xsl:element                   name = "h4">
    <xsl:text>More Information</xsl:text>
   </xsl:element>

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
  <xsl:template                  match = "RelatedExternalRef[@UseWith = 'es']">
   <xsl:element                   name = "h4">
    <xsl:text>M&#xE1;s informaci&#xF3;n</xsl:text>
   </xsl:element>

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
========================================================================
    NAMED TEMPLATES
======================================================================== -->
  <!--
  Template to remove display of SectionMetaData
  ================================================================ -->
  <xsl:template                   name = "toc">
     <!-- xsl:for-each              select = "descendant::Title" -->
      <xsl:apply-templates select = "SummarySection" mode = "toc"/>
  </xsl:template>

</xsl:stylesheet>
