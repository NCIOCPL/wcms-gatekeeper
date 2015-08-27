<?xml version="1.0" encoding="utf-8"?>
<!--
  Shared templates for the set of XSL files used for rendering JSON.
-->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
>
    <xsl:output method="text" indent="yes"/>

  
  <!--
    GetNumericID - Converts an id formatted CDR000012345 to a number with no leading zeros.
    
    Args:
      cdrid - A numeric ID with the first four characters being "CDR0".
  -->
  <xsl:template name="GetNumericID">
    <xsl:param name="cdrid" />
    <!--
      translate - renders the letters to lowercase.
      substring-after - remove the leading "cdr0"
      number - convert to numeric, implictly removing leading zeros.
    -->
    <xsl:value-of select="number(substring-after(translate($cdrid,'CDR','cdr'), 'cdr0'))" />
  </xsl:template>



  <!--
    Matches the ExternalRef, LOERef, ProtocolRef, GlossaryTermRef elements, outputting the common
    xref attribute as a link.
  -->
  <xsl:template match="ExternalRef|LOERef|ProtocolRef|GlossaryTermRef"><!--
    -->&lt;a href=\"<xsl:value-of select="@xref" />\"&gt;<xsl:apply-templates select="node()" />&lt;/a&gt;<!--
--></xsl:template>


  <!--
    Convert a blob of text into text suitable for inclusion in a JavaScript string literal.
  -->
  <xsl:template name="OutputJsonText">
    <xsl:param name="text" /><!--
      translate() changes linefeed and newline into spaces.  (Simply removing them can result in words separated by only
      a newline being placed together.)
    --><xsl:value-of select="normalize-space(translate($text, '&#xa;&#xd;','  '))"/>
  </xsl:template>

</xsl:stylesheet>
