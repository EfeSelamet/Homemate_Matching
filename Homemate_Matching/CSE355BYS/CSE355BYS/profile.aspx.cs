using System;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web;
using System.Text;
using System.Globalization;

namespace Homemate_Matching
{
    public partial class profile : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadUserProfile();
            }
        }

        private void LoadUserProfile()
        {
            string username = HttpContext.Current.User.Identity.Name;
            string connectionString = ConfigurationManager.ConnectionStrings["conStr"].ConnectionString;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                try
                {
                    con.Open();

                    // --- UPDATED SQL QUERY ---
                    // Using explicit column names with aliases for UserPreference to avoid ambiguity.
                    string masterQuery = @"
                        SELECT 
                            r.firstName, r.lastName, r.ProfilePicturePath,
                            ua.gender, ua.sleepSchedule, ua.isSmoker, ua.isDrinker, ua.Hygiene, ua.noisiness, ua.GuestFrequency, ua.IsStudent, ua.homeID,
                            p.gender AS PrefGender, 
                            p.sleepSchedule AS PrefSleepSchedule,
                            p.noSmoker AS PrefIsSmoker,
                            p.noDrinker AS PrefIsDrinker,
                            p.minBudget, p.StudentPref,p.maxBudget, p.desiredHomeLocation, p.SignificanceOfHygiene, p.NoiseSensitivity, p.GuestPreference,
                            h.Location, h.NumberOfRooms, h.NumberOfSaloon, h.Rent, h.Description, h.SurfaceArea, h.FlatFloor, h.BuildingFloorCount, h.BuildingAge, h.Heating
                        FROM RegistrationInfo r
                        LEFT JOIN UserAttribute ua ON r.username = ua.username
                        LEFT JOIN UserPreference p ON r.username = p.username
                        LEFT JOIN Home h ON ua.homeID = h.homeID
                        WHERE r.username = @username";

                    using (SqlCommand cmd = new SqlCommand(masterQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Populate Profile Header
                                lblName.Text = $"{reader["firstName"]} {reader["lastName"]}";
                                var path = reader["ProfilePicturePath"]?.ToString();
                                imgUserProfile.ImageUrl = !string.IsNullOrEmpty(path) ? path : "~/images/default-avatar.png";

                                // Populate sections
                                PopulateAttributes(reader);
                                PopulateHouseInfo(reader, con);
                                PopulatePreferences(reader); 
                            }
                            else
                            {
                                lblName.Text = "User not found.";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Error logging
                    lblName.Text = "An error occurred while loading your profile.";
                    System.Diagnostics.Debug.WriteLine("Profile Load Error: " + ex.ToString());
                }
            }
        }

        private void PopulateAttributes(SqlDataReader reader)
        {
            if (reader["gender"] == DBNull.Value)
            {
                lblAttributes.Text = "No attributes entered yet.";
                return;
            }
            var html = new StringBuilder("<div class='attributes-grid'>");
            string gender = reader["gender"].ToString() == "f" ? "Kadın" : "Erkek";
            html.Append($"<div class='attribute-item'><span class='attribute-icon'>{(gender == "Erkek" ? "♂️" : "♀️")}</span><div><strong>Cinsiyet:</strong> {gender}</div></div>");
            html.Append($"<div class='attribute-item'><span class='attribute-icon'>😴</span><div><strong>Uyku Düzeni:</strong> {reader["sleepSchedule"]}</div></div>");
            html.Append($"<div class='attribute-item'><span class='attribute-icon'>{((bool)reader["isSmoker"] ? "🚬" : "🚭")}</span><div><strong>Sigara:</strong> {((bool)reader["isSmoker"] ? "Evet" : "Hayır")}</div></div>");
            html.Append($"<div class='attribute-item'><span class='attribute-icon'>{((bool)reader["isDrinker"] ? "🍻" : "🚫")}</span><div><strong>Alkol:</strong> {((bool)reader["isDrinker"] ? "Evet" : "Hayır")}</div></div>");
            html.Append($"<div class='attribute-item'><span class='attribute-icon'>🧼</span><div><strong>Hijyen:</strong> {reader["Hygiene"]}</div></div>");
            html.Append($"<div class='attribute-item'><span class='attribute-icon'>🔊</span><div><strong>Gürültü:</strong> {reader["noisiness"]}</div></div>");
            html.Append($"<div class='attribute-item'><span class='attribute-icon'>👥</span><div><strong>Misafir Sıklığı:</strong> {reader["GuestFrequency"]}</div></div>");
            html.Append($"<div class='attribute-item'><span class='attribute-icon'>{((bool)reader["IsStudent"] ? "🎓" : "🧑‍💼")}</span><div><strong>Öğrenci:</strong> {((bool)reader["IsStudent"] ? "Evet" : "Hayır")}</div></div>");
            html.Append("</div>");
            lblAttributes.Text = html.ToString();
        }

        private void PopulateHouseInfo(SqlDataReader reader, SqlConnection con)
        {
            if (reader["homeID"] == DBNull.Value)
            {
                pnlMyHome.Visible = false;
                return;
            }
            string username = HttpContext.Current.User.Identity.Name;

            pnlMyHome.Visible = true;
            var html = new StringBuilder("<div class='attributes-grid'>");
            html.Append($"<div class='attribute-item'><span class='attribute-icon'>📍</span><div><strong>Konum:</strong> {reader["Location"]}</div></div>");
            html.Append($"<div class='attribute-item'><span class='attribute-icon'>💰</span><div><strong>Kira:</strong> {Convert.ToDecimal(reader["Rent"]).ToString("N0", new CultureInfo("tr-TR"))}₺</div></div>");
            html.Append($"<div class='attribute-item'><span class='attribute-icon'>🚪</span><div><strong>Oda Sayısı:</strong> {reader["NumberOfRooms"]} + {reader["NumberOfSaloon"]}</div></div>");
            html.Append($"<div class='attribute-item'><span class='attribute-icon'>📏</span><div><strong>Alan:</strong> {reader["SurfaceArea"]} m²</div></div>");
            html.Append($"<div class='attribute-item-full'><span class='attribute-icon'>📝</span><div><strong>Ek Bilgi:</strong> {reader["Description"]}</div></div>");
            html.Append("</div>");
            lblHouseInfo.Text = html.ToString();

            string getPicsQuery = "SELECT imagePath FROM HousePicture WHERE username = @username";
            using (SqlCommand picCmd = new SqlCommand(getPicsQuery, con))
            {
                picCmd.Parameters.AddWithValue("@username", username);
                var picsHtml = new StringBuilder("<h5 class='mt-4'>Ev Fotoğrafı:</h5>");
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
                        picsHtml.Append("<span>Henüz Fotoğraf Yüklenmedi.</span>");
                    }
                }
                houseImagesDiv.InnerHtml = picsHtml.ToString();
            }
        }

        private void PopulatePreferences(SqlDataReader reader)
        {
            // --- UPDATED: Using aliased column names like 'PrefGender' ---
            if (reader["PrefGender"] == DBNull.Value)
            {
                lblPreferences.Text = "Henüz tercih yapılmadı.";
                return;
            }
            var html = new StringBuilder("<div class='attributes-grid'>");
            html.Append($"<div class='attribute-item'><span class='attribute-icon'>🚻</span><div><strong>Tercih Edilen Cinsiyet:</strong> {(reader["PrefGender"].ToString() == "m" ? "Erkek" : (reader["PrefGender"].ToString() == "f" ? "Kadın" : "Farketmez"))}</div></div>");
            html.Append($"<div class='attribute-item'><span class='attribute-icon'>🌙</span><div><strong>Tercih edilen  uyku düzeni:</strong> {reader["PrefSleepSchedule"]}</div></div>");
            html.Append($"<div class='attribute-item'><span class='attribute-icon'>{((bool)reader["PrefIsSmoker"] ? "🚭" : "🚬")}</span><div><strong>Sigara içmesin:</strong> {((bool)reader["PrefIsSmoker"] ? "Evet" : "İçebilir")}</div></div>");
            html.Append($"<div class='attribute-item'><span class='attribute-icon'>{((bool)reader["PrefIsDrinker"] ? "🚫" : "🍻")}</span><div><strong>Alkol kullanmasın:</strong> {((bool)reader["PrefIsDrinker"] ? "Evet" : "Kullanabilir")}</div></div>");
            html.Append($"<div class='attribute-item'><span class='attribute-icon'>🎯</span><div><strong>Ev konumu:</strong> {reader["desiredHomeLocation"]}</div></div>");
            html.Append($"<div class='attribute-item'><span class='attribute-icon'>💸</span><div><strong>Bütçe:</strong> {Convert.ToDecimal(reader["minBudget"]).ToString("N0", new CultureInfo("tr-TR"))}₺ - {Convert.ToDecimal(reader["maxBudget"]).ToString("N0", new CultureInfo("tr-TR"))}₺</div></div>");
            html.Append($"<div class='attribute-item'><span class='attribute-icon'>✨</span><div><strong>Hijyenin önemi:</strong> {reader["SignificanceOfHygiene"]}</div></div>");
            html.Append($"<div class='attribute-item'><span class='attribute-icon'>🎧</span><div><strong>Ses hassasiyeti:</strong> {reader["NoiseSensitivity"]}</div></div>");
            html.Append($"<div class='attribute-item'><span class='attribute-icon'>🎓</span><div><strong>Öğrenci Tercihi:</strong> {((bool)reader["StudentPref"] ? "Evet" : "Hayır")}</div></div>");
            html.Append("</div>");
            lblPreferences.Text = html.ToString();
        }

        // --- Button Click Events ---
        protected void userAttributeBtn_Click(object sender, EventArgs e)
        {
            Response.Redirect("userAttributes.aspx");
        }
        protected void userPreferenceBtn_Click(object sender, EventArgs e)
        {
            Response.Redirect("userPreference.aspx");
        }
        protected void matchBtn_Click(object sender, EventArgs e)
        {
            Response.Redirect("matchUp.aspx");
        }
    }
}