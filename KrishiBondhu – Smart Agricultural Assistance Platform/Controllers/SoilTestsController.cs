using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KrishiBondhu___Smart_Agricultural_Assistance_Platform.Controllers
{
    public class SoilTestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ISoilTestService _soilTestService;

        public SoilTestsController(
            ApplicationDbContext context,
            ISoilTestService soilTestService)
        {
            _context = context;
            _soilTestService = soilTestService;
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("Role") == "Admin";
        }

        // GET: SoilTests
        // Admin and User can view soil tests
        public async Task<IActionResult> Index()
        {
            var soilTests =
                await _soilTestService.GetAllSoilTestsAsync();

            return View(soilTests);
        }

        // GET: SoilTests/Details/5
        // Admin and User can view details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var soilTest =
                await _soilTestService.GetSoilTestByIdAsync(id.Value);

            if (soilTest == null)
            {
                return NotFound();
            }

            return View(soilTest);
        }

        // GET: SoilTests/Create
        // Only Admin can create
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
        // Only Admin can create
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
                await _soilTestService
                    .CreateSoilTestAsync(soilTest);

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
        // Only Admin can edit
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

            var soilTest =
                await _soilTestService.GetSoilTestByIdAsync(id.Value);

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
        // Only Admin can edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            SoilTest soilTest)
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
                var updated =
                    await _soilTestService
                        .UpdateSoilTestAsync(soilTest);

                if (!updated)
                {
                    return NotFound();
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
        // Only Admin can delete
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

            var soilTest =
                await _soilTestService.GetSoilTestByIdAsync(id.Value);

            if (soilTest == null)
            {
                return NotFound();
            }

            return View(soilTest);
        }

        // POST: SoilTests/Delete/5
        // Only Admin can delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            var deleted =
                await _soilTestService.DeleteSoilTestAsync(id);

            if (!deleted)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}