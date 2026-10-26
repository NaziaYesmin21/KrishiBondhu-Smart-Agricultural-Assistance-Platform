
using KrishiBondhu___Smart_Agricultural_Assistance_Platform.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KrishiBondhu___Smart_Agricultural_Assistance_Platform.Controllers
{
    public class AgroProductServicesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AgroProductServicesController(ApplicationDbContext context)
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

        // List this firm's products and services
        public async Task<IActionResult> Index()
        {
            if (!IsAgroFirm())
                return RedirectToAction("Login", "Auth");

            var firm = await GetCurrentFirmAsync();

            if (firm == null)
                return NotFound("Agro Firm profile was not found.");

            var items = await _context.AgroProductServices
                .Where(p => p.AgroFirmId == firm.AgroFirmId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(items);
        }

        // Open Add form
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!IsAgroFirm())
                return RedirectToAction("Login", "Auth");

            var firm = await GetCurrentFirmAsync();

            if (firm == null)
                return NotFound("Agro Firm profile was not found.");

            return View(new AgroProductService());
        }

        // Save a new product or service
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AgroProductService model)
        {
            if (!IsAgroFirm())
                return RedirectToAction("Login", "Auth");

            var firm = await GetCurrentFirmAsync();

            if (firm == null)
                return NotFound("Agro Firm profile was not found.");

            if (model.ItemType != "Product" && model.ItemType != "Service")
            {
                ModelState.AddModelError(
                    nameof(model.ItemType),
                    "Please select Product or Service."
                );
            }

            if (!ModelState.IsValid)
                return View(model);

            // Never trust the firm ID submitted by the browser.
            model.AgroFirmId = firm.AgroFirmId;
            model.CreatedAt = DateTime.UtcNow;

            _context.AgroProductServices.Add(model);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Product or service added successfully!";

            return RedirectToAction(nameof(Index));
        }

        // Open Edit form for this firm's item only
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAgroFirm())
                return RedirectToAction("Login", "Auth");

            var firm = await GetCurrentFirmAsync();

            if (firm == null)
                return NotFound("Agro Firm profile was not found.");

            var item = await _context.AgroProductServices
                .FirstOrDefaultAsync(p =>
                    p.AgroProductServiceId == id &&
                    p.AgroFirmId == firm.AgroFirmId);

            if (item == null)
                return NotFound();

            return View(item);
        }

        // Update this firm's item
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AgroProductService model)
        {
            if (!IsAgroFirm())
                return RedirectToAction("Login", "Auth");

            var firm = await GetCurrentFirmAsync();

            if (firm == null)
                return NotFound("Agro Firm profile was not found.");

            var item = await _context.AgroProductServices
                .FirstOrDefaultAsync(p =>
                    p.AgroProductServiceId == id &&
                    p.AgroFirmId == firm.AgroFirmId);

            if (item == null)
                return NotFound();

            if (model.ItemType != "Product" && model.ItemType != "Service")
            {
                ModelState.AddModelError(
                    nameof(model.ItemType),
                    "Please select Product or Service."
                );
            }

            if (!ModelState.IsValid)
            {
                model.AgroProductServiceId = item.AgroProductServiceId;
                return View(model);
            }

            item.Name = model.Name.Trim();
            item.ItemType = model.ItemType;
            item.Description = model.Description.Trim();
            item.Price = model.Price;
            item.Unit = model.Unit?.Trim();
            item.IsAvailable = model.IsAvailable;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Item updated successfully!";

            return RedirectToAction(nameof(Index));
        }

        // Delete only this firm's item
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAgroFirm())
                return RedirectToAction("Login", "Auth");

            var firm = await GetCurrentFirmAsync();

            if (firm == null)
                return NotFound("Agro Firm profile was not found.");

            var item = await _context.AgroProductServices
                .FirstOrDefaultAsync(p =>
                    p.AgroProductServiceId == id &&
                    p.AgroFirmId == firm.AgroFirmId);

            if (item == null)
                return NotFound();

            _context.AgroProductServices.Remove(item);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Item deleted successfully!";

            return RedirectToAction(nameof(Index));
        }
    }
}
