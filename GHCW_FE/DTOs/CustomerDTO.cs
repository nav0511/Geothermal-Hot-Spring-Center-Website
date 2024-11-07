using System.ComponentModel.DataAnnotations;

namespace GHCW_FE.DTOs
{
    public class CustomerDTO
    {
        public int Id { get; set; }
        public int? AccountId { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
    }
}
