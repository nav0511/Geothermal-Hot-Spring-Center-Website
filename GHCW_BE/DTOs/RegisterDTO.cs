﻿namespace GHCW_BE.DTOs
{
    public class RegisterDTO
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }
    public class SendMailDTO
    {
        public string FromEmail { get; set; }
        public string Password { get; set; }
        public string ToEmail { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
    public class ResetPassword
    {
        public string Email { get; set; }
    }
}
