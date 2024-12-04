using System.ComponentModel.DataAnnotations;

namespace GHCW_BE.DTOs
{
    public class DiscountDTO
    {
        [Required(ErrorMessage = "Yêu cầu phải có mã Code.")]
        public string Code { get; set; } = null!;

        [Required(ErrorMessage = "Yêu cầu phải có tên.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Yêu cầu phải có giá trị.")]
        public int Value { get; set; }

        [Required(ErrorMessage = "Yêu cầu phải có ngày bắt đầu.")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Yêu cầu phải có ngày kết thúc.")]
        public DateTime EndDate { get; set; }

        public string? Description { get; set; }

        [Required(ErrorMessage = "Yêu cầu phải có trạng thái.")]
        public bool IsAvailable { get; set; }
    }

    public class DiscountDTO2
    {
        [Required(ErrorMessage = "Yêu cầu phải có tên.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Yêu cầu phải có giá trị.")]
        public int Value { get; set; }

        [Required(ErrorMessage = "Yêu cầu phải có ngày bắt đầu.")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Yêu cầu phải có ngày kết thúc.")]
        public DateTime EndDate { get; set; }

        public string? Description { get; set; }

        [Required(ErrorMessage = "Yêu cầu phải có trạng thái.")]
        public bool IsAvailable { get; set; }
    }


}
