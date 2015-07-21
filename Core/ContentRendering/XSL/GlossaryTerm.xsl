<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    <xsl:output method="text" indent="yes"/>

  <!-- Default to English, Patient, Cancer.gov dictionary -->
  <xsl:param        name = "targetLanguage"
                  select = "'English'"/>

  <xsl:param        name = "targetAudience"
                  select = "'Patient'" />

  <xsl:param        name = "targetDictionary"
                  select = "'Term'"/>

  <!--
    audienceCode, languageCode, and dictionaryCode contain the constant values
    used by the CDR. These variables provide a translation from the constants used by GateKeeper.
  -->
  <xsl:variable name="audienceCode">
    <xsl:choose>
      <xsl:when test="$targetAudience = 'Patient'">Patient</xsl:when>
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

  <xsl:template match="/">
<!-- Create the JSON structure directly within the root template. -->
term: {
  id: "12345",
  term: "<xsl:call-template name="RenderTermName" />",
  alias: [ ], <!-- The alias array is always empty for Glossary Term docs. -->
  <xsl:call-template name="RenderDateFirstPublished" />
  <xsl:call-template name="RenderDateLastModified" />
  <xsl:call-template name="RenderDefinition" />
  definition: {
    html: "<p>NOT IMPLEMENTED</p>",
    text: "NOT IMPLEMENTED"
  },
  <xsl:call-template name="RenderImageMediaLinks" />
  <xsl:call-template name="RenderPronunciation" />
  related: {
    drug_summary: [
      {
        language: "en",
        text: "Related Drug Summary",
        url: "http://www.cancer.gov/publications/dictionaries/cancer-terms?cdrid=45693"
      }
    ],
    external: [
      {
        language: "en",
        text: "Great Googly Moogly!",
        url: "http://www.google.com/"
      }
    ],
    summary: [
      {
        language: "en",
        text: "A Summary",
        url: "http://www.cancer.gov/types/lung/patient/non-small-cell-lung-treatment-pdq"
      }
    ],
    term: [
      {
        dictionary: "Term",
        id: "12345",
        text: "A related Term"
      }
    ]
  }
}
  </xsl:template>

  <xsl:template name="RenderTermName">
    <xsl:choose>
      <xsl:when test="$targetLanguage = 'English'"><xsl:value-of select="//TermName"/></xsl:when>
      <xsl:when test="$targetLanguage = 'Spanish'"><xsl:value-of select="//SpanishTermName"/></xsl:when>
      <xsl:otherwise><xsl:value-of select="//TermName"/></xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="RenderDateFirstPublished">
    <xsl:if test="//DateFirstPublished">date_first_published: "<xsl:value-of select="//DateFirstPublished"/>", </xsl:if>
  </xsl:template>

  <xsl:template name="RenderDateLastModified">
    <xsl:if test="//DateLastModified">date_last_modified: "<xsl:value-of select="//DateLastModified"/>", </xsl:if>
  </xsl:template>

  <xsl:template name="RenderDefinition">
<!--
  Using copy-of to copy nested elements, value-of to retrieve only the text.
  Is the "copy-of" the right way to do this?
-->
    <xsl:choose>
      <xsl:when test="$targetLanguage = 'English'">
  definition: {
    html: "<xsl:copy-of  select="//TermDefinition[Dictionary = $dictionaryCode and Audience = $audienceCode]/DefinitionText" />",
    text: "<xsl:value-of select="//TermDefinition[Dictionary = $dictionaryCode and Audience = $audienceCode]/DefinitionText" />"
  },
      </xsl:when>
      <xsl:when test="$targetLanguage = 'Spanish'">
  definition: {
    html: "<xsl:copy-of  select="//SpanishTermDefinition[Dictionary = $dictionaryCode and Audience = $audienceCode]/DefinitionText" />",
    text: "<xsl:value-of select="//SpanishTermDefinition[Dictionary = $dictionaryCode and Audience = $audienceCode]/DefinitionText" />"
  },
      </xsl:when>
    </xsl:choose>
  </xsl:template>
  
  <xsl:template name="RenderImageMediaLinks">
    <xsl:if test="//MediaLink[@type='image/jpeg' and @language=$languageCode and @audience=$audienceCode]">
  images: [
      <xsl:for-each select="//MediaLink[@type='image/jpeg' and @language=$languageCode and @audience=$audienceCode]">
        <xsl:apply-templates select="."/>
  </xsl:for-each>],
    </xsl:if>
  </xsl:template>

  <xsl:template match="MediaLink">
    {
      ref: "NOT_IMPLEMENTED",
      alt: "<xsl:value-of select="@alt"/>"
      <!-- If caption is rendered, it includes a comma for alt.-->
      <xsl:if test="Caption[@language = $languageCode]">, caption: "<xsl:copy-of select="Caption"/>"</xsl:if>
    }
  </xsl:template>


  <xsl:template name="RenderPronunciation">
    <xsl:choose>
      <xsl:when test="$targetLanguage = 'English'">
  pronunciation: {
    audio: "NOT IMPLEMENTED",
    key: "<xsl:value-of select="//TermPronunciation"/>"
  },
      </xsl:when>
      <xsl:when test="$targetLanguage = 'Spanish'">
  pronunciation: {
    audio: "NOT IMPLEMENTED",
    key: "" <!-- Never present for Spanish. -->
  },
      </xsl:when>
    </xsl:choose>
  </xsl:template>
  
</xsl:stylesheet>
