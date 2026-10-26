
using Microsoft.AspNetCore.Mvc;
using KrishiBondhu___Smart_Agricultural_Assistance_Platform.Services;

namespace KrishiBondhu___Smart_Agricultural_Assistance_Platform.Controllers
{
    public class AIAssistantController : Controller
    {
        private readonly IAIAssistantService _aiAssistantService;
        private readonly IAuditLogService _auditLogService;
        private readonly CropDiseaseDetectionService _cropDiseaseDetectionService;

        public AIAssistantController(
            IAIAssistantService aiAssistantService,
            IAuditLogService auditLogService,
            CropDiseaseDetectionService cropDiseaseDetectionService)
        {
            _aiAssistantService = aiAssistantService;
            _auditLogService = auditLogService;
            _cropDiseaseDetectionService = cropDiseaseDetectionService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ask(
            string? question,
            string? cropName,
            IFormFile? image)
        {
            question = question?.Trim();

            bool hasQuestion = !string.IsNullOrWhiteSpace(question);
            bool hasImage = image != null && image.Length > 0;

            if (!hasQuestion && !hasImage)
            {
                ViewBag.Question = question;
                ViewBag.Answer =
                    "প্রশ্ন লিখুন অথবা গাছের পাতার একটি ছবি upload করুন।";

                return View("Index");
            }

            var username =
                HttpContext.Session.GetString("Username")
                ?? "Unknown";

            try
            {
                if (hasImage)
                {
                    if (image!.Length > 10 * 1024 * 1024)
                    {
                        ViewBag.Question = question;
                        ViewBag.Answer =
                            "ছবির size 10 MB-এর কম হতে হবে।";

                        return View("Index");
                    }

                    string contentType = image.ContentType.ToLowerInvariant();

                    if (contentType != "image/jpeg" &&
                        contentType != "image/png" &&
                        contentType != "image/webp")
                    {
                        ViewBag.Question = question;
                        ViewBag.Answer =
                            "JPG, PNG অথবা WebP format-এর ছবি upload করুন।";

                        return View("Index");
                    }

                    byte[] imageBytes;

                    using (var memoryStream = new MemoryStream())
                    {
                        await image.CopyToAsync(memoryStream);
                        imageBytes = memoryStream.ToArray();
                    }

                    await _auditLogService.LogAsync(
                        username,
                        "AI Crop Image Analysis",
                        $"Crop: {cropName ?? "Not specified"}; " +
                        $"Question: {question ?? "No question"}");

                    var answer =
                        await _cropDiseaseDetectionService.AnalyzeCropImageAsync(
                            imageBytes,
                            contentType,
                            cropName,
                            question);

                    ViewBag.Question = question;
                    ViewBag.Answer = answer;
                    ViewBag.SelectedCrop = cropName;
                    ViewBag.ImageName = Path.GetFileName(image.FileName);

                    return View("Index");
                }

                await _auditLogService.LogAsync(
                    username,
                    "AI Question",
                    question!);

                var textAnswer =
                    await _aiAssistantService.AskAsync(question!);

                ViewBag.Question = question;
                ViewBag.Answer = textAnswer;

                return View("Index");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "AI Assistant Controller Error: " + ex);

                ViewBag.Question = question;
                ViewBag.SelectedCrop = cropName;
                ViewBag.Answer =
                    "AI Assistant সাময়িকভাবে কাজ করছে না। " +
                    "কিছুক্ষণ পর আবার চেষ্টা করুন।";

                return View("Index");
            }
        }
    }
}
