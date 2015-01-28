<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method = "xml"/>
  <xsl:param    name = "section"/>

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

  <xsl:template match="/CTGovProtocol">
    <html>
      <head>
        <title>
          <xsl:text>WCMS CTGovProtocol: </xsl:text>
          <xsl:value-of             select = "IDInfo/OrgStudyID"/>
        </title>

        <link href="http://www.cancer.gov/PublishedContent/Styles/nvcg.css"
              type="text/css" rel="StyleSheet" />
      </head>
      <body>
        <div class="contentzone">
          <div style="background-color: yellow;">
            <p>
              <xsl:apply-templates     select="@id"/>
              <xsl:text>;</xsl:text>
              <xsl:apply-templates     select="IDInfo/OrgStudyID"/>
            </p>
          </div>

          <div>
            <xsl:apply-templates    select = "BriefTitle"/>

            <div class="on-this-page">
              <h4>Contents on this page</h4>
              <ul>
                <li>
                  <a href="#basictrial">Basic Trial Information</a>
                </li>
                <li>
                  <a href="#basictrial">Trial Description</a>
                  <ul>
                    <li>
                      <a href="#summary">Summary</a>
                    </li>
                    <li>
                      <a href="#furtherinfo">Further Trial Information</a>
                    </li>
                    <li>
                      <a href="#eligibilitycriteria">Eligibility Criteria</a>
                    </li>
                  </ul>
                </li>
                <li>
                  <a href="#trialcontact">Trial Contact Information</a>
                </li>
              </ul>
            </div>

            <xsl:call-template        name = "trialInfoTable"/>

            <xsl:apply-templates    select = "BriefSummary"/>
            <xsl:apply-templates    select = "DetailedDescription"/>
            <xsl:apply-templates    select = "CTEntryCriteria"/>
            <xsl:apply-templates    select = "Sponsors"/>
            <xsl:call-template        name = "trialLocations"/>
            <xsl:apply-templates    select = "RequiredHeader"/>
            <hr />
            <xsl:apply-templates    select = "CTGovDisclaimer"/>
            <hr />
          </div>

        </div>
      </body>
    </html>
  </xsl:template>

  <!--
  Template to remove display of ProtocolTitle
  ================================================================ -->
  <xsl:template                  match = "BriefTitle">
    <h1>
      <xsl:apply-templates/>
    </h1>
  </xsl:template>

  <!--
  Template to remove display of ProtocolSummary
  ================================================================ -->
  <xsl:template                  match = "BriefSummary">
    <xsl:element                   name = "h3">
      <xsl:attribute                name = "id">
        <xsl:text>TrialDescription</xsl:text>
      </xsl:attribute>
      <xsl:text>Trial Description</xsl:text>
    </xsl:element>

    <xsl:element                   name = "h4">
      <xsl:attribute                name = "id">
        <xsl:text>summary</xsl:text>
      </xsl:attribute>
      <xsl:text>Summary</xsl:text>
    </xsl:element>

    <xsl:apply-templates/>

  </xsl:template>

  <!--
  Template to remove display of Description
  ================================================================ -->
  <xsl:template                  match = "DetailedDescription">
    <xsl:element                   name = "h4">
      <xsl:attribute                name = "id">
        <xsl:text>furtherinfo</xsl:text>
      </xsl:attribute>
      <xsl:text>Further Study Information</xsl:text>
    </xsl:element>

    <xsl:apply-templates/>
  </xsl:template>

  <!--
  Template to remove display of Eligibility
  ================================================================ -->
  <xsl:template                  match = "CTEntryCriteria">
    <xsl:element                   name = "h4">
      <xsl:attribute                name = "id">
        <xsl:text>eligibilitycriteria</xsl:text>
      </xsl:attribute>
      <xsl:text>Eligibility Criteria</xsl:text>
    </xsl:element>

    <xsl:apply-templates/>
  </xsl:template>


  <!--
  Template to remove display of Eligibility
  ================================================================ -->
  <xsl:template                  match = "RequiredHeader">
    <xsl:element                   name = "p">
      <xsl:element                  name = "a">
        <xsl:attribute               name = "href">
          <xsl:apply-templates      select = "LinkText/@xref"/>
        </xsl:attribute>
        <xsl:apply-templates       select = "LinkText"/>
      </xsl:element>
      <br />
      <xsl:text>NLM Identifier </xsl:text>
      <xsl:apply-templates        select = "../IDInfo/NCTID"/>
      <br />
      <strong>
        <xsl:apply-templates       select = "DownloadDate"/>
      </strong>
    </xsl:element>
  </xsl:template>


  <!--
  Template to remove display of Contacts
  ================================================================ -->
  <xsl:template                  match = "Sponsors">
    <xsl:element                   name = "h4">
      <xsl:attribute                name = "id">
        <xsl:text>trialcontact</xsl:text>
      </xsl:attribute>
      <xsl:text>Trial Contact Information</xsl:text>
    </xsl:element>

    <xsl:element                   name = "h5">
      <xsl:attribute                name = "id">
        <xsl:text>ContactLead</xsl:text>
      </xsl:attribute>
      <xsl:text>Trial Lead Organization/Sponsors</xsl:text>
    </xsl:element>

    <strong>
      <xsl:apply-templates          select = "LeadSponsor"/>
    </strong>
    <!--
    <xsl:text> and </xsl:text>
    -->
    <br />
    <br />
    <xsl:for-each                 select = "Collaborator">
      <xsl:apply-templates/>
    </xsl:for-each>

    <br />
    <br />
    <xsl:apply-templates          select = "OverallOfficial"/>
    <xsl:apply-templates          select = "OverallContact"/>
  </xsl:template>

  <!--
  Template to remove display of Overall Investigator
  ================================================================ -->
  <xsl:template                  match = "OverallOfficial">
    <xsl:element                   name = "div">
      <xsl:apply-templates        select = "GivenName"/>
      <xsl:text> </xsl:text>
      <xsl:apply-templates        select = "SurName"/>
      <span style="background-color:yellow;">
        <xsl:text> (</xsl:text>
        <xsl:apply-templates        select = "Role"/>
        <xsl:text>)</xsl:text>
      </span>
    </xsl:element>
  </xsl:template>

  <!--
  Template to remove display of Overall Contact
  ================================================================ -->
  <xsl:template                  match = "OverallContact">
    <xsl:element                   name = "div">
      <xsl:apply-templates        select = "SurName"/>
      <xsl:apply-templates        select = "Phone"/>
      <xsl:apply-templates        select = "Email"/>
    </xsl:element>
  </xsl:template>

  <!--
  Template to remove display of Overall Contact Phone
  ================================================================ -->
  <xsl:template                  match = "Phone">
    <br />
    <xsl:text>Ph: </xsl:text>
    <xsl:apply-templates/>
  </xsl:template>

  <!--
  Template to remove display of Overall Contact
  ??? Brief or Official Title
  ??? Which IDs included in Subject
  ================================================================ -->
  <xsl:template                  match = "Email">
    <br />
    <xsl:text>Email: </xsl:text>
    <xsl:element                   name = "a">
      <xsl:attribute                name = "href">
        <xsl:text>mailto:</xsl:text>
        <xsl:apply-templates/>
        <xsl:text>?Subject=</xsl:text>
        <xsl:value-of               select = "/CTGovProtocol/IDInfo/NCTID"/>
        <xsl:text>,</xsl:text>
        <xsl:value-of               select = "/CTGovProtocol/IDInfo/OrgStudyID"/>
        <xsl:text>:</xsl:text>
        <xsl:value-of               select = "/CTGovProtocol/BriefTitle"/>
      </xsl:attribute>
      <xsl:apply-templates/>
    </xsl:element>
  </xsl:template>

  <!--
  Template to remove display of Locations
  ================================================================ -->
  <xsl:template                  match = "Location">
    <div>
      <strong>
        <xsl:apply-templates        select = "Facility/PostalAddress/PoliticalSubUnitName"/>
      </strong>
      <br />

      <xsl:apply-templates        select = "Facility/PostalAddress/City"/>
    </div>

  </xsl:template>

  <!--
  ================================================================ -->
  <xsl:template                  match = "TermName |
                                          SpanishTermName">
    <strong>
      <xsl:apply-templates/>
    </strong>
  </xsl:template>

  <!--
  ================================================================ -->
  <xsl:template                  match = "TermPronunciation">
    <span>
      <xsl:element                 name = "a">
        <xsl:attribute              name = "href">
          <!--
       Next Line For Testing on DEV only !!! 
       ===================================== -->
          <xsl:text>http://www.cancer.gov</xsl:text>
          <xsl:text>/PublishedContent/Media/CDR/Media/</xsl:text>
          <xsl:value-of            select = "number(
                                           substring-after(
                                            MediaLink[@language = 'en']/@ref,
                                            'CDR'))"/>
          <xsl:choose>
            <xsl:when                test = "MediaLink[@language = 'en']/@type
                                           = 'audio/mpeg'">
              <xsl:text>.mp3</xsl:text>
            </xsl:when>
          </xsl:choose>
        </xsl:attribute>
        <!--
      Next Line For Testing on DEV only - hard coded Server name !!! 
      ===================================== -->
        <img width="16" height="16" border="0" alt="listen"
            src="http://www.cancer.gov/images/audio-icon.gif"></img>
      </xsl:element>

      <xsl:text> </xsl:text>
      <xsl:value-of            select = "."/>
      <xsl:text></xsl:text>
      <span style="background: yellow;">
        <xsl:text> XXX mpeg vs mp3 XXX</xsl:text>
      </span>
    </span>
  </xsl:template>


  <!--
  ================================================================ -->
  <xsl:template                  match = "Para">
    <xsl:param                     name = "topSection"
                                 select = "'para'"/>
    <xsl:element                   name = "p">
      <xsl:attribute                name = "id">
        <xsl:value-of              select = "@id"/>
      </xsl:attribute>
      <xsl:attribute                name = "tabindex">
        <xsl:text>0</xsl:text>
      </xsl:attribute>
      <xsl:apply-templates>
        <xsl:with-param          name = "topSection"
                               select = "$topSection"/>
      </xsl:apply-templates>
    </xsl:element>
  </xsl:template>

  <!--
  ================================================================ -->
  <xsl:template                  match = "ItemizedList">
    <xsl:param                     name = "topSection"
                                 select = "'il'"/>
    <xsl:element                   name = "ul">
      <xsl:attribute                name = "id">
        <xsl:value-of              select = "@id"/>
      </xsl:attribute>
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
      <xsl:attribute                name = "id">
        <xsl:value-of              select = "@id"/>
      </xsl:attribute>
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
  <xsl:template                  match = "Strong">
    <strong>
      <xsl:apply-templates/>
    </strong>
  </xsl:template>

  <!--
  ================================================================ -->
  <xsl:template                  match = "Emphasis">
    <em>
      <xsl:apply-templates/>
    </em>
  </xsl:template>

  <!--
  ================================================================ -->
  <xsl:template                  match = "CTGovDisclaimer">
    <xsl:apply-templates/>
  </xsl:template>

  <!--
  ================================================================ -->
  <xsl:template                  match = "GlossaryTermRef">
    <xsl:element                   name = "a">
      <xsl:attribute                name = "class">
        <xsl:text>definition</xsl:text>
      </xsl:attribute>
      <xsl:attribute                name = "href">
        <!--
     Next Line For Testing on DEV only !!! 
     ===================================== -->
        <xsl:text>http://www.cancer.gov</xsl:text>
        <xsl:text>/Common/PopUps/popDefinition.aspx?id=</xsl:text>
        <xsl:value-of              select = "number(
                                           substring-after(@href, 'CDR'))"/>
        <xsl:text>&amp;version=Patient&amp;language=English</xsl:text>
      </xsl:attribute>
      <xsl:attribute                name = "onclick">
        <xsl:text>javascript:popWindow('defbyid','</xsl:text>
        <xsl:value-of              select = "@href"/>
        <xsl:text>&amp;version=Patient&amp;language=English'); </xsl:text>
        <xsl:text>return(false);</xsl:text>
      </xsl:attribute>
      <xsl:apply-templates/>
    </xsl:element>
  </xsl:template>

  <!--
  ================================================================ -->
  <xsl:template                  match = "SummaryRef">
    <xsl:element                   name = "a">
      <xsl:attribute                name = "href">
        <!--
     Next Line For Testing on DEV only !!! 
     ===================================== -->
        <xsl:text>http://www.cancer.gov</xsl:text>
        <xsl:value-of              select = "@url"/>
      </xsl:attribute>
      <xsl:apply-templates/>
    </xsl:element>
  </xsl:template>

  <!--
  ================================================================ -->
  <xsl:template                  match = "ExternalRef">
    <xsl:element                   name = "a">
      <xsl:attribute                name = "href">
        <!--
     Next Line For Testing on DEV only !!! 
     ===================================== -->
        <xsl:value-of              select = "@xref"/>
      </xsl:attribute>
      <xsl:apply-templates/>
    </xsl:element>
  </xsl:template>

  <!--
  ================================================================ -->
  <xsl:template                  match = "MediaLink">
    <xsl:element                   name = "figure">
      <xsl:attribute                name = "class">
        <xsl:text>image-center</xsl:text>
      </xsl:attribute>

      <!--
   Display the 'Enlarge' button
   ============================= -->
      <xsl:element                   name = "a">
        <xsl:attribute                name = "class">
          <xsl:text>article-image-enlarge</xsl:text>
        </xsl:attribute>
        <xsl:attribute                name = "target">
          <xsl:text>_blank</xsl:text>
        </xsl:attribute>
        <xsl:attribute                name = "href">
          <!--
     Next Line For Testing on DEV only !!! 
     ===================================== -->
          <xsl:text>http://www.cancer.gov</xsl:text>
          <xsl:text>/images/cdr/live/CDR</xsl:text>
          <xsl:value-of              select = "number(
                                           substring-after(@ref, 'CDR'))"/>
          <xsl:text>.jpg</xsl:text>
        </xsl:attribute>
        <xsl:text>Enlarge</xsl:text>
      </xsl:element>

      <!--
    Display the Image
    ============================= -->
      <xsl:element                  name = "img">
        <xsl:attribute               name = "__id">
          <xsl:value-of             select = "@id"/>
        </xsl:attribute>
        <xsl:attribute               name = "alt">
          <xsl:value-of             select = "@alt"/>
        </xsl:attribute>
        <xsl:attribute               name = "src">
          <!--
     Next Line For Testing on DEV only !!! 
     ===================================== -->
          <xsl:text>http://www.cancer.gov</xsl:text>
          <xsl:text>/images/cdr/live/CDR</xsl:text>
          <xsl:value-of              select = "number(
                                           substring-after(@ref, 'CDR'))"/>
          <xsl:text>.jpg</xsl:text>
        </xsl:attribute>
      </xsl:element>


      <xsl:element                 name = "figcaption">
        <xsl:element                 name = "div">
          <xsl:attribute              name = "class">
            <xsl:text>caption-container</xsl:text>
          </xsl:attribute>

          <xsl:apply-templates/>
        </xsl:element>
      </xsl:element>
    </xsl:element>
  </xsl:template>


  <!--
  ================================================================ -->
  <xsl:template                  match = "RelatedExternalRef[@UseWith = 'en']">
    <xsl:element                   name = "h4">
      <xsl:text>More Information</xsl:text>
    </xsl:element>

    <xsl:element                   name = "a">
      <xsl:attribute                name = "href">
        <!--
     Next Line For Testing on DEV only !!! 
     ===================================== -->
        <xsl:value-of              select = "@xref"/>
      </xsl:attribute>
      <xsl:apply-templates/>
    </xsl:element>
  </xsl:template>

  <!--
  ================================================================ -->
  <xsl:template                  match = "RelatedExternalRef[@UseWith = 'es']">
    <xsl:element                   name = "h4">
      <xsl:text>M&#xE1;s informaci&#xF3;n</xsl:text>
    </xsl:element>

    <xsl:element                   name = "a">
      <xsl:attribute                name = "href">
        <!--
     Next Line For Testing on DEV only !!! 
     ===================================== -->
        <xsl:value-of              select = "@xref"/>
      </xsl:attribute>
      <xsl:apply-templates/>
    </xsl:element>
  </xsl:template>

  <!--
  ================================================================ -->
  <xsl:template               match = "IDInfo">
    <strong>
      <xsl:value-of            select = "OrgStudyID"/>
    </strong>
    <br />
    <xsl:for-each              select = "SecondaryID |
                                        NCTID">
      <xsl:value-of             select = "normalize-space(.)"/>
      <xsl:if                     test = "not(position() = last())">
        <xsl:text>, </xsl:text>
      </xsl:if>
    </xsl:for-each>
  </xsl:template>

  <!--
  ================================================================ -->
  <xsl:template                   match = "StudyCategoryName">
    <xsl:value-of               select = "normalize-space(.)"/>
    <xsl:if                       test = "not(position() = last())">
      <xsl:text>, </xsl:text>
    </xsl:if>
  </xsl:template>


  <!--
========================================================================
    NAMED TEMPLATES
======================================================================== -->
  <!--
  Template to remove display of SectionMetaData
  ================================================================ -->
  <xsl:template                   name = "toc">
    <!-- xsl:for-each              select = "descendant::Title" -->
    <xsl:apply-templates select = "SummarySection" mode = "toc"/>
  </xsl:template>

  <!--
  Template to display the Protocol Basic Trial Info table
  ================================================================ -->
  <xsl:template                   name = "trialInfoTable">
    <h3 id="basictrial">Basic Trial Information</h3>

    <table class="table-default">
      <colgroup>
        <col class="phaseCol"/>
        <col class="typeCol"/>
        <col class="statusCol"/>
        <col class="ageCol"/>
        <col class="sponsorCol"/>
        <col class="protocolIDCol"/>
      </colgroup>
      <thead>
        <tr>
          <th scope="col">Phase</th>
          <th scope="col">Type</th>
          <th scope="col">Status</th>
          <th scope="col">Age</th>
          <th scope="col">Sponsor</th>
          <th scope="col">Protocol IDs</th>
        </tr>
      </thead>
      <tbody>
        <td>
          <xsl:for-each          select = "ProtocolPhase">
            <xsl:value-of         select = "."/>
            <xsl:if                 test = "not(position() = last())">
              <xsl:text>, </xsl:text>
            </xsl:if>
          </xsl:for-each>
        </td>
        <td>
          <xsl:apply-templates   select = "ProtocolDetail/
                                         StudyCategory/
                                         StudyCategoryName"/>
        </td>
        <td>
          <xsl:value-of          select = "CurrentProtocolStatus"/>
        </td>
        <td>
          <xsl:value-of          select = "Eligibility/
                                         AgeText"/>
        </td>
        <td>
          <xsl:for-each          select = "Sponsors/
                                         PDQSponsorship">
            <xsl:value-of         select = "."/>
            <xsl:if                 test = "not(position() = last())">
              <xsl:text>, </xsl:text>
            </xsl:if>
          </xsl:for-each>
        </td>
        <td>
          <xsl:apply-templates   select = "IDInfo"/>
        </td>
      </tbody>
    </table>
    <xsl:apply-templates select = "SummarySection" mode = "toc"/>
  </xsl:template>

  <!--
  ================================================================
  Template to display the Protocol Basic Trial Info table
  ================================================================ -->
  <xsl:template                   name = "trialLocations">
    <h3>Trial Sites</h3>

    <div class="trial-sites">
      <xsl:call-template        name = "countries"/>
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

      <xsl:sort        order = "descending"/>
      <xsl:variable                   name = "country"
                                    select = "."/>
      <h4>
        <xsl:value-of               select = "."/>
      </h4>

      <xsl:call-template             name = "states">
        <xsl:with-param               name = "country"
                                    select = "$country"/>
      </xsl:call-template>

    </xsl:for-each>
  </xsl:template>


  <!--
  ================================================================
  Template to find unique states by country
  ================================================================ -->
  <xsl:template                   name = "states">
    <xsl:param                     name = "country"/>

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
      <h5>
        <xsl:value-of               select = "."/>
      </h5>

      <xsl:call-template       name = "cities">
        <xsl:with-param               name = "country"
                                    select = "$country"/>
        <xsl:with-param               name = "state"
                                    select = "$state"/>
      </xsl:call-template>

      <!--
    <tr>
     <td>
     <xsl:apply-templates    select = "Location[Facility/PostalAddress/
                                                CountryName = 'U.S.A.']">
      <xsl:sort              select = "Facility/
                                        PostalAddress/
                                        PoliticalSubUnitName"/>
     </xsl:apply-templates>
     </td>
    </tr>
    -->
    </xsl:for-each>
  </xsl:template>


  <!--
  ================================================================
  Template to find unique cities
  ================================================================ -->
  <xsl:template                   name = "cities">
    <xsl:param                     name = "country"/>
    <xsl:param                     name = "state"/>

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

    </xsl:for-each>
  </xsl:template>


  <!--
  ================================================================
  Template to display the org records
  ================================================================ -->
  <xsl:template                   name = "organization">
    <xsl:param                     name = "country"/>
    <xsl:param                     name = "state"/>
    <xsl:param                     name = "city"/>

    <xsl:for-each                 select="/CTGovProtocol/Location/
                                           Facility/
                                           PostalAddress[CountryName = 
                                                                  $country]
                                                        [PoliticalSubUnitName = 
                                                                  $state]
                                                        [City = $city]">

      <!-- Display the organization name row -->
      <xsl:sort/>
      <div class="two-columns">
        <p>
          <strong>
            <xsl:value-of               select = "../FacilityName"/>
          </strong>
        </p>

        <!-- Display the CTGovContact -->
        <div class="column1">
          <xsl:value-of           select = "../../CTGovContact/GivenName"/>
          <xsl:text> </xsl:text>
          <xsl:value-of           select = "../../CTGovContact/MiddleInitial"/>
          <xsl:text> </xsl:text>
          <xsl:value-of           select = "../../CTGovContact/SurName"/>
        </div>
        <div class="column2">
          <xsl:text>Ph: </xsl:text>
          <xsl:value-of           select = "../../CTGovContact/Phone"/>
          <xsl:if                   test = "../../CTGovContact/Email/text()">
            <xsl:apply-templates   select = "../../CTGovContact/Email"/>
          </xsl:if>
        </div>
      </div>

    </xsl:for-each>
  </xsl:template>

</xsl:stylesheet>
