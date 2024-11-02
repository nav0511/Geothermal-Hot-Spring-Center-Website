using System.ComponentModel.DataAnnotations;

namespace GHCW_BE.DTOs
{
    public class LoginDTO
    {
        [Required(ErrorMessage = "Yêu cầu nhập email.")]
        [EmailAddress(ErrorMessage = "Không đúng định dạng email.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Yêu cầu nhập mật khẩu.")]
        [MinLength(8, ErrorMessage = "Mật khẩu phải dài ít nhất 8 kí tự.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$#^!%*?&])[A-Za-z\d@$#^!%*?&]{8,}$", ErrorMessage = "Mật khẩu phải chứa ít nhất 1 kí tự thường, 1 kí tự hoa, 1 kí tự đặc biệt (@$#^!%*?&) và 1 kí tự số.")]
        public string Password { get; set; } = null!;
    }

    public class LoginResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string Message { get; set; }
    }

    public class RefreshRequest
    {
        [Required]
        public string RefreshToken { get; set; }
    }
}
