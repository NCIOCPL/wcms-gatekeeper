using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace GateKeeperAdmin.Security
{
    public partial class ResetPassword : System.Web.UI.Page
    {
        private string userName;

        protected void Page_Load(object sender, EventArgs e)
        {
            userName = Page.User.Identity.Name;
            if (userName == string.Empty)
                Response.Redirect("~/Home.aspx");

                
            if (!IsPostBack)
            {
                Display();
            }
        }

        private void Display()
        {
            this.txtUserName.Text = userName;
            MembershipUser user = Membership.GetUser(userName);
           
            this.txtEmail.Text = user.Email.ToString();
            
            string[] roleList = Roles.GetRolesForUser(userName);
    
            if (roleList != null && roleList.Length>0)
                this.txtRole.Text = string.Join(",", roleList);

        }
               
        protected void btnCancel_Click(object sender, EventArgs e)
        {           
            this.txtEmail.Text = string.Empty;
            this.lblStatus.Text = string.Empty;
        }

        protected void Redirect(object sender, EventArgs e)
        {
            this.ChangePassword1.SuccessTemplate = new SuccessTemplate();
        }

        protected void btnUpdate_Click(object sender, EventArgs e)
        { 
             this.lblStatus.Text = string.Empty;
            if (this.txtEmail.Text.Trim().Length == 0 )
            {
                this.lblStatus.Text ="Email's Required";
                return;
            }            
            
            MembershipUser user = Membership.GetUser(userName);
            user.Email = this.txtEmail.Text.Trim();
                         
            try
            {
                Membership.UpdateUser(user);
            }
            catch(Exception ex)
            {
                 this.lblStatus.Text = ex.Message;
            }
        }

    }

    public class SuccessTemplate : ITemplate
    {
        public void InstantiateIn(System.Web.UI.Control container)
        {
            Literal lc = new Literal();

            lc.Text = "<TABLE cellspacing=\"0\" cellpadding=\"1\" border=\"0\"><TR><TH>Change Password Complete</TH></TR>"
                    +"<TR><TD>Your password has been changed! </TD></TR></TABLE>";

            container.Controls.Add(lc);
        }
    }
}
