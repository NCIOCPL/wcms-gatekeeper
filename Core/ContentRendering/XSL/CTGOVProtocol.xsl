<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" xmlns:scripts="urn:local-scripts">
  <xsl:include href="Common/CommonElements.xsl"/>
  <xsl:include href="Common/CommonScripts.xsl"/>
  <xsl:include href="Common/CustomTemplates.xsl"/>
  <xsl:include href="Common/CommonTableFormatter.xsl"/>
  <xsl:output method="xml"/>
  <xsl:param name="RootUrl"/>
  <xsl:param name="ProtocolIDString"/>
  <xsl:variable                   name = "cdrId"
                                select = "/CTGovProtocol/@id"/>
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

      <xsl:for-each select="BriefTitle">
        <h1>
          <xsl:apply-templates/>
        </h1>
      </xsl:for-each>
    <p>
      <xsl:element name="a">
        <xsl:attribute name="href">
          #StudyIdInfo_<xsl:value-of select="@id"/>
        </xsl:attribute>
        <span Class="Protocol-TOC-Link">Basic Trial Information</span>
      </xsl:element>
      <br/>
      <xsl:element name="a">
        <xsl:attribute name="href">
          #TrialDescription_<xsl:value-of select="@id"/>
        </xsl:attribute>
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
          <xsl:attribute name="href">
            #Outline_<xsl:value-of select="@id"/>
          </xsl:attribute>
          <span Class="Protocol-TOC-Link">Further Trial Information</span>
        </xsl:element>
        <br/>
      </xsl:if>
      <img src="/images/spacer.gif" width="12px" height="1px" alt="" border="0"/>
      <xsl:element name="a">
        <xsl:attribute name="href">
          #EntryCriteria_<xsl:value-of select="@id"/>
        </xsl:attribute>
        <span Class="Protocol-TOC-Link">Eligibility Criteria</span>
      </xsl:element>
      <br/>
      <xsl:element name="a">
        <xsl:attribute name="href">
          #TrialContact_<xsl:value-of select="@id"/>
        </xsl:attribute>
        <span Class="Protocol-TOC-Link">Trial Contact Information</span>
      </xsl:element>
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
      <a name="TrialSites">

        <!-- Create a list of locations, with a separate location record for each contact.
            The defaultContactKey value is used when no contacts exist. -->
        <locationData>
          <xsl:for-each select="Location">
            <xsl:sort select="Facility/PostalAddress/CountryName" />
            <xsl:sort select="Facility/PostalAddress/PoliticalSubUnitName" />
            <xsl:sort select="Facility/PostalAddress/City" />
            <xsl:sort select="Facility/FacilityName" />

            <!-- These don't vary between contacts at a single facility. -->
            <xsl:variable name="city" select="Facility/PostalAddress/City" />
            <xsl:variable name="politicalSubUnit" select="Facility/PostalAddress/PoliticalSubUnitName" />
            <xsl:variable name="countryName" select="Facility/PostalAddress/CountryName" />
            <xsl:variable name="siteRef" select="Facility/FacilityName/@ref" />
            <xsl:variable name="facilityName" select="Facility/FacilityName" />

            <!-- If there are contacts, create location records. But see below for the case
                where there are no contacts. -->
            <xsl:for-each select="CTGovContact | CTGovContactBackup | Investigator">
              <location contactKey="{@site}">
                <city><xsl:value-of select="$city"/></city>
                <politicalSubUnitName><xsl:value-of select="$politicalSubUnit"/></politicalSubUnitName>
                <country><xsl:value-of select="$countryName"/></country>
                <facilityName siteRef="{$siteRef}"><xsl:value-of select="$facilityName"/></facilityName>
                <contactHTML>
                  <!-- Put the blob of HTML in a CDATA section to prevent surprises -->
                  <xsl:text disable-output-escaping="yes">&lt;![CDATA[</xsl:text>
                    <!-- Contact person's name. -->

                      <!-- This concatenation will lead to extra whitespaces when there's a node missing,
                      but this is an HTML snippet, so the extra spaces will collapse when the snippet
                      displays in the browsr. -->
                      <xsl:value-of select="concat(GivenName, ' ', MiddleInitial, ' ', SurName)"/>
                      <xsl:if test="ProfessionalSuffix">, <xsl:value-of select="ProfessionalSuffix"/></xsl:if>

                      <!-- If this is an investigator, include the Role -->
                      <xsl:if test="(name() = 'Investigator') and (Role[node()] = true())">
                        <br/>
                        <xsl:value-of select="Role"/>
                      </xsl:if>
                      
                      <!-- Phone number, only applicable for CTGovContact or CTGovContactBackup -->
                      <xsl:if test="Phone[node()] = true()">
                        <br />Ph: <xsl:value-of select="Phone"/>
                        <xsl:if test="PhoneExt[node()] = true()">Ext. <xsl:value-of select="PhoneExt"/></xsl:if>
                      </xsl:if>

                      <!-- Email address, only applicable for CTGovContact or CTGovContactBackup -->
                      <xsl:if test="Email[node()] = true()">
                          <br />Email: <a>
                            <!-- Create a mailto: link with a default subject line containing the protocol ID, OrgStudID, and Title -->
                            <xsl:attribute name="href">mailto:<xsl:value-of select="Email"/>?Subject=<xsl:value-of select="concat(/CTGovProtocol/IDInfo/NCTID,',',/CTGovProtocol/IDInfo/OrgStudyID,':- ',normalize-space(translate(translate(/CTGovProtocol/BriefTitle,'&#xA;',' '),'&#xD;',' ')))"/></xsl:attribute>
                            <xsl:value-of select="Email"/>
                          </a>
                      </xsl:if>
                  <xsl:text disable-output-escaping="yes">]]&gt;</xsl:text>
                </contactHTML>
              </location>
            </xsl:for-each>
            
            <!-- If there are no contacts, create a contactless location record. -->
            <xsl:if test="(CTGovContact[node()] = false()) and (CTGovContactBackup[node()] = false()) and (Investigator[node()] = false())">
              <location contactKey="{Facility/@site}">
                <city><xsl:value-of select="$city"/></city>
                <politicalSubUnitName><xsl:value-of select="$politicalSubUnit"/></politicalSubUnitName>
                <country><xsl:value-of select="$countryName"/></country>
                <facilityName siteRef="{$siteRef}"><xsl:value-of select="$facilityName"/></facilityName>
                <contactHTML></contactHTML>
              </location>
            </xsl:if>
            
          </xsl:for-each>
        </locationData>
        
        <h3>Trial Sites</h3>
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
      </a>
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
      <h2>Basic Trial Information</h2>
    <table width="100%" cellspacing="0" cellpadding="1" border="0" class="Protocol-BasicStudy-Grayborder">
      <tr>
        <td valign="top">
          <table width="100%" cellspacing="0" cellpadding="0" border="0" bgcolor="#FFFFFF">
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
      <span Class="Protocol-OfficialTitle">
        Alternate Title: <xsl:apply-templates/>
      </span>
    </P>
  </xsl:template>
  <!-- ******************************** End Official Title ********************************* -->
  <!-- ******************************** Summary ********************************** -->
  <xsl:template match="BriefSummary">
    <xsl:element name="a">
      <xsl:attribute name="name">Summary</xsl:attribute>
      <xsl:element name="h3">
        <xsl:attribute name="id">
          <xsl:text>Objectives_</xsl:text><xsl:value-of select="$cdrId"/>
        </xsl:attribute>
        <xsl:text>Summary</xsl:text>
      </xsl:element>
      <xsl:apply-templates/>
    </xsl:element>
  </xsl:template>
  <xsl:template match="DetailedDescription">
    <xsl:element name="a">
      <xsl:attribute name="name">DetailedDescription</xsl:attribute>
      <xsl:element name="h3">
        <xsl:attribute name="id">
          <xsl:text>Outline_</xsl:text>
          <xsl:value-of select="$cdrId"/>
        </xsl:attribute>
        <xsl:text>Further Study Information</xsl:text>
      </xsl:element>
      <xsl:apply-templates/>
    </xsl:element>
  </xsl:template>
  <xsl:template match="CTEntryCriteria">
    <xsl:element name="a">
      <xsl:attribute name="name">EntryCriteria</xsl:attribute>
      <xsl:element name="h3">
        <xsl:attribute name="id">
          <xsl:text>EntryCriteria_</xsl:text>
          <xsl:value-of select="$cdrId"/>
        </xsl:attribute>
        <xsl:text>Eligibility Criteria</xsl:text>
      </xsl:element>      
      <xsl:apply-templates/>
    </xsl:element>
  </xsl:template>
  <xsl:template match="CTGovDisclaimer">
    <xsl:element name="a">
      <xsl:attribute name="name">Disclaimer</xsl:attribute>
      <p>
        <xsl:apply-templates/>
      </p>
    </xsl:element>
  </xsl:template>
  <!-- ****************************** End Summary  ******************************** -->
  <!-- ********************** Leading Organizations ************************* -->
  <xsl:template match="Sponsors">
    <xsl:element name="a">
      <xsl:attribute name="name">TrialContact</xsl:attribute>
    </xsl:element>

      <h2>Trial Contact Information</h2>

    <xsl:element name="a">
      <xsl:attribute name="name">LeadOrgs</xsl:attribute>
      <xsl:element name="h3">
        <xsl:attribute name="id">
          <xsl:text>LeadOrgs_</xsl:text>
          <xsl:value-of select="$cdrId"/>
        </xsl:attribute>
        <xsl:attribute name="do-not-show">
          <xsl:text>toc</xsl:text>
        </xsl:attribute>
        <xsl:text>Trial Lead Organizations/Sponsors</xsl:text>
      </xsl:element>

      <!-- hide from table of contents on the front end-->
      <h4 do-not-show="toc">
        <xsl:value-of select="LeadSponsor"/>
      </h4>

      <ul>
        <xsl:apply-templates select="Collaborator"/>
      </ul>
      
      <xsl:apply-templates          select = "OverallOfficial"/>
      <xsl:apply-templates          select = "OverallContact"/>
    </xsl:element>
  </xsl:template>
  
  <xsl:template match="Collaborator">
    <li>
      <xsl:value-of select="."/>
    </li>
  </xsl:template>

  <xsl:template match="OverallContact">
    <xsl:element                   name = "div">
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

      <xsl:if test="ProfessionalSuffix[node()] =true() ">

        <xsl:value-of select="concat(', ',ProfessionalSuffix)"/>

      </xsl:if>
      <br />

      <xsl:if test="Phone[node()] = true()">
        Ph:&#160;<xsl:value-of select="Phone"/>
      </xsl:if>
      <xsl:if test="PhoneExt[node()] = true()">
        <xsl:value-of select="concat('  Ext.', PhoneExt)"/>
      </xsl:if>

      <xsl:if test="Email[node()] = true()">
        <br />
        Email:
        <xsl:element name="a">
          <xsl:attribute name="href">mailto:<xsl:value-of select="Email"/>?Subject=<xsl:value-of select="concat(/CTGovProtocol/IDInfo/NCTID,',',/CTGovProtocol/IDInfo/OrgStudyID,':- ',normalize-space(translate(translate(/CTGovProtocol/BriefTitle,'&#xA;',' '),'&#xD;',' ')))"/></xsl:attribute>
          <xsl:value-of select="Email"/>
        </xsl:element>

      </xsl:if>

    </xsl:element>
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
        <tr>
          <td Colspan="2">&#160;</td>
          <td valign="top" align="left">
            <span Class="Protocol-LeadOrg-Email">
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
    </table>
    <br/>
  </xsl:template>
  <xsl:template match="OverallOfficial">
    <xsl:element                   name = "div">

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

      <xsl:if test="ProfessionalSuffix[node()] = true()">
        <xsl:value-of select="concat(', ',ProfessionalSuffix)"/>
      </xsl:if>

      <xsl:if test="Role[node()] = true()">
        <xsl:value-of select="concat(', ',Role)"/>
      </xsl:if>
    </xsl:element>
  </xsl:template>
  
  <!-- ********************** End Leading Organizations ********************* -->
  <!--Beginning of RequiredHeader -->
  <xsl:template match="RequiredHeader">
    <xsl:element name="a">
      <xsl:attribute name="name">RequiredHeader</xsl:attribute>
      <P>
        <xsl:element name="a">
          <xsl:attribute name="href">
            <xsl:value-of select="./LinkText/@xref"/>
          </xsl:attribute>
          <xsl:value-of select="./LinkText"/>
        </xsl:element>
        <Br/>
          NLM Identifier <xsl:value-of select="/CTGovProtocol/IDInfo/NCTID"/>
        <Br/>
        <strong>
          <xsl:value-of select="./DownloadDate"/>
        </strong>
      </P>
    </xsl:element>
  </xsl:template>
  <!-- END of RequiredHeader -->

  <!--Clean list Styles from the Prototype XSL for CTGovProtocols-->
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
      <xsl:apply-templates>
        <xsl:with-param          name = "topSection"
                               select = "$topSection"/>
      </xsl:apply-templates>
    </xsl:element>
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
