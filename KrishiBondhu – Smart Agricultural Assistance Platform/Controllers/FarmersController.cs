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

        public async Task<IActionResult> Index(string searchTerm)
        {
            var farmers = await _farmerService
                .SearchFarmersAsync(searchTerm);

            ViewBag.SearchTerm = searchTerm;

            return View(farmers);
        }

        public async Task<IActionResult> Details(int? id)
        {
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

        public IActionResult Create()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            return View();
        }

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

                return RedirectToAction(nameof(Index));
            }

            return View(farmer);
        }

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

                return RedirectToAction(nameof(Index));
            }

            return View(farmer);
        }

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

            return RedirectToAction(nameof(Index));
        }
    }
}