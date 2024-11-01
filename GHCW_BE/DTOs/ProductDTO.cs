namespace GHCW_BE.DTOs
{
    public class ProductDTO
    {
        public int Id { get; set; }
        public int? CategoryId { get; set; }
        public string? Name { get; set; }
        public string? Size { get; set; }
        public string? Description { get; set; }
        public double? Price { get; set; }
        public bool? IsForRent { get; set; }
        public string? Image { get; set; }
        public int? Quantity { get; set; }
        public bool? IsAvailable { get; set; }
    }

    public class ProductDTO2
    {
        public int? CategoryId { get; set; }
        public string? Name { get; set; }
        public string? Size { get; set; }
        public string? Description { get; set; }
        public double? Price { get; set; }
        public bool? IsForRent { get; set; }
        public string? Image { get; set; }
        public int? Quantity { get; set; }
        public bool? IsAvailable { get; set; }

    }

    public class ProductDTOImg
    {
        public int? CategoryId { get; set; }
        public string? Name { get; set; }
        public string? Size { get; set; }
        public string? Description { get; set; }
        public double? Price { get; set; }
        public bool? IsForRent { get; set; }
        public string? Image { get; set; }
        public int? Quantity { get; set; }
        public bool? IsAvailable { get; set; }
        public IFormFile? Img { get; set; }

    }
}
