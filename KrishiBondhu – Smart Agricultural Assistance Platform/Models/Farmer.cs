using System.ComponentModel.DataAnnotations;

public class Farmer
{
    public int FarmerId { get; set; }

    [Required(ErrorMessage = "Farmer name is required.")]
    [StringLength(100, MinimumLength = 2,
        ErrorMessage = "Name must be between 2 and 100 characters.")]
    public string Name { get; set; } = string.Empty;


    [Required(ErrorMessage = "Phone number is required.")]
    [Phone(ErrorMessage = "Please enter a valid phone number.")]
    public string Phone { get; set; } = string.Empty;


    [Required(ErrorMessage = "Address is required.")]
    [StringLength(200,
        ErrorMessage = "Address cannot exceed 200 characters.")]
    public string Address { get; set; } = string.Empty;


    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public string Email { get; set; } = string.Empty;
}