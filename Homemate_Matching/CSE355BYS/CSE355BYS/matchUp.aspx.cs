using System;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI;
using static System.Net.Mime.MediaTypeNames;
// using System.Activities.Expressions; // Bu sat?r kald?r?ld?
using System.Web.UI.WebControls;
using System.Xml.Serialization;
using Microsoft.AspNet.SignalR.Infrastructure;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Web;
using System.Reflection;
using System.Text;
using System.Globalization;


namespace Homemate_Matching
{
    public partial class matchUp : System.Web.UI.Page
    {
        public List<string> GetPotentialUsernames(string currentUsername)
        {
            MatchmakingService ms = new MatchmakingService();
            return ms.FindTopMatches(currentUsername);
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string username = HttpContext.Current.User.Identity.Name;
                string connectionString = ConfigurationManager.ConnectionStrings["conStr"].ConnectionString;

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    try
                    {
                        con.Open();
                        string[] potMatch = GetPotentialUsernames(username).ToArray();
                        Random random = new Random();
                        int potUsers = potMatch.Length;

                        // UPDATED: Control panel visibility for a cleaner UI
                        if (potUsers == 0)
                        {
                            pnlMatchContent.Visible = false; // Hide the match card
                            pnlNoMatches.Visible = true;    // Show the "No new users" message
                        }
                        else
                        {
                            pnlMatchContent.Visible = true;
                            pnlNoMatches.Visible = false;

                            int randomIndex = random.Next(potMatch.Length);
                            string randomUsername = potMatch[randomIndex];
                            Session["MatchedUser"] = randomUsername;

                            // Profile picture logic remains the same...
                            string picQuery = "SELECT ProfilePicturePath FROM RegistrationInfo WHERE username = @username";
                            SqlCommand cmd = new SqlCommand(picQuery, con);
                            cmd.Parameters.AddWithValue("@username", randomUsername);
                            var path = cmd.ExecuteScalar()?.ToString();
                            if (!string.IsNullOrEmpty(path))
                            {
                                imgPotUserProfile.ImageUrl = path;
                            }
                            else
                            {
                                imgPotUserProfile.ImageUrl = "~/images/default-avatar.png"; // Set a default
                            }

                            // User info logic remains the same...
                            string userInfoQuery = "SELECT firstName, lastName, BirthDate FROM RegistrationInfo WHERE username = @username";
                            SqlCommand userInfoCmd = new SqlCommand(userInfoQuery, con);
                            userInfoCmd.Parameters.AddWithValue("@username", randomUsername);
                            SqlDataReader reader = userInfoCmd.ExecuteReader();
                            if (reader.Read())
                            {
                                lblName.Text = reader["firstName"] + " " + reader["lastName"]; // Just the name for the header
                                lblBirthDate.Text = "Doğum Tarihi: " + Convert.ToDateTime(reader["BirthDate"]).ToString("yyyy-MM-dd");
                            }
                            else
                            {
                                lblName.Text = "Kullanıcı Bulunamadı.";
                                return;
                            }
                            reader.Close();

                            // UPDATED: Attribute logic to create the visual grid
                            string query = "SELECT * FROM UserAttribute WHERE username = @username";
                            SqlCommand attrCmd = new SqlCommand(query, con);
                            attrCmd.Parameters.AddWithValue("@username", randomUsername);
                            reader = attrCmd.ExecuteReader();
                            if (reader.Read())
                            {
                                // REUSE logic from profile page for visual consistency
                                var attributesHtml = new System.Text.StringBuilder("<div class='attributes-grid'>");
                                string gender = reader["gender"].ToString() == "m" ? "Erkek" : "Kadın";
                                string genderIcon = gender == "Erkek" ? "♂️" : "♀️";
                                attributesHtml.Append($"<div class='attribute-item'><span class='attribute-icon'>{genderIcon}</span> <span class='attribute-label'>Cinsiyet:</span> <span class='attribute-value'>{gender}</span></div>");
                                attributesHtml.Append($"<div class='attribute-item'><span class='attribute-icon'>😴</span> <span class='attribute-label'>Uyku Düzeni:</span> <span class='attribute-value'>{reader["sleepSchedule"]}</span></div>");
                                bool isSmoker = (bool)reader["isSmoker"];
                                string smokerText = isSmoker ? "Evet" : "Hayır";
                                string smokerIcon = isSmoker ? "🚬" : "🚭";
                                attributesHtml.Append($"<div class='attribute-item'><span class='attribute-icon'>{smokerIcon}</span> <span class='attribute-label'>Sigara:</span> <span class='attribute-value'>{smokerText}</span></div>");
                                bool isDrinker = (bool)reader["isDrinker"];
                                string drinkerText = isDrinker ? "Evet" : "Hayır";
                                string drinkerIcon = isDrinker ? "🍻" : "🚫";
                                attributesHtml.Append($"<div class='attribute-item'><span class='attribute-icon'>{drinkerIcon}</span> <span class='attribute-label'>Alkol:</span> <span class='attribute-value'>{drinkerText}</span></div>");
                                attributesHtml.Append($"<div class='attribute-item'><span class='attribute-icon'>🧼</span> <span class='attribute-label'>Hijyen:</span> <span class='attribute-value'>{reader["Hygiene"]}</span></div>");
                                attributesHtml.Append($"<div class='attribute-item'><span class='attribute-icon'>🔊</span> <span class='attribute-label'>Gürültü:</span> <span class='attribute-value'>{reader["noisiness"]}</span></div>");
                                attributesHtml.Append($"<div class='attribute-item'><span class='attribute-icon'>👥</span> <span class='attribute-label'>Misafir Sıklığı:</span> <span class='attribute-value'>{reader["GuestFrequency"]}</span></div>");
                                bool isStudent = (bool)reader["IsStudent"];
                                string studentText = isStudent ? "Evet" : "Hayır";
                                string studentIcon = isStudent ? "🎓" : "🧑‍💼";
                                attributesHtml.Append($"<div class='attribute-item'><span class='attribute-icon'>{studentIcon}</span> <span class='attribute-label'>Öğrenci:</span> <span class='attribute-value'>{studentText}</span></div>");
                                attributesHtml.Append("</div>");
                                lblAttributes.Text = attributesHtml.ToString();
                            }
                            else
                            {
                                lblAttributes.Text = "No attributes entered yet.";
                            }
                            reader.Close();
                            try
                            {
                                string houseQuery = "SELECT * FROM Home h INNER JOIN UserAttribute u ON u.homeID = h.homeID WHERE u.username = @username";
                                using (SqlCommand houseCmd = new SqlCommand(houseQuery, con))
                                {
                                    houseCmd.Parameters.AddWithValue("@username", randomUsername);
                                    using (SqlDataReader houseReader = houseCmd.ExecuteReader())
                                    {
                                        if (houseReader.Read())
                                        {
                                            // UPDATED: Build a styled grid with icons for house info
                                            var houseHtml = new System.Text.StringBuilder("<div class='house-info-grid'>");

                                            houseHtml.Append($"<div class='house-item'><span class='house-icon'>📍</span><div><strong>Konum:</strong> {houseReader["Location"]}</div></div>");
                                            houseHtml.Append($"<div class='house-item'><span class='house-icon'>💰</span><div><strong>Kira:</strong> {Convert.ToDecimal(houseReader["Rent"]).ToString("N0", new CultureInfo("tr-TR"))}₺</div></div>");
                                            houseHtml.Append($"<div class='house-item'><span class='house-icon'>🚪</span><div><strong>Oda Sayısı:</strong> {houseReader["NumberOfRooms"]} + {houseReader["NumberOfSaloon"]}</div></div>");
                                            houseHtml.Append($"<div class='house-item'><span class='house-icon'>📏</span><div><strong>Alan:</strong> {houseReader["SurfaceArea"]} m²</div></div>");
                                            houseHtml.Append($"<div class='house-item'><span class='house-icon'>🏢</span><div><strong>Kat:</strong> {houseReader["FlatFloor"]} / {houseReader["BuildingFloorCount"]}</div></div>");
                                            houseHtml.Append($"<div class='house-item'><span class='house-icon'>🗓️</span><div><strong>Bina Yaşı:</strong> {houseReader["BuildingAge"]} yaşında</div></div>");
                                            houseHtml.Append($"<div class='house-item'><span class='house-icon'>🔥</span><div><strong>Isıtma:</strong> {houseReader["Heating"]}</div></div>");

                                            // Description can span full width
                                            houseHtml.Append($"<div class='house-item house-item-full'><span class='house-icon'>📝</span><div><strong>Ek bilgi:</strong> {houseReader["Description"]}</div></div>");

                                            houseHtml.Append("</div>");
                                            lblHouseInfo.Text = houseHtml.ToString();
                                            pnlHouseInfo.Visible = true; // Make the entire section visible

                                            string getPicsQuery = "SELECT imagePath FROM HousePicture WHERE username = @username";
                                            using (SqlCommand picCmd = new SqlCommand(getPicsQuery, con))
                                            {
                                                picCmd.Parameters.AddWithValue("@username", randomUsername);
                                                var picsHtml = new StringBuilder("<h5 class='mt-4'>Ev Resmi:</h5>");
                                                using (SqlDataReader picReader = picCmd.ExecuteReader())
                                                {
                                                    if (picReader.HasRows)
                                                    {
                                                        while (picReader.Read())
                                                        {
                                                            string rawPath = picReader["imagePath"].ToString();
                                                            string resolvedPath = ResolveUrl(rawPath);
                                                            picsHtml.Append($"<img src='{resolvedPath}' style='max-width:200px; margin:5px; border-radius:8px;' />");

                                                        }
                                                    }
                                                    else
                                                    {
                                                        picsHtml.Append("<span>Fotoğraf Yüklenmedi.</span>");
                                                    }
                                                }
                                                houseImagesDiv.InnerHtml = picsHtml.ToString();
                                            }
                                        }
                                        else
                                        {
                                            pnlHouseInfo.Visible = false; // Hide the section if no house is listed
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                // Your existing error handling for this block
                                string safeErrorMessage = System.Web.HttpUtility.JavaScriptStringEncode(ex.Message);
                                ClientScript.RegisterStartupScript(this.GetType(), "houseInfoError", $@"
                                <script>
                                     console.error('Error fetching house details: {safeErrorMessage}');
                                </script>", false);
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        lblError.Text = "Database connection error: " + ex.Message;
                        pnlMatchContent.Visible = false;
                        pnlNoMatches.Visible = true;
                    }
                }
            }
        }
        protected void acceptBtn_Click(object sender, EventArgs e)
        {

            string viewer = HttpContext.Current.User.Identity.Name;
            string viewed = Session["MatchedUser"].ToString(); // matched user selected during Page_Load

            string connStr = ConfigurationManager.ConnectionStrings["conStr"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connStr))
            {
                con.Open();

                // 1. Insert into Accept table
                string insertAccept = "INSERT INTO Accept (viewer, viewed) VALUES (@viewer, @viewed)";
                using (SqlCommand insertCmd = new SqlCommand(insertAccept, con))
                {
                    insertCmd.Parameters.AddWithValue("@viewer", viewer);
                    insertCmd.Parameters.AddWithValue("@viewed", viewed);
                    insertCmd.ExecuteNonQuery();
                }

                // 2. Check if other user already accepted current user
                string checkMutual = "SELECT COUNT(*) FROM Accept WHERE viewer = @viewed AND viewed = @viewer";
                using (SqlCommand checkCmd = new SqlCommand(checkMutual, con))
                {
                    checkCmd.Parameters.AddWithValue("@viewer", viewer);
                    checkCmd.Parameters.AddWithValue("@viewed", viewed);

                    int isMutual = (int)checkCmd.ExecuteScalar();
                    if (isMutual > 0)
                    {
                        // 3. Insert into Match table
                        string insertMatch = "INSERT INTO Match (username1, username2) VALUES (@user1, @user2)";
                        using (SqlCommand matchCmd = new SqlCommand(insertMatch, con))
                        {
                            matchCmd.Parameters.AddWithValue("@user1", viewer);
                            matchCmd.Parameters.AddWithValue("@user2", viewed);
                            matchCmd.ExecuteNonQuery();
                        }
                    }
                }
            }

            // Redirect to load next potential match
            Response.Redirect("matchUp.aspx");
        }


        protected void rejectBtn_Click(object sender, EventArgs e)
        {
            string viewer = HttpContext.Current.User.Identity.Name;
            string viewed = Session["MatchedUser"].ToString();
            string connStr = ConfigurationManager.ConnectionStrings["conStr"].ConnectionString;

            using (SqlConnection con = new SqlConnection(connStr))
            {
                string insertQuery = "INSERT INTO Reject (viewer, viewed) VALUES (@viewer, @viewed)";
                SqlCommand cmd = new SqlCommand(insertQuery, con);
                cmd.Parameters.AddWithValue("@viewer", viewer);
                cmd.Parameters.AddWithValue("@viewed", viewed);

                try
                {
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    lblError.Text = "Error rejecting match: " + ex.Message;
                    return;
                }
            }

            Response.Redirect("matchUp.aspx"); // Refresh to show next match
        }

    }
}
