namespace GHCW_FE.DTOs
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
