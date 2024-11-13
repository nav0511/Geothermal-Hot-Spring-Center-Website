using System;
using System.Collections.Generic;

namespace GHCW_BE.Models
{
    public partial class Service
    {
        public Service()
        {
            TicketDetails = new HashSet<TicketDetail>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public double Price { get; set; }
        public string Time { get; set; } = null!;
        public string? Description { get; set; }
        public string? Image { get; set; }
        public bool IsActive { get; set; }


        public virtual ICollection<TicketDetail> TicketDetails { get; set; }
    }
}
