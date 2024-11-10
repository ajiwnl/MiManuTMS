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
                // Step 1: Sign in with email and password
                var authResult = await _firebaseauth.SignInWithEmailAndPasswordAsync(model.EmailAdd, model.Password);
                var user = authResult.User;

                // Step 2: Sign out to refresh user session
                        //_firebaseauth.SignOut();  // Sign out to clear cache

                // Step 3: Re-authenticate the user to get the updated email verification status
                var reAuthResult = await _firebaseauth.SignInWithEmailAndPasswordAsync(model.EmailAdd, model.Password);
                user = reAuthResult.User;

                // Step 4: Check if the email is verified
                if (!user.IsEmailVerified)
                {
                    TempData["ErrorMsg"] = "Please verify your email before logging in.";
                    return View(model);
                }

                // Step 5: Store the user ID in session after successful login
                HttpContext.Session.SetString("FirebaseUserId", user.LocalId);
                TempData["SuccessMsg"] = "Login successful!";
                return RedirectToAction("ProfileSetup");  // Redirect to a profile setup page or dashboard
            }
            catch (FirebaseAuthException ex)
            {
                _logger.LogError($"Firebase Exception: {ex.Message}");

                if (ex.Message.Contains("INVALID_EMAIL"))
                {
                    TempData["ErrorMsg"] = "Invalid email address. Please check the email entered.";
                }
                else if (ex.Message.Contains("WRONG_PASSWORD"))
                {
                    TempData["ErrorMsg"] = "Incorrect password. Please try again.";
                }
                else
                {
                    TempData["ErrorMsg"] = "An error occurred: " + ex.Message;
                }

                return View(model);  // Return to the login page with error message
            }
        }

        public IActionResult ProfileSetup() 
        {
            return View();
        }
        

    }
}