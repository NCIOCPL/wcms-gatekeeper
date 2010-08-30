<?xml version="1.0"?>	
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
							  xmlns:msxsl="urn:schemas-microsoft-com:xslt"
							  xmlns:scripts="urn:local-scripts">
	
	<xsl:include href="Common/CommonElements.xsl"/>
	<xsl:include href="Common/CommonScripts.xsl"/>
	<xsl:include href="Common/CustomTemplates.xsl"/>
	<xsl:include href="Common/CommonTableFormatter.xsl"/>
	
	<xsl:output method="xml"/>
	
	<xsl:param name="RootUrl"/>
	<xsl:param name="ProtocolIDString"/>
	<xsl:param name="Sections"/>
	<xsl:param name="OrgIDs"/>
			
	<xsl:template match="/">
		<Protocol>
			<xsl:apply-templates/>
		</Protocol>
	</xsl:template>
	
	<xsl:template match="*">
		<!-- suppress defaults -->		
	</xsl:template>
	
	<xsl:template match="Protocol">
		<p>
			<xsl:apply-templates select="ProtocolTitle"/>
		
			<xsl:if test="contains($Sections,'Objectives')">
				<xsl:apply-templates select="ProtocolAbstract/Professional/Objectives"/>
			</xsl:if>
			
			<xsl:if test="contains($Sections,'EntryCriteria')">
				<xsl:apply-templates select="ProtocolAbstract/Professional/EntryCriteria"/>
			</xsl:if>
			
			<xsl:if test="contains($Sections,'ProjectedAccrual')">
				<xsl:apply-templates select="ProtocolAbstract/Professional/ProjectedAccrual"/>
			</xsl:if>
			
			<xsl:if test="contains($Sections,'StudyOutline')">
				<xsl:apply-templates select="ProtocolAbstract/Professional/Outline"/>
			</xsl:if>
			
			<xsl:if test="contains($Sections,'PublishedResults')">
				<xsl:if test="PublishedResults">
					<H2>PUBLISHED RESULTS</H2>					
					<xsl:apply-templates select="PublishedResults"/>		
				</xsl:if>
			</xsl:if>
			
			<xsl:if test="contains($Sections,'LeadOrgs')">
				<H2>STUDY LEAD ORGANIZATIONS</H2>
				<xsl:apply-templates select="ProtocolAdminInfo/ProtocolLeadOrg"/>				
			</xsl:if>
						
			<xsl:if test="contains($Sections,'StudySites')">
				<xsl:if test="ProtocolAdminInfo/ProtocolSites">
					<xsl:apply-templates select="ProtocolAdminInfo/ProtocolSites"/>
				</xsl:if>
			</xsl:if>
			
			<xsl:if test="contains($Sections,'Terms')">
				<h2>TERMS</h2>
				<xsl:if test="//SpecificDiagnosis | //StudyCondition">
					<b>Eligible Diagnoses/Conditions Studied</b>
					<p>
						<table cellpadding="3" cellspacing="0" border="1" width="100%">
							<xsl:if test="//SpecificDiagnosis">
								<tr>
									<td nowrap="true"><b>Diagnosis</b></td>
									<td nowrap="true"><b>Parent Diagnosis</b></td>
								</tr>
								<xsl:for-each select="//Diagnosis">
									<tr>
										<td valign="top" nowrap="true"><xsl:apply-templates select="SpecificDiagnosis"/>&#160;&#160;&#160;&#160;</td>
										<td valign="top" width="100%">
											<xsl:call-template name="CommaDelimit">
												<xsl:with-param name="NodeSet" select="DiagnosisParent"/>
											</xsl:call-template>&#160;
										</td>
									</tr>
								</xsl:for-each>
							</xsl:if>
							<xsl:if test="//SpecificCondition">
								<tr>
									<td nowrap="true"><b>Condition</b></td>
									<td nowrap="true"><b>Parent Condition</b></td>
								</tr>
								<xsl:for-each select="//StudyCondition">
									<tr>
										<td valign="top" nowrap="true"><xsl:apply-templates select="SpecificCondition"/>&#160;&#160;&#160;&#160;</td>
										<td valign="top" width="100%">
											<xsl:call-template name="CommaDelimit">
												<xsl:with-param name="NodeSet" select="ConditionParent"/>
											</xsl:call-template>&#160;
										</td>
									</tr>
								</xsl:for-each>
							</xsl:if>
						</table>			
					</p>
				</xsl:if>		
				
				<xsl:if test="//Gene">
					<b>Genes</b>
					<p>
					<xsl:call-template name="CommaDelimit">
						<xsl:with-param name="NodeSet" select="//SpecificGene"/>
					</xsl:call-template> 					
					</p>
				</xsl:if>		
				
				<xsl:if test="//Intervention">
					<b>Interventions</b>
					<p>
					<table cellpadding="2" border="1" cellspacing="0" width="100%">
						<tr>
							<td nowrap="true"><b>Intervention Type</b>&#160;&#160;&#160;&#160;</td>
							<td nowrap="true"><b>Intervention Name</b></td>
						</tr>
					<xsl:for-each select="//Intervention">
						<tr>
							<td nowrap="true"><xsl:value-of select="InterventionType"/>&#160;&#160;&#160;&#160;</td>
							<td width="100%">
								<xsl:call-template name="CommaDelimit">
									<xsl:with-param name="NodeSet" select="InterventionNameLink"/>
								</xsl:call-template>&#160;
							</td>
						</tr>
					</xsl:for-each>
					</table>
					</p>
				</xsl:if>
			</xsl:if>
			
			<xsl:if test="contains($Sections,'Disclaimer')">
				<xsl:apply-templates select="ProtocolAbstract/Professional/ProfessionalDisclaimer"/>
			</xsl:if>				
		</p>		
	</xsl:template>
	
	<xsl:template match="ProtocolTitle">
		<xsl:if test="@Audience = 'Professional'">
			<xsl:if test="../DateLastModified!=''">
				<H2>Date Last Modified:  <xsl:value-of select="../DateLastModified"/></H2>
			</xsl:if>

			<xsl:if test="../DateFirstPublished!=''">
				<H2>Date First Published:  <xsl:value-of select="../DateFirstPublished"/></H2>
			</xsl:if>
				
			<xsl:if test="../ProtocolSpecialCategory!=''">
				<H2>Special Category: <xsl:value-of select="../ProtocolSpecialCategory"/></H2>
			</xsl:if>
		</xsl:if>
	</xsl:template>
	
	<xsl:template match="SpecificDiagnosis">
		<xsl:value-of select="."/>
	</xsl:template>
	
	<xsl:template match="DiagnosisParent">
		<xsl:value-of select="."/>
	</xsl:template>
	
	<xsl:template match="SpecificCondition">
		<xsl:value-of select="."/>
	</xsl:template>
	
	<xsl:template match="ConditionParent">
		<xsl:value-of select="."/>
	</xsl:template>
	
	<xsl:template match="SpecificGene">
		<xsl:value-of select="."/>
	</xsl:template>
	
	<xsl:template match="InterventionNameLink">
		<xsl:value-of select="."/>
	</xsl:template>
	
	<!-- ************************* Professional Abstract **************************** -->
		
	<xsl:template match="Objectives">	
		<xsl:element name="a">
			<xsl:attribute name="name">Objectives_<xsl:value-of select="../../../@id"/></xsl:attribute>
			<H2>OBJECTIVES</H2>
		</xsl:element>
		<xsl:apply-imports/>
	</xsl:template>
	
	<xsl:template match="EntryCriteria">				
		<xsl:element name="a">
			<xsl:attribute name="name">EntryCriteria_<xsl:value-of select="../../../@id"/></xsl:attribute>
			<H2>ENTRY CRITERIA</H2>
		</xsl:element>
		<xsl:apply-templates select="DiseaseCharacteristics"/>
		<xsl:apply-templates select="PriorConcurrentTherapy"/>
		<xsl:apply-templates select="PatientCharacteristics"/>
		<xsl:apply-templates select="GeneralEligibilityCriteria"/>
	</xsl:template>
	
	<xsl:template match="ProjectedAccrual">		
		<xsl:element name="a">
			<xsl:attribute name="name">ProjectedAccrual_<xsl:value-of select="../../../@id"/></xsl:attribute>
			<H2>PROJECTED ACCRUAL</H2>
		</xsl:element>
		<xsl:apply-templates/>
	</xsl:template>

	<xsl:template match="Outline">		
		<xsl:element name="a">
			<xsl:attribute name="name">Outline_<xsl:value-of select="../../../@id"/></xsl:attribute>
			<H2>OUTLINE</H2>
		</xsl:element>
		<xsl:apply-templates/>
	</xsl:template>

	<xsl:template match="ProfessionalDisclaimer">	
		<xsl:if test="../ProfessionalDisclaimer!=''">
			<H2>DISCLAIMER</H2>
		</xsl:if>	
		<xsl:apply-templates/>
	</xsl:template>
	
	<xsl:template match="DiseaseCharacteristics">
		<P><B>Disease Characteristics:</B></P>
		<P>
			<xsl:apply-templates/>
		</P>
	</xsl:template>
	
	<xsl:template match="PatientCharacteristics">
		<P><B>Patient Characteristics:</B></P>
		<P>
			<xsl:apply-templates/>
		</P>
	</xsl:template>
	
	<xsl:template match="PriorConcurrentTherapy">
		<P><B>Prior/Concurrent Therapy:</B></P>
		<P>
			<xsl:apply-templates/>
		</P>
	</xsl:template>
	
	<xsl:template match="GeneralEligibilityCriteria">
		<P><B>General Eligibility Criteria:</B></P>
		<P>
			<xsl:apply-templates/>
		</P>
	</xsl:template>
	
	<!-- ************************ End Professional Abstract ************************* -->
	
	<xsl:template match="ProtocolRef">
		<xsl:element name="A">
			<xsl:attribute name="href">/templates/view_clinicaltrials.aspx?version=healthprofessional
			&amp;cdrid=<xsl:value-of select="number(substring-after(@href,'CDR'))"/>
			</xsl:attribute>
			<xsl:value-of select="."/>
		</xsl:element>
		
		<xsl:if test="following-sibling::node()">
			<xsl:for-each select="following-sibling::node()">
				<xsl:if test="name()='GlossaryTermRef' and position()=1">&#160;</xsl:if>
				<xsl:if test="name()='ProtocolRef' and position()=1">&#160;</xsl:if>
				<xsl:if test="name()='SummaryRef' and position()=1">&#160;</xsl:if>
			</xsl:for-each>
		</xsl:if>
	</xsl:template>
	
	<!-- ****************************** Study Detail ******************************** -->

	<xsl:template match="ProtocolDetail">
		<xsl:apply-templates/>
	</xsl:template>	

	<xsl:template match="StudyCategory">
		<P>
		<table border="1" cellspacing="0" cellpadding="3" width="100%">
			<tr>
				<xsl:choose>
					<xsl:when test="position()=2">
						<td><b>Primary Study Type</b></td>
					</xsl:when>
					<xsl:when test="position()=3">
						<td><b>Secondary Study Type</b></td>
					</xsl:when>
					<xsl:when test="position()=4">
						<td><b>Ternary Study Type</b></td>
					</xsl:when>
					<xsl:when test="position()=5">
						<td><b>Tertiary Study Type</b></td>
					</xsl:when>
					<xsl:otherwise>
						<td><b>Study Type</b></td>		
					</xsl:otherwise>
				</xsl:choose>
				<td><b>Study Focus</b></td>
				<td><b>Intervention Type (Modality)</b></td>
				<td><b>Intervention Name (Drug)</b></td>
			</tr>
			<tr>
				<td valign="top"><xsl:value-of select="./StudyCategoryName"/>&#160;</td>
				<td valign="top">
				<!-- To be supplied later -->
				&#160;</td>
				<td valign="top">
					<xsl:call-template name="CommaDelimit">
						<xsl:with-param name="NodeSet" select="./Intervention/InterventionType | ./Intervention/InterventionParentType"/>
					</xsl:call-template>
					&#160;
				</td>
				<td valign="top">
					<xsl:call-template name="CommaDelimit">
						<xsl:with-param name="NodeSet"  select="./Intervention/InterventionNameLink"/>
					</xsl:call-template>
					&#160;
				</td>
			</tr>
		</table>
		</P>
	</xsl:template>
	
	<xsl:template match="InterventionType">
		<xsl:value-of select="."/>	
	</xsl:template>

	<xsl:template match="InterventionParentType">
		<xsl:value-of select="."/>	
	</xsl:template>

	<xsl:template match="InterventionNameLink">
		<xsl:value-of select="."/>	
	</xsl:template>

	<!-- **************************** End Study Details ***************************** -->

	
	<!-- **************************** Published Results ***************************** -->
	
	<xsl:template match="PublishedResults">
		<P>
		<xsl:apply-templates/>
		<xsl:element name="A">
			<xsl:choose>
				<xsl:when test="@PMID !=''">
					<xsl:attribute name="href">http://www.ncbi.nlm.nih.gov/entrez/query.fcgi?cmd=Retrieve&amp;db=PubMed&amp;list_uids=<xsl:value-of select="@PMID"/>&amp;dopt=Abstract</xsl:attribute><span class="PUBMED">[PUBMED Abstract]</span>
				</xsl:when>
				<xsl:when test="@ProtocolID !=''">
					<xsl:attribute name="href">http://www.cancer.gov/templates/view_clinicaltrials.aspx?protocolid=<xsl:value-of select="@ProtocolID"/></xsl:attribute><span class="PUBMED">[PDQ Clinical Trial]</span>
				</xsl:when>		
			</xsl:choose>
		</xsl:element>
		</P>
	</xsl:template>
	
	<!-- ************************** End Published Results *************************** -->
		
	
	<!-- ********************** Participating Organizations ************************* -->
	<xsl:template match="ProtocolLeadOrg">
		<!--xsl:if test="contains($OrgIDs, ./LeadOrgName/@ref) or $OrgIDs=''"-->
			<P>
				<b><xsl:value-of select="./LeadOrgName"/></b>
				<br/>
				<table>
					<xsl:for-each select="./LeadOrgPersonnel/ProtPerson">
						<tr>
						<td width="50%">
							<xsl:value-of select="concat(./PersonNameInformation/GivenName, ' ', ./PersonNameInformation/SurName)"/>
							<xsl:if test="./PersonNameInformation/ProfessionalSuffix!=''">
								<xsl:value-of select="concat(', ', ./PersonNameInformation/ProfessionalSuffix)"/>
							</xsl:if>
							<xsl:if test="./PersonRole	!=''">
								<xsl:value-of select="concat(', ', ./PersonRole)"/>
							</xsl:if><xsl:if test="@status !=''">
								(<xsl:value-of select="@status"/>)
							</xsl:if>
						</td>
						<td>&#160;&#160;&#160;&#160;</td>
						<td>
							<xsl:if test="./Contact/ContactDetail/Phone!='' or ./Contact/ContactDetail/TollFreePhone !=''">Ph: </xsl:if>
					
							<xsl:if  test="./Contact/ContactDetail/Phone!=''">
								<xsl:value-of select="./Contact/ContactDetail/Phone"/>
							</xsl:if>
							<xsl:if test="./Contact/ContactDetail/TollFreePhone">
								<xsl:if test="./Contact/ContactDetail/Phone!=''">;&#160;</xsl:if>
								<xsl:value-of select="./Contact/ContactDetail/TollFreePhone"/>
							</xsl:if>
						</td>
					</tr>
					</xsl:for-each>	
				</table>
			</P>
		<!--/xsl:if-->		
	</xsl:template>
		
	<!-- ********************** End Participating Organizations ********************* -->
	
	
	<!-- ************************* Study Contacts *********************************** -->
		
	<xsl:template match="ProtocolSites">
		<xsl:variable name="ProtocolSites" select=".//PostalAddress"/>
		
		<xsl:variable name="ProtSite">	<!--Decide whether the sites has ProtocolSiteID searched-->
			<xsl:for-each select="ProtocolSite">
				<xsl:if test="contains($OrgIDs, @ref) or $OrgIDs=''">
					<xsl:value-of select="1"/> 
				</xsl:if>
			</xsl:for-each>
		</xsl:variable>	
				
		<xsl:if test="$ProtSite !=''">
			<H2>STUDY SITES AND CONTACTS</H2>
		</xsl:if>
				
		<table border="0" cellspacing="0" cellpadding="0">
		<tr>
			<td width="30"></td>
			<td></td>
			<td></td>
			<td></td>
		</tr>
		
		<!--Beginning of USA study sites and contacts-->
		
			<!--Beginning of USA study sites and contacts-->
		
		<xsl:for-each select="$ProtocolSites">
			<xsl:sort select="CountryName"/>
			<xsl:sort select="PoliticalSubUnitName"/>
			<xsl:sort select="City"/>
			<xsl:sort select="../../../../SiteName"/>	
			
			
			<xsl:variable name="CurrentPosition">
				<xsl:value-of select="position()"/> 
			</xsl:variable>
			
			<xsl:variable name="PrevOrg">	<!--Decide whether the sites before this site has OrgID-->
				<xsl:for-each select="$ProtocolSites">
					<xsl:sort select="CountryName"/>
					<xsl:sort select="PoliticalSubUnitName"/>
					<xsl:sort select="City"/>
					<xsl:sort select="../../../../SiteName"/>
					<xsl:if test="position() &lt; $CurrentPosition and contains($OrgIDs, ancestor::ProtocolSite/@ref) ">
						<xsl:value-of select="1"/> 
					</xsl:if>
				</xsl:for-each>
			</xsl:variable>	
			
			<xsl:if test="contains($OrgIDs, ancestor::ProtocolSite/@ref) or $OrgIDs = ''">
			
			<xsl:choose>
				<xsl:when test="CountryName='U.S.A.'">
					<xsl:choose>
						<xsl:when test="position()!=1"> 
							<xsl:choose>
								<xsl:when test="$OrgIDs !=''">
									<xsl:choose>
										<xsl:when test="$PrevOrg !=''">
											<xsl:if test="$CurrentCountry!=CountryName">
												<tr><td colspan="4"> <BR/><B><font color="red"><xsl:value-of select="CountryName"/></font></B></td></tr>								
											</xsl:if>
										</xsl:when>
										<xsl:otherwise>
											<tr><td colspan="4"> <BR/><B><font color="red"><xsl:value-of select="CountryName"/></font></B></td></tr>								
										</xsl:otherwise>
									</xsl:choose>
								</xsl:when>
								<xsl:otherwise>
									<xsl:if test="$CurrentCountry!=CountryName">
										<tr><td colspan="4"> <BR/><B><font color="red"><xsl:value-of select="CountryName"/></font></B></td></tr>								
									</xsl:if>
								</xsl:otherwise>
							</xsl:choose>
						</xsl:when>
						<xsl:otherwise>
							<tr><td colspan="4"><BR/><B><font color="red"><xsl:value-of select="CountryName"/></font></B></td></tr>
						</xsl:otherwise>
					</xsl:choose>
					
					<xsl:choose>
						<xsl:when test="position()!=1">
							<xsl:choose>
								<xsl:when test="$OrgIDs !=''">
									<xsl:choose>
										<xsl:when test="$PrevOrg !=''">
											<xsl:if test="$CurrentState!=PoliticalSubUnitName">
												<tr><td colspan="4"><BR/><B><xsl:value-of select="PoliticalSubUnitName"/></B></td></tr>
											</xsl:if>
										</xsl:when>
										<xsl:otherwise>
											<tr><td colspan="4"><BR/><B><xsl:value-of select="PoliticalSubUnitName"/></B></td></tr>
										</xsl:otherwise>					
									</xsl:choose>
								</xsl:when>
								<xsl:otherwise>
									<xsl:if test="$CurrentState!=PoliticalSubUnitName">
										<tr><td colspan="4"><BR/><B><xsl:value-of select="PoliticalSubUnitName"/></B></td></tr>
									</xsl:if>
								</xsl:otherwise>
							</xsl:choose>
						</xsl:when>
						<xsl:otherwise>
							<tr><td colspan="4"><BR/><B><xsl:value-of select="PoliticalSubUnitName"/></B></td></tr>
						</xsl:otherwise>
					</xsl:choose>	
					
					<xsl:choose>
						<xsl:when test="position()!=1">
							<xsl:choose>
								<xsl:when test="$OrgIDs !=''">
									<xsl:choose>
										<xsl:when test="$PrevOrg !=''">
											<xsl:if test="$CurrentCity!=City">
												<tr><td width="90">&#160;</td><td colspan="3"><BR/><B><xsl:value-of select="City"/></B></td></tr>
											</xsl:if>
										</xsl:when>
										<xsl:otherwise>	 
											<tr><td width="90">&#160;</td><td colspan="3"><BR/><B><xsl:value-of select="City"/></B></td></tr>
										</xsl:otherwise>
									</xsl:choose>
								</xsl:when>
								<xsl:otherwise>	 
									<xsl:if test="$CurrentCity!=City">
										<tr><td width="90">&#160;</td><td colspan="3"><BR/><B><xsl:value-of select="City"/></B></td></tr>
									</xsl:if>
								</xsl:otherwise>
							</xsl:choose>
						</xsl:when>
						<xsl:otherwise>
							<tr><td width="90">&#160;</td><td colspan="3"><BR/><B><xsl:value-of select="City"/></B></td></tr>
						</xsl:otherwise>					
					</xsl:choose>	
					
					<xsl:choose>
						<xsl:when test="position() !=1">
							<xsl:choose>
								<xsl:when test="$OrgIDs !=''">
									<xsl:choose>
										<xsl:when test="$PrevOrg !=''">
											<xsl:if test="$CurrentSiteName != ../../../../SiteName">
												<tr><td width="90">&#160;</td><td colspan="3"><B><i><xsl:value-of select="../../../../SiteName"/></i></B></td></tr>
											</xsl:if>
										</xsl:when>
										<xsl:otherwise>	 
											<tr><td width="90">&#160;</td><td colspan="3"><B><i><xsl:value-of select="../../../../SiteName"/></i></B></td></tr>									
										</xsl:otherwise>
									</xsl:choose>
								</xsl:when>
								<xsl:otherwise>
									<xsl:if test="$CurrentSiteName != ../../../../SiteName">
										<tr><td width="90">&#160;</td><td colspan="3"><B><i><xsl:value-of select="../../../../SiteName"/></i></B></td></tr>
									</xsl:if>
								</xsl:otherwise>
							</xsl:choose>
						</xsl:when>
						<xsl:otherwise>
							<tr><td width="90">&#160;</td><td colspan="3"><B><i><xsl:value-of select="../../../../SiteName"/></i></B></td></tr> 
						</xsl:otherwise>
					</xsl:choose>
			
						
					<xsl:choose>
						<xsl:when test="concat(../../../PersonNameInformation/GivenName, ../../../PersonNameInformation/SurName) =''">
							<tr><td width="90">&#160;</td>
								<td valign="top"><xsl:value-of select="../../../PersonRole"/></td>
								<td>&#160;&#160;&#160;&#160;</td>
								<td  valign="top">
									<table align="left" valign="top" border="0" cellspacing="0" cellpadding="0">
										<tr valign="top" >
											<td>Ph:&#160;</td>
											<td>
												<xsl:choose>
													<xsl:when test="../Phone[node()] = true()"><xsl:value-of select="../Phone"/></xsl:when>
													<xsl:otherwise><xsl:if test="../TollFreePhone[node()] = true()"><xsl:value-of select="../TollFreePhone"/></xsl:if></xsl:otherwise>
												</xsl:choose>
											</td>
										</tr>
										<tr valign="top" >
											<td></td>
											<td>
												<xsl:if test="../Phone[node()] = true()">
													<xsl:if test="../TollFreePhone[node()] = true()"><xsl:value-of select="../TollFreePhone"/></xsl:if>
												</xsl:if>
											</td>
										</tr>
									</table>
								</td>
							</tr>
						</xsl:when>
						<xsl:otherwise>
							<xsl:choose>
								<xsl:when test=" ../../../PersonNameInformation/ProfessionalSuffix !=''">
									<tr><td width="90">&#160;</td>
										<td valign="top"><xsl:value-of select="concat(../../../PersonNameInformation/GivenName, ' ', ../../../PersonNameInformation/SurName, ', ',../../../PersonNameInformation/ProfessionalSuffix)"/>
											<xsl:if test="../../../@status !=''">
												(<xsl:value-of select="../../../@status"/>)
											</xsl:if>
										</td>
										<td>&#160;&#160;&#160;&#160;</td>
										<td  valign="top">
											<table align="left" valign="top" border="0" cellspacing="0" cellpadding="0">
												<tr valign="top" >
													<td>Ph:&#160;</td>
													<td>
														<xsl:choose>
															<xsl:when test="../Phone[node()] = true()"><xsl:value-of select="../Phone"/></xsl:when>
															<xsl:otherwise><xsl:if test="../TollFreePhone[node()] = true()"><xsl:value-of select="../TollFreePhone"/></xsl:if></xsl:otherwise>
														</xsl:choose>
													</td>
												</tr>
												<tr valign="top" >
													<td></td>
													<td>
														<xsl:if test="../Phone[node()] = true()">
															<xsl:if test="../TollFreePhone[node()] = true()"><xsl:value-of select="../TollFreePhone"/></xsl:if>
														</xsl:if>
													</td>
												</tr>
											</table>
										</td>
									</tr>
								</xsl:when>
								<xsl:otherwise>
									<tr><td width="90">&#160;</td>
										<td valign="top"><xsl:value-of select="concat(../../../PersonNameInformation/GivenName, ' ', ../../../PersonNameInformation/SurName)"/>
											<xsl:if test="../../../@status !=''">
												(<xsl:value-of select="../../../@status"/>)
											</xsl:if>
										</td>
										<td>&#160;&#160;&#160;&#160;</td>
										<td  valign="top">
											<table align="left" valign="top" border="0" cellspacing="0" cellpadding="0">
												<tr valign="top" >
													<td>Ph:&#160;</td>
													<td>
														<xsl:choose>
															<xsl:when test="../Phone[node()] = true()"><xsl:value-of select="../Phone"/></xsl:when>
															<xsl:otherwise><xsl:if test="../TollFreePhone[node()] = true()"><xsl:value-of select="../TollFreePhone"/></xsl:if></xsl:otherwise>
														</xsl:choose>
													</td>
												</tr>
												<tr valign="top" >
													<td></td>
													<td>
														<xsl:if test="../Phone[node()] = true()">
															<xsl:if test="../TollFreePhone[node()] = true()"><xsl:value-of select="../TollFreePhone"/></xsl:if>
														</xsl:if>
													</td>
												</tr>
											</table>
										</td>
									</tr>
								</xsl:otherwise>
							</xsl:choose>
						</xsl:otherwise>
					</xsl:choose>
				
					<xsl:if test="../Email!=''">
						<tr>
						<td>&#160;</td>
						<td>&#160;</td>
						<td>&#160;</td>
						<td align="left">
							<xsl:element name="a">
								<xsl:attribute name="href">mailto:<xsl:value-of select="../Email"/></xsl:attribute>
								<xsl:value-of select="../Email"/>
							</xsl:element>
						</td>
						</tr>
						<tr>
						<td colspan="4">&#160;</td>
						</tr>
					</xsl:if>
					<xsl:if test="../Email[node()] != true()">
						<tr>
						<td colspan="4">&#160;</td>
						</tr>
					</xsl:if>
				</xsl:when>
			</xsl:choose>
			
			<!-- CountryName is required for Contact -->
			<xsl:variable name="CurrentCountry" select="CountryName"/>	
			<!-- PoliticalSubUnitName is not required for Contact. We need to distinguish null -->
			<xsl:variable name="CurrentState">
				<xsl:if test="PoliticalSubUnitName[node()] = true()">
					<xsl:value-of select="PoliticalSubUnitName"/>	
				</xsl:if>
			</xsl:variable>
			<!-- City is not required for Contact. We need to distinguish null -->			
			<xsl:variable name="CurrentCity">
				<xsl:if test="City[node()] = true()">
					<xsl:value-of select="City"/>	
				</xsl:if>
			</xsl:variable>
			<!-- site name is not required for Contact. We need to distinguish null -->	
			<xsl:variable name="CurrentSiteName">
				<xsl:if test="../../../../SiteName[node()] = true()">
					<xsl:value-of select="../../../../SiteName"/>
				</xsl:if>
			</xsl:variable> 
			
			</xsl:if>
			
		</xsl:for-each>
		
		<!-- END of USA Study sites and contacts -->
			
		<!--Beginning of Non-USA study sites and contacts-->
						
		<xsl:for-each select="$ProtocolSites">
			<xsl:sort select="CountryName"/>
			<xsl:sort select="PoliticalSubUnitName"/>
			<xsl:sort select="City"/>
			<xsl:sort select="../../../../SiteName"/>	
			
			<xsl:variable name="CurrentPosition">
				<xsl:value-of select="position()"/> 
			</xsl:variable>
			
			<xsl:variable name="PrevOrg">	<!--Decide whether the sites before this site has OrgID-->
				<xsl:for-each select="$ProtocolSites">
					<xsl:sort select="CountryName"/>
					<xsl:sort select="PoliticalSubUnitName"/>
					<xsl:sort select="City"/>
					<xsl:sort select="../../../../SiteName"/>
					<xsl:if test="position() &lt; $CurrentPosition and contains($OrgIDs, ancestor::ProtocolSite/@ref) ">
						<xsl:value-of select="1"/> 
					</xsl:if>
				</xsl:for-each>
			</xsl:variable>	
			
			<xsl:if test="contains($OrgIDs, ancestor::ProtocolSite/@ref) or $OrgIDs = ''">
								
			<xsl:choose>
				<xsl:when test="CountryName !='U.S.A.'">
					<xsl:choose>
						<xsl:when test="position()!=1"> 
							<xsl:choose>
								<xsl:when test="$OrgIDs !=''">
									<xsl:choose>
										<xsl:when test="$PrevOrg !=''">
											<xsl:if test="$CurrentCountryNon!=CountryName">
												<tr><td colspan="4"> <BR/><B><font color="red"><xsl:value-of select="CountryName"/></font></B></td></tr>								
											</xsl:if>
										</xsl:when>
										<xsl:otherwise>
											<tr><td colspan="4"> <BR/><B><font color="red"><xsl:value-of select="CountryName"/></font></B></td></tr>								
										</xsl:otherwise>
									</xsl:choose>
								</xsl:when>
								<xsl:otherwise>
									<xsl:if test="$CurrentCountryNon!=CountryName">
										<tr><td colspan="4"> <BR/><B><font color="red"><xsl:value-of select="CountryName"/></font></B></td></tr>								
									</xsl:if>
								</xsl:otherwise>
							</xsl:choose>
						</xsl:when>
						<xsl:otherwise>
							<tr><td colspan="4"><BR/><B><font color="red"><xsl:value-of select="CountryName"/></font></B></td></tr>
						</xsl:otherwise>
					</xsl:choose>
							
					<xsl:choose>
						<xsl:when test="position()!=1">
							<xsl:choose>
								<xsl:when test="$OrgIDs !=''">
									<xsl:choose>
										<xsl:when test="$PrevOrg !=''">
											<xsl:if test="$CurrentStateNon!=PoliticalSubUnitName">
												<tr><td colspan="4"><BR/><B><xsl:value-of select="PoliticalSubUnitName"/></B></td></tr>
											</xsl:if>
										</xsl:when>
										<xsl:otherwise>
											<tr><td colspan="4"><BR/><B><xsl:value-of select="PoliticalSubUnitName"/></B></td></tr>
										</xsl:otherwise>					
									</xsl:choose>
								</xsl:when>
								<xsl:otherwise>
									<xsl:if test="$CurrentStateNon!=PoliticalSubUnitName">
										<tr><td colspan="4"><BR/><B><xsl:value-of select="PoliticalSubUnitName"/></B></td></tr>
									</xsl:if>
								</xsl:otherwise>
							</xsl:choose>
						</xsl:when>
						<xsl:otherwise>
							<tr><td colspan="4"><BR/><B><xsl:value-of select="PoliticalSubUnitName"/></B></td></tr>
						</xsl:otherwise>
					</xsl:choose>	
					
					<xsl:choose>
						<xsl:when test="position()!=1">
							<xsl:choose>
								<xsl:when test="$OrgIDs !=''">
									<xsl:choose>
										<xsl:when test="$PrevOrg !=''">
											<xsl:if test="$CurrentCityNon!=City">
												<tr><td width="90">&#160;</td><td colspan="3"><BR/><B><xsl:value-of select="City"/></B></td></tr>
											</xsl:if>
										</xsl:when>
										<xsl:otherwise>	 
											<tr><td width="90">&#160;</td><td colspan="3"><BR/><B><xsl:value-of select="City"/></B></td></tr>
										</xsl:otherwise>
									</xsl:choose>
								</xsl:when>
								<xsl:otherwise>	 
									<xsl:if test="$CurrentCityNon!=City">
										<tr><td width="90">&#160;</td><td colspan="3"><BR/><B><xsl:value-of select="City"/></B></td></tr>
									</xsl:if>
								</xsl:otherwise>
							</xsl:choose>
						</xsl:when>
						<xsl:otherwise>
							<tr><td width="90">&#160;</td><td colspan="3"><BR/><B><xsl:value-of select="City"/></B></td></tr>
						</xsl:otherwise>					
					</xsl:choose>	
					
					<xsl:choose>
						<xsl:when test="position() !=1">
							<xsl:choose>
								<xsl:when test="$OrgIDs !=''">
									<xsl:choose>
										<xsl:when test="$PrevOrg !=''">
											<xsl:if test="$CurrentSiteNameNon != ../../../../SiteName">
												<tr><td width="90">&#160;</td><td colspan="3"><B><i><xsl:value-of select="../../../../SiteName"/></i></B></td></tr>
											</xsl:if>
										</xsl:when>
										<xsl:otherwise>	 
											<tr><td width="90">&#160;</td><td colspan="3"><B><i><xsl:value-of select="../../../../SiteName"/></i></B></td></tr>									
										</xsl:otherwise>
									</xsl:choose>
								</xsl:when>
								<xsl:otherwise>
									<xsl:if test="$CurrentSiteNameNon != ../../../../SiteName">
										<tr><td width="90">&#160;</td><td colspan="3"><B><i><xsl:value-of select="../../../../SiteName"/></i></B></td></tr>
									</xsl:if>
								</xsl:otherwise>
							</xsl:choose>
						</xsl:when>
						<xsl:otherwise>
							<tr><td width="90">&#160;</td><td colspan="3"><B><i><xsl:value-of select="../../../../SiteName"/></i></B></td></tr> 
						</xsl:otherwise>
					</xsl:choose>
					
					<xsl:choose>
						<xsl:when test="concat(../../../PersonNameInformation/GivenName, ../../../PersonNameInformation/SurName) =''">
							<tr><td width="90">&#160;</td>
								<td valign="top"><xsl:value-of select="../../../PersonRole"/></td>
								<td>&#160;&#160;&#160;&#160;</td>
								<td  valign="top">
									<table align="left" valign="top" border="0" cellspacing="0" cellpadding="0">
										<tr valign="top" >
											<td>Ph:&#160;</td>
											<td>
												<xsl:choose>
													<xsl:when test="../Phone[node()] = true()"><xsl:value-of select="../Phone"/></xsl:when>
													<xsl:otherwise><xsl:if test="../TollFreePhone[node()] = true()"><xsl:value-of select="../TollFreePhone"/></xsl:if></xsl:otherwise>
												</xsl:choose>
											</td>
										</tr>
										<tr valign="top" >
											<td></td>
											<td>
												<xsl:if test="../Phone[node()] = true()">
													<xsl:if test="../TollFreePhone[node()] = true()"><xsl:value-of select="../TollFreePhone"/></xsl:if>
												</xsl:if>
											</td>
										</tr>
									</table>
								</td>
							</tr>
						</xsl:when>
						<xsl:otherwise>
							<xsl:choose>
								<xsl:when test=" ../../../PersonNameInformation/ProfessionalSuffix !=''">
									<tr><td width="90">&#160;</td>
										<td valign="top"><xsl:value-of select="concat(../../../PersonNameInformation/GivenName, ' ', ../../../PersonNameInformation/SurName, ', ',../../../PersonNameInformation/ProfessionalSuffix)"/>
											<xsl:if test="../../../@status !=''">
												(<xsl:value-of select="../../../@status"/>)
											</xsl:if>
										</td>
										<td>&#160;&#160;&#160;&#160;</td>
										<td  valign="top">
											<table align="left" valign="top" border="0" cellspacing="0" cellpadding="0">
												<tr valign="top" >
													<td>Ph:&#160;</td>
													<td>
														<xsl:choose>
															<xsl:when test="../Phone[node()] = true()"><xsl:value-of select="../Phone"/></xsl:when>
															<xsl:otherwise><xsl:if test="../TollFreePhone[node()] = true()"><xsl:value-of select="../TollFreePhone"/></xsl:if></xsl:otherwise>
														</xsl:choose>
													</td>
												</tr>
												<tr valign="top" >
													<td></td>
													<td>
														<xsl:if test="../Phone[node()] = true()">
															<xsl:if test="../TollFreePhone[node()] = true()"><xsl:value-of select="../TollFreePhone"/></xsl:if>
														</xsl:if>
													</td>
												</tr>
											</table>
										</td>
									</tr>									
								</xsl:when>
								<xsl:otherwise>
									<tr><td width="90">&#160;</td>
										<td valign="top"><xsl:value-of select="concat(../../../PersonNameInformation/GivenName, ' ', ../../../PersonNameInformation/SurName)"/>
											<xsl:if test="../../../@status !=''">
												(<xsl:value-of select="../../../@status"/>)
											</xsl:if>
										</td>
										<td>&#160;&#160;&#160;&#160;</td>
										<td  valign="top">
											<table align="left" valign="top" border="0" cellspacing="0" cellpadding="0">
												<tr valign="top" >
													<td>Ph:&#160;</td>
													<td>
														<xsl:choose>
															<xsl:when test="../Phone[node()] = true()"><xsl:value-of select="../Phone"/></xsl:when>
															<xsl:otherwise><xsl:if test="../TollFreePhone[node()] = true()"><xsl:value-of select="../TollFreePhone"/></xsl:if></xsl:otherwise>
														</xsl:choose>
													</td>
												</tr>
												<tr valign="top" >
													<td></td>
													<td>
														<xsl:if test="../Phone[node()] = true()">
															<xsl:if test="../TollFreePhone[node()] = true()"><xsl:value-of select="../TollFreePhone"/></xsl:if>
														</xsl:if>
													</td>
												</tr>
											</table>
										</td>
									</tr>
								</xsl:otherwise>
							</xsl:choose>
						</xsl:otherwise>
					</xsl:choose>
			
		
					<xsl:if test="../Email!=''">
						<tr>
						<td>&#160;</td>
						<td>&#160;</td>
						<td>&#160;</td>
						<td align="left">
							<xsl:element name="a">
								<xsl:attribute name="href">mailto:<xsl:value-of select="../Email"/></xsl:attribute>
								<xsl:value-of select="../Email"/>
							</xsl:element>
						</td>
						</tr>
						<tr>
						<td colspan="4">&#160;</td>
						</tr>
					</xsl:if>
					<xsl:if test="../Email[node()] != true()">
						<tr>
						<td colspan="4">&#160;</td>
						</tr>
					</xsl:if>
				</xsl:when>
			</xsl:choose>
			
						<!-- CountryName is required for Contact -->
			<xsl:variable name="CurrentCountryNon" select="CountryName"/>	
			<!-- PoliticalSubUnitName is not required for Contact. We need to distinguish null -->
			<xsl:variable name="CurrentStateNon">
				<xsl:if test="PoliticalSubUnitName[node()] = true()">
					<xsl:value-of select="PoliticalSubUnitName"/>	
				</xsl:if>
			</xsl:variable>
			<!-- City is not required for Contact. We need to distinguish null -->			
			<xsl:variable name="CurrentCityNon">
				<xsl:if test="City[node()] = true()">
					<xsl:value-of select="City"/>	
				</xsl:if>
			</xsl:variable>
			<!-- site name is not required for Contact. We need to distinguish null -->	
			<xsl:variable name="CurrentSiteNameNon">
				<xsl:if test="../../../../SiteName[node()] = true()">
					<xsl:value-of select="../../../../SiteName"/>
				</xsl:if>
			</xsl:variable> 
			
			<!--xsl:variable name="CurrentCountryNon" select="CountryName"/-->	
			<!--xsl:variable name="CurrentStateNon" select="PoliticalSubUnitName"/-->	
			<!--xsl:variable name="CurrentCityNon" select="City"/-->	
			<!--xsl:variable name="CurrentSiteNameNon" select="../../../../SiteName"/-->
			
			</xsl:if>
		</xsl:for-each>
		
		</table>
	</xsl:template>
	<!-- ************************* End Study Contacts ******************************* -->

</xsl:stylesheet>
