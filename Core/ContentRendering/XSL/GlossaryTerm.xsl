<?xml version = "1.0" encoding = "utf-8"?>
<!-- File name: GlossaryTerm.xsl    -->
<!--
==========================================================================
This stylesheet creates the Dictionary JSON structure containing GlossaryTerm
details for a single targeted Language, Audience and Dictionary.  Separate
transformations are required to generate the JSON structure for each
permutation of the three parameters.

Correct function of these templates is VERY SENSITIVE TO EXTRA CARRIAGE
RETURNS.  Be very careful when using an editor (e.g. Visual Studio) which
automatically reformats text to a "suggested" format.  Consider using a
diff tool to check for unexpected formatting changes before committing to
source control.
========================================================================== -->
<xsl:stylesheet              xmlns:xsl = "http://www.w3.org/1999/XSL/Transform"
                               version = "1.0">

 <xsl:import                      href = "Common/JSON-common-templates.xsl"/>

 <xsl:output                    method = "text"
                                indent = "yes"/>

 <!-- Default targets are English, Patient, Cancer.gov dictionary -->
 <xsl:param                       name = "targetLanguage"
                                select = "'English'"/>

 <xsl:param                       name = "targetAudience"
                                select = "'Patient'" />

 <xsl:param                       name = "targetDictionary"
                                select = "'Term'"/>

 <!--
 Set the audienceCode, languageCode, and dictionaryCode variables to use
 the same values as the CDR. These variables provide a translation from
 the constants used by GateKeeper.
 -->
 <!-- General Audience code. MediaLink images and related glossary terms uses different values. -->
 <xsl:variable                    name = "audienceCode">
  <xsl:choose>
   <xsl:when                      test = "$targetAudience = 'Patient'">
    <xsl:text>Patient</xsl:text>
   </xsl:when>
   <xsl:when                      test = "$targetAudience =
                                                 'HealthProfessional'">
    <xsl:text>Health professional</xsl:text>
   </xsl:when>
  </xsl:choose>
 </xsl:variable>

 <!--
 MediaLink elements of type image/jpeg use a different string literal
 for identifying the audience. -->
 <xsl:variable                    name = "imageLinkAudienceCode">
  <xsl:choose>
   <xsl:when                      test = "$targetAudience = 'Patient'">
    <xsl:text>Patients</xsl:text>
   </xsl:when>
   <xsl:when                      test = "$targetAudience =
                                                 'HealthProfessional'">
    <xsl:text>Health_professionals</xsl:text>
   </xsl:when>
  </xsl:choose>
 </xsl:variable>

  <!--
 RelatedGlossaryTermRef elements use a different string literal
 for identifying the audience. -->
  <xsl:variable                   name = "relatedGlossaryTermRefLinkAudienceCode">
    <xsl:choose>
      <xsl:when                   test = "$targetAudience = 'Patient'">
        <xsl:text>Patients</xsl:text>
      </xsl:when>
      <xsl:when                   test = "$targetAudience =
                                                 'HealthProfessional'">
        <xsl:text>Health_professionals</xsl:text>
      </xsl:when>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable                   name = "languageCode">
  <xsl:choose>
   <xsl:when                      test = "$targetLanguage = 'English'">
    <xsl:text>en</xsl:text>
   </xsl:when>
   <xsl:when                      test = "$targetLanguage = 'Spanish'">
    <xsl:text>es</xsl:text>
   </xsl:when>
  </xsl:choose>
 </xsl:variable>

 <xsl:variable                    name = "dictionaryCode">
  <xsl:choose>
   <xsl:when                      test = "$targetDictionary = 'Term'">
    <xsl:text>Cancer.gov</xsl:text>
   </xsl:when>
   <xsl:when                      test = "$targetDictionary = 'Genetic'">
    <xsl:text>Genetics</xsl:text>
   </xsl:when>
   <xsl:otherwise>
    <xsl:value-of               select = "$targetDictionary"/>
   </xsl:otherwise>
  </xsl:choose>
 </xsl:variable>


 <!--
 =======================================================================
  Match for the root (GlossaryTerm) element.
  The entire JSON structure is created here.
 ======================================================================= -->
 <xsl:template                   match = "/GlossaryTerm">
  <xsl:text>{</xsl:text>
  <xsl:text>&#xa;</xsl:text>
  <xsl:call-template              name = "RenderDocumentID"/>
  <xsl:call-template              name = "RenderTermName"/>
  <xsl:call-template              name = "RenderAliasList"/>
  <xsl:call-template              name = "RenderDateFirstPublished"/>
  <xsl:call-template              name = "RenderDateLastModified"/>
  <xsl:call-template              name = "RenderDefinition"/>
  <xsl:call-template              name = "RenderImageMediaLinks"/>
  <xsl:call-template              name = "RenderVideoLinks"/>
  <xsl:call-template              name = "RenderPronunciation"/>
  <xsl:call-template              name = "RenderRelatedInformation"/>
  <xsl:text>&#xa;</xsl:text>
  <xsl:text>}</xsl:text>
 </xsl:template>


 <!--
 =======================================================================
 Template to create the id JSON value
 ======================================================================= -->
 <xsl:template                    name = "RenderDocumentID">
  <xsl:text> "id": "</xsl:text>
  <xsl:call-template              name = "GetNumericID">
   <xsl:with-param                name = "cdrid"
                                select = "/GlossaryTerm/@id"/>
  </xsl:call-template>
  <xsl:text>",</xsl:text>
  <xsl:text>&#xa;</xsl:text>
 </xsl:template>


 <!--
 =======================================================================
 Template to create the term JSON value
 ======================================================================= -->
 <xsl:template                    name = "RenderTermName">
  <xsl:variable                   name = "termName">
   <xsl:choose>
    <xsl:when                     test = "$targetLanguage = 'English'">
     <xsl:value-of              select = "//TermName"/>
    </xsl:when>
    <xsl:when                     test = "$targetLanguage = 'Spanish'">
     <xsl:value-of              select = "//SpanishTermName"/>
    </xsl:when>
    <xsl:otherwise>
     <xsl:value-of              select = "//TermName"/>
    </xsl:otherwise>
   </xsl:choose>
  </xsl:variable>

  <xsl:text> "term": "</xsl:text>
  <xsl:value-of                 select = "$termName"/>
  <xsl:text>",</xsl:text>
  <xsl:text>&#xa;</xsl:text>
 </xsl:template>


 <!--
 =======================================================================
 Template to create the alias JSON value
 The alias array is always empty for Glossary Term docs.
 ======================================================================= -->
 <xsl:template                    name = "RenderAliasList">
  <xsl:text> "alias": [],</xsl:text>
  <xsl:text>&#xa;</xsl:text>
 </xsl:template>


 <!--
 =======================================================================
 Template to create the date_first_published JSON value
 ======================================================================= -->
 <xsl:template                    name = "RenderDateFirstPublished">
  <xsl:if                         test = "//DateFirstPublished">
   <xsl:text> "date_first_published": "</xsl:text>
   <xsl:value-of                select = "//DateFirstPublished"/>
   <xsl:text>", </xsl:text>
   <xsl:text>&#xa;</xsl:text>
  </xsl:if>
 </xsl:template>


 <!--
 =======================================================================
 Template to create the date_last_modified JSON value
 ======================================================================= -->
 <xsl:template                    name = "RenderDateLastModified">
  <xsl:if                         test = "//DateLastModified">
   <xsl:text> "date_last_modified": "</xsl:text>
   <xsl:value-of                select = "//DateLastModified"/>
   <xsl:text>", </xsl:text>
   <xsl:text>&#xa;</xsl:text>
  </xsl:if>
 </xsl:template>

 <!--
 =======================================================================
 Template to create the definition JSON value
 ======================================================================= -->
 <xsl:template                    name = "RenderDefinition">
  <xsl:variable                   name = "j-def-tag">
    <xsl:text> "definition": {</xsl:text>
    <xsl:text>&#xa;</xsl:text>
  </xsl:variable>

  <xsl:variable                   name = "j-html-tag">
    <xsl:text>    "html": "</xsl:text>
  </xsl:variable>

  <xsl:variable                   name = "j-text-tag">
    <xsl:text>    "text": "</xsl:text>
  </xsl:variable>

  <xsl:variable                   name = "j-end-nl">
    <xsl:text>",</xsl:text>
    <xsl:text>&#xa;</xsl:text>
  </xsl:variable>

  <xsl:choose>
   <xsl:when                      test = "$targetLanguage = 'English'">
    <xsl:value-of               select = "$j-def-tag"/>
    <xsl:value-of               select = "$j-html-tag"/>
    <xsl:apply-templates        select = "//TermDefinition[
                                              (Dictionary = $dictionaryCode
                                               or
                                               (not(Dictionary)
                                                and
                                                $targetDictionary = 'NotSet'
                                               )
                                              )
                                              and
                                              Audience = $audienceCode
                                                         ]/DefinitionText" />
    <xsl:value-of               select = "$j-end-nl"/>
    <xsl:value-of               select = "$j-text-tag"/>
    <xsl:apply-templates        select = "//TermDefinition[
                                              (Dictionary = $dictionaryCode
                                               or
                                               (not(Dictionary)
                                                and
                                                $targetDictionary = 'NotSet'
                                               )
                                              )
                                              and
                                              Audience = $audienceCode
                                                         ]/DefinitionText"
                                  mode = "JSON" />
    <xsl:text>"</xsl:text>
    <xsl:text>&#xa;</xsl:text>
    <xsl:text> },</xsl:text>
    <xsl:text>&#xa;</xsl:text>
   </xsl:when>
   <xsl:when                      test = "$targetLanguage = 'Spanish'">
    <xsl:value-of               select = "$j-def-tag"/>
    <xsl:value-of               select = "$j-html-tag"/>
    <xsl:apply-templates        select = "//SpanishTermDefinition[
                                               (Dictionary = $dictionaryCode
                                                or
                                                (not(Dictionary)
                                                 and
                                                 $targetDictionary = 'NotSet'
                                                )
                                               )
                                               and
                                               Audience = $audienceCode
                                                          ]/DefinitionText" />
    <xsl:value-of               select = "$j-end-nl"/>
    <xsl:value-of               select = "$j-text-tag"/>
    <xsl:apply-templates        select = "//SpanishTermDefinition[
                                               (Dictionary = $dictionaryCode
                                                or
                                                (not(Dictionary)
                                                 and
                                                 $targetDictionary = 'NotSet'
                                                )
                                               )
                                               and
                                               Audience = $audienceCode
                                                          ]/DefinitionText"
                                  mode = "JSON" />
    <xsl:text>"</xsl:text>
    <xsl:text>&#xa;</xsl:text>
    <xsl:text> },</xsl:text>
    <xsl:text>&#xa;</xsl:text>
   </xsl:when>
  </xsl:choose>
 </xsl:template>


 <!--
 =======================================================================
 Template to create the definition JSON value
 ======================================================================= -->
 <xsl:template                    name = "RenderImageMediaLinks">
  <xsl:if                         test = "//MediaLink[@type = 'image/jpeg'
                                                  and
                                                  @language = $languageCode
                                                  and
                                                  @audience =
                                                      $imageLinkAudienceCode]">
   <xsl:text> "images": [</xsl:text>

   <xsl:for-each                select = "//MediaLink[string-length(@ref) > 0
                                          and
                                          @type = 'image/jpeg'
                                          and
                                          @language = $languageCode
                                          and
                                          @audience = $imageLinkAudienceCode]">
    <xsl:apply-templates        select = "."
                                  mode = "images"/>
     <!--
     Output a comma unless this is the last element of the set. -->
    <xsl:if                       test = "position() != last()">
     <xsl:text>,</xsl:text>
    </xsl:if>
   </xsl:for-each>

   <xsl:text>],</xsl:text>
   <xsl:text>&#xa;</xsl:text>
  </xsl:if>
 </xsl:template>


 <!--
 =======================================================================
 Template to process MediaLink elements
 ======================================================================= -->
 <xsl:template                   match = "MediaLink"
                                  mode = "images">
  <xsl:text>{</xsl:text>
  <xsl:text>&#xa;</xsl:text>
  <xsl:text>    "ref": "[__imagelocation]CDR</xsl:text>

  <xsl:call-template              name = "GetNumericID">
   <xsl:with-param                name = "cdrid"
                                select = "@ref" />
  </xsl:call-template>

  <xsl:text>.jpg",</xsl:text>
  <xsl:text>&#xa;</xsl:text>
  <xsl:text>    "alt": "</xsl:text>
  <xsl:apply-templates          select = "@alt"
                                  mode = "JSON" />
  <xsl:text>"</xsl:text>
  <!--
  If caption is rendered, it includes a comma for alt.-->
  <xsl:if                         test = "Caption[@language = $languageCode]">
   <xsl:text>,</xsl:text>
   <xsl:text>&#xa;</xsl:text>
   <xsl:text>    "caption": "</xsl:text>
   <xsl:apply-templates         select = "Caption" />
   <xsl:text>"</xsl:text>
  </xsl:if>

  <xsl:text>&#xa;</xsl:text>
  <xsl:text> }</xsl:text>
 </xsl:template>


 <!--
 =======================================================================
 Template to create the videos JSON value
 ======================================================================= -->
 <xsl:template                    name = "RenderVideoLinks">
   <xsl:text> "videos": [</xsl:text>
   <xsl:for-each                select = "EmbeddedVideo[
                                              @language = $languageCode
                                              and
                                              @audience =
                                                      $imageLinkAudienceCode]">
    <xsl:apply-templates        select = "."/>
     <!--
     Output a comma unless this is the last element of the set. -->
    <xsl:if                       test = "position() != last()">
     <xsl:text>,</xsl:text>
    </xsl:if>
   </xsl:for-each>

   <xsl:text>],</xsl:text>
   <xsl:text>&#xa;</xsl:text>
  <!--
  </xsl:if>
  -->
 </xsl:template>


 <!--
 =======================================================================
 Template to process EmbeddedVideo elements
 ======================================================================= -->
 <xsl:template                   match = "EmbeddedVideo">
  <xsl:text>{</xsl:text>
  <xsl:text>&#xa;</xsl:text>
  <!--
  <xsl:text>    "ref": "[__videolocation]CDR</xsl:text>

  <xsl:call-template              name = "GetNumericID">
   <xsl:with-param                name = "cdrid"
                                select = "@ref" />
  </xsl:call-template>

  <xsl:text>.xml",</xsl:text>
  <xsl:text>&#xa;</xsl:text>
  -->

  <!--
  <xsl:text>    "id": "</xsl:text>
  <xsl:value-of                 select = "@id"/>
  <xsl:text>",</xsl:text>
  <xsl:text>&#xa;</xsl:text>
  -->

  <xsl:text>    "hosting": "</xsl:text>
  <xsl:value-of                 select = "@hosting"/>
  <xsl:text>",</xsl:text>
  <xsl:text>&#xa;</xsl:text>

  <xsl:text>    "unique_id": "</xsl:text>
  <xsl:value-of                 select = "@unique_id"/>
  <xsl:text>",</xsl:text>
  <xsl:text>&#xa;</xsl:text>

  <xsl:text>    "template": "</xsl:text>
  <xsl:value-of                 select = "@template"/>
  <xsl:text>",</xsl:text>
  <xsl:text>&#xa;</xsl:text>

  <xsl:text>    "language": "</xsl:text>
  <xsl:value-of                 select = "@language"/>
  <xsl:text>",</xsl:text>
  <xsl:text>&#xa;</xsl:text>

  <!--
  <xsl:text>    "audience": "</xsl:text>
  <xsl:value-of                 select = "@audience"/>
  <xsl:text>",</xsl:text>
  <xsl:text>&#xa;</xsl:text>
  -->

  <xsl:text>    "audience": "</xsl:text>
  <xsl:value-of                 select = "$imageLinkAudienceCode"/>
  <xsl:text>"</xsl:text>

  <xsl:apply-templates          select = "VideoTitle" />
  <xsl:apply-templates          select = "Caption"
                                  mode = "video"/>

  <!--
  If caption is rendered, it includes a comma for alt.-->
  <!--
  <xsl:if                         test = "Caption[@language = $languageCode]">
   <xsl:text>,</xsl:text>
   <xsl:text>&#xa;</xsl:text>
   <xsl:text>    "caption": "</xsl:text>
   <xsl:text>"</xsl:text>
  </xsl:if>
  -->

  <xsl:text>&#xa;</xsl:text>
  <xsl:text> }</xsl:text>
 </xsl:template>


 <!--
 =======================================================================
  Match for the VideoTitle
 ======================================================================= -->
 <xsl:template                   match = "VideoTitle">
  <xsl:text>,</xsl:text>
  <xsl:text>&#xa;</xsl:text>
  <xsl:text>    "title": "</xsl:text>
  <xsl:apply-templates/>
  <xsl:text>"</xsl:text>
 </xsl:template>


 <!--
 =======================================================================
  Match for the VideoTitle
 ======================================================================= -->
 <xsl:template                   match = "Caption"
                                  mode = "video">
  <xsl:text>,</xsl:text>
  <xsl:text>&#xa;</xsl:text>
  <xsl:text>    "caption": "</xsl:text>
  <xsl:apply-templates/>
  <xsl:text>"</xsl:text>
 </xsl:template>


 <!--
 =======================================================================
 Template to create the definition JSON value
 Render the pronunciation data structure with pronunciation key and
 audio file.
 ======================================================================= -->
 <xsl:template                    name = "RenderPronunciation">
  <!-- Calculate media file name. -->
  <xsl:variable                   name = "mediaFile">
   <xsl:if                        test = "//MediaLink[string-length(@ref) > 0
                                                      and
                                                      @type = 'audio/mpeg'
                                                      and
                                                      @language =
                                                              $languageCode]">
    <xsl:text>[__audiolocation]</xsl:text>
    <xsl:call-template            name = "GetNumericID">
     <xsl:with-param              name = "cdrid"
                                select = "//MediaLink[@type = 'audio/mpeg'
                                                      and
                                                      @language =
                                                         $languageCode]/@ref" />
    </xsl:call-template>

    <xsl:text>.mp3</xsl:text>
   </xsl:if>
  </xsl:variable>

  <xsl:variable                   name = "pronunciationKey">
   <xsl:choose>
    <xsl:when                     test = "$targetLanguage = 'English'">
     <xsl:value-of              select = "//TermPronunciation"/>
    </xsl:when>
    <xsl:when                     test = "$targetLanguage = 'Spanish'">
    <!-- Never present for Spanish -->
    </xsl:when>
   </xsl:choose>
  </xsl:variable>

  <xsl:if                         test = "string-length($mediaFile) > 0
                                          or
                                          string-length($pronunciationKey) > 0">
   <xsl:text> "pronunciation": {</xsl:text>
   <xsl:text>&#xa;</xsl:text>
   <xsl:text>    "audio": "</xsl:text>
   <xsl:value-of                select = "$mediaFile"/>
   <xsl:text>",</xsl:text>
   <xsl:text>&#xa;</xsl:text>
   <xsl:text>    "key": "</xsl:text>
   <xsl:value-of                select = "$pronunciationKey"/>
   <xsl:text>"</xsl:text>
   <xsl:text>&#xa;</xsl:text>
   <xsl:text> },</xsl:text>
   <xsl:text>&#xa;</xsl:text>
  </xsl:if>
 </xsl:template>


 <!--
 =======================================================================
 Template to create the definition JSON value
 Renders the Related Information structure.

 NOTE: This is the last item in the output data structure and therefore
       does not render a comma at the end.
 ======================================================================= -->
 <xsl:template                    name = "RenderRelatedInformation">
  <xsl:text> "related": {</xsl:text>
  <xsl:text>&#xa;</xsl:text>
  <xsl:call-template              name = "RenderRelatedDrugSummaries" />
  <xsl:call-template              name = "RenderRelatedExternalRefs" />
  <xsl:call-template              name = "RenderRelatedSummaryRefs" />
  <xsl:call-template              name = "RenderRelatedTermRefs" />
  <xsl:text>&#xa;</xsl:text>
  <xsl:text> }</xsl:text>
 </xsl:template>


 <!--
 =======================================================================
 Template to create the definition JSON value
 Helper template to render the Drug Info Summary portion of the related
 information section.
 ======================================================================= -->
 <xsl:template                    name = "RenderRelatedDrugSummaries">
  <xsl:text>    "drug_summary": [</xsl:text>
  <xsl:for-each                 select = "//RelatedInformation/
                                            RelatedDrugSummaryRef[
                                                     @UseWith = $languageCode]">
   <xsl:text>{</xsl:text>
   <xsl:text>&#xa;</xsl:text>
   <xsl:text>        "url": "</xsl:text>
   <xsl:value-of                select = "@url"/>
   <xsl:text>",</xsl:text>
   <xsl:text>&#xa;</xsl:text>
   <xsl:text>        "text" : "</xsl:text>
   <xsl:apply-templates         select = "node()"
                                  mode = "JSON" />
   <xsl:text>"</xsl:text>
   <xsl:text>}</xsl:text>
   <xsl:text>&#xa;</xsl:text>

   <xsl:if                        test = "position() != last()">
    <xsl:text>,</xsl:text>
   </xsl:if>
  </xsl:for-each>
  <xsl:text>],</xsl:text>
  <xsl:text>&#xa;</xsl:text>
 </xsl:template>


 <!--
 =======================================================================
 Template to create the definition JSON value
 Helper template to render the External reference portion of the related
 information section.
 ======================================================================= -->
 <xsl:template                    name = "RenderRelatedExternalRefs">
  <xsl:text>    "external": [</xsl:text>
  <xsl:for-each                 select = "//RelatedInformation/
                                            RelatedExternalRef[
                                                     @UseWith = $languageCode]">
   <xsl:text>{</xsl:text>
   <xsl:text>&#xa;</xsl:text>
   <xsl:text>        "url": "</xsl:text>
   <xsl:value-of                select = "@xref"/>
   <xsl:text>",</xsl:text>
   <xsl:text>&#xa;</xsl:text>
   <xsl:text>        "text": "</xsl:text>
   <xsl:apply-templates         select = "node()"
                                  mode = "JSON" />
   <xsl:text>"</xsl:text>
   <xsl:text>&#xa;</xsl:text>
   <xsl:text>    }</xsl:text>

   <xsl:if                        test = "position() != last()">
    <xsl:text>,</xsl:text>
   </xsl:if>
  </xsl:for-each>

  <xsl:text>],</xsl:text>
  <xsl:text>&#xa;</xsl:text>
 </xsl:template>


 <!--
 =======================================================================
 Template to create the definition JSON value
 Helper template to render the Summary reference portion of the related
 information section.
 ======================================================================= -->
 <xsl:template                    name = "RenderRelatedSummaryRefs">
  <xsl:text>    "summary": [</xsl:text>
  <xsl:for-each                 select = "//RelatedInformation/
                                            RelatedSummaryRef[
                                                     @UseWith = $languageCode]">
   <xsl:text>{</xsl:text>
   <xsl:text>"url": "</xsl:text>
   <xsl:value-of                select = "@url"/>
   <xsl:text>",</xsl:text>
   <xsl:text>"text": "</xsl:text>
   <xsl:apply-templates         select = "node()"
                                  mode = "JSON" />
   <xsl:text>"</xsl:text>
   <xsl:text>}</xsl:text>

   <xsl:if                        test = "position() != last()">
    <xsl:text>,</xsl:text>
   </xsl:if>
  </xsl:for-each>
  <xsl:text>],</xsl:text>
  <xsl:text>&#xa;</xsl:text>
 </xsl:template>


 <!--
 =======================================================================
 Template to create the definition JSON value
 Helper template to render the Glossary Term reference portion of the
 related information section.

 ASSUMPTION: Terms in one dictionary will only reference terms in the
             same dictionary.

 NOTE: RelatedGlossaryTermRef elements will *NOT* match the document which
       is sent from the CDR.  During Extract processing, the
       RelatedGlossaryTermRef elements are rewritten in
       GlossaryTermExtractor.RewriteRelatedGlossaryTerms() to replace
       any existing RelatedGlossaryTermRef elements with new elements
       containing UseWith= and audience= attributes reflecting the language
       and audiences for which the referenced GlossaryTerm is available.
       The GlossaryTerm XSL is then able to determine which JSON blocks
       should include references to the related term and those from which
       it should be omitted.
 ======================================================================= -->
 <xsl:template                    name = "RenderRelatedTermRefs">
  <xsl:text>    "term": [</xsl:text>

  <xsl:for-each                 select = "//RelatedInformation
                                           /RelatedGlossaryTermRef
                                                   [@UseWith = $languageCode
                                                    and
                                                    @audience = $relatedGlossaryTermRefLinkAudienceCode
                                                   ]">
   <xsl:text>{</xsl:text>
   <xsl:text>&#xa;</xsl:text>
   <xsl:text> "id": </xsl:text>

   <xsl:call-template             name = "GetNumericID">
    <xsl:with-param               name = "cdrid"
                                select = "@href"/>
   </xsl:call-template>

   <xsl:text>,</xsl:text>
   <xsl:text> "dictionary": "</xsl:text>
   <xsl:value-of                select = "$targetDictionary" />
   <xsl:text>",</xsl:text>
   <xsl:text> "text": "</xsl:text>
   <xsl:apply-templates         select = "node()"
                                  mode = "JSON"/>
   <xsl:text>"</xsl:text>
   <xsl:text>}</xsl:text>

   <!--
   No comma after the last sub-element in the releated items structure  -->
   <xsl:if                        test = "position() != last()">
    <xsl:text>,</xsl:text>
   </xsl:if>
  </xsl:for-each>

  <xsl:text>]</xsl:text>
 </xsl:template>

</xsl:stylesheet>
