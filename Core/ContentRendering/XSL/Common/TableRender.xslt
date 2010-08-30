<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
>
    <xsl:output method="xml" indent="yes"/>
 
    <xsl:template name="TableRender">

      <!-- CSS class to use for each cell in the table.
           If no value is supplied, cells will be rendered without
           a class attribute. -->
      <xsl:param name="table-cell-class" />

      <xsl:variable name="tframe">
        <xsl:choose>
          <xsl:when test="@Frame ='All'">border</xsl:when>
          <xsl:when test="@Frame ='Bottom'">below</xsl:when>
          <xsl:when test="@Frame ='Sides'">vsides</xsl:when>
          <xsl:when test="@Frame ='Top'">above</xsl:when>
          <xsl:when test="@Frame ='TopBot'">hsides</xsl:when>
          <xsl:when test="@Frame ='None'">void</xsl:when>
          <xsl:otherwise>border</xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:variable name="border">
        <xsl:choose>
          <xsl:when test="@Frame ='None'">0</xsl:when>
          <xsl:otherwise>1</xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:variable name="trules">
        <xsl:choose>
          <xsl:when test="@ColSep !='0' and @RowSep !='0'">all</xsl:when>
          <xsl:when test="@ColSep ='0' and @RowSep ='0'">none</xsl:when>
          <xsl:when test="@ColSep !='0'">cols</xsl:when>
          <xsl:when test="@RowSep !='0'">rows</xsl:when>
          <xsl:otherwise>
            <xsl:if test="@Frame !='None'">all</xsl:if>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      
      <p />
      <table Rules="{$trules}" Frame="{$tframe}" Border="{$border}" valign="top" cellspacing="0" cellpadding="5" width="100%">
        <xsl:if test="Title !=''">
          <xsl:for-each select="Title">
            <span class="Summary-Table-Caption">
              <b>
                <xsl:apply-templates/>
              </b>
            </span>
          </xsl:for-each>
        </xsl:if>
        <div name="EnlargePlaceholder" align="right"></div>
        <COLGROUP>
          <xsl:for-each select="TGroup/ColSpec">
            <xsl:variable name="cellength">
              <xsl:choose>
                <xsl:when test="contains(@ColWidth, '*')">
                  <xsl:choose>
                    <xsl:when test="string-length(@ColWidth)=1">1*</xsl:when>
                    <xsl:when test="contains(@ColWidth, '.')">1*</xsl:when>
                    <xsl:otherwise>
                      <xsl:value-of select="@ColWidth"/>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="@ColWidth"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>

            <xsl:element name="COL">
              <xsl:attribute name="Width">
                <xsl:value-of select="$cellength"/>
              </xsl:attribute>
              <!-- TODO: REMOVE - following line is added for string comparison purpose-->
              <!--xsl:element name="Replace"/-->
            </xsl:element>
          </xsl:for-each>
        </COLGROUP>

        <!--table head-->
        <xsl:if test="count(TGroup/THead/Row) &gt; 0">
          <THEAD>
            <xsl:for-each select="TGroup/THead/Row">
              <tr class="Summary-Table-Head">
                <xsl:for-each select="entry">
                  <xsl:variable name="span3">
                    <xsl:choose>
                      <xsl:when test="@NameEnd !='' and @NameSt !=''">
                        <xsl:value-of select="substring(@NameEnd,4) - substring(@NameSt,4) + 1"/>
                      </xsl:when>
                      <xsl:otherwise>1</xsl:otherwise>
                    </xsl:choose>
                  </xsl:variable>

                  <xsl:variable name="hvalign">
                    <xsl:choose>
                      <xsl:when test="@Valign !=''">
                        <xsl:value-of select="@Valign"/>
                      </xsl:when>
                      <xsl:otherwise>Middle</xsl:otherwise>
                    </xsl:choose>
                  </xsl:variable>

                  <xsl:variable name="halign">
                    <xsl:choose>
                      <xsl:when test="@Align !=''">
                        <xsl:value-of select="@Align "/>
                      </xsl:when>
                      <xsl:otherwise>Center</xsl:otherwise>
                    </xsl:choose>
                  </xsl:variable>

                  <td colspan="{$span3}" align="{$halign}" Valign="{$hvalign}">
                    <xsl:if test="$table-cell-class"><!-- Summary-SummarySection-Small -->
                      <xsl:attribute name="class"><xsl:value-of select="$table-cell-class"/></xsl:attribute>
                    </xsl:if>
                    <b>
                      <xsl:apply-templates/>
                    </b>&#160;
                  </td>
                </xsl:for-each>
              </tr>
            </xsl:for-each>
          </THEAD>
        </xsl:if>
        <!-- Foot without row separation -->

        <!--xsl:if test="not(TGroup/TFoot/Row/entry/@RowSep)"-->
        <xsl:if test="count(TGroup/TFoot/Row/entry) =1">
          <TFOOT>
            <xsl:for-each select="TGroup/TFoot/Row">
              <tr>
                <xsl:for-each select="entry">
                  <xsl:variable name="span" select="substring(@NameEnd,4) - substring(@NameSt,4) + 1"/>

                  <xsl:variable name="footvalign">
                    <xsl:choose>
                      <xsl:when test="@Valign !=''">
                        <xsl:value-of select="@Valign"/>
                      </xsl:when>
                      <xsl:otherwise>Top</xsl:otherwise>
                    </xsl:choose>
                  </xsl:variable>

                  <xsl:variable name="footalign">
                    <xsl:choose>
                      <xsl:when test="@Align !=''">
                        <xsl:value-of select="@Align "/>
                      </xsl:when>
                      <xsl:otherwise>Left</xsl:otherwise>
                    </xsl:choose>
                  </xsl:variable>

                  <xsl:choose>
                    <xsl:when test="$footalign = 'Left'">
                      <td colspan="{$span}" Valign="{$footvalign}" Align="{$footalign}">
                        <xsl:if test="$table-cell-class"><!-- Summary-SummarySection-Small -->
                          <xsl:attribute name="class"><xsl:value-of select="$table-cell-class"/></xsl:attribute>
                        </xsl:if>
                        <i>
                          <xsl:apply-templates/>
                        </i>
                      </td>
                    </xsl:when>
                    <xsl:otherwise>
                      <td colspan="{$span}" Valign="{$footvalign}" Align="{$footalign}">
                        <xsl:if test="$table-cell-class"><!-- Summary-SummarySection-Small -->
                          <xsl:attribute name="class"><xsl:value-of select="$table-cell-class"/></xsl:attribute>
                        </xsl:if>
                        <i>
                          <xsl:apply-templates/>
                        </i>
                      </td>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:for-each>
              </tr>
            </xsl:for-each>
          </TFOOT>
        </xsl:if>


        <!-- table body after table footer -->
        <TBODY>
          <xsl:for-each select="TGroup/TBody/Row">

            <xsl:variable name="LastRow">
              <!-- Define last row which won't have to deal with td's border-bottom  -->
              <xsl:if test="following-sibling::node()">
                <xsl:for-each select="following-sibling::node()">
                  <xsl:if test="name()='Row' and position()=last()">
                    <xsl:value-of select="name()"/>
                  </xsl:if>
                </xsl:for-each>
              </xsl:if>
            </xsl:variable>

            <xsl:variable name="CurrentRowPosition">
              <!-- Define current row position -->
              <xsl:value-of select="position()"/>
            </xsl:variable>

            <xsl:choose>
              <xsl:when test="$LastRow ='Row'">
                <!-- When a row is the last row -->
                <tr>
                  <xsl:for-each select="entry">
                    <xsl:variable name="spanb">
                      <!-- Define column span-->
                      <xsl:choose>
                        <xsl:when test="@NameEnd !='' and @NameSt !=''">
                          <xsl:value-of select="substring(@NameEnd,4) - substring(@NameSt,4) + 1"/>
                        </xsl:when>
                        <xsl:otherwise>1</xsl:otherwise>
                      </xsl:choose>
                    </xsl:variable>

                    <xsl:variable name="EndCol">
                      <!-- Define column span's end column-->
                      <xsl:if test="@NameEnd !=''">
                        <xsl:value-of select="@NameEnd"/>
                      </xsl:if>
                    </xsl:variable>

                    <xsl:variable name="bvalign">
                      <!-- Define cell vertical alignment-->
                      <xsl:choose>
                        <xsl:when test="@Valign !=''">
                          <xsl:value-of select="@Valign"/>
                        </xsl:when>
                        <xsl:otherwise>Top</xsl:otherwise>
                      </xsl:choose>
                    </xsl:variable>

                    <xsl:variable name="balign">
                      <!-- Define cell alignment -->
                      <xsl:choose>
                        <xsl:when test="@Align !=''">
                          <xsl:value-of select="@Align "/>
                        </xsl:when>
                        <xsl:otherwise>Left</xsl:otherwise>
                      </xsl:choose>
                    </xsl:variable>

                    <xsl:variable name="browspan">
                      <!-- Define row span-->
                      <xsl:choose>
                        <xsl:when test="@MoreRows !=''">
                          <xsl:value-of select="@MoreRows + 1 "/>
                        </xsl:when>
                        <xsl:otherwise>1</xsl:otherwise>
                      </xsl:choose>
                    </xsl:variable>

                    <xsl:variable name="ColumnColSpe">
                      <!-- If colspan is not 0, select the corresponding rightmost colspec if its colsep atribute exits-->
                      <xsl:if test="$spanb !=0 ">
                        <xsl:for-each select="../../../ColSpec">
                          <xsl:if test="@ColName= $EndCol and @ColSep !=''">
                            <xsl:value-of select="@ColSep"/>
                          </xsl:if>
                        </xsl:for-each>
                      </xsl:if>
                    </xsl:variable>

                    <xsl:variable name="GridLine">
                      <!-- Define style for table cell's right border -->
                      <xsl:choose>
                        <xsl:when test="@ColSep !=''">
                          <xsl:if test="@ColSep ='0'">0</xsl:if>
                          <xsl:if test="@ColSep ='1'">1</xsl:if>
                        </xsl:when>
                        <xsl:otherwise>
                          <xsl:choose>
                            <xsl:when test="$ColumnColSpe !='' ">
                              <xsl:value-of select="$ColumnColSpe"/>
                            </xsl:when>
                            <xsl:otherwise>
                              <xsl:choose>
                                <xsl:when test="../../../@ColSep !=''">
                                  <xsl:value-of select="../../../@ColSep"/>
                                </xsl:when>
                                <xsl:otherwise>
                                  <xsl:choose>
                                    <xsl:when test="../../../../@ColSep !=''">
                                      <xsl:value-of select="../../../../@ColSep"/>
                                    </xsl:when>
                                    <xsl:otherwise>1</xsl:otherwise>
                                  </xsl:choose>
                                </xsl:otherwise>
                              </xsl:choose>
                            </xsl:otherwise>
                          </xsl:choose>
                        </xsl:otherwise>
                      </xsl:choose>
                    </xsl:variable>

                    <xsl:variable name="CellStyle">
                      <!-- Define class for right border -->
                      <xsl:if test="$GridLine =0">NoRight</xsl:if>
                      <xsl:if test="$GridLine =1">Border</xsl:if>
                    </xsl:variable>

                    <td rowspan="{$browspan}" colspan="{$spanb}" align="{$balign}" Valign="{$bvalign}">
                      <xsl:if test="$table-cell-class"><!-- Summary-SummarySection-Small -->
                        <xsl:attribute name="class"><xsl:value-of select="$table-cell-class"/></xsl:attribute>
                      </xsl:if>
                      <xsl:apply-templates/>
                    </td>
                  </xsl:for-each>
                </tr>
              </xsl:when>
              <xsl:otherwise>
                <!-- Not last row -->
                <tr>
                  <xsl:for-each select="entry">
                    <xsl:variable name="spanb">
                      <xsl:choose>
                        <xsl:when test="@NameEnd !='' and @NameSt !=''">
                          <xsl:value-of select="substring(@NameEnd,4) - substring(@NameSt,4) + 1"/>
                        </xsl:when>
                        <xsl:otherwise>1</xsl:otherwise>
                      </xsl:choose>
                    </xsl:variable>

                    <xsl:variable name="EndCol">
                      <xsl:if test="@NameEnd !=''">
                        <xsl:value-of select="@NameEnd"/>
                      </xsl:if>
                    </xsl:variable>

                    <xsl:variable name="ColPosition">
                      <xsl:value-of select="position()"/>
                    </xsl:variable>

                    <xsl:variable name="bvalign">
                      <xsl:choose>
                        <xsl:when test="@Valign !=''">
                          <xsl:value-of select="@Valign"/>
                        </xsl:when>
                        <xsl:otherwise>Top</xsl:otherwise>
                      </xsl:choose>
                    </xsl:variable>

                    <xsl:variable name="balign">
                      <xsl:choose>
                        <xsl:when test="@Align !=''">
                          <xsl:value-of select="@Align "/>
                        </xsl:when>
                        <xsl:otherwise>Left</xsl:otherwise>
                      </xsl:choose>
                    </xsl:variable>

                    <xsl:variable name="browspan">
                      <xsl:choose>
                        <xsl:when test="@MoreRows !=''">
                          <xsl:value-of select="@MoreRows + 1 "/>
                        </xsl:when>
                        <xsl:otherwise>1</xsl:otherwise>
                      </xsl:choose>
                    </xsl:variable>

                    <xsl:variable name="ColumnColSpe">
                      <xsl:if test="$spanb !=0 ">
                        <xsl:for-each select="../../../ColSpec">
                          <xsl:if test="@ColName= $EndCol and @ColSep !=''">
                            <xsl:value-of select="@ColSep"/>
                          </xsl:if>
                        </xsl:for-each>
                      </xsl:if>
                    </xsl:variable>

                    <xsl:variable name="GridLine">
                      <xsl:choose>
                        <xsl:when test="@ColSep !=''">
                          <xsl:if test="@ColSep ='0'">0</xsl:if>
                          <xsl:if test="@ColSep ='1'">1</xsl:if>
                        </xsl:when>
                        <xsl:otherwise>
                          <xsl:choose>
                            <xsl:when test="$ColumnColSpe !='' ">
                              <xsl:value-of select="$ColumnColSpe"/>
                            </xsl:when>
                            <xsl:otherwise>
                              <xsl:choose>
                                <xsl:when test="../../../@ColSep !=''">
                                  <xsl:value-of select="../../../@ColSep"/>
                                </xsl:when>
                                <xsl:otherwise>
                                  <xsl:choose>
                                    <xsl:when test="../../../../@ColSep !=''">
                                      <xsl:value-of select="../../../../@ColSep"/>
                                    </xsl:when>
                                    <xsl:otherwise>1</xsl:otherwise>
                                  </xsl:choose>
                                </xsl:otherwise>
                              </xsl:choose>
                            </xsl:otherwise>
                          </xsl:choose>
                        </xsl:otherwise>
                      </xsl:choose>
                    </xsl:variable>

                    <xsl:variable name="RowRowSpe">
                      <xsl:if test="$browspan !=0 ">
                        <xsl:for-each select="../../Row">
                          <xsl:if test="position() = $CurrentRowPosition + $browspan -1 and @RowSep !=''">
                            <xsl:value-of select="@RowSep"/>
                          </xsl:if>
                        </xsl:for-each>
                      </xsl:if>
                    </xsl:variable>

                    <xsl:variable name="RowColSpe">
                      <xsl:if test="$spanb !=0 ">
                        <xsl:for-each select="../../../ColSpec">
                          <xsl:if test="@ColName= $EndCol and @RowSep !=''">
                            <xsl:value-of select="@RowSep"/>
                          </xsl:if>
                        </xsl:for-each>
                      </xsl:if>
                    </xsl:variable>

                    <xsl:variable name="BottomLine">
                      <xsl:choose>
                        <xsl:when test="@RowSep !=''">
                          <xsl:if test="@RowSep ='0'">0</xsl:if>
                          <xsl:if test="@RowSep ='1'">1</xsl:if>
                        </xsl:when>
                        <xsl:otherwise>
                          <xsl:choose>
                            <xsl:when test="$RowRowSpe !=''">
                              <xsl:value-of select="$RowRowSpe"/>
                            </xsl:when>
                            <xsl:otherwise>
                              <xsl:choose>
                                <xsl:when test="$RowColSpe !='' ">
                                  <xsl:value-of select="$RowColSpe"/>
                                </xsl:when>
                                <xsl:otherwise>
                                  <xsl:choose>
                                    <xsl:when test="../../../@RowSep !=''">
                                      <xsl:value-of select="../../../@RowSep"/>
                                    </xsl:when>
                                    <xsl:otherwise>
                                      <xsl:choose>
                                        <xsl:when test="../../../../@RowSep !=''">
                                          <xsl:value-of select="../../../../@RowSep"/>
                                        </xsl:when>
                                        <xsl:otherwise>1</xsl:otherwise>
                                      </xsl:choose>
                                    </xsl:otherwise>
                                  </xsl:choose>
                                </xsl:otherwise>
                              </xsl:choose>
                            </xsl:otherwise>
                          </xsl:choose>
                        </xsl:otherwise>
                      </xsl:choose>
                    </xsl:variable>

                    <xsl:variable name="CellStyle">
                      <xsl:if test="$BottomLine !=0 and $GridLine !=0">Border</xsl:if>
                      <xsl:if test="$BottomLine  =0 and $GridLine !=0">NoBottom</xsl:if>
                      <xsl:if test="$BottomLine !=0 and $GridLine =0">NoRight</xsl:if>
                      <xsl:if test="$BottomLine  =0 and $GridLine =0">NoRightBottom</xsl:if>
                    </xsl:variable>

                    <xsl:variable name="LastCellStyle">
                      <xsl:if test="$BottomLine =0">NoBottom</xsl:if>
                      <xsl:if test="$BottomLine =1">Bottom</xsl:if>
                    </xsl:variable>

                    <xsl:choose>
                      <xsl:when test="position()=last()">
                        <!--Last Cell -->
                        <td rowspan="{$browspan}" colspan="{$spanb}" align="{$balign}" Valign="{$bvalign}">
                          <xsl:if test="$table-cell-class"><!-- Summary-SummarySection-Small -->
                            <xsl:attribute name="class"><xsl:value-of select="$table-cell-class"/></xsl:attribute>
                          </xsl:if>
                          <xsl:apply-templates/>
                        </td>
                      </xsl:when>
                      <xsl:otherwise>
                        <td rowspan="{$browspan}" colspan="{$spanb}" align="{$balign}" Valign="{$bvalign}">
                          <xsl:if test="$table-cell-class"><!-- Summary-SummarySection-Small -->
                            <xsl:attribute name="class"><xsl:value-of select="$table-cell-class"/></xsl:attribute>
                          </xsl:if>
                          <xsl:apply-templates/>
                        </td>
                      </xsl:otherwise>
                    </xsl:choose>
                  </xsl:for-each>
                </tr>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:for-each>
        </TBODY>
      </table>
      <br />

      <!-- Foot with row separation -->
      <xsl:if test="count(TGroup/TFoot/Row/entry) > 1">
        <table border="0" width="100%">
          <xsl:for-each select="TGroup/TFoot/Row">
            <tr>
              <xsl:for-each select="entry">
                <xsl:variable name="span" select="substring(@NameEnd,4) - substring(@NameSt,4) + 1"/>
                <xsl:variable name="footvalign">
                  <xsl:choose>
                    <xsl:when test="@Valign !=''">
                      <xsl:value-of select="@Valign"/>
                    </xsl:when>
                    <xsl:otherwise>Top</xsl:otherwise>
                  </xsl:choose>
                </xsl:variable>

                <xsl:variable name="footalign">
                  <xsl:choose>
                    <xsl:when test="@Align !=''">
                      <xsl:value-of select="@Align "/>
                    </xsl:when>
                    <xsl:otherwise>Left</xsl:otherwise>
                  </xsl:choose>
                </xsl:variable>

                <td colspan="{$span}"  Valign="{$footvalign}" Align="{$footalign}">
                  <xsl:if test="$table-cell-class"><!-- Summary-SummarySection-Small -->
                    <xsl:attribute name="class"><xsl:value-of select="$table-cell-class"/></xsl:attribute>
                  </xsl:if>
                  <i>
                    <xsl:apply-templates/>
                  </i>
                </td>
              </xsl:for-each>
            </tr>
          </xsl:for-each>
        </table>
        <Br />
      </xsl:if>
    </xsl:template>
</xsl:stylesheet>
