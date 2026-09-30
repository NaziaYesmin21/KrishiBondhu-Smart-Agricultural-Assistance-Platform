using System.Collections.Generic;

public class FarmerProfileViewModel
{
    public Farmer Farmer { get; set; } = null!;

    public List<Farm> Farms { get; set; } =
        new List<Farm>();

    public List<Cultivation> Cultivations { get; set; } =
        new List<Cultivation>();

    public List<SoilTest> SoilTests { get; set; } =
        new List<SoilTest>();
}