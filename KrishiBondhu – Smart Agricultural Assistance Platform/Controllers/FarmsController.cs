using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KrishiBondhu___Smart_Agricultural_Assistance_Platform.Controllers
{
    public class FarmsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFarmService _farmService;

        public FarmsController(
            ApplicationDbContext context,
            IFarmService farmService)
        {
            _context = context;
            _farmService = farmService;
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("Role") == "Admin";
        }

        public async Task<IActionResult> Index()
        {
            var farms = await _farmService.GetAllFarmsAsync();

            return View(farms);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var farm = await _farmService.GetFarmByIdAsync(id.Value);

            if (farm == null)
            {
                return NotFound();
            }

            return View(farm);
        }

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Farm farm)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            ModelState.Remove("Farmer");

            if (ModelState.IsValid)
            {
                await _farmService.CreateFarmAsync(farm);

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

            var farm = await _farmService.GetFarmByIdAsync(id.Value);

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

            ModelState.Remove("Farmer");

            if (ModelState.IsValid)
            {
                var updated = await _farmService.UpdateFarmAsync(farm);

                if (!updated)
                {
                    return NotFound();
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

            var farm = await _farmService.GetFarmByIdAsync(id.Value);

            if (farm == null)
            {
                return NotFound();
            }

            return View(farm);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            var deleted = await _farmService.DeleteFarmAsync(id);

            if (!deleted)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}