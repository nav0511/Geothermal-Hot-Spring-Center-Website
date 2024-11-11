namespace GHCW_FE.DTOs
{
    public class ScheduleDTO
    {
        public int Id { get; set; }
        public int ReceptionistId { get; set; }
        public string ReceptionistName { get; set; }
        public byte Shift { get; set; }
        public DateTime Date { get; set; }
        public bool IsActive { get; set; }
    }

    public class ScheduleByWeek
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class AddScheduleRequest
    {
        public int ReceptionistId { get; set; }
        public byte Shift { get; set; }
        public DateTime Date { get; set; }
        public bool IsActive { get; set; }
    }

    public class EditScheduleRequest
    {
        public int Id { get; set; }
        public int ReceptionistId { get; set; }
        public byte Shift { get; set; }
        public DateTime Date { get; set; }
        public bool IsActive { get; set; }
    }
}
