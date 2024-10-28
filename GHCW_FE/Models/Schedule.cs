using System;
using System.Collections.Generic;

namespace GHCW_FE.Models
{
    public partial class Schedule
    {
        public int Id { get; set; }
        public int ReceptionistId { get; set; }
        public byte Shift { get; set; }
        public DateTime Date { get; set; }
        public bool IsActive { get; set; }

        public virtual Account Receptionist { get; set; } = null!;
    }
}
