using Firebase.Auth;
using Microsoft.AspNetCore.Mvc;

namespace TMS.Controllers.Home
{
    public class HomeController : Controller
    {
        //Shows The Dashboard and Gets User Role When Logging In, Displaying Their Respective Views Based On Roles.
        public IActionResult Dashboard()
        {
            var userRole = HttpContext.Session.GetString("UserRole");

            if(userRole == "Employee") 
            { 
               return View("EmployeeDashboard"); 
            } 
            else if (userRole == "Trainor")
            { 
               return View("TrainorDashboard"); 
            }
            else if (userRole == "Admin")
            { 
               return View("AdminDashboard");
            }

            TempData["NotLoggedInMsg"] = "Please Try Logging In!";
            return RedirectToAction("Error", "Status");
        }
    }
}
