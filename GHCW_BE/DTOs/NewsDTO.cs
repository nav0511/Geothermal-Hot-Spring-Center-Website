using GHCW_BE.Models;

namespace GHCW_BE.DTOs
{
    public class NewsDTO
    {
        public int Id { get; set; }
        public string? DiscountId { get; set; }
        public string? Title { get; set; }
        public DateTime? UploadDate { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }
        public bool? IsActive { get; set; }
        public virtual DiscountDTO? Discount { get; set; }
    }

    public class NewsDTO2
    {
        public string? DiscountId { get; set; }
        public string? Title { get; set; }
        public DateTime? UploadDate { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }
        public bool? IsActive { get; set; }
        public virtual DiscountDTO? Discount { get; set; }
    }


}
