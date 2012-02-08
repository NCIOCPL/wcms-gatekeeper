<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
>
    <xsl:output method="xml" indent="yes"/>

  <!-- Device to target, passed from DocumentRenderer.Render(). -->
  <xsl:param name="targetedDevice" select="screen" />

  <!--
      Filter for devices using the IncludedDevices and ExcludedDevices attributes.
      This is a work-around for XSLT not allowing xpath expressions to be
      stored in or evaluated from variables.
      
      Criteria for passing the filter are:
        $targetedDevice is found in the IncludedDevices attribute or IncludedDevices is not present.
        and $targetedDevice is not present in the ExcludedDevices attribute.

      Elements which don't have @IncludedDevices/@ExcludedDevices attributes are unaffected due
        to the inclusion of the not(@IncludedDevices) and not(@ExcludedDevices) conditions.

      Usage:
      
        Anywhere a filtered element is meant to be output, use:
        
            <xsl:apply-templates select="ELEMENTNAME" mode="ApplyDeviceFilter" />

        Create a template with mode="deviceFiltered" to output the element.
        
            <xsl:template match="ELEMENTNAME" mode="deviceFiltered">
                <- - Only outputs elemements which match the filter - ->
            </xsl:template>

      Based on http://stackoverflow.com/questions/3884927/how-to-use-xsl-variable-in-xslapply-templates/3885071#3885071
    -->

  <xsl:template match="*" mode="ApplyDeviceFilter">
    <!--
      This filter expression MUST be synchronized with the Extract path values for:
          SummaryMediaLink
          SummaryTopSection
          SummarySubSection
          SummarySectionTable
      -->
    <xsl:if test="(contains(concat(' ', @IncludedDevices, ' '), concat(' ', $targetedDevice, ' ')) or not(@IncludedDevices)) and (not(contains(concat(' ', @ExcludedDevices, ' '), concat(' ', $targetedDevice, ' '))) or not(@ExcludedDevices))">
      <xsl:apply-templates select="." mode="deviceFiltered"/>
    </xsl:if>
  </xsl:template>
  
</xsl:stylesheet>
