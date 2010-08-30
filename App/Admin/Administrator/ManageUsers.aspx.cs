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
    public partial class ManageUsers : BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //Check Permission. If not permitted, redirect. 
            if (!IsUserAdmin())
            {
                Response.Redirect("../Home.aspx");
                return;
            }

            if (!IsPostBack)
            {
                this.lblStatus.Text = string.Empty;
                Display();
            }
        }

        private void Display()
        {
            MembershipUserCollection users = Membership.GetAllUsers();
            DataTable dt = new DataTable();
            DataRow row;
         
            dt.Columns.Add(new DataColumn("UserName"));
            dt.Columns.Add(new DataColumn("Email"));
            dt.Columns.Add(new DataColumn("CreationDate"));
            dt.Columns.Add(new DataColumn("LastLoginDate"));
            dt.Columns.Add(new DataColumn("Role"));

            foreach (MembershipUser u in users)
            {
                row = dt.NewRow();
                row["UserName"] = u.UserName;
                row["Email"] = u.Email;
                row["CreationDate"] = u.CreationDate;
                row["LastLoginDate"] = u.LastLoginDate;

                string[] rolelist = Roles.GetRolesForUser(u.UserName);
                if (rolelist != null && rolelist.Length >0)
                {
                    row["Role"] =  string.Join(", ", rolelist);
                }
                else
                    row["Role"]=string.Empty;

                dt.Rows.Add(row);            
            }


            this.grdItem.DataSource = dt;
            this.grdItem.Caption = "All Users";
            this.grdItem.DataBind();

        }
               
        protected override void OnPreRender(EventArgs e)
        {
            // Add some javascript to the delete button to ask user for confirmation.
            foreach (GridViewRow row in this.grdItem.Rows)
            {
                row.Cells[1].Attributes["onclick"] = "return confirm('Are you sure?');";
                row.Cells[7].Attributes["onclick"] = "return confirm('Are you sure?');";
            }
            base.OnPreRender(e);
        }

        public void LinkButtonClick(object s, CommandEventArgs e)
        {
            this.lblStatus.Text = string.Empty;
            string username = ((string)e.CommandArgument);
            switch (e.CommandName.ToLower())
            {
                case "edit":
                    Response.Redirect("EditUser.aspx?UserName=" + Server.UrlEncode(username));

                break;
                case "delete":
                    try
                    {
                        Membership.DeleteUser(username);
                        Display();
                    }
                    catch (Exception ex)
                    {
                        this.lblStatus.Text = ex.Message;
                    }
                    break;
                case "deleterole":
                    string rolename = string.Join(",", Roles.GetRolesForUser(username));
           
                    try
                    {
                        Roles.RemoveUserFromRole(username,rolename);
                        Display();
                    }
                    catch (Exception ex)
                    {
                        this.lblStatus.Text = ex.Message;
                    }
                break;
            }
        }

        protected void grdItem_RowDeleting(object sender, System.Web.UI.WebControls.GridViewDeleteEventArgs e)
        {
            this.lblStatus.Text = string.Empty;
            try
            {
                string username = grdItem.Rows[e.RowIndex].Cells[2].Text;
                Membership.DeleteUser(username);
                Display();
            }
            catch (Exception ex)
            {
                this.lblStatus.Text = ex.Message;
            }
        }
                
        protected void grdItem_RowEditing(object sender, System.Web.UI.WebControls.GridViewEditEventArgs e)
        {
            string name = grdItem.Rows[e.NewEditIndex].Cells[2].Text;
            Response.Redirect("EditUser.aspx?UserName=" + Server.UrlEncode(name));
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        { 
            ClearControls();
        }

        
        private void ClearControls()
        {
            this.txtEmail.Text= string.Empty;
            this.txtPassword.Text = string.Empty;
            this.txtPasswordConfirmation.Text = string.Empty;
            this.txtPasswordAnswer.Text = string.Empty;
            this.txtPasswordQuestion.Text = string.Empty;
            this.txtUserName.Text = string.Empty;
            this.lblStatus.Text = string.Empty;
        }

        protected void btnAdd_Click(object sender, EventArgs e)
        { 
            string error = "";
            error += (this.txtUserName.Text.Trim().Length == 0 ? "User Name's Required<br>" : "");
            error += (this.txtEmail.Text.Trim().Length == 0 ? "Email's Required<br>" : "");
            error += (this.txtPassword.Text.Trim().Length == 0 ? "Password's Required<br>" : "");
            error += (this.txtPasswordConfirmation.Text.Trim().Length == 0 ? "Password Confirmation's Required<br>" : "");
          error += (this.txtPasswordQuestion.Text.Trim().Length == 0 ? "Password Question's Required<br>" : "");
            error += (this.txtPasswordAnswer.Text.Trim().Length == 0 ? "Password Answer's Required<br>" : "");

            if (error.Trim().Length > 0)
            {
                lblStatus.Text = error;
                return;
            }
            try
            {
                MembershipCreateStatus status;
                Membership.CreateUser(
                    this.txtUserName.Text.Trim(),
                    this.txtPassword.Text.Trim(),
                    this.txtEmail.Text.Trim(),
                    this.txtPasswordAnswer.Text.Trim(),
                    this.txtPasswordQuestion.Text.Trim(),
                    true,
                    out status);

                ClearControls();
                lblStatus.Text = "Status = " + status.ToString();

                Display();
            }
            catch (Exception ex)
            {
                lblStatus.Text = ex.Message;
            }
        }
    }
}
