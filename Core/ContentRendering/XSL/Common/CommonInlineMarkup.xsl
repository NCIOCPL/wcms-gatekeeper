<?xml version="1.0"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
			                  xmlns:cdr="cips.nci.nih.gov/cdr">
	<!--written by C.Burg 7/30/2002 -->
	
	<!-- ====================================================================
                    Display a Summary Reference
    =================================================================== -->

    <xsl:template match="SummaryRef">
		<u><xsl:value-of select="."/></u>
    </xsl:template>

	<!-- =========================================================
    Link to glossaryTermRefs 
    =========================================================-->
	<xsl:template match="GlossaryTermRef">
		<a>
			<xsl:attribute name="href">
				<xsl:value-of select="concat('/cgi-bin/cdr/Filter.py?DocId=',@cdr:href,'&amp;Filter=name:Glossary+Term+Display+for+Patient+Summaries')"/>
			</xsl:attribute>
			<xsl:value-of select="."/>
		</a>
	</xsl:template>

	<!--======================================================================
    Display an ordered list 
    ===================================================================== -->
	<xsl:template match="OrderedList[@Style='Arabic']">
		<b><i><xsl:value-of select="ListTitle"/></i></b>
		<ol>
			<xsl:for-each select="ListItem">
				<li><xsl:apply-templates/></li><br />
			</xsl:for-each>
		</ol>
	</xsl:template>

	<!--======================================================================
                       Display an ordered list Style-UAlpha
    ===================================================================== -->
	<xsl:template match="OrderedList[@Style='UAlpha']">
		<b><i><xsl:value-of select="ListTitle"/></i></b>
		<ol>
			<xsl:for-each select="ListItem">
				<li type='A'><xsl:apply-templates/></li><br />
			</xsl:for-each>
		</ol>
	</xsl:template>

	<!--======================================================================
                       Display an ordered list Style-LAlpha
    ===================================================================== -->
	<xsl:template match="OrderedList[@Style='LAlpha']">
		<b><i><xsl:value-of select="ListTitle"/></i></b>
		<ol>
			<xsl:for-each select="ListItem">
				<li type='a'><xsl:apply-templates/></li><br />
			</xsl:for-each>
		</ol>
	</xsl:template>

	<!--======================================================================
    Display an ordered list Style-URoman
    ===================================================================== -->
	<xsl:template match="OrderedList[@Style='URoman']">
		<b><i><xsl:value-of select="ListTitle"/></i></b>
		<ol>
			<xsl:for-each select="ListItem">
				<li type='I'><xsl:apply-templates/></li><br />
			</xsl:for-each>
		</ol>
	</xsl:template>

	<!--======================================================================
    Display an ordered list Style-LRoman
    ===================================================================== -->
	<xsl:template match="OrderedList[@Style='LRoman']">
		<b><i><xsl:value-of select="ListTitle"/></i></b>
		<ol>
			<xsl:for-each select="ListItem">
				<li type='i'><xsl:apply-templates/></li><br />
			</xsl:for-each>
		</ol>
	</xsl:template>

	<!-- ===================================================================
                Display Itemized Lists
    ================================================================== -->
     
    <!-- ===================================================================
                Display Itemized Lists for KeyPoints
     ================================================================== -->      
              
    <xsl:template match="KeyPointsList[@Style='bullet']">
		<ul>
			<xsl:for-each select="ListItem">
				<li type='disc'>
					<a href="#{translate(substring(.,1,15),' ','-')}">
					<xsl:apply-templates/></a>
				</li>
			</xsl:for-each>
		</ul>
    </xsl:template>

    <xsl:template match="KeyPointsList[@Style='dash']">
		<ul>
			<xsl:for-each select="ListItem">
				<li type='square'>
					<a href="#{translate(substring(.,1,15),' ','-')}">
					<xsl:apply-templates/></a>
				</li>
			</xsl:for-each>
		</ul>
    </xsl:template>
                       
	<!-- ===================================================================
                Display Itemized Lists for text 
    ================================================================== -->                   
	<xsl:template match="ItemizedList[@Style='bullet']">
		<xsl:if test="ListTitle">
			<br />
			<b><i><xsl:value-of select="ListTitle"/></i></b>
        </xsl:if>
        <xsl:choose>
			<xsl:when test="@Compact">
				<ul>
					<xsl:for-each select="ListItem">
						<li type='disc'><xsl:apply-templates/></li>
						<xsl:if test="position() !=last()"><br /><br /></xsl:if>
					</xsl:for-each>
				</ul>
			</xsl:when>
            <xsl:otherwise>
				<ul>
					<xsl:for-each select="ListItem">
						<li type='disc'><xsl:apply-templates/></li>
						<xsl:if test="position() !=last()"><br /></xsl:if>
					</xsl:for-each>
				</ul>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>    

	<xsl:template match="ItemizedList[@Style='dash']">
		<xsl:if test="ListTitle">
			<br />
			<b><i><xsl:value-of select="ListTitle"/></i></b>
		</xsl:if>
           
        <xsl:choose>
			<xsl:when test="@Compact">
				<ul>
					<xsl:for-each select="ListItem">
						<li type='square'><xsl:apply-templates/></li>
						<xsl:if test="position() !=last()"><br /><br /></xsl:if>
					</xsl:for-each>
				</ul>
            </xsl:when>
			<xsl:otherwise>
				<ul>
					<xsl:for-each select="ListItem">
						<li type='square'><xsl:apply-templates/></li>
						<xsl:if test="position() !=last()"><!--<br />--></xsl:if>
					</xsl:for-each>
				</ul>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

    <xsl:template match="ItemizedList[@Style='simple']">
		<xsl:if test="ListTitle">
			<br />
			<b><i><xsl:value-of select="ListTitle"/></i></b>
		</xsl:if>
           
        <xsl:choose>
			<xsl:when test="@Compact">
				<ul>
					<xsl:for-each select="ListItem">
						<li class='lnone'><xsl:apply-templates/></li>
						<xsl:if test="position() !=last()"><br /><br /></xsl:if>
					</xsl:for-each>
				</ul>
			</xsl:when>
			<xsl:otherwise>
				<ul>
					<xsl:for-each select="ListItem">
						<li class='lnone'><xsl:apply-templates/></li>
						<xsl:if test="position() !=last()"><br /></xsl:if>
					</xsl:for-each>
				</ul>
            </xsl:otherwise>
		</xsl:choose>
	</xsl:template>
       
    <xsl:template match="ItemizedList">
		<xsl:if test="ListTitle">
			<b><i><xsl:value-of select="ListTitle"/></i></b>
		</xsl:if>
        <xsl:choose>
			<xsl:when test="@Compact">
				<ul>
					<xsl:for-each select="ListItem">
						<li class='lnone'><xsl:apply-templates/></li>
						<xsl:if test="position() !=last()"><br /><br />
						</xsl:if>
		            </xsl:for-each>
				</ul>
            </xsl:when>
            <xsl:otherwise>
				<ul>
					<xsl:for-each select="ListItem">
						<li class='lnone'><xsl:apply-templates/></li>
						<xsl:if test="position() !=last()"><br /></xsl:if>
					</xsl:for-each>
				</ul>
            </xsl:otherwise>
		</xsl:choose>
	</xsl:template>        
 
   <!-- ==================================================================
                     Display a Subscript 
     =================================================================== -->

	<xsl:template match="Subscript">
		<sub>
			<xsl:apply-templates/>
		</sub>
	</xsl:template>

<!-- ==================================================================
                     Display a Superscript 
     =================================================================== -->

    <xsl:template match="Superscript">
		<sup>
			<xsl:apply-templates/>
		</sup>
	</xsl:template>

<!-- ==================================================================
                     Display Strong 
     =================================================================== -->

    <xsl:template match="Strong">
		<b><xsl:apply-templates/></b>
	</xsl:template>

<!-- ==================================================================
                     Display Emphasis 
     =================================================================== -->

    <xsl:template match="Emphasis">
		<i><xsl:apply-templates/></i>
    </xsl:template>

<!-- ==================================================================
              Display DrugName - No formatting required at this time
     =================================================================== 

              <xsl:template match="DrugName">
              <xsl:apply-templates/>
              </xsl:template>-->


<!-- ==================================================================
                     Display Scientific Name 
     =================================================================== -->

    <xsl:template match="ScientificName">
		<i><xsl:apply-templates/></i>
	</xsl:template>
<!-- ==================================================================
                     Display Foreign Word 
     =================================================================== -->

    <xsl:template match="ForeignWord">
		<i><xsl:apply-templates/></i>
    </xsl:template>

<!-- ==================================================================
                     Display Gene Name 
     =================================================================== -->

    <xsl:template match="GeneName">
		<i><xsl:apply-templates/></i>
    </xsl:template>

<!-- ============================================================ 
                         Display a note
      ============================================================ -->

	<xsl:template match="Note">                
		<xsl:text>&#xa0;[
		</xsl:text>
		<xsl:choose>
			<xsl:when test="/Summary/SummaryMetaData/SummaryLanguage='Spanish'">
				<i><xsl:text>Nota:</xsl:text></i>
            </xsl:when>
            <xsl:otherwise>
				<i><xsl:text>[Note:</xsl:text></i>
			</xsl:otherwise>
		</xsl:choose>
        <i><xsl:value-of select="."/></i>
        <xsl:text>]</xsl:text>
	</xsl:template>
</xsl:stylesheet>
