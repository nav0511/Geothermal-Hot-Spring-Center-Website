using System.ComponentModel.DataAnnotations;

namespace GHCW_BE.DTOs
{
    public class CustomerDTO
    {
        [Required(ErrorMessage = "Yêu cầu phải có Id.")]
        public int Id { get; set; }

        public int? AccountId { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập họ tên.")]
        [RegularExpression(@"^[a-zA-ZÀÁÂÃÈÉÊÌÍÒÓÔÕÙÚĂĐĨŨƠàáâãèéêìíòóôõùúăđĩũơưƯẠ-ỹ\s]+$", ErrorMessage = "Họ tên chỉ được chứa chữ cái và khoảng trắng.")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Yêu cầu nhập email.")]
        [EmailAddress(ErrorMessage = "Không đúng định dạng email.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Yêu cầu nhập số điện thoại.")]
        [RegularExpression(@"^(0[3||5||7||8||9])\d{8}$", ErrorMessage = "Số điện thoại chưa đúng định dạng.")]
        public string PhoneNumber { get; set; } = null!;

        public DateTime? DoB { get; set; }
        public bool? Gender { get; set; }
        public string? Address { get; set; }
        public bool IsEmailNotify { get; set; }
    }

    public class AddCustomerRequest
    {
        public int? AccountId { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập họ tên.")]
        [RegularExpression(@"^[a-zA-ZÀÁÂÃÈÉÊÌÍÒÓÔÕÙÚĂĐĨŨƠàáâãèéêìíòóôõùúăđĩũơưƯẠ-ỹ\s]+$", ErrorMessage = "Họ tên chỉ được chứa chữ cái và khoảng trắng.")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Yêu cầu nhập email.")]
        [EmailAddress(ErrorMessage = "Không đúng định dạng email.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Yêu cầu nhập số điện thoại.")]
        [RegularExpression(@"^(0[3||5||7||8||9])\d{8}$", ErrorMessage = "Số điện thoại chưa đúng định dạng.")]
        public string PhoneNumber { get; set; } = null!;
        public bool IsEmailNotify { get; set; }
    }
}
