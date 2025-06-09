using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace CSE355BYS
{
    public partial class student : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                PopulateParentsDropdown();
            }
        }

        private void PopulateParentsDropdown()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["conStr"].ConnectionString;
            string sql = "SELECT ParentID, fName + ' ' + lName AS FullName FROM Parent";

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    ddlParent1.Items.Clear();
                    ddlParent2.Items.Clear();

                    ddlParent1.Items.Add(new ListItem("Select Parent 1", ""));
                    ddlParent2.Items.Add(new ListItem("Select Parent 2", ""));

                    while (reader.Read())
                    {
                        ListItem item = new ListItem(reader["FullName"].ToString(), reader["ParentID"].ToString());
                        ddlParent1.Items.Add(item);
                        ddlParent2.Items.Add(item);
                    }
                }
            }
        }

        protected void btnAddStudent_Click(object sender, EventArgs e)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["conStr"].ConnectionString;

            string sql = "INSERT INTO Student (fName, lName, Parent1ID, Parent2ID) VALUES (@fName, @lName, @Parent1ID, @Parent2ID)";

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@fName", txtFName.Text);
                    cmd.Parameters.AddWithValue("@lName", txtLName.Text);
                    cmd.Parameters.AddWithValue("@Parent1ID", string.IsNullOrEmpty(ddlParent1.SelectedValue) ? DBNull.Value : ddlParent1.SelectedValue);
                    cmd.Parameters.AddWithValue("@Parent2ID", string.IsNullOrEmpty(ddlParent2.SelectedValue) ? DBNull.Value : ddlParent2.SelectedValue);

                    con.Open();
                    cmd.ExecuteNonQuery();
                    lblMessage.Text = "Student added successfully!";
                }
            }
        }
    }
}
