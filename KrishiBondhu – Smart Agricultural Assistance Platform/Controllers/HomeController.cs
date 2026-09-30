using KrishiBondhu___Smart_Agricultural_Assistance_Platform.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace KrishiBondhu___Smart_Agricultural_Assistance_Platform.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDashboardService _dashboardService;
        private readonly HarvestReminderService _harvestReminderService;

        public HomeController(
            IDashboardService dashboardService,
            HarvestReminderService harvestReminderService)
        {
            _dashboardService = dashboardService;
            _harvestReminderService = harvestReminderService;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Username =
                HttpContext.Session.GetString("Username") ?? "Guest";

            ViewBag.Role =
                HttpContext.Session.GetString("Role") ?? "User";

            ViewBag.FarmerCount =
                await _dashboardService.GetFarmerCountAsync();

            ViewBag.FarmCount =
                await _dashboardService.GetFarmCountAsync();

            ViewBag.CropCount =
                await _dashboardService.GetCropCountAsync();

            ViewBag.CultivationCount =
                await _dashboardService.GetCultivationCountAsync();

            ViewBag.SoilTestCount =
                await _dashboardService.GetSoilTestCountAsync();

            ViewBag.DiseaseCount =
                await _dashboardService.GetDiseaseCountAsync();

            ViewBag.CropStatistics =
                await _dashboardService.GetCropStatisticsAsync();

            ViewBag.DiseaseStatistics =
                await _dashboardService.GetDiseaseStatisticsAsync();

            // Harvest Reminder
            ViewBag.UpcomingHarvests =
                await _harvestReminderService.GetUpcomingHarvestsAsync();

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id
                    ?? HttpContext.TraceIdentifier
            });
        }
    }
}