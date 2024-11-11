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

        public AuthController(ILogger<AuthController> logger, FirebaseAuthProvider firebaseauth)
        {
            _logger = logger;
            _firebaseauth = firebaseauth;
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
                // Attempt to sign in with email and password
                var authResult = await _firebaseauth.SignInWithEmailAndPasswordAsync(model.EmailAdd, model.Password);

                // Retrieve user's info to check if email is verified
                var user = await _firebaseauth.GetUserAsync(authResult.FirebaseToken);

                if (!user.IsEmailVerified)
                {
                    TempData["ErrorMsg"] = "Please verify your email before logging in.";
                    return View(model); // Prevent login if email is not verified
                }

                // Email is verified, proceed with login
                // Save user details in session or proceed as per your application needs
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


        public IActionResult ProfileSetup() 
        {
            return View();
        }
        

    }
}