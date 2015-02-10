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
		<div class='result'>
			<xsl:apply-templates/>
		</div>
	</xsl:template>
	
	<xsl:template match="*">
		<!-- suppress defaults -->		
	</xsl:template>
	
	<!-- ************************************ Formatting ************************************* -->	
	
	<xsl:template match="GENETICSPROFESSIONAL">
		<xsl:apply-templates select="NAME"/>
		
		<div class="description">
		<div class="row">
			<div class="medium-3 columns header">Practice Locations</div>
			<div class="medium-9 columns">
				<xsl:apply-templates select="PRACTICELOCATIONS"/>
			</div>
		</div>
		
		<!-- from dtd, optional element -->
		<xsl:if test="TYPE">
			<div class="row">
				<div class="medium-3 columns header">Profession</div>
				<div class="medium-9 columns">
					<xsl:apply-templates select="TYPE"/>
				</div>
			</div>
		</xsl:if>
		
		<!-- from dtd, optional element -->
		<xsl:if test="SPECIALTY">
			<table class="table-default">
			<caption>Medical/Genetics Specialties</caption>
				<thead>
					<tr>
						<th>Specialty</th>
						<th>Board Certified?</th>
					</tr>
				</thead>
				<tbody>
					<xsl:apply-templates select="SPECIALTY"/>
				</tbody>
			</table>
		</xsl:if>
		
		<!-- from dtd, optional element -->
		<xsl:if test="TEAMSERVICES">
			<div class="row">
				<div class="medium-3 columns header">Member of a Team That Provides These Services</div>
				<div class="medium-9 columns">
					<ul>
						<xsl:apply-templates select="TEAMSERVICES"/>
					</ul>
				</div>
			</div>
		</xsl:if>
		
		<table class="table-default">
			<caption>Areas of Specialization</caption>
			<thead>
				<tr>
					<th>Family Cancer Syndrome</th>
					<th>Type of Cancer</th>
				</tr>
			</thead>
			<tbody>
				<xsl:apply-templates select="GENETICSERVICES/FAMILYCANCERSYNDROME"/>
			</tbody>
		</table>				
		
		<!-- from dtd, optional element -->
		<xsl:if test="MEMBERSHIP">
			<div class="row">
				<div class="medium-3 columns header">Professional Organizations</div>
				<div class="medium-9 columns">
					<xsl:apply-templates select="MEMBERSHIP"/>		
				</div>
			</div>					
		</xsl:if>
	
		<!-- from dtd, optional element -->
		<xsl:if test="NOTES">
			<div class="row">
				<div class="medium-3 columns header">Additional Information</div>
				<div class="medium-9 columns">
					<xsl:value-of select="NOTES"/>
				</div>
			</div>
		</xsl:if>
		
		<!-- new search button -->
		<p>
			<a class="action button" href="/search/search_geneticsservices.aspx">New Search</a>
		</p>
		</div>
	</xsl:template>
	
	<!-- ********************************** End Formatting *********************************** -->


	<!-- *************************************** Name **************************************** -->
	
	<xsl:template match="NAME">
		<h2>
			<xsl:value-of select="FIRSTNAME"/>&#160;<xsl:value-of select="LASTNAME"/><xsl:apply-templates select="../DEGREE"/>
		</h2>
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
	

	<!-- *************************** Medical/Genetics Specialities **************************** -->

	<xsl:template match="SPECIALTY">
		<tr>
			<td><xsl:apply-templates select="SPECIALTYNAME"/>&#160;</td>
			<td>
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
			<td><xsl:value-of select="SYNDROMENAME/text()[position()=1]" />&#160;</td>
			<td>
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
	
