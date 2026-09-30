using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KrishiBondhu___Smart_Agricultural_Assistance_Platform.Controllers
{
    public class DiseasesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDiseaseService _diseaseService;

        public DiseasesController(
            ApplicationDbContext context,
            IDiseaseService diseaseService)
        {
            _context = context;
            _diseaseService = diseaseService;
        }

        // =========================
        // CHECK ADMIN
        // =========================

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("Role") == "Admin";
        }

        // =========================
        // INDEX
        // =========================

        public async Task<IActionResult> Index()
        {
            var diseases =
                await _diseaseService.GetAllDiseasesAsync();

            return View(diseases);
        }

        // =========================
        // DETAILS
        // =========================

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var disease =
                await _diseaseService.GetDiseaseByIdAsync(id.Value);

            if (disease == null)
            {
                return NotFound();
            }

            return View(disease);
        }

        // =========================
        // CREATE - GET
        // =========================

        [HttpGet]
        public IActionResult Create()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            ViewData["CropId"] = new SelectList(
                _context.Crops,
                "CropId",
                "CropName"
            );

            return View();
        }

        // =========================
        // CREATE - POST
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Disease disease)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            ModelState.Remove("Crop");

            if (ModelState.IsValid)
            {
                await _diseaseService
                    .CreateDiseaseAsync(disease);

                return RedirectToAction(nameof(Index));
            }

            ViewData["CropId"] = new SelectList(
                _context.Crops,
                "CropId",
                "CropName",
                disease.CropId
            );

            return View(disease);
        }

        // =========================
        // EDIT - GET
        // =========================

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            if (id == null)
            {
                return NotFound();
            }

            var disease =
                await _diseaseService.GetDiseaseByIdAsync(id.Value);

            if (disease == null)
            {
                return NotFound();
            }

            ViewData["CropId"] = new SelectList(
                _context.Crops,
                "CropId",
                "CropName",
                disease.CropId
            );

            return View(disease);
        }

        // =========================
        // EDIT - POST
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Disease disease)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            if (id != disease.DiseaseId)
            {
                return NotFound();
            }

            ModelState.Remove("Crop");

            if (ModelState.IsValid)
            {
                var updated =
                    await _diseaseService
                        .UpdateDiseaseAsync(disease);

                if (!updated)
                {
                    return NotFound();
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["CropId"] = new SelectList(
                _context.Crops,
                "CropId",
                "CropName",
                disease.CropId
            );

            return View(disease);
        }

        // =========================
        // DELETE - GET
        // =========================

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            if (id == null)
            {
                return NotFound();
            }

            var disease =
                await _diseaseService.GetDiseaseByIdAsync(id.Value);

            if (disease == null)
            {
                return NotFound();
            }

            return View(disease);
        }

        // =========================
        // DELETE - POST
        // =========================

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            var deleted =
                await _diseaseService.DeleteDiseaseAsync(id);

            if (!deleted)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}