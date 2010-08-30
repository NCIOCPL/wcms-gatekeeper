<%@ Page language="c#" Codebehind="popImage.aspx.cs" AutoEventWireup="false" Inherits="www.Common.PopUps.popImage" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
  <head>
    <meta http-equiv="content-type" content="text/html;charset=UTF-8" />
    <link rel="stylesheet" href="http://www.cancer.gov/stylesheets/nci.css" type="text/css"/>
    <!--style type = "text/css">
.caption {
font-size : 11px; 
color : #4d4d4d; 
font-family : Arial, Verdana, Trebuchet MS, Tahoma, sans-serif; 
} 
.caption-image {
border-top-width : 0; 
border-right-width : 0; 
border-bottom-width : 1px; 
border-left-width : 0; 
border-top-style : solid; 
border-right-style : solid; 
border-bottom-style : solid; 
border-left-style : solid; 
border-bottom-color : #bdbdbd; 
border-top-color : #bdbdbd; 
border-right-color : #bdbdbd; 
border-left-color : #bdbdbd; 
border-color : #bdbdbd; 
} 
    </style-->
    <title>National Cancer Institute</title>
  </head>
  <body style="margin:0 0 0 0; padding:0 0 0 0"> 
  <table border="0" cellpadding="0" cellspacing="0" width="750" height="600" align="center">
  <% if (Caption!="") { %>
  <tr><td valign="middle" align="center">
    <table border="0" cellpadding="0" cellspacing="0">
    
    <tr><td align="center">
		<div class="caption-image">
		<table border="0" cellpadding="0" cellspacing="0">
			<tr>
				<td ><img border="0" src="<%=ImageName%>" /></td>
			</tr>
			<tr><td valign="top" height="10">
			</td></tr>
		</table>
		</div>
    </td></tr>
    <tr><td valign="top" height="3">
	</td></tr>
	
	<tr><td class="caption" align="center" valign="top"><%=Caption%> </td></tr>	
	
	</td></tr>
	</table>
	
 <% }else{ %>
		<tr>
			<td valign="middle" align="center"><img border="0" src="<%=ImageName%>" /></td>
		</tr>
 <% } %>
  </table>	
  </body>
</html>
