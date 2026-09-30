using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KrishiBondhu___Smart_Agricultural_Assistance_Platform.Controllers
{
    public class CultivationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICultivationService _cultivationService;

        public CultivationsController(
            ApplicationDbContext context,
            ICultivationService cultivationService)
        {
            _context = context;
            _cultivationService = cultivationService;
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("Role") == "Admin";
        }

        // GET: Cultivations
        // Admin and User can view cultivation list
        public async Task<IActionResult> Index()
        {
            var cultivations =
                await _cultivationService.GetAllCultivationsAsync();

            return View(cultivations);
        }

        // GET: Cultivations/Details/5
        // Admin and User can view details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cultivation =
                await _cultivationService.GetCultivationByIdAsync(id.Value);

            if (cultivation == null)
            {
                return NotFound();
            }

            return View(cultivation);
        }

        // GET: Cultivations/Create
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

            ViewData["CropId"] = new SelectList(
                _context.Crops,
                "CropId",
                "CropName"
            );

            return View();
        }

        // POST: Cultivations/Create
        // Only Admin can create
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
                await _cultivationService
                    .CreateCultivationAsync(cultivation);

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

            var cultivation =
                await _cultivationService.GetCultivationByIdAsync(id.Value);

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
        // Only Admin can edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Cultivation cultivation)
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
                var updated =
                    await _cultivationService
                        .UpdateCultivationAsync(cultivation);

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

            var cultivation =
                await _cultivationService.GetCultivationByIdAsync(id.Value);

            if (cultivation == null)
            {
                return NotFound();
            }

            return View(cultivation);
        }

        // POST: Cultivations/Delete/5
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
                await _cultivationService.DeleteCultivationAsync(id);

            if (!deleted)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}