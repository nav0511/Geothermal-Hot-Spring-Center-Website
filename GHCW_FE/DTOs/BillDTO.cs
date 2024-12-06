
namespace GHCW_FE.DTOs
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

        public virtual CustomerDTO? Customer { get; set; }
        public virtual AccountDTO Receptionist { get; set; } = null!;
    }

    public class BillDTOForBuyProducts
    {
        public int ReceptionistId { get; set; }
        public int? CustomerId { get; set; }
        public string? DiscountCode { get; set; }
        public decimal Total { get; set; }
        public byte PaymentStatus { get; set; }
        public DateTime OrderDate { get; set; }
        public bool IsActive { get; set; }
        public virtual ICollection<BillDetailDTOForBuyProducts> BillDetails { get; set; }
    }

    public class BillDetailDTOForBuyProducts
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
