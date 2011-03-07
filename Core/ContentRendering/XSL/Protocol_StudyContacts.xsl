<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
						  xmlns:msxsl="urn:schemas-microsoft-com:xslt"
						  xmlns:scripts="urn:local-scripts">

	<xsl:include href="Common/CommonElements.xsl"/>
	<xsl:include href="Common/CommonScripts.xsl"/>
	<xsl:include href="Common/CustomTemplates.xsl"/>

	<xsl:output method="xml"/>

	<xsl:param name="RootUrl"/>
	<xsl:param name="ProtocolIDString"/>
	
	<xsl:template match="/">
		<Protocol>
			<xsl:apply-templates/>
		</Protocol>
	</xsl:template>
	
	<xsl:template match="*">
		<!-- suppress defaults -->		
	</xsl:template>
	
	<!-- **************************** Control Section ******************************* -->
	
	<xsl:template match="Protocol">
		<p>
			<xsl:apply-templates select="ProtocolTitle"/>
			<xsl:apply-templates select="ProtocolAdminInfo/ProtocolSites"/>
		</p>
		<p>
			<xsl:copy-of select="$ReturnToTopBar"/>
		</p>
	</xsl:template>
	
	<!-- ************************** End Control Section ***************************** -->
	

	<!-- ********************************* Title ************************************ -->

	<xsl:template match="ProtocolTitle">
		<xsl:if test="@Audience = 'Patient'">
			<H1><xsl:apply-templates/></H1>
		</xsl:if>
	</xsl:template>
	
	<!-- ******************************** End Title ********************************* -->
	
	
	<!-- ************************* Study Contacts *********************************** -->
		
	<xsl:template match="ProtocolSites">
		<xsl:variable name="ProtocolSites" select=".//PostalAddress"/>

		<a name="SitesAndContacts">
			<H2>STUDY SITES AND CONTACTS</H2>
		</a>			

		<table border="0" cellspacing="0" cellpadding="0">
			<tr>
				<td width="30"></td>
				<td></td>
			</tr>
			
		<xsl:for-each select="$ProtocolSites">
			<xsl:sort select="CountryName"/>
			<xsl:sort select="PoliticalSubUnitName"/>
			<xsl:sort select="City"/>
			
			<xsl:choose>
				<xsl:when test="position()!=1">
					<xsl:choose>
						<xsl:when test="$CurrentCountry!=' '">
							<xsl:if test="$CurrentCountry!=CountryName">
								<tr><td colspan="2"><BR/><B><xsl:value-of select="CountryName"/></B></td></tr>								
							</xsl:if>
						</xsl:when>
						<xsl:otherwise>
								<tr><td colspan="2"><BR/><B><xsl:value-of select="CountryName"/></B></td></tr>
						</xsl:otherwise>
					</xsl:choose>
					
					<xsl:choose>
						<xsl:when test="$CurrentState!=' '">
							<xsl:if test="$CurrentState!=PoliticalSubUnitName">
								<tr><td colspan="2"><BR/><B><xsl:value-of select="PoliticalSubUnitName"/></B></td></tr>
							</xsl:if>
						</xsl:when>
						<xsl:otherwise>
							<tr><td colspan="2"><BR/><B><xsl:value-of select="PoliticalSubUnitName"/></B></td></tr>
						</xsl:otherwise>					
					</xsl:choose>

					<xsl:choose>
						<xsl:when test="$CurrentCity!=' '">
							<xsl:if test="$CurrentCity!=City">
								<tr><td></td><td><B><xsl:value-of select="City"/></B></td></tr>
							</xsl:if>
						</xsl:when>
						<xsl:otherwise>
								<tr><td></td><td><B><xsl:value-of select="City"/></B></td></tr>
						</xsl:otherwise>					
					</xsl:choose>
				</xsl:when>
				<xsl:otherwise>
					<tr><td colspan="2"><B><xsl:value-of select="CountryName"/></B></td></tr>
					<tr><td colspan="2"><BR/><B><xsl:value-of select="PoliticalSubUnitName"/></B></td></tr>
					<tr><td></td><td><B><xsl:value-of select="City"/></B></td></tr>
				</xsl:otherwise>
			</xsl:choose>
									
			<tr>
				<td></td>
				<td>
					<ul>
					<li>
						<xsl:value-of select="../../../../../SiteName"/>
						<br/>
						<xsl:value-of select="concat(../../../PersonNameInformation/GivenName, ' ', ../../../PersonNameInformation/SurName, ', ')"/>
						Ph: <xsl:value-of select="../Phone"/>
					</li></ul>
				</td>
			</tr>
			
			<xsl:variable name="CurrentCountry" select="CountryName"/>	
			<xsl:variable name="CurrentState" select="PoliticalSubUnitName"/>	
			<xsl:variable name="CurrentCity" select="City"/>	
		
		</xsl:for-each>
		
		</table>

	</xsl:template>

	<!-- ************************* End Study Contacts ******************************* -->

</xsl:stylesheet>


  