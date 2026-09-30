using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class FarmersController : Controller
{
    private readonly IFarmerService _farmerService;

    public FarmersController(IFarmerService farmerService)
    {
        _farmerService = farmerService;
    }

    // Check whether the logged-in user is an Admin
    private bool IsAdmin()
    {
        return HttpContext.Session.GetString("Role") == "Admin";
    }

    // GET: Farmers
    // Admin and User can view farmer list
    public async Task<IActionResult> Index()
    {
        var farmers = await _farmerService.GetAllFarmersAsync();

        return View(farmers);
    }

    // GET: Farmers/Details/5
    // Admin and User can view details
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var farmer = await _farmerService.GetFarmerByIdAsync(id.Value);

        if (farmer == null)
        {
            return NotFound();
        }

        return View(farmer);
    }

    // GET: Farmers/Create
    // Only Admin can create
    public IActionResult Create()
    {
        if (!IsAdmin())
        {
            return RedirectToAction("Login", "Auth");
        }

        return View();
    }

    // POST: Farmers/Create
    // Only Admin can create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("FarmerId,Name,Phone,Address,Email")] Farmer farmer)
    {
        if (!IsAdmin())
        {
            return RedirectToAction("Login", "Auth");
        }

        if (ModelState.IsValid)
        {
            await _farmerService.CreateFarmerAsync(farmer);

            return RedirectToAction(nameof(Index));
        }

        return View(farmer);
    }

    // GET: Farmers/Edit/5
    // Only Admin can edit
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

        var farmer = await _farmerService.GetFarmerByIdAsync(id.Value);

        if (farmer == null)
        {
            return NotFound();
        }

        return View(farmer);
    }

    // POST: Farmers/Edit/5
    // Only Admin can edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("FarmerId,Name,Phone,Address,Email")] Farmer farmer)
    {
        if (!IsAdmin())
        {
            return RedirectToAction("Login", "Auth");
        }

        if (id != farmer.FarmerId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            var updated = await _farmerService.UpdateFarmerAsync(farmer);

            if (!updated)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }

        return View(farmer);
    }

    // GET: Farmers/Delete/5
    // Only Admin can delete
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

        var farmer = await _farmerService.GetFarmerByIdAsync(id.Value);

        if (farmer == null)
        {
            return NotFound();
        }

        return View(farmer);
    }

    // POST: Farmers/Delete/5
    // Only Admin can delete
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!IsAdmin())
        {
            return RedirectToAction("Login", "Auth");
        }

        var deleted = await _farmerService.DeleteFarmerAsync(id);

        if (!deleted)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> FarmerExists(int id)
    {
        return await _farmerService.FarmerExistsAsync(id);
    }
}