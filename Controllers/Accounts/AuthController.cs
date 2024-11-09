using Microsoft.AspNetCore.Mvc;
using Firebase.Auth;
using Newtonsoft.Json;
using System.Text;
using Google.Cloud.Firestore;
using TMS.Models;

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

        public async Task<IActionResult> Register(Auth auth)
        {
            if (!ModelState.IsValid)
            {
                return View(auth);
            }

            try
            {
                // Create the user with email and password
                var authResult = await _firebaseauth.CreateUserWithEmailAndPasswordAsync(auth.EmailAdd, auth.Password);

                // Send email verification
                await _firebaseauth.SendEmailVerificationAsync(authResult.FirebaseToken);

                // Inform the user to check their email for verification
                TempData["RegistrationMsg"] = "Registration successful! Please check your email to verify your account before logging in.";

                return RedirectToAction("Login"); // Redirect to the login page
            }
            catch (FirebaseAuthException ex)
            {
                Console.WriteLine($"Firebase Exception: {ex.Message}");

                // Handle specific Firebase exceptions, such as invalid password
                if (ex.Message.Contains("INVALID_PASSWORD") || ex.Message.Contains("wrong-password"))
                {
                    TempData["IncorrectPassword"] = "Incorrect password. Please try again!";
                    return View(auth);
                }
                else
                {
                    TempData["ErrorMessage"] = "An error occurred: " + ex.Message;
                }

                return View(auth);
            }
        }

        public IActionResult Login()
        {
            return View();
        }

    }
}