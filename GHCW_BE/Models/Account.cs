using System;
using System.Collections.Generic;

namespace GHCW_BE.Models
{
    public partial class Account
    {
        public Account()
        {
            Bills = new HashSet<Bill>();
            Customers = new HashSet<Customer>();
            Schedules = new HashSet<Schedule>();
            Tickets = new HashSet<Ticket>();
        }

        public int Id { get; set; }
        public string? Name { get; set; }
        public bool? Gender { get; set; }
        public string? Address { get; set; }
        public string PhoneNumber { get; set; } = null!;
        public DateTime? DoB { get; set; }
        public int Role { get; set; }
        public string? ActivationCode { get; set; }
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public bool IsEmailNotify { get; set; }
        public bool IsActive { get; set; }
        public string? RefreshToken { get; set; }

        public virtual ICollection<Bill> Bills { get; set; }
        public virtual ICollection<Customer> Customers { get; set; }
        public virtual ICollection<Schedule> Schedules { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; }
    }
}
