using System.ComponentModel.DataAnnotations;

namespace GHCW_BE.DTOs
{

    public class ChangePassRequest
    {
        [Required(ErrorMessage = "Yêu cầu phải có Id.")]
        public int Id { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập mật khẩu.")]
        [MinLength(8, ErrorMessage = "Mật khẩu phải dài ít nhất 8 kí tự.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$#^!%*?&])[A-Za-z\d@$#^!%*?&]{8,}$",
        ErrorMessage = "Mật khẩu phải chứa ít nhất 1 kí tự thường, 1 kí tự hoa, 1 kí tự đặc biệt (@$#^!%*?&) và 1 kí tự số.")]
        public string NewPassword { get; set; } = null!;
    }

    public class AddRequest
    {
        [Required(ErrorMessage = "Yêu cầu nhập email.")]
        [EmailAddress(ErrorMessage = "Không đúng định dạng email.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Yêu cầu nhập mật khẩu.")]
        [MinLength(8, ErrorMessage = "Mật khẩu phải dài ít nhất 8 kí tự.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$#^!%*?&])[A-Za-z\d@$#^!%*?&]{8,}$",
        ErrorMessage = "Mật khẩu phải chứa ít nhất 1 kí tự thường, 1 kí tự hoa, 1 kí tự đặc biệt (@$#^!%*?&) và 1 kí tự số.")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Yêu cầu nhập họ tên.")]
        [RegularExpression(@"^[a-zA-ZÀÁÂÃÈÉÊÌÍÒÓÔÕÙÚĂĐĨŨƠàáâãèéêìíòóôõùúăđĩũơưƯẠ-ỹ\s]+$", ErrorMessage = "Họ tên chỉ được chứa chữ cái và khoảng trắng.")]
        public string Name { get; set; }

        public string? Address { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập số điện thoại.")]
        [RegularExpression(@"^(0[3|5|7|8|9])\d{8}$", ErrorMessage = "Số điện thoại chưa đúng định dạng.")]
        public string PhoneNumber { get; set; }

        public DateTime? DoB { get; set; }

        public bool? Gender { get; set; }

        public int Role { get; set; }

        public bool IsActive { get; set; }

        public bool IsEmailNotify { get; set; }
    }

    public class UpdateRequest
    {
        [Required(ErrorMessage = "Yêu cầu phải có Id.")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập họ tên.")]
        [RegularExpression(@"^[a-zA-ZÀÁÂÃÈÉÊÌÍÒÓÔÕÙÚĂĐĨŨƠàáâãèéêìíòóôõùúăđĩũơưƯẠ-ỹ\s]+$", ErrorMessage = "Họ tên chỉ được chứa chữ cái và khoảng trắng.")]
        public string Name { get; set; }

        public string Address { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập số điện thoại.")]
        [RegularExpression(@"^(0[3|5|7|8|9])\d{8}$", ErrorMessage = "Số điện thoại chưa đúng định dạng.")]
        public string PhoneNumber { get; set; }

        public DateTime? DoB { get; set; }

        public bool? Gender { get; set; }

        public bool IsEmailNotify { get; set; }
    }

    public class EditRequest
    {
        [Required(ErrorMessage = "Yêu cầu phải có Id.")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập họ tên.")]
        [RegularExpression(@"^[a-zA-ZÀÁÂÃÈÉÊÌÍÒÓÔÕÙÚĂĐĨŨƠàáâãèéêìíòóôõùúăđĩũơưƯẠ-ỹ\s]+$", ErrorMessage = "Họ tên chỉ được chứa chữ cái và khoảng trắng.")]
        public string Name { get; set; }

        public string Address { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập số điện thoại.")]
        [RegularExpression(@"^(0[3|5|7|8|9])\d{8}$", ErrorMessage = "Số điện thoại chưa đúng định dạng.")]
        public string PhoneNumber { get; set; }

        public DateTime? DoB { get; set; }

        public bool? Gender { get; set; }

        public int Role { get; set; }

        public bool IsActive { get; set; }

        public bool IsEmailNotify { get; set; }
    }
}
