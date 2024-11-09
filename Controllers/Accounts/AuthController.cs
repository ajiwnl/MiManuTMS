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
                // Create the user with email and password
                var authResult = await _firebaseauth.CreateUserWithEmailAndPasswordAsync(model.EmailAdd, model.Password);

                // Send email verification
                await _firebaseauth.SendEmailVerificationAsync(authResult.FirebaseToken);

                TempData["RegistrationMsg"] = "Registration successful! Please check your email to verify your account before logging in.";
                return RedirectToAction("Login");
            }
            catch (FirebaseAuthException ex)
            {
                _logger.LogError($"Firebase Exception: {ex.Message}");

                // Handle specific Firebase registration exceptions
                if (ex.Message.Contains("EMAIL_EXISTS"))
                {
                    TempData["ErrorMessage"] = "This email is already registered. Please use a different email.";
                }
                else if (ex.Message.Contains("WEAK_PASSWORD"))
                {
                    TempData["ErrorMessage"] = "Your password is too weak. Please choose a stronger password.";
                }
                else if (ex.Message.Contains("INVALID_EMAIL"))
                {
                    TempData["ErrorMessage"] = "The email address you entered is invalid. Please check and try again.";
                }
                else
                {
                    TempData["ErrorMessage"] = "An error occurred: " + ex.Message;
                }

                return View(model);
            }
 
        }


        public IActionResult Login()
        {
            return View();
        }

    }
}