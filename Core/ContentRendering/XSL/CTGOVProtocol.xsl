<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" xmlns:scripts="urn:local-scripts">
	<xsl:include href="Common/CommonElements.xsl"/>
	<xsl:include href="Common/CommonScripts.xsl"/>
	<xsl:include href="Common/CustomTemplates.xsl"/>
	<xsl:include href="Common/CommonTableFormatter.xsl"/>
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
	<xsl:template match="CTGovProtocol">
		<p>
			<xsl:for-each select="BriefTitle">
				<span Class="Protocol-Title">
					<xsl:apply-templates/>
				</span>
			</xsl:for-each>
		</p>
		<p>
			<!--xsl:element name="a">
				<xsl:attribute name="href">#Title</xsl:attribute>
					<span Class="Protocol-TOC-Link">Title</span>	
				</xsl:element>
			<br/-->
			<!-- Seems like this part is not displayed in cancer.gov site-->
			<!--xsl:if test="OfficialTitle[node()] = true()">
				<xsl:element name="a">
					<xsl:attribute name="href">
						#AlternateTitle_<xsl:value-of select="@id"/>
					</xsl:attribute>
					<span Class="Protocol-TOC-Link">Alternate Title</span>
				</xsl:element>
				<br/>
			</xsl:if-->
			<xsl:element name="a">
				<xsl:attribute name="href">#StudyIdInfo_<xsl:value-of select="@id"/></xsl:attribute>
				<span Class="Protocol-TOC-Link">Basic Trial Information</span>
			</xsl:element>
			<br/>
			<xsl:element name="a">
				<xsl:attribute name="href">#TrialDescription_<xsl:value-of select="@id"/></xsl:attribute>
				<span Class="Protocol-TOC-Link">Trial Description</span>
			</xsl:element>
			<br/>
			<img src="/images/spacer.gif" width="12px" height="1px" alt="" border="0"/>
			<xsl:element name="a">
				<xsl:attribute name="href">
					#Objectives_<xsl:value-of select="@id"/>
				</xsl:attribute>
				<span Class="Protocol-TOC-Link">Summary</span>
			</xsl:element>
			<br/>
			<xsl:if test="DetailedDescription[node()] = true()">
				<img src="/images/spacer.gif" width="12px" height="1px" alt="" border="0"/>
				<xsl:element name="a">
					<xsl:attribute name="href">#Outline_<xsl:value-of select="@id"/></xsl:attribute>
					<span Class="Protocol-TOC-Link">Further Trial Information</span></xsl:element>
				<br/>
			</xsl:if>
			<img src="/images/spacer.gif" width="12px" height="1px" alt="" border="0"/>
			<xsl:element name="a">
				<xsl:attribute name="href">#EntryCriteria_<xsl:value-of select="@id"/></xsl:attribute>
				<span Class="Protocol-TOC-Link">Eligibility Criteria</span></xsl:element>
			<br/>
			<xsl:element name="a">
				<xsl:attribute name="href">#TrialContact_<xsl:value-of select="@id"/></xsl:attribute>
			<span Class="Protocol-TOC-Link">Trial Contact Information</span></xsl:element>
			<br/>
		</p>
		<!--xsl:if test="OfficialTitle[node()] = true()">
			<xsl:apply-templates select="OfficialTitle"/>
		</xsl:if-->
		<xsl:apply-templates select="BriefTitle"/>
		<xsl:apply-templates select="BriefSummary"/>
		<xsl:if test="DetailedDescription[node()] = true()">
			<xsl:apply-templates select="DetailedDescription"/>
		</xsl:if>
		<xsl:apply-templates select="CTEntryCriteria"/>
		<xsl:apply-templates select="Sponsors"/>
		<!-- Start of the order of Location -->
		<xsl:if test="Location[node()] = true()">
			<xsl:element name="a">
				<xsl:attribute name="name">TrialSites</xsl:attribute>
				<span Class="Protocol-Section-SubHeading">Trial Sites</span>
				<xsl:variable name="ProtocolSites" select="//PostalAddress"/>
				<P>
					<table width="100%" border="0" cellspacing="0" cellpadding="0">
						<tr space="1">
							<td valign="top"  style="height:1px; width: 7%;"></td>
							<td valign="top" style="height:1px; width: 7%;"></td>
							<td valign="top" style="height:1px; width: 33%;"></td>
							<td valign="top" style="height:1px; width: 53%;"></td>
						</tr>
						<!--Beginning of USA study sites and contacts-->
						<xsl:for-each select="$ProtocolSites">
							<xsl:sort select="CountryName"/>
							<xsl:sort select="PoliticalSubUnitName"/>
							<xsl:sort select="City"/>
							<xsl:sort select="../FacilityName"/>
							<xsl:variable name="CurrentCountry" select="CountryName"/>
							<xsl:variable name="CurrentState">
								<xsl:if test="PoliticalSubUnitName[node()] = true()">
									<xsl:value-of select="PoliticalSubUnitName"/>
								</xsl:if>
							</xsl:variable>
							<xsl:variable name="CurrentCity">
								<xsl:if test="City[node()] = true()">
									<xsl:value-of select="City"/>
								</xsl:if>
							</xsl:variable>
							<xsl:variable name="CurrentSiteName">
								<xsl:if test="../FacilityName = true()">
									<xsl:value-of select="../FacilityName"/>
								</xsl:if>
							</xsl:variable>
							<xsl:choose>
								<xsl:when test="CountryName='U.S.A.'">
									<xsl:choose>
										<xsl:when test="position()!=1">
											<xsl:if test="$CurrentCountry != CountryName">
												<tr country="1">
													<td valign="top" colspan="4" class="Protocol-Site-CountryName">
														<span>
															<xsl:value-of select="CountryName"/>
														</span>
													</td>
												</tr>
												<tr space="1">
													<td valign="top" colspan="4" style="height:5px"></td>
												</tr>
											</xsl:if>
										</xsl:when>
										<xsl:otherwise>
											<tr country="1">
												<td valign="top" colspan="4" class="Protocol-Site-CountryName">
													<span>
														<xsl:value-of select="CountryName"/>
													</span>
												</td>
											</tr>
											<tr space="1">
												<td valign="top" colspan="4" style="height:5px"></td>
											</tr>
										</xsl:otherwise>
									</xsl:choose>
									<xsl:if test="PoliticalSubUnitName != ''">
										<tr state="1">
											<td valign="top" colspan="4">
												<span Class="Protocol-Site-StateName">
													<xsl:value-of select="PoliticalSubUnitName"/>
												</span>
											</td>
										</tr>
										<tr space="1">
											<td valign="top" colspan="4" style="height:5px"></td>
										</tr>
									</xsl:if>
									<xsl:if test="City != ''">
										<tr city="1">
											<td valign="top">&#160;</td>
											<td valign="top" colspan="3">
												<span  class="Protocol-city">
													<xsl:value-of select="City"/>
												</span>
											</td>
										</tr>
										<tr space="1">
											<td valign="top" colspan="4" style="height:15px"></td>
										</tr>
									</xsl:if>

									<xsl:variable name="SiteRef">
										<xsl:if test="../FacilityName/@ref!=''">
											<xsl:value-of select="../FacilityName/@ref"/>
										</xsl:if>
									</xsl:variable>
									<xsl:variable name="SiteName">
										<xsl:if test="../FacilityName!=''">
											<xsl:value-of select="../FacilityName"/>
										</xsl:if>
									</xsl:variable>
									<xsl:if test="../../CTGovContact[node()] = true()">
										<xsl:for-each select="../../CTGovContact">
											<tr facility="1" ref="{$SiteRef}">
												<td valign="top">&#160;</td>
												<td valign="top" colspan="3">
													<span Class="Protocol-Site-SiteName">
														<xsl:value-of select="$SiteName"/>
													</span>
												</td>
											</tr>
											<tr space="1">
												<td valign="top" colspan="4" style="height:5px"></td>
											</tr>
											<tr name="1" ref="{@ref}" type="2">
												<td valign="top" colspan="2">&#160;</td>
												<td valign="top">
													<span Class="Protocol-Site-PersonName">
														<xsl:choose>
															<xsl:when test="GivenName[node()] =true() ">
																<xsl:choose>
																	<xsl:when test="MiddleInitial[node()] =true() ">
																		<xsl:value-of select="concat(GivenName, ' ', MiddleInitial, ' ', SurName)"/>
																	</xsl:when>
																	<xsl:otherwise>
																		<xsl:value-of select="concat(GivenName, ' ', SurName)"/>
																	</xsl:otherwise>
																</xsl:choose>
															</xsl:when>
															<xsl:otherwise>
																<xsl:value-of select="SurName"/>
															</xsl:otherwise>
														</xsl:choose>
													</span>
													<span Class="Protocol-Site-Suffix">
														<xsl:if test="ProfessionalSuffix[node()] =true()">
															<xsl:value-of select="concat(', ',ProfessionalSuffix)"/>
														</xsl:if>
													</span>
												</td>
												<td valign="top">
													<span Class="Protocol-Site-Phone">
														<xsl:if test="Phone[node()] = true()">
															Ph:&#160;<xsl:value-of select="Phone"/>
														</xsl:if>
														<xsl:if test="PhoneExt[node()] = true()">
															<xsl:value-of select="concat('  Ext.', PhoneExt)"/>
														</xsl:if>
													</span>
												</td>
											</tr>
											<xsl:if test="Email[node()] = true()">
												<tr email="1">
													<td valign="top" colspan="3">&#160;</td>
													<td valign="top" align="left">
														<span Class="Protocol-Site-Email">
															Email:
															<xsl:element name="a">
																<xsl:attribute name="href">
																	mailto:<xsl:value-of select="Email"/>?Subject=<xsl:value-of select="concat(/CTGovProtocol/IDInfo/NCTID,',',/CTGovProtocol/IDInfo/OrgStudyID,':- ',normalize-space(translate(translate(/CTGovProtocol/BriefTitle,'&#xA;',' '),'&#xD;',' ')))"/>
																</xsl:attribute>
																<xsl:value-of select="Email"/>
															</xsl:element>
														</span>
													</td>
												</tr>
											</xsl:if>
											<tr space="1">
												<td valign="top" colspan="4" style="height:5px"></td>
											</tr>
											<tr contact="{@site}"/>
										</xsl:for-each>
									</xsl:if>
									<xsl:if test="../../CTGovContactBackup[node()] = true()">
										<tr facility="1" ref="{$SiteRef}">
											<td valign="top">&#160;</td>
											<td valign="top" colspan="3">
												<span Class="Protocol-Site-SiteName">
													<xsl:value-of select="$SiteName"/>
												</span>
											</td>
										</tr>
										<tr space="1">
											<td valign="top" colspan="4" style="height:5px"></td>
										</tr>
										<tr name="1" ref="{@ref}" type="3">
											<td valign="top" colspan="2">&#160;</td>
											<td valign="top">
												<span Class="Protocol-Site-PersonName">
													<xsl:choose>
														<xsl:when test="../../CTGovContactBackup/GivenName[node()] =true() ">
															<xsl:choose>
																<xsl:when test="../../CTGovContactBackup/MiddleInitial[node()] =true() ">
																	<xsl:value-of select="concat(../../CTGovContactBackup/GivenName, ' ', ../../CTGovContactBackup/MiddleInitial, ' ', ../../CTGovContactBackup/SurName)"/>
																</xsl:when>
																<xsl:otherwise>
																	<xsl:value-of select="concat(../../CTGovContactBackup/GivenName, ' ', ../../CTGovContactBackup/SurName)"/>
																</xsl:otherwise>
															</xsl:choose>
														</xsl:when>
														<xsl:otherwise>
															<xsl:value-of select="../../CTGovContactBackup/SurName"/>
														</xsl:otherwise>
													</xsl:choose>
												</span>
												<span Class="Protocol-Site-Suffix">
													<xsl:if test="../../CTGovContactBackup/ProfessionalSuffix[node()] = true()">
														<xsl:value-of select="concat(', ',../../CTGovContactBackup/ProfessionalSuffix)"/>
													</xsl:if>
												</span>
											</td>
											<td valign="top">
												<span Class="Protocol-Site-Phone">
													<xsl:if test="../../CTGovContactBackup/Phone[node()] = true()">
														Ph:&#160;<xsl:value-of select="../../CTGovContactBackup/Phone"/>
													</xsl:if>
													<xsl:if test="../../CTGovContactBackup/PhoneExt[node()] = true()">
														<xsl:value-of select="concat('  Ext.', ../../CTGovContactBackup/PhoneExt)"/>
													</xsl:if>
												</span>
											</td>
										</tr>
										<xsl:if test="../../CTGovContactBackup/Email[node()] = true()">
											<tr email="1">
												<td valign="top" colspan="3">&#160;</td>
												<td valign="top" align="left">
													<span Class="Protocol-Site-Email">
														Email:
														<xsl:element name="a">
															<xsl:attribute name="href">
																mailto:<xsl:value-of select="../../CTGovContactBackup/Email"/>?Subject=<xsl:value-of select="concat(/CTGovProtocol/IDInfo/NCTID,',',/CTGovProtocol/IDInfo/OrgStudyID,':- ',normalize-space(translate(translate(/CTGovProtocol/BriefTitle,'&#xA;',' '),'&#xD;',' ')))"/>
															</xsl:attribute>
															<xsl:value-of select="../../CTGovContactBackup/Email"/>
														</xsl:element>
													</span>
												</td>
											</tr>
										</xsl:if>
										<tr space="1">
											<td valign="top" colspan="4" style="height:5px"></td>
										</tr>
										<tr contact="{../../CTGovContactBackup/@site}"/>
									</xsl:if>
									<xsl:if test="../../Investigator[node()] = true()">
										<xsl:for-each select="../../Investigator">
											<tr facility="1" ref="{$SiteRef}">
												<td valign="top">&#160;</td>
												<td valign="top" colspan="3">
													<span Class="Protocol-Site-SiteName">
														<xsl:value-of select="$SiteName"/>
													</span>
												</td>
											</tr>
											<tr space="1">
												<td valign="top" colspan="4" style="height:5px"></td>
											</tr>
											<tr name="1" ref="{@ref}" type="4">
												<td valign="top" colspan="2">&#160;</td>
												<td valign="top">
													<span Class="Protocol-Site-PersonName">
														<xsl:choose>
															<xsl:when test="GivenName[node()] =true() ">
																<xsl:choose>
																	<xsl:when test="MiddleInitial[node()] =true() ">
																		<xsl:value-of select="concat(GivenName, ' ', MiddleInitial, ' ', SurName)"/>
																	</xsl:when>
																	<xsl:otherwise>
																		<xsl:value-of select="concat(GivenName, ' ', SurName)"/>
																	</xsl:otherwise>
																</xsl:choose>
															</xsl:when>
															<xsl:otherwise>
																<xsl:value-of select="SurName"/>
															</xsl:otherwise>
														</xsl:choose>
													</span>
													<span Class="Protocol-Site-Suffix">
														<xsl:if test="ProfessionalSuffix[node()] =true()">
															<xsl:value-of select="concat(', ',ProfessionalSuffix)"/>
														</xsl:if>
													</span>
												</td>
												<td valign="top">
													<span Class="Protocol-Site-PersonRole">
														<xsl:if test="Role[node()] = true()">
															<xsl:value-of select="Role"/>
														</xsl:if>
													</span>
												</td>
											</tr>
											<tr space="1">
												<td valign="top" colspan="4" style="height:5px"></td>
											</tr>
											<tr contact="{@site}"/>
										</xsl:for-each>
									</xsl:if>

									<!-- for case that there is no contact node under facility -->
									<xsl:if test="../../Investigator[node()] != true() and ../../CTGovContact[node()] != true() and ../../CTGovContactBackup[node()] != true()">
										<tr facility="1" ref="{$SiteRef}">
											<td valign="top">&#160;</td>
											<td valign="top" colspan="3">
												<span Class="Protocol-Site-SiteName">
													<xsl:value-of select="$SiteName"/>
												</span>
											</td>
										</tr>
										<tr space="1">
											<td valign="top" colspan="4" style="height:5px"></td>
										</tr>
										<tr contact="{../@site}"/>
									</xsl:if>
								</xsl:when>
							</xsl:choose>
						</xsl:for-each>
						<!-- END of USA Study sites and contacts -->
						<!--Beginning of Non-USA study sites and contacts-->
						<xsl:for-each select="$ProtocolSites">
							<xsl:sort select="CountryName"/>
							<xsl:sort select="PoliticalSubUnitName"/>
							<xsl:sort select="City"/>
							<xsl:sort select="../FacilityName"/>
							<xsl:variable name="NonCurrentCountry" select="CountryName"/>
							<xsl:variable name="NonCurrentState">
								<xsl:if test="PoliticalSubUnitName[node()] = true()">
									<xsl:value-of select="PoliticalSubUnitName"/>
								</xsl:if>
							</xsl:variable>
							<xsl:variable name="NonCurrentCity">
								<xsl:if test="City[node()] = true()">
									<xsl:value-of select="City"/>
								</xsl:if>
							</xsl:variable>
							<xsl:variable name="NonCurrentSiteName">
								<xsl:if test="../FacilityName = true()">
									<xsl:value-of select="../FacilityName"/>
								</xsl:if>
							</xsl:variable>
							<xsl:choose>
								<xsl:when test="CountryName !='U.S.A.'">
									<xsl:if test="CountryName != ''">
										<tr country="1">
											<td valign="top" colspan="4" class="Protocol-Site-CountryName">
												<span>
													<xsl:value-of select="CountryName"/>
												</span>
											</td>
										</tr>
										<tr space="1">
											<td valign="top" colspan="4" style="height:5px"></td>
										</tr>
									</xsl:if>
									<xsl:if test="PoliticalSubUnitName != ''">
										<tr state="1">
											<td valign="top" colspan="4">
												<span Class="Protocol-Site-StateName">
													<xsl:value-of select="PoliticalSubUnitName"/>
												</span>
											</td>
										</tr>
										<tr space="1">
											<td valign="top" colspan="4" style="height:5px"></td>
										</tr>
									</xsl:if>
									<xsl:if test="City != ''">
										<tr city="1">
											<td valign="top">&#160;</td>
											<td valign="top" colspan="3">
												<span class="Protocol-city">
													<xsl:value-of select="City"/>
												</span>
											</td>
										</tr>
										<tr space="1">
											<td valign="top" colspan="4" style="height:15px"></td>
										</tr>
									</xsl:if>
									<xsl:variable name="SiteRef">
										<xsl:if test="../FacilityName/@ref!=''">
											<xsl:value-of select="../FacilityName/@ref"/>
										</xsl:if>
									</xsl:variable>
									<xsl:variable name="SiteName">
										<xsl:if test="../FacilityName!=''">
											<xsl:value-of select="../FacilityName"/>
										</xsl:if>
									</xsl:variable>
									<xsl:if test="../../CTGovContact[node()] = true()">
										<xsl:for-each select="../../CTGovContact">
											<tr facility="1" ref="{$SiteRef}">
												<td valign="top">&#160;</td>
												<td valign="top" colspan="3">
													<span Class="Protocol-Site-SiteName">
														<xsl:value-of select="$SiteName"/>
													</span>
												</td>
											</tr>
											<tr space="1">
												<td valign="top" colspan="4" style="height:5px"></td>
											</tr>
											<tr name="1" ref="{@ref}" type="2">
												<td valign="top" colspan="2">&#160;</td>
												<td valign="top">
													<span Class="Protocol-Site-PersonName">
														<xsl:choose>
															<xsl:when test="GivenName[node()] =true() ">
																<xsl:choose>
																	<xsl:when test="MiddleInitial[node()] =true() ">
																		<xsl:value-of select="concat(GivenName, ' ', MiddleInitial, ' ', SurName)"/>
																	</xsl:when>
																	<xsl:otherwise>
																		<xsl:value-of select="concat(GivenName, ' ', SurName)"/>
																	</xsl:otherwise>
																</xsl:choose>
															</xsl:when>
															<xsl:otherwise>
																<xsl:value-of select="SurName"/>
															</xsl:otherwise>
														</xsl:choose>
													</span>
													<span Class="Protocol-Site-Suffix">
														<xsl:if test="ProfessionalSuffix[node()] =true()">
															<xsl:value-of select="concat(', ',ProfessionalSuffix)"/>
														</xsl:if>
													</span>
												</td>
												<td valign="top">
													<span Class="Protocol-Site-Phone">
														<xsl:if test="Phone[node()] = true()">
															Ph:&#160;<xsl:value-of select="Phone"/>
														</xsl:if>
														<xsl:if test="PhoneExt[node()] = true()">
															<xsl:value-of select="concat('  Ext.', PhoneExt)"/>
														</xsl:if>
													</span>
												</td>
											</tr>
											<xsl:if test="Email[node()] = true()">
												<tr email="1">
													<td valign="top" colspan="3">&#160;</td>
													<td valign="top" align="left">
														<span Class="Protocol-Site-Email">
															Email:
															<xsl:element name="a">
																<xsl:attribute name="href">
																	mailto:<xsl:value-of select="Email"/>?Subject=<xsl:value-of select="concat(/CTGovProtocol/IDInfo/NCTID,',',/CTGovProtocol/IDInfo/OrgStudyID,':- ',normalize-space(translate(translate(/CTGovProtocol/BriefTitle,'&#xA;',' '),'&#xD;',' ')))"/>
																</xsl:attribute>
																<xsl:value-of select="Email"/>
															</xsl:element>
														</span>
													</td>
												</tr>
											</xsl:if>
											<tr space="1">
												<td valign="top" colspan="4" style="height:5px"></td>
											</tr>
											<tr contact="{@site}"/>
										</xsl:for-each>
									</xsl:if>
									<xsl:if test="../../CTGovContactBackup[node()] = true()">
										<tr facility="1" ref="{$SiteRef}">
											<td valign="top">&#160;</td>
											<td valign="top" colspan="3">
												<span Class="Protocol-Site-SiteName">
													<xsl:value-of select="$SiteName"/>
												</span>
											</td>
										</tr>
										<tr space="1">
											<td valign="top" colspan="4" style="height:5px"></td>
										</tr>
										<tr name="1" ref="{@ref}" type="3">
											<td valign="top" colspan="2">&#160;</td>
											<td valign="top">
												<span Class="Protocol-Site-PersonName">
													<xsl:choose>
														<xsl:when test="../../CTGovContactBackup/GivenName[node()] =true() ">
															<xsl:choose>
																<xsl:when test="../../CTGovContactBackup/MiddleInitial[node()] =true() ">
																	<xsl:value-of select="concat(../../CTGovContactBackup/GivenName, ' ', ../../CTGovContactBackup/MiddleInitial, ' ', ../../CTGovContactBackup/SurName)"/>
																</xsl:when>
																<xsl:otherwise>
																	<xsl:value-of select="concat(../../CTGovContactBackup/GivenName, ' ', ../../CTGovContactBackup/SurName)"/>
																</xsl:otherwise>
															</xsl:choose>
														</xsl:when>
														<xsl:otherwise>
															<xsl:value-of select="../../CTGovContactBackup/SurName"/>
														</xsl:otherwise>
													</xsl:choose>
												</span>
												<span Class="Protocol-Site-Suffix">
													<xsl:if test="../../CTGovContactBackup/ProfessionalSuffix[node()] = true()">
														<xsl:value-of select="concat(', ',../../CTGovContactBackup/ProfessionalSuffix)"/>
													</xsl:if>
												</span>
											</td>
											<td valign="top">
												<span Class="Protocol-Site-Phone">
													<xsl:if test="../../CTGovContactBackup/Phone[node()] = true()">
														Ph:&#160;<xsl:value-of select="../../CTGovContactBackup/Phone"/>
													</xsl:if>
													<xsl:if test="../../CTGovContactBackup/PhoneExt[node()] = true()">
														<xsl:value-of select="concat('  Ext.', ../../CTGovContactBackup/PhoneExt)"/>
													</xsl:if>
												</span>
											</td>
										</tr>
										<xsl:if test="../../CTGovContactBackup/Email[node()] = true()">
											<tr email="1">
												<td valign="top" colspan="3">&#160;</td>
												<td valign="top" align="left">
													<span Class="Protocol-Site-Email">
														Email:
														<xsl:element name="a">
															<xsl:attribute name="href">
																mailto:<xsl:value-of select="../../CTGovContactBackup/Email"/>?Subject=<xsl:value-of select="concat(/CTGovProtocol/IDInfo/NCTID,',',/CTGovProtocol/IDInfo/OrgStudyID,':- ',normalize-space(translate(translate(/CTGovProtocol/BriefTitle,'&#xA;',' '),'&#xD;',' ')))"/>
															</xsl:attribute>
															<xsl:value-of select="../../CTGovContactBackup/Email"/>
														</xsl:element>
													</span>
												</td>
											</tr>
										</xsl:if>
										<tr space="1">
											<td valign="top" colspan="4" style="height:5px"></td>
										</tr>
										<tr contact="{../../CTGovContactBackup/@site}"/>
									</xsl:if>
									<xsl:if test="../../Investigator[node()] = true()">
										<xsl:for-each select="../../Investigator">
											<tr facility="1" ref="{$SiteRef}">
												<td valign="top">&#160;</td>
												<td valign="top" colspan="3">
													<span Class="Protocol-Site-SiteName">
														<xsl:value-of select="$SiteName"/>
													</span>
												</td>
											</tr>
											<tr space="1">
												<td valign="top" colspan="4" style="height:5px"></td>
											</tr>
											<tr name="1" ref="{@ref}" type="4">
												<td valign="top" colspan="2">&#160;</td>
												<td valign="top">
													<span Class="Protocol-Site-PersonName">
														<xsl:choose>
															<xsl:when test="GivenName[node()] =true() ">
																<xsl:choose>
																	<xsl:when test="MiddleInitial[node()] =true() ">
																		<xsl:value-of select="concat(GivenName, ' ', MiddleInitial, ' ', SurName)"/>
																	</xsl:when>
																	<xsl:otherwise>
																		<xsl:value-of select="concat(GivenName, ' ', SurName)"/>
																	</xsl:otherwise>
																</xsl:choose>
															</xsl:when>
															<xsl:otherwise>
																<xsl:value-of select="SurName"/>
															</xsl:otherwise>
														</xsl:choose>
													</span>
													<span Class="Protocol-Site-Suffix">
														<xsl:if test="ProfessionalSuffix[node()] =true()">
															<xsl:value-of select="concat(', ',ProfessionalSuffix)"/>
														</xsl:if>
													</span>
												</td>
												<td valign="top">
													<span Class="Protocol-Site-PersonRole">
														<xsl:if test="Role[node()] = true()">
															<xsl:value-of select="Role"/>
														</xsl:if>
													</span>
												</td>
											</tr>
											<tr space="1">
												<td valign="top" colspan="4" style="height:5px"></td>
											</tr>
											<tr contact="{@site}"/>
										</xsl:for-each>
									</xsl:if>

									<!-- for case that there is no contact node under facility -->
									<xsl:if test="../../Investigator[node()] != true() and ../../CTGovContact[node()] != true() and ../../CTGovContactBackup[node()] != true()">
										<tr facility="1" ref="{$SiteRef}">
											<td valign="top">&#160;</td>
											<td valign="top" colspan="3">
												<span Class="Protocol-Site-SiteName">
													<xsl:value-of select="$SiteName"/>
												</span>
											</td>
										</tr>
										<tr space="1">
											<td valign="top" colspan="4" style="height:5px"></td>
										</tr>
										<tr contact="{../@site}"/>
									</xsl:if>

								</xsl:when>
							</xsl:choose>
						</xsl:for-each>
						<!-- END of Non USA Study sites and contacts -->
					</table>
				</P>
			</xsl:element>
		</xsl:if>
		<!-- End of the order of Location -->
		<xsl:apply-templates select="RequiredHeader"/>
		<xsl:apply-templates select="CTGovDisclaimer"/>
		<!--P>
			<xsl:copy-of select="$ReturnToTopBar"/>
		</P-->
	</xsl:template>
	<!-- ************************** End Control Section ***************************** -->
	<!-- ********************************* Title ************************************ -->
	<xsl:template match="BriefTitle">
		<xsl:element name="a">
			<xsl:attribute name="name">StudyIdInfo</xsl:attribute>
		</xsl:element>
		<P>
			<span Class="Protocol-Section-Heading">Basic Trial Information</span>
		</P>
		<table width="570" cellspacing="0" cellpadding="1" border="0" class="Protocol-BasicStudy-Grayborder">
			<tr>
				<td valign="top">
					<table width="568" cellspacing="0" cellpadding="0" border="0" bgcolor="#FFFFFF">
						<tr bgcolor="#E6E6E2">
							<td valign="top">
								<img src="/images/spacer.gif" width="4px" height="21px" alt="" border="0"/>
							</td>
							<td class="black-text" width="53px" valign="middle">
								Phase
								<br/>
								<img width="56" height="1px" border="0" alt="" src="/images/spacer.gif"/>
							</td>
							<td valign="top">
								<img src="/images/spacer.gif" width="4px" height="1px" alt="" border="0"/>
							</td>
							<td valign="top" class="Protocol-BasicStudy-TD-Grayborder">
								<img src="/images/spacer.gif" width="1px" height="1px" alt="" border="0"/>
							</td>
							<td width="4" valign="middle">
								<img src="/images/spacer.gif" width="4px" height="1px" alt="" border="0"/>
							</td>
							<td class="black-text" width="95" valign="middle" align="left">
								Type
								<br/>
								<img width="47" height="1" border="0" alt="" src="/images/spacer.gif"/>
							</td>
							<td width="4" valign="middle">
								<img src="/images/spacer.gif" width="4px" height="1px" alt="" border="0"/>
							</td>
							<td valign="top" class="Protocol-BasicStudy-TD-Grayborder">
								<img src="/images/spacer.gif" width="1px" height="1px" alt="" border="0"/>
							</td>
							<td width="4" valign="middle">
								<img src="/images/spacer.gif" width="4px" height="1px" alt="" border="0"/>
							</td>
							<td class="black-text" width="65px" valign="middle" align="left">
								Status
								<br/>
								<img width="37" height="1px" border="0" alt="" src="/images/spacer.gif"/>
							</td>
							<td width="4" valign="middle">
								<img src="/images/spacer.gif" width="4px" height="1px" alt="" border="0"/>
							</td>
							<xsl:if test="/CTGovProtocol/Eligibility/AgeText[node()] = true() ">
								<td valign="top" class="Protocol-BasicStudy-TD-Grayborder">
									<img src="/images/spacer.gif" width="1px" height="1px" alt="" border="0"/>
								</td>
								<td  width="4" valign="middle">
									<img src="/images/spacer.gif" width="4px" height="1px" alt="" border="0"/>
								</td>
								<td class="black-text" width="85px" valign="middle" align="left">
									Age
								</td>
								<td width="4" valign="middle">
									<img src="/images/spacer.gif" width="4px" height="1px" alt="" border="0"/>
								</td>
							</xsl:if>
							<xsl:if test="/CTGovProtocol/Sponsors/PDQSponsorship[node()] = true()">
								<td valign="top" class="Protocol-BasicStudy-TD-Grayborder">
									<img src="/images/spacer.gif" width="1px" height="1px" alt="" border="0"/>
								</td>
								<td width="4" valign="middle">
									<img src="/images/spacer.gif" width="4px" height="1px" alt="" border="0"/>
								</td>
								<td class="black-text" width="85px" valign="middle" align="left">
									<xsl:choose>
										<xsl:when test="count(/CTGovProtocol/Sponsors/PDQSponsorship) =1">
											Sponsor
										</xsl:when>
										<xsl:otherwise>
											Sponsors
										</xsl:otherwise>
									</xsl:choose>
								</td>
								<td valign="middle">
									<img src="/images/spacer.gif" width="4px" height="1px" alt="" border="0"/>
								</td>
							</xsl:if>
							<td valign="top" class="Protocol-BasicStudy-TD-Grayborder">
								<img src="/images/spacer.gif" width="1px" height="1px" alt="" border="0"/>
							</td>
							<td width="4" valign="middle">
								<img src="/images/spacer.gif" width="4px" height="1px" alt="" border="0"/>
							</td>
							<td class="black-text" width="141" valign="middle" align="left">
								<span Class="Protocol-BasicStudy-Heading">Protocol IDs</span>
							</td>
							<td width="4" valign="middle">
								<img src="/images/spacer.gif" width="4px" height="1px" alt="" border="0"/>
							</td>
						</tr>
						<tr>
							<td valign="top" class="Protocol-BasicStudy-TD-Grayborder" height="1" colspan="23">
							</td>
						</tr>
						<tr bgcolor="#FFFFFF">
							<td valign="top">
								<img src="/images/spacer.gif" alt="" border="0" height="1px" width="4"/>
							</td>
							<td valign="top">
								<img src="/images/spacer.gif" width="1px" height="10px" alt="" border="0"/>
								<br/>
								<xsl:for-each select="/CTGovProtocol/ProtocolPhase">
									<xsl:if test="position()=1">
										<xsl:value-of select="."/>
									</xsl:if>
									<xsl:if test="position() !=1">
										<xsl:value-of select="', '"/>
										<xsl:value-of select="."/>
									</xsl:if>
								</xsl:for-each>
								<br/>
								<img src="/images/spacer.gif" width="1px" height="10px" alt="" border="0"/>
								<br/>
							</td>
							<td valign="top">&#160;</td>
							<td valign="top" class="Protocol-BasicStudy-TD-Grayborder">
								<img src="/images/spacer.gif" width="1px" height="1px" alt="" border="0"/>
							</td>
							<td valign="middle">&#160;</td>
							<td valign="top">
								<img src="/images/spacer.gif" width="1px" height="10px" alt="" border="0"/>
								<br/>
								<xsl:for-each select="/CTGovProtocol/ProtocolDetail/StudyCategory">
									<xsl:if test="position()=1">
										<xsl:value-of select="./StudyCategoryName"/>
									</xsl:if>
									<xsl:if test="position() !=1">
										<xsl:value-of select="', '"/>
										<xsl:value-of select="./StudyCategoryName"/>
									</xsl:if>
								</xsl:for-each>
								<Br/>
								<img src="/images/spacer.gif" width="1px" height="10px" alt="" border="0"/>
								<br/>
							</td>
							<td valign="top">&#160;</td>
							<td valign="top" class="Protocol-BasicStudy-TD-Grayborder">
								<img src="/images/spacer.gif" width="1px" height="1px" alt="" border="0"/>
							</td>
							<td valign="middle">&#160;</td>
							<td valign="top">
								<img src="/images/spacer.gif" width="1px" height="10px" alt="" border="0"/>
								<br/>
								<xsl:value-of select="/CTGovProtocol/CurrentProtocolStatus"/>
								<br/>
								<img src="/images/spacer.gif" width="1px" height="10px" alt="" border="0"/>
								<br/>
							</td>
							<td valign="top">&#160;</td>
							<xsl:if test="/CTGovProtocol/Eligibility/AgeText[node()] = true() ">
								<td valign="top" class="Protocol-BasicStudy-TD-Grayborder">
									<img src="/images/spacer.gif" width="1px" height="1px" alt="" border="0"/>
								</td>
								<td valign="middle">&#160;</td>
								<td valign="top">
									<img src="/images/spacer.gif" width="1px" height="10px" alt="" border="0"/>
									<br/>
									<xsl:value-of select="/CTGovProtocol/Eligibility/AgeText"/>
									<br/>
									<img src="/images/spacer.gif" width="1px" height="10px" alt="" border="0"/>
									<br/>
								</td>
								<td valign="top">&#160;</td>
							</xsl:if>
							<xsl:if test="/CTGovProtocol/Sponsors/PDQSponsorship[node()] = true() ">
								<td valign="top" class="Protocol-BasicStudy-TD-Grayborder">
									<img src="/images/spacer.gif" width="1px" height="1px" alt="" border="0"/>
								</td>
								<td valign="middle">&#160;</td>
								<td valign="top">
									<img src="/images/spacer.gif" width="1px" height="10px" alt="" border="0"/>
									<br/>
									<xsl:for-each select="/CTGovProtocol/Sponsors/PDQSponsorship">
										<xsl:if test="position()=1">
											<xsl:value-of select="."/>
										</xsl:if>
										<xsl:if test="position() !=1">
											<xsl:value-of select="', '"/>
											<xsl:value-of select="."/>
										</xsl:if>
									</xsl:for-each>
									<br/>
									<img src="/images/spacer.gif" width="1px" height="10px" alt="" border="0"/>
									<br/>
								</td>
								<td valign="top">&#160;</td>
							</xsl:if>
							<td valign="top" class="Protocol-BasicStudy-TD-Grayborder">
								<img src="/images/spacer.gif" width="1px" height="1px" alt="" border="0"/>
							</td>
							<td valign="middle">&#160;</td>
							<td valign="top">
								<img src="/images/spacer.gif" width="1px" height="10px" alt="" border="0"/>
								<br/>
								<span Class="Protocol-BasicStudy-PrimaryID">
									<xsl:value-of select="/CTGovProtocol/IDInfo/OrgStudyID"/>
								</span>
								<xsl:for-each select="/CTGovProtocol/IDInfo/SecondaryID">
									<xsl:if test="position() !=1">
										<xsl:value-of select="','"/>
									</xsl:if>
									<br/>
									<xsl:value-of select="."/>
								</xsl:for-each>
								<xsl:if test="count(/CTGovProtocol/IDInfo/SecondaryID) &gt; 0">
									<xsl:value-of select="','"/>
								</xsl:if>
								<br/>
								<xsl:value-of select="/CTGovProtocol/IDInfo/NCTID"/>
								<br/>
								<img src="/images/spacer.gif" width="1px" height="10px" alt="" border="0"/>
								<br/>
							</td>
							<td valign="top">&#160;</td>
						</tr>
					</table>
				</td>
			</tr>
		</table>
	</xsl:template>
	<!-- ******************************** End brief Title ********************************* -->
	<!-- ******************************** Start Official Title ********************************* -->
	<xsl:template match="OfficialTitle">
		<xsl:element name="a">
			<xsl:attribute name="name">AlternateTitle</xsl:attribute>
		</xsl:element>
		<P>
			<span Class="Protocol-OfficialTitle">Alternate Title: <xsl:apply-templates/>
			</span>
		</P>
	</xsl:template>
	<!-- ******************************** End Official Title ********************************* -->
	<!-- ******************************** Summary ********************************** -->
	<xsl:template match="BriefSummary">
			<xsl:element name="a">
				<xsl:attribute name="name">Summary</xsl:attribute>
			<P>
				<span Class="Protocol-Section-SubHeading">Summary</span>
			</P>
			<xsl:apply-templates/>
		</xsl:element>
	</xsl:template>
	<xsl:template match="DetailedDescription">
		<xsl:element name="a">
			<xsl:attribute name="name">DetailedDescription</xsl:attribute>
			<P>
				<span Class="Protocol-Section-SubHeading">Further Study Information</span>
			</P>
			<xsl:apply-templates/>
		</xsl:element>
	</xsl:template>
	<xsl:template match="CTEntryCriteria">
		<xsl:element name="a">
			<xsl:attribute name="name">EntryCriteria</xsl:attribute>
			<P>
				<span Class="Protocol-Section-SubHeading">Eligibility Criteria</span>
			</P>
			<xsl:apply-templates/>
		</xsl:element>
	</xsl:template>
	<xsl:template match="CTGovDisclaimer">
		<xsl:element name="a">
			<xsl:attribute name="name">Disclaimer</xsl:attribute>
			<div class="note">
				<xsl:apply-templates/>
			</div>
			<br/>
		</xsl:element>
	</xsl:template>
	<!-- ****************************** End Summary  ******************************** -->
	<!-- ********************** Leading Organizations ************************* -->
	<xsl:template match="Sponsors">
		<xsl:element name="a">
			<xsl:attribute name="name">TrialContact</xsl:attribute>
		</xsl:element>
		<P>
			<span Class="Protocol-Section-Heading">Trial Contact Information</span>
		</P>
		<xsl:element name="a">
			<xsl:attribute name="name">LeadOrgs</xsl:attribute>
			<P>
				<span Class="Protocol-Section-SubHeading">Trial Lead Organizations/Sponsors</span>
			</P>
			<P>
				<span Class="Protocol-LeadOrg-OrgName">
					<xsl:value-of select="LeadSponsor"/>
				</span>
			</P>
			<xsl:for-each select="Collaborator">
				<span Class="Protocol-LeadOrg-PersonName">
					<xsl:value-of select="."/>
				</span>
				<br/>
				<br/>
			</xsl:for-each>
			<xsl:apply-templates/>
		</xsl:element>
	</xsl:template>
	
	<xsl:template match="OverallContact">
		<table width="100%" cellspacing="0" cellpadding="0" border="0">
			<tr>
				<td valign="top" width="44%">
					<span Class="Protocol-LeadOrg-PersonName">
						<xsl:choose>
							<xsl:when test="GivenName[node()] =true() ">
								<xsl:choose>
									<xsl:when test="MiddleInitial[node()] =true() ">
										<xsl:value-of select="concat(GivenName, ' ', MiddleInitial, ' ', SurName)"/>
									</xsl:when>
									<xsl:otherwise>
										<xsl:value-of select="concat(GivenName, ' ', SurName)"/>
									</xsl:otherwise>
								</xsl:choose>
							</xsl:when>
							<xsl:otherwise>
								<xsl:value-of select="SurName"/>
							</xsl:otherwise>
						</xsl:choose>
					</span>
					<xsl:if test="ProfessionalSuffix[node()] =true() ">
						<span Class="Protocol-LeadOrg-Suffix">
							<xsl:value-of select="concat(', ',ProfessionalSuffix)"/>
						</span>
					</xsl:if>
				</td>
				<td valign="top" width="2%">
					<img src="/images/spacer.gif" width="100%" height="1px" alt="" border="0"/>
				</td>
				<td valign="top" width="54%">
					<span Class="Protocol-LeadOrg-Phone">
						<xsl:if test="Phone[node()] = true()">Ph:&#160;<xsl:value-of select="Phone"/>
						</xsl:if>
						<xsl:if test="PhoneExt[node()] = true()">
							<xsl:value-of select="concat('  Ext.', PhoneExt)"/>
						</xsl:if>
					</span>
				</td>
			</tr>
			<xsl:if test="Email[node()] = true()">
				<tr>
					<td Colspan="2">&#160;</td>
					<td Valign="top" align="left">
						<span Class="Protocol-LeadOrg-Email">
							Email:
							<xsl:element name="a">
								<xsl:attribute name="href">mailto:<xsl:value-of select="Email"/>?Subject=<xsl:value-of select="concat(/CTGovProtocol/IDInfo/NCTID,',',/CTGovProtocol/IDInfo/OrgStudyID,':- ',normalize-space(translate(translate(/CTGovProtocol/BriefTitle,'&#xA;',' '),'&#xD;',' ')))"/></xsl:attribute>
								<xsl:value-of select="Email"/>
							</xsl:element>
						</span>
					</td>
				</tr>
			</xsl:if>
		</table>
		<br/>
	</xsl:template>
	<xsl:template match="OverallContactBackup">
		<table width="100%" cellspacing="0" cellpadding="0" border="0">
			<tr>
				<td valign="top" width="44%">
					<span Class="Protocol-LeadOrg-PersonName">
						<xsl:choose>
							<xsl:when test="GivenName[node()] =true() ">
								<xsl:choose>
									<xsl:when test="MiddleInitial[node()] =true() ">
										<xsl:value-of select="concat(GivenName, ' ', MiddleInitial, ' ', SurName)"/>
									</xsl:when>
									<xsl:otherwise>
										<xsl:value-of select="concat(GivenName, ' ', SurName)"/>
									</xsl:otherwise>
								</xsl:choose>
							</xsl:when>
							<xsl:otherwise>
								<xsl:value-of select="SurName"/>
							</xsl:otherwise>
						</xsl:choose>
					</span>
					<xsl:if test="ProfessionalSuffix[node()] = true()">
						<span Class="Protocol-LeadOrg-Suffix">
							<xsl:value-of select="concat(', ',ProfessionalSuffix)"/>
						</span>
					</xsl:if>
				</td>
				<td valign="top" width="2%">
					<img src="/images/spacer.gif" width="100%" height="1px" alt="" border="0"/>
				</td>
				<td valign="top" width="54%">
					<span Class="Protocol-LeadOrg-Phone">
						<xsl:if test="Phone[node()] = true()">Ph:&#160;<xsl:value-of select="Phone"/>
						</xsl:if>
						<xsl:if test="PhoneExt[node()] = true()">
							<xsl:value-of select="concat('  Ext.', PhoneExt)"/>
						</xsl:if>
					</span>
				</td>
			</tr>
			<xsl:if test="Email[node()] = true()">
				<tr>
					<td Colspan="2">&#160;</td>
					<td valign="top" align="left">
						<span Class="Protocol-LeadOrg-Email">
					Email:
					<xsl:element name="a">
								<xsl:attribute name="href">mailto:<xsl:value-of select="Email"/>?Subject=<xsl:value-of select="concat(/CTGovProtocol/IDInfo/NCTID,',',/CTGovProtocol/IDInfo/OrgStudyID,':- ',normalize-space(translate(translate(/CTGovProtocol/BriefTitle,'&#xA;',' '),'&#xD;',' ')))"/></xsl:attribute>
								<xsl:value-of select="Email"/>
							</xsl:element>
						</span>
					</td>
				</tr>
			</xsl:if>
		</table>
		<br/>
	</xsl:template>
	<xsl:template match="OverallOfficial">
		<table width="100%" cellspacing="0" cellpadding="0" border="0">
			<tr>
				<td valign="top" width="44%">
					<span Class="Protocol-LeadOrg-PersonName">
						<xsl:choose>
							<xsl:when test="GivenName[node()] =true() ">
								<xsl:choose>
									<xsl:when test="MiddleInitial[node()] =true() ">
										<xsl:value-of select="concat(GivenName, ' ', MiddleInitial, ' ', SurName)"/>
									</xsl:when>
									<xsl:otherwise>
										<xsl:value-of select="concat(GivenName, ' ', SurName)"/>
									</xsl:otherwise>
								</xsl:choose>
							</xsl:when>
							<xsl:otherwise>
								<xsl:value-of select="SurName"/>
							</xsl:otherwise>
						</xsl:choose>
					</span>
					<xsl:if test="ProfessionalSuffix[node()] = true()">
						<span Class="Protocol-LeadOrg-Suffix">
							<xsl:value-of select="concat(', ',ProfessionalSuffix)"/>
						</span>
					</xsl:if>
				</td>
				<td valign="top" width="2%">
					<img src="/images/spacer.gif" width="100%" height="1px" alt="" border="0"/>
				</td>
				<td valign="top" width="54%">
					<xsl:if test="Role[node()] = true()">
						<span Class="Protocol-LeadOrg-PersonRole">
							<xsl:value-of select="Role"/>
						</span>
					</xsl:if>
				</td>
			</tr>
		</table>
		<br/>
	</xsl:template>
	<!-- ********************** End Leading Organizations ********************* -->
	<!--Beginning of RequiredHeader -->
	<xsl:template match="RequiredHeader">
		<xsl:element name="a">
			<xsl:attribute name="name">RequiredHeader</xsl:attribute>
			<P>
				<xsl:element name="a">
					<xsl:attribute name="href"><xsl:value-of select="./LinkText/@xref"/></xsl:attribute>
					<xsl:value-of select="./LinkText"/>
				</xsl:element>
				<Br/>
				<span Class="Protocol-NLMIdentifer">
					NLM Identifer <xsl:value-of select="/CTGovProtocol/IDInfo/NCTID"/>
				</span>
				<Br/>
				<span Class="Protocol-DownloadDate">
					<xsl:value-of select="./DownloadDate"/>
				</span>
			</P>
		</xsl:element>
	</xsl:template>
	<!-- END of RequiredHeader -->
</xsl:stylesheet>
