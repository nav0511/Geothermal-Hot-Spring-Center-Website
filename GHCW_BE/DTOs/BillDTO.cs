using GHCW_BE.Models;

namespace GHCW_BE.DTOs
{
    public class BillDTO
    {
        public int Id { get; set; }
        public int ReceptionistId { get; set; }
        public int? CustomerId { get; set; }
        public string? DiscountCode { get; set; }
        public decimal Total { get; set; }
        public byte PaymentStatus { get; set; }
        public DateTime OrderDate { get; set; }
        public bool IsActive { get; set; }

        public virtual Customer? Customer { get; set; }
        public virtual Account Receptionist { get; set; } = null!;
    }
}
