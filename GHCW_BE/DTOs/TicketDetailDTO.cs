﻿using GHCW_BE.Models;

namespace GHCW_BE.DTOs
{
    public class TicketDetailDTO
    {
        public int TicketId { get; set; }
        public int ServiceId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total { get; set; }

        public virtual TicketDTO Ticket { get; set; } = null!;
        public virtual ServiceDTO Service { get; set; } = null!;

    }

    public class TicketDetailDTOForPayment
    {
        public int ServiceId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
