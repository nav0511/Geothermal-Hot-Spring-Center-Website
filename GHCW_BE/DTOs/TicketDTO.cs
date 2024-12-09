using GHCW_BE.Models;

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
        public bool IsActive { get; set; }


        public virtual CustomerDTO Customer { get; set; } = null!;
        public virtual AccountDTO? Receptionist { get; set; }
        public virtual AccountDTO? Sale { get; set; }
        public virtual DiscountDTO? DiscountCodeNavigation { get; set; }
    }

    public class TicketDTO2
    {
        public int Id { get; set; }
        public int? ReceptionistId { get; set; }
        public byte PaymentStatus { get; set; }
        public byte CheckIn { get; set; }
    }

    public class TicketDTOForPayment
    {
        public int CustomerId { get; set; }
        public string? DiscountCode { get; set; }
        public decimal Total { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime BookDate { get; set; }
        public byte PaymentStatus { get; set; }
        public byte CheckIn { get; set; }
        public virtual ICollection<TicketDetailDTOForPayment> TicketDetails { get; set; }
    }

    public class TicketDTOForStaff
    {
        public int CustomerId { get; set; }
        public int? ReceptionistId { get; set; }
        public int? SaleId { get; set; }
        public string? DiscountCode { get; set; }
        public decimal Total { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime BookDate { get; set; }
        public byte PaymentStatus { get; set; }
        public byte CheckIn { get; set; }
        public virtual ICollection<TicketDetailDTOForPayment> TicketDetails { get; set; }
    }
}
