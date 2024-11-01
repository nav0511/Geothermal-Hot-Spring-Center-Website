using System.ComponentModel.DataAnnotations;

namespace GHCW_FE.DTOs
{
    public class RegisterDTO
    {
        [Required(ErrorMessage = "Yêu cầu nhập email.")]
        [EmailAddress(ErrorMessage = "Không đúng định dạng email.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập Mật khẩu.")]
        [MinLength(8, ErrorMessage = "Mật khẩu phải có ít nhất 8 kí tự.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
        ErrorMessage = "Mật khẩu phải chứa ít nhất 1 kí tự thường, 1 kí tự hoa, 1 kí tự số và 1 kí tự đặc biệt.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập Họ tên.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập Số điện thoại.")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Số điện thoại phải có đúng 10 chữ số.")]
        public string PhoneNumber { get; set; }
    }
    public class SendEmailDTO
    {
        public string FromEmail { get; set; }
        public string Password { get; set; }
        public string ToEmail { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
    public class ActivationCode
    {
        public string Email { get; set; }
        public string Code { get; set; } = string.Empty;
    }
    public class ResetPassword
    {
        public string Email { get; set; }
    }
}
