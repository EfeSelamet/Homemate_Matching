using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.Web;
using System.Web.UI.WebControls; // Added for ListItem

namespace Homemate_Matching
{
    public partial class userPreferences : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string username = HttpContext.Current.User.Identity.Name;
                string connStr = ConfigurationManager.ConnectionStrings["conStr"].ConnectionString;

                using (SqlConnection con = new SqlConnection(connStr))
                {
                    try
                    {
                        con.Open();

                        string prefQuery = "SELECT * FROM UserPreference WHERE username = @username";
                        SqlCommand prefCmd = new SqlCommand(prefQuery, con);
                        prefCmd.Parameters.AddWithValue("@username", username);

                        SqlDataReader reader = prefCmd.ExecuteReader();
                        if (reader.Read())
                        {
                            string gender = reader["gender"].ToString();
                            txtGenderPref.Text = gender; // Set the hidden textbox value
                            ClientScript.RegisterStartupScript(this.GetType(), "setGenderPrefRadio", $@"
                            <script>
                                var genderValue = '{gender}';
                                if (genderValue === 'm') {{
                                    document.getElementById('genderPrefMaleRadio').checked = true;
                                }} else if (genderValue === 'f') {{
                                    document.getElementById('genderPrefFemaleRadio').checked = true;
                                }} else if (genderValue === 'i') {{
                                    document.getElementById('genderPrefIrrelevantRadio').checked = true;
                                }}
                                document.getElementById('{txtGenderPref.ClientID}').value = '{gender}';
                            </script>", false);

                            ddlSleepPref.SelectedValue = reader["sleepSchedule"].ToString();

                            // Using conditional operator to handle potential DBNull or empty string for booleans
                            chkSmokePref.Checked = reader["NoSmoker"] != DBNull.Value ? Convert.ToBoolean(reader["NoSmoker"]) : false;
                            chkDrinkPref.Checked = reader["NoDrinker"] != DBNull.Value ? Convert.ToBoolean(reader["NoDrinker"]) : false;
                            chkStudentPref.Checked = reader["StudentPref"] != DBNull.Value ? Convert.ToBoolean(reader["StudentPref"]) : false;
                            // Using conditional operator to handle potential DBNull or empty string for numeric values
                            txtMinBudget.Text = reader["minBudget"] != DBNull.Value ? Convert.ToDecimal(reader["minBudget"]).ToString(CultureInfo.InvariantCulture) : "0"; // Provide a default if null
                            txtMaxBudget.Text = reader["maxBudget"] != DBNull.Value ? Convert.ToDecimal(reader["maxBudget"]).ToString(CultureInfo.InvariantCulture) : "0"; // Provide a default if null

                            ddlLocationPref.SelectedValue = reader["desiredHomeLocation"].ToString();

                            // Safely load integer values with defaults
                            txtHygieneImportance.Text = reader["SignificanceOfHygiene"] != DBNull.Value ? reader["SignificanceOfHygiene"].ToString() : "5";
                            txtNoiseSensitivity.Text = reader["NoiseSensitivity"] != DBNull.Value ? reader["NoiseSensitivity"].ToString() : "5";
                            txtGuestPref.Text = reader["GuestPreference"] != DBNull.Value ? reader["GuestPreference"].ToString() : "5";
                        }
                        reader.Close();
                    }
                    catch (Exception ex)
                    {
                        lblMessage.Text = "Error loading preferences: " + ex.Message;
                        lblMessage.ForeColor = System.Drawing.Color.Red;
                    }
                }
            }
        }

        protected void btnSubmitPref_Click(object sender, EventArgs e)
        {
            // Veri alma ve temel dönüşümler
            string username = HttpContext.Current.User.Identity.Name;
            string gender = txtGenderPref.Text.Trim();
            string sleep = ddlSleepPref.SelectedValue;
            bool noSmoker = chkSmokePref.Checked;
            bool noDrinker = chkDrinkPref.Checked;
            string location = ddlLocationPref.SelectedValue;
            bool studentPref = chkStudentPref.Checked;
            // Safely parse integer values
            int hygiene;
            if (!int.TryParse(txtHygieneImportance.Text.Trim(), out hygiene))
            {
                lblMessage.Text = "Hijyen Önem Derecesi için geçerli bir değer girin.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            int noise;
            if (!int.TryParse(txtNoiseSensitivity.Text.Trim(), out noise))
            {
                lblMessage.Text = "Gürültü Hassasiyeti için geçerli bir değer girin.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            string guestPrefText = txtGuestPref.Text.Trim(); // Read as string first
            int guestPref;
            if (!int.TryParse(guestPrefText, out guestPref))
            {
                lblMessage.Text = "Misafir Tercihi için geçerli bir değer girin.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

           

            decimal minBudget;
            decimal maxBudget;

            // Bütçe alanlarını güvenli bir şekilde ayrıştırma
            if (!decimal.TryParse(txtMinBudget.Text.Trim(), NumberStyles.Currency | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out minBudget))
            {
                lblMessage.Text = "Lütfen geçerli bir minimum bütçe girin.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }
            if (!decimal.TryParse(txtMaxBudget.Text.Trim(), NumberStyles.Currency | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out maxBudget))
            {
                lblMessage.Text = "Lütfen geçerli bir maksimum bütçe girin.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            // Doğrulama: Maksimum bütçe minimum bütçeden küçük olamaz.
            if (maxBudget < minBudget)
            {
                lblMessage.Text = "Maksimum bütçe, minimum bütçeden küçük olamaz.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return; // İşlemi durdur
            }

            string connStr = ConfigurationManager.ConnectionStrings["conStr"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connStr))
            {
                try
                {
                    con.Open();

                    string checkQuery = "SELECT COUNT(*) FROM UserPreference WHERE username = @username";
                    SqlCommand checkCmd = new SqlCommand(checkQuery, con);
                    checkCmd.Parameters.AddWithValue("@username", username);
                    int exists = (int)checkCmd.ExecuteScalar();

                    string query;
                    if (exists > 0)
                    {
                        query = @"UPDATE UserPreference
                                  SET gender = @gender,
                                      sleepSchedule = @sleepSchedule,
                                      NoSmoker = @noSmoker,
                                      NoDrinker = @noDrinker,
                                      minBudget = @minBudget,
                                      maxBudget = @maxBudget,
                                      desiredHomeLocation = @desiredHomeLocation,
                                      SignificanceOfHygiene = @SignificanceOfHygiene,
                                      NoiseSensitivity = @NoiseSensitivity,
                                      GuestPreference = @GuestPreference,
                                      StudentPref = @StudentPref
                                  WHERE username = @username";
                    }
                    else
                    {
                        query = @"INSERT INTO UserPreference (username, gender, sleepSchedule, NoSmoker, NoDrinker, minBudget, maxBudget, desiredHomeLocation, SignificanceOfHygiene, NoiseSensitivity, GuestPreference, StudentPref)
                                  VALUES (@username, @gender, @sleepSchedule, @noSmoker, @noDrinker, @minBudget, @maxBudget, @desiredHomeLocation, @SignificanceOfHygiene, @NoiseSensitivity, @GuestPreference, @StudentPref)";
                    }

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.Parameters.AddWithValue("@gender", gender);
                        cmd.Parameters.AddWithValue("@sleepSchedule", sleep);
                        cmd.Parameters.AddWithValue("@noSmoker", noSmoker); // Parameter name changed to match column
                        cmd.Parameters.AddWithValue("@noDrinker", noDrinker); // Parameter name changed to match column
                        cmd.Parameters.AddWithValue("@minBudget", minBudget);
                        cmd.Parameters.AddWithValue("@maxBudget", maxBudget);
                        cmd.Parameters.AddWithValue("@desiredHomeLocation", location);
                        cmd.Parameters.AddWithValue("@SignificanceOfHygiene", hygiene);
                        cmd.Parameters.AddWithValue("@NoiseSensitivity", noise);
                        cmd.Parameters.AddWithValue("@GuestPreference", guestPref);
                        cmd.Parameters.AddWithValue("@StudentPref", studentPref); // Add parameter for StudentPref
                        cmd.ExecuteNonQuery();
                    }

                    lblMessage.Text = "Preferences saved successfully!";
                    Response.Redirect("profile.aspx");
                }
                catch (Exception ex)
                {
                    lblMessage.Text = "Error saving preferences: " + ex.Message;
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                }
            }
        }
    }
}