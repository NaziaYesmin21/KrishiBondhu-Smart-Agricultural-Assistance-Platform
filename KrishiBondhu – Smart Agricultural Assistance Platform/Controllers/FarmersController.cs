
using Microsoft.AspNetCore.Mvc;
using ClosedXML.Excel;

namespace KrishiBondhu___Smart_Agricultural_Assistance_Platform.Controllers
{
    public class FarmersController : Controller
    {
        private readonly IFarmerService _farmerService;
        private readonly IAuditLogService _auditLogService;
        private readonly IWebHostEnvironment _environment;

        public FarmersController(
            IFarmerService farmerService,
            IAuditLogService auditLogService,
            IWebHostEnvironment environment)
        {
            _farmerService = farmerService;
            _auditLogService = auditLogService;
            _environment = environment;
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("Role") == "Admin";
        }

        private string GetUsername()
        {
            return HttpContext.Session.GetString("Username")
                   ?? "Unknown User";
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

            if (!string.IsNullOrWhiteSpace(address))
            {
                farmers = farmers
                    .Where(f => f.Address.Contains(address))
                    .ToList();
            }

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
        // Export Farmers to Excel
        // =========================
        public async Task<IActionResult> ExportExcel()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            var farmers = await _farmerService
                .SearchFarmersAsync(null);

            using var workbook = new XLWorkbook();

            var worksheet = workbook.Worksheets.Add("Farmers");

            worksheet.Cell(1, 1).Value = "Farmer ID";
            worksheet.Cell(1, 2).Value = "Name";
            worksheet.Cell(1, 3).Value = "Phone";
            worksheet.Cell(1, 4).Value = "Address";
            worksheet.Cell(1, 5).Value = "Email";

            var headerRange = worksheet.Range("A1:E1");

            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor =
                XLColor.DarkGreen;
            headerRange.Style.Font.FontColor =
                XLColor.White;

            int row = 2;

            foreach (var farmer in farmers)
            {
                worksheet.Cell(row, 1).Value = farmer.FarmerId;
                worksheet.Cell(row, 2).Value = farmer.Name;
                worksheet.Cell(row, 3).Value = farmer.Phone;
                worksheet.Cell(row, 4).Value = farmer.Address;
                worksheet.Cell(row, 5).Value = farmer.Email;

                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            var content = stream.ToArray();

            await _auditLogService.LogAsync(
                GetUsername(),
                "Export Excel",
                "Exported farmer list to Excel file."
            );

            return File(
                content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Farmers.xlsx");
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
            [Bind("FarmerId,Name,Phone,Address,Email")] Farmer farmer,
            IFormFile? photo)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            // Photo validation
            if (photo != null && photo.Length > 0)
            {
                var allowedExtensions = new[]
                {
                    ".jpg",
                    ".jpeg",
                    ".png"
                };

                var extension =
                    Path.GetExtension(photo.FileName)
                        .ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(
                        "PhotoPath",
                        "Only JPG, JPEG and PNG images are allowed.");
                }

                // Maximum 5 MB
                if (photo.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError(
                        "PhotoPath",
                        "Photo size must be less than 5 MB.");
                }
            }

            if (ModelState.IsValid)
            {
                // Save farmer first
                await _farmerService.CreateFarmerAsync(farmer);

                // Save photo
                if (photo != null && photo.Length > 0)
                {
                    farmer.PhotoPath =
                        await SaveFarmerPhotoAsync(
                            photo,
                            farmer.FarmerId);

                    // Update farmer with photo path
                    await _farmerService
                        .UpdateFarmerAsync(farmer);
                }

                await _auditLogService.LogAsync(
                    GetUsername(),
                    "Create Farmer",
                    $"Created farmer: {farmer.Name}"
                );

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
            [Bind("FarmerId,Name,Phone,Address,Email,PhotoPath")]
            Farmer farmer,
            IFormFile? photo)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (id != farmer.FarmerId)
            {
                return NotFound();
            }

            // Photo validation
            if (photo != null && photo.Length > 0)
            {
                var allowedExtensions = new[]
                {
                    ".jpg",
                    ".jpeg",
                    ".png"
                };

                var extension =
                    Path.GetExtension(photo.FileName)
                        .ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(
                        "PhotoPath",
                        "Only JPG, JPEG and PNG images are allowed.");
                }

                if (photo.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError(
                        "PhotoPath",
                        "Photo size must be less than 5 MB.");
                }
            }

            if (ModelState.IsValid)
            {
                // Get existing farmer
                var existingFarmer = await _farmerService
                    .GetFarmerByIdAsync(id);

                if (existingFarmer == null)
                {
                    return NotFound();
                }

                // Keep old photo if no new photo is selected
                if (photo == null || photo.Length == 0)
                {
                    farmer.PhotoPath =
                        existingFarmer.PhotoPath;
                }
                else
                {
                    // Delete old photo
                    DeleteFarmerPhoto(
                        existingFarmer.PhotoPath);

                    // Save new photo
                    farmer.PhotoPath =
                        await SaveFarmerPhotoAsync(
                            photo,
                            farmer.FarmerId);
                }

                var updated = await _farmerService
                    .UpdateFarmerAsync(farmer);

                if (!updated)
                {
                    return NotFound();
                }

                await _auditLogService.LogAsync(
                    GetUsername(),
                    "Update Farmer",
                    $"Updated farmer ID: {farmer.FarmerId}"
                );

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

            // Get farmer before deleting
            var farmer = await _farmerService
                .GetFarmerByIdAsync(id);

            if (farmer == null)
            {
                return NotFound();
            }

            var deleted = await _farmerService
                .DeleteFarmerAsync(id);

            if (!deleted)
            {
                return NotFound();
            }

            // Delete photo from wwwroot
            DeleteFarmerPhoto(farmer.PhotoPath);

            await _auditLogService.LogAsync(
                GetUsername(),
                "Delete Farmer",
                $"Deleted farmer ID: {id}"
            );

            TempData["SuccessMessage"] =
                "Farmer deleted successfully!";

            return RedirectToAction(nameof(Index));
        }


        // =========================
        // Save Farmer Photo
        // =========================
        private async Task<string> SaveFarmerPhotoAsync(
            IFormFile photo,
            int farmerId)
        {
            string uploadsFolder =
                Path.Combine(
                    _environment.WebRootPath,
                    "uploads",
                    "farmers");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string extension =
                Path.GetExtension(photo.FileName)
                    .ToLowerInvariant();

            string fileName =
                "farmer_" +
                farmerId +
                "_" +
                Guid.NewGuid().ToString("N") +
                extension;

            string filePath =
                Path.Combine(
                    uploadsFolder,
                    fileName);

            using (var stream =
                   new FileStream(
                       filePath,
                       FileMode.Create))
            {
                await photo.CopyToAsync(stream);
            }

            return "/uploads/farmers/" + fileName;
        }


        // =========================
        // Delete Farmer Photo
        // =========================
        private void DeleteFarmerPhoto(string? photoPath)
        {
            if (string.IsNullOrWhiteSpace(photoPath))
            {
                return;
            }

            string fileName =
                Path.GetFileName(photoPath);

            string filePath =
                Path.Combine(
                    _environment.WebRootPath,
                    "uploads",
                    "farmers",
                    fileName);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }
    }
}

