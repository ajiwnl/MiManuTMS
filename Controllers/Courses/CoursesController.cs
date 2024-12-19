﻿using Microsoft.AspNetCore.Mvc;

namespace TMS.Controllers.Courses
{
    public class CoursesController : Controller
    {
        public IActionResult Course(int? month, int? year)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            ViewData["ActivePage"] = "Courses";

            Calendar(month, year);

            if (userRole == "Employee")
            {
                return View("Employee");
            }
            else if (userRole == "Trainor")
            {
                return View("Trainor");
            }

            TempData["NotLoggedInMsg"] = "Please Try Logging In!";
            return RedirectToAction("Error", "Status");
        }

        [HttpGet]
        public IActionResult CourseInfo(string? name)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            ViewData["ActivePage"] = "Courses";
            ViewData["CourseName"] = name;
            ViewData["CurrentTab"] = "CourseInfo";

            if (userRole == "Employee")
            {
                return View("CourseInfo");
            }
            else if (userRole == "Trainor")
            {
                return View("TCourseInfo");
            }

            TempData["NotLoggedInMsg"] = "Please Try Logging In!";
            return RedirectToAction("Error", "Status");
        }

        [HttpGet]
        public IActionResult CourseModules(string? name)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            ViewData["ActivePage"] = "Courses";
            ViewData["CourseName"] = name;
            ViewData["CurrentTab"] = "CourseModules";

            if (userRole == "Employee")
            {
                return View("CourseModules");
            }
            else if (userRole == "Trainor")
            {
                return View("TCourseModules");
            }

            TempData["NotLoggedInMsg"] = "Please Try Logging In!";
            return RedirectToAction("Error", "Status");
        }

        public void Calendar(int? month, int? year)
        {
            var currentDate = DateTime.Now;
            int currentMonth = month ?? currentDate.Month;
            int currentYear = year ?? currentDate.Year;

            // Pass month and year to the view
            ViewBag.CurrentMonth = currentMonth;
            ViewBag.CurrentYear = currentYear;
        }
    }
}
