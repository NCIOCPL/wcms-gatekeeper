<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" xmlns:scripts="urn:local-scripts">
	<xsl:include href="Common/CommonElements.xsl"/>
	<xsl:include href="Common/CommonScripts.xsl"/>
	<xsl:include href="Common/CustomTemplates.xsl"/>
	<xsl:include href="Common/CommonTableFormatter.xsl"/>
	<xsl:output method="xml"/>
	<xsl:param name="RootUrl"/>
	<xsl:param name="ProtocolIDString"/>

	<xsl:variable name="alterName">
		<xsl:choose>
			<xsl:when test="Protocol/PDQProtocolTitle[@Audience ='Patient'] = 'No Patient Title'">false</xsl:when>
			<xsl:when test="Protocol/PDQProtocolTitle[@Audience='Patient'] = '' or PDQProtocolTitle[@Audience='Patient'] = null">false</xsl:when>
			<xsl:otherwise>true</xsl:otherwise>
		</xsl:choose>
	</xsl:variable>
	
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
		<xsl:variable name="ProtocolTitleHP">
			<xsl:for-each select="PDQProtocolTitle">
				<xsl:if test="@Audience = 'Professional'">
					<xsl:apply-templates/>
				</xsl:if>
			</xsl:for-each>
		</xsl:variable>
		<!--P>
			<Table border="0" cellpadding="0" cellspacing="0">
				<tr>
					<td>
						<xsl:if test="DateLastModified[node()] = true()">
							Date Last Modified:  <xsl:value-of select="DateLastModified"/>
						</xsl:if>
					</td>
					<td>
						<img src="/images/spacer.gif" width="40" height="21" alt="" border="0"/>
					</td>
					<td>
						<xsl:if test="DateFirstPublished[node()] = true()">
							Date First Published:  <xsl:value-of select="DateFirstPublished"/>
						</xsl:if>
					</td>
				</tr>
			</Table>
		</P-->
		<p>
			<xsl:for-each select="PDQProtocolTitle">
				<xsl:if test="@Audience = 'Patient'">
					<xsl:choose>
						<xsl:when test="text() !='No Patient Title'">
							<Span Class="Protocol-Title">
								<xsl:apply-templates/>
							</Span>
						</xsl:when>
						<xsl:otherwise>
							<xsl:if test="/">
								<Span Class="Protocol-Title">
									<xsl:value-of select="$ProtocolTitleHP"/>
								</Span>
							</xsl:if>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:if>
			</xsl:for-each>
		</p>
		<p>
			<xsl:if test="$alterName = 'true'">
				<xsl:element name="a">
					<xsl:attribute name="href">
						#AlternateTitle_<xsl:value-of select="@id"/>
					</xsl:attribute>
					<Span Class="Protocol-TOC-Link">Alternate Title</Span>
				</xsl:element>
				<br/>
			</xsl:if>
			<xsl:element name="a">
				<xsl:attribute name="href">
					#TrialIdInfo_<xsl:value-of select="@id"/>
				</xsl:attribute>
				<Span Class="Protocol-TOC-Link">Basic Trial Information</Span>
			</xsl:element>
			<br/>
			
			<xsl:if test="ProtocolAbstract/Patient/Rationale !='Patient abstract not available'">
				<xsl:element name="a">
					<xsl:attribute name="href">#TrialDescription_<xsl:value-of select="@id"/></xsl:attribute>
					<Span Class="Protocol-TOC-Link">Trial Description</Span>
				</xsl:element>
				<br/>
				
					&#160;&#160;&#160;&#160;<xsl:element name="a">
					<xsl:attribute name="href">#Purpose_<xsl:value-of select="@id"/></xsl:attribute>
					<Span Class="Protocol-TOC-Link">Purpose</Span>
				</xsl:element>
				<br/>
				
					&#160;&#160;&#160;&#160;<xsl:element name="a">
					<xsl:attribute name="href">#Eligibility_<xsl:value-of select="@id"/></xsl:attribute>
					<Span Class="Protocol-TOC-Link">Eligibility</Span>
				</xsl:element>
				<br/>
				
					&#160;&#160;&#160;&#160;<xsl:element name="a">
					<xsl:attribute name="href">#TreatmentIntervention_<xsl:value-of select="@id"/></xsl:attribute>
					<Span Class="Protocol-TOC-Link">Treatment/Intervention</Span>
				</xsl:element>
				<br/>
			</xsl:if>
			<xsl:element name="a">
				<xsl:attribute name="href">#TrialContact_<xsl:value-of select="@id"/></xsl:attribute>
				<Span Class="Protocol-TOC-Link">Trial Contact Information</Span>
			</xsl:element>
			<br/>
			<xsl:if test="ProtocolRelatedLinks[node()] = true()">
				<xsl:element name="a">
					<xsl:attribute name="href">#ProtocolRelatedLinks_<xsl:value-of select="@id"/></xsl:attribute>
					<Span Class="Protocol-TOC-Link">Related Information</Span>
				</xsl:element>
				<br/>
			</xsl:if>
			<xsl:if test="RegistryInfo[node()] = true()">
				<xsl:element name="a">
					<xsl:attribute name="href">#RegistryInfo_<xsl:value-of select="@id"/></xsl:attribute>
					<Span Class="Protocol-TOC-Link">Registry Information</Span>
				</xsl:element>
				<br/>
			</xsl:if>
			<!--xsl:element name="a">
				<xsl:attribute name="href">#TreatmentIntervention_<xsl:value-of select="@id"/></xsl:attribute>
				<Span Class="Protocol-TOC-Link">Treatment Intervention</Span>	
			</xsl:element>
			<br/-->
			<!--xsl:element name="a">
				<xsl:attribute name="href">#LeadOrgs_<xsl:value-of select="@id"/></xsl:attribute>
				<Span Class="Protocol-TOC-Link">Trial Lead Organizations</Span>	
			</xsl:element>
			<br/-->
			<!-- From DTD, this is an optional element -->
			<!--xsl:if test="ProtocolAdminInfo/ProtocolSites[node()] = true()">
				<xsl:element name="a">
					<xsl:attribute name="href">#SitesAndContacts_<xsl:value-of select="@id"/></xsl:attribute>
					<Span Class="Protocol-TOC-Link">Trial Sites and Contacts</Span>
				</xsl:element>			
				<br/>
			</xsl:if-->
			<xsl:apply-templates select="PDQProtocolTitle"/>
			<xsl:apply-templates select="ProtocolAbstract"/>
			<!--xsl:apply-templates select="ProtocolAbstract/Patient/PatientDisclaimer"/-->
			<xsl:apply-templates select="ProtocolAdminInfo"/>
			<xsl:if test="ProtocolRelatedLinks[node()] = true()">
				<xsl:element name="a">
					<xsl:attribute name="name">ProtocolRelatedLinks</xsl:attribute>
					<xsl:apply-templates select="ProtocolRelatedLinks"/>
				</xsl:element>
			</xsl:if>
			<xsl:apply-templates select="RegistryInfo"/>
		</p>
		<!--p>
			<xsl:copy-of select="$ReturnToTopBar"/>
		</p-->
	</xsl:template>
	<!-- ************************** End Control Section ***************************** -->
	<!-- ********************************* Title ************************************ -->
	<xsl:template match="PDQProtocolTitle">
		<xsl:if test="@Audience = 'Professional'">
			<xsl:if test="$alterName = 'true'">
				<xsl:element name="a"><xsl:attribute name="name">AlternateTitle_<xsl:value-of select="../@id"/></xsl:attribute></xsl:element>
				
					<h2>Alternate Title</h2>
				
				<p>
					<xsl:apply-templates/>
				</p>
			</xsl:if>
			<!--xsl:if test="../DateLastModified!=''">
				<P>
					<Span Class="Protocol-Section-Heading">Date Last Modified:  <xsl:value-of select="../DateLastModified"/></Span>
				</P>
			</xsl:if>

			<xsl:if test="../DateFirstPublished!=''">
				<P>
					<Span Class="Protocol-Section-Heading">Date First Published:  <xsl:value-of select="../DateFirstPublished"/></Span>
				</P>
			</xsl:if-->
			<xsl:element name="a">
				<xsl:attribute name="name">TrialIdInfo_<xsl:value-of select="../@id"/></xsl:attribute>
			</xsl:element>
			
				<h2>Basic Trial Information</h2>
			
			<p>
			<table width="100%" cellspacing="0" cellpadding="1" border="0" class="Protocol-BasicStudy-Grayborder">
				<tr>
					<td valign="top">
						<table width="100%" cellspacing="0" cellpadding="0" border="0" bgcolor="#FFFFFF">
							<tr bgcolor="#E6E6E2">
								<td  width="4" valign="top">
									<img src="/images/spacer.gif" width="4" height="21" alt="" border="0"/>
								</td>
								<td class="black-text" width="53" valign="middle">
									<Span Class="Protocol-BasicStudy-Heading">Phase</Span>
									<Br/>
									<img src="/images/spacer.gif" width="56" height="1" alt="" border="0"/>
								</td>
								<td  width="4" valign="top">
									<img src="/images/spacer.gif" width="4" height="1" alt="" border="0"/>
								</td>
								<td valign="top" width="1" class="Protocol-BasicStudy-TD-Grayborder">
									<img src="/images/spacer.gif" width="1" height="1" alt="" border="0"/>
								</td>
								<td  width="4" valign="middle">
									<img src="/images/spacer.gif" width="4" height="1" alt="" border="0"/>
								</td>
								<td class="black-text" width="95" valign="middle" align="left">
									<Span Class="Protocol-BasicStudy-Heading">Type</Span>
									<br/>
									<img src="/images/spacer.gif" width="47" height="1" alt="" border="0"/>
								</td>
								<td width="4" valign="middle">
									<img src="/images/spacer.gif" width="4" height="1" alt="" border="0"/>
								</td>
								<td width="1" valign="top" class="Protocol-BasicStudy-TD-Grayborder">
									<img src="/images/spacer.gif" width="1" height="1" alt="" border="0"/>
								</td>
								<td  width="4" valign="middle">
									<img src="/images/spacer.gif" width="4" height="1" alt="" border="0"/>
								</td>
								<td class="black-text" width="65" valign="middle" align="left">
									<Span Class="Protocol-BasicStudy-Heading">Status</Span>
									<br/>
									<img src="/images/spacer.gif" width="37" height="1" alt="" border="0"/>
								</td>
								<td width="4" valign="middle">
									<img src="/images/spacer.gif" width="4" height="1" alt="" border="0"/>
								</td>
								<td width="1" valign="top" class="Protocol-BasicStudy-TD-Grayborder">
									<img src="/images/spacer.gif" width="1" height="1" alt="" border="0"/>
								</td>
								<td width="4" valign="middle">
									<img src="/images/spacer.gif" width="4" height="1" alt="" border="0"/>
								</td>
								<td class="black-text" width="85" valign="middle" align="left">
									<Span Class="Protocol-BasicStudy-Heading">Age</Span>
									<Br/>
									<img src="/images/spacer.gif" width="70" height="1" alt="" border="0"/>
								</td>
								<td width="4" valign="middle">
									<img src="/images/spacer.gif" width="4" height="1" alt="" border="0"/>
								</td>
								<xsl:choose>
										<xsl:when test="count(/Protocol/ProtocolSponsor) &gt; 1">
											<td width="1" valign="top" class="Protocol-BasicStudy-TD-Grayborder">
												<img src="/images/spacer.gif" width="1" height="1" alt="" border="0"/>
											</td>
											<td width="4" valign="middle">
												<img src="/images/spacer.gif" width="4" height="1" alt="" border="0"/>
											</td>
											<td class="black-text" width="85" valign="middle" align="left">
												<Span Class="Protocol-BasicStudy-Heading">Sponsors</Span>
												<Br/>
												<img src="/images/spacer.gif" width="49" height="1" alt="" border="0"/>
											</td>
											<td  width="4" valign="middle">
												<img src="/images/spacer.gif" width="4" height="1" alt="" border="0"/>
											</td>
										</xsl:when>
										<xsl:otherwise>
											<td width="1" valign="top" class="Protocol-BasicStudy-TD-Grayborder">
												<img src="/images/spacer.gif" width="1" height="1" alt="" border="0"/>
											</td>
											<td width="4" valign="middle">
												<img src="/images/spacer.gif" width="4" height="1" alt="" border="0"/>
											</td>
											<td class="black-text" width="85" valign="middle" align="left">
												<Span Class="Protocol-BasicStudy-Heading">Sponsor</Span>
												<Br/>
												<img src="/images/spacer.gif" width="49" height="1" alt="" border="0"/>
											</td>
											<td width="4" valign="middle">
												<img src="/images/spacer.gif" width="4" height="1" alt="" border="0"/>
											</td>
										</xsl:otherwise>
									</xsl:choose>

								<td width="1" valign="top" class="Protocol-BasicStudy-TD-Grayborder">
									<img src="/images/spacer.gif" width="1" height="1" alt="" border="0"/>
								</td>
								<td width="4" valign="middle">
									<img src="/images/spacer.gif" width="4" height="1" alt="" border="0"/>
								</td>
								<td class="black-text" width="141" valign="middle" align="left">
									<Span Class="Protocol-BasicStudy-Heading">Protocol IDs</Span>
									<Br/>
									<img src="/images/spacer.gif" width="70" height="1" alt="" border="0"/>
								</td>
								<td width="4" valign="middle">
									<img src="/images/spacer.gif" width="4" height="1" alt="" border="0"/>
								</td>
							</tr>
							<tr>
								<td valign="top" class="Protocol-BasicStudy-TD-Grayborder" height="1" colspan="23"></td>
							</tr>
							<tr bgcolor="#FFFFFF">
								<td valign="top">&#160;</td>
								<td valign="top">
									<img src="/images/spacer.gif" width="1" height="10" alt="" border="0"/>
									<br/>
									<xsl:for-each select="/Protocol/ProtocolPhase">
										<xsl:if test="position()=1">
											<xsl:value-of select="."/>
										</xsl:if>
										<xsl:if test="position() !=1">
											<xsl:value-of select="', '"/>
											<xsl:value-of select="."/>
										</xsl:if>
									</xsl:for-each>
									<br/>
									<img src="/images/spacer.gif" width="1" height="10" alt="" border="0"/>
									<br/>
								</td>
								<td valign="top">&#160;</td>
								<td valign="top" class="Protocol-BasicStudy-TD-Grayborder">
									<img src="/images/spacer.gif" width="1" height="1" alt="" border="0"/>
								</td>
								<td valign="middle">&#160;</td>
								<td valign="top">
									<img src="/images/spacer.gif" width="1" height="10" alt="" border="0"/>
									<br/>
									<xsl:for-each select="/Protocol/ProtocolDetail/StudyCategory">
										<xsl:if test="position()=1">
											<xsl:value-of select="./StudyCategoryName"/>
										</xsl:if>
										<xsl:if test="position() !=1">
											<xsl:value-of select="', '"/>
											<xsl:value-of select="./StudyCategoryName"/>
										</xsl:if>
									</xsl:for-each>
									<br/>
									<img src="/images/spacer.gif" width="1" height="10" alt="" border="0"/>
									<br/>
								</td>
								<td valign="top">&#160;</td>
								<td valign="top" class="Protocol-BasicStudy-TD-Grayborder">
									<img src="/images/spacer.gif" width="1" height="1" alt="" border="0"/>
								</td>
								<td valign="middle">&#160;</td>
								<td valign="top">
									<img src="/images/spacer.gif" width="1" height="10" alt="" border="0"/>
									<br/>
									<xsl:value-of select="/Protocol/ProtocolAdminInfo/CurrentProtocolStatus"/>
									<br/>
									<img src="/images/spacer.gif" width="1" height="10" alt="" border="0"/>
									<br/>
								</td>
								<td valign="top">&#160;</td>
								<td valign="top" class="Protocol-BasicStudy-TD-Grayborder">
									<img src="/images/spacer.gif" width="1" height="1" alt="" border="0"/>
								</td>
								<td valign="middle">&#160;</td>
								<td valign="top">
									<img src="/images/spacer.gif" width="1" height="10" alt="" border="0"/>
									<br/>
									<xsl:value-of select="/Protocol/Eligibility/AgeText"/>
									<br/>
									<img src="/images/spacer.gif" width="1" height="10" alt="" border="0"/>
									<br/>
								</td>
								<td valign="top">&#160;</td>
								<td valign="top" class="Protocol-BasicStudy-TD-Grayborder">
										<img src="/images/spacer.gif" width="1" height="1" alt="" border="0"/>
									</td>
									<td valign="middle">&#160;</td>
									<td valign="top">
										<img src="/images/spacer.gif" width="1" height="10" alt="" border="0"/>
										<br/>
										<xsl:for-each select="/Protocol/ProtocolSponsor">
											<xsl:if test="position()=1">
												<xsl:value-of select="."/>
											</xsl:if>
											<xsl:if test="position() !=1">
												<xsl:value-of select="', '"/>
												<xsl:value-of select="."/>
											</xsl:if>
										</xsl:for-each>
										<br/>
										<img src="/images/spacer.gif" width="1" height="10" alt="" border="0"/>
										<br/>
									</td>
									<td valign="top">&#160;</td>

								<td valign="top" class="Protocol-BasicStudy-TD-Grayborder">
									<img src="/images/spacer.gif" width="1" height="1" alt="" border="0"/>
								</td>
								<td valign="middle">&#160;</td>
								<td valign="top">
									<img src="/images/spacer.gif" width="1" height="10" alt="" border="0"/>
									<br/>
									<xsl:for-each select="/Protocol/ProtocolIDs/*">
										<xsl:if test="position()=1">
											<xsl:choose>
												<xsl:when test="name() = 'PrimaryID'">
													<Span class="Protocol-BasicStudy-PrimaryID">
														<xsl:value-of select="./IDString"/>
													</Span>
												</xsl:when>
												<xsl:otherwise>
													<Span class="Protocol-BasicStudy-AlternateID">
														<xsl:value-of select="./IDString"/>
													</Span>
												</xsl:otherwise>
											</xsl:choose>
											<xsl:apply-templates/>
										</xsl:if>
										<xsl:choose>
											<xsl:when test="position()=1">
												<br/>
											</xsl:when>
											<xsl:when test="position()=2">
												<xsl:value-of select="./IDString"/>
											</xsl:when>
											<xsl:when test="position() &gt; 2">
												<xsl:value-of select="', '"/>
												<xsl:value-of select="./IDString"/>
											</xsl:when>
										</xsl:choose>
									</xsl:for-each>
									<br/>
									<img src="/images/spacer.gif" width="1" height="10" alt="" border="0"/>
								</td>
								<td valign="top">&#160;</td>
							</tr>
						</table>
					</td>
				</tr>
			</table>
			</p>		
				<xsl:if test="../ProtocolSpecialCategory!=''">
				<xsl:element name="a">
					<xsl:attribute name="name">SpecialCategory:<xsl:value-of select="../@id"/></xsl:attribute>
					<a name="SpecialCategory_{../@id}"/>
					<P>
            <h2 do-not-show="toc">
							Special Category:

							<xsl:for-each select="../ProtocolSpecialCategory">
								<xsl:if test="position()=1">
									<xsl:value-of select="."/>
								</xsl:if>
								<xsl:if test="position() !=1">
									<xsl:value-of select="', '"/>
									<xsl:value-of select="."/>
								</xsl:if>
							</xsl:for-each>
						</h2>
					</P>
				</xsl:element>
			</xsl:if>
		</xsl:if>
	</xsl:template>
	<!-- ******************************** End Title ********************************* -->
	<!-- ******************************** Abstract ********************************** -->
	<xsl:template match="ProtocolAbstract">
		<xsl:element name="a">
			<xsl:attribute name="name">TrialDescription:<xsl:value-of select="../@id"/></xsl:attribute>
		<a name="TrialDescription_{../@id}"/>
		<xsl:apply-templates/>
		</xsl:element>
	</xsl:template>
	
	<xsl:template match="Patient">
		<xsl:if test=" Rationale !='Patient abstract not available'">
		
				<h2>Trial Description</h2>
		
			<xsl:apply-templates/>
		</xsl:if>
	</xsl:template>
	
	<xsl:template match="Rationale">
		<P>
			<xsl:element name="a">
				<xsl:attribute name="name">Purpose_<xsl:value-of select="../../../@id"/></xsl:attribute>
			</xsl:element>
			<Span Class="Protocol-Purpose">Purpose: </Span>
			<xsl:apply-templates/>
		</P>
	</xsl:template>
	
	<xsl:template match="Purpose">
		<p>
			<xsl:apply-templates/>
		</p>
	</xsl:template>
	
	<xsl:template match="EligibilityText">
		<P>
			<xsl:element name="a">
				<xsl:attribute name="name">Eligibility_<xsl:value-of select="../../../@id"/></xsl:attribute>
			</xsl:element>
			<Span Class="Protocol-EligibilityText">Eligibility: </Span>
			<xsl:apply-templates/>
		</P>
	</xsl:template>
	
	<xsl:template match="TreatmentIntervention">
		<P>
			<xsl:element name="a">
				<xsl:attribute name="name">TreatmentIntervention_<xsl:value-of select="../../../@id"/></xsl:attribute>
			</xsl:element>
			<Span Class="Protocol-TreatmentIntervention">Treatment/Intervention: </Span>
			<xsl:apply-templates/>
		</P>
	</xsl:template>
	
	<xsl:template match="PatientDisclaimer">
		<xsl:element name="a">
			<xsl:attribute name="name">Disclaimer</xsl:attribute>
			<P>
				<!--Span Class="Protocol-PatientDisclaimer">Disclaimer: </Span-->
				<xsl:apply-templates/>
			</P>
		</xsl:element>
	</xsl:template>
	<!-- ****************************** End Abstract ******************************** -->
	<!-- ********************** Participating Organizations ************************* -->
	<xsl:template match="ProtocolAdminInfo">
		<xsl:element name="a">
			<xsl:attribute name="name">TrialContact_<xsl:value-of select="../@id"/></xsl:attribute>
		</xsl:element>
		
			<h2>Trial Contact Information</h2>
		
		<!--xsl:attribute name="name">LeadOrgs_<xsl:value-of select="../@id"/></xsl:attribute-->

    <h3 do-not-show="toc">Trial Lead Organizations</h3>
		
		<xsl:apply-templates/>
	</xsl:template>
	<xsl:template match="ProtocolLeadOrg">
		
      <!-- hide from table of contents on the front end-->
      <h4 do-not-show="toc">
				<xsl:value-of select="./LeadOrgName"/>
			</h4>
		
		
				
				<xsl:for-each select="./LeadOrgPersonnel/ProtPerson">
					<div>
						
							<xsl:choose>
								<xsl:when test="./PersonNameInformation/SurName !=''">
									
										<xsl:value-of select="concat(./PersonNameInformation/GivenName, ' ', ./PersonNameInformation/SurName)"/>
                 
									<xsl:if test="./PersonNameInformation/ProfessionalSuffix!=''">
										
											<xsl:value-of select="concat(', ', ./PersonNameInformation/ProfessionalSuffix)"/>
									
									</xsl:if>
									<xsl:if test="./PersonRole	!=''">
										
											<xsl:value-of select="concat(', ', ./PersonRole)"/>
										
									</xsl:if>
								</xsl:when>
								<xsl:otherwise>
									<xsl:if test="./PersonRole	!=''">
										
											<xsl:value-of select="./PersonRole"/>
										
									</xsl:if>
								</xsl:otherwise>
							</xsl:choose>
							<xsl:if test="@status !=''">
								(<xsl:value-of select="@status"/>)
							</xsl:if>
            <br/>
            
											<xsl:if test="./Contact/ContactDetail/Phone!='' or ./Contact/ContactDetail/TollFreePhone !=''">Ph: </xsl:if>
											<xsl:if test="./Contact/ContactDetail/Phone!=''">
												<xsl:value-of select="./Contact/ContactDetail/Phone"/>
											</xsl:if>
											<xsl:if test="./Contact/ContactDetail/TollFreePhone">
												<xsl:if test="./Contact/ContactDetail/Phone!=''">;&#160;</xsl:if>
												<xsl:value-of select="./Contact/ContactDetail/TollFreePhone"/>
											</xsl:if>
										
            <br/>
            <xsl:if test="./Contact/ContactDetail/Email!=''">
									
											Email:
												<xsl:element name="a">
													<xsl:attribute name="href">mailto:<xsl:value-of select="./Contact/ContactDetail/Email"/></xsl:attribute>
													<xsl:value-of select="./Contact/ContactDetail/Email"/>
												</xsl:element>
											
										
								</xsl:if>
							
					</div>
					
				</xsl:for-each>
			
	</xsl:template>
	<!-- ********************** End Participating Organizations ********************* -->
	<xsl:template match="ProtocolRef">
		<xsl:element name="A">
			<xsl:attribute name="Class">Protocol-ProtocolRef</xsl:attribute>
			<xsl:attribute name="href">/search/viewclinicaltrials.aspx?version=patient&amp;cdrid=<xsl:value-of select="number(substring-after(@href,'CDR'))"/></xsl:attribute>
			<xsl:value-of select="."/>
		</xsl:element>
		<xsl:if test="following-sibling::node()">
			<xsl:for-each select="following-sibling::node()">
				<xsl:if test="name() !='' and position()=1">&#160;</xsl:if>
			</xsl:for-each>
		</xsl:if>
	</xsl:template>
	<xsl:template match="GlossaryTermRef">
		<a>
			<xsl:attribute name="Class">Protocol-GlossaryTermRef</xsl:attribute>
			<xsl:attribute name="href">/Common/PopUps/popDefinition.aspx?id=<xsl:value-of select="number(substring-after(@href,'CDR'))"/>&amp;version=Patient&amp;language=English</xsl:attribute>
			<xsl:attribute name="onclick">javascript:popWindow('defbyid','<xsl:value-of select="@href"/>&amp;version=Patient&amp;language=English'); return(false);</xsl:attribute>
			<xsl:value-of select="."/>
		</a>
		<xsl:if test="following-sibling::node()">
			<xsl:for-each select="following-sibling::node()">
				<xsl:if test="name() !='' and position()=1">&#160;</xsl:if>
			</xsl:for-each>
		</xsl:if>
	</xsl:template>
	<!-- ************************* Study Contacts *********************************** -->
	<xsl:variable name="CurrentCountry" select="test"/>
	<xsl:variable name="CurrentState" select="test"/>
	<xsl:variable name="CurrentCity" select="test"/>
	<xsl:variable name="CurrentSiteName" select="test"/>
	<xsl:variable name="CurrentCountryNon" select="CountryName"/>
	<xsl:variable name="CurrentStateNon" select="PoliticalSubUnitName"/>
	<xsl:variable name="CurrentCityNon" select="City"/>

	<xsl:template match="ProtocolSites">
		<xsl:element name="a">
			<xsl:attribute name="name">SitesAndContacts</xsl:attribute>
			
			<h3>Trial Sites</h3>
			
		<xsl:variable name="ProtocolSites" select=".//PostalAddress"/>
      <!-- Create a list of locations, with a separate location record for each contact.
            The defaultContactKey value is used when no contacts exist. -->
      <locationData>
        <xsl:for-each select="$ProtocolSites">
          <xsl:sort select="CountryName"/>
          <xsl:sort select="PoliticalSubUnitName"/>
          <xsl:sort select="City"/>
          <xsl:sort select="../../../../SiteName"/>

          <!-- These don't vary between contacts at a single facility. -->
          <xsl:variable name="city" select="City" />
          <xsl:variable name="politicalSubUnit" select="PoliticalSubUnitName" />
          <xsl:variable name="countryName" select="CountryName" />
          <xsl:variable name="siteRef" select="../../../../@ref" />
          <xsl:variable name="facilityName" select="../../../../SiteName"/>

          <!-- If there are contacts, create location records. But see below for the case
                where there are no contacts. -->
          <xsl:for-each select="../../Contact">
            <location contactKey="{../../../@site}">
              <city>
                <xsl:value-of select="$city"/>
              </city>
              <politicalSubUnitName>
                <xsl:value-of select="$politicalSubUnit"/>
              </politicalSubUnitName>
              <country>
                <xsl:value-of select="$countryName"/>
              </country>
              <facilityName siteRef="{$siteRef}">
                <xsl:value-of select="$facilityName"/>
              </facilityName>
              <contactHTML>
                <!-- Put the blob of HTML in a CDATA section to prevent surprises -->
                <xsl:text disable-output-escaping="yes">&lt;![CDATA[</xsl:text>
                <!-- Contact person's name. -->

                <!-- This concatenation will lead to extra whitespaces when there's a node missing,
                      but this is an HTML snippet, so the extra spaces will collapse when the snippet
                      displays in the browsr. -->
                <xsl:value-of select="concat(../../../PersonNameInformation/GivenName, ' ', ../../../PersonNameInformation/MiddleInitial, ' ', ../../../PersonNameInformation/SurName)"/>
                <xsl:if test="../../../PersonNameInformation/ProfessionalSuffix">
                  , <xsl:value-of select="../../../PersonNameInformation/ProfessionalSuffix"/>
                </xsl:if>

                <!-- Include the Role -->
                <xsl:if test="(../../../PersonRole[node()] = true())">
                  <br/>
                  <xsl:value-of select="../../../PersonRole"/>
                </xsl:if>

                <!-- Phone number, only applicable for CTGovContact or CTGovContactBackup -->
                <xsl:if test="../Phone[node()] = true() or ../TollFreePhone[node()] = true()">
                  <br />Ph: <xsl:choose>
                    <xsl:when test="../Phone[node()] = true()">
                      <xsl:value-of select="../Phone"/>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:if test="../TollFreePhone[node()] = true()">
                        <xsl:value-of select="../TollFreePhone"/>
                      </xsl:if>
                    </xsl:otherwise>
                  </xsl:choose>
                  <xsl:if test="PhoneExt[node()] = true()">
                    Ext. <xsl:value-of select="PhoneExt"/>
                  </xsl:if>
                </xsl:if>

                <!-- Email address, only applicable for CTGovContact or CTGovContactBackup -->
                <xsl:if test="Email[node()] = true()">
                  <br />Email: <a>
                    <!-- Create a mailto: link with a default subject line containing the protocol ID, OrgStudID, and Title -->
                    <xsl:attribute name="href">
                      mailto:<xsl:value-of select="Email"/>?Subject=<xsl:value-of select="concat(/CTGovProtocol/IDInfo/NCTID,',',/CTGovProtocol/IDInfo/OrgStudyID,':- ',normalize-space(translate(translate(/CTGovProtocol/BriefTitle,'&#xA;',' '),'&#xD;',' ')))"/>
                    </xsl:attribute>
                    <xsl:value-of select="Email"/>
                  </a>
                </xsl:if>
                <xsl:text disable-output-escaping="yes">]]&gt;</xsl:text>
              </contactHTML>
            </location>
          </xsl:for-each>

          <!-- If there are no contacts, create a contactless location record. -->
          <xsl:if test="(../../Contact[node()] = false())">
            <location contactKey="{../../../@site}">
              <city>
                <xsl:value-of select="$city"/>
              </city>
              <politicalSubUnitName>
                <xsl:value-of select="$politicalSubUnit"/>
              </politicalSubUnitName>
              <country>
                <xsl:value-of select="$countryName"/>
              </country>
              <facilityName siteRef="{$siteRef}">
                <xsl:value-of select="$facilityName"/>
              </facilityName>
              <contactHTML></contactHTML>
            </location>
          </xsl:if>

        </xsl:for-each>
      </locationData>

      <P>
			<table width="100%" border="0" cellspacing="0" cellpadding="0">
				<tr space="1">
					<td valign="top" width="7%">
						<img src="/images/spacer.gif" width="100%" height="1" alt="" border="0"/>
					</td>
					<td valign="top" width="7%">
						<img src="/images/spacer.gif" width="100%" height="1" alt="" border="0"/>
					</td>
					<td valign="top" width="33%">
						<img src="/images/spacer.gif" width="100%" height="1" alt="" border="0"/>
					</td>
					<td valign="top" width="53%">
						<img src="/images/spacer.gif" width="100%" height="1" alt="" border="0"/>
					</td>
				</tr>
				<!--Beginning of USA study sites and contacts-->
				<!--Beginning of USA study sites and contacts-->
				<xsl:for-each select="$ProtocolSites">
					<xsl:sort select="CountryName"/>
					<xsl:sort select="PoliticalSubUnitName"/>
					<xsl:sort select="City"/>
					<xsl:sort select="../../../../SiteName"/>
					<!--xsl:variable name="CurrentCountry" select="CountryName"/>
			<xsl:variable name="CurrentState" select="PoliticalSubUnitName"/>
			<xsl:variable name="CurrentCity" select="City"/>
			<xsl:variable name="CurrentSiteName" select="../../../../SiteName"/-->
					<xsl:choose>
						<xsl:when test="CountryName='U.S.A.'">
							<xsl:choose>
								<xsl:when test="position()!=1">
									<xsl:choose>
										<xsl:when test="$CurrentCountry!=CountryName">
											<tr country="1">
												<td valign="top" colspan="4" Class="Protocol-country">
													<xsl:value-of select="CountryName"/>
												</td>
											</tr>
											<tr space="1">
												<td valign="top"  colspan="4" style="height:5px"></td>
											</tr>
										</xsl:when>
										<xsl:otherwise>
											<tr country="1">
												<td valign="top" colspan="4" Class="Protocol-country">
													<xsl:value-of select="CountryName"/>
												</td>
											</tr>
											<tr space="1">
												<td valign="top"  colspan="4" style="height:5px"></td>
											</tr>
										</xsl:otherwise>
									</xsl:choose>
								</xsl:when>
								<xsl:otherwise>
									<tr country="1">
										<td valign="top" colspan="4" Class="Protocol-country">
											<xsl:value-of select="CountryName"/>
										</td>
									</tr>
									<tr space="1">
										<td valign="top"  colspan="4" style="height:5px"></td>
									</tr>
								</xsl:otherwise>
							</xsl:choose>

							<xsl:choose>
								<xsl:when test="position()!=1">
									<xsl:choose>
										<xsl:when test="$CurrentState!=''">
											<xsl:if test="$CurrentState!=PoliticalSubUnitName">
												<tr state="1">
													<td valign="top" colspan="4" Class="Protocol-state">
														<xsl:value-of select="PoliticalSubUnitName"/>
													</td>
												</tr>
												<tr space="1">
													<td valign="top" colspan="4">
														<img src="/images/spacer.gif" width="1" height="5" alt="" border="0"/>
													</td>
												</tr>
											</xsl:if>
										</xsl:when>
										<xsl:otherwise>
											<tr state="1">
												<td valign="top" colspan="4" Class="Protocol-state">
													<xsl:value-of select="PoliticalSubUnitName"/>
												</td>
											</tr>
											<tr space="1">
												<td valign="top" colspan="4">
													<img src="/images/spacer.gif" width="1" height="5" alt="" border="0"/>
												</td>
											</tr>
										</xsl:otherwise>
									</xsl:choose>
								</xsl:when>
								<xsl:otherwise>
									<tr state="1">
										<td valign="top" colspan="4" Class="Protocol-state">
											<xsl:value-of select="PoliticalSubUnitName"/>
										</td>
									</tr>
									<tr space="1">
										<td valign="top" colspan="4">
											<img src="/images/spacer.gif" width="1" height="5" alt="" border="0"/>
										</td>
									</tr>
								</xsl:otherwise>
							</xsl:choose>
							<xsl:choose>
								<xsl:when test="position()!=1">
									<xsl:choose>
										<xsl:when test="$CurrentCity!=' '">
											<xsl:choose>
												<xsl:when test="$CurrentState !=' '">
													<xsl:if test="$CurrentState =PoliticalSubUnitName">
														<xsl:if test="$CurrentCity!=City">
															<tr city="1">
																<td valign="top">&#160;</td>
																<td valign="top" colspan="3" Class="Protocol-city">
																	<xsl:value-of select="City"/>
																</td>
															</tr>
															<tr space="1">
																<td valign="top" colspan="4">
																	<img src="/images/spacer.gif" width="1" height="20px" alt="" border="0"/>
																</td>
															</tr>
														</xsl:if>
														<!-- state exists, if non-existing state, display it -->
													</xsl:if>
													<xsl:if test="$CurrentState !=PoliticalSubUnitName">
														<tr city="1">
															<td valign="top">&#160;</td>
															<td valign="top" colspan="3" Class="Protocol-city">
																<xsl:value-of select="City"/>
															</td>
														</tr>
														<tr space="1">
															<td valign="top" colspan="4">
																<img src="/images/spacer.gif" width="1" height="20px" alt="" border="0"/>
															</td>
														</tr>
													</xsl:if>
												</xsl:when>
												<xsl:otherwise>
													<xsl:if test="$CurrentCity!=City">
														<tr city="1">
															<td valign="top">&#160;</td>
															<td valign="top" colspan="3" Class="Protocol-city">
																<xsl:value-of select="City"/>
															</td>
														</tr>
														<tr space="1">
															<td valign="top" colspan="4">
																<img src="/images/spacer.gif" width="1" height="20px" alt="" border="0"/>
															</td>
														</tr>
													</xsl:if>
												</xsl:otherwise>
											</xsl:choose>
											<!--Nothing for equal city-->
										</xsl:when>
										<xsl:otherwise>
											<tr city="1">
												<td valign="top">&#160;</td>
												<td valign="top" colspan="3" Class="Protocol-city">
													<xsl:value-of select="City"/>
												</td>
											</tr>
											<tr space="1">
												<td valign="top" colspan="4">
													<img src="/images/spacer.gif" width="1" height="20px" alt="" border="0"/>
												</td>
											</tr>
										</xsl:otherwise>
									</xsl:choose>
								</xsl:when>
								<xsl:otherwise>
									<tr city="1">
										<td valign="top">&#160;</td>
										<td valign="top" colspan="3" Class="Protocol-city">
											<xsl:value-of select="City"/>
										</td>
									</tr>
									<tr space="1">
										<td valign="top" colspan="4">
											<img src="/images/spacer.gif" width="1" height="20px" alt="" border="0"/>
										</td>
									</tr>
								</xsl:otherwise>
							</xsl:choose>
							
							<xsl:variable name="SiteRef">
								<xsl:if test="../../../../@ref!=''">
									<xsl:value-of select="../../../../@ref"/>
								</xsl:if>
							</xsl:variable>

							<xsl:variable name="PersonRef">
								<xsl:if test="../../../@ref!=''">
									<xsl:value-of select="../../../@ref"/>
								</xsl:if>
							</xsl:variable>
							
							<xsl:choose>
								<xsl:when test="position() !=1">
									<xsl:choose>
										<xsl:when test="string-length($CurrentSiteName) =0 and string-length(../../../../SiteName) !=0">
											<tr facility="1" ref="{$SiteRef}">
												<td valign="top">&#160;</td>
												<td valign="top" colspan="3">
													<Span Class="Protocol-Site-SiteName">
														<xsl:value-of select="../../../../SiteName"/>
													</Span>
												</td>
											</tr>
											<tr space="1">
												<td valign="top" colspan="4">
													<img src="/images/spacer.gif" width="1" height="5" alt="" border="0"/>
												</td>
											</tr>
										</xsl:when>
										<xsl:otherwise>
											<xsl:if test="$CurrentSiteName != ../../../../SiteName">
												<tr facility="1" ref="{$SiteRef}">
													<td valign="top">&#160;</td>
													<td valign="top" colspan="3">
														<Span Class="Protocol-Site-SiteName">
															<xsl:value-of select="../../../../SiteName"/>
														</Span>
													</td>
												</tr>
												<tr space="1">
													<td valign="top" colspan="4">
														<img src="/images/spacer.gif" width="1" height="5" alt="" border="0"/>
													</td>
												</tr>
											</xsl:if>
										</xsl:otherwise>
									</xsl:choose>
								</xsl:when>
								<xsl:otherwise>
									<tr facility="1" ref="{$SiteRef}">
										<td valign="top">&#160;</td>
										<td valign="top" colspan="3">
											<Span Class="Protocol-Site-SiteName">
												<xsl:value-of select="../../../../SiteName"/>
											</Span>
										</td>
									</tr>
									<tr space="1">
										<td valign="top" colspan="4">
											<img src="/images/spacer.gif" width="1" height="5" alt="" border="0"/>
										</td>
									</tr>
								</xsl:otherwise>
							</xsl:choose>
							<xsl:choose>
								<xsl:when test="concat(../../../PersonNameInformation/GivenName, ../../../PersonNameInformation/SurName) =''">
									<tr name="1" ref="{$PersonRef}">
										<td valign="top" colspan="2">&#160;</td>
										<td valign="top">
											<Span Class="Protocol-Site-PersonRole">
												<xsl:value-of select="../../../PersonRole"/>
											</Span>
										</td>
										<td valign="top">
											<table align="left" valign="top" border="0" cellspacing="0" cellpadding="0">
												<tr valign="top">
													<td>
														<xsl:if test="../Phone[node()] = true() or ../TollFreePhone[node()] = true()">
															<Span Class="Protocol-Site-Phone">Ph:&#160;</Span>
														</xsl:if>
													</td>
													<td>
														<xsl:choose>
															<xsl:when test="../Phone[node()] = true()">
																<Span Class="Protocol-Site-Phone">
																	<xsl:value-of select="../Phone"/>
																</Span>
															</xsl:when>
															<xsl:otherwise>
																<xsl:if test="../TollFreePhone[node()] = true()">
																	<Span Class="Protocol-Site-Phone">
																		<xsl:value-of select="../TollFreePhone"/>
																	</Span>
																</xsl:if>
															</xsl:otherwise>
														</xsl:choose>
													</td>
												</tr>
												<tr valign="top">
													<td/>
													<td>
														<xsl:if test="../Phone[node()] = true()">
															<xsl:if test="../TollFreePhone[node()] = true()">
																<Span Class="Protocol-Site-TollFreePhone">
																	<xsl:value-of select="../TollFreePhone"/>
																</Span>
															</xsl:if>
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
											<tr name="1" ref="{$PersonRef}">
												<td valign="top" colspan="2">&#160;</td>
												<td valign="top">
													<Span Class="Protocol-Site-PersonName">
														<xsl:value-of select="concat(../../../PersonNameInformation/GivenName, ' ', ../../../PersonNameInformation/SurName)"/>
													</Span>
													<Span Class="Protocol-Site-Suffix">
														<xsl:value-of select="concat(', ',../../../PersonNameInformation/ProfessionalSuffix)"/>
													</Span>
													<xsl:if test="../../../@status !=''">
														<Span Class="Protocol-Site-Status">(<xsl:value-of select="../../../@status"/>)</Span>
													</xsl:if>
												</td>
												<td valign="top">
													<table align="left" valign="top" border="0" cellspacing="0" cellpadding="0">
														<tr valign="top">
															<td>
																<xsl:if test="../Phone[node()] = true() or ../TollFreePhone[node()] = true()">
																	<Span Class="Protocol-Site-Phone">Ph:&#160;</Span>
																</xsl:if>
															</td>
															<td>
																<xsl:choose>
																	<xsl:when test="../Phone[node()] = true()">
																		<Span Class="Protocol-Site-Phone">
																			<xsl:value-of select="../Phone"/>
																		</Span>
																	</xsl:when>
																	<xsl:otherwise>
																		<xsl:if test="../TollFreePhone[node()] = true()">
																			<Span Class="Protocol-Site-Phone">
																				<xsl:value-of select="../TollFreePhone"/>
																			</Span>
																		</xsl:if>
																	</xsl:otherwise>
																</xsl:choose>
															</td>
														</tr>
														<tr valign="top">
															<td/>
															<td>
																<xsl:if test="../Phone[node()] = true()">
																	<xsl:if test="../TollFreePhone[node()] = true()">
																		<Span Class="Protocol-Site-TollFreePhone">
																			<xsl:value-of select="../TollFreePhone"/>
																		</Span>
																	</xsl:if>
																</xsl:if>
															</td>
														</tr>
													</table>
												</td>
											</tr>
										</xsl:when>
										<xsl:otherwise>
											<tr name="1" ref="{$PersonRef}">
												<td valign="top" colspan="2">&#160;</td>
												<td valign="top">
													<Span Class="Protocol-Site-PersonName">
														<xsl:value-of select="concat(../../../PersonNameInformation/GivenName, ' ', ../../../PersonNameInformation/SurName)"/>
													</Span>
													<xsl:if test="../../../@status !=''">
														<Span Class="Protocol-Site-Status">(<xsl:value-of select="../../../@status"/>)</Span>
													</xsl:if>
												</td>
												<td valign="top">
													<table align="left" valign="top" border="0" cellspacing="0" cellpadding="0">
														<tr valign="top">
															<td>
																<xsl:if test="../Phone[node()] = true() or ../TollFreePhone[node()] = true()">
																	<Span Class="Protocol-Site-Phone">Ph:&#160;</Span>
																</xsl:if>
															</td>
															<td>
																<xsl:choose>
																	<xsl:when test="../Phone[node()] = true()">
																		<Span Class="Protocol-Site-Phone">
																			<xsl:value-of select="../Phone"/>
																		</Span>
																	</xsl:when>
																	<xsl:otherwise>
																		<xsl:if test="../TollFreePhone[node()] = true()">
																			<Span Class="Protocol-Site-Phone">
																				<xsl:value-of select="../TollFreePhone"/>
																			</Span>
																		</xsl:if>
																	</xsl:otherwise>
																</xsl:choose>
															</td>
														</tr>
														<tr valign="top">
															<td/>
															<td>
																<xsl:if test="../Phone[node()] = true()">
																	<xsl:if test="../TollFreePhone[node()] = true()">
																		<Span Class="Protocol-Site-TollFreePhone">
																			<xsl:value-of select="../TollFreePhone"/>
																		</Span>
																	</xsl:if>
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
								<tr email="1">
									<td valign="top" colspan="3">&#160;</td>
									<td align="left">
										<Span Class="Protocol-Site-Email">Email:
								<xsl:element name="a">
												<xsl:attribute name="href">mailto:<xsl:value-of select="../Email"/></xsl:attribute>
												<xsl:value-of select="../Email"/>
											</xsl:element>
										</Span>
									</td>
								</tr>
							</xsl:if>
							<tr space="1">
								<td valign="top" colspan="4">
									<img src="/images/spacer.gif" width="1" height="5" alt="" border="0"/>
								</td>
							</tr>
						</xsl:when>
					</xsl:choose>
					<xsl:variable name="CurrentCountry" select="CountryName"/>
					<xsl:variable name="CurrentState" select="PoliticalSubUnitName"/>
					<xsl:variable name="CurrentCity" select="City"/>
					<xsl:variable name="CurrentSiteName" select="../../../../SiteName"/>

					<xsl:variable name="Site">
						<xsl:if test="../../../@site!=''">
							<xsl:value-of select="../../../@site"/>
						</xsl:if>
					</xsl:variable>

					<tr contact="{$Site}"></tr>
				</xsl:for-each>
				<!-- END of USA Study sites and contacts -->
				<!--Beginning of Non-USA study sites and contacts-->
				<xsl:for-each select="$ProtocolSites">
					<xsl:sort select="CountryName"/>
					<xsl:sort select="PoliticalSubUnitName"/>
					<xsl:sort select="City"/>
					<xsl:sort select="../../../../SiteName"/>
					
					<xsl:choose>
						<xsl:when test="CountryName!='U.S.A.'">
							<xsl:choose>
								<xsl:when test="position()!=1">
									<xsl:choose>
										<xsl:when test="$CurrentCountryNon!=CountryName">
											<tr country="1">
												<td valign="top" colspan="4" Class="Protocol-country">
													<xsl:value-of select="CountryName"/>
												</td>
											</tr>
											<tr space="1">
												<td valign="top"  colspan="4" style="height:5px"></td>
											</tr>
										</xsl:when>
										<xsl:otherwise>
											<tr country="1">
												<td valign="top" colspan="4" Class="Protocol-country">
													<xsl:value-of select="CountryName"/>
												</td>
											</tr>
											<tr space="1">
												<td valign="top"  colspan="4" style="height:5px"></td>
											</tr>
										</xsl:otherwise>
									</xsl:choose>
								</xsl:when>
								<xsl:otherwise>
									<tr country="1">
										<td valign="top" colspan="4" Class="Protocol-country">
											<xsl:value-of select="CountryName"/>
										</td>
									</tr>
									<tr space="1">
										<td valign="top"  colspan="4" style="height:5px"></td>
									</tr>
								</xsl:otherwise>
							</xsl:choose>

							<xsl:choose>
								<xsl:when test="position()!=1">
									<xsl:choose>
										<xsl:when test="$CurrentStateNon!=''">
											<xsl:if test="$CurrentStateNon!=PoliticalSubUnitName">
												<tr state="1">
													<td valign="top" colspan="4" Class="Protocol-state">
														<xsl:value-of select="PoliticalSubUnitName"/>
													</td>
												</tr>
												<tr space="1">
													<td valign="top"  colspan="4" style="height:5px"></td>
												</tr>
											</xsl:if>
										</xsl:when>
										<xsl:otherwise>
											<tr state="1">
												<td valign="top"  colspan="4" Class="Protocol-state">
													<xsl:value-of select="PoliticalSubUnitName"/>
												</td>
											</tr>
											<tr space="1">
												<td valign="top"  colspan="4" style="height:5px"></td>
											</tr>
										</xsl:otherwise>
									</xsl:choose>
								</xsl:when>
								<xsl:otherwise>
									<tr state="1">
										<td  valign="top" colspan="4" Class="Protocol-state">
											<xsl:value-of select="PoliticalSubUnitName"/>
										</td>
									</tr>
									<tr space="1">
										<td valign="top"  colspan="4" style="height:5px"></td>
									</tr>
								</xsl:otherwise>
							</xsl:choose>

							<xsl:choose>
								<xsl:when test="position()!=1">
									<xsl:choose>
										<xsl:when test="$CurrentCityNon!=' '">
											<xsl:choose>
												<xsl:when test="$CurrentStateNon !=' '">
													<xsl:if test="$CurrentStateNon =PoliticalSubUnitName">
														<xsl:if test="$CurrentCityNon!=City">
															<tr city="1">
																<td valign="top">&#160;</td>
																<td valign="top" colspan="3" Class="Protocol-city">
																	<xsl:value-of select="City"/>
																</td>
															</tr>
															<tr space="1">
																<td valign="top"  colspan="4" style="height:20px"></td>
															</tr>
														</xsl:if>
														<!-- state exists, if non-existing state, display it -->
													</xsl:if>
													<xsl:if test="$CurrentStateNon !=PoliticalSubUnitName">
														<tr city="1">
															<td valign="top">&#160;</td>
															<td valign="top" colspan="3" Class="Protocol-city">
																<xsl:value-of select="City"/>
															</td>
														</tr>
														<tr space="1">
															<td valign="top"  colspan="4" style="height:20px"></td>
														</tr>
													</xsl:if>
												</xsl:when>
												<xsl:otherwise>
													<xsl:if test="$CurrentCityNon!=City">
														<tr city="1">
															<td valign="top">&#160;</td>
															<td valign="top" colspan="3" Class="Protocol-city">
																<xsl:value-of select="City"/>
															</td>
														</tr>
														<tr space="1">
															<td valign="top"  colspan="4" style="height:20px"></td>
														</tr>
													</xsl:if>
												</xsl:otherwise>
											</xsl:choose>
											<!--Nothing for equal city-->
										</xsl:when>
										<xsl:otherwise>
											<tr city="1">
												<td valign="top">&#160;</td>
												<td valign="top" colspan="3" Class="Protocol-city">
													<xsl:value-of select="City"/>
												</td>
											</tr>
											<tr space="1">
												<td valign="top"  colspan="4" style="height:20px"></td>
											</tr>
										</xsl:otherwise>
									</xsl:choose>
								</xsl:when>
								<xsl:otherwise>
									<tr city="1">
										<td valign="top">&#160;</td>
										<td  valign="top" colspan="3" Class="Protocol-city">
											<xsl:value-of select="City"/>
										</td>
									</tr>
									<tr space="1">
										<td valign="top"  colspan="4" style="height:20px"></td>
									</tr>
								</xsl:otherwise>
							</xsl:choose>

							<xsl:variable name="SiteRef">
								<xsl:if test="../../../../@ref!=''">
									<xsl:value-of select="../../../../@ref"/>
								</xsl:if>
							</xsl:variable>

							<xsl:variable name="PersonRef">
								<xsl:if test="../../../@ref!=''">
									<xsl:value-of select="../../../@ref"/>
								</xsl:if>
							</xsl:variable>

							<xsl:choose>
								<xsl:when test="position() !=1">
									<xsl:choose>
										<xsl:when test="string-length($CurrentSiteName) =0 and string-length(../../../../SiteName) !=0">
											<tr facility="1" ref="{$SiteRef}">
												<td valign="top">&#160;</td>
												<td valign="top" colspan="3">
													<Span Class="Protocol-Site-SiteName">
														<xsl:value-of select="../../../../SiteName"/>
													</Span>
												</td>
											</tr>
											<tr space="1">
												<td valign="top" colspan="4">
													<img src="/images/spacer.gif" width="1" height="5" alt="" border="0"/>
												</td>
											</tr>
										</xsl:when>
										<xsl:otherwise>
											<xsl:if test="$CurrentSiteName != ../../../../SiteName">
												<tr facility="1" ref="{$SiteRef}">
													<td valign="top">&#160;</td>
													<td valign="top" colspan="3">
														<Span Class="Protocol-Site-SiteName">
															<xsl:value-of select="../../../../SiteName"/>
														</Span>
													</td>
												</tr>
												<tr space="1">
													<td valign="top" colspan="4">
														<img src="/images/spacer.gif" width="1" height="5" alt="" border="0"/>
													</td>
												</tr>
											</xsl:if>
										</xsl:otherwise>
									</xsl:choose>
								</xsl:when>
								<xsl:otherwise>
									<tr facility="1" ref="{$SiteRef}">
										<td valign="top">&#160;</td>
										<td valign="top" colspan="3">
											<Span Class="Protocol-Site-SiteName">
												<xsl:value-of select="../../../../SiteName"/>
											</Span>
										</td>
									</tr>
									<tr space="1">
										<td valign="top" colspan="4">
											<img src="/images/spacer.gif" width="1" height="5" alt="" border="0"/>
										</td>
									</tr>
								</xsl:otherwise>
							</xsl:choose>
							<xsl:choose>
								<xsl:when test="concat(../../../PersonNameInformation/GivenName, ../../../PersonNameInformation/SurName) =''">
									<tr name="1" ref="{$PersonRef}">
										<td valign="top" colspan="2">&#160;</td>
										<td valign="top">
											<Span Class="Protocol-Site-PersonRole">
												<xsl:value-of select="../../../PersonRole"/>
											</Span>
										</td>
										<td valign="top">
											<table align="left" valign="top" border="0" cellspacing="0" cellpadding="0">
												<tr valign="top">
													<td>
														<xsl:if test="../Phone[node()] = true() or ../TollFreePhone[node()] = true()">
															<Span Class="Protocol-Site-Phone">Ph:&#160;</Span>
														</xsl:if>
													</td>
													<td>
														<xsl:choose>
															<xsl:when test="../Phone[node()] = true()">
																<Span Class="Protocol-Site-Phone">
																	<xsl:value-of select="../Phone"/>
																</Span>
															</xsl:when>
															<xsl:otherwise>
																<xsl:if test="../TollFreePhone[node()] = true()">
																	<Span Class="Protocol-Site-Phone">
																		<xsl:value-of select="../TollFreePhone"/>
																	</Span>
																</xsl:if>
															</xsl:otherwise>
														</xsl:choose>
													</td>
												</tr>
												<tr valign="top">
													<td/>
													<td>
														<xsl:if test="../Phone[node()] = true()">
															<xsl:if test="../TollFreePhone[node()] = true()">
																<Span Class="Protocol-Site-TollFreePhone">
																	<xsl:value-of select="../TollFreePhone"/>
																</Span>
															</xsl:if>
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
											<tr name="1" ref="{$PersonRef}">
												<td valign="top" colspan="2">&#160;</td>
												<td valign="top">
													<Span Class="Protocol-Site-PersonName">
														<xsl:value-of select="concat(../../../PersonNameInformation/GivenName, ' ', ../../../PersonNameInformation/SurName)"/>
													</Span>
													<Span Class="Protocol-Site-Suffix">
														<xsl:value-of select="concat(', ',../../../PersonNameInformation/ProfessionalSuffix)"/>
													</Span>
													<xsl:if test="../../../@status !=''">
														<Span Class="Protocol-Site-Status">(<xsl:value-of select="../../../@status"/>)</Span>
													</xsl:if>
												</td>
												<td valign="top">
													<table align="left" valign="top" border="0" cellspacing="0" cellpadding="0">
														<tr valign="top">
															<td>
																<xsl:if test="../Phone[node()] = true() or ../TollFreePhone[node()] = true()">
																	<Span Class="Protocol-Site-Phone">Ph:&#160;</Span>
																</xsl:if>
															</td>
															<td>
																<xsl:choose>
																	<xsl:when test="../Phone[node()] = true()">
																		<Span Class="Protocol-Site-Phone">
																			<xsl:value-of select="../Phone"/>
																		</Span>
																	</xsl:when>
																	<xsl:otherwise>
																		<xsl:if test="../TollFreePhone[node()] = true()">
																			<Span Class="Protocol-Site-Phone">
																				<xsl:value-of select="../TollFreePhone"/>
																			</Span>
																		</xsl:if>
																	</xsl:otherwise>
																</xsl:choose>
															</td>
														</tr>
														<tr valign="top">
															<td/>
															<td>
																<xsl:if test="../Phone[node()] = true()">
																	<xsl:if test="../TollFreePhone[node()] = true()">
																		<Span Class="Protocol-Site-TollFreePhone">
																			<xsl:value-of select="../TollFreePhone"/>
																		</Span>
																	</xsl:if>
																</xsl:if>
															</td>
														</tr>
													</table>
												</td>
											</tr>
										</xsl:when>
										<xsl:otherwise>
											<tr name="1" ref="{$PersonRef}">
												<td valign="top" colspan="2">&#160;</td>
												<td valign="top">
													<Span Class="Protocol-Site-PersonName">
														<xsl:value-of select="concat(../../../PersonNameInformation/GivenName, ' ', ../../../PersonNameInformation/SurName)"/>
													</Span>
													<xsl:if test="../../../@status !=''">
														<Span Class="Protocol-Site-Status">(<xsl:value-of select="../../../@status"/>)</Span>
													</xsl:if>
												</td>
												<td valign="top">
													<table align="left" valign="top" border="0" cellspacing="0" cellpadding="0">
														<tr valign="top">
															<td>
																<xsl:if test="../Phone[node()] = true() or ../TollFreePhone[node()] = true()">
																	<Span Class="Protocol-Site-Phone">Ph:&#160;</Span>
																</xsl:if>
															</td>
															<td>
																<xsl:choose>
																	<xsl:when test="../Phone[node()] = true()">
																		<Span Class="Protocol-Site-Phone">
																			<xsl:value-of select="../Phone"/>
																		</Span>
																	</xsl:when>
																	<xsl:otherwise>
																		<xsl:if test="../TollFreePhone[node()] = true()">
																			<Span Class="Protocol-Site-Phone">
																				<xsl:value-of select="../TollFreePhone"/>
																			</Span>
																		</xsl:if>
																	</xsl:otherwise>
																</xsl:choose>
															</td>
														</tr>
														<tr valign="top">
															<td/>
															<td>
																<xsl:if test="../Phone[node()] = true()">
																	<xsl:if test="../TollFreePhone[node()] = true()">
																		<Span Class="Protocol-Site-TollFreePhone">
																			<xsl:value-of select="../TollFreePhone"/>
																		</Span>
																	</xsl:if>
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
								<tr email="1">
									<td valign="top" colspan="3">&#160;</td>
									<td align="left">
										<Span Class="Protocol-Site-Email">Email:
								<xsl:element name="a">
												<xsl:attribute name="href">mailto:<xsl:value-of select="../Email"/></xsl:attribute>
												<xsl:value-of select="../Email"/>
											</xsl:element>
										</Span>
									</td>
								</tr>
							</xsl:if>
							<tr space="1">
								<td valign="top" colspan="4">
									<img src="/images/spacer.gif" width="1" height="5" alt="" border="0"/>
								</td>
							</tr>
						</xsl:when>
					</xsl:choose>
					<xsl:variable name="CurrentCountryNon" select="CountryName"/>
					<xsl:variable name="CurrentStateNon" select="PoliticalSubUnitName"/>
					<xsl:variable name="CurrentCityNon" select="City"/>
					<xsl:variable name="CurrentSiteName" select="../../../../SiteName"/>

					<xsl:variable name="Site">
						<xsl:if test="../../../@site!=''">
							<xsl:value-of select="../../../@site"/>
						</xsl:if>
					</xsl:variable>

					<tr contact="{$Site}"></tr>
				</xsl:for-each>
			</table>
		</P>
		</xsl:element>
	</xsl:template>
	<!-- ********************** End StudyContacts ********************* -->
	<!-- ********************** Begin ProtocolRelatedLinks ********************* -->
	<xsl:template match="ProtocolRelatedLinks">
		<xsl:element name="a">
			<xsl:attribute name="name">ProtocolRelatedLinks:<xsl:value-of select="../@id"/></xsl:attribute>
		<a name="ProtocolRelatedLinks_{../@id}"/>
		
			<h2>Related Information</h2>
		
		<p>
			<xsl:apply-templates/>
		</p>
		</xsl:element>
	</xsl:template>
	<xsl:template match="RelatedProtocols">
		<a>
			<xsl:attribute name="Class">Protocol-RelatedProtocols</xsl:attribute>
			<xsl:attribute name="href">/search/ViewClinicalTrials.aspx?cdrid=<xsl:value-of select="number(substring-after(@ref,'CDR'))"/>&amp;version=patient</xsl:attribute>PDQ® clinical trial <xsl:value-of select="."/>
		</a>
		<br/>
	</xsl:template>
	<xsl:template match="RelatedWebsites">
		<a>
			<xsl:attribute name="Class">Protocol-RelatedWebsites</xsl:attribute>
			<xsl:attribute name="href"><xsl:if test="starts-with(@xref, 'http') = false()">Http://</xsl:if><xsl:value-of select="@xref"/></xsl:attribute>
			<xsl:value-of select="."/>
		</a>
		<br/>
	</xsl:template>
	<!-- ************************* End ProtocolRelatedLinks *********************************** -->
	<!-- **************************** RegistryInfo Section ******************************* -->
	<xsl:template match="RegistryInfo">
		<xsl:element name="a">
			<xsl:attribute name="name">RegistryInfo_<xsl:value-of select="../@id"/></xsl:attribute>
		</xsl:element>
		<p/>
		<div class="pdq-shaded-area">
      <h2>
        Registry Information
      </h2>
			<table class="table-default">
				
				<xsl:if test="ProtocolTitle[node()] = true()">
					<xsl:if test="ProtocolTitle/@Type = 'Official' ">
						<tr>
							<td>
								<strong>Official Title</strong>
							</td>
							<td>
								<xsl:value-of select="ProtocolTitle"/>
							</td>
						</tr>
					</xsl:if>
				</xsl:if>
				<xsl:if test="../ProtocolAdminInfo/StartDate[node()] = true()">
					<tr>
						<td>
							<strong>Trial Start Date</strong>
						</td>
						<td>
							<xsl:value-of select="../ProtocolAdminInfo/StartDate"/>
							<xsl:if test="../ProtocolAdminInfo/StartDate/@DateType = 'Projected' "> (estimated)</xsl:if>
						</td>
					</tr>
				</xsl:if>
				<xsl:if test="../ProtocolAdminInfo/CompletionDate[node()] = true()">
					<tr>
						<td>
							<strong>Trial Completion Date</strong>
						</td>
						<td>
							<xsl:value-of select="../ProtocolAdminInfo/CompletionDate"/>
							<xsl:if test="../ProtocolAdminInfo/CompletionDate/@DateType = 'Projected' "> (estimated)</xsl:if>
						</td>
					</tr>
				</xsl:if>
				<xsl:if test="ClinicalTrialsGovID[node()] = true()">
					<tr>
						<td>
							<strong>Registered in ClinicalTrials.gov</strong>
						</td>
						<td>
							<a>
								<xsl:attribute name="Class">Protocol-RelatedWebsites</xsl:attribute>
								<xsl:attribute name="href"><xsl:if test="starts-with(ClinicalTrialsGovID/@xref, 'http') = false()">Http://</xsl:if><xsl:value-of select="ClinicalTrialsGovID/@xref"/></xsl:attribute>
								<xsl:value-of select="ClinicalTrialsGovID"/>
							</a>
						</td>
					</tr>
				</xsl:if>
				<xsl:if test="DateSubmittedtoPDQ[node()] = true()">
					<tr>
						<td>
							<strong>Date Submitted to PDQ</strong>
						</td>
						<td>
							<xsl:value-of select="DateSubmittedtoPDQ"/>
						</td>
					</tr>
				</xsl:if>
				<xsl:if test="../DateLastVerified[node()] = true()">
					<tr>
						<td>
							<strong>Information Last Verified</strong>
						</td>
						<td>
							<xsl:value-of select="../DateLastVerified"/>
						</td>
					</tr>
				</xsl:if>
				<xsl:if test="../FundingInfo/NIHGrantContract/GrantContractNo[node()] = true()">
					<tr>
						<td>
							<strong>NCI Grant/Contract Number</strong>
						</td>
						<td>
							<xsl:for-each select="../FundingInfo/NIHGrantContract/GrantContractNo">                                       
				                            <xsl:if test="position()=1">
				                                   <xsl:value-of select="."/>
					                     </xsl:if>
			                                   <xsl:if test="position() !=1">
			                                           <xsl:value-of select="', '"/>
			                                           <xsl:value-of select="."/>
			                                    </xsl:if>
			                          </xsl:for-each>
						</td>
					</tr>
				</xsl:if>
			</table>
		</div>
		<p/>
	</xsl:template>
	<!-- **************************** End RegistryInfo Section ******************************* -->
  
  <!--Clean list Styles from the Prototype XSL for Protocols-->
  <!--
  ================================================================ -->
  <xsl:template                  match = "ItemizedList">
    <xsl:param                     name = "topSection"
                                 select = "'il'"/>
    
    <xsl:element                   name = "ul">
      <xsl:if                    test = "@id">
        <xsl:attribute                name = "id">
          <xsl:value-of              select = "@id"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:apply-templates>
        <xsl:with-param          name = "topSection"
                               select = "$topSection"/>
      </xsl:apply-templates>
    </xsl:element>
  </xsl:template>

  <!--
  ================================================================ -->
  <xsl:template                  match = "OrderedList">
    <xsl:param                     name = "topSection"
                                 select = "'ol'"/>
    
    <xsl:element                   name = "ol">
      <xsl:if                    test = "@id">
        <xsl:attribute                name = "id">
          <xsl:value-of              select = "@id"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:apply-templates       select = "@Style"/>
      <xsl:apply-templates>
        <xsl:with-param          name = "topSection"
                               select = "$topSection"/>
      </xsl:apply-templates>
    </xsl:element>
  </xsl:template>

  <!--
  Ordered lists will be displayed as is
  Unordered lists will be displayed without style and without 
  compact mode.  No 'dash', no 'bullet'.  
  Style="simple" will be converted into class="no-bullets" (indentation?)
  and eventually converted to some sort of address block when available
  ================================================================ -->
  <xsl:template                  match = "@Style">
    <xsl:choose>
      <xsl:when                      test = ". = 'bullet'"/>
      <!-- Arabic (i.e. class="decimal") is the default.  
         Don't need to include this in the HTML output -->
      <xsl:when                      test = ". = 'Arabic'"/>
      <xsl:otherwise>
        <xsl:attribute                 name = "class">
          <xsl:choose>
            <xsl:when                    test = ". = 'LAlpha'">
              <xsl:text>lower-alpha</xsl:text>
            </xsl:when>
            <xsl:when                    test = ". = 'LRoman'">
              <xsl:text>lower-roman</xsl:text>
            </xsl:when>
            <xsl:when                    test = ". = 'UAlpha'">
              <xsl:text>upper-alpha</xsl:text>
            </xsl:when>
            <xsl:when                    test = ". = 'URoman'">
              <xsl:text>upper-roman</xsl:text>
            </xsl:when>
            <xsl:when                    test = ". = 'circle'">
              <xsl:text>list-circle</xsl:text>
            </xsl:when>
            <xsl:when                    test = ". = 'square'">
              <xsl:text>list-square</xsl:text>
            </xsl:when>
            <xsl:when                    test = ". = 'dash'">
              <xsl:text>list-dash</xsl:text>
            </xsl:when>
            <xsl:when                    test = ". = 'simple'">
              <xsl:text>pdq-address-block</xsl:text>
            </xsl:when>
            <xsl:otherwise>
              <xsl:text>style_undefined</xsl:text>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!--
  ================================================================ -->
  <xsl:template                  match = "ListItem">
    <xsl:param                     name = "topSection"
                                 select = "'li'"/>
    <li>
      <xsl:apply-templates>
        <xsl:with-param          name = "topSection"
                               select = "$topSection"/>
      </xsl:apply-templates>
    </li>
  </xsl:template>

  <!--
  ================================================================ -->
  <xsl:template                  match = "ListTitle">
    <xsl:element                   name = "p">
      <xsl:attribute                name = "class">
        <xsl:text>pdq-list-title</xsl:text>
      </xsl:attribute>

      <xsl:apply-templates/>
    </xsl:element>
  </xsl:template>
  
</xsl:stylesheet>
