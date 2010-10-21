<?xml version='1.0'?>	
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
							  xmlns:msxsl="urn:schemas-microsoft-com:xslt"
							  xmlns:scripts="urn:local-scripts">

	<xsl:include href="Common/CommonElements.xsl"/>
	<xsl:include href="Common/CommonScripts.xsl"/>
	<xsl:include href="Common/CustomTemplates.xsl"/>
	<xsl:include href="Common/CommonTableFormatter.xsl"/>
	
	<xsl:output method="xml"/>

	<xsl:template match="/">
		<Summary>
			<xsl:apply-templates/>
		</Summary>
	</xsl:template>
	
	<xsl:template match="*">
		<!-- suppress defaults -->	
	</xsl:template>
	
	<!-- ****************************** TOC Section ***************************** -->
	
	<xsl:template match="Summary">
		<xsl:apply-templates/>
	</xsl:template>

	<xsl:template match="SummaryTitle">
		<Span Class="Summary-Title"><xsl:apply-templates/></Span>
			<ul>
			<xsl:for-each select="//SummarySection"> <!-- Only select a valid title element  -->
				<xsl:if test="./Title[node()] = true()"> <!-- Only select top 3 levels of title for TOC -->
					<xsl:if test="count(ancestor::SummarySection) &lt; 3">
						<xsl:choose>
							<xsl:when test="count(ancestor::SummarySection) = 0">
								<li Class="Summary-SummaryTitle-Level1">
									<xsl:element name="a"><xsl:attribute name="href">#Section<xsl:value-of select="@id"/></xsl:attribute>
										<xsl:apply-templates select="Title"/>
									</xsl:element>
								</li>
							</xsl:when>
							<xsl:when test="count(ancestor::SummarySection) = 1">
								<ul>
									<li Class="Summary-SummaryTitle-Level2">
										<xsl:element name="a"><xsl:attribute name="href">#Section<xsl:apply-templates select="@id"/></xsl:attribute>
											<xsl:apply-templates select="Title"/>
										</xsl:element>
									</li>
								</ul>			
							</xsl:when>
							<xsl:when test="count(ancestor::SummarySection) = 2">
								<ul>
									<ul>
										<li Class="Summary-SummaryTitle-Level3">
											<xsl:element name="a"><xsl:attribute name="href">#Section<xsl:apply-templates select="@id"/></xsl:attribute>
												<xsl:apply-templates select="Title"/>
											</xsl:element>
										</li>
									</ul>
								</ul>
							</xsl:when>
						</xsl:choose>
					</xsl:if>
				</xsl:if>		
			</xsl:for-each>
			</ul>
		<!--xsl:copy-of select="$ReturnToTopBar"/-->	
	
	</xsl:template>

	<!-- *************************** End TOC Section **************************** -->
	
	
	<!-- *********************** Summary Section Section ************************ -->

    <xsl:template match="SummarySection">
		<xsl:choose>
			<xsl:when test="count(ancestor::SummarySection) = 0">
				<xsl:element name="a">
					<xsl:attribute name="name">Section<xsl:value-of select="@id"/></xsl:attribute>
					<xsl:call-template name="SectionDetails"/>
				</xsl:element>
			</xsl:when>
			<xsl:otherwise>
				<xsl:element name="a">
					<xsl:attribute name="name">Section<xsl:value-of select="@id"/></xsl:attribute>
				</xsl:element>
				<xsl:call-template name="SectionDetails"/>
				<xsl:element name="a">
					<xsl:attribute name="name">END_Section<xsl:value-of select="@id"/></xsl:attribute>
				</xsl:element>
			</xsl:otherwise>
			
		</xsl:choose>
	</xsl:template>
	
	<xsl:template name="SectionDetails">
		<xsl:if test="name(..) = 'Summary'">
				<xsl:if test="count(descendant-or-self::KeyPoint) != 0">
				<table cellSpacing="0" cellPadding="0" width="100%" align="center" border="1">
					<tr>
						<td class="Summary-SummarySection-Keypoint-Title">
							<xsl:choose>
								<xsl:when test="/Summary/SummaryMetaData/SummaryLanguage != 'English'">
									Puntos importantes de esta sección
								</xsl:when>
								<xsl:otherwise>Key Points for This Section 
								</xsl:otherwise>
							</xsl:choose>
						</td>
					</tr>
					<tr>
						<td>
						<img src="/images/spacer.gif" width="1" height="5" alt="" border="0" />
						<ul Class="Summary-SummarySection-KeyPoint-UL-Bullet">
							<xsl:for-each  select="descendant-or-self::KeyPoint">
								<xsl:choose>
									<xsl:when test="count(ancestor::SummarySection) = 2">
										<li Class="Summary-SummarySection-KeyPoint-LI">
											<xsl:element name="a">
												<xsl:attribute name="href">#Keypoint<xsl:value-of select="count(preceding::KeyPoint) + 1"/></xsl:attribute>
												<xsl:apply-templates/>
											</xsl:element>
										</li>
									</xsl:when>
									<xsl:when test="count(ancestor::SummarySection) = 3">
										<ul Class="Summary-SummarySection-KeyPoint-UL-Dash">
											<li Class="Summary-SummarySection-KeyPoint-LI">
												<xsl:element name="a">
													<xsl:attribute name="href">#Keypoint<xsl:value-of select="count(preceding::KeyPoint) + 1"/></xsl:attribute>
													<xsl:apply-templates/>
												</xsl:element>
											</li>
										</ul>			
									</xsl:when>
									<xsl:when test="count(ancestor::SummarySection) = 4">
										<li Class="Summary-SummarySection-KeyPoint-LI">
											<xsl:element name="a">
												<xsl:attribute name="href">#Keypoint<xsl:value-of select="count(preceding::KeyPoint) + 1"/></xsl:attribute>
												<xsl:apply-templates/>
											</xsl:element>
										</li>
									</xsl:when>
									<xsl:otherwise>
										<ul Class="Summary-SummarySection-KeyPoint-UL-Dash">
											<li Class="Summary-SummarySection-KeyPoint-LI">
												<xsl:element name="a">
													<xsl:attribute name="href">#Keypoint<xsl:value-of select="count(preceding::KeyPoint) + 1"/></xsl:attribute>
													<xsl:apply-templates/>
												</xsl:element>
											</li>
										</ul>			
									</xsl:otherwise>
								</xsl:choose>
							</xsl:for-each>
						</ul>
						<img src="/images/spacer.gif" width="1" height="5" alt="" border="0" />
						</td>
					</tr>	
				</table>
				</xsl:if>
			</xsl:if>
			<xsl:apply-templates/>

	</xsl:template>
	
	<xsl:template match="Title">
		<xsl:if test="count(ancestor::SummarySection) = 1">
			<!--Span class="Summary-SummarySection-Title-Level1"><xsl:apply-templates/></Span-->
		</xsl:if>
		<xsl:if test="count(ancestor::SummarySection) = 2">
			<Span class="Summary-SummarySection-Title-Level2"><xsl:apply-templates/></Span>
		</xsl:if>
		<xsl:if test="count(ancestor::SummarySection) = 3">
			<Span class="Summary-SummarySection-Title-Level3"><xsl:apply-templates/></Span>
		</xsl:if>
		<xsl:if test="count(ancestor::SummarySection) &gt; 3 ">
			<Span class="Summary-SummarySection-Title-Level4"><xsl:apply-templates/></Span>
		</xsl:if>
		<xsl:if test="following-sibling::node()">
			<xsl:for-each select="following-sibling::node()">
				<xsl:choose>
					<xsl:when test="name() ='SummarySection' and position()=1"><br /><br /></xsl:when>
					<!--xsl:when test="name() ='SummarySection' and position()=2"><br /><br /></xsl:when-->
					<xsl:when test="name() ='Table' and position()=1"><br /><br /></xsl:when>
					<!--xsl:when test="name() ='Table' and position()=2"><br /><br /></xsl:when-->
				</xsl:choose>
			</xsl:for-each>
		</xsl:if>
	</xsl:template>
	
	<!-- ******************** End Summary Section Section *********************** -->
	
	
	<!-- 
		************************* Reference Section **************************** 
		Reference will be pre-rendered.
	-->

  
  <xsl:template match="SummaryRef">
    <div style="border: 1px solid green;">A SummaryRef goes here.<br />
      href = <xsl:value-of select="@href"/><br />
      url = <xsl:value-of select="@url"/><br />
      text content:<br/>
      <div style="border: 1px solid blue; margin: 5px;">
        <xsl:copy-of select="text()"/>
      </div>
    </div>
  </xsl:template>
  
  <!--
    Renders a placeholder tag structure for MediaLinks.  The MediaLink data is
    gathered during the Extract step and the tag structure replaced by the CMS,
    using the value of the objectID attribute.
  -->
	<xsl:template match="MediaLink">
    <div inlinetype="rxvariant" templatename="pdqSnMediaLink" objectId="{@ref}">
      Placeholder slot
    </div>
	</xsl:template>

	<xsl:template match="Reference">
		<xsl:element name="a">
			<xsl:attribute name="href">#Reference<xsl:for-each select="ancestor::SummarySection[child::ReferenceSection]"><xsl:value-of select="count(preceding-sibling::SummarySection) + 1"/>.</xsl:for-each><xsl:value-of select="@refidx"/></xsl:attribute>
			<xsl:value-of select="@refidx"/>
		</xsl:element>	
	</xsl:template>
	
	<xsl:template match="ReferenceSection">
		<xsl:element name="a">
			<xsl:attribute name="name">ReferenceSection</xsl:attribute>
		</xsl:element>
		<p>
		    <!-- When Language is English, use References. Otherwise, Biblioggrafia -->
		    <Span Class="Summary-ReferenceSection">
				<xsl:choose>
					<xsl:when test="//SummaryMetaData/SummaryLanguage != 'English'">
						Bibliografía
					</xsl:when>
					<xsl:otherwise>
						References
					</xsl:otherwise>
				</xsl:choose>
			</Span>
			<ol>
				<xsl:apply-templates/>
			</ol>
		</p>
		<xsl:element name="a">
			<xsl:attribute name="name">END_ReferenceSection</xsl:attribute>
		</xsl:element>
	</xsl:template>
	
	<xsl:template match="Citation">
		<li>			
			<xsl:element name="a">
				<xsl:attribute name="name">Reference<xsl:for-each select="ancestor::SummarySection"><xsl:value-of select="count(preceding-sibling::SummarySection) + 1"/>.</xsl:for-each><xsl:value-of select="@idx"/></xsl:attribute>
			</xsl:element>
			<xsl:apply-templates/>&#160;
			<xsl:element name="A">
				<xsl:choose>
					<xsl:when test="@ProtocolID !=''">
						<xsl:attribute name="href">/search/viewclinicaltrials.aspx?version=&#xD;
							<xsl:choose>
								<xsl:when test="/Summary/SummaryMetaData/SummaryAudience= 'Patients'">patient&#xD;</xsl:when>
								<xsl:otherwise>healthprofessional&#xD;</xsl:otherwise>
							</xsl:choose>
							&amp;cdrid=<xsl:value-of select="number(substring-after(@ProtocolID,'CDR'))"/>
						</xsl:attribute>
						<span class="Summary-Citation-PUBMED">[PDQ Clinical Trial]</span>
					</xsl:when>
					<xsl:when test="@PMID !=''">
						<xsl:attribute name="href">http://www.ncbi.nlm.nih.gov/entrez/query.fcgi?cmd=Retrieve&amp;db=PubMed&amp;list_uids=<xsl:value-of select="@PMID"/>&amp;dopt=Abstract</xsl:attribute><span class="Summary-Citation-PUBMED">[PUBMED Abstract]</span>
					</xsl:when>
				</xsl:choose>				
			</xsl:element>
			<br/><br/>
		</li>
	</xsl:template>
			
	<!-- Key point is with h4 tag-->
	<xsl:template match="KeyPoint">
		<p>
			<xsl:element name="a">
				<xsl:attribute name="name">Keypoint<xsl:value-of select="count(preceding::KeyPoint) + 1"/></xsl:attribute>
			</xsl:element>
			<Span Class="Summary-KeyPoint">
				<xsl:apply-templates/>
			</Span>
		</p>
	</xsl:template>

	<xsl:template match="ProfessionalDisclaimer">
		<xsl:element name="a">
			<xsl:attribute name="name">Disclaimer</xsl:attribute>
		</xsl:element>
		<br />
		<xsl:for-each  select="descendant::Section">
			<Span Class="Summary-ProfessionalDisclaimer-Title"><xsl:value-of select="Title"/></Span>:
			<Span Class="Summary-ProfessionalDisclaimer"><xsl:apply-templates/></Span>
		</xsl:for-each>
		<xsl:element name="a">
			<xsl:attribute name="name">END_Disclaimer</xsl:attribute>
		</xsl:element>
	</xsl:template>
	
	<xsl:template match="ProtocolRef">
		<xsl:element name="A">
			<xsl:attribute name="Class">Summary-ProtocolRef</xsl:attribute>
			<xsl:attribute name="href">/search/viewclinicaltrials.aspx?version=&#xD;
				<xsl:choose>
					<xsl:when test="/Summary/SummaryMetaData/SummaryAudience= 'Patients'">patient&#xD;</xsl:when>
					<xsl:otherwise>healthprofessional&#xD;</xsl:otherwise>
				</xsl:choose>
				&amp;cdrid=<xsl:value-of select="number(substring-after(@href,'CDR'))"/>
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
			<xsl:choose>
				<xsl:when test="/Summary/SummaryMetaData/SummaryLanguage != 'English'">
					<xsl:attribute name="href">/Common/PopUps/popDefinition.aspx?id=<xsl:value-of select="number(substring-after(@href,'CDR'))"/>&amp;version=HealthProfessional&amp;language=Spanish</xsl:attribute>
				</xsl:when>
				<xsl:otherwise>
					<xsl:attribute name="href">/Common/PopUps/popDefinition.aspx?id=<xsl:value-of select="number(substring-after(@href,'CDR'))"/>&amp;version=HealthProfessional&amp;language=English</xsl:attribute>
				</xsl:otherwise>
			</xsl:choose>
				
			<xsl:choose>
				<xsl:when test="/Summary/SummaryMetaData/SummaryLanguage != 'English'">
					<xsl:attribute name="onclick">javascript:popWindow('defbyid','<xsl:value-of select="@href"/>&amp;version=HealthProfessional&amp;language=Spanish'); return(false);</xsl:attribute>	
				</xsl:when>
				<xsl:otherwise>	
					<xsl:attribute name="onclick">javascript:popWindow('defbyid','<xsl:value-of select="@href"/>&amp;version=HealthProfessional&amp;language=English'); return(false);</xsl:attribute>	
				</xsl:otherwise>
			</xsl:choose>
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
			<xsl:choose>
				<xsl:when test="/Summary/SummaryMetaData/SummaryLanguage != 'English'">
					<xsl:choose>
						<xsl:when test="/Summary/SummaryMetaData/SummaryAudience != 'Patients'">
							<xsl:choose>
								<xsl:when test="/Summary/SummaryMetaData/SummaryType = 'Complementary and alternative medicine'">
									<xsl:attribute name="href">/Common/PopUps/popDefinition.aspx?id=<xsl:value-of select="number(substring-after(@href,'CDR'))"/>&amp;version=Patient&amp;language=Spanish</xsl:attribute>
								</xsl:when>
								<xsl:otherwise>
									<xsl:attribute name="href">/Common/PopUps/popDefinition.aspx?id=<xsl:value-of select="number(substring-after(@href,'CDR'))"/>&amp;version=HealthProfessional&amp;language=Spanish</xsl:attribute>
								</xsl:otherwise>
							</xsl:choose>
						</xsl:when>
						<xsl:otherwise>
							<xsl:attribute name="href">/Common/PopUps/popDefinition.aspx?id=<xsl:value-of select="number(substring-after(@href,'CDR'))"/>&amp;version=Patient&amp;language=Spanish</xsl:attribute>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:when>
				<xsl:otherwise>
					<xsl:choose>
						<xsl:when test="/Summary/SummaryMetaData/SummaryAudience != 'Patients'">
							<xsl:choose>
								<xsl:when test="/Summary/SummaryMetaData/SummaryType = 'Complementary and alternative medicine'">
									<xsl:attribute name="href">/Common/PopUps/popDefinition.aspx?id=<xsl:value-of select="number(substring-after(@href,'CDR'))"/>&amp;version=Patient&amp;language=English</xsl:attribute>
								</xsl:when>
								<xsl:otherwise>
									<xsl:attribute name="href">/Common/PopUps/popDefinition.aspx?id=<xsl:value-of select="number(substring-after(@href,'CDR'))"/>&amp;version=HealthProfessional&amp;language=English</xsl:attribute>
								</xsl:otherwise>
							</xsl:choose>
						</xsl:when>
						<xsl:otherwise>
							<xsl:attribute name="href">/Common/PopUps/popDefinition.aspx?id=<xsl:value-of select="number(substring-after(@href,'CDR'))"/>&amp;version=Patient&amp;language=English</xsl:attribute>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:otherwise>
			</xsl:choose>

			
			<xsl:choose>
				<xsl:when test="/Summary/SummaryMetaData/SummaryLanguage != 'English'">
					<xsl:choose>
						<xsl:when test="/Summary/SummaryMetaData/SummaryAudience != 'Patients'">
							<xsl:choose>
								<xsl:when test="/Summary/SummaryMetaData/SummaryType = 'Complementary and alternative medicine'">
									<xsl:attribute name="onclick">javascript:popWindow('defbyid','<xsl:value-of select="@href"/>&amp;version=Patient&amp;language=Spanish');  return(false);</xsl:attribute>
								</xsl:when>
								<xsl:otherwise>									
									<xsl:attribute name="onclick">javascript:popWindow('defbyid','<xsl:value-of select="@href"/>&amp;version=HealthProfessional&amp;language=Spanish');  return(false);</xsl:attribute>
								</xsl:otherwise>
							</xsl:choose>
						</xsl:when>
						<xsl:otherwise>
							<xsl:attribute name="onclick">javascript:popWindow('defbyid','<xsl:value-of select="@href"/>&amp;version=Patient&amp;language=Spanish');  return(false);</xsl:attribute>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:when>
				<xsl:otherwise>
					<xsl:choose>
						<xsl:when test="/Summary/SummaryMetaData/SummaryAudience != 'Patients'">
							<xsl:choose>
								<xsl:when test="/Summary/SummaryMetaData/SummaryType = 'Complementary and alternative medicine'">
									<xsl:attribute name="onclick">javascript:popWindow('defbyid','<xsl:value-of select="@href"/>&amp;version=Patient&amp;language=English');  return(false);</xsl:attribute>
								</xsl:when>
								<xsl:otherwise>									
									<xsl:attribute name="onclick">javascript:popWindow('defbyid','<xsl:value-of select="@href"/>&amp;version=HealthProfessional&amp;language=English');  return(false);</xsl:attribute>
								</xsl:otherwise>
							</xsl:choose>
						</xsl:when>
						<xsl:otherwise>
							<xsl:attribute name="onclick">javascript:popWindow('defbyid','<xsl:value-of select="@href"/>&amp;version=Patient&amp;language=English');  return(false);</xsl:attribute>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:otherwise>
			</xsl:choose>
			<xsl:value-of select="."/>
		</a>
		<xsl:if test="following-sibling::node()">
			<xsl:for-each select="following-sibling::node()">
				<xsl:if test="name() !='' and position()=1">&#160;</xsl:if>
			</xsl:for-each>
		</xsl:if>
	</xsl:template>
	
	<!-- *********************** End Content Section **************************** -->

</xsl:stylesheet>
