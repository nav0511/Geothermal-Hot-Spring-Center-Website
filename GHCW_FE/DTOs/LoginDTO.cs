using System.ComponentModel.DataAnnotations;

<<<<<<< HEAD
namespace GHCW_BE.DTOs
=======
namespace GHCW_FE.DTOs
>>>>>>> 8b5e9fcb489bcfbf172a08a5baaca5deee132c4b
{
    public class LoginDTO
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
        ErrorMessage = "Password must contain at least one lowercase letter, one uppercase letter, one number, and one special character.")]
        public string Password { get; set; } = null!;
        public string? Type { get; set; }
    }

    public class LoginResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }

    public class RefreshRequest
    {
        [Required]
        public string RefreshToken { get; set; }
    }
}