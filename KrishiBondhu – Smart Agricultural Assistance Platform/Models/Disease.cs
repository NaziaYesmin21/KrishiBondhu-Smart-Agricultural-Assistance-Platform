public class Disease
{
    public int DiseaseId { get; set; }

    public int CropId { get; set; }

    public string DiseaseName { get; set; } = string.Empty;

    public string Symptoms { get; set; } = string.Empty;

    public string Treatment { get; set; } = string.Empty;

    public Crop Crop { get; set; } = null!;
}