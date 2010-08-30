<?xml version="1.0" encoding="UTF-8" ?>
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
	
	<!-- **************************** Control Section ******************************* -->
	<xsl:template match="Protocol">
		<p>
			<xsl:apply-templates select="ProtocolTitle"/>
		
			<xsl:if test="contains($Sections, 'Abstract')">
				<xsl:apply-templates select="ProtocolAbstract"/>
			</xsl:if>
			
			<xsl:if test="contains($Sections, 'Disclaimer')">
				<xsl:apply-templates select="ProtocolAbstract/Patient/PatientDisclaimer"/>
			</xsl:if>			
						
			<xsl:if test="contains($Sections,'LeadOrgs')">
				<H2>STUDY LEAD ORGANIZATIONS</H2>
				<xsl:apply-templates select="ProtocolAdminInfo/ProtocolLeadOrg"/>				
			</xsl:if>
			
			<xsl:if test="contains($Sections, 'ParticipatingSites')">
				<xsl:apply-templates select="ProtocolAdminInfo/ProtocolSites"/>
			</xsl:if>						
		</p>		
	</xsl:template>
	
	<!-- ************************** End Control Section ***************************** -->
	
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
	
	<!-- ******************************** Abstract ********************************** -->
	
	<xsl:template match="ProtocolAbstract">
		<xsl:apply-templates/>
	</xsl:template>
	
	<xsl:template match="Patient">
		<xsl:if test=" Rationale !='Patient abstract not available'">		
			<H2>PATIENT ABSTRACT</H2>
			<xsl:apply-templates select="Rationale"/>
			<xsl:apply-templates select="Purpose"/>
			<xsl:apply-templates select="EligibilityText"/>
			<xsl:apply-templates select="TreatmentIntervention"/>
		</xsl:if>
	</xsl:template>
	
	<xsl:template match="Rationale">
		<P>
			<B>Rationale: </B><xsl:apply-templates/>
		</P>
	</xsl:template>	
	
	<xsl:template match="Purpose">
		<P>
			<B>Purpose: </B><xsl:apply-templates/>
		</P>
	</xsl:template>	

	<xsl:template match="EligibilityText">
		<P>
			<B>Eligibility: </B><xsl:apply-templates/>
		</P>
	</xsl:template>	

	<xsl:template match="TreatmentIntervention">
		<P>
			<B>Treatment Intervention: </B><xsl:apply-templates/>
		</P>
	</xsl:template>	

	<!-- ****************************** End Abstract ******************************** -->


	<!-- ************************** Patient Disclaimer ****************************** -->

	<xsl:template match="PatientDisclaimer">
			<P><B>Disclaimer: </B><xsl:apply-templates/></P>		
	</xsl:template>

	<!-- **************************** End Disclaimer ******************************** -->

		
	<!-- ********************** Participating Organizations ************************* -->
	
	<xsl:template match="ProtocolAdminInfo">
		<H2>STUDY LEAD ORGANIZATIONS</H2>
		<xsl:apply-templates/>
	</xsl:template>
	
	<xsl:template match="ProtocolLeadOrg">
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
	</xsl:template>
	
	<!-- ********************** End Participating Organizations ********************* -->

		
	<xsl:template match="ProtocolRef">
		<xsl:element name="A">
			<xsl:attribute name="href">/templates/view_clinicaltrials.aspx?version=patient
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
			
	<xsl:template match="GlossaryTermRef">
		<a>
			<xsl:attribute name="href">/dictionary/db_alpha.aspx?cdrid=<xsl:value-of select="number(substring-after(@href,'CDR'))"/>&version=Patient&Language=English</xsl:attribute>
			<xsl:attribute name="onclick">javascript:popWindow('defbyid','<xsl:value-of select="@href"/>'); return(false);</xsl:attribute>
			<xsl:value-of select="."/>
		</a>
		<xsl:if test="following-sibling::node()">
			<xsl:for-each select="following-sibling::node()">
				<xsl:if test="name()='GlossaryTermRef' and position()=1">&#160;</xsl:if>
				<xsl:if test="name()='ProtocolRef' and position()=1">&#160;</xsl:if>
				<xsl:if test="name()='SummaryRef' and position()=1">&#160;</xsl:if>
			</xsl:for-each>
		</xsl:if>
	</xsl:template>
	
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
			
			<!--xsl:variable name="CurrentCountry" select="CountryName"/>	
			<xsl:variable name="CurrentState" select="PoliticalSubUnitName"/>	
			<xsl:variable name="CurrentCity" select="City"/>	
			<xsl:variable name="CurrentSiteName" select="../../../../SiteName"/-->
			
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
			
			<!--xsl:variable name="CurrentCountryNon" select="CountryName"/>	
			<xsl:variable name="CurrentStateNon" select="PoliticalSubUnitName"/>	
			<xsl:variable name="CurrentCityNon" select="City"/>	
			<xsl:variable name="CurrentSiteNameNon" select="../../../../SiteName"/-->
			
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
			
			</xsl:if>
		</xsl:for-each>
		
		</table>
	</xsl:template>
	<!-- ************************* End Study Contacts ******************************* -->

</xsl:stylesheet>


  