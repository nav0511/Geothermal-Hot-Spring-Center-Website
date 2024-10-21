using System;
using System.Collections.Generic;

namespace GHCW_BE.Models
{
    public partial class Customer
    {
        public Customer()
        {
            Bills = new HashSet<Bill>();
            Tickets = new HashSet<Ticket>();
        }

        public int Id { get; set; }
        public int? AccountId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }

        public virtual Account? Account { get; set; }
        public virtual ICollection<Bill> Bills { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; }
    }
}
