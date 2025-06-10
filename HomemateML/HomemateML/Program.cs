// Gerekli using bildirimleri
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ML;
using Microsoft.ML.Data;

// Ana namespace'imiz
namespace HomemateMatcher.ML
{
    // -----------------------------------------------------------------
    // VERİ YAPISI SINIFLARI
    // -----------------------------------------------------------------

    /// <summary>
    /// SQL sorgusundan veya CSV dosyasından okunacak verinin C# karşılığı.
    /// Her bir [LoadColumn] niteliği, CSV dosyasındaki bir sütuna karşılık gelir.
    /// </summary>
    public class ModelInput
    {
        // Modelin eğitilmesi için kullanılmayacak, sadece tanımlayıcı sütunlar
        [LoadColumn(0)] public string ViewerUsername { get; set; }
        [LoadColumn(1)] public string ViewedUsername { get; set; }

        // ETİKET (LABEL): Modelin tahmin etmeyi öğreneceği hedef değer.
        // SQL'de 1 (Accept) veya 0 (Reject) olarak oluşturuluyor.
        [LoadColumn(2)] public float Matched { get; set; }

        // --- ÖZELLİKLER (FEATURES) ---
        // Modelin tahminde bulunurken kullanacağı tüm girdiler.

        // Karar Veren Kullanıcının (Viewer) Tercihleri
        [LoadColumn(3)] public string ViewerPrefGender { get; set; }
        [LoadColumn(4)] public string ViewerPrefSleepSchedule { get; set; }
        [LoadColumn(5)] public bool ViewerPrefNoSmoker { get; set; }
        [LoadColumn(6)] public bool ViewerPrefNoDrinker { get; set; }
        [LoadColumn(7)] public float ViewerPrefHygiene { get; set; }
        [LoadColumn(8)] public float ViewerPrefNoise { get; set; }
        [LoadColumn(9)] public float ViewerPrefGuests { get; set; }
        [LoadColumn(10)] public bool ViewerPrefStudent { get; set; }

        // İncelenen Kullanıcının (Viewed) Nitelikleri
        [LoadColumn(11)] public string ViewedGender { get; set; }
        [LoadColumn(12)] public string ViewedSleepSchedule { get; set; }
        [LoadColumn(13)] public bool ViewedIsSmoker { get; set; }
        [LoadColumn(14)] public bool ViewedIsDrinker { get; set; }
        [LoadColumn(15)] public float ViewedHygiene { get; set; }
        [LoadColumn(16)] public float ViewedNoisiness { get; set; }
        [LoadColumn(17)] public float ViewedGuestFrequency { get; set; }
        [LoadColumn(18)] public bool ViewedIsStudent { get; set; }
    }

    /// <summary>
    /// Modelin bir tahmin yaptıktan sonra üreteceği çıktının yapısı.
    /// </summary>
    public class ModelOutput
    {
        // ML.NET'in tahmin sonucunu (true/false) atadığı varsayılan sütun.
        [ColumnName("PredictedLabel")]
        public bool Prediction { get; set; }

        // Modelin pozitif sınıfa ('Kabul') olan güvenini 0-1 arasında bir olasılık olarak verir.
        public float Probability { get; set; }

        // Modelin tahmin için kullandığı ham (kalibre edilmemiş) sayısal değer.
        public float Score { get; set; }
    }


    // -----------------------------------------------------------------
    // ANA PROGRAM
    // -----------------------------------------------------------------
    public static class Program
    {
        // Veri dosyasının yolu. SQL sorgunuzun çıktısını bu isimle kaydettiğinizi varsayalım.
        private static readonly string DataPath = "training-data.csv";
        private static readonly string ModelPath = "HomemateMatchingModel.zip";

        public static void Main(string[] args)
        {
            Console.WriteLine("✨ Homemate Eşleştirme Modeli Eğitim Programı Başlatılıyor... ✨");

            // 1. ML.NET Ortamını Oluşturma
            // 'seed: 0' parametresi, işlemlerin her seferinde aynı rastgele sonuçları üretmesini sağlar,
            // bu da test ve geliştirmeyi kolaylaştırır.
            var mlContext = new MLContext(seed: 0);

            // 2. Veriyi Dosyadan Yükleme
            Console.WriteLine($"\nVeri seti yükleniyor: {DataPath}");
            IDataView fullData;
            try
            {
                // SQL sorgunuzun sonucunu ; ile ayrılmış bir CSV olarak kaydedin.
                fullData = mlContext.Data.LoadFromTextFile<ModelInput>(DataPath, separatorChar: ';', hasHeader: true);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nHATA: Veri dosyası okunamadı. '{DataPath}' dosyasının doğru yerde ve formatta olduğundan emin olun.");
                Console.WriteLine($"Detay: {ex.Message}");
                Console.ResetColor();
                return;
            }

            // 3. Veri İşleme ve Eğitim Pipeline'ını Tanımlama
            // Bu pipeline, ham veriyi modelin anlayacağı bir formata dönüştüren ve
            // ardından modeli eğiten adımların bir bütünüdür.
            var pipeline = DefinePipeline(mlContext);

            // 4. Modelin Performansını Çapraz Doğrulama ile Değerlendirme
            // Küçük veri setleri için en güvenilir yöntemdir. Veriyi 5 parçaya böler,
            // 4'üyle eğitip 1'iyle test eder ve bunu 5 kez tekrarlayarak ortalama bir sonuç çıkarır.
            // Bu, önceki kodda yaşadığınız "test setinde pozitif örnek olmaması" sorununu tamamen çözer.
            EvaluateModelWithCrossValidation(mlContext, fullData, pipeline);

            // 5. Nihai Modeli Tüm Veri Setiyle Eğitme ve Kaydetme
            // Modelin performansından memnun olduğumuzda, onu deploy etmeden önce
            // eldeki tüm veriden öğrenmesini sağlamak en iyisidir.
            TrainAndSaveFinalModel(mlContext, fullData, pipeline);

            Console.WriteLine("\n🎉 Tüm işlemler başarıyla tamamlandı. Çıkmak için bir tuşa basın.");
            Console.ReadKey();
        }

        /// <summary>
        /// ML.NET veri işleme ve eğitim pipeline'ını oluşturur.
        /// </summary>
        private static IEstimator<ITransformer> DefinePipeline(MLContext mlContext)
        {
            // Adım A: Kategorik (metin) verileri 'One-Hot Encoding' ile sayısala çevir.
            // Model 'Erkek', 'Gece Kuşu' gibi metinleri anlayamaz. Bu adım onları sayısal vektörlere dönüştürür.
            var featureProcessingPipeline = mlContext.Transforms.Categorical.OneHotEncoding(
                    outputColumnName: "ViewerPrefGenderEncoded", inputColumnName: nameof(ModelInput.ViewerPrefGender))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(
                    outputColumnName: "ViewerPrefSleepScheduleEncoded", inputColumnName: nameof(ModelInput.ViewerPrefSleepSchedule)))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(
                    outputColumnName: "ViewedGenderEncoded", inputColumnName: nameof(ModelInput.ViewedGender)))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(
                    outputColumnName: "ViewedSleepScheduleEncoded", inputColumnName: nameof(ModelInput.ViewedSleepSchedule)))

                // Adım B: Boolean (true/false) verileri float (1.0/0.0) formatına çevir.
                // 'Concatenate' adımı tüm girdilerin sayısal olmasını bekler.
                .Append(mlContext.Transforms.Conversion.ConvertType(
                    new[] {
                        new InputOutputColumnPair("ViewerPrefNoSmokerNum", nameof(ModelInput.ViewerPrefNoSmoker)),
                        new InputOutputColumnPair("ViewerPrefNoDrinkerNum", nameof(ModelInput.ViewerPrefNoDrinker)),
                        new InputOutputColumnPair("ViewerPrefStudentNum", nameof(ModelInput.ViewerPrefStudent)),
                        new InputOutputColumnPair("ViewedIsSmokerNum", nameof(ModelInput.ViewedIsSmoker)),
                        new InputOutputColumnPair("ViewedIsDrinkerNum", nameof(ModelInput.ViewedIsDrinker)),
                        new InputOutputColumnPair("ViewedIsStudentNum", nameof(ModelInput.ViewedIsStudent))
                    }, outputKind: DataKind.Single))

                // Adım C: Tüm özellikleri 'Features' adında tek bir vektörde birleştir.
                // Eğitim algoritmaları tüm girdileri tek bir sütunda bekler.
                .Append(mlContext.Transforms.Concatenate("Features",
                    // One-Hot Encoding ile oluşturulanlar
                    "ViewerPrefGenderEncoded", "ViewerPrefSleepScheduleEncoded", "ViewedGenderEncoded", "ViewedSleepScheduleEncoded",
                    // Bool'dan dönüştürülenler
                    "ViewerPrefNoSmokerNum", "ViewerPrefNoDrinkerNum", "ViewerPrefStudentNum",
                    "ViewedIsSmokerNum", "ViewedIsDrinkerNum", "ViewedIsStudentNum",
                    // Orijinalde zaten sayısal olanlar
                    nameof(ModelInput.ViewerPrefHygiene), nameof(ModelInput.ViewerPrefNoise), nameof(ModelInput.ViewerPrefGuests),
                    nameof(ModelInput.ViewedHygiene), nameof(ModelInput.ViewedNoisiness), nameof(ModelInput.ViewedGuestFrequency)
                ))

                // Adım D: Etiket (Label) sütununu hazırla.
                // Tahmin edilecek 'Matched' sütununu, algoritmaların beklediği 'Label' adına ve boolean tipine dönüştür.
                .Append(mlContext.Transforms.Conversion.ConvertType("Label", nameof(ModelInput.Matched), outputKind: DataKind.Boolean));

            // Adım E: Öğrenme algoritmasını (Trainer) pipeline'a ekle.
            // LightGBM, bu tür tablo verilerinde genellikle çok iyi performans gösteren güçlü bir algoritmadır.
            var trainer = mlContext.BinaryClassification.Trainers.LightGbm(labelColumnName: "Label", featureColumnName: "Features");
            var fullPipeline = featureProcessingPipeline.Append(trainer);

            return fullPipeline;
        }

        /// <summary>
        /// Modeli Çapraz Doğrulama ile test eder ve metrikleri ekrana yazdırır.
        /// </summary>
        private static void EvaluateModelWithCrossValidation(MLContext mlContext, IDataView data, IEstimator<ITransformer> pipeline)
        {
            Console.WriteLine("\n-------------------------------------------------");
            Console.WriteLine("Model Performansı Çapraz Doğrulama ile Değerlendiriliyor...");

            var crossValidationResults = mlContext.BinaryClassification.CrossValidate(data, pipeline, numberOfFolds: 5, labelColumnName: "Label");

            // Sonuçların ortalamasını alarak daha güvenilir metrikler elde et
            var metrics = crossValidationResults.Select(r => r.Metrics).ToList();
            var avgAccuracy = metrics.Average(m => m.Accuracy);
            var avgAuc = metrics.Average(m => m.AreaUnderRocCurve);
            var avgF1Score = metrics.Average(m => m.F1Score);
            var avgPrecision = metrics.Average(m => m.PositivePrecision);
            var avgRecall = metrics.Average(m => m.PositiveRecall);

            Console.WriteLine("\nOrtalama Performans Metrikleri:");
            Console.WriteLine($"  - Accuracy (Doğruluk):         {avgAccuracy:P2}");
            Console.WriteLine($"  - AUC (Genel Kalite):          {avgAuc:P2}");
            Console.WriteLine($"  - F1-Score (Denge):            {avgF1Score:P2}");
            Console.WriteLine($"  - Precision (Kesinlik):        {avgPrecision:P2}");
            Console.WriteLine($"  - Recall (Duyarlılık):         {avgRecall:P2}");
            Console.WriteLine("-------------------------------------------------");
        }

        /// <summary>
        /// Modeli tüm veriyle eğitir ve diske kaydeder.
        /// </summary>
        private static void TrainAndSaveFinalModel(MLContext mlContext, IDataView data, IEstimator<ITransformer> pipeline)
        {
            Console.WriteLine("\nNihai model tüm veri seti kullanılarak eğitiliyor...");
            ITransformer finalModel = pipeline.Fit(data);

            Console.WriteLine($"Model diske kaydediliyor: {ModelPath}");
            mlContext.Model.Save(finalModel, data.Schema, ModelPath);
        }
    }
}