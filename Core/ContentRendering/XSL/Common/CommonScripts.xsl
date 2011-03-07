<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
							  xmlns:msxsl="urn:schemas-microsoft-com:xslt"
							  xmlns:scripts="urn:local-scripts">

	<msxsl:script language="C#" implements-prefix="scripts">
		<![CDATA[

		//public string truncLine(string protocolField)
		//{			
		//	return CancerGov.Common.Functions.TruncLine(protocolField, 80, false, false);
		//}
		
		public string truncLine(string line, int lineSize, string forceWrap, string htmlLineBreak)
		{
			string result = "";
			
			string lineBreak = "\n";
			int wrapIndex = 0;

			if(htmlLineBreak=="true")
			{
				lineBreak = "<BR>";
			}

			if(line != null && line.Trim().Length > 0)
			{
				line = line.Trim();
				while(line.Length > lineSize)
				{
					//Determine wrap index
					wrapIndex = line.LastIndexOf("\n", lineSize);
					if(wrapIndex == -1)
					{
						wrapIndex = line.LastIndexOf(" ", lineSize);
						if(wrapIndex == -1)
						{
							if(forceWrap=="true")
							{
								wrapIndex = lineSize;
							}
							else
							{
								wrapIndex = line.IndexOf("\n");
								if(wrapIndex == -1)
								{
									wrapIndex = line.IndexOf(" ");
									if(wrapIndex == -1)
									{
										wrapIndex = line.Length;
									}
								}								
							}
						}						
					}
					
					//build return value and truncate line
					result += line.Substring(0, wrapIndex + 1) + lineBreak;
					line = line.Remove(0, wrapIndex).Trim();
				}
					
				result += line;
			}

/*			
			foreach(char dig in line.ToCharArray())
			{
				if(Convert.ToInt16(dig) < 32 || Convert.ToInt16(dig) > 126)
				{
					result += "[" + Convert.ToInt16(dig).ToString() + "]";
				}
				else
				{
					result += dig.ToString();
				}
			}
*/		
			return result;
		}
		]]>
	</msxsl:script>
</xsl:stylesheet>

  