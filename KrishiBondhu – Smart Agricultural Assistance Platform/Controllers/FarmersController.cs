using Microsoft.AspNetCore.Mvc;

namespace KrishiBondhu___Smart_Agricultural_Assistance_Platform.Controllers
{
    public class FarmersController : Controller
    {
        private readonly IFarmerService _farmerService;

        public FarmersController(IFarmerService farmerService)
        {
            _farmerService = farmerService;
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("Role") == "Admin";
        }


        // =========================
        // Farmer List
        // Search + Filter + Pagination
        // =========================
        public async Task<IActionResult> Index(
            string searchTerm,
            string address,
            int page = 1)
        {
            var farmers = await _farmerService
                .SearchFarmersAsync(searchTerm);

            // Address Filter
            if (!string.IsNullOrWhiteSpace(address))
            {
                farmers = farmers
                    .Where(f => f.Address.Contains(address))
                    .ToList();
            }

            // Pagination
            int pageSize = 5;

            int totalFarmers = farmers.Count;

            int totalPages = (int)Math.Ceiling(
                totalFarmers / (double)pageSize);

            if (totalPages == 0)
            {
                totalPages = 1;
            }

            if (page < 1)
            {
                page = 1;
            }

            if (page > totalPages)
            {
                page = totalPages;
            }

            var paginatedFarmers = farmers
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.SearchTerm = searchTerm;
            ViewBag.Address = address;

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalFarmers = totalFarmers;

            return View(paginatedFarmers);
        }


        // =========================
        // Farmer Profile / Details
        // =========================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var profile = await _farmerService
                .GetFarmerProfileAsync(id.Value);

            if (profile == null)
            {
                return NotFound();
            }

            return View(profile);
        }


        // =========================
        // Create Farmer - GET
        // =========================
        public IActionResult Create()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            return View();
        }


        // =========================
        // Create Farmer - POST
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("FarmerId,Name,Phone,Address,Email")] Farmer farmer)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (ModelState.IsValid)
            {
                await _farmerService.CreateFarmerAsync(farmer);

                TempData["SuccessMessage"] =
                    "Farmer created successfully!";

                return RedirectToAction(nameof(Index));
            }

            return View(farmer);
        }


        // =========================
        // Edit Farmer - GET
        // =========================
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

            var farmer = await _farmerService
                .GetFarmerByIdAsync(id.Value);

            if (farmer == null)
            {
                return NotFound();
            }

            return View(farmer);
        }


        // =========================
        // Edit Farmer - POST
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("FarmerId,Name,Phone,Address,Email")] Farmer farmer)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (id != farmer.FarmerId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var updated = await _farmerService
                    .UpdateFarmerAsync(farmer);

                if (!updated)
                {
                    return NotFound();
                }

                TempData["SuccessMessage"] =
                    "Farmer updated successfully!";

                return RedirectToAction(nameof(Index));
            }

            return View(farmer);
        }


        // =========================
        // Delete Farmer - GET
        // =========================
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

            var farmer = await _farmerService
                .GetFarmerByIdAsync(id.Value);

            if (farmer == null)
            {
                return NotFound();
            }

            return View(farmer);
        }


        // =========================
        // Delete Farmer - POST
        // =========================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            var deleted = await _farmerService
                .DeleteFarmerAsync(id);

            if (!deleted)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] =
                "Farmer deleted successfully!";

            return RedirectToAction(nameof(Index));
        }
    }
}