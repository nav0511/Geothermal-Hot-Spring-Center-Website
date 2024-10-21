namespace GHCW_BE.DTOs
{
    public class LoginDTO
    {
        public string Password { get; set; } = null!;
        public string? Email { get; set; }
        public string? Type { get; set; }
    }
}
