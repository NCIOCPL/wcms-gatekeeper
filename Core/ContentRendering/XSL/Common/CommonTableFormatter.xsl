<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
							  xmlns:msxsl="urn:schemas-microsoft-com:xslt"
							  xmlns:scripts="urn:local-scripts">

	<xsl:output method="xml"/>
  <xsl:include href="TableRender.xslt"/>


  <!--
  
    Shared code for rendering tables in Protocol, CTGovProtocol, and Summary documents.
  
  -->

  <xsl:template match="Table">
				
		<TableSectionXML>
				<xsl:element name="a">
				<xsl:attribute name="name">SectionXML<xsl:value-of select="@id"/></xsl:attribute>
		
				
				<!--xsl:element name="a"><xsl:attribute name="name">TableSection</xsl:attribute></xsl:element>
				<xsl:element name="a"><xsl:attribute name="name">Section<xsl:value-of select="@id"/></xsl:attribute></xsl:element-->
          <xsl:call-template name="TableRender">
            <xsl:with-param name="table-cell-class">Summary-SummarySection-Small</xsl:with-param>
          </xsl:call-template>

          <xsl:element name="a"><xsl:attribute name="name">END_TableSection</xsl:attribute></xsl:element>
	  </xsl:element>
		</TableSectionXML>

	</xsl:template>
	
	<!--End of table -->
</xsl:stylesheet>
