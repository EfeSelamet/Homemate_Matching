using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;

namespace Homemate_Matching
{
    /// <summary>
    /// Diğer kullanıcılarla uyumluluk puanını hesaplamak için kullanılan veri modelidir.
    /// </summary>
    public class MatchCandidate
    {
        public string Username { get; set; }
        public int CompatibilityScore { get; set; }

        // UserAttribute'dan gelen alanlar
        public string Gender { get; set; }
        public string SleepSchedule { get; set; }
        public bool IsSmoker { get; set; }
        public bool IsDrinker { get; set; }
        public int? Hygiene { get; set; }
        public int? Noisiness { get; set; }
        public int? GuestFrequency { get; set; }
        public bool IsStudent { get; set; }

        // UserPreference'dan gelen alanlar
        public string PrefGender { get; set; }
        public string PrefSleepSchedule { get; set; }
        public bool? NoSmokerPref { get; set; } // NoSmoker, isSmoker'ın tersi gibi çalışır
        public bool? NoDrinkerPref { get; set; } // NoDrinker, isDrinker'ın tersi gibi çalışır
        public int? SignificanceOfHygiene { get; set; }
        public int? NoiseSensitivity { get; set; }
        public int? GuestPreference { get; set; }
        public bool? StudentPref { get; set; }
    }

    /// <summary>
    /// Kullanıcılar arasında uyumluluk skorlarını hesaplar ve en iyi eşleşmeleri bulur.
    /// </summary>
    public class MatchmakingService
    {
        private readonly string _connectionString;

        public MatchmakingService()
        {
            // Web.config veya App.config dosyanızdaki bağlantı dizisini alır.
            _connectionString = ConfigurationManager.ConnectionStrings["conStr"].ConnectionString;
        }

        /// <summary>
        /// Belirtilen bir kullanıcı için en uyumlu 10 kullanıcıyı bulur.
        /// </summary>
        /// <param name="currentUsername">Skorların hesaplanacağı ana kullanıcının adı.</param>
        /// <returns>En yüksek skora sahip 10 kullanıcının kullanıcı adlarını içeren bir liste.</returns>
        public List<string> FindTopMatches(string currentUsername)
        {
            List<MatchCandidate> allUsers = GetAllUserData(currentUsername);

            MatchCandidate currentUser = allUsers.FirstOrDefault(u => u.Username.Equals(currentUsername, StringComparison.OrdinalIgnoreCase));
            List<MatchCandidate> otherUsers = allUsers.Where(u => !u.Username.Equals(currentUsername, StringComparison.OrdinalIgnoreCase)).ToList();

            if (currentUser == null)
            {
                // Belirtilen kullanıcı bulunamazsa boş liste döndür.
                return new List<string>();
            }
            System.Diagnostics.Debug.WriteLine("========== EŞLEŞTİRME SKORLAMA BAŞLADI ==========");

            // Her bir kullanıcı için uyumluluk skorunu hesapla
            foreach (var candidate in otherUsers)
            {
                var scoreResult = CalculateScore(currentUser, candidate);

                candidate.CompatibilityScore = scoreResult;

                // --- DEĞİŞİKLİK: Veritabanına kaydetmek yerine konsola yazdır ---
                System.Diagnostics.Debug.WriteLine($"Hesaplama: [{currentUser.Username}] vs [{candidate.Username}]");
                System.Diagnostics.Debug.WriteLine($" -> Toplam Skor: {candidate.CompatibilityScore}");
                System.Diagnostics.Debug.WriteLine("-------------------------------------------------");
                // --- DEĞİŞİKLİK SONU ---
            }
            System.Diagnostics.Debug.WriteLine("========== EŞLEŞTİRME SKORLAMA BİTTİ ==========");
            // Kullanıcıları skora göre azalan şekilde sırala ve ilk 10'unu al
            return otherUsers
                .OrderByDescending(u => u.CompatibilityScore)
                .Take(10)
                .Select(u => u.Username)
                .ToList();
        }

        /// <summary>
        /// İki kullanıcı arasındaki uyumluluk skorunu hesaplar.
        /// </summary>
        private int CalculateScore(MatchCandidate userA, MatchCandidate userB)
        {
            int score = 0;

            // --- İki Yönlü Karşılaştırma ---

            // 1. Cinsiyet
            if (userA.PrefGender != "i" && userA.PrefGender != userB.Gender) score -= 100;
            if (userB.PrefGender != "i" && userB.PrefGender != userA.Gender) score -= 100;

            // 2. Sigara
            // A'nın tercihi, B'nin durumuyla eşleşiyor mu?
            if (userA.NoSmokerPref == true && !userB.IsSmoker) score += 15; // A sigara içmeyen birini istiyor ve B içmiyor (Güçlü Eşleşme)
            if (userA.NoSmokerPref == true && userB.IsSmoker) score -= 50;  // A sigara içmeyen birini istiyor ama B içiyor (Anlaşmazlık)
            // B'nin tercihi, A'nın durumuyla eşleşiyor mu?
            if (userB.NoSmokerPref == true && !userA.IsSmoker) score += 15;
            if (userB.NoSmokerPref == true && userA.IsSmoker) score -= 50;

            // 3. Alkol
            if (userA.NoDrinkerPref == true && !userB.IsDrinker) score += 15;
            if (userA.NoDrinkerPref == true && userB.IsDrinker) score -= 50;
            if (userB.NoDrinkerPref == true && !userA.IsDrinker) score += 15;
            if (userB.NoDrinkerPref == true && userA.IsDrinker) score -= 50;

            // 4. Öğrenci Durumu
            if (userA.StudentPref.HasValue && userA.StudentPref == userB.IsStudent) score += 5;
            if (userB.StudentPref.HasValue && userB.StudentPref == userA.IsStudent) score += 5;

            // 5. Sayısal Değerler (Hijyen, Gürültü, Misafir) - Fark azaldıkça puan artar (Maks 10 puan)
            score += CalculateNumericalScore(userA.Hygiene, userB.SignificanceOfHygiene);
            score += CalculateNumericalScore(userB.Hygiene, userA.SignificanceOfHygiene);

            score += CalculateNumericalScore(userA.Noisiness, userB.NoiseSensitivity);
            score += CalculateNumericalScore(userB.Noisiness, userA.NoiseSensitivity);

            score += CalculateNumericalScore(userA.GuestFrequency, userB.GuestPreference);
            score += CalculateNumericalScore(userB.GuestFrequency, userA.GuestPreference);

            return score;
        }

        /// <summary>
        /// Sayısal nitelikler ve tercihler arasındaki uyum için bir puan hesaplar.
        /// Varsayılan olarak 10 puanlık bir ölçek kullanır.
        /// </summary>
        private int CalculateNumericalScore(int? attribute, int? preference, int maxScore = 20)
        {
            if (!attribute.HasValue || !preference.HasValue)
            {
                return 0; // Eğer veri yoksa puan yok.
            }
            // İki değer arasındaki fark ne kadar azsa, puan o kadar yüksek olur.
            int difference = (int)(Math.Pow(Math.Abs(attribute.Value - preference.Value), 2)/ 2.5);
            int score = maxScore - difference;
            return Math.Max(0, score); // Puanın negatif olmasını engelle.
        }

        /// <summary>
        /// Veritabanından tüm kullanıcıların nitelik ve tercih verilerini çeker.
        /// </summary>
        private List<MatchCandidate> GetAllUserData(string username)
        {
            var users = new List<MatchCandidate>();

            // Sütun adı çakışmalarını önlemek için alias (takma ad) kullanıldı.
            string query = @"
                SELECT 
                    ua.username,
                    ua.gender, ua.sleepSchedule, ua.isSmoker, ua.isDrinker, ua.Hygiene, ua.noisiness, ua.GuestFrequency, ua.IsStudent,
                    up.gender AS PrefGender, up.sleepSchedule AS PrefSleepSchedule, up.NoSmoker, up.NoDrinker, up.SignificanceOfHygiene, up.NoiseSensitivity, up.GuestPreference, up.StudentPref
                FROM UserAttribute ua
                INNER JOIN UserPreference up ON ua.username = up.username
                WHERE ua.username NOT IN (
                SELECT viewed FROM Accept WHERE viewer = @username
                UNION
                SELECT viewed FROM Reject WHERE viewer = @username
                UNION
                SELECT viewer FROM Reject WHERE viewed = @username
                UNION
                SELECT username2 FROM Match WHERE username1 = @username
                UNION
                SELECT username1 FROM Match WHERE username2 = @username
            )";
            using (var con = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    cmd.Parameters.AddWithValue("@username", username);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            users.Add(new MatchCandidate
                            {
                                Username = reader["username"].ToString(),
                                // Attribute'ları doldur
                                Gender = reader["gender"] != DBNull.Value ? reader["gender"].ToString() : null,
                                SleepSchedule = reader["sleepSchedule"] != DBNull.Value ? reader["sleepSchedule"].ToString() : null,
                                IsSmoker = reader["isSmoker"] != DBNull.Value ? (bool)reader["isSmoker"] : false,
                                IsDrinker = reader["isDrinker"] != DBNull.Value ? (bool)reader["isDrinker"] : false,
                                Hygiene = reader["Hygiene"] != DBNull.Value ? (int?)reader["Hygiene"] : null,
                                Noisiness = reader["noisiness"] != DBNull.Value ? (int?)reader["noisiness"] : null,
                                GuestFrequency = reader["GuestFrequency"] != DBNull.Value ? (int?)reader["GuestFrequency"] : null,
                                IsStudent = reader["IsStudent"] != DBNull.Value ? (bool)reader["IsStudent"] : false,

                                // Preference'ları doldur
                                PrefGender = reader["PrefGender"] != DBNull.Value ? reader["PrefGender"].ToString() : null,
                                PrefSleepSchedule = reader["PrefSleepSchedule"] != DBNull.Value ? reader["PrefSleepSchedule"].ToString() : null,
                                NoSmokerPref = reader["NoSmoker"] != DBNull.Value ? (bool?)reader["NoSmoker"] : null,
                                NoDrinkerPref = reader["NoDrinker"] != DBNull.Value ? (bool?)reader["NoDrinker"] : null,
                                SignificanceOfHygiene = reader["SignificanceOfHygiene"] != DBNull.Value ? (int?)reader["SignificanceOfHygiene"] : null,
                                NoiseSensitivity = reader["NoiseSensitivity"] != DBNull.Value ? (int?)reader["NoiseSensitivity"] : null,
                                GuestPreference = reader["GuestPreference"] != DBNull.Value ? (int?)reader["GuestPreference"] : null,
                                StudentPref = reader["StudentPref"] != DBNull.Value ? (bool?)reader["StudentPref"] : null,
                            });
                        }
                    }
                }
            }
            return users;
        }
    }
}