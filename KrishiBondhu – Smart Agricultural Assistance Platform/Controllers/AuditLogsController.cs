using Microsoft.AspNetCore.Mvc;

namespace KrishiBondhu___Smart_Agricultural_Assistance_Platform.Controllers
{
    public class AuditLogsController : Controller
    {
        private readonly IAuditLogService _auditLogService;

        public AuditLogsController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("Role") == "Admin";
        }

        public async Task<IActionResult> Index()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            var logs = await _auditLogService.GetAllLogsAsync();

            return View(logs);
        }
    }
}