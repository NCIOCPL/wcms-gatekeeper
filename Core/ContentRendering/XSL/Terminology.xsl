﻿<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:import href="Common/JSON-common-templates.xsl"/>
  <xsl:output method="text" indent="yes"/>

<!--
  This is the stylesheet for rendering Terminology documents into the JSON structure
  used for the Drug Dictionary.
-->

  <!-- Default targets are English, Patient, drug dictionary -->
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
    Match for the root (Term) element.  The entire JSON structure is created here.
  -->
<xsl:template match="/Term">
{ <xsl:call-template name="RenderDocumentID" />
  <xsl:call-template name="RenderTermName" />
  <xsl:call-template name="RenderAliasList" />
  <xsl:call-template name="RenderDateFirstPublished" />
  <xsl:call-template name="RenderDateLastModified" />
  <xsl:call-template name="RenderDefinition" />
}
</xsl:template>

  <xsl:template name="RenderDocumentID">
  "id": "<xsl:call-template name="GetNumericID"><xsl:with-param name="cdrid" select="/Term/@id"/></xsl:call-template>",
  </xsl:template>
  
  <xsl:template name="RenderTermName">
  "term": "<xsl:call-template name="CreateJsonText"><xsl:with-param name="text" select="//PreferredName"/></xsl:call-template>",
  </xsl:template>

  <xsl:template name="RenderAliasList">
  "alias": <xsl:choose>
      <xsl:when test="//OtherName">
        <xsl:variable name="count" select="count(//OtherName)"/>
        [<xsl:for-each select="//OtherName">
          {
            "name": "<xsl:call-template name="CreateJsonText"><xsl:with-param name="text" select="OtherTermName"/></xsl:call-template>",
            "type": "<xsl:call-template name="CreateJsonText"><xsl:with-param name="text" select="OtherNameType"/></xsl:call-template>"
          }<xsl:if test="position() != $count">,</xsl:if>
      </xsl:for-each>]
      </xsl:when>
      <xsl:otherwise>[] <!-- Empty list --></xsl:otherwise>
    </xsl:choose>,
  </xsl:template>
  
  <xsl:template name="RenderDateFirstPublished">
    <xsl:if test="//DateFirstPublished">"date_first_published": "<xsl:value-of select="//DateFirstPublished"/>", </xsl:if>
  </xsl:template>

  <xsl:template name="RenderDateLastModified">
    <xsl:if test="//DateLastModified">"date_last_modified": "<xsl:value-of select="//DateLastModified"/>", </xsl:if>
  </xsl:template>

  <!--
    Renders the data structure containing the term's definition.
    
    NOTE: Makes use of a locally defined "ExternalRef" template, rather than the one found in JSON-common-templates.

          This is the last item in the output data structure and therefore does not render a comma
          at the end.
  -->
  <xsl:template name="RenderDefinition">
  "definition": {
    "html": "<xsl:apply-templates select="//Definition/DefinitionText" />",
    "text": "<xsl:call-template name="CreateJsonText"><xsl:with-param name="text" select="//Definition/DefinitionText" /></xsl:call-template>"
  }
  </xsl:template>


  
  <!--
  
    Begin utility templates
    
  -->
  

  <!--
    ExternalRef - matches the ExternalRef template, outputting external URLs in an <a> tag.
    
    This overrides the ExternalRef template seen in Common/JSON-common-templates.xsl
  -->
  <xsl:template match="ExternalRef"><!--
    -->&lt;a class=\"navigation-dark-red\" href=\"<xsl:value-of select="@xref" />\"&gt;<xsl:apply-templates select="node()" />&lt;/a&gt;<!--
--></xsl:template>

  <!--
    Helper template for any element which must be output without spaces.
  -->
  <xsl:template match="DefinitionText">
    <xsl:call-template name="CreateJsonText">
      <xsl:with-param name="text" select="." />
    </xsl:call-template>
  </xsl:template>


</xsl:stylesheet>
