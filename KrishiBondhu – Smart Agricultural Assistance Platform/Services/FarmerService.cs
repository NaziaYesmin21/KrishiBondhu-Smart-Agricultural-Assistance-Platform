using Microsoft.EntityFrameworkCore;

public class FarmerService : IFarmerService
{
    private readonly ApplicationDbContext _context;

    public FarmerService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Farmer>> GetAllFarmersAsync()
    {
        return await _context.Farmers.ToListAsync();
    }

    public async Task<List<Farmer>> SearchFarmersAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return await _context.Farmers.ToListAsync();
        }

        return await _context.Farmers
            .Where(f =>
                f.Name.Contains(searchTerm) ||
                f.Phone.Contains(searchTerm))
            .ToListAsync();
    }

    public async Task<List<Farmer>> FilterFarmersAsync(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            return await _context.Farmers.ToListAsync();
        }

        return await _context.Farmers
            .Where(f => f.Address.Contains(address))
            .ToListAsync();
    }

    public async Task<Farmer?> GetFarmerByIdAsync(int id)
    {
        return await _context.Farmers
            .FirstOrDefaultAsync(f => f.FarmerId == id);
    }

    public async Task CreateFarmerAsync(Farmer farmer)
    {
        _context.Farmers.Add(farmer);
        await _context.SaveChangesAsync();
    }

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

    public async Task<bool> DeleteFarmerAsync(int id)
    {
        var farmer = await _context.Farmers.FindAsync(id);

        if (farmer == null)
        {
            return false;
        }

        _context.Farmers.Remove(farmer);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> FarmerExistsAsync(int id)
    {
        return await _context.Farmers
            .AnyAsync(f => f.FarmerId == id);
    }
}