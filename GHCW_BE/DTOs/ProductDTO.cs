using System.ComponentModel.DataAnnotations;

namespace GHCW_BE.DTOs
{
    public class ProductDTO
    {
        [Required(ErrorMessage = "Yêu cầu phải có Id.")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Yêu cầu phải có mã danh mục.")]
        public int? CategoryId { get; set; }

        [Required(ErrorMessage = "Yêu cầu phải có tên sản phẩm.")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Yêu cầu phải có kích thước.")]
        public string? Size { get; set; }

        [Required(ErrorMessage = "Yêu cầu phải có mô tả.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Yêu cầu phải có giá.")]
        public double? Price { get; set; }

        [Required(ErrorMessage = "Yêu cầu phải có trạng thái thuê.")]
        public bool? IsForRent { get; set; }

        [Required(ErrorMessage = "Yêu cầu phải có hình ảnh.")]
        public string? Image { get; set; }

        [Required(ErrorMessage = "Yêu cầu phải có số lượng.")]
        public int? Quantity { get; set; }

        [Required(ErrorMessage = "Yêu cầu phải có trạng thái sẵn có.")]
        public bool? IsAvailable { get; set; }
    }

    public class ProductDTO2
    {
        [Required(ErrorMessage = "Yêu cầu phải có mã danh mục.")]
        public int? CategoryId { get; set; }

        [Required(ErrorMessage = "Yêu cầu phải có tên sản phẩm.")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Yêu cầu phải có kích thước.")]
        public string? Size { get; set; }

        [Required(ErrorMessage = "Yêu cầu phải có mô tả.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Yêu cầu phải có giá.")]
        public double? Price { get; set; }

        [Required(ErrorMessage = "Yêu cầu phải có trạng thái thuê.")]
        public bool? IsForRent { get; set; }

        [Required(ErrorMessage = "Yêu cầu phải có hình ảnh.")]
        public string? Image { get; set; }

        [Required(ErrorMessage = "Yêu cầu phải có số lượng.")]
        public int? Quantity { get; set; }

        [Required(ErrorMessage = "Yêu cầu phải có trạng thái sẵn có.")]
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
