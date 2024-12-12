using Microsoft.AspNetCore.Mvc;

namespace TMS.Controllers.Status
{
    public class StatusController : Controller
    {
        public IActionResult Error()
        {
            return View();
        }
    }
}
