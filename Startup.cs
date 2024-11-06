using Firebase.Auth;
using Google.Cloud.Firestore;

namespace TMS
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {

            // Set the environment variable for Firestore credentials
            string path = Path.Combine(Directory.GetCurrentDirectory(), "json", "tmsproject-e504e-firebase-adminsdk-b90fh-4c2403d3a2.json");
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);

            // Add FirebaseAuthProvider and FirestoreDb as services
            services.AddSingleton(new FirebaseAuthProvider(new FirebaseConfig("AIzaSyD56boXG1u4axBuEom02AEoxjLcu_QpmL4")));
            services.AddSingleton(FirestoreDb.Create("tms-project"));
            services.AddSession();

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30); // Set timeout
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
            ;
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSession();
        }
    }
}
