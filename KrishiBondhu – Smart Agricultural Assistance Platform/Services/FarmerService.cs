using Microsoft.EntityFrameworkCore;

public class FarmerService : IFarmerService
{
    private readonly ApplicationDbContext _context;

    public FarmerService(ApplicationDbContext context)
    {
        _context = context;
    }


    // =========================
    // Get All Farmers
    // =========================
    public async Task<List<Farmer>> GetAllFarmersAsync()
    {
        return await _context.Farmers
            .ToListAsync();
    }


    // =========================
    // Search Farmers
    // =========================
    public async Task<List<Farmer>> SearchFarmersAsync(
        string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return await _context.Farmers
                .ToListAsync();
        }

        return await _context.Farmers
            .Where(f =>
                f.Name.Contains(searchTerm) ||
                f.Phone.Contains(searchTerm))
            .ToListAsync();
    }


    // =========================
    // Filter Farmers by Address
    // =========================
    public async Task<List<Farmer>> FilterFarmersAsync(
        string address)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            return await _context.Farmers
                .ToListAsync();
        }

        return await _context.Farmers
            .Where(f => f.Address.Contains(address))
            .ToListAsync();
    }


    // =========================
    // Get Farmer by ID
    // =========================
    public async Task<Farmer?> GetFarmerByIdAsync(int id)
    {
        return await _context.Farmers
            .FirstOrDefaultAsync(f => f.FarmerId == id);
    }


    // =========================
    // Get Complete Farmer Profile
    // =========================
    public async Task<FarmerProfileViewModel?> GetFarmerProfileAsync(
        int id)
    {
        var farmer = await _context.Farmers
            .FirstOrDefaultAsync(f => f.FarmerId == id);

        if (farmer == null)
        {
            return null;
        }

        var farms = await _context.Farms
            .Where(f => f.FarmerId == id)
            .ToListAsync();

        var farmIds = farms
            .Select(f => f.FarmId)
            .ToList();

        var cultivations = await _context.Cultivations
            .Where(c => farmIds.Contains(c.FarmId))
            .Include(c => c.Crop)
            .Include(c => c.Farm)
            .ToListAsync();

        var soilTests = await _context.SoilTests
            .Where(s => farmIds.Contains(s.FarmId))
            .Include(s => s.Farm)
            .ToListAsync();

        return new FarmerProfileViewModel
        {
            Farmer = farmer,
            Farms = farms,
            Cultivations = cultivations,
            SoilTests = soilTests
        };
    }


    // =========================
    // Create Farmer
    // =========================
    public async Task CreateFarmerAsync(Farmer farmer)
    {
        _context.Farmers.Add(farmer);

        await _context.SaveChangesAsync();
    }


    // =========================
    // Update Farmer
    // =========================
    public async Task<bool> UpdateFarmerAsync(Farmer farmer)
    {
        var existingFarmer = await _context.Farmers
            .FindAsync(farmer.FarmerId);

        if (existingFarmer == null)
        {
            return false;
        }

        existingFarmer.Name = farmer.Name;
        existingFarmer.Phone = farmer.Phone;
        existingFarmer.Address = farmer.Address;
        existingFarmer.Email = farmer.Email;

        await _context.SaveChangesAsync();

        return true;
    }


    // =========================
    // Delete Farmer
    // =========================
    public async Task<bool> DeleteFarmerAsync(int id)
    {
        var farmer = await _context.Farmers
            .FindAsync(id);

        if (farmer == null)
        {
            return false;
        }

        _context.Farmers.Remove(farmer);

        await _context.SaveChangesAsync();

        return true;
    }


    // =========================
    // Check Farmer Exists
    // =========================
    public async Task<bool> FarmerExistsAsync(int id)
    {
        return await _context.Farmers
            .AnyAsync(f => f.FarmerId == id);
    }
}