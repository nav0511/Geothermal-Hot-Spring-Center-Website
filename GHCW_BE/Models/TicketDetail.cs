using System;
using System.Collections.Generic;

namespace GHCW_BE.Models
{
    public partial class TicketDetail
    {
        public int TicketId { get; set; }
        public int ServiceId { get; set; }
        public int? Quantity { get; set; }
        public decimal? Price { get; set; }
        public decimal? Total { get; set; }

        public virtual Service Service { get; set; } = null!;
        public virtual Ticket Ticket { get; set; } = null!;
    }
}
