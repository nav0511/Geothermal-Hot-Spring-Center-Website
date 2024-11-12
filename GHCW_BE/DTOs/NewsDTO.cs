using GHCW_BE.Models;
using System.ComponentModel.DataAnnotations;

namespace GHCW_BE.DTOs
{
    public class NewsDTO
    {
        [Required(ErrorMessage = "Yêu cầu phải có Id.")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Yêu cầu phải có mã giảm giá.")]
        public string? DiscountId { get; set; }

        [Required(ErrorMessage = "Yêu cầu phải có tiêu đề.")]
        public string? Title { get; set; }

        [Required(ErrorMessage = "Yêu cầu phải có ngày tải lên.")]
        public DateTime? UploadDate { get; set; }

        [Required(ErrorMessage = "Yêu cầu phải có mô tả.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Yêu cầu phải có hình ảnh.")]
        public string? Image { get; set; }

        [Required(ErrorMessage = "Yêu cầu phải có trạng thái kích hoạt.")]
        public bool? IsActive { get; set; }

        public virtual DiscountDTO? Discount { get; set; }
    }

    public class NewsDTO2
    {
        [Required(ErrorMessage = "Yêu cầu phải có mã giảm giá.")]
        public string? DiscountId { get; set; }

        [Required(ErrorMessage = "Yêu cầu phải có tiêu đề.")]
        public string? Title { get; set; }

        [Required(ErrorMessage = "Yêu cầu phải có ngày tải lên.")]
        public DateTime? UploadDate { get; set; }

        [Required(ErrorMessage = "Yêu cầu phải có mô tả.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Yêu cầu phải có hình ảnh.")]
        public string? Image { get; set; }

        [Required(ErrorMessage = "Yêu cầu phải có trạng thái kích hoạt.")]
        public bool? IsActive { get; set; }
        public virtual DiscountDTO? Discount { get; set; }
    }

    public class NewsDTOForAdd
    {
        public string? DiscountId { get; set; }
        public string? Title { get; set; }
        public DateTime? UploadDate { get; set; }
        public string? Description { get; set; }
        public IFormFile? Image { get; set; }
        public bool? IsActive { get; set; }
    }

}
