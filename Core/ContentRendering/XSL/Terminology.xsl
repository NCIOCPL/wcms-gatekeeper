﻿<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:import href="Common/JSON-common-templates.xsl"/>
  <xsl:output method="text" indent="yes"/>

  <!--
  
    This stylesheet creates the Dictionary JSON structure containing Term details for
    a single targeted Language, Audience and Dictionary.  Separate transformations are
    required to generate the JSON structure for each permutation of the three parameters.

    Correct function of these templates is VERY SENSITIVE TO EXTRA CARRIAGE RETURNS.
    Be very careful when using an editor (e.g. Visual Studio) which automatically
    reformats text to a "suggested" format.  Consider using a diff tool to check for
    unexpected formatting changes before committing to source control.

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
  <xsl:call-template name="RenderRelatedInformation" />
}
</xsl:template>

  <xsl:template name="RenderDocumentID">
  "id": "<xsl:call-template name="GetNumericID"><xsl:with-param name="cdrid" select="/Term/@id"/></xsl:call-template>",
  </xsl:template>
  
  <xsl:template name="RenderTermName">
  "term": "<xsl:apply-templates select="//PreferredName"/>",
  </xsl:template>

  <xsl:template name="RenderAliasList">
  "alias": <xsl:choose>
      <xsl:when test="//OtherName">
        <xsl:variable name="count" select="count(//OtherName)"/>
        [<xsl:for-each select="//OtherName">
          {
            "name": "<xsl:apply-templates select="OtherTermName" />",
            "type": "<xsl:value-of select="OtherNameType"/>"
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
  -->
  <xsl:template name="RenderDefinition">
  "definition": {
    "html": "<xsl:apply-templates select="//Definition/DefinitionText" />",
    "text": "<xsl:apply-templates select="//Definition/DefinitionText" mode="JSON" />"
  },
  </xsl:template>

  <!--
    Local implementation of ExternalRef (overrides JSON-common-templates) in order
    to output the navigation-dark-red CSS class.
  -->
  <xsl:template match="ExternalRef"><!--
    -->&lt;a class=\"navigation-dark-red\" href=\"<xsl:value-of select="@xref" />\"&gt;<xsl:apply-templates select="node()" />&lt;/a&gt;<!--
--></xsl:template>



  <!--
    Renders the Related Information structure.
    
    NOTE: This is the last item in the output data structure and therefore does not render a comma
          at the end.
  -->
  <xsl:template name="RenderRelatedInformation">
    "related": {
    <xsl:call-template name="RenderRelatedDrugSummaries" />
    }
  </xsl:template>

  <!--
    Helper template to render the Drug Info Summary portion of the related information section.
    
    NOTE: This data is not a native part of the Terminology document type.  This template will
          likely need to be updated as OCEPROJECT-3605.
  -->
  <xsl:template name="RenderRelatedDrugSummaries">
    "drug_summary": [
    <xsl:variable name="count" select="count(//RelatedDrugInfoSummary)" />
    <xsl:for-each select="//RelatedDrugInfoSummary">
      {
      "url": "<xsl:value-of select="."/>",
      "text" : "<xsl:apply-templates select="//PreferredName" />"
      }<xsl:if test="position() != $count">
        ,
      </xsl:if>
    </xsl:for-each>
    ]
  </xsl:template>



</xsl:stylesheet>
