using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Security;

namespace Homemate_Matching
{
    public partial class signIn : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void btnSignIn_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                lblMessage.Text = "Please enter both username and password.";
                return;
            }

            string connectionString = ConfigurationManager.ConnectionStrings["conStr"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                try
                {
                    string query = "SELECT password FROM RegistrationInfo WHERE username = @username";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@username", username);

                    con.Open();
                    object result = cmd.ExecuteScalar();
                    con.Close();

                    if (result != null)
                    {
                        string dbPassword = result.ToString();
                        if (dbPassword == password)
                        {
                            FormsAuthentication.SetAuthCookie(username, false);
                            Response.Redirect("profile.aspx");
                        }
                        else
                        {
                            lblMessage.Text = "Incorrect password.";
                        }
                    }
                    else
                    {
                        lblMessage.Text = "User not found.";
                    }
                }
                catch (Exception ex)
                {
                    lblMessage.Text = "Error: " + ex.Message;
                }
            }
        }
        protected void btnSignUp_Click(object sender, EventArgs e)
        {
            Response.Redirect("signUp.aspx");
        }

    }
}
