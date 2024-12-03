
namespace GHCW_FE.DTOs
{
    public class BillDetailDTO
    {
        public int BillId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total { get; set; }

        public virtual BillDTO Bill { get; set; } = null!;
        public virtual ProductDTO Product { get; set; } = null!;
    }
}
