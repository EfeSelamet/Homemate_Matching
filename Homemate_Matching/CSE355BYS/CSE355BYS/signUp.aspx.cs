using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
// 
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net;


namespace Homemate_Matching
{
    public partial class signUp : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            string UFName = TextBox1.Text.Trim();
            string ULastName = TextBox2.Text.Trim();
            string UPhoneNumber = TextBox3.Text.Trim();
            string username = TextBox4.Text.Trim();
            string password = TextBox5.Text.Trim();
            string UBirthDate = TextBox6.Text.Trim();

            // Debug output

            DateTime birthDate;
            if (!DateTime.TryParseExact(UBirthDate, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out birthDate))
            {
                lblMessage.Text = "Invalid birth date format. Please use YYYY-MM-DD.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            int age = DateTime.Now.Year - birthDate.Year;
            if (birthDate > DateTime.Now.AddYears(-age)) age--; // adjust if birthday hasn't occurred yet this year



            if (string.IsNullOrEmpty(UFName) || string.IsNullOrEmpty(ULastName) || string.IsNullOrEmpty(UPhoneNumber) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(UBirthDate))
            {
                lblMessage.Text = "Please fill all the blank spaces.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            if (age < 18)
            {
                lblMessage.Text = "You must be an adult to use this site.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }
            else
            {
                
            }


            // Database connection
            string connectionString = ConfigurationManager.ConnectionStrings["conStr"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                try
                {
                    // SQL command to check if the parent already exists
                    string checkQuery = "SELECT COUNT(*) FROM RegistrationInfo WHERE username = @UserName AND phoneNumber = @PhoneNumber";
                    SqlCommand checkCmd = new SqlCommand(checkQuery, con);
                    checkCmd.Parameters.AddWithValue("@username", username);
                    checkCmd.Parameters.AddWithValue("@PhoneNumber", UPhoneNumber);
                    
                    con.Open();
                    int count = (int)checkCmd.ExecuteScalar();
                    con.Close();

                    if (count > 0)
                    {
                        lblMessage.Text = "This user already exists.";
                        lblMessage.ForeColor = System.Drawing.Color.Red;
                        return;
                    }

                    else
                    {
                        int rowsAffected = 0;
                        using (SqlCommand insertCmd = new SqlCommand("InsertRegistrationInfo", con))
                        {
                            insertCmd.CommandType = CommandType.StoredProcedure;

                            insertCmd.Parameters.AddWithValue("@FirstName", UFName);
                            insertCmd.Parameters.AddWithValue("@LastName", ULastName);
                            insertCmd.Parameters.AddWithValue("@PhoneNumber", UPhoneNumber);
                            insertCmd.Parameters.AddWithValue("@Username", username);
                            insertCmd.Parameters.AddWithValue("@Password", password);
                            insertCmd.Parameters.AddWithValue("@BirthDate", UBirthDate); // Consider using DateTime instead of string

                            con.Open();
                            rowsAffected = insertCmd.ExecuteNonQuery();
                            con.Close();

                        }



                        //if (rowsAffected > 0)
                        {
                            lblMessage.Text = "Welcome.";
                            lblMessage.ForeColor = System.Drawing.Color.Green;

                            // Optionally clear form fields after successful addition
                            TextBox1.Text = "";
                            TextBox2.Text = "";
                            TextBox3.Text = "";
                            TextBox4.Text = "";
                            TextBox5.Text = "";
                            TextBox6.Text = "";

                            Session["FirstName"] = UFName;
                            Session["LastName"] = ULastName;
                            Session["PhoneNumber"] = UPhoneNumber;
                            Session["Username"] = username;
                            Session["BirthDate"] = UBirthDate;


                            Response.Redirect("profile.aspx");

                        }/*
                        else
                        {
                            lblMessage.Text = "There has been an error." + rowsAffected;
                            lblMessage.ForeColor = System.Drawing.Color.Red;
                         
                        }*/
                    }
                }
                catch (Exception ex)
                {
                    lblMessage.Text = "Error: " + ex.Message;
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                }
            }

        }
        protected void btnSignIn_Click(object sender, EventArgs e)
        {
            Response.Redirect("signIn.aspx");
        }
    }
}