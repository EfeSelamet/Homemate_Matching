using System.Configuration;
using System.Data.SqlClient;
using System;
using System.Web;
using System.IO;
using System.Web.UI.WebControls; // Needed for DropDownList
using System.Globalization; // Needed for CultureInfo

namespace Homemate_Matching
{
    public partial class userAttributes : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                pnlHouseInfo.Visible = false;
                string username = HttpContext.Current.User.Identity.Name;
                string connStr = ConfigurationManager.ConnectionStrings["conStr"].ConnectionString;

                using (SqlConnection con = new SqlConnection(connStr))
                {
                    try
                    {
                        con.Open();

                        // Get user attributes if they exist
                        string attrQuery = "SELECT * FROM UserAttribute WHERE username = @username";
                        SqlCommand attrCmd = new SqlCommand(attrQuery, con);
                        attrCmd.Parameters.AddWithValue("@username", username);

                        SqlDataReader reader = attrCmd.ExecuteReader();
                        if (reader.Read())
                        {
                            // Load Gender (radio buttons)
                            string gender = reader["gender"] != DBNull.Value ? reader["gender"].ToString() : ""; // No default 'i'
                            txtGender.Text = gender;
                            ClientScript.RegisterStartupScript(this.GetType(), "setGenderRadio", $@"
                            <script>
                                var genderValue = '{gender}';
                                if (genderValue === 'm') {{
                                    document.getElementById('genderMale').checked = true;
                                }} else if (genderValue === 'f') {{
                                    document.getElementById('genderFemale').checked = true;
                                }}
                                document.getElementById('{txtGender.ClientID}').value = '{gender}';
                            </script > ", false);

                            // Load Sleep Schedule (dropdown)
                            ddlSleepSchedule.SelectedValue = reader["sleepSchedule"] != DBNull.Value ? reader["sleepSchedule"].ToString() : "Sabah Kuşu"; // Default to a valid option

                            // Update checkbox loading logic based on "I use" instead of "I don't use"
                            // If DB column isSmoker means "is a smoker", then chkSmoker.Checked (I am a smoker) should be isSmoker
                            // If DB column NoSmoker means "is NOT a smoker", then chkSmoker.Checked (I am a smoker) should be !NoSmoker
                            // I'm assuming your DB columns are 'isSmoker' and 'isDrinker' as per initial code for UserAttribute. If they are 'NoSmoker'/'NoDrinker', adjust accordingly.
                            chkSmoker.Checked = reader["isSmoker"] != DBNull.Value ? Convert.ToBoolean(reader["isSmoker"]) : false;
                            chkDrinker.Checked = reader["isDrinker"] != DBNull.Value ? Convert.ToBoolean(reader["isDrinker"]) : false;

                            // Load Is Student
                            chkIsStudent.Checked = reader["IsStudent"] != DBNull.Value ? Convert.ToBoolean(reader["IsStudent"]) : false;

                            // Load Hygiene (slider/hidden textbox)
                            txtHygiene.Text = reader["Hygiene"] != DBNull.Value ? reader["Hygiene"].ToString() : "5";
                            ClientScript.RegisterStartupScript(this.GetType(), "setHygieneSlider", $@"
                            <script>
                                document.getElementById('sliderHygiene').value = '{txtHygiene.Text}';
                                document.getElementById('sliderHygieneValue').textContent = '{txtHygiene.Text}';
                            </script>", false);


                            // Load Noisiness (slider/hidden textbox)
                            txtNoisiness.Text = reader["noisiness"] != DBNull.Value ? reader["noisiness"].ToString() : "5";
                            ClientScript.RegisterStartupScript(this.GetType(), "setNoisinessSlider", $@"
                            <script>
                                document.getElementById('sliderNoisiness').value = '{txtNoisiness.Text}';
                                document.getElementById('sliderNoisinessValue').textContent = '{txtNoisiness.Text}';
                            </script>", false);

                            // Load Guest Frequency (slider/hidden textbox)
                            txtGuestFrequency.Text = reader["GuestFrequency"] != DBNull.Value ? reader["GuestFrequency"].ToString() : "5";
                            ClientScript.RegisterStartupScript(this.GetType(), "setGuestFrequencySlider", $@"
                            <script>
                                document.getElementById('sliderGuestFrequency').value = '{txtGuestFrequency.Text}';
                                document.getElementById('sliderGuestFrequencyValue').textContent = '{txtGuestFrequency.Text}';
                            </script>", false);

                            chkHasHouse.Checked = reader["homeID"] != DBNull.Value ? Convert.ToBoolean(reader["homeID"]): false;
                            pnlHouseInfo.Visible = reader["homeID"] != DBNull.Value ? Convert.ToBoolean(reader["homeID"]) : false;

                            // Load House Info if homeID exists
                            if (reader["homeID"] != DBNull.Value)
                            {

                                int homeID = Convert.ToInt32(reader["homeID"]);
                                reader.Close(); // Close the first reader before opening a new one

                                // Get house info
                                string houseQuery = "SELECT * FROM Home WHERE homeID = @homeID";
                                SqlCommand houseCmd = new SqlCommand(houseQuery, con);
                                houseCmd.Parameters.AddWithValue("@homeID", homeID);

                                SqlDataReader houseReader = houseCmd.ExecuteReader();
                                if (houseReader.Read())
                                {
                                    txtRent.Text = houseReader["Rent"] != DBNull.Value ? Convert.ToDecimal(houseReader["Rent"]).ToString(CultureInfo.InvariantCulture) : "";
                                    ddlLocation.SelectedValue = houseReader["Location"] != DBNull.Value ? houseReader["Location"].ToString() : "Ataşehir"; // Default to a valid option
                                    txtRooms.Text = houseReader["NumberOfRooms"] != DBNull.Value ? houseReader["NumberOfRooms"].ToString() : "";
                                    txtSaloons.Text = houseReader["NumberOfSaloon"] != DBNull.Value ? houseReader["NumberOfSaloon"].ToString() : "";
                                    txtSurfaceArea.Text = houseReader["SurfaceArea"] != DBNull.Value ? houseReader["SurfaceArea"].ToString() : "";
                                    txtDescription.Text = houseReader["Description"] != DBNull.Value ? houseReader["Description"].ToString() : "";
                                    txtFlatFloor.Text = houseReader["FlatFloor"] != DBNull.Value ? houseReader["FlatFloor"].ToString() : "";
                                    txtBuildingFloor.Text = houseReader["BuildingFloor"] != DBNull.Value ? houseReader["BuildingFloor"].ToString() : "";
                                    txtBuildingFloorCount.Text = houseReader["BuildingFloorCount"] != DBNull.Value ? houseReader["BuildingFloorCount"].ToString() : "";
                                    txtHeating.Text = houseReader["Heating"] != DBNull.Value ? houseReader["Heating"].ToString() : "";
                                    txtBuildingAge.Text = houseReader["BuildingAge"] != DBNull.Value ? houseReader["BuildingAge"].ToString() : "";
                                }
                                houseReader.Close();
                            }
                            else
                            {
                                chkHasHouse.Checked = false;
                                pnlHouseInfo.Visible = false;
                                reader.Close(); // Close the first reader
                            }
                        }
                        else
                        {
                            reader.Close(); // No attributes yet, ensure reader is closed
                        }
                    }
                    catch (Exception ex)
                    {
                        lblMessage.Text = "Error loading data: " + ex.Message;
                        lblMessage.ForeColor = System.Drawing.Color.Red;
                    }
                }
            }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            string username = HttpContext.Current.User.Identity.Name;
            string gender = txtGender.Text.Trim(); // From hidden textbox
            string sleepSchedule = ddlSleepSchedule.SelectedValue; // From DropDownList

            // Checkbox values directly correspond to "I use" for UserAttribute
            bool isSmoker = chkSmoker.Checked;
            bool isDrinker = chkDrinker.Checked;

            bool isStudent = chkIsStudent.Checked;
            bool hasHouse = chkHasHouse.Checked;

            // Safe parsing for Hygiene, Noisiness, GuestFrequency
            int hygiene;
            if (!int.TryParse(txtHygiene.Text.Trim(), out hygiene))
            {
                lblMessage.Text = "Hijyen değeri geçerli bir sayı olmalıdır.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            int noisiness;
            if (!int.TryParse(txtNoisiness.Text.Trim(), out noisiness))
            {
                lblMessage.Text = "Gürültü değeri geçerli bir sayı olmalıdır.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            int guestFrequency;
            if (!int.TryParse(txtGuestFrequency.Text.Trim(), out guestFrequency))
            {
                lblMessage.Text = "Misafir Sıklığı değeri geçerli bir sayı olmalıdır.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            int? homeID = null;

            string connStr = ConfigurationManager.ConnectionStrings["conStr"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connStr))
            {
                try
                {
                    con.Open();

                    // Check if the user already has a homeID
                    string getOldHomeQuery = "SELECT homeID FROM UserAttribute WHERE username = @username";
                    SqlCommand getOldHomeCmd = new SqlCommand(getOldHomeQuery, con);
                    getOldHomeCmd.Parameters.AddWithValue("@username", username);
                    object oldHomeObj = getOldHomeCmd.ExecuteScalar();
                    int? oldHomeID = (oldHomeObj != DBNull.Value && oldHomeObj != null) ? Convert.ToInt32(oldHomeObj) : (int?)null;


                    if (hasHouse)
                    {
                        // Safely parse house info
                        decimal rent;
                        if (!decimal.TryParse(txtRent.Text.Trim(), NumberStyles.Currency | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out rent))
                        {
                            lblMessage.Text = "Kira değeri geçerli bir sayı olmalıdır.";
                            lblMessage.ForeColor = System.Drawing.Color.Red;
                            return;
                        }

                        string location = ddlLocation.SelectedValue; // Get value from DropDownList

                        int numberOfRooms, numberOfSaloons, flatFloor, buildingFloor, buildingFloorCount, buildingAge;
                        float surfaceArea;

                        if (!int.TryParse(txtRooms.Text.Trim(), out numberOfRooms) ||
                            !int.TryParse(txtSaloons.Text.Trim(), out numberOfSaloons) ||
                            !float.TryParse(txtSurfaceArea.Text.Trim(), out surfaceArea) ||
                            !int.TryParse(txtFlatFloor.Text.Trim(), out flatFloor) ||
                            !int.TryParse(txtBuildingFloor.Text.Trim(), out buildingFloor) ||
                            !int.TryParse(txtBuildingFloorCount.Text.Trim(), out buildingFloorCount) ||
                            !int.TryParse(txtBuildingAge.Text.Trim(), out buildingAge))
                        {
                            lblMessage.Text = "Ev bilgileri için geçerli sayısal değerler giriniz (Oda, Salon, Alan, Katlar, Bina Yaşı).";
                            lblMessage.ForeColor = System.Drawing.Color.Red;
                            return;
                        }

                        // Insert new house info
                        string insertHouseQuery = @"
                            INSERT INTO Home (Rent, Location, NumberOfRooms, NumberOfSaloon, SurfaceArea, Description, FlatFloor, BuildingFloor, BuildingFloorCount, Heating, BuildingAge)
                            OUTPUT INSERTED.homeID
                            VALUES (@Rent, @Location, @NumberOfRooms, @NumberOfSaloon, @SurfaceArea, @Description, @FlatFloor, @BuildingFloor, @BuildingFloorCount, @Heating, @BuildingAge)";

                        using (SqlCommand houseCmd = new SqlCommand(insertHouseQuery, con))
                        {
                            houseCmd.Parameters.AddWithValue("@Rent", rent);
                            houseCmd.Parameters.AddWithValue("@Location", location);
                            houseCmd.Parameters.AddWithValue("@NumberOfRooms", numberOfRooms);
                            houseCmd.Parameters.AddWithValue("@NumberOfSaloon", numberOfSaloons);
                            houseCmd.Parameters.AddWithValue("@SurfaceArea", surfaceArea);
                            houseCmd.Parameters.AddWithValue("@Description", txtDescription.Text.Trim());
                            houseCmd.Parameters.AddWithValue("@FlatFloor", flatFloor);
                            houseCmd.Parameters.AddWithValue("@BuildingFloor", buildingFloor);
                            houseCmd.Parameters.AddWithValue("@BuildingFloorCount", buildingFloorCount);
                            houseCmd.Parameters.AddWithValue("@Heating", txtHeating.Text.Trim());
                            houseCmd.Parameters.AddWithValue("@BuildingAge", buildingAge);

                            homeID = (int)houseCmd.ExecuteScalar();
                        }

                        // Optional: Delete old home if it exists (to avoid orphan records)
                        if (oldHomeID != null)
                        {
                            // First, set homeID to null in UserAttribute for the user
                            string nullOldHomeAttrQuery = @"UPDATE UserAttribute SET homeID = NULL WHERE username = @username";
                            SqlCommand nullOldHomeAttrCmd = new SqlCommand(nullOldHomeAttrQuery, con);
                            nullOldHomeAttrCmd.Parameters.AddWithValue("@username", username);
                            nullOldHomeAttrCmd.ExecuteNonQuery();

                            // Then, delete the old home record
                            SqlCommand deleteOldHomeCmd = new SqlCommand("DELETE FROM Home WHERE homeID = @oldHomeID", con);
                            deleteOldHomeCmd.Parameters.AddWithValue("@oldHomeID", oldHomeID);
                            deleteOldHomeCmd.ExecuteNonQuery();

                        }
                    }
                    else // if (!hasHouse)
                    {
                        // If user unselected the house checkbox and had a house before, delete old house
                        if (oldHomeID != null)
                        {
                            // First, set homeID to null in UserAttribute for the user
                            string nullOldHomeAttrQuery = @"UPDATE UserAttribute SET homeID = NULL WHERE username = @username";
                            SqlCommand nullOldHomeAttrCmd = new SqlCommand(nullOldHomeAttrQuery, con);
                            nullOldHomeAttrCmd.Parameters.AddWithValue("@username", username);
                            nullOldHomeAttrCmd.ExecuteNonQuery();

                            // Then, delete the old home record
                            SqlCommand deleteOldHouseCmd = new SqlCommand("DELETE FROM Home WHERE homeID = @oldHomeID", con);
                            deleteOldHouseCmd.Parameters.AddWithValue("@oldHomeID", oldHomeID);
                            deleteOldHouseCmd.ExecuteNonQuery();

                            // Also delete associated house pictures
                            SqlCommand deleteHousePics = new SqlCommand("DELETE FROM HousePicture WHERE username = @username", con);
                            deleteHousePics.Parameters.AddWithValue("@username", username);
                            deleteHousePics.ExecuteNonQuery();
                        }
                    }

                    // Insert or update UserAttribute
                    string checkQuery = "SELECT COUNT(*) FROM UserAttribute WHERE username = @username";
                    SqlCommand checkCmd = new SqlCommand(checkQuery, con);
                    checkCmd.Parameters.AddWithValue("@username", username);
                    int exists = (int)checkCmd.ExecuteScalar();

                    string query;
                    if (exists > 0)
                    {
                        query = @"UPDATE UserAttribute
                                  SET gender = @gender,
                                      sleepSchedule = @sleepSchedule,
                                      isSmoker = @isSmoker,
                                      isDrinker = @isDrinker,
                                      Hygiene = @Hygiene,
                                      noisiness = @noisiness,
                                      GuestFrequency = @GuestFrequency,
                                      IsStudent = @IsStudent,
                                      homeID = @homeID
                                  WHERE username = @username";
                    }
                    else
                    {
                        query = @"INSERT INTO UserAttribute (username, gender, sleepSchedule, isSmoker, isDrinker, Hygiene, noisiness, homeID, GuestFrequency, IsStudent)
                                  VALUES (@username, @gender, @sleepSchedule, @isSmoker, @isDrinker, @Hygiene, @noisiness, @homeID, @GuestFrequency, @IsStudent)";
                    }

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.Parameters.AddWithValue("@gender", gender);
                        cmd.Parameters.AddWithValue("@sleepSchedule", sleepSchedule);
                        cmd.Parameters.AddWithValue("@isSmoker", isSmoker);
                        cmd.Parameters.AddWithValue("@isDrinker", isDrinker);
                        cmd.Parameters.AddWithValue("@Hygiene", hygiene);
                        cmd.Parameters.AddWithValue("@noisiness", noisiness);
                        cmd.Parameters.AddWithValue("@GuestFrequency", guestFrequency);
                        cmd.Parameters.AddWithValue("@IsStudent", isStudent);
                        cmd.Parameters.AddWithValue("@homeID", (object)homeID ?? DBNull.Value); // Handles null homeID
                        cmd.ExecuteNonQuery();
                    }

                    lblMessage.Text = "Information saved successfully!";
                    Response.Redirect("profile.aspx");
                }
                catch (Exception ex)
                {
                    lblMessage.Text = "Error: " + ex.Message;
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                }
            }
        }

        protected void btnUpload_Click(object sender, EventArgs e)
        {
            string username = HttpContext.Current.User.Identity.Name;
            if (username == null)
            {
                Response.Redirect("signIn.aspx");
                return;
            }

            if (fileUploadProfilePic.HasFile)
            {
                string extension = Path.GetExtension(fileUploadProfilePic.FileName)?.ToLower().Trim();
                if (extension == ".jpg" || extension == ".jpeg" || extension == ".png")
                {
                    string fileName = username + extension;
                    string savePath = Server.MapPath("~/profilePictures/") + fileName;
                    fileUploadProfilePic.SaveAs(savePath);

                    string connStr = ConfigurationManager.ConnectionStrings["conStr"].ConnectionString;
                    using (SqlConnection con = new SqlConnection(connStr))
                    {
                        string query = "UPDATE RegistrationInfo SET profilePicturePath = @path WHERE username = @username";
                        using (SqlCommand cmd = new SqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@path", "~/profilePictures/" + fileName);
                            cmd.Parameters.AddWithValue("@username", username);
                            con.Open();
                            cmd.ExecuteNonQuery();
                        }
                    }

                    lblMessage.Text = "Profile picture uploaded successfully.";
                }
                else
                {
                    lblMessage.Text = "Only .jpg, .jpeg, and .png files are allowed.";
                }
            }
            else
            {
                lblMessage.Text = "Please select a file first.";
            }
        }


        protected void btnUploadHousePics_Click(object sender, EventArgs e)
        {
            string username = HttpContext.Current.User.Identity.Name;
            if (string.IsNullOrEmpty(username))
            {
                Response.Redirect("signIn.aspx");
                return;
            }

            string connStr = ConfigurationManager.ConnectionStrings["conStr"].ConnectionString;
            int? currentHomeID = null;

            using (SqlConnection con = new SqlConnection(connStr))
            {
                con.Open();
                string getHomeIDQuery = "SELECT homeID FROM UserAttribute WHERE username = @username";
                using (SqlCommand cmd = new SqlCommand(getHomeIDQuery, con))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    object result = cmd.ExecuteScalar();
                    if (result != DBNull.Value && result != null)
                        currentHomeID = Convert.ToInt32(result);
                }

                if (chkHasHouse.Checked == false)
                {
                    lblMessage.Text = "Ev ID'si bulunamadı. Lütfen önce ev bilgilerini kaydedin.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                    return;
                }

                HttpFileCollection uploadedFiles = Request.Files;
                bool filesUploaded = false;
                for (int i = 0; i < uploadedFiles.Count; i++)
                {
                    HttpPostedFile file = uploadedFiles[i];

                    if (file != null && file.ContentLength > 0)
                    {
                        string ext = Path.GetExtension(file.FileName)?.ToLower();
                        if (ext == ".jpg" || ext == ".jpeg" || ext == ".png")
                        {
                            string fileName = $"house_{currentHomeID}_{Guid.NewGuid()}{ext}";
                            string savePath = Server.MapPath("~/housePictures/") + fileName;
                            file.SaveAs(savePath);

                            string relativePath = "~/housePictures/" + fileName;
                            string insertPicQuery = "INSERT INTO HousePicture (username, imagePath) VALUES (@username, @imagePath)";
                            using (SqlCommand insertCmd = new SqlCommand(insertPicQuery, con))
                            {
                                insertCmd.Parameters.AddWithValue("@username", username);
                                insertCmd.Parameters.AddWithValue("@imagePath", relativePath);
                                insertCmd.ExecuteNonQuery();
                            }
                            filesUploaded = true;
                        }
                    }
                }

                if (filesUploaded)
                {
                    lblMessage.Text = "Ev resimleri başarıyla yüklendi!";
                    lblMessage.ForeColor = System.Drawing.Color.Green;
                }
                else
                {
                    lblMessage.Text = "Yüklenecek bir dosya seçilmedi veya geçersiz dosya tipi.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                }
            }
        }
        protected void btnDeleteHousePics_Click(object sender, EventArgs e)
        {
            string username = HttpContext.Current.User.Identity.Name;
            if (string.IsNullOrEmpty(username))
            {
                Response.Redirect("signIn.aspx");
                return;
            }

            string connStr = ConfigurationManager.ConnectionStrings["conStr"].ConnectionString;

            using (SqlConnection con = new SqlConnection(connStr))
            {
                con.Open();
                // Also delete associated house pictures
                SqlCommand deleteHousePics = new SqlCommand("DELETE FROM HousePicture WHERE username = @username", con);
                deleteHousePics.Parameters.AddWithValue("@username", username);
                deleteHousePics.ExecuteNonQuery();
            }
        }


        protected void chkHasHouse_CheckedChanged(object sender, EventArgs e)
        {
            pnlHouseInfo.Visible = chkHasHouse.Checked;
        }
    }
}