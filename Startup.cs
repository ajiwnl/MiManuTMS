using Firebase.Auth;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

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
            string path = Path.Combine(Directory.GetCurrentDirectory(), "json", "mimanutms-d516c-firebase-adminsdk-bwby4-88fc704792.json");
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);

            // Initialize FirebaseAdmin SDK for Admin use
            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromFile(path)

            });

            // Add FirebaseAuthProvider and FirestoreDb as services
            services.AddSingleton(new FirebaseAuthProvider(new FirebaseConfig("AIzaSyD5vELQoepsUV_K6740n1DGuJk1NOMKXrk")));
            services.AddSingleton(FirestoreDb.Create("mimanutms-d516c"));
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

public class CloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService()
    {
        var account = new Account(
            "dip5gm9sj",
            "942788152523517",
            "OFNSf-cxYC3zwb-uAozj7XwxXHU"
        );
        _cloudinary = new Cloudinary(account);
    }

    public async Task<string> UploadImageAsync(IFormFile file)
    {
        var transformation = new Transformation()
            .Width(500)    // Set the width to 500px
            .Height(500)   // Set the height to 500px
            .Crop("fill"); 
        // Prepare the upload parameters with transformation
        var uploadParams = new ImageUploadParams()
        {
            File = new FileDescription(file.FileName, file.OpenReadStream()),
            Folder = "profile_images",
            Transformation = transformation // Apply transformation
        };

        // Upload the image and get the result
        var uploadResult = await _cloudinary.UploadAsync(uploadParams);

        // Return the secure URL of the uploaded image
        return uploadResult?.SecureUrl.ToString();
    }
}
