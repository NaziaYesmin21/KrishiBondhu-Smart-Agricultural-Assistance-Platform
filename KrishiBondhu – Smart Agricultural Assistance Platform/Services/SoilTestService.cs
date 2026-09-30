using Microsoft.EntityFrameworkCore;

public class SoilTestService : ISoilTestService
{
    private readonly ApplicationDbContext _context;

    public SoilTestService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SoilTest>> GetAllSoilTestsAsync()
    {
        return await _context.SoilTests
            .Include(s => s.Farm)
            .ToListAsync();
    }

    public async Task<SoilTest?> GetSoilTestByIdAsync(int id)
    {
        return await _context.SoilTests
            .Include(s => s.Farm)
            .FirstOrDefaultAsync(s => s.SoilTestId == id);
    }

    public async Task CreateSoilTestAsync(SoilTest soilTest)
    {
        _context.SoilTests.Add(soilTest);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> UpdateSoilTestAsync(SoilTest soilTest)
    {
        var existingSoilTest = await _context.SoilTests
            .FindAsync(soilTest.SoilTestId);

        if (existingSoilTest == null)
        {
            return false;
        }

        _context.Entry(existingSoilTest)
            .CurrentValues
            .SetValues(soilTest);

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteSoilTestAsync(int id)
    {
        var soilTest = await _context.SoilTests
            .FindAsync(id);

        if (soilTest == null)
        {
            return false;
        }

        _context.SoilTests.Remove(soilTest);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> SoilTestExistsAsync(int id)
    {
        return await _context.SoilTests
            .AnyAsync(s => s.SoilTestId == id);
    }
}