using Microsoft.AspNetCore.Mvc;

namespace TMS.Controllers.Courses
{
    public class CoursesController : Controller
    {
        public IActionResult Course()
        {
            return View();
        }
    }
}
