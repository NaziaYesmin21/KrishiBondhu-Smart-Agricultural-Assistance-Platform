using System.Collections.Generic;
using System.Threading.Tasks;

public interface IFarmService
{
    Task<List<Farm>> GetAllFarmsAsync();

    Task<Farm?> GetFarmByIdAsync(int id);

    Task CreateFarmAsync(Farm farm);

    Task<bool> UpdateFarmAsync(Farm farm);

    Task<bool> DeleteFarmAsync(int id);

    Task<bool> FarmExistsAsync(int id);
}