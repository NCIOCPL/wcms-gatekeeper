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
    public partial class ManagerRoles : BasePage
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
                Display();
            }
        }

        private void Display()
        {
            string[] roles = Roles.GetAllRoles();
            DataTable dt = new DataTable();
            DataRow row;
         
            dt.Columns.Add(new DataColumn("Role"));

            foreach (string dr in roles)
            {
                row = dt.NewRow();
                row["Role"] = dr;
                dt.Rows.Add(row);            
            }


            this.grdItem.DataSource = dt;
            this.grdItem.Caption = "All Roles";
            this.grdItem.DataBind();

            ClearControls();
         }
       
        protected override void OnPreRender(EventArgs e)
        {
            // Add some javascript to the delete button to ask user for confirmation.
            foreach (GridViewRow row in this.grdItem.Rows)
            {
                row.Cells[1].Attributes["onclick"] = "return confirm('Are you sure?');";
            }
            base.OnPreRender(e);
        }

        protected void grdItem_RowDeleting(object sender, System.Web.UI.WebControls.GridViewDeleteEventArgs e)
        {
            try
            {
                string name = grdItem.Rows[e.RowIndex].Cells[0].Text;
                Roles.DeleteRole(name, true);
                Display();
            }
            catch (Exception ex)
            {
                this.lblStatus.Text = ex.Message;
            }
        }

        private void ClearControls()
        {
            this.txtRoleName.Text = string.Empty;
            this.lblStatus.Text = string.Empty;
        }

        protected void btnAdd_Click(object sender, EventArgs e)
        {
            if (this.txtRoleName.Text == string.Empty)
            {
                this.lblStatus.Text = "Role Name is required.";
                return;
            }
            try
            {
                Roles.CreateRole(this.txtRoleName.Text.Trim());
                ClearControls();
                Display();
            }
            catch (Exception ex)
            {
                this.lblStatus.Text = ex.Message;
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            ClearControls();
        }
    }
}
