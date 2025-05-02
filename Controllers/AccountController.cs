using Microsoft.AspNetCore.Mvc;

namespace YourAppNamespace.Controllers
{
    public class AccountController : Controller
    {
        // Redirect to the Identity Login page
        [HttpGet]
        public IActionResult Login()
        {
            return Redirect("/Identity/Account/Login");
        }

        // Redirect to the Identity Register page
        [HttpGet]
        public IActionResult Register()
        {
            return Redirect("/Identity/Account/Register");
        }

        // Optional: Redirect to Identity logout (if exposed)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            return Redirect("/Identity/Account/Logout");
        }

        // Optional: Handle access denied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return Redirect("/Identity/Account/AccessDenied");
        }
    }
}
