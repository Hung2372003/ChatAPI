using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

namespace FakeFacebook.Service
{
    public static class FirebaseServiceCollectionExtensions
    {
        public static IServiceCollection AddFirebase(this IServiceCollection services)
        {
            // Nếu đã init FirebaseApp rồi thì bỏ qua
            if (FirebaseApp.DefaultInstance != null)
            {
                Console.WriteLine("ℹ️ FirebaseApp already initialized. Skipping...");
                return services;
            }

            try
            {
                // Ưu tiên ENV (Production)
                var firebaseJson = Environment.GetEnvironmentVariable("FIREBASE_KEY_JSON");
                if (!string.IsNullOrEmpty(firebaseJson))
                {
                    FirebaseApp.Create(new AppOptions
                    {
                        Credential = GoogleCredential.FromJson(firebaseJson)
                    });
                    Console.WriteLine("✅ Firebase initialized from ENV variable.");
                }
                else
                {
                    // Fallback sang file local
                    var path = Path.Combine(AppContext.BaseDirectory, "serviceAccountKey.json");
                    if (File.Exists(path))
                    {
                        FirebaseApp.Create(new AppOptions
                        {
                            Credential = GoogleCredential.FromFile(path)
                        });
                        Console.WriteLine("✅ Firebase initialized from serviceAccountKey.json.");
                    }
                    else
                    {
                        Console.WriteLine("⚠️ Firebase not initialized. No ENV or file found.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Firebase initialization failed: " + ex.Message);
            }

            return services;
        }
    }
}
