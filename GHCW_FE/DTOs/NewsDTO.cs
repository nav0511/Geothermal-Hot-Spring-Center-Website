using System.ComponentModel.DataAnnotations;

namespace GHCW_FE.DTOs
{
    public class NewsDTO
    {
        [Required(ErrorMessage = "Yêu cầu phải có Id.")]
        public int Id { get; set; }

        public string? DiscountId { get; set; }

        [Required(ErrorMessage = "Yêu cầu phải có tiêu đề.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Yêu cầu phải có ngày tải lên.")]
        public DateTime UploadDate { get; set; }

        public string? Description { get; set; }

        public string? Image { get; set; }

        [Required(ErrorMessage = "Yêu cầu phải có trạng thái hoạt động.")]
        public bool IsActive { get; set; }

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

    public class NewsDTOForUpdate
    {
        [Required(ErrorMessage = "Yêu cầu phải có Id.")]
        public int Id { get; set; }

        public string? DiscountId { get; set; }

        [Required(ErrorMessage = "Yêu cầu phải có tiêu đề.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Yêu cầu phải có ngày tải lên.")]
        public DateTime UploadDate { get; set; }

        public string? Description { get; set; }

        public IFormFile? Image { get; set; }

        [Required(ErrorMessage = "Yêu cầu phải có trạng thái hoạt động.")]

        public bool IsActive { get; set; }
    }
}
