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
  <xsl:template name="CreateJsonText">
    <xsl:param name="text" />
    <!-- Escape backslash -->
    <xsl:variable name="slashEscaped">
      <xsl:call-template name="Replace">
        <xsl:with-param name="string" select="$text" />
        <xsl:with-param name="target" select="'\'" />
        <xsl:with-param name="replace" select="'\\'" />
      </xsl:call-template>
    </xsl:variable>
    <!-- Replace Carriage return -->
    <xsl:variable name="returnEscaped">
      <xsl:call-template name="Replace">
        <xsl:with-param name="string" select="$slashEscaped" />
        <xsl:with-param name="target" select="'&#xa;'" />
        <xsl:with-param name="replace" select="'\r'" />
      </xsl:call-template>
    </xsl:variable>
    <!-- Replace Newline -->
    <xsl:variable name="newlineEscaped">
      <xsl:call-template name="Replace">
        <xsl:with-param name="string" select="$returnEscaped" />
        <xsl:with-param name="target" select="'&#xd;'" />
        <xsl:with-param name="replace" select="'\n'" />
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="escaped">
      <xsl:call-template name="Replace">
        <xsl:with-param name="string" select="$newlineEscaped" />
        <xsl:with-param name="target" select="'&quot;'" />
        <xsl:with-param name="replace" select="'\&quot;'" />
      </xsl:call-template>
    </xsl:variable>
    <xsl:value-of select="normalize-space($escaped)"/>
  </xsl:template>


  <!--
    Scans through the value contained in the $string parameter and replaces
    all instances of $target with $replace.
  -->
  <xsl:template name="Replace">
    <xsl:param name="string" />
    <xsl:param name="target" />
    <xsl:param name="replace" />

    <xsl:choose>

      <!-- string contains the target -->
      <xsl:when test="contains($string, $target)">

        <!-- Everything before the target-->
        <xsl:variable name="pre" select="substring-before($string, $target)" />

        <!-- Everything after the target goes back through the template. -->
        <xsl:variable name="post">
          <xsl:call-template name="Replace">
            <xsl:with-param name="string" select="substring-after($string, $target)" />
            <xsl:with-param name="target" select="$target" />
            <xsl:with-param name="replace" select="$replace" />
          </xsl:call-template>
        </xsl:variable>

        <!-- Rebuild the string with the replacement characters. -->
        <xsl:value-of select="concat($pre, $replace, $post)"/>
        <!--<xsl:value-of select="$pre"/>-->
      </xsl:when>

      <!--Just return the string-->
      <xsl:otherwise>
        <xsl:value-of select="$string"/>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>
  
</xsl:stylesheet>
