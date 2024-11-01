using System.ComponentModel.DataAnnotations;

namespace GHCW_FE.DTOs
{
    public class LoginDTO
    {
        [Required(ErrorMessage = "Phải nhập Email")]
        [EmailAddress(ErrorMessage = "Không đúng định dạng email.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Phải nhập Mật khẩu")]
        [MinLength(8, ErrorMessage = "Mật khẩu phải có ít nhất 8 kí tự.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
        ErrorMessage = "Mật khẩu phải chứa ít nhất 1 kí tự thường, 1 kí tự hoa, 1 kí tự số và 1 kí tự đặc biệt.")]
        public string Password { get; set; } = null!;
        public string? Type { get; set; }
    }

    public class LoginResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
