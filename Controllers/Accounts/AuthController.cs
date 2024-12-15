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
                ViewData["ActivePage"] = "Register";
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
                var uid = authResult.User.LocalId;
                // Retrieve user record from Firebase Admin SDK (Server SDK)
                var userRecord = await FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance.GetUserAsync(authResult.User.LocalId);

                // Check if the email is verified
                if (!userRecord.EmailVerified)
                {
                    TempData["ErrorMsg"] = "Please verify your email before logging in.";
                    return View(model);
                }

                DocumentReference docRef = _firestoreDb.Collection("Users").Document(uid);
                DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

                if (!snapshot.Exists)
                {
                    TempData["ErrorMsg"] = "User not found.";
                    return View(model);
                }

                var user = snapshot.ConvertTo<mAuth>();

                // Store user details in the session
                HttpContext.Session.SetString("UID", user.UID);
                HttpContext.Session.SetString("FirstName", user.FirstName);
                HttpContext.Session.SetString("LastName", user.LastName);
                HttpContext.Session.SetString("UserImg", user.UserImg);
                HttpContext.Session.SetString("UserRole", user.UserRole);
                TempData["SuccessMsg"] = "Login successful!";
                return RedirectToAction("Dashboard", "Home");
            }
            catch (Firebase.Auth.FirebaseAuthException ex)
            {
                // Handle Firebase client authentication errors
                _logger.LogError($"Firebase Client Exception: {ex.Message}");

           
                if (ex.Message.Contains("INVALID_LOGIN_CREDENTIALS"))
                {
                    TempData["ErrorMsg"] = "Incorrect email or password. Please try again.";
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

        public async Task<IActionResult> Profile()
        {
            try
            {
                // Get the logged-in user's UID from the session
                var uid = HttpContext.Session.GetString("UID");
                if (string.IsNullOrEmpty(uid))
                {
                    TempData["ErrorMsg"] = "You need to log in to view your profile.";
                    return RedirectToAction("Login");
                }

                // Retrieve the user data from Firestore
                DocumentReference docRef = _firestoreDb.Collection("Users").Document(uid);
                DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

                if (!snapshot.Exists)
                {
                    TempData["ErrorMsg"] = "User profile not found.";
                    return RedirectToAction("Login");
                }

                // Map Firestore data to a ViewModel
                var user = snapshot.ConvertTo<mAuth>();
                var profileViewModel = new ProfileViewModel
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Username = user.Username,
                    Email = user.Email,
                    UserRole = user.UserRole,
                    UserImg = user.UserImg
                };

                return View(profileViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching user profile: {ex.Message}");
                TempData["ErrorMsg"] = "An unexpected error occurred. Please try again.";
                return RedirectToAction("Login");
            }
        }

        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMsg"] = "Please ensure all fields are correctly filled.";
                return RedirectToAction("Profile");
            }

            try
            {
                var uid = HttpContext.Session.GetString("UID");
                if (string.IsNullOrEmpty(uid))
                {
                    TempData["ErrorMsg"] = "You need to log in to edit your profile.";
                    return RedirectToAction("Login");
                }

                var userDocRef = _firestoreDb.Collection("Users").Document(uid);
                var userSnapshot = await userDocRef.GetSnapshotAsync();

                if (!userSnapshot.Exists)
                {
                    TempData["ErrorMsg"] = "User profile not found.";
                    return RedirectToAction("Login");
                }

                var updates = new Dictionary<string, object>();

                // Update email
                if (!string.IsNullOrEmpty(model.Email))
                {
                    var authResult = await _firebaseauth.SignInWithEmailAndPasswordAsync(model.CurrentEmail, model.CurrentPassword);
                    var token = authResult.FirebaseToken;

                    // Use Firebase Admin SDK to update email
                    var userRecordArgs = new FirebaseAdmin.Auth.UserRecordArgs
                    {
                        Uid = authResult.User.LocalId,
                        Email = model.Email
                    };

                    await FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance.UpdateUserAsync(userRecordArgs);

                    // Optionally send email verification
                    await _firebaseauth.SendEmailVerificationAsync(token);

                    updates["Email"] = model.Email;
                    TempData["SuccessMsg"] = "Email updated successfully. Please verify your new email address.";
                    HttpContext.Session.Clear();
                    return RedirectToAction("Login");
                }


                // Update password
                if (!string.IsNullOrEmpty(model.OldPassword) &&
                    !string.IsNullOrEmpty(model.NewPassword) &&
                    !string.IsNullOrEmpty(model.ConfirmPassword))
                {
                    if (model.NewPassword != model.ConfirmPassword)
                    {
                        TempData["ErrorMsg"] = "New password and confirmation password do not match.";
                        return RedirectToAction("Profile");
                    }

                    var authResult = await _firebaseauth.SignInWithEmailAndPasswordAsync(model.CurrentEmail, model.OldPassword);

                    // Use Firebase Admin SDK to update password
                    var userRecordArgs = new FirebaseAdmin.Auth.UserRecordArgs
                    {
                        Uid = authResult.User.LocalId,
                        Password = model.NewPassword
                    };

                    await FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance.UpdateUserAsync(userRecordArgs);

                    TempData["SuccessMsg"] = "Password updated successfully.";
                }


                // Update profile image
                if (model.UserImgFile != null)
                {
                    if (model.UserImgFile.ContentType.StartsWith("image/"))
                    {
                        // Save the image to a file hosting service or storage (not shown here)
                        // Example: var imageUrl = await UploadImageToStorageAsync(model.UserImgFile);
                        var imageUrl = "https://example.com/your-uploaded-image.jpg"; // Placeholder URL
                        updates["UserImg"] = imageUrl;
                        TempData["SuccessMsg"] = "Profile image updated successfully.";
                    }
                    else
                    {
                        TempData["ErrorMsg"] = "Please upload a valid image file.";
                        return RedirectToAction("Profile");
                    }
                }

                // Apply updates in batch
                if (updates.Count > 0)
                {
                    await userDocRef.UpdateAsync(updates);
                }

                TempData["SuccessMsg"] = "Profile updated successfully.";
                return RedirectToAction("Profile");
            }
            catch (FirebaseAuthException ex)
            {
                _logger.LogError($"Firebase Exception: {ex.Message}");
                TempData["ErrorMsg"] = "An error occurred: " + ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError($"General Exception: {ex.Message}");
                TempData["ErrorMsg"] = "An unexpected error occurred. Please try again.";
            }

            return RedirectToAction("Profile");
        }


        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Try to send the password reset email
                await _firebaseauth.SendPasswordResetEmailAsync(model.Email);

                // If no error occurs, inform the user that the reset link is sent
                TempData["SuccessMsg"] = "A password reset link has been sent to your email.";
            }
            catch (FirebaseAuthException ex)
            {
                _logger.LogError($"Firebase Exception: {ex.Message}");

                // Check if the error is related to a non-existing user
                if (ex.Message.Contains("USER_NOT_FOUND"))
                {
                    TempData["ErrorMsg"] = "The email address is not registered. Please check the email and try again.";
                }
                else
                {
                    TempData["ErrorMsg"] = "An error occurred. Please try again later.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"General Exception: {ex.Message}");
                TempData["ErrorMsg"] = "An unexpected error occurred. Please try again.";
            }

            return RedirectToAction("Login");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            HttpContext.Response.Cookies.Delete(".AspNetCore.Identity.Application");

            TempData["SuccessMsg"] = "You have successfully logged out.";
            return RedirectToAction("Login");
        }

    }
}
