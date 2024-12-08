using Microsoft.AspNetCore.Mvc;
using Firebase.Auth;
using Google.Cloud.Firestore;
using TMS.Models;
using TMS.ViewModels;

namespace TMS.Controllers.Accounts
{
    public class AuthController : Controller
    {
        private readonly ILogger<AuthController> _logger;
        private readonly FirebaseAuthProvider _firebaseauth;
        private readonly FirestoreDb _firestoreDb;

        public AuthController(ILogger<AuthController> logger, FirebaseAuthProvider firebaseauth, FirestoreDb firestoreDb)
        {
            _logger = logger;
            _firebaseauth = firebaseauth;
            _firestoreDb = firestoreDb;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                Query usernameQuery = _firestoreDb.Collection("Users").WhereEqualTo("Username", model.Username);
                QuerySnapshot usernameQuerySnapshot = await usernameQuery.GetSnapshotAsync();

                if (usernameQuerySnapshot.Documents.Count > 0)
                {
                    TempData["ErrorMsg"] = "This username is already taken. Please choose a different username.";
                    return View(model);
                }

                // Create the user with email and password
                var authResult = await _firebaseauth.CreateUserWithEmailAndPasswordAsync(model.Email, model.Password);
                var uid = authResult.User.LocalId;

                var user = new mAuth
                {
                    UID = uid,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Username = model.Username,
                    Email = model.Email,
                    UserRole = model.Role,
                    UserImg = "https://i.pinimg.com/474x/65/25/a0/6525a08f1df98a2e3a545fe2ace4be47.jpg"
                };

                DocumentReference docRef = _firestoreDb.Collection("Users").Document(uid);
                await docRef.SetAsync(user);

                try
                {
                    await _firebaseauth.SendEmailVerificationAsync(authResult.FirebaseToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error sending email verification: {ex.Message}");
                    TempData["ErrorMsg"] = "An error occurred while sending the verification email. Please try again.";
                    return View(model);
                }

                TempData["SuccessMsg"] = "Registration successful! Please check your email to verify your account before logging in.";
                return RedirectToAction("Login");
            }
            catch (Firebase.Auth.FirebaseAuthException ex)
            {
                _logger.LogError($"Firebase Exception: {ex.Message}");

                if (ex.Message.Contains("EMAIL_EXISTS"))
                {
                    TempData["ErrorMsg"] = "This email is already registered. Please use a different email.";
                }
                else
                {

                    TempData["ErrorMsg"] = "An error occurred: " + ex.Message;
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"General Exception: {ex.Message}");
                TempData["ErrorMsg"] = "An unexpected error occurred. Please try again.";
                return View(model);
            }
        }

        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Attempt to sign in with Firebase Authentication (Client SDK)
                var authResult = await _firebaseauth.SignInWithEmailAndPasswordAsync(model.Email, model.Password);

                // Retrieve user record from Firebase Admin SDK (Server SDK)
                var userRecord = await FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance.GetUserAsync(authResult.User.LocalId);

                // Check if the email is verified
                if (!userRecord.EmailVerified)
                {
                    TempData["ErrorMsg"] = "Please verify your email before logging in.";
                    return View(model);
                }

                // Save session data for the authenticated user
                HttpContext.Session.SetString("FirebaseUserId", authResult.User.LocalId);
                TempData["SuccessMsg"] = "Login successful!";
                return RedirectToAction("Dashboard", "Home");
            }
            catch (Firebase.Auth.FirebaseAuthException ex)
            {
                // Handle Firebase client authentication errors
                _logger.LogError($"Firebase Client Exception: {ex.Message}");

                if (ex.Message.Contains("INVALID_EMAIL") || ex.Message.Contains("EMAIL_NOT_FOUND"))
                {
                    TempData["ErrorMsg"] = "Email not found. Please check your email address.";
                }
                else if (ex.Message.Contains("INVALID_PASSWORD"))
                {
                    TempData["ErrorMsg"] = "Incorrect password. Please try again.";
                }
                else
                {
                    TempData["ErrorMsg"] = "An error occurred: " + ex.Message;
                }

                return View(model);
            }
            catch (FirebaseAdmin.Auth.FirebaseAuthException ex)
            {
                // Handle Firebase Admin SDK errors
                _logger.LogError($"Firebase Admin Exception: {ex.Message}");
                TempData["ErrorMsg"] = "There was an issue with verifying your account. Please try again.";
                return View(model);
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                _logger.LogError($"General Exception: {ex.Message}");
                TempData["ErrorMsg"] = "An unexpected error occurred. Please try again.";
                return View(model);
            }
        }
    }
}
