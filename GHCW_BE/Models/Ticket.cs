using System;
using System.Collections.Generic;

namespace GHCW_BE.Models
{
    public partial class Ticket
    {
        public Ticket()
        {
            TicketDetails = new HashSet<TicketDetail>();
        }

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

        public virtual Customer Customer { get; set; } = null!;
        public virtual Discount? DiscountCodeNavigation { get; set; }
        public virtual Account? Receptionist { get; set; }
        public virtual Account? Sale { get; set; }
        public virtual ICollection<TicketDetail> TicketDetails { get; set; }
    }
}
