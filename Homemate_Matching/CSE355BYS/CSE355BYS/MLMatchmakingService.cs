// Gerekli using bildirimleri
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Microsoft.ML; // ML.NET için gerekli

namespace Homemate_Matching.Services
{
    #region ML Modeli Veri Sınıfları
    // Bu sınıflar, ML modelini eğitirken kullandıklarımızla birebir aynı olmalıdır.
    // İdeal olarak ayrı bir "Shared" veya "Contracts" projesinde tutulurlar.

    public class ModelInput
    {
        public string ViewerUsername { get; set; }
        public string ViewedUsername { get; set; }
        public float Matched { get; set; } // Tahmin sırasında kullanılmaz, sadece modelin kontratı için var.
        public string ViewerPrefGender { get; set; }
        public string ViewerPrefSleepSchedule { get; set; }
        public bool ViewerPrefNoSmoker { get; set; }
        public bool ViewerPrefNoDrinker { get; set; }
        public float ViewerPrefHygiene { get; set; }
        public float ViewerPrefNoise { get; set; }
        public float ViewerPrefGuests { get; set; }
        public bool ViewerPrefStudent { get; set; }
        public string ViewedGender { get; set; }
        public string ViewedSleepSchedule { get; set; }
        public bool ViewedIsSmoker { get; set; }
        public bool ViewedIsDrinker { get; set; }
        public float ViewedHygiene { get; set; }
        public float ViewedNoisiness { get; set; }
        public float ViewedGuestFrequency { get; set; }
        public bool ViewedIsStudent { get; set; }
    }

    public class ModelOutput
    {
        public bool Prediction { get; set; } // Modelin tahmini (true/false)
        public float Probability { get; set; } // Pozitif sınıfa ait olma olasılığı (YENİ SKORUMUZ!)
        public float Score { get; set; }
    }
    #endregion

    #region Veritabanı ve Yardımcı Sınıflar
    // Bu sınıf, veritabanından veri çekmek için kullanılan eski yapıyı temsil ediyor.
    public class MatchCandidate
    {
        public string Username { get; set; }
        public string Gender { get; set; }
        public string SleepSchedule { get; set; }
        public bool IsSmoker { get; set; }
        public bool IsDrinker { get; set; }
        public int? Hygiene { get; set; }
        public int? Noisiness { get; set; }
        public int? GuestFrequency { get; set; }
        public bool IsStudent { get; set; }
        public string PrefGender { get; set; }
        public string PrefSleepSchedule { get; set; }
        public bool? NoSmokerPref { get; set; }
        public bool? NoDrinkerPref { get; set; }
        public int? SignificanceOfHygiene { get; set; }
        public int? NoiseSensitivity { get; set; }
        public int? GuestPreference { get; set; }
        public bool? StudentPref { get; set; }
    }

    // Tahmin sonuçlarını saklamak için küçük bir yardımcı sınıf.
    public class MatchPrediction
    {
        public string Username { get; set; }
        public float Probability { get; set; } // Modelin verdiği uyumluluk olasılığı
    }
    #endregion

    /// <summary>
    /// Eğitilmiş Makine Öğrenmesi modelini kullanarak kullanıcılar için en iyi eşleşmeleri bulur.
    /// </summary>
    public class MLMatchmakingService
    {
        private readonly string _connectionString;
        private readonly PredictionEngine<ModelInput, ModelOutput> _predictionEngine;

        public MLMatchmakingService()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["conStr"].ConnectionString;

            // --- ML MODELİNİ YÜKLEME ---
            var mlContext = new MLContext();
            string modelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HomemateMatchingModel.zip");

            if (!File.Exists(modelPath))
            {
                throw new FileNotFoundException("Eğitilmiş model dosyası bulunamadı!", modelPath);
            }

            ITransformer trainedModel = mlContext.Model.Load(modelPath, out var modelSchema);
            _predictionEngine = mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(trainedModel);

            Console.WriteLine("✅ ML Eşleştirme Modeli başarıyla yüklendi.");
        }

        /// <summary>
        /// Belirtilen kullanıcı için ML modelini kullanarak en uyumlu 10 kullanıcıyı bulur.
        /// </summary>
        /// <param name="currentUsername">Ana kullanıcının adı.</param>
        /// <returns>En yüksek uyumluluk olasılığına sahip 10 kullanıcının listesi.</returns>
        public List<string> FindTopMatches(string currentUsername)
        {
            // 1. Veritabanından mevcut kullanıcı ve potansiyel adayların verilerini çek.
            List<MatchCandidate> allUsers = GetAllUserData(currentUsername);
            MatchCandidate currentUser = allUsers.FirstOrDefault(u => u.Username.Equals(currentUsername, StringComparison.OrdinalIgnoreCase));
            List<MatchCandidate> otherUsers = allUsers.Where(u => !u.Username.Equals(currentUsername, StringComparison.OrdinalIgnoreCase)).ToList();

            if (currentUser == null) return new List<string>();

            Console.WriteLine($"\n🤖 [{currentUser.Username}] için ML tabanlı eşleştirme başlıyor...");
            var predictions = new List<MatchPrediction>();

            // 2. Her bir aday için modelden tahmin al.
            foreach (var candidate in otherUsers)
            {
                // Modelin istediği formata (ModelInput) verileri dönüştür.
                var modelInput = new ModelInput
                {
                    // Karar veren (Viewer) -> currentUser
                    ViewerPrefGender = currentUser.PrefGender,
                    ViewerPrefSleepSchedule = currentUser.PrefSleepSchedule,
                    ViewerPrefNoSmoker = currentUser.NoSmokerPref ?? false,
                    ViewerPrefNoDrinker = currentUser.NoDrinkerPref ?? false,
                    ViewerPrefHygiene = currentUser.SignificanceOfHygiene ?? 5, // Null ise ortalama bir değer ata
                    ViewerPrefNoise = currentUser.NoiseSensitivity ?? 5,
                    ViewerPrefGuests = currentUser.GuestPreference ?? 5,
                    ViewerPrefStudent = currentUser.StudentPref ?? false,

                    // İncelenen (Viewed) -> candidate
                    ViewedGender = candidate.Gender,
                    ViewedSleepSchedule = candidate.SleepSchedule,
                    ViewedIsSmoker = candidate.IsSmoker,
                    ViewedIsDrinker = candidate.IsDrinker,
                    ViewedHygiene = candidate.Hygiene ?? 5,
                    ViewedNoisiness = candidate.Noisiness ?? 5,
                    ViewedGuestFrequency = candidate.GuestFrequency ?? 5,
                    ViewedIsStudent = candidate.IsStudent
                };

                // Modelden tahmini iste!
                var prediction = _predictionEngine.Predict(modelInput);

                // Sonucu listeye ekle.
                predictions.Add(new MatchPrediction { Username = candidate.Username, Probability = prediction.Probability });

                System.Diagnostics.Debug.WriteLine($" -> Aday: {candidate.Username}, Uyum Olasılığı: {prediction.Probability:P2}");
            }

            // 3. Adayları uyumluluk olasılığına göre sırala ve en iyi 10'unu seç.
            return predictions
                .OrderByDescending(p => p.Probability)
                .Take(10)
                .Select(p => p.Username)
                .ToList();
        }

        /// <summary>
        /// Veritabanından tüm kullanıcıların nitelik ve tercih verilerini çeker.
        /// (Bu metod, veri yapısı aynı kaldığı için eski servisten değiştirilmeden alınmıştır.)
        /// </summary>
        private List<MatchCandidate> GetAllUserData(string username)
        {
            var users = new List<MatchCandidate>();
            string query = @"
                SELECT 
                    ua.username, ua.gender, ua.sleepSchedule, ua.isSmoker, ua.isDrinker, ua.Hygiene, ua.noisiness, ua.GuestFrequency, ua.IsStudent,
                    up.gender AS PrefGender, up.sleepSchedule AS PrefSleepSchedule, up.NoSmoker, up.NoDrinker, up.SignificanceOfHygiene, up.NoiseSensitivity, up.GuestPreference, up.StudentPref
                FROM UserAttribute ua
                INNER JOIN UserPreference up ON ua.username = up.username
                WHERE ua.username NOT IN (
                    SELECT viewed FROM Accept WHERE viewer = @username
                    UNION SELECT viewed FROM Reject WHERE viewer = @username
                    UNION SELECT viewer FROM Reject WHERE viewed = @username
                    UNION SELECT username2 FROM Match WHERE username1 = @username
                    UNION SELECT username1 FROM Match WHERE username2 = @username
                ) AND ua.username != @username"; // Kendisini de hariç tut

            // Mevcut kullanıcıyı da listeye eklemek için ayrı bir sorgu
            string currentUserQuery = "SELECT ua.username, ua.gender, ua.sleepSchedule, ua.isSmoker, ua.isDrinker, ua.Hygiene, ua.noisiness, ua.GuestFrequency, ua.IsStudent, up.gender AS PrefGender, up.sleepSchedule AS PrefSleepSchedule, up.NoSmoker, up.NoDrinker, up.SignificanceOfHygiene, up.NoiseSensitivity, up.GuestPreference, up.StudentPref FROM UserAttribute ua INNER JOIN UserPreference up ON ua.username = up.username WHERE ua.username = @username";

            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();
                // Önce diğer kullanıcıları çek
                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    using (var reader = cmd.ExecuteReader()) { while (reader.Read()) users.Add(MapReaderToCandidate(reader)); }
                }
                // Sonra mevcut kullanıcıyı çek
                using (var cmd = new SqlCommand(currentUserQuery, con))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    using (var reader = cmd.ExecuteReader()) { while (reader.Read()) users.Add(MapReaderToCandidate(reader)); }
                }
            }
            return users;
        }

        private MatchCandidate MapReaderToCandidate(SqlDataReader reader)
        {
            return new MatchCandidate
            {
                Username = reader["username"].ToString(),
                Gender = reader["gender"] != DBNull.Value ? reader["gender"].ToString() : null,
                SleepSchedule = reader["sleepSchedule"] != DBNull.Value ? reader["sleepSchedule"].ToString() : null,
                IsSmoker = reader["isSmoker"] != DBNull.Value && (bool)reader["isSmoker"],
                IsDrinker = reader["isDrinker"] != DBNull.Value && (bool)reader["isDrinker"],
                Hygiene = reader["Hygiene"] as int?,
                Noisiness = reader["noisiness"] as int?,
                GuestFrequency = reader["GuestFrequency"] as int?,
                IsStudent = reader["IsStudent"] != DBNull.Value && (bool)reader["IsStudent"],
                PrefGender = reader["PrefGender"] != DBNull.Value ? reader["PrefGender"].ToString() : null,
                PrefSleepSchedule = reader["PrefSleepSchedule"] != DBNull.Value ? reader["PrefSleepSchedule"].ToString() : null,
                NoSmokerPref = reader["NoSmoker"] as bool?,
                NoDrinkerPref = reader["NoDrinker"] as bool?,
                SignificanceOfHygiene = reader["SignificanceOfHygiene"] as int?,
                NoiseSensitivity = reader["NoiseSensitivity"] as int?,
                GuestPreference = reader["GuestPreference"] as int?,
                StudentPref = reader["StudentPref"] as bool?
            };
        }
    }
}