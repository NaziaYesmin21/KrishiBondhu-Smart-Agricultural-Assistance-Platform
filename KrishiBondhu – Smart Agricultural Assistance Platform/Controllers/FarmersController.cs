using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class FarmersController : Controller
{
    private readonly ApplicationDbContext _context;

    public FarmersController(ApplicationDbContext context)
    {
        _context = context;
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
        return View(await _context.Farmers.ToListAsync());
    }

    // GET: Farmers/Details/5
    // Admin and User can view details
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var farmer = await _context.Farmers
            .FirstOrDefaultAsync(m => m.FarmerId == id);

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
            _context.Add(farmer);
            await _context.SaveChangesAsync();

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

        var farmer = await _context.Farmers.FindAsync(id);

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
            try
            {
                _context.Update(farmer);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FarmerExists(farmer.FarmerId))
                {
                    return NotFound();
                }

                throw;
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

        var farmer = await _context.Farmers
            .FirstOrDefaultAsync(m => m.FarmerId == id);

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

        var farmer = await _context.Farmers.FindAsync(id);

        if (farmer != null)
        {
            _context.Farmers.Remove(farmer);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private bool FarmerExists(int id)
    {
        return _context.Farmers.Any(e => e.FarmerId == id);
    }
}