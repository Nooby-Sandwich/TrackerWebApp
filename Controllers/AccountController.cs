using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TrackerWebApp.Models;

namespace TrackerWebApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<IdentityUser> _signIn;
        private readonly UserManager<IdentityUser> _users;

        public AccountController(
            UserManager<IdentityUser> users,
            SignInManager<IdentityUser> signInManager)
        {
            _users = users;
            _signIn = signInManager;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel m)
        {
            if (!ModelState.IsValid) return View(m);

            var result = await _signIn.PasswordSignInAsync(m.Email, m.Password, m.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
                return LocalRedirect(m.ReturnUrl ?? Url.Content("~/"));

            ModelState.AddModelError("", "Invalid login attempt");
            return View(m);
        }

        [HttpGet]
        public IActionResult Register(string returnUrl = null)
        {
            return View(new RegisterViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel m)
        {
            if (!ModelState.IsValid) return View(m);

            var user = new IdentityUser { UserName = m.Email, Email = m.Email };
            var res = await _users.CreateAsync(user, m.Password);
            if (res.Succeeded)
            {
                await _signIn.SignInAsync(user, isPersistent: false);
                return LocalRedirect(m.ReturnUrl ?? Url.Content("~/"));
            }

            foreach (var e in res.Errors)
                ModelState.AddModelError("", e.Description);

            return View(m);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signIn.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
