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
using System.Net.Mail;

namespace GateKeeperAdmin.Security
{
    public partial class RecoverPassword : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            this.PasswordRecovery1.MailDefinition.From =ConfigurationManager.AppSettings["AdminEmailAddress"].ToString();

        }
        //protected void PasswordRecovery_SendingMail(object sender, MailMessageEventArgs e)
        //{
        //    string email = "";
        //    string pwd = "";
        //    try
        //    {
        //        MembershipUser user = Membership.GetUser(this.PasswordRecovery1.UserName.Trim());
        //        email = user.Email;
        //        pwd = user.ResetPassword();
        //    }
        //    catch (Exception ex)
        //    {
        //        this.PasswordRecovery1.GeneralFailureText = "Can't reset password for the user " + this.PasswordRecovery1.UserName.Trim() + ". Detailed info: " + ex.ToString();
        //    }

        //    if (string.IsNullOrEmpty(email)) this.PasswordRecovery1.GeneralFailureText = "This user's email is empty or null.";

        //    try
        //    {
        //        string senderEmail = ConfigurationManager.AppSettings["AdminEmailAddress"].ToString();

        //        using (MailMessage mail = new MailMessage())
        //        {
        //            mail.To.Add(email);
        //            mail.From = new MailAddress(senderEmail);
        //            mail.Subject = "Your new password";
        //            mail.Body = "New Password:" + pwd;
        //            mail.IsBodyHtml = false;

        //            SmtpClient mailer = new SmtpClient();
        //            mailer.Send(mail);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        this.PasswordRecovery1.GeneralFailureText = "Error in sending the email. " + ex.ToString();
        //    }
        //}

    }
}
