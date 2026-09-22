public class SoilTest
{
    public int SoilTestId { get; set; }

    public int FarmId { get; set; }

    public DateTime TestDate { get; set; }

    public double PH { get; set; }

    public double Nitrogen { get; set; }

    public double Phosphorus { get; set; }

    public double Potassium { get; set; }

    public Farm Farm { get; set; } = null!;
}