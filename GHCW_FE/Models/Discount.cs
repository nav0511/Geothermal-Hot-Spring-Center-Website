using System;
using System.Collections.Generic;

namespace GHCW_FE.Models
{
    public partial class Discount
    {
        public Discount()
        {
            Bills = new HashSet<Bill>();
            News = new HashSet<News>();
            Tickets = new HashSet<Ticket>();
        }

        public string Code { get; set; } = null!;
        public string? Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Description { get; set; }
        public bool IsAvailable { get; set; }

        public virtual ICollection<Bill> Bills { get; set; }
        public virtual ICollection<News> News { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; }
    }
}
