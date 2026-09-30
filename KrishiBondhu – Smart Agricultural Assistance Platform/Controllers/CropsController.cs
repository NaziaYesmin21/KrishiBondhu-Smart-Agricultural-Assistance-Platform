using Microsoft.AspNetCore.Mvc;

namespace KrishiBondhu___Smart_Agricultural_Assistance_Platform.Controllers
{
    public class CropsController : Controller
    {
        private readonly ICropService _cropService;

        public CropsController(ICropService cropService)
        {
            _cropService = cropService;
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("Role") == "Admin";
        }

        // GET: Crops
        // Admin and User can view crop list
        public async Task<IActionResult> Index()
        {
            var crops = await _cropService.GetAllCropsAsync();

            return View(crops);
        }

        // GET: Crops/Details/5
        // Admin and User can view details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var crop = await _cropService.GetCropByIdAsync(id.Value);

            if (crop == null)
            {
                return NotFound();
            }

            return View(crop);
        }

        // GET: Crops/Create
        // Only Admin can create
        [HttpGet]
        public IActionResult Create()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            return View();
        }

        // POST: Crops/Create
        // Only Admin can create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Crop crop)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (ModelState.IsValid)
            {
                await _cropService.CreateCropAsync(crop);

                return RedirectToAction(nameof(Index));
            }

            return View(crop);
        }

        // GET: Crops/Edit/5
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

            var crop = await _cropService.GetCropByIdAsync(id.Value);

            if (crop == null)
            {
                return NotFound();
            }

            return View(crop);
        }

        // POST: Crops/Edit/5
        // Only Admin can edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Crop crop)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (id != crop.CropId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var updated = await _cropService.UpdateCropAsync(crop);

                if (!updated)
                {
                    return NotFound();
                }

                return RedirectToAction(nameof(Index));
            }

            return View(crop);
        }

        // GET: Crops/Delete/5
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

            var crop = await _cropService.GetCropByIdAsync(id.Value);

            if (crop == null)
            {
                return NotFound();
            }

            return View(crop);
        }

        // POST: Crops/Delete/5
        // Only Admin can delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            var deleted = await _cropService.DeleteCropAsync(id);

            if (!deleted)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}