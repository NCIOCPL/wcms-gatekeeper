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

namespace GateKeeperAdmin.Administrator
{
    public partial class AssignRoleToUser : BasePage
    {
        private string userName;

        protected void Page_Load(object sender, EventArgs e)
        {
            //Check Permission. If not permitted, redirect. 
            if (!IsUserAdmin())
            {
                Response.Redirect("../Home.aspx");
                return;
            }


            userName = Request.QueryString["UserName"].ToString();
        
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

            this.ddlRole.DataSource = Roles.GetAllRoles();
            this.ddlRole.DataBind();
            this.ddlRole.Items.Add(new ListItem("Please select a role", string.Empty));

            string role = string.Empty;
            string[] roleList = Roles.GetRolesForUser(userName);
    
            if (roleList != null && roleList.Length>0)
                role = string.Join(",",roleList);

            foreach(ListItem item in this.ddlRole.Items)
            {
                if (item.Value == role)
                    item.Selected = true;
            }

        }
               
        protected void btnCancel_Click(object sender, EventArgs e)
        {           
            this.txtEmail.Text = string.Empty;
            this.lblStatus.Text = string.Empty;
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

        protected void btnAssign_Click(object sender, EventArgs e)
        {   
             this.lblStatus.Text = string.Empty;
            if (this.ddlRole.SelectedValue == string.Empty)
                return;

            string[] roleList = Roles.GetRolesForUser(userName);
   
            try
            {
                if (roleList != null && roleList.Length >0)
                {
                    Roles.RemoveUserFromRole(userName, string.Join(",", roleList));
                }
                Roles.AddUserToRole(userName,this.ddlRole.SelectedValue);
            }
            catch(Exception ex)
            {
                 this.lblStatus.Text = ex.Message;
            }
        }
    }
}
