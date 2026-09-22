using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace KrishiBondhu___Smart_Agricultural_Assistance_Platform.Controllers
{
    public class CultivationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CultivationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("Role") == "Admin";
        }

        // GET: Cultivations
        public async Task<IActionResult> Index()
        {
            var cultivations = await _context.Cultivations
                .Include(c => c.Farm)
                .Include(c => c.Crop)
                .ToListAsync();

            return View(cultivations);
        }

        // GET: Cultivations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cultivation = await _context.Cultivations
                .Include(c => c.Farm)
                .Include(c => c.Crop)
                .FirstOrDefaultAsync(c => c.CultivationId == id);

            if (cultivation == null)
            {
                return NotFound();
            }

            return View(cultivation);
        }

        // GET: Cultivations/Create
        [HttpGet]
        public IActionResult Create()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewData["FarmId"] = new SelectList(
                _context.Farms,
                "FarmId",
                "FarmName"
            );

            ViewData["CropId"] = new SelectList(
                _context.Crops,
                "CropId",
                "CropName"
            );

            return View();
        }

        // POST: Cultivations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Cultivation cultivation)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            ModelState.Remove("Farm");
            ModelState.Remove("Crop");

            if (ModelState.IsValid)
            {
                _context.Cultivations.Add(cultivation);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewData["FarmId"] = new SelectList(
                _context.Farms,
                "FarmId",
                "FarmName",
                cultivation.FarmId
            );

            ViewData["CropId"] = new SelectList(
                _context.Crops,
                "CropId",
                "CropName",
                cultivation.CropId
            );

            return View(cultivation);
        }

        // GET: Cultivations/Edit/5
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

            var cultivation = await _context.Cultivations.FindAsync(id);

            if (cultivation == null)
            {
                return NotFound();
            }

            ViewData["FarmId"] = new SelectList(
                _context.Farms,
                "FarmId",
                "FarmName",
                cultivation.FarmId
            );

            ViewData["CropId"] = new SelectList(
                _context.Crops,
                "CropId",
                "CropName",
                cultivation.CropId
            );

            return View(cultivation);
        }

        // POST: Cultivations/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Cultivation cultivation)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (id != cultivation.CultivationId)
            {
                return NotFound();
            }

            ModelState.Remove("Farm");
            ModelState.Remove("Crop");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cultivation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CultivationExists(cultivation.CultivationId))
                    {
                        return NotFound();
                    }

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["FarmId"] = new SelectList(
                _context.Farms,
                "FarmId",
                "FarmName",
                cultivation.FarmId
            );

            ViewData["CropId"] = new SelectList(
                _context.Crops,
                "CropId",
                "CropName",
                cultivation.CropId
            );

            return View(cultivation);
        }

        // GET: Cultivations/Delete/5
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

            var cultivation = await _context.Cultivations
                .Include(c => c.Farm)
                .Include(c => c.Crop)
                .FirstOrDefaultAsync(c => c.CultivationId == id);

            if (cultivation == null)
            {
                return NotFound();
            }

            return View(cultivation);
        }

        // POST: Cultivations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            var cultivation = await _context.Cultivations.FindAsync(id);

            if (cultivation != null)
            {
                _context.Cultivations.Remove(cultivation);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CultivationExists(int id)
        {
            return _context.Cultivations
                .Any(c => c.CultivationId == id);
        }
    }

}