using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using BrewNestCafe.Models;
using System.Security.Claims;

namespace BrewNestCafe.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<AdminUser> _signInManager;
        private readonly UserManager<AdminUser> _userManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            SignInManager<AdminUser> signInManager,
            UserManager<AdminUser> userManager,
            ILogger<AccountController> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                try
                {
                    // Find user by email
                    var user = await _userManager.FindByEmailAsync(model.Email);

                    if (user != null)
                    {
                        // Sign in with password
                        var result = await _signInManager.PasswordSignInAsync(
                            user.UserName,
                            model.Password,
                            model.RememberMe,
                            lockoutOnFailure: false);

                        if (result.Succeeded)
                        {
                            // Check if user is in Admin role
                            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

                            if (isAdmin)
                            {
                                _logger.LogInformation("Admin user {Email} logged in.", model.Email);
                                return RedirectToLocal(returnUrl ?? "/Admin/Dashboard");
                            }
                            else
                            {
                                await _signInManager.SignOutAsync();
                                ModelState.AddModelError(string.Empty, "Access denied. Admin privileges required.");
                                _logger.LogWarning("Non-admin user {Email} attempted login.", model.Email);
                            }
                        }
                        else if (result.IsLockedOut)
                        {
                            ModelState.AddModelError(string.Empty, "Account locked out. Please try again later.");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during login for {Email}", model.Email);
                    ModelState.AddModelError(string.Empty, "An error occurred during login.");
                }
            }

            // If we got this far, something failed
            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
    }
}