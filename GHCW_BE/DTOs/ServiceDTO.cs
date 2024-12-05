using System.ComponentModel.DataAnnotations;

namespace GHCW_BE.DTOs
{
    public class ServiceDTO
    {
        [Required(ErrorMessage = "Yêu cầu phải có Id.")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Yêu cầu phải có tên.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Yêu cầu phải có giá.")]
        public double Price { get; set; }

        [Required(ErrorMessage = "Yêu cầu phải có thời gian.")]
        public string Time { get; set; }

        public string? Description { get; set; }

        public string? Image { get; set; }

        [Required(ErrorMessage = "Yêu cầu phải có trạng thái.")]
        public bool IsActive { get; set; }

    }

    public class ServiceDTO2
    {
        [Required(ErrorMessage = "Yêu cầu phải có tên.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Yêu cầu phải có giá.")]
        public double Price { get; set; }

        [Required(ErrorMessage = "Yêu cầu phải có thời gian.")]
        public string Time { get; set; }

        public string? Description { get; set; }

        public IFormFile? Image { get; set; }

        [Required(ErrorMessage = "Yêu cầu phải có trạng thái.")]
        public bool IsActive { get; set; }

    }

    public class ServiceDTOForUpdate
    {
        [Required(ErrorMessage = "Yêu cầu phải có Id.")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Yêu cầu phải có tên.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Yêu cầu phải có giá.")]
        public double Price { get; set; }

        [Required(ErrorMessage = "Yêu cầu phải có thời gian.")]
        public string Time { get; set; }

        public string? Description { get; set; }

        public IFormFile? Image { get; set; }

        [Required(ErrorMessage = "Yêu cầu phải có trạng thái.")]
        public bool IsActive { get; set; }

    }
}
