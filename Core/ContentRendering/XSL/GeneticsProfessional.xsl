<?xml version='1.0'?>	
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
							  xmlns:msxsl="urn:schemas-microsoft-com:xslt"
							  xmlns:scripts="urn:local-scripts">
	
	<xsl:include href="Common/CommonElements.xsl"/>
	<xsl:include href="Common/CommonScripts.xsl"/>
	<xsl:include href="Common/CustomTemplates.xsl"/>

	<xsl:output method="xml"/>
	
	<xsl:param name="RootUrl"/>
	
	<xsl:template match="/">
		<GeneticsProfessional>
			<xsl:apply-templates/>
		</GeneticsProfessional>
	</xsl:template>
	
	<xsl:template match="*">
		<!-- suppress defaults -->		
	</xsl:template>
	
	<!-- ************************************ Formatting ************************************* -->	
	
	<xsl:template match="GENETICSPROFESSIONAL">
		<xsl:apply-templates select="NAME"/>
		
		<p/>
		<span class="header-A">Practice Locations</span>
		<br/>
		<xsl:apply-templates select="PRACTICELOCATIONS"/>
		
		<!-- from dtd, optional element -->
		<xsl:if test="TYPE">
			<p/>
			<span class="header-A">Profession</span>
			<br/>
			<xsl:apply-templates select="TYPE"/>
		</xsl:if>
		
		<!-- from dtd, optional element -->
		<xsl:if test="SPECIALTY">
			<p/>
			<span class="header-A">Medical/Genetics Specialties</span>
			<p/>
			<table border="1" cellspacing="0" cellpadding="5" class="Genetics">
				<tr>
					<td class="box-title"><b>Specialty</b></td>
					<td class="box-title"><b>Board Certified?</b></td>
				</tr>
				<xsl:apply-templates select="SPECIALTY"/>
			</table>
		</xsl:if>
		
		<!-- from dtd, optional element -->
		<xsl:if test="TEAMSERVICES">
			<p/>
			<span class="header-A">Member of a Team That Provides These Services</span>
			<ul>
				<xsl:apply-templates select="TEAMSERVICES"/>
			</ul>
		</xsl:if>
		
		<p/>
		<span class="header-A">Areas of Specialization</span>
		<p/>		
		<table border="1" cellspacing="0" cellpadding="5" class="Genetics">
			<tr>
				<td nowrap="1" class="box-title"><b>Family Cancer Syndrome</b></td>
				<td nowrap="1" class="box-title"><b>Type of Cancer</b></td>
			</tr>
			<xsl:apply-templates select="GENETICSERVICES/FAMILYCANCERSYNDROME"/>
		</table>				
		
		<!-- from dtd, optional element -->
		<xsl:if test="MEMBERSHIP">
			<p/>
			<span class="header-A">Professional Organizations</span><br/>
			<xsl:apply-templates select="MEMBERSHIP"/>			
		</xsl:if>
	
		<!-- from dtd, optional element -->
		<xsl:if test="NOTES">
			<p/>
			<span class="header-A">Additional Information</span>&#160;
			<xsl:value-of select="NOTES"/>
		</xsl:if>
		
		<!-- new search button -->
		<p>
			<a href="/search/search_geneticsservices.aspx"><img src="/images/new_search_red.gif" border="0" alt="New Search"/></a>
		</p>
		<p/>
	</xsl:template>
	
	<!-- ********************************** End Formatting *********************************** -->


	<!-- *************************************** Name **************************************** -->
	
	<xsl:template match="NAME">
		<span class="page-title">
			<xsl:value-of select="FIRSTNAME"/>&#160;<xsl:value-of select="LASTNAME"/><xsl:apply-templates select="../DEGREE"/>
		</span>
	</xsl:template>
	
	<xsl:template match="DEGREE">
		<xsl:value-of select="concat(', ',.)"/>
	</xsl:template>
	
	<!-- ************************************* End Name ************************************** -->
	
	
	<!-- ******************************** Practice Locations ********************************* -->
	
	<xsl:template match="PRACTICELOCATIONS">
		<xsl:if test="position() !=1">
			<br />
		</xsl:if>
		<xsl:apply-templates/>
	</xsl:template>
	
	<xsl:template match="INSTITUTION">
		<xsl:value-of select="."/><br/>
	</xsl:template>

	<xsl:template match="CADD">
		<xsl:value-of select="."/><br/>
	</xsl:template>

	<xsl:template match="CCTY">
    <!-- After 4.6 release, change this to only test for U.S.A. -->
		<xsl:if test="normalize-space(.) !='U.S.A.' and normalize-space(.) !='United States'">
			<xsl:value-of select="."/><br/>
		</xsl:if>
	</xsl:template>

	<xsl:template match="CPHN">
		ph: <xsl:value-of select="."/><br/>
	</xsl:template>

	<xsl:template match="CEML">
		e-mail: 
		<xsl:element name="a">
			<xsl:attribute name="href">
				mailto:<xsl:value-of select="."/>
			</xsl:attribute>
			<xsl:value-of select="."/>
		</xsl:element>
		<br/>
	</xsl:template>
	
	<!-- ****************************** End Practice Locations ******************************* -->
	
	
	<!-- ************************************ Profession ************************************* -->
	
	<xsl:template match="TYPE">
		<xsl:value-of select="."/><br/>		
	</xsl:template>
	
	<!-- ********************************** End Profession *********************************** -->
	

	<!-- *************************** Medical/Genetics Specialties **************************** -->

	<xsl:template match="SPECIALTY">
		<tr>
			<xsl:if test="position() mod 2 = 0">
				<xsl:attribute name="bgcolor">#f5f5f3</xsl:attribute>
			</xsl:if>
							
			<td align="left" valign="top"><xsl:apply-templates select="SPECIALTYNAME"/>&#160;</td>
			<td align="left" valign="top">
				<xsl:choose>
					<xsl:when test="BDCT"><xsl:value-of select="BDCT"/></xsl:when>
					<xsl:otherwise>No</xsl:otherwise>
				</xsl:choose>
			</td>
		</tr>
	</xsl:template>

	<xsl:template match="SPECIALTYNAME">
		<xsl:value-of select="."/>
	</xsl:template>
	
	<!-- ************************* End Medical/Genetics Specialties ************************** -->
	

	<!-- ********************************** Team Services ************************************ -->
	
	<xsl:template match="TEAMSERVICES">
		<li class="Genetics">
			<xsl:value-of select="."/>
		</li>
	</xsl:template>
	
	<!-- ******************************** End Team Services ********************************** -->


	<!-- ***************************** Areas of Specialization ******************************* -->
	
	<xsl:template match="FAMILYCANCERSYNDROME">
		<tr>
			<xsl:if test="position() mod 2 = 0">
				<xsl:attribute name="bgcolor">#f5f5f3</xsl:attribute>
			</xsl:if>				
			
			<td valign="top"><xsl:value-of select="SYNDROMENAME/text()[position()=1]" />&#160;</td>
			<td valign="top">
				<xsl:for-each select=".//CANCERSITE">
					<xsl:if test="position()!=1"><xsl:value-of select="', '"/></xsl:if><xsl:apply-templates select="text()"/>
				</xsl:for-each>&#160;
			</td>
		</tr>
	</xsl:template>
	
	<!-- *************************** End Areas of Specialization ***************************** -->
	
	
	<!-- **************************** Professional Organizations ***************************** -->
	
	<xsl:template match="MEMBERSHIP">
		<xsl:for-each select="*">
			<xsl:value-of select="."/><br/>
		</xsl:for-each>
	</xsl:template>		
	
	<!-- ************************** End Professional Organizations *************************** -->
	
</xsl:stylesheet>
	
