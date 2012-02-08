<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
							  xmlns:msxsl="urn:schemas-microsoft-com:xslt"
							  xmlns:scripts="urn:local-scripts">

  <xsl:import href="DeviceFilter.xslt"/>

  <xsl:output method="xml"/>

  <!-- Enable device-specific filtering for select elements -->
  <xsl:template match="QandASet|ItemizedList|OrderedList|Para">
    <xsl:apply-templates select="." mode="ApplyDeviceFilter"/>
  </xsl:template>


  <xsl:template match="QandASet" mode="deviceFiltered">
		<xsl:element name="a">
			<xsl:attribute name="name">Section<xsl:value-of select="@id"/></xsl:attribute>
		</xsl:element>
		
		<xsl:element name="a">
			<xsl:attribute name="name">ListSection</xsl:attribute>
		</xsl:element>	
		
		<xsl:if test="MarkedUpTitle !=''">
			<p><Span Class="QandA-Title"><xsl:value-of select="MarkedUpTitle"/></Span></p>
		</xsl:if>
		
		<xsl:element name="OL">
			<xsl:attribute name="class">QandA-List</xsl:attribute>
			<xsl:for-each select="QandAEntry">
				<xsl:element name="LI">
					<xsl:attribute name="__id"><xsl:value-of select="@id"/></xsl:attribute>
					<xsl:attribute name="class">QandA-ListItem</xsl:attribute>
					<xsl:element name="a">
						<xsl:attribute name="name">Section<xsl:value-of select="@id"/></xsl:attribute>
					</xsl:element>
					<xsl:apply-templates/>
				</xsl:element>
			</xsl:for-each>
		</xsl:element>
	
		<xsl:element name="a">
			<xsl:attribute name="name">END_ListSection</xsl:attribute>
		</xsl:element>
	</xsl:template>
	
	<xsl:template match="Question">
		<span class="QandA-Question"><xsl:apply-templates/></span>
	</xsl:template>
	
	<xsl:template match="Answer">
		<span class="QandA-Answer"><xsl:apply-templates/></span>
	</xsl:template>
	
	<xsl:template match="ItemizedList" mode="deviceFiltered">
		<xsl:element name="a">
			<xsl:attribute name="name">Section<xsl:value-of select="@id"/></xsl:attribute>
		</xsl:element>
	
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
	
	<xsl:template match="OrderedList" mode="deviceFiltered">
		<xsl:element name="a">
			<xsl:attribute name="name">Section<xsl:value-of select="@id"/></xsl:attribute>
		</xsl:element>
	
		<xsl:if test="ListTitle !=''">
			<p><Span Class="Protocol-OL-Title"><xsl:value-of select="ListTitle"/></Span></p>
		</xsl:if>
		
		<xsl:element name="OL">
			<xsl:attribute name="__id"><xsl:value-of select="@id"/></xsl:attribute>
			<xsl:attribute name="class">Protocol-OL</xsl:attribute>
			<xsl:choose>
				<xsl:when test="@Style='LRoman'">
					<xsl:apply-templates>
						<xsl:with-param name="ListType">Protocol-OL-LRoman</xsl:with-param>
					</xsl:apply-templates>
				</xsl:when>
				<xsl:when test="@Style='URoman'">
					<xsl:apply-templates>
						<xsl:with-param name="ListType">Protocol-OL-URoman</xsl:with-param>
					</xsl:apply-templates>
				</xsl:when>
				<xsl:when test="@Style='LAlpha'">
					<xsl:apply-templates>
						<xsl:with-param name="ListType">Protocol-OL-LAlpha</xsl:with-param>
					</xsl:apply-templates>
				</xsl:when>
				<xsl:when test="@Style='UAlpha'">
					<xsl:apply-templates>
						<xsl:with-param name="ListType">Protocol-OL-UAlpha</xsl:with-param>
					</xsl:apply-templates>
				</xsl:when>
				<xsl:when test="@Style='Arabic'">
					<xsl:apply-templates>
						<xsl:with-param name="ListType">Protocol-OL-Arabic</xsl:with-param>
					</xsl:apply-templates>
				</xsl:when>
				<xsl:otherwise>
					<xsl:apply-templates>
						<xsl:with-param name="ListType">Protocol-OL-Arabic</xsl:with-param>
					</xsl:apply-templates>
				</xsl:otherwise>					
			</xsl:choose>
		</xsl:element>
	</xsl:template>
		
	<xsl:template match="KeyPointsList[@Style='bullet']">
		<ul>
			<xsl:for-each select="ListItem">
				<li Class="Protocol-KeyPoints-Bullet">
					<a href="#{translate(substring(.,1,15),' ','-')}">
					<xsl:apply-templates/></a>
				</li>
			</xsl:for-each>
		</ul>
    </xsl:template>

    <xsl:template match="KeyPointsList[@Style='dash']">
		<ul>
			<xsl:for-each select="ListItem">
				<li Class="Protocol-KeyPoints-Dash">
					<a href="#{translate(substring(.,1,15),' ','-')}">
					<xsl:apply-templates/></a>
				</li>
			</xsl:for-each>
		</ul>
    </xsl:template>
	
	<xsl:template match="ListTitle"></xsl:template>
	
	<xsl:template match="ListItem">
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
		
	<xsl:template match="Para" mode="deviceFiltered">
    <a name="Section{@id}"></a>
		<P id="Section{@id}" __id="{@id}">
			<xsl:apply-templates/>
		</P>
	</xsl:template>
	
	<xsl:template match="TT">
		<xsl:element name="a">
			<xsl:attribute name="name">Section<xsl:value-of select="@id"/></xsl:attribute>
		</xsl:element>
		<PRE>
			<xsl:attribute name="__id"><xsl:value-of select="@id"/></xsl:attribute>
			<xsl:apply-templates/>
		</PRE>
	</xsl:template>
		
	<xsl:template match="SummaryRef">
		<xsl:element name="SummaryRef">
			<xsl:attribute name="href">
				<xsl:value-of select="@href" />
			</xsl:attribute>
			<!-- This block is for pretty url, uncomment it to enable prettyurl transform-->
			<xsl:attribute name="url">
				<xsl:value-of select="@url" />
			</xsl:attribute>
			<xsl:value-of select="." />
		</xsl:element>
	</xsl:template>
	<!--
	<xsl:template match="SummaryRef">
		<xsl:element name="A">
			<xsl:attribute name="Class">SummaryRef</xsl:attribute>
			<xsl:attribute name="href">
				<xsl:choose>
					<xsl:when test="contains(@href, '#')">	
						<xsl:choose>
							<xsl:when test="number(substring-after(/Summary/@id,'CDR')) = number(substring-before(substring-after(@href,'CDR'), '#'))">#Section<xsl:value-of select="substring-after(@href,'#')"/></xsl:when>
							<xsl:otherwise>/templates/doc_pdq.aspx?cdrid=<xsl:value-of select="number(substring-before(substring-after(@href,'CDR'), '#'))"/>#Section<xsl:value-of select="substring-after(@href,'#')"/></xsl:otherwise>		
						</xsl:choose>
					</xsl:when>
					<xsl:otherwise>/templates/doc_pdq.aspx?cdrid=<xsl:value-of select="number(substring-after(@href,'CDR'))"/></xsl:otherwise>
				</xsl:choose>
			</xsl:attribute>
			<xsl:value-of select="."/>
		</xsl:element>
		<xsl:if test="following-sibling::node()">
			<xsl:for-each select="following-sibling::node()">
				<xsl:if test="name() !='' and position()=1">&#160;</xsl:if>
			</xsl:for-each>
		</xsl:if>
    </xsl:template>
    -->
	
	<xsl:template match="MediaRef">
		<xsl:element name="A">
			<xsl:attribute name="Class">SummaryRef</xsl:attribute>
			<xsl:attribute name="href">
				<xsl:choose>
					<xsl:when test="contains(@href, '#')">
						<xsl:choose>
							<xsl:when test="starts-with(@href, '#')">#Section<xsl:value-of select="substring-after(@href,'#')"/></xsl:when>	
							<xsl:otherwise>
								<xsl:choose>
									<xsl:when test="number(substring-after(/Summary/@id,'CDR')) = number(substring-before(substring-after(@href,'CDR'), '#'))">#Section<xsl:value-of select="substring-after(@href,'#')"/></xsl:when>
									<xsl:otherwise>/templates/doc_pdq.aspx?cdrid=<xsl:value-of select="number(substring-before(substring-after(@href,'CDR'), '#'))"/>#Section<xsl:value-of select="substring-after(@href,'#')"/></xsl:otherwise>		
								</xsl:choose>
							</xsl:otherwise>
						</xsl:choose>
					</xsl:when>
					<xsl:otherwise>/templates/doc_pdq.aspx?cdrid=<xsl:value-of select="number(substring-after(@href,'CDR'))"/></xsl:otherwise>
				</xsl:choose>
			</xsl:attribute>
			<xsl:value-of select="."/>
		</xsl:element>
		<xsl:if test="following-sibling::node()">
			<xsl:for-each select="following-sibling::node()">
				<xsl:if test="name() !='' and position()=1">&#160;</xsl:if>
			</xsl:for-each>
		</xsl:if>
    </xsl:template>

	
	<xsl:variable name="ReturnToTopBar">
		<table border="0" cellpadding="1" cellspacing="0" width="100%" bgcolor="#333366">
			<tr>
				<td>
					<table border="0" cellpadding="1" cellspacing="0" width="100%" bgcolor="#f1f1e7">
						<tr>
							<td>
								<div align="right"><a href="#top"><img src="http://www.cancer.gov/images/return_to_top.gif" width="69" height="12" border="0" alt="return to top"/></a></div>
							</td>
						</tr>
					</table>
				</td>
			</tr>
		</table>
	</xsl:variable>

	<xsl:template match="Subscript">
		<sub>
			<xsl:apply-templates/>
		</sub>
	</xsl:template>
	
	<xsl:template match="Superscript">
		<sup>
			<xsl:apply-templates/>
		</sup>
	</xsl:template>
	
	<xsl:template match="Strong">
		<b><xsl:apply-templates/></b>
	</xsl:template>
	    
	<xsl:template match="Emphasis">
		<i><xsl:apply-templates/></i>
    </xsl:template>
    
    <xsl:template match="ScientificName">
		<Span Class="Protocol-ScientificName"><xsl:apply-templates/></Span>
	</xsl:template>
	
	<xsl:template match="ForeignWord">
		<Span Class="Protocol-ForeignWord"><xsl:apply-templates/></Span>
    </xsl:template>
    
    <xsl:template match="GeneName">
		<Span Class="Protocol-GeneName"><xsl:apply-templates/></Span>
    </xsl:template>
    
    <xsl:template match="Note">                
		<xsl:text>&#xa0;[</xsl:text>
		<Span Class="Protocol-Note">
			<xsl:choose>
				<xsl:when test="/Summary/SummaryMetaData/SummaryLanguage='Spanish'">
					<xsl:text>Nota: </xsl:text>
				</xsl:when>
				<xsl:otherwise>
					<xsl:text>Note: </xsl:text>
				</xsl:otherwise>
			</xsl:choose>
			<xsl:apply-templates/>
        </Span>
        <xsl:text>]</xsl:text>
	</xsl:template>
	
	<xsl:template match="ExternalRef">
		<xsl:element name="a">
			<xsl:attribute name="Class">Protocol-ExternalRef</xsl:attribute>
			<xsl:attribute name="href">
				<xsl:if test="starts-with(@xref, 'http') = false()">Http://</xsl:if>
				<xsl:value-of select="@xref"/>
			</xsl:attribute>
			<xsl:value-of select="."/>
		</xsl:element>
	</xsl:template>
	
</xsl:stylesheet>

  
