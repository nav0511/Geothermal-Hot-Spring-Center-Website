namespace GHCW_FE.DTOs
{
    public class CartItemDTO
    {
        public int ServiceId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }

    public class CartItemWithNameDTO
    {
        public int ServiceId { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
