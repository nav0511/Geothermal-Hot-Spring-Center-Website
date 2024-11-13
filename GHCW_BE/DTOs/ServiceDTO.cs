namespace GHCW_BE.DTOs
{
    public class ServiceDTO
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public double? Price { get; set; }
        public string? Time { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }
        public bool IsActive { get; set; }

    }

    public class ServiceDTO2
    {
        public string? Name { get; set; }
        public double? Price { get; set; }
        public string? Time { get; set; }
        public string? Description { get; set; }
        public IFormFile? Image { get; set; }
        public bool? IsActive { get; set; }

    }
}
