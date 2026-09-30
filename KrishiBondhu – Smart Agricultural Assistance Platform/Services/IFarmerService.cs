using System.Collections.Generic;
using System.Threading.Tasks;

public interface IFarmerService
{
    Task<List<Farmer>> GetAllFarmersAsync();

    Task<List<Farmer>> SearchFarmersAsync(string searchTerm);

    Task<Farmer?> GetFarmerByIdAsync(int id);

    Task CreateFarmerAsync(Farmer farmer);

    Task<bool> UpdateFarmerAsync(Farmer farmer);

    Task<bool> DeleteFarmerAsync(int id);

    Task<bool> FarmerExistsAsync(int id);
}