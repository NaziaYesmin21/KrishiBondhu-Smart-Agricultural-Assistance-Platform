using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace KrishiBondhu___Smart_Agricultural_Assistance_Platform.Controllers
{
    public class SoilTestsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SoilTestsController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("Role") == "Admin";
        }

        // GET: SoilTests
        public async Task<IActionResult> Index()
        {
            var soilTests = await _context.SoilTests
                .Include(s => s.Farm)
                .ToListAsync();

            return View(soilTests);
        }

        // GET: SoilTests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var soilTest = await _context.SoilTests
                .Include(s => s.Farm)
                .FirstOrDefaultAsync(s => s.SoilTestId == id);

            if (soilTest == null)
            {
                return NotFound();
            }

            return View(soilTest);
        }

        // GET: SoilTests/Create
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

            return View();
        }

        // POST: SoilTests/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SoilTest soilTest)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            ModelState.Remove("Farm");

            if (ModelState.IsValid)
            {
                _context.SoilTests.Add(soilTest);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewData["FarmId"] = new SelectList(
                _context.Farms,
                "FarmId",
                "FarmName",
                soilTest.FarmId
            );

            return View(soilTest);
        }

        // GET: SoilTests/Edit/5
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

            var soilTest = await _context.SoilTests.FindAsync(id);

            if (soilTest == null)
            {
                return NotFound();
            }

            ViewData["FarmId"] = new SelectList(
                _context.Farms,
                "FarmId",
                "FarmName",
                soilTest.FarmId
            );

            return View(soilTest);
        }

        // POST: SoilTests/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SoilTest soilTest)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (id != soilTest.SoilTestId)
            {
                return NotFound();
            }

            ModelState.Remove("Farm");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(soilTest);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SoilTestExists(soilTest.SoilTestId))
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
                soilTest.FarmId
            );

            return View(soilTest);
        }

        // GET: SoilTests/Delete/5
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

            var soilTest = await _context.SoilTests
                .Include(s => s.Farm)
                .FirstOrDefaultAsync(s => s.SoilTestId == id);

            if (soilTest == null)
            {
                return NotFound();
            }

            return View(soilTest);
        }

        // POST: SoilTests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            var soilTest = await _context.SoilTests.FindAsync(id);

            if (soilTest != null)
            {
                _context.SoilTests.Remove(soilTest);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool SoilTestExists(int id)
        {
            return _context.SoilTests.Any(s => s.SoilTestId == id);
        }
    }
}