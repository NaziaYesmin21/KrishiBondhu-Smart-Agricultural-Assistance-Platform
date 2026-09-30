using Microsoft.AspNetCore.Mvc;

public class ReportsController : Controller
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    public async Task<IActionResult> FarmerReport(int id)
    {
        var pdf = await _reportService.GenerateFarmerReportAsync(id);

        if (pdf.Length == 0)
        {
            return NotFound("Farmer not found.");
        }

        return File(
            pdf,
            "application/pdf",
            $"FarmerReport_{id}.pdf");
    }
}