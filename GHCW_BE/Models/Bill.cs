using System;
using System.Collections.Generic;

namespace GHCW_BE.Models
{
    public partial class Bill
    {
        public Bill()
        {
            BillDetails = new HashSet<BillDetail>();
        }

        public int Id { get; set; }
        public int ReceptionistId { get; set; }
        public int CustomerId { get; set; }
        public string DiscountCode { get; set; } = null!;
        public decimal Total { get; set; }
        public byte PaymentStatus { get; set; }
        public DateTime OrderDate { get; set; }

        public virtual Customer Customer { get; set; } = null!;
        public virtual Discount DiscountCodeNavigation { get; set; } = null!;
        public virtual Account Receptionist { get; set; } = null!;
        public virtual ICollection<BillDetail> BillDetails { get; set; }
    }
}
