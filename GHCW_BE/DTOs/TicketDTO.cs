namespace GHCW_BE.DTOs
{
    public class TicketDTO
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int? ReceptionistId { get; set; }
        public int? SaleId { get; set; }
        public string? DiscountCode { get; set; }
        public decimal Total { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime BookDate { get; set; }
        public byte PaymentStatus { get; set; }
        public byte CheckIn { get; set; }
    }
}
