using GHCW_BE.DTOs;
using GHCW_BE.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace GHCW_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatbotController : ControllerBase
    {
        private readonly Helper _helper;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _client;
        public ChatbotController(IConfiguration configuration, Helper helper, IHttpClientFactory client)
        {
            _configuration= configuration;
            _helper = helper;
            _client = client.CreateClient("OpenAI");
        }

        [HttpPost]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            string systemPrompt = _helper.ReadSystemPrompt("ChatbotPrompt.txt");

            var payload = new
            {
                model = "gpt-4o-mini",
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = request.Prompt }
                }
            };

            // Gửi yêu cầu tới OpenAI API
            var response = await _client.PostAsJsonAsync("/v1/chat/completions", payload);

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(responseContent);

            var choices = document.RootElement.GetProperty("choices");
            if (choices.GetArrayLength() == 0)
            {
                return BadRequest(new { Error = "No choices available in response." });
            }

            string botReply = choices[0].GetProperty("message").GetProperty("content").GetString();

            // Xử lý vượt ngữ cảnh (nếu câu trả lời không liên quan)
            if (string.IsNullOrWhiteSpace(botReply) || botReply.Contains("Tôi không biết") || botReply.Contains("ngoài phạm vi"))
            {
                botReply = "Xin lỗi, tôi không thể hỗ trợ về vấn đề này." +
                    "Vui lòng liên hệ trực tiếp qua hotline: 02033 860 669 - 0969 365 916 hoặc email: trungtamkhoangnongdiachat@email.com để được giải đáp thắc mắc. Xin cảm ơn.";
            }

            return Ok(new { Reply = botReply });
        }
    }
}
