using System.ComponentModel.DataAnnotations;

public class Farm
{
    public int FarmId { get; set; }

    [Required]
    public string FarmName { get; set; } = string.Empty;

    [Required]
    public string Location { get; set; } = string.Empty;

    public double Area { get; set; }

    public int FarmerId { get; set; }

    public Farmer Farmer { get; set; } = null!;
}