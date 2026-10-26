
using System.ComponentModel.DataAnnotations;

namespace KrishiBondhu___Smart_Agricultural_Assistance_Platform.Models
{
    public class AgroProductService
    {
        [Key]
        public int AgroProductServiceId { get; set; }

        [Required]
        public int AgroFirmId { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        public string ItemType { get; set; } = "Product";

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Range(0, 99999999)]
        public decimal Price { get; set; }

        [StringLength(30)]
        public string? Unit { get; set; }

        public bool IsAvailable { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
