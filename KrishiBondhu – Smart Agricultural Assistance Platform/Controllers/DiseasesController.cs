
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace KrishiBondhu___Smart_Agricultural_Assistance_Platform.Controllers
{
    public class DiseasesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DiseasesController(ApplicationDbContext context)
        {
            _context = context;
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
            var diseases = await _context.Diseases
                .Include(d => d.Crop)
                .ToListAsync();

            return View(diseases);
        }

        // =========================
        // DETAILS
        // =========================

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var disease = await _context.Diseases
                .Include(d => d.Crop)
                .FirstOrDefaultAsync(d => d.DiseaseId == id);

            if (disease == null)
                return NotFound();

            return View(disease);
        }

        // =========================
        // CREATE - GET
        // =========================

        [HttpGet]
        public IActionResult Create()
        {
            if (!IsAdmin())
                return RedirectToAction("AccessDenied", "Auth");

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
                return RedirectToAction("AccessDenied", "Auth");

            ModelState.Remove("Crop");

            if (ModelState.IsValid)
            {
                _context.Diseases.Add(disease);
                await _context.SaveChangesAsync();

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
                return RedirectToAction("AccessDenied", "Auth");

            if (id == null)
                return NotFound();

            var disease = await _context.Diseases.FindAsync(id);

            if (disease == null)
                return NotFound();

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
        public async Task<IActionResult> Edit(int id, Disease disease)
        {
            if (!IsAdmin())
                return RedirectToAction("AccessDenied", "Auth");

            if (id != disease.DiseaseId)
                return NotFound();

            ModelState.Remove("Crop");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(disease);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DiseaseExists(disease.DiseaseId))
                        return NotFound();

                    throw;
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
                return RedirectToAction("AccessDenied", "Auth");

            if (id == null)
                return NotFound();

            var disease = await _context.Diseases
                .Include(d => d.Crop)
                .FirstOrDefaultAsync(d => d.DiseaseId == id);

            if (disease == null)
                return NotFound();

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
                return RedirectToAction("AccessDenied", "Auth");

            var disease = await _context.Diseases.FindAsync(id);

            if (disease != null)
            {
                _context.Diseases.Remove(disease);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // CHECK DISEASE EXISTS
        // =========================

        private bool DiseaseExists(int id)
        {
            return _context.Diseases.Any(d => d.DiseaseId == id);
        }
    }
}

