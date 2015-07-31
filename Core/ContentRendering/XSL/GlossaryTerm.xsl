﻿<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:import href="Common/JSON-common-templates.xsl"/>
  <xsl:output method="text" indent="yes"/>

  <!-- Default targets are English, Patient, Cancer.gov dictionary -->
  <xsl:param        name = "targetLanguage"
                  select = "'English'"/>

  <xsl:param        name = "targetAudience"
                  select = "'Patient'" />

  <xsl:param        name = "targetDictionary"
                  select = "'Term'"/>

  <!--
    Set the audienceCode, languageCode, and dictionaryCode variables to use the same values
    as the CDR. These variables provide a translation from the constants used by GateKeeper.
  -->
  <!-- General Audience code. MediaLink image uses different values. -->
  <xsl:variable name="audienceCode">
    <xsl:choose>
      <xsl:when test="$targetAudience = 'Patient'">Patient</xsl:when>
      <xsl:when test="$targetAudience = 'HealthProfessional'">Health professional</xsl:when>
    </xsl:choose>
  </xsl:variable>

  <!-- MediaLink elements of type image/jpeg use a different string literal for identifying
      the audience.-->
  <xsl:variable name="imageLinkAudienceCode">
    <xsl:choose>
      <xsl:when test="$targetAudience = 'Patient'">Patients</xsl:when>
      <xsl:when test="$targetAudience = 'HealthProfessional'">Health_professionals</xsl:when>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name="languageCode">
    <xsl:choose>
      <xsl:when test="$targetLanguage = 'English'">en</xsl:when>
      <xsl:when test="$targetLanguage = 'Spanish'">es</xsl:when>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name="dictionaryCode">
    <xsl:choose>
      <xsl:when test="$targetDictionary = 'Term'">Cancer.gov</xsl:when>
      <xsl:when test="$targetDictionary = 'Genetic'">Genetics</xsl:when>
    </xsl:choose>
  </xsl:variable>

  
  <!--
    Match for the root (GlossaryTerm) element.  The entire JSON structure is created here.
  -->
  <xsl:template match="/GlossaryTerm">
{
"term": {
  <xsl:call-template name="RenderDocumentID" />
  <xsl:call-template name="RenderTermName" />
  <xsl:call-template name="RenderAliasList" />
  <xsl:call-template name="RenderDateFirstPublished" />
  <xsl:call-template name="RenderDateLastModified" />
  <xsl:call-template name="RenderDefinition" />
  <xsl:call-template name="RenderImageMediaLinks" />
  <xsl:call-template name="RenderPronunciation" />
  <xsl:call-template name="RenderRelatedInformation" />
}
}
  </xsl:template>

  <xsl:template name="RenderDocumentID">
  "id": "<xsl:call-template name="GetNumericID"><xsl:with-param name="cdrid" select="/GlossaryTerm/@id"/></xsl:call-template>",
  </xsl:template>
  
  <xsl:template name="RenderTermName">
    <xsl:variable name="termName">
      <xsl:choose>
        <xsl:when test="$targetLanguage = 'English'"><xsl:value-of select="//TermName"/></xsl:when>
        <xsl:when test="$targetLanguage = 'Spanish'"><xsl:value-of select="//SpanishTermName"/></xsl:when>
        <xsl:otherwise><xsl:value-of select="//TermName"/></xsl:otherwise>
      </xsl:choose>
    </xsl:variable><!-- Supress line break
  -->"term": "<xsl:value-of select="$termName"/>",
  </xsl:template>

  <xsl:template name="RenderAliasList">
  "alias": [ ], <!-- The alias array is always empty for Glossary Term docs. -->
  </xsl:template>
  
  <xsl:template name="RenderDateFirstPublished">
    <xsl:if test="//DateFirstPublished">"date_first_published": "<xsl:value-of select="//DateFirstPublished"/>", </xsl:if>
  </xsl:template>

  <xsl:template name="RenderDateLastModified">
    <xsl:if test="//DateLastModified">"date_last_modified": "<xsl:value-of select="//DateLastModified"/>", </xsl:if>
  </xsl:template>

  <xsl:template name="RenderDefinition">
    <xsl:choose>
      <xsl:when test="$targetLanguage = 'English'">
  "definition": {
    "html": "<xsl:apply-templates  select="//TermDefinition[Dictionary = $dictionaryCode and Audience = $audienceCode]/DefinitionText" />",
    "text": "<xsl:value-of select="//TermDefinition[Dictionary = $dictionaryCode and Audience = $audienceCode]/DefinitionText" />"
  },
      </xsl:when>
      <xsl:when test="$targetLanguage = 'Spanish'">
  "definition": {
    "html": "<xsl:apply-templates select="//SpanishTermDefinition[Dictionary = $dictionaryCode and Audience = $audienceCode]/DefinitionText" />",
    "text": "<xsl:value-of select="//SpanishTermDefinition[Dictionary = $dictionaryCode and Audience = $audienceCode]/DefinitionText" />"
  },
      </xsl:when>
    </xsl:choose>
  </xsl:template>
  
  <xsl:template name="RenderImageMediaLinks">
    <xsl:if test="//MediaLink[@type='image/jpeg' and @language=$languageCode and @audience=$imageLinkAudienceCode]">
      <xsl:variable name="count" select="count(//MediaLink[@type='image/jpeg' and @language=$languageCode and @audience=$imageLinkAudienceCode])" />
  "images": [
      <xsl:for-each select="//MediaLink[@type='image/jpeg' and @language=$languageCode and @audience=$imageLinkAudienceCode]">
        <xsl:apply-templates select="." mode="images"/><!--
    Output a comma unless this is the last element of the set.
    --><xsl:if test="position() != $count">,</xsl:if>
  </xsl:for-each>
    ],<!--
--></xsl:if>
  </xsl:template>

  <xsl:template match="MediaLink" mode="images">
  {
      "ref": "CDR<xsl:call-template name="GetNumericID">
        <xsl:with-param name="cdrid" select="@ref" />
      </xsl:call-template>.jpg",
      "alt": "<xsl:value-of select="@alt"/>"
      <!-- If caption is rendered, it includes a comma for alt.-->
      <xsl:if test="Caption[@language = $languageCode]">, "caption": "<xsl:copy-of select="Caption"/>"</xsl:if>
    }<!--
--></xsl:template>


  <!--
    Render the pronunciation data structure with pronunciation key and audio file.
  -->
  <xsl:template name="RenderPronunciation">

    <!-- Calculate media file name. -->
    <xsl:variable name="mediaFile">
      <xsl:call-template name="GetNumericID">
        <xsl:with-param name="cdrid" select="//MediaLink[@type='audio/mpeg' and @language=$languageCode]/@ref" />
      </xsl:call-template>.mp3<!-- suppress line break.
--></xsl:variable>

    <xsl:variable name="pronunciationKey">
      <xsl:choose>
        <xsl:when test="$targetLanguage = 'English'"><xsl:value-of select="//TermPronunciation"/></xsl:when>
        <xsl:when test="$targetLanguage = 'Spanish'"><!-- Never present for Spanish --></xsl:when>
      </xsl:choose>
    </xsl:variable>

    "pronunciation": {
      "audio": "<xsl:value-of select="$mediaFile"/>",
      "key": "<xsl:value-of select="$pronunciationKey"/>"
    },
  </xsl:template>

  <!--
    Renders the Related Information structure.
    
    NOTE: This is the last item in the output data structure and therefore does not render a comma
          at the end.
  -->
  <xsl:template name="RenderRelatedInformation">
    "related": {
    <xsl:call-template name="RenderRelatedDrugSummaries" />
    <xsl:call-template name="RenderRelatedExternalRefs" />
    <xsl:call-template name="RenderRelatedSummaryRefs" />
    <xsl:call-template name="RenderRelatedTermRefs" />
    }
  </xsl:template>

  <!--
    Helper template to render the Drug Info Summary portion of the related information section.
  -->
  <xsl:template name="RenderRelatedDrugSummaries">
    "drug_summary": [
      <xsl:variable name="count" select="count(//RelatedInformation/RelatedDrugSummaryRef)" />
      <xsl:for-each select="//RelatedInformation/RelatedDrugSummaryRef">{
          "id": "<xsl:call-template name="GetNumericID"><xsl:with-param name="cdrid" select="@href"/></xsl:call-template>",
          "text" : "<xsl:value-of select="node()"/>
      }<xsl:if test="position() != $count">,
      </xsl:if>
      </xsl:for-each>
    ],
  </xsl:template>

  <!--
    Helper template to render the External reference portion of the related information section.
  -->
  <xsl:template name="RenderRelatedExternalRefs">
    "external": [
    <xsl:variable name="count" select="count(//RelatedInformation/RelatedExternalRef[@UseWith = $languageCode])" />
    <xsl:for-each select="//RelatedInformation/RelatedExternalRef[@UseWith = $languageCode]">  {
        "url": "<xsl:value-of select="@xref"/>",
        "text": "<xsl:value-of select="node()"/>
      }<xsl:if test="position() != $count">,
    </xsl:if>
    </xsl:for-each>
    ],
  </xsl:template>

  <!--
    Helper template to render the Summary reference portion of the related information section.
  -->
  <xsl:template name="RenderRelatedSummaryRefs">
    "summary": [
    <xsl:variable name="count" select="count(//RelatedInformation/RelatedSummaryRef[@UseWith = $languageCode])" />
    <xsl:for-each select="//RelatedInformation/RelatedSummaryRef[@UseWith = $languageCode]">  {
        "id": "<xsl:call-template name="GetNumericID"><xsl:with-param name="cdrid" select="@href"/></xsl:call-template>",
        "text": "<xsl:value-of select="node()"/>
      }<xsl:if test="position() != $count">,
      </xsl:if>
    </xsl:for-each>
    ],
  </xsl:template>

  <!--
    Helper template to render the Glossary Term reference portion of the related information section.

    ASSUMPTION: Terms in one dictionary will only reference terms in the same dictionary.
  -->
  <xsl:template name="RenderRelatedTermRefs">
    "term": [
    <xsl:variable name="count" select="count(//RelatedInformation/RelatedGlossaryTermRef)" />
    <xsl:for-each select="//RelatedInformation/RelatedGlossaryTermRef"> {
        "id": "<xsl:call-template name="GetNumericID"><xsl:with-param name="cdrid" select="@href"/></xsl:call-template>",
        "dictionary": "<xsl:value-of select="$targetDictionary" />",
        "text": "<xsl:value-of select="node()"/>
      }<xsl:if test="position() != $count">,
      </xsl:if>
    </xsl:for-each>
    ],
  </xsl:template>

</xsl:stylesheet>
