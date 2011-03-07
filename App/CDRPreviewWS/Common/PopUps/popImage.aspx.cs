using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Configuration;
using System.Text;
using System.Text.RegularExpressions;

namespace www.Common.PopUps
{
	/// <summary>
	/// Summary description for popImage.
	/// </summary>
	public class popImage : System.Web.UI.Page
	{
		private string imageName="";
		private string caption="";

		public string ImageName 
		{
			get { return imageName; }
		}

		public string Caption 
		{
			get { return caption; }
		}

		private void Page_Load(object sender, System.EventArgs e)
		{
			Object o=Request.Params["imageName"];
			if (o != null)
				imageName = o.ToString();
			if (imageName=="")
			{
				Response.Redirect(ConfigurationSettings.AppSettings["NotFoundPage"], true);
			}

			o=Request.Params["caption"];
            //if (o != null)
            //    caption = o.ToString();

            //This is a hack for SCR30157 and SCR30283, which IIS7 has some bugs in encoding non-ascii character.

            Encoding unicode = Encoding.UTF8;
            Encoding latin1Code = Encoding.GetEncoding(28591);

            // Convert the string into a byte[] from UTF8 to western european 
            byte[] unicodeBytes = unicode.GetBytes(Request.RawUrl.ToString());
            byte[] latinBytes = Encoding.Convert(unicode, latin1Code, unicodeBytes);
            string latincap = unicode.GetString(latinBytes); //Get back to Unicode string.

            //use regular expression to get caption out
            Regex r = new Regex(@"caption=(([^&]*))?");
            Match match = r.Match(latincap);

            latincap = HttpUtility.UrlDecode(match.Groups[1].Value);

            if (o != null)
                caption = latincap.ToString();
        }

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{    
			this.Load += new System.EventHandler(this.Page_Load);
		}
		#endregion
	}
}
