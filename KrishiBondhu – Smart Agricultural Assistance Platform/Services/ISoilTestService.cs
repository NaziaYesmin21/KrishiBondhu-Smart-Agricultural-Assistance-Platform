using System.Collections.Generic;
using System.Threading.Tasks;

public interface ISoilTestService
{
    Task<List<SoilTest>> GetAllSoilTestsAsync();

    Task<SoilTest?> GetSoilTestByIdAsync(int id);

    Task CreateSoilTestAsync(SoilTest soilTest);

    Task<bool> UpdateSoilTestAsync(SoilTest soilTest);

    Task<bool> DeleteSoilTestAsync(int id);

    Task<bool> SoilTestExistsAsync(int id);
}