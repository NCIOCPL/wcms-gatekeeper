<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    <xsl:output method="xml" indent="yes"/>

  <!-- Default to English, Patient, Cancer.gov dictionary -->
  <xsl:param        name = "targetLanguage"
                  select = "'English'"/>

  <xsl:param        name = "targetAudience"
                  select = "'Patient'" />

  <xsl:param        name = "targetDictionary"
                  select = "'Term'"/>

  
  <xsl:template match="/">
    <!-- The middle tier renderer expects the transform to produce a valid XML document,
         so we end up wrapping the rendered JSON...  (bleah) -->
    <Rendered>
      <xsl:apply-templates select="*" />
    </Rendered>
  </xsl:template>
  
  <xsl:template match="/GlossaryTerm">
    Placeholder.
  </xsl:template>
  
  
    <!--<xsl:template match="@* | node()">
        <xsl:copy>
            <xsl:apply-templates select="@* | node()"/>
        </xsl:copy>
    </xsl:template>-->
</xsl:stylesheet>
