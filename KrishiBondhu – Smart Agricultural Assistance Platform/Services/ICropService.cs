using System.Collections.Generic;
using System.Threading.Tasks;

public interface ICropService
{
    Task<List<Crop>> GetAllCropsAsync();

    Task<Crop?> GetCropByIdAsync(int id);

    Task CreateCropAsync(Crop crop);

    Task<bool> UpdateCropAsync(Crop crop);

    Task<bool> DeleteCropAsync(int id);

    Task<bool> CropExistsAsync(int id);
}