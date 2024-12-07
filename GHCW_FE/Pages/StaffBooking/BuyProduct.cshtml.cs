using GHCW_FE.DTOs;
using GHCW_FE.Services;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.IdentityModel.Tokens.Jwt;

namespace GHCW_FE.Pages.StaffBooking
{
    public class BuyProductModel : PageModel
    {
        private readonly ProductService _ProductService;
        private readonly BillService _BillService;
        private readonly TokenService _tokenService;
        private readonly AuthenticationService _authService;
        private readonly DiscountService _discountService;
        private readonly AccountService _accService;
        private readonly CustomerService _customerService;
        public BuyProductModel(ProductService ProductService, BillService BillService,
            TokenService tokenService, AuthenticationService authService,
            DiscountService discountService, AccountService accountService,
            CustomerService customerService)
        {
            _ProductService = ProductService;
            _BillService = BillService;
            _tokenService = tokenService;
            _authService = authService;
            _discountService = discountService;
            _accService = accountService;
            _customerService = customerService;
        }
        public string Message { get; set; }
        public bool Success { get; set; }
        public bool IsLoggedIn { get; set; }
        public string ErrorMessage { get; set; }
        [BindProperty]
        public string CartData { get; set; }
        [BindProperty]
        public string? SelectedDiscountCode { get; set; }
        [BindProperty]
        public string? SelectedCustomerEmail { get; set; }
        public bool HasBillSaved { get; set; } = false;
        public string Token { get; set; }

        public List<ProductDTO> AvailableProducts { get; set; } = new List<ProductDTO>();
        public List<DiscountDTO> AvailableDiscounts { get; set; } = new List<DiscountDTO>();
        public Dictionary<int, int> SelectedProduct { get; set; } = new Dictionary<int, int>();
        [BindProperty]
        public AddCustomerRequest AddRequest { get; set; }
        [BindProperty]
        public bool AddNewCustomer { get; set; }

        public async Task<IActionResult> OnGetAsync()
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
            if (roleClaim != null && int.Parse(roleClaim.Value) != 4)
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này.";
                return RedirectToPage("/Authentications/Login");
            }
            (HttpStatusCode StatusCode, List<ProductDTO>? ListProducts) = await _ProductService.GetProducts($"Product?$filter=IsAvailable eq true");
            AvailableProducts = ListProducts;
            (HttpStatusCode discountStatusCode, List<DiscountDTO>? Discounts) = await _discountService.GetDiscounts("Discount?$filter=IsAvailable eq true");
            if (discountStatusCode == HttpStatusCode.OK && Discounts != null)
            {
                AvailableDiscounts = Discounts;
            }
            Token = accessToken;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            //if (!ModelState.IsValid)
            //{
            //    TempData["ErrorMessage"] = "Thông tin đăng ký không hợp lệ, vui lòng thử lại.";
            //    await OnGetAsync();
            //    return Page();
            //}
            try
            {
                var accessToken = await _tokenService.CheckAndRefreshTokenAsync();
                if (string.IsNullOrEmpty(accessToken))
                {
                    await _authService.LogoutAsync();
                    TempData["ErrorMessage"] = "Bạn cần đăng nhập để thực hiện hành động này.";
                    return RedirectToPage("/Authentications/Login");
                }
                _accService.SetAccessToken(accessToken);
                _BillService.SetAccessToken(accessToken);
                _customerService.SetAccessToken(accessToken);
                Token = accessToken;

                if (AddNewCustomer)
                {
                    if (SelectedCustomerEmail == null)
                    {
                        var errorMessages = new List<string>();

                        // Validate Name
                        if (string.IsNullOrWhiteSpace(AddRequest.Name))
                        {
                            errorMessages.Add("Yêu cầu nhập họ tên.");
                        }
                        else if (!Regex.IsMatch(AddRequest.Name, @"^[a-zA-ZÀÁÂÃÈÉÊÌÍÒÓÔÕÙÚĂĐĨŨƠƯÝàáâãèéêìíòóôõùúăđĩũơưýẠ-ỹ\s]+$"))
                        {
                            errorMessages.Add("Họ tên chỉ được chứa chữ cái và khoảng trắng.");
                        }

                        // Validate Email
                        if (string.IsNullOrWhiteSpace(AddRequest.Email))
                        {
                            errorMessages.Add("Yêu cầu nhập email.");
                        }
                        else if (!Regex.IsMatch(AddRequest.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                        {
                            errorMessages.Add("Không đúng định dạng email.");
                        }

                        // Validate PhoneNumber
                        if (string.IsNullOrWhiteSpace(AddRequest.PhoneNumber))
                        {
                            errorMessages.Add("Yêu cầu nhập số điện thoại.");
                        }
                        else if (!Regex.IsMatch(AddRequest.PhoneNumber, @"^(0[3||5||7||8||9])\d{8}$"))
                        {
                            errorMessages.Add("Số điện thoại chưa đúng định dạng.");
                        }

                        // If there are errors, set them in TempData and return
                        if (errorMessages.Any())
                        {
                            TempData["ErrorMessage"] = string.Join("<br/>", errorMessages);
                            await OnGetAsync();
                            return Page();
                        }

                        var (statusCode1, account) = await _accService.GetUserByEmail(accessToken, AddRequest.Email);

                        if (statusCode1 == HttpStatusCode.OK && account != null)
                        {
                            AddRequest.AccountId = account.Id;
                        }
                        else if (statusCode1 != HttpStatusCode.NotFound)
                        {
                            AddRequest.AccountId = null;
                        }

                        var statusCode = await _customerService.AddCustomer(accessToken, AddRequest);
                        if (statusCode == HttpStatusCode.OK) { }
                        else if (statusCode == HttpStatusCode.Forbidden)
                        {
                            TempData["ErrorMessage"] = "Bạn không có quyền thêm khách hàng mới.";
                            await OnGetAsync();
                            return Page();
                        }
                        else if (statusCode == HttpStatusCode.Conflict)
                        {
                            TempData["ErrorMessage"] = "Đã có người sử dụng email này, xin vui lòng chọn email khác.";
                            await OnGetAsync();
                            return Page();
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Thêm người dùng mới thất bại, vui lòng thử lại.";
                            await OnGetAsync();
                            return Page();
                        }
                    }
                }
                
                var cart = JsonConvert.DeserializeObject<List<CartItemDTO>>(CartData);
                var user = JsonConvert.DeserializeObject<AccountDTO>(HttpContext.Session.GetString("acc"));
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin nhân viên.";
                    await OnGetAsync();
                    return Page();
                }

                if (cart == null || cart.Count == 0)
                {
                    TempData["ErrorMessage"] = "Giỏ hàng trống hoặc không hợp lệ.";
                    await OnGetAsync();
                    return Page();
                }

                string email = SelectedCustomerEmail != null ? SelectedCustomerEmail : AddRequest.Email;
                var (statusCode2, acc) = await _customerService.GetCustomerByEmail(accessToken, email);
                if (acc == null && AddNewCustomer)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin khách hàng.";
                    await OnGetAsync();
                    return Page();
                }

                if (user == null || cart == null || cart.Count == 0)
                {
                    Message = "Có lỗi xảy ra, xin quý khách vui lòng thử lại sau";
                }
                else
                {
                    var newBill = new BillDTOForBuyProducts
                    {
                        ReceptionistId = user.Id,
                        DiscountCode = SelectedDiscountCode,
                        Total = cart.Sum(item => item.Quantity * item.Price),
                        OrderDate = DateTime.Now,
                        PaymentStatus = 1,
                        IsActive = true,
                        BillDetails = cart.Select(item => new BillDetailDTOForBuyProducts
                        {
                            ProductId = item.ServiceId,
                            Quantity = item.Quantity,
                            Price = item.Price
                        }).ToList()
                    };
                    if (acc != null)
                    {
                        newBill.CustomerId = acc.Id;
                    }

                    var saveBillStatusCode = await _BillService.SaveBillAsync(newBill, accessToken);
                    if (saveBillStatusCode == HttpStatusCode.OK)
                    {
                        TempData["SuccessMessage"] = "Thanh toán thành công.";
                        Success = true;
                        HasBillSaved = true;
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Thanh toán không thành công.";
                        Success = false;
                    }

                }
                await OnGetAsync();
                return Page();
            }
            catch (UnauthorizedAccessException)
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.";
                return RedirectToPage("/Authentications/Login");
            }


        }
    }
}
