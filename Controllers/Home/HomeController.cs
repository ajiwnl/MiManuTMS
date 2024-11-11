using Microsoft.AspNetCore.Mvc;

namespace TMS.Controllers.Home
{
    public class HomeController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
