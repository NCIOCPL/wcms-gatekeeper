<?xml version='1.0'?>	
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
							  xmlns:msxsl="urn:schemas-microsoft-com:xslt"
							  xmlns:scripts="urn:local-scripts">

  <xsl:import href="Common/CommonTableFormatter.xsl"/>
  <xsl:import href="Common/DeviceFilter.xslt"/>
  
  <xsl:include href="Common/CommonElements.xsl"/>
  <xsl:include href="Common/CustomTemplates.xsl"/>
  
	<xsl:output method="xml"/>

  <!--
    XSL Parameters from DocumentRenderer.Render().
    
    $targetedDevice - the particular device to output for.
  -->
  <xsl:param name="targetedDevice" select="screen" />

  <xsl:template match="/">
		<Summary>
			<xsl:apply-templates/>
		</Summary>
	</xsl:template>
	
	<xsl:template match="*">
		<!-- Suppress default output for unmatched elements.
         This has higher precedence than any imported templates
         which therefore require explicit matches to invoke
         the the imported one. -->
	</xsl:template>

  <!-- Enable device-specific filtering for select elements -->
  <xsl:template match="SummarySection|MediaLink">
    <xsl:apply-templates select="." mode="ApplyDeviceFilter"/>
  </xsl:template>
	
	<!-- ****************************** TOC Section ***************************** -->
	
	<xsl:template match="Summary">
		<xsl:apply-templates/>
	</xsl:template>

	<xsl:template match="SummaryTitle">
    <xsl:if test="$targetedDevice != 'mobile'">
		  <Span Class="Summary-Title"><xsl:apply-templates/></Span>
			  <ul>
          <!-- This filter expression MUST, MUST, MUST be synchronized with the Extract
             path for SummaryTopSection and SummarySubSection. -->
          <xsl:for-each select="//SummarySection[(contains(concat(' ', @IncludedDevices, ' '), concat(' ', $targetedDevice, ' ')) or not(@IncludedDevices)) and (not(contains(concat(' ', @ExcludedDevices, ' '), concat(' ', $targetedDevice, ' '))) or not(@ExcludedDevices))]"> <!-- Only select a valid title element  -->
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
    </xsl:if>
	</xsl:template>

	<!-- *************************** End TOC Section **************************** -->
	
	
	<!-- *********************** Summary Section Section ************************ -->

  <xsl:template match="SummarySection" mode="deviceFiltered">
    <!-- Dispatch and call the device-specific version of the template. -->
    <xsl:choose>
      <xsl:when test="$targetedDevice = 'mobile'">
        <xsl:apply-templates select="." mode="mobile"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:apply-templates select="." mode="screen"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="SummarySection" mode="screen">
    <xsl:choose>
      <xsl:when test="count(ancestor::SummarySection) = 0">
        <span name="Section{@id}" id="Section{@id}">
          <xsl:call-template name="SectionDetails"/>
        </span>
      </xsl:when>
      <xsl:otherwise>
        <a name="Section{@id}" id="Section{@id}"></a>
        <xsl:call-template name="SectionDetails"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="SummarySection" mode="mobile">
    <xsl:choose>
      <xsl:when test="count(ancestor::SummarySection) = 0">
        <div data-role="collapsible" data-collapsed="true" data-iconpos="right" name="Section{@id}" id="Section{@id}">
          <h2 class='section_heading'>
            <span>
              <xsl:value-of select="Title" />
            </span>
          </h2>
          <xsl:call-template name="SectionDetails"/>
        </div>
      </xsl:when>
      <xsl:otherwise>
        <a name="Section{@id}" id="Section{@id}"></a>
        <xsl:call-template name="SectionDetails"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>


  <xsl:template name="SectionDetails">
    <xsl:if test="name(..) = 'Summary' and $targetedDevice != 'mobile'">

      <!-- If the section contains keypoints at any level, build up a list with
           appropriate nesting by recursively walking the SummarySection hierarchy. -->
        <xsl:if test="count(descendant-or-self::KeyPoint) != 0">
          <div class="keyPoints">
            <h4>
							<xsl:choose>
								<xsl:when test="/Summary/SummaryMetaData/SummaryLanguage != 'English'">
									Puntos importantes de esta sección
								</xsl:when>
								<xsl:otherwise>Key Points for This Section 
								</xsl:otherwise>
							</xsl:choose>
						</h4>

              <!-- Current node is a top-level SummarySection.  Keypoints are normally
                   found one-per sub-section, and generally none-at-all at the outermost
                   level of SummarySection tags. -->
              <xsl:choose>

                <!-- Normal case, top-level section has no keypoints -->
                <xsl:when test="count(KeyPoint) = 0">
                  <xsl:apply-templates select="." mode="build-keypoint-list" />
                </xsl:when>

                <!-- Unusual case, roll-up the top-level section's keypoints. -->
                <xsl:otherwise>
                  <ul class="Summary-SummarySection-KeyPoint-UL-Dash">
                    <xsl:for-each select="KeyPoint">
                      <li class="Summary-SummarySection-KeyPoint-LI">
                        <xsl:apply-templates select="." mode="build-keypoint-list" />
                        <xsl:if test="position() = last()">
                          <!-- Current node is a KeyPoint in the for-each, so we need to go up a level
                               to pass the topmost section into the keypoint building list. -->
                          <xsl:apply-templates select=".." mode="build-keypoint-list" />
                        </xsl:if>
                      </li>
                    </xsl:for-each>
                  </ul>
                </xsl:otherwise>
              </xsl:choose>
				  </div>
				</xsl:if>
		</xsl:if>
		<xsl:apply-templates/>
	</xsl:template>

  <!-- Match Summary Sections, but only when we're building a list of keypoints. -->
  <xsl:template match="SummarySection" mode="build-keypoint-list">

    <!-- Current node is a summary section, does it contain subsections with keypoints? -->
    <xsl:choose>

      <!-- Has Keypoints -->
      <xsl:when test="count(SummarySection/KeyPoint) > 0">
        <ul>
          <!-- Set the bullet-point style according to the level of nesting. -->
          <xsl:attribute name="class">
            <xsl:choose>
              <xsl:when test="count(ancestor::SummarySection) = 0">Summary-SummarySection-KeyPoint-UL-Bullet</xsl:when>
              <xsl:otherwise>Summary-SummarySection-KeyPoint-UL-Dash</xsl:otherwise>
            </xsl:choose>
          </xsl:attribute>

          <!-- Roll-up the keypoints from the next level of sub-sections. -->
          <xsl:for-each select="SummarySection">
            <xsl:choose>
              <xsl:when test="count(KeyPoint) > 0">
                <li class="Summary-SummarySection-KeyPoint-LI">
                  <xsl:apply-templates select="KeyPoint" mode="build-keypoint-list" />
                  <xsl:apply-templates select="."        mode="build-keypoint-list" />
                </li>
              </xsl:when>
              <xsl:otherwise>
                <xsl:apply-templates select="." mode="build-keypoint-list" />
              </xsl:otherwise>
            </xsl:choose>
          </xsl:for-each>
        </ul>
      </xsl:when>

      <!-- No Keypoints, process keypoints in sub-sections -->
      <xsl:otherwise>
        <xsl:apply-templates select="SummarySection" mode="build-keypoint-list" />
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>

  <xsl:template match="KeyPoint" mode="build-keypoint-list">
    <xsl:element name="a">
      <xsl:attribute name="href">#Keypoint<xsl:value-of select="count(preceding::KeyPoint) + 1"/></xsl:attribute>
      <xsl:apply-templates/>
    </xsl:element>
  </xsl:template>
  
	<xsl:template match="Title">
    <xsl:choose>
      <xsl:when test="$targetedDevice = 'mobile'">
        <xsl:call-template name="Mobile-Title-and-Keypoint" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="Desktop-Title"/>
      </xsl:otherwise>
    </xsl:choose>
	</xsl:template>

  <xsl:template name="Desktop-Title">
    <xsl:if test="count(ancestor::SummarySection) = 1">
      <!--<Span class="Summary-SummarySection-Title-Level1"><xsl:apply-templates/></Span>-->
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

  <xsl:template name="Mobile-Title-and-Keypoint">
    <xsl:if test="count(ancestor::SummarySection) = 1">
      <!-- <h2 class="Summary-SummarySection-Title-Level1"><xsl:apply-templates/></h2> -->
    </xsl:if>
    <xsl:if test="count(ancestor::SummarySection) = 2">
      <h3 class="Summary-SummarySection-Title-Level2"><xsl:apply-templates/></h3>
    </xsl:if>
    <xsl:if test="count(ancestor::SummarySection) = 3">
      <h4 class="Summary-SummarySection-Title-Level3"><xsl:apply-templates/></h4>
    </xsl:if>
    <xsl:if test="count(ancestor::SummarySection) &gt; 3 ">
      <h5 class="Summary-SummarySection-Title-Level4"><xsl:apply-templates/></h5>
    </xsl:if>
  </xsl:template>
	
	<!-- ******************** End Summary Section Section *********************** -->
	
	
	<!-- 
		************************* Reference Section **************************** 
		Reference will be pre-rendered.
	-->

  
  <xsl:template match="SummaryRef">
    <a inlinetype="SummaryRef" objectid="{@href}">
      <xsl:copy-of select="text()"/>
    </a>
  </xsl:template>
  
  <!--
    Renders a placeholder tag structure for MediaLinks.  The MediaLink data is
    gathered during the Extract step and the tag structure replaced by the CMS,
    using the value of the objectid attribute.
  -->
  <xsl:template match="MediaLink" mode="deviceFiltered">
    <div inlinetype="rxvariant" objectid="{@id}">
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
            <xsl:variable name="audience">
              <xsl:choose>
                <xsl:when test="/Summary/SummaryMetaData/SummaryAudience= 'Patients'">patient</xsl:when>
                <xsl:otherwise>healthprofessional</xsl:otherwise>
              </xsl:choose>
            </xsl:variable>

            <xsl:attribute name="href">/clinicaltrials/search/view?version=<xsl:value-of select="$audience"/>&amp;cdrid=<xsl:value-of select="number(substring-after(@ProtocolID,'CDR'))"/></xsl:attribute>
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
    <xsl:choose>
      <xsl:when test="$targetedDevice = 'mobile'">
        <xsl:call-template name="Mobile-Title-and-Keypoint" />
      </xsl:when>
      <xsl:otherwise>
        <p>
          <a name="Keypoint{count(preceding::KeyPoint) + 1}" id="Keypoint{count(preceding::KeyPoint) + 1}"></a>
          <Span Class="Summary-KeyPoint">
            <xsl:apply-templates/>
          </Span>
        </p>

      </xsl:otherwise>
    </xsl:choose>
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
    <xsl:variable name="audience">
      <xsl:choose>
        <xsl:when test="/Summary/SummaryMetaData/SummaryAudience= 'Patients'">patient</xsl:when>
        <xsl:otherwise>healthprofessional</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    
		<xsl:element name="A">
			<xsl:attribute name="Class">Summary-ProtocolRef</xsl:attribute>
			<xsl:attribute name="href">/clinicaltrials/search/view?version=<xsl:value-of select="$audience"/>&amp;cdrid=<xsl:value-of select="number(substring-after(@href,'CDR'))"/></xsl:attribute>
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

  <!-- Necessitated by the match="*" template to suppress unknown elements. -->
  <xsl:template match="Table">
    <xsl:apply-imports/>
  </xsl:template>

  <!-- Override imported templates. -->

  <!-- Dispatch to device-specific version. -->
  <xsl:template match="ItemizedList|ListItem" mode="deviceFiltered">
    <xsl:choose>
      <xsl:when test="$targetedDevice = 'mobile'">
        <xsl:apply-templates select="." mode="mobile" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:apply-templates select="." mode="screen" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  
  <xsl:template match="ItemizedList" mode="screen">
    <a name="Section{@id}"></a>

    <xsl:element name="a">
      <xsl:attribute name="name">ListSection</xsl:attribute>
    </xsl:element>

    <xsl:if test="ListTitle !=''">
      <p><Span Class="Protocol-IL-Title"><xsl:value-of select="ListTitle"/></Span></p>
    </xsl:if>

    <xsl:choose>
      <xsl:when test="@Style !='simple'">
        <xsl:element name="UL">
          <xsl:attribute name="__id"><xsl:value-of select="@id"/></xsl:attribute>
          <xsl:attribute name="class">Protocol-UL</xsl:attribute>
          <xsl:choose>
            <xsl:when test="@Style='bullet'">
              <xsl:apply-templates>
                <xsl:with-param name="ListType">Protocol-IL-Bullet</xsl:with-param>
              </xsl:apply-templates>
            </xsl:when>
            <xsl:when test="@Style='dash'">
              <xsl:apply-templates>
                <xsl:with-param name="ListType">Protocol-IL-Dash</xsl:with-param>
              </xsl:apply-templates>
            </xsl:when>
            <xsl:otherwise>
              <xsl:apply-templates>
                <xsl:with-param name="ListType">Protocol-IL-Simple</xsl:with-param>
              </xsl:apply-templates>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:element>
      </xsl:when>
      <xsl:otherwise>
        <xsl:element name="DL">
          <xsl:attribute name="__id"><xsl:value-of select="@id"/></xsl:attribute>
          <xsl:apply-templates/>
        </xsl:element>
      </xsl:otherwise>
    </xsl:choose>
    <xsl:element name="a">
      <xsl:attribute name="name">END_ListSection</xsl:attribute>
    </xsl:element>
  </xsl:template>

  <xsl:template match="ItemizedList" mode="mobile">
    <xsl:choose>
      <xsl:when test="@Style !='simple'">
        <xsl:element name="ul">
          <xsl:attribute name="class">
            <xsl:choose>
              <xsl:when test="@Style='bullet'">list-bullet</xsl:when>
              <xsl:when test="@Style='dash'">list-dash</xsl:when>
              <xsl:otherwise>list-simple</xsl:otherwise>
            </xsl:choose>
          </xsl:attribute>
          <xsl:apply-templates />
        </xsl:element>
      </xsl:when>
      <xsl:otherwise>
        <xsl:element name="dl">
          <xsl:attribute name="__id"><xsl:value-of select="@id"/></xsl:attribute>
          <xsl:apply-templates/>
        </xsl:element>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="ListItem" mode="screen">
    <xsl:param name="ListType">Protocol-LI</xsl:param>
    <xsl:choose>
      <xsl:when test="../@Style !='simple'">
        <xsl:choose>
          <xsl:when test="../@Compact ='No'">
            <LI class="{$ListType}">
              <xsl:apply-templates/>
              <p></p>
            </LI>
          </xsl:when>
          <xsl:otherwise>
            <LI class="{$ListType}">
              <xsl:apply-templates/>
            </LI>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:otherwise>
        <xsl:choose>
          <xsl:when test="../@Compact ='No'">
            <DD>
              <xsl:apply-templates/>
              <p></p>
            </DD>
          </xsl:when>
          <xsl:otherwise>
            <DD>
              <xsl:apply-templates/>
            </DD>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="ListItem" mode="mobile">
    <li>
      <xsl:apply-templates />
    </li>
  </xsl:template>

  <!-- End override imported templates. -->


</xsl:stylesheet>
