using Microsoft.AspNetCore.Mvc;
using Firebase.Auth;
using Newtonsoft.Json;
using System.Text;
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
                var authResult = await _firebaseauth.CreateUserWithEmailAndPasswordAsync(model.EmailAdd, model.Password);

                await _firebaseauth.SendEmailVerificationAsync(authResult.FirebaseToken);

                TempData["SuccessMsg"] = "Registration successful! Please check your email to verify your account before logging in.";
                return RedirectToAction("Login");
            }
            catch (FirebaseAuthException ex)
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

        }

        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var authResult = await _firebaseauth.SignInWithEmailAndPasswordAsync(model.EmailAdd, model.Password);

                var user = await _firebaseauth.GetUserAsync(authResult.FirebaseToken);

                if (!user.IsEmailVerified)
                {
                    TempData["ErrorMsg"] = "Please verify your email before logging in.";
                    return View(model);
                }

                TempData["SuccessMsg"] = "Login successful!";
                return RedirectToAction("ProfileSetup");
            }
            catch (FirebaseAuthException ex)
            {
                _logger.LogError($"Firebase Exception: {ex.Message}");

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
        }

        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await _firebaseauth.SendPasswordResetEmailAsync(model.EmailAdd);

            TempData["SuccessMsg"] = "A password reset link has been sent to your email.";
            return RedirectToAction("Login");
        }

        public IActionResult ProfileSetup()
        {
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            HttpContext.Response.Cookies.Delete(".AspNetCore.Identity.Application");

            TempData["SuccessMsg"] = "You have successfully logged out.";
            return RedirectToAction("Login", "Credentials");
        }


    }
}