using System.ComponentModel.DataAnnotations;

namespace GHCW_FE.DTOs
{
    public class AccountDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public bool? Gender { get; set; }
        public string? Address { get; set; }
        public string PhoneNumber { get; set; } = null!;
        public DateTime? DoB { get; set; }
        public int Role { get; set; }
        public string? ActivationCode { get; set; }
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public bool IsEmailNotify { get; set; }
        public bool IsActive { get; set; }
        public string? RefreshToken { get; set; }
    }

    public class ForgotPassRequest
    {
        [Required(ErrorMessage = "Yêu cầu nhập email.")]
        [EmailAddress(ErrorMessage = "Không đúng định dạng email.")]
        public string Email { get; set; } = null!;
    }

    public class ChangePassRequest
    {
        [Required(ErrorMessage = "Yêu cầu phải có Id.")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập mật khẩu cũ.")]
        public string OldPassword { get; set; } = null!;

        [Required(ErrorMessage = "Yêu cầu nhập mật khẩu.")]
        [MinLength(8, ErrorMessage = "Mật khẩu phải dài ít nhất 8 kí tự.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$#^!%*?&])[A-Za-z\d@$#^!%*?&]{8,}$",
        ErrorMessage = "Mật khẩu phải chứa ít nhất 1 kí tự thường, 1 kí tự hoa, 1 kí tự đặc biệt (@$#^!%*?&) và 1 kí tự số.")]
        public string NewPassword { get; set; } = null!;

        [Required(ErrorMessage = "Yêu cầu xác nhận mật khẩu mới.")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        public string ConfirmNewPassword { get; set; } = null!;
    }
}
