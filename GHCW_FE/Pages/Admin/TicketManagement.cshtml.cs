using GHCW_FE.DTOs;
using GHCW_FE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IdentityModel.Tokens.Jwt;
using System.Net;

namespace GHCW_FE.Pages.Admin
{
    public class TicketManagementModel : PageModel
    {
        private readonly TicketService _ticketService;
        private readonly TokenService _tokenService;
        private readonly AuthenticationService _authService;
        private readonly AccountService _accService;

        public TicketManagementModel(TokenService tokenService, AuthenticationService authService, TicketService ticketService, AccountService accService)
        {
            _authService = authService;
            _ticketService = ticketService;
            _tokenService = tokenService;
            _accService = accService;
        }

        public List<TicketDTO> TicketDTOs { get; set; } = new List<TicketDTO>();

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int OrderOption { get; set; }

        [BindProperty(SupportsGet = true)]
        public int SortOption { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        private const int PageSize = 9;
        public async Task<IActionResult> OnGetAsync(int pageNumber = 1, string? searchTerm = null, int orderOption = 0, int sortOption = 0)
        {
            var accessToken = await _tokenService.CheckAndRefreshTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để xem thông tin.";
                return RedirectToPage("/Authentications/Login");
            }

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(accessToken);
            var roleClaim = jwtToken.Claims.FirstOrDefault(claim => claim.Type == "Role");
            if (roleClaim != null && (int.Parse(roleClaim.Value) > 3 || int.Parse(roleClaim.Value) == 2))
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này.";
                return RedirectToPage("/Authentications/Login");
            }
            _accService.SetAccessToken(accessToken);

            var (statusCode, userProfile) = await _accService.UserProfile(accessToken);
            if (userProfile?.Role > 3 || userProfile?.Role == 2)
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập thông tin này.";
                return RedirectToPage("/Authentications/Login");
            }
            else if (statusCode == HttpStatusCode.NotFound)
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Không tìm thấy người dùng này.";
                return RedirectToPage("/Authentications/Login");
            }
            else if (statusCode != HttpStatusCode.OK)
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi lấy thông tin người dùng.";
                return RedirectToPage("/Authentications/Login");
            }

            SearchTerm = searchTerm;
            OrderOption = orderOption;
            SortOption = sortOption;
            CurrentPage = pageNumber;
            int skip = (pageNumber - 1) * PageSize;

            var (statusCode1, tickets) = await _ticketService.GetBookingList("Ticket", userProfile.Role, userProfile.Id);
            if (statusCode1 != HttpStatusCode.OK || tickets == null)
            {
                TempData["ErrorMessage"] = "Không lấy được danh sách vé.";
                tickets = new List<TicketDTO>();
                return RedirectToPage();
            }

            if (userProfile?.Role == 3)
            {
                tickets = tickets?.Where(t => t.SaleId == userProfile.Id).ToList();
            }

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                tickets = tickets?.Where(t =>
                   (t.Receptionist.Name != null && t.Receptionist.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)) ||
                   (t.Customer.Name != null && t.Customer.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
               ).ToList();
            }

            tickets = OrderOption switch
            {
                1 => tickets.OrderBy(d => d.Id).ToList(),
                2 => tickets.OrderByDescending(d => d.Id).ToList(),
                3 => tickets.OrderBy(d => d.OrderDate).ToList(),
                4 => tickets.OrderByDescending(d => d.OrderDate).ToList(),
                5 => tickets.OrderBy(d => d.BookDate).ToList(),
                6 => tickets.OrderByDescending(d => d.BookDate).ToList(),
                _ => tickets.ToList(),
            };

            tickets = SortOption switch
            {
                1 => tickets.Where(d => d.PaymentStatus == 1).ToList(),
                2 => tickets.Where(d => d.PaymentStatus == 0).ToList(),
                3 => tickets.Where(d => d.CheckIn == 0).ToList(),
                4 => tickets.Where(d => d.CheckIn == 1).ToList(),
                5 => tickets.Where(d => d.CheckIn == 2).ToList(),
                6 => tickets.Where(d => d.IsActive).ToList(),
                7 => tickets.Where(d => !d.IsActive).ToList(),
                _ => tickets.ToList(),
            };

            var totalTickets = tickets?.Count() ?? 0;
            TotalPages = (int)Math.Ceiling((double)totalTickets / PageSize);
            TicketDTOs = tickets?.Skip(skip).Take(PageSize).ToList() ?? new List<TicketDTO>();
            return Page();
        }

        public async Task<IActionResult> OnPostTicketActivation(int nId)
        {
            var accessToken = await _tokenService.CheckAndRefreshTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để thực hiện việc này.";
                return RedirectToPage("/Authentications/Login");
            }
            _accService.SetAccessToken(accessToken);

            var statusCode = await _ticketService.TicketActivation(accessToken, nId);
            if (statusCode == HttpStatusCode.Forbidden)
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập thông tin này.";
                return RedirectToPage("/Authentications/Login");
            }
            else if (statusCode != HttpStatusCode.OK)
            {
                TempData["ErrorMessage"] = "Đổi trạng thái thất bại, vui lòng thử lại sau.";
                await OnGetAsync();
                return Page();
            }
            else
            {
                TempData["SuccessMessage"] = "Đổi trạng thái thành công.";
                await OnGetAsync();
                return Page();
            }
        }
    }
}
