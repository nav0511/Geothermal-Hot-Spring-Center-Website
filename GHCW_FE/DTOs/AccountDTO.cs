namespace GHCW_FE.DTOs
{
    public class AccountDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
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
    }
}
