public class Cultivation
{
    public int CultivationId { get; set; }

    public int FarmId { get; set; }

    public int CropId { get; set; }

    public DateTime PlantingDate { get; set; }

    public DateTime? HarvestDate { get; set; }

    public double Yield { get; set; }

    public Farm Farm { get; set; } = null!;

    public Crop Crop { get; set; } = null!;
}