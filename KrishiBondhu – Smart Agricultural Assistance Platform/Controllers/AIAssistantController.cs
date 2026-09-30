using Microsoft.AspNetCore.Mvc;
using KrishiBondhu___Smart_Agricultural_Assistance_Platform.Services;

namespace KrishiBondhu___Smart_Agricultural_Assistance_Platform.Controllers
{
    public class AIAssistantController : Controller
    {
        private readonly IAIAssistantService _aiAssistantService;
        private readonly IAuditLogService _auditLogService;

        public AIAssistantController(
            IAIAssistantService aiAssistantService,
            IAuditLogService auditLogService)
        {
            _aiAssistantService = aiAssistantService;
            _auditLogService = auditLogService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Ask(string question)
        {
            if (string.IsNullOrWhiteSpace(question))
            {
                ViewBag.Answer = "Please enter an agricultural question.";
                return View("Index");
            }

            var username =
                HttpContext.Session.GetString("Username")
                ?? "Unknown";

            try
            {
                // Save the user's AI question first
                await _auditLogService.LogAsync(
                    username,
                    "AI Question",
                    question);

                // Get AI answer
                var answer =
                    await _aiAssistantService.AskAsync(question);

                ViewBag.Question = question;
                ViewBag.Answer = answer;

                return View("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Question = question;

                ViewBag.Answer =
                    "AI ERROR: " + ex.Message;

                return View("Index");
            }
        }
    }
}