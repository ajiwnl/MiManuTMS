using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;

namespace TMS.Controllers.Diagnostics
{
    public class DiagnosticController : Controller
    {
        public IActionResult Summary()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            ViewData["ActivePage"] = "Diagnostic";

            if (userRole == "Employee")
            {
                return View("Summary");
            }
            else if (userRole == "Trainor")
            {
                return View("TSummary");
            }

            TempData["NotLoggedInMsg"] = "Please Try Logging In!";
            return RedirectToAction("Error", "Status");
        }
    }
}
