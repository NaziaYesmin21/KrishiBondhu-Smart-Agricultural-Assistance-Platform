
using KrishiBondhu___Smart_Agricultural_Assistance_Platform.Services;
using Microsoft.AspNetCore.Mvc;

namespace KrishiBondhu___Smart_Agricultural_Assistance_Platform.Controllers
{
    public class CropDiseaseDetectionController : Controller
    {
        private readonly CropDiseaseDetectionService _detectionService;
        private readonly IAuditLogService _auditLogService;

        public CropDiseaseDetectionController(
            CropDiseaseDetectionService detectionService,
            IAuditLogService auditLogService)
        {
            _detectionService = detectionService;
            _auditLogService = auditLogService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Analyze(
            IFormFile? image,
            string? cropName)
        {
            if (image == null || image.Length == 0)
            {
                ViewBag.Error = "অনুগ্রহ করে গাছের পাতার একটি ছবি নির্বাচন করুন।";
                ViewBag.CropName = cropName;
                return View("Index");
            }

            if (image.Length > 10 * 1024 * 1024)
            {
                ViewBag.Error = "ছবির size 10 MB-এর কম হতে হবে।";
                ViewBag.CropName = cropName;
                return View("Index");
            }

            var allowedTypes = new[]
            {
                "image/jpeg",
                "image/png",
                "image/webp"
            };

            if (!allowedTypes.Contains(image.ContentType))
            {
                ViewBag.Error = "শুধু JPG, PNG অথবা WebP ছবি upload করুন।";
                ViewBag.CropName = cropName;
                return View("Index");
            }

            try
            {
                using var memoryStream = new MemoryStream();
                await image.CopyToAsync(memoryStream);

                string answer =
                    await _detectionService.AnalyzeCropImageAsync(
                        memoryStream.ToArray(),
                        image.ContentType,
                        cropName);

                ViewBag.CropName = cropName;
                ViewBag.Answer = answer;

                var username =
                    HttpContext.Session.GetString("Username")
                    ?? "Unknown";

                await _auditLogService.LogAsync(
                    username,
                    "Crop Disease Image Analysis",
                    $"Crop: {cropName ?? "Not specified"}; " +
                    $"Image size: {image.Length} bytes");

                return View("Index");
            }
            catch (Exception)
            {
                ViewBag.Error =
                    "ছবিটি বিশ্লেষণ করা যায়নি। আবার চেষ্টা করুন।";

                ViewBag.CropName = cropName;
                return View("Index");
            }
        }
    }
}
