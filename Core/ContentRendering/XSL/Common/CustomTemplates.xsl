<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
							  xmlns:msxsl="urn:schemas-microsoft-com:xslt"
							  xmlns:scripts="urn:local-scripts">

	<xsl:output method="xml"/>

	<xsl:template name="CommaDelimit">
		<xsl:param name="NodeSet" />
		
		<xsl:for-each select="$NodeSet">
			<xsl:if test="position()!=1">
				<xsl:value-of select="', '"/>				
			</xsl:if>
			<xsl:apply-templates select="."/>				
		</xsl:for-each>			
		
	</xsl:template>
							  
</xsl:stylesheet>