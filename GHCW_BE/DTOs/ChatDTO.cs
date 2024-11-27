namespace GHCW_BE.DTOs
{
    public class ChatRequest
    {
        public string Prompt { get; set; }
    }

    public class ChatMessage
    {
        public string Role { get; set; }
        public string Content { get; set; }
    }

    public class ChatResponse
    {
        public string Reply { get; set; }
    }
}
