namespace GHCW_FE.DTOs
{
    public class DiscountDTO
    {
        public string Code { get; set; } = null!;
        public string? Name { get; set; }
        public int? Value { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Description { get; set; }
        public bool? IsAvailable { get; set; }
    }
}
