
using KrishiBondhu___Smart_Agricultural_Assistance_Platform.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KrishiBondhu___Smart_Agricultural_Assistance_Platform.Controllers
{
    public class AgroFirmController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AgroFirmController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsAgroFirm()
        {
            return HttpContext.Session.GetString("Role") == "AgroFirm";
        }

        private async Task<AgroFirm?> GetCurrentFirmAsync()
        {
            var username = HttpContext.Session.GetString("Username");

            if (string.IsNullOrWhiteSpace(username))
                return null;

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
                return null;

            return await _context.AgroFirms
                .FirstOrDefaultAsync(f => f.Email == user.Email);
        }

        public async Task<IActionResult> Dashboard()
        {
            if (!IsAgroFirm())
                return RedirectToAction("Login", "Auth");

            var firm = await GetCurrentFirmAsync();

            if (firm == null)
            {
                ViewBag.FirmName = HttpContext.Session.GetString("Username");
                ViewBag.OwnerName = "Not provided";
                ViewBag.BusinessType = "Not specified";
                ViewBag.Phone = "Not provided";
                ViewBag.Address = "Not provided";
                ViewBag.Description = "No firm profile found.";
                ViewBag.IsActive = false;

                return View();
            }

            ViewBag.FirmName = firm.FirmName;
            ViewBag.OwnerName = firm.OwnerName;

            ViewBag.BusinessType = firm.BusinessType switch
            {
                "InputFirm" => "Agricultural Input Firm",
                "ServiceFirm" => "Agricultural Service Firm",
                _ => firm.BusinessType
            };

            ViewBag.Phone = firm.Phone;
            ViewBag.Address = firm.Address;
            ViewBag.Description = firm.Description ?? "No description added.";
            ViewBag.IsActive = firm.IsActive;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            if (!IsAgroFirm())
                return RedirectToAction("Login", "Auth");

            var firm = await GetCurrentFirmAsync();

            if (firm == null)
                return NotFound("Agro Firm profile was not found.");

            return View(firm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(AgroFirm model)
        {
            if (!IsAgroFirm())
                return RedirectToAction("Login", "Auth");

            var firm = await GetCurrentFirmAsync();

            if (firm == null)
                return NotFound("Agro Firm profile was not found.");

            if (model.BusinessType != "InputFirm" &&
                model.BusinessType != "ServiceFirm")
            {
                ModelState.AddModelError(
                    nameof(model.BusinessType),
                    "Please select a valid business type."
                );
            }

            if (!ModelState.IsValid)
            {
                model.AgroFirmId = firm.AgroFirmId;
                model.Email = firm.Email;
                return View(model);
            }

            // Update only the fields the firm is allowed to edit.
            firm.FirmName = model.FirmName.Trim();
            firm.OwnerName = model.OwnerName.Trim();
            firm.Phone = model.Phone.Trim();
            firm.Address = model.Address.Trim();
            firm.BusinessType = model.BusinessType;
            firm.Description = model.Description?.Trim();

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Firm profile updated successfully!";

            return RedirectToAction(nameof(Dashboard));
        }
    }
}
