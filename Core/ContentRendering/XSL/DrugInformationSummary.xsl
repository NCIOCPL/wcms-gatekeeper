<?xml version='1.0'?>	
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
							  xmlns:msxsl="urn:schemas-microsoft-com:xslt"
							  xmlns:scripts="urn:local-scripts">

	<xsl:include href="Common/CommonElements.xsl"/>
	<xsl:include href="Common/CommonScripts.xsl"/>
	<xsl:include href="Common/CustomTemplates.xsl"/>
	<xsl:include href="Common/TableRender.xslt"/>
	
	<xsl:output method="xml"/>

	<xsl:template match="/">
		<DrugInformationSummary>
			<xsl:apply-templates/>
		</DrugInformationSummary>
	</xsl:template>
	
	<xsl:template match="*">
		<!-- suppress defaults -->	
	</xsl:template>
	
	<!-- ****************************** DrugInfoSummary ***************************** -->
	<xsl:template match="DrugInformationSummary">
		<xsl:apply-templates select="DrugInfoTitle"/>
		<xsl:apply-templates select="DrugInfoMetaData/DrugInfoDescription"/>
		<xsl:apply-templates select="DrugInfoMetaData"/>
		<xsl:apply-templates select="Section"/>
		<xsl:apply-templates select="DrugInfoDisclaimer"/>
	</xsl:template>
	
	<xsl:template match="DrugInfoMetaData/DrugInfoDescription">
		<p><xsl:apply-templates /></p>
	</xsl:template>	
		
	<xsl:template match="DrugInfoMetaData">
		<table width="571" cellspacing="0" cellpadding="0" border="0">
			<xsl:if test="USBrandNames[node()] = true()">
				<xsl:for-each select="USBrandNames/USBrandName">
					<tr>
					
					<xsl:choose>
						<xsl:when test="position()=1">
							<td valign="top" width="25%"><b>US Brand Name(s):</b></td>
						</xsl:when>
						<xsl:otherwise>
							<td valign="top" width="25%"></td>
						</xsl:otherwise>
					</xsl:choose>
					
					<td valign="top" width="10"><img src="/images/spacer.gif" alt="" width="10" height="1" border="0" /></td>
					
					<td valign="top" width="75%">
            <xsl:apply-templates />
					</td>
					
					</tr>
					<tr>
						<td valign="top" colspan="3"><img src="/images/spacer.gif" alt="" width="10" height="6" border="0" /></td>
					</tr>
				</xsl:for-each>
			</xsl:if>
			
			<xsl:if test="Synonyms[node()] = true()">

				<xsl:for-each select="Synonyms/Synonym">
					<tr>
					
					<xsl:choose>
						<xsl:when test="position()=1">
							<td valign="top" width="25%"><b>Other Name(s):</b></td>
						</xsl:when>
						<xsl:otherwise>
							<td valign="top" width="25%"></td>
						</xsl:otherwise>
					</xsl:choose>

					<td valign="top" width="10"><img src="/images/spacer.gif" alt="" width="10" height="1" border="0" /></td>
					
					<td valign="top" width="75%">
            <xsl:apply-templates />
					</td>

            <tr>
              <td valign="top" colspan="3"><img src="/images/spacer.gif" alt="" width="10" height="6" border="0" /></td>
            </tr>
          </tr>
				</xsl:for-each>
			</xsl:if>
			
			<tr>
				<td valign="top" colspan="3"><img src="/images/spacer.gif" alt="" width="10" height="6" border="0"></img></td>
			</tr>
			
			<xsl:if test="string-length(FDAApproved) &gt; 0">
				<tr>
				<td valign="top" width="28%"><b>FDA Approved:</b> </td>
				<td valign="top" width="10"><img src="/images/spacer.gif" alt="" width="10" height="1" border="0"></img>
				</td>
				<td valign="top" width="68%"><xsl:value-of select="FDAApproved"/></td>
				</tr>
			</xsl:if>
		</table>
	</xsl:template>	
			
    <xsl:template match="Section">
		<xsl:apply-templates/>
	</xsl:template>
	
	<xsl:template match="Title">
		<xsl:if test="count(ancestor::Section) = 1">
			<Span class="DrugInfoSummary-SummarySection-Title-Level1"><xsl:apply-templates/></Span>
		</xsl:if>
		<xsl:if test="count(ancestor::Section) = 2">
			<Span class="DrugInfoSummary-SummarySection-Title-Level2"><xsl:apply-templates/></Span>
		</xsl:if>
		<xsl:if test="count(ancestor::Section) = 3">
			<Span class="DrugInfoSummary-SummarySection-Title-Level3"><xsl:apply-templates/></Span>
		</xsl:if>
		<xsl:if test="count(ancestor::Section) &gt; 3 ">
			<Span class="DrugInfoSummary-SummarySection-Title-Level4"><xsl:apply-templates/></Span>
		</xsl:if>
		<xsl:if test="following-sibling::node()">
			<xsl:for-each select="following-sibling::node()">
				<xsl:choose>
					<xsl:when test="name() ='Section' and position()=1"><br /><br /></xsl:when>
					<xsl:when test="name() ='Section' and position()=2"><br /><br /></xsl:when>
					<xsl:when test="name() ='Table' and position()=1"><br /><br /></xsl:when>
					<xsl:when test="name() ='Table' and position()=2"><br /><br /></xsl:when>
				</xsl:choose>
			</xsl:for-each>
		</xsl:if>
	</xsl:template>
	

	<xsl:template match="ProtocolRef">
		<xsl:element name="A">
			<xsl:attribute name="Class">Summary-ProtocolRef</xsl:attribute>
			<xsl:attribute name="href">/search/viewclinicaltrials.aspx?version=patient&amp;cdrid=<xsl:value-of select="number(substring-after(@href,'CDR'))"/>
			</xsl:attribute>
			<xsl:value-of select="."/>
		</xsl:element>
		<xsl:if test="following-sibling::node()">
			<xsl:for-each select="following-sibling::node()">
				<xsl:if test="name() !='' and position()=1">&#160;</xsl:if>
			</xsl:for-each>
		</xsl:if>
	</xsl:template>
					
	<xsl:template match="LOERef">
		<a>
			<xsl:attribute name="Class">Summary-LOERef</xsl:attribute>
			<xsl:attribute name="href">/Common/PopUps/popDefinition.aspx?id=<xsl:value-of select="number(substring-after(@href,'CDR'))"/>&amp;version=HealthProfessional&amp;language=English</xsl:attribute>
			<xsl:attribute name="onclick">javascript:popWindow('defbyid','<xsl:value-of select="@href"/>&amp;version=HealthProfessional&amp;language=English'); return(false);</xsl:attribute>	
			<xsl:value-of select="."/>
		</a>
		<xsl:if test="following-sibling::node()">
			<xsl:for-each select="following-sibling::node()">
				<xsl:if test="name() !='' and position()=1">&#160;</xsl:if>
			</xsl:for-each>
		</xsl:if>
	</xsl:template>				
					
	<xsl:template match="GlossaryTermRef">
		<a>
		<xsl:attribute name="Class">Summary-GlossaryTermRef</xsl:attribute>
		<xsl:attribute name="href">/Common/PopUps/popDefinition.aspx?id=<xsl:value-of select="number(substring-after(@href,'CDR'))"/>&amp;version=Patient&amp;language=English</xsl:attribute>
		<xsl:attribute name="onclick">javascript:popWindow('defbyid','<xsl:value-of select="@href"/>&amp;version=Patient&amp;language=English');  return(false);</xsl:attribute>
		<xsl:value-of select="."/>
		</a>
		<xsl:if test="following-sibling::node()">
			<xsl:for-each select="following-sibling::node()">
				<xsl:if test="name() !='' and position()=1">&#160;</xsl:if>
			</xsl:for-each>
		</xsl:if>
	</xsl:template>
	
	<xsl:template match="DrugInfoDisclaimer">	
		<xsl:element name="a">
			<xsl:attribute name="name">Disclaimer</xsl:attribute>
		</xsl:element>
		<div class="note">
			<xsl:apply-templates/>
		</div>
		<br/>
		<xsl:element name="a">
			<xsl:attribute name="name">EndOfDisclaimer</xsl:attribute>
		</xsl:element>
	</xsl:template>
	
	<!-- *********************** End Content Section **************************** -->


  <xsl:template match="Table">
   <xsl:call-template name="TableRender">
        </xsl:call-template>
  </xsl:template>
  
</xsl:stylesheet>
  