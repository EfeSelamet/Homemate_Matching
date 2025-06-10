using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;
using System.Web.Security; // FormsAuthentication için gerekli

namespace Homemate_Matching
{
    public partial class WebForm3 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Kullanıcı giriş yapmamışsa, giriş sayfasına yönlendir.
                if (!HttpContext.Current.User.Identity.IsAuthenticated)
                {
                    Response.Redirect("signIn.aspx");
                    return;
                }

                // Sayfa ilk yüklendiğinde mevcut kullanıcı adını label'a yazdır.
                lblUsername.Text = HttpContext.Current.User.Identity.Name;
            }
        }

        protected void btnSaveNameSurname_Click(object sender, EventArgs e)
        {
            // 1. Gerekli bilgileri al
            string username = HttpContext.Current.User.Identity.Name; // Hangi kullanıcıyı güncelleyeceğimizi bilmek için
            string newName = txtNewName.Text.Trim();
            string newSurname = txtNewSurname.Text.Trim();

            // 2. Girdilerin boş olup olmadığını kontrol et
            if (string.IsNullOrEmpty(newName) || string.IsNullOrEmpty(newSurname))
            {
                ShowMessage("İsim ve soyisim alanları boş bırakılamaz.", true);
                return;
            }

            // 3. Veritabanını güncelle
            string connStr = ConfigurationManager.ConnectionStrings["conStr"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connStr))
            {
                // Sadece tek bir tabloyu güncellediğimiz için bu işlem çok daha basit.
                // NOT: Tablo adının 'RegistrationInfo', sütun adlarının 'name' ve 'surname' olduğunu varsayıyorum.
                string query = "UPDATE RegistrationInfo SET firstName = @newName, lastName = @newSurname WHERE username = @username";

                try
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        // Parametreleri ekleyerek SQL Injection'a karşı koruma sağla
                        cmd.Parameters.AddWithValue("@newName", newName);
                        cmd.Parameters.AddWithValue("@newSurname", newSurname);
                        cmd.Parameters.AddWithValue("@username", username);

                        cmd.ExecuteNonQuery(); // Komutu çalıştır
                    }

                    ShowMessage("İsim ve soyisminiz başarıyla güncellendi.", false);
                }
                catch (Exception ex)
                {
                    ShowMessage("Güncelleme sırasında bir hata oluştu: " + ex.Message, true);
                }
            }
        }




        protected void btnSavePassword_Click(object sender, EventArgs e)
        {
            string username = HttpContext.Current.User.Identity.Name;
            string newPassword = txtNewPassword.Text;

            if (string.IsNullOrEmpty(newPassword))
            {
                ShowMessage("Yeni şifre boş olamaz.", true);
                return;
            }

            // GÜVENLİK UYARISI: Şifreler her zaman hash'lenerek saklanmalıdır!
            string connStr = ConfigurationManager.ConnectionStrings["conStr"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connStr))
            {
                try
                {
                    con.Open();
                    string query = "UPDATE RegistrationInfo SET password = @password WHERE username = @username";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@password", newPassword);
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.ExecuteNonQuery();
                    }
                    ShowMessage("Şifre başarıyla güncellendi.", false);
                    txtNewPassword.Text = "";
                }
                catch (Exception ex)
                {
                    ShowMessage("Şifre güncellenirken bir hata oluştu: " + ex.Message, true);
                }
            }
        }

        protected void btnConfirmDelete_Click(object sender, EventArgs e)
        {
            string username = HttpContext.Current.User.Identity.Name;
            string connStr = ConfigurationManager.ConnectionStrings["conStr"].ConnectionString;

            using (SqlConnection con = new SqlConnection(connStr))
            {
                con.Open();
                SqlTransaction transaction = con.BeginTransaction("DeleteUserTransaction");

                try
                {
                    // Adım 1: Kullanıcının ev ID'sini al (varsa daha sonra evi silmek için)
                    int? homeID = null;
                    string getHomeIdQuery = "SELECT homeID FROM UserAttribute WHERE username = @username";
                    using (SqlCommand cmd = new SqlCommand(getHomeIdQuery, con, transaction))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            homeID = Convert.ToInt32(result);
                        }
                    }

                    // Adım 2: Tüm "çocuk" tablolardan kullanıcıyla ilgili kayıtları sil.
                    // Sıralama burada çok önemli değil, yeter ki RegistrationInfo'dan önce olsunlar.

                    // Tek 'username' sütunu olanlar
                    DeleteFromTable("HousePicture", "username", username, con, transaction);
                    DeleteFromTable("UserPreference", "username", username, con, transaction);

                    // 'viewer' veya 'viewed' sütunu olanlar
                    DeleteFromTable("Accept", "viewer", username, con, transaction);
                    DeleteFromTable("Accept", "viewed", username, con, transaction);
                    DeleteFromTable("Reject", "viewer", username, con, transaction);
                    DeleteFromTable("Reject", "viewed", username, con, transaction);

                    // 'username1' veya 'username2' sütunu olanlar (tek komutta OR ile silinir)
                    string deleteMatchQuery = "DELETE FROM Match WHERE username1 = @username OR username2 = @username";
                    using (SqlCommand cmd = new SqlCommand(deleteMatchQuery, con, transaction))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.ExecuteNonQuery();
                    }

                    string deleteMessageQuery = "DELETE FROM Message WHERE username1 = @username OR username2 = @username";
                    using (SqlCommand cmd = new SqlCommand(deleteMessageQuery, con, transaction))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.ExecuteNonQuery();
                    }

                    // UserAttribute tablosu
                    DeleteFromTable("UserAttribute", "username", username, con, transaction);

                    // Adım 3: Eğer kullanıcının evi varsa, Home tablosundan sil
                    if (homeID.HasValue)
                    {
                        string deleteHomeQuery = "DELETE FROM Home WHERE homeID = @homeID";
                        using (SqlCommand cmd = new SqlCommand(deleteHomeQuery, con, transaction))
                        {
                            cmd.Parameters.AddWithValue("@homeID", homeID.Value);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // Adım 4: EN SON olarak ana kullanıcı kaydını (RegistrationInfo) sil
                    DeleteFromTable("RegistrationInfo", "username", username, con, transaction);

                    transaction.Commit();

                    // Kullanıcıyı sistemden çıkar ve ana sayfaya yönlendir.
                    FormsAuthentication.SignOut();
                    Response.Redirect("Default.aspx?message=Hesabınız başarıyla silindi.");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    ShowMessage("Hesap silinirken bir hata oluştu: " + ex.Message, true);
                }
            }
        }

        // Silme işlemini basitleştirmek için bir yardımcı metod
        private void DeleteFromTable(string tableName, string columnName, string username, SqlConnection con, SqlTransaction transaction)
        {
            string query = $"DELETE FROM {tableName} WHERE {columnName} = @username";
            using (SqlCommand cmd = new SqlCommand(query, con, transaction))
            {
                cmd.Parameters.AddWithValue("@username", username);
                cmd.ExecuteNonQuery();
            }
        }

        private void ShowMessage(string message, bool isError)
        {
            lblMessage.Text = message;
            lblMessage.ForeColor = isError ? System.Drawing.Color.Red : System.Drawing.Color.Green;
        }
    }
}