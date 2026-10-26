
using System.ComponentModel.DataAnnotations;

namespace KrishiBondhu___Smart_Agricultural_Assistance_Platform.Models
{
    public class RegisterViewModel
    {
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        // Account type
        [Required]
        public string AccountType { get; set; } = "User";

        // Agro Firm information
        [StringLength(150)]
        public string? FirmName { get; set; }

        [StringLength(100)]
        public string? OwnerName { get; set; }

        [Phone]
        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(250)]
        public string? Address { get; set; }

        [StringLength(50)]
        public string? BusinessType { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }
    }
}
