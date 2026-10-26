
using KrishiBondhu___Smart_Agricultural_Assistance_Platform.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KrishiBondhu___Smart_Agricultural_Assistance_Platform.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // REGISTER - GET
        // =========================

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        // =========================
        // REGISTER - POST
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // Validate account type
            if (model.AccountType != "User" &&
                model.AccountType != "AgroFirm")
            {
                ModelState.AddModelError(
                    "AccountType", "Invalid account type.");
            }

            // Validate Agro Firm information
            if (model.AccountType == "AgroFirm")
            {
                if (string.IsNullOrWhiteSpace(model.FirmName))
                    ModelState.AddModelError(
                        "FirmName", "Firm name is required.");

                if (string.IsNullOrWhiteSpace(model.OwnerName))
                    ModelState.AddModelError(
                        "OwnerName", "Owner name is required.");

                if (string.IsNullOrWhiteSpace(model.Phone))
                    ModelState.AddModelError(
                        "Phone", "Phone number is required.");

                if (string.IsNullOrWhiteSpace(model.Address))
                    ModelState.AddModelError(
                        "Address", "Address is required.");

                if (model.BusinessType != "InputFirm" &&
                    model.BusinessType != "ServiceFirm")
                {
                    ModelState.AddModelError(
                        "BusinessType", "Select a valid business type.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var username = model.Username.Trim();
            var email = model.Email.Trim();

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Username == username ||
                    u.Email == email);

            if (existingUser != null)
            {
                ModelState.AddModelError(
                    "",
                    "Username or email already exists.");

                return View(model);
            }

            // Create the user account
            var user = new User
            {
                Username = username,
                Email = email,
                Password = model.Password,
                Role = model.AccountType == "AgroFirm"
                    ? "AgroFirm"
                    : "User"
            };

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Create Agro Firm profile
                if (model.AccountType == "AgroFirm")
                {
                    var firm = new AgroFirm
                    {
                        FirmName = model.FirmName!.Trim(),
                        OwnerName = model.OwnerName!.Trim(),
                        Phone = model.Phone!.Trim(),
                        Email = email,
                        Address = model.Address!.Trim(),
                        BusinessType = model.BusinessType!,
                        Description = model.Description?.Trim(),
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };

                    _context.Set<AgroFirm>().Add(firm);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                TempData["SuccessMessage"] =
                    "Registration successful! Please login.";

                return RedirectToAction(nameof(Login));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                System.Diagnostics.Debug.WriteLine(
                    "Registration Error: " + ex);

                ModelState.AddModelError(
                    "",
                    "Registration failed. Please try again.");

                return View(model);
            }
        }

        // =========================
        // LOGIN - GET
        // =========================

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // =========================
        // LOGIN - POST
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var username = model.Username.Trim();

            var user = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Username == username &&
                    u.Password == model.Password);

            if (user == null)
            {
                ModelState.AddModelError(
                    "",
                    "Invalid username or password.");

                return View(model);
            }

            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Role", user.Role);

            if (user.Role == "AgroFirm")
            {
                return RedirectToAction("Dashboard", "AgroFirm");
            }

            return RedirectToAction("Index", "Home");
        }

        // =========================
        // LOGOUT
        // =========================

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            Response.Headers["Cache-Control"] =
                "no-cache, no-store, must-revalidate";

            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            return RedirectToAction(nameof(Login));
        }
    }
}
