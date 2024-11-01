using System.ComponentModel.DataAnnotations;

namespace GHCW_BE.DTOs
{
    public class RegisterDTO
    {
        [Required(ErrorMessage = "Yêu cầu nhập email.")]
        [EmailAddress(ErrorMessage = "Không đúng định dạng email.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập mật khẩu.")]
        [MinLength(8, ErrorMessage = "Mật khẩu phải dài ít nhất 8 kí tự.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$#^!%*?&])[A-Za-z\d@$#^!%*?&]{8,}$", ErrorMessage = "Mật khẩu phải chứa ít nhất 1 kí tự thường, 1 kí tự hoa, 1 kí tự đặc biệt (@$#^!%*?&) và 1 kí tự số.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập họ tên.")]
        [RegularExpression(@"^[a-zA-ZÀÁÂÃÈÉÊÌÍÒÓÔÕÙÚĂĐĨŨƠàáâãèéêìíòóôõùúăđĩũơưƯẠ-ỹ\s]+$", ErrorMessage = "Họ tên chỉ được chứa chữ cái và khoảng trắng.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập số điện thoại.")]
        [RegularExpression(@"^(0[3|5|7|8|9])\d{8}$", ErrorMessage = "Số điện thoại chưa đúng định dạng.")]
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
