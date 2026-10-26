
using System.ComponentModel.DataAnnotations;

namespace KrishiBondhu___Smart_Agricultural_Assistance_Platform.Models
{
    public class AgroFirm
    {
        [Key]
        public int AgroFirmId { get; set; }

        [Required]
        [StringLength(150)]
        public string FirmName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string OwnerName { get; set; } = string.Empty;

        [Required]
        [Phone]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(150)]
        public string? Email { get; set; }

        [Required]
        [StringLength(250)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string BusinessType { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;
    }
}