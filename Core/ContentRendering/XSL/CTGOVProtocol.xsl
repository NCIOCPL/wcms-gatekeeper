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

  <xsl:key                          name = "CountryName"
                                 match = "/CTGovProtocol/Location/
                                           Facility/
                                           PostalAddress/
                                           CountryName/text()"
                                   use = "." />
  <xsl:key                          name = "state"
                                   match = "/CTGovProtocol/Location/
                                           Facility/
                                           PostalAddress/
                                           PoliticalSubUnitName/text()"
                                     use = "." />
  <xsl:key                          name = "city"
                                   match = "/CTGovProtocol/Location/
                                           Facility/
                                           PostalAddress/
                                           City/text()"
                                     use = "." />
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
        <locationData style="display:none;">
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
                      displays in the browser. -->
                  <xsl:value-of select="concat(GivenName, ' ', MiddleInitial, ' ', SurName)"/>
                  <xsl:if test="ProfessionalSuffix">
                    , <xsl:value-of select="ProfessionalSuffix"/>
                  </xsl:if>

                  <!-- If this is an investigator, include the Role -->
                  <xsl:if test="(name() = 'Investigator') and (Role[node()] = true())">
                    <br/>
                    <xsl:value-of select="Role"/>
                  </xsl:if>

                  <!-- Phone number, only applicable for CTGovContact or CTGovContactBackup -->
                  <xsl:if test="Phone[node()] = true()">
                    <br />Ph: <xsl:value-of select="Phone"/>
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
            <xsl:if test="(CTGovContact[node()] = false()) and (CTGovContactBackup[node()] = false()) and (Investigator[node()] = false())">
              <location contactKey="{Facility/@site}">
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

      </a>
    </xsl:if>
    <!-- End of the order of Location -->
    <xsl:call-template        name = "trialLocations"/>
    <xsl:apply-templates select="RequiredHeader"/>
    <xsl:call-template name="CTGovDisclaimer" />

  </xsl:template>

  <xsl:template                   name = "trialLocations">
    <h3 do-not-show="toc">Trial Sites</h3>

    <div>
      <xsl:call-template        name = "countries"/>
      <xsl:call-template        name = "NonUSCountries"/>
    </div>
  </xsl:template>

  <!--
  ================================================================
  Template to find unique country names
  ================================================================ -->
  <xsl:template                   name = "countries">
    <xsl:for-each                 select="/CTGovProtocol/Location/
                                          Facility/PostalAddress/
                                          CountryName/text()[generate-id()
                                       = generate-id(key('CountryName',.)[1])]">

      <xsl:sort        order = "ascending"/>
      <xsl:variable                   name = "country"
                                    select = "."/>


      <xsl:if                     test="$country = 'U.S.A.'">
        <h4 do-not-show="toc">
          <xsl:value-of               select = "."/>
        </h4>

        <xsl:call-template             name = "states">
          <xsl:with-param               name = "country"
                                      select = "$country"/>
        </xsl:call-template>

      </xsl:if>
    </xsl:for-each>
  </xsl:template>

  <xsl:template                   name = "NonUSCountries">
    <xsl:for-each                 select="/CTGovProtocol/Location/
                                          Facility/PostalAddress/
                                          CountryName/text()[generate-id()
                                       = generate-id(key('CountryName',.)[1])]">

      <xsl:sort        order = "ascending"/>
      <xsl:variable                   name = "country"
                                    select = "."/>

      <xsl:if                     test="$country != 'U.S.A.'">
        <h4 do-not-show="toc">
          <xsl:value-of               select = "."/>
        </h4>

        <xsl:call-template             name = "states">
          <xsl:with-param               name = "country"
                                      select = "$country"/>
        </xsl:call-template>
      </xsl:if>

    </xsl:for-each>
  </xsl:template>

  <!--
  ================================================================
  Template to find unique states by country
  ================================================================ -->
  <xsl:template                   name = "states">
    <xsl:param                     name = "country"/>

    <xsl:choose>
      <xsl:when test="/CTGovProtocol/Location/
                                          Facility/
                                          PostalAddress[CountryName = 
                                                                 $country]/
                                          PoliticalSubUnitName">

        <xsl:for-each                 select="/CTGovProtocol/Location/
                                          Facility/
                                          PostalAddress[CountryName = 
                                                                 $country]/
                                          PoliticalSubUnitName/
                                          text()[generate-id()
                                            = generate-id(key('state',.)[1])]">

          <!-- Display the State rows -->
          <xsl:sort/>
          <xsl:variable                   name = "state"
                                        select = "../../PoliticalSubUnitName"/>
          <div class="study-site-state">
            <h5>
              <xsl:value-of               select = "."/>
            </h5>
            <xsl:call-template       name = "cities">
              <xsl:with-param               name = "country"
                                          select = "$country"/>
              <xsl:with-param               name = "state"
                                          select = "$state"/>
            </xsl:call-template>

          </div>

        </xsl:for-each>
      </xsl:when>
      <xsl:otherwise>
        <xsl:variable                   name = "state" select="''"/>
        <xsl:call-template       name = "cities">
          <xsl:with-param               name = "country"
                                      select = "$country"/>
          <xsl:with-param               name = "state"
                                      select = "$state"/>
        </xsl:call-template>

      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>

  <!--
  ================================================================
  Template to find unique cities
  ================================================================ -->
  <xsl:template                   name = "cities">
    <xsl:param                     name = "country"/>
    <xsl:param                     name = "state"/>
    <xsl:choose>
      <xsl:when                     test="$state != ''">

        <xsl:for-each                 select="/CTGovProtocol/Location/
                                           Facility/
                                           PostalAddress[CountryName = 
                                                                  $country]
                                                        [PoliticalSubUnitName = 
                                                                  $state]/
                                           City/text()[generate-id()
                                             = generate-id(key('city',.)[1])]">

          <!-- Display the city rows -->
          <xsl:sort/>
          <xsl:variable                  name = "city"
                                       select = "."/>
          <div class="study-site-city">
            <h6>
              <xsl:value-of               select = "."/>
            </h6>

            <xsl:call-template              name = "organization">
              <xsl:with-param                name = "country"
                                           select = "$country"/>
              <xsl:with-param                name = "state"
                                           select = "$state"/>
              <xsl:with-param                name = "city"
                                           select = "$city"/>
            </xsl:call-template>
          </div>
        </xsl:for-each>

      </xsl:when>
      <xsl:otherwise>
        <xsl:for-each                 select="/CTGovProtocol/Location/
                                           Facility/
                                           PostalAddress[CountryName = 
                                                                  $country]/
                                           City/text()[generate-id()
                                             = generate-id(key('city',.)[1])]">

          <!-- Display the city rows -->
          <xsl:sort/>
          <xsl:variable                  name = "city"
                                       select = "."/>
          <div class="study-site-city">
            <h6>
              <xsl:value-of               select = "."/>
            </h6>

            <xsl:call-template              name = "organization">
              <xsl:with-param                name = "country"
                                           select = "$country"/>
              <xsl:with-param                name = "state"
                                           select = "''"/>
              <xsl:with-param                name = "city"
                                           select = "$city"/>
            </xsl:call-template>
          </div>
        </xsl:for-each>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!--
  ================================================================
  Template to display the org records
  ================================================================ -->
  <xsl:template                   name = "organization">
    <xsl:param                     name = "country"/>
    <xsl:param                     name = "state"/>
    <xsl:param                     name = "city"/>

    <xsl:choose>
      <xsl:when                     test="$state != ''">
        <xsl:for-each                 select="/CTGovProtocol/Location/
                                           Facility/
                                           PostalAddress[CountryName = 
                                                                  $country]
                                                        [PoliticalSubUnitName = 
                                                                  $state]
                                                        [City = $city]">

          <!-- Display the organization name row -->
          <xsl:sort select = "../FacilityName" order="ascending"/>
          <p class="study-site-name">
            <xsl:value-of               select = "../FacilityName"/>

          </p>

          <xsl:for-each select="../../CTGovContact | ../../CTGovContactBackup | ../../Investigator">
            <!-- Contact person's name. -->

            <!-- This concatenation will lead to extra whitespaces when there's a node missing,
                      but this is an HTML snippet, so the extra spaces will collapse when the snippet
                      displays in the browser. -->
            <p>
              <xsl:value-of select="concat(GivenName, ' ', MiddleInitial, ' ', SurName)"/>
              <xsl:if test="ProfessionalSuffix">
                , <xsl:value-of select="ProfessionalSuffix"/>
              </xsl:if>

              <!-- If this is an investigator, include the Role -->
              <xsl:if test="(name() = 'Investigator') and (Role[node()] = true())">
                <br/>
                <xsl:value-of select="Role"/>
              </xsl:if>

              <!-- Phone number, only applicable for CTGovContact or CTGovContactBackup -->
              <xsl:if test="Phone[node()] = true()">
                <br />Ph: <xsl:value-of select="Phone"/>
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
            </p>
          </xsl:for-each>

        </xsl:for-each>
      </xsl:when>
      <xsl:otherwise>
        <xsl:for-each                 select="/CTGovProtocol/Location/
                                           Facility/
                                           PostalAddress[CountryName = 
                                                                  $country]
                                                        [City = $city]">

          <!-- Display the organization name row -->
          <xsl:sort select = "../FacilityName" order="ascending"/>
          <p class="study-site-name">
            <xsl:value-of               select = "../FacilityName"/>

          </p>

          <xsl:for-each select="../../CTGovContact | ../../CTGovContactBackup | ../../Investigator">
            <!-- Contact person's name. -->

            <!-- This concatenation will lead to extra whitespaces when there's a node missing,
                      but this is an HTML snippet, so the extra spaces will collapse when the snippet
                      displays in the browser. -->
            <p>
              <xsl:value-of select="concat(GivenName, ' ', MiddleInitial, ' ', SurName)"/>
              <xsl:if test="ProfessionalSuffix">
                , <xsl:value-of select="ProfessionalSuffix"/>
              </xsl:if>

              <!-- If this is an investigator, include the Role -->
              <xsl:if test="(name() = 'Investigator') and (Role[node()] = true())">
                <br/>
                <xsl:value-of select="Role"/>
              </xsl:if>

              <!-- Phone number, only applicable for CTGovContact or CTGovContactBackup -->
              <xsl:if test="Phone[node()] = true()">
                <br />Ph: <xsl:value-of select="Phone"/>
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
            </p>
          </xsl:for-each>

        </xsl:for-each>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>


  <!-- ************************** End Control Section ***************************** -->
  <!-- ********************************* Title ************************************ -->
  <xsl:template match="BriefTitle">

    <xsl:element name="h2">
      <xsl:attribute name="id">
        <xsl:text>StudyIdInfo_</xsl:text>
        <xsl:value-of select="$cdrId"/>
      </xsl:attribute>
      <xsl:text>Basic Trial Information</xsl:text>
    </xsl:element>

    <table class="table-default">
      <tr>

        <th>Phase</th>
        <th>Type</th>

        <th>Age</th>
        <th>Trial IDs</th>
      </tr>

      <tr>

        <td >

          <xsl:for-each select="/CTGovProtocol/ProtocolPhase">
            <xsl:if test="position()=1">
              <xsl:value-of select="."/>
            </xsl:if>
            <xsl:if test="position() !=1">
              <xsl:value-of select="', '"/>
              <xsl:value-of select="."/>
            </xsl:if>
          </xsl:for-each>

        </td>

        <td >

          <xsl:for-each select="/CTGovProtocol/ProtocolDetail/StudyCategory">
            <xsl:sort select="current()" order="ascending"/>
            <xsl:if test="position()=1">
              <xsl:value-of select="./StudyCategoryName"/>
            </xsl:if>
            <xsl:if test="position() !=1">
              <xsl:value-of select="', '"/>
              <xsl:value-of select="./StudyCategoryName"/>
            </xsl:if>
          </xsl:for-each>

        </td>




        <xsl:if test="/CTGovProtocol/Eligibility/AgeText[node()] = true() ">

          <td>

            <xsl:value-of select="/CTGovProtocol/Eligibility/AgeText"/>

          </td>

        </xsl:if>

        <td>

          <b>
            <xsl:value-of select="/CTGovProtocol/IDInfo/OrgStudyID"/>
          </b>
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
    <xsl:element name="h2">
      <xsl:attribute name="id">
        <xsl:text>AlternateTitle_</xsl:text>
        <xsl:value-of select="$cdrId"/>
      </xsl:attribute>
      <xsl:text> Alternate Title: </xsl:text>
    </xsl:element>
    <xsl:apply-templates/>

  </xsl:template>
  <!-- ******************************** End Official Title ********************************* -->
  <!-- ******************************** Summary ********************************** -->
  <xsl:template match="BriefSummary">
    <xsl:element name="a">
      <xsl:attribute name="name">Summary</xsl:attribute>
      <xsl:element name="h3">
        <xsl:attribute name="id">
          <xsl:text>Objectives_</xsl:text>
          <xsl:value-of select="$cdrId"/>
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
  <xsl:template name="CTGovDisclaimer">
    <a name="Disclaimer">
      <div class="note">
        <p>
          <strong>Note:</strong> Information about participating sites on pharmaceutical industry trials may be incomplete.
          Please visit the ClinicalTrials.gov record via the link provided for more information about participating sites.</p>
      </div>
    </a>
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
        <xsl:text>Trial Lead Organizations / Sponsors / Collaborators</xsl:text>
      </xsl:element>

      <!-- hide from table of contents on the front end-->
      <h4 do-not-show="toc">
        <xsl:value-of select="LeadSponsor"/>
      </h4>

      <ul class="collaborators">
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
          <xsl:attribute name="href">
            mailto:<xsl:value-of select="Email"/>?Subject=<xsl:value-of select="concat(/CTGovProtocol/IDInfo/NCTID,',',/CTGovProtocol/IDInfo/OrgStudyID,':- ',normalize-space(translate(translate(/CTGovProtocol/BriefTitle,'&#xA;',' '),'&#xD;',' ')))"/>
          </xsl:attribute>
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
      <p>
        <a href="{./LinkText/@xref}">
          <xsl:value-of select="./LinkText" />
        </a>
        <br/>
        <span Class="Protocol-NLMIdentifer">
          NLM Identifer <xsl:value-of select="/CTGovProtocol/IDInfo/NCTID"/>
        </span>
      </p>
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
