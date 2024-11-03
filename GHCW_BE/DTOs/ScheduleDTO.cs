namespace GHCW_BE.DTOs
{
    public class ScheduleDTO
    {
        public int Id { get; set; }
        public int ReceptionistId { get; set; }
        public byte Shift { get; set; }
        public DateTime Date { get; set; }
        public bool IsActive { get; set; }
    }
}
