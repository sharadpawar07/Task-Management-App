using Microsoft.AspNetCore.Mvc;

namespace TaskManagementApp.Controllers
{
    public class AuthViewController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        public IActionResult Logout()
        {
            return RedirectToAction("Login");
        }
    }
}
