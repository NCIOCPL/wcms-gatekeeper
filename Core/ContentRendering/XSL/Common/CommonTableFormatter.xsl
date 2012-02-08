<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
							  xmlns:msxsl="urn:schemas-microsoft-com:xslt"
							  xmlns:scripts="urn:local-scripts">

  <xsl:import href="DeviceFilter.xslt"/>

  <xsl:include href="TableRender.xslt"/>

  <xsl:output method="xml"/>

  <!--
  
    Shared code for rendering tables in Protocol, CTGovProtocol, and Summary documents.
  
  -->
  <xsl:template match="Table">
    <xsl:apply-templates select="." mode="ApplyDeviceFilter"/>
  </xsl:template>

  <xsl:template match="Table" mode="deviceFiltered">
      <!-- The TableSectionXML tag is needed so the renderer can limit itself
    to only rewriting styles in a limited region of the rendered page. -->
      <TableSectionXML>
        <a name="SectionXML{@id}">

          <xsl:call-template name="TableRender">
            <xsl:with-param name="table-cell-class">Summary-SummarySection-Small</xsl:with-param>
          </xsl:call-template>

          <xsl:element name="a">
            <xsl:attribute name="name">END_TableSection</xsl:attribute>
          </xsl:element>
        </a>
      </TableSectionXML>

	</xsl:template>
	
	<!--End of table -->
</xsl:stylesheet>
