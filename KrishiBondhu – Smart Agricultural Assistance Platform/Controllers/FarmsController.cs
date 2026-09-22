using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace KrishiBondhu___Smart_Agricultural_Assistance_Platform.Controllers
{
    public class FarmsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FarmsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================
        // ADMIN CHECK
        // =========================================

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("Role") == "Admin";
        }

        // =========================================
        // INDEX
        // =========================================

        public async Task<IActionResult> Index()
        {
            var farms = await _context.Farms
                .Include(f => f.Farmer)
                .ToListAsync();

            return View(farms);
        }

        // =========================================
        // DETAILS
        // =========================================

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var farm = await _context.Farms
                .Include(f => f.Farmer)
                .FirstOrDefaultAsync(f => f.FarmId == id);

            if (farm == null)
            {
                return NotFound();
            }

            return View(farm);
        }

        // =========================================
        // CREATE - GET
        // =========================================

        [HttpGet]
        public IActionResult Create()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewData["FarmerId"] = new SelectList(
                _context.Farmers,
                "FarmerId",
                "Name"
            );

            return View();
        }

        // =========================================
        // CREATE - POST
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Farm farm)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            // Farmer is a navigation property.
            // Only FarmerId is submitted from the form.
            ModelState.Remove("Farmer");

            if (ModelState.IsValid)
            {
                _context.Farms.Add(farm);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewData["FarmerId"] = new SelectList(
                _context.Farmers,
                "FarmerId",
                "Name",
                farm.FarmerId
            );

            return View(farm);
        }

        // =========================================
        // EDIT - GET
        // =========================================

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (id == null)
            {
                return NotFound();
            }

            var farm = await _context.Farms.FindAsync(id);

            if (farm == null)
            {
                return NotFound();
            }

            ViewData["FarmerId"] = new SelectList(
                _context.Farmers,
                "FarmerId",
                "Name",
                farm.FarmerId
            );

            return View(farm);
        }

        // =========================================
        // EDIT - POST
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Farm farm)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (id != farm.FarmId)
            {
                return NotFound();
            }

            // Farmer is a navigation property.
            ModelState.Remove("Farmer");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(farm);

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FarmExists(farm.FarmId))
                    {
                        return NotFound();
                    }

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["FarmerId"] = new SelectList(
                _context.Farmers,
                "FarmerId",
                "Name",
                farm.FarmerId
            );

            return View(farm);
        }

        // =========================================
        // DELETE - GET
        // =========================================

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (id == null)
            {
                return NotFound();
            }

            var farm = await _context.Farms
                .Include(f => f.Farmer)
                .FirstOrDefaultAsync(f => f.FarmId == id);

            if (farm == null)
            {
                return NotFound();
            }

            return View(farm);
        }

        // =========================================
        // DELETE - POST
        // =========================================

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            var farm = await _context.Farms.FindAsync(id);

            if (farm != null)
            {
                _context.Farms.Remove(farm);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // =========================================
        // FARM EXISTS
        // =========================================

        private bool FarmExists(int id)
        {
            return _context.Farms.Any(e => e.FarmId == id);
        }
    }

}